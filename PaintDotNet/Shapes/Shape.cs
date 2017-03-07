namespace PaintDotNet.Shapes
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Direct2D;
    using PaintDotNet.DirectWrite;
    using PaintDotNet.Drawing;
    using PaintDotNet.Imaging;
    using PaintDotNet.MemoryManagement;
    using PaintDotNet.PropertySystem;
    using PaintDotNet.Rendering;
    using PaintDotNet.Settings;
    using PaintDotNet.Settings.App;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.UI;
    using PaintDotNet.UI.Media;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal abstract class Shape
    {
        private double? cachedAspectRatio;
        private ShapeCategory category;
        private string displayName;
        private ConcurrentDictionary<int, ImageResource> edgeSizeToImageMap;
        private Lazy<HashSet<string>> lazyRenderSettingPaths;
        private ShapeOptions options;

        protected Shape(string displayName, ShapeCategory category) : this(displayName, category, ShapeOptions.Default)
        {
        }

        protected Shape(string displayName, ShapeCategory category, ShapeOptions options)
        {
            this.edgeSizeToImageMap = new ConcurrentDictionary<int, ImageResource>();
            Validate.Begin().IsNotNullOrWhiteSpace(displayName, "displayName").IsNotNull<ShapeOptions>(options, "options").Check();
            this.displayName = displayName;
            this.category = category;
            this.options = options;
            this.lazyRenderSettingPaths = new Lazy<HashSet<string>>(() => new HashSet<string>(this.OnGetRenderSettingPaths().Distinct<string>()), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        private static string ConvertToolsPathToToolDefaultsPath(string settingPath)
        {
            string[] pathComponents = SettingPath.GetPathComponents(settingPath);
            if (pathComponents[0] == ToolSettings.Null.Path)
            {
                pathComponents[0] = AppSettings.Instance.ToolDefaults.Path;
            }
            return SettingPath.CombinePathComponents(pathComponents);
        }

        public ShapeRenderData CreateImageRenderData(ShapeRenderParameters renderParams) => 
            this.CreateRenderDataImpl(renderParams, new Func<ShapeRenderParameters, ShapeRenderData>(this.OnCreateImageRenderData));

        private ImageResource CreateImageResource(int edgeSizePx)
        {
            int width = edgeSizePx;
            int height = edgeSizePx;
            int num3 = edgeSizePx / 0x10;
            int num4 = Math.Max(1, num3);
            return this.CreateImageResource(width, height, (double) num4);
        }

        private ImageResource CreateImageResource(int width, int height, double borderSize)
        {
            ImageResource resource2;
            double x = borderSize / 2.0;
            using (ISurface<ColorBgra> surface = SurfaceAllocator.Bgra.Allocate(width, height, AllocationOptions.ZeroFillNotRequired))
            {
                using (IDrawingContext context = DrawingContext.FromSurface(surface, AlphaMode.Premultiplied, FactorySource.PerThread))
                {
                    RectDouble num4;
                    context.Clear(null);
                    RectDouble num2 = new RectDouble(x, x, width - borderSize, height - borderSize);
                    double aspectRatio = this.AspectRatio;
                    if (aspectRatio > 1.0)
                    {
                        double num5 = num2.Height / aspectRatio;
                        num4 = new RectDouble(num2.X, num2.Y + ((num2.Height - num5) / 2.0), num2.Width, num5);
                    }
                    else if (aspectRatio < 1.0)
                    {
                        double num6 = num2.Width * aspectRatio;
                        num4 = new RectDouble(num2.X + ((num2.Width - num6) / 2.0), num2.Y, num6, num2.Height);
                    }
                    else
                    {
                        num4 = num2;
                    }
                    IDictionary<string, object> settingValues = (from p in this.RenderSettingPaths select KeyValuePairUtil.Create<string, object>(p, AppSettings.Instance[ConvertToolsPathToToolDefaultsPath(p)].Value)).ToDictionary<string, object>();
                    ShapeRenderParameters renderParams = new ShapeRenderParameters(num4.TopLeft, num4.BottomRight, new VectorDouble(1.0, 1.0), settingValues, null);
                    PropertyCollection properties = this.CreatePropertyCollection(renderParams);
                    this.OnSetImagePropertyCollectionValues(renderParams, properties);
                    IDictionary<object, object> propertyValues = (from p in properties select KeyValuePairUtil.Create<object, object>(p.GetOriginalNameValue(), p.Value)).ToDictionary<object, object>();
                    ShapeRenderParameters parameters2 = new ShapeRenderParameters(num4.TopLeft, num4.BottomRight, new VectorDouble(1.0, 1.0), settingValues, propertyValues);
                    ShapeRenderData data = this.CreateImageRenderData(parameters2);
                    PaintDotNet.UI.Media.Brush brush = SolidColorBrushCache.Get((ColorRgba128Float) ColorBgra.FromUInt32(0xff5894c1));
                    if (data.InteriorFill != null)
                    {
                        context.FillGeometry(data.InteriorFill.Geometry, SolidColorBrushCache.Get((ColorRgba128Float) Colors.White), null);
                        LinearGradientBrush brush2 = new LinearGradientBrush {
                            StartPoint = num4.TopLeft,
                            EndPoint = num4.BottomRight
                        };
                        brush2.GradientStops.Add(new GradientStop((ColorRgba128Float) ColorBgra32.FromUInt32(0xffc0e1f3), 0.0));
                        brush2.GradientStops.Add(new GradientStop((ColorRgba128Float) ColorBgra32.FromUInt32(0xffe0eff8), 1.0));
                        context.FillGeometry(data.InteriorFill.Geometry, brush2, null);
                    }
                    if ((data.InteriorFill != null) && (data.OutlineDraw != null))
                    {
                        RenderLayer layer = RenderLayerCache.Get();
                        using (context.UseLayer(layer, null, data.InteriorFill.Geometry, AntialiasMode.PerPrimitive, null, 1.0, null, LayerOptions.None))
                        {
                            context.DrawGeometry(data.OutlineDraw.Geometry, SolidColorBrushCache.Get((ColorRgba128Float) Colors.White), 3.0);
                        }
                        RenderLayerCache.Return(layer);
                    }
                    if (data.OutlineDraw != null)
                    {
                        context.DrawGeometry(data.OutlineDraw.Geometry, brush, 1.0);
                    }
                    if (data.OutlineFill != null)
                    {
                        context.FillGeometry(data.OutlineFill.Geometry, brush, null);
                    }
                    string imageStringOverlay = this.ImageStringOverlay;
                    if (imageStringOverlay != string.Empty)
                    {
                        double num7 = (width * 7.0) / 16.0;
                        double fontSize = UIUtil.ScaleWidth(num7);
                        TextLayout textLayout = new TextLayout(imageStringOverlay, "Arial", FontWeight.Normal, PaintDotNet.DirectWrite.FontStyle.Normal, FontStretch.Normal, fontSize) {
                            ParagraphAlignment = ParagraphAlignment.Center,
                            TextAlignment = PaintDotNet.DirectWrite.TextAlignment.Center,
                            MaxWidth = width - 2,
                            MaxHeight = height - 2
                        };
                        context.DrawTextLayout(new PointDouble(1.0, 1.0), textLayout, SolidColorBrushCache.Get((ColorRgba128Float) ColorBgra.FromUInt32(0xff5894c1)), DrawTextOptions.None);
                    }
                }
                surface.ConvertFromPremultipliedAlpha();
                using (System.Drawing.Bitmap bitmap = surface.CreateAliasedGdipBitmap())
                {
                    System.Drawing.Bitmap image = new System.Drawing.Bitmap(bitmap);
                    resource2 = ImageResource.FromImage(image);
                }
            }
            return resource2;
        }

        public PropertyCollection CreatePropertyCollection(ShapeRenderParameters renderParams)
        {
            Validate.IsNotNull<ShapeRenderParameters>(renderParams, "renderParams");
            if (renderParams.HasPropertyValues)
            {
                throw new ArgumentException("renderParams.HasPropertyValues must be false");
            }
            return this.OnCreatePropertyCollection(renderParams).Clone();
        }

        public ShapeRenderData CreateRenderData(ShapeRenderParameters renderParams) => 
            this.CreateRenderDataImpl(renderParams, new Func<ShapeRenderParameters, ShapeRenderData>(this.OnCreateRenderData));

        private ShapeRenderData CreateRenderDataImpl(ShapeRenderParameters renderParams, Func<ShapeRenderParameters, ShapeRenderData> onCreate)
        {
            Validate.IsNotNull<ShapeRenderParameters>(renderParams, "renderParams");
            VerifyRenderSettingValues(this.RenderSettingPaths, renderParams.SettingValues);
            if (((!renderParams.StartPoint.IsFinite || !renderParams.EndPoint.IsFinite) || ((this.options.Elide == ShapeElideOption.ZeroWidthOrZeroHeight) && !RectDouble.FromCorners(renderParams.StartPoint, renderParams.EndPoint).HasPositiveArea)) || ((this.options.Elide == ShapeElideOption.ZeroWidthAndZeroHeight) && (renderParams.StartPoint == renderParams.EndPoint)))
            {
                return new ShapeRenderData(Geometry.Empty);
            }
            ShapeRenderData renderData = onCreate(renderParams);
            RectDouble bounds = RectDouble.FromCorners(renderParams.StartPoint, renderParams.EndPoint);
            Matrix3x2Double alignmentTransform = GetAlignmentTransform(renderData.Guide.Bounds, bounds);
            return ShapeRenderData.Transform(renderData, alignmentTransform);
        }

        private static Matrix3x2Double GetAlignmentTransform(RectDouble geometryBounds, RectDouble bounds)
        {
            if (geometryBounds == bounds)
            {
                return Matrix3x2Double.Identity;
            }
            double dx = bounds.X - geometryBounds.X;
            double dy = bounds.Y - geometryBounds.Y;
            double x = bounds.Width / geometryBounds.Width;
            double scaleX = x.IsFinite() ? x : 1.0;
            double num5 = bounds.Height / geometryBounds.Height;
            double scaleY = num5.IsFinite() ? num5 : 1.0;
            Matrix3x2Double num7 = Matrix3x2Double.Translation(dx, dy);
            Matrix3x2Double num8 = Matrix3x2Double.ScalingAt(scaleX, scaleY, bounds.X, bounds.Y);
            return (num7 * num8);
        }

        private double GetAspectRatio()
        {
            if (!this.cachedAspectRatio.HasValue)
            {
                double num = this.OnGetAspectRatio();
                if (!this.cachedAspectRatio.HasValue)
                {
                    this.cachedAspectRatio = new double?(num);
                }
            }
            return this.cachedAspectRatio.Value;
        }

        public ImageResource GetImageResourceDip(int edgeSizeDip) => 
            this.GetImageResourcePx(UIUtil.ScaleHeight(edgeSizeDip));

        public ImageResource GetImageResourcePx(int edgeSizePx) => 
            this.edgeSizeToImageMap.GetOrAdd(edgeSizePx, new Func<int, ImageResource>(this.CreateImageResource));

        protected virtual ShapeRenderData OnCreateImageRenderData(ShapeRenderParameters renderParams) => 
            this.OnCreateRenderData(renderParams);

        protected virtual PropertyCollection OnCreatePropertyCollection(ShapeRenderParameters renderParams) => 
            PropertyCollection.CreateEmpty();

        protected abstract ShapeRenderData OnCreateRenderData(ShapeRenderParameters renderParams);
        protected virtual double OnGetAspectRatio() => 
            1.0;

        protected virtual IEnumerable<string> OnGetRenderSettingPaths() => 
            Array.Empty<string>();

        protected virtual void OnSetImagePropertyCollectionValues(ShapeRenderParameters renderParams, PropertyCollection properties)
        {
        }

        private static void VerifyRenderSettingValues(ICollection<string> settingPaths, IDictionary<string, object> settingValues)
        {
            Validate.Begin().IsNotNull<ICollection<string>>(settingPaths, "settingPaths").IsNotNull<IDictionary<string, object>>(settingValues, "settingValues").Check();
            foreach (string str in settingPaths)
            {
                if (!settingValues.ContainsKey(str))
                {
                    throw new ArgumentException($"missing setting: {str}");
                }
            }
        }

        public double AspectRatio =>
            this.GetAspectRatio();

        public ShapeCategory Category =>
            this.category;

        public string DisplayName =>
            this.displayName;

        public abstract string ID { get; }

        protected virtual string ImageStringOverlay =>
            string.Empty;

        public ShapeOptions Options =>
            this.options;

        public ICollection<string> RenderSettingPaths =>
            this.lazyRenderSettingPaths.Value;

        public abstract string ToolTipText { get; }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly Shape.<>c <>9 = new Shape.<>c();
            public static Func<string, KeyValuePair<string, object>> <>9__22_0;
            public static Func<Property, KeyValuePair<object, object>> <>9__22_1;

            internal KeyValuePair<string, object> <CreateImageResource>b__22_0(string p) => 
                KeyValuePairUtil.Create<string, object>(p, AppSettings.Instance[Shape.ConvertToolsPathToToolDefaultsPath(p)].Value);

            internal KeyValuePair<object, object> <CreateImageResource>b__22_1(Property p) => 
                KeyValuePairUtil.Create<object, object>(p.GetOriginalNameValue(), p.Value);
        }
    }
}

