namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Canvas;
    using PaintDotNet.Controls;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using System;
    using System.Windows.Forms;

    internal sealed class ZoomTool : PaintDotNet.Tools.Tool
    {
        private Cursor cursorZoom;
        private Cursor cursorZoomIn;
        private Cursor cursorZoomOut;
        private Cursor cursorZoomPan;
        private PointInt32 downPt;
        private PointInt32 lastPt;
        private MouseButtons mouseDown;
        private bool moveOffsetMode;
        private Selection outline;
        private SelectionCanvasLayer outlineRenderer;
        private RectInt32 rect;

        public ZoomTool(DocumentWorkspace documentWorkspace) : base(documentWorkspace, PdnResources.GetImageResource("Icons.ZoomToolIcon.png"), PdnResources.GetString("ZoomTool.Name"), PdnResources.GetString("ZoomTool.HelpText"), 'z', false, ToolBarConfigItems.None)
        {
            this.rect = RectInt32.Zero;
            this.mouseDown = MouseButtons.None;
        }

        protected override void OnActivate()
        {
            this.cursorZoom = PdnResources.GetCursor("Cursors.ZoomToolCursor.cur");
            this.cursorZoomIn = PdnResources.GetCursor("Cursors.ZoomInToolCursor.cur");
            this.cursorZoomOut = PdnResources.GetCursor("Cursors.ZoomOutToolCursor.cur");
            this.cursorZoomPan = PdnResources.GetCursor("Cursors.ZoomOutToolCursor.cur");
            base.Cursor = this.cursorZoom;
            base.OnActivate();
            this.outline = new Selection();
            this.outlineRenderer = new SelectionCanvasLayer();
            this.outlineRenderer.Selection = this.outline;
            base.DocumentCanvas.CanvasLayers.Add(this.outlineRenderer);
        }

        protected override void OnDeactivate()
        {
            DisposableUtil.Free<Cursor>(ref this.cursorZoom);
            DisposableUtil.Free<Cursor>(ref this.cursorZoomIn);
            DisposableUtil.Free<Cursor>(ref this.cursorZoomOut);
            DisposableUtil.Free<Cursor>(ref this.cursorZoomPan);
            base.DocumentCanvas.CanvasLayers.Remove(this.outlineRenderer);
            DisposableUtil.Free<SelectionCanvasLayer>(ref this.outlineRenderer);
            base.OnDeactivate();
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (!e.Handled && (this.mouseDown != MouseButtons.None))
            {
                e.Handled = true;
            }
            base.OnKeyPress(e);
        }

        protected override void OnMouseDown(MouseEventArgsF e)
        {
            base.OnMouseDown(e);
            if (this.mouseDown != MouseButtons.None)
            {
                this.moveOffsetMode = true;
            }
            else
            {
                switch (e.Button)
                {
                    case MouseButtons.Left:
                        base.Cursor = this.cursorZoomIn;
                        break;

                    case MouseButtons.Right:
                        base.Cursor = this.cursorZoomOut;
                        break;

                    case MouseButtons.Middle:
                        base.Cursor = this.cursorZoomPan;
                        break;
                }
                this.mouseDown = e.Button;
                this.lastPt = new PointInt32(e.X, e.Y);
                this.downPt = this.lastPt;
                this.OnMouseMove(e);
            }
        }

        protected override void OnMouseMove(MouseEventArgsF e)
        {
            base.OnMouseMove(e);
            PointInt32 b = new PointInt32(e.X, e.Y);
            if (this.moveOffsetMode)
            {
                VectorInt32 num2 = (VectorInt32) (b - this.lastPt);
                this.downPt.X += num2.X;
                this.downPt.Y += num2.Y;
            }
            if ((e.Button == MouseButtons.Left) && (this.mouseDown == MouseButtons.Left))
            {
                VectorDouble num3 = (VectorDouble) (b - this.downPt);
                if (num3.Length >= 10.0)
                {
                    goto Label_00B1;
                }
            }
            if (!this.rect.HasPositiveArea)
            {
                if ((e.Button == MouseButtons.Middle) && (this.mouseDown == MouseButtons.Middle))
                {
                    PointDouble documentScrollPosition = base.DocumentWorkspace.DocumentScrollPosition;
                    documentScrollPosition.X += b.X - this.lastPt.X;
                    documentScrollPosition.Y += b.Y - this.lastPt.Y;
                    base.DocumentWorkspace.DocumentScrollPosition = documentScrollPosition;
                    base.Update();
                }
                else
                {
                    this.rect = RectInt32.Zero;
                }
                goto Label_0173;
            }
        Label_00B1:
            this.rect = RectInt32Util.FromPixelPoints(this.downPt, b);
            this.rect = RectInt32.Intersect(this.rect, base.ActiveLayer.Bounds());
            this.UpdateDrawnRect();
        Label_0173:
            this.lastPt = b;
        }

        protected override void OnMouseUp(MouseEventArgsF e)
        {
            base.OnMouseUp(e);
            this.OnMouseMove(e);
            bool flag = true;
            base.Cursor = this.cursorZoom;
            if (this.moveOffsetMode)
            {
                this.moveOffsetMode = false;
                flag = false;
            }
            else if ((this.mouseDown == MouseButtons.Left) || (this.mouseDown == MouseButtons.Right))
            {
                RectInt32 rect = this.rect;
                this.rect = RectInt32.Zero;
                this.UpdateDrawnRect();
                if (e.Button == MouseButtons.Left)
                {
                    VectorDouble num2 = new VectorDouble((double) rect.Width, (double) rect.Height);
                    if (num2.Length < 10.0)
                    {
                        base.DocumentWorkspace.ZoomIn();
                    }
                    else
                    {
                        base.DocumentWorkspace.ZoomToRectangle(rect);
                    }
                }
                else
                {
                    base.DocumentWorkspace.ZoomOut();
                }
                this.outline.Reset();
            }
            if (flag)
            {
                this.mouseDown = MouseButtons.None;
            }
        }

        private void UpdateDrawnRect()
        {
            if (this.rect.HasPositiveArea)
            {
                this.outline.PerformChanging();
                this.outline.Reset();
                this.outline.SetContinuation(this.rect, SelectionCombineMode.Replace);
                this.outline.CommitContinuation();
                this.outline.PerformChanged();
                base.Update();
            }
        }
    }
}

