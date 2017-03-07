namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal sealed class UnitsComboBox : UserControl, IUnitsComboBox
    {
        private PdnDropDownList comboBox = new PdnDropDownList();
        private UnitsComboBoxHandler comboBoxHandler;

        public event EventHandler UnitsChanged
        {
            add
            {
                this.comboBoxHandler.UnitsChanged += value;
            }
            remove
            {
                this.comboBoxHandler.UnitsChanged -= value;
            }
        }

        public UnitsComboBox()
        {
            this.comboBox.Name = "comboBox";
            base.Controls.Add(this.comboBox);
            this.comboBoxHandler = new UnitsComboBoxHandler(this.comboBox);
        }

        public void AddUnit(MeasurementUnit addMe)
        {
            this.comboBoxHandler.AddUnit(addMe);
        }

        public override Size GetPreferredSize(Size proposedSize) => 
            this.comboBox.GetPreferredSize(proposedSize);

        protected override void OnLayout(LayoutEventArgs e)
        {
            this.comboBox.Bounds = new Rectangle(new Point(0, 0), base.Size);
            if (this.comboBox.Size != base.Size)
            {
                base.Size = this.comboBox.Size;
            }
            base.OnLayout(e);
        }

        public void RemoveUnit(MeasurementUnit removeMe)
        {
            this.comboBoxHandler.AddUnit(removeMe);
        }

        public bool CentimetersAvailable =>
            this.comboBoxHandler.CentimetersAvailable;

        public bool InchesAvailable =>
            this.comboBoxHandler.InchesAvailable;

        public bool LowercaseStrings
        {
            get => 
                this.comboBoxHandler.LowercaseStrings;
            set
            {
                this.comboBoxHandler.LowercaseStrings = value;
            }
        }

        public bool PixelsAvailable
        {
            get => 
                this.comboBoxHandler.PixelsAvailable;
            set
            {
                this.comboBoxHandler.PixelsAvailable = value;
            }
        }

        public MeasurementUnit Units
        {
            get => 
                this.comboBoxHandler.Units;
            set
            {
                this.comboBoxHandler.Units = value;
            }
        }

        public PaintDotNet.Controls.UnitsDisplayType UnitsDisplayType
        {
            get => 
                this.comboBoxHandler.UnitsDisplayType;
            set
            {
                this.comboBoxHandler.UnitsDisplayType = value;
            }
        }

        public string UnitsText =>
            this.comboBoxHandler.UnitsText;
    }
}

