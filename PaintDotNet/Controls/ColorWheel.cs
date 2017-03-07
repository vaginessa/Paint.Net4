namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Animation;
    using PaintDotNet.Direct2D;
    using PaintDotNet.Drawing;
    using PaintDotNet.Imaging;
    using PaintDotNet.Rendering;
    using PaintDotNet.UI.Media;
    using PaintDotNet.VisualStyling;
    using System;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Forms;

    internal class ColorWheel : Direct2DControl
    {
        private MouseButtons currentButton;
        private const double Degrees2Radians = 0.017453292519943295;
        private const double displayValue = 1.0;
        private static readonly SolidColorBrush grayBrush = SolidColorBrushCache.Get(new ColorRgba128Float(0.5f, 0.5f, 0.5f, 1f));
        private Int32HsvColor hsvColor;
        private PointDouble lastHueGuidePoint;
        private System.Windows.Point lastMouseXY;
        private double lastRadiusGuideRadius;
        private bool lockHue;
        private SolidColorBrush lockHueGuideBrush;
        private AnimatedDouble lockHueOpacity;
        private bool lockRadius;
        private SolidColorBrush lockRadiusGuideBrush;
        private AnimatedDouble lockRadiusOpacity;
        private System.Windows.Forms.Timer modifierKeysCheckTimer;
        private static readonly SolidColorBrush nubOutlineBrush = SolidColorBrushCache.Get(new ColorRgba128Float(0f, 0f, 0f, 1f));
        private const double opacityFadeInDuration = 0.15;
        private const double opacityFadeOutDuration = 0.225;
        private const double Radians2Degrees = 57.295779513082323;
        private const double satGamma = 1.4;
        private SolidColorBrush selectorNubBrush;
        private int selectorOffset;
        private int selectorWidth;
        private bool snap;
        private const int snapAngle = 15;
        private SolidColorBrush snapLineBrush;
        private SolidColorBrush snapMarkBrush;
        private AnimatedDouble snapOpacity;
        private bool tracking;
        private IBitmap wheelBackgroundBitmap;
        private DeviceBitmap wheelBackgroundDeviceBitmap;
        private ISurface<ColorBgra> wheelBackgroundSurface;
        private int wheelCenterOffset;
        private int wheelOffset;
        private int wheelRadius;
        private static readonly SolidColorBrush whiteBrush = SolidColorBrushCache.Get(new ColorRgba128Float(1f, 1f, 1f, 1f));

        [field: CompilerGenerated]
        public event EventHandler ColorChanged;

        public ColorWheel() : base(FactorySource.PerThread)
        {
            this.selectorNubBrush = new SolidColorBrush();
            this.selectorWidth = 7;
            this.selectorOffset = 4;
            this.snapMarkBrush = new SolidColorBrush(new ColorRgba128Float(0f, 0f, 0f, 0.25f));
            this.snapLineBrush = new SolidColorBrush(new ColorRgba128Float(0f, 0f, 0f, 0.25f));
            this.lockRadiusGuideBrush = new SolidColorBrush(new ColorRgba128Float(0f, 0f, 0f, 0.25f));
            this.lockHueGuideBrush = new SolidColorBrush(new ColorRgba128Float(0f, 0f, 0f, 0.25f));
            this.modifierKeysCheckTimer = new System.Windows.Forms.Timer();
            this.modifierKeysCheckTimer.Interval = 0x19;
            this.modifierKeysCheckTimer.Tick += new EventHandler(this.OnModifierKeysCheckTimerTick);
            this.snapOpacity = new AnimatedDouble();
            this.snapOpacity.ValueChanged += new ValueChangedEventHandler<double>(this.OnOpacityValueChanged);
            this.lockRadiusOpacity = new AnimatedDouble();
            this.lockRadiusOpacity.ValueChanged += new ValueChangedEventHandler<double>(this.OnOpacityValueChanged);
            this.lockHueOpacity = new AnimatedDouble();
            this.lockHueOpacity.ValueChanged += new ValueChangedEventHandler<double>(this.OnOpacityValueChanged);
        }

        private void CheckModifierKeys()
        {
            Keys modifierKeys = Control.ModifierKeys;
            bool newSnap = (modifierKeys & Keys.Shift) > Keys.None;
            bool newLockHue = (modifierKeys & Keys.Alt) > Keys.None;
            bool newLockRadius = (modifierKeys & Keys.Control) > Keys.None;
            this.ProcessModifierKeys(newSnap, newLockHue, newLockRadius);
        }

        private static int ComputeRadius(System.Windows.Size size)
        {
            int num = (int) Math.Min(size.Width, size.Height);
            int num2 = num / 2;
            if ((num % 2) == 0)
            {
                num2--;
            }
            return num2;
        }

        private void CreateBitmapResources()
        {
            DisposableUtil.Free<ISurface<ColorBgra>>(ref this.wheelBackgroundSurface);
            this.wheelBackgroundSurface = new Surface(base.ClientRectangle.Size.ToSizeInt32());
            this.DrawWheelSurface(this.wheelBackgroundSurface);
            if (!base.Enabled)
            {
                DisabledRendering.ConvertToDisabled(this.wheelBackgroundSurface);
            }
            this.wheelBackgroundBitmap = this.wheelBackgroundSurface.CreateAliasedImagingBitmap(PixelFormats.Bgra32);
            this.wheelBackgroundDeviceBitmap = new DeviceBitmap(this.wheelBackgroundBitmap);
        }

        private void DestroyBitmapResources()
        {
            if (this.wheelBackgroundDeviceBitmap != null)
            {
                this.wheelBackgroundDeviceBitmap.BitmapSource = null;
                this.wheelBackgroundDeviceBitmap = null;
            }
            DisposableUtil.Free<ISurface<ColorBgra>>(ref this.wheelBackgroundSurface);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.modifierKeysCheckTimer != null)
                {
                    this.modifierKeysCheckTimer.Tick -= new EventHandler(this.OnModifierKeysCheckTimerTick);
                    DisposableUtil.Free<System.Windows.Forms.Timer>(ref this.modifierKeysCheckTimer);
                }
                this.DestroyBitmapResources();
                DisposableUtil.Free<AnimatedDouble>(ref this.snapOpacity);
                DisposableUtil.Free<AnimatedDouble>(ref this.lockRadiusOpacity);
                DisposableUtil.Free<AnimatedDouble>(ref this.lockHueOpacity);
            }
            base.Dispose(disposing);
        }

        private void DrawHueGuide(IDrawingContext dc, double theta)
        {
            double x = this.wheelRadius * Math.Cos(theta);
            double y = this.wheelRadius * Math.Sin(theta);
            PointDouble num3 = new PointDouble(x, y);
            dc.DrawLine(PointDouble.Zero, this.lockHue ? num3 : this.lastHueGuidePoint, this.lockHueGuideBrush, 1.0);
            if (this.lockHue)
            {
                this.lastHueGuidePoint = num3;
            }
        }

        private void DrawRadiusGuide(IDrawingContext dc, double radius)
        {
            double num = this.wheelRadius * radius;
            dc.DrawCircle(PointDouble.Zero, this.lockRadius ? num : this.lastRadiusGuideRadius, this.lockRadiusGuideBrush, 1.0);
            if (this.lockRadius)
            {
                this.lastRadiusGuideRadius = num;
            }
        }

        private void DrawSelectorNub(IDrawingContext dc, double theta, double radius)
        {
            PointDouble center = new PointDouble((radius * this.wheelRadius) * Math.Cos(theta), (radius * this.wheelRadius) * Math.Sin(theta));
            double num2 = this.selectorWidth / 2;
            Int32HsvColor hsvColor = this.hsvColor;
            hsvColor.Value = 100;
            this.selectorNubBrush.Color = base.Enabled ? hsvColor.ToGdipColor() : Color.Silver;
            dc.FillCircle(center, num2, this.selectorNubBrush);
            dc.DrawCircle(center, num2 - 1.0, whiteBrush, 1.0);
            dc.DrawCircle(center, num2, base.Enabled ? nubOutlineBrush : grayBrush, 1.0);
        }

        private void DrawSnapIndicators(IDrawingContext dc, double radius)
        {
            for (int i = 0; i < 360; i += 15)
            {
                double d = i * 0.017453292519943295;
                if (this.lockRadius)
                {
                    double centerX = (this.wheelRadius * radius) * Math.Cos(d);
                    double centerY = (this.wheelRadius * radius) * Math.Sin(d);
                    dc.FillCircle(centerX, centerY, 2.0, this.snapMarkBrush);
                }
                else
                {
                    double num5 = this.wheelRadius * Math.Cos(d);
                    double num6 = this.wheelRadius * Math.Sin(d);
                    dc.FillCircle(num5, num6, 2.0, this.snapMarkBrush);
                }
            }
            if (!this.lockRadius)
            {
                for (int j = 0; j < 360; j += 15)
                {
                    double num8 = j * 0.017453292519943295;
                    PointDouble num9 = new PointDouble(3.0 * Math.Cos(num8), 3.0 * Math.Sin(num8));
                    PointDouble num10 = new PointDouble(this.wheelRadius * Math.Cos(num8), this.wheelRadius * Math.Sin(num8));
                    dc.DrawLine(num9, num10, this.snapLineBrush, 1.0);
                }
            }
        }

        private unsafe void DrawWheelSurface(ISurface<ColorBgra> surface)
        {
            int width = surface.Width;
            int height = surface.Height;
            this.wheelOffset = this.selectorOffset - 1;
            this.wheelRadius = ComputeRadius(new System.Windows.Size((double) ((width - this.wheelOffset) - this.wheelOffset), (double) ((height - this.wheelOffset) - this.wheelOffset)));
            this.wheelCenterOffset = this.wheelOffset + this.wheelRadius;
            for (int i = 0; i < height; i++)
            {
                ColorBgra* rowPointer = (ColorBgra*) surface.GetRowPointer<ColorBgra>(i);
                for (int j = 0; j < width; j++)
                {
                    double dx = j - this.wheelCenterOffset;
                    double dy = i - this.wheelCenterOffset;
                    ColorBgra bgra = this.GetWheelPixelColor(this.wheelRadius, dx, dy);
                    rowPointer->Bgra = bgra.Bgra;
                    rowPointer++;
                }
            }
        }

        public ColorBgra FromHueSaturationAndAlpha(double h, double s, double a)
        {
            double num = 0.0;
            double num2 = 0.0;
            double num3 = 0.0;
            if (s == 0.0)
            {
                return ColorBgra.White;
            }
            double num4 = h;
            int num5 = (int) num4;
            double num6 = num4 - num5;
            double num7 = 255.0 * (1.0 - s);
            double num8 = 255.0 * (1.0 - (s * num6));
            double num9 = 255.0 * (1.0 - (s * (1.0 - num6)));
            switch (num5)
            {
                case 0:
                    num = 255.0;
                    num2 = num9;
                    num3 = num7;
                    break;

                case 1:
                    num = num8;
                    num2 = 255.0;
                    num3 = num7;
                    break;

                case 2:
                    num = num7;
                    num2 = 255.0;
                    num3 = num9;
                    break;

                case 3:
                    num = num7;
                    num2 = num8;
                    num3 = 255.0;
                    break;

                case 4:
                    num = num9;
                    num2 = num7;
                    num3 = 255.0;
                    break;

                case 5:
                    num = 255.0;
                    num2 = num7;
                    num3 = num8;
                    break;
            }
            return ColorBgra.FromBgra((byte) (num3 + 0.5), (byte) (num2 + 0.5), (byte) (num + 0.5), (byte) ((a * 255.0) + 0.5));
        }

        private ColorBgra GetWheelPixelColor(int targetRadius, double dx, double dy)
        {
            ColorBgra transparentBlack = ColorBgra.TransparentBlack;
            double num = Math.Sqrt((dx * dx) + (dy * dy));
            double num2 = ((dx == 0.0) && (dy == 0.0)) ? 0.0 : Math.Atan2(dy, dx);
            if (num2 < 0.0)
            {
                num2 += 6.2831853071795862;
            }
            double h = (num2 * 3.0) / 3.1415926535897931;
            if (num <= targetRadius)
            {
                double s = Math.Pow(num / ((double) targetRadius), 1.4);
                double a = 1.0;
                return this.FromHueSaturationAndAlpha(h, s, a);
            }
            double num6 = num - targetRadius;
            if ((num - targetRadius) < 1.0)
            {
                double num7 = num6 - ((int) num6);
                double num8 = 1.0 - num7;
                transparentBlack = this.FromHueSaturationAndAlpha(h, 1.0, num8);
            }
            return transparentBlack;
        }

        private void GrabColor(System.Windows.Point mouseXY)
        {
            double num5;
            double x = mouseXY.X - this.wheelCenterOffset;
            double y = mouseXY.Y - this.wheelCenterOffset;
            double num4 = Math.Atan2(y, x);
            if (num4 < 0.0)
            {
                num4 += 6.2831853071795862;
            }
            if (this.snap)
            {
                double num12;
                double d = (num4 * 57.295779513082323) / 15.0;
                double num9 = Math.Floor(d);
                double num10 = Math.Abs((double) (d - num9));
                double num11 = Math.Ceiling(d);
                if (Math.Abs((double) (d - num11)) < num10)
                {
                    num12 = num11;
                }
                else
                {
                    num12 = num9;
                }
                num4 = (num12 * 15.0) * 0.017453292519943295;
            }
            if (this.lockHue)
            {
                double num13 = this.hsvColor.Hue * 0.017453292519943295;
                double num14 = (x * Math.Cos(-num13)) - (y * Math.Sin(-num13));
                num5 = num14;
                if (num14 < 0.0)
                {
                    num5 = 0.0;
                }
            }
            else
            {
                num5 = Math.Sqrt((x * x) + (y * y));
            }
            double num6 = num4 * 57.295779513082323;
            double num7 = Math.Pow(Math.Min((double) 1.0, (double) (num5 / ((double) this.wheelRadius))), 1.4);
            if (!this.lockHue)
            {
                this.hsvColor.Hue = (int) (0.5 + num6);
            }
            if (!this.lockRadius)
            {
                this.hsvColor.Saturation = (int) (0.5 + (100.0 * num7));
            }
            if (this.hsvColor.Value == 0)
            {
                this.hsvColor.Value = 100;
            }
            base.Invalidate();
            this.QueueUpdate();
            this.OnColorChanged();
        }

        private void OnColorChanged()
        {
            this.ColorChanged.Raise(this);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            this.DestroyBitmapResources();
            base.Invalidate();
        }

        private void OnModifierKeysCheckTimerTick(object sender, EventArgs e)
        {
            this.CheckModifierKeys();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if ((e.Button == MouseButtons.Left) || (e.Button == MouseButtons.Right))
            {
                this.currentButton = e.Button;
                this.Tracking = true;
                System.Windows.Point mouseXY = new System.Windows.Point((double) e.X, (double) e.Y);
                if (this.Tracking && (mouseXY != this.lastMouseXY))
                {
                    this.GrabColor(mouseXY);
                    this.lastMouseXY = mouseXY;
                }
            }
            this.CheckModifierKeys();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            this.modifierKeysCheckTimer.Enabled = true;
            this.CheckModifierKeys();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            this.modifierKeysCheckTimer.Enabled = false;
            this.ProcessModifierKeys(false, false, false);
            base.OnMouseLeave(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            System.Windows.Point mouseXY = new System.Windows.Point((double) e.X, (double) e.Y);
            if (this.Tracking && (mouseXY != this.lastMouseXY))
            {
                this.GrabColor(mouseXY);
                this.lastMouseXY = mouseXY;
            }
            this.CheckModifierKeys();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (Control.MouseButtons == MouseButtons.None)
            {
                this.Tracking = false;
            }
        }

        private void OnOpacityValueChanged(object sender, ValueChangedEventArgs<double> e)
        {
            this.snapMarkBrush.Opacity = this.snapOpacity.Value;
            this.snapLineBrush.Opacity = this.snapOpacity.Value;
            this.lockRadiusGuideBrush.Opacity = this.lockRadiusOpacity.Value;
            this.lockHueGuideBrush.Opacity = this.lockHueOpacity.Value;
            base.Invalidate();
            this.QueueUpdate();
        }

        protected override void OnRender(IDrawingContext dc, RectFloat clipRect)
        {
            if (this.wheelBackgroundDeviceBitmap == null)
            {
                this.CreateBitmapResources();
            }
            double num = this.wheelCenterOffset + 0.5;
            double theta = this.hsvColor.Hue * 0.017453292519943295;
            double radius = Math.Pow(this.hsvColor.Saturation * 0.01, 0.7142857142857143);
            dc.Clear(new ColorRgba128Float?(this.BackColor));
            RectDouble? dstRect = null;
            dc.DrawBitmap(this.wheelBackgroundDeviceBitmap, dstRect, 1.0, BitmapInterpolationMode.Linear, null);
            using (dc.UseTranslateTransform((float) num, (float) num, MatrixMultiplyOrder.Prepend))
            {
                if (this.snap || (this.snapOpacity.Value > 0.0))
                {
                    this.DrawSnapIndicators(dc, radius);
                }
                if (this.lockRadius || (this.lockRadiusOpacity.Value > 0.0))
                {
                    this.DrawRadiusGuide(dc, radius);
                }
                if (this.lockHue || (this.lockHueOpacity.Value > 0.0))
                {
                    this.DrawHueGuide(dc, theta);
                }
                this.DrawSelectorNub(dc, theta, radius);
            }
            base.OnRender(dc, clipRect);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.DestroyBitmapResources();
            this.selectorWidth = base.Width / 0x10;
            if (this.selectorWidth < 7)
            {
                this.selectorWidth = 7;
            }
            if ((this.selectorWidth % 2) == 0)
            {
                this.selectorWidth++;
            }
            this.selectorOffset = (this.selectorWidth >> 1) + 1;
        }

        private bool ProcessModifierKeys(bool newSnap, bool newLockHue, bool newLockRadius)
        {
            if (((this.lockHue != newLockHue) || (this.lockRadius != newLockRadius)) || (this.snap != newSnap))
            {
                this.lockHueOpacity.AnimateValueTo(newLockHue ? 1.0 : 0.0, newLockHue ? 0.15 : 0.225, AnimationTransitionType.SmoothStop);
                this.lockRadiusOpacity.AnimateValueTo(newLockRadius ? 1.0 : 0.0, newLockRadius ? 0.15 : 0.225, AnimationTransitionType.SmoothStop);
                this.snapOpacity.AnimateValueTo(newSnap ? 1.0 : 0.0, newSnap ? 0.15 : 0.225, AnimationTransitionType.SmoothStop);
                this.lockHue = newLockHue;
                this.lockRadius = !this.lockHue && newLockRadius;
                this.snap = !this.lockHue && newSnap;
                base.Invalidate();
                this.QueueUpdate();
                return true;
            }
            return false;
        }

        public Int32HsvColor HsvColor
        {
            get => 
                this.hsvColor;
            set
            {
                if (this.hsvColor != value)
                {
                    Int32HsvColor hsvColor = this.hsvColor;
                    this.hsvColor = value;
                    this.OnColorChanged();
                    base.Invalidate();
                    this.QueueUpdate();
                }
            }
        }

        public bool Tracking
        {
            get => 
                this.tracking;
            set
            {
                this.tracking = value;
            }
        }
    }
}

