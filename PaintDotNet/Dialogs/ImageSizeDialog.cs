namespace PaintDotNet.Dialogs
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.Drawing;
    using PaintDotNet.Resources;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Threading;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;

    internal abstract class ImageSizeDialog : PdnBaseFormInternal
    {
        private PdnRadioButton absoluteRB;
        private AnchorChooserControl anchorChooserControl;
        private PdnDropDownList anchorEdgeCB;
        private static readonly EnumLocalizer anchorEdgeNames = EnumLocalizer.Create(typeof(PaintDotNet.AnchorEdge));
        private HeadingLabel anchorHeader;
        private PdnLabel asteriskLabel;
        private PdnLabel asteriskTextLabel;
        private PdnPushButton cancelButton;
        private Container components;
        private PdnCheckBox constrainCheckBox;
        private ResizeConstrainer constrainer;
        private readonly ImageSizeDialogType dialogType;
        private int getValueFromText;
        private int ignoreUpDownValueChanged;
        private int layers;
        private PdnLabel newHeightLabel1;
        private PdnLabel newHeightLabel2;
        private PdnLabel newWidthLabel1;
        private PdnLabel newWidthLabel2;
        private PdnPushButton okButton;
        private EventHandler onUpDownValueChangedDelegate;
        private double originalDpu = Document.GetDefaultDpu(Document.DefaultDpuUnit);
        private MeasurementUnit originalDpuUnit = Document.DefaultDpuUnit;
        private PdnRadioButton percentRB;
        private PdnLabel percentSignLabel;
        private NumericUpDown percentUpDown;
        private Action performLayoutAction;
        private NumericUpDown pixelHeightUpDown;
        private HeadingLabel pixelSizeHeader;
        private PdnLabel pixelsLabel1;
        private PdnLabel pixelsLabel2;
        private NumericUpDown pixelWidthUpDown;
        private NumericUpDown printHeightUpDown;
        private HeadingLabel printSizeHeader;
        private NumericUpDown printWidthUpDown;
        private PdnDropDownList resamplingAlgorithmComboBox;
        private PdnLabel resamplingLabel;
        private HeadingLabel resizedImageHeader;
        private PdnLabel resolutionLabel;
        private NumericUpDown resolutionUpDown;
        private PaintDotNet.Controls.SeparatorLine separatorLine;
        private UnitsComboBox unitsComboBox1;
        private UnitsComboBox unitsComboBox2;
        private PdnLabel unitsLabel1;

        protected ImageSizeDialog(ImageSizeDialogType dialogType)
        {
            this.dialogType = dialogType;
            base.SuspendLayout();
            base.AutoHandleGlassRelatedOptimizations = true;
            base.IsGlassDesired = !OS.IsWin10OrLater;
            this.DoubleBuffered = true;
            this.InitializeComponent();
            this.asteriskLabel.Text = PdnResources.GetString("ResizeDialog.AsteriskLabel.Text");
            this.percentSignLabel.Text = PdnResources.GetString("ResizeDialog.PercentSignLabel.Text");
            this.pixelSizeHeader.Text = PdnResources.GetString("ResizeDialog.PixelSizeHeader.Text");
            this.printSizeHeader.Text = PdnResources.GetString("ResizeDialog.PrintSizeHeader.Text");
            this.pixelsLabel1.Text = PdnResources.GetString("ResizeDialog.PixelsLabel1.Text");
            this.pixelsLabel2.Text = PdnResources.GetString("ResizeDialog.PixelsLabel2.Text");
            this.resolutionLabel.Text = PdnResources.GetString("ResizeDialog.ResolutionLabel.Text");
            this.percentRB.Text = PdnResources.GetString("ResizeDialog.PercentRB.Text");
            this.absoluteRB.Text = PdnResources.GetString("ResizeDialog.AbsoluteRB.Text");
            this.resamplingLabel.Text = PdnResources.GetString("ResizeDialog.ResamplingLabel.Text");
            this.cancelButton.Text = PdnResources.GetString("Form.CancelButton.Text");
            this.okButton.Text = PdnResources.GetString("Form.OkButton.Text");
            this.newWidthLabel1.Text = PdnResources.GetString("ResizeDialog.NewWidthLabel1.Text");
            this.newHeightLabel1.Text = PdnResources.GetString("ResizeDialog.NewHeightLabel1.Text");
            this.newWidthLabel2.Text = PdnResources.GetString("ResizeDialog.NewWidthLabel1.Text");
            this.newHeightLabel2.Text = PdnResources.GetString("ResizeDialog.NewHeightLabel1.Text");
            this.constrainCheckBox.Text = PdnResources.GetString("ResizeDialog.ConstrainCheckBox.Text");
            this.unitsLabel1.Text = this.unitsComboBox1.UnitsText;
            this.anchorHeader.Text = PdnResources.GetString("CanvasSizeDialog.AnchorHeader.Text");
            this.onUpDownValueChangedDelegate = new EventHandler(this.OnUpDownValueChanged);
            this.constrainer = new ResizeConstrainer(new Size((int) this.pixelWidthUpDown.Value, (int) this.pixelHeightUpDown.Value));
            this.SetupConstrainerEvents();
            this.resamplingAlgorithmComboBox.Items.Clear();
            this.resamplingAlgorithmComboBox.Items.Add(new ResampleMethod(PaintDotNet.ResamplingAlgorithm.Fant));
            this.resamplingAlgorithmComboBox.Items.Add(new ResampleMethod(PaintDotNet.ResamplingAlgorithm.Bicubic));
            this.resamplingAlgorithmComboBox.Items.Add(new ResampleMethod(PaintDotNet.ResamplingAlgorithm.Bilinear));
            this.resamplingAlgorithmComboBox.Items.Add(new ResampleMethod(PaintDotNet.ResamplingAlgorithm.NearestNeighbor));
            this.resamplingAlgorithmComboBox.SelectedItem = new ResampleMethod(PaintDotNet.ResamplingAlgorithm.Fant);
            this.layers = 1;
            this.percentUpDown.Enabled = false;
            this.PopulateAsteriskLabels();
            this.OnRadioButtonIsCheckedChanged(this, EventArgs.Empty);
            foreach (LocalizedEnumValue value2 in anchorEdgeNames.GetLocalizedEnumValues())
            {
                PaintDotNet.AnchorEdge enumValue = (PaintDotNet.AnchorEdge) value2.EnumValue;
                this.anchorEdgeCB.Items.Add(value2);
                if (enumValue == this.AnchorEdge)
                {
                    this.anchorEdgeCB.SelectedItem = value2;
                }
            }
            EventHandler handler = (s, e) => this.InvalidateLayout();
            foreach (Control control in base.Controls)
            {
                control.TextChanged += handler;
            }
            this.OnAnchorChooserControlAnchorEdgeChanged(this.anchorChooserControl, EventArgs.Empty);
        }

        protected override void Dispose(bool disposing)
        {
            DisposableUtil.Free<Container>(ref this.components, disposing);
            base.Dispose(disposing);
        }

        private Size DoLayout(int clientWidth, bool applyLayout)
        {
            int num7;
            TableLayoutData data = new TableLayoutData();
            Size proposedSize = UIUtil.ScaleSize(0x55, 0x18);
            bool isGlassEffectivelyEnabled = base.IsGlassEffectivelyEnabled;
            int y = UIUtil.ScaleHeight(6);
            int num2 = UIUtil.ScaleWidth(6);
            int x = UIUtil.ScaleWidth(8);
            int num4 = UIUtil.ScaleHeight(8);
            int num5 = UIUtil.ScaleWidth(7);
            int num6 = UIUtil.ScaleHeight(7);
            switch (this.dialogType)
            {
                case ImageSizeDialogType.FileNew:
                    num7 = 0;
                    break;

                default:
                    num7 = 0x11;
                    break;
            }
            int num8 = UIUtil.ScaleWidth(num7);
            int num9 = x;
            int num10 = UIUtil.ScaleWidth(0x48);
            int num11 = -2;
            int num12 = -UIUtil.ScaleHeight(1);
            int num13 = UIUtil.ScaleHeight(2);
            int num14 = UIUtil.ScaleHeight(1);
            data.AddControl(this.resizedImageHeader, 0);
            Point location = new Point(num9 + num11, y);
            int width = (clientWidth - location.X) - num5;
            int height = this.resizedImageHeader.GetPreferredSize(width, 0).Height;
            Size size = new Size(width, height);
            data.Controls[this.resizedImageHeader].Bounds = new Rectangle(location, size);
            int num17 = data.Controls[this.resizedImageHeader].Bounds.Bottom + y;
            data.AddControl(this.resamplingLabel, 1);
            Point point2 = new Point(num9, num17);
            Size size3 = this.resamplingLabel.GetPreferredSize((clientWidth - point2.X) - num5, 0);
            data.AddControl(this.resamplingAlgorithmComboBox, 1);
            Size size4 = this.resamplingAlgorithmComboBox.GetPreferredSize(UIUtil.ScaleWidth(0x98), 0);
            data.AddControl(this.asteriskLabel, 1);
            Size size5 = this.asteriskLabel.GetPreferredSize(0, 0);
            int num18 = MathUtil.Max(size3.Height, size5.Height, size4.Height);
            int num19 = num17 + num18;
            Size size6 = new Size(size3.Width, num18);
            data.Controls[this.resamplingLabel].Bounds = new Rectangle(point2, size6);
            Size size7 = new Size(size4.Width, num18);
            Size size8 = new Size(size5.Width, num18);
            int num20 = num19 + y;
            data.AddControl(this.percentRB, 2);
            Point point3 = new Point(num9, num20 + num12);
            Size size9 = this.percentRB.GetPreferredSize((clientWidth - point3.X) - num5, 0);
            data.AddControl(this.percentUpDown, 2);
            int num21 = num10;
            int num22 = this.percentUpDown.GetPreferredSize(num21, 0).Height;
            data.AddControl(this.percentSignLabel, 2);
            Size size10 = this.percentSignLabel.GetPreferredSize(0, 0);
            int num23 = MathUtil.Max(size9.Height, num22, size10.Height);
            Size size11 = new Size(size9.Width, num23);
            data.Controls[this.percentRB].Bounds = new Rectangle(point3, size11);
            Size size12 = new Size(num21, num23);
            int num24 = num20 + num23;
            int num25 = num24 + y;
            data.AddControl(this.absoluteRB, 3);
            Point point4 = new Point(num9, num25 + num12);
            Size size13 = this.absoluteRB.GetPreferredSize((clientWidth - point4.X) - num5, 0);
            int num26 = size13.Height;
            int num27 = num25 + num26;
            Size size14 = size13;
            data.Controls[this.absoluteRB].Bounds = new Rectangle(point4, size14);
            int num28 = num27 + y;
            data.AddControl(this.constrainCheckBox, 4);
            Point point5 = new Point(num9 + num8, num28);
            Size size15 = this.constrainCheckBox.GetPreferredSize((clientWidth - point5.X) - num5, 0);
            int num29 = size15.Height;
            Size size16 = new Size(size15.Width, num29);
            data.Controls[this.constrainCheckBox].Bounds = new Rectangle(point5, size16);
            int num30 = num28 + num29;
            int num31 = num30 + y;
            data.AddControl(this.pixelSizeHeader, 5);
            Point point6 = new Point((num9 + num8) + num11, num31);
            int num32 = (clientWidth - point6.X) - num5;
            int num34 = this.pixelSizeHeader.GetPreferredSize(num32, 0).Height;
            Size size17 = new Size(num32, num34);
            data.Controls[this.pixelSizeHeader].Bounds = new Rectangle(point6, size17);
            int num35 = num31 + num34;
            int num36 = num35 + y;
            int num37 = num7 + 8;
            int num38 = UIUtil.ScaleWidth(num37);
            data.AddControl(this.newWidthLabel1, 6);
            Point point7 = new Point(num9 + num38, num36);
            Size size18 = this.newWidthLabel1.GetPreferredSize(0, 0);
            int num39 = num38 + size18.Width;
            data.AddControl(this.pixelWidthUpDown, 6);
            int num40 = num10;
            int num41 = this.pixelWidthUpDown.GetPreferredSize(num40, 0).Height;
            data.AddControl(this.pixelsLabel1, 6);
            Size size19 = this.pixelsLabel1.GetPreferredSize(0, 0);
            int num42 = MathUtil.Max(size18.Height, num41, size19.Height);
            Size size20 = new Size(size18.Width, num42);
            data.Controls[this.newWidthLabel1].Bounds = new Rectangle(point7, size20);
            Size size21 = new Size(num40, num42);
            Size size22 = new Size(size19.Width, num42);
            int num43 = num36 + num42;
            int num44 = num43 + y;
            data.AddControl(this.newHeightLabel1, 7);
            Point point8 = new Point(num9 + num38, num44);
            Size size23 = this.newHeightLabel1.GetPreferredSize(0, 0);
            int num45 = num38 + size23.Width;
            data.AddControl(this.pixelHeightUpDown, 7);
            int num46 = num10;
            int num47 = this.pixelHeightUpDown.GetPreferredSize(num46, 0).Height;
            data.AddControl(this.pixelsLabel2, 7);
            Size size24 = this.pixelsLabel2.GetPreferredSize(0, 0);
            int num48 = MathUtil.Max(size23.Height, num47, size24.Height);
            Size size25 = new Size(size23.Width, num48);
            data.Controls[this.newHeightLabel1].Bounds = new Rectangle(point8, size25);
            Size size26 = new Size(num46, num48);
            Size size27 = new Size(size24.Width, num48);
            int num49 = num44 + num48;
            int num50 = num49 + y;
            data.AddControl(this.resolutionLabel, 8);
            Point point9 = new Point(num9 + num38, num50);
            Size size28 = this.resolutionLabel.GetPreferredSize(0, 0);
            int num51 = num38 + size28.Width;
            data.AddControl(this.resolutionUpDown, 8);
            int num52 = num10;
            int num53 = this.resolutionUpDown.GetPreferredSize(num52, 0).Height;
            data.AddControl(this.unitsComboBox2, 8);
            Size size29 = this.unitsComboBox2.GetPreferredSize(UIUtil.ScaleWidth(0x58), 0);
            int num54 = MathUtil.Max(size28.Height, num53, size29.Height);
            Size size30 = new Size(size28.Width, num54);
            data.Controls[this.resolutionLabel].Bounds = new Rectangle(point9, size30);
            Size size31 = new Size(num52, num54);
            Size size32 = new Size(size29.Width, num54);
            int num55 = num50 + num54;
            int num56 = num55 + y;
            data.AddControl(this.printSizeHeader, 9);
            Point point10 = new Point(num9 + num8, num56);
            int num57 = (clientWidth - point10.X) - num5;
            int num58 = this.printSizeHeader.GetPreferredSize(num57, 0).Height;
            int num59 = num58;
            Size size33 = new Size(num57, num58);
            data.Controls[this.printSizeHeader].Bounds = new Rectangle(point10, size33);
            int num60 = num56 + num59;
            int num61 = num60 + y;
            data.AddControl(this.newWidthLabel2, 10);
            Point point11 = new Point(num9 + num38, num61);
            Size size34 = this.newWidthLabel2.GetPreferredSize(0, 0);
            int num62 = num38 + size34.Width;
            data.AddControl(this.printWidthUpDown, 10);
            int num63 = num10;
            int num64 = this.printWidthUpDown.GetPreferredSize(num63, 0).Height;
            data.AddControl(this.unitsComboBox1, 10);
            Size size35 = this.unitsComboBox1.GetPreferredSize(UIUtil.ScaleWidth(0x58), 0);
            int num65 = MathUtil.Max(size34.Height, num64, size35.Height);
            Size size36 = new Size(size34.Width, num65);
            data.Controls[this.newWidthLabel2].Bounds = new Rectangle(point11, size36);
            Size size37 = new Size(num63, num65);
            Size size38 = new Size(size35.Width, num65);
            int num66 = num61 + num65;
            int num67 = num66 + y;
            data.AddControl(this.newHeightLabel2, 11);
            Point point12 = new Point(num9 + num38, num67);
            Size size39 = this.newHeightLabel2.GetPreferredSize(0, 0);
            int num68 = num38 + size39.Width;
            data.AddControl(this.printHeightUpDown, 11);
            int num69 = num10;
            int num70 = this.printHeightUpDown.GetPreferredSize(num69, 0).Height;
            data.AddControl(this.unitsLabel1, 11);
            Size size40 = this.unitsLabel1.GetPreferredSize(0, 0);
            int num71 = MathUtil.Max(size39.Height, num70, size40.Height);
            Size size41 = new Size(size39.Width, num71);
            data.Controls[this.newHeightLabel2].Bounds = new Rectangle(point12, size41);
            Size size42 = new Size(num69, num71);
            Size size43 = new Size(size40.Width, num71);
            int num72 = num67 + num71;
            int num73 = num72 + y;
            if (this.dialogType == ImageSizeDialogType.FileNew)
            {
                num73 += y;
            }
            data.AddControl(this.anchorHeader, 12);
            Point point13 = new Point(num9 + num11, num73);
            int num74 = (clientWidth - point13.X) - num5;
            int num75 = this.anchorHeader.GetPreferredSize(num74, 0).Height;
            Size size44 = new Size(num74, num75);
            data.Controls[this.anchorHeader].Bounds = new Rectangle(point13, size44);
            int num76 = num75;
            int num77 = num73 + num76;
            int num78 = num77 + y;
            data.AddControl(this.anchorEdgeCB, 13);
            Point point14 = new Point(num9 + num38, num78);
            Size size46 = this.anchorEdgeCB.GetPreferredSize(UIUtil.ScaleWidth(120), 0);
            data.Controls[this.anchorEdgeCB].Bounds = new Rectangle(point14, size46);
            data.AddControl(this.anchorChooserControl, 13);
            Point point15 = new Point(data.Controls[this.anchorEdgeCB].Bounds.Right + (num2 * 4), num78);
            Size size47 = this.anchorChooserControl.GetPreferredSize(0, 0);
            data.Controls[this.anchorChooserControl].Bounds = new Rectangle(point15, size47);
            int num79 = MathUtil.Max(data.Controls[this.anchorEdgeCB].Bounds.Height, data.Controls[this.anchorChooserControl].Bounds.Height);
            int num80 = num78 + num79;
            int num81 = num80 + y;
            data.AddControl(this.asteriskTextLabel, 14);
            Point point16 = new Point(num9, num81);
            int num82 = (clientWidth - point16.X) - num5;
            int num83 = this.asteriskTextLabel.GetPreferredSize(num82, 0).Height;
            Size size48 = new Size(num82, num83);
            data.Controls[this.asteriskTextLabel].Bounds = new Rectangle(point16, size48);
            int num84 = data.Controls[this.asteriskTextLabel].Bounds.Height;
            int num85 = num81 + num84;
            List<Control> list = new List<Control>();
            switch (this.dialogType)
            {
                case ImageSizeDialogType.FileNew:
                {
                    Control[] collection = new Control[] { this.resamplingLabel, this.resamplingAlgorithmComboBox, this.asteriskLabel, this.percentRB, this.percentUpDown, this.percentSignLabel, this.absoluteRB, this.asteriskTextLabel, this.anchorHeader, this.anchorEdgeCB, this.anchorChooserControl };
                    list.AddRange(collection);
                    break;
                }
                case ImageSizeDialogType.ImageResize:
                {
                    Control[] controlArray3 = new Control[] { this.anchorHeader, this.anchorEdgeCB, this.anchorChooserControl };
                    list.AddRange(controlArray3);
                    break;
                }
                case ImageSizeDialogType.ImageCanvasSize:
                {
                    Control[] controlArray2 = new Control[] { this.resamplingLabel, this.resamplingAlgorithmComboBox, this.asteriskLabel, this.asteriskTextLabel };
                    list.AddRange(controlArray2);
                    break;
                }
                default:
                    throw ExceptionUtil.InvalidEnumArgumentException<ImageSizeDialogType>(this.dialogType, "dialogType");
            }
            foreach (Control control in list)
            {
                data.Controls[control].IsVisible = false;
            }
            int num86 = MathUtil.Max(data.Controls[this.resamplingLabel].IsVisible ? size3.Width : 0, data.Controls[this.percentRB].IsVisible ? size9.Width : 0, num39, num45, num51, num62, num68);
            int num87 = num9 + num86;
            int num88 = num87 + (num2 * 2);
            Point point17 = new Point(num88, num20 + num13);
            data.Controls[this.percentUpDown].Bounds = new Rectangle(point17, size12);
            Point point18 = new Point(data.Controls[this.percentUpDown].Bounds.Right + num2, num20);
            data.Controls[this.percentSignLabel].Bounds = new Rectangle(point18, new Size(size10.Width, num23));
            Point point19 = new Point((clientWidth - size8.Width) - num5, num17);
            data.Controls[this.asteriskLabel].Bounds = new Rectangle(point19, size8);
            Point point20 = new Point(num88, num17 + num14);
            Size size49 = new Size((point19.X - num2) - point20.X, size7.Height);
            data.Controls[this.resamplingAlgorithmComboBox].Bounds = new Rectangle(point20, size49);
            Point point21 = new Point(num88, num36 + num13);
            data.Controls[this.pixelWidthUpDown].Bounds = new Rectangle(point21, size21);
            Point point22 = new Point(data.Controls[this.pixelWidthUpDown].Bounds.Right + num2, num36);
            data.Controls[this.pixelsLabel1].Bounds = new Rectangle(point22, size22);
            Point point23 = new Point(num88, num44 + num13);
            data.Controls[this.pixelHeightUpDown].Bounds = new Rectangle(point23, size26);
            Point point24 = new Point(data.Controls[this.pixelHeightUpDown].Bounds.Right + num2, num44);
            data.Controls[this.pixelsLabel2].Bounds = new Rectangle(point24, size27);
            Point point25 = new Point(num88, num50 + num13);
            data.Controls[this.resolutionUpDown].Bounds = new Rectangle(point25, size31);
            int num89 = Math.Max(size32.Width, size38.Width);
            Point point26 = new Point(data.Controls[this.resolutionUpDown].Bounds.Right + num2, num50 + num14);
            int num90 = Math.Max(num89, (clientWidth - point26.X) - num5);
            Size size50 = new Size(num90, size32.Height);
            data.Controls[this.unitsComboBox2].Bounds = new Rectangle(point26, size50);
            Point point27 = new Point(num88, num61 + num13);
            data.Controls[this.printWidthUpDown].Bounds = new Rectangle(point27, size37);
            Point point28 = new Point(data.Controls[this.printWidthUpDown].Bounds.Right + num2, num61 + num14);
            int num91 = Math.Max(num89, (clientWidth - point28.X) - num5);
            Size size51 = new Size(num91, size38.Height);
            data.Controls[this.unitsComboBox1].Bounds = new Rectangle(point28, size51);
            Point point29 = new Point(num88, num67 + num13);
            data.Controls[this.printHeightUpDown].Bounds = new Rectangle(point29, size42);
            Point point30 = new Point(data.Controls[this.printHeightUpDown].Bounds.Right + num2, num67);
            data.Controls[this.unitsLabel1].Bounds = new Rectangle(point30, size43);
            int num92 = num85;
            data.AddControl(this.separatorLine, 15);
            Point point31 = new Point(x, num92 + y);
            int num93 = (clientWidth - point31.X) - num5;
            int num94 = this.separatorLine.GetPreferredSize(num93, 0).Height;
            Size size52 = new Size(num93, num94);
            data.Controls[this.separatorLine].Bounds = new Rectangle(point31, size52);
            int num95 = data.Controls[this.separatorLine].Bounds.Bottom + y;
            data.AddControl(this.cancelButton, 0x10);
            Size preferredSize = this.cancelButton.GetPreferredSize(proposedSize);
            Size size54 = GdipSizeUtil.Max(proposedSize, preferredSize);
            Point point32 = new Point((clientWidth - num5) - size54.Width, num95);
            data.Controls[this.cancelButton].Bounds = new Rectangle(point32, size54);
            data.AddControl(this.okButton, 0x10);
            Size sizeMax = this.okButton.GetPreferredSize(proposedSize);
            Size size56 = GdipSizeUtil.Max(proposedSize, sizeMax);
            Point point33 = new Point((data.Controls[this.cancelButton].Bounds.Left - num2) - size56.Width, num95);
            data.Controls[this.okButton].Bounds = new Rectangle(point33, size56);
            int num96 = 0;
            int count = data.Rows.Count;
            while (num96 < count)
            {
                RowInfo info = data.Rows[num96];
                if (num96 > 0)
                {
                    info.VerticalOffset = data.Rows[num96 - 1].VerticalOffset;
                }
                if (!info.AreAnyControlsVisible())
                {
                    info.IsExpanded = false;
                    Rectangle bounds = info.Bounds;
                    info.VerticalOffset -= bounds.Height;
                    if (num96 != (count - 1))
                    {
                        int num98 = data.Rows[num96 + 1].Bounds.Top - bounds.Bottom;
                        info.VerticalOffset -= num98;
                    }
                }
                num96++;
            }
            foreach (RowInfo info2 in data.Rows)
            {
                foreach (ControlInfo info3 in info2.Controls)
                {
                    Control control2 = info3.Control;
                    Rectangle rectangle5 = info3.Bounds;
                    rectangle5.Y += info2.VerticalOffset;
                    info3.Bounds = rectangle5;
                }
                info2.VerticalOffset = 0;
            }
            Rectangle rectangle = RectangleUtil.Bounds((IEnumerable<Rectangle>) (from ci in data.Controls.Values
                where ci.IsVisible
                select ci.Bounds));
            Size size57 = new Size(rectangle.Right + num5, rectangle.Bottom + num6);
            if (isGlassEffectivelyEnabled)
            {
                Rectangle rectangle6 = data.Controls[this.cancelButton].Bounds;
                Rectangle rectangle7 = data.Controls[this.okButton].Bounds;
                Rectangle rectangle8 = rectangle6;
                rectangle8.X = (size57.Width - rectangle6.Width) + 1;
                Rectangle rectangle9 = rectangle7;
                rectangle9.X += rectangle8.X - rectangle6.X;
                data.Controls[this.cancelButton].Bounds = rectangle8;
                data.Controls[this.okButton].Bounds = rectangle9;
            }
            if (applyLayout)
            {
                foreach (RowInfo info4 in data.Rows)
                {
                    foreach (ControlInfo info5 in info4.Controls)
                    {
                        info5.Control.Bounds = info5.Bounds;
                        info5.Control.Visible = info5.IsVisible;
                    }
                }
            }
            if (applyLayout)
            {
                Padding padding;
                if (isGlassEffectivelyEnabled)
                {
                    padding = new Padding(0, 0, 0, size57.Height - data.Controls[this.separatorLine].Bounds.Top);
                }
                else
                {
                    padding = new Padding(0);
                }
                this.separatorLine.Visible = !isGlassEffectivelyEnabled;
                base.GlassInset = padding;
            }
            return size57;
        }

        private void InitializeComponent()
        {
            this.constrainCheckBox = new PdnCheckBox();
            this.newWidthLabel1 = new PdnLabel();
            this.newHeightLabel1 = new PdnLabel();
            this.pixelWidthUpDown = new NumericUpDown();
            this.pixelHeightUpDown = new NumericUpDown();
            this.resizedImageHeader = new HeadingLabel();
            this.asteriskLabel = new PdnLabel();
            this.asteriskTextLabel = new PdnLabel();
            this.absoluteRB = new PdnRadioButton();
            this.percentRB = new PdnRadioButton();
            this.pixelsLabel1 = new PdnLabel();
            this.percentUpDown = new NumericUpDown();
            this.percentSignLabel = new PdnLabel();
            this.resolutionLabel = new PdnLabel();
            this.resolutionUpDown = new NumericUpDown();
            this.unitsComboBox2 = new UnitsComboBox();
            this.unitsComboBox1 = new UnitsComboBox();
            this.printWidthUpDown = new NumericUpDown();
            this.printHeightUpDown = new NumericUpDown();
            this.newWidthLabel2 = new PdnLabel();
            this.newHeightLabel2 = new PdnLabel();
            this.pixelsLabel2 = new PdnLabel();
            this.unitsLabel1 = new PdnLabel();
            this.pixelSizeHeader = new HeadingLabel();
            this.printSizeHeader = new HeadingLabel();
            this.resamplingLabel = new PdnLabel();
            this.resamplingAlgorithmComboBox = new PdnDropDownList();
            this.anchorChooserControl = new AnchorChooserControl();
            this.anchorHeader = new HeadingLabel();
            this.anchorEdgeCB = new PdnDropDownList();
            this.separatorLine = new PaintDotNet.Controls.SeparatorLine();
            this.okButton = new PdnPushButton();
            this.cancelButton = new PdnPushButton();
            this.pixelWidthUpDown.BeginInit();
            this.pixelHeightUpDown.BeginInit();
            this.percentUpDown.BeginInit();
            this.resolutionUpDown.BeginInit();
            this.printWidthUpDown.BeginInit();
            this.printHeightUpDown.BeginInit();
            base.SuspendLayout();
            this.constrainCheckBox.Name = "constrainCheckBox";
            this.constrainCheckBox.TabIndex = 0x19;
            this.constrainCheckBox.IsCheckedChanged += new EventHandler(this.OnConstrainCheckBoxIsCheckedChanged);
            this.newWidthLabel1.Name = "newWidthLabel1";
            this.newWidthLabel1.Padding = new Padding(0, 0, 0, 0);
            this.newWidthLabel1.TabIndex = 0;
            this.newWidthLabel1.TextAlign = ContentAlignment.MiddleLeft;
            this.newHeightLabel1.Name = "newHeightLabel1";
            this.newHeightLabel1.Padding = new Padding(0, 0, 0, 0);
            this.newHeightLabel1.TabIndex = 3;
            this.newHeightLabel1.TextAlign = ContentAlignment.MiddleLeft;
            this.okButton.Name = "okButton";
            this.okButton.TabIndex = 0x11;
            this.okButton.Click += new EventHandler(this.OnOkButtonClick);
            this.cancelButton.DialogResult = DialogResult.Cancel;
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.TabIndex = 0x12;
            this.cancelButton.Click += new EventHandler(this.OnCancelButtonClick);
            int[] bits = new int[4];
            bits[0] = 0x7fffffff;
            this.pixelWidthUpDown.Maximum = new decimal(bits);
            this.pixelWidthUpDown.Minimum = decimal.Zero;
            this.pixelWidthUpDown.Name = "pixelWidthUpDown";
            this.pixelWidthUpDown.TabIndex = 1;
            this.pixelWidthUpDown.TextAlign = HorizontalAlignment.Right;
            int[] numArray2 = new int[4];
            numArray2[0] = 4;
            this.pixelWidthUpDown.Value = new decimal(numArray2);
            this.pixelWidthUpDown.Enter += new EventHandler(this.OnUpDownEnter);
            this.pixelWidthUpDown.KeyUp += new KeyEventHandler(this.OnUpDownKeyUp);
            this.pixelWidthUpDown.ValueChanged += new EventHandler(this.OnUpDownValueChanged);
            this.pixelWidthUpDown.TextChanged += new EventHandler(this.OnUpDownTextChanged);
            this.pixelWidthUpDown.Leave += new EventHandler(this.OnUpDownLeave);
            int[] numArray3 = new int[4];
            numArray3[0] = 0x7fffffff;
            this.pixelHeightUpDown.Maximum = new decimal(numArray3);
            this.pixelHeightUpDown.Minimum = decimal.Zero;
            this.pixelHeightUpDown.Name = "pixelHeightUpDown";
            this.pixelHeightUpDown.TabIndex = 4;
            this.pixelHeightUpDown.TextAlign = HorizontalAlignment.Right;
            int[] numArray4 = new int[4];
            numArray4[0] = 3;
            this.pixelHeightUpDown.Value = new decimal(numArray4);
            this.pixelHeightUpDown.Enter += new EventHandler(this.OnUpDownEnter);
            this.pixelHeightUpDown.KeyUp += new KeyEventHandler(this.OnUpDownKeyUp);
            this.pixelHeightUpDown.ValueChanged += new EventHandler(this.OnUpDownValueChanged);
            this.pixelHeightUpDown.TextChanged += new EventHandler(this.OnUpDownTextChanged);
            this.pixelHeightUpDown.Leave += new EventHandler(this.OnUpDownLeave);
            this.resizedImageHeader.Name = "resizedImageHeader";
            this.resizedImageHeader.TabIndex = 20;
            this.resizedImageHeader.TabStop = false;
            this.resizedImageHeader.RightMargin = 0;
            this.asteriskLabel.Name = "asteriskLabel";
            this.asteriskLabel.Padding = new Padding(0, 0, 0, 0);
            this.asteriskLabel.TabIndex = 15;
            this.asteriskLabel.Visible = false;
            this.asteriskTextLabel.Name = "asteriskTextLabel";
            this.asteriskTextLabel.Padding = new Padding(0, 0, 0, 0);
            this.asteriskTextLabel.TabIndex = 0x10;
            this.asteriskTextLabel.Visible = false;
            this.absoluteRB.AutoSize = false;
            this.absoluteRB.IsChecked = true;
            this.absoluteRB.Name = "absoluteRB";
            this.absoluteRB.TabIndex = 0x18;
            this.absoluteRB.TabStop = true;
            this.absoluteRB.IsCheckedChanged += new EventHandler(this.OnRadioButtonIsCheckedChanged);
            this.percentRB.AutoSize = false;
            this.percentRB.Name = "percentRB";
            this.percentRB.TabIndex = 0x17;
            this.percentRB.TabStop = true;
            this.percentRB.IsCheckedChanged += new EventHandler(this.OnRadioButtonIsCheckedChanged);
            this.pixelsLabel1.Name = "pixelsLabel1";
            this.pixelsLabel1.Padding = new Padding(0, 0, 0, 0);
            this.pixelsLabel1.TabIndex = 2;
            this.pixelsLabel1.TextAlign = ContentAlignment.MiddleLeft;
            int[] numArray5 = new int[4];
            numArray5[0] = 0x7d0;
            this.percentUpDown.Maximum = new decimal(numArray5);
            this.percentUpDown.Name = "percentUpDown";
            this.percentUpDown.TabIndex = 0x17;
            this.percentUpDown.TextAlign = HorizontalAlignment.Right;
            int[] numArray6 = new int[4];
            numArray6[0] = 100;
            this.percentUpDown.Value = new decimal(numArray6);
            this.percentUpDown.Enter += new EventHandler(this.OnUpDownEnter);
            this.percentUpDown.KeyUp += new KeyEventHandler(this.OnUpDownKeyUp);
            this.percentUpDown.ValueChanged += new EventHandler(this.OnUpDownValueChanged);
            this.percentUpDown.TextChanged += new EventHandler(this.OnUpDownTextChanged);
            this.percentSignLabel.Name = "percentSignLabel";
            this.percentSignLabel.Padding = new Padding(0, 0, 0, 0);
            this.percentSignLabel.TabIndex = 13;
            this.percentSignLabel.TextAlign = ContentAlignment.MiddleLeft;
            this.resolutionLabel.Name = "resolutionLabel";
            this.resolutionLabel.Padding = new Padding(0, 0, 0, 0);
            this.resolutionLabel.TabIndex = 6;
            this.resolutionLabel.TextAlign = ContentAlignment.MiddleLeft;
            this.resolutionUpDown.DecimalPlaces = 2;
            int[] numArray7 = new int[4];
            numArray7[0] = 0xffff;
            this.resolutionUpDown.Maximum = new decimal(numArray7);
            int[] numArray8 = new int[4];
            numArray8[0] = 1;
            numArray8[3] = 0x50000;
            this.resolutionUpDown.Minimum = new decimal(numArray8);
            this.resolutionUpDown.Name = "resolutionUpDown";
            this.resolutionUpDown.TabIndex = 7;
            this.resolutionUpDown.TextAlign = HorizontalAlignment.Right;
            int[] numArray9 = new int[4];
            numArray9[0] = 0x48;
            this.resolutionUpDown.Value = new decimal(numArray9);
            this.resolutionUpDown.Enter += new EventHandler(this.OnUpDownEnter);
            this.resolutionUpDown.KeyUp += new KeyEventHandler(this.OnUpDownKeyUp);
            this.resolutionUpDown.ValueChanged += new EventHandler(this.OnUpDownValueChanged);
            this.resolutionUpDown.TextChanged += new EventHandler(this.OnUpDownTextChanged);
            this.resolutionUpDown.Leave += new EventHandler(this.OnUpDownLeave);
            this.unitsComboBox2.Name = "unitsComboBox2";
            this.unitsComboBox2.PixelsAvailable = false;
            this.unitsComboBox2.TabIndex = 8;
            this.unitsComboBox2.Units = MeasurementUnit.Inch;
            this.unitsComboBox2.UnitsDisplayType = UnitsDisplayType.Ratio;
            this.unitsComboBox2.UnitsChanged += new EventHandler(this.OnUnitsComboBox2UnitsChanged);
            this.unitsComboBox1.Name = "unitsComboBox1";
            this.unitsComboBox1.PixelsAvailable = false;
            this.unitsComboBox1.TabIndex = 12;
            this.unitsComboBox1.Units = MeasurementUnit.Inch;
            this.unitsComboBox1.UnitsChanged += new EventHandler(this.OnUnitsComboBox1UnitsChanged);
            this.printWidthUpDown.DecimalPlaces = 2;
            int[] numArray10 = new int[4];
            numArray10[0] = 0x7fffffff;
            this.printWidthUpDown.Maximum = new decimal(numArray10);
            this.printWidthUpDown.Minimum = decimal.Zero;
            this.printWidthUpDown.Name = "printWidthUpDown";
            this.printWidthUpDown.TabIndex = 11;
            this.printWidthUpDown.TextAlign = HorizontalAlignment.Right;
            int[] numArray11 = new int[4];
            numArray11[0] = 2;
            this.printWidthUpDown.Value = new decimal(numArray11);
            this.printWidthUpDown.Enter += new EventHandler(this.OnUpDownEnter);
            this.printWidthUpDown.KeyUp += new KeyEventHandler(this.OnUpDownKeyUp);
            this.printWidthUpDown.ValueChanged += new EventHandler(this.OnUpDownValueChanged);
            this.printWidthUpDown.TextChanged += new EventHandler(this.OnUpDownTextChanged);
            this.printWidthUpDown.Leave += new EventHandler(this.OnUpDownLeave);
            this.printHeightUpDown.DecimalPlaces = 2;
            int[] numArray12 = new int[4];
            numArray12[0] = 0x7fffffff;
            this.printHeightUpDown.Maximum = new decimal(numArray12);
            this.printHeightUpDown.Minimum = decimal.Zero;
            this.printHeightUpDown.Name = "printHeightUpDown";
            this.printHeightUpDown.TabIndex = 14;
            this.printHeightUpDown.TextAlign = HorizontalAlignment.Right;
            int[] numArray13 = new int[4];
            numArray13[0] = 1;
            this.printHeightUpDown.Value = new decimal(numArray13);
            this.printHeightUpDown.Enter += new EventHandler(this.OnUpDownEnter);
            this.printHeightUpDown.KeyUp += new KeyEventHandler(this.OnUpDownKeyUp);
            this.printHeightUpDown.ValueChanged += new EventHandler(this.OnUpDownValueChanged);
            this.printHeightUpDown.TextChanged += new EventHandler(this.OnUpDownTextChanged);
            this.printHeightUpDown.Leave += new EventHandler(this.OnUpDownLeave);
            this.newWidthLabel2.Name = "newWidthLabel2";
            this.newWidthLabel2.Padding = new Padding(0, 0, 0, 0);
            this.newWidthLabel2.TabIndex = 10;
            this.newWidthLabel2.TextAlign = ContentAlignment.MiddleLeft;
            this.newHeightLabel2.Name = "newHeightLabel2";
            this.newHeightLabel2.Padding = new Padding(0, 0, 0, 0);
            this.newHeightLabel2.TabIndex = 13;
            this.newHeightLabel2.TextAlign = ContentAlignment.MiddleLeft;
            this.pixelsLabel2.Name = "pixelsLabel2";
            this.pixelsLabel2.Padding = new Padding(0, 0, 0, 0);
            this.pixelsLabel2.TabIndex = 5;
            this.pixelsLabel2.TextAlign = ContentAlignment.MiddleLeft;
            this.unitsLabel1.Name = "unitsLabel1";
            this.unitsLabel1.Padding = new Padding(0, 0, 0, 0);
            this.unitsLabel1.TabIndex = 15;
            this.unitsLabel1.TextAlign = ContentAlignment.MiddleLeft;
            this.pixelSizeHeader.Name = "pixelSizeHeader";
            this.pixelSizeHeader.RightMargin = 0;
            this.pixelSizeHeader.TabIndex = 0x1a;
            this.pixelSizeHeader.TabStop = false;
            this.printSizeHeader.Name = "printSizeHeader";
            this.printSizeHeader.RightMargin = 0;
            this.printSizeHeader.TabIndex = 9;
            this.printSizeHeader.TabStop = false;
            this.resamplingLabel.Name = "resamplingLabel";
            this.resamplingLabel.Padding = new Padding(0, 0, 0, 0);
            this.resamplingLabel.TabIndex = 20;
            this.resamplingLabel.TextAlign = ContentAlignment.MiddleLeft;
            this.resamplingAlgorithmComboBox.Name = "resamplingAlgorithmComboBox";
            this.resamplingAlgorithmComboBox.Sorted = true;
            this.resamplingAlgorithmComboBox.TabIndex = 0x15;
            this.resamplingAlgorithmComboBox.SelectedIndexChanged += new EventHandler(this.OnResamplingAlgorithmComboBoxSelectedIndexChanged);
            this.anchorHeader.Name = "anchorHeader";
            this.anchorHeader.TabIndex = 15;
            this.anchorHeader.TabStop = false;
            this.anchorHeader.RightMargin = 0;
            this.anchorEdgeCB.DropDownStyle = ComboBoxStyle.DropDownList;
            this.anchorEdgeCB.Name = "anchorEdgeCB";
            this.anchorEdgeCB.TabIndex = 0x10;
            this.anchorEdgeCB.SelectedIndexChanged += new EventHandler(this.OnAnchorEdgeCBSelectedIndexChanged);
            this.anchorChooserControl.Name = "anchorChooserControl";
            this.anchorChooserControl.TabIndex = 0x11;
            this.anchorChooserControl.TabStop = false;
            this.anchorChooserControl.AnchorEdgeChanged += new EventHandler(this.OnAnchorChooserControlAnchorEdgeChanged);
            base.AcceptButton = this.okButton;
            base.CancelButton = this.cancelButton;
            Control[] controls = new Control[] { 
                this.printSizeHeader, this.pixelSizeHeader, this.unitsLabel1, this.pixelsLabel2, this.newHeightLabel2, this.newWidthLabel2, this.printHeightUpDown, this.printWidthUpDown, this.unitsComboBox1, this.unitsComboBox2, this.resolutionUpDown, this.resolutionLabel, this.resizedImageHeader, this.cancelButton, this.okButton, this.asteriskLabel,
                this.asteriskTextLabel, this.absoluteRB, this.percentRB, this.pixelWidthUpDown, this.pixelHeightUpDown, this.pixelsLabel1, this.newHeightLabel1, this.newWidthLabel1, this.resamplingAlgorithmComboBox, this.resamplingLabel, this.constrainCheckBox, this.percentUpDown, this.percentSignLabel, this.anchorHeader, this.anchorEdgeCB, this.anchorChooserControl,
                this.separatorLine
            };
            base.Controls.AddRange(controls);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "ResizeDialog";
            base.ResizeRedraw = true;
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.CenterParent;
            this.pixelWidthUpDown.EndInit();
            this.pixelHeightUpDown.EndInit();
            this.percentUpDown.EndInit();
            this.resolutionUpDown.EndInit();
            this.printWidthUpDown.EndInit();
            this.printHeightUpDown.EndInit();
            base.ResumeLayout(false);
        }

        private void InvalidateLayout()
        {
            base.VerifyAccess();
            if (this.performLayoutAction == null)
            {
                this.performLayoutAction = delegate {
                    if (base.IsHandleCreated && !base.IsDisposed)
                    {
                        base.PerformLayout();
                    }
                };
            }
            if (base.IsHandleCreated && !base.IsDisposed)
            {
                PdnSynchronizationContext.Instance.EnsurePosted(this.performLayoutAction);
            }
        }

        private void OnAnchorChooserControlAnchorEdgeChanged(object sender, EventArgs e)
        {
            LocalizedEnumValue localizedEnumValue = anchorEdgeNames.GetLocalizedEnumValue(this.anchorChooserControl.AnchorEdge);
            this.anchorEdgeCB.SelectedItem = localizedEnumValue;
        }

        private void OnAnchorEdgeCBSelectedIndexChanged(object sender, EventArgs e)
        {
            LocalizedEnumValue selectedItem = (LocalizedEnumValue) this.anchorEdgeCB.SelectedItem;
            this.AnchorEdge = (PaintDotNet.AnchorEdge) selectedItem.EnumValue;
        }

        private void OnCancelButtonClick(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.Cancel;
            base.Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if ((base.DialogResult == DialogResult.OK) && (((this.ImageWidth < 0) || (this.ImageHeight < 0)) || (!this.Resolution.IsFinite() || (this.Resolution < 0.0))))
            {
                e.Cancel = true;
            }
            base.OnClosing(e);
        }

        private void OnConstrainCheckBoxIsCheckedChanged(object sender, EventArgs e)
        {
            this.constrainer.ConstrainToAspect = this.constrainCheckBox.IsChecked;
        }

        private void OnConstrainerConstrainToAspectChanged(object sender, EventArgs e)
        {
            this.constrainCheckBox.IsChecked = this.constrainer.ConstrainToAspect;
            this.UpdateSizeText();
            this.TryToEnableOkButton();
        }

        private void OnConstrainerNewHeightChanged(object sender, EventArgs e)
        {
            double num;
            this.ignoreUpDownValueChanged++;
            if (NumericUpDownUtil.GetValueFromText(this.pixelHeightUpDown, out num) && (num != this.constrainer.NewPixelHeight))
            {
                this.SafeSetNudValue(this.pixelHeightUpDown, this.constrainer.NewPixelHeight);
            }
            if (NumericUpDownUtil.GetValueFromText(this.printHeightUpDown, out num) && (num != this.constrainer.NewHeight))
            {
                this.SafeSetNudValue(this.printHeightUpDown, this.constrainer.NewHeight);
            }
            this.ignoreUpDownValueChanged--;
            this.UpdateSizeText();
            this.TryToEnableOkButton();
        }

        private void OnConstrainerNewWidthChanged(object sender, EventArgs e)
        {
            this.ignoreUpDownValueChanged++;
            double val = 0.0;
            if (!NumericUpDownUtil.GetValueFromText(this.pixelWidthUpDown, out val) || (val != this.constrainer.NewPixelWidth))
            {
                this.SafeSetNudValue(this.pixelWidthUpDown, this.constrainer.NewPixelWidth);
            }
            if (!NumericUpDownUtil.GetValueFromText(this.printWidthUpDown, out val) || (val != this.constrainer.NewWidth))
            {
                this.SafeSetNudValue(this.printWidthUpDown, this.constrainer.NewWidth);
            }
            this.ignoreUpDownValueChanged--;
            this.UpdateSizeText();
            this.TryToEnableOkButton();
        }

        private void OnConstrainerResolutionChanged(object sender, EventArgs e)
        {
            double num;
            this.ignoreUpDownValueChanged++;
            if (NumericUpDownUtil.GetValueFromText(this.resolutionUpDown, out num) && (num != this.constrainer.Resolution))
            {
                this.SafeSetNudValue(this.resolutionUpDown, this.constrainer.Resolution);
            }
            this.ignoreUpDownValueChanged--;
            this.UpdateSizeText();
            this.TryToEnableOkButton();
        }

        private void OnConstrainerUnitsChanged(object sender, EventArgs e)
        {
            this.unitsComboBox1.Units = this.constrainer.Units;
            this.unitsComboBox2.Units = this.constrainer.Units;
            this.UpdateSizeText();
            this.TryToEnableOkButton();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int clientWidth = UIUtil.ScaleWidth(250);
            Size size = this.DoLayout(clientWidth, false);
            for (int i = 0; i < 4; i++)
            {
                Size size3 = size;
                Size size4 = this.DoLayout(size3.Width, false);
                size = size4;
                if (size3.Width == size4.Width)
                {
                    break;
                }
            }
            Size size2 = this.DoLayout(size.Width, true);
            base.ClientSize = size2;
            base.OnLayout(levent);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.ResumeLayout(true);
            base.OnLoad(e);
            this.pixelWidthUpDown.Select();
            this.pixelWidthUpDown.Select(0, this.pixelWidthUpDown.Text.Length);
            this.PopulateAsteriskLabels();
        }

        private void OnOkButtonClick(object sender, EventArgs e)
        {
            this.TryToEnableOkButton();
            if (this.okButton.Enabled)
            {
                base.DialogResult = DialogResult.OK;
                base.Close();
            }
        }

        private void OnRadioButtonIsCheckedChanged(object sender, EventArgs e)
        {
            if (this.absoluteRB.IsChecked)
            {
                this.pixelWidthUpDown.Enabled = true;
                this.pixelHeightUpDown.Enabled = true;
                this.printWidthUpDown.Enabled = true;
                this.printHeightUpDown.Enabled = true;
                this.constrainCheckBox.Enabled = true;
                this.unitsComboBox1.Enabled = true;
                this.unitsComboBox2.Enabled = true;
                this.resolutionUpDown.Enabled = true;
                this.percentUpDown.Enabled = false;
                this.absoluteRB.Select();
            }
            else if (this.percentRB.IsChecked)
            {
                this.pixelWidthUpDown.Enabled = false;
                this.pixelHeightUpDown.Enabled = false;
                this.printWidthUpDown.Enabled = false;
                this.printHeightUpDown.Enabled = false;
                this.constrainCheckBox.Enabled = false;
                this.unitsComboBox1.Enabled = false;
                this.unitsComboBox2.Enabled = false;
                this.resolutionUpDown.Enabled = false;
                this.percentUpDown.Enabled = true;
                this.percentUpDown.Select();
                if (this.getValueFromText > 0)
                {
                    double num;
                    if ((NumericUpDownUtil.GetValueFromText(this.percentUpDown, out num) && (num >= ((double) this.percentUpDown.Minimum))) && (num <= ((double) this.percentUpDown.Maximum)))
                    {
                        this.constrainer.SetByPercent(num / 100.0);
                    }
                }
                else
                {
                    this.constrainer.SetByPercent(((double) this.percentUpDown.Value) / 100.0);
                }
            }
            this.TryToEnableOkButton();
        }

        private void OnResamplingAlgorithmComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            this.PopulateAsteriskLabels();
        }

        private void OnUnitsComboBox1UnitsChanged(object sender, EventArgs e)
        {
            this.constrainer.Units = this.unitsComboBox1.Units;
            this.unitsLabel1.Text = this.unitsComboBox1.UnitsText;
            this.UpdateSizeText();
            this.TryToEnableOkButton();
        }

        private void OnUnitsComboBox2UnitsChanged(object sender, EventArgs e)
        {
            this.unitsComboBox1.Units = this.unitsComboBox2.Units;
            this.UpdateSizeText();
            this.TryToEnableOkButton();
        }

        private void OnUpDownEnter(object sender, EventArgs e)
        {
            NumericUpDown down = (NumericUpDown) sender;
            down.Select(0, down.Text.Length);
        }

        private void OnUpDownKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Tab)
            {
                double num;
                if (NumericUpDownUtil.GetValueFromText((NumericUpDown) sender, out num))
                {
                    this.UpdateSizeText();
                    this.getValueFromText++;
                    this.OnUpDownValueChanged(sender, e);
                    this.getValueFromText--;
                }
                this.TryToEnableOkButton();
            }
        }

        private void OnUpDownLeave(object sender, EventArgs e)
        {
            ((NumericUpDown) sender).Value = ((NumericUpDown) sender).Value;
            this.TryToEnableOkButton();
        }

        private void OnUpDownTextChanged(object sender, EventArgs e)
        {
            this.TryToEnableOkButton();
        }

        private void OnUpDownValueChanged(object sender, EventArgs e)
        {
            if (this.ignoreUpDownValueChanged <= 0)
            {
                double num;
                if (sender == this.percentUpDown)
                {
                    if (this.getValueFromText > 0)
                    {
                        if ((NumericUpDownUtil.GetValueFromText(this.percentUpDown, out num) && (num >= ((double) this.percentUpDown.Minimum))) && (num <= ((double) this.percentUpDown.Maximum)))
                        {
                            this.constrainer.SetByPercent(num / 100.0);
                        }
                    }
                    else
                    {
                        this.constrainer.SetByPercent(((double) this.percentUpDown.Value) / 100.0);
                    }
                }
                if (sender == this.pixelWidthUpDown)
                {
                    if (this.getValueFromText > 0)
                    {
                        if (NumericUpDownUtil.GetValueFromText(this.pixelWidthUpDown, out num))
                        {
                            this.constrainer.NewPixelWidth = num;
                        }
                    }
                    else
                    {
                        this.constrainer.NewPixelWidth = (double) this.pixelWidthUpDown.Value;
                    }
                }
                if (sender == this.pixelHeightUpDown)
                {
                    if (this.getValueFromText > 0)
                    {
                        if (NumericUpDownUtil.GetValueFromText(this.pixelHeightUpDown, out num))
                        {
                            this.constrainer.NewPixelHeight = num;
                        }
                    }
                    else
                    {
                        this.constrainer.NewPixelHeight = (double) this.pixelHeightUpDown.Value;
                    }
                }
                if (sender == this.printWidthUpDown)
                {
                    if (this.getValueFromText > 0)
                    {
                        if (NumericUpDownUtil.GetValueFromText(this.printWidthUpDown, out num))
                        {
                            this.constrainer.NewWidth = num;
                        }
                    }
                    else
                    {
                        this.constrainer.NewWidth = (double) this.printWidthUpDown.Value;
                    }
                }
                if (sender == this.printHeightUpDown)
                {
                    if (this.getValueFromText > 0)
                    {
                        if (NumericUpDownUtil.GetValueFromText(this.printHeightUpDown, out num))
                        {
                            this.constrainer.NewHeight = num;
                        }
                    }
                    else
                    {
                        this.constrainer.NewHeight = (double) this.printHeightUpDown.Value;
                    }
                }
                if (sender == this.resolutionUpDown)
                {
                    if (this.getValueFromText > 0)
                    {
                        if (NumericUpDownUtil.GetValueFromText(this.resolutionUpDown, out num) && (num >= 0.01))
                        {
                            this.constrainer.Resolution = num;
                        }
                    }
                    else if (((double) this.resolutionUpDown.Value) >= 0.01)
                    {
                        this.constrainer.Resolution = (double) this.resolutionUpDown.Value;
                    }
                }
                this.UpdateSizeText();
                this.PopulateAsteriskLabels();
                this.TryToEnableOkButton();
            }
        }

        private void PopulateAsteriskLabels()
        {
            ResampleMethod selectedItem = this.resamplingAlgorithmComboBox.SelectedItem as ResampleMethod;
            if (selectedItem != null)
            {
                if (selectedItem.method != PaintDotNet.ResamplingAlgorithm.Fant)
                {
                    this.asteriskLabel.Visible = false;
                    this.asteriskTextLabel.Visible = false;
                }
                else
                {
                    if ((this.ImageWidth < this.OriginalSize.Width) && (this.ImageHeight < this.OriginalSize.Height))
                    {
                        this.asteriskTextLabel.Text = PdnResources.GetString("ResizeDialog.AsteriskTextLabel.Fant");
                    }
                    else
                    {
                        this.asteriskTextLabel.Text = PdnResources.GetString("ResizeDialog.AsteriskTextLabel.Bicubic");
                    }
                    if (this.resamplingAlgorithmComboBox.Visible)
                    {
                        this.asteriskLabel.Visible = true;
                        this.asteriskTextLabel.Visible = true;
                    }
                }
            }
        }

        private void SafeSetNudValue(NumericUpDown nud, double value)
        {
            try
            {
                decimal num = (decimal) value;
                if ((num >= nud.Minimum) && (num <= nud.Maximum))
                {
                    nud.Value = num;
                }
            }
            catch (OverflowException)
            {
            }
        }

        private void SetupConstrainerEvents()
        {
            this.constrainer.ConstrainToAspectChanged += new EventHandler(this.OnConstrainerConstrainToAspectChanged);
            this.constrainer.NewHeightChanged += new EventHandler(this.OnConstrainerNewHeightChanged);
            this.constrainer.NewWidthChanged += new EventHandler(this.OnConstrainerNewWidthChanged);
            this.constrainer.ResolutionChanged += new EventHandler(this.OnConstrainerResolutionChanged);
            this.constrainer.UnitsChanged += new EventHandler(this.OnConstrainerUnitsChanged);
            this.constrainCheckBox.IsChecked = this.constrainer.ConstrainToAspect;
            this.SafeSetNudValue(this.pixelWidthUpDown, this.constrainer.NewPixelWidth);
            this.SafeSetNudValue(this.pixelHeightUpDown, this.constrainer.NewPixelHeight);
            this.SafeSetNudValue(this.printWidthUpDown, this.constrainer.NewWidth);
            this.SafeSetNudValue(this.printHeightUpDown, this.constrainer.NewHeight);
            this.SafeSetNudValue(this.resolutionUpDown, this.constrainer.Resolution);
            this.unitsComboBox1.Units = this.constrainer.Units;
        }

        private static string SizeStringFromBytes(long bytes)
        {
            string str;
            string str2;
            double num = bytes;
            if (num > 1073741824.0)
            {
                num /= 1073741824.0;
                str = "F1";
                str2 = PdnResources.GetString("Utility.SizeStringFromBytes.GBFormat");
            }
            else if (num > 1048576.0)
            {
                num /= 1048576.0;
                str = "F1";
                str2 = PdnResources.GetString("Utility.SizeStringFromBytes.MBFormat");
            }
            else if (num > 1024.0)
            {
                num /= 1024.0;
                str = "F1";
                str2 = PdnResources.GetString("Utility.SizeStringFromBytes.KBFormat");
            }
            else
            {
                str = "F0";
                str2 = PdnResources.GetString("Utility.SizeStringFromBytes.BytesFormat");
            }
            string str3 = num.ToString(str);
            return string.Format(str2, str3);
        }

        private void TryToEnableOkButton()
        {
            double num;
            double num2;
            double num3;
            double num4;
            double num5;
            double num6;
            bool isChecked = this.percentRB.IsChecked;
            bool valueFromText = NumericUpDownUtil.GetValueFromText(this.pixelWidthUpDown, out num);
            bool flag3 = NumericUpDownUtil.GetValueFromText(this.pixelHeightUpDown, out num2);
            bool flag4 = NumericUpDownUtil.GetValueFromText(this.printWidthUpDown, out num3);
            bool flag5 = NumericUpDownUtil.GetValueFromText(this.printHeightUpDown, out num4);
            bool flag6 = NumericUpDownUtil.GetValueFromText(this.resolutionUpDown, out num5);
            bool flag7 = NumericUpDownUtil.GetValueFromText(this.percentUpDown, out num6);
            bool flag8 = (num >= 1.0) && (num <= 65535.0);
            bool flag9 = (num2 >= 1.0) && (num2 <= 65535.0);
            bool flag10 = num3 > 0.0;
            bool flag11 = num4 > 0.0;
            bool flag12 = (num5 >= 0.01) && (num5 < 2000000.0);
            bool flag13 = (num6 >= ((double) this.percentUpDown.Minimum)) && (num6 <= ((double) this.percentUpDown.Maximum));
            bool flag14 = ((((((((((valueFromText & flag3) & flag4) & flag5) & flag6) && (flag7 || !isChecked)) & flag8) & flag9) & flag10) & flag11) & flag12) && (flag13 || !isChecked);
            this.okButton.Enabled = flag14;
        }

        private void UpdateSizeText()
        {
            long bytes = ((this.layers * 4L) * ((long) this.constrainer.NewPixelWidth)) * ((long) this.constrainer.NewPixelHeight);
            string str = SizeStringFromBytes(bytes);
            string format = PdnResources.GetString("ResizeDialog.ResizedImageHeader.Text.Format");
            this.resizedImageHeader.Text = string.Format(format, str);
        }

        [DefaultValue(0)]
        public PaintDotNet.AnchorEdge AnchorEdge
        {
            get => 
                this.anchorChooserControl.AnchorEdge;
            set
            {
                this.anchorChooserControl.AnchorEdge = value;
            }
        }

        public bool ConstrainToAspect
        {
            get => 
                this.constrainer.ConstrainToAspect;
            set
            {
                this.constrainer.ConstrainToAspect = value;
            }
        }

        public int ImageHeight
        {
            get
            {
                double num;
                if (!NumericUpDownUtil.GetValueFromText(this.pixelHeightUpDown, out num))
                {
                    num = Math.Round(this.constrainer.NewPixelHeight, MidpointRounding.AwayFromZero);
                }
                return (int) num.Clamp(-2147483648.0, 2147483647.0);
            }
            set
            {
                this.constrainer.NewPixelHeight = value;
            }
        }

        public int ImageWidth
        {
            get
            {
                double num;
                if (!NumericUpDownUtil.GetValueFromText(this.pixelWidthUpDown, out num))
                {
                    num = Math.Round(this.constrainer.NewPixelWidth, MidpointRounding.AwayFromZero);
                }
                return (int) num.Clamp(-2147483648.0, 2147483647.0);
            }
            set
            {
                this.constrainer.NewPixelWidth = value;
            }
        }

        public int LayerCount
        {
            get => 
                this.layers;
            set
            {
                this.layers = value;
                this.UpdateSizeText();
            }
        }

        public double OriginalDpu
        {
            get => 
                this.originalDpu;
            set
            {
                this.originalDpu = value;
                this.UpdateSizeText();
            }
        }

        public MeasurementUnit OriginalDpuUnit
        {
            get => 
                this.originalDpuUnit;
            set
            {
                this.originalDpuUnit = value;
                this.UpdateSizeText();
            }
        }

        public Size OriginalSize
        {
            get => 
                this.constrainer.OriginalPixelSize;
            set
            {
                this.constrainer = new ResizeConstrainer(value);
                this.SetupConstrainerEvents();
                this.UpdateSizeText();
            }
        }

        public PaintDotNet.ResamplingAlgorithm ResamplingAlgorithm
        {
            get => 
                ((ResampleMethod) this.resamplingAlgorithmComboBox.SelectedItem).method;
            set
            {
                if (value == PaintDotNet.ResamplingAlgorithm.SuperSampling)
                {
                    value = PaintDotNet.ResamplingAlgorithm.Fant;
                }
                this.resamplingAlgorithmComboBox.SelectedItem = new ResampleMethod(value);
                this.PopulateAsteriskLabels();
            }
        }

        public double Resolution
        {
            get => 
                this.constrainer.Resolution;
            set
            {
                this.constrainer.Resolution = Math.Max(0.01, value);
            }
        }

        public MeasurementUnit Units
        {
            get => 
                this.constrainer.Units;
            set
            {
                this.constrainer.Units = value;
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly ImageSizeDialog.<>c <>9 = new ImageSizeDialog.<>c();
            public static Func<ImageSizeDialog.ControlInfo, bool> <>9__82_0;
            public static Func<ImageSizeDialog.ControlInfo, Rectangle> <>9__82_1;

            internal bool <DoLayout>b__82_0(ImageSizeDialog.ControlInfo ci) => 
                ci.IsVisible;

            internal Rectangle <DoLayout>b__82_1(ImageSizeDialog.ControlInfo ci) => 
                ci.Bounds;
        }

        public sealed class ControlInfo
        {
            public ControlInfo(System.Windows.Forms.Control control, int row, Rectangle bounds)
            {
                this.Control = control;
                this.Row = row;
                this.Bounds = bounds;
                this.IsVisible = true;
            }

            public Rectangle Bounds { get; set; }

            public System.Windows.Forms.Control Control { get; private set; }

            public bool IsVisible { get; set; }

            public int Row { get; private set; }
        }

        private sealed class ResampleMethod
        {
            public readonly ResamplingAlgorithm method;

            public ResampleMethod(ResamplingAlgorithm method)
            {
                this.method = method;
            }

            public override bool Equals(object obj) => 
                ((obj is ImageSizeDialog.ResampleMethod) && (((ImageSizeDialog.ResampleMethod) obj).method == this.method));

            public override int GetHashCode() => 
                this.method.GetHashCode();

            public override string ToString()
            {
                switch (this.method)
                {
                    case ResamplingAlgorithm.NearestNeighbor:
                        return PdnResources.GetString("ResizeDialog.ResampleMethod.NearestNeighbor");

                    case ResamplingAlgorithm.Bilinear:
                        return PdnResources.GetString("ResizeDialog.ResampleMethod.Bilinear");

                    case ResamplingAlgorithm.Bicubic:
                        return PdnResources.GetString("ResizeDialog.ResampleMethod.Bicubic");

                    case ResamplingAlgorithm.SuperSampling:
                        return PdnResources.GetString("ResizeDialog.ResampleMethod.SuperSampling");

                    case ResamplingAlgorithm.Fant:
                        return PdnResources.GetString("ResizeDialog.ResampleMethod.BestQuality");
                }
                return this.method.ToString();
            }
        }

        private sealed class ResizeConstrainer
        {
            private bool constrainToAspect = false;
            public const double MinResolution = 0.01;
            private double newHeight;
            private double newWidth;
            private Size originalPixelSize;
            private double resolution;
            private MeasurementUnit units;

            [field: CompilerGenerated]
            public event EventHandler ConstrainToAspectChanged;

            [field: CompilerGenerated]
            public event EventHandler NewHeightChanged;

            [field: CompilerGenerated]
            public event EventHandler NewWidthChanged;

            [field: CompilerGenerated]
            public event EventHandler ResolutionChanged;

            [field: CompilerGenerated]
            public event EventHandler UnitsChanged;

            public ResizeConstrainer(Size originalPixelSize)
            {
                this.originalPixelSize = originalPixelSize;
                this.units = Document.DefaultDpuUnit;
                this.resolution = Document.GetDefaultDpu(this.units);
                this.newWidth = ((double) this.originalPixelSize.Width) / this.resolution;
                this.newHeight = ((double) this.originalPixelSize.Height) / this.resolution;
            }

            private void OnConstrainToAspectChanged()
            {
                this.ConstrainToAspectChanged.Raise(this);
            }

            private void OnNewHeightChanged()
            {
                this.NewHeightChanged.Raise(this);
            }

            private void OnNewWidthChanged()
            {
                this.NewWidthChanged.Raise(this);
            }

            private void OnResolutionChanged()
            {
                this.ResolutionChanged.Raise(this);
            }

            private void OnUnitsChanged()
            {
                this.UnitsChanged.Raise(this);
            }

            public void SetByPercent(double scale)
            {
                bool constrainToAspect = this.constrainToAspect;
                this.constrainToAspect = false;
                this.NewPixelWidth = this.OriginalPixelSize.Width * scale;
                this.NewPixelHeight = this.OriginalPixelSize.Height * scale;
                this.constrainToAspect = constrainToAspect;
            }

            public bool ConstrainToAspect
            {
                get => 
                    this.constrainToAspect;
                set
                {
                    if (this.constrainToAspect != value)
                    {
                        if (value)
                        {
                            double num = this.newWidth / this.OriginalAspect;
                            if (this.newHeight != num)
                            {
                                this.newHeight = num;
                                this.OnNewHeightChanged();
                            }
                        }
                        this.constrainToAspect = value;
                        this.OnConstrainToAspectChanged();
                    }
                }
            }

            public double NewHeight
            {
                get => 
                    this.newHeight;
                set
                {
                    if (this.newHeight != value)
                    {
                        this.newHeight = value;
                        this.OnNewHeightChanged();
                        if (this.constrainToAspect)
                        {
                            double num = value * this.OriginalAspect;
                            if (this.newWidth != num)
                            {
                                this.newWidth = num;
                                this.OnNewWidthChanged();
                            }
                        }
                    }
                }
            }

            public double NewPixelHeight
            {
                get
                {
                    if (this.Units == MeasurementUnit.Pixel)
                    {
                        return this.newHeight;
                    }
                    return (this.newHeight * this.resolution);
                }
                set
                {
                    if (this.Units == MeasurementUnit.Pixel)
                    {
                        this.NewHeight = value;
                    }
                    else
                    {
                        this.NewHeight = value / this.resolution;
                    }
                }
            }

            public double NewPixelWidth
            {
                get
                {
                    if (this.Units == MeasurementUnit.Pixel)
                    {
                        return this.newWidth;
                    }
                    return (this.newWidth * this.resolution);
                }
                set
                {
                    if (this.Units == MeasurementUnit.Pixel)
                    {
                        this.NewWidth = value;
                    }
                    else
                    {
                        this.NewWidth = value / this.resolution;
                    }
                }
            }

            public double NewWidth
            {
                get => 
                    this.newWidth;
                set
                {
                    if (this.newWidth != value)
                    {
                        this.newWidth = value;
                        this.OnNewWidthChanged();
                        if (this.constrainToAspect)
                        {
                            double num = value / this.OriginalAspect;
                            if (this.newHeight != num)
                            {
                                this.newHeight = num;
                                this.OnNewHeightChanged();
                            }
                        }
                    }
                }
            }

            private double OriginalAspect =>
                (((double) this.originalPixelSize.Width) / ((double) this.originalPixelSize.Height));

            public Size OriginalPixelSize =>
                this.originalPixelSize;

            public double Resolution
            {
                get => 
                    this.resolution;
                set
                {
                    if (value < 0.01)
                    {
                        throw new ArgumentOutOfRangeException("value", value, "value must be >= 0.01");
                    }
                    if (this.resolution != value)
                    {
                        if (this.Units != MeasurementUnit.Pixel)
                        {
                            this.newWidth = (this.newWidth * this.resolution) / value;
                            this.newHeight = (this.newHeight * this.resolution) / value;
                        }
                        this.resolution = value;
                        this.OnResolutionChanged();
                        if (this.Units != MeasurementUnit.Pixel)
                        {
                            this.OnNewWidthChanged();
                            this.OnNewHeightChanged();
                        }
                    }
                }
            }

            public MeasurementUnit Units
            {
                get => 
                    this.units;
                set
                {
                    if (this.units != value)
                    {
                        switch (value)
                        {
                            case MeasurementUnit.Pixel:
                                this.newWidth *= this.resolution;
                                this.newHeight *= this.resolution;
                                this.units = value;
                                this.OnUnitsChanged();
                                this.OnNewWidthChanged();
                                this.OnNewHeightChanged();
                                return;

                            case MeasurementUnit.Inch:
                                if (this.units != MeasurementUnit.Centimeter)
                                {
                                    throw ExceptionUtil.InvalidEnumArgumentException<MeasurementUnit>(this.units, "this.units");
                                }
                                this.newWidth = Document.CentimetersToInches(this.newWidth);
                                this.newHeight = Document.CentimetersToInches(this.newHeight);
                                this.units = value;
                                this.resolution = Document.InchesToCentimeters(this.resolution);
                                this.OnUnitsChanged();
                                this.OnResolutionChanged();
                                this.OnNewWidthChanged();
                                this.OnNewHeightChanged();
                                return;

                            case MeasurementUnit.Centimeter:
                                if (this.units != MeasurementUnit.Inch)
                                {
                                    throw ExceptionUtil.InvalidEnumArgumentException<MeasurementUnit>(this.units, "this.units");
                                }
                                this.newWidth = Document.InchesToCentimeters(this.newWidth);
                                this.newHeight = Document.InchesToCentimeters(this.newHeight);
                                this.units = value;
                                this.resolution = Document.CentimetersToInches(this.resolution);
                                this.OnUnitsChanged();
                                this.OnResolutionChanged();
                                this.OnNewWidthChanged();
                                this.OnNewHeightChanged();
                                break;

                            default:
                                throw ExceptionUtil.InvalidEnumArgumentException<MeasurementUnit>(value, "value");
                        }
                    }
                }
            }
        }

        public sealed class RowInfo
        {
            private List<ImageSizeDialog.ControlInfo> controls;
            private IReadOnlyList<ImageSizeDialog.ControlInfo> controlsRO;

            public RowInfo(int index)
            {
                this.Index = index;
                this.controls = new List<ImageSizeDialog.ControlInfo>();
                this.controlsRO = new ReadOnlyCollection<ImageSizeDialog.ControlInfo>(this.controls);
            }

            internal void AddControlInfo(ImageSizeDialog.ControlInfo controlInfo)
            {
                this.controls.Add(controlInfo);
            }

            public bool AreAnyControlsVisible()
            {
                foreach (ImageSizeDialog.ControlInfo info in this.controls)
                {
                    if (info.IsVisible)
                    {
                        return true;
                    }
                }
                return false;
            }

            public Rectangle Bounds
            {
                get
                {
                    int count = this.controls.Count;
                    if (count == 0)
                    {
                        return Rectangle.Empty;
                    }
                    Rectangle bounds = this.controls[0].Bounds;
                    for (int i = 1; i < count; i++)
                    {
                        bounds = Rectangle.Union(bounds, this.controls[i].Bounds);
                    }
                    return bounds;
                }
            }

            public IReadOnlyList<ImageSizeDialog.ControlInfo> Controls =>
                this.controlsRO;

            public int Index { get; private set; }

            public bool IsExpanded { get; set; }

            public int VerticalOffset { get; set; }
        }

        private sealed class TableLayoutData
        {
            private Dictionary<Control, ImageSizeDialog.ControlInfo> controls = new Dictionary<Control, ImageSizeDialog.ControlInfo>();
            private System.Collections.ObjectModel.ReadOnlyDictionary<Control, ImageSizeDialog.ControlInfo> controlsRO;
            private List<ImageSizeDialog.RowInfo> rows;
            private ReadOnlyCollection<ImageSizeDialog.RowInfo> rowsRO;

            public TableLayoutData()
            {
                this.controlsRO = new System.Collections.ObjectModel.ReadOnlyDictionary<Control, ImageSizeDialog.ControlInfo>(this.controls);
                this.rows = new List<ImageSizeDialog.RowInfo>();
                this.rowsRO = new ReadOnlyCollection<ImageSizeDialog.RowInfo>(this.rows);
            }

            public void AddControl(Control control, int rowIndex)
            {
                this.AddControl(control, rowIndex, Rectangle.Empty);
            }

            public void AddControl(Control control, int rowIndex, Rectangle bounds)
            {
                if (this.controls.ContainsKey(control))
                {
                    throw new InvalidOperationException();
                }
                ImageSizeDialog.ControlInfo info = new ImageSizeDialog.ControlInfo(control, rowIndex, bounds);
                this.controls.Add(control, info);
                this.rows.EnsureCount<ImageSizeDialog.RowInfo>(rowIndex + 1);
                ImageSizeDialog.RowInfo info2 = this.rows[rowIndex];
                if (info2 == null)
                {
                    info2 = new ImageSizeDialog.RowInfo(rowIndex);
                    this.rows[rowIndex] = info2;
                }
                info2.AddControlInfo(info);
            }

            public IReadOnlyDictionary<Control, ImageSizeDialog.ControlInfo> Controls =>
                this.controlsRO;

            public IReadOnlyList<ImageSizeDialog.RowInfo> Rows =>
                this.rowsRO;
        }
    }
}

