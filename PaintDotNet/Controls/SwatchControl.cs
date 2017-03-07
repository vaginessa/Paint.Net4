namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.VisualStyling;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;

    internal sealed class SwatchControl : Control
    {
        private bool blinkHighlight;
        private System.Windows.Forms.Timer blinkHighlightTimer;
        private const int blinkInterval = 500;
        private SegmentedList<ColorBgra> colors = new SegmentedList<ColorBgra>();
        private const int defaultUnscaledSwatchSize = 12;
        private bool mouseDown;
        private int mouseDownIndex = -1;
        private int unscaledSwatchSize = 12;

        [field: CompilerGenerated]
        public event ValueEventHandler<Tuple<int, MouseButtons>> ColorClicked;

        [field: CompilerGenerated]
        public event EventHandler ColorsChanged;

        public SwatchControl()
        {
            this.InitializeComponent();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.blinkHighlightTimer != null))
            {
                this.blinkHighlightTimer.Dispose();
                this.blinkHighlightTimer = null;
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.blinkHighlightTimer = new System.Windows.Forms.Timer();
            this.blinkHighlightTimer.Tick += new EventHandler(this.OnBlinkHighlightTimerTick);
            this.blinkHighlightTimer.Enabled = false;
            this.blinkHighlightTimer.Interval = 500;
            this.DoubleBuffered = true;
            base.ResizeRedraw = true;
        }

        private int MouseXYToColorIndex(int x, int y)
        {
            if (((x < 0) || (y < 0)) || ((x >= base.ClientSize.Width) || (y >= base.ClientSize.Height)))
            {
                return -1;
            }
            int num = UIUtil.ScaleWidth(this.unscaledSwatchSize);
            int num2 = base.ClientSize.Width / num;
            int num3 = y / num;
            int num4 = x / num;
            int num5 = num4 + (num3 * num2);
            if (num4 == num2)
            {
                num5 = -1;
            }
            return num5;
        }

        private void OnBlinkHighlightTimerTick(object sender, EventArgs e)
        {
            base.Invalidate();
        }

        private void OnColorClicked(int index, MouseButtons buttons)
        {
            this.ColorClicked.Raise<Tuple<int, MouseButtons>>(this, Tuple.Create<int, MouseButtons>(index, buttons));
        }

        private void OnColorsChanged()
        {
            this.ColorsChanged.Raise(this);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            this.mouseDown = true;
            this.mouseDownIndex = this.MouseXYToColorIndex(e.X, e.Y);
            base.Invalidate();
            base.OnMouseDown(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            this.mouseDown = false;
            base.Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.Invalidate();
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            int index = this.MouseXYToColorIndex(e.X, e.Y);
            if (((index == this.mouseDownIndex) && (index >= 0)) && (index < this.colors.Count))
            {
                this.OnColorClicked(index, e.Button);
            }
            this.mouseDown = false;
            base.Invalidate();
            base.OnMouseUp(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.CompositingMode = CompositingMode.SourceOver;
            int width = UIUtil.ScaleWidth(this.unscaledSwatchSize);
            int num2 = base.ClientSize.Width / width;
            Point mousePosition = Control.MousePosition;
            mousePosition = base.PointToClient(mousePosition);
            int num3 = this.MouseXYToColorIndex(mousePosition.X, mousePosition.Y);
            for (int i = 0; i < this.colors.Count; i++)
            {
                PushButtonState pressed;
                bool flag;
                ColorBgra bgra = this.colors[i];
                int num5 = i % num2;
                int num6 = i / num2;
                Rectangle rect = new Rectangle(num5 * width, num6 * width, width, width);
                if (this.mouseDown)
                {
                    if (i == this.mouseDownIndex)
                    {
                        pressed = PushButtonState.Pressed;
                    }
                    else
                    {
                        pressed = PushButtonState.Normal;
                    }
                }
                else if (i == num3)
                {
                    pressed = PushButtonState.Hot;
                }
                else
                {
                    pressed = PushButtonState.Normal;
                }
                switch (pressed)
                {
                    case PushButtonState.Normal:
                    case PushButtonState.Disabled:
                    case PushButtonState.Default:
                        flag = false;
                        break;

                    case PushButtonState.Hot:
                        flag = true;
                        break;

                    case PushButtonState.Pressed:
                        flag = false;
                        break;

                    default:
                        throw ExceptionUtil.InvalidEnumArgumentException<PushButtonState>(pressed, "state");
                }
                ColorRectangle.Draw(e.Graphics, rect, bgra.ToColor(), base.Enabled, flag);
            }
            if (this.blinkHighlight)
            {
                Color window;
                switch (((Math.Abs(Environment.TickCount) / 500) % 2))
                {
                    case 0:
                        window = SystemColors.Window;
                        break;

                    case 1:
                        window = SystemColors.Highlight;
                        break;

                    default:
                        throw new InvalidOperationException();
                }
                using (Pen pen = new Pen(window))
                {
                    e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, base.Width - 1, base.Height - 1));
                }
            }
            base.OnPaint(e);
        }

        public bool BlinkHighlight
        {
            get => 
                this.blinkHighlight;
            set
            {
                this.blinkHighlight = value;
                this.blinkHighlightTimer.Enabled = value;
                base.Invalidate();
            }
        }

        public IList<ColorBgra> Colors
        {
            get => 
                new SegmentedList<ColorBgra>(this.colors, 7);
            set
            {
                this.colors = new SegmentedList<ColorBgra>(value, 7);
                this.mouseDown = false;
                base.Invalidate();
                this.OnColorsChanged();
            }
        }

        [DefaultValue(12), Browsable(true)]
        public int UnscaledSwatchSize
        {
            get => 
                this.unscaledSwatchSize;
            set
            {
                this.unscaledSwatchSize = value;
                this.mouseDown = false;
                base.Invalidate();
            }
        }
    }
}

