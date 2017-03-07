namespace PaintDotNet.Controls.ToolConfigUI
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Direct2D;
    using PaintDotNet.Imaging;
    using PaintDotNet.MemoryManagement;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.Settings;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.UI.Media;
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;

    internal sealed class BrushStyleComboBox : PdnToolStripComboBox
    {
        private EnumSetting<PaintDotNet.BrushType> brushTypeSetting;
        private readonly EnumLocalizer hatchStyleNames;
        private EnumSetting<System.Drawing.Drawing2D.HatchStyle> hatchStyleSetting;
        private readonly PenBrushCache penBrushCache;
        private string solidBrushText;

        public BrushStyleComboBox(EnumSetting<PaintDotNet.BrushType> brushTypeSetting, EnumSetting<System.Drawing.Drawing2D.HatchStyle> hatchStyleSetting) : base(false)
        {
            this.penBrushCache = PenBrushCache.ThreadInstance;
            this.hatchStyleNames = EnumLocalizer.Create(typeof(System.Drawing.Drawing2D.HatchStyle));
            Validate.Begin().IsNotNull<EnumSetting<PaintDotNet.BrushType>>(brushTypeSetting, "brushTypeSetting").IsNotNull<EnumSetting<System.Drawing.Drawing2D.HatchStyle>>(hatchStyleSetting, "hatchStyleSetting").Check();
            this.brushTypeSetting = brushTypeSetting;
            this.hatchStyleSetting = hatchStyleSetting;
            base.Name = "brushStyleComboBox";
            base.DropDownStyle = ComboBoxStyle.DropDownList;
            base.DropDownWidth = 0xea;
            base.DropDownHeight *= 2;
            base.AutoSize = true;
            base.AutoToolTip = true;
            this.Size = new Size(UIUtil.ScaleWidth(base.Width), base.Height);
            base.DropDownWidth = UIUtil.ScaleWidth(base.DropDownWidth);
            base.DropDownHeight = UIUtil.ScaleHeight(base.DropDownHeight);
            base.ComboBox.SelectedValueChanged += new EventHandler(this.OnComboBoxSelectedValueChanged);
            this.brushTypeSetting.ValueChangedT += new ValueChangedEventHandler<PaintDotNet.BrushType>(this.OnBrushTypeSettingValueChanged);
            this.hatchStyleSetting.ValueChangedT += new ValueChangedEventHandler<System.Drawing.Drawing2D.HatchStyle>(this.OnHatchStyleSettingValueChanged);
        }

        private void AddItems()
        {
            this.solidBrushText = PdnResources.GetString("BrushConfigWidget.SolidBrush.Text");
            base.Items.Add(this.solidBrushText);
            LocalizedEnumValue[] items = (from lev in this.hatchStyleNames.GetLocalizedEnumValues()
                orderby lev.EnumValue
                select lev).ToArrayEx<LocalizedEnumValue>();
            base.Items.AddRange(items);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.brushTypeSetting != null)
                {
                    this.brushTypeSetting.ValueChangedT -= new ValueChangedEventHandler<PaintDotNet.BrushType>(this.OnBrushTypeSettingValueChanged);
                    this.brushTypeSetting = null;
                }
                if (this.hatchStyleSetting != null)
                {
                    this.hatchStyleSetting.ValueChangedT -= new ValueChangedEventHandler<System.Drawing.Drawing2D.HatchStyle>(this.OnHatchStyleSettingValueChanged);
                    this.hatchStyleSetting = null;
                }
            }
            base.Dispose(disposing);
        }

        protected override DeviceBitmap GetItemBitmap(object item, int maxHeight)
        {
            if (item == this.solidBrushText)
            {
                return null;
            }
            LocalizedEnumValue value2 = (LocalizedEnumValue) item;
            System.Drawing.Drawing2D.HatchStyle enumValue = (System.Drawing.Drawing2D.HatchStyle) value2.EnumValue;
            using (IBitmap<ColorPbgra32> bitmap = BitmapAllocator.Pbgra32.Allocate<ColorPbgra32>(maxHeight, maxHeight, AllocationOptions.Default))
            {
                using (IDrawingContext context = DrawingContext.FromBitmap(bitmap, FactorySource.PerThread))
                {
                    PaintDotNet.UI.Media.HatchBrush brush = new PaintDotNet.UI.Media.HatchBrush((PaintDotNet.UI.Media.HatchStyle) enumValue, (ColorRgba128Float) Colors.Black, (ColorRgba128Float) Colors.White);
                    context.FillRectangle(new RectDouble(PointDouble.Zero, bitmap.Size), brush);
                }
                return new DeviceBitmap(bitmap);
            }
        }

        protected override string GetItemText(object item)
        {
            if (item == this.solidBrushText)
            {
                return this.solidBrushText;
            }
            string str = ((LocalizedEnumValue) item).ToString();
            return base.GetItemText(item);
        }

        protected override void OnAvailableChanged(EventArgs e)
        {
            if (base.Available && (base.ComboBox.Items.Count == 0))
            {
                this.AddItems();
            }
            this.SyncUI();
            base.OnAvailableChanged(e);
        }

        private void OnBrushTypeSettingValueChanged(object sender, ValueChangedEventArgs<PaintDotNet.BrushType> e)
        {
            this.SyncUI();
        }

        private void OnComboBoxSelectedValueChanged(object sender, EventArgs e)
        {
            if (this.solidBrushText == base.ComboBox.SelectedItem)
            {
                this.brushTypeSetting.Value = PaintDotNet.BrushType.Solid;
            }
            else if (base.ComboBox.SelectedIndex == -1)
            {
                this.brushTypeSetting.Value = PaintDotNet.BrushType.Solid;
            }
            else
            {
                this.hatchStyleSetting.Value = (System.Drawing.Drawing2D.HatchStyle) ((LocalizedEnumValue) base.ComboBox.SelectedItem).EnumValue;
                this.brushTypeSetting.Value = PaintDotNet.BrushType.Hatch;
            }
        }

        private void OnHatchStyleSettingValueChanged(object sender, ValueChangedEventArgs<System.Drawing.Drawing2D.HatchStyle> e)
        {
            this.SyncUI();
        }

        private void SyncUI()
        {
            if (((PaintDotNet.BrushType) this.brushTypeSetting.Value) == PaintDotNet.BrushType.Solid)
            {
                base.ComboBox.SelectedItem = this.solidBrushText;
            }
            else
            {
                base.ComboBox.SelectedItem = this.hatchStyleNames.GetLocalizedEnumValue(this.hatchStyleSetting.Value);
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly BrushStyleComboBox.<>c <>9 = new BrushStyleComboBox.<>c();
            public static Func<LocalizedEnumValue, object> <>9__8_0;

            internal object <AddItems>b__8_0(LocalizedEnumValue lev) => 
                lev.EnumValue;
        }
    }
}

