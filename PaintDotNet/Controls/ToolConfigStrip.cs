namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Controls.ToolConfigUI;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.DirectWrite;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.Settings;
    using PaintDotNet.Settings.App;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;

    internal class ToolConfigStrip : ToolStripEx
    {
        private BooleanSplitButton antiAliasingSplitButton;
        private EnumSplitButton<ContentBlendMode> blendModeSplitButton;
        private PdnToolStripSeparator brushSeparator;
        private BrushStyleComboBox brushStyleComboBox;
        private ToolStripLabel brushStyleLabel;
        private ToolStripLabel colorPickerBehaviorLabel;
        private EnumSplitButton<ColorPickerClickBehavior> colorPickerClickBehaviorSplitButton;
        private ToolStripLabel colorPickerSampleLabel;
        private EnumSplitButton<PixelSampleMode> colorPickerSampleSizeSplitButton;
        private BooleanSplitButton colorPickerSampleTypeSplitButton;
        private PdnToolStripSeparator colorPickerSeparator;
        private PdnToolStripSeparator colorPickerSeparator2;
        private PdnToolStripSeparator colorPickerSeparator3;
        private static readonly int[] comboBoxValues = new int[] { 
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 20,
            0x19, 30, 0x23, 40, 0x2d, 50, 0x37, 60, 0x41, 70, 0x4b, 80, 0x55, 90, 0x5f, 100,
            0x7d, 150, 0xaf, 200, 0xe1, 250, 0x113, 300, 0x145, 350, 0x177, 400, 0x1a9, 450, 0x1db, 500,
            550, 600, 650, 700, 750, 800, 850, 900, 950, 0x3e8, 0x44c, 0x4b0, 0x514, 0x578, 0x5dc, 0x640,
            0x6a4, 0x708, 0x76c, 0x7d0
        };
        private ToolStripButton commitButton;
        private PdnToolStripSeparator commitButtonSeparator;
        private int[] defaultFontSizes = new int[] { 
            8, 9, 10, 11, 12, 14, 0x10, 0x12, 20, 0x16, 0x18, 0x1a, 0x1c, 0x24, 0x30, 0x48,
            0x54, 0x60, 0x6c, 0x90, 0xc0, 0xd8, 0x120
        };
        private ToolStripLabel floodModeLabel;
        private PdnToolStripSeparator floodModeSeparator;
        private EnumSplitButton<FloodMode> floodModeSplitButton;
        private EnumRadioButtonGroup<PaintDotNet.TextAlignment> fontAlignRadioButtonGroup;
        private PdnToolStripSeparator fontAlignSeparator;
        private PdnToolStripComboBox fontFamilyComboBox;
        private const int fontFamilyComboBoxFontSize = 12;
        private FontListComboBoxHandler fontFamilyComboBoxHandler;
        private ToolStripLabel fontLabel;
        private IGdiFontMap fontMap;
        private EnumSplitButton<TextToolRenderingMode> fontRenderingModeSplitButton;
        private PdnToolStripSeparator fontSeparator;
        private PdnToolStripComboBox fontSizeComboBox;
        private EnumFlagsButtonGroup<System.Drawing.FontStyle> fontStyleButtonGroup;
        private PdnToolStripSeparator fontStyleSeparator;
        private BooleanSplitButton gradientChannelsSplitButton;
        private EnumSplitButton<GradientRepeatType> gradientRepeatTypeSplitButton;
        private PdnToolStripSeparator gradientSeparator1;
        private PdnToolStripSeparator gradientSeparator2;
        private EnumRadioButtonGroup<GradientType> gradientTypeRadioButtonGroup;
        private const int initialFontSize = 12;
        private EnumRadioButtonGroup<CurveType> lineCurveShapeTypeRadioButtonGroup;
        private const int maxFontSize = 0x7d0;
        private const int minFontSize = 1;
        private const NumberStyles parseRealNumberStyles = NumberStyles.Number;
        private EnumSplitButton<DashStyle> penDashStyleSplitButton;
        private EnumSplitButton<LineCap2> penEndCapSplitButton;
        private ToolStripLabel penHardnessLabel;
        private SliderControl penHardnessSlider;
        private PdnToolStripSeparator penSeparator;
        private PdnToolStripComboBox penSizeComboBox;
        private ToolStripButton penSizeDecButton;
        private ToolStripButton penSizeIncButton;
        private ToolStripLabel penSizeLabel;
        private EnumSplitButton<LineCap2> penStartCapSplitButton;
        private ToolStripLabel penStyleLabel;
        private PdnToolStripComboBox radiusComboBox;
        private ToolStripButton radiusDecButton;
        private ToolStripButton radiusIncButton;
        private ToolStripLabel radiusLabel;
        private PdnToolStripSeparator rasterizationSeparator;
        private EnumRadioButtonGroup<RecolorToolSamplingMode> recolorToolSamplingModeRBGroup;
        private PdnToolStripSeparator recolorToolSeparator;
        private EnumLocalizer resamplingAlgorithmNames = EnumLocalizer.Create(typeof(ResamplingAlgorithm));
        private ToolStripLabel resamplingLabel;
        private PdnToolStripSeparator resamplingSeparator;
        private EnumSplitButton<ResamplingAlgorithm> resamplingSplitButton;
        private EnumRadioButtonGroup<SelectionCombineMode> selectionCombineModeRadioButtonGroup;
        private PdnToolStripSeparator selectionCombineModeSeparator;
        private ToolStripLabel selectionDrawModeHeightLabel;
        private PdnToolStripTextBox selectionDrawModeHeightTextBox;
        private ToolStripLabel selectionDrawModeModeLabel;
        private PdnToolStripSeparator selectionDrawModeSeparator;
        private EnumSplitButton<SelectionDrawMode> selectionDrawModeSplitButton;
        private ToolStripButton selectionDrawModeSwapButton;
        private UnitsComboBoxStrip selectionDrawModeUnits;
        private ToolStripLabel selectionDrawModeWidthLabel;
        private PdnToolStripTextBox selectionDrawModeWidthTextBox;
        private EnumSplitButton<SelectionRenderingQuality> selectionRenderingQualitySplitButton;
        private EnumSplitButton<ShapeDrawType> shapeDrawTypeButton;
        private PdnToolStripSeparator shapeSeparator;
        private ShapeTypeDropDownButton shapeTypeDropDownButton;
        private bool showFirstAndLastSeparators = true;
        private bool suspendWriteBackToPenSizeComboBox;
        private bool suspendWriteBackToRadiusComboBox;
        private ToolStripLabel toleranceLabel;
        private PdnToolStripSeparator toleranceSeparator;
        private SliderControl toleranceSliderStrip;
        private PaintDotNet.ToolBarConfigItems toolBarConfigItems;
        private AppSettings.ToolsSection toolSettings;

        [field: CompilerGenerated]
        public event EventHandler CommitButtonClicked;

        [field: CompilerGenerated]
        public event EventHandler ToolBarConfigItemsChanged;

        public ToolConfigStrip(AppSettings.ToolsSection toolSettings)
        {
            Validate.IsNotNull<AppSettings.ToolsSection>(toolSettings, "toolSettings");
            this.toolSettings = toolSettings;
            this.InitializeComponent();
            this.BindPenSettings();
            this.BindSelectionDrawModeSettings();
            this.BindTextSettings();
            this.ToolBarConfigItems = PaintDotNet.ToolBarConfigItems.None;
        }

        private void AddPenSectionToToolStrip()
        {
            ToolStripItem[] toolStripItems = new ToolStripItem[] { this.penSeparator, this.penSizeLabel, this.penSizeDecButton, this.penSizeComboBox, this.penSizeIncButton, this.radiusLabel, this.radiusDecButton, this.radiusComboBox, this.radiusIncButton, this.penHardnessLabel, this.penHardnessSlider, this.penStyleLabel, this.penStartCapSplitButton, this.penDashStyleSplitButton, this.penEndCapSplitButton };
            this.Items.AddRange(toolStripItems);
        }

        private void AddResamplingSectionToToolStrip()
        {
            ToolStripItem[] toolStripItems = new ToolStripItem[] { this.resamplingSeparator, this.resamplingLabel, this.resamplingSplitButton };
            this.Items.AddRange(toolStripItems);
        }

        private void AddSelectionDrawModeSectionToToolStrip()
        {
            ToolStripItem[] toolStripItems = new ToolStripItem[] { this.selectionDrawModeSeparator, this.selectionDrawModeModeLabel, this.selectionDrawModeSplitButton, this.selectionDrawModeWidthLabel, this.selectionDrawModeWidthTextBox, this.selectionDrawModeSwapButton, this.selectionDrawModeHeightLabel, this.selectionDrawModeHeightTextBox, this.selectionDrawModeUnits };
            this.Items.AddRange(toolStripItems);
        }

        private void AddTextSectionToToolStrip()
        {
            ToolStripItem[] toolStripItems = new ToolStripItem[] { this.fontSeparator, this.fontLabel, this.fontFamilyComboBox, this.fontSizeComboBox, this.fontStyleSeparator };
            this.Items.AddRange(toolStripItems);
            this.Items.AddRange(this.fontStyleButtonGroup.Items);
            this.Items.Add(this.fontRenderingModeSplitButton);
            this.Items.Add(this.fontAlignSeparator);
            this.Items.AddRange(this.fontAlignRadioButtonGroup.Items);
        }

        public void AddToPenSize(float delta)
        {
            if ((this.toolBarConfigItems & (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.PenWidth)) == (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.PenWidth))
            {
                float num2 = (this.toolSettings.Pen.Width.Value + delta).Clamp(this.toolSettings.Pen.Width.MinValue, this.toolSettings.Pen.Width.MaxValue);
                this.toolSettings.Pen.Width.Value = num2;
            }
        }

        private void AddToRadius(float delta)
        {
            if ((this.toolBarConfigItems & (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.Radius)) == (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.Radius))
            {
                float num2 = (this.toolSettings.Radius.Value + delta).Clamp(this.toolSettings.Radius.MinValue, this.toolSettings.Radius.MaxValue);
                this.toolSettings.Radius.Value = num2;
            }
        }

        private void BindPenSettings()
        {
            this.toolSettings.Pen.Width.ValueChangedT += new ValueChangedEventHandler<float>(this.OnAppSettingsPenWidthChanged);
            this.toolSettings.Radius.ValueChangedT += new ValueChangedEventHandler<float>(this.OnAppSettingsRadiusChanged);
        }

        private void BindSelectionDrawModeSettings()
        {
            this.toolSettings.Selection.DrawMode.ValueChangedT += new ValueChangedEventHandler<SelectionDrawMode>(this.OnAppSettingsSelectionDrawModeChanged);
            this.toolSettings.Selection.DrawHeight.ValueChangedT += new ValueChangedEventHandler<double>(this.OnAppSettingsSelectionDrawHeightChanged);
            this.toolSettings.Selection.DrawUnits.ValueChangedT += new ValueChangedEventHandler<MeasurementUnit>(this.OnAppSettingsSelectionDrawUnitsChanged);
            this.toolSettings.Selection.DrawWidth.ValueChangedT += new ValueChangedEventHandler<double>(this.OnAppSettingsSelectionDrawWidthChanged);
        }

        private void BindTextSettings()
        {
            this.toolSettings.Text.FontFamilyName.ValueChangedT += new ValueChangedEventHandler<string>(this.OnAppSettingsTextFontFamilyNameChanged);
            this.toolSettings.Text.FontSize.ValueChangedT += new ValueChangedEventHandler<float>(this.OnAppSettingsTextFontSizeChanged);
        }

        public void CyclePenDashStyle()
        {
            this.penDashStyleSplitButton.PerformButtonClick();
        }

        public void CyclePenEndCap()
        {
            this.penEndCapSplitButton.PerformButtonClick();
        }

        public void CyclePenStartCap()
        {
            this.penStartCapSplitButton.PerformButtonClick();
        }

        protected override void Dispose(bool disposing)
        {
            DisposableUtil.Free<EnumRadioButtonGroup<GradientType>>(ref this.gradientTypeRadioButtonGroup, disposing);
            DisposableUtil.Free<EnumRadioButtonGroup<RecolorToolSamplingMode>>(ref this.recolorToolSamplingModeRBGroup, disposing);
            DisposableUtil.Free<EnumRadioButtonGroup<CurveType>>(ref this.lineCurveShapeTypeRadioButtonGroup);
            if (disposing && (this.toolSettings != null))
            {
                this.UnbindPenSettings();
                this.UnbindSelectionDrawModeSettings();
                this.UnbindTextSettings();
                this.toolSettings = null;
            }
            this.DisposeTextSection(disposing);
            base.Dispose(disposing);
        }

        private void DisposeTextSection(bool disposing)
        {
            if (disposing)
            {
                this.fontFamilyComboBox.AvailableChanged -= new EventHandler(this.OnFontFamilyComboBoxAvailableChanged);
            }
            DisposableUtil.Free<FontListComboBoxHandler>(ref this.fontFamilyComboBoxHandler, disposing);
            DisposableUtil.Free<EnumRadioButtonGroup<PaintDotNet.TextAlignment>>(ref this.fontAlignRadioButtonGroup, disposing);
            DisposableUtil.Free<EnumFlagsButtonGroup<System.Drawing.FontStyle>>(ref this.fontStyleButtonGroup, disposing);
            DisposableUtil.Free<IGdiFontMap>(ref this.fontMap, disposing);
        }

        private void InitializeComponent()
        {
            this.selectionCombineModeSeparator = new PdnToolStripSeparator();
            this.selectionCombineModeRadioButtonGroup = new EnumRadioButtonGroup<SelectionCombineMode>(this.toolSettings.Selection.CombineMode);
            this.InitializeSelectionDrawModeSection();
            this.floodModeSeparator = new PdnToolStripSeparator();
            this.floodModeLabel = new ToolStripLabel();
            this.floodModeSplitButton = new EnumSplitButton<FloodMode>(this.toolSettings.FloodMode);
            this.InitializeResamplingSection();
            this.InitializeTextSection();
            this.shapeSeparator = new PdnToolStripSeparator();
            this.shapeTypeDropDownButton = new ShapeTypeDropDownButton(this.toolSettings.Shapes.Shape);
            this.shapeTypeDropDownButton.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            this.shapeDrawTypeButton = new EnumSplitButton<ShapeDrawType>(this.toolSettings.Shapes.DrawType);
            this.shapeDrawTypeButton.ToolTipText = PdnResources.GetString("ToolConfigStrip.ShapeDrawTypeButton.ToolTipText");
            this.lineCurveShapeTypeRadioButtonGroup = new EnumRadioButtonGroup<CurveType>(this.toolSettings.Shapes.CurveType);
            this.gradientSeparator1 = new PdnToolStripSeparator();
            this.gradientTypeRadioButtonGroup = new EnumRadioButtonGroup<GradientType>(this.toolSettings.Gradient.Type);
            this.gradientSeparator2 = new PdnToolStripSeparator();
            this.gradientChannelsSplitButton = new BooleanSplitButton(this.toolSettings.Gradient.IsAlphaOnly, "GradientIsAlphaOnly");
            this.gradientRepeatTypeSplitButton = new EnumSplitButton<GradientRepeatType>(this.toolSettings.Gradient.RepeatType);
            this.gradientRepeatTypeSplitButton.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            this.InitializePenSection();
            this.brushSeparator = new PdnToolStripSeparator();
            this.brushStyleLabel = new ToolStripLabel();
            this.brushStyleComboBox = new BrushStyleComboBox(this.toolSettings.Brush.Type, this.toolSettings.Brush.HatchStyle);
            this.toleranceSeparator = new PdnToolStripSeparator();
            this.toleranceLabel = new ToolStripLabel();
            this.toleranceSliderStrip = new SliderControl(this.toolSettings.Tolerance);
            this.recolorToolSeparator = new PdnToolStripSeparator();
            this.recolorToolSamplingModeRBGroup = new EnumRadioButtonGroup<RecolorToolSamplingMode>(this.toolSettings.RecolorToolSamplingMode);
            this.rasterizationSeparator = new PdnToolStripSeparator();
            this.antiAliasingSplitButton = new BooleanSplitButton(this.toolSettings.Antialiasing, "AntiAliasing");
            this.blendModeSplitButton = new EnumSplitButton<ContentBlendMode>(this.toolSettings.BlendMode, "BlendMode");
            this.blendModeSplitButton.ToolTipText = PdnResources.GetString("ToolConfigStrip.BlendModeSplitButton.ToolTipText");
            this.blendModeSplitButton.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            this.blendModeSplitButton.ToolBarImageSelector = delegate (ContentBlendMode value) {
                if (value.IsCompositionOp())
                {
                    return PdnResources.GetImageResource("Icons.AlphaBlending.True.png");
                }
                return PdnResources.GetImageResource("Icons.AlphaBlending.False.png");
            };
            this.selectionRenderingQualitySplitButton = new EnumSplitButton<SelectionRenderingQuality>(this.toolSettings.Selection.RenderingQuality);
            this.selectionRenderingQualitySplitButton.ToolTipText = PdnResources.GetString("ToolConfigStrip.SelectionRenderingQualitySplitButton.ToolTipText");
            this.colorPickerSeparator = new PdnToolStripSeparator();
            this.colorPickerSampleLabel = new ToolStripLabel();
            this.colorPickerSampleTypeSplitButton = new BooleanSplitButton(this.toolSettings.SampleAllLayers, "SampleAllLayers");
            this.colorPickerSeparator2 = new PdnToolStripSeparator();
            this.colorPickerSampleSizeSplitButton = new EnumSplitButton<PixelSampleMode>(this.toolSettings.PixelSampleMode);
            this.colorPickerSeparator3 = new PdnToolStripSeparator();
            this.colorPickerBehaviorLabel = new ToolStripLabel();
            this.colorPickerClickBehaviorSplitButton = new EnumSplitButton<ColorPickerClickBehavior>(this.toolSettings.ColorPickerClickBehavior);
            this.commitButtonSeparator = new PdnToolStripSeparator();
            this.commitButton = new ToolStripButton();
            this.commitButton.Text = PdnResources.GetString("ToolConfigStrip.CommitButton.Text");
            this.commitButton.ToolTipText = PdnResources.GetString("ToolConfigStrip.CommitButton.ToolTipText");
            this.commitButton.Image = UIUtil.GetScaledImage(PdnResources.GetImageResource("Icons.ToolConfigStrip.CommitButton.png").Reference);
            this.commitButton.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            this.commitButton.Click += new EventHandler(this.OnCommitButtonClicked);
            this.commitButton.Enabled = false;
            this.commitButton.Visible = false;
            base.SuspendLayout();
            this.floodModeLabel.Text = PdnResources.GetString("ToolConfigStrip.FloodModeLabel.Text");
            this.brushStyleLabel.Text = PdnResources.GetString("BrushConfigWidget.FillStyleLabel.Text");
            this.toleranceLabel.Text = PdnResources.GetString("ToleranceConfig.ToleranceLabel.Text");
            this.toleranceSliderStrip.Name = "toleranceSliderStrip";
            this.colorPickerSampleLabel.Text = PdnResources.GetString("ToolConfigStrip.ColorPickerSizeLabel.Text");
            this.colorPickerSampleTypeSplitButton.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            this.colorPickerSampleSizeSplitButton.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            this.colorPickerBehaviorLabel.Text = PdnResources.GetString("ToolConfigStrip.ColorPickerBehaviorLabel.Text");
            this.colorPickerClickBehaviorSplitButton.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            this.AutoSize = true;
            this.Items.Add(this.selectionCombineModeSeparator);
            this.Items.AddRange(this.selectionCombineModeRadioButtonGroup.Items);
            this.AddSelectionDrawModeSectionToToolStrip();
            ToolStripItem[] toolStripItems = new ToolStripItem[] { this.floodModeSeparator, this.floodModeLabel, this.floodModeSplitButton };
            this.Items.AddRange(toolStripItems);
            this.AddResamplingSectionToToolStrip();
            this.AddTextSectionToToolStrip();
            ToolStripItem[] itemArray2 = new ToolStripItem[] { this.shapeSeparator, this.shapeTypeDropDownButton, this.shapeDrawTypeButton };
            this.Items.AddRange(itemArray2);
            this.Items.AddRange(this.lineCurveShapeTypeRadioButtonGroup.Items);
            this.Items.Add(this.gradientSeparator1);
            this.Items.AddRange(this.gradientTypeRadioButtonGroup.Items);
            this.Items.Add(this.gradientSeparator2);
            this.Items.Add(this.gradientChannelsSplitButton);
            this.Items.Add(this.gradientRepeatTypeSplitButton);
            this.AddPenSectionToToolStrip();
            ToolStripItem[] itemArray3 = new ToolStripItem[] { this.brushSeparator, this.brushStyleLabel, this.brushStyleComboBox, this.toleranceSeparator, this.toleranceLabel, this.toleranceSliderStrip, this.recolorToolSeparator };
            this.Items.AddRange(itemArray3);
            this.Items.AddRange(this.recolorToolSamplingModeRBGroup.Items);
            ToolStripItem[] itemArray4 = new ToolStripItem[] { this.colorPickerSeparator, this.colorPickerSampleLabel, this.colorPickerSampleTypeSplitButton, this.colorPickerSeparator2, this.colorPickerSampleSizeSplitButton, this.colorPickerSeparator3, this.colorPickerBehaviorLabel, this.colorPickerClickBehaviorSplitButton, this.rasterizationSeparator, this.antiAliasingSplitButton, this.blendModeSplitButton, this.selectionRenderingQualitySplitButton, this.commitButtonSeparator, this.commitButton };
            this.Items.AddRange(itemArray4);
            this.SetConfigItemsVisibility(PaintDotNet.ToolBarConfigItems.None);
            foreach (ToolStripItem item in this.Items)
            {
                item.Anchor = AnchorStyles.Left;
            }
            base.ResumeLayout(false);
        }

        private void InitializePenSection()
        {
            this.penSeparator = new PdnToolStripSeparator();
            this.penSizeLabel = new ToolStripLabel();
            this.penSizeDecButton = new ToolStripButton();
            this.penSizeComboBox = new PdnToolStripComboBox(false);
            this.penSizeIncButton = new ToolStripButton();
            this.penHardnessLabel = new ToolStripLabel();
            this.penHardnessSlider = new SliderControl(this.toolSettings.Pen.Hardness);
            this.penStyleLabel = new ToolStripLabel();
            this.penStartCapSplitButton = new EnumSplitButton<LineCap2>(this.toolSettings.Pen.StartCap, "Start");
            this.penDashStyleSplitButton = new EnumSplitButton<DashStyle>(this.toolSettings.Pen.DashStyle, string.Empty, x => x != DashStyle.Custom);
            this.penEndCapSplitButton = new EnumSplitButton<LineCap2>(this.toolSettings.Pen.EndCap, "End");
            this.penSizeLabel.Text = PdnResources.GetString("PenConfigWidget.BrushWidthLabel");
            this.penSizeDecButton.Name = "penSizeDecButton";
            this.penSizeDecButton.ToolTipText = PdnResources.GetString("ToolConfigStrip.PenSizeDecButton.ToolTipText");
            this.penSizeDecButton.Image = PdnResources.GetImageResource("Icons.MinusButtonIcon.png").Reference;
            this.penSizeDecButton.Click += delegate (object sender, EventArgs e) {
                float delta = -1f;
                if ((Control.ModifierKeys & Keys.Control) != Keys.None)
                {
                    delta *= 5f;
                }
                this.AddToPenSize(delta);
            };
            this.penSizeComboBox.Name = "penSizeComboBox";
            this.penSizeComboBox.Validating += new CancelEventHandler(this.OnPenSizeComboBoxValidating);
            this.penSizeComboBox.TextChanged += new EventHandler(this.OnPenSizeComboBoxTextChanged);
            this.penSizeComboBox.AutoSize = false;
            this.penSizeComboBox.Width = 0x2c;
            this.penSizeComboBox.Size = new Size(UIUtil.ScaleWidth(this.penSizeComboBox.Width), this.penSizeComboBox.Height);
            this.penSizeComboBox.DropDownWidth = (this.penSizeComboBox.DropDownWidth * 3) / 2;
            this.penSizeComboBox.AvailableChanged += new EventHandler(this.OnPenSizeComboBoxAvailableChanged);
            this.penSizeIncButton.Name = "penSizeIncButton";
            this.penSizeIncButton.ToolTipText = PdnResources.GetString("ToolConfigStrip.PenSizeIncButton.ToolTipText");
            this.penSizeIncButton.Image = PdnResources.GetImageResource("Icons.PlusButtonIcon.png").Reference;
            this.penSizeIncButton.Click += delegate (object sender, EventArgs e) {
                float delta = 1f;
                if ((Control.ModifierKeys & Keys.Control) != Keys.None)
                {
                    delta *= 5f;
                }
                this.AddToPenSize(delta);
            };
            this.radiusLabel = new ToolStripLabel();
            this.radiusLabel.Text = PdnResources.GetString("ToolConfigStrip.RadiusLabel.Text");
            this.radiusDecButton = new ToolStripButton();
            this.radiusDecButton.Image = PdnResources.GetImageResource("Icons.MinusButtonIcon.png").Reference;
            this.radiusDecButton.Click += new EventHandler(this.OnRadiusIncDecButtonClick);
            this.radiusDecButton.ToolTipText = PdnResources.GetString("ToolConfigStrip.RadiusDecButton.ToolTipText");
            this.radiusComboBox = new PdnToolStripComboBox(false);
            this.radiusComboBox.Name = "radiusComboBox";
            this.radiusComboBox.AutoSize = false;
            this.radiusComboBox.Width = 0x2c;
            this.radiusComboBox.Size = new Size(UIUtil.ScaleWidth(this.radiusComboBox.Width), this.radiusComboBox.Height);
            this.radiusComboBox.DropDownWidth = (this.radiusComboBox.DropDownWidth * 3) / 2;
            this.radiusComboBox.Validating += new CancelEventHandler(this.OnRadiusComboBoxValidating);
            this.radiusComboBox.TextChanged += new EventHandler(this.OnRadiusComboBoxTextChanged);
            this.radiusComboBox.AvailableChanged += new EventHandler(this.OnRadiusComboBoxAvailableChanged);
            this.radiusIncButton = new ToolStripButton();
            this.radiusIncButton.Click += new EventHandler(this.OnRadiusIncDecButtonClick);
            this.radiusIncButton.Image = PdnResources.GetImageResource("Icons.PlusButtonIcon.png").Reference;
            this.radiusIncButton.ToolTipText = PdnResources.GetString("ToolConfigStrip.RadiusIncButton.ToolTipText");
            this.penHardnessLabel.Text = PdnResources.GetString("ToolConfigStrip.PenHardnessLabel.Text");
            this.penStyleLabel.Text = PdnResources.GetString("ToolConfigStrip.PenStyleLabel.Text");
            this.penStartCapSplitButton.ToolTipText = PdnResources.GetString("ToolConfigStrip.PenStartCapSplitButton.ToolTipText");
            this.penDashStyleSplitButton.ToolTipText = PdnResources.GetString("ToolConfigStrip.PenDashStyleSplitButton.ToolTipText");
            this.penEndCapSplitButton.ToolTipText = PdnResources.GetString("ToolConfigStrip.PenEndCapSplitButton.ToolTipText");
        }

        private void InitializeResamplingSection()
        {
            this.resamplingSeparator = new PdnToolStripSeparator();
            this.resamplingLabel = new ToolStripLabel();
            this.resamplingSplitButton = new EnumSplitButton<ResamplingAlgorithm>(this.toolSettings.MoveToolResamplingAlgorithm, string.Empty, delegate (ResamplingAlgorithm ra) {
                if (ra != ResamplingAlgorithm.Bilinear)
                {
                    return ra == ResamplingAlgorithm.NearestNeighbor;
                }
                return true;
            });
            this.resamplingLabel.Text = PdnResources.GetString("ToolConfigStrip.ResamplingLabel.Text");
            this.resamplingSplitButton.Name = "resamplingSplitButton";
            this.resamplingSplitButton.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
        }

        private void InitializeSelectionDrawModeSection()
        {
            this.selectionDrawModeSeparator = new PdnToolStripSeparator();
            this.selectionDrawModeModeLabel = new ToolStripLabel();
            this.selectionDrawModeSplitButton = new EnumSplitButton<SelectionDrawMode>(this.toolSettings.Selection.DrawMode);
            this.selectionDrawModeWidthLabel = new ToolStripLabel();
            this.selectionDrawModeWidthTextBox = new PdnToolStripTextBox();
            this.selectionDrawModeSwapButton = new ToolStripButton();
            this.selectionDrawModeHeightLabel = new ToolStripLabel();
            this.selectionDrawModeHeightTextBox = new PdnToolStripTextBox();
            this.selectionDrawModeUnits = new UnitsComboBoxStrip();
            this.selectionDrawModeModeLabel.Text = PdnResources.GetString("ToolConfigStrip.SelectionDrawModeLabel.Text");
            this.selectionDrawModeSplitButton.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            this.selectionDrawModeWidthLabel.Text = PdnResources.GetString("ToolConfigStrip.SelectionDrawModeWidthLabel.Text");
            this.selectionDrawModeWidthTextBox.Name = "selectionDrawModeWidthTextBox";
            this.selectionDrawModeWidthTextBox.TextBox.Width = 50;
            this.selectionDrawModeWidthTextBox.Size = new Size(UIUtil.ScaleWidth(this.selectionDrawModeWidthTextBox.Width), this.selectionDrawModeWidthTextBox.Height);
            this.selectionDrawModeWidthTextBox.TextBoxTextAlign = HorizontalAlignment.Right;
            this.selectionDrawModeWidthTextBox.Enter += (sender, e) => this.selectionDrawModeWidthTextBox.TextBox.Select(0, this.selectionDrawModeWidthTextBox.TextBox.Text.Length);
            this.selectionDrawModeWidthTextBox.Leave += delegate (object sender, EventArgs e) {
                double num;
                if (double.TryParse(this.selectionDrawModeWidthTextBox.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out num))
                {
                    this.toolSettings.Selection.DrawWidth.Value = num;
                }
                else
                {
                    this.selectionDrawModeWidthTextBox.Text = this.toolSettings.Selection.DrawWidth.Value.ToString(CultureInfo.CurrentCulture);
                }
            };
            this.selectionDrawModeSwapButton.Name = "selectionDrawModeSwapButton";
            this.selectionDrawModeSwapButton.Image = PdnResources.GetImageResource("Icons.ToolConfigStrip.SelectionDrawModeSwapButton.png").Reference;
            this.selectionDrawModeSwapButton.Click += delegate (object sender, EventArgs e) {
                double num = this.toolSettings.Selection.DrawWidth.Value;
                double num2 = this.toolSettings.Selection.DrawHeight.Value;
                using (this.toolSettings.Selection.DrawWidth.SuspendValueChangedEvent())
                {
                    using (this.toolSettings.Selection.DrawHeight.SuspendValueChangedEvent())
                    {
                        this.toolSettings.Selection.DrawWidth.Value = num2;
                        this.toolSettings.Selection.DrawHeight.Value = num;
                    }
                }
            };
            this.selectionDrawModeHeightLabel.Text = PdnResources.GetString("ToolConfigStrip.SelectionDrawModeHeightLabel.Text");
            this.selectionDrawModeHeightTextBox.Name = "selectionDrawModeHeightTextBox";
            this.selectionDrawModeHeightTextBox.TextBox.Width = 50;
            this.selectionDrawModeHeightTextBox.Size = new Size(UIUtil.ScaleWidth(this.selectionDrawModeHeightTextBox.Width), this.selectionDrawModeHeightTextBox.Height);
            this.selectionDrawModeHeightTextBox.TextBoxTextAlign = HorizontalAlignment.Right;
            this.selectionDrawModeHeightTextBox.Enter += (sender, e) => this.selectionDrawModeHeightTextBox.TextBox.Select(0, this.selectionDrawModeHeightTextBox.TextBox.Text.Length);
            this.selectionDrawModeHeightTextBox.Leave += delegate (object sender, EventArgs e) {
                double num;
                if (double.TryParse(this.selectionDrawModeHeightTextBox.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out num))
                {
                    this.toolSettings.Selection.DrawHeight.Value = num;
                }
                else
                {
                    this.selectionDrawModeHeightTextBox.Text = this.toolSettings.Selection.DrawHeight.Value.ToString(CultureInfo.CurrentCulture);
                }
            };
            this.selectionDrawModeUnits.Name = "selectionDrawModeUnits";
            this.selectionDrawModeUnits.UnitsDisplayType = UnitsDisplayType.Plural;
            this.selectionDrawModeUnits.LowercaseStrings = true;
            this.selectionDrawModeUnits.Size = new Size(100, this.selectionDrawModeUnits.Height);
            this.selectionDrawModeUnits.Size = new Size(UIUtil.ScaleWidth(this.selectionDrawModeUnits.Width), this.selectionDrawModeUnits.Height);
            this.selectionDrawModeUnits.UnitsChanged += new EventHandler(this.OnSelectionDrawModeUnitsChanged);
        }

        private void InitializeTextSection()
        {
            this.fontMap = DirectWriteFactory.Instance.GetGdiFontMapRef(true);
            this.fontSeparator = new PdnToolStripSeparator();
            this.fontLabel = new ToolStripLabel();
            this.fontFamilyComboBox = new PdnToolStripComboBox(true);
            this.fontSizeComboBox = new PdnToolStripComboBox(false);
            this.fontStyleSeparator = new PdnToolStripSeparator();
            this.fontStyleButtonGroup = new EnumFlagsButtonGroup<System.Drawing.FontStyle>(this.toolSettings.Text.FontStyle);
            this.fontAlignSeparator = new PdnToolStripSeparator();
            this.fontAlignRadioButtonGroup = new EnumRadioButtonGroup<PaintDotNet.TextAlignment>(this.toolSettings.Text.Alignment);
            this.fontRenderingModeSplitButton = new EnumSplitButton<TextToolRenderingMode>(this.toolSettings.Text.RenderingMode, "TextToolRenderingMode");
            this.fontLabel.Text = PdnResources.GetString("TextConfigWidget.FontLabel.Text");
            this.fontFamilyComboBox.Name = "fontFamilyComboBox";
            this.fontFamilyComboBox.Size = new Size(140, 0x15);
            this.fontFamilyComboBox.DropDownWidth = 300;
            this.fontFamilyComboBox.DropDownHeight = 600;
            this.fontFamilyComboBox.MaxDropDownItems = 12;
            this.fontFamilyComboBox.SelectedIndexChanged += new EventHandler(this.OnFontFamilyNameComboBoxSelectedIndexChanged);
            this.fontFamilyComboBox.Size = new Size(UIUtil.ScaleWidth(this.fontFamilyComboBox.Width), this.fontFamilyComboBox.Height);
            this.fontFamilyComboBox.ComboBox.DropDownHeight = UIUtil.ScaleHeight(this.fontFamilyComboBox.ComboBox.DropDownHeight);
            this.fontFamilyComboBox.ComboBox.DropDownWidth = UIUtil.ScaleWidth(this.fontFamilyComboBox.DropDownWidth);
            this.fontFamilyComboBox.AvailableChanged += new EventHandler(this.OnFontFamilyComboBoxAvailableChanged);
            this.fontSizeComboBox.Name = "fontSizeComboBox";
            this.fontSizeComboBox.AutoSize = false;
            this.fontSizeComboBox.TextChanged += new EventHandler(this.OnFontSizeComboBoxTextChanged);
            this.fontSizeComboBox.Validating += new CancelEventHandler(this.OnFontSizeComboBoxValidating);
            this.fontSizeComboBox.Width = 0x2c;
            this.fontSizeComboBox.Size = new Size(UIUtil.ScaleWidth(this.fontSizeComboBox.Width), this.fontSizeComboBox.Height);
            this.fontSizeComboBox.AvailableChanged += new EventHandler(this.OnFontSizeComboBoxAvailableChanged);
            this.fontRenderingModeSplitButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.fontRenderingModeSplitButton.ToolTipText = PdnResources.GetString("ToolConfigStrip.FontRenderingModeSplitButton.ToolTipText");
        }

        public void LoadFromSettings(AppSettings.ToolsSection toolSettingsSource)
        {
            List<IDisposable> list = new List<IDisposable>(toolSettingsSource.Count);
            try
            {
                foreach (Setting setting in this.toolSettings.Settings)
                {
                    list.Add(setting.SuspendValueChangedEvent());
                }
                this.toolSettings.LoadFrom(toolSettingsSource);
                foreach (Setting setting2 in this.toolSettings.Settings)
                {
                    setting2.RaiseValueChangedEvent();
                }
            }
            finally
            {
                foreach (IDisposable disposable in list)
                {
                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }
            }
        }

        private void OnAppSettingsPenWidthChanged(object sender, ValueChangedEventArgs<float> e)
        {
            this.SyncPenUI();
        }

        private void OnAppSettingsRadiusChanged(object sender, ValueChangedEventArgs<float> e)
        {
            this.SyncRadiusUI();
        }

        private void OnAppSettingsSelectionDrawHeightChanged(object sender, ValueChangedEventArgs<double> e)
        {
            this.selectionDrawModeHeightTextBox.Text = this.toolSettings.Selection.DrawHeight.Value.ToString(CultureInfo.CurrentCulture);
        }

        private void OnAppSettingsSelectionDrawModeChanged(object sender, ValueChangedEventArgs<SelectionDrawMode> e)
        {
            this.SyncSelectionConstraintUIVisibility();
        }

        private void OnAppSettingsSelectionDrawUnitsChanged(object sender, ValueChangedEventArgs<MeasurementUnit> e)
        {
            this.selectionDrawModeUnits.Units = this.toolSettings.Selection.DrawUnits.Value;
        }

        private void OnAppSettingsSelectionDrawWidthChanged(object sender, ValueChangedEventArgs<double> e)
        {
            this.selectionDrawModeWidthTextBox.Text = this.toolSettings.Selection.DrawWidth.Value.ToString(CultureInfo.CurrentCulture);
        }

        private void OnAppSettingsTextFontFamilyNameChanged(object sender, ValueChangedEventArgs<string> e)
        {
            this.SyncTextFontFamilyNameUI();
        }

        private void OnAppSettingsTextFontSizeChanged(object sender, ValueChangedEventArgs<float> e)
        {
            this.SyncTextFontSizeUI();
        }

        private void OnCommitButtonClicked(object sender, EventArgs e)
        {
            if (this.IsCommitButtonEnabled && this.IsCommitButtonVisible)
            {
                this.CommitButtonClicked.Raise(this);
            }
        }

        private void OnFontFamilyComboBoxAvailableChanged(object sender, EventArgs e)
        {
            if (this.fontFamilyComboBox.Available && (this.fontFamilyComboBoxHandler == null))
            {
                this.fontFamilyComboBoxHandler = new FontListComboBoxHandler(this.fontFamilyComboBox.ComboBox, this.fontMap, this.toolSettings.Text.FontFamilyName.Value);
            }
        }

        private void OnFontFamilyNameComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.fontFamilyComboBox.SelectedIndex != -1)
            {
                this.toolSettings.Text.FontFamilyName.Value = (string) this.fontFamilyComboBox.Items[this.fontFamilyComboBox.SelectedIndex];
            }
        }

        private void OnFontSizeComboBoxAvailableChanged(object sender, EventArgs e)
        {
            if (this.fontSizeComboBox.Available && (this.fontSizeComboBox.Items.Count == 0))
            {
                this.PopulateDefaultFontSizes();
            }
        }

        private void OnFontSizeComboBoxTextChanged(object sender, EventArgs e)
        {
            this.OnFontSizeComboBoxValidating(sender, new CancelEventArgs());
        }

        private void OnFontSizeComboBoxValidating(object sender, CancelEventArgs e)
        {
            try
            {
                float num;
                if (!float.TryParse(this.fontSizeComboBox.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out num))
                {
                    this.fontSizeComboBox.BackColor = Color.Red;
                    this.fontSizeComboBox.ToolTipText = PdnResources.GetString("TextConfigWidget.Error.InvalidNumber");
                }
                else if (num < 1f)
                {
                    this.fontSizeComboBox.BackColor = Color.Red;
                    string str2 = string.Format(PdnResources.GetString("TextConfigWidget.Error.TooSmall.Format"), 1);
                    this.fontSizeComboBox.ToolTipText = str2;
                }
                else if (num > 2000f)
                {
                    this.fontSizeComboBox.BackColor = Color.Red;
                    string str4 = string.Format(PdnResources.GetString("TextConfigWidget.Error.TooLarge.Format"), 0x7d0);
                    this.fontSizeComboBox.ToolTipText = str4;
                }
                else
                {
                    this.fontSizeComboBox.ToolTipText = string.Empty;
                    this.fontSizeComboBox.BackColor = SystemColors.Window;
                    this.toolSettings.Text.FontSize.Value = num;
                }
            }
            catch (FormatException)
            {
                e.Cancel = true;
            }
        }

        private void OnPenSizeComboBoxAvailableChanged(object sender, EventArgs e)
        {
            if (this.penSizeComboBox.Available && (this.penSizeComboBox.Items.Count == 0))
            {
                this.PopulateDefaultPenSizes();
            }
        }

        private void OnPenSizeComboBoxTextChanged(object sender, EventArgs e)
        {
            this.suspendWriteBackToPenSizeComboBox = true;
            this.OnPenSizeComboBoxValidating(sender, new CancelEventArgs());
            this.suspendWriteBackToPenSizeComboBox = false;
        }

        private void OnPenSizeComboBoxValidating(object sender, CancelEventArgs e)
        {
            float num;
            if (!float.TryParse(this.penSizeComboBox.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out num))
            {
                this.penSizeComboBox.BackColor = Color.Red;
                this.penSizeComboBox.ToolTipText = PdnResources.GetString("PenConfigWidget.Error.InvalidNumber");
            }
            else if (num < this.toolSettings.Pen.Width.MinValue)
            {
                this.penSizeComboBox.BackColor = Color.Red;
                string str2 = string.Format(PdnResources.GetString("PenConfigWidget.Error.TooSmall.Format"), this.toolSettings.Pen.Width.MinValue);
                this.penSizeComboBox.ToolTipText = str2;
            }
            else if (num > this.toolSettings.Pen.Width.MaxValue)
            {
                this.penSizeComboBox.BackColor = Color.Red;
                string str4 = string.Format(PdnResources.GetString("PenConfigWidget.Error.TooLarge.Format"), this.toolSettings.Pen.Width.MaxValue);
                this.penSizeComboBox.ToolTipText = str4;
            }
            else
            {
                this.penSizeComboBox.BackColor = SystemColors.Window;
                this.penSizeComboBox.ToolTipText = string.Empty;
                this.toolSettings.Pen.Width.Value = num;
            }
        }

        private void OnRadiusComboBoxAvailableChanged(object sender, EventArgs e)
        {
            if (this.radiusComboBox.Available && (this.radiusComboBox.Items.Count == 0))
            {
                this.PopulateDefaultRadiusValues();
            }
        }

        private void OnRadiusComboBoxTextChanged(object sender, EventArgs e)
        {
            this.suspendWriteBackToRadiusComboBox = true;
            this.OnRadiusComboBoxValidating(sender, new CancelEventArgs());
            this.suspendWriteBackToRadiusComboBox = false;
        }

        private void OnRadiusComboBoxValidating(object sender, CancelEventArgs e)
        {
            float num;
            if (!float.TryParse(this.radiusComboBox.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out num))
            {
                this.radiusComboBox.BackColor = Color.Red;
                this.radiusComboBox.ToolTipText = PdnResources.GetString("PenConfigWidget.Error.InvalidNumber");
            }
            else if (num < this.toolSettings.Radius.MinValue)
            {
                this.radiusComboBox.BackColor = Color.Red;
                string str2 = string.Format(PdnResources.GetString("PenConfigWidget.Error.TooSmall.Format"), this.toolSettings.Radius.MinValue);
                this.radiusComboBox.ToolTipText = str2;
            }
            else if (num > this.toolSettings.Radius.MaxValue)
            {
                this.radiusComboBox.BackColor = Color.Red;
                string str4 = string.Format(PdnResources.GetString("PenConfigWidget.Error.TooLarge.Format"), this.toolSettings.Radius.MaxValue);
                this.radiusComboBox.ToolTipText = str4;
            }
            else
            {
                this.radiusComboBox.BackColor = SystemColors.Window;
                this.radiusComboBox.ToolTipText = string.Empty;
                this.toolSettings.Radius.Value = num;
            }
        }

        private void OnRadiusIncDecButtonClick(object sender, EventArgs e)
        {
            float delta = (sender == this.radiusIncButton) ? 1f : ((sender == this.radiusDecButton) ? -1f : 0f);
            if ((Control.ModifierKeys & Keys.Control) != Keys.None)
            {
                delta *= 5f;
            }
            this.AddToRadius(delta);
        }

        private void OnSelectionDrawModeUnitsChanged(object sender, EventArgs e)
        {
            this.toolSettings.Selection.DrawUnits.Value = this.selectionDrawModeUnits.Units;
        }

        private void OnToolBarConfigItemsChanged()
        {
            this.ToolBarConfigItemsChanged.Raise(this);
        }

        private void PopulateDefaultFontSizes()
        {
            this.fontSizeComboBox.ComboBox.SuspendLayout();
            for (int i = 0; i < this.defaultFontSizes.Length; i++)
            {
                this.fontSizeComboBox.Items.Add(this.defaultFontSizes[i].ToString(CultureInfo.CurrentCulture));
            }
            this.fontSizeComboBox.ComboBox.ResumeLayout(false);
        }

        private void PopulateDefaultPenSizes()
        {
            this.penSizeComboBox.ComboBox.SuspendLayout();
            for (int i = 0; i < comboBoxValues.Length; i++)
            {
                this.penSizeComboBox.Items.Add(comboBoxValues[i].ToString(CultureInfo.CurrentCulture));
            }
            this.penSizeComboBox.ComboBox.ResumeLayout(false);
        }

        private void PopulateDefaultRadiusValues()
        {
            this.radiusComboBox.ComboBox.SuspendLayout();
            for (int i = 0; i < comboBoxValues.Length; i++)
            {
                this.radiusComboBox.Items.Add(comboBoxValues[i].ToString(CultureInfo.CurrentCulture));
            }
            this.radiusComboBox.ComboBox.ResumeLayout(false);
        }

        private void SetConfigItemsVisibility(PaintDotNet.ToolBarConfigItems value)
        {
            bool showPenWidth = (value & (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.PenWidth)) > PaintDotNet.ToolBarConfigItems.None;
            bool showRadius = (value & (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.Radius)) > PaintDotNet.ToolBarConfigItems.None;
            bool showPenHardness = (value & (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.PenHardness)) > PaintDotNet.ToolBarConfigItems.None;
            bool showPenStartCap = (value & (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.PenStartCap)) > PaintDotNet.ToolBarConfigItems.None;
            bool showPenDashStyle = (value & (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.PenDashStyle)) > PaintDotNet.ToolBarConfigItems.None;
            bool showPenEndCap = (value & (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.PenEndCap)) > PaintDotNet.ToolBarConfigItems.None;
            this.SetPenSectionVisibility(showPenWidth, showRadius, showPenHardness, showPenStartCap, showPenDashStyle, showPenEndCap);
            bool flag7 = (value & PaintDotNet.ToolBarConfigItems.Brush) > PaintDotNet.ToolBarConfigItems.None;
            this.brushSeparator.Visible = flag7;
            this.brushStyleLabel.Visible = flag7;
            this.brushStyleComboBox.Visible = flag7;
            bool flag8 = (value & (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.RecolorToolSamplingMode)) > PaintDotNet.ToolBarConfigItems.None;
            this.recolorToolSeparator.Visible = flag8;
            this.recolorToolSamplingModeRBGroup.Visible = flag8;
            bool flag9 = (value & (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.ShapeDrawType)) > PaintDotNet.ToolBarConfigItems.None;
            bool flag10 = (value & (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.ShapeType)) > PaintDotNet.ToolBarConfigItems.None;
            this.shapeTypeDropDownButton.Visible = flag10;
            this.shapeDrawTypeButton.Visible = flag9;
            bool flag11 = (value & PaintDotNet.ToolBarConfigItems.LineCurveShapeType) > PaintDotNet.ToolBarConfigItems.None;
            this.lineCurveShapeTypeRadioButtonGroup.Visible = flag11;
            this.shapeSeparator.Visible = (flag10 | flag9) | flag11;
            bool flag12 = (value & PaintDotNet.ToolBarConfigItems.Gradient) > PaintDotNet.ToolBarConfigItems.None;
            this.gradientSeparator1.Visible = flag12;
            this.gradientTypeRadioButtonGroup.Visible = flag12;
            this.gradientSeparator2.Visible = flag12;
            this.gradientChannelsSplitButton.Visible = flag12;
            this.gradientRepeatTypeSplitButton.Visible = flag12;
            bool flag13 = (value & PaintDotNet.ToolBarConfigItems.Antialiasing) > PaintDotNet.ToolBarConfigItems.None;
            this.antiAliasingSplitButton.Visible = flag13;
            bool flag14 = (value & PaintDotNet.ToolBarConfigItems.BlendMode) > PaintDotNet.ToolBarConfigItems.None;
            this.blendModeSplitButton.Visible = flag14;
            bool flag15 = (value & (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.SelectionRenderingQuality)) > PaintDotNet.ToolBarConfigItems.None;
            this.selectionRenderingQualitySplitButton.Visible = flag15;
            bool flag16 = (flag13 | flag14) | flag15;
            this.rasterizationSeparator.Visible = flag16;
            bool flag17 = (value & (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.Tolerance)) > PaintDotNet.ToolBarConfigItems.None;
            this.toleranceSeparator.Visible = flag17;
            this.toleranceLabel.Visible = flag17;
            this.toleranceSliderStrip.Visible = flag17;
            bool showText = (value & (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.Text)) > PaintDotNet.ToolBarConfigItems.None;
            this.SetTextSectionVisibility(showText);
            bool showResampling = (value & (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.Resampling)) > PaintDotNet.ToolBarConfigItems.None;
            this.SetResamplingSectionVisibility(showResampling);
            bool flag20 = (value & (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.SampleImageOrLayer)) > PaintDotNet.ToolBarConfigItems.None;
            this.colorPickerSeparator.Visible = flag20;
            this.colorPickerSampleLabel.Visible = flag20;
            this.colorPickerSampleTypeSplitButton.Visible = flag20;
            bool flag21 = (value & (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.PixelSampleMode)) > PaintDotNet.ToolBarConfigItems.None;
            this.colorPickerSeparator2.Visible = flag21 && !flag20;
            this.colorPickerSampleSizeSplitButton.Visible = flag21;
            bool flag22 = (value & PaintDotNet.ToolBarConfigItems.ColorPickerBehavior) > PaintDotNet.ToolBarConfigItems.None;
            this.colorPickerSeparator3.Visible = flag22;
            this.colorPickerBehaviorLabel.Visible = flag22;
            this.colorPickerClickBehaviorSplitButton.Visible = flag22;
            bool flag23 = (value & (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.SelectionCombineMode)) > PaintDotNet.ToolBarConfigItems.None;
            this.selectionCombineModeSeparator.Visible = flag23;
            this.selectionCombineModeRadioButtonGroup.Visible = flag23;
            bool flag24 = (value & PaintDotNet.ToolBarConfigItems.FloodMode) > PaintDotNet.ToolBarConfigItems.None;
            this.floodModeSeparator.Visible = flag24;
            this.floodModeLabel.Visible = flag24;
            this.floodModeSplitButton.Visible = flag24;
            bool showSelectionDrawMode = (value & (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.SelectionDrawMode)) > PaintDotNet.ToolBarConfigItems.None;
            this.SetSelectionDrawModeVisibility(showSelectionDrawMode);
            if (!this.showFirstAndLastSeparators)
            {
                int count = this.Items.Count;
                for (int i = 0; i < count; i++)
                {
                    ToolStripSeparator separator = this.Items[i] as ToolStripSeparator;
                    if ((separator != null) && separator.Visible)
                    {
                        separator.Visible = false;
                        break;
                    }
                }
                for (int j = count - 1; j >= 0; j--)
                {
                    ToolStripSeparator separator2 = this.Items[j] as ToolStripSeparator;
                    if ((separator2 != null) && separator2.Visible)
                    {
                        separator2.Visible = false;
                        break;
                    }
                }
            }
            if (value == PaintDotNet.ToolBarConfigItems.None)
            {
                base.Visible = false;
            }
            else
            {
                base.Visible = true;
            }
        }

        private void SetPenSectionVisibility(bool showPenWidth, bool showRadius, bool showPenHardness, bool showPenStartCap, bool showPenDashStyle, bool showPenEndCap)
        {
            bool flag = ((((showPenWidth | showRadius) | showPenHardness) | showPenStartCap) | showPenDashStyle) | showPenEndCap;
            this.penSeparator.Visible = flag;
            this.penSizeLabel.Visible = showPenWidth;
            this.penSizeDecButton.Visible = showPenWidth;
            this.penSizeComboBox.Visible = showPenWidth;
            this.penSizeIncButton.Visible = showPenWidth;
            this.radiusLabel.Visible = showRadius;
            this.radiusDecButton.Visible = showRadius;
            this.radiusComboBox.Visible = showRadius;
            this.radiusIncButton.Visible = showRadius;
            this.penHardnessLabel.Visible = showPenHardness;
            this.penHardnessSlider.Visible = showPenHardness;
            bool flag2 = (showPenStartCap | showPenDashStyle) | showPenEndCap;
            this.penStyleLabel.Visible = flag2;
            this.penStartCapSplitButton.Visible = showPenStartCap;
            this.penDashStyleSplitButton.Visible = showPenDashStyle;
            this.penEndCapSplitButton.Visible = showPenEndCap;
        }

        private void SetResamplingSectionVisibility(bool showResampling)
        {
            this.resamplingSeparator.Visible = showResampling;
            this.resamplingLabel.Visible = showResampling;
            this.resamplingSplitButton.Visible = showResampling;
        }

        private void SetSelectionDrawModeVisibility(bool showSelectionDrawMode)
        {
            this.selectionDrawModeSeparator.Visible = showSelectionDrawMode;
            this.selectionDrawModeModeLabel.Visible = showSelectionDrawMode;
            this.selectionDrawModeSplitButton.Visible = showSelectionDrawMode;
            this.selectionDrawModeWidthLabel.Visible = showSelectionDrawMode;
            this.selectionDrawModeSwapButton.Visible = showSelectionDrawMode;
            this.selectionDrawModeHeightLabel.Visible = showSelectionDrawMode;
            this.SyncSelectionConstraintUIVisibility();
        }

        private void SetTextSectionVisibility(bool showText)
        {
            this.fontSeparator.Visible = showText;
            this.fontLabel.Visible = showText;
            this.fontFamilyComboBox.Visible = showText;
            this.fontSizeComboBox.Visible = showText;
            this.fontStyleSeparator.Visible = showText;
            this.fontStyleButtonGroup.Visible = showText;
            this.fontAlignSeparator.Visible = showText;
            this.fontAlignRadioButtonGroup.Visible = showText;
            this.fontRenderingModeSplitButton.Visible = showText;
            if (showText && base.IsHandleCreated)
            {
                this.fontFamilyComboBoxHandler.AsyncPrefetchFontNames();
            }
        }

        public void SyncPenUI()
        {
            if (!this.suspendWriteBackToPenSizeComboBox)
            {
                this.penSizeComboBox.Text = this.toolSettings.Pen.Width.Value.ToString(CultureInfo.CurrentCulture);
            }
        }

        private void SyncRadiusUI()
        {
            if (!this.suspendWriteBackToRadiusComboBox)
            {
                this.radiusComboBox.Text = this.toolSettings.Radius.Value.ToString(CultureInfo.CurrentCulture);
            }
        }

        private void SyncSelectionConstraintUIVisibility()
        {
            base.SuspendLayout();
            bool flag = (this.ToolBarConfigItems & (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.SelectionDrawMode)) > PaintDotNet.ToolBarConfigItems.None;
            this.selectionDrawModeModeLabel.Visible = false;
            bool flag2 = flag & (((SelectionDrawMode) this.toolSettings.Selection.DrawMode.Value) > SelectionDrawMode.Normal);
            this.selectionDrawModeWidthTextBox.Visible = flag2;
            this.selectionDrawModeHeightTextBox.Visible = flag2;
            this.selectionDrawModeWidthLabel.Visible = flag2;
            this.selectionDrawModeHeightLabel.Visible = flag2;
            this.selectionDrawModeSwapButton.Visible = flag2;
            this.selectionDrawModeUnits.Visible = flag & (((SelectionDrawMode) this.toolSettings.Selection.DrawMode.Value) == SelectionDrawMode.FixedSize);
            base.ResumeLayout();
        }

        private void SyncTextFontFamilyNameUI()
        {
            string selectedItem = (string) this.fontFamilyComboBox.SelectedItem;
            string str2 = this.toolSettings.Text.FontFamilyName.Value;
            if (selectedItem != str2)
            {
                int index = this.fontFamilyComboBox.Items.IndexOf(str2);
                if (index != -1)
                {
                    this.fontFamilyComboBox.SelectedIndex = index;
                }
                else
                {
                    this.fontFamilyComboBox.Items.Add(str2);
                    this.fontFamilyComboBox.SelectedItem = str2;
                }
            }
        }

        private void SyncTextFontSizeUI()
        {
            string str = this.toolSettings.Text.FontSize.Value.ToString();
            if (str != this.fontSizeComboBox.Text)
            {
                this.fontSizeComboBox.Text = str;
            }
        }

        private void SyncTextUI()
        {
            this.SyncTextFontSizeUI();
            this.SyncTextFontFamilyNameUI();
        }

        private void UnbindPenSettings()
        {
            this.toolSettings.Pen.Width.ValueChangedT -= new ValueChangedEventHandler<float>(this.OnAppSettingsPenWidthChanged);
            this.toolSettings.Radius.ValueChangedT -= new ValueChangedEventHandler<float>(this.OnAppSettingsRadiusChanged);
        }

        private void UnbindSelectionDrawModeSettings()
        {
            this.toolSettings.Selection.DrawMode.ValueChangedT -= new ValueChangedEventHandler<SelectionDrawMode>(this.OnAppSettingsSelectionDrawModeChanged);
            this.toolSettings.Selection.DrawHeight.ValueChangedT -= new ValueChangedEventHandler<double>(this.OnAppSettingsSelectionDrawHeightChanged);
            this.toolSettings.Selection.DrawUnits.ValueChangedT -= new ValueChangedEventHandler<MeasurementUnit>(this.OnAppSettingsSelectionDrawUnitsChanged);
            this.toolSettings.Selection.DrawWidth.ValueChangedT -= new ValueChangedEventHandler<double>(this.OnAppSettingsSelectionDrawWidthChanged);
        }

        private void UnbindTextSettings()
        {
            this.toolSettings.Text.FontFamilyName.ValueChangedT -= new ValueChangedEventHandler<string>(this.OnAppSettingsTextFontFamilyNameChanged);
            this.toolSettings.Text.FontSize.ValueChangedT -= new ValueChangedEventHandler<float>(this.OnAppSettingsTextFontSizeChanged);
        }

        private void UpdateConfigItemsVisibility(PaintDotNet.ToolBarConfigItems value)
        {
            bool flag = base.Visible && base.IsHandleCreated;
            if (flag)
            {
                UIUtil.SuspendControlPainting(this);
            }
            base.SuspendLayout();
            this.SetConfigItemsVisibility(value);
            base.ResumeLayout(false);
            base.PerformLayout(this, "toolBarConfigItems");
            if (flag)
            {
                UIUtil.ResumeControlPainting(this);
                base.Invalidate(true);
            }
        }

        public bool IsCommitButtonEnabled
        {
            get
            {
                this.VerifyThreadAccess();
                return this.commitButton.Enabled;
            }
            set
            {
                this.VerifyThreadAccess();
                this.commitButton.Enabled = value;
                this.QueueUpdate();
            }
        }

        public bool IsCommitButtonVisible
        {
            get
            {
                this.VerifyThreadAccess();
                return this.commitButton.Visible;
            }
            set
            {
                this.VerifyThreadAccess();
                this.commitButton.Visible = value;
                this.commitButtonSeparator.Visible = value;
            }
        }

        public bool ShowFirstAndLastSeparators
        {
            get => 
                this.showFirstAndLastSeparators;
            set
            {
                this.VerifyThreadAccess();
                if (value != this.showFirstAndLastSeparators)
                {
                    this.showFirstAndLastSeparators = value;
                    this.UpdateConfigItemsVisibility(this.toolBarConfigItems);
                }
            }
        }

        public PaintDotNet.ToolBarConfigItems ToolBarConfigItems
        {
            get => 
                this.toolBarConfigItems;
            set
            {
                if (this.toolBarConfigItems != value)
                {
                    this.toolBarConfigItems = value;
                    this.UpdateConfigItemsVisibility(value);
                    this.OnToolBarConfigItemsChanged();
                }
            }
        }

        public AppSettings.ToolsSection ToolSettings =>
            this.toolSettings;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly ToolConfigStrip.<>c <>9 = new ToolConfigStrip.<>c();
            public static Func<ContentBlendMode, ImageResource> <>9__163_0;
            public static Func<ResamplingAlgorithm, bool> <>9__6_0;
            public static Func<DashStyle, bool> <>9__81_0;

            internal ImageResource <InitializeComponent>b__163_0(ContentBlendMode value)
            {
                if (value.IsCompositionOp())
                {
                    return PdnResources.GetImageResource("Icons.AlphaBlending.True.png");
                }
                return PdnResources.GetImageResource("Icons.AlphaBlending.False.png");
            }

            internal bool <InitializePenSection>b__81_0(DashStyle x) => 
                (x != DashStyle.Custom);

            internal bool <InitializeResamplingSection>b__6_0(ResamplingAlgorithm ra)
            {
                if (ra != ResamplingAlgorithm.Bilinear)
                {
                    return (ra == ResamplingAlgorithm.NearestNeighbor);
                }
                return true;
            }
        }
    }
}

