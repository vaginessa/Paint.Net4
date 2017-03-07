namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Tools;
    using PaintDotNet.VisualStyling;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;

    internal class ToolsControl : UserControl
    {
        private Container components;
        private int ignoreToolClicked;
        private const int tbWidth = 2;
        private ToolStripEx toolStripEx;
        private const int toolstripPadding = 1;

        [field: CompilerGenerated]
        public event EventHandler RelinquishFocus;

        [field: CompilerGenerated]
        public event ToolClickedEventHandler ToolClicked;

        public ToolsControl()
        {
            this.InitializeComponent();
            this.toolStripEx.Renderer = new ToolsControlRenderer();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
                this.components = null;
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.toolStripEx = new ToolStripEx();
            base.SuspendLayout();
            this.toolStripEx.Dock = DockStyle.Top;
            this.toolStripEx.GripStyle = ToolStripGripStyle.Hidden;
            this.toolStripEx.LayoutStyle = ToolStripLayoutStyle.Flow;
            this.toolStripEx.ItemClicked += new ToolStripItemClickedEventHandler(this.OnToolStripExItemClicked);
            this.toolStripEx.Name = "toolStripEx";
            this.toolStripEx.AutoSize = true;
            this.toolStripEx.RelinquishFocus += new EventHandler(this.OnToolStripExRelinquishFocus);
            this.toolStripEx.Padding = new Padding(0, 0, 1, 1);
            base.Controls.Add(this.toolStripEx);
            base.AutoScaleDimensions = new SizeF(96f, 96f);
            base.AutoScaleMode = AutoScaleMode.Dpi;
            base.Name = "MainToolBar";
            base.Size = new Size(0x30, 0x148);
            base.ResumeLayout(false);
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            int num;
            if (this.toolStripEx.Items.Count > 0)
            {
                ToolStripItem item = this.toolStripEx.Items[0];
                num = item.Width + item.Margin.Horizontal;
            }
            else
            {
                num = 0;
            }
            this.toolStripEx.Width = (num * 2) + UIUtil.ScaleWidth(1);
            this.toolStripEx.Height = this.toolStripEx.GetPreferredSize(this.toolStripEx.Size).Height;
            base.Width = this.toolStripEx.Width;
            base.Height = this.toolStripEx.Height;
            base.OnLayout(e);
            this.toolStripEx.Height++;
        }

        private void OnRelinquishFocus()
        {
            this.RelinquishFocus.Raise(this);
        }

        protected virtual void OnToolClicked(System.Type toolType)
        {
            if ((this.ignoreToolClicked <= 0) && (this.ToolClicked != null))
            {
                this.ToolClicked(this, new ToolClickedEventArgs(toolType));
            }
        }

        private void OnToolStripExItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            foreach (ToolStripButton button in this.toolStripEx.Items)
            {
                button.Checked = button == e.ClickedItem;
            }
            this.OnToolClicked((System.Type) e.ClickedItem.Tag);
        }

        private void OnToolStripExRelinquishFocus(object sender, EventArgs e)
        {
            this.OnRelinquishFocus();
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
                foreach (ToolStripButton button in this.toolStripEx.Items)
                {
                    if (((System.Type) button.Tag) == toolType)
                    {
                        this.OnToolStripExItemClicked(this, new ToolStripItemClickedEventArgs(button));
                        return;
                    }
                }
                throw new ArgumentException("Tool type not found");
            }
            finally
            {
                if (!raiseEvent)
                {
                    this.ignoreToolClicked--;
                }
            }
        }

        public void SetTools(ToolInfo[] toolInfos)
        {
            if (this.toolStripEx != null)
            {
                this.toolStripEx.Items.Clear();
            }
            ToolStripItem[] toolStripItems = new ToolStripItem[toolInfos.Length];
            string format = PdnResources.GetString("ToolsControl.ToolToolTip.Format");
            for (int i = 0; i < toolInfos.Length; i++)
            {
                ToolInfo info = toolInfos[i];
                ToolStripButton button = new ToolStripButton {
                    Image = info.Image.Reference,
                    Tag = info.ToolType,
                    ToolTipText = string.Format(format, info.DisplayName, char.ToUpperInvariant(info.HotKey).ToString()),
                    Padding = new Padding(2),
                    Margin = new Padding(1, 1, 0, 0)
                };
                toolStripItems[i] = button;
                if (i == (toolInfos.Length - 1))
                {
                    Image image2;
                    UIUtil.ScaleImage(info.LargeImage.Reference, out image2);
                    button.Image = image2;
                    button.ImageScaling = ToolStripItemImageScaling.None;
                    button.Padding = new Padding(2, 1, 2, 1);
                }
            }
            this.toolStripEx.Items.AddRange(toolStripItems);
        }

        private sealed class ToolsControlRenderer : PdnToolStripRenderer
        {
            private readonly PenBrushCache penBrushCache = PenBrushCache.ThreadInstance;

            protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
            {
                if (e.ToolStrip.Parent == null)
                {
                    base.OnRenderToolStripBackground(e);
                }
                else
                {
                    e.Graphics.FillRectangle(this.penBrushCache.GetSolidBrush(e.ToolStrip.Parent.BackColor), e.AffectedBounds);
                }
            }
        }
    }
}

