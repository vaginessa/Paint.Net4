namespace PaintDotNet.Controls.ToolConfigUI
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Settings;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;

    internal class ToolConfigSplitButton<T> : PdnToolStripSplitButton
    {
        private bool isInitialized;
        private Func<T, ImageResource> itemDropDownImageSelector;
        private IEnumerable<T> itemsSource;
        private Func<T, string> itemTextSelector;
        private Func<T, ImageResource> itemToolBarImageSelector;
        private Setting<T> setting;

        public ToolConfigSplitButton(Setting<T> setting, IEnumerable<T> itemsSource, Func<T, string> itemTextSelector, Func<T, ImageResource> itemToolBarImageSelector, Func<T, ImageResource> itemDropDownImageSelector)
        {
            Validate.Begin().IsNotNull<Setting<T>>(setting, "setting").IsNotNull<IEnumerable<T>>(itemsSource, "itemsSource").IsNotNull<Func<T, string>>(itemTextSelector, "itemTextSelector").IsNotNull<Func<T, ImageResource>>(itemToolBarImageSelector, "itemToolBarImageSelector").IsNotNull<Func<T, ImageResource>>(itemDropDownImageSelector, "itemDropDownImageSelector").Check();
            this.setting = setting;
            this.itemsSource = itemsSource;
            this.itemTextSelector = itemTextSelector;
            this.itemToolBarImageSelector = itemToolBarImageSelector;
            this.itemDropDownImageSelector = itemDropDownImageSelector;
            this.setting.ValueChangedT += new ValueChangedEventHandler<T>(this.OnSettingValueChanged);
            base.Name = typeof(T).Name;
            this.DisplayStyle = ToolStripItemDisplayStyle.Image;
            base.AutoSize = true;
            base.AutoToolTip = false;
            base.Available = false;
        }

        private void AddDropDownItems()
        {
            foreach (T local in this.itemsSource)
            {
                this.AddMenuItem(local);
            }
        }

        private void AddMenuItem(T itemValue)
        {
            Image reference;
            string text = this.itemTextSelector(itemValue);
            ImageResource resource = this.itemDropDownImageSelector(itemValue);
            if (resource == null)
            {
                reference = null;
            }
            else
            {
                reference = resource.Reference;
            }
            ToolStripMenuItem item = new ToolStripMenuItem(text, reference, new EventHandler(this.OnDropDownItemClick)) {
                Tag = itemValue,
                ImageScaling = ToolStripItemImageScaling.None
            };
            base.DropDownItems.Add(item);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.setting != null))
            {
                this.setting.ValueChangedT -= new ValueChangedEventHandler<T>(this.OnSettingValueChanged);
                this.setting = null;
            }
            base.Dispose(disposing);
        }

        protected override void OnAvailableChanged(EventArgs e)
        {
            if (base.Available && !this.isInitialized)
            {
                this.AddDropDownItems();
                this.SyncUI();
                this.isInitialized = true;
            }
            base.OnAvailableChanged(e);
        }

        protected override void OnButtonClick(EventArgs e)
        {
            int num = 0;
            while (num < base.DropDownItems.Count)
            {
                ToolStripItem item = base.DropDownItems[num];
                num++;
                if (this.setting.Value.Equals((T) item.Tag))
                {
                    break;
                }
            }
            num = num % base.DropDownItems.Count;
            this.setting.Value = (T) base.DropDownItems[num].Tag;
        }

        private void OnDropDownItemClick(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem) sender;
            this.setting.Value = (T) item.Tag;
        }

        private void OnSettingValueChanged(object sender, ValueChangedEventArgs<T> e)
        {
            this.SyncUI();
        }

        private void SyncUI()
        {
            if (base.Available)
            {
                base.Owner.SuspendLayout();
                foreach (ToolStripItem item in base.DropDownItems)
                {
                    ToolStripMenuItem item2 = (ToolStripMenuItem) item;
                    T tag = (T) item2.Tag;
                    if (tag.Equals(this.setting.Value))
                    {
                        item2.Checked = true;
                        this.Text = item2.Text;
                        if (this.itemToolBarImageSelector == null)
                        {
                            this.Image = item2.Image;
                        }
                        else
                        {
                            ImageResource resource = this.itemToolBarImageSelector(tag);
                            if (resource != null)
                            {
                                Image image;
                                UIUtil.ScaleImage(resource.Reference, out image);
                                this.Image = image;
                            }
                        }
                    }
                    else
                    {
                        item2.Checked = false;
                    }
                }
                base.Owner.ResumeLayout();
            }
        }
    }
}

