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
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;

    internal sealed class EnumRadioButtonGroup<TEnumType> : Disposable
    {
        private List<ToolStripButton> buttons;
        private Setting<TEnumType> setting;
        private bool visible;

        public EnumRadioButtonGroup(Setting<TEnumType> setting)
        {
            this.buttons = new List<ToolStripButton>();
            this.visible = true;
            Validate.IsNotNull<Setting<TEnumType>>(setting, "setting");
            this.setting = setting;
            this.AddButtons();
            this.setting.ValueChangedT += new ValueChangedEventHandler<TEnumType>(this.OnSettingValueChanged);
        }

        private void AddButton(LocalizedEnumValue enumValue)
        {
            Image reference = PdnResources.GetImageResource($"Icons.Enum.{typeof(TEnumType).Name}.{enumValue.EnumValueName}.png").Reference;
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
                ToolStripButton item = new ToolStripButton(enumValue.LocalizedName, reference, new EventHandler(this.OnButtonClick)) {
                    Tag = enumValue.EnumValue,
                    ImageScaling = sizeToFit,
                    DisplayStyle = ToolStripItemDisplayStyle.Image
                };
                this.buttons.Add(item);
            }
        }

        private void AddButtons()
        {
            foreach (LocalizedEnumValue value2 in from lev in EnumLocalizer.Create(typeof(TEnumType)).GetLocalizedEnumValues()
                orderby lev.EnumValue
                select lev)
            {
                this.AddButton(value2);
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
            this.setting.Value = (TEnumType) item.Tag;
        }

        private void OnSettingValueChanged(object sender, ValueChangedEventArgs<TEnumType> e)
        {
            this.SyncUI();
        }

        public void SyncUI()
        {
            foreach (ToolStripButton button in this.buttons)
            {
                button.Checked = ((TEnumType) button.Tag).Equals(this.setting.Value);
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
            public static readonly EnumRadioButtonGroup<TEnumType>.<>c <>9;
            public static Func<LocalizedEnumValue, object> <>9__10_0;

            static <>c()
            {
                EnumRadioButtonGroup<TEnumType>.<>c.<>9 = new EnumRadioButtonGroup<TEnumType>.<>c();
            }

            internal object <AddButtons>b__10_0(LocalizedEnumValue lev) => 
                lev.EnumValue;
        }
    }
}

