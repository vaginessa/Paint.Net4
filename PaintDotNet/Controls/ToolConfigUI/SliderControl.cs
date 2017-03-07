namespace PaintDotNet.Controls.ToolConfigUI
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Settings;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal sealed class SliderControl : ToolStripControlHost
    {
        private FloatSetting setting;

        public SliderControl(FloatSetting setting) : base(CreateControlInstance())
        {
            Validate.IsNotNull<FloatSetting>(setting, "setting");
            this.setting = setting;
            this.setting.ValueChangedT += new ValueChangedEventHandler<float>(this.OnSettingValueChanged);
            this.Control.ToleranceChanged += new EventHandler(this.OnToleranceChanged);
            base.Control.Size = new Size(0xaf, 0x10);
            base.Control.Size = UIUtil.ScaleSize(base.Control.Size);
            base.AutoSize = false;
        }

        private static System.Windows.Forms.Control CreateControlInstance() => 
            new ToleranceSliderControl();

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.setting != null))
            {
                this.setting.ValueChangedT -= new ValueChangedEventHandler<float>(this.OnSettingValueChanged);
                this.setting = null;
            }
            base.Dispose(disposing);
        }

        private void OnSettingValueChanged(object sender, ValueChangedEventArgs<float> e)
        {
            this.SyncUI();
        }

        private void OnToleranceChanged(object sender, EventArgs e)
        {
            this.setting.Value = this.Control.Tolerance;
        }

        private void SyncUI()
        {
            this.Control.Tolerance = this.setting.Value;
        }

        public ToleranceSliderControl Control =>
            ((ToleranceSliderControl) base.Control);
    }
}

