namespace PaintDotNet.Rendering
{
    using PaintDotNet;
    using PaintDotNet.Direct2D;
    using PaintDotNet.Imaging;
    using PaintDotNet.ObjectModel;
    using PaintDotNet.UI.Media;
    using System;
    using System.Drawing.Drawing2D;
    using System.Windows;

    internal sealed class PdnLegacyBrush : PdnBrush
    {
        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register("Background", typeof(ColorBgra), typeof(PdnLegacyBrush), new PropertyMetadata((ColorBgra) Colors.White));
        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register("Foreground", typeof(ColorBgra), typeof(PdnLegacyBrush), new PropertyMetadata((ColorBgra) Colors.Black));
        public static readonly DependencyProperty HatchStyleProperty = DependencyProperty.Register("HatchStyle", typeof(PaintDotNet.UI.Media.HatchStyle), typeof(PdnLegacyBrush), new PropertyMetadata(EnumUtil.GetBoxed<PaintDotNet.UI.Media.HatchStyle>(PaintDotNet.UI.Media.HatchStyle.BackwardDiagonal)));
        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register("Type", typeof(PaintDotNet.BrushType), typeof(PdnLegacyBrush), new PropertyMetadata(EnumUtil.GetBoxed<PaintDotNet.BrushType>(PaintDotNet.BrushType.None)));

        public PdnLegacyBrush()
        {
        }

        public PdnLegacyBrush(PaintDotNet.BrushType type, PaintDotNet.UI.Media.HatchStyle hatchStyle, ColorBgra foreground, ColorBgra background)
        {
            this.Type = type;
            this.HatchStyle = hatchStyle;
            this.Foreground = foreground;
            this.Background = background;
        }

        public PdnLegacyBrush(PaintDotNet.BrushType type, System.Drawing.Drawing2D.HatchStyle hatchStyle, ColorBgra foreground, ColorBgra background) : this(type, (PaintDotNet.UI.Media.HatchStyle) hatchStyle, foreground, background)
        {
        }

        protected override Freezable CreateInstanceCore() => 
            new PdnLegacyBrush();

        protected override IRenderer<ColorBgra> OnCreateRenderer(int width, int height) => 
            new Renderer(width, height, this.Type, this.HatchStyle, this.Foreground, this.Background);

        public ColorBgra Background
        {
            get => 
                ((ColorBgra) base.GetValue(BackgroundProperty));
            set
            {
                base.SetValue(BackgroundProperty, value);
            }
        }

        public ColorBgra Foreground
        {
            get => 
                ((ColorBgra) base.GetValue(ForegroundProperty));
            set
            {
                base.SetValue(ForegroundProperty, value);
            }
        }

        public PaintDotNet.UI.Media.HatchStyle HatchStyle
        {
            get => 
                ((PaintDotNet.UI.Media.HatchStyle) base.GetValue(HatchStyleProperty));
            set
            {
                base.SetValue(HatchStyleProperty, EnumUtil.GetBoxed<PaintDotNet.UI.Media.HatchStyle>(value));
            }
        }

        public PaintDotNet.BrushType Type
        {
            get => 
                ((PaintDotNet.BrushType) base.GetValue(TypeProperty));
            set
            {
                base.SetValue(TypeProperty, EnumUtil.GetBoxed<PaintDotNet.BrushType>(value));
            }
        }

        private sealed class Renderer : IRenderer<ColorBgra>
        {
            private ColorBgra background;
            private Brush d2dBrush;
            private ColorBgra foreground;
            private PaintDotNet.UI.Media.HatchStyle hatchStyle;
            private ISurface<ColorBgra> hatchSurface;
            private int height;
            private PaintDotNet.BrushType type;
            private int width;

            public Renderer(int width, int height, PaintDotNet.BrushType type, PaintDotNet.UI.Media.HatchStyle hatchStyle, ColorBgra foreground, ColorBgra background)
            {
                Brush brush;
                this.width = width;
                this.height = height;
                this.type = type;
                this.hatchStyle = hatchStyle;
                this.foreground = foreground;
                this.background = background;
                switch (type)
                {
                    case PaintDotNet.BrushType.Solid:
                        brush = SolidColorBrushCache.Get((ColorRgba128Float) foreground);
                        break;

                    case PaintDotNet.BrushType.Hatch:
                        brush = new PaintDotNet.UI.Media.HatchBrush(hatchStyle, (ColorRgba128Float) foreground, (ColorRgba128Float) background);
                        break;

                    case PaintDotNet.BrushType.None:
                        brush = SolidColorBrushCache.Get((ColorRgba128Float) Colors.Transparent, 0.0);
                        break;

                    default:
                        throw ExceptionUtil.InvalidEnumArgumentException<PaintDotNet.BrushType>(type, "type");
                }
                this.d2dBrush = brush.EnsureFrozen<Brush>();
                if (this.d2dBrush is PaintDotNet.UI.Media.HatchBrush)
                {
                    this.hatchSurface = ((PaintDotNet.UI.Media.HatchBrush) this.d2dBrush).CreateHatchSurface();
                }
            }

            public unsafe void Render(ISurface<ColorBgra> dst, PointInt32 renderOffset)
            {
                RectDouble rect = new RectDouble(0.0, 0.0, (double) this.width, (double) this.height);
                if (this.type == PaintDotNet.BrushType.Solid)
                {
                    dst.Clear(this.foreground);
                }
                else if (this.hatchSurface != null)
                {
                    int width = dst.Width;
                    int height = dst.Height;
                    int num4 = this.hatchSurface.Width;
                    int num5 = this.hatchSurface.Height;
                    for (int i = 0; i < height; i++)
                    {
                        int y = (i + renderOffset.Y) % num5;
                        ColorBgra* rowAddressUnchecked = dst.GetRowAddressUnchecked(i);
                        ColorBgra* bgraPtr2 = this.hatchSurface.GetRowAddressUnchecked(y);
                        for (int j = 0; j < width; j++)
                        {
                            int index = (j + renderOffset.X) % num4;
                            rowAddressUnchecked[j] = bgraPtr2[index];
                        }
                    }
                }
                else
                {
                    using (IDrawingContext context = DrawingContext.FromSurface(dst, AlphaMode.Premultiplied, FactorySource.PerThread))
                    {
                        using (context.UseTranslateTransform((float) -renderOffset.X, (float) -renderOffset.Y, MatrixMultiplyOrder.Prepend))
                        {
                            context.FillRectangle(rect, this.d2dBrush);
                        }
                    }
                    dst.ConvertFromPremultipliedAlpha();
                }
            }

            public int Height =>
                this.height;

            public int Width =>
                this.width;
        }
    }
}

