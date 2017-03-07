namespace PaintDotNet.Canvas
{
    using PaintDotNet;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Imaging;
    using PaintDotNet.Rendering;
    using System;
    using System.Collections.Concurrent;

    internal sealed class DocumentRenderer : BitmapSource<ColorBgra32>
    {
        private Document document;
        private ConcurrentDictionary<Layer, DocumentLayerOverlay> overlays;
        private DocumentCanvasLayer owner;

        public DocumentRenderer(DocumentCanvasLayer owner) : base(owner.Document.Size())
        {
            Validate.IsNotNull<DocumentCanvasLayer>(owner, "owner");
            this.owner = owner;
            this.document = this.owner.Document;
            Validate.IsNotNull<Document>(this.document, "this.document");
            this.overlays = new ConcurrentDictionary<Layer, DocumentLayerOverlay>();
        }

        public IRenderer<ColorBgra> CreateLayerRenderer(int layerIndex)
        {
            DocumentLayerOverlay overlay;
            Layer key = (Layer) this.document.Layers[layerIndex];
            if (this.overlays.TryGetValue(key, out overlay))
            {
                return overlay;
            }
            return key.CreateIsolatedRenderer();
        }

        public IRenderer<ColorBgra> CreateRenderer()
        {
            IRenderer<ColorBgra> sourceLHS = null;
            for (int i = 0; i < this.document.Layers.Count; i++)
            {
                DocumentLayerOverlay overlay;
                Layer key = (Layer) this.document.Layers[i];
                if (this.overlays.TryGetValue(key, out overlay))
                {
                    if (key.Visible && (key.Opacity != 0))
                    {
                        CompositionOp op = LayerBlendModeUtil.CreateCompositionOp(key.BlendMode, key.Opacity);
                        sourceLHS = sourceLHS.DrawBlend(op, overlay);
                    }
                }
                else
                {
                    sourceLHS = ((Layer) this.document.Layers[i]).CreateRenderer(sourceLHS);
                }
            }
            if (sourceLHS == null)
            {
                sourceLHS = new SolidColorRendererBgra(this.Width, this.Height, ColorBgra.Zero);
            }
            return sourceLHS;
        }

        protected override void Dispose(bool disposing)
        {
            this.owner = null;
            this.document = null;
            this.overlays = null;
            base.Dispose(disposing);
        }

        protected override void OnCopyPixels<TBitmapLockData>(TBitmapLockData dst, PointInt32 srcOffset) where TBitmapLockData: IBitmapLockData<ColorBgra32>
        {
            using (SharedSurface<ColorBgra> surface = new SharedSurface<ColorBgra>(dst.Scan0, dst.Size.Width, dst.Size.Height, dst.Stride))
            {
                this.Render(surface, srcOffset);
            }
        }

        public void RemoveLayerOverlay(Layer layer)
        {
            DocumentLayerOverlay overlay;
            if (!this.overlays.TryRemove(layer, out overlay))
            {
                throw new InvalidOperationException();
            }
        }

        public void Render(ISurface<ColorBgra> dst, PointInt32 renderOffset)
        {
            this.CreateRenderer().Render(dst, renderOffset);
        }

        public void ReplaceLayerOverlay(Layer layer, DocumentLayerOverlay overlay)
        {
            this.overlays[layer] = overlay;
        }

        public void SetLayerOverlay(Layer layer, DocumentLayerOverlay overlay)
        {
            if (!this.overlays.TryAdd(layer, overlay))
            {
                throw new InvalidOperationException();
            }
        }

        public int Height =>
            this.document.Height;

        public int Width =>
            this.document.Width;
    }
}

