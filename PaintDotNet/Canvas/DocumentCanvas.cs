namespace PaintDotNet.Canvas
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class DocumentCanvas : PaintDotNet.Canvas.Canvas
    {
        private BackgroundCanvasLayer backgroundCanvasLayer = new BackgroundCanvasLayer();
        private BorderCanvasLayer borderCanvasLayer;
        private CheckerboardCanvasLayer checkerboardCanvasLayer;
        private PaintDotNet.Document document;
        private PaintDotNet.Canvas.DocumentCanvasLayer documentCanvasLayer;
        private PixelGridCanvasLayer pixelGridCanvasLayer;
        private PaintDotNet.Canvas.SelectionCanvasLayer selectionCanvasLayer;

        [field: CompilerGenerated]
        public event EventHandler CompositionIdle;

        public DocumentCanvas()
        {
            base.CanvasLayers.Insert(0, this.backgroundCanvasLayer);
            this.borderCanvasLayer = new BorderCanvasLayer();
            base.CanvasLayers.Insert(1, this.borderCanvasLayer);
            this.checkerboardCanvasLayer = new CheckerboardCanvasLayer();
            base.CanvasLayers.Insert(2, this.checkerboardCanvasLayer);
            this.documentCanvasLayer = new PaintDotNet.Canvas.DocumentCanvasLayer();
            this.documentCanvasLayer.CompositionIdle += new EventHandler(this.OnDocumentCanvasLayerCompositionIdle);
            base.CanvasLayers.Insert(3, this.documentCanvasLayer);
            this.pixelGridCanvasLayer = new PixelGridCanvasLayer();
            base.CanvasLayers.Insert(4, this.pixelGridCanvasLayer);
            this.selectionCanvasLayer = new PaintDotNet.Canvas.SelectionCanvasLayer();
            base.CanvasLayers.Insert(5, this.selectionCanvasLayer);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.documentCanvasLayer != null)
                {
                    this.documentCanvasLayer.Document = null;
                    this.documentCanvasLayer.CompositionIdle -= new EventHandler(this.OnDocumentCanvasLayerCompositionIdle);
                }
                base.CanvasLayers.Clear();
                DisposableUtil.Free<PixelGridCanvasLayer>(ref this.pixelGridCanvasLayer);
                DisposableUtil.Free<PaintDotNet.Canvas.SelectionCanvasLayer>(ref this.selectionCanvasLayer);
                DisposableUtil.Free<PaintDotNet.Canvas.DocumentCanvasLayer>(ref this.documentCanvasLayer);
                DisposableUtil.Free<CheckerboardCanvasLayer>(ref this.checkerboardCanvasLayer);
                DisposableUtil.Free<BorderCanvasLayer>(ref this.borderCanvasLayer);
                DisposableUtil.Free<BackgroundCanvasLayer>(ref this.backgroundCanvasLayer);
            }
            base.Dispose(disposing);
        }

        private void OnCompositionIdle()
        {
            this.CompositionIdle.Raise(this);
        }

        private void OnDocumentCanvasLayerCompositionIdle(object sender, EventArgs e)
        {
            this.OnCompositionIdle();
        }

        public void PreRenderSync(CanvasView canvasView)
        {
            base.VerifyAccess();
            this.documentCanvasLayer.PreRenderSync(canvasView);
        }

        public void RemoveLayerOverlay(Layer layer, DocumentLayerOverlay overlay, RectInt32? invalidateRect = new RectInt32?())
        {
            this.documentCanvasLayer.RemoveLayerOverlay(layer, overlay, invalidateRect);
        }

        public void ReplaceLayerOverlay(Layer layer, DocumentLayerOverlay oldOverlay, DocumentLayerOverlay newOverlay, RectInt32? invalidateRect = new RectInt32?())
        {
            this.documentCanvasLayer.ReplaceLayerOverlay(layer, oldOverlay, newOverlay, invalidateRect);
        }

        public void SetLayerOverlay(Layer layer, DocumentLayerOverlay overlay, RectInt32? invalidateRect = new RectInt32?())
        {
            this.documentCanvasLayer.SetLayerOverlay(layer, overlay, invalidateRect);
        }

        public PaintDotNet.Document Document
        {
            get => 
                this.document;
            set
            {
                base.VerifyAccess();
                if (value != this.document)
                {
                    this.document = value;
                    this.documentCanvasLayer.Document = this.document;
                    if (this.document != null)
                    {
                        base.CanvasSize = new SizeDouble((double) this.document.Width, (double) this.document.Height);
                    }
                    else
                    {
                        base.CanvasSize = new SizeDouble(0.0, 0.0);
                    }
                }
            }
        }

        internal PaintDotNet.Canvas.DocumentCanvasLayer DocumentCanvasLayer =>
            this.documentCanvasLayer;

        public PointDouble MouseLocation
        {
            get => 
                this.documentCanvasLayer.MouseLocation;
            set
            {
                base.VerifyAccess();
                this.documentCanvasLayer.MouseLocation = value;
            }
        }

        public PaintDotNet.Selection Selection
        {
            get => 
                this.selectionCanvasLayer.Selection;
            set
            {
                this.selectionCanvasLayer.Selection = value;
            }
        }

        internal PaintDotNet.Canvas.SelectionCanvasLayer SelectionCanvasLayer =>
            this.selectionCanvasLayer;
    }
}

