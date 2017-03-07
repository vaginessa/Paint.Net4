namespace PaintDotNet.IndirectUI
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.PropertySystem;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.Globalization;
    using System.Windows.Forms;

    [PropertyControlInfo(typeof(StaticListChoiceProperty), PropertyControlType.RadioButton)]
    internal sealed class StaticListRadioButtonPropertyControl : PropertyControl<object, StaticListChoiceProperty>
    {
        private Label descriptionText;
        private HeadingLabel header;
        private PdnRadioButton[] radioButtons;

        public StaticListRadioButtonPropertyControl(PropertyControlInfo propInfo) : base(propInfo)
        {
            base.SuspendLayout();
            this.header = new HeadingLabel();
            this.header.Name = "header";
            this.header.RightMargin = 0;
            this.header.Text = base.DisplayName;
            object[] valueChoices = base.Property.ValueChoices;
            this.radioButtons = new PdnRadioButton[valueChoices.Length];
            for (int i = 0; i < this.radioButtons.Length; i++)
            {
                this.radioButtons[i] = new PdnRadioButton();
                this.radioButtons[i].Name = "radioButton" + i.ToString(CultureInfo.InvariantCulture);
                this.radioButtons[i].IsCheckedChanged += new EventHandler(this.OnRadioButtonCheckedChanged);
                string valueDisplayName = propInfo.GetValueDisplayName(valueChoices[i]);
                this.radioButtons[i].Text = valueDisplayName;
            }
            this.descriptionText = new PdnLabel();
            this.descriptionText.Name = "descriptionText";
            this.descriptionText.AutoSize = false;
            this.descriptionText.Text = base.Description;
            base.Controls.Add(this.header);
            base.Controls.AddRange(this.radioButtons);
            base.Controls.Add(this.descriptionText);
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        protected override bool OnFirstSelect()
        {
            foreach (PdnRadioButton button in this.radioButtons)
            {
                if (button.IsChecked)
                {
                    button.Select();
                    return true;
                }
            }
            return false;
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int num = UIUtil.ScaleHeight(4);
            Size clientSize = base.ClientSize;
            this.header.SetBounds(0, 0, clientSize.Width, this.header.GetPreferredSize(new Size(clientSize.Width, 0)).Height);
            int bottom = this.header.Bottom;
            for (int i = 0; i < this.radioButtons.Length; i++)
            {
                this.radioButtons[i].Location = new Point(0, bottom + num);
                this.radioButtons[i].Width = clientSize.Width;
                this.radioButtons[i].Height = this.radioButtons[i].GetPreferredSize(this.radioButtons[i].Width, 1).Height;
                bottom = this.radioButtons[i].Bottom;
            }
            this.descriptionText.Location = new Point(0, bottom + (string.IsNullOrEmpty(this.descriptionText.Text) ? 0 : num));
            this.descriptionText.Width = clientSize.Width;
            this.descriptionText.Height = string.IsNullOrEmpty(this.descriptionText.Text) ? 0 : this.descriptionText.GetPreferredSize(new Size(clientSize.Width, 1)).Height;
            bottom = this.descriptionText.Bottom;
            base.ClientSize = new Size(clientSize.Width, bottom);
            base.OnLayout(levent);
        }

        protected override void OnPropertyReadOnlyChanged()
        {
            this.header.Enabled = !base.Property.ReadOnly;
            foreach (PdnRadioButton button in this.radioButtons)
            {
                button.Enabled = !base.Property.ReadOnly;
            }
            this.descriptionText.Enabled = !base.Property.ReadOnly;
        }

        protected override void OnPropertyValueChanged()
        {
            int index = Array.IndexOf<object>(base.Property.ValueChoices, base.Property.Value);
            if (((index >= 0) && (index < this.radioButtons.Length)) && !this.radioButtons[index].IsChecked)
            {
                this.radioButtons[index].IsChecked = true;
            }
        }

        private void OnRadioButtonCheckedChanged(object sender, EventArgs e)
        {
            PdnRadioButton button = (PdnRadioButton) sender;
            if (button.IsChecked)
            {
                int index = Array.IndexOf<PdnRadioButton>(this.radioButtons, button);
                object obj2 = base.Property.ValueChoices[index];
                if (!base.Property.Value.Equals(obj2))
                {
                    base.Property.Value = obj2;
                }
            }
        }
    }
}

