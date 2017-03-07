namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Direct2D;
    using PaintDotNet.Drawing;
    using PaintDotNet.Rendering;
    using PaintDotNet.UI.Media;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class AngleChooser : Direct2DControl
    {
        private Container components;
        private static readonly SystemBrush gripBrush = PaintDotNet.UI.Media.SystemBrushes.ControlDark;
        private static readonly SystemBrush gripFillBrush = PaintDotNet.UI.Media.SystemBrushes.ControlDark;
        private bool hover;
        private static readonly SystemBrush invalidAnglesFillBrush = PaintDotNet.UI.Media.SystemBrushes.Control;
        private Point lastMouseXY;
        private double maxValue;
        private double minValue;
        private static readonly SystemBrush outlineBrush = PaintDotNet.UI.Media.SystemBrushes.ControlDark;
        private AngleChooserPhase phase;
        private bool tracking;
        private static readonly SystemBrush validAnglesFillBrush = PaintDotNet.UI.Media.SystemBrushes.ControlLightLight;
        private ArcSegment validRangeArcSegment;
        private PathFigure validRangeFigure;
        private PathGeometry validRangeGeometry;
        private LineSegment validRangeLineSegment1;
        private LineSegment validRangeLineSegment2;
        private double value;

        [field: CompilerGenerated]
        public event EventHandler ValueChanged;

        public AngleChooser() : base(FactorySource.PerThread)
        {
            this.phase = AngleChooserPhase.OffsetByPi;
            this.minValue = -180.0;
            this.maxValue = 180.0;
            base.SetStyle(ControlStyles.Selectable, false);
            this.InitializeComponent();
            base.TabStop = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.components != null)
                {
                    this.components.Dispose();
                    this.components = null;
                }
                TrimmableUtil.Free<PathGeometry>(ref this.validRangeGeometry);
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            base.Name = "AngleChooser";
            base.Size = new Size(0xa8, 0x90);
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            this.tracking = true;
            this.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 1, this.lastMouseXY.X, this.lastMouseXY.Y, 0));
            this.tracking = false;
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            base.OnDoubleClick(e);
            this.tracking = true;
            this.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 1, this.lastMouseXY.X, this.lastMouseXY.Y, 0));
            this.tracking = false;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            this.tracking = true;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            this.hover = true;
            base.Invalidate(true);
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            this.hover = false;
            base.Invalidate(true);
            base.OnMouseLeave(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            this.lastMouseXY = new Point(e.X, e.Y);
            if (this.tracking)
            {
                double num6;
                Rectangle rectangle = Rectangle.Inflate(base.ClientRectangle, -2, -2);
                int num = Math.Min(rectangle.Width, rectangle.Height);
                Point point = new Point(rectangle.X + (num / 2), rectangle.Y + (num / 2));
                int num2 = e.X - point.X;
                int num3 = e.Y - point.Y;
                double num5 = MathUtil.RadiansToDegrees(Math.Atan2((double) -num3, (double) num2));
                if (this.phase == AngleChooserPhase.Regular)
                {
                    for (num6 = num5; num6 < 0.0; num6 += 360.0)
                    {
                    }
                }
                else
                {
                    num6 = num5;
                }
                if ((Control.ModifierKeys & Keys.Shift) != Keys.None)
                {
                    double d = num6 / 15.0;
                    double num8 = Math.Floor(d);
                    double num9 = Math.Abs((double) (num8 - d));
                    double num10 = Math.Ceiling(d);
                    double num12 = (Math.Abs((double) (num10 - d)) < num9) ? num10 : num8;
                    num6 = num12 * 15.0;
                }
                this.Value = num6;
                this.QueueUpdate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            this.tracking = false;
        }

        protected override void OnRender(IDrawingContext dc, RectFloat clipRect)
        {
            dc.Clear(new ColorRgba128Float?(this.BackColor));
            using (dc.UseTranslateTransform(0.5f, 0.5f, MatrixMultiplyOrder.Append))
            {
                RectDouble num = RectDouble.Inflate(base.ClientRectangle.ToRectInt32(), -1.0, -1.0);
                double num3 = Math.Min(num.Width, num.Height) / 2.0;
                PointDouble center = new PointDouble(num.X + num3, num.Y + num3);
                double d = MathUtil.DegreesToRadians(this.value);
                EllipseDouble ellipse = new EllipseDouble(center, num3 - 0.5, num3 - 0.5);
                double thickness = this.hover ? 2.0 : 1.0;
                double num8 = this.maxValue - this.minValue;
                if (num8 >= 360.0)
                {
                    dc.FillEllipse(ellipse, validAnglesFillBrush);
                    dc.DrawEllipse(ellipse, outlineBrush, thickness);
                }
                else
                {
                    dc.FillEllipse(ellipse, invalidAnglesFillBrush);
                    double width = num3;
                    PointDouble num14 = new PointDouble(center.X + (width * Math.Cos(MathUtil.DegreesToRadians(this.minValue))), center.Y - (width * Math.Sin(MathUtil.DegreesToRadians(this.minValue))));
                    PointDouble num15 = new PointDouble(center.X + (width * Math.Cos(MathUtil.DegreesToRadians(this.maxValue))), center.Y - (width * Math.Sin(MathUtil.DegreesToRadians(this.maxValue))));
                    SizeDouble num16 = new SizeDouble(width, width);
                    if (this.validRangeGeometry == null)
                    {
                        this.validRangeGeometry = new PathGeometry();
                        this.validRangeFigure = new PathFigure();
                        this.validRangeFigure.IsFilled = true;
                        this.validRangeFigure.IsClosed = true;
                        this.validRangeGeometry.Figures.Add(this.validRangeFigure);
                        this.validRangeLineSegment1 = new LineSegment();
                        this.validRangeFigure.Segments.Add(this.validRangeLineSegment1);
                        this.validRangeArcSegment = new ArcSegment();
                        this.validRangeArcSegment.SweepDirection = SweepDirection.Counterclockwise;
                        this.validRangeFigure.Segments.Add(this.validRangeArcSegment);
                        this.validRangeLineSegment2 = new LineSegment();
                        this.validRangeFigure.Segments.Add(this.validRangeLineSegment2);
                    }
                    this.validRangeFigure.StartPoint = center;
                    this.validRangeLineSegment1.Point = num14;
                    this.validRangeArcSegment.Point = num15;
                    this.validRangeArcSegment.IsLargeArc = num8 >= 180.0;
                    this.validRangeArcSegment.Size = num16;
                    this.validRangeLineSegment2.Point = num15;
                    dc.FillGeometry(this.validRangeGeometry, validAnglesFillBrush, null);
                    dc.DrawEllipse(ellipse, outlineBrush, thickness);
                    dc.DrawLine(center, num14, outlineBrush, 0.5);
                    dc.DrawLine(center, num15, outlineBrush, 0.5);
                }
                double num9 = num3 - 2.0;
                PointDouble num10 = new PointDouble(center.X + (num9 * Math.Cos(d)), center.Y - (num9 * Math.Sin(d)));
                double radius = 2.5;
                EllipseDouble num12 = new EllipseDouble(center, radius);
                dc.FillEllipse(num12, gripFillBrush);
                dc.DrawLine(center, num10, gripBrush, this.hover ? 2.0 : 1.5);
            }
            base.OnRender(dc, clipRect);
        }

        private void OnValueChanged()
        {
            this.ValueChanged.Raise(this);
        }

        public double MaxValue
        {
            get => 
                this.maxValue;
            set
            {
                if (this.maxValue != value)
                {
                    this.maxValue = value;
                    this.Value = this.Value;
                }
            }
        }

        public double MinValue
        {
            get => 
                this.minValue;
            set
            {
                if (this.minValue != value)
                {
                    this.minValue = value;
                    this.Value = this.Value;
                }
            }
        }

        public AngleChooserPhase Phase
        {
            get => 
                this.phase;
            set
            {
                if ((value != AngleChooserPhase.Regular) && (value != AngleChooserPhase.OffsetByPi))
                {
                    throw ExceptionUtil.InvalidEnumArgumentException<AngleChooserPhase>(value, "value");
                }
                if (this.phase != value)
                {
                    this.phase = value;
                    this.Value = this.Value;
                }
            }
        }

        public double Value
        {
            get => 
                this.value;
            set
            {
                double num;
                double num2;
                AngleChooserPhase phase = this.phase;
                if (phase != AngleChooserPhase.Regular)
                {
                    if (phase != AngleChooserPhase.OffsetByPi)
                    {
                        throw new PaintDotNet.InternalErrorException();
                    }
                }
                else
                {
                    num = value;
                    while (num < 0.0)
                    {
                        num += 360.0;
                    }
                    while (num > 360.0)
                    {
                        num -= 360.0;
                    }
                    goto Label_0060;
                }
                num = Math.IEEERemainder(value, 360.0);
            Label_0060:
                num2 = DoubleUtil.Clamp(num, this.minValue, this.maxValue);
                if (this.value != num2)
                {
                    this.value = num2;
                    this.OnValueChanged();
                    base.Invalidate();
                }
            }
        }
    }
}

