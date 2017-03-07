namespace PaintDotNet.IndirectUI
{
    using PaintDotNet.Controls;
    using PaintDotNet.PropertySystem;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    [PropertyControlInfo(typeof(DoubleVector3Property), PropertyControlType.RollBallAndSliders, IsDefault=false)]
    internal sealed class DoubleVector3RollBallAndSliderPropertyControl : PropertyControl<Tuple<double, double, double>, Vector3Property<double>>
    {
        private HeadingLabel header;
        private RollControl rollControl;
        private DoubleVector3SliderPropertyControl sliders;
        private Label textDescription;

        public DoubleVector3RollBallAndSliderPropertyControl(PropertyControlInfo propInfo) : base(propInfo)
        {
            base.SuspendLayout();
            this.header = new HeadingLabel();
            this.header.Name = "header";
            this.header.RightMargin = 0;
            this.header.Text = base.DisplayName;
            this.rollControl = new RollControl();
            this.rollControl.Name = "rollControl";
            this.rollControl.ValueChanged += new EventHandler(this.OnRollControlValueChanged);
            this.rollControl.Size = new Size(1, 1);
            this.sliders = new DoubleVector3SliderPropertyControl(propInfo);
            this.sliders.Name = "sliders";
            this.sliders.DisplayName = "";
            this.sliders.Description = "";
            this.textDescription = new PdnLabel();
            this.textDescription.Name = "textDescription";
            this.textDescription.Text = base.Description;
            Control[] controls = new Control[] { this.header, this.rollControl, this.sliders, this.textDescription };
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
            int width = Math.Min(this.rollControl.Width, this.rollControl.Height);
            int num4 = 3;
            while (num4 > 0)
            {
                num4--;
                this.rollControl.Location = new Point(0, this.header.Bottom + num);
                this.rollControl.Size = new Size(width, width);
                this.rollControl.PerformLayout();
                this.sliders.Location = new Point(this.rollControl.Right + num2, this.header.Bottom + num);
                this.sliders.Width = clientSize.Width - this.sliders.Left;
                this.sliders.PerformLayout();
                int num5 = this.sliders.Bottom + UIUtil.ScaleHeight((int) ((20 + (this.SliderShowTickMarksX ? 0 : 4)) + (this.SliderShowTickMarksY ? 0 : 4)));
                this.textDescription.Location = new Point(0, (string.IsNullOrEmpty(base.Description) ? 0 : num) + Math.Max(this.rollControl.Bottom, this.sliders.Bottom));
                this.textDescription.Width = clientSize.Width;
                this.textDescription.Height = string.IsNullOrEmpty(base.Description) ? 0 : this.textDescription.GetPreferredSize(new Size(clientSize.Width, 1)).Height;
                this.rollControl.Top += ((this.sliders.Bottom - this.rollControl.Top) - this.rollControl.Height) / 2;
                base.ClientSize = new Size(clientSize.Width, this.textDescription.Bottom);
                width = ((this.textDescription.Top - this.rollControl.Top) - num) / 1;
                width |= 1;
            }
            base.OnLayout(levent);
        }

        protected override void OnPropertyReadOnlyChanged()
        {
            this.header.Enabled = !base.Property.ReadOnly;
            this.rollControl.Enabled = !base.Property.ReadOnly;
            this.textDescription.Enabled = !base.Property.ReadOnly;
        }

        protected override void OnPropertyValueChanged()
        {
            Tuple<double, double, double> tuple = base.Property.Value;
            this.rollControl.Angle = tuple.Item1;
            this.rollControl.RollDirection = tuple.Item2;
            this.rollControl.RollAmount = tuple.Item3;
        }

        private void OnRollControlValueChanged(object sender, EventArgs e)
        {
            double angle = this.rollControl.Angle;
            double rollDirection = this.rollControl.RollDirection;
            double rollAmount = this.rollControl.RollAmount;
            Tuple<double, double, double> tuple = Tuple.Create<double, double, double>(angle, rollDirection, rollAmount);
            base.Property.Value = tuple;
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

        [PropertyControlProperty(DefaultValue=5.0)]
        public double SliderLargeChangeZ
        {
            get => 
                this.sliders.SliderLargeChangeZ;
            set
            {
                this.sliders.SliderLargeChangeZ = value;
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

        [PropertyControlProperty(DefaultValue=false)]
        public bool SliderShowTickMarksZ
        {
            get => 
                this.sliders.SliderShowTickMarksZ;
            set
            {
                this.sliders.SliderShowTickMarksZ = value;
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

        [PropertyControlProperty(DefaultValue=1.0)]
        public double SliderSmallChangeZ
        {
            get => 
                this.sliders.SliderSmallChangeZ;
            set
            {
                this.sliders.SliderSmallChangeZ = value;
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

        [PropertyControlProperty(DefaultValue=1.0)]
        public double UpDownIncrementZ
        {
            get => 
                this.sliders.UpDownIncrementZ;
            set
            {
                this.sliders.UpDownIncrementZ = value;
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

