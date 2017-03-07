namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.VisualStyling;
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;

    internal sealed class ArrowButton : PdnPushButtonBase
    {
        private System.Windows.Forms.ArrowDirection arrowDirection = System.Windows.Forms.ArrowDirection.Right;
        private Image arrowImage;
        private float arrowOutlineWidth = 1f;
        private RenderArgs backBuffer;
        private Surface backBufferSurface;
        private bool forcedPushed;
        private PenBrushCache penBrushCache;
        private bool reverseArrowColors;
        private bool showVectorChevron;

        public ArrowButton()
        {
            this.BackColor = Color.Transparent;
            this.penBrushCache = PenBrushCache.ThreadInstance;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.backBuffer != null)
                {
                    this.backBuffer.Dispose();
                    this.backBuffer = null;
                }
                if (this.backBufferSurface != null)
                {
                    this.backBufferSurface.Dispose();
                    this.backBufferSurface = null;
                }
            }
            base.Dispose(disposing);
        }

        protected override int GetAnimationDuration(PdnPushButtonState fromState, PdnPushButtonState toState) => 
            0;

        private void OnPaintButtonImpl(Graphics g, PdnPushButtonState state, bool drawFocusCues, bool drawKeyboardCues)
        {
            HighlightState hover;
            Color black;
            Color white;
            PointF tf;
            PointF tf2;
            PointF tf3;
            switch (state)
            {
                case PdnPushButtonState.Normal:
                case PdnPushButtonState.Default:
                case PdnPushButtonState.DefaultAnimate:
                    hover = HighlightState.Default;
                    black = Color.Black;
                    white = Color.White;
                    break;

                case PdnPushButtonState.Hot:
                    hover = HighlightState.Hover;
                    black = Color.Blue;
                    white = Color.White;
                    break;

                case PdnPushButtonState.Pressed:
                    hover = HighlightState.Checked;
                    black = Color.Blue;
                    white = Color.White;
                    break;

                case PdnPushButtonState.Disabled:
                    hover = HighlightState.Disabled;
                    black = Color.Gray;
                    white = Color.Black;
                    break;

                default:
                    throw ExceptionUtil.InvalidEnumArgumentException<PdnPushButtonState>(state, "state");
            }
            if (!base.GetStyle(ControlStyles.SupportsTransparentBackColor) || (this.BackColor.A >= 0xff))
            {
                g.FillRectangle(this.penBrushCache.GetSolidBrush(this.BackColor), base.ClientRectangle);
            }
            SelectionHighlight.DrawBackground(g, this.penBrushCache, base.ClientRectangle, hover);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int num = Math.Min((int) (base.ClientSize.Width - 6), (int) (base.ClientSize.Height - 6)) - 1;
            switch (this.arrowDirection)
            {
                case System.Windows.Forms.ArrowDirection.Right:
                    tf = new PointF((float) (base.ClientSize.Width - 3), (float) (base.ClientSize.Height / 2));
                    tf2 = new PointF(3f, (float) ((base.ClientSize.Height - num) / 2));
                    tf3 = new PointF(3f, (float) ((base.ClientSize.Height + num) / 2));
                    break;

                case System.Windows.Forms.ArrowDirection.Down:
                    tf = new PointF((float) (base.ClientSize.Width / 2), (float) ((base.ClientSize.Height + num) / 2));
                    tf2 = new PointF((float) ((base.ClientSize.Width - num) / 2), (float) ((base.ClientSize.Height - num) / 2));
                    tf3 = new PointF((float) ((base.ClientSize.Width + num) / 2), (float) ((base.ClientSize.Height - num) / 2));
                    break;

                case System.Windows.Forms.ArrowDirection.Left:
                    tf = new PointF(3f, (float) (base.ClientSize.Height / 2));
                    tf2 = new PointF((float) (base.ClientSize.Width - 3), (float) ((base.ClientSize.Height - num) / 2));
                    tf3 = new PointF((float) (base.ClientSize.Width - 3), (float) ((base.ClientSize.Height + num) / 2));
                    break;

                case System.Windows.Forms.ArrowDirection.Up:
                    tf = new PointF((float) (base.ClientSize.Width / 2), (float) ((base.ClientSize.Height - num) / 2));
                    tf2 = new PointF((float) ((base.ClientSize.Width - num) / 2), (float) ((base.ClientSize.Height + num) / 2));
                    tf3 = new PointF((float) ((base.ClientSize.Width + num) / 2), (float) ((base.ClientSize.Height + num) / 2));
                    break;

                default:
                    throw ExceptionUtil.InvalidEnumArgumentException<System.Windows.Forms.ArrowDirection>(this.arrowDirection, "this.arrowDirection");
            }
            if (((this.arrowDirection == System.Windows.Forms.ArrowDirection.Down) && this.showVectorChevron) && (this.arrowImage == null))
            {
                SmoothingMode smoothingMode = g.SmoothingMode;
                g.SmoothingMode = SmoothingMode.None;
                float y = tf2.Y - 2f;
                float x = tf2.X;
                float num4 = tf3.X;
                int num5 = (int) ((num4 - x) / 3f);
                Brush solidBrush = this.penBrushCache.GetSolidBrush(black);
                g.FillRectangle(solidBrush, x, y, (num4 - x) + 1f, 3f);
                x++;
                Brush brush = this.penBrushCache.GetSolidBrush(white);
                while (x < num4)
                {
                    RectangleF rect = new RectangleF(x, y + 1f, 1f, 1f);
                    g.FillRectangle(brush, rect);
                    x += 2f;
                }
                tf.Y += 2f;
                tf2.Y += 2f;
                tf3.Y += 2f;
                g.SmoothingMode = smoothingMode;
            }
            if (this.arrowImage == null)
            {
                if (this.reverseArrowColors)
                {
                    ObjectUtil.Swap<Color>(ref black, ref white);
                }
                PointF[] points = new PointF[] { tf, tf2, tf3 };
                g.FillPolygon(this.penBrushCache.GetSolidBrush(black), points);
                PointF[] tfArray2 = new PointF[] { tf, tf2, tf3 };
                g.DrawPolygon(this.penBrushCache.GetPen(white, this.arrowOutlineWidth), tfArray2);
            }
            else
            {
                int num6 = (int) Math.Min(tf.Y, Math.Min(tf2.Y, tf3.Y));
                float num7 = Math.Min(tf.X, Math.Min(tf2.X, tf3.X));
                float num8 = Math.Max(tf.X, Math.Max(tf2.X, tf3.X));
                float num9 = (num7 + num8) / 2f;
                int width = UIUtil.ScaleWidth(this.arrowImage.Width);
                int num11 = (int) (num9 - (((float) width) / 2f));
                Rectangle destRect = new Rectangle(num11, num6, width, UIUtil.ScaleHeight(this.arrowImage.Height));
                g.DrawImage(this.arrowImage, destRect, new Rectangle(Point.Empty, this.arrowImage.Size), GraphicsUnit.Pixel);
            }
        }

        protected override void OnPaintPushButton(Graphics g, PdnPushButtonState state, bool drawFocusCues, bool drawKeyboardCues)
        {
            PdnPushButtonState pressed;
            if (this.forcedPushed)
            {
                pressed = PdnPushButtonState.Pressed;
            }
            else
            {
                pressed = state;
            }
            this.OnPaintButtonImpl(g, pressed, drawFocusCues, drawKeyboardCues);
        }

        public System.Windows.Forms.ArrowDirection ArrowDirection
        {
            get => 
                this.arrowDirection;
            set
            {
                if (this.arrowDirection != value)
                {
                    this.arrowDirection = value;
                    base.Invalidate();
                }
            }
        }

        public Image ArrowImage
        {
            get => 
                this.arrowImage;
            set
            {
                if (value != this.arrowImage)
                {
                    this.arrowImage = value;
                    base.Invalidate();
                }
            }
        }

        public float ArrowOutlineWidth
        {
            get => 
                this.arrowOutlineWidth;
            set
            {
                if (this.arrowOutlineWidth != value)
                {
                    this.arrowOutlineWidth = value;
                    base.Invalidate();
                }
            }
        }

        public bool ForcedPushedAppearance
        {
            get => 
                this.forcedPushed;
            set
            {
                if (this.forcedPushed != value)
                {
                    this.forcedPushed = value;
                    base.Invalidate();
                }
            }
        }

        public bool ReverseArrowColors
        {
            get => 
                this.reverseArrowColors;
            set
            {
                if (this.reverseArrowColors != value)
                {
                    this.reverseArrowColors = value;
                    base.Invalidate();
                }
            }
        }

        public bool ShowVectorChevron
        {
            get => 
                this.showVectorChevron;
            set
            {
                if (value != this.showVectorChevron)
                {
                    this.showVectorChevron = value;
                    base.Invalidate();
                }
            }
        }

        protected override System.Windows.Forms.VisualStyles.VisualStyleElement VisualStyleElement =>
            System.Windows.Forms.VisualStyles.VisualStyleElement.Button.PushButton.Normal;
    }
}

