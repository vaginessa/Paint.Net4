namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Direct2D;
    using PaintDotNet.DirectWrite;
    using PaintDotNet.Drawing;
    using PaintDotNet.Imaging;
    using PaintDotNet.ObjectModel;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Threading;
    using PaintDotNet.UI.Media;
    using PaintDotNet.VisualStyling;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class Ruler : Direct2DControl
    {
        private LinearGradientBrush backHighlightBrush;
        private LinearGradientBrush backNormalBrush;
        private SolidColorBrush cursorBrush;
        private double dpu;
        private static readonly double halfTickThickness = 0.5;
        private bool highlightEnabled;
        private double highlightLength;
        private double highlightStart;
        private static readonly double[] integerSubDivisors = new double[] { 2.0, 5.0, 2.0 };
        private bool isPrefetchingTickLabelFont;
        private SolidColorBrush lineBrush;
        private static readonly double[] majorDivisors = new double[] { 2.0, 2.5, 2.0 };
        private static readonly CachingMeasurementsUITextFactory measurementsUITextFactory = new CachingMeasurementsUITextFactory();
        private PaintDotNet.MeasurementUnit measurementUnit;
        private double offset;
        private System.Windows.Forms.Orientation orientation;
        private double rulerValue;
        private PaintDotNet.ScaleFactor scaleFactor;
        private static readonly int[] subdivsCm = new int[] { 2, 5 };
        private static readonly int[] subdivsInch = new int[] { 2 };
        private static readonly int[] subdivsOther = null;
        private SolidColorBrush textBrush;
        private static readonly double[] tickHeights = new double[] { 1.0, 0.6, 0.35, 0.25, 0.1, 0.075 };
        private Lazy<SizedFontProperties> tickLabelFontLazy;
        private static readonly string[] tickLabelFontNames = new string[] { "Calibri", "Segoe UI", "Arial" };
        private static readonly double tickThickness = 1.0;
        private static readonly CachingUITextFactory uiTextFactory = new CachingUITextFactory();
        private static char[] unitAbreviationTrimChars = new char[] { ' ', '.', '(', ')' };
        private SolidColorBrush unitsBoxBrush;
        private SizedFontProperties unitsBoxFont;

        public Ruler() : base(FactorySource.PerThread)
        {
            this.cursorBrush = new SolidColorBrush();
            this.lineBrush = new SolidColorBrush();
            this.textBrush = new SolidColorBrush();
            this.unitsBoxBrush = new SolidColorBrush();
            this.measurementUnit = PaintDotNet.MeasurementUnit.Inch;
            this.dpu = 96.0;
            this.scaleFactor = PaintDotNet.ScaleFactor.OneToOne;
            base.UseHwndRenderTarget = true;
            this.tickLabelFontLazy = new Lazy<SizedFontProperties>(delegate {
                using (IGdiFontMap map = DirectWriteFactory.Instance.GetGdiFontMapRef(true))
                {
                    FontProperties fontProperties;
                    try
                    {
                        fontProperties = map.GetFontProperties(tickLabelFontNames);
                    }
                    catch (NoFontException)
                    {
                        fontProperties = new FontProperties(string.Empty, string.Empty, FontWeight.Normal, FontStretch.Normal, PaintDotNet.DirectWrite.FontStyle.Normal, TextDecorations.None);
                    }
                    return new SizedFontProperties(fontProperties, UIUtil.ScaleHeight((float) 10.66667f));
                }
            }, LazyThreadSafetyMode.PublicationOnly);
            using (ISystemFonts fonts = new PaintDotNet.DirectWrite.SystemFonts())
            {
                this.unitsBoxFont = fonts.Menu;
            }
        }

        private void AssignThemedBrushes()
        {
            if (ThemeConfig.EffectiveTheme != PdnTheme.Aero)
            {
                this.lineBrush.Color = (ColorRgba128Float) PaintDotNet.Imaging.SystemColors.ControlDark;
                this.textBrush.Color = (ColorRgba128Float) PaintDotNet.Imaging.SystemColors.WindowText;
                this.unitsBoxBrush.Color = (ColorRgba128Float) PaintDotNet.Imaging.SystemColors.Window;
            }
            else
            {
                this.lineBrush.Color = AeroColors.RulerLineColor;
                this.textBrush.Color = AeroColors.RulerTextColor;
                this.unitsBoxBrush.Color = AeroColors.ToolBarBackFillGradTopColor;
            }
        }

        private static LinearGradientBrush CreateBackHighlightBrush()
        {
            LinearGradientBrush brush = new LinearGradientBrush();
            brush.GradientStops.Add(new GradientStop(ColorRgba128Float.Blend((ColorRgba128Float) ColorBgra.White, (ColorRgba128Float) PaintDotNet.Imaging.SystemColors.Highlight, 0.8f), 0.0));
            brush.GradientStops.Add(new GradientStop(ColorRgba128Float.Blend((ColorRgba128Float) ColorBgra.White, (ColorRgba128Float) PaintDotNet.Imaging.SystemColors.Highlight, 0.8f), 1.0));
            brush.ColorInterpolationMode = ColorInterpolationMode.SRgbLinearInterpolation;
            brush.SpreadMethod = GradientSpreadMethod.Pad;
            return brush;
        }

        private static LinearGradientBrush CreateBackNormalBrush()
        {
            LinearGradientBrush brush = new LinearGradientBrush();
            brush.GradientStops.Add(new GradientStop(AeroColors.ToolBarBackFillGradTopColor, 0.0));
            ColorRgba128Float color = ColorRgba128Float.Blend(AeroColors.ToolBarBackFillGradTopColor, AeroColors.ToolBarBackFillGradMidColor, 0.33f);
            brush.GradientStops.Add(new GradientStop(color, 1.0));
            brush.ColorInterpolationMode = ColorInterpolationMode.SRgbLinearInterpolation;
            brush.SpreadMethod = GradientSpreadMethod.Pad;
            return brush;
        }

        private int[] GetSubdivisions(PaintDotNet.MeasurementUnit unit)
        {
            if (unit != PaintDotNet.MeasurementUnit.Inch)
            {
                if (unit == PaintDotNet.MeasurementUnit.Centimeter)
                {
                    return subdivsCm;
                }
                return subdivsOther;
            }
            return subdivsInch;
        }

        protected override void OnRender(IDrawingContext dc, RectFloat clipRect)
        {
            if (!this.tickLabelFontLazy.IsValueCreated)
            {
                if (!this.isPrefetchingTickLabelFont)
                {
                    this.isPrefetchingTickLabelFont = true;
                    this.tickLabelFontLazy.Prefetch<SizedFontProperties>(<obj> => PdnSynchronizationContext.Instance.Post(delegate (object <state>) {
                        try
                        {
                            base.Invalidate(true);
                        }
                        catch (Exception)
                        {
                        }
                    }));
                }
                dc.Clear(new ColorRgba128Float?(this.BackColor));
            }
            else
            {
                RectDouble empty;
                RectDouble num8;
                RectDouble num9;
                RectInt32 a = base.ClientRectangle.ToRectInt32();
                double adjustedOffset = this.AdjustedOffset;
                double num3 = this.scaleFactor.Scale((double) (this.rulerValue + adjustedOffset));
                double num4 = this.scaleFactor.Scale((double) ((this.rulerValue + 1.0) + adjustedOffset));
                double x = this.scaleFactor.Scale((double) (this.highlightStart + adjustedOffset));
                double num6 = this.scaleFactor.Scale((double) ((this.highlightStart + this.highlightLength) + adjustedOffset));
                if (this.orientation == System.Windows.Forms.Orientation.Horizontal)
                {
                    RectDouble num10;
                    if (this.highlightEnabled)
                    {
                        num10 = new RectDouble(x, (double) a.Top, num6 - x, (double) a.Height);
                    }
                    else
                    {
                        num10 = new RectDouble(0.0, (double) a.Top, 0.0, (double) a.Height);
                    }
                    empty = RectDouble.Intersect(a, num10);
                    num8 = new RectDouble((double) a.Left, (double) a.Top, empty.X - a.Left, (double) a.Height);
                    num9 = new RectDouble(empty.Right, (double) a.Top, a.Right - empty.Right, (double) a.Height);
                }
                else
                {
                    RectDouble num11;
                    if (this.highlightEnabled)
                    {
                        num11 = new RectDouble((double) a.Left, x, (double) a.Width, num6 - x);
                    }
                    else
                    {
                        num11 = new RectDouble((double) a.Left, 0.0, (double) a.Width, 0.0);
                    }
                    empty = RectDouble.Intersect(a, num11);
                    num8 = new RectDouble((double) a.Left, (double) a.Top, (double) a.Width, empty.Top - a.Top);
                    num9 = new RectDouble((double) a.Left, empty.Bottom, (double) a.Width, a.Bottom - empty.Bottom);
                }
                if (!this.highlightEnabled)
                {
                    empty = RectDouble.Empty;
                }
                this.AssignThemedBrushes();
                this.RenderRuler(dc, num8, false);
                this.RenderRuler(dc, num9, false);
                this.RenderRuler(dc, empty, true);
                if (this.orientation == System.Windows.Forms.Orientation.Horizontal)
                {
                    this.RenderMeasurementUnitsBox(dc, a);
                }
                this.RenderBorder(dc, a);
            }
        }

        private void RenderBackground(IDrawingContext dc, RectDouble clipRect, bool highlighted)
        {
            if (ThemeConfig.EffectiveTheme != PdnTheme.Aero)
            {
                if (highlighted)
                {
                    dc.Clear(new ColorRgba128Float?((ColorRgba128Float) PaintDotNet.Imaging.SystemColors.Highlight));
                }
                else
                {
                    dc.Clear(new ColorRgba128Float?((ColorRgba128Float) PaintDotNet.Imaging.SystemColors.Window));
                }
            }
            else
            {
                if (highlighted)
                {
                    this.backHighlightBrush = this.backHighlightBrush ?? CreateBackHighlightBrush();
                }
                else
                {
                    this.backNormalBrush = this.backNormalBrush ?? CreateBackNormalBrush();
                }
                LinearGradientBrush brush = highlighted ? this.backHighlightBrush : this.backNormalBrush;
                RectDouble num = base.ClientRectangle.ToRectFloat();
                if (this.orientation == System.Windows.Forms.Orientation.Horizontal)
                {
                    brush.StartPoint = num.TopLeft;
                    brush.EndPoint = num.BottomLeft;
                }
                else
                {
                    brush.StartPoint = num.TopLeft;
                    brush.EndPoint = num.TopRight;
                }
                dc.FillRectangle(clipRect, brush);
            }
        }

        private void RenderBorder(IDrawingContext dc, RectInt32 clientRect)
        {
            if (this.orientation == System.Windows.Forms.Orientation.Horizontal)
            {
                dc.DrawLine((double) clientRect.Left, clientRect.Bottom - halfTickThickness, (double) clientRect.Right, clientRect.Bottom - halfTickThickness, this.lineBrush, tickThickness);
            }
            else
            {
                dc.DrawLine(clientRect.Right - halfTickThickness, (double) clientRect.Top, clientRect.Right - halfTickThickness, (double) clientRect.Bottom, this.lineBrush, tickThickness);
            }
        }

        private void RenderMeasurementUnitsBox(IDrawingContext dc, RectInt32 clientRect)
        {
            int num = Math.Min(base.Width, base.Height) - 1;
            RectFloat rect = new RectFloat(0f, 0f, (float) num, (float) num);
            dc.FillRectangle(rect, this.unitsBoxBrush);
            dc.DrawLine(num + halfTickThickness, (double) clientRect.Top, num + halfTickThickness, (double) clientRect.Bottom, this.lineBrush, tickThickness);
            string abbString = PdnResources.GetString("MeasurementUnit." + this.MeasurementUnit.ToString() + ".Abbreviation").Trim(unitAbreviationTrimChars);
            TextLayout textLayout = measurementsUITextFactory.CreateLayout(dc, abbString, this.unitsBoxFont, (double) num);
            dc.DrawTextLayout(0.0, 0.0, textLayout, this.textBrush, DrawTextOptions.Clip);
        }

        private void RenderRuler(IDrawingContext dc, RectDouble clipRect, bool highlighted)
        {
            if (clipRect.HasPositiveArea)
            {
                using (dc.UseAxisAlignedClip((RectFloat) clipRect, AntialiasMode.PerPrimitive))
                {
                    RectInt32 clientRect = base.ClientRectangle.ToRectInt32();
                    this.RenderBackground(dc, clipRect, highlighted);
                    this.RenderTicksAndLabels(dc, clientRect, highlighted);
                    this.RenderValueCursor(dc, clientRect, highlighted);
                }
            }
        }

        private void RenderTicksAndLabels(IDrawingContext dc, RectInt32 clientRect, bool highlighted)
        {
            PaintDotNet.UI.Media.Brush highlightText;
            PaintDotNet.UI.Media.Brush textBrush;
            int num;
            if (highlighted)
            {
                highlightText = PaintDotNet.UI.Media.SystemBrushes.HighlightText;
                textBrush = PaintDotNet.UI.Media.SystemBrushes.HighlightText;
            }
            else
            {
                highlightText = this.lineBrush;
                textBrush = this.textBrush;
            }
            if (this.orientation == System.Windows.Forms.Orientation.Horizontal)
            {
                num = (int) this.ScaleFactor.Unscale(((double) clientRect.Width));
            }
            else
            {
                num = (int) this.ScaleFactor.Unscale(((double) clientRect.Height));
            }
            double adjustedOffset = this.AdjustedOffset;
            double division = 1.0;
            int num4 = 0;
            double dpu = this.dpu;
            double num6 = this.ScaleFactor.Scale(dpu);
            int[] subdivisions = this.GetSubdivisions(this.measurementUnit);
            double num7 = this.ScaleFactor.Scale(adjustedOffset) - 1.0;
            int num8 = ((int) (-adjustedOffset / dpu)) - 1;
            int num9 = ((int) ((num - adjustedOffset) / dpu)) + 1;
            while ((num6 * division) < 60.0)
            {
                division *= majorDivisors[num4 % majorDivisors.Length];
                num4++;
            }
            num8 = (int) (division * Math.Floor((double) (((double) num8) / division)));
            using (dc.UseAntialiasMode(AntialiasMode.Aliased))
            {
                for (int i = num8; i <= num9; i += (int) division)
                {
                    TextLayoutAlgorithm? nullable;
                    double num11 = (i * num6) + num7;
                    string text = i.ToString();
                    if (this.Orientation == System.Windows.Forms.Orientation.Horizontal)
                    {
                        this.SubdivideX(dc, highlightText, clientRect.Left + num11, num6 * division, division, -num4, (double) (clientRect.Bottom - 1), (double) (clientRect.Height - 1), 0, subdivisions);
                        nullable = null;
                        TextLayout textLayout = uiTextFactory.CreateLayout(dc, text, this.tickLabelFontLazy.Value, nullable, HotkeyRenderMode.Ignore, 65535.0, (double) clientRect.Height);
                        double originX = (2.0 + clientRect.X) + num11;
                        double originY = -1.0;
                        dc.DrawTextLayout(originX, originY, textLayout, textBrush, DrawTextOptions.None);
                    }
                    else
                    {
                        this.SubdivideY(dc, highlightText, clientRect.Top + num11, num6 * division, division, -num4, (double) (clientRect.Right - 1), (double) (clientRect.Width - 1), 0, subdivisions);
                        nullable = null;
                        TextLayout resourceSource = uiTextFactory.CreateLayout(dc, text, this.tickLabelFontLazy.Value, nullable, HotkeyRenderMode.Ignore, 65535.0, (double) clientRect.Width);
                        double d = clientRect.Left - 1;
                        double dy = Math.Floor(d);
                        double num16 = (clientRect.Y + num11) + 2.0;
                        Matrix3x2Double num18 = Matrix3x2Double.Translation(-Math.Floor(num16), dy) * Matrix3x2Double.Rotation(-90.0);
                        using (dc.UseTransform((Matrix3x2Float) num18))
                        {
                            ITextLayout cachedOrCreateResource = dc.GetCachedOrCreateResource<ITextLayout>(resourceSource);
                            dc.DrawTextLayout((double) -cachedOrCreateResource.Metrics.WidthMax, 0.0, resourceSource, textBrush, DrawTextOptions.NoSnap);
                        }
                    }
                }
            }
        }

        private void RenderValueCursor(IDrawingContext dc, RectInt32 clientRect, bool highlighted)
        {
            ColorRgba128Float hotTrack;
            if (highlighted)
            {
                ColorRgba128Float highlight = (ColorRgba128Float) PaintDotNet.Imaging.SystemColors.Highlight;
                ColorRgba128Float black = (ColorRgba128Float) ColorBgra.Black;
                hotTrack = ColorRgba128Float.Blend((ColorRgba128Float) PaintDotNet.Imaging.SystemColors.Highlight, (ColorRgba128Float) ColorBgra.Black, 0.5f);
            }
            else
            {
                hotTrack = (ColorRgba128Float) PaintDotNet.Imaging.SystemColors.HotTrack;
            }
            this.cursorBrush.Color = hotTrack;
            this.cursorBrush.Opacity = 0.75;
            double adjustedOffset = this.AdjustedOffset;
            if (this.orientation == System.Windows.Forms.Orientation.Horizontal)
            {
                dc.FillRectangle((double) ((float) this.scaleFactor.Scale(((double) ((clientRect.Left + this.Value) + adjustedOffset)))), (double) clientRect.Top, (double) ((float) Math.Max(1.0, this.scaleFactor.Scale((double) 1.0))), (double) clientRect.Height, this.cursorBrush);
            }
            else
            {
                dc.FillRectangle((double) clientRect.X, (double) ((float) this.scaleFactor.Scale(((double) ((clientRect.Top + this.Value) + adjustedOffset)))), (double) clientRect.Width, (double) ((float) Math.Max(1.0, this.scaleFactor.Scale((double) 1.0))), this.cursorBrush);
            }
        }

        private void SubdivideX(IDrawingContext dc, PaintDotNet.UI.Media.Brush tickBrush, double x, double divisionInPixels, double division, int index, double y, double height, int tickLevel, int[] subdivs)
        {
            double num = height * tickHeights[tickLevel];
            dc.DrawLine(x + halfTickThickness, y, x + halfTickThickness, y - num, tickBrush, tickThickness);
            if ((index <= 10) && (tickLevel < 5))
            {
                double num2;
                if ((subdivs != null) && (index >= 0))
                {
                    num2 = subdivs[index % subdivs.Length];
                }
                else
                {
                    if (index >= 0)
                    {
                        return;
                    }
                    int num5 = (-index - 1) % integerSubDivisors.Length;
                    if (((tickLevel == 0) && (num5 != 0)) && (divisionInPixels <= 80.0))
                    {
                        num5 = 1;
                        height *= 0.6;
                    }
                    if ((tickLevel == 1) && (divisionInPixels >= 40.0))
                    {
                        num5 = 1;
                    }
                    num2 = integerSubDivisors[num5];
                }
                double num3 = divisionInPixels / num2;
                double num4 = division / num2;
                if (!((subdivs == null) & (num4 != ((int) num4))) && (num3 > 6.5))
                {
                    for (int i = 0; i < num2; i++)
                    {
                        double num7 = x + ((divisionInPixels * i) / num2);
                        this.SubdivideX(dc, tickBrush, num7, num3, num4, index + 1, y, height, tickLevel + 1, subdivs);
                    }
                }
            }
        }

        private void SubdivideY(IDrawingContext dc, PaintDotNet.UI.Media.Brush tickBrush, double y, double divisionInPixels, double division, int index, double x, double width, int tickLevel, int[] subdivs)
        {
            double num = width * tickHeights[tickLevel];
            dc.DrawLine(x, y + halfTickThickness, x - num, y + halfTickThickness, tickBrush, tickThickness);
            if ((index <= 10) && (tickLevel < 5))
            {
                double num2;
                if ((subdivs != null) && (index >= 0))
                {
                    num2 = subdivs[index % subdivs.Length];
                }
                else
                {
                    if (index >= 0)
                    {
                        return;
                    }
                    int num4 = (-index - 1) % integerSubDivisors.Length;
                    if (((tickLevel == 0) && (num4 != 0)) && (divisionInPixels <= 80.0))
                    {
                        num4 = 1;
                        width *= 0.6;
                    }
                    if ((tickLevel == 1) && (divisionInPixels >= 40.0))
                    {
                        num4 = 1;
                    }
                    num2 = integerSubDivisors[num4];
                }
                double num3 = divisionInPixels / num2;
                division /= num2;
                if (!((subdivs == null) & (division != ((int) division))) && (num3 > 6.5))
                {
                    for (int i = 0; i < num2; i++)
                    {
                        double num6 = y + ((divisionInPixels * i) / num2);
                        this.SubdivideY(dc, tickBrush, num6, num3, division, index + 1, x, width, tickLevel + 1, subdivs);
                    }
                }
            }
        }

        private double AdjustedOffset
        {
            get
            {
                if (this.orientation == System.Windows.Forms.Orientation.Horizontal)
                {
                    return (this.Offset + this.ScaleFactor.Unscale(((double) base.ClientRectangle.Height)));
                }
                return this.Offset;
            }
        }

        [DefaultValue((double) 96.0)]
        public double Dpu
        {
            get => 
                this.dpu;
            set
            {
                if (value != this.dpu)
                {
                    this.dpu = value;
                    base.Invalidate();
                }
            }
        }

        public bool HighlightEnabled
        {
            get => 
                this.highlightEnabled;
            set
            {
                if (this.highlightEnabled != value)
                {
                    this.highlightEnabled = value;
                    base.Invalidate();
                }
            }
        }

        public double HighlightLength
        {
            get => 
                this.highlightLength;
            set
            {
                if (this.highlightLength != value)
                {
                    this.highlightLength = value;
                    base.Invalidate();
                }
            }
        }

        public double HighlightStart
        {
            get => 
                this.highlightStart;
            set
            {
                if (this.highlightStart != value)
                {
                    this.highlightStart = value;
                    base.Invalidate();
                }
            }
        }

        public PaintDotNet.MeasurementUnit MeasurementUnit
        {
            get => 
                this.measurementUnit;
            set
            {
                if (value != this.measurementUnit)
                {
                    this.measurementUnit = value;
                    base.Invalidate();
                }
            }
        }

        [DefaultValue(0)]
        public double Offset
        {
            get => 
                this.offset;
            set
            {
                if (this.offset != value)
                {
                    this.offset = value;
                    base.Invalidate();
                }
            }
        }

        [DefaultValue(0)]
        public System.Windows.Forms.Orientation Orientation
        {
            get => 
                this.orientation;
            set
            {
                if (this.orientation != value)
                {
                    this.orientation = value;
                    base.Invalidate();
                }
            }
        }

        public PaintDotNet.ScaleFactor ScaleFactor
        {
            get => 
                this.scaleFactor;
            set
            {
                if (this.scaleFactor != value)
                {
                    this.scaleFactor = value;
                    base.Invalidate();
                }
            }
        }

        [DefaultValue(0)]
        public double Value
        {
            get => 
                this.rulerValue;
            set
            {
                if (this.rulerValue != value)
                {
                    RectDouble num4;
                    RectDouble num7;
                    Rectangle clientRectangle = base.ClientRectangle;
                    double adjustedOffset = this.AdjustedOffset;
                    double x = this.scaleFactor.Scale(((double) (this.rulerValue + adjustedOffset))) - 1.0;
                    double num3 = this.scaleFactor.Scale(((double) ((this.rulerValue + 1.0) + adjustedOffset))) + 1.0;
                    if (this.orientation == System.Windows.Forms.Orientation.Horizontal)
                    {
                        num4 = new RectDouble(x, (double) base.ClientRectangle.Top, num3 - x, (double) clientRectangle.Height);
                    }
                    else
                    {
                        num4 = new RectDouble((double) clientRectangle.Left, x, (double) clientRectangle.Width, num3 - x);
                    }
                    double num5 = this.scaleFactor.Scale((double) (value + adjustedOffset));
                    double num6 = this.scaleFactor.Scale((double) ((value + 1.0) + adjustedOffset));
                    if (this.orientation == System.Windows.Forms.Orientation.Horizontal)
                    {
                        num7 = new RectDouble(num5, (double) clientRectangle.Top, num6 - num5, (double) clientRectangle.Height);
                    }
                    else
                    {
                        num7 = new RectDouble((double) clientRectangle.Left, num5, (double) clientRectangle.Width, num6 - num5);
                    }
                    this.rulerValue = value;
                    base.Invalidate(num4.Int32Bound);
                    base.Invalidate(num7.Int32Bound);
                }
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly Ruler.<>c <>9 = new Ruler.<>c();
            public static Func<SizedFontProperties> <>9__61_0;

            internal SizedFontProperties <.ctor>b__61_0()
            {
                using (IGdiFontMap map = DirectWriteFactory.Instance.GetGdiFontMapRef(true))
                {
                    FontProperties fontProperties;
                    try
                    {
                        fontProperties = map.GetFontProperties(Ruler.tickLabelFontNames);
                    }
                    catch (NoFontException)
                    {
                        fontProperties = new FontProperties(string.Empty, string.Empty, FontWeight.Normal, FontStretch.Normal, PaintDotNet.DirectWrite.FontStyle.Normal, TextDecorations.None);
                    }
                    return new SizedFontProperties(fontProperties, UIUtil.ScaleHeight((float) 10.66667f));
                }
            }
        }

        private sealed class CachingMeasurementsUITextFactory : Ruler.CachingUITextFactory
        {
            private MeasurementsTextLayoutCache measurementsTextLayoutCache;

            public CachingMeasurementsUITextFactory()
            {
                this.measurementsTextLayoutCache = new MeasurementsTextLayoutCache(this, 100);
            }

            public TextLayout CreateLayout(IDrawingContext dc, string abbString, SizedFontProperties unitsBoxFont, double boxLength)
            {
                TextLayout key = base.CreateLayout(dc, abbString, unitsBoxFont, null, HotkeyRenderMode.Ignore, boxLength, boxLength);
                return this.measurementsTextLayoutCache.Get(key);
            }

            private sealed class MeasurementsTextLayoutCache : BoundedGenerationalCache<TextLayout, TextLayout>
            {
                private readonly Ruler.CachingMeasurementsUITextFactory owner;

                public MeasurementsTextLayoutCache(Ruler.CachingMeasurementsUITextFactory owner, int genItemCountThreshold) : base(genItemCountThreshold)
                {
                    this.owner = owner;
                }

                public override TextLayout CreateItem(TextLayout key)
                {
                    TextLayout layout = key.Clone();
                    layout.ParagraphAlignment = ParagraphAlignment.Center;
                    layout.TextAlignment = PaintDotNet.DirectWrite.TextAlignment.Center;
                    layout.WordWrapping = WordWrapping.NoWrap;
                    layout.Freeze();
                    return layout;
                }

                public override void DisposeItem(TextLayout item)
                {
                }
            }
        }

        private class CachingUITextFactory : UITextFactory
        {
            protected const int genItemCountThreshold = 100;
            private readonly TextFormatCache textFormatCache;
            private readonly TextLayoutCache textLayoutCache;

            public CachingUITextFactory()
            {
                this.textFormatCache = new TextFormatCache(this, 100);
                this.textLayoutCache = new TextLayoutCache(this, 100);
            }

            public override TextFormat CreateFormat(string fontFamilyName, FontWeight fontWeight, PaintDotNet.DirectWrite.FontStyle fontStyle, FontStretch fontStretch, double fontSize) => 
                this.textFormatCache.Get(TupleStruct.Create<string, FontWeight, PaintDotNet.DirectWrite.FontStyle, FontStretch, double>(fontFamilyName, fontWeight, fontStyle, fontStretch, fontSize));

            private TextFormat CreateFormatBase(string fontFamilyName, FontWeight fontWeight, PaintDotNet.DirectWrite.FontStyle fontStyle, FontStretch fontStretch, double fontSize) => 
                base.CreateFormat(fontFamilyName, fontWeight, fontStyle, fontStretch, fontSize);

            public override TextLayout CreateLayout(string text, TextFormat textFormat, TextLayoutAlgorithm layoutAlgorithm = 0, double gdiPixelsPerDip = 1.0, HotkeyRenderMode hotkeyRenderMode = 0, TextDecorations textDecorations = 0, double maxWidth = 65535.0, double maxHeight = 65535.0) => 
                this.textLayoutCache.Get(TupleStruct.Create<string, TextFormat, TextLayoutAlgorithm, double, HotkeyRenderMode, TextDecorations, double, double>(text, textFormat, layoutAlgorithm, gdiPixelsPerDip, hotkeyRenderMode, textDecorations, maxWidth, maxHeight));

            private TextLayout CreateLayoutBase(string text, TextFormat textFormat, TextLayoutAlgorithm layoutAlgorithm = 0, double gdiPixelsPerDip = 1.0, HotkeyRenderMode hotkeyRenderMode = 0, TextDecorations textDecorations = 0, double maxWidth = 65535.0, double maxHeight = 65535.0) => 
                base.CreateLayout(text, textFormat, layoutAlgorithm, gdiPixelsPerDip, hotkeyRenderMode, textDecorations, maxWidth, maxHeight);

            private sealed class TextFormatCache : BoundedGenerationalCache<TupleStruct<string, FontWeight, PaintDotNet.DirectWrite.FontStyle, FontStretch, double>, TextFormat>
            {
                private readonly Ruler.CachingUITextFactory owner;

                public TextFormatCache(Ruler.CachingUITextFactory owner, int genItemCountThreshold) : base(genItemCountThreshold)
                {
                    this.owner = owner;
                }

                public override TextFormat CreateItem(TupleStruct<string, FontWeight, PaintDotNet.DirectWrite.FontStyle, FontStretch, double> key) => 
                    this.owner.CreateFormatBase(key.Item1, key.Item2, key.Item3, key.Item4, key.Item5).EnsureFrozen<TextFormat>();

                public override void DisposeItem(TextFormat item)
                {
                }
            }

            private sealed class TextLayoutCache : BoundedGenerationalCache<TupleStruct<string, TextFormat, TextLayoutAlgorithm, double, HotkeyRenderMode, TextDecorations, double, double>, TextLayout>
            {
                private readonly Ruler.CachingUITextFactory owner;

                public TextLayoutCache(Ruler.CachingUITextFactory owner, int genItemCountThreshold) : base(genItemCountThreshold)
                {
                    this.owner = owner;
                }

                public override TextLayout CreateItem(TupleStruct<string, TextFormat, TextLayoutAlgorithm, double, HotkeyRenderMode, TextDecorations, double, double> key) => 
                    this.owner.CreateLayoutBase(key.Item1, key.Item2, key.Item3, key.Item4, key.Item5, key.Item6, key.Item7, key.Item8).EnsureFrozen<TextLayout>();

                public override void DisposeItem(TextLayout item)
                {
                }
            }
        }
    }
}

