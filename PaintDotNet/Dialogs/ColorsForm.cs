namespace PaintDotNet.Dialogs
{
    using PaintDotNet;
    using PaintDotNet.AppModel;
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.Drawing;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.Settings.App;
    using PaintDotNet.Snap;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.VisualStyling;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class ColorsForm : FloatingToolForm
    {
        private ColorGradientControl alphaGradientControl;
        private HeadingLabel alphaHeader;
        private NumericUpDown alphaUpDown;
        private ColorGradientControl blueGradientControl;
        private Label blueLabel;
        private NumericUpDown blueUpDown;
        private ToolStripButton colorAddButton;
        private Bitmap colorAddIcon;
        private Image colorAddOverlay;
        private ColorDisplayWidget colorDisplayWidget;
        private PdnToolStripSplitButton colorPalettesButton;
        private ColorWheel colorWheel;
        private Container components;
        private ColorGradientControl greenGradientControl;
        private Label greenLabel;
        private NumericUpDown greenUpDown;
        private TextBox hexBox;
        private Label hexLabel;
        private HeadingLabel hsvHeader;
        private ColorGradientControl hueGradientControl;
        private Label hueLabel;
        private NumericUpDown hueUpDown;
        private uint ignore;
        private int ignoreChangedEvents;
        private bool inMoreState = true;
        private ColorBgra lastPrimaryColor;
        private ColorBgra lastSecondaryColor;
        private Control lessModeButtonSentinel;
        private Control lessModeHeaderSentinel;
        private Size lessSize;
        private string lessText;
        private PdnPushButton moreLessButton;
        private Control moreModeButtonSentinel;
        private Control moreModeHeaderSentinel;
        private Size moreSize;
        private string moreText;
        private ColorGradientControl redGradientControl;
        private Label redLabel;
        private NumericUpDown redUpDown;
        private HeadingLabel rgbHeader;
        private ColorGradientControl saturationGradientControl;
        private Label saturationLabel;
        private NumericUpDown saturationUpDown;
        private int suspendSetWhichUserColor;
        private SwatchControl swatchControl;
        private PaintDotNet.Controls.SeparatorLine swatchHeader;
        private ToolStripEx toolStrip;
        private ColorBgra userPrimaryColor;
        private ColorBgra userSecondaryColor;
        private ColorGradientControl valueGradientControl;
        private Label valueLabel;
        private NumericUpDown valueUpDown;
        private PdnDropDownList whichUserColorBox;

        [field: CompilerGenerated]
        public event ColorEventHandler UserPrimaryColorChanged;

        [field: CompilerGenerated]
        public event ColorEventHandler UserSecondaryColorChanged;

        public ColorsForm()
        {
            this.InitializeComponent();
            this.whichUserColorBox.Items.Add(new WhichUserColorWrapper(PaintDotNet.WhichUserColor.Primary));
            this.whichUserColorBox.Items.Add(new WhichUserColorWrapper(PaintDotNet.WhichUserColor.Secondary));
            this.whichUserColorBox.SelectedIndex = 0;
            this.moreSize = base.ClientSize;
            this.lessSize = new Size(this.swatchHeader.Width + UIUtil.ScaleWidth(0x10), this.moreSize.Height);
            this.Text = PdnResources.GetString("ColorsForm.Text");
            this.redLabel.Text = PdnResources.GetString("ColorsForm.RedLabel.Text");
            this.blueLabel.Text = PdnResources.GetString("ColorsForm.BlueLabel.Text");
            this.greenLabel.Text = PdnResources.GetString("ColorsForm.GreenLabel.Text");
            this.saturationLabel.Text = PdnResources.GetString("ColorsForm.SaturationLabel.Text");
            this.valueLabel.Text = PdnResources.GetString("ColorsForm.ValueLabel.Text");
            this.hueLabel.Text = PdnResources.GetString("ColorsForm.HueLabel.Text");
            this.rgbHeader.Text = PdnResources.GetString("ColorsForm.RgbHeader.Text");
            this.hexLabel.Text = PdnResources.GetString("ColorsForm.HexLabel.Text");
            this.hsvHeader.Text = PdnResources.GetString("ColorsForm.HsvHeader.Text");
            this.alphaHeader.Text = PdnResources.GetString("ColorsForm.AlphaHeader.Text");
            this.lessText = "<< " + PdnResources.GetString("ColorsForm.MoreLessButton.Text.Less");
            this.moreText = PdnResources.GetString("ColorsForm.MoreLessButton.Text.More") + " >>";
            this.moreLessButton.Text = this.lessText;
            this.toolStrip.Renderer = new ColorsFormToolStripRenderer();
            this.colorAddOverlay = PdnResources.GetImageResource("Icons.ColorAddOverlay.png").Reference;
            this.colorPalettesButton.Image = PdnResources.GetImageResource("Icons.ColorPalettes.png").Reference;
            this.RenderColorAddIcon(this.UserPrimaryColor);
            this.colorAddButton.ToolTipText = PdnResources.GetString("ColorsForm.ColorAddButton.ToolTipText");
            this.colorPalettesButton.ToolTipText = PdnResources.GetString("ColorsForm.ColorPalettesButton.ToolTipText");
            string paletteSaveString = AppSettings.Instance.Workspace.CurrentPalette.Value;
            if (string.IsNullOrWhiteSpace(paletteSaveString))
            {
                paletteSaveString = UserPalettesService.Instance.GetPaletteSaveString(UserPalettesService.Instance.DefaultPalette);
            }
            ColorBgra[] bgraArray = UserPalettesService.Instance.ParsePaletteString(paletteSaveString);
            this.swatchControl.Colors = bgraArray;
        }

        private bool CheckHexBox(string hexBoxText)
        {
            int num;
            if (hexBoxText.StartsWith("#"))
            {
                hexBoxText = hexBoxText.Substring(1);
            }
            try
            {
                num = int.Parse(hexBoxText, NumberStyles.HexNumber);
            }
            catch (FormatException)
            {
                return false;
            }
            catch (OverflowException)
            {
                return false;
            }
            return ((num <= 0xffffff) && (num >= 0));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
                this.components = null;
            }
            base.Dispose(disposing);
        }

        private ColorBgra GetColorFromUpDowns()
        {
            int num = (int) this.redUpDown.Value;
            int num2 = (int) this.greenUpDown.Value;
            int num3 = (int) this.blueUpDown.Value;
            int num4 = (int) this.alphaUpDown.Value;
            return ColorBgra.FromBgra((byte) num3, (byte) num2, (byte) num, (byte) num4);
        }

        private string GetHexNumericUpDownValue(int red, int green, int blue)
        {
            int num = ((red << 0x10) | (green << 8)) | blue;
            string str = Convert.ToString(num, 0x10);
            while (str.Length < 6)
            {
                str = "0" + str;
            }
            return str.ToUpper();
        }

        private void InitializeComponent()
        {
            this.valueGradientControl = new ColorGradientControl();
            this.colorWheel = new ColorWheel();
            this.redUpDown = new NumericUpDown();
            this.greenUpDown = new NumericUpDown();
            this.blueUpDown = new NumericUpDown();
            this.redLabel = new PdnLabel();
            this.blueLabel = new PdnLabel();
            this.greenLabel = new PdnLabel();
            this.saturationLabel = new PdnLabel();
            this.valueLabel = new PdnLabel();
            this.hueLabel = new PdnLabel();
            this.valueUpDown = new NumericUpDown();
            this.saturationUpDown = new NumericUpDown();
            this.hueUpDown = new NumericUpDown();
            this.hexBox = new TextBox();
            this.hexLabel = new PdnLabel();
            this.whichUserColorBox = new PdnDropDownList();
            this.alphaUpDown = new NumericUpDown();
            this.moreLessButton = new PdnPushButton();
            this.lessModeButtonSentinel = new Control();
            this.moreModeButtonSentinel = new Control();
            this.lessModeHeaderSentinel = new Control();
            this.moreModeHeaderSentinel = new Control();
            this.rgbHeader = new HeadingLabel();
            this.hsvHeader = new HeadingLabel();
            this.alphaHeader = new HeadingLabel();
            this.swatchHeader = new PaintDotNet.Controls.SeparatorLine();
            this.swatchControl = new SwatchControl();
            this.colorDisplayWidget = new ColorDisplayWidget();
            this.toolStrip = new ToolStripEx();
            this.colorAddButton = new ToolStripButton();
            this.colorPalettesButton = new PdnToolStripSplitButton();
            this.hueGradientControl = new ColorGradientControl();
            this.saturationGradientControl = new ColorGradientControl();
            this.alphaGradientControl = new ColorGradientControl();
            this.redGradientControl = new ColorGradientControl();
            this.greenGradientControl = new ColorGradientControl();
            this.blueGradientControl = new ColorGradientControl();
            this.redUpDown.BeginInit();
            this.greenUpDown.BeginInit();
            this.blueUpDown.BeginInit();
            this.valueUpDown.BeginInit();
            this.saturationUpDown.BeginInit();
            this.hueUpDown.BeginInit();
            this.alphaUpDown.BeginInit();
            this.toolStrip.SuspendLayout();
            base.SuspendLayout();
            this.valueGradientControl.Count = 1;
            this.valueGradientControl.CustomGradient = null;
            this.valueGradientControl.DrawFarNub = true;
            this.valueGradientControl.DrawNearNub = false;
            this.valueGradientControl.Location = new Point(0xf3, 0xb9);
            this.valueGradientControl.MaxColor = Color.White;
            this.valueGradientControl.MinColor = Color.Black;
            this.valueGradientControl.Name = "valueGradientControl";
            this.valueGradientControl.Orientation = Orientation.Horizontal;
            this.valueGradientControl.Size = new Size(0x49, 0x13);
            this.valueGradientControl.TabIndex = 2;
            this.valueGradientControl.TabStop = false;
            this.valueGradientControl.Value = 0;
            this.valueGradientControl.ValueChanged += new IndexEventHandler(this.OnHsvGradientControlValueChanged);
            this.colorWheel.Location = new Point(0x36, 0x20);
            this.colorWheel.Name = "colorWheel";
            this.colorWheel.Size = new Size(0x95, 0x95);
            this.colorWheel.TabIndex = 3;
            this.colorWheel.TabStop = false;
            this.colorWheel.ColorChanged += new EventHandler(this.OnColorWheelColorChanged);
            this.redUpDown.Location = new Point(320, 0x18);
            int[] bits = new int[4];
            bits[0] = 0xff;
            this.redUpDown.Maximum = new decimal(bits);
            this.redUpDown.Name = "redUpDown";
            this.redUpDown.Size = new Size(0x38, 20);
            this.redUpDown.TabIndex = 2;
            this.redUpDown.TextAlign = HorizontalAlignment.Right;
            this.redUpDown.Enter += new EventHandler(this.OnUpDownEnter);
            this.redUpDown.ValueChanged += new EventHandler(this.OnUpDownValueChanged);
            this.redUpDown.Leave += new EventHandler(this.OnUpDownLeave);
            this.redUpDown.KeyUp += new KeyEventHandler(this.OnUpDownKeyUp);
            this.greenUpDown.Location = new Point(320, 0x30);
            int[] numArray2 = new int[4];
            numArray2[0] = 0xff;
            this.greenUpDown.Maximum = new decimal(numArray2);
            this.greenUpDown.Name = "greenUpDown";
            this.greenUpDown.Size = new Size(0x38, 20);
            this.greenUpDown.TabIndex = 3;
            this.greenUpDown.TextAlign = HorizontalAlignment.Right;
            this.greenUpDown.Enter += new EventHandler(this.OnUpDownEnter);
            this.greenUpDown.ValueChanged += new EventHandler(this.OnUpDownValueChanged);
            this.greenUpDown.Leave += new EventHandler(this.OnUpDownLeave);
            this.greenUpDown.KeyUp += new KeyEventHandler(this.OnUpDownKeyUp);
            this.blueUpDown.Location = new Point(320, 0x48);
            int[] numArray3 = new int[4];
            numArray3[0] = 0xff;
            this.blueUpDown.Maximum = new decimal(numArray3);
            this.blueUpDown.Name = "blueUpDown";
            this.blueUpDown.Size = new Size(0x38, 20);
            this.blueUpDown.TabIndex = 4;
            this.blueUpDown.TextAlign = HorizontalAlignment.Right;
            this.blueUpDown.Enter += new EventHandler(this.OnUpDownEnter);
            this.blueUpDown.ValueChanged += new EventHandler(this.OnUpDownValueChanged);
            this.blueUpDown.Leave += new EventHandler(this.OnUpDownLeave);
            this.blueUpDown.KeyUp += new KeyEventHandler(this.OnUpDownKeyUp);
            this.redLabel.AutoSize = true;
            this.redLabel.Location = new Point(0xde, 0x1c);
            this.redLabel.Name = "redLabel";
            this.redLabel.Size = new Size(15, 13);
            this.redLabel.TabIndex = 7;
            this.redLabel.Text = "R";
            this.redLabel.TextAlign = ContentAlignment.MiddleRight;
            this.blueLabel.AutoSize = true;
            this.blueLabel.Location = new Point(0xde, 0x4c);
            this.blueLabel.Name = "blueLabel";
            this.blueLabel.Size = new Size(14, 13);
            this.blueLabel.TabIndex = 8;
            this.blueLabel.Text = "B";
            this.blueLabel.TextAlign = ContentAlignment.MiddleLeft;
            this.greenLabel.AutoSize = true;
            this.greenLabel.Location = new Point(0xde, 0x34);
            this.greenLabel.Name = "greenLabel";
            this.greenLabel.Size = new Size(15, 13);
            this.greenLabel.TabIndex = 9;
            this.greenLabel.Text = "G";
            this.greenLabel.TextAlign = ContentAlignment.MiddleRight;
            this.saturationLabel.AutoSize = true;
            this.saturationLabel.Location = new Point(0xde, 0xa4);
            this.saturationLabel.Name = "saturationLabel";
            this.saturationLabel.Size = new Size(0x11, 13);
            this.saturationLabel.TabIndex = 0x10;
            this.saturationLabel.Text = "S:";
            this.saturationLabel.TextAlign = ContentAlignment.MiddleLeft;
            this.valueLabel.AutoSize = true;
            this.valueLabel.Location = new Point(0xde, 0xbc);
            this.valueLabel.Name = "valueLabel";
            this.valueLabel.Size = new Size(0x11, 13);
            this.valueLabel.TabIndex = 15;
            this.valueLabel.Text = "V:";
            this.valueLabel.TextAlign = ContentAlignment.MiddleLeft;
            this.hueLabel.AutoSize = true;
            this.hueLabel.Location = new Point(0xde, 140);
            this.hueLabel.Name = "hueLabel";
            this.hueLabel.Size = new Size(0x12, 13);
            this.hueLabel.TabIndex = 14;
            this.hueLabel.Text = "H:";
            this.hueLabel.TextAlign = ContentAlignment.MiddleLeft;
            this.valueUpDown.Location = new Point(320, 0xb8);
            this.valueUpDown.Name = "valueUpDown";
            this.valueUpDown.Size = new Size(0x38, 20);
            this.valueUpDown.TabIndex = 8;
            this.valueUpDown.TextAlign = HorizontalAlignment.Right;
            this.valueUpDown.Enter += new EventHandler(this.OnUpDownEnter);
            this.valueUpDown.ValueChanged += new EventHandler(this.OnUpDownValueChanged);
            this.valueUpDown.Leave += new EventHandler(this.OnUpDownLeave);
            this.valueUpDown.KeyUp += new KeyEventHandler(this.OnUpDownKeyUp);
            this.saturationUpDown.Location = new Point(320, 160);
            this.saturationUpDown.Name = "saturationUpDown";
            this.saturationUpDown.Size = new Size(0x38, 20);
            this.saturationUpDown.TabIndex = 7;
            this.saturationUpDown.TextAlign = HorizontalAlignment.Right;
            this.saturationUpDown.Enter += new EventHandler(this.OnUpDownEnter);
            this.saturationUpDown.ValueChanged += new EventHandler(this.OnUpDownValueChanged);
            this.saturationUpDown.Leave += new EventHandler(this.OnUpDownLeave);
            this.saturationUpDown.KeyUp += new KeyEventHandler(this.OnUpDownKeyUp);
            this.hueUpDown.Location = new Point(320, 0x88);
            int[] numArray4 = new int[4];
            numArray4[0] = 360;
            this.hueUpDown.Maximum = new decimal(numArray4);
            this.hueUpDown.Name = "hueUpDown";
            this.hueUpDown.Size = new Size(0x38, 20);
            this.hueUpDown.TabIndex = 6;
            this.hueUpDown.TextAlign = HorizontalAlignment.Right;
            this.hueUpDown.Enter += new EventHandler(this.OnUpDownEnter);
            this.hueUpDown.ValueChanged += new EventHandler(this.OnUpDownValueChanged);
            this.hueUpDown.Leave += new EventHandler(this.OnUpDownLeave);
            this.hueUpDown.KeyUp += new KeyEventHandler(this.OnUpDownKeyUp);
            this.hexBox.Location = new Point(320, 0x60);
            this.hexBox.Name = "hexBox";
            this.hexBox.Size = new Size(0x38, 20);
            this.hexBox.TabIndex = 5;
            this.hexBox.Text = "000000";
            this.hexBox.TextAlign = HorizontalAlignment.Right;
            this.hexBox.Enter += new EventHandler(this.OnHexUpDownEnter);
            this.hexBox.Leave += new EventHandler(this.OnHexUpDownLeave);
            this.hexBox.KeyUp += new KeyEventHandler(this.OnHexUpDownKeyUp);
            this.hexBox.TextChanged += new EventHandler(this.OnUpDownValueChanged);
            this.hexLabel.AutoSize = true;
            this.hexLabel.Location = new Point(0xde, 0x63);
            this.hexLabel.Name = "hexLabel";
            this.hexLabel.Size = new Size(0x1a, 13);
            this.hexLabel.TabIndex = 13;
            this.hexLabel.Text = "Hex";
            this.hexLabel.TextAlign = ContentAlignment.MiddleRight;
            this.whichUserColorBox.Location = new Point(8, 8);
            this.whichUserColorBox.Name = "whichUserColorBox";
            this.whichUserColorBox.Size = new Size(0x70, 0x15);
            this.whichUserColorBox.TabIndex = 0;
            this.whichUserColorBox.SelectedIndexChanged += new EventHandler(this.OnWhichUserColorBoxSelectedIndexChanged);
            this.alphaUpDown.Location = new Point(320, 0xe4);
            int[] numArray5 = new int[4];
            numArray5[0] = 0xff;
            this.alphaUpDown.Maximum = new decimal(numArray5);
            this.alphaUpDown.Name = "alphaUpDown";
            this.alphaUpDown.Size = new Size(0x38, 20);
            this.alphaUpDown.TabIndex = 10;
            this.alphaUpDown.TextAlign = HorizontalAlignment.Right;
            this.alphaUpDown.Enter += new EventHandler(this.OnUpDownEnter);
            this.alphaUpDown.ValueChanged += new EventHandler(this.OnUpDownValueChanged);
            this.alphaUpDown.Leave += new EventHandler(this.OnUpDownLeave);
            this.alphaUpDown.KeyUp += new KeyEventHandler(this.OnUpDownKeyUp);
            this.moreLessButton.Location = new Point(0x7e, 7);
            this.moreLessButton.Name = "moreLessButton";
            this.moreLessButton.Size = new Size(0x4b, 0x17);
            this.moreLessButton.TabIndex = 1;
            this.moreLessButton.Click += new EventHandler(this.OnMoreLessButtonClick);
            this.lessModeButtonSentinel.Location = new Point(0x80, 7);
            this.lessModeButtonSentinel.Name = "lessModeButtonSentinel";
            this.lessModeButtonSentinel.Size = new Size(0, 0);
            this.lessModeButtonSentinel.TabIndex = 0x16;
            this.lessModeButtonSentinel.Text = "we put the lessMore control here when in \"Less\" mode";
            this.lessModeButtonSentinel.Visible = false;
            this.moreModeButtonSentinel.Location = new Point(0xa5, 7);
            this.moreModeButtonSentinel.Name = "moreModeButtonSentinel";
            this.moreModeButtonSentinel.Size = new Size(0, 0);
            this.moreModeButtonSentinel.TabIndex = 0x17;
            this.moreModeButtonSentinel.Visible = false;
            this.lessModeHeaderSentinel.Location = new Point(8, 40);
            this.lessModeHeaderSentinel.Name = "lessModeHeaderSentinel";
            this.lessModeHeaderSentinel.Size = new Size(0xc3, 0xb9);
            this.lessModeHeaderSentinel.TabIndex = 0x18;
            this.lessModeHeaderSentinel.Visible = false;
            this.moreModeHeaderSentinel.Location = new Point(8, 40);
            this.moreModeHeaderSentinel.Name = "moreModeHeaderSentinel";
            this.moreModeHeaderSentinel.Size = new Size(0xe8, 0xd8);
            this.moreModeHeaderSentinel.TabIndex = 0x19;
            this.moreModeHeaderSentinel.TabStop = false;
            this.moreModeHeaderSentinel.Visible = false;
            this.rgbHeader.Location = new Point(0xde, 8);
            this.rgbHeader.Name = "rgbHeader";
            this.rgbHeader.RightMargin = 0;
            this.rgbHeader.Size = new Size(0x9a, 14);
            this.rgbHeader.TabIndex = 0x1b;
            this.rgbHeader.TabStop = false;
            this.hsvHeader.Location = new Point(0xde, 120);
            this.hsvHeader.Name = "hsvHeader";
            this.hsvHeader.RightMargin = 0;
            this.hsvHeader.Size = new Size(0x9a, 14);
            this.hsvHeader.TabIndex = 0x1c;
            this.hsvHeader.TabStop = false;
            this.alphaHeader.Location = new Point(0xde, 0xd4);
            this.alphaHeader.Name = "alphaHeader";
            this.alphaHeader.RightMargin = 0;
            this.alphaHeader.Size = new Size(0x9a, 14);
            this.alphaHeader.TabIndex = 0x1d;
            this.alphaHeader.TabStop = false;
            this.swatchHeader.Location = new Point(8, 0xb5);
            this.swatchHeader.Name = "swatchHeader";
            this.swatchHeader.Size = new Size(0xc1, 8);
            this.swatchHeader.TabIndex = 30;
            this.swatchHeader.TabStop = false;
            this.swatchControl.BlinkHighlight = false;
            this.swatchControl.Colors = Array.Empty<ColorBgra>();
            this.swatchControl.Location = new Point(8, 0xbd);
            this.swatchControl.Name = "swatchControl";
            this.swatchControl.Size = new Size(0xc0, 0x4a);
            this.swatchControl.TabIndex = 0x1f;
            this.swatchControl.Text = "swatchControl1";
            this.swatchControl.ColorsChanged += new EventHandler(this.OnSwatchControlColorsChanged);
            this.swatchControl.ColorClicked += new ValueEventHandler<Tuple<int, MouseButtons>>(this.OnSwatchControlColorClicked);
            this.colorDisplayWidget.Location = new Point(4, 0x20);
            this.colorDisplayWidget.Name = "colorDisplayWidget";
            this.colorDisplayWidget.Size = new Size(0x34, 0x34);
            this.colorDisplayWidget.TabIndex = 0x20;
            this.colorDisplayWidget.BlackAndWhiteButtonClicked += new EventHandler(this.OnColorDisplayWidgetBlackAndWhiteButtonClicked);
            this.colorDisplayWidget.SwapColorsClicked += new EventHandler(this.OnColorDisplayWidgetSwapColorsClicked);
            this.colorDisplayWidget.UserPrimaryColorClick += new EventHandler(this.OnColorDisplayPrimaryColorClicked);
            this.colorDisplayWidget.UserSecondaryColorClick += new EventHandler(this.OnColorDisplaySecondaryColorClicked);
            this.toolStrip.ClickThrough = true;
            this.toolStrip.Dock = DockStyle.None;
            this.toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            ToolStripItem[] toolStripItems = new ToolStripItem[] { this.colorAddButton, this.colorPalettesButton };
            this.toolStrip.Items.AddRange(toolStripItems);
            this.toolStrip.Location = new Point(5, 0x9d);
            this.toolStrip.ManagedFocus = true;
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new Size(0x41, 0x19);
            this.toolStrip.TabIndex = 0x21;
            this.toolStrip.Text = "toolStrip";
            this.toolStrip.RelinquishFocus += (s, e) => this.OnRelinquishFocus();
            this.colorAddButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.colorAddButton.Name = "colorAddButton";
            this.colorAddButton.Size = new Size(0x17, 0x16);
            this.colorAddButton.Text = "colorAddButton";
            this.colorAddButton.Click += new EventHandler(this.OnColorAddButtonClick);
            this.colorPalettesButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.colorPalettesButton.Name = "colorPalettesButton";
            this.colorPalettesButton.Size = new Size(0x10, 0x16);
            this.colorPalettesButton.Click += new EventHandler(this.OnColorPalettesButtonClick);
            this.colorPalettesButton.DropDownOpening += new EventHandler(this.OnColorPalettesButtonDropDownOpening);
            this.hueGradientControl.Count = 1;
            this.hueGradientControl.CustomGradient = null;
            this.hueGradientControl.DrawFarNub = true;
            this.hueGradientControl.DrawNearNub = false;
            this.hueGradientControl.Location = new Point(0xf3, 0x89);
            this.hueGradientControl.MaxColor = Color.White;
            this.hueGradientControl.MinColor = Color.Black;
            this.hueGradientControl.Name = "hueGradientControl";
            this.hueGradientControl.Orientation = Orientation.Horizontal;
            this.hueGradientControl.Size = new Size(0x49, 0x13);
            this.hueGradientControl.TabIndex = 0x22;
            this.hueGradientControl.TabStop = false;
            this.hueGradientControl.Value = 0;
            this.hueGradientControl.ValueChanged += new IndexEventHandler(this.OnHsvGradientControlValueChanged);
            this.saturationGradientControl.Count = 1;
            this.saturationGradientControl.CustomGradient = null;
            this.saturationGradientControl.DrawFarNub = true;
            this.saturationGradientControl.DrawNearNub = false;
            this.saturationGradientControl.Location = new Point(0xf3, 0xa1);
            this.saturationGradientControl.MaxColor = Color.White;
            this.saturationGradientControl.MinColor = Color.Black;
            this.saturationGradientControl.Name = "saturationGradientControl";
            this.saturationGradientControl.Orientation = Orientation.Horizontal;
            this.saturationGradientControl.Size = new Size(0x49, 0x13);
            this.saturationGradientControl.TabIndex = 0x23;
            this.saturationGradientControl.TabStop = false;
            this.saturationGradientControl.Value = 0;
            this.saturationGradientControl.ValueChanged += new IndexEventHandler(this.OnHsvGradientControlValueChanged);
            this.alphaGradientControl.Count = 1;
            this.alphaGradientControl.CustomGradient = null;
            this.alphaGradientControl.DrawFarNub = true;
            this.alphaGradientControl.DrawNearNub = false;
            this.alphaGradientControl.Location = new Point(0xf3, 0xe5);
            this.alphaGradientControl.MaxColor = Color.White;
            this.alphaGradientControl.MinColor = Color.Black;
            this.alphaGradientControl.Name = "alphaGradientControl";
            this.alphaGradientControl.Orientation = Orientation.Horizontal;
            this.alphaGradientControl.Size = new Size(0x49, 0x13);
            this.alphaGradientControl.TabIndex = 0x24;
            this.alphaGradientControl.TabStop = false;
            this.alphaGradientControl.Value = 0;
            this.alphaGradientControl.ValueChanged += new IndexEventHandler(this.OnAlphaGradientControlValueChanged);
            this.redGradientControl.Count = 1;
            this.redGradientControl.CustomGradient = null;
            this.redGradientControl.DrawFarNub = true;
            this.redGradientControl.DrawNearNub = false;
            this.redGradientControl.Location = new Point(0xf3, 0x19);
            this.redGradientControl.MaxColor = Color.White;
            this.redGradientControl.MinColor = Color.Black;
            this.redGradientControl.Name = "redGradientControl";
            this.redGradientControl.Orientation = Orientation.Horizontal;
            this.redGradientControl.Size = new Size(0x49, 0x13);
            this.redGradientControl.TabIndex = 0x25;
            this.redGradientControl.TabStop = false;
            this.redGradientControl.Value = 0;
            this.redGradientControl.ValueChanged += new IndexEventHandler(this.OnRgbGradientControlValueChanged);
            this.greenGradientControl.Count = 1;
            this.greenGradientControl.CustomGradient = null;
            this.greenGradientControl.DrawFarNub = true;
            this.greenGradientControl.DrawNearNub = false;
            this.greenGradientControl.Location = new Point(0xf3, 0x31);
            this.greenGradientControl.MaxColor = Color.White;
            this.greenGradientControl.MinColor = Color.Black;
            this.greenGradientControl.Name = "greenGradientControl";
            this.greenGradientControl.Orientation = Orientation.Horizontal;
            this.greenGradientControl.Size = new Size(0x49, 0x13);
            this.greenGradientControl.TabIndex = 0x26;
            this.greenGradientControl.TabStop = false;
            this.greenGradientControl.Value = 0;
            this.greenGradientControl.ValueChanged += new IndexEventHandler(this.OnRgbGradientControlValueChanged);
            this.blueGradientControl.Count = 1;
            this.blueGradientControl.CustomGradient = null;
            this.blueGradientControl.DrawFarNub = true;
            this.blueGradientControl.DrawNearNub = false;
            this.blueGradientControl.Location = new Point(0xf3, 0x49);
            this.blueGradientControl.MaxColor = Color.White;
            this.blueGradientControl.MinColor = Color.Black;
            this.blueGradientControl.Name = "blueGradientControl";
            this.blueGradientControl.Orientation = Orientation.Horizontal;
            this.blueGradientControl.Size = new Size(0x49, 0x13);
            this.blueGradientControl.TabIndex = 0x27;
            this.blueGradientControl.TabStop = false;
            this.blueGradientControl.Value = 0;
            this.blueGradientControl.ValueChanged += new IndexEventHandler(this.OnRgbGradientControlValueChanged);
            base.AutoScaleDimensions = new SizeF(96f, 96f);
            base.AutoScaleMode = AutoScaleMode.Dpi;
            base.ClientSize = new Size(0x182, 0x10a);
            base.Controls.Add(this.valueLabel);
            base.Controls.Add(this.saturationLabel);
            base.Controls.Add(this.hueLabel);
            base.Controls.Add(this.greenLabel);
            base.Controls.Add(this.blueLabel);
            base.Controls.Add(this.redLabel);
            base.Controls.Add(this.hexLabel);
            base.Controls.Add(this.blueGradientControl);
            base.Controls.Add(this.greenGradientControl);
            base.Controls.Add(this.redGradientControl);
            base.Controls.Add(this.alphaGradientControl);
            base.Controls.Add(this.saturationGradientControl);
            base.Controls.Add(this.hueGradientControl);
            base.Controls.Add(this.toolStrip);
            base.Controls.Add(this.colorWheel);
            base.Controls.Add(this.colorDisplayWidget);
            base.Controls.Add(this.swatchControl);
            base.Controls.Add(this.swatchHeader);
            base.Controls.Add(this.alphaHeader);
            base.Controls.Add(this.hsvHeader);
            base.Controls.Add(this.rgbHeader);
            base.Controls.Add(this.valueGradientControl);
            base.Controls.Add(this.moreModeButtonSentinel);
            base.Controls.Add(this.lessModeButtonSentinel);
            base.Controls.Add(this.moreLessButton);
            base.Controls.Add(this.whichUserColorBox);
            base.Controls.Add(this.lessModeHeaderSentinel);
            base.Controls.Add(this.moreModeHeaderSentinel);
            base.Controls.Add(this.blueUpDown);
            base.Controls.Add(this.greenUpDown);
            base.Controls.Add(this.redUpDown);
            base.Controls.Add(this.hexBox);
            base.Controls.Add(this.hueUpDown);
            base.Controls.Add(this.saturationUpDown);
            base.Controls.Add(this.valueUpDown);
            base.Controls.Add(this.alphaUpDown);
            base.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            base.Name = "ColorsForm";
            base.Controls.SetChildIndex(this.alphaUpDown, 0);
            base.Controls.SetChildIndex(this.valueUpDown, 0);
            base.Controls.SetChildIndex(this.saturationUpDown, 0);
            base.Controls.SetChildIndex(this.hueUpDown, 0);
            base.Controls.SetChildIndex(this.hexBox, 0);
            base.Controls.SetChildIndex(this.redUpDown, 0);
            base.Controls.SetChildIndex(this.greenUpDown, 0);
            base.Controls.SetChildIndex(this.blueUpDown, 0);
            base.Controls.SetChildIndex(this.moreModeHeaderSentinel, 0);
            base.Controls.SetChildIndex(this.lessModeHeaderSentinel, 0);
            base.Controls.SetChildIndex(this.whichUserColorBox, 0);
            base.Controls.SetChildIndex(this.moreLessButton, 0);
            base.Controls.SetChildIndex(this.lessModeButtonSentinel, 0);
            base.Controls.SetChildIndex(this.moreModeButtonSentinel, 0);
            base.Controls.SetChildIndex(this.valueGradientControl, 0);
            base.Controls.SetChildIndex(this.rgbHeader, 0);
            base.Controls.SetChildIndex(this.hsvHeader, 0);
            base.Controls.SetChildIndex(this.alphaHeader, 0);
            base.Controls.SetChildIndex(this.swatchControl, 0);
            base.Controls.SetChildIndex(this.colorDisplayWidget, 0);
            base.Controls.SetChildIndex(this.colorWheel, 0);
            base.Controls.SetChildIndex(this.swatchHeader, 0);
            base.Controls.SetChildIndex(this.toolStrip, 0);
            base.Controls.SetChildIndex(this.hueGradientControl, 0);
            base.Controls.SetChildIndex(this.saturationGradientControl, 0);
            base.Controls.SetChildIndex(this.alphaGradientControl, 0);
            base.Controls.SetChildIndex(this.redGradientControl, 0);
            base.Controls.SetChildIndex(this.greenGradientControl, 0);
            base.Controls.SetChildIndex(this.blueGradientControl, 0);
            base.Controls.SetChildIndex(this.hexLabel, 0);
            base.Controls.SetChildIndex(this.redLabel, 0);
            base.Controls.SetChildIndex(this.blueLabel, 0);
            base.Controls.SetChildIndex(this.greenLabel, 0);
            base.Controls.SetChildIndex(this.hueLabel, 0);
            base.Controls.SetChildIndex(this.saturationLabel, 0);
            base.Controls.SetChildIndex(this.valueLabel, 0);
            this.redUpDown.EndInit();
            this.greenUpDown.EndInit();
            this.blueUpDown.EndInit();
            this.valueUpDown.EndInit();
            this.saturationUpDown.EndInit();
            this.hueUpDown.EndInit();
            this.alphaUpDown.EndInit();
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void OnAlphaGradientControlValueChanged(object sender, EventArgs e)
        {
            this.OnUpDownValueChanged(sender, e);
        }

        private void OnColorAddButtonClick(object sender, EventArgs e)
        {
            if (this.colorAddButton.Checked)
            {
                this.colorAddButton.Checked = false;
                this.swatchControl.BlinkHighlight = false;
            }
            else
            {
                this.colorAddButton.Checked = true;
                this.swatchControl.BlinkHighlight = true;
            }
        }

        private void OnColorDisplayPrimaryColorClicked(object sender, EventArgs e)
        {
            this.WhichUserColor = PaintDotNet.WhichUserColor.Primary;
            this.OnRelinquishFocus();
        }

        private void OnColorDisplaySecondaryColorClicked(object sender, EventArgs e)
        {
            this.WhichUserColor = PaintDotNet.WhichUserColor.Secondary;
            this.OnRelinquishFocus();
        }

        private void OnColorDisplayWidgetBlackAndWhiteButtonClicked(object sender, EventArgs e)
        {
            this.SetUserColorsToBlackAndWhite();
            this.OnRelinquishFocus();
        }

        private void OnColorDisplayWidgetSwapColorsClicked(object sender, EventArgs e)
        {
            this.SwapUserColors();
            this.OnRelinquishFocus();
        }

        private void OnColorPalettesButtonClick(object sender, EventArgs e)
        {
            this.colorPalettesButton.ShowDropDown();
        }

        private void OnColorPalettesButtonDropDownOpening(object sender, EventArgs e)
        {
            this.colorPalettesButton.DropDownItems.Clear();
            using (new WaitCursorChanger(this))
            {
                UserPalettesService.Instance.Load();
            }
            IReadOnlyList<string> paletteNames = UserPalettesService.Instance.PaletteNames;
            foreach (string str in paletteNames)
            {
                this.colorPalettesButton.DropDownItems.Add(str, PdnResources.GetImageResource("Icons.SwatchIcon.png").Reference, new EventHandler(this.OnPaletteClickedHandler));
            }
            if (paletteNames.Any<string>())
            {
                this.colorPalettesButton.DropDownItems.Add(new ToolStripSeparator());
            }
            this.colorPalettesButton.DropDownItems.Add(PdnResources.GetString("ColorsForm.ColorPalettesButton.SaveCurrentPaletteAs.Text"), PdnResources.GetImageResource("Icons.SavePaletteIcon.png").Reference, new EventHandler(this.OnSavePaletteAsHandler));
            this.colorPalettesButton.DropDownItems.Add(PdnResources.GetString("ColorsForm.ColorPalettesButton.OpenPalettesFolder.Text"), PdnResources.GetImageResource("Icons.ColorPalettes.png").Reference, new EventHandler(this.OnOpenPalettesFolderClickedHandler));
            this.colorPalettesButton.DropDownItems.Add(new ToolStripSeparator());
            this.colorPalettesButton.DropDownItems.Add(PdnResources.GetString("ColorsForm.ColorPalettesButton.ResetToDefaultPalette.Text"), null, new EventHandler(this.OnResetPaletteHandler));
        }

        private void OnColorWheelColorChanged(object sender, EventArgs e)
        {
            if (this.IgnoreChangedEvents)
            {
                return;
            }
            this.PushIgnoreChangedEvents();
            Int32HsvColor hsvColor = this.colorWheel.HsvColor;
            Int32RgbColor color2 = hsvColor.ToRgb();
            ColorBgra newColor = ColorBgra.FromBgra((byte) color2.Blue, (byte) color2.Green, (byte) color2.Red, (byte) this.alphaUpDown.Value);
            NumericUpDownUtil.SetValueIfDifferent(this.hueUpDown, hsvColor.Hue);
            NumericUpDownUtil.SetValueIfDifferent(this.saturationUpDown, hsvColor.Saturation);
            NumericUpDownUtil.SetValueIfDifferent(this.valueUpDown, hsvColor.Value);
            NumericUpDownUtil.SetValueIfDifferent(this.redUpDown, (int) newColor.R);
            NumericUpDownUtil.SetValueIfDifferent(this.greenUpDown, (int) newColor.G);
            NumericUpDownUtil.SetValueIfDifferent(this.blueUpDown, (int) newColor.B);
            string str = this.GetHexNumericUpDownValue(newColor.R, newColor.G, newColor.B);
            this.hexBox.Text = str;
            NumericUpDownUtil.SetValueIfDifferent(this.alphaUpDown, (int) newColor.A);
            this.SetColorGradientValuesHsv(hsvColor.Hue, hsvColor.Saturation, hsvColor.Value);
            this.SetColorGradientMinMaxColorsHsv(hsvColor.Hue, hsvColor.Saturation, hsvColor.Value);
            this.SetColorGradientValuesRgb(newColor.R, newColor.G, newColor.B);
            this.SetColorGradientMinMaxColorsRgb(newColor.R, newColor.G, newColor.B);
            this.SetColorGradientMinMaxColorsAlpha(newColor.A);
            PaintDotNet.WhichUserColor whichUserColor = this.WhichUserColor;
            if (whichUserColor != PaintDotNet.WhichUserColor.Primary)
            {
                if (whichUserColor != PaintDotNet.WhichUserColor.Secondary)
                {
                    throw ExceptionUtil.InvalidEnumArgumentException<PaintDotNet.WhichUserColor>(this.WhichUserColor, "this.WhichUserColor");
                }
            }
            else
            {
                this.userPrimaryColor = newColor;
                this.OnUserPrimaryColorChanged(newColor);
                this.OnRelinquishFocus();
                goto Label_01B2;
            }
            this.userSecondaryColor = newColor;
            this.OnUserSecondaryColorChanged(newColor);
            this.OnRelinquishFocus();
        Label_01B2:
            this.PopIgnoreChangedEvents();
            this.QueueUpdate();
        }

        private void OnHexUpDownEnter(object sender, EventArgs e)
        {
            TextBox box = (TextBox) sender;
            box.Select(0, box.Text.Length);
        }

        private void OnHexUpDownKeyUp(object sender, KeyEventArgs e)
        {
            TextBox box = (TextBox) sender;
            if (this.CheckHexBox(box.Text))
            {
                this.OnUpDownValueChanged(sender, e);
            }
        }

        private void OnHexUpDownLeave(object sender, EventArgs e)
        {
            this.hexBox.Text = this.hexBox.Text.ToUpper();
            this.OnUpDownValueChanged(sender, e);
        }

        private void OnHsvGradientControlValueChanged(object sender, IndexEventArgs e)
        {
            int num;
            int num2;
            int num3;
            if (this.IgnoreChangedEvents)
            {
                return;
            }
            if (sender == this.hueGradientControl)
            {
                num = (this.hueGradientControl.Value * 360) / 0xff;
            }
            else
            {
                num = (int) this.hueUpDown.Value;
            }
            if (sender == this.saturationGradientControl)
            {
                num2 = (this.saturationGradientControl.Value * 100) / 0xff;
            }
            else
            {
                num2 = (int) this.saturationUpDown.Value;
            }
            if (sender == this.valueGradientControl)
            {
                num3 = (this.valueGradientControl.Value * 100) / 0xff;
            }
            else
            {
                num3 = (int) this.valueUpDown.Value;
            }
            Int32HsvColor color = new Int32HsvColor(num, num2, num3);
            this.colorWheel.HsvColor = color;
            Int32RgbColor color2 = color.ToRgb();
            ColorBgra bgra = ColorBgra.FromBgra((byte) color2.Blue, (byte) color2.Green, (byte) color2.Red, (byte) this.alphaUpDown.Value);
            NumericUpDownUtil.SetValueIfDifferent(this.hueUpDown, color.Hue);
            NumericUpDownUtil.SetValueIfDifferent(this.saturationUpDown, color.Saturation);
            NumericUpDownUtil.SetValueIfDifferent(this.valueUpDown, color.Value);
            NumericUpDownUtil.SetValueIfDifferent(this.redUpDown, color2.Red);
            NumericUpDownUtil.SetValueIfDifferent(this.greenUpDown, color2.Green);
            NumericUpDownUtil.SetValueIfDifferent(this.blueUpDown, color2.Blue);
            string str = this.GetHexNumericUpDownValue(color2.Red, color2.Green, color2.Blue);
            this.hexBox.Text = str;
            PaintDotNet.WhichUserColor whichUserColor = this.WhichUserColor;
            if (whichUserColor != PaintDotNet.WhichUserColor.Primary)
            {
                if (whichUserColor != PaintDotNet.WhichUserColor.Secondary)
                {
                    throw ExceptionUtil.InvalidEnumArgumentException<PaintDotNet.WhichUserColor>(this.WhichUserColor, "this.WhichUserColor");
                }
            }
            else
            {
                this.UserPrimaryColor = bgra;
                this.OnRelinquishFocus();
                goto Label_01C7;
            }
            this.UserSecondaryColor = bgra;
            this.OnRelinquishFocus();
        Label_01C7:
            this.QueueUpdate();
        }

        protected override void OnLoad(EventArgs e)
        {
            this.inMoreState = true;
            this.moreLessButton.PerformClick();
            base.OnLoad(e);
        }

        private void OnMoreLessButtonClick(object sender, EventArgs e)
        {
            this.OnRelinquishFocus();
            base.SuspendLayout();
            if (this.inMoreState)
            {
                this.inMoreState = false;
                Size lessSize = this.lessSize;
                this.moreLessButton.Text = this.moreText;
                int num = this.moreModeHeaderSentinel.Height - this.lessModeHeaderSentinel.Height;
                lessSize.Height -= num;
                lessSize.Height -= UIUtil.ScaleHeight(0x12);
                base.ClientSize = lessSize;
            }
            else
            {
                this.inMoreState = true;
                this.moreLessButton.Text = this.lessText;
                base.ClientSize = this.moreSize;
            }
            this.swatchControl.Height = (base.ClientSize.Height - UIUtil.ScaleHeight(4)) - this.swatchControl.Top;
            base.ResumeLayout(false);
        }

        private void OnOpenPalettesFolderClickedHandler(object sender, EventArgs e)
        {
            try
            {
                UserPalettesService.Instance.EnsurePalettesPathExists();
            }
            catch (Exception)
            {
            }
            try
            {
                using (new WaitCursorChanger(this))
                {
                    ShellUtil.BrowseFolder2(this, UserPalettesService.Instance.PalettesPath);
                }
            }
            catch (Exception exception)
            {
                ExceptionDialog.ShowErrorDialog(this, exception);
            }
        }

        private void OnPaletteClickedHandler(object sender, EventArgs e)
        {
            ToolStripItem item = sender as ToolStripItem;
            if (item != null)
            {
                ColorBgra[] bgraArray = UserPalettesService.Instance.Get(item.Text);
                if (bgraArray != null)
                {
                    this.swatchControl.Colors = bgraArray;
                }
            }
        }

        private void OnResetPaletteHandler(object sender, EventArgs e)
        {
            this.swatchControl.Colors = UserPalettesService.Instance.DefaultPalette.ToArrayEx<ColorBgra>();
        }

        private void OnRgbGradientControlValueChanged(object sender, IndexEventArgs ce)
        {
            int num;
            int num2;
            int num3;
            int num4;
            if (this.IgnoreChangedEvents)
            {
                return;
            }
            if (sender == this.redGradientControl)
            {
                num = this.redGradientControl.Value;
            }
            else
            {
                num = (int) this.redUpDown.Value;
            }
            if (sender == this.greenGradientControl)
            {
                num2 = this.greenGradientControl.Value;
            }
            else
            {
                num2 = (int) this.greenUpDown.Value;
            }
            if (sender == this.blueGradientControl)
            {
                num3 = this.blueGradientControl.Value;
            }
            else
            {
                num3 = (int) this.blueUpDown.Value;
            }
            if (sender == this.alphaGradientControl)
            {
                num4 = this.alphaGradientControl.Value;
            }
            else
            {
                num4 = (int) this.alphaUpDown.Value;
            }
            Color color = Color.FromArgb(num4, num, num2, num3);
            Int32HsvColor color2 = Int32HsvColor.FromGdipColor(color);
            this.PushIgnoreChangedEvents();
            NumericUpDownUtil.SetValueIfDifferent(this.hueUpDown, color2.Hue);
            NumericUpDownUtil.SetValueIfDifferent(this.saturationUpDown, color2.Saturation);
            NumericUpDownUtil.SetValueIfDifferent(this.valueUpDown, color2.Value);
            NumericUpDownUtil.SetValueIfDifferent(this.redUpDown, (int) color.R);
            NumericUpDownUtil.SetValueIfDifferent(this.greenUpDown, (int) color.G);
            NumericUpDownUtil.SetValueIfDifferent(this.blueUpDown, (int) color.B);
            this.PopIgnoreChangedEvents();
            NumericUpDownUtil.SetValueIfDifferent(this.alphaUpDown, (int) color.A);
            string str = this.GetHexNumericUpDownValue(color.R, color.G, color.B);
            this.hexBox.Text = str;
            ColorBgra bgra = ColorBgra.FromColor(color);
            PaintDotNet.WhichUserColor whichUserColor = this.WhichUserColor;
            if (whichUserColor != PaintDotNet.WhichUserColor.Primary)
            {
                if (whichUserColor != PaintDotNet.WhichUserColor.Secondary)
                {
                    throw ExceptionUtil.InvalidEnumArgumentException<PaintDotNet.WhichUserColor>(this.WhichUserColor, "this.WhichUserColor");
                }
            }
            else
            {
                this.UserPrimaryColor = bgra;
                this.OnRelinquishFocus();
                goto Label_01BE;
            }
            this.UserSecondaryColor = bgra;
            this.OnRelinquishFocus();
        Label_01BE:
            this.QueueUpdate();
        }

        private void OnSavePaletteAsHandler(object sender, EventArgs e)
        {
            using (SavePaletteDialog dialog = new SavePaletteDialog())
            {
                dialog.PaletteNames = UserPalettesService.Instance.PaletteNames.ToArrayEx<string>();
                dialog.ShowDialog(this);
                if (dialog.DialogResult == DialogResult.OK)
                {
                    UserPalettesService.Instance.AddOrUpdate(dialog.PaletteName, this.swatchControl.Colors);
                    WaitCursorChanger changer = new WaitCursorChanger(this);
                    try
                    {
                        UserPalettesService.Instance.Save();
                    }
                    catch (Exception exception)
                    {
                        ExceptionDialog.ShowErrorDialog(this, exception);
                    }
                    finally
                    {
                        if (changer != null)
                        {
                            changer.Dispose();
                        }
                    }
                }
            }
        }

        private void OnSwatchControlColorClicked(object sender, ValueEventArgs<Tuple<int, MouseButtons>> e)
        {
            SegmentedList<ColorBgra> list = new SegmentedList<ColorBgra>(this.swatchControl.Colors, 7);
            if (this.colorAddButton.Checked)
            {
                list[e.Value.Item1] = this.GetColorFromUpDowns();
                this.swatchControl.Colors = list;
                this.colorAddButton.Checked = false;
                this.swatchControl.BlinkHighlight = false;
            }
            else
            {
                ColorBgra secondary = list[e.Value.Item1];
                if (((MouseButtons) e.Value.Item2) == MouseButtons.Right)
                {
                    this.SetUserColors(this.UserPrimaryColor, secondary);
                }
                else
                {
                    PaintDotNet.WhichUserColor whichUserColor = this.WhichUserColor;
                    if (whichUserColor != PaintDotNet.WhichUserColor.Primary)
                    {
                        if (whichUserColor != PaintDotNet.WhichUserColor.Secondary)
                        {
                            throw ExceptionUtil.InvalidEnumArgumentException<PaintDotNet.WhichUserColor>(this.WhichUserColor, "this.WhichUserColor");
                        }
                    }
                    else
                    {
                        this.UserPrimaryColor = secondary;
                        goto Label_00C2;
                    }
                    this.UserSecondaryColor = secondary;
                }
            }
        Label_00C2:
            this.OnRelinquishFocus();
        }

        private void OnSwatchControlColorsChanged(object sender, EventArgs e)
        {
            string paletteSaveString = UserPalettesService.Instance.GetPaletteSaveString(this.swatchControl.Colors);
            AppSettings.Instance.Workspace.CurrentPalette.Value = paletteSaveString;
        }

        private void OnUpDownEnter(object sender, EventArgs e)
        {
            NumericUpDown down = (NumericUpDown) sender;
            down.Select(0, down.Text.Length);
        }

        private void OnUpDownKeyUp(object sender, KeyEventArgs e)
        {
            NumericUpDown upDown = (NumericUpDown) sender;
            if (NumericUpDownUtil.CheckTextAsInt32Value(upDown))
            {
                this.OnUpDownValueChanged(sender, e);
            }
        }

        private void OnUpDownLeave(object sender, EventArgs e)
        {
            this.OnUpDownValueChanged(sender, e);
        }

        private void OnUpDownValueChanged(object sender, EventArgs e)
        {
            if ((sender != this.alphaUpDown) && (sender != this.alphaGradientControl))
            {
                if (!this.IgnoreChangedEvents)
                {
                    this.PushIgnoreChangedEvents();
                    if (((sender == this.redUpDown) || (sender == this.greenUpDown)) || (sender == this.blueUpDown))
                    {
                        string str = this.GetHexNumericUpDownValue((int) this.redUpDown.Value, (int) this.greenUpDown.Value, (int) this.blueUpDown.Value);
                        this.hexBox.Text = str;
                        ColorBgra bgra3 = ColorBgra.FromBgra((byte) this.blueUpDown.Value, (byte) this.greenUpDown.Value, (byte) this.redUpDown.Value, (byte) this.alphaUpDown.Value);
                        this.SetColorGradientMinMaxColorsRgb(bgra3.R, bgra3.G, bgra3.B);
                        this.SetColorGradientMinMaxColorsAlpha(bgra3.A);
                        this.SetColorGradientValuesRgb(bgra3.R, bgra3.G, bgra3.B);
                        this.SetColorGradientMinMaxColorsAlpha(bgra3.A);
                        this.SyncHsvFromRgb(bgra3);
                        this.OnUserColorChanged(bgra3);
                    }
                    else if (sender == this.hexBox)
                    {
                        int num = 0;
                        if (this.hexBox.Text.Length > 0)
                        {
                            string text = this.hexBox.Text;
                            if (text.StartsWith("#"))
                            {
                                text = text.Substring(1);
                            }
                            try
                            {
                                num = int.Parse(text, NumberStyles.HexNumber);
                            }
                            catch (FormatException)
                            {
                                num = 0;
                                this.hexBox.Text = "";
                            }
                            catch (OverflowException)
                            {
                                num = 0xffffff;
                                this.hexBox.Text = "FFFFFF";
                            }
                            if ((num > 0xffffff) || (num < 0))
                            {
                                num = 0xffffff;
                                this.hexBox.Text = "FFFFFF";
                            }
                        }
                        int newValue = (num & 0xff0000) >> 0x10;
                        int num3 = (num & 0xff00) >> 8;
                        int num4 = num & 0xff;
                        NumericUpDownUtil.SetValueIfDifferent(this.redUpDown, newValue);
                        NumericUpDownUtil.SetValueIfDifferent(this.greenUpDown, num3);
                        NumericUpDownUtil.SetValueIfDifferent(this.blueUpDown, num4);
                        this.SetColorGradientMinMaxColorsRgb(newValue, num3, num4);
                        this.SetColorGradientValuesRgb(newValue, num3, num4);
                        this.SetColorGradientMinMaxColorsAlpha((int) this.alphaUpDown.Value);
                        ColorBgra bgra4 = ColorBgra.FromBgra((byte) num4, (byte) num3, (byte) newValue, (byte) this.alphaUpDown.Value);
                        this.SyncHsvFromRgb(bgra4);
                        this.OnUserColorChanged(bgra4);
                    }
                    else if (((sender == this.hueUpDown) || (sender == this.saturationUpDown)) || (sender == this.valueUpDown))
                    {
                        Int32HsvColor hsvColor = this.colorWheel.HsvColor;
                        Int32HsvColor color3 = new Int32HsvColor((int) this.hueUpDown.Value, (int) this.saturationUpDown.Value, (int) this.valueUpDown.Value);
                        if (hsvColor != color3)
                        {
                            this.colorWheel.HsvColor = color3;
                            this.SetColorGradientValuesHsv(color3.Hue, color3.Saturation, color3.Value);
                            this.SetColorGradientMinMaxColorsHsv(color3.Hue, color3.Saturation, color3.Value);
                            this.SyncRgbFromHsv(color3);
                            Int32RgbColor color4 = color3.ToRgb();
                            this.OnUserColorChanged(ColorBgra.FromBgra((byte) color4.Blue, (byte) color4.Green, (byte) color4.Red, (byte) this.alphaUpDown.Value));
                        }
                    }
                    this.PopIgnoreChangedEvents();
                }
                return;
            }
            bool ignoreChangedEvents = this.IgnoreChangedEvents;
            this.PushIgnoreChangedEvents();
            if (sender == this.alphaGradientControl)
            {
                if (this.alphaUpDown.Value != this.alphaGradientControl.Value)
                {
                    this.alphaUpDown.Value = this.alphaGradientControl.Value;
                }
            }
            else if (this.alphaGradientControl.Value != ((int) this.alphaUpDown.Value))
            {
                this.alphaGradientControl.Value = (int) this.alphaUpDown.Value;
            }
            PaintDotNet.WhichUserColor whichUserColor = this.WhichUserColor;
            if (whichUserColor != PaintDotNet.WhichUserColor.Primary)
            {
                if (whichUserColor != PaintDotNet.WhichUserColor.Secondary)
                {
                    throw ExceptionUtil.InvalidEnumArgumentException<PaintDotNet.WhichUserColor>(this.WhichUserColor, "this.WhichUserColor");
                }
            }
            else
            {
                ColorBgra bgra = ColorBgra.FromBgra(this.lastPrimaryColor.B, this.lastPrimaryColor.G, this.lastPrimaryColor.R, (byte) this.alphaGradientControl.Value);
                this.userPrimaryColor = bgra;
                this.OnUserPrimaryColorChanged(bgra);
                goto Label_014C;
            }
            ColorBgra newColor = ColorBgra.FromBgra(this.lastSecondaryColor.B, this.lastSecondaryColor.G, this.lastSecondaryColor.R, (byte) this.alphaGradientControl.Value);
            this.userSecondaryColor = newColor;
            this.OnUserSecondaryColorChanged(newColor);
        Label_014C:
            this.PopIgnoreChangedEvents();
            if (!ignoreChangedEvents && (sender == this.alphaGradientControl))
            {
                this.OnRelinquishFocus();
            }
            this.QueueUpdate();
        }

        private void OnUserColorChanged(ColorBgra newColor)
        {
            PaintDotNet.WhichUserColor whichUserColor = this.WhichUserColor;
            if (whichUserColor != PaintDotNet.WhichUserColor.Primary)
            {
                if (whichUserColor != PaintDotNet.WhichUserColor.Secondary)
                {
                    throw ExceptionUtil.InvalidEnumArgumentException<PaintDotNet.WhichUserColor>(this.WhichUserColor, "this.WhichUserColor");
                }
            }
            else
            {
                this.OnUserPrimaryColorChanged(newColor);
                return;
            }
            this.OnUserSecondaryColorChanged(newColor);
        }

        private void OnUserPrimaryColorChanged(ColorBgra newColor)
        {
            if ((this.UserPrimaryColorChanged != null) && (this.ignore == 0))
            {
                this.userPrimaryColor = newColor;
                this.UserPrimaryColorChanged(this, new ColorEventArgs(newColor));
                this.lastPrimaryColor = newColor;
                this.colorDisplayWidget.UserPrimaryColor = newColor;
            }
            this.RenderColorAddIcon(newColor);
        }

        private void OnUserSecondaryColorChanged(ColorBgra newColor)
        {
            if ((this.UserSecondaryColorChanged != null) && (this.ignore == 0))
            {
                this.userSecondaryColor = newColor;
                this.UserSecondaryColorChanged(this, new ColorEventArgs(newColor));
                this.lastSecondaryColor = newColor;
                this.colorDisplayWidget.UserSecondaryColor = newColor;
            }
            this.RenderColorAddIcon(newColor);
        }

        private void OnWhichUserColorBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            ColorBgra userSecondaryColor;
            PaintDotNet.WhichUserColor whichUserColor = this.WhichUserColor;
            if (whichUserColor != PaintDotNet.WhichUserColor.Primary)
            {
                if (whichUserColor != PaintDotNet.WhichUserColor.Secondary)
                {
                    throw ExceptionUtil.InvalidEnumArgumentException<PaintDotNet.WhichUserColor>(this.WhichUserColor, "this.WhichUserColor");
                }
            }
            else
            {
                userSecondaryColor = this.userPrimaryColor;
                goto Label_0033;
            }
            userSecondaryColor = this.userSecondaryColor;
        Label_0033:
            this.PushIgnoreChangedEvents();
            NumericUpDownUtil.SetValueIfDifferent(this.redUpDown, (int) userSecondaryColor.R);
            NumericUpDownUtil.SetValueIfDifferent(this.greenUpDown, (int) userSecondaryColor.G);
            NumericUpDownUtil.SetValueIfDifferent(this.blueUpDown, (int) userSecondaryColor.B);
            string str = this.GetHexNumericUpDownValue(userSecondaryColor.R, userSecondaryColor.G, userSecondaryColor.B);
            this.hexBox.Text = str;
            NumericUpDownUtil.SetValueIfDifferent(this.alphaUpDown, (int) userSecondaryColor.A);
            this.PopIgnoreChangedEvents();
            this.SetColorGradientMinMaxColorsRgb(userSecondaryColor.R, userSecondaryColor.G, userSecondaryColor.B);
            this.SetColorGradientValuesRgb(userSecondaryColor.R, userSecondaryColor.G, userSecondaryColor.B);
            this.SetColorGradientMinMaxColorsAlpha(userSecondaryColor.A);
            this.SyncHsvFromRgb(userSecondaryColor);
            this.OnRelinquishFocus();
        }

        private void PopIgnoreChangedEvents()
        {
            this.ignoreChangedEvents--;
        }

        private void PushIgnoreChangedEvents()
        {
            this.ignoreChangedEvents++;
        }

        private void RenderColorAddIcon(ColorBgra newColor)
        {
            if (this.colorAddIcon == null)
            {
                this.colorAddIcon = new Bitmap(0x10, 0x10, PixelFormat.Format32bppArgb);
            }
            using (Graphics graphics = Graphics.FromImage(this.colorAddIcon))
            {
                graphics.Clear(Color.Transparent);
                Rectangle rect = new Rectangle(1, 1, this.colorAddIcon.Width - 2, this.colorAddIcon.Height - 2);
                ColorRectangle.Draw(graphics, rect, newColor.ToColor(), true, false);
                DropShadow.DrawOutside(graphics, rect, 1);
                graphics.DrawImage(this.colorAddOverlay, 0, 0);
            }
            this.colorAddButton.Image = this.colorAddIcon;
            this.colorAddButton.Invalidate();
        }

        public void ResumeSetWhichUserColor()
        {
            this.suspendSetWhichUserColor--;
        }

        public void SetColorControlsRedraw(bool enabled)
        {
            Control[] controlArray = new Control[] { this.whichUserColorBox, this.hueUpDown, this.saturationUpDown, this.valueUpDown, this.redUpDown, this.greenUpDown, this.blueUpDown, this.alphaUpDown, this.toolStrip };
            foreach (Control control in controlArray)
            {
                if (enabled)
                {
                    UIUtil.ResumeControlPainting(control);
                }
                else
                {
                    UIUtil.SuspendControlPainting(control);
                }
            }
            base.Invalidate(true);
        }

        private void SetColorGradientMinMaxColorsAlpha(int a)
        {
            Color[] colorArray = new Color[0x100];
            for (int i = 0; i <= 0xff; i++)
            {
                colorArray[i] = Color.FromArgb(i, this.redGradientControl.Value, this.greenGradientControl.Value, this.blueGradientControl.Value);
            }
            this.alphaGradientControl.CustomGradient = colorArray;
        }

        private void SetColorGradientMinMaxColorsHsv(int h, int s, int v)
        {
            Color[] colorArray = new Color[0x169];
            for (int i = 0; i <= 360; i++)
            {
                colorArray[i] = new Int32HsvColor(i, 100, 100).ToGdipColor();
            }
            this.hueGradientControl.CustomGradient = colorArray;
            Color[] colorArray2 = new Color[0x65];
            for (int j = 0; j <= 100; j++)
            {
                colorArray2[j] = new Int32HsvColor(h, j, v).ToGdipColor();
            }
            this.saturationGradientControl.CustomGradient = colorArray2;
            this.valueGradientControl.MaxColor = new Int32HsvColor(h, s, 100).ToGdipColor();
            this.valueGradientControl.MinColor = new Int32HsvColor(h, s, 0).ToGdipColor();
        }

        private void SetColorGradientMinMaxColorsRgb(int r, int g, int b)
        {
            this.redGradientControl.MaxColor = Color.FromArgb(0xff, g, b);
            this.redGradientControl.MinColor = Color.FromArgb(0, g, b);
            this.greenGradientControl.MaxColor = Color.FromArgb(r, 0xff, b);
            this.greenGradientControl.MinColor = Color.FromArgb(r, 0, b);
            this.blueGradientControl.MaxColor = Color.FromArgb(r, g, 0xff);
            this.blueGradientControl.MinColor = Color.FromArgb(r, g, 0);
        }

        private void SetColorGradientValuesHsv(int h, int s, int v)
        {
            this.PushIgnoreChangedEvents();
            if (((this.hueGradientControl.Value * 360) / 0xff) != h)
            {
                this.hueGradientControl.Value = (0xff * h) / 360;
            }
            if (((this.saturationGradientControl.Value * 100) / 0xff) != s)
            {
                this.saturationGradientControl.Value = (0xff * s) / 100;
            }
            if (((this.valueGradientControl.Value * 100) / 0xff) != v)
            {
                this.valueGradientControl.Value = (0xff * v) / 100;
            }
            this.PopIgnoreChangedEvents();
        }

        private void SetColorGradientValuesRgb(int r, int g, int b)
        {
            this.PushIgnoreChangedEvents();
            if (this.redGradientControl.Value != r)
            {
                this.redGradientControl.Value = r;
            }
            if (this.greenGradientControl.Value != g)
            {
                this.greenGradientControl.Value = g;
            }
            if (this.blueGradientControl.Value != b)
            {
                this.blueGradientControl.Value = b;
            }
            this.PopIgnoreChangedEvents();
        }

        public void SetUserColors(ColorBgra primary, ColorBgra secondary)
        {
            this.SetColorControlsRedraw(false);
            PaintDotNet.WhichUserColor whichUserColor = this.WhichUserColor;
            this.UserPrimaryColor = primary;
            this.UserSecondaryColor = secondary;
            this.WhichUserColor = whichUserColor;
            this.SetColorControlsRedraw(true);
        }

        public void SetUserColorsToBlackAndWhite()
        {
            this.SetUserColors(ColorBgra.Black, ColorBgra.White);
        }

        public void SuspendSetWhichUserColor()
        {
            this.suspendSetWhichUserColor++;
        }

        public void SwapUserColors()
        {
            ColorBgra userPrimaryColor = this.UserPrimaryColor;
            ColorBgra userSecondaryColor = this.UserSecondaryColor;
            this.SetUserColors(userSecondaryColor, userPrimaryColor);
        }

        private void SyncHsvFromRgb(ColorBgra newColor)
        {
            if (this.ignore == 0)
            {
                this.ignore++;
                Int32HsvColor color = Int32HsvColor.FromGdipColor(newColor.ToColor());
                NumericUpDownUtil.SetValueIfDifferent(this.hueUpDown, color.Hue);
                NumericUpDownUtil.SetValueIfDifferent(this.saturationUpDown, color.Saturation);
                NumericUpDownUtil.SetValueIfDifferent(this.valueUpDown, color.Value);
                this.SetColorGradientValuesHsv(color.Hue, color.Saturation, color.Value);
                this.SetColorGradientMinMaxColorsHsv(color.Hue, color.Saturation, color.Value);
                this.colorWheel.HsvColor = color;
                this.ignore--;
            }
        }

        private void SyncRgbFromHsv(Int32HsvColor newColor)
        {
            if (this.ignore == 0)
            {
                this.ignore++;
                Int32RgbColor color = newColor.ToRgb();
                NumericUpDownUtil.SetValueIfDifferent(this.redUpDown, color.Red);
                NumericUpDownUtil.SetValueIfDifferent(this.greenUpDown, color.Green);
                NumericUpDownUtil.SetValueIfDifferent(this.blueUpDown, color.Blue);
                string str = this.GetHexNumericUpDownValue(color.Red, color.Green, color.Blue);
                this.hexBox.Text = str;
                this.SetColorGradientValuesRgb(color.Red, color.Green, color.Blue);
                this.SetColorGradientMinMaxColorsRgb(color.Red, color.Green, color.Blue);
                this.SetColorGradientMinMaxColorsAlpha((int) this.alphaUpDown.Value);
                this.ignore--;
            }
        }

        public void ToggleWhichUserColor()
        {
            PaintDotNet.WhichUserColor whichUserColor = this.WhichUserColor;
            if (whichUserColor != PaintDotNet.WhichUserColor.Primary)
            {
                if (whichUserColor != PaintDotNet.WhichUserColor.Secondary)
                {
                    throw ExceptionUtil.InvalidEnumArgumentException<PaintDotNet.WhichUserColor>(this.WhichUserColor, "this.WhichUserColor");
                }
            }
            else
            {
                this.WhichUserColor = PaintDotNet.WhichUserColor.Secondary;
                return;
            }
            this.WhichUserColor = PaintDotNet.WhichUserColor.Primary;
        }

        private bool IgnoreChangedEvents =>
            (this.ignoreChangedEvents > 0);

        protected override string SnapObstacleName =>
            "Colors";

        protected override ISnapObstaclePersist SnapObstacleSettings =>
            AppSettings.Instance.Window.Colors;

        public ColorBgra UserPrimaryColor
        {
            get => 
                this.userPrimaryColor;
            set
            {
                if (!this.IgnoreChangedEvents && (this.userPrimaryColor != value))
                {
                    this.userPrimaryColor = value;
                    this.OnUserPrimaryColorChanged(value);
                    if (this.WhichUserColor != PaintDotNet.WhichUserColor.Primary)
                    {
                        this.WhichUserColor = PaintDotNet.WhichUserColor.Primary;
                    }
                    this.ignore++;
                    NumericUpDownUtil.SetValueIfDifferent(this.alphaUpDown, (int) value.A);
                    NumericUpDownUtil.SetValueIfDifferent(this.redUpDown, (int) value.R);
                    NumericUpDownUtil.SetValueIfDifferent(this.greenUpDown, (int) value.G);
                    this.SetColorGradientValuesRgb(value.R, value.G, value.B);
                    this.SetColorGradientMinMaxColorsRgb(value.R, value.G, value.B);
                    this.SetColorGradientMinMaxColorsAlpha(value.A);
                    this.ignore--;
                    NumericUpDownUtil.SetValueIfDifferent(this.blueUpDown, (int) value.B);
                    this.QueueUpdate();
                    string str = this.GetHexNumericUpDownValue(value.R, value.G, value.B);
                    this.hexBox.Text = str;
                    this.SyncHsvFromRgb(value);
                    this.colorDisplayWidget.UserPrimaryColor = this.userPrimaryColor;
                }
            }
        }

        public ColorBgra UserSecondaryColor
        {
            get => 
                this.userSecondaryColor;
            set
            {
                if (!this.IgnoreChangedEvents && (this.userSecondaryColor != value))
                {
                    this.userSecondaryColor = value;
                    this.OnUserSecondaryColorChanged(value);
                    if (this.WhichUserColor != PaintDotNet.WhichUserColor.Secondary)
                    {
                        this.WhichUserColor = PaintDotNet.WhichUserColor.Secondary;
                    }
                    this.ignore++;
                    NumericUpDownUtil.SetValueIfDifferent(this.alphaUpDown, (int) value.A);
                    NumericUpDownUtil.SetValueIfDifferent(this.redUpDown, (int) value.R);
                    NumericUpDownUtil.SetValueIfDifferent(this.greenUpDown, (int) value.G);
                    this.SetColorGradientValuesRgb(value.R, value.G, value.B);
                    this.SetColorGradientMinMaxColorsRgb(value.R, value.G, value.B);
                    this.SetColorGradientMinMaxColorsAlpha(value.A);
                    this.ignore--;
                    NumericUpDownUtil.SetValueIfDifferent(this.blueUpDown, (int) value.B);
                    this.QueueUpdate();
                    string str = this.GetHexNumericUpDownValue(value.R, value.G, value.B);
                    this.hexBox.Text = str;
                    this.SyncHsvFromRgb(value);
                    this.colorDisplayWidget.UserSecondaryColor = this.userSecondaryColor;
                }
            }
        }

        public PaintDotNet.WhichUserColor WhichUserColor
        {
            get => 
                ((WhichUserColorWrapper) this.whichUserColorBox.SelectedItem).WhichUserColor;
            set
            {
                if (this.suspendSetWhichUserColor <= 0)
                {
                    this.whichUserColorBox.SelectedItem = new WhichUserColorWrapper(value);
                }
            }
        }

        private sealed class ColorsFormToolStripRenderer : PdnToolStripRenderer
        {
            protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
            {
                if (e.ToolStrip is ToolStripDropDown)
                {
                    base.OnRenderToolStripBackground(e);
                }
                else
                {
                    using (SolidBrush brush = new SolidBrush(e.ToolStrip.Parent.BackColor))
                    {
                        e.Graphics.FillRectangle(brush, e.AffectedBounds);
                    }
                }
            }

            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
            }
        }

        private sealed class WhichUserColorWrapper : IEquatable<ColorsForm.WhichUserColorWrapper>
        {
            private PaintDotNet.WhichUserColor whichUserColor;

            public WhichUserColorWrapper(PaintDotNet.WhichUserColor whichUserColor)
            {
                this.whichUserColor = whichUserColor;
            }

            public bool Equals(ColorsForm.WhichUserColorWrapper other)
            {
                if (other == null)
                {
                    return false;
                }
                return (this.whichUserColor == other.whichUserColor);
            }

            public override bool Equals(object obj) => 
                EquatableUtil.Equals<ColorsForm.WhichUserColorWrapper, object>(this, obj);

            public override int GetHashCode() => 
                ((int) this.whichUserColor);

            public override string ToString() => 
                PdnResources.GetString("WhichUserColor." + this.whichUserColor.ToString());

            public PaintDotNet.WhichUserColor WhichUserColor =>
                this.whichUserColor;
        }
    }
}

