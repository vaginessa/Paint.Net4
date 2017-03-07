namespace PaintDotNet.Settings.UI
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Resources;
    using PaintDotNet.Settings;
    using PaintDotNet.Settings.App;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Tools;
    using PaintDotNet.VisualStyling;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;

    internal class ToolsSettingsPage : SettingsDialogPage
    {
        private PdnLabel defaultToolLabel;
        private PdnLabel introText;
        private bool isInitialized;
        private PdnPushButton loadFromToolBarButton;
        private PdnPushButton resetButton;
        private ToolChooserStrip toolChooserStrip;
        private List<ToolConfigRow> toolConfigRows;
        private AppSettings.ToolsSection toolSettings;
        private System.Type toolType;

        public ToolsSettingsPage(ToolsSettingsSection section) : base(section)
        {
            this.toolType = PaintDotNet.Tools.Tool.DefaultToolType;
            this.toolConfigRows = new List<ToolConfigRow>();
            this.toolSettings = section.AppSettings.ToolDefaults;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.toolSettings.ActiveToolName.ValueChangedT -= new ValueChangedEventHandler<string>(this.OnToolSettingsActiveToolNameChanged);
            }
            base.Dispose(disposing);
        }

        private void Initialize()
        {
            using (new WaitCursorChanger(this))
            {
                base.SuspendLayout();
                this.introText = new PdnLabel();
                this.defaultToolLabel = new PdnLabel();
                this.loadFromToolBarButton = new PdnPushButton();
                this.resetButton = new PdnPushButton();
                this.toolChooserStrip = new ToolChooserStrip();
                this.introText.Name = "introText";
                this.introText.TabStop = false;
                this.introText.Text = PdnResources.GetString("SettingsDialog.Tools.IntroText.Text");
                this.defaultToolLabel.Name = "defaultToolLabel";
                this.defaultToolLabel.AutoSize = true;
                this.defaultToolLabel.TabStop = false;
                this.defaultToolLabel.Text = PdnResources.GetString("SettingsDialog.Tools.DefaultToolLabel.Text");
                this.resetButton.Name = "resetButton";
                this.resetButton.AutoSize = true;
                this.resetButton.Click += new EventHandler(this.OnResetButtonClick);
                this.resetButton.TabIndex = 0;
                this.resetButton.Text = PdnResources.GetString("SettingsDialog.Tools.ResetButton.Text");
                this.loadFromToolBarButton.Name = "loadFromToolBarButton";
                this.loadFromToolBarButton.AutoSize = true;
                this.loadFromToolBarButton.Click += new EventHandler(this.OnLoadFromToolBarButtonClick);
                this.loadFromToolBarButton.TabIndex = 1;
                this.loadFromToolBarButton.Text = PdnResources.GetString("SettingsDialog.Tools.LoadFromToolBarButton.Text");
                this.toolChooserStrip.Name = "toolChooserStrip";
                this.toolChooserStrip.Dock = DockStyle.None;
                this.toolChooserStrip.GripStyle = ToolStripGripStyle.Hidden;
                this.toolChooserStrip.UseToolNameForLabel = true;
                this.toolChooserStrip.ToolClicked += new ToolClickedEventHandler(this.OnToolChooserStripToolClicked);
                base.Controls.Add(this.loadFromToolBarButton);
                base.Controls.Add(this.resetButton);
                base.Controls.Add(this.introText);
                base.Controls.Add(this.defaultToolLabel);
                base.Controls.Add(this.toolChooserStrip);
                base.Location = new Point(0, 0);
                this.toolConfigRows.Add(new ToolConfigRow(this.toolSettings, ToolBarConfigItems.Brush | ToolBarConfigItems.LineCurveShapeType | ToolBarConfigItems.PenDashStyle | ToolBarConfigItems.PenEndCap | ToolBarConfigItems.PenHardness | ToolBarConfigItems.PenStartCap | ToolBarConfigItems.PenWidth | ToolBarConfigItems.Radius | ToolBarConfigItems.ShapeDrawType | ToolBarConfigItems.ShapeType));
                this.toolConfigRows.Add(new ToolConfigRow(this.toolSettings, ToolBarConfigItems.None | ToolBarConfigItems.SelectionCombineMode | ToolBarConfigItems.SelectionDrawMode));
                this.toolConfigRows.Add(new ToolConfigRow(this.toolSettings, ToolBarConfigItems.None | ToolBarConfigItems.Text));
                this.toolConfigRows.Add(new ToolConfigRow(this.toolSettings, ToolBarConfigItems.Gradient));
                this.toolConfigRows.Add(new ToolConfigRow(this.toolSettings, ToolBarConfigItems.FloodMode | ToolBarConfigItems.RecolorToolSamplingMode | ToolBarConfigItems.Tolerance));
                this.toolConfigRows.Add(new ToolConfigRow(this.toolSettings, ToolBarConfigItems.None | ToolBarConfigItems.PixelSampleMode | ToolBarConfigItems.SampleImageOrLayer));
                this.toolConfigRows.Add(new ToolConfigRow(this.toolSettings, ToolBarConfigItems.ColorPickerBehavior));
                this.toolConfigRows.Add(new ToolConfigRow(this.toolSettings, ToolBarConfigItems.None | ToolBarConfigItems.Resampling));
                this.toolConfigRows.Add(new ToolConfigRow(this.toolSettings, ToolBarConfigItems.Antialiasing | ToolBarConfigItems.BlendMode | ToolBarConfigItems.SelectionRenderingQuality));
                OurToolStripRenderer renderer = new OurToolStripRenderer {
                    DrawToolStripExBackgroundTopSeparatorLine = false
                };
                for (int i = 0; i < this.toolConfigRows.Count; i++)
                {
                    base.Controls.Add(this.toolConfigRows[i].HeaderLabel);
                    base.Controls.Add(this.toolConfigRows[i].ToolConfigStrip);
                    this.toolConfigRows[i].ToolConfigStrip.Renderer = renderer;
                    this.toolConfigRows[i].ToolConfigStrip.Layout += (s, e) => base.PerformLayout();
                }
                this.toolChooserStrip.Renderer = renderer;
                this.toolChooserStrip.SetTools(DocumentWorkspace.ToolInfos);
                this.toolChooserStrip.SelectToolByName(this.toolSettings.ActiveToolName.Value);
                this.toolSettings.ActiveToolName.ValueChangedT += new ValueChangedEventHandler<string>(this.OnToolSettingsActiveToolNameChanged);
                UIUtil.SuspendControlPainting(this);
                foreach (Setting setting in this.toolSettings.Settings)
                {
                    setting.RaiseValueChangedEvent();
                }
                UIUtil.ResumeControlPainting(this);
                base.ResumeLayout(false);
                base.PerformLayout();
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            if (!this.isInitialized)
            {
                this.isInitialized = true;
                this.Initialize();
            }
            base.OnHandleCreated(e);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            if (this.isInitialized)
            {
                int num = UIUtil.ScaleWidth(8);
                int num2 = UIUtil.ScaleHeight(8);
                int x = 0;
                int num4 = UIUtil.ScaleWidth(4);
                int num5 = 0;
                int num6 = UIUtil.ScaleWidth(7);
                int num7 = UIUtil.ScaleHeight(0x10);
                int num8 = UIUtil.ScaleHeight(3);
                Size size = UIUtil.ScaleSize(0x4b, 0x18);
                this.loadFromToolBarButton.Size = size;
                this.resetButton.Size = size;
                this.loadFromToolBarButton.PerformLayout();
                this.resetButton.PerformLayout();
                this.defaultToolLabel.PerformLayout();
                this.toolChooserStrip.PerformLayout();
                int num9 = (this.defaultToolLabel.Width + num) + this.toolChooserStrip.Width;
                int num10 = 0;
                for (int i = 0; i < this.toolConfigRows.Count; i++)
                {
                    Size preferredSize = this.toolConfigRows[i].ToolConfigStrip.GetPreferredSize(new Size(1, 1));
                    num10 = Math.Max(num10, preferredSize.Width);
                }
                int width = (base.ClientSize.Width - x) - num4;
                int y = num5;
                this.introText.Location = new Point(x, y);
                this.introText.Width = width;
                this.introText.Height = this.introText.GetPreferredSize(this.introText.Size).Height;
                y = this.introText.Bottom + num2;
                this.loadFromToolBarButton.Location = new Point(x, y);
                this.resetButton.Location = new Point(this.loadFromToolBarButton.Right + num, y);
                y = Math.Max(this.resetButton.Bottom, this.loadFromToolBarButton.Bottom) + ((num2 * 3) / 2);
                this.defaultToolLabel.Location = new Point(x, y);
                this.toolChooserStrip.Location = new Point(this.defaultToolLabel.Right + num, this.defaultToolLabel.Top + ((this.defaultToolLabel.Height - this.toolChooserStrip.Height) / 2));
                y = num2 + Math.Max(this.defaultToolLabel.Bottom, this.toolChooserStrip.Bottom);
                for (int j = 0; j < this.toolConfigRows.Count; j++)
                {
                    this.toolConfigRows[j].HeaderLabel.Location = new Point(x, y);
                    this.toolConfigRows[j].HeaderLabel.Size = this.toolConfigRows[j].HeaderLabel.GetPreferredSize(new Size(width, 1));
                    y = this.toolConfigRows[j].HeaderLabel.Bottom + num8;
                    this.toolConfigRows[j].ToolConfigStrip.Location = new Point(x + num4, y);
                    int num15 = (width - this.toolConfigRows[j].ToolConfigStrip.Left) - num4;
                    Size size4 = this.toolConfigRows[j].ToolConfigStrip.GetPreferredSize(new Size(num15, 1));
                    size4.Height += 2;
                    this.toolConfigRows[j].ToolConfigStrip.Size = size4;
                    y = this.toolConfigRows[j].ToolConfigStrip.Bottom + num2;
                }
            }
            base.OnLayout(levent);
        }

        private void OnLoadFromToolBarButtonClick(object sender, EventArgs e)
        {
            this.toolSettings.LoadFrom(((ToolsSettingsSection) base.Section).ToolBarSettings);
        }

        private void OnResetButtonClick(object sender, EventArgs e)
        {
            base.SuspendLayout();
            foreach (Setting setting in this.toolSettings.Settings)
            {
                setting.Reset();
            }
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void OnToolChooserStripToolClicked(object sender, ToolClickedEventArgs e)
        {
            this.toolSettings.ActiveToolName.Value = e.ToolType.Name;
            this.toolChooserStrip.SelectTool(e.ToolType, false);
        }

        private void OnToolSettingsActiveToolNameChanged(object sender, ValueChangedEventArgs<string> e)
        {
            this.toolChooserStrip.SelectToolByName(e.NewValue);
        }

        public void SetDefaultToolType(System.Type newDefaultToolType)
        {
            this.toolChooserStrip.SelectTool(newDefaultToolType);
        }

        public AppSettings.ToolsSection ToolSettings =>
            this.toolSettings;

        private sealed class OurToolStripRenderer : PdnToolStripRenderer
        {
            public override PaintDotNet.VisualStyling.AeroColorTheme AeroColorTheme =>
                AeroColors.GetTheme(AeroColorScheme.Light);
        }

        private sealed class ToolConfigRow
        {
            private HeadingLabel headerLabel;
            private PaintDotNet.ToolBarConfigItems toolBarConfigItems;
            private PaintDotNet.Controls.ToolConfigStrip toolConfigStrip;
            private AppSettings.ToolsSection toolSettings;

            public ToolConfigRow(AppSettings.ToolsSection toolSettings, PaintDotNet.ToolBarConfigItems toolBarConfigItems)
            {
                Validate.IsNotNull<AppSettings.ToolsSection>(toolSettings, "toolSettings");
                this.toolSettings = toolSettings;
                this.toolBarConfigItems = toolBarConfigItems;
                this.headerLabel = new HeadingLabel();
                this.headerLabel.Name = "headerLabel:" + toolBarConfigItems.ToString();
                string str2 = PdnResources.GetString(this.GetHeaderResourceName());
                this.headerLabel.Text = str2;
                this.headerLabel.RightMargin = 0;
                this.toolConfigStrip = new PaintDotNet.Controls.ToolConfigStrip(toolSettings);
                this.toolConfigStrip.Name = "toolConfigStrip:" + toolBarConfigItems.ToString();
                this.toolConfigStrip.AutoSize = false;
                this.toolConfigStrip.Dock = DockStyle.None;
                this.toolConfigStrip.GripStyle = ToolStripGripStyle.Hidden;
                this.toolConfigStrip.LayoutStyle = ToolStripLayoutStyle.Flow;
                this.toolConfigStrip.ToolBarConfigItems = this.toolBarConfigItems;
                this.toolConfigStrip.ShowFirstAndLastSeparators = false;
            }

            private string GetHeaderResourceName()
            {
                string str2 = this.toolBarConfigItems.ToString().Replace(", ", "");
                return ("SettingsDialog.Tools.ToolConfigRow." + str2 + ".HeaderLabel.Text");
            }

            public HeadingLabel HeaderLabel =>
                this.headerLabel;

            public PaintDotNet.ToolBarConfigItems ToolBarConfigItems =>
                this.toolBarConfigItems;

            public PaintDotNet.Controls.ToolConfigStrip ToolConfigStrip =>
                this.toolConfigStrip;
        }
    }
}

