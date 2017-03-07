namespace PaintDotNet.Controls.ToolConfigUI
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Resources;
    using PaintDotNet.Settings;
    using System;
    using System.Drawing;
    using System.Globalization;
    using System.Windows.Forms;

    internal sealed class BooleanSplitButton : PdnToolStripSplitButton
    {
        private bool isInitialized;
        private string resourceRoot;
        private BooleanSetting setting;

        public BooleanSplitButton(BooleanSetting setting, string resourceRoot)
        {
            Validate.Begin().IsNotNull<BooleanSetting>(setting, "setting").IsNotNullOrWhiteSpace(resourceRoot, "resourceRoot").Check();
            this.setting = setting;
            this.resourceRoot = resourceRoot;
            this.setting.ValueChangedT += new ValueChangedEventHandler<bool>(this.OnSettingValueChanged);
            base.Name = resourceRoot;
            this.DisplayStyle = ToolStripItemDisplayStyle.Image;
            base.AutoSize = true;
            base.Available = false;
        }

        private void AddDropDownItems(string resourceRoot)
        {
            this.AddMenuItem(this.resourceRoot, true);
            this.AddMenuItem(this.resourceRoot, false);
        }

        private void AddMenuItem(string resourceRoot, bool state)
        {
            Image reference = PdnResources.GetImageResource($"Icons.{this.resourceRoot}.{state.ToString(CultureInfo.InvariantCulture)}.png").Reference;
            ToolStripMenuItem item = new ToolStripMenuItem(PdnResources.GetString($"{this.resourceRoot}.{state.ToString(CultureInfo.InvariantCulture)}", KeyNotFoundResult.ThrowException), reference, new EventHandler(this.OnDropDownItemClick)) {
                Tag = state
            };
            base.DropDownItems.Add(item);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.setting != null))
            {
                this.setting.ValueChangedT -= new ValueChangedEventHandler<bool>(this.OnSettingValueChanged);
                this.setting = null;
            }
            base.Dispose(disposing);
        }

        protected override void OnAvailableChanged(EventArgs e)
        {
            if (base.Available && !this.isInitialized)
            {
                this.AddDropDownItems(this.resourceRoot);
                this.SyncUI();
                this.isInitialized = true;
            }
            base.OnAvailableChanged(e);
        }

        protected override void OnButtonClick(EventArgs e)
        {
            this.setting.Value = !this.setting.Value;
            base.OnButtonClick(e);
        }

        private void OnDropDownItemClick(object sender, EventArgs e)
        {
            this.setting.Value = (bool) ((ToolStripItem) sender).Tag;
        }

        private void OnSettingValueChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            this.SyncUI();
        }

        private void SyncUI()
        {
            if (base.Available)
            {
                base.Owner.SuspendLayout();
                if (this.setting.Value)
                {
                    this.Image = base.DropDownItems[0].Image;
                    this.Text = base.DropDownItems[0].Text;
                    ((ToolStripMenuItem) base.DropDownItems[0]).Checked = true;
                    ((ToolStripMenuItem) base.DropDownItems[1]).Checked = false;
                }
                else
                {
                    this.Image = base.DropDownItems[1].Image;
                    this.Text = base.DropDownItems[1].Text;
                    ((ToolStripMenuItem) base.DropDownItems[1]).Checked = true;
                    ((ToolStripMenuItem) base.DropDownItems[0]).Checked = false;
                }
                base.Owner.ResumeLayout();
            }
        }
    }
}

