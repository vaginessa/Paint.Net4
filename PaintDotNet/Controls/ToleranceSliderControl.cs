namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Direct2D;
    using PaintDotNet.DirectWrite;
    using PaintDotNet.Imaging;
    using PaintDotNet.Rendering;
    using PaintDotNet.UI.Media;
    using System;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class ToleranceSliderControl : Direct2DControl
    {
        private const int fillInset = 1;
        private bool hovering;
        private static readonly string percentageFormat = PdnResources.GetString("ToleranceSliderControl.Percentage.Format");
        private const int textInset = 3;
        private TextLayout textLayout;
        private float tolerance;
        private static readonly string toleranceText = PdnResources.GetString("ToleranceSliderControl.Tolerance");
        private bool tracking;

        [field: CompilerGenerated]
        public event EventHandler ToleranceChanged;

        public ToleranceSliderControl() : base(FactorySource.PerThread)
        {
            base.Name = "ToleranceSliderControl";
            this.tolerance = 0.5f;
            base.SetStyle(ControlStyles.Selectable, true);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (!this.tracking && ((e.Button & MouseButtons.Left) == MouseButtons.Left))
            {
                this.tracking = true;
                this.OnMouseMove(e);
                base.Invalidate();
                this.QueueUpdate();
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            this.hovering = true;
            base.Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            this.hovering = false;
            base.Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            Size clientSize = base.ClientSize;
            if (this.tracking && (clientSize.Width != 0))
            {
                int num = clientSize.Width - 2;
                int num2 = Math.Min(e.X - 1, num);
                this.Tolerance = ((float) e.X) / ((float) num);
                base.Update();
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (this.tracking && ((e.Button & MouseButtons.Left) == MouseButtons.Left))
            {
                this.tracking = false;
                base.Invalidate();
                this.QueueUpdate();
            }
            base.OnMouseUp(e);
        }

        protected override void OnRender(IDrawingContext dc, RectFloat clipRect)
        {
            using (dc.UseAntialiasMode(AntialiasMode.Aliased))
            {
                SizeInt32 num;
                Size clientSize = base.ClientSize;
                if ((clientSize.Width < 2) || (clientSize.Height < 2))
                {
                    num = new SizeInt32(3, 3);
                }
                else
                {
                    num = new SizeInt32(clientSize.Width, clientSize.Height);
                }
                dc.Clear(new ColorRgba128Float?((ColorRgba128Float) PaintDotNet.Imaging.SystemColors.Window));
                int num2 = (int) (this.tolerance * 100f);
                string text = string.Format(percentageFormat, num2);
                if (this.textLayout == null)
                {
                    this.textLayout = UIText.CreateLayout(dc, text, this.Font, null, HotkeyRenderMode.Ignore, 65535.0, 65535.0);
                    this.textLayout.ParagraphAlignment = ParagraphAlignment.Center;
                    this.textLayout.FontSize *= 0.9;
                }
                else
                {
                    this.textLayout.Text = text;
                }
                this.textLayout.MaxWidth = num.Width - 6;
                this.textLayout.MaxHeight = num.Height - 6;
                PointDouble origin = new PointDouble(3.0, 3.0);
                dc.DrawTextLayout(origin, this.textLayout, SolidColorBrushCache.Get(base.Enabled ? ((ColorRgba128Float) PaintDotNet.Imaging.SystemColors.WindowText) : ((ColorRgba128Float) PaintDotNet.Imaging.SystemColors.GrayText)), DrawTextOptions.None);
                RectDouble rect = new RectDouble(0.0, 0.0, (double) num.Width, (double) num.Height);
                RectDouble num5 = RectDouble.Inflate(rect, -0.5, -0.5);
                dc.DrawRectangle(num5, SolidColorBrushCache.Get(base.Enabled ? ((ColorRgba128Float) PaintDotNet.Imaging.SystemColors.ControlDark) : ((ColorRgba128Float) PaintDotNet.Imaging.SystemColors.ControlDark)), 1.0);
                RectDouble num6 = new RectDouble(1.0, 1.0, (num.Width - 2.0) * this.tolerance, (double) (num.Height - 2));
                PaintDotNet.UI.Media.Brush brush = SolidColorBrushCache.Get(base.Enabled ? (this.hovering ? ((ColorRgba128Float) PaintDotNet.Imaging.SystemColors.HotTrack) : ((ColorRgba128Float) PaintDotNet.Imaging.SystemColors.Highlight)) : ((ColorRgba128Float) PaintDotNet.Imaging.SystemColors.Control));
                dc.FillRectangle(num6, brush);
                using (dc.UseAxisAlignedClip((RectFloat) num6, AntialiasMode.PerPrimitive))
                {
                    dc.DrawTextLayout(origin, this.textLayout, SolidColorBrushCache.Get(base.Enabled ? ((ColorRgba128Float) PaintDotNet.Imaging.SystemColors.HighlightText) : ((ColorRgba128Float) PaintDotNet.Imaging.SystemColors.GrayText)), DrawTextOptions.None);
                }
            }
            base.OnRender(dc, clipRect);
        }

        private void OnToleranceChanged()
        {
            base.Invalidate();
            this.QueueUpdate();
            this.ToleranceChanged.Raise(this);
        }

        public void PerformToleranceChanged()
        {
            this.OnToleranceChanged();
        }

        public float Tolerance
        {
            get => 
                this.tolerance;
            set
            {
                base.VerifyAccess();
                if (this.tolerance != value)
                {
                    this.tolerance = value.Clamp(0f, 1f);
                    this.OnToleranceChanged();
                }
            }
        }
    }
}

