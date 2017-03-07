namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using System;

    internal sealed class UnitsComboBoxStrip : PdnToolStripComboBox, IUnitsComboBox
    {
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

        public UnitsComboBoxStrip() : base(false)
        {
            this.comboBoxHandler = new UnitsComboBoxHandler(base.ComboBox);
        }

        public void AddUnit(MeasurementUnit addMe)
        {
            this.comboBoxHandler.AddUnit(addMe);
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

