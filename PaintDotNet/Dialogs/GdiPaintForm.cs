namespace PaintDotNet.Dialogs
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal class GdiPaintForm : PdnBaseFormInternal, IGdiPaintHost
    {
        private GdiPaintHandler gdiPaintHandler;

        public GdiPaintForm()
        {
            this.gdiPaintHandler = new GdiPaintHandler(this);
        }

        protected override void Dispose(bool disposing)
        {
            DisposableUtil.Free<GdiPaintHandler>(ref this.gdiPaintHandler);
            base.Dispose(disposing);
        }

        protected virtual void OnBeforeGdiPaint(RectInt32 updateRect, ref bool cancel)
        {
        }

        protected virtual void OnGdiPaint(GdiPaintContext ctx)
        {
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            this.gdiPaintHandler.RelayHandleCreated(e);
            base.OnHandleCreated(e);
        }

        protected sealed override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
        }

        protected sealed override void OnPaintBackground(PaintEventArgs pevent)
        {
            base.OnPaintBackground(pevent);
        }

        void IGdiPaintHost.Invalidate(bool includeChildren)
        {
            base.Invalidate(includeChildren);
        }

        void IGdiPaintHost.OnBeforeGdiPaint(RectInt32 updateRect, ref bool cancel)
        {
            this.OnBeforeGdiPaint(updateRect, ref cancel);
        }

        void IGdiPaintHost.OnGdiPaint(GdiPaintContext ctx)
        {
            this.OnGdiPaint(ctx);
        }

        void IGdiPaintHost.SetStyle(ControlStyles style, bool value)
        {
            base.SetStyle(style, value);
        }

        protected override void WndProc(ref Message m)
        {
            if ((this.gdiPaintHandler == null) || !this.gdiPaintHandler.RelayWndProc(ref m))
            {
                base.WndProc(ref m);
            }
        }

        public bool IsBuffered
        {
            get => 
                this.gdiPaintHandler.IsBuffered;
            set
            {
                this.gdiPaintHandler.IsBuffered = value;
            }
        }

        Size IGdiPaintHost.ClientSize =>
            base.ClientSize;

        IntPtr IGdiPaintHost.Handle =>
            base.Handle;

        bool IGdiPaintHost.IsHandleCreated =>
            base.IsHandleCreated;

        protected bool ShowRepaints
        {
            get => 
                this.gdiPaintHandler.ShowRepaints;
            set
            {
                this.gdiPaintHandler.ShowRepaints = value;
            }
        }
    }
}

