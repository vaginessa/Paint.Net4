namespace PaintDotNet.Settings.App
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Direct2D;
    using PaintDotNet.DirectWrite;
    using PaintDotNet.Dxgi;
    using PaintDotNet.Imaging;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.Settings;
    using PaintDotNet.Shapes;
    using PaintDotNet.Shapes.Lines;
    using PaintDotNet.Snap;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Tools;
    using PaintDotNet.Tools.Recolor;
    using PaintDotNet.Tools.Shapes;
    using PaintDotNet.VisualStyling;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    internal class AppSettings : SettingsBase
    {
        public readonly EffectsSection Effects;
        public readonly FileSection File;
        private static AppSettings publicNullInstance;
        private static readonly AppSettings publicRegistryInstance = new AppSettings(RegistryStorageHandler.Instance);
        public readonly ToolsSection ToolDefaults;
        public readonly UISection UI;
        public readonly UpdatesSection Updates;
        public readonly WindowSection Window;
        public readonly WorkspaceSection Workspace;

        public AppSettings(SettingsStorageHandler storageHandler) : base(storageHandler)
        {
            this.Effects = base.RegisterSectionDuringCtor<EffectsSection>(new EffectsSection());
            this.File = base.RegisterSectionDuringCtor<FileSection>(new FileSection());
            this.ToolDefaults = base.RegisterSectionDuringCtor<ToolsSection>(new ToolsSection("ToolDefaults"));
            this.UI = base.RegisterSectionDuringCtor<UISection>(new UISection());
            this.Updates = base.RegisterSectionDuringCtor<UpdatesSection>(new UpdatesSection());
            this.Window = base.RegisterSectionDuringCtor<WindowSection>(new WindowSection());
            this.Workspace = base.RegisterSectionDuringCtor<WorkspaceSection>(new WorkspaceSection());
            base.EndInit();
        }

        public static AppSettings Instance =>
            publicRegistryInstance;

        public static AppSettings Null
        {
            get
            {
                if (publicNullInstance == null)
                {
                    publicNullInstance = new AppSettings(NullStorageHandler.Instance);
                }
                return publicNullInstance;
            }
        }

        public sealed class EffectsSection : SettingsSection
        {
            public readonly Int32Setting DefaultQualityLevel;

            public EffectsSection() : base("Effects")
            {
                this.DefaultQualityLevel = base.Register<Int32Setting>(new Int32Setting("Effects/DefaultQualityLevel", SettingScope.CurrentUser, 2, 1, 5));
            }
        }

        public sealed class FileSection : SettingsSection
        {
            public readonly StringSetting DialogDirectory;
            public readonly MostRecentSection MostRecent;

            public FileSection() : base("File")
            {
                this.DialogDirectory = base.Register<StringSetting>(new StringSetting(base.GetSettingPath("DialogDirectory"), SettingScope.CurrentUser, string.Empty, true));
                this.MostRecent = new MostRecentSection(this);
            }

            public sealed class MostRecentSection : SettingsSection
            {
                public readonly Int32Setting FileCount;
                private const int maxCount = 8;
                public readonly StringSetting[] Paths;
                public readonly ByteArraySetting[] Thumbnails;

                public MostRecentSection(SettingsSection parent) : base(parent, "MostRecent")
                {
                    this.FileCount = base.Register<Int32Setting>(new Int32Setting(base.GetSettingPath("FileCount"), SettingScope.CurrentUser, 0, 0, this.MaxCount));
                    this.Paths = new StringSetting[8];
                    this.Thumbnails = new ByteArraySetting[8];
                    for (int i = 0; i < 8; i++)
                    {
                        this.Paths[i] = base.Register<StringSetting>(new StringSetting(base.GetSettingPath("Path" + i.ToString(CultureInfo.InvariantCulture)), SettingScope.CurrentUser, string.Empty, true));
                        this.Thumbnails[i] = base.Register<ByteArraySetting>(new ByteArraySetting(base.GetSettingPath("Thumbnail" + i.ToString(CultureInfo.InvariantCulture)), SettingScope.CurrentUser, Array.Empty<byte>()));
                    }
                }

                public int MaxCount =>
                    8;
            }
        }

        public sealed class ToolsSection : SettingsSection
        {
            public readonly StringSetting ActiveToolName;
            public readonly BooleanSetting Antialiasing;
            public readonly EnumSetting<ContentBlendMode> BlendMode;
            public readonly BrushSection Brush;
            public readonly EnumSetting<PaintDotNet.ColorPickerClickBehavior> ColorPickerClickBehavior;
            public readonly EnumSetting<PaintDotNet.FloodMode> FloodMode;
            public readonly GradientSection Gradient;
            public readonly EnumSetting<ResamplingAlgorithm> MoveToolResamplingAlgorithm;
            public readonly PenSection Pen;
            public readonly EnumSetting<PaintDotNet.PixelSampleMode> PixelSampleMode;
            public readonly ColorBgra32Setting PrimaryColor;
            public readonly FloatSetting Radius;
            public readonly EnumSetting<PaintDotNet.Tools.Recolor.RecolorToolSamplingMode> RecolorToolSamplingMode;
            public readonly BooleanSetting SampleAllLayers;
            public readonly ColorBgra32Setting SecondaryColor;
            public readonly SelectionSection Selection;
            public readonly ShapesSection Shapes;
            public readonly TextSection Text;
            public readonly FloatSetting Tolerance;

            public ToolsSection(string pathPrefix) : base(pathPrefix)
            {
                this.Brush = new BrushSection(this);
                this.Gradient = new GradientSection(this);
                this.Pen = new PenSection(this);
                this.Selection = new SelectionSection(this);
                this.Shapes = new ShapesSection(this);
                this.Text = new TextSection(this);
                this.ActiveToolName = base.Register<StringSetting>(new StringSetting(base.GetSettingPath("ActiveToolName"), SettingScope.CurrentUser, PaintDotNet.Tools.Tool.DefaultToolType.Name, true));
                this.Antialiasing = base.Register<BooleanSetting>(new BooleanSetting(base.GetSettingPath("Antialiasing"), SettingScope.CurrentUser, true));
                this.BlendMode = base.Register<EnumSetting<ContentBlendMode>>(new EnumSetting<ContentBlendMode>(base.GetSettingPath("BlendMode"), SettingScope.CurrentUser, ContentBlendMode.Normal));
                this.FloodMode = base.Register<EnumSetting<PaintDotNet.FloodMode>>(new EnumSetting<PaintDotNet.FloodMode>(base.GetSettingPath("FloodMode"), SettingScope.CurrentUser, PaintDotNet.FloodMode.Local));
                this.PixelSampleMode = base.Register<EnumSetting<PaintDotNet.PixelSampleMode>>(new EnumSetting<PaintDotNet.PixelSampleMode>(base.GetSettingPath("PixelSampleMode"), SettingScope.CurrentUser, PaintDotNet.PixelSampleMode.PointSample));
                this.SampleAllLayers = base.Register<BooleanSetting>(new BooleanSetting(base.GetSettingPath("SampleAllLayers"), SettingScope.CurrentUser, false));
                this.ColorPickerClickBehavior = base.Register<EnumSetting<PaintDotNet.ColorPickerClickBehavior>>(new EnumSetting<PaintDotNet.ColorPickerClickBehavior>(base.GetSettingPath("ColorPickerClickBehavior"), SettingScope.CurrentUser, PaintDotNet.ColorPickerClickBehavior.NoToolSwitch));
                this.MoveToolResamplingAlgorithm = base.Register<EnumSetting<ResamplingAlgorithm>>(new EnumSetting<ResamplingAlgorithm>(base.GetSettingPath("MoveToolResamplingAlgorithm"), SettingScope.CurrentUser, ResamplingAlgorithm.Bilinear));
                this.PrimaryColor = base.Register<ColorBgra32Setting>(new ColorBgra32Setting(base.GetSettingPath("PrimaryColor"), SettingScope.CurrentUser, (ColorBgra32) ColorBgra.Black));
                this.Radius = base.Register<FloatSetting>(new FloatSetting(base.GetSettingPath("Radius"), SettingScope.CurrentUser, 10f, 0f, 2000f));
                this.RecolorToolSamplingMode = base.Register<EnumSetting<PaintDotNet.Tools.Recolor.RecolorToolSamplingMode>>(new EnumSetting<PaintDotNet.Tools.Recolor.RecolorToolSamplingMode>(base.GetSettingPath("RecolorToolSamplingMode"), SettingScope.CurrentUser, PaintDotNet.Tools.Recolor.RecolorToolSamplingMode.Once));
                this.SecondaryColor = base.Register<ColorBgra32Setting>(new ColorBgra32Setting(base.GetSettingPath("SecondaryColor"), SettingScope.CurrentUser, (ColorBgra32) ColorBgra.White));
                this.Tolerance = base.Register<FloatSetting>(new FloatSetting(base.GetSettingPath("Tolerance"), SettingScope.CurrentUser, 0.5f, 0f, 1f));
            }

            public sealed class BrushSection : SettingsSection
            {
                public readonly EnumSetting<System.Drawing.Drawing2D.HatchStyle> HatchStyle;
                public readonly EnumSetting<PaintDotNet.BrushType> Type;

                public BrushSection(SettingsSection parent) : base(parent, "Brush")
                {
                    this.Type = base.Register<EnumSetting<PaintDotNet.BrushType>>(new EnumSetting<PaintDotNet.BrushType>(base.GetSettingPath("Type"), SettingScope.CurrentUser, PaintDotNet.BrushType.Solid));
                    this.HatchStyle = base.Register<EnumSetting<System.Drawing.Drawing2D.HatchStyle>>(new EnumSetting<System.Drawing.Drawing2D.HatchStyle>(base.GetSettingPath("HatchStyle"), SettingScope.CurrentUser, System.Drawing.Drawing2D.HatchStyle.BackwardDiagonal));
                }
            }

            public sealed class GradientSection : SettingsSection
            {
                public readonly BooleanSetting IsAlphaOnly;
                public readonly EnumSetting<GradientRepeatType> RepeatType;
                public readonly EnumSetting<GradientType> Type;

                public GradientSection(SettingsSection parent) : base(parent, "Gradient")
                {
                    this.Type = base.Register<EnumSetting<GradientType>>(new EnumSetting<GradientType>(base.GetSettingPath("Type"), SettingScope.CurrentUser, GradientType.LinearClamped));
                    this.RepeatType = base.Register<EnumSetting<GradientRepeatType>>(new EnumSetting<GradientRepeatType>(base.GetSettingPath("RepeatType"), SettingScope.CurrentUser, GradientRepeatType.NoRepeat));
                    this.IsAlphaOnly = base.Register<BooleanSetting>(new BooleanSetting(base.GetSettingPath("IsAlphaOnly"), SettingScope.CurrentUser, false));
                }
            }

            public sealed class PenSection : SettingsSection
            {
                public readonly EnumSetting<System.Drawing.Drawing2D.DashStyle> DashStyle;
                public readonly EnumSetting<LineCap2> EndCap;
                public readonly FloatSetting Hardness;
                public readonly EnumSetting<LineCap2> StartCap;
                public readonly FloatSetting Width;

                public PenSection(SettingsSection parent) : base(parent, "Pen")
                {
                    this.DashStyle = base.Register<EnumSetting<System.Drawing.Drawing2D.DashStyle>>(new EnumSetting<System.Drawing.Drawing2D.DashStyle>(base.GetSettingPath("DashStyle"), SettingScope.CurrentUser, System.Drawing.Drawing2D.DashStyle.Solid));
                    this.EndCap = base.Register<EnumSetting<LineCap2>>(new EnumSetting<LineCap2>(base.GetSettingPath("EndCap"), SettingScope.CurrentUser, LineCap2.Flat));
                    this.StartCap = base.Register<EnumSetting<LineCap2>>(new EnumSetting<LineCap2>(base.GetSettingPath("StartCap"), SettingScope.CurrentUser, LineCap2.Flat));
                    this.Width = base.Register<FloatSetting>(new FloatSetting(base.GetSettingPath("Width"), SettingScope.CurrentUser, 2f, 1f, 2000f));
                    this.Hardness = base.Register<FloatSetting>(new FloatSetting(base.GetSettingPath("Hardness"), SettingScope.CurrentUser, 0.75f, 0f, 1f));
                }
            }

            public sealed class SelectionSection : SettingsSection
            {
                public readonly EnumSetting<SelectionCombineMode> CombineMode;
                public readonly DoubleSetting DrawHeight;
                public readonly EnumSetting<SelectionDrawMode> DrawMode;
                public readonly EnumSetting<MeasurementUnit> DrawUnits;
                public readonly DoubleSetting DrawWidth;
                public readonly EnumSetting<SelectionRenderingQuality> RenderingQuality;

                public SelectionSection(SettingsSection parent) : base(parent, "Selection")
                {
                    this.CombineMode = base.Register<EnumSetting<SelectionCombineMode>>(new EnumSetting<SelectionCombineMode>(base.GetSettingPath("CombineMode"), SettingScope.CurrentUser, SelectionCombineMode.Replace));
                    this.DrawMode = base.Register<EnumSetting<SelectionDrawMode>>(new EnumSetting<SelectionDrawMode>(base.GetSettingPath("DrawMode"), SettingScope.CurrentUser, SelectionDrawMode.Normal));
                    this.DrawWidth = base.Register<DoubleSetting>(new DoubleSetting(base.GetSettingPath("DrawWidth"), SettingScope.CurrentUser, 400.0, double.MinValue, double.MaxValue));
                    this.DrawHeight = base.Register<DoubleSetting>(new DoubleSetting(base.GetSettingPath("DrawHeight"), SettingScope.CurrentUser, 300.0, double.MinValue, double.MaxValue));
                    this.DrawUnits = base.Register<EnumSetting<MeasurementUnit>>(new EnumSetting<MeasurementUnit>(base.GetSettingPath("DrawUnits"), SettingScope.CurrentUser, MeasurementUnit.Pixel));
                    this.RenderingQuality = base.Register<EnumSetting<SelectionRenderingQuality>>(new EnumSetting<SelectionRenderingQuality>(base.GetSettingPath("RenderingQuality"), SettingScope.CurrentUser, SelectionRenderingQuality.HighQualityAntialiased));
                }
            }

            public sealed class ShapesSection : SettingsSection
            {
                public readonly EnumSetting<PaintDotNet.Shapes.Lines.CurveType> CurveType;
                public readonly EnumSetting<ShapeDrawType> DrawType;
                public readonly ShapeInfoSetting LineCurveShape;
                public readonly FloatSetting Radius;
                public readonly ShapeInfoSetting Shape;

                public ShapesSection(SettingsSection parent) : base(parent, "Shapes")
                {
                    this.CurveType = base.Register<EnumSetting<PaintDotNet.Shapes.Lines.CurveType>>(new EnumSetting<PaintDotNet.Shapes.Lines.CurveType>(base.GetSettingPath("CurveType"), SettingScope.CurrentUser, PaintDotNet.Shapes.Lines.CurveType.Spline));
                    this.DrawType = base.Register<EnumSetting<ShapeDrawType>>(new EnumSetting<ShapeDrawType>(base.GetSettingPath("DrawType"), SettingScope.CurrentUser, ShapeDrawType.Outline));
                    this.LineCurveShape = base.Register<ShapeInfoSetting>(new ShapeInfoSetting(base.GetSettingPath("LineCurveShape"), SettingScope.CurrentUser, PdnShapeBase.GetShapeInfo<PaintDotNet.Shapes.Lines.LineCurveShape>(), () => LineCurveTool.GetShapesCatalog()));
                    this.Shape = base.Register<ShapeInfoSetting>(new ShapeInfoSetting(base.GetSettingPath("Shape"), SettingScope.CurrentUser, PdnShapeBase.GetShapeInfo<RectangleShape>(), () => ShapesTool.GetShapesCatalog()));
                }

                [Serializable, CompilerGenerated]
                private sealed class <>c
                {
                    public static readonly AppSettings.ToolsSection.ShapesSection.<>c <>9 = new AppSettings.ToolsSection.ShapesSection.<>c();
                    public static Func<IEnumerable<ShapeInfo>> <>9__5_0;
                    public static Func<IEnumerable<ShapeInfo>> <>9__5_1;

                    internal IEnumerable<ShapeInfo> <.ctor>b__5_0() => 
                        LineCurveTool.GetShapesCatalog();

                    internal IEnumerable<ShapeInfo> <.ctor>b__5_1() => 
                        ShapesTool.GetShapesCatalog();
                }
            }

            public sealed class TextSection : SettingsSection
            {
                public readonly EnumSetting<PaintDotNet.TextAlignment> Alignment;
                public readonly StringSetting FontFamilyName;
                public readonly FloatSetting FontSize;
                public readonly EnumSetting<System.Drawing.FontStyle> FontStyle;
                public readonly EnumSetting<TextToolRenderingMode> RenderingMode;

                public TextSection(SettingsSection parent) : base(parent, "Text")
                {
                    this.Alignment = base.Register<EnumSetting<PaintDotNet.TextAlignment>>(new EnumSetting<PaintDotNet.TextAlignment>(base.GetSettingPath("Alignment"), SettingScope.CurrentUser, PaintDotNet.TextAlignment.Left));
                    this.FontFamilyName = base.Register<StringSetting>(new StringSetting(base.GetSettingPath("FontFamilyName"), SettingScope.CurrentUser, "Calibri", false));
                    this.FontSize = base.Register<FloatSetting>(new FloatSetting(base.GetSettingPath("FontSize"), SettingScope.CurrentUser, 12f, 1f, 2000f));
                    this.FontStyle = base.Register<EnumSetting<System.Drawing.FontStyle>>(new EnumSetting<System.Drawing.FontStyle>(base.GetSettingPath("FontStyle"), SettingScope.CurrentUser, System.Drawing.FontStyle.Regular));
                    this.RenderingMode = base.Register<EnumSetting<TextToolRenderingMode>>(new EnumSetting<TextToolRenderingMode>(base.GetSettingPath("RenderingMode"), SettingScope.CurrentUser, TextToolRenderingMode.Smooth));
                }
            }
        }

        public sealed class UISection : SettingsSection
        {
            public readonly EnumSetting<PaintDotNet.VisualStyling.AeroColorScheme> AeroColorScheme;
            public readonly EnumSetting<TextAntialiasMode> DefaultTextAntialiasMode;
            public readonly EnumSetting<TextRenderingMode> DefaultTextRenderingMode;
            public readonly BooleanSetting EnableAnimations;
            public readonly BooleanSetting EnableAntialiasedSelectionOutline;
            public readonly BooleanSetting EnableCanvasHwndRenderTarget;
            public readonly BooleanSetting EnableHardwareAcceleration;
            public readonly BooleanSetting EnableHighQualityScaling;
            public readonly BooleanSetting EnableOverscroll;
            public readonly BooleanSetting EnableSmoothMouseInput;
            public readonly EnumSetting<AppErrorFlags> ErrorFlags;
            public readonly EnumSetting<AppErrorFlags> ErrorFlagsAtStartup;
            public readonly BooleanSetting GlassButtonFooters;
            public readonly CultureInfoSetting Language;
            public readonly BooleanSetting ShowTaskbarPreviews;
            public readonly BooleanSetting TranslucentWindows;

            public UISection() : base("UI")
            {
                this.AeroColorScheme = base.Register<EnumSetting<PaintDotNet.VisualStyling.AeroColorScheme>>(new EnumSetting<PaintDotNet.VisualStyling.AeroColorScheme>(base.GetSettingPath("AeroColorScheme"), SettingScope.CurrentUser, AeroColors.DefaultScheme));
                this.DefaultTextAntialiasMode = base.Register<EnumSetting<TextAntialiasMode>>(new EnumSetting<TextAntialiasMode>(base.GetSettingPath("DefaultTextAntialiasMode"), SettingScope.CurrentUser, TextAntialiasMode.Default));
                this.DefaultTextRenderingMode = base.Register<EnumSetting<TextRenderingMode>>(new EnumSetting<TextRenderingMode>(base.GetSettingPath("DefaultTextRenderingMode"), SettingScope.CurrentUser, TextRenderingMode.Default));
                this.EnableAnimations = base.Register<BooleanSetting>(new BooleanSetting(base.GetSettingPath("EnableAnimations"), SettingScope.CurrentUser, true));
                this.EnableAntialiasedSelectionOutline = base.Register<BooleanSetting>(new BooleanSetting(base.GetSettingPath("EnableAntialiasedSelectionOutline"), SettingScope.CurrentUser, true));
                this.EnableCanvasHwndRenderTarget = base.Register<BooleanSetting>(new BooleanSetting(base.GetSettingPath("EnableCanvasHwndRenderTarget"), SettingScope.SystemWideWithCurrentUserOverride, true));
                bool enableHardwareAccelerationDefault = GetEnableHardwareAccelerationDefault();
                this.EnableHardwareAcceleration = base.Register<BooleanSetting>(new BooleanSetting(base.GetSettingPath("EnableHardwareAcceleration"), SettingScope.SystemWideWithCurrentUserOverride, enableHardwareAccelerationDefault));
                this.EnableHighQualityScaling = base.Register<BooleanSetting>(new BooleanSetting(base.GetSettingPath("EnableHighQualityScaling"), SettingScope.CurrentUser, true));
                this.EnableOverscroll = base.Register<BooleanSetting>(new BooleanSetting(base.GetSettingPath("EnableOverscroll"), SettingScope.CurrentUser, true));
                bool enableSmoothMouseInputDefault = GetEnableSmoothMouseInputDefault();
                this.EnableSmoothMouseInput = base.Register<BooleanSetting>(new BooleanSetting(base.GetSettingPath("EnableSmoothMouseInput"), SettingScope.SystemWideWithCurrentUserOverride, enableSmoothMouseInputDefault));
                this.ErrorFlags = base.Register<EnumSetting<AppErrorFlags>>(new EnumSetting<AppErrorFlags>(base.GetSettingPath("ErrorFlags"), SettingScope.CurrentUser, AppErrorFlags.None));
                this.ErrorFlagsAtStartup = base.Register<EnumSetting<AppErrorFlags>>(new EnumSetting<AppErrorFlags>(base.GetSettingPath("ErrorFlagsAtStartup"), SettingScope.CurrentUser, AppErrorFlags.None));
                this.GlassButtonFooters = base.Register<BooleanSetting>(new BooleanSetting(base.GetSettingPath("GlassButtonFooters"), SettingScope.CurrentUser, true));
                CultureInfo languageDefault = GetLanguageDefault();
                this.Language = base.Register<CultureInfoSetting>(new CultureInfoSetting(base.GetSettingPath("Language"), SettingScope.SystemWideWithCurrentUserOverride, languageDefault));
                this.ShowTaskbarPreviews = base.Register<BooleanSetting>(new BooleanSetting(base.GetSettingPath("ShowTaskbarPreviews"), SettingScope.CurrentUser, true));
                this.TranslucentWindows = base.Register<BooleanSetting>(new BooleanSetting(base.GetSettingPath("TranslucentWindows"), SettingScope.CurrentUser, true));
            }

            private static bool GetEnableHardwareAccelerationDefault()
            {
                try
                {
                    return GetEnableHardwareAccelerationDefaultImpl();
                }
                catch (Exception)
                {
                    return false;
                }
            }

            private static bool GetEnableHardwareAccelerationDefaultImpl()
            {
                using (IDxgiFactory1 factory = DxgiFactory1.CreateFactory1())
                {
                    IDxgiAdapter1[] adapters = factory.EnumerateAdapters1().ToArrayEx<IDxgiAdapter1>();
                    try
                    {
                        foreach (IDxgiAdapter1 adapter in adapters)
                        {
                            if (!GetHardwareAccelerationDefaultForIndividualDxgiAdapter(adapter))
                            {
                                return false;
                            }
                        }
                        if (!GetHardwareAccelerationDefaultForAllDxgiAdapters(adapters))
                        {
                            return false;
                        }
                    }
                    finally
                    {
                        DisposableUtil.FreeContents<IDxgiAdapter1>(adapters);
                    }
                }
                return true;
            }

            private static bool GetEnableSmoothMouseInputDefault()
            {
                try
                {
                    return GetEnableSmoothMouseInputDefaultImpl();
                }
                catch (Exception)
                {
                    return true;
                }
            }

            private static bool GetEnableSmoothMouseInputDefaultImpl()
            {
                if (!OS.IsWin7OrLater || OS.IsWin8OrLater)
                {
                    using (IDxgiFactory1 factory = DxgiFactory1.CreateFactory1())
                    {
                        IDxgiAdapter1[] disposeUs = factory.EnumerateAdapters1().ToArrayEx<IDxgiAdapter1>();
                        try
                        {
                            foreach (IDxgiAdapter1 adapter in disposeUs)
                            {
                                if (adapter.Description1.VendorID == 0x15ad)
                                {
                                    return false;
                                }
                            }
                        }
                        finally
                        {
                            DisposableUtil.FreeContents<IDxgiAdapter1>(disposeUs);
                        }
                    }
                }
                return true;
            }

            private static bool GetHardwareAccelerationDefault(uint vendorID, uint deviceID, uint revision, string descriptionText)
            {
                if (vendorID != 0x10de)
                {
                    if (vendorID == 0x15ad)
                    {
                        return GetHardwareAccelerationDefaultForVMWare(deviceID, revision, descriptionText);
                    }
                    if (vendorID == 0x8086)
                    {
                        return GetHardwareAccelerationDefaultForIntel(deviceID, revision, descriptionText);
                    }
                    return true;
                }
                return GetHardwareAccelerationDefaultForNVIDIA(deviceID, revision, descriptionText);
            }

            private static bool GetHardwareAccelerationDefaultForAllDxgiAdapters(IDxgiAdapter1[] adapters) => 
                true;

            private static bool GetHardwareAccelerationDefaultForIndividualDxgiAdapter(IDxgiAdapter1 adapter)
            {
                AdapterDescription1 description = adapter.Description1;
                AdapterFlags flags = description.Flags;
                uint vendorID = description.VendorID;
                uint deviceID = description.DeviceID;
                uint revision = description.Revision;
                string descriptionText = description.Description;
                if (((flags & AdapterFlags.Software) != AdapterFlags.Software) && !GetHardwareAccelerationDefault(vendorID, deviceID, revision, descriptionText))
                {
                    foreach (IDxgiOutput output in adapter.EnumerateOutputs())
                    {
                        if (output.Description.IsAttachedToDesktop)
                        {
                            return false;
                        }
                        output.Dispose();
                    }
                }
                return true;
            }

            private static bool GetHardwareAccelerationDefaultForIntel(uint deviceID, uint revision, string descriptionText)
            {
                if (deviceID <= 0x1132)
                {
                    if (deviceID <= 0xa0e)
                    {
                        if (deviceID <= 0x166)
                        {
                            if (deviceID <= 0x112)
                            {
                                if (deviceID <= 0x102)
                                {
                                    if ((deviceID == 0x42) || (deviceID == 70))
                                    {
                                        return false;
                                    }
                                    if (deviceID == 0x102)
                                    {
                                        goto Label_078D;
                                    }
                                }
                                else if (((deviceID == 0x106) || (deviceID == 0x10a)) || (deviceID == 0x112))
                                {
                                    goto Label_078D;
                                }
                            }
                            else if (deviceID <= 0x126)
                            {
                                if (((deviceID == 0x116) || (deviceID == 290)) || (deviceID == 0x126))
                                {
                                    goto Label_078D;
                                }
                            }
                            else
                            {
                                switch (deviceID)
                                {
                                    case 0x152:
                                    case 0x155:
                                    case 0x156:
                                    case 0x157:
                                    case 0x15a:
                                    case 0x162:
                                    case 0x166:
                                        goto Label_078F;
                                }
                            }
                        }
                        else if (deviceID <= 0x416)
                        {
                            if (deviceID <= 0x406)
                            {
                                if (((deviceID == 0x16a) || (deviceID == 0x402)) || (deviceID == 0x406))
                                {
                                    goto Label_078F;
                                }
                            }
                            else
                            {
                                switch (deviceID)
                                {
                                    case 0x40a:
                                    case 0x40b:
                                    case 0x40e:
                                    case 0x412:
                                    case 0x416:
                                        goto Label_078F;
                                }
                            }
                        }
                        else if (deviceID <= 0x426)
                        {
                            switch (deviceID)
                            {
                                case 0x41a:
                                case 0x41b:
                                case 0x41e:
                                case 0x422:
                                case 0x426:
                                    goto Label_078F;
                            }
                        }
                        else if (deviceID <= 0xa02)
                        {
                            switch (deviceID)
                            {
                                case 0x42a:
                                case 0x42b:
                                case 0x42e:
                                case 0xa02:
                                    goto Label_078F;
                            }
                        }
                        else
                        {
                            switch (deviceID)
                            {
                                case 0xa0a:
                                case 0xa0b:
                                case 0xa0e:
                                case 0xa06:
                                    goto Label_078F;
                            }
                        }
                    }
                    else if (deviceID <= 0xc22)
                    {
                        if (deviceID <= 0xa2e)
                        {
                            if (deviceID <= 0xa1e)
                            {
                                switch (deviceID)
                                {
                                    case 0xa1a:
                                    case 0xa1b:
                                    case 0xa1e:
                                    case 0xa16:
                                    case 0xa12:
                                        goto Label_078F;
                                }
                            }
                            else
                            {
                                switch (deviceID)
                                {
                                    case 0xa2a:
                                    case 0xa2b:
                                    case 0xa2e:
                                    case 0xa26:
                                    case 0xa22:
                                        goto Label_078F;
                                }
                            }
                        }
                        else if (deviceID <= 0xc0e)
                        {
                            switch (deviceID)
                            {
                                case 0xc0b:
                                case 0xc0c:
                                case 0xc0e:
                                case 0xc06:
                                case 0xc02:
                                    goto Label_078F;
                            }
                        }
                        else if (deviceID <= 0xc16)
                        {
                            if ((deviceID == 0xc12) || (deviceID == 0xc16))
                            {
                                goto Label_078F;
                            }
                        }
                        else
                        {
                            switch (deviceID)
                            {
                                case 0xc1b:
                                case 0xc1c:
                                case 0xc1e:
                                case 0xc22:
                                    goto Label_078F;
                            }
                        }
                    }
                    else if (deviceID <= 0xd12)
                    {
                        if (deviceID <= 0xd02)
                        {
                            switch (deviceID)
                            {
                                case 0xc2b:
                                case 0xc2c:
                                case 0xc2e:
                                case 0xd02:
                                case 0xc26:
                                    goto Label_078F;
                            }
                        }
                        else
                        {
                            switch (deviceID)
                            {
                                case 0xd0a:
                                case 0xd0b:
                                case 0xd0e:
                                case 0xd12:
                                case 0xd06:
                                    goto Label_078F;
                            }
                        }
                    }
                    else if (deviceID <= 0xd22)
                    {
                        switch (deviceID)
                        {
                            case 0xd1a:
                            case 0xd1b:
                            case 0xd1e:
                            case 0xd22:
                            case 0xd16:
                                goto Label_078F;
                        }
                    }
                    else if (deviceID <= 0xd2e)
                    {
                        switch (deviceID)
                        {
                            case 0xd2a:
                            case 0xd2b:
                            case 0xd2e:
                            case 0xd26:
                                goto Label_078F;
                        }
                    }
                    else
                    {
                        switch (deviceID)
                        {
                            case 0xf30:
                            case 0xf31:
                            case 0xf32:
                            case 0xf33:
                                goto Label_078F;

                            case 0x1132:
                                goto Label_0783;
                        }
                    }
                }
                else if (deviceID <= 0x29d2)
                {
                    if (deviceID <= 0x27ae)
                    {
                        if (deviceID <= 0x2592)
                        {
                            if (deviceID <= 0x2572)
                            {
                                if (deviceID == 0x1240)
                                {
                                    goto Label_0783;
                                }
                                if ((deviceID == 0x2562) || (deviceID == 0x2572))
                                {
                                    goto Label_0785;
                                }
                            }
                            else if (((deviceID == 0x2582) || (deviceID == 0x258a)) || (deviceID == 0x2592))
                            {
                                goto Label_0787;
                            }
                        }
                        else if (deviceID <= 0x2782)
                        {
                            if (((deviceID == 0x2772) || (deviceID == 0x2776)) || (deviceID == 0x2782))
                            {
                                goto Label_0787;
                            }
                        }
                        else if (deviceID <= 0x27a2)
                        {
                            if ((deviceID == 0x2792) || (deviceID == 0x27a2))
                            {
                                goto Label_0787;
                            }
                        }
                        else if ((deviceID == 0x27a6) || (deviceID == 0x27ae))
                        {
                            goto Label_0787;
                        }
                    }
                    else if (deviceID <= 0x2993)
                    {
                        if (deviceID <= 0x2982)
                        {
                            if (((deviceID == 0x2972) || (deviceID == 0x2973)) || (deviceID == 0x2982))
                            {
                                goto Label_0789;
                            }
                        }
                        else if (((deviceID == 0x2983) || (deviceID == 0x2992)) || (deviceID == 0x2993))
                        {
                            goto Label_0789;
                        }
                    }
                    else if (deviceID <= 0x29b2)
                    {
                        if ((deviceID == 0x29a2) || (deviceID == 0x29a3))
                        {
                            goto Label_0789;
                        }
                        if (deviceID == 0x29b2)
                        {
                            goto Label_0787;
                        }
                    }
                    else if (deviceID <= 0x29c2)
                    {
                        if ((deviceID == 0x29b3) || (deviceID == 0x29c2))
                        {
                            goto Label_0787;
                        }
                    }
                    else if ((deviceID == 0x29c3) || (deviceID == 0x29d2))
                    {
                        goto Label_0787;
                    }
                }
                else if (deviceID <= 0x2e33)
                {
                    if (deviceID <= 0x2a42)
                    {
                        if (deviceID <= 0x2a03)
                        {
                            if (deviceID == 0x29d3)
                            {
                                goto Label_0787;
                            }
                            if ((deviceID == 0x2a02) || (deviceID == 0x2a03))
                            {
                                goto Label_0789;
                            }
                        }
                        else if (((deviceID == 0x2a12) || (deviceID == 0x2a13)) || (deviceID == 0x2a42))
                        {
                            goto Label_0789;
                        }
                    }
                    else if (deviceID <= 0x2e13)
                    {
                        if (((deviceID == 0x2a43) || (deviceID == 0x2e12)) || (deviceID == 0x2e13))
                        {
                            goto Label_0789;
                        }
                    }
                    else if (deviceID <= 0x2e23)
                    {
                        if ((deviceID == 0x2e22) || (deviceID == 0x2e23))
                        {
                            goto Label_0789;
                        }
                    }
                    else if ((deviceID == 0x2e32) || (deviceID == 0x2e33))
                    {
                        goto Label_0789;
                    }
                }
                else if (deviceID <= 0x3582)
                {
                    if (deviceID <= 0x2e92)
                    {
                        if (((deviceID == 0x2e42) || (deviceID == 0x2e43)) || (deviceID == 0x2e92))
                        {
                            goto Label_0789;
                        }
                    }
                    else
                    {
                        if (deviceID == 0x2e93)
                        {
                            goto Label_0789;
                        }
                        if ((deviceID == 0x3577) || (deviceID == 0x3582))
                        {
                            goto Label_0785;
                        }
                    }
                }
                else if (deviceID <= 0x7800)
                {
                    switch (deviceID)
                    {
                        case 0x7121:
                        case 0x7123:
                        case 0x7125:
                        case 0x7800:
                            goto Label_0783;

                        case 0x358e:
                            goto Label_0785;
                    }
                }
                else if (deviceID <= 0xa002)
                {
                    if ((deviceID == 0xa001) || (deviceID == 0xa002))
                    {
                        goto Label_0787;
                    }
                }
                else if ((deviceID == 0xa011) || (deviceID == 0xa012))
                {
                    goto Label_0787;
                }
                return true;
            Label_0783:
                return false;
            Label_0785:
                return false;
            Label_0787:
                return false;
            Label_0789:
                return false;
            Label_078D:
                return false;
            Label_078F:
                return true;
            }

            private static bool GetHardwareAccelerationDefaultForNVIDIA(uint deviceID, uint revision, string descriptionText)
            {
                if (descriptionText.StartsWith("NVIDIA ION"))
                {
                    return false;
                }
                return true;
            }

            private static bool GetHardwareAccelerationDefaultForVMWare(uint deviceID, uint revision, string descriptionText) => 
                false;

            public static CultureInfo[] GetInstalledLanguages() => 
                PdnResources.GetInstalledLocales().Select<string, CultureInfo>(l => new CultureInfo(l)).ToArrayEx<CultureInfo>();

            private static CultureInfo GetLanguageDefault()
            {
                try
                {
                    CultureInfo[] installedLanguages = GetInstalledLanguages();
                    for (CultureInfo info2 = CultureInfo.InstalledUICulture; info2.Name != string.Empty; info2 = info2.Parent)
                    {
                        if (installedLanguages.IndexOf<CultureInfo>(info2) != -1)
                        {
                            return info2;
                        }
                    }
                }
                catch (Exception)
                {
                }
                return new CultureInfo("en-US");
            }

            [Serializable, CompilerGenerated]
            private sealed class <>c
            {
                public static readonly AppSettings.UISection.<>c <>9 = new AppSettings.UISection.<>c();
                public static Func<string, CultureInfo> <>9__16_0;

                internal CultureInfo <GetInstalledLanguages>b__16_0(string l) => 
                    new CultureInfo(l);
            }
        }

        public sealed class UpdatesSection : SettingsSection
        {
            public readonly IntegerBooleanSetting AutoCheck;
            public readonly IntegerBooleanSetting AutoCheckForPrerelease;
            public readonly DateTimeSetting LastCheckTimeUtc;
            public readonly StringSetting PackageFileName;

            public UpdatesSection() : base("Updates")
            {
                this.AutoCheck = base.Register<IntegerBooleanSetting>(new IntegerBooleanSetting("CHECKFORUPDATES", SettingScope.SystemWideWithCurrentUserOverride, true));
                this.AutoCheckForPrerelease = base.Register<IntegerBooleanSetting>(new IntegerBooleanSetting("CHECKFORBETAS", SettingScope.SystemWideWithCurrentUserOverride, PdnInfo.IsPrereleaseBuild));
                this.LastCheckTimeUtc = base.Register<DateTimeSetting>(new DateTimeSetting(base.GetSettingPath("LastCheckTimeUtc"), SettingScope.CurrentUser, DateTime.MinValue));
                this.PackageFileName = base.Register<StringSetting>(new StringSetting(base.GetSettingPath("PackageFileName"), SettingScope.CurrentUser, string.Empty, true));
            }

            public void PingLastAutoCheckTime()
            {
                this.LastCheckTimeUtc.Value = DateTime.UtcNow;
            }
        }

        public sealed class WindowSection : SettingsSection
        {
            public readonly ToolWindowSection Colors;
            public readonly ToolWindowSection History;
            public readonly ToolWindowSection Layers;
            public readonly DialogSection Main;
            public readonly DialogSection SaveConfiguration;
            public readonly ToolWindowSection Tools;

            public WindowSection() : base("Window")
            {
                this.Main = new DialogSection(this, "Main");
                this.SaveConfiguration = new DialogSection(this, "SaveConfiguration");
                this.Tools = new ToolWindowSection(this, "Tools", true);
                this.History = new ToolWindowSection(this, "History", true);
                this.Layers = new ToolWindowSection(this, "Layers", true);
                this.Colors = new ToolWindowSection(this, "Colors", true);
            }

            public sealed class DialogSection : SettingsSection
            {
                public readonly RectInt32Setting Bounds;
                public readonly EnumSetting<System.Windows.Forms.FormWindowState> FormWindowState;

                public DialogSection(SettingsSection parent, string dialogName) : base(parent, dialogName)
                {
                    this.FormWindowState = base.Register<EnumSetting<System.Windows.Forms.FormWindowState>>(new EnumSetting<System.Windows.Forms.FormWindowState>(base.GetSettingPath("FormWindowState"), SettingScope.CurrentUser, System.Windows.Forms.FormWindowState.Normal));
                    this.Bounds = base.Register<RectInt32Setting>(new RectInt32Setting(base.GetSettingPath("Bounds"), SettingScope.CurrentUser, new RectInt32(0, 0, 0, 0), MaxBounds));
                }

                public bool IsBoundsSpecified
                {
                    get
                    {
                        RectInt32 num = this.Bounds.Value;
                        return ((!num.HasZeroArea && (num.Width >= 0)) && (num.Height >= 0));
                    }
                }

                public static RectInt32 MaxBounds =>
                    new RectInt32(-65536, -65536, 0x20000, 0x20000);
            }

            public sealed class ToolWindowSection : SettingsSection, ISnapObstaclePersist
            {
                public readonly RectInt32Setting Bounds;
                public readonly EnumSetting<HorizontalSnapEdge> HorizontalEdge;
                public readonly BooleanSetting IsSnapped;
                public readonly BooleanSetting IsVisible;
                public readonly PointInt32Setting Offset;
                public readonly StringSetting SnappedToName;
                public readonly EnumSetting<VerticalSnapEdge> VerticalEdge;

                public ToolWindowSection(SettingsSection parent, string toolWindowName, bool defaultIsVisible = true) : base(parent, toolWindowName)
                {
                    this.IsVisible = base.Register<BooleanSetting>(new BooleanSetting(base.GetSettingPath("IsVisible"), SettingScope.CurrentUser, defaultIsVisible));
                    this.IsSnapped = base.Register<BooleanSetting>(new BooleanSetting(base.GetSettingPath("IsSnapped"), SettingScope.CurrentUser, false));
                    this.Bounds = base.Register<RectInt32Setting>(new RectInt32Setting(base.GetSettingPath("Bounds"), SettingScope.CurrentUser, RectInt32.Zero, MaxBounds));
                    this.SnappedToName = base.Register<StringSetting>(new StringSetting(base.GetSettingPath("SnappedToName"), SettingScope.CurrentUser, string.Empty, true));
                    this.HorizontalEdge = base.Register<EnumSetting<HorizontalSnapEdge>>(new EnumSetting<HorizontalSnapEdge>(base.GetSettingPath("HorizontalEdge"), SettingScope.CurrentUser, HorizontalSnapEdge.Neither));
                    this.VerticalEdge = base.Register<EnumSetting<VerticalSnapEdge>>(new EnumSetting<VerticalSnapEdge>(base.GetSettingPath("VerticalEdge"), SettingScope.CurrentUser, VerticalSnapEdge.Neither));
                    this.Offset = base.Register<PointInt32Setting>(new PointInt32Setting(base.GetSettingPath("Offset"), SettingScope.CurrentUser, PointInt32.Zero, MaxBounds));
                }

                public static RectInt32 MaxBounds =>
                    new RectInt32(-65536, -65536, 0x20000, 0x20000);

                RectInt32 ISnapObstaclePersist.Bounds
                {
                    get => 
                        this.Bounds.Value;
                    set
                    {
                        this.Bounds.Value = value;
                    }
                }

                HorizontalSnapEdge ISnapObstaclePersist.HorizontalEdge
                {
                    get => 
                        this.HorizontalEdge.Value;
                    set
                    {
                        this.HorizontalEdge.Value = value;
                    }
                }

                bool ISnapObstaclePersist.IsDataAvailable
                {
                    get
                    {
                        RectInt32 num = this.Bounds.Value;
                        return ((num.HasPositiveArea && (num.Width >= 0)) && (num.Height >= 0));
                    }
                }

                bool ISnapObstaclePersist.IsSnapped
                {
                    get => 
                        this.IsSnapped.Value;
                    set
                    {
                        this.IsSnapped.Value = value;
                    }
                }

                PointInt32 ISnapObstaclePersist.Offset
                {
                    get => 
                        this.Offset.Value;
                    set
                    {
                        this.Offset.Value = value;
                    }
                }

                string ISnapObstaclePersist.SnappedToName
                {
                    get => 
                        this.SnappedToName.Value;
                    set
                    {
                        this.SnappedToName.Value = value;
                    }
                }

                VerticalSnapEdge ISnapObstaclePersist.VerticalEdge
                {
                    get => 
                        this.VerticalEdge.Value;
                    set
                    {
                        this.VerticalEdge.Value = value;
                    }
                }
            }
        }

        public sealed class WorkspaceSection : SettingsSection
        {
            public readonly Int32Setting AutoScrollViewportPxPerSecond;
            public readonly StringSetting CurrentPalette;
            public readonly EnumSetting<AnchorEdge> LastCanvasSizeAnchorEdge;
            public readonly BooleanSetting LastMaintainAspectRatio;
            public readonly BooleanSetting LastMaintainAspectRatioCS;
            public readonly BooleanSetting LastMaintainAspectRatioNF;
            public readonly EnumSetting<PaintDotNet.MeasurementUnit> LastNonPixelUnits;
            public readonly EnumSetting<ResamplingAlgorithm> LastResamplingMethod;
            public readonly EnumSetting<PaintDotNet.MeasurementUnit> MeasurementUnit;
            public readonly BooleanSetting ShowPixelGrid;
            public readonly BooleanSetting ShowRulers;

            public WorkspaceSection() : base("Workspace")
            {
                this.AutoScrollViewportPxPerSecond = base.Register<Int32Setting>(new Int32Setting(base.GetSettingPath("AutoScrollViewportPxPerSecond"), SettingScope.CurrentUser, 0x7d0, 20, 0x30d40));
                this.CurrentPalette = base.Register<StringSetting>(new StringSetting(base.GetSettingPath("CurrentPalette"), SettingScope.CurrentUser, string.Empty, true));
                this.LastCanvasSizeAnchorEdge = base.Register<EnumSetting<AnchorEdge>>(new EnumSetting<AnchorEdge>(base.GetSettingPath("LastCanvasSizeAnchorEdge"), SettingScope.CurrentUser, AnchorEdge.TopLeft));
                this.LastMaintainAspectRatio = base.Register<BooleanSetting>(new BooleanSetting(base.GetSettingPath("LastMaintainAspectRatio"), SettingScope.CurrentUser, true));
                this.LastMaintainAspectRatioCS = base.Register<BooleanSetting>(new BooleanSetting(base.GetSettingPath("LastMaintainAspectRatioCS"), SettingScope.CurrentUser, false));
                this.LastMaintainAspectRatioNF = base.Register<BooleanSetting>(new BooleanSetting(base.GetSettingPath("LastMaintainAspectRatioNF"), SettingScope.CurrentUser, false));
                this.LastResamplingMethod = base.Register<EnumSetting<ResamplingAlgorithm>>(new EnumSetting<ResamplingAlgorithm>(base.GetSettingPath("LastResamplingMethod"), SettingScope.CurrentUser, ResamplingAlgorithm.Fant));
                this.LastNonPixelUnits = base.Register<EnumSetting<PaintDotNet.MeasurementUnit>>(new EnumSetting<PaintDotNet.MeasurementUnit>(base.GetSettingPath("LastNonPixelUnits"), SettingScope.CurrentUser, PaintDotNet.MeasurementUnit.Inch));
                this.MeasurementUnit = base.Register<EnumSetting<PaintDotNet.MeasurementUnit>>(new EnumSetting<PaintDotNet.MeasurementUnit>(base.GetSettingPath("MeasurementUnit"), SettingScope.CurrentUser, PaintDotNet.MeasurementUnit.Pixel));
                this.ShowPixelGrid = base.Register<BooleanSetting>(new BooleanSetting(base.GetSettingPath("ShowPixelGrid"), SettingScope.CurrentUser, false));
                this.ShowRulers = base.Register<BooleanSetting>(new BooleanSetting(base.GetSettingPath("ShowRulers"), SettingScope.CurrentUser, false));
            }
        }
    }
}

