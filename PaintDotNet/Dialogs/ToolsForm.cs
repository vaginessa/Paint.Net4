namespace PaintDotNet.Dialogs
{
    using PaintDotNet.Controls;
    using PaintDotNet.Resources;
    using PaintDotNet.Settings.App;
    using PaintDotNet.Snap;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    internal sealed class ToolsForm : FloatingToolForm
    {
        private Container components;
        private PaintDotNet.Controls.ToolsControl toolsControl;

        public ToolsForm()
        {
            this.InitializeComponent();
            this.Text = PdnResources.GetString("MainToolBarForm.Text");
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
            this.toolsControl = new PaintDotNet.Controls.ToolsControl();
            base.SuspendLayout();
            this.toolsControl.Location = new Point(0, 0);
            this.toolsControl.Name = "toolsControl";
            this.toolsControl.Size = new Size(50, 0x58);
            this.toolsControl.TabIndex = 0;
            this.toolsControl.RelinquishFocus += new EventHandler(this.OnToolsControlRelinquishFocus);
            base.AutoScaleMode = AutoScaleMode.None;
            base.ClientSize = new Size(50, 0x111);
            base.Controls.Add(this.toolsControl);
            base.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            base.Name = "ToolsForm";
            base.Controls.SetChildIndex(this.toolsControl, 0);
            base.ResumeLayout(false);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            base.ClientSize = new Size(this.toolsControl.Width, this.toolsControl.Height);
        }

        private void OnToolsControlRelinquishFocus(object sender, EventArgs e)
        {
            this.OnRelinquishFocus();
        }

        protected override string SnapObstacleName =>
            "Tools";

        protected override ISnapObstaclePersist SnapObstacleSettings =>
            AppSettings.Instance.Window.Tools;

        public PaintDotNet.Controls.ToolsControl ToolsControl =>
            this.toolsControl;
    }
}

