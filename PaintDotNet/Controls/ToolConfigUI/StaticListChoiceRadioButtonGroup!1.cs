namespace PaintDotNet.Controls.ToolConfigUI
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Settings;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;

    internal sealed class StaticListChoiceRadioButtonGroup<TValue> : Disposable
    {
        private List<ToolStripButton> buttons;
        private Func<TValue, ImageResource> getImageForValueFn;
        private Func<TValue, string> getTextForValueFn;
        private StaticListChoiceSetting<TValue> setting;
        private bool visible;

        public StaticListChoiceRadioButtonGroup(StaticListChoiceSetting<TValue> setting, Func<TValue, string> getTextForValueFn, Func<TValue, ImageResource> getImageForValueFn)
        {
            this.buttons = new List<ToolStripButton>();
            this.visible = true;
            Validate.Begin().IsNotNull<StaticListChoiceSetting<TValue>>(setting, "setting").IsNotNull<Func<TValue, string>>(getTextForValueFn, "getTextForValueFn").IsNotNull<Func<TValue, ImageResource>>(getImageForValueFn, "getImageForValueFn").Check();
            this.setting = setting;
            this.getTextForValueFn = getTextForValueFn;
            this.getImageForValueFn = getImageForValueFn;
            this.AddButtons();
            this.setting.ValueChangedT += new ValueChangedEventHandler<TValue>(this.OnSettingValueChanged);
        }

        private void AddButton(TValue value)
        {
            string str = this.getTextForValueFn(value);
            ToolStripButton sender = new ToolStripButton {
                Text = str
            };
            sender.Click += new EventHandler(this.OnButtonClick);
            sender.Tag = value;
            sender.DisplayStyle = ToolStripItemDisplayStyle.Image;
            sender.AvailableChanged += new EventHandler(this.OnButtonAvailableChanged);
            this.OnButtonAvailableChanged(sender, EventArgs.Empty);
            this.buttons.Add(sender);
        }

        private void AddButtons()
        {
            foreach (TValue local in this.setting.ValueChoices)
            {
                this.AddButton(local);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.setting != null))
            {
                this.setting.ValueChangedT -= new ValueChangedEventHandler<TValue>(this.OnSettingValueChanged);
                this.setting = null;
            }
            base.Dispose(disposing);
        }

        private void OnButtonAvailableChanged(object sender, EventArgs e)
        {
            ToolStripButton button = (ToolStripButton) sender;
            if ((button.Owner != null) && button.Available)
            {
                button.AvailableChanged -= new EventHandler(this.OnButtonAvailableChanged);
                TValue tag = (TValue) button.Tag;
                ImageResource resource = this.getImageForValueFn(tag);
                Image reference = resource?.Reference;
                ToolStripItemImageScaling sizeToFit = ToolStripItemImageScaling.SizeToFit;
                if (reference.Width > reference.Height)
                {
                    if ((UIUtil.GetXScaleFactor() != 1f) || (UIUtil.GetYScaleFactor() != 1f))
                    {
                        reference = new Bitmap(reference, UIUtil.ScaleSize(reference.Size));
                    }
                    sizeToFit = ToolStripItemImageScaling.None;
                }
                button.ImageScaling = sizeToFit;
                button.Image = reference;
            }
        }

        private void OnButtonClick(object sender, EventArgs e)
        {
            ToolStripItem item = (ToolStripItem) sender;
            this.setting.Value = (TValue) item.Tag;
        }

        private void OnSettingValueChanged(object sender, ValueChangedEventArgs<TValue> e)
        {
            this.SyncUI();
        }

        public void SyncUI()
        {
            foreach (ToolStripButton button in this.buttons)
            {
                TValue tag = (TValue) button.Tag;
                button.Checked = this.setting.ValueComparer.Equals(tag, this.setting.Value);
            }
        }

        public ToolStripButton[] Items =>
            this.buttons.ToArrayEx<ToolStripButton>();

        public bool Visible
        {
            get => 
                this.visible;
            set
            {
                if (this.visible != value)
                {
                    this.visible = value;
                    foreach (ToolStripButton button in this.buttons)
                    {
                        button.Visible = this.visible;
                    }
                }
            }
        }
    }
}

