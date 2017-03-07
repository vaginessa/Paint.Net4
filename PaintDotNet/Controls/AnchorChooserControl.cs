namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Drawing;
    using PaintDotNet.Resources;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;

    internal class AnchorChooserControl : GdiBufferedAnimationControl
    {
        private PaintDotNet.AnchorEdge anchorEdge;
        private Dictionary<PaintDotNet.AnchorEdge, Point> anchorEdgeToXy;
        private Image centerImage;
        private bool drawHotPush;
        private Point hotAnchorButton = new Point(-1, -1);
        private MouseButtons mouseButtonDown;
        private bool mouseDown;
        private Point mouseDownPoint;
        private PaintDotNet.AnchorEdge[][] xyToAnchorEdge;

        [field: CompilerGenerated]
        public event EventHandler AnchorEdgeChanged;

        public AnchorChooserControl()
        {
            base.ResizeRedraw = true;
            this.centerImage = PdnResources.GetImageResource("Images.AnchorChooserControl.AnchorImage.png").Reference;
            PaintDotNet.AnchorEdge[][] edgeArrayArray1 = new PaintDotNet.AnchorEdge[3][];
            PaintDotNet.AnchorEdge[] edgeArray1 = new PaintDotNet.AnchorEdge[3];
            edgeArray1[1] = PaintDotNet.AnchorEdge.Top;
            edgeArray1[2] = PaintDotNet.AnchorEdge.TopRight;
            edgeArrayArray1[0] = edgeArray1;
            edgeArrayArray1[1] = new PaintDotNet.AnchorEdge[] { PaintDotNet.AnchorEdge.Left };
            edgeArrayArray1[2] = new PaintDotNet.AnchorEdge[] { PaintDotNet.AnchorEdge.BottomLeft };
            this.xyToAnchorEdge = edgeArrayArray1;
            this.anchorEdgeToXy = new Dictionary<PaintDotNet.AnchorEdge, Point>();
            this.anchorEdgeToXy.Add(PaintDotNet.AnchorEdge.TopLeft, new Point(0, 0));
            this.anchorEdgeToXy.Add(PaintDotNet.AnchorEdge.Top, new Point(1, 0));
            this.anchorEdgeToXy.Add(PaintDotNet.AnchorEdge.TopRight, new Point(2, 0));
            this.anchorEdgeToXy.Add(PaintDotNet.AnchorEdge.Left, new Point(0, 1));
            this.anchorEdgeToXy.Add(PaintDotNet.AnchorEdge.Middle, new Point(1, 1));
            this.anchorEdgeToXy.Add(PaintDotNet.AnchorEdge.Right, new Point(2, 1));
            this.anchorEdgeToXy.Add(PaintDotNet.AnchorEdge.BottomLeft, new Point(0, 2));
            this.anchorEdgeToXy.Add(PaintDotNet.AnchorEdge.Bottom, new Point(1, 2));
            this.anchorEdgeToXy.Add(PaintDotNet.AnchorEdge.BottomRight, new Point(2, 2));
            base.AnimationDurationMs = 110;
        }

        public override Size GetPreferredSize(Size proposedSize) => 
            UIUtil.ScaleSize(0x51, 0x51);

        protected virtual void OnAnchorEdgeChanged()
        {
            this.AnchorEdgeChanged.Raise(this);
        }

        protected override void OnGdiPaintFrame(GdiPaintContext ctx)
        {
            using (Graphics graphics = Graphics.FromHdc(ctx.Hdc))
            {
                using (PaintEventArgs args = new PaintEventArgs(graphics, ctx.UpdateRect.ToGdipRectangle()))
                {
                    this.OnGdipPaint(args);
                }
            }
        }

        private void OnGdipPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(this.BackColor);
            Point point = this.anchorEdgeToXy[this.anchorEdge];
            double num = ((double) base.Width) / 2.0;
            double num2 = ((double) base.Height) / 2.0;
            using (Pen pen = new Pen(SystemColors.WindowText, ((base.Width + base.Height) / 2f) / 64f))
            {
                AdjustableArrowCap cap = new AdjustableArrowCap(((float) base.Width) / 32f, ((float) base.Height) / 32f, true);
                pen.CustomEndCap = cap;
                Point point2 = base.PointToClient(Control.MousePosition);
                int num3 = (int) Math.Floor((double) ((point2.X * 3f) / ((float) base.Width)));
                int num4 = (int) Math.Floor((double) ((point2.Y * 3f) / ((float) base.Height)));
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        PaintDotNet.AnchorEdge edge = this.xyToAnchorEdge[i][j];
                        Point point3 = this.anchorEdgeToXy[edge];
                        Point point4 = new Point(point3.X - point.X, point3.Y - point.Y);
                        int x = (base.Width * j) / 3;
                        int y = (base.Height * i) / 3;
                        int num9 = Math.Min((int) (base.Width - 1), (int) ((base.Width * (j + 1)) / 3));
                        int num10 = Math.Min((int) (base.Height - 1), (int) ((base.Height * (i + 1)) / 3));
                        int width = num9 - x;
                        int height = num10 - y;
                        if ((point4.X == 0) && (point4.Y == 0))
                        {
                            ButtonRenderer.DrawButton(e.Graphics, new Rectangle(x, y, width, height), PushButtonState.Pressed);
                            e.Graphics.DrawImage(this.centerImage, (int) (x + 3), (int) (y + 3), (int) (width - 6), (int) (height - 6));
                        }
                        else
                        {
                            PushButtonState pressed;
                            if ((this.drawHotPush && (j == this.hotAnchorButton.X)) && (i == this.hotAnchorButton.Y))
                            {
                                pressed = PushButtonState.Pressed;
                            }
                            else
                            {
                                pressed = PushButtonState.Normal;
                                if ((!this.mouseDown && (num3 == j)) && (num4 == i))
                                {
                                    pressed = PushButtonState.Hot;
                                }
                            }
                            ButtonRenderer.DrawButton(e.Graphics, new Rectangle(x, y, width, height), pressed);
                            if (((point4.X <= 1) && (point4.X >= -1)) && ((point4.Y <= 1) && (point4.Y >= -1)))
                            {
                                double num13 = Math.Sqrt((double) ((point4.X * point4.X) + (point4.Y * point4.Y)));
                                double num14 = ((double) point4.X) / num13;
                                double num15 = ((double) point4.Y) / num13;
                                Point point5 = new Point((x + num9) / 2, (y + num10) / 2);
                                Point point6 = new Point(point5.X - ((width / 4) * point4.X), point5.Y - ((height / 4) * point4.Y));
                                Point point7 = new Point(point6.X + ((int) ((((double) width) / 2.0) * num14)), point6.Y + ((int) ((((double) height) / 2.0) * num15)));
                                PixelOffsetMode pixelOffsetMode = e.Graphics.PixelOffsetMode;
                                e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
                                e.Graphics.DrawLine(pen, point6, point7);
                                e.Graphics.PixelOffsetMode = pixelOffsetMode;
                            }
                        }
                    }
                }
            }
            base.OnPaint(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (!this.mouseDown)
            {
                this.mouseDown = true;
                this.mouseButtonDown = e.Button;
                this.mouseDownPoint = new Point(e.X, e.Y);
                int x = (e.X * 3) / base.Width;
                int y = (e.Y * 3) / base.Height;
                this.hotAnchorButton = new Point(x, y);
                this.drawHotPush = true;
                base.Invalidate();
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (this.mouseDown && (e.Button == this.mouseButtonDown))
            {
                int num = (e.X * 3) / base.Width;
                int num2 = (e.Y * 3) / base.Height;
                this.drawHotPush = (num == this.hotAnchorButton.X) && (num2 == this.hotAnchorButton.Y);
            }
            base.Invalidate();
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (this.mouseDown && (e.Button == this.mouseButtonDown))
            {
                int index = (e.X * 3) / base.Width;
                int num2 = (e.Y * 3) / base.Height;
                if ((((index == this.hotAnchorButton.X) && (num2 == this.hotAnchorButton.Y)) && ((index >= 0) && (index <= 2))) && ((num2 >= 0) && (num2 <= 2)))
                {
                    PaintDotNet.AnchorEdge edge = this.xyToAnchorEdge[num2][index];
                    this.AnchorEdge = edge;
                    base.Invalidate();
                }
            }
            this.drawHotPush = false;
            this.mouseDown = false;
            base.OnMouseUp(e);
        }

        [DefaultValue(0)]
        public PaintDotNet.AnchorEdge AnchorEdge
        {
            get => 
                this.anchorEdge;
            set
            {
                if (this.anchorEdge != value)
                {
                    this.anchorEdge = value;
                    this.OnAnchorEdgeChanged();
                    base.Invalidate();
                    this.QueueUpdate();
                }
            }
        }
    }
}

