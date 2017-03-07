namespace PaintDotNet.IndirectUI
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.PropertySystem;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    [PropertyControlInfo(typeof(Int32Property), PropertyControlType.ColorWheel)]
    internal sealed class Int32ColorWheelPropertyControl : PropertyControl<int, ScalarProperty<int>>
    {
        private Label blueLabel;
        private PdnNumericUpDown blueNud;
        private int changingStack;
        private ColorRectangleControl colorRectangle;
        private Label description;
        private Label greenLabel;
        private PdnNumericUpDown greenNud;
        private HeadingLabel header;
        private ColorWheel hsvColorWheel;
        private int inOnPropertyValueChanged;
        private Label redLabel;
        private PdnNumericUpDown redNud;
        private const int requiredMax = 0xffffff;
        private const int requiredMin = 0;
        private PdnPushButton resetButton;
        private ColorGradientControl saturationSlider;
        private ColorGradientControl valueSlider;

        public Int32ColorWheelPropertyControl(PropertyControlInfo propInfo) : base(propInfo)
        {
            if ((base.Property.MinValue != 0) || (base.Property.MaxValue != 0xffffff))
            {
                object[] objArray1 = new object[] { "The only range allowed for this control is [", 0, ", ", 0xffffff, "]" };
                throw new ArgumentException(string.Concat(objArray1));
            }
            base.SuspendLayout();
            this.header = new HeadingLabel();
            this.header.Name = "header";
            this.header.RightMargin = 0;
            this.header.Text = base.DisplayName;
            this.colorRectangle = new ColorRectangleControl();
            this.colorRectangle.Name = "colorRectangle";
            this.colorRectangle.TabStop = false;
            this.colorRectangle.TabIndex = 0;
            this.hsvColorWheel = new ColorWheel();
            this.hsvColorWheel.Name = "hsvColorWheel";
            this.hsvColorWheel.ColorChanged += new EventHandler(this.OnHsvColorWheelColorChanged);
            this.hsvColorWheel.TabStop = false;
            this.hsvColorWheel.TabIndex = 1;
            this.saturationSlider = new ColorGradientControl();
            this.saturationSlider.Name = "saturationSlider";
            this.saturationSlider.Orientation = Orientation.Vertical;
            this.saturationSlider.ValueChanged += new IndexEventHandler(this.OnSaturationSliderValueChanged);
            this.saturationSlider.TabStop = false;
            this.saturationSlider.TabIndex = 2;
            this.valueSlider = new ColorGradientControl();
            this.valueSlider.Name = "valueSlider";
            this.valueSlider.Orientation = Orientation.Vertical;
            this.valueSlider.ValueChanged += new IndexEventHandler(this.OnValueSliderValueChanged);
            this.valueSlider.TabStop = false;
            this.valueSlider.TabIndex = 3;
            this.redLabel = new PdnLabel();
            this.redLabel.Name = "redLabel";
            this.redLabel.AutoSize = true;
            this.redLabel.Text = PdnResources.GetString("ColorsForm.RedLabel.Text");
            this.redNud = new PdnNumericUpDown();
            this.redNud.Name = "redNud";
            this.redNud.Minimum = decimal.Zero;
            this.redNud.Maximum = 255M;
            this.redNud.TextAlign = HorizontalAlignment.Right;
            this.redNud.ValueChanged += new EventHandler(this.OnRedNudValueChanged);
            this.redNud.TabIndex = 4;
            this.greenLabel = new PdnLabel();
            this.greenLabel.Name = "greenLabel";
            this.greenLabel.AutoSize = true;
            this.greenLabel.Text = PdnResources.GetString("ColorsForm.GreenLabel.Text");
            this.greenNud = new PdnNumericUpDown();
            this.greenNud.Name = "greenNud";
            this.greenNud.Minimum = decimal.Zero;
            this.greenNud.Maximum = 255M;
            this.greenNud.TextAlign = HorizontalAlignment.Right;
            this.greenNud.ValueChanged += new EventHandler(this.OnGreenNudValueChanged);
            this.greenNud.TabIndex = 5;
            this.blueLabel = new PdnLabel();
            this.blueLabel.Name = "blueLabel";
            this.blueLabel.AutoSize = true;
            this.blueLabel.Text = PdnResources.GetString("ColorsForm.BlueLabel.Text");
            this.blueNud = new PdnNumericUpDown();
            this.blueNud.Name = "blueNud";
            this.blueNud.Minimum = decimal.Zero;
            this.blueNud.Maximum = 255M;
            this.blueNud.TextAlign = HorizontalAlignment.Right;
            this.blueNud.ValueChanged += new EventHandler(this.OnBlueNudValueChanged);
            this.blueNud.TabIndex = 6;
            this.resetButton = new PdnPushButton();
            this.resetButton.AutoSize = true;
            this.resetButton.Name = "resetButton";
            this.resetButton.Click += new EventHandler(this.OnResetButtonClick);
            this.resetButton.Image = PdnResources.GetImage("Icons.ResetIcon.png");
            this.resetButton.Width = 1;
            this.resetButton.Visible = (bool) propInfo.ControlProperties[ControlInfoPropertyNames.ShowResetButton].Value;
            base.ToolTip.SetToolTip(this.resetButton, PdnResources.GetString("Form.ResetButton.Text").Replace("&", ""));
            this.resetButton.TabIndex = 7;
            this.description = new PdnLabel();
            this.description.Name = "description";
            this.description.Text = base.Description;
            Control[] controls = new Control[] { this.header, this.hsvColorWheel, this.saturationSlider, this.valueSlider, this.colorRectangle, this.redLabel, this.redNud, this.greenLabel, this.greenNud, this.blueLabel, this.blueNud, this.resetButton, this.description };
            base.Controls.AddRange(controls);
            base.ResumeLayout(false);
        }

        private void OnBlueNudValueChanged(object sender, EventArgs e)
        {
            if (this.changingStack == 0)
            {
                this.changingStack++;
                this.SetPropertyValueFromRgb((int) this.redNud.Value, (int) this.greenNud.Value, (int) this.blueNud.Value);
                this.changingStack--;
            }
        }

        protected override bool OnFirstSelect()
        {
            this.redNud.Select();
            return true;
        }

        private void OnGreenNudValueChanged(object sender, EventArgs e)
        {
            if (this.changingStack == 0)
            {
                this.changingStack++;
                this.SetPropertyValueFromRgb((int) this.redNud.Value, (int) this.greenNud.Value, (int) this.blueNud.Value);
                this.changingStack--;
            }
        }

        private void OnHsvColorWheelColorChanged(object sender, EventArgs e)
        {
            if (this.changingStack == 0)
            {
                this.changingStack++;
                Int32HsvColor hsvColor = this.hsvColorWheel.HsvColor;
                this.SetPropertyValueFromHsv(hsvColor);
                this.changingStack--;
            }
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            int num = UIUtil.ScaleHeight(4);
            int num2 = UIUtil.ScaleWidth(4);
            Size clientSize = base.ClientSize;
            this.header.SetBounds(0, 0, clientSize.Width, this.header.GetPreferredSize(new Size(clientSize.Width, 0)).Height);
            if (this.resetButton.Visible)
            {
                this.resetButton.PerformLayout();
            }
            else
            {
                this.resetButton.Size = new Size(0, 0);
            }
            int num4 = Math.Max(UIUtil.ScaleWidth(50), this.resetButton.Width);
            this.redNud.PerformLayout();
            this.redNud.Width = num4;
            this.redNud.Location = new Point(clientSize.Width - this.redNud.Width, this.header.Bottom + num);
            this.redLabel.PerformLayout();
            this.redLabel.Location = new Point((this.redNud.Left - this.redLabel.Width) - num2, this.redNud.Top + ((this.redNud.Height - this.redLabel.Height) / 2));
            this.greenNud.PerformLayout();
            this.greenNud.Width = num4;
            this.greenNud.Location = new Point(clientSize.Width - this.greenNud.Width, Math.Max(this.redNud.Bottom, this.redLabel.Bottom) + num);
            this.greenLabel.PerformLayout();
            this.greenLabel.Location = new Point((this.greenNud.Left - this.greenLabel.Width) - num2, this.greenNud.Top + ((this.greenNud.Height - this.greenLabel.Height) / 2));
            this.blueNud.PerformLayout();
            this.blueNud.Width = num4;
            this.blueNud.Location = new Point(clientSize.Width - this.blueNud.Width, Math.Max(this.greenNud.Bottom, this.greenLabel.Bottom) + num);
            this.blueLabel.PerformLayout();
            this.blueLabel.Location = new Point((this.blueNud.Left - this.blueLabel.Width) - num2, this.blueNud.Top + ((this.blueNud.Height - this.blueLabel.Height) / 2));
            this.resetButton.Location = new Point(clientSize.Width - this.resetButton.Width, Math.Max(this.blueNud.Bottom, this.blueLabel.Bottom) + num);
            this.resetButton.Width = Math.Max(this.resetButton.Width, num4);
            int num5 = Math.Min(this.redLabel.Left, Math.Min(this.greenLabel.Left, this.blueLabel.Left));
            int num6 = 0;
            int num7 = num5 - num6;
            this.colorRectangle.Top = this.header.Bottom + num;
            this.hsvColorWheel.Top = this.header.Bottom;
            int num8 = this.resetButton.Bottom - this.hsvColorWheel.Top;
            this.colorRectangle.Size = UIUtil.ScaleSize(new Size(0x1c, 0x1c));
            this.hsvColorWheel.Size = new Size(num8 + UIUtil.ScaleHeight(2), num8 + UIUtil.ScaleHeight(2));
            this.saturationSlider.Top = this.header.Bottom + num;
            this.saturationSlider.Size = new Size(UIUtil.ScaleWidth(20), num8 - num);
            this.valueSlider.Top = this.header.Bottom + num;
            this.valueSlider.Size = new Size(UIUtil.ScaleWidth(20), num8 - num);
            int num9 = (((((this.colorRectangle.Width + num2) + this.hsvColorWheel.Width) + num2) + this.saturationSlider.Width) + num2) + this.valueSlider.Width;
            int num10 = num6 + ((num7 - num9) / 2);
            this.colorRectangle.Left = num10;
            this.hsvColorWheel.Left = this.colorRectangle.Right + num2;
            this.saturationSlider.Left = this.hsvColorWheel.Right + num2;
            this.valueSlider.Left = this.saturationSlider.Right + num2;
            this.description.Location = new Point(0, (string.IsNullOrEmpty(base.Description) ? 0 : UIUtil.ScaleHeight(2)) + this.hsvColorWheel.Bottom);
            this.description.Width = clientSize.Width;
            this.description.Height = string.IsNullOrEmpty(base.Description) ? 0 : this.description.GetPreferredSize(new Size(clientSize.Width, 1)).Height;
            base.ClientSize = new Size(clientSize.Width, this.description.Bottom);
            base.OnLayout(e);
        }

        protected override void OnPropertyReadOnlyChanged()
        {
            this.header.Enabled = !base.Property.ReadOnly;
            this.hsvColorWheel.Enabled = !base.Property.ReadOnly;
            this.valueSlider.Enabled = !base.Property.ReadOnly;
            this.saturationSlider.Enabled = !base.Property.ReadOnly;
            this.colorRectangle.Enabled = !base.Property.ReadOnly;
            this.redNud.Enabled = !base.Property.ReadOnly;
            this.redLabel.Enabled = !base.Property.ReadOnly;
            this.greenNud.Enabled = !base.Property.ReadOnly;
            this.greenLabel.Enabled = !base.Property.ReadOnly;
            this.blueNud.Enabled = !base.Property.ReadOnly;
            this.blueLabel.Enabled = !base.Property.ReadOnly;
            this.resetButton.Enabled = !base.Property.ReadOnly;
            this.description.Enabled = !base.Property.ReadOnly;
        }

        protected override void OnPropertyValueChanged()
        {
            this.inOnPropertyValueChanged++;
            try
            {
                int num = base.Property.Value;
                int red = (num >> 0x10) & 0xff;
                int green = (num >> 8) & 0xff;
                int blue = num & 0xff;
                this.SetPropertyValueFromRgb(red, green, blue);
                this.colorRectangle.RectangleColor = ColorBgra.FromBgr((byte) blue, (byte) green, (byte) red).ToColor();
            }
            finally
            {
                this.inOnPropertyValueChanged--;
            }
        }

        private void OnRedNudValueChanged(object sender, EventArgs e)
        {
            if (this.changingStack == 0)
            {
                this.changingStack++;
                this.SetPropertyValueFromRgb((int) this.redNud.Value, (int) this.greenNud.Value, (int) this.blueNud.Value);
                this.changingStack--;
            }
        }

        private void OnResetButtonClick(object sender, EventArgs e)
        {
            if (base.Property.Value != base.Property.DefaultValue)
            {
                base.Property.Value = base.Property.DefaultValue;
            }
        }

        private void OnSaturationSliderValueChanged(object sender, IndexEventArgs ce)
        {
            if (this.changingStack == 0)
            {
                this.changingStack++;
                Int32HsvColor hsvColor = this.hsvColorWheel.HsvColor;
                hsvColor.Saturation = (this.saturationSlider.Value * 100) / 0xff;
                this.SetPropertyValueFromHsv(hsvColor);
                this.changingStack--;
            }
        }

        private void OnValueSliderValueChanged(object sender, IndexEventArgs ce)
        {
            if (this.changingStack == 0)
            {
                this.changingStack++;
                Int32HsvColor hsvColor = this.hsvColorWheel.HsvColor;
                hsvColor.Value = (this.valueSlider.Value * 100) / 0xff;
                this.SetPropertyValueFromHsv(hsvColor);
                this.changingStack--;
            }
        }

        private void SetPropertyValueFromHsv(Int32HsvColor hsv)
        {
            UIUtil.SuspendControlPainting(this);
            try
            {
                Int32RgbColor color = hsv.ToRgb();
                this.SetPropertyValueFromRgb(color.Red, color.Green, color.Blue);
                if (this.hsvColorWheel.HsvColor != hsv)
                {
                    this.hsvColorWheel.HsvColor = hsv;
                }
                if (this.valueSlider.Value != ((hsv.Value * 0xff) / 100))
                {
                    this.valueSlider.Value = (hsv.Value * 0xff) / 100;
                }
                if (this.saturationSlider.Value != ((hsv.Saturation * 0xff) / 100))
                {
                    this.saturationSlider.Value = (hsv.Saturation * 0xff) / 100;
                }
                Int32HsvColor color2 = hsv;
                color2.Value = 0;
                Int32HsvColor color3 = hsv;
                color3.Value = 100;
                this.valueSlider.MinColor = color2.ToGdipColor();
                this.valueSlider.MaxColor = color3.ToGdipColor();
                Int32HsvColor color4 = hsv;
                color4.Saturation = 0;
                Int32HsvColor color5 = hsv;
                color5.Saturation = 100;
                this.saturationSlider.MinColor = color4.ToGdipColor();
                this.saturationSlider.MaxColor = color5.ToGdipColor();
            }
            finally
            {
                UIUtil.ResumeControlPainting(this);
                if (UIUtil.IsControlPaintingEnabled(this))
                {
                    this.Refresh();
                }
            }
        }

        private void SetPropertyValueFromRgb(int red, int green, int blue)
        {
            UIUtil.SuspendControlPainting(this);
            try
            {
                if (this.redNud.Value != red)
                {
                    this.redNud.Value = red;
                }
                if (this.greenNud.Value != green)
                {
                    this.greenNud.Value = green;
                }
                if (this.blueNud.Value != blue)
                {
                    this.blueNud.Value = blue;
                }
                if (this.inOnPropertyValueChanged == 0)
                {
                    int num = ((red << 0x10) | (green << 8)) | blue;
                    if (base.Property.Value != num)
                    {
                        base.Property.Value = num;
                    }
                }
                Int32HsvColor color2 = new Int32RgbColor(red, green, blue).ToHsv();
                this.hsvColorWheel.HsvColor = color2;
                this.valueSlider.Value = (color2.Value * 0xff) / 100;
                this.saturationSlider.Value = (color2.Saturation * 0xff) / 100;
                Int32HsvColor color3 = color2;
                color3.Value = 0;
                Int32HsvColor color4 = color2;
                color4.Value = 100;
                this.valueSlider.MinColor = color3.ToGdipColor();
                this.valueSlider.MaxColor = color4.ToGdipColor();
                Int32HsvColor color5 = color2;
                color5.Saturation = 0;
                Int32HsvColor color6 = color2;
                color6.Saturation = 100;
                this.saturationSlider.MinColor = color5.ToGdipColor();
                this.saturationSlider.MaxColor = color6.ToGdipColor();
            }
            finally
            {
                UIUtil.ResumeControlPainting(this);
                if (UIUtil.IsControlPaintingEnabled(this))
                {
                    this.Refresh();
                }
            }
        }

        [PropertyControlProperty(DefaultValue=true)]
        public bool ShowResetButton
        {
            get => 
                this.resetButton.Visible;
            set
            {
                this.resetButton.Visible = value;
                this.resetButton.AutoSize = value;
                base.PerformLayout();
            }
        }
    }
}

