namespace PaintDotNet.IndirectUI
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.PropertySystem;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    [PropertyControlInfo(typeof(StaticListChoiceProperty), PropertyControlType.DropDown, IsDefault=true)]
    internal sealed class StaticListDropDownPropertyControl : PropertyControl<object, StaticListChoiceProperty>
    {
        private int? cachedMaxWidthOfComboBoxItems;
        private PdnDropDownList comboBox;
        private Label descriptionText;
        private HeadingLabel header;

        public StaticListDropDownPropertyControl(PropertyControlInfo propInfo) : base(propInfo)
        {
            base.SuspendLayout();
            this.header = new HeadingLabel();
            this.header.Name = "header";
            this.header.RightMargin = 0;
            this.header.Text = base.DisplayName;
            this.comboBox = new PdnDropDownList();
            this.comboBox.Name = "comboBox";
            this.comboBox.SelectedIndexChanged += new EventHandler(this.OnComboBoxSelectedIndexChanged);
            this.comboBox.BeginUpdate();
            foreach (object obj2 in base.Property.ValueChoices)
            {
                string valueDisplayName = propInfo.GetValueDisplayName(obj2);
                this.comboBox.Items.Add(valueDisplayName);
            }
            this.comboBox.EndUpdate();
            this.descriptionText = new PdnLabel();
            this.descriptionText.Name = "descriptionText";
            this.descriptionText.AutoSize = false;
            this.descriptionText.Text = base.Description;
            Control[] controls = new Control[] { this.header, this.comboBox, this.descriptionText };
            base.Controls.AddRange(controls);
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private int GetMaxWidthOfComboBoxItems()
        {
            if (!this.cachedMaxWidthOfComboBoxItems.HasValue)
            {
                this.cachedMaxWidthOfComboBoxItems = new int?(this.comboBox.GetMaxWidthOfComboBoxItems());
            }
            return this.cachedMaxWidthOfComboBoxItems.Value;
        }

        private void OnComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            if (Array.IndexOf<object>(base.Property.ValueChoices, base.Property.Value) != this.comboBox.SelectedIndex)
            {
                base.Property.Value = base.Property.ValueChoices[this.comboBox.SelectedIndex];
            }
        }

        protected override bool OnFirstSelect()
        {
            this.comboBox.Select();
            return true;
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            if (base.Parent != null)
            {
                base.Parent.SuspendLayout();
                if (levent.AffectedProperty == "Font")
                {
                    this.cachedMaxWidthOfComboBoxItems = null;
                }
                Size clientSize = base.ClientSize;
                int num = UIUtil.ScaleHeight(4);
                this.header.SetBounds(0, 0, clientSize.Width, this.header.GetPreferredSize(new Size(clientSize.Width, 0)).Height);
                this.comboBox.Location = new Point(0, this.header.Bottom + num);
                int num3 = Int32Util.ClampSafe(Math.Max(this.GetMaxWidthOfComboBoxItems(), 1) + UIUtil.ScaleWidth(30), 1, clientSize.Width);
                this.comboBox.Width = num3;
                this.comboBox.PerformLayout();
                this.descriptionText.Location = new Point(0, this.comboBox.Bottom + (string.IsNullOrEmpty(this.descriptionText.Text) ? 0 : num));
                this.descriptionText.Width = clientSize.Width;
                this.descriptionText.Height = string.IsNullOrEmpty(this.descriptionText.Text) ? 0 : this.descriptionText.GetPreferredSize(new Size(clientSize.Width, 1)).Height;
                base.ClientSize = new Size(clientSize.Width, this.descriptionText.Bottom);
                base.OnLayout(levent);
                base.Parent.ResumeLayout(false);
            }
        }

        protected override void OnPropertyReadOnlyChanged()
        {
            this.header.Enabled = !base.Property.ReadOnly;
            this.comboBox.Enabled = !base.Property.ReadOnly;
            this.descriptionText.Enabled = !base.Property.ReadOnly;
        }

        protected override void OnPropertyValueChanged()
        {
            int index = Array.IndexOf<object>(base.Property.ValueChoices, base.Property.Value);
            if (this.comboBox.SelectedIndex != index)
            {
                this.comboBox.SelectedIndex = index;
            }
        }
    }
}

