namespace PaintDotNet.Controls.ToolConfigUI
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Resources;
    using PaintDotNet.Settings;
    using PaintDotNet.Shapes;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Threading;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Reflection;
    using System.Windows.Forms;

    internal class ShapeTypeDropDownButton : ToolStripDropDownButton
    {
        private const int dropDownItemImageSizeDip = 0x18;
        private bool isInitialized;
        private Screen lastDropDownScreen;
        private Rectangle lastWorkingArea = Rectangle.Empty;
        private StaticListChoiceSetting<ShapeInfo> setting;

        public ShapeTypeDropDownButton(StaticListChoiceSetting<ShapeInfo> setting)
        {
            Validate.IsNotNull<StaticListChoiceSetting<ShapeInfo>>(setting, "setting");
            this.setting = setting;
            this.setting.ValueChangedT += new ValueChangedEventHandler<ShapeInfo>(this.OnSettingValueChanged);
            this.DisplayStyle = ToolStripItemDisplayStyle.Image;
            base.AutoSize = true;
            base.AutoToolTip = false;
            base.Available = false;
            base.ToolTipText = PdnResources.GetString("ToolConfigUI.ShapeTypeDropDownButton.ToolTipText");
            WorkItemDispatcher.Default.Enqueue(delegate {
                foreach (ShapeInfo info in this.setting.ValueChoices)
                {
                    Image reference = ShapeManager.GetShape(info).GetImageResourceDip(0x18).Reference;
                }
            }, WorkItemQueuePriority.BelowNormal);
            base.DropDownOpening += new EventHandler(this.OnDropDownOpening);
        }

        private void AddDropDownItems()
        {
            IList<ShapeInfo> valueChoices = this.setting.ValueChoices;
            WorkItemDispatcher.Default.Enqueue(delegate {
                foreach (ShapeInfo info in this.setting.ValueChoices)
                {
                    Shape shape = ShapeManager.GetShape(info);
                    WorkItemDispatcher.Default.Enqueue(() => Image reference = shape.GetImageResourceDip(0x18).Reference, WorkItemQueuePriority.AboveNormal);
                }
            }, WorkItemQueuePriority.AboveNormal);
            ShapeCategory? nullable = null;
            List<ToolStripItem> list = new List<ToolStripItem>();
            foreach (ShapeInfo info in valueChoices)
            {
                Shape shape = ShapeManager.GetShape(info);
                if (!nullable.HasValue || (((ShapeCategory) nullable.Value) != shape.Category))
                {
                    if (list.LastOrDefault<ToolStripItem>() != null)
                    {
                        ((FlowLayoutSettings) base.DropDown.LayoutSettings).SetFlowBreak(list.Last<ToolStripItem>(), true);
                    }
                    ShapeCategoryLabel item = this.CreateCategoryLabel(shape.Category);
                    list.Add(item);
                    ((FlowLayoutSettings) base.DropDown.LayoutSettings).SetFlowBreak(item, true);
                    nullable = new ShapeCategory?(shape.Category);
                }
                list.Add(this.CreateButtonItem(info));
            }
            base.DropDownItems.AddRange(list.ToArrayEx<ToolStripItem>());
        }

        private ToolStripButton CreateButtonItem(ShapeInfo shapeInfo)
        {
            Shape shape = ShapeManager.GetShape(shapeInfo);
            string displayName = shape.DisplayName;
            return new ToolStripButton(displayName, shape.GetImageResourceDip(0x18).Reference, new EventHandler(this.OnDropDownItemClick)) { 
                Padding = new Padding(2),
                Margin = new Padding(0, 0, 1, 1),
                Tag = shapeInfo,
                ImageScaling = ToolStripItemImageScaling.None,
                DisplayStyle = ToolStripItemDisplayStyle.Image,
                AutoToolTip = false,
                ToolTipText = shape.ToolTipText
            };
        }

        private ShapeCategoryLabel CreateCategoryLabel(ShapeCategory category) => 
            new ShapeCategoryLabel(PdnResources.GetString("ShapeCategory." + category.ToString())) { 
                BackColor = SystemColors.ControlDark,
                TextAlign = ContentAlignment.MiddleLeft
            };

        protected override ToolStripDropDown CreateDefaultDropDown() => 
            new ShapeTypeDropDown(this);

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.setting != null))
            {
                this.setting.ValueChangedT -= new ValueChangedEventHandler<ShapeInfo>(this.OnSettingValueChanged);
                this.setting = null;
            }
            base.Dispose(disposing);
        }

        private void DoDropDownLayout()
        {
            Screen screen = Screen.FromHandle(base.Owner.Handle);
            if (!screen.Equals(this.lastDropDownScreen) || !screen.WorkingArea.Equals(this.lastWorkingArea))
            {
                this.lastDropDownScreen = screen;
                this.lastWorkingArea = screen.WorkingArea;
                foreach (ToolStripItem item in base.DropDown.Items)
                {
                    ShapeCategoryLabel label = item as ShapeCategoryLabel;
                    if (label != null)
                    {
                        label.AutoSize = true;
                    }
                }
                int width = 270;
                while (true)
                {
                    int num2 = UIUtil.ScaleWidth(width);
                    base.DropDown.MaximumSize = new Size(num2, 0x8000);
                    base.DropDown.PerformLayout();
                    if ((num2 >= screen.WorkingArea.Width) || (base.DropDown.PreferredSize.Height < screen.WorkingArea.Height))
                    {
                        break;
                    }
                    width += 0x10;
                }
                base.DropDown.SuspendLayout();
                foreach (ToolStripItem item2 in base.DropDown.Items)
                {
                    ShapeCategoryLabel label2 = item2 as ShapeCategoryLabel;
                    if (label2 != null)
                    {
                        label2.AutoSize = false;
                        label2.Width = UIUtil.ScaleWidth(width);
                        label2.Height = label2.GetPreferredSize(Size.Empty).Height + 2;
                    }
                }
                base.DropDown.ResumeLayout(true);
            }
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

        private void OnDropDownItemClick(object sender, EventArgs e)
        {
            ToolStripButton button = (ToolStripButton) sender;
            this.setting.Value = (ShapeInfo) button.Tag;
        }

        private void OnDropDownOpening(object sender, EventArgs e)
        {
            this.DoDropDownLayout();
        }

        private void OnSettingValueChanged(object sender, ValueChangedEventArgs<ShapeInfo> e)
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
                    ToolStripButton button = item as ToolStripButton;
                    if (button != null)
                    {
                        ShapeInfo tag = (ShapeInfo) button.Tag;
                        if (tag.Equals(this.setting.Value))
                        {
                            button.Checked = true;
                            this.Text = button.Text;
                            this.Image = ShapeManager.GetShape(tag).GetImageResourceDip(0x10).Reference;
                        }
                        else
                        {
                            button.Checked = false;
                        }
                    }
                }
                base.Owner.ResumeLayout();
            }
        }

        private sealed class ShapeTypeDropDown : ToolStripDropDown
        {
            public ShapeTypeDropDown(ToolStripItem owner)
            {
                typeof(ToolStripDropDown).GetField("isAutoGenerated", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this, BooleanUtil.GetBoxed(true));
                typeof(ToolStripDropDown).GetField("ownerItem", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this, owner);
            }

            protected override LayoutSettings CreateLayoutSettings(ToolStripLayoutStyle style)
            {
                LayoutSettings settings = base.CreateLayoutSettings(style);
                if (style == ToolStripLayoutStyle.Flow)
                {
                    FlowLayoutSettings settings2 = settings as FlowLayoutSettings;
                    settings2.FlowDirection = FlowDirection.LeftToRight;
                    settings2.WrapContents = true;
                    return settings2;
                }
                return settings;
            }

            protected override Padding DefaultPadding =>
                new Padding(2, 0, 2, 3);
        }
    }
}

