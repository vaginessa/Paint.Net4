namespace PaintDotNet.Controls.ToolConfigUI
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Resources;
    using PaintDotNet.Settings;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;

    internal sealed class EnumFlagsButtonGroup<TEnumType> : Disposable
    {
        private List<ToolStripButton> buttons;
        private Setting<TEnumType> setting;
        private bool visible;

        public EnumFlagsButtonGroup(Setting<TEnumType> setting)
        {
            this.buttons = new List<ToolStripButton>();
            this.visible = true;
            Validate.IsNotNull<Setting<TEnumType>>(setting, "setting");
            this.setting = setting;
            this.AddButtons();
            this.setting.ValueChangedT += new ValueChangedEventHandler<TEnumType>(this.OnSettingValueChanged);
        }

        private void AddButton(TEnumType enumValue)
        {
            ulong num = Convert.ToUInt64(enumValue);
            if (num != 0)
            {
                Image reference = PdnResources.GetImageResource($"Icons.Enum.{typeof(TEnumType).Name}.{enumValue.ToString()}.png").Reference;
                if (reference != null)
                {
                    ToolStripItemImageScaling sizeToFit = ToolStripItemImageScaling.SizeToFit;
                    if (reference.Width > reference.Height)
                    {
                        if ((UIUtil.GetXScaleFactor() != 1f) || (UIUtil.GetYScaleFactor() != 1f))
                        {
                            reference = new Bitmap(reference, UIUtil.ScaleSize(reference.Size));
                        }
                        sizeToFit = ToolStripItemImageScaling.None;
                    }
                    ToolStripButton item = new ToolStripButton(PdnResources.GetString(typeof(TEnumType).Name + "." + enumValue.ToString(), KeyNotFoundResult.ReturnNull), reference, new EventHandler(this.OnButtonClick)) {
                        Tag = num,
                        ImageScaling = sizeToFit,
                        DisplayStyle = ToolStripItemDisplayStyle.Image,
                        AutoToolTip = true
                    };
                    this.buttons.Add(item);
                }
            }
        }

        private void AddButtons()
        {
            foreach (object obj2 in from lev in Enum.GetValues(typeof(TEnumType)).Cast<object>().Distinct<object>()
                orderby Convert.ToUInt64(lev)
                select lev)
            {
                this.AddButton((TEnumType) obj2);
            }
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

        private void OnButtonClick(object sender, EventArgs e)
        {
            ToolStripItem item = (ToolStripItem) sender;
            ulong num = Convert.ToUInt64(this.setting.Value);
            ulong tag = (ulong) item.Tag;
            ulong num3 = num ^ tag;
            this.setting.Value = (TEnumType) Enum.ToObject(typeof(TEnumType), num3);
        }

        private void OnSettingValueChanged(object sender, ValueChangedEventArgs<TEnumType> e)
        {
            this.SyncUI();
        }

        public void SyncUI()
        {
            ulong num = Convert.ToUInt64(this.setting.Value);
            foreach (ToolStripButton button in this.buttons)
            {
                ulong tag = (ulong) button.Tag;
                button.Checked = (num & tag) == tag;
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

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly EnumFlagsButtonGroup<TEnumType>.<>c <>9;
            public static Func<object, ulong> <>9__10_0;

            static <>c()
            {
                EnumFlagsButtonGroup<TEnumType>.<>c.<>9 = new EnumFlagsButtonGroup<TEnumType>.<>c();
            }

            internal ulong <AddButtons>b__10_0(object lev) => 
                Convert.ToUInt64(lev);
        }
    }
}

