namespace PaintDotNet.IndirectUI
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.PropertySystem;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    [PropertyControlInfo(typeof(DoubleVectorProperty), PropertyControlType.PanAndSlider, IsDefault=true)]
    internal sealed class DoubleVectorPanAndSliderPropertyControl : PropertyControl<Pair<double, double>, VectorProperty<double>>
    {
        private HeadingLabel header;
        private PanControl panControl;
        private DoubleVectorSliderPropertyControl sliders;
        private Label textDescription;

        public DoubleVectorPanAndSliderPropertyControl(PropertyControlInfo propInfo) : base(propInfo)
        {
            base.SuspendLayout();
            this.header = new HeadingLabel();
            this.header.Name = "header";
            this.header.RightMargin = 0;
            this.header.Text = base.DisplayName;
            this.panControl = new PanControl();
            this.panControl.Name = "panControl";
            this.panControl.StaticImageUnderlay = (ImageResource) propInfo.ControlProperties[ControlInfoPropertyNames.StaticImageUnderlay].Value;
            this.panControl.PositionChanged += new EventHandler(this.OnPanControlPositionChanged);
            this.panControl.Size = new Size(1, 1);
            this.sliders = new DoubleVectorSliderPropertyControl(propInfo);
            this.sliders.Name = "sliders";
            this.sliders.DisplayName = "";
            this.sliders.Description = "";
            this.textDescription = new PdnLabel();
            this.textDescription.Name = "textDescription";
            this.textDescription.Text = base.Description;
            Control[] controls = new Control[] { this.header, this.panControl, this.sliders, this.textDescription };
            base.Controls.AddRange(controls);
            base.ResumeLayout(false);
        }

        protected override void OnDescriptionChanged()
        {
            this.textDescription.Text = base.Description;
            base.OnDescriptionChanged();
        }

        protected override void OnDisplayNameChanged()
        {
            this.header.Text = base.DisplayName;
            base.OnDisplayNameChanged();
        }

        protected override bool OnFirstSelect() => 
            ((IFirstSelection) this.sliders).FirstSelect();

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int num = UIUtil.ScaleHeight(4);
            int num2 = UIUtil.ScaleWidth(4);
            Size clientSize = base.ClientSize;
            this.header.SetBounds(0, 0, clientSize.Width, this.header.GetPreferredSize(new Size(clientSize.Width, 0)).Height);
            int width = Math.Min(this.panControl.Width, this.panControl.Height);
            int num4 = 2;
            while (num4 > 0)
            {
                num4--;
                this.panControl.SetBounds(0, this.header.Bottom + num, width, width);
                this.sliders.SetBounds(this.panControl.Right + num2, this.header.Bottom + num, this.sliders.Width = clientSize.Width - this.sliders.Left, -1, BoundsSpecified.Width | BoundsSpecified.Location);
                int num5 = this.sliders.Bottom + UIUtil.ScaleHeight((int) ((20 + (this.SliderShowTickMarksX ? 0 : 4)) + (this.SliderShowTickMarksY ? 0 : 4)));
                this.textDescription.Location = new Point(0, (string.IsNullOrEmpty(base.Description) ? 0 : num) + Math.Max(this.panControl.Bottom, num5));
                this.textDescription.Width = clientSize.Width;
                this.textDescription.Height = string.IsNullOrEmpty(base.Description) ? 0 : this.textDescription.GetPreferredSize(new Size(clientSize.Width, 1)).Height;
                base.ClientSize = new Size(clientSize.Width, this.textDescription.Bottom);
                width = (this.textDescription.Top - this.panControl.Top) - num;
                width |= 1;
            }
            base.OnLayout(levent);
        }

        private void OnPanControlPositionChanged(object sender, EventArgs e)
        {
            PointF position = this.panControl.Position;
            float x = position.X.Clamp((float) base.Property.MinValueX, (float) base.Property.MaxValueX);
            PointF tf2 = new PointF(x, position.Y.Clamp((float) base.Property.MinValueY, (float) base.Property.MaxValueY));
            this.panControl.Position = tf2;
            Pair<double, double> pair = Pair.Create<double, double>((double) tf2.X, (double) tf2.Y);
            base.Property.Value = pair;
        }

        protected override void OnPropertyReadOnlyChanged()
        {
            this.header.Enabled = !base.Property.ReadOnly;
            this.panControl.Enabled = !base.Property.ReadOnly;
            this.textDescription.Enabled = !base.Property.ReadOnly;
        }

        protected override void OnPropertyValueChanged()
        {
            PointF tf = new PointF((float) base.Property.ValueX, (float) base.Property.ValueY);
            this.panControl.Position = tf;
        }

        [PropertyControlProperty(DefaultValue=2)]
        public int DecimalPlaces
        {
            get => 
                this.sliders.DecimalPlaces;
            set
            {
                this.sliders.DecimalPlaces = value;
            }
        }

        [PropertyControlProperty(DefaultValue=true)]
        public bool ShowResetButton
        {
            get => 
                this.sliders.ShowResetButton;
            set
            {
                this.sliders.ShowResetButton = value;
            }
        }

        [PropertyControlProperty(DefaultValue=5.0)]
        public double SliderLargeChangeX
        {
            get => 
                this.sliders.SliderLargeChangeX;
            set
            {
                this.sliders.SliderLargeChangeX = value;
            }
        }

        [PropertyControlProperty(DefaultValue=5.0)]
        public double SliderLargeChangeY
        {
            get => 
                this.sliders.SliderLargeChangeY;
            set
            {
                this.sliders.SliderLargeChangeY = value;
            }
        }

        [PropertyControlProperty(DefaultValue=false)]
        public bool SliderShowTickMarksX
        {
            get => 
                this.sliders.SliderShowTickMarksX;
            set
            {
                this.sliders.SliderShowTickMarksX = value;
            }
        }

        [PropertyControlProperty(DefaultValue=false)]
        public bool SliderShowTickMarksY
        {
            get => 
                this.sliders.SliderShowTickMarksY;
            set
            {
                this.sliders.SliderShowTickMarksY = value;
            }
        }

        [PropertyControlProperty(DefaultValue=1.0)]
        public double SliderSmallChangeX
        {
            get => 
                this.sliders.SliderSmallChangeX;
            set
            {
                this.sliders.SliderSmallChangeX = value;
            }
        }

        [PropertyControlProperty(DefaultValue=1.0)]
        public double SliderSmallChangeY
        {
            get => 
                this.sliders.SliderSmallChangeY;
            set
            {
                this.sliders.SliderSmallChangeY = value;
            }
        }

        [PropertyControlProperty(DefaultValue=null)]
        public ImageResource StaticImageUnderlay
        {
            get => 
                this.panControl.StaticImageUnderlay;
            set
            {
                this.panControl.StaticImageUnderlay = value;
                if (value == null)
                {
                    this.panControl.BorderStyle = BorderStyle.FixedSingle;
                }
                else
                {
                    this.panControl.BorderStyle = BorderStyle.None;
                }
            }
        }

        [PropertyControlProperty(DefaultValue=1.0)]
        public double UpDownIncrementX
        {
            get => 
                this.sliders.UpDownIncrementX;
            set
            {
                this.sliders.UpDownIncrementX = value;
            }
        }

        [PropertyControlProperty(DefaultValue=1.0)]
        public double UpDownIncrementY
        {
            get => 
                this.sliders.UpDownIncrementY;
            set
            {
                this.sliders.UpDownIncrementY = value;
            }
        }

        [PropertyControlProperty(DefaultValue=false)]
        public bool UseExponentialScale
        {
            get => 
                this.sliders.UseExponentialScale;
            set
            {
                this.sliders.UseExponentialScale = value;
            }
        }
    }
}

