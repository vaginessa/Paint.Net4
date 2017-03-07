namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Animation;
    using PaintDotNet.Collections;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Rendering;
    using PaintDotNet.Threading;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class LayersStrip : VerticalImageStrip
    {
        private Layer activeLayer;
        private PaintDotNet.Document document;
        private PaintDotNet.Controls.DocumentWorkspace documentWorkspace;
        private ConcurrentSet<Layer> invalidatedLayers;
        private List<Layer> layers;
        private KeyValuePair<Layer, VerticalImageStrip.Item>[] layersAndItemsBeforeChanging;
        private int layersListChangingCount;
        private Dictionary<Layer, VerticalImageStrip.Item> layerToItemMap;
        private readonly ProtectedRegion onLayersListChangedRegion = new ProtectedRegion("OnLayersListChanged", ProtectedRegionOptions.ErrorOnPerThreadReentrancy | ProtectedRegionOptions.DisablePumpingWhenEntered);
        private Action processInvalidatedLayersCallback;
        private Dictionary<Layer, double> renderSlotsBeforeChanging;
        private ThumbnailManager thumbnailManager;

        [field: CompilerGenerated]
        public event ValueEventHandler<Layer> LayerClicked;

        public LayersStrip()
        {
            using (ISynchronizationContext context = SynchronizationContextDispatcher.CreateRef())
            {
                this.thumbnailManager = new ThumbnailManager(context);
            }
            this.layerToItemMap = new Dictionary<Layer, VerticalImageStrip.Item>();
            this.layers = new List<Layer>();
            this.activeLayer = null;
            this.invalidatedLayers = new ConcurrentSet<Layer>();
            this.processInvalidatedLayersCallback = new Action(this.ProcessInvalidatedLayers);
            base.AllowReorder = true;
            base.ManagedFocus = true;
        }

        private void BeginLayersListChange()
        {
            base.BeginUpdate();
            this.layersListChangingCount++;
            if (this.layersListChangingCount == 1)
            {
                this.OnLayersListChanging();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.document != null))
            {
                this.Document = null;
            }
            DisposableUtil.Free<ThumbnailManager>(ref this.thumbnailManager, disposing);
            base.Dispose(disposing);
        }

        private void EndLayersListChange()
        {
            base.EndUpdate();
            if (this.layersListChangingCount == 1)
            {
                this.OnLayersListChanged();
            }
            this.layersListChangingCount--;
        }

        private void InsertLayer(int layerIndex, Layer layer)
        {
            layer.Invalidated += new EventHandler<RectInt32InvalidatedEventArgs>(this.OnLayerInvalidated);
            layer.PropertyChanged += new PropertyEventHandler(this.OnLayerPropertyChanged);
            VerticalImageStrip.Item item = new VerticalImageStrip.Item(null) {
                ImageOpacity = { Value = 0.0 },
                Tag = layer,
                Text = layer.Name,
                IsChecked = layer.Visible
            };
            this.BeginLayersListChange();
            this.layers.Insert(layerIndex, layer);
            this.layerToItemMap.Add(layer, item);
            int index = this.LayerIndexToRenderSlot(layerIndex);
            base.InsertItem(index, item);
            this.EndLayersListChange();
            this.RefreshLayerThumbnail(layer);
            if (this.activeLayer == layer)
            {
                using (base.UseSuppressEnsureItemFullyVisible())
                {
                    this.ActiveLayer = null;
                    this.ActiveLayer = layer;
                }
            }
        }

        private void InvalidateThumbnail(Layer layer)
        {
            if (!base.IsDisposed && this.layerToItemMap.ContainsKey(layer))
            {
                VerticalImageStrip.Item item;
                this.RefreshLayerThumbnail(layer);
                if (this.layerToItemMap.TryGetValue(layer, out item) && (layer == this.activeLayer))
                {
                    base.EnsureItemFullyVisible(item);
                }
            }
        }

        internal int LayerIndexToRenderSlot(int layerIndex) => 
            ((this.layers.Count - layerIndex) - 1);

        private void OnDocumentLayersChanged(object sender, EventArgs e)
        {
            this.EndLayersListChange();
        }

        private void OnDocumentLayersChanging(object sender, EventArgs e)
        {
            this.BeginLayersListChange();
        }

        private void OnDocumentLayersInserted(object sender, IndexEventArgs e)
        {
        }

        private void OnDocumentLayersRemovingAt(object sender, IndexEventArgs e)
        {
        }

        protected override void OnItemClicked(VerticalImageStrip.Item item, VerticalImageStrip.ItemPart itemPart, MouseButtons mouseButtons)
        {
            Layer tag = (Layer) item.Tag;
            if (itemPart == VerticalImageStrip.ItemPart.CheckBox)
            {
                tag.Visible = !tag.Visible;
            }
            else if (mouseButtons == MouseButtons.Left)
            {
                this.OnLayerClicked(tag);
            }
            base.OnItemClicked(item, itemPart, mouseButtons);
        }

        protected override void OnItemMoved(VerticalImageStripItemMovedEventArgs e)
        {
            e.Item.IsSelected = true;
            base.OnItemMoved(e);
            this.EndLayersListChange();
        }

        protected override void OnItemMoving(VerticalImageStripItemMovingEventArgs e)
        {
            this.BeginLayersListChange();
            base.OnItemMoving(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.CancelReordering();
            base.OnKeyDown(e);
        }

        private void OnLayerClicked(Layer layer)
        {
            this.LayerClicked.Raise<Layer>(this, layer);
        }

        private void OnLayerInvalidated(object sender, RectInt32InvalidatedEventArgs e)
        {
            ConcurrentSet<Layer> invalidatedLayers = this.invalidatedLayers;
            lock (invalidatedLayers)
            {
                this.invalidatedLayers.Add((Layer) sender);
            }
            PdnSynchronizationContext.Instance.EnsurePosted(this.processInvalidatedLayersCallback);
        }

        private void OnLayerPropertyChanged(object sender, PropertyEventArgs e)
        {
            if (base.InvokeRequired)
            {
                PdnSynchronizationContext.Instance.Post((SendOrPostCallback) (_ => this.OnLayerPropertyChanged(sender, e)));
            }
            else if (!base.IsDisposed)
            {
                Layer key = (Layer) sender;
                if (this.layerToItemMap.ContainsKey(key))
                {
                    VerticalImageStrip.Item item = this.layerToItemMap[key];
                    item.Text = key.Name;
                    item.IsChecked = key.Visible;
                }
            }
        }

        private void OnLayersListChanged()
        {
            using (this.onLayersListChangedRegion.UseEnterScope())
            {
                this.OnLayersListChangedImpl();
            }
            this.layersAndItemsBeforeChanging = null;
            this.renderSlotsBeforeChanging = null;
        }

        private void OnLayersListChangedImpl()
        {
            Layer[] newLayers;
            Layer[] excluded = this.layers.ToArrayEx<Layer>();
            if (this.document != null)
            {
                newLayers = this.document.Layers.Cast<Layer>().ToArrayEx<Layer>();
            }
            else
            {
                newLayers = Array.Empty<Layer>();
            }
            IEnumerable<Layer> enumerable = newLayers.Except<Layer>(excluded);
            IEnumerable<Layer> enumerable2 = excluded.Except<Layer>(newLayers);
            IEnumerable<Layer> enumerable3 = newLayers.Intersect<Layer>(excluded);
            foreach (Layer layer in enumerable)
            {
                this.InsertLayer(this.layers.Count, layer);
            }
            foreach (Layer layer2 in enumerable2)
            {
                this.RemoveLayer(layer2);
            }
            int length = newLayers.Length;
            VerticalImageStrip.Item item = null;
            for (int i = 0; i < length; i++)
            {
                int layerIndex = this.RenderSlotToLayerIndex(i);
                VerticalImageStrip.Item itemB = base.ItemIndexToItem(i);
                if (itemB.IsSelected)
                {
                    item = itemB;
                    itemB.IsSelected = false;
                }
                if (itemB.Tag != newLayers[layerIndex])
                {
                    double num5;
                    int num4 = base.Items.IndexOf<VerticalImageStrip.Item>(it => it.Tag == newLayers[layerIndex]);
                    if (num4 == -1)
                    {
                        ExceptionUtil.ThrowInternalErrorException();
                    }
                    VerticalImageStrip.Item itemA = base.ItemIndexToItem(num4);
                    if (itemA.IsSelected)
                    {
                        item = itemA;
                        itemA.IsSelected = false;
                    }
                    base.SwapItems(itemA, itemB);
                    if (!this.renderSlotsBeforeChanging.TryGetValue((Layer) itemB.Tag, out num5))
                    {
                        num5 = Math.Min(i, excluded.Length - 1);
                    }
                    if (itemB.RenderSlot.FinalValue != num5)
                    {
                        itemB.RenderSlot.Value = num5;
                    }
                }
            }
            for (int j = 0; j < newLayers.Length; j++)
            {
                this.layers[j] = newLayers[j];
            }
            base.UpdateItemRenderSlots();
            if (item != null)
            {
                item.IsSelected = true;
            }
            int index = this.LayerIndexToRenderSlot(this.ActiveLayerIndex);
            base.EnsureItemFullyVisible(index);
            base.ForceMouseMove(false);
            base.AllowReorder = true;
        }

        private void OnLayersListChanging()
        {
            base.AllowReorder = false;
            this.layersAndItemsBeforeChanging = this.layers.Select<Layer, KeyValuePair<Layer, VerticalImageStrip.Item>>(l => new KeyValuePair<Layer, VerticalImageStrip.Item>(l, this.layerToItemMap[l])).ToArrayEx<KeyValuePair<Layer, VerticalImageStrip.Item>>();
            this.renderSlotsBeforeChanging = DictionaryUtil.From<Layer, double>(this.layersAndItemsBeforeChanging.Select<KeyValuePair<Layer, VerticalImageStrip.Item>, Layer>(kv => kv.Key), this.layersAndItemsBeforeChanging.Select<KeyValuePair<Layer, VerticalImageStrip.Item>, double>(kv => kv.Value.RenderSlot.Value));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
        }

        private void OnThumbnailUpdated(object sender, ValueEventArgs<Tuple<IThumbnailProvider, ISurface<ColorBgra>>> e)
        {
            if (!base.IsDisposed)
            {
                VerticalImageStrip.Item item;
                OurThumbnailProvider provider = (OurThumbnailProvider) e.Value.Item1;
                Layer key = provider.Layer;
                if (this.layerToItemMap.TryGetValue(key, out item))
                {
                    Image image = item.Image;
                    using (Bitmap bitmap = new Bitmap(e.Value.Item2.Width, e.Value.Item2.Height, e.Value.Item2.Stride, PixelFormat.Format32bppArgb, e.Value.Item2.Scan0))
                    {
                        Image image2 = new Bitmap(bitmap);
                        item.Image = image2;
                    }
                    e.Value.Item2.Dispose();
                    if (image != null)
                    {
                        image.Dispose();
                    }
                    if (item.ImageOpacity.FinalValue != 1.0)
                    {
                        item.ImageOpacity.AnimateValueTo(1.0, 0.5, AnimationTransitionType.SmoothStop);
                    }
                }
            }
        }

        private void ProcessInvalidatedLayers()
        {
            if (!base.IsDisposed)
            {
                Layer[] layerArray;
                ConcurrentSet<Layer> invalidatedLayers = this.invalidatedLayers;
                lock (invalidatedLayers)
                {
                    layerArray = this.invalidatedLayers.ToArrayEx<Layer>();
                    this.invalidatedLayers.Clear();
                }
                foreach (Layer layer in layerArray)
                {
                    this.InvalidateThumbnail(layer);
                }
            }
        }

        private void RefreshLayerThumbnail(Layer layer)
        {
            if (this.layerToItemMap.ContainsKey(layer))
            {
                IRenderer<ColorBgra> layerRenderer = null;
                if ((layerRenderer == null) && (this.documentWorkspace != null))
                {
                    int index = this.document.Layers.IndexOf(layer);
                    if (index != -1)
                    {
                        layerRenderer = this.documentWorkspace.DocumentCanvas.DocumentCanvasLayer.DocumentRenderer.CreateLayerRenderer(index);
                    }
                }
                if (layerRenderer == null)
                {
                    layerRenderer = new SolidColorRendererBgra(layer.Width, layer.Height, ColorBgra.TransparentBlack);
                }
                OurThumbnailProvider updateMe = new OurThumbnailProvider(layerRenderer, layer);
                this.thumbnailManager.QueueThumbnailUpdate(updateMe, base.ThumbEdgeLength, new ValueEventHandler<Tuple<IThumbnailProvider, ISurface<ColorBgra>>>(this.OnThumbnailUpdated));
            }
        }

        private void RemoveLayer(Layer layer)
        {
            int index = this.layers.IndexOf(layer);
            this.RemoveLayerAt(index);
        }

        private void RemoveLayerAt(int layerIndex)
        {
            Layer key = this.layers[layerIndex];
            if (this.activeLayer == key)
            {
                this.ActiveLayer = null;
            }
            this.BeginLayersListChange();
            VerticalImageStrip.Item item = this.layerToItemMap[key];
            item.Tag = null;
            base.RemoveItem(item);
            if (item.Image != null)
            {
                item.Image.Dispose();
            }
            key.Invalidated -= new EventHandler<RectInt32InvalidatedEventArgs>(this.OnLayerInvalidated);
            key.PropertyChanged -= new PropertyEventHandler(this.OnLayerPropertyChanged);
            this.layers.RemoveAt(layerIndex);
            this.layerToItemMap.Remove(key);
            OurThumbnailProvider nukeMe = new OurThumbnailProvider(null, key);
            this.thumbnailManager.RemoveFromQueue(nukeMe);
            this.EndLayersListChange();
        }

        internal int RenderSlotToLayerIndex(int renderSlot) => 
            ((this.layers.Count - renderSlot) - 1);

        public Layer ActiveLayer
        {
            get
            {
                base.VerifyAccess();
                return this.activeLayer;
            }
            set
            {
                base.VerifyAccess();
                if (value != this.activeLayer)
                {
                    VerticalImageStrip.Item item;
                    VerticalImageStrip.Item item2;
                    if ((this.activeLayer != null) && this.layerToItemMap.TryGetValue(this.activeLayer, out item))
                    {
                        item.IsSelected = false;
                    }
                    if ((value != null) && this.layerToItemMap.TryGetValue(value, out item2))
                    {
                        item2.IsSelected = true;
                        base.EnsureItemFullyVisible(item2);
                    }
                    this.activeLayer = value;
                }
            }
        }

        public int ActiveLayerIndex =>
            this.layers.IndexOf(this.ActiveLayer);

        public PaintDotNet.Document Document
        {
            get
            {
                base.VerifyAccess();
                return this.document;
            }
            set
            {
                base.VerifyAccess();
                if (value != this.document)
                {
                    this.ActiveLayer = null;
                    if (this.layersListChangingCount != 0)
                    {
                        ExceptionUtil.ThrowInternalErrorException("this.layersListChangingCount != 0");
                    }
                    this.BeginLayersListChange();
                    this.thumbnailManager.ClearQueue();
                    if (this.document != null)
                    {
                        for (int i = this.layers.Count - 1; i >= 0; i--)
                        {
                            this.RemoveLayerAt(i);
                        }
                        this.document.Layers.Changing -= new EventHandler(this.OnDocumentLayersChanging);
                        this.document.Layers.Changed -= new EventHandler(this.OnDocumentLayersChanged);
                        this.document.Layers.Inserted -= new IndexEventHandler(this.OnDocumentLayersInserted);
                        this.document.Layers.RemovingAt -= new IndexEventHandler(this.OnDocumentLayersRemovingAt);
                    }
                    this.document = value;
                    if (this.document != null)
                    {
                        this.document.Layers.Changing += new EventHandler(this.OnDocumentLayersChanging);
                        this.document.Layers.Changed += new EventHandler(this.OnDocumentLayersChanged);
                        this.document.Layers.Inserted += new IndexEventHandler(this.OnDocumentLayersInserted);
                        this.document.Layers.RemovingAt += new IndexEventHandler(this.OnDocumentLayersRemovingAt);
                        for (int j = 0; j < this.document.Layers.Count; j++)
                        {
                            this.InsertLayer(j, (Layer) this.document.Layers[j]);
                        }
                    }
                    this.EndLayersListChange();
                }
            }
        }

        public PaintDotNet.Controls.DocumentWorkspace DocumentWorkspace
        {
            get => 
                this.documentWorkspace;
            set
            {
                this.BeginLayersListChange();
                this.documentWorkspace = value;
                this.EndLayersListChange();
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly LayersStrip.<>c <>9 = new LayersStrip.<>c();
            public static Func<KeyValuePair<Layer, VerticalImageStrip.Item>, Layer> <>9__17_1;
            public static Func<KeyValuePair<Layer, VerticalImageStrip.Item>, double> <>9__17_2;

            internal Layer <OnLayersListChanging>b__17_1(KeyValuePair<Layer, VerticalImageStrip.Item> kv) => 
                kv.Key;

            internal double <OnLayersListChanging>b__17_2(KeyValuePair<Layer, VerticalImageStrip.Item> kv) => 
                kv.Value.RenderSlot.Value;
        }

        private sealed class OurThumbnailProvider : IThumbnailProvider
        {
            private PaintDotNet.Layer layer;
            private IRenderer<ColorBgra> layerRenderer;

            public OurThumbnailProvider(IRenderer<ColorBgra> layerRenderer, PaintDotNet.Layer layer)
            {
                Validate.IsNotNull<PaintDotNet.Layer>(layer, "layer");
                this.layerRenderer = layerRenderer;
                this.layer = layer;
            }

            public IRenderer<ColorBgra> CreateThumbnailRenderer(int maxEdgeLength)
            {
                SizeInt32 size = ThumbnailHelpers.ComputeThumbnailSize(this.layerRenderer.Size<ColorBgra>(), maxEdgeLength);
                IRenderer<ColorBgra> sourceLHS = RendererBgra.Checkers(size);
                IRenderer<ColorBgra> sourceRHS = this.layerRenderer.ResizeSuperSampling(size);
                return sourceLHS.DrawBlend(CompositionOps.Normal.Static, sourceRHS);
            }

            public override bool Equals(object obj) => 
                (((obj != null) && (obj is LayersStrip.OurThumbnailProvider)) && this.layer.Equals(((LayersStrip.OurThumbnailProvider) obj).layer));

            public override int GetHashCode() => 
                this.layer.GetHashCode();

            public PaintDotNet.Layer Layer =>
                this.layer;
        }
    }
}

