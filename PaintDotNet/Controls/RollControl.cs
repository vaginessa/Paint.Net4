namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Direct2D;
    using PaintDotNet.Drawing;
    using PaintDotNet.Imaging;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.UI.Media;
    using System;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class RollControl : Direct2DControl
    {
        public double angle;
        private static readonly SystemBrush darkBrush = PaintDotNet.UI.Media.SystemBrushes.ControlDarkDark;
        private Point lastMouseXY;
        private SolidColorBrush[] latBrushCache;
        private static readonly ColorBgra latGradEnd = System.Drawing.SystemColors.ControlDarkDark;
        private static readonly ColorBgra latGradStart = System.Drawing.SystemColors.Control;
        private static readonly SystemBrush lightBrush = PaintDotNet.UI.Media.SystemBrushes.ControlDark;
        private bool mouseEntered;
        private bool onSphere;
        private static readonly SystemBrush ringFillBrush = PaintDotNet.UI.Media.SystemBrushes.ControlLightLight;
        private CombinedGeometry ringFillGeometry;
        private static readonly SystemBrush ringInlineBrush = PaintDotNet.UI.Media.SystemBrushes.ControlDarkDark;
        private EllipseGeometry ringInnerEllipseGeometry;
        private EllipseGeometry ringOuterEllipseGeometry;
        private static readonly SystemBrush ringOutlineBrush = PaintDotNet.UI.Media.SystemBrushes.ControlDark;
        private PaintDotNet.UI.Media.Pen ringOutlinePen;
        private double rollAmount;
        private double rollDirection;
        private double startAngle;
        private Point startPt;
        private PointF startRoll;
        private double startTheta;
        private static readonly SystemBrush thetaLineBrush = PaintDotNet.UI.Media.SystemBrushes.ControlDark;
        private bool tracking;

        [field: CompilerGenerated]
        public event EventHandler ValueChanged;

        public RollControl() : base(FactorySource.PerThread)
        {
            this.latBrushCache = new SolidColorBrush[0x100];
            base.Name = "RollControl";
            base.Size = new Size(0xa8, 0x90);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                TrimmableUtil.Free<CombinedGeometry>(ref this.ringFillGeometry);
                TrimmableUtil.Free<EllipseGeometry>(ref this.ringInnerEllipseGeometry);
                TrimmableUtil.Free<EllipseGeometry>(ref this.ringOuterEllipseGeometry);
            }
            base.Dispose(disposing);
        }

        private void Draw3DLine(IDrawingContext dc, double rx, double ry, double scale, double xs, double ys, double zs, double xe, double ye, double ze, PaintDotNet.UI.Media.Brush brush, double thickness)
        {
            double sinAmount = Math.Sqrt((rx * rx) + (ry * ry));
            if (sinAmount != 0.0)
            {
                double a = Math.Atan2(ry, rx);
                double sinAngle = Math.Sin(a);
                double cosAngle = Math.Cos(a);
                this.Transform(sinAngle, cosAngle, sinAmount, Math.Cos(Math.Asin(sinAmount)), ref xs, ref ys, ref zs);
                this.Transform(sinAngle, cosAngle, sinAmount, Math.Cos(Math.Asin(sinAmount)), ref xe, ref ye, ref ze);
            }
            xs *= scale;
            xe *= scale;
            ys *= scale;
            ye *= scale;
            if ((ze < 0.03) && (zs < 0.03))
            {
                dc.DrawLine(xs, ys, xe, ye, brush, thickness);
            }
        }

        private void DrawToDrawingContext(IDrawingContext dc)
        {
            RectInt32 rect = base.ClientRectangle.ToRectInt32();
            dc.Clear(new ColorRgba128Float?(this.BackColor));
            using (dc.UseTranslateTransform(0.5f, 0.5f, MatrixMultiplyOrder.Append))
            {
                using (dc.UseAntialiasMode(AntialiasMode.PerPrimitive))
                {
                    RectInt32 num2 = RectInt32.Inflate(rect, -2, -2);
                    int num3 = Math.Min(num2.Width, num2.Height);
                    PointInt32 center = new PointInt32(num2.X + (num3 / 2), num2.Y + (num3 / 2));
                    double radius = ((double) num3) / 2.0;
                    double scale = ((double) num3) / 3.0;
                    double num7 = ((double) num3) / 2.0;
                    double d = -MathUtil.DegreesToRadians(this.angle);
                    double num9 = Math.Cos(d);
                    double num10 = Math.Sin(d);
                    double rx = (this.rollAmount * Math.Cos(MathUtil.DegreesToRadians(this.rollDirection))) / 90.0;
                    double num12 = (this.rollAmount * Math.Sin(MathUtil.DegreesToRadians(this.rollDirection))) / 90.0;
                    double num13 = rx / (((num12 * num12) < 0.99) ? Math.Sqrt(1.0 - (num12 * num12)) : 1.0);
                    double num14 = num12 / (((rx * rx) < 0.99) ? Math.Sqrt(1.0 - (rx * rx)) : 1.0);
                    double thickness = (this.mouseEntered && !this.onSphere) ? 2.0 : 1.0;
                    if (this.ringOuterEllipseGeometry == null)
                    {
                        this.ringOuterEllipseGeometry = new EllipseGeometry();
                    }
                    if (this.ringInnerEllipseGeometry == null)
                    {
                        this.ringInnerEllipseGeometry = new EllipseGeometry();
                    }
                    if (this.ringFillGeometry == null)
                    {
                        this.ringFillGeometry = new CombinedGeometry(GeometryCombineMode.Exclude, this.ringOuterEllipseGeometry, this.ringInnerEllipseGeometry);
                    }
                    this.ringOuterEllipseGeometry.Center = center;
                    this.ringOuterEllipseGeometry.RadiusX = radius - 0.5;
                    this.ringOuterEllipseGeometry.RadiusY = radius - 0.5;
                    this.ringInnerEllipseGeometry.Center = center;
                    this.ringInnerEllipseGeometry.RadiusX = radius;
                    this.ringInnerEllipseGeometry.RadiusY = radius;
                    dc.FillGeometry(this.ringFillGeometry, ringFillBrush, null);
                    if (this.ringOutlinePen == null)
                    {
                        this.ringOutlinePen = new PaintDotNet.UI.Media.Pen();
                    }
                    this.ringOutlinePen.Brush = ringOutlineBrush;
                    this.ringOutlinePen.Thickness = thickness;
                    dc.DrawCircle(center, radius, this.ringOutlinePen);
                    double num16 = (this.mouseEntered && !this.onSphere) ? ((double) 2) : ((double) 1);
                    dc.DrawLine(center.X + (scale * num9), center.Y + (scale * num10), center.X + (num7 * num9), center.Y + (num7 * num10), thetaLineBrush, num16);
                    using (dc.UseTranslateTransform((float) center.X, (float) center.Y, MatrixMultiplyOrder.Prepend))
                    {
                        double num17 = (this.angle * 3.1415926535897931) / 180.0;
                        float num18 = (this.mouseEntered && this.onSphere) ? 1.5f : 1f;
                        int num19 = 0x18;
                        for (int i = 0; i >= (-num19 / 2); i--)
                        {
                            double num22 = (i * 3.1415926535897931) / ((double) num19);
                            double num23 = -num17 - 3.1415926535897931;
                            double xs = Math.Cos(num23) * Math.Cos(num22);
                            double ys = Math.Sin(num23) * Math.Cos(num22);
                            double zs = Math.Sin(num22);
                            double num30 = ((double) (i + (num19 / 2))) / ((double) (num19 / 2));
                            byte index = Int32Util.ClampToByte((int) (num30 * 255.0));
                            if (this.latBrushCache[index] == null)
                            {
                                ColorBgra bgra = ColorBgra.Blend(latGradStart, latGradEnd, index);
                                this.latBrushCache[index] = SolidColorBrushCache.Get((ColorRgba128Float) bgra);
                            }
                            SolidColorBrush brush = this.latBrushCache[index];
                            for (int k = -num19 * 6; k <= (num19 * 6); k++)
                            {
                                num23 = -num17 + ((k * 3.1415926535897931) / ((double) (num19 * 6)));
                                double num33 = Math.Cos(num22);
                                double num34 = Math.Sin(num22);
                                double xe = Math.Cos(num23) * Math.Cos(num22);
                                double ye = Math.Sin(num23) * Math.Cos(num22);
                                double ze = Math.Sin(num22);
                                double num35 = (this.mouseEntered && this.onSphere) ? 1.5 : 1.0;
                                this.Draw3DLine(dc, rx, -num12, scale, xs, ys, zs, xe, ye, ze, brush, num35);
                                xs = xe;
                                ys = ye;
                                zs = ze;
                            }
                        }
                        int num20 = 4;
                        for (int j = -num20; j < num20; j++)
                        {
                            double num37 = -num17 + ((j * 3.1415926535897931) / ((double) num20));
                            double num38 = -1.5707963267948966;
                            double num39 = Math.Cos(num37) * Math.Cos(num38);
                            double num40 = Math.Sin(num37) * Math.Cos(num38);
                            double num41 = Math.Sin(num38);
                            for (int m = -num20 * 4; m <= 0; m++)
                            {
                                num38 = (m * 3.1415926535897931) / ((double) (num20 * 8));
                                double num42 = Math.Cos(num37) * Math.Cos(num38);
                                double num43 = Math.Sin(num37) * Math.Cos(num38);
                                double num44 = Math.Sin(num38);
                                double num46 = (this.mouseEntered && this.onSphere) ? 2.0 : 1.0;
                                this.Draw3DLine(dc, rx, -num12, scale, num39, num40, num41, num42, num43, num44, lightBrush, num46);
                                num39 = num42;
                                num40 = num43;
                                num41 = num44;
                            }
                        }
                    }
                    dc.DrawCircle(center, scale, ringInlineBrush, thickness);
                }
            }
        }

        private bool IsMouseOnSphere(int x, int y)
        {
            Rectangle rectangle = Rectangle.Inflate(base.ClientRectangle, -2, -2);
            int num = Math.Min(rectangle.Width, rectangle.Height);
            float num2 = ((float) num) / 3f;
            Point point = new Point(rectangle.X + (num / 2), rectangle.Y + (num / 2));
            Point point2 = new Point(x - point.X, y - point.Y);
            return (Math.Sqrt((double) ((point2.X * point2.X) + (point2.Y * point2.Y))) <= num2);
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
            this.RollAmount = 0.0;
            this.RollDirection = 0.0;
            this.Angle = 0.0;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            this.startPt = new Point(e.X, e.Y);
            base.OnMouseDown(e);
            this.tracking = true;
            this.onSphere = this.IsMouseOnSphere(e.X, e.Y);
            this.startAngle = this.angle;
            this.startTheta = this.rollDirection;
            this.startRoll = new PointF((float) (this.rollAmount * Math.Cos((this.rollDirection * 3.1415926535897931) / 180.0)), (float) (this.rollAmount * Math.Sin((this.rollDirection * 3.1415926535897931) / 180.0)));
            this.OnMouseMove(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            this.mouseEntered = true;
            this.onSphere = this.IsMouseOnSphere(Control.MousePosition.X, Control.MousePosition.Y);
            base.Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            this.mouseEntered = false;
            this.onSphere = this.IsMouseOnSphere(Control.MousePosition.X, Control.MousePosition.Y);
            base.Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!this.tracking)
            {
                this.onSphere = this.IsMouseOnSphere(e.X, e.Y);
                base.Invalidate();
            }
            Point point = new Point(e.X, e.Y);
            bool flag = point != this.lastMouseXY;
            this.lastMouseXY = point;
            if (this.tracking & flag)
            {
                Rectangle rectangle = Rectangle.Inflate(base.ClientRectangle, -2, -2);
                int num = Math.Min(rectangle.Width, rectangle.Height);
                Point point2 = new Point(rectangle.X + (num / 2), rectangle.Y + (num / 2));
                if (this.onSphere)
                {
                    int num2 = e.X - this.startPt.X;
                    int num3 = e.Y - this.startPt.Y;
                    float num4 = (this.startRoll.X / 89.9f) + ((3f * num2) / ((float) (num - 4)));
                    float num5 = (this.startRoll.Y / 89.9f) + ((3f * num3) / ((float) (num - 4)));
                    float num6 = (float) Math.Sqrt((double) ((num4 * num4) + (num5 * num5)));
                    float num7 = (num6 > 1f) ? 1f : num6;
                    if (num6 == 0f)
                    {
                        num4 = 0f;
                        num5 = 0f;
                    }
                    else
                    {
                        num4 = (num4 * num7) / num6;
                        num5 = (num5 * num7) / num6;
                    }
                    if ((Control.ModifierKeys & Keys.Shift) != Keys.None)
                    {
                        if ((num4 * num4) > (num5 * num5))
                        {
                            num5 = 0f;
                        }
                        else
                        {
                            num4 = 0f;
                        }
                    }
                    double num8 = (180.0 * Math.Atan2((double) num5, (double) num4)) / 3.1415926535897931;
                    double num9 = 89.94 * Math.Sqrt((double) ((num4 * num4) + (num5 * num5)));
                    UIUtil.SuspendControlPainting(this);
                    this.RollDirection = num8;
                    this.RollAmount = num9;
                    UIUtil.ResumeControlPainting(this);
                    this.OnValueChanged();
                    this.Refresh();
                }
                else
                {
                    double num13;
                    int num10 = e.X - point2.X;
                    int num11 = e.Y - point2.Y;
                    double num12 = Math.Atan2((double) -num11, (double) num10);
                    if ((Control.ModifierKeys & Keys.Shift) != Keys.None)
                    {
                        num13 = Math.Round((double) ((12.0 * num12) / 3.1415926535897931), MidpointRounding.AwayFromZero) * 15.0;
                    }
                    else
                    {
                        num13 = (num12 * 360.0) / 6.2831853071795862;
                    }
                    this.Angle = num13;
                    this.QueueUpdate();
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            this.tracking = false;
        }

        protected override void OnRender(IDrawingContext dc, RectFloat clipRect)
        {
            this.DrawToDrawingContext(dc);
            base.OnRender(dc, clipRect);
        }

        private void OnValueChanged()
        {
            this.ValueChanged.Raise(this);
        }

        private void Transform(double sinAngle, double cosAngle, double sinAmount, double cosAmount, ref double x, ref double y, ref double z)
        {
            double num = x;
            double num2 = y;
            double num3 = z;
            x = (cosAngle * num) - (sinAngle * num2);
            y = (sinAngle * num) + (cosAngle * num2);
            num = x;
            num2 = y;
            x = (cosAmount * num) - (sinAmount * num3);
            z = (sinAmount * num) + (cosAmount * num3);
            num = x;
            x = (cosAngle * num) + (sinAngle * num2);
            y = (-sinAngle * num) + (cosAngle * num2);
        }

        public double Angle
        {
            get => 
                this.angle;
            set
            {
                double num = Math.IEEERemainder(value, 360.0);
                if (this.angle != num)
                {
                    this.angle = num;
                    this.OnValueChanged();
                    base.Invalidate();
                }
            }
        }

        public double RollAmount
        {
            get => 
                this.rollAmount;
            set
            {
                double num = Math.IEEERemainder(value, 360.0);
                if ((num <= 90.0) && (this.rollAmount != num))
                {
                    this.rollAmount = num;
                    this.OnValueChanged();
                    base.Invalidate();
                }
            }
        }

        public double RollDirection
        {
            get => 
                this.rollDirection;
            set
            {
                double num = Math.IEEERemainder(value, 360.0);
                if (this.rollDirection != num)
                {
                    this.rollDirection = num;
                    this.OnValueChanged();
                    base.Invalidate();
                }
            }
        }
    }
}

