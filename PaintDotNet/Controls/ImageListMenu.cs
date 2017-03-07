namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.ComponentModel;
    using PaintDotNet.Direct2D;
    using PaintDotNet.DirectWrite;
    using PaintDotNet.Drawing;
    using PaintDotNet.Imaging;
    using PaintDotNet.Imaging.Proxies;
    using PaintDotNet.ObjectModel;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.UI.Media;
    using PaintDotNet.VisualStyling;
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class ImageListMenu : Control
    {
        private System.Windows.Forms.ComboBox comboBox;
        private DropShadowRenderer dropShadowRenderer = new DropShadowRenderer();
        private int imageXInset;
        private int imageYInset;
        private SizeInt32 itemSize = SizeInt32.Zero;
        private SizeInt32 maxImageSize = SizeInt32.Zero;
        private SelectionHighlightRenderer selectionHighlightRenderer = new SelectionHighlightRenderer();
        private int textLeftMargin;
        private int textRightMargin;
        private int textVMargin;

        [field: CompilerGenerated]
        public event EventHandler Closed;

        [field: CompilerGenerated]
        public event ValueEventHandler<Item> ItemClicked;

        public ImageListMenu()
        {
            this.InitializeComponent();
            this.imageXInset = UIUtil.ScaleWidth(5);
            this.imageYInset = UIUtil.ScaleHeight(6);
            this.textLeftMargin = UIUtil.ScaleWidth(4);
            this.textRightMargin = UIUtil.ScaleWidth(0x10);
            this.textVMargin = UIUtil.ScaleHeight(2);
        }

        private void DetermineMaxItemSize(IDrawingContext dc, Item[] items, out SizeInt32 maxItemSizeResult, out SizeInt32 maxImageSizeResult)
        {
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            foreach (Item item in items)
            {
                num = Math.Max(num, (item.Image == null) ? 0 : item.Image.Width);
                num2 = Math.Max(num2, (item.Image == null) ? 0 : item.Image.Height);
                TextLayoutAlgorithm? layoutAlgorithm = null;
                TextLayout resourceSource = UIText.CreateLayout(dc, item.Name, this.Font, layoutAlgorithm, HotkeyRenderMode.Ignore, 65535.0, 65535.0);
                TextMetrics metrics = dc.GetCachedOrCreateResource<ITextLayout>(resourceSource).Metrics;
                SizeInt32 num9 = new SizeInt32((int) Math.Ceiling((double) metrics.WidthMax), (int) Math.Ceiling((double) metrics.Height));
                num3 = Math.Max(num9.Width, num3);
                num4 = Math.Max(num9.Height, num4);
            }
            int recommendedExtent = this.dropShadowRenderer.GetRecommendedExtent(num, num2);
            int width = ((((((recommendedExtent + this.imageXInset) + num) + this.imageXInset) + recommendedExtent) + this.textLeftMargin) + num3) + this.textRightMargin;
            int height = Math.Max((int) ((((this.imageYInset + recommendedExtent) + num2) + this.imageYInset) + recommendedExtent), (int) ((this.textVMargin + num4) + this.textVMargin));
            maxItemSizeResult = new SizeInt32(width, height);
            maxImageSizeResult = new SizeInt32(num, num2);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposableUtil.Free<DropShadowRenderer>(ref this.dropShadowRenderer);
                DisposableUtil.Free<SelectionHighlightRenderer>(ref this.selectionHighlightRenderer);
            }
            base.Dispose(disposing);
        }

        public void HideImageList()
        {
            UIUtil.ShowComboBox(this.comboBox, false);
        }

        private void InitializeComponent()
        {
            this.comboBox = new System.Windows.Forms.ComboBox();
            this.comboBox.Name = "comboBox";
            this.comboBox.MeasureItem += new MeasureItemEventHandler(this.OnComboBoxMeasureItem);
            this.comboBox.DrawItem += new DrawItemEventHandler(this.OnComboBoxDrawItem);
            this.comboBox.DropDown += new EventHandler(this.OnComboBoxDropDown);
            this.comboBox.DropDownClosed += new EventHandler(this.OnComboBoxDropDownClosed);
            this.comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboBox.SelectionChangeCommitted += new EventHandler(this.OnComboBoxSelectionChangeCommitted);
            this.comboBox.DrawMode = DrawMode.OwnerDrawFixed;
            this.comboBox.Visible = true;
            base.TabStop = false;
            base.Controls.Add(this.comboBox);
            base.Name = "ImageListMenu";
        }

        private void OnClosed()
        {
            this.Closed.Raise(this);
        }

        private void OnComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index != -1)
            {
                RectInt32 rect = e.Bounds.ToRectInt32();
                using (IDrawingContext context = DrawingContextUtil.FromGraphics(e.Graphics, rect, false, FactorySource.PerThread))
                {
                    using (context.UseTranslateTransform((float) e.Bounds.X, (float) e.Bounds.Y, MatrixMultiplyOrder.Prepend))
                    {
                        HighlightState hover;
                        RectInt32 num2 = new RectInt32(0, 0, rect.Width, rect.Height);
                        Item item = (Item) this.comboBox.Items[e.Index];
                        if ((e.State & DrawItemState.Selected) > DrawItemState.None)
                        {
                            hover = HighlightState.Hover;
                        }
                        else
                        {
                            hover = HighlightState.Default;
                        }
                        Color embeddedTextColor = SelectionHighlight.GetEmbeddedTextColor(hover);
                        context.FillRectangle(num2, PaintDotNet.UI.Media.SystemBrushes.Window);
                        this.selectionHighlightRenderer.HighlightState = hover;
                        this.selectionHighlightRenderer.RenderBackground(context, num2);
                        int extent = 0;
                        if ((item.Image != null) && (item.Image.PixelFormat != System.Drawing.Imaging.PixelFormat.Undefined))
                        {
                            extent = this.dropShadowRenderer.GetRecommendedExtent(item.Image.Size.ToSizeInt32());
                            RectInt32 num4 = new RectInt32((this.imageXInset + extent) + ((this.maxImageSize.Width - item.Image.Width) / 2), (this.imageYInset + extent) + ((this.maxImageSize.Height - item.Image.Height) / 2), item.Image.Width, item.Image.Height);
                            context.DrawBitmap(item.DeviceImage, new RectDouble?(num4), 1.0, BitmapInterpolationMode.Linear, null);
                            this.dropShadowRenderer.RenderOutside(context, num4, extent);
                        }
                        TextLayout resourceSource = UIText.CreateLayout(context, item.Name, this.Font, null, HotkeyRenderMode.Ignore, (double) e.Bounds.Width, (double) e.Bounds.Height);
                        ITextLayout cachedOrCreateResource = context.GetCachedOrCreateResource<ITextLayout>(resourceSource);
                        int num5 = ((((this.imageXInset + extent) + this.maxImageSize.Width) + extent) + this.imageXInset) + this.textLeftMargin;
                        int num6 = (this.itemSize.Height - ((int) cachedOrCreateResource.Metrics.Height)) / 2;
                        context.DrawTextLayout((double) num5, (double) num6, resourceSource, this.selectionHighlightRenderer.EmbeddedTextBrush, DrawTextOptions.None);
                    }
                }
            }
        }

        private void OnComboBoxDropDown(object sender, EventArgs e)
        {
            MenuStripEx.PushMenuActivate();
        }

        private void OnComboBoxDropDownClosed(object sender, EventArgs e)
        {
            MenuStripEx.PopMenuActivate();
            this.comboBox.Items.Clear();
            this.OnClosed();
        }

        private void OnComboBoxMeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemWidth = this.itemSize.Width;
            e.ItemHeight = this.itemSize.Height;
        }

        private void OnComboBoxSelectionChangeCommitted(object sender, EventArgs e)
        {
            int selectedIndex = this.comboBox.SelectedIndex;
            if ((selectedIndex >= 0) && (selectedIndex < this.comboBox.Items.Count))
            {
                this.OnItemClicked((Item) this.comboBox.Items[selectedIndex]);
            }
        }

        private void OnItemClicked(Item item)
        {
            this.ItemClicked.Raise<Item>(this, item);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            this.comboBox.Location = new PointInt32(0, -this.comboBox.Height).ToGdipPoint();
            base.OnLayout(levent);
        }

        public void ShowImageList(Item[] items)
        {
            this.HideImageList();
            this.comboBox.Items.AddRange(items);
            using (IDrawingContext context = DrawingContext.CreateNull(FactorySource.PerThread))
            {
                this.DetermineMaxItemSize(context, items, out this.itemSize, out this.maxImageSize);
            }
            this.comboBox.ItemHeight = this.itemSize.Height;
            this.comboBox.DropDownWidth = (this.itemSize.Width + SystemInformation.VerticalScrollBarWidth) + UIUtil.ScaleWidth(2);
            System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.FromControl(this);
            PointInt32 num = this.PointToScreen(new PointInt32(this.comboBox.Left, this.comboBox.Bottom));
            int num2 = screen.WorkingArea.Height - num.Y;
            num2 = this.itemSize.Height * (num2 / this.itemSize.Height);
            num2 += 2;
            int num3 = 2 + (this.itemSize.Height * 3);
            int num4 = Math.Max(num2, num3);
            this.comboBox.DropDownHeight = num4;
            int num5 = Array.FindIndex<Item>(items, item => item.Selected);
            this.comboBox.SelectedIndex = num5;
            int x = this.PointToScreen(new PointInt32(0, base.Height)).X;
            if ((x + this.comboBox.DropDownWidth) > screen.WorkingArea.Right)
            {
                x = screen.WorkingArea.Right - this.comboBox.DropDownWidth;
            }
            PointInt32 num7 = this.PointToClient(new PointInt32(x, num.Y));
            base.SuspendLayout();
            this.comboBox.Left = num7.X;
            base.ResumeLayout(false);
            this.comboBox.Focus();
            UIUtil.ShowComboBox(this.comboBox, true);
        }

        public bool IsImageListVisible =>
            this.comboBox.DroppedDown;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly ImageListMenu.<>c <>9 = new ImageListMenu.<>c();
            public static Predicate<ImageListMenu.Item> <>9__22_0;

            internal bool <ShowImageList>b__22_0(ImageListMenu.Item item) => 
                item.Selected;
        }

        public sealed class Item
        {
            private DeviceBitmap deviceImage;
            private System.Drawing.Image image;
            private string name;
            private bool selected;
            private object tag;

            public Item(System.Drawing.Image image, string name, bool selected)
            {
                this.image = image;
                this.name = name;
                this.selected = selected;
                if (image != null)
                {
                    Surface cleanupObject = Surface.CopyFromGdipImage(this.image);
                    using (BitmapProxy proxy = new BitmapProxy(cleanupObject.CreateAliasedImagingBitmap(), ObjectRefProxyOptions.AssumeOwnership))
                    {
                        proxy.AddCleanupObject(cleanupObject);
                        this.deviceImage = new DeviceBitmap(proxy);
                    }
                }
            }

            public override string ToString() => 
                this.name;

            internal DeviceBitmap DeviceImage =>
                this.deviceImage;

            public System.Drawing.Image Image =>
                this.image;

            public string Name =>
                this.name;

            public bool Selected =>
                this.selected;

            public object Tag
            {
                get => 
                    this.tag;
                set
                {
                    this.tag = value;
                }
            }
        }
    }
}

