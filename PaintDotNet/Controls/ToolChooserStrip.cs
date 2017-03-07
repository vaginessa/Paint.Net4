namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Resources;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Tools;
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;

    internal class ToolChooserStrip : ToolStripEx
    {
        private System.Type activeTool;
        private PdnToolStripSplitButton chooseToolButton;
        private string chooseToolLabelText = PdnResources.GetString("ToolStripChooser.ChooseToolButton.Text");
        private int ignoreToolClicked;
        private ToolInfo[] toolInfos;
        private bool useToolNameForLabel;

        [field: CompilerGenerated]
        public event ToolClickedEventHandler ToolClicked;

        public ToolChooserStrip()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.chooseToolButton = new PdnToolStripSplitButton();
            base.SuspendLayout();
            this.chooseToolButton.Name = "chooseToolButton";
            this.chooseToolButton.Text = this.chooseToolLabelText;
            this.chooseToolButton.AutoToolTip = false;
            this.chooseToolButton.Image = PdnResources.GetImageResource("Icons.BlankIcon.png").Reference;
            this.chooseToolButton.TextImageRelation = TextImageRelation.TextBeforeImage;
            this.chooseToolButton.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            this.chooseToolButton.DropDownOpening += new EventHandler(this.OnChooseToolButtonDropDownOpening);
            this.chooseToolButton.DropDownClosed += new EventHandler(this.OnChooseToolButtonDropDownClosed);
            this.chooseToolButton.DropDownItemClicked += new ToolStripItemClickedEventHandler(this.OnChooseToolButtonDropDownItemClicked);
            this.chooseToolButton.Click += (sender, e) => this.chooseToolButton.ShowDropDown();
            this.Items.Add(this.chooseToolButton);
            base.ResumeLayout(false);
        }

        private void OnChooseToolButtonDropDownClosed(object sender, EventArgs e)
        {
            this.chooseToolButton.DropDownItems.Clear();
        }

        private void OnChooseToolButtonDropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolInfo tag = e.ClickedItem.Tag as ToolInfo;
            if (tag != null)
            {
                this.OnToolClicked(tag.ToolType);
            }
        }

        private void OnChooseToolButtonDropDownOpening(object sender, EventArgs e)
        {
            this.chooseToolButton.DropDownItems.Clear();
            for (int i = 0; i < this.toolInfos.Length; i++)
            {
                ToolStripMenuItem item = new ToolStripMenuItem {
                    Image = this.toolInfos[i].Image.Reference,
                    Text = this.toolInfos[i].DisplayName,
                    Tag = this.toolInfos[i]
                };
                if (this.toolInfos[i].ToolType == this.activeTool)
                {
                    item.Checked = true;
                }
                else
                {
                    item.Checked = false;
                }
                this.chooseToolButton.DropDownItems.Add(item);
            }
        }

        protected virtual void OnToolClicked(System.Type toolType)
        {
            if (this.ignoreToolClicked <= 0)
            {
                this.SetToolButtonLabel();
                if (this.ToolClicked != null)
                {
                    this.ToolClicked(this, new ToolClickedEventArgs(toolType));
                }
            }
        }

        public void SelectTool(System.Type toolType)
        {
            this.SelectTool(toolType, true);
        }

        public void SelectTool(System.Type toolType, bool raiseEvent)
        {
            if (!raiseEvent)
            {
                this.ignoreToolClicked++;
            }
            try
            {
                if (toolType != this.activeTool)
                {
                    foreach (ToolInfo info in this.toolInfos)
                    {
                        if (info.ToolType == toolType)
                        {
                            this.chooseToolButton.Image = info.Image.Reference;
                            this.activeTool = toolType;
                            this.SetToolButtonLabel();
                            return;
                        }
                    }
                }
            }
            finally
            {
                if (!raiseEvent)
                {
                    this.ignoreToolClicked--;
                }
            }
        }

        public bool SelectToolByName(string toolTypeName)
        {
            this.VerifyThreadAccess();
            System.Type toolType = this.toolInfos.FindByName(toolTypeName);
            if (toolType == null)
            {
                return false;
            }
            this.SelectTool(toolType);
            return true;
        }

        private void SetToolButtonLabel()
        {
            if (!this.useToolNameForLabel)
            {
                this.chooseToolButton.TextImageRelation = TextImageRelation.TextBeforeImage;
                this.chooseToolButton.Text = this.chooseToolLabelText;
            }
            else
            {
                this.chooseToolButton.TextImageRelation = TextImageRelation.ImageBeforeText;
                ToolInfo info = null;
                if (this.toolInfos != null)
                {
                    info = Array.Find<ToolInfo>(this.toolInfos, check => check.ToolType == this.activeTool);
                }
                if (info == null)
                {
                    this.chooseToolButton.Text = string.Empty;
                }
                else
                {
                    this.chooseToolButton.Text = info.DisplayName;
                }
            }
        }

        public void SetTools(ToolInfo[] newToolInfos)
        {
            this.toolInfos = newToolInfos;
        }

        public bool UseToolNameForLabel
        {
            get => 
                this.useToolNameForLabel;
            set
            {
                this.useToolNameForLabel = value;
                this.SetToolButtonLabel();
            }
        }
    }
}

