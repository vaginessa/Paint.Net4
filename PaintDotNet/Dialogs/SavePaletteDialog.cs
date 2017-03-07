namespace PaintDotNet.Dialogs
{
    using PaintDotNet.AppModel;
    using PaintDotNet.Controls;
    using PaintDotNet.Drawing;
    using PaintDotNet.Resources;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    internal class SavePaletteDialog : PdnBaseFormInternal
    {
        private PdnPushButton cancelButton;
        private ListBox listBox;
        private Label palettesLabel;
        private PdnPushButton saveButton;
        private TextBox textBox;
        private Label typeANameLabel;

        public SavePaletteDialog()
        {
            this.InitializeComponent();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (base.Icon != null))
            {
                Icon icon = base.Icon;
                base.Icon = null;
                icon.Dispose();
                icon = null;
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.typeANameLabel = new PdnLabel();
            this.textBox = new TextBox();
            this.listBox = new ListBox();
            this.saveButton = new PdnPushButton();
            this.palettesLabel = new PdnLabel();
            this.cancelButton = new PdnPushButton();
            base.SuspendLayout();
            this.typeANameLabel.AutoSize = true;
            this.typeANameLabel.Location = new Point(5, 8);
            this.typeANameLabel.Margin = new Padding(0);
            this.typeANameLabel.Name = "typeANameLabel";
            this.typeANameLabel.Size = new Size(50, 13);
            this.typeANameLabel.TabIndex = 0;
            this.typeANameLabel.Text = "infoLabel";
            this.textBox.AutoCompleteMode = AutoCompleteMode.Suggest;
            this.textBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
            this.textBox.Location = new Point(8, 0x19);
            this.textBox.Name = "textBox";
            this.textBox.Size = new Size(0x120, 20);
            this.textBox.TabIndex = 2;
            this.textBox.Validating += new CancelEventHandler(this.OnTextBoxValidating);
            this.textBox.TextChanged += new EventHandler(this.OnTextBoxTextChanged);
            this.palettesLabel.AutoSize = true;
            this.palettesLabel.Location = new Point(5, 50);
            this.palettesLabel.Margin = new Padding(0);
            this.palettesLabel.Name = "palettesLabel";
            this.palettesLabel.Size = new Size(0x23, 13);
            this.palettesLabel.TabIndex = 5;
            this.palettesLabel.Text = "label1";
            this.listBox.FormattingEnabled = true;
            this.listBox.Location = new Point(8, 0x43);
            this.listBox.Name = "listBox";
            this.listBox.Size = new Size(0x121, 0x6c);
            this.listBox.Sorted = true;
            this.listBox.TabIndex = 3;
            this.listBox.SelectedIndexChanged += new EventHandler(this.OnListBoxSelectedIndexChanged);
            this.saveButton.DialogResult = DialogResult.Cancel;
            this.saveButton.Location = new Point(8, 0xb9);
            this.saveButton.Name = "saveButton2";
            this.saveButton.Size = new Size(0x4b, 0x17);
            this.saveButton.TabIndex = 4;
            this.saveButton.Text = "button1";
            this.saveButton.Click += new EventHandler(this.OnSaveButtonClick);
            this.cancelButton.DialogResult = DialogResult.Cancel;
            this.cancelButton.Location = new Point(0x59, 0xb9);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new Size(0x4b, 0x17);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = "button1";
            this.cancelButton.Click += new EventHandler(this.OnCancelButtonClick);
            base.AutoScaleDimensions = new SizeF(96f, 96f);
            base.AutoScaleMode = AutoScaleMode.Dpi;
            base.CancelButton = this.cancelButton;
            base.ClientSize = new Size(310, 0xd9);
            base.Controls.Add(this.palettesLabel);
            base.Controls.Add(this.listBox);
            base.Controls.Add(this.textBox);
            base.Controls.Add(this.saveButton);
            base.Controls.Add(this.cancelButton);
            base.Controls.Add(this.typeANameLabel);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "SavePaletteDialog";
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.CenterParent;
            this.Text = "SavePaletteDialog";
            base.Controls.SetChildIndex(this.typeANameLabel, 0);
            base.Controls.SetChildIndex(this.cancelButton, 0);
            base.Controls.SetChildIndex(this.saveButton, 0);
            base.Controls.SetChildIndex(this.textBox, 0);
            base.Controls.SetChildIndex(this.listBox, 0);
            base.Controls.SetChildIndex(this.palettesLabel, 0);
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        public override void LoadResources()
        {
            this.Text = PdnResources.GetString("SavePaletteDialog.Text");
            base.Icon = PdnResources.GetImageResource("Icons.MenuFileSaveAsIcon.png").Reference.ToIcon();
            this.cancelButton.Text = PdnResources.GetString("Form.CancelButton.Text");
            this.saveButton.Text = PdnResources.GetString("Form.SaveButton.Text");
            this.typeANameLabel.Text = PdnResources.GetString("SavePaletteDialog.TypeANameLabel.Text");
            this.palettesLabel.Text = PdnResources.GetString("SavePaletteDialog.PalettesLabel.Text");
            base.LoadResources();
        }

        private void OnCancelButtonClick(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.Cancel;
            base.Close();
        }

        private void OnListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listBox.SelectedItem != null)
            {
                this.textBox.Text = this.listBox.SelectedItem.ToString();
                this.textBox.Focus();
                this.listBox.SelectedItem = null;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            this.ValidatePaletteName();
            base.OnLoad(e);
        }

        private void OnSaveButtonClick(object sender, EventArgs e)
        {
            if (this.saveButton.Enabled)
            {
                base.DialogResult = DialogResult.OK;
                base.Close();
            }
        }

        private void OnTextBoxTextChanged(object sender, EventArgs e)
        {
            this.ValidatePaletteName();
        }

        private void OnTextBoxValidating(object sender, CancelEventArgs e)
        {
            this.ValidatePaletteName();
        }

        private void ValidatePaletteName()
        {
            if (UserPalettesService.Instance.ValidatePaletteName(this.textBox.Text))
            {
                this.saveButton.Enabled = true;
                base.AcceptButton = this.saveButton;
                this.textBox.BackColor = SystemColors.Window;
            }
            else
            {
                this.saveButton.Enabled = false;
                base.AcceptButton = null;
                if (!string.IsNullOrEmpty(this.textBox.Text))
                {
                    this.textBox.BackColor = Color.Red;
                }
            }
        }

        public string PaletteName
        {
            get => 
                this.textBox.Text;
            set
            {
                this.textBox.Text = value;
            }
        }

        public string[] PaletteNames
        {
            set
            {
                this.listBox.Items.Clear();
                AutoCompleteStringCollection strings = new AutoCompleteStringCollection();
                foreach (string str in value)
                {
                    strings.Add(str);
                    this.listBox.Items.Add(str);
                }
                this.textBox.AutoCompleteCustomSource = strings;
                this.textBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
            }
        }
    }
}

