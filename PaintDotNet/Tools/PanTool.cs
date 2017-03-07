namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal sealed class PanTool : PaintDotNet.Tools.Tool
    {
        private Cursor cursorMouseDown;
        private Cursor cursorMouseInvalid;
        private Cursor cursorMouseUp;
        private int ignoreMouseMove;
        private Point lastMouseXY;
        private bool tracking;

        public PanTool(DocumentWorkspace documentWorkspace) : base(documentWorkspace, PdnResources.GetImageResource("Icons.PanToolIcon.png"), PdnResources.GetString("PanTool.Name"), PdnResources.GetString("PanTool.HelpText"), 'h', false, ToolBarConfigItems.None)
        {
            base.IsAutoScrollEnabled = false;
            this.tracking = false;
        }

        protected override void OnActivate()
        {
            this.cursorMouseDown = PdnResources.GetCursor("Cursors.PanToolCursorMouseDown.cur");
            this.cursorMouseUp = PdnResources.GetCursor("Cursors.PanToolCursor.cur");
            this.cursorMouseInvalid = PdnResources.GetCursor("Cursors.PanToolCursorInvalid.cur");
            base.Cursor = this.cursorMouseUp;
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            DisposableUtil.Free<Cursor>(ref this.cursorMouseDown);
            DisposableUtil.Free<Cursor>(ref this.cursorMouseUp);
            DisposableUtil.Free<Cursor>(ref this.cursorMouseInvalid);
            base.OnDeactivate();
        }

        protected override void OnMouseDown(MouseEventArgsF e)
        {
            base.OnMouseDown(e);
            this.lastMouseXY = new Point(e.X, e.Y);
            this.tracking = true;
            if (base.CanPan())
            {
                base.Cursor = this.cursorMouseDown;
            }
            else
            {
                base.Cursor = this.cursorMouseInvalid;
            }
        }

        protected override void OnMouseMove(MouseEventArgsF e)
        {
            base.OnMouseMove(e);
            if (this.ignoreMouseMove > 0)
            {
                this.ignoreMouseMove--;
            }
            else if (this.tracking)
            {
                Point point = new Point(e.X, e.Y);
                Size size = new Size(point.X - this.lastMouseXY.X, point.Y - this.lastMouseXY.Y);
                if ((size.Width != 0) || (size.Height != 0))
                {
                    PointDouble documentScrollPosition = base.DocumentWorkspace.DocumentScrollPosition;
                    PointDouble num2 = new PointDouble(documentScrollPosition.X - size.Width, documentScrollPosition.Y - size.Height);
                    this.ignoreMouseMove++;
                    base.DocumentWorkspace.DocumentScrollPosition = num2;
                    this.lastMouseXY = point;
                    this.lastMouseXY.X -= size.Width;
                    this.lastMouseXY.Y -= size.Height;
                }
            }
            else if (base.CanPan())
            {
                base.Cursor = this.cursorMouseUp;
            }
            else
            {
                base.Cursor = this.cursorMouseInvalid;
            }
        }

        protected override void OnMouseUp(MouseEventArgsF e)
        {
            base.OnMouseUp(e);
            if (base.CanPan())
            {
                base.Cursor = this.cursorMouseUp;
            }
            else
            {
                base.Cursor = this.cursorMouseInvalid;
            }
            this.tracking = false;
        }
    }
}

