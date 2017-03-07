namespace PaintDotNet.Controls.ToolConfigUI
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Resources;
    using PaintDotNet.Settings;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;

    internal sealed class EnumSplitButton<TEnumType> : PdnToolStripSplitButton
    {
        private Func<TEnumType, bool> enumValueFilter;
        private bool isInitialized;
        private string resourceSpecialization;
        private Setting<TEnumType> setting;
        private Func<TEnumType, ImageResource> toolBarImageSelector;

        public EnumSplitButton(Setting<TEnumType> setting) : this(setting, string.Empty)
        {
        }

        public EnumSplitButton(Setting<TEnumType> setting, string resourceSpecialization) : this(setting, resourceSpecialization, x => true)
        {
        }

        public EnumSplitButton(Setting<TEnumType> setting, string resourceSpecialization, Func<TEnumType, bool> enumValueFilter)
        {
            Validate.Begin().IsNotNull<Setting<TEnumType>>(setting, "setting").IsNotNull<string>(resourceSpecialization, "resourceSpecialization").IsNotNull<Func<TEnumType, bool>>(enumValueFilter, "enumValueFilter").Check();
            this.setting = setting;
            this.resourceSpecialization = resourceSpecialization;
            this.enumValueFilter = enumValueFilter;
            this.setting.ValueChangedT += new ValueChangedEventHandler<TEnumType>(this.OnSettingValueChanged);
            base.Name = typeof(TEnumType).Name + ":" + resourceSpecialization;
            this.DisplayStyle = ToolStripItemDisplayStyle.Image;
            base.AutoSize = true;
            base.AutoToolTip = false;
            base.Available = false;
            base.ImageScaling = ToolStripItemImageScaling.None;
        }

        private void AddDropDownItems(string resourceSpecialization)
        {
            foreach (LocalizedEnumValue value2 in from lev in EnumLocalizer.Create(typeof(TEnumType)).GetLocalizedEnumValues()
                where base.enumValueFilter((TEnumType) lev.EnumValue)
                orderby lev.EnumValue
                select lev)
            {
                this.AddMenuItem(value2, resourceSpecialization);
            }
        }

        private void AddMenuItem(LocalizedEnumValue enumValue, string resourceSpecialization)
        {
            string str = resourceSpecialization.IsNullOrEmpty() ? enumValue.EnumValueName : (enumValue.EnumValueName + "." + resourceSpecialization);
            Image reference = PdnResources.GetImageResource($"Icons.Enum.{typeof(TEnumType).Name}.{str}.png").Reference;
            Image scaledImage = null;
            if (reference != null)
            {
                UIUtil.ScaleImage(reference, out scaledImage);
            }
            ToolStripMenuItem item = new ToolStripMenuItem(enumValue.LocalizedName, scaledImage, new EventHandler(this.OnDropDownItemClick)) {
                Tag = enumValue.EnumValue,
                ImageScaling = ToolStripItemImageScaling.None
            };
            base.DropDownItems.Add(item);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.setting != null))
            {
                this.setting.ValueChangedT -= new ValueChangedEventHandler<TEnumType>(this.OnSettingValueChanged);
                this.setting = null;
            }
            base.Dispose(disposing);
        }

        protected override void OnAvailableChanged(EventArgs e)
        {
            if (base.Available && !this.isInitialized)
            {
                this.AddDropDownItems(this.resourceSpecialization);
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
                if (this.setting.Value.Equals((TEnumType) item.Tag))
                {
                    break;
                }
            }
            num = num % base.DropDownItems.Count;
            this.setting.Value = (TEnumType) base.DropDownItems[num].Tag;
        }

        private void OnDropDownItemClick(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem) sender;
            this.setting.Value = (TEnumType) item.Tag;
        }

        private void OnSettingValueChanged(object sender, ValueChangedEventArgs<TEnumType> e)
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
                    TEnumType tag = (TEnumType) item2.Tag;
                    if (tag.Equals(this.setting.Value))
                    {
                        item2.Checked = true;
                        this.Text = item2.Text;
                        if (this.toolBarImageSelector == null)
                        {
                            this.Image = item2.Image;
                        }
                        else
                        {
                            ImageResource resource = this.toolBarImageSelector(tag);
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

        public Func<TEnumType, ImageResource> ToolBarImageSelector
        {
            get => 
                this.toolBarImageSelector;
            set
            {
                this.toolBarImageSelector = value;
                this.SyncUI();
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly EnumSplitButton<TEnumType>.<>c <>9;
            public static Func<LocalizedEnumValue, object> <>9__12_1;
            public static Func<TEnumType, bool> <>9__9_0;

            static <>c()
            {
                EnumSplitButton<TEnumType>.<>c.<>9 = new EnumSplitButton<TEnumType>.<>c();
            }

            internal bool <.ctor>b__9_0(TEnumType x) => 
                true;

            internal object <AddDropDownItems>b__12_1(LocalizedEnumValue lev) => 
                lev.EnumValue;
        }
    }
}

