namespace PaintDotNet.Dialogs
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryFunctions;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.Settings.App;
    using PaintDotNet.Snap;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.VisualStyling;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class LayerForm : FloatingToolForm
    {
        private ToolStripButton addNewLayerButton;
        private PaintDotNet.Controls.AppWorkspace appWorkspace;
        private IContainer components;
        private ToolStripButton deleteLayerButton;
        private ToolStripButton duplicateLayerButton;
        private VScrollBar layersScrollBar;
        private LayersStrip layersStrip;
        private ToolStripButton mergeLayerDownButton;
        private ToolStripButton moveLayerDownButton;
        private ToolStripButton moveLayerUpButton;
        private ToolStripButton propertiesButton;
        private ToolStripEx toolStrip;

        [field: CompilerGenerated]
        public event EventHandler DeleteLayerButtonClick;

        [field: CompilerGenerated]
        public event EventHandler DuplicateLayerButtonClick;

        [field: CompilerGenerated]
        public event EventHandler MergeLayerDownClick;

        [field: CompilerGenerated]
        public event EventHandler MoveLayerDownButtonClick;

        [field: CompilerGenerated]
        public event EventHandler MoveLayerToBottomButtonClick;

        [field: CompilerGenerated]
        public event EventHandler MoveLayerToTopButtonClick;

        [field: CompilerGenerated]
        public event EventHandler MoveLayerUpButtonClick;

        [field: CompilerGenerated]
        public event EventHandler NewLayerButtonClick;

        [field: CompilerGenerated]
        public event EventHandler PropertiesButtonClick;

        public LayerForm()
        {
            this.InitializeComponent();
            this.addNewLayerButton.Image = PdnResources.GetImageResource("Icons.MenuLayersAddNewLayerIcon.png").Reference;
            this.deleteLayerButton.Image = PdnResources.GetImageResource("Icons.MenuLayersDeleteLayerIcon.png").Reference;
            this.moveLayerUpButton.Image = PdnResources.GetImageResource("Icons.MenuLayersMoveLayerUpIcon.png").Reference;
            this.moveLayerDownButton.Image = PdnResources.GetImageResource("Icons.MenuLayersMoveLayerDownIcon.png").Reference;
            this.duplicateLayerButton.Image = PdnResources.GetImageResource("Icons.MenuLayersDuplicateLayerIcon.png").Reference;
            this.mergeLayerDownButton.Image = PdnResources.GetImageResource("Icons.MenuLayersMergeLayerDownIcon.png").Reference;
            this.propertiesButton.Image = PdnResources.GetImageResource("Icons.MenuLayersLayerPropertiesIcon.png").Reference;
            this.Text = PdnResources.GetString("LayerForm.Text");
            this.addNewLayerButton.ToolTipText = PdnResources.GetString("LayerForm.AddNewLayerButton.ToolTipText");
            this.deleteLayerButton.ToolTipText = PdnResources.GetString("LayerForm.DeleteLayerButton.ToolTipText");
            this.duplicateLayerButton.ToolTipText = PdnResources.GetString("LayerForm.DuplicateLayerButton.ToolTipText");
            this.mergeLayerDownButton.ToolTipText = PdnResources.GetString("LayerForm.MergeLayerDownButton.ToolTipText");
            this.moveLayerUpButton.ToolTipText = PdnResources.GetString("LayerForm.MoveLayerUpButton.ToolTipText");
            this.moveLayerDownButton.ToolTipText = PdnResources.GetString("LayerForm.MoveLayerDownButton.ToolTipText");
            this.propertiesButton.ToolTipText = PdnResources.GetString("LayerForm.PropertiesButton.ToolTipText");
            this.MinimumSize = base.Size;
            this.toolStrip.Renderer = new PdnToolStripRenderer();
        }

        private void DetermineButtonEnableStates()
        {
            int activeLayerIndex;
            if ((this.appWorkspace.ActiveDocumentWorkspace == null) || (this.appWorkspace.ActiveDocumentWorkspace.Document == null))
            {
                activeLayerIndex = 0;
            }
            else
            {
                activeLayerIndex = this.appWorkspace.ActiveDocumentWorkspace.ActiveLayerIndex;
            }
            this.DetermineButtonEnableStates(activeLayerIndex);
        }

        private void DetermineButtonEnableStates(int activeLayerIndex)
        {
            if (((this.AppWorkspace == null) || (this.AppWorkspace.ActiveDocumentWorkspace == null)) || ((this.AppWorkspace.ActiveDocumentWorkspace.Document == null) || (this.AppWorkspace.ActiveDocumentWorkspace.ActiveLayer == null)))
            {
                this.moveLayerDownButton.Enabled = false;
                this.moveLayerUpButton.Enabled = false;
                this.deleteLayerButton.Enabled = false;
                this.mergeLayerDownButton.Enabled = false;
                this.duplicateLayerButton.Enabled = false;
            }
            else
            {
                this.duplicateLayerButton.Enabled = true;
                if (activeLayerIndex == 0)
                {
                    this.moveLayerDownButton.Enabled = false;
                }
                else
                {
                    this.moveLayerDownButton.Enabled = true;
                }
                if (activeLayerIndex == (this.AppWorkspace.ActiveDocumentWorkspace.Document.Layers.Count - 1))
                {
                    this.moveLayerUpButton.Enabled = false;
                }
                else
                {
                    this.moveLayerUpButton.Enabled = true;
                }
                if (this.AppWorkspace.ActiveDocumentWorkspace.Document.Layers.Count <= 1)
                {
                    this.deleteLayerButton.Enabled = false;
                }
                else
                {
                    this.deleteLayerButton.Enabled = true;
                }
                if ((this.AppWorkspace.ActiveDocumentWorkspace.ActiveLayerIndex == 0) || (this.AppWorkspace.ActiveDocumentWorkspace.Document.Layers.Count < 2))
                {
                    this.mergeLayerDownButton.Enabled = false;
                }
                else
                {
                    this.mergeLayerDownButton.Enabled = true;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposableUtil.Free<IContainer>(ref this.components, disposing);
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            this.layersStrip = new LayersStrip();
            this.layersScrollBar = new VScrollBar();
            this.toolStrip = new ToolStripEx();
            this.addNewLayerButton = new ToolStripButton();
            this.deleteLayerButton = new ToolStripButton();
            this.duplicateLayerButton = new ToolStripButton();
            this.mergeLayerDownButton = new ToolStripButton();
            this.moveLayerUpButton = new ToolStripButton();
            this.moveLayerDownButton = new ToolStripButton();
            this.propertiesButton = new ToolStripButton();
            this.toolStrip.SuspendLayout();
            base.SuspendLayout();
            this.Font = SystemFonts.MenuFont;
            this.layersStrip.Dock = DockStyle.Fill;
            this.layersStrip.Location = new Point(0, 0);
            this.layersStrip.Name = "layersStrip";
            this.layersStrip.TabIndex = 5;
            this.layersStrip.ScrollOffsetChanged += new EventHandler(this.OnLayersStripScrollOffsetChanged);
            this.layersStrip.LayerClicked += new ValueEventHandler<Layer>(this.OnLayersStripLayerClicked);
            this.layersStrip.LayoutRequested += new EventHandler(this.OnLayersStripLayoutRequested);
            this.layersStrip.ClientSizeChanged += new EventHandler(this.OnLayersStripClientSizeChanged);
            this.layersStrip.ItemMoved += new VerticalImageStripItemMovedEventHandler(this.OnLayersStripItemMoved);
            this.layersStrip.ItemDoubleClicked += new ValueEventHandler<Tuple<VerticalImageStrip.Item, VerticalImageStrip.ItemPart, MouseButtons>>(this.OnLayersStripItemDoubleClicked);
            this.layersStrip.RelinquishFocus += new EventHandler(this.OnLayersStripRelinquishFocus);
            this.layersStrip.Layout += new LayoutEventHandler(this.OnLayersStripLayout);
            this.layersScrollBar.Dock = DockStyle.Right;
            this.layersScrollBar.Name = "layersScrollBar";
            this.layersScrollBar.ValueChanged += new EventHandler(this.OnLayersScrollBarValueChanged);
            this.toolStrip.Dock = DockStyle.Bottom;
            this.toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            ToolStripItem[] toolStripItems = new ToolStripItem[] { this.addNewLayerButton, this.deleteLayerButton, this.duplicateLayerButton, this.mergeLayerDownButton, this.moveLayerUpButton, this.moveLayerDownButton, this.propertiesButton };
            this.toolStrip.Items.AddRange(toolStripItems);
            this.toolStrip.LayoutStyle = ToolStripLayoutStyle.Flow;
            this.toolStrip.Location = new Point(0, 0x84);
            this.toolStrip.Padding = new Padding(2, 2, 0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new Size(160, 0x1a);
            this.toolStrip.TabIndex = 7;
            this.toolStrip.TabStop = true;
            this.toolStrip.RelinquishFocus += new EventHandler(this.OnToolStripRelinquishFocus);
            this.addNewLayerButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.addNewLayerButton.Name = "addNewLayerButton";
            this.addNewLayerButton.Size = new Size(0x17, 4);
            this.addNewLayerButton.Click += new EventHandler(this.OnToolStripButtonClick);
            this.deleteLayerButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.deleteLayerButton.Name = "deleteLayerButton";
            this.deleteLayerButton.Size = new Size(0x17, 4);
            this.deleteLayerButton.Click += new EventHandler(this.OnToolStripButtonClick);
            this.duplicateLayerButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.duplicateLayerButton.Name = "duplicateLayerButton";
            this.duplicateLayerButton.Size = new Size(0x17, 4);
            this.duplicateLayerButton.Click += new EventHandler(this.OnToolStripButtonClick);
            this.mergeLayerDownButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.mergeLayerDownButton.Name = "mergeLayerDownButton";
            this.mergeLayerDownButton.Click += new EventHandler(this.OnToolStripButtonClick);
            this.moveLayerUpButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.moveLayerUpButton.Name = "moveLayerUpButton";
            this.moveLayerUpButton.Size = new Size(0x17, 4);
            this.moveLayerUpButton.Click += new EventHandler(this.OnToolStripButtonClick);
            this.moveLayerDownButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.moveLayerDownButton.Name = "moveLayerDownButton";
            this.moveLayerDownButton.Size = new Size(0x17, 4);
            this.moveLayerDownButton.Click += new EventHandler(this.OnToolStripButtonClick);
            this.propertiesButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.propertiesButton.Name = "propertiesButton";
            this.propertiesButton.Size = new Size(0x17, 4);
            this.propertiesButton.Click += new EventHandler(this.OnToolStripButtonClick);
            base.AutoScaleDimensions = new SizeF(96f, 96f);
            base.AutoScaleMode = AutoScaleMode.Dpi;
            base.ClientSize = new Size(0xa5, 0x9e);
            base.Controls.Add(this.layersStrip);
            base.Controls.Add(this.layersScrollBar);
            base.Controls.Add(this.toolStrip);
            base.Name = "LayersForm";
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void OnActiveDocumentWorkspaceActiveLayerChanged(object sender, EventArgs e)
        {
            DocumentWorkspace activeDocumentWorkspace = this.appWorkspace.ActiveDocumentWorkspace;
            if (activeDocumentWorkspace == null)
            {
                this.layersStrip.ActiveLayer = null;
            }
            else
            {
                Layer activeLayer = activeDocumentWorkspace.ActiveLayer;
                this.layersStrip.ActiveLayer = activeLayer;
            }
            this.DetermineButtonEnableStates();
        }

        private void OnActiveDocumentWorkspaceChanged(object sender, EventArgs e)
        {
            DocumentWorkspace activeDocumentWorkspace = this.appWorkspace.ActiveDocumentWorkspace;
            if (activeDocumentWorkspace == null)
            {
                this.layersStrip.Document = null;
            }
            else
            {
                activeDocumentWorkspace.DocumentChanged += new EventHandler(this.OnActiveDocumentWorkspaceDocumentChanged);
                activeDocumentWorkspace.ActiveLayerChanged += new EventHandler(this.OnActiveDocumentWorkspaceActiveLayerChanged);
                this.layersStrip.DocumentWorkspace = activeDocumentWorkspace;
                this.layersStrip.Document = activeDocumentWorkspace.Document;
                this.layersStrip.ActiveLayer = activeDocumentWorkspace.ActiveLayer;
                this.DetermineButtonEnableStates();
            }
        }

        private void OnActiveDocumentWorkspaceChanging(object sender, EventArgs e)
        {
            DocumentWorkspace activeDocumentWorkspace = this.appWorkspace.ActiveDocumentWorkspace;
            if (activeDocumentWorkspace != null)
            {
                activeDocumentWorkspace.DocumentChanged -= new EventHandler(this.OnActiveDocumentWorkspaceDocumentChanged);
                activeDocumentWorkspace.ActiveLayerChanged -= new EventHandler(this.OnActiveDocumentWorkspaceActiveLayerChanged);
            }
        }

        private void OnActiveDocumentWorkspaceDocumentChanged(object sender, EventArgs e)
        {
            if (this.appWorkspace.ActiveDocumentWorkspace == null)
            {
                this.layersStrip.Document = null;
                this.layersStrip.ActiveLayer = null;
            }
            else
            {
                this.layersStrip.Document = this.appWorkspace.ActiveDocumentWorkspace.Document;
                this.layersStrip.ActiveLayer = this.appWorkspace.ActiveDocumentWorkspace.ActiveLayer;
            }
            this.DetermineButtonEnableStates();
        }

        private void OnDeleteLayerButtonClick()
        {
            this.DeleteLayerButtonClick.Raise(this);
        }

        private void OnDeleteLayerButtonClick(object sender, EventArgs e)
        {
            this.OnDeleteLayerButtonClick();
        }

        private void OnDuplicateLayerButtonClick()
        {
            this.DuplicateLayerButtonClick.Raise(this);
        }

        private void OnDuplicateLayerButtonClick(object sender, EventArgs e)
        {
            this.OnDuplicateLayerButtonClick();
        }

        private void OnLayersScrollBarValueChanged(object sender, EventArgs e)
        {
            this.layersStrip.MoveScrollToOffset(this.layersScrollBar.Value);
            this.UpdateLayersScrollBar();
        }

        private void OnLayersStripClientSizeChanged(object sender, EventArgs e)
        {
            this.UpdateLayersScrollBar();
        }

        private void OnLayersStripItemDoubleClicked(object sender, ValueEventArgs<Tuple<VerticalImageStrip.Item, VerticalImageStrip.ItemPart, MouseButtons>> e)
        {
            if ((((VerticalImageStrip.ItemPart) e.Value.Item2) != VerticalImageStrip.ItemPart.CheckBox) && (((VerticalImageStrip.ItemPart) e.Value.Item2) != VerticalImageStrip.ItemPart.CloseButton))
            {
                this.PerformPropertiesClick();
            }
        }

        private void OnLayersStripItemMoved(object sender, VerticalImageStripItemMovedEventArgs e)
        {
            int oldIndex = this.layersStrip.RenderSlotToLayerIndex(e.OldIndex);
            int newIndex = this.layersStrip.RenderSlotToLayerIndex(e.NewIndex);
            this.appWorkspace.ActiveDocumentWorkspace.ApplyFunction(new MoveLayerFunction(oldIndex, newIndex));
            this.DetermineButtonEnableStates();
        }

        private void OnLayersStripLayerClicked(object sender, ValueEventArgs<Layer> e)
        {
            if (e.Value != this.appWorkspace.ActiveDocumentWorkspace.ActiveLayer)
            {
                this.appWorkspace.ActiveDocumentWorkspace.ActiveLayer = e.Value;
            }
            this.DetermineButtonEnableStates();
        }

        private void OnLayersStripLayout(object sender, LayoutEventArgs e)
        {
            this.UpdateLayersScrollBar();
        }

        private void OnLayersStripLayoutRequested(object sender, EventArgs e)
        {
            this.UpdateLayersScrollBar();
        }

        private void OnLayersStripRelinquishFocus(object sender, EventArgs e)
        {
            this.OnRelinquishFocus();
        }

        private void OnLayersStripScrollOffsetChanged(object sender, EventArgs e)
        {
            this.layersScrollBar.Value = this.layersStrip.ScrollOffset;
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            if (this.layersScrollBar != null)
            {
                this.UpdateLayersScrollBar();
            }
            base.OnLayout(levent);
        }

        private void OnMergeLayerDownButtonClick()
        {
            this.MergeLayerDownClick.Raise(this);
        }

        private void OnMoveDownButtonClick(object sender, EventArgs e)
        {
            this.OnMoveLayerDownButtonClick();
        }

        private void OnMoveLayerDownButtonClick()
        {
            this.MoveLayerDownButtonClick.Raise(this);
        }

        private void OnMoveLayerToBottomButtonClick()
        {
            this.MoveLayerToBottomButtonClick.Raise(this);
        }

        private void OnMoveLayerToTopButtonClick()
        {
            this.MoveLayerToTopButtonClick.Raise(this);
        }

        private void OnMoveLayerUpButtonClick()
        {
            this.MoveLayerUpButtonClick.Raise(this);
        }

        private void OnMoveUpButtonClick(object sender, EventArgs e)
        {
            this.OnMoveLayerUpButtonClick();
        }

        private void OnNewLayerButtonClick()
        {
            this.NewLayerButtonClick.Raise(this);
        }

        private void OnNewLayerButtonClick(object sender, EventArgs e)
        {
            this.OnNewLayerButtonClick();
        }

        private void OnPropertiesButtonClick()
        {
            this.PropertiesButtonClick.Raise(this);
        }

        private void OnPropertiesButtonClick(object sender, EventArgs e)
        {
            this.OnPropertiesButtonClick();
        }

        private void OnToolStripButtonClick(object sender, EventArgs e)
        {
            if (sender == this.addNewLayerButton)
            {
                this.OnNewLayerButtonClick();
            }
            else if (sender == this.deleteLayerButton)
            {
                this.OnDeleteLayerButtonClick();
            }
            else if (sender == this.duplicateLayerButton)
            {
                this.OnDuplicateLayerButtonClick();
            }
            else if (sender == this.mergeLayerDownButton)
            {
                this.OnMergeLayerDownButtonClick();
            }
            else if (sender == this.moveLayerUpButton)
            {
                if ((Control.ModifierKeys & Keys.Control) != Keys.None)
                {
                    this.OnMoveLayerToTopButtonClick();
                }
                else
                {
                    this.OnMoveLayerUpButtonClick();
                }
            }
            else if (sender == this.moveLayerDownButton)
            {
                if ((Control.ModifierKeys & Keys.Control) != Keys.None)
                {
                    this.OnMoveLayerToBottomButtonClick();
                }
                else
                {
                    this.OnMoveLayerDownButtonClick();
                }
            }
            else if (sender == this.propertiesButton)
            {
                this.OnPropertiesButtonClick();
            }
            this.DetermineButtonEnableStates();
            this.OnRelinquishFocus();
        }

        private void OnToolStripRelinquishFocus(object sender, EventArgs e)
        {
            this.OnRelinquishFocus();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            bool visible = base.Visible;
            base.OnVisibleChanged(e);
        }

        public void PerformDeleteLayerClick()
        {
            this.OnDeleteLayerButtonClick();
        }

        public void PerformDuplicateLayerClick()
        {
            this.OnDuplicateLayerButtonClick();
        }

        public void PerformMoveLayerDownClick()
        {
            this.OnMoveLayerDownButtonClick();
        }

        public void PerformMoveLayerUpClick()
        {
            this.OnMoveLayerUpButtonClick();
        }

        public void PerformNewLayerClick()
        {
            this.OnNewLayerButtonClick();
        }

        public void PerformPropertiesClick()
        {
            this.OnPropertiesButtonClick();
        }

        private void UpdateLayersScrollBar()
        {
            SizeInt32 itemSize = this.layersStrip.ItemSize;
            RectInt32 viewRectangle = this.layersStrip.ViewRectangle;
            this.layersScrollBar.Minimum = 0;
            this.layersScrollBar.Maximum = viewRectangle.Height;
            this.layersScrollBar.SmallChange = itemSize.Height;
            this.layersScrollBar.LargeChange = this.layersStrip.ClientSize.Height;
            if (this.layersStrip.ClientRectangle.Height > viewRectangle.Height)
            {
                this.layersScrollBar.Visible = false;
            }
            else
            {
                this.layersScrollBar.Visible = true;
            }
        }

        public PaintDotNet.Controls.AppWorkspace AppWorkspace
        {
            get => 
                this.appWorkspace;
            set
            {
                if (this.appWorkspace != null)
                {
                    ExceptionUtil.ThrowInvalidOperationException();
                }
                this.appWorkspace = value;
                this.appWorkspace.ActiveDocumentWorkspaceChanging += new EventHandler(this.OnActiveDocumentWorkspaceChanging);
                this.appWorkspace.ActiveDocumentWorkspaceChanged += new EventHandler(this.OnActiveDocumentWorkspaceChanged);
            }
        }

        protected override string SnapObstacleName =>
            "Layers";

        protected override ISnapObstaclePersist SnapObstacleSettings =>
            AppSettings.Instance.Window.Layers;
    }
}

