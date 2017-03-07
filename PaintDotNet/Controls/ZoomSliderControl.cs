namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Direct2D;
    using PaintDotNet.Drawing;
    using PaintDotNet.Imaging;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.UI.Media;
    using PaintDotNet.VisualStyling;
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class ZoomSliderControl : ToolStripControlHost
    {
        [field: CompilerGenerated]
        public event EventHandler ScaleFactorChanged;

        public ZoomSliderControl() : base(new SliderImpl())
        {
        }

        private void OnScaleFactorChanged(object sender, EventArgs e)
        {
            this.ScaleFactorChanged.Raise(this);
        }

        protected override void OnSubscribeControlEvents(Control control)
        {
            base.OnSubscribeControlEvents(control);
            ((SliderImpl) base.Control).ScaleFactorChanged += new EventHandler(this.OnScaleFactorChanged);
        }

        protected override void OnUnsubscribeControlEvents(Control control)
        {
            base.OnUnsubscribeControlEvents(control);
            ((SliderImpl) base.Control).ScaleFactorChanged -= new EventHandler(this.OnScaleFactorChanged);
        }

        public void SetMaxDocScaleFactor(PaintDotNet.ScaleFactor sf)
        {
            ((SliderImpl) base.Control).SetMaxDocScaleFactor(sf);
        }

        public void SetMinDocScaleFactor(PaintDotNet.ScaleFactor sf)
        {
            ((SliderImpl) base.Control).SetMinDocScaleFactor(sf);
        }

        public PaintDotNet.ScaleFactor ScaleFactor
        {
            get => 
                ((SliderImpl) base.Control).ScaleFactor;
            set
            {
                ((SliderImpl) base.Control).ScaleFactor = value;
            }
        }

        private sealed class SliderImpl : Direct2DControl
        {
            private System.Windows.Forms.Timer autoIncrementTimer;
            private PathGeometry classicThumbFillGeometry;
            private IContainer components;
            private double currentIx;
            private RectInt32 currentThumbRect;
            private readonly int defaultIx;
            private LinearGradientBrush enabledFillBrush;
            private LinearGradientBrush hotFillBrush;
            private AutoIncrementDirection incrementDirection;
            private bool initialLayoutCompleted;
            private bool isHot;
            private bool isPressed;
            private PointInt32 lastMousePosition;
            private double maxDocIx;
            private readonly int maximumIx;
            private double minDocIx;
            private readonly int minimumIx;
            private LinearGradientBrush normalFillBrush;
            private LinearGradientBrush pressedFillBrush;
            private PaintDotNet.ScaleFactor scaleFactor;
            private static readonly SolidColorBrush shadowBrush48 = SolidColorBrushCache.Get(Color.FromArgb(0x30, Color.Black));
            private static readonly SolidColorBrush shadowBrush64 = SolidColorBrushCache.Get(Color.FromArgb(0x40, Color.Black));
            private PathGeometry thumbFillGeometry;
            private int thumbOffset;
            private PathGeometry thumbOutlineGeometry;
            private SizeInt32 thumbSize;
            private RectInt32 trackRect;

            [field: CompilerGenerated]
            public event EventHandler ScaleFactorChanged;

            public SliderImpl() : base(FactorySource.PerThread)
            {
                this.maximumIx = PaintDotNet.ScaleFactor.PresetValues.Length - 1;
                this.defaultIx = (PaintDotNet.ScaleFactor.PresetValues.Length - 1) / 2;
                this.scaleFactor = PaintDotNet.ScaleFactor.OneToOne;
                this.currentIx = (PaintDotNet.ScaleFactor.PresetValues.Length - 1) / 2;
                base.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
                this.components = new Container();
                this.autoIncrementTimer = new System.Windows.Forms.Timer(this.components);
                this.autoIncrementTimer.Enabled = false;
                this.autoIncrementTimer.Interval = 400;
                this.autoIncrementTimer.Tick += new EventHandler(this.OnAutoIncrementTimerTick);
                base.Name = "ZoomSliderControl";
            }

            private double CalculateSliderIx(PaintDotNet.ScaleFactor sf)
            {
                double ratio = sf.Ratio;
                int index = PaintDotNet.ScaleFactor.PresetValues.IndexOf<PaintDotNet.ScaleFactor>(s => ratio < s.Ratio);
                if (index < 0)
                {
                    index = 0;
                }
                if (ratio != PaintDotNet.ScaleFactor.PresetValues[index].Ratio)
                {
                    index--;
                }
                if (index < this.minimumIx)
                {
                    index = this.minimumIx;
                }
                double num2 = 0.0;
                if (index < this.maximumIx)
                {
                    double d = PaintDotNet.ScaleFactor.PresetValues[index].Ratio;
                    if (sf.Ratio > d)
                    {
                        double num5 = PaintDotNet.ScaleFactor.PresetValues[index + 1].Ratio;
                        num2 = (Math.Log(ratio) - Math.Log(d)) / (Math.Log(num5) - Math.Log(d));
                    }
                }
                double maximumIx = index + num2;
                if (maximumIx > this.maximumIx)
                {
                    maximumIx = this.maximumIx;
                }
                return maximumIx;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    DisposableUtil.Free<IContainer>(ref this.components);
                    TrimmableUtil.Free<PathGeometry>(ref this.thumbOutlineGeometry);
                    TrimmableUtil.Free<PathGeometry>(ref this.thumbFillGeometry);
                    TrimmableUtil.Free<PathGeometry>(ref this.classicThumbFillGeometry);
                }
                base.Dispose(disposing);
            }

            public Brush GetFillBrush()
            {
                if (!base.Enabled)
                {
                    return this.InitializeFillBrush(ref this.enabledFillBrush, ColorTable.Fill1Disabled, ColorTable.Fill2Disabled, ColorTable.Fill3Disabled, ColorTable.Fill4Disabled);
                }
                if (this.isPressed)
                {
                    return this.InitializeFillBrush(ref this.pressedFillBrush, ColorTable.Fill1Pressed, ColorTable.Fill2Pressed, ColorTable.Fill3Pressed, ColorTable.Fill4Pressed);
                }
                if (this.isHot)
                {
                    return this.InitializeFillBrush(ref this.hotFillBrush, ColorTable.Fill1Hot, ColorTable.Fill2Hot, ColorTable.Fill3Hot, ColorTable.Fill4Hot);
                }
                return this.InitializeFillBrush(ref this.normalFillBrush, ColorTable.Fill1Normal, ColorTable.Fill2Normal, ColorTable.Fill3Normal, ColorTable.Fill4Normal);
            }

            private static double GetInflectionPosition(RectInt32 thumbRect) => 
                ((double) (thumbRect.Y + ((int) (0.66667 * thumbRect.Height))));

            private ColorBgra GetOutlineColor()
            {
                if (!base.Enabled)
                {
                    return ColorTable.OutlineDisabled;
                }
                if (this.isPressed)
                {
                    return ColorTable.OutlinePressed;
                }
                if (this.isHot)
                {
                    return ColorTable.OutlineHot;
                }
                return ColorTable.OutlineNormal;
            }

            private RectInt32 GetThumbRectangle()
            {
                double num = (this.currentIx - this.minimumIx) / ((double) (this.maximumIx - this.minimumIx));
                double num2 = this.trackRect.Left + (num * this.trackRect.Width);
                PointInt32 pt = new PointInt32((int) num2, (base.ClientRectangle.Height / 2) - 1);
                return new RectInt32(PointInt32.Offset(pt, -this.thumbSize.Width / 2, -this.thumbSize.Height / 2), this.thumbSize);
            }

            private RectInt32 GetUsableTrackRect()
            {
                double num = this.minDocIx / ((double) (this.maximumIx - this.minimumIx));
                int x = this.trackRect.X + ((int) (num * this.trackRect.Width));
                double num3 = this.maxDocIx / ((double) (this.maximumIx - this.minimumIx));
                int num4 = this.trackRect.X + ((int) (num3 * this.trackRect.Width));
                return new RectInt32(x, this.trackRect.Y, num4 - x, this.trackRect.Height);
            }

            private void IncrementDown()
            {
                int index = ((int) this.currentIx) - 1;
                if (index < this.minimumIx)
                {
                    index = this.minimumIx;
                }
                this.currentIx = index;
                this.scaleFactor = PaintDotNet.ScaleFactor.PresetValues[index];
                this.RecalculateThumbPosition();
                this.OnScaleFactorChanged();
            }

            private void IncrementUp()
            {
                int index = ((int) this.currentIx) + 1;
                if (index > this.maximumIx)
                {
                    index = this.maximumIx;
                }
                this.currentIx = index;
                this.scaleFactor = PaintDotNet.ScaleFactor.PresetValues[index];
                this.RecalculateThumbPosition();
                this.OnScaleFactorChanged();
            }

            private Brush InitializeFillBrush(ref LinearGradientBrush brush, ColorBgra color1, ColorBgra color2, ColorBgra color3, ColorBgra color4)
            {
                if (brush == null)
                {
                    brush = new LinearGradientBrush();
                    brush.ColorInterpolationMode = ColorInterpolationMode.SRgbLinearInterpolation;
                    brush.SpreadMethod = GradientSpreadMethod.Pad;
                    brush.GradientStops.Add(new GradientStop());
                    brush.GradientStops.Add(new GradientStop());
                    brush.GradientStops.Add(new GradientStop());
                    brush.GradientStops.Add(new GradientStop());
                }
                brush.StartPoint = new PointDouble((double) this.currentThumbRect.X, (double) this.currentThumbRect.Top);
                brush.EndPoint = new PointDouble((double) this.currentThumbRect.X, (double) this.currentThumbRect.Bottom);
                double offset = this.isPressed ? 0.55 : (((double) (this.currentThumbRect.Height / 2)) / ((double) this.currentThumbRect.Height));
                brush.GradientStops[0] = new GradientStop((ColorRgba128Float) color1, 0.0);
                brush.GradientStops[1] = new GradientStop((ColorRgba128Float) color2, offset);
                brush.GradientStops[2] = new GradientStop((ColorRgba128Float) color3, offset);
                brush.GradientStops[3] = new GradientStop((ColorRgba128Float) color4, 1.0);
                return brush;
            }

            private static void InitializeThumbGeometry(ref PathGeometry thumbGeometry, RectInt32 thumbRect, RectInt32 fillRect)
            {
                double inflectionPosition = GetInflectionPosition(thumbRect);
                double num2 = thumbRect.X + (thumbRect.Width / 2);
                if (thumbGeometry == null)
                {
                    thumbGeometry = new PathGeometry();
                    thumbGeometry.FillRule = FillRule.Nonzero;
                    PathFigure item = new PathFigure {
                        IsClosed = true,
                        IsFilled = true
                    };
                    thumbGeometry.Figures.Add(item);
                    PolyLineSegment segment2 = new PolyLineSegment();
                    PointDouble num3 = new PointDouble();
                    segment2.Points.Add(num3);
                    num3 = new PointDouble();
                    segment2.Points.Add(num3);
                    num3 = new PointDouble();
                    segment2.Points.Add(num3);
                    num3 = new PointDouble();
                    segment2.Points.Add(num3);
                    segment2.Points.Add(new PointDouble());
                    thumbGeometry.Figures[0].Segments.Add(segment2);
                }
                PathFigure figure = thumbGeometry.Figures[0];
                PolyLineSegment segment = (PolyLineSegment) figure.Segments[0];
                figure.StartPoint = new PointDouble((double) fillRect.X, (double) fillRect.Y);
                segment.Points[0] = new PointDouble((double) (fillRect.Right + 1), (double) fillRect.Y);
                segment.Points[1] = new PointDouble((double) (fillRect.Right + 1), inflectionPosition);
                segment.Points[2] = new PointDouble(num2 + 1.5, (double) (fillRect.Bottom + 1));
                segment.Points[3] = new PointDouble(num2 + 0.5, (double) (fillRect.Bottom + 1));
                segment.Points[4] = new PointDouble((double) fillRect.X, inflectionPosition);
            }

            private void OnAutoIncrementTimerTick(object sender, EventArgs e)
            {
                if (this.autoIncrementTimer.Enabled)
                {
                    PointInt32 lastMousePosition = this.lastMousePosition;
                    if (this.currentThumbRect.Contains(lastMousePosition.X, lastMousePosition.Y))
                    {
                        this.autoIncrementTimer.Enabled = false;
                    }
                    else if (lastMousePosition.X < this.currentThumbRect.Left)
                    {
                        if (this.incrementDirection == AutoIncrementDirection.Down)
                        {
                            this.IncrementDown();
                        }
                        else
                        {
                            this.incrementDirection = AutoIncrementDirection.None;
                            this.autoIncrementTimer.Enabled = false;
                        }
                    }
                    else if (lastMousePosition.X > this.currentThumbRect.Right)
                    {
                        if (this.incrementDirection == AutoIncrementDirection.Up)
                        {
                            this.IncrementUp();
                        }
                        else
                        {
                            this.incrementDirection = AutoIncrementDirection.None;
                            this.autoIncrementTimer.Enabled = false;
                        }
                    }
                }
            }

            protected override void OnLayout(LayoutEventArgs e)
            {
                if (!this.initialLayoutCompleted)
                {
                    int num = UIUtil.ScaleWidth(9);
                    int num2 = UIUtil.ScaleHeight(15);
                    this.thumbSize = new SizeInt32(num - ((num + 1) % 2), num2 + ((num2 + 1) % 2));
                    this.initialLayoutCompleted = true;
                }
                if (e.AffectedProperty == "Bounds")
                {
                    RectInt32 num3 = base.ClientRectangle.ToRectInt32();
                    int num4 = UIUtil.ScaleWidth(4);
                    int num5 = UIUtil.ScaleHeight(4);
                    int num6 = UIUtil.ScaleWidth(11);
                    int num7 = UIUtil.ScaleHeight(10);
                    this.trackRect = new RectInt32(num3.X + num4, num3.Y + num5, num3.Width - num6, num3.Height - num7);
                }
                base.OnLayout(e);
            }

            protected override void OnMouseDoubleClick(MouseEventArgs e)
            {
                base.OnMouseDoubleClick(e);
                if (this.isHot)
                {
                    this.currentIx = this.defaultIx;
                    this.ScaleFactor = PaintDotNet.ScaleFactor.OneToOne;
                    this.RecalculateThumbPosition();
                    this.OnScaleFactorChanged();
                }
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);
                if (!this.isPressed && (e.Button == MouseButtons.Left))
                {
                    this.isPressed = true;
                    base.Invalidate(this.currentThumbRect);
                    base.Capture = true;
                    if (this.currentThumbRect.Contains(e.X, e.Y))
                    {
                        this.thumbOffset = e.X - (this.currentThumbRect.Left + (this.currentThumbRect.Width / 2));
                        this.incrementDirection = AutoIncrementDirection.None;
                    }
                    else if (e.X < this.currentThumbRect.Left)
                    {
                        this.IncrementDown();
                        this.incrementDirection = AutoIncrementDirection.Down;
                    }
                    else if (e.X > this.currentThumbRect.Right)
                    {
                        this.IncrementUp();
                        this.incrementDirection = AutoIncrementDirection.Up;
                    }
                    this.autoIncrementTimer.Enabled = true;
                }
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);
                if (!this.isHot)
                {
                    this.isHot = false;
                    base.Invalidate(this.currentThumbRect);
                }
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);
                PointInt32 num = new PointInt32(e.X, e.Y);
                PointInt32 lastMousePosition = this.lastMousePosition;
                if (this.lastMousePosition != num)
                {
                    this.lastMousePosition = num;
                    bool flag = this.currentThumbRect.Contains(e.X, e.Y);
                    if (this.isHot != flag)
                    {
                        this.isHot = flag;
                        base.Invalidate(this.currentThumbRect);
                    }
                    if (this.isPressed)
                    {
                        int index = this.PositionToIx(e.X - this.thumbOffset);
                        if (index != this.currentIx)
                        {
                            this.currentIx = index;
                            this.scaleFactor = PaintDotNet.ScaleFactor.PresetValues[index];
                            this.RecalculateThumbPosition();
                            this.OnScaleFactorChanged();
                        }
                    }
                }
            }

            protected override void OnMouseUp(MouseEventArgs e)
            {
                base.OnMouseUp(e);
                if (this.isPressed)
                {
                    this.isPressed = false;
                    base.Invalidate(this.currentThumbRect);
                    base.Capture = false;
                }
                this.autoIncrementTimer.Enabled = false;
            }

            protected override void OnRender(IDrawingContext dc, RectFloat clipRect)
            {
                this.currentThumbRect = this.GetThumbRectangle();
                if (ThemeConfig.EffectiveTheme == PdnTheme.Aero)
                {
                    this.PaintThemed(dc, clipRect, this.trackRect, this.currentThumbRect);
                }
                else
                {
                    this.PaintClassic(dc, clipRect, this.trackRect, this.currentThumbRect);
                }
            }

            protected override void OnResize(EventArgs e)
            {
                this.initialLayoutCompleted = false;
                base.OnResize(e);
                base.Invalidate();
            }

            private void OnScaleFactorChanged()
            {
                this.ScaleFactorChanged.Raise(this);
            }

            private void PaintClassic(IDrawingContext dc, RectFloat clipRect, RectInt32 trackRect, RectInt32 thumbRect)
            {
                int num = (trackRect.Y + (trackRect.Height / 2)) - 1;
                int num2 = trackRect.X + (trackRect.Width / 2);
                dc.DrawLine((double) trackRect.Left, (double) num, (double) trackRect.Right, (double) num, SystemBrushes.ControlDark, 1.0);
                dc.DrawLine((double) num2, (double) trackRect.Top, (double) num2, (double) (trackRect.Bottom - 1), SystemBrushes.ControlDark, 1.0);
                dc.DrawLine((double) trackRect.Left, (double) (num + 1), (double) trackRect.Right, (double) (num + 1), SystemBrushes.ControlLightLight, 1.0);
                dc.DrawLine((double) (num2 + 1), (double) trackRect.Top, (double) (num2 + 1), (double) (trackRect.Bottom - 1), SystemBrushes.ControlLightLight, 1.0);
                double num3 = thumbRect.Y + ((int) (0.66667 * thumbRect.Height));
                double num4 = thumbRect.X + ((thumbRect.Width - 1) / 2);
                dc.DrawLine((double) (thumbRect.Right - 1), (double) (thumbRect.Y + 1), (double) (thumbRect.Right - 1), num3 - 1.0, shadowBrush64, 1.0);
                dc.DrawLine((double) (thumbRect.Right - 1), num3 - 1.0, num4, (double) (thumbRect.Bottom - 1), shadowBrush64, 1.0);
                thumbRect.Height--;
                InitializeThumbGeometry(ref this.classicThumbFillGeometry, thumbRect, thumbRect);
                dc.FillGeometry(this.classicThumbFillGeometry, SystemBrushes.Control, null);
                dc.DrawLine((double) thumbRect.X, (double) thumbRect.Y, (double) (thumbRect.Right - 1), (double) thumbRect.Y, SystemBrushes.ControlLightLight, 1.0);
                dc.DrawLine((double) thumbRect.X, (double) thumbRect.Y, (double) thumbRect.X, num3, SystemBrushes.ControlLightLight, 1.0);
                dc.DrawLine((double) (thumbRect.Right - 1), (double) thumbRect.Y, (double) (thumbRect.Right - 1), num3, SystemBrushes.ControlDarkDark, 1.0);
                dc.DrawLine((double) thumbRect.X, num3, num4, (double) (thumbRect.Bottom - 1), SystemBrushes.ControlDarkDark, 1.0);
                dc.DrawLine(num4 + 1.0, (double) (thumbRect.Bottom - 1), (double) (thumbRect.Right - 1), num3, SystemBrushes.ControlDarkDark, 1.0);
            }

            private void PaintThemed(IDrawingContext dc, RectFloat clipRect, RectInt32 trackRect, RectInt32 thumbRect)
            {
                this.PaintThemedTrackBar(dc, trackRect);
                double inflectionPosition = GetInflectionPosition(thumbRect);
                double num2 = thumbRect.X + (thumbRect.Width / 2);
                if (base.Enabled)
                {
                    using (dc.UseTranslateTransform(0.5f, 0.5f, MatrixMultiplyOrder.Append))
                    {
                        dc.DrawLine((double) (thumbRect.Right + 1), (double) (thumbRect.Y + 1), (double) (thumbRect.Right + 1), inflectionPosition + 1.0, shadowBrush48, 1.0);
                        dc.DrawLine((double) (thumbRect.Right + 1), inflectionPosition + 1.0, num2 + 2.0, (double) (thumbRect.Bottom - 1), shadowBrush48, 1.0);
                    }
                }
                RectInt32 fillRect = thumbRect;
                fillRect.Inflate(-1, -1);
                InitializeThumbGeometry(ref this.thumbOutlineGeometry, thumbRect, fillRect);
                dc.FillGeometry(this.thumbOutlineGeometry, SolidColorBrushCache.Get((ColorRgba128Float) ColorTable.Fill1Normal), null);
                using (dc.UseTranslateTransform(0.5f, 0.5f, MatrixMultiplyOrder.Append))
                {
                    SolidColorBrush brush = SolidColorBrushCache.Get((ColorRgba128Float) this.GetOutlineColor());
                    dc.DrawLine(thumbRect.X + 0.5, (double) thumbRect.Y, thumbRect.Right - 0.5, (double) thumbRect.Y, brush, 1.0);
                    dc.DrawLine((double) thumbRect.X, (double) thumbRect.Y, (double) thumbRect.X, inflectionPosition, brush, 1.0);
                    dc.DrawLine((double) thumbRect.Right, (double) thumbRect.Y, (double) thumbRect.Right, inflectionPosition, brush, 1.0);
                    dc.DrawLine((double) thumbRect.X, inflectionPosition, num2, (double) (thumbRect.Bottom - 1), brush, 1.0);
                    dc.DrawLine((double) thumbRect.Right, inflectionPosition, num2 + 1.0, (double) (thumbRect.Bottom - 1), brush, 1.0);
                    dc.DrawLine(num2, (double) (thumbRect.Bottom - 1), num2 + 1.0, (double) (thumbRect.Bottom - 1), brush, 1.0);
                }
                fillRect.Inflate(-1, -1);
                InitializeThumbGeometry(ref this.thumbFillGeometry, thumbRect, fillRect);
                dc.FillGeometry(this.thumbFillGeometry, this.GetFillBrush(), null);
            }

            private void PaintThemedTrackBar(IDrawingContext dc, RectInt32 trackRect)
            {
                int num = (trackRect.Y + (trackRect.Height / 2)) - 1;
                int num2 = trackRect.X + (trackRect.Width / 2);
                using (dc.UseTranslateTransform(0.5f, 0.5f, MatrixMultiplyOrder.Append))
                {
                    RectInt32 usableTrackRect = trackRect;
                    if ((this.maxDocIx < this.maximumIx) || (this.minDocIx > this.minimumIx))
                    {
                        usableTrackRect = this.GetUsableTrackRect();
                        dc.DrawLine((double) trackRect.Left, (double) (num + 1), (double) trackRect.Right, (double) (num + 1), SolidColorBrushCache.Get((ColorRgba128Float) ColorTable.Fill1Disabled), 1.0);
                        dc.DrawLine((double) trackRect.Left, (double) num, (double) trackRect.Right, (double) num, SolidColorBrushCache.Get((ColorRgba128Float) ColorTable.OutlineDisabled), 1.0);
                    }
                    Brush brush = SolidColorBrushCache.Get(base.Enabled ? ((ColorRgba128Float) ColorBgra.White) : ((ColorRgba128Float) ColorTable.Fill1Disabled));
                    dc.DrawLine((double) usableTrackRect.Left, (double) (num + 1), (double) usableTrackRect.Right, (double) (num + 1), brush, 1.0);
                    dc.DrawLine((double) (num2 + 1), (double) trackRect.Top, (double) (num2 + 1), (double) (trackRect.Bottom - 2), brush, 1.0);
                    Brush brush2 = SolidColorBrushCache.Get(base.Enabled ? ((ColorRgba128Float) ColorTable.OutlineNormal) : ((ColorRgba128Float) ColorTable.OutlineDisabled));
                    dc.DrawLine((double) usableTrackRect.Left, (double) num, (double) usableTrackRect.Right, (double) num, brush2, 1.0);
                    dc.DrawLine((double) num2, (double) trackRect.Top, (double) num2, (double) (trackRect.Bottom - 2), brush2, 1.0);
                }
            }

            private int PositionToIx(int x)
            {
                double num = x - this.trackRect.Left;
                num /= (double) this.trackRect.Width;
                if (num < 0.0)
                {
                    num = 0.0;
                }
                else if (num > 1.0)
                {
                    num = 1.0;
                }
                return (int) Math.Round((double) ((num * (this.maximumIx - this.minimumIx)) + this.minimumIx), MidpointRounding.AwayFromZero);
            }

            private void RecalculateThumbPosition()
            {
                this.currentThumbRect = this.GetThumbRectangle();
                if ((this.currentIx < this.maxDocIx) && (this.currentIx > this.minDocIx))
                {
                    base.Invalidate();
                    this.QueueUpdate();
                }
            }

            public void SetMaxDocScaleFactor(PaintDotNet.ScaleFactor sf)
            {
                this.VerifyThreadAccess();
                double num = this.CalculateSliderIx(sf);
                this.maxDocIx = Math.Min(num, (double) this.maximumIx);
            }

            public void SetMinDocScaleFactor(PaintDotNet.ScaleFactor sf)
            {
                this.VerifyThreadAccess();
                double num = this.CalculateSliderIx(sf);
                this.minDocIx = Math.Max(num, (double) this.minimumIx);
            }

            public PaintDotNet.ScaleFactor ScaleFactor
            {
                get => 
                    this.scaleFactor;
                set
                {
                    this.VerifyThreadAccess();
                    if (this.scaleFactor != value)
                    {
                        this.scaleFactor = value;
                        this.currentIx = this.CalculateSliderIx(this.scaleFactor);
                        this.RecalculateThumbPosition();
                    }
                }
            }

            private enum AutoIncrementDirection
            {
                None,
                Up,
                Down
            }

            private static class ColorTable
            {
                public static readonly ColorBgra Fill1Disabled = ColorBgra.FromBgr(0xf4, 0xf4, 0xf4);
                public static readonly ColorBgra Fill1Hot = ColorBgra.FromBgr(0xff, 0xf8, 0xed);
                public static readonly ColorBgra Fill1Normal = ColorBgra.FromBgr(0xf4, 0xf4, 0xf4);
                public static readonly ColorBgra Fill1Pressed = ColorBgra.FromBgr(0xfc, 0xf4, 0xe5);
                public static readonly ColorBgra Fill2Disabled = ColorBgra.FromBgr(0xf4, 0xf4, 0xf4);
                public static readonly ColorBgra Fill2Hot = ColorBgra.FromBgr(0xf7, 0xeb, 0xd4);
                public static readonly ColorBgra Fill2Normal = ColorBgra.FromBgr(0xe9, 0xe9, 0xe9);
                public static readonly ColorBgra Fill2Pressed = ColorBgra.FromBgr(0xf3, 0xd5, 0x9d);
                public static readonly ColorBgra Fill3Disabled = ColorBgra.FromBgr(0xec, 0xec, 0xec);
                public static readonly ColorBgra Fill3Hot = ColorBgra.FromBgr(0xfd, 230, 0xbd);
                public static readonly ColorBgra Fill3Normal = ColorBgra.FromBgr(220, 220, 220);
                public static readonly ColorBgra Fill3Pressed = ColorBgra.FromBgr(0xe5, 0xbb, 0x6c);
                public static readonly ColorBgra Fill4Disabled = ColorBgra.FromBgr(0xec, 0xec, 0xec);
                public static readonly ColorBgra Fill4Hot = ColorBgra.FromBgr(0xf2, 0xd9, 0xab);
                public static readonly ColorBgra Fill4Normal = ColorBgra.FromBgr(0xd0, 0xd0, 0xd0);
                public static readonly ColorBgra Fill4Pressed = ColorBgra.FromBgr(0xcc, 0xa1, 80);
                public static readonly ColorBgra OutlineDisabled = ColorBgra.FromBgr(0xb5, 0xb2, 0xad);
                public static readonly ColorBgra OutlineHot = ColorBgra.FromBgr(0xb1, 0x7f, 60);
                public static readonly ColorBgra OutlineNormal = ColorBgra.FromBgr(0x70, 0x70, 0x70);
                public static readonly ColorBgra OutlinePressed = ColorBgra.FromBgr(0x8b, 0x62, 0x2c);
            }
        }
    }
}

