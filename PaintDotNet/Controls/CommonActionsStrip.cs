namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Resources;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class CommonActionsStrip : ToolStripEx
    {
        private ToolStripButton copyButton;
        private ToolStripButton cropButton;
        private ToolStripButton cutButton;
        private ToolStripButton deselectButton;
        private bool itemClickedMutex;
        private ToolStripButton newButton;
        private ToolStripButton openButton;
        private ToolStripButton pasteButton;
        private ToolStripButton printButton;
        private ToolStripButton redoButton;
        private ToolStripButton saveButton;
        private PdnToolStripSeparator separator0;
        private PdnToolStripSeparator separator1;
        private PdnToolStripSeparator separator2;
        private PdnToolStripSeparator separator3;
        private ToolStripButton toggleGridButton;
        private ToolStripButton toggleRulersButton;
        private ToolStripButton undoButton;

        [field: CompilerGenerated]
        public event ValueEventHandler<CommonAction> ButtonClick;

        [field: CompilerGenerated]
        public event EventHandler DrawGridChanged;

        [field: CompilerGenerated]
        public event EventHandler RulersEnabledChanged;

        public CommonActionsStrip()
        {
            this.InitializeComponent();
            this.newButton.ToolTipText = PdnResources.GetString("CommonAction.New");
            this.openButton.ToolTipText = PdnResources.GetString("CommonAction.Open");
            this.saveButton.ToolTipText = PdnResources.GetString("CommonAction.Save");
            this.printButton.ToolTipText = PdnResources.GetString("CommonAction.Print");
            this.cutButton.ToolTipText = PdnResources.GetString("CommonAction.Cut");
            this.copyButton.ToolTipText = PdnResources.GetString("CommonAction.Copy");
            this.pasteButton.ToolTipText = PdnResources.GetString("CommonAction.Paste");
            this.cropButton.ToolTipText = PdnResources.GetString("CommonAction.CropToSelection");
            this.deselectButton.ToolTipText = PdnResources.GetString("CommonAction.Deselect");
            this.undoButton.ToolTipText = PdnResources.GetString("CommonAction.Undo");
            this.redoButton.ToolTipText = PdnResources.GetString("CommonAction.Redo");
            this.toggleGridButton.ToolTipText = PdnResources.GetString("WorkspaceOptionsConfigWidget.DrawGridToggleButton.ToolTipText");
            this.toggleRulersButton.ToolTipText = PdnResources.GetString("WorkspaceOptionsConfigWidget.RulersToggleButton.ToolTipText");
            this.newButton.Tag = CommonAction.New;
            this.openButton.Tag = CommonAction.Open;
            this.saveButton.Tag = CommonAction.Save;
            this.printButton.Tag = CommonAction.Print;
            this.cutButton.Tag = CommonAction.Cut;
            this.copyButton.Tag = CommonAction.Copy;
            this.pasteButton.Tag = CommonAction.Paste;
            this.cropButton.Tag = CommonAction.CropToSelection;
            this.deselectButton.Tag = CommonAction.Deselect;
            this.undoButton.Tag = CommonAction.Undo;
            this.redoButton.Tag = CommonAction.Redo;
            this.toggleGridButton.Tag = CommonAction.ToggleGrid;
            this.toggleRulersButton.Tag = CommonAction.ToggleRulers;
        }

        private ToolStripButton FindButton(CommonAction action)
        {
            switch (action)
            {
                case CommonAction.New:
                    return this.newButton;

                case CommonAction.Open:
                    return this.openButton;

                case CommonAction.Save:
                    return this.saveButton;

                case CommonAction.Print:
                    return this.printButton;

                case CommonAction.Cut:
                    return this.cutButton;

                case CommonAction.Copy:
                    return this.copyButton;

                case CommonAction.Paste:
                    return this.pasteButton;

                case CommonAction.CropToSelection:
                    return this.cropButton;

                case CommonAction.Deselect:
                    return this.deselectButton;

                case CommonAction.Undo:
                    return this.undoButton;

                case CommonAction.Redo:
                    return this.redoButton;

                case CommonAction.ToggleRulers:
                    return this.toggleRulersButton;

                case CommonAction.ToggleGrid:
                    return this.toggleGridButton;
            }
            throw ExceptionUtil.InvalidEnumArgumentException<CommonAction>(action, "action");
        }

        public bool GetButtonEnabled(CommonAction action) => 
            this.FindButton(action).Enabled;

        public bool GetButtonVisible(CommonAction action) => 
            this.FindButton(action).Visible;

        private void InitializeComponent()
        {
            this.newButton = new ToolStripButton();
            this.openButton = new ToolStripButton();
            this.saveButton = new ToolStripButton();
            this.separator0 = new PdnToolStripSeparator();
            this.printButton = new ToolStripButton();
            this.separator1 = new PdnToolStripSeparator();
            this.cutButton = new ToolStripButton();
            this.copyButton = new ToolStripButton();
            this.pasteButton = new ToolStripButton();
            this.cropButton = new ToolStripButton();
            this.deselectButton = new ToolStripButton();
            this.separator2 = new PdnToolStripSeparator();
            this.undoButton = new ToolStripButton();
            this.redoButton = new ToolStripButton();
            this.separator3 = new PdnToolStripSeparator();
            this.toggleGridButton = new ToolStripButton();
            this.toggleRulersButton = new ToolStripButton();
            base.SuspendLayout();
            this.newButton.Image = PdnResources.GetImageResource("Icons.MenuFileNewIcon.png").Reference;
            this.openButton.Image = PdnResources.GetImageResource("Icons.MenuFileOpenIcon.png").Reference;
            this.saveButton.Image = PdnResources.GetImageResource("Icons.MenuFileSaveIcon.png").Reference;
            this.printButton.Image = PdnResources.GetImageResource("Icons.MenuFilePrintIcon.png").Reference;
            this.cutButton.Image = PdnResources.GetImageResource("Icons.MenuEditCutIcon.png").Reference;
            this.copyButton.Image = PdnResources.GetImageResource("Icons.MenuEditCopyIcon.png").Reference;
            this.pasteButton.Image = PdnResources.GetImageResource("Icons.MenuEditPasteIcon.png").Reference;
            this.cropButton.Image = PdnResources.GetImageResource("Icons.MenuImageCropIcon.png").Reference;
            this.deselectButton.Image = PdnResources.GetImageResource("Icons.MenuEditDeselectIcon.png").Reference;
            this.undoButton.Image = PdnResources.GetImageResource("Icons.MenuEditUndoIcon.png").Reference;
            this.redoButton.Image = PdnResources.GetImageResource("Icons.MenuEditRedoIcon.png").Reference;
            this.toggleGridButton.Image = PdnResources.GetImageResource("Icons.MenuViewGridIcon.png").Reference;
            this.toggleRulersButton.Image = PdnResources.GetImageResource("Icons.MenuViewRulersIcon.png").Reference;
            this.Items.Add(this.newButton);
            this.Items.Add(this.openButton);
            this.Items.Add(this.saveButton);
            this.Items.Add(this.separator0);
            this.Items.Add(this.printButton);
            this.Items.Add(this.separator1);
            this.Items.Add(this.cutButton);
            this.Items.Add(this.copyButton);
            this.Items.Add(this.pasteButton);
            this.Items.Add(this.cropButton);
            this.Items.Add(this.deselectButton);
            this.Items.Add(this.separator2);
            this.Items.Add(this.undoButton);
            this.Items.Add(this.redoButton);
            this.Items.Add(this.separator3);
            this.Items.Add(this.toggleGridButton);
            this.Items.Add(this.toggleRulersButton);
            base.ResumeLayout(false);
        }

        private void OnButtonClick(CommonAction action)
        {
            this.ButtonClick.Raise<CommonAction>(this, action);
        }

        protected override void OnItemClicked(ToolStripItemClickedEventArgs e)
        {
            if (!this.itemClickedMutex)
            {
                this.itemClickedMutex = true;
                try
                {
                    if (e.ClickedItem is ToolStripButton)
                    {
                        CommonAction tag = (CommonAction) e.ClickedItem.Tag;
                        this.OnButtonClick(tag);
                    }
                }
                finally
                {
                    this.itemClickedMutex = false;
                }
            }
            base.OnItemClicked(e);
        }

        public void SetButtonEnabled(CommonAction action, bool enabled)
        {
            this.FindButton(action).Enabled = enabled;
        }

        public void SetButtonVisible(CommonAction action, bool visible)
        {
            this.FindButton(action).Visible = visible;
        }

        public bool DrawGrid
        {
            get => 
                this.toggleGridButton.Checked;
            set
            {
                if (this.toggleGridButton.Checked != value)
                {
                    this.toggleGridButton.Checked = value;
                    this.DrawGridChanged.Raise(this);
                }
            }
        }

        public bool RulersEnabled
        {
            get => 
                this.toggleRulersButton.Checked;
            set
            {
                if (this.toggleRulersButton.Checked != value)
                {
                    this.toggleRulersButton.Checked = value;
                    this.RulersEnabledChanged.Raise(this);
                }
            }
        }
    }
}

