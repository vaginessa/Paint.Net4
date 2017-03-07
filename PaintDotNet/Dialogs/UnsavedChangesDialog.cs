namespace PaintDotNet.Dialogs
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Drawing;
    using PaintDotNet.Resources;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class UnsavedChangesDialog : PdnBaseFormInternal
    {
        private CommandButton cancelButton;
        private HeadingLabel documentListHeader;
        private DocumentWorkspace[] documents;
        private DocumentStrip documentStrip;
        private CommandButton dontSaveButton;
        private HScrollBar hScrollBar;
        private PdnLabel infoLabel;
        private CommandButton saveButton;

        [field: CompilerGenerated]
        public event ValueEventHandler<DocumentWorkspace> DocumentClicked;

        public UnsavedChangesDialog()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.documentStrip = new DocumentStrip();
            this.documentListHeader = new HeadingLabel();
            this.hScrollBar = new HScrollBar();
            this.saveButton = new CommandButton();
            this.dontSaveButton = new CommandButton();
            this.cancelButton = new CommandButton();
            this.infoLabel = new PdnLabel();
            base.SuspendLayout();
            this.saveButton.SuspendLayout();
            this.dontSaveButton.SuspendLayout();
            this.cancelButton.SuspendLayout();
            this.documentStrip.BackColor = SystemColors.ButtonHighlight;
            this.documentStrip.DocumentClicked += new ValueEventHandler<Tuple<DocumentWorkspace, DocumentClickAction>>(this.OnDocumentListDocumentClicked);
            this.documentStrip.DrawDirtyOverlay = false;
            this.documentStrip.EnsureSelectedIsVisible = false;
            this.documentStrip.ManagedFocus = true;
            this.documentStrip.Name = "documentList";
            this.documentStrip.ScrollOffsetChanged += new EventHandler(this.OnDocumentListScrollOffsetChanged);
            this.documentStrip.ShowCloseButtons = false;
            this.documentStrip.ShowScrollButtons = false;
            this.documentStrip.TabIndex = 0;
            this.documentListHeader.Name = "documentListHeader";
            this.documentListHeader.RightMargin = 0;
            this.documentListHeader.TabIndex = 1;
            this.documentListHeader.TabStop = false;
            this.hScrollBar.Name = "hScrollBar";
            this.hScrollBar.TabIndex = 2;
            this.hScrollBar.ValueChanged += new EventHandler(this.OnHScrollBarValueChanged);
            this.saveButton.ActionImage = null;
            this.saveButton.AutoSize = true;
            this.saveButton.Name = "saveButton3";
            this.saveButton.TabIndex = 4;
            this.saveButton.Click += new EventHandler(this.OnSaveButtonClick);
            this.dontSaveButton.ActionImage = null;
            this.dontSaveButton.AutoSize = true;
            this.dontSaveButton.Name = "dontSaveButton";
            this.dontSaveButton.TabIndex = 5;
            this.dontSaveButton.Click += new EventHandler(this.OnDontSaveButtonClick);
            this.cancelButton.ActionImage = null;
            this.cancelButton.AutoSize = true;
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Click += new EventHandler(this.OnCancelButtonClick);
            this.infoLabel.Name = "infoLabel";
            this.infoLabel.TabIndex = 7;
            base.AutoScaleDimensions = new SizeF(96f, 96f);
            base.AutoScaleMode = AutoScaleMode.Dpi;
            base.ClientSize = new Size(450, 100);
            base.Controls.Add(this.infoLabel);
            base.Controls.Add(this.documentListHeader);
            base.Controls.Add(this.cancelButton);
            base.Controls.Add(this.hScrollBar);
            base.Controls.Add(this.dontSaveButton);
            base.Controls.Add(this.documentStrip);
            base.Controls.Add(this.saveButton);
            base.AcceptButton = this.saveButton;
            base.CancelButton = this.cancelButton;
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.Location = new Point(0, 0);
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "UnsavedChangesDialog";
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.CenterParent;
            base.Controls.SetChildIndex(this.saveButton, 0);
            base.Controls.SetChildIndex(this.documentStrip, 0);
            base.Controls.SetChildIndex(this.dontSaveButton, 0);
            base.Controls.SetChildIndex(this.hScrollBar, 0);
            base.Controls.SetChildIndex(this.cancelButton, 0);
            base.Controls.SetChildIndex(this.documentListHeader, 0);
            base.Controls.SetChildIndex(this.infoLabel, 0);
            this.LoadResources();
            this.saveButton.ResumeLayout(false);
            this.dontSaveButton.ResumeLayout(false);
            this.cancelButton.ResumeLayout(false);
            base.ResumeLayout(false);
        }

        public override void LoadResources()
        {
            this.Text = PdnResources.GetString("UnsavedChangesDialog.Text");
            base.Icon = PdnResources.GetImageResource("Icons.WarningIcon.png").Reference.ToIcon();
            this.infoLabel.Text = PdnResources.GetString("UnsavedChangesDialog.InfoLabel.Text");
            this.documentListHeader.Text = PdnResources.GetString("UnsavedChangesDialog.DocumentListHeader.Text");
            this.saveButton.ActionText = PdnResources.GetString("UnsavedChangesDialog.SaveButton.ActionText");
            this.saveButton.ExplanationText = PdnResources.GetString("UnsavedChangesDialog.SaveButton.ExplanationText");
            this.saveButton.ActionImage = PdnResources.GetImageResource("Icons.UnsavedChangesDialog.SaveButton.png").Reference;
            this.dontSaveButton.ActionText = PdnResources.GetString("UnsavedChangesDialog.DontSaveButton.ActionText");
            this.dontSaveButton.ExplanationText = PdnResources.GetString("UnsavedChangesDialog.DontSaveButton.ExplanationText");
            this.dontSaveButton.ActionImage = PdnResources.GetImageResource("Icons.MenuFileCloseIcon.png").Reference;
            this.cancelButton.ActionText = PdnResources.GetString("UnsavedChangesDialog.CancelButton.ActionText");
            this.cancelButton.ExplanationText = PdnResources.GetString("UnsavedChangesDialog.CancelButton.ExplanationText");
            this.cancelButton.ActionImage = PdnResources.GetImageResource("Icons.CancelIcon.png").Reference;
            base.LoadResources();
        }

        private void OnCancelButtonClick(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.Cancel;
            base.Close();
        }

        private void OnDocumentClicked(DocumentWorkspace dw)
        {
            this.DocumentClicked.Raise<DocumentWorkspace>(this, dw);
        }

        private void OnDocumentListDocumentClicked(object sender, ValueEventArgs<Tuple<DocumentWorkspace, DocumentClickAction>> e)
        {
            this.documentStrip.QueueUpdate();
            this.OnDocumentClicked(e.Value.Item1);
        }

        private void OnDocumentListScrollOffsetChanged(object sender, EventArgs e)
        {
            this.hScrollBar.Value = this.documentStrip.ScrollOffset;
        }

        private void OnDontSaveButtonClick(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.No;
            base.Close();
        }

        private void OnHScrollBarValueChanged(object sender, EventArgs e)
        {
            this.documentStrip.MoveScrollToOffset(this.hScrollBar.Value);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int x = UIUtil.ScaleWidth(8);
            int num2 = UIUtil.ScaleWidth(8);
            int num3 = UIUtil.ScaleHeight(8);
            int num4 = UIUtil.ScaleHeight(8);
            int num5 = UIUtil.ScaleHeight(8);
            int num6 = UIUtil.ScaleHeight(8);
            int num7 = UIUtil.ScaleHeight(8);
            int num8 = 1;
            int width = (base.ClientSize.Width - x) - num2;
            int y = num3;
            this.infoLabel.Location = new Point(x, y);
            this.infoLabel.Width = width;
            this.infoLabel.Height = this.infoLabel.GetPreferredSize(new Size(this.infoLabel.Width, 0)).Height;
            y += this.infoLabel.Height + num5;
            this.documentListHeader.Location = new Point(x, y);
            this.documentListHeader.Width = width;
            y += this.documentListHeader.Height + num6;
            this.documentStrip.Location = new Point(x, y);
            this.documentStrip.Size = new Size(width, UIUtil.ScaleHeight(0x48));
            this.hScrollBar.Location = new Point(x, this.documentStrip.Bottom);
            this.hScrollBar.Width = width;
            y += (this.documentStrip.Height + this.hScrollBar.Height) + num7;
            this.saveButton.Location = new Point(x, y);
            this.saveButton.Width = width;
            this.saveButton.PerformLayout();
            y += this.saveButton.Height + num8;
            this.dontSaveButton.Location = new Point(x, y);
            this.dontSaveButton.Width = width;
            this.dontSaveButton.PerformLayout();
            y += this.dontSaveButton.Height + num8;
            this.cancelButton.Location = new Point(x, y);
            this.cancelButton.Width = width;
            this.cancelButton.PerformLayout();
            y += this.cancelButton.Height + num4;
            base.ClientSize = new Size(base.ClientSize.Width, y);
            base.OnLayout(levent);
        }

        private void OnSaveButtonClick(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.Yes;
            base.Close();
        }

        public DocumentWorkspace[] Documents
        {
            get
            {
                this.VerifyThreadAccess();
                return (DocumentWorkspace[]) this.documents.Clone();
            }
            set
            {
                this.VerifyThreadAccess();
                this.documents = (DocumentWorkspace[]) value.Clone();
                this.documentStrip.ClearItems();
                foreach (DocumentWorkspace workspace in this.documents)
                {
                    this.documentStrip.AddDocumentWorkspace(workspace);
                }
                this.hScrollBar.Maximum = this.documentStrip.ViewRectangle.Width;
                this.hScrollBar.SmallChange = this.documentStrip.ItemSize.Width;
                this.hScrollBar.LargeChange = this.documentStrip.ClientSize.Width;
                if (this.documentStrip.ClientRectangle.Width > this.documentStrip.ViewRectangle.Width)
                {
                    this.hScrollBar.Enabled = false;
                }
                else
                {
                    this.hScrollBar.Enabled = true;
                }
                foreach (ImageStrip.Item item in this.documentStrip.Items)
                {
                    item.IsSelected = false;
                }
            }
        }

        public DocumentWorkspace SelectedDocument
        {
            get
            {
                this.VerifyThreadAccess();
                return this.documentStrip.SelectedDocument;
            }
            set
            {
                this.VerifyThreadAccess();
                this.documentStrip.SelectDocumentWorkspace(value);
            }
        }
    }
}

