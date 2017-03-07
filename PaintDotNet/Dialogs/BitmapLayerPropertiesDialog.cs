namespace PaintDotNet.Dialogs
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Resources;
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal sealed class BitmapLayerPropertiesDialog : LayerPropertiesDialog
    {
        private HeadingLabel blendingHeader;
        private PdnLabel blendModeLabel;
        private PdnDropDownList blendOpComboBox;
        private PdnLabel opacityLabel;
        private TrackBar opacityTrackBar;
        private NumericUpDown opacityUpDown;

        public BitmapLayerPropertiesDialog()
        {
            this.InitializeComponent();
            this.blendingHeader.Text = PdnResources.GetString("BitmapLayerPropertiesDialog.BlendingHeader.Text");
            this.blendModeLabel.Text = PdnResources.GetString("BitmapLayerPropertiesDialog.BlendModeLabel.Text");
            this.opacityLabel.Text = PdnResources.GetString("BitmapLayerPropertiesDialog.OpacityLabel.Text");
            this.blendOpComboBox.DisplayMember = "Tag";
            foreach (LayerBlendMode mode in Enum.GetValues(typeof(LayerBlendMode)))
            {
                BlendModeComboBoxItem item = new BlendModeComboBoxItem(mode);
                this.blendOpComboBox.Items.Add(item);
            }
        }

        private void ChangeLayerOpacity()
        {
            if (((BitmapLayer) base.Layer).Opacity != ((byte) this.opacityUpDown.Value))
            {
                base.Layer.PushSuppressPropertyChanged();
                ((BitmapLayer) base.Layer).Opacity = (byte) this.opacityTrackBar.Value;
                base.Layer.PopSuppressPropertyChanged();
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        protected override void InitDialogFromLayer()
        {
            this.opacityUpDown.Value = base.Layer.Opacity;
            this.SelectBlendMode(base.Layer.BlendMode);
            base.InitDialogFromLayer();
        }

        private void InitializeComponent()
        {
            this.blendModeLabel = new PdnLabel();
            this.blendOpComboBox = new PdnDropDownList();
            this.opacityUpDown = new NumericUpDown();
            this.opacityTrackBar = new TrackBar();
            this.opacityLabel = new PdnLabel();
            this.blendingHeader = new HeadingLabel();
            this.opacityUpDown.BeginInit();
            this.opacityTrackBar.BeginInit();
            base.SuspendLayout();
            base.nameLabel.Location = new Point(6, 30);
            base.nameLabel.Name = "nameLabel";
            base.nameBox.Name = "nameBox";
            base.visibleCheckBox.Location = new Point(8, 0x33);
            base.visibleCheckBox.Name = "visibleCheckBox";
            this.blendingHeader.Location = new Point(6, 0x4b);
            this.blendingHeader.Name = "blendingHeader";
            this.blendingHeader.Margin = new Padding(1, 3, 1, 1);
            this.blendingHeader.Size = new Size(0x10d, 0x11);
            this.blendingHeader.TabIndex = 8;
            this.blendingHeader.TabStop = false;
            this.blendModeLabel.Location = new Point(6, 0x5f);
            this.blendModeLabel.Name = "blendModeLabel";
            this.blendModeLabel.AutoSize = true;
            this.blendModeLabel.Size = new Size(50, 0x17);
            this.blendModeLabel.TabIndex = 4;
            this.blendOpComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.blendOpComboBox.Location = new Point(0x40, 0x5b);
            this.blendOpComboBox.Name = "blendOpComboBox";
            this.blendOpComboBox.Size = new Size(0x79, 0x15);
            this.blendOpComboBox.TabIndex = 4;
            this.blendOpComboBox.SelectedIndexChanged += new EventHandler(this.OnBlendOpComboBoxSelectedIndexChanged);
            this.blendOpComboBox.MaxDropDownItems = 100;
            this.opacityLabel.Location = new Point(6, 0x7c);
            this.opacityLabel.AutoSize = true;
            this.opacityLabel.Name = "opacityLabel";
            this.opacityLabel.Size = new Size(0x30, 0x10);
            this.opacityLabel.TabIndex = 0;
            this.opacityUpDown.Location = new Point(0x40, 0x7a);
            int[] bits = new int[4];
            bits[0] = 0xff;
            this.opacityUpDown.Maximum = new decimal(bits);
            this.opacityUpDown.Name = "opacityUpDown";
            this.opacityUpDown.Size = new Size(0x38, 20);
            this.opacityUpDown.TabIndex = 5;
            this.opacityUpDown.TextAlign = HorizontalAlignment.Right;
            this.opacityUpDown.Enter += new EventHandler(this.OnOpacityUpDownEnter);
            this.opacityUpDown.KeyUp += new KeyEventHandler(this.OnOpacityUpDownKeyUp);
            this.opacityUpDown.ValueChanged += new EventHandler(this.OnOpacityUpDownValueChanged);
            this.opacityUpDown.Leave += new EventHandler(this.OnOpacityUpDownLeave);
            this.opacityTrackBar.AutoSize = false;
            this.opacityTrackBar.LargeChange = 0x20;
            this.opacityTrackBar.Location = new Point(0x81, 0x7a);
            this.opacityTrackBar.Maximum = 0xff;
            this.opacityTrackBar.Name = "opacityTrackBar";
            this.opacityTrackBar.Size = new Size(0x92, 0x18);
            this.opacityTrackBar.TabIndex = 6;
            this.opacityTrackBar.TickStyle = TickStyle.None;
            this.opacityTrackBar.ValueChanged += new EventHandler(this.OnOpacityTrackBarValueChanged);
            base.cancelButton.Location = new Point(0xc1, 0x9e);
            base.cancelButton.Name = "cancelButton";
            base.okButton.Location = new Point(0x70, 0x9e);
            base.okButton.Name = "okButton";
            base.AutoScaleDimensions = new SizeF(96f, 96f);
            base.AutoScaleMode = AutoScaleMode.Dpi;
            base.ClientSize = new Size(0x112, 190);
            base.Controls.Add(this.blendingHeader);
            base.Controls.Add(this.blendOpComboBox);
            base.Controls.Add(this.opacityUpDown);
            base.Controls.Add(this.opacityLabel);
            base.Controls.Add(this.blendModeLabel);
            base.Controls.Add(this.opacityTrackBar);
            base.Location = new Point(0, 0);
            base.Name = "BitmapLayerPropertiesDialog";
            base.Controls.SetChildIndex(this.opacityTrackBar, 0);
            base.Controls.SetChildIndex(this.blendModeLabel, 0);
            base.Controls.SetChildIndex(this.opacityLabel, 0);
            base.Controls.SetChildIndex(this.opacityUpDown, 0);
            base.Controls.SetChildIndex(base.nameLabel, 0);
            base.Controls.SetChildIndex(base.visibleCheckBox, 0);
            base.Controls.SetChildIndex(base.nameBox, 0);
            base.Controls.SetChildIndex(this.blendingHeader, 0);
            base.Controls.SetChildIndex(this.blendOpComboBox, 0);
            base.Controls.SetChildIndex(base.cancelButton, 0);
            base.Controls.SetChildIndex(base.okButton, 0);
            this.opacityUpDown.EndInit();
            this.opacityTrackBar.EndInit();
            base.ResumeLayout(false);
        }

        protected override void InitLayerFromDialog()
        {
            base.Layer.Opacity = (byte) this.opacityUpDown.Value;
            if (this.blendOpComboBox.SelectedItem != null)
            {
                base.Layer.BlendMode = ((BlendModeComboBoxItem) this.blendOpComboBox.SelectedItem).BlendMode;
            }
            base.InitLayerFromDialog();
        }

        private void OnBlendOpComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            using (new WaitCursorChanger(this))
            {
                base.Layer.PushSuppressPropertyChanged();
                if (this.blendOpComboBox.SelectedItem != null)
                {
                    base.Layer.BlendMode = ((BlendModeComboBoxItem) this.blendOpComboBox.SelectedItem).BlendMode;
                }
                base.Layer.PopSuppressPropertyChanged();
            }
        }

        private void OnOpacityTrackBarValueChanged(object sender, EventArgs e)
        {
            if (this.opacityUpDown.Value != this.opacityTrackBar.Value)
            {
                this.opacityUpDown.Value = this.opacityTrackBar.Value;
                this.ChangeLayerOpacity();
            }
        }

        private void OnOpacityUpDownEnter(object sender, EventArgs e)
        {
            this.opacityUpDown.Select(0, this.opacityUpDown.Text.Length);
        }

        private void OnOpacityUpDownKeyUp(object sender, KeyEventArgs e)
        {
        }

        private void OnOpacityUpDownLeave(object sender, EventArgs e)
        {
            this.OnOpacityUpDownValueChanged(sender, e);
        }

        private void OnOpacityUpDownValueChanged(object sender, EventArgs e)
        {
            if (this.opacityTrackBar.Value != ((int) this.opacityUpDown.Value))
            {
                using (new WaitCursorChanger(this))
                {
                    this.opacityTrackBar.Value = (int) this.opacityUpDown.Value;
                    this.ChangeLayerOpacity();
                }
            }
        }

        private void SelectBlendMode(LayerBlendMode blendMode)
        {
            foreach (BlendModeComboBoxItem item in this.blendOpComboBox.Items)
            {
                if (item.BlendMode == blendMode)
                {
                    this.blendOpComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private sealed class BlendModeComboBoxItem
        {
            private LayerBlendMode blendMode;

            public BlendModeComboBoxItem(LayerBlendMode blendMode)
            {
                this.blendMode = blendMode;
            }

            public override bool Equals(object obj) => 
                (((obj != null) && (obj is BitmapLayerPropertiesDialog.BlendModeComboBoxItem)) && (((BitmapLayerPropertiesDialog.BlendModeComboBoxItem) obj).blendMode == this.blendMode));

            public override int GetHashCode() => 
                ((int) this.blendMode);

            public override string ToString() => 
                PdnResources.GetString($"LayerBlendMode.{this.blendMode.ToString()}");

            public LayerBlendMode BlendMode =>
                this.blendMode;
        }
    }
}

