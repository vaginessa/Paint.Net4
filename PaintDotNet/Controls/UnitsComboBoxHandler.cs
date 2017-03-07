namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Resources;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class UnitsComboBoxHandler : IUnitsComboBox
    {
        private System.Windows.Forms.ComboBox comboBox;
        private MeasurementUnit lastUnits;
        private bool lowercase = true;
        private Dictionary<MeasurementUnit, bool> measurementItems;
        private Dictionary<string, MeasurementUnit> stringToUnits;
        private PaintDotNet.Controls.UnitsDisplayType unitsDisplayType = PaintDotNet.Controls.UnitsDisplayType.Plural;
        private Dictionary<MeasurementUnit, string> unitsToString;

        [field: CompilerGenerated]
        public event EventHandler UnitsChanged;

        public UnitsComboBoxHandler(System.Windows.Forms.ComboBox comboBox)
        {
            this.comboBox = comboBox;
            this.comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboBox.SelectedIndexChanged += new EventHandler(this.OnComboBoxSelectedIndexChanged);
            this.ReloadItems();
        }

        public void AddUnit(MeasurementUnit addMe)
        {
            this.InitMeasurementItems();
            this.measurementItems[addMe] = true;
            this.ReloadItems();
        }

        private void InitMeasurementItems()
        {
            if (this.measurementItems == null)
            {
                this.measurementItems = new Dictionary<MeasurementUnit, bool>();
                this.measurementItems.Add(MeasurementUnit.Pixel, true);
                this.measurementItems.Add(MeasurementUnit.Centimeter, true);
                this.measurementItems.Add(MeasurementUnit.Inch, true);
            }
        }

        private void OnComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.comboBox.SelectedIndex != -1)
            {
                object obj2 = this.stringToUnits[(string) this.ComboBox.SelectedItem];
                this.lastUnits = (MeasurementUnit) obj2;
            }
            this.OnUnitsChanged();
        }

        private void OnUnitsChanged()
        {
            this.UnitsChanged.Raise(this);
        }

        private void ReloadItems()
        {
            string str;
            MeasurementUnit pixel;
            switch (this.unitsDisplayType)
            {
                case PaintDotNet.Controls.UnitsDisplayType.Singular:
                    str = string.Empty;
                    break;

                case PaintDotNet.Controls.UnitsDisplayType.Plural:
                    str = ".Plural";
                    break;

                case PaintDotNet.Controls.UnitsDisplayType.Ratio:
                    str = ".Ratio";
                    break;

                default:
                    throw ExceptionUtil.InvalidEnumArgumentException<PaintDotNet.Controls.UnitsDisplayType>(this.unitsDisplayType, "this.unitsDisplayType");
            }
            this.InitMeasurementItems();
            if (this.unitsToString == null)
            {
                pixel = MeasurementUnit.Pixel;
            }
            else
            {
                pixel = this.Units;
            }
            this.ComboBox.Items.Clear();
            string str2 = PdnResources.GetString("MeasurementUnit.Pixel" + str);
            string str3 = PdnResources.GetString("MeasurementUnit.Inch" + str);
            string str4 = PdnResources.GetString("MeasurementUnit.Centimeter" + str);
            if (this.lowercase)
            {
                str2 = str2.ToLower();
                str3 = str3.ToLower();
                str4 = str4.ToLower();
            }
            this.unitsToString = new Dictionary<MeasurementUnit, string>();
            this.unitsToString.Add(MeasurementUnit.Pixel, str2);
            this.unitsToString.Add(MeasurementUnit.Inch, str3);
            this.unitsToString.Add(MeasurementUnit.Centimeter, str4);
            this.stringToUnits = new Dictionary<string, MeasurementUnit>();
            if (this.measurementItems[MeasurementUnit.Pixel])
            {
                this.stringToUnits.Add(str2, MeasurementUnit.Pixel);
                this.ComboBox.Items.Add(str2);
            }
            if (this.measurementItems[MeasurementUnit.Inch])
            {
                this.stringToUnits.Add(str3, MeasurementUnit.Inch);
                this.ComboBox.Items.Add(str3);
            }
            if (this.measurementItems[MeasurementUnit.Centimeter])
            {
                this.stringToUnits.Add(str4, MeasurementUnit.Centimeter);
                this.ComboBox.Items.Add(str4);
            }
            if (!this.measurementItems[pixel])
            {
                if (this.ComboBox.Items.Count == 0)
                {
                    this.ComboBox.SelectedItem = null;
                }
                else
                {
                    this.ComboBox.SelectedIndex = 0;
                }
            }
            else
            {
                this.Units = pixel;
            }
        }

        public void RemoveUnit(MeasurementUnit removeMe)
        {
            this.InitMeasurementItems();
            this.measurementItems[removeMe] = false;
            this.ReloadItems();
        }

        [DefaultValue(true)]
        public bool CentimetersAvailable =>
            this.measurementItems[MeasurementUnit.Centimeter];

        public System.Windows.Forms.ComboBox ComboBox =>
            this.comboBox;

        [DefaultValue(true)]
        public bool InchesAvailable =>
            this.measurementItems[MeasurementUnit.Inch];

        [DefaultValue(true)]
        public bool LowercaseStrings
        {
            get => 
                this.lowercase;
            set
            {
                if (this.lowercase != value)
                {
                    this.lowercase = value;
                    this.ReloadItems();
                }
            }
        }

        [DefaultValue(true)]
        public bool PixelsAvailable
        {
            get => 
                this.measurementItems[MeasurementUnit.Pixel];
            set
            {
                if (value != this.PixelsAvailable)
                {
                    if (value)
                    {
                        this.AddUnit(MeasurementUnit.Pixel);
                    }
                    else
                    {
                        if (this.Units == MeasurementUnit.Pixel)
                        {
                            if (this.InchesAvailable)
                            {
                                this.Units = MeasurementUnit.Inch;
                            }
                            else if (this.CentimetersAvailable)
                            {
                                this.Units = MeasurementUnit.Centimeter;
                            }
                        }
                        this.RemoveUnit(MeasurementUnit.Pixel);
                    }
                }
            }
        }

        [DefaultValue(1)]
        public MeasurementUnit Units
        {
            get => 
                this.lastUnits;
            set
            {
                object obj2 = this.unitsToString[value];
                this.ComboBox.SelectedItem = obj2;
            }
        }

        [DefaultValue(1)]
        public PaintDotNet.Controls.UnitsDisplayType UnitsDisplayType
        {
            get => 
                this.unitsDisplayType;
            set
            {
                if (this.unitsDisplayType != value)
                {
                    this.unitsDisplayType = value;
                    this.ReloadItems();
                }
            }
        }

        public string UnitsText
        {
            get
            {
                if (this.ComboBox.SelectedItem == null)
                {
                    return string.Empty;
                }
                return (string) this.ComboBox.SelectedItem;
            }
        }
    }
}

