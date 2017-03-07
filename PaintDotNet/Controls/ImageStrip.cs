namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Animation;
    using PaintDotNet.Collections;
    using PaintDotNet.Direct2D;
    using PaintDotNet.Drawing;
    using PaintDotNet.Imaging;
    using PaintDotNet.MemoryManagement;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.UI.Media;
    using PaintDotNet.VisualStyling;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;

    internal class ImageStrip : Direct2DControl
    {
        private const int busyAnimationFPS = 20;
        private AnimatedInt32 busyAnimationFrame;
        private Image[] busyAnimationFrames;
        private DeviceBitmap[] busyAnimationFramesDevice;
        private const double closeButtonFadeInDuration = 0.2;
        private const double closeButtonFadeOutDuration = 0.5;
        private const int closeButtonLength = 0x11;
        private const int closeButtonPadding = 0;
        private int ctorThreadID;
        private const double dirtyOverlayFadeInDuration = 1.0;
        private const double dirtyOverlayFadeOutDuration = 1.0;
        private const int dirtyOverlayLength = 11;
        private const int dirtyOverlayPaddingLeft = 0;
        private const int dirtyOverlayPaddingTop = 2;
        private bool drawDirtyOverlay;
        private bool drawShadow;
        private Dictionary<SizeInt32, DeviceBitmap> dropShadowDeviceBitmapCache;
        private DropShadowRenderer dropShadowRenderer;
        private const double hoverHighlightFadeInDuration = 0.05;
        private const double hoverHighlightFadeOutDuration = 0.05;
        private RenderLayer hoverHighlightLayer;
        private const int imagePadding = 4;
        private bool isMouseEntered;
        private bool isReordering;
        private List<Item> items;
        private PointInt32 lastMouseMoveClientPt;
        private PaintDotNet.Controls.ArrowButton leftScrollButton;
        private bool managedFocus;
        private bool mouseDownApplyRendering;
        private MouseButtons mouseDownButton;
        private PointInt32 mouseDownClientPt;
        private int mouseDownIndex;
        private ItemPart mouseDownItemPart;
        private PointInt32 mouseDownViewPt;
        private bool mouseOverApplyRendering;
        private int mouseOverIndex;
        private ItemPart mouseOverItemPart;
        private const double reorderAnimationDuration = 0.25;
        private int reorderInsertIndex;
        private int reorderSourceIndex;
        private PaintDotNet.Controls.ArrowButton rightScrollButton;
        private AnimatedInt32 scrollOffset;
        private const double scrollOffsetAnimationDuration = 0.35;
        private SelectionHighlightRenderer selectionHighlightRenderer;
        private AnimatedDouble selectionHighlightRenderSlot;
        private const double selectionHighlightSnapDuration = 0.1;
        private bool showCloseButtons;
        private bool showScrollButtons;

        [field: CompilerGenerated]
        public event ValueEventHandler<Tuple<Item, ItemPart, MouseButtons>> ItemClicked;

        [field: CompilerGenerated]
        public event ImageStripItemMovedEventHandler ItemMoved;

        [field: CompilerGenerated]
        public event ImageStripItemMovingEventHandler ItemMoving;

        [field: CompilerGenerated]
        public event EventHandler LayoutRequested;

        [field: CompilerGenerated]
        public event EventHandler RelinquishFocus;

        [field: CompilerGenerated]
        public event ValueEventHandler<ArrowDirection> ScrollArrowClicked;

        [field: CompilerGenerated]
        public event EventHandler ScrollOffsetChanged;

        public ImageStrip() : base(FactorySource.PerThread)
        {
            this.selectionHighlightRenderer = new SelectionHighlightRenderer();
            this.dropShadowRenderer = new DropShadowRenderer();
            this.reorderSourceIndex = -1;
            this.reorderInsertIndex = -1;
            this.mouseOverIndex = -1;
            this.hoverHighlightLayer = new RenderLayer();
            this.mouseDownIndex = -1;
            this.drawShadow = true;
            this.drawDirtyOverlay = true;
            this.dropShadowDeviceBitmapCache = new Dictionary<SizeInt32, DeviceBitmap>();
            this.lastMouseMoveClientPt = new PointInt32(-32000, -32000);
            this.items = new List<Item>();
            this.ctorThreadID = Thread.CurrentThread.ManagedThreadId;
            this.scrollOffset = new AnimatedInt32(0, AnimationRoundingMode.Floor);
            this.scrollOffset.ValueChanged += new ValueChangedEventHandler<int>(this.OnScrollOffsetValueChanged);
            this.busyAnimationFrame = new AnimatedInt32(0, AnimationRoundingMode.Floor);
            this.busyAnimationFrame.ValueChanged += new ValueChangedEventHandler<int>(this.OnBusyAnimationFrameValueChanged);
            this.selectionHighlightRenderSlot = new AnimatedDouble(0.0);
            this.selectionHighlightRenderSlot.ValueChanged += new ValueChangedEventHandler<double>(this.OnSelectionHighlightRenderSlotValueChanged);
            base.SetStyle(ControlStyles.Selectable, false);
            base.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            base.ResizeRedraw = true;
            this.AllowDrop = true;
            this.InitializeComponent();
            this.leftScrollButton.ArrowImage = PdnResources.GetImageResource("Images.ImageStrip.ScrollLeftArrow.png").Reference;
            this.rightScrollButton.ArrowImage = PdnResources.GetImageResource("Images.ImageStrip.ScrollRightArrow.png").Reference;
        }

        public void AddItem(Item newItem)
        {
            if (this.items.Contains(newItem))
            {
                throw new ArgumentException("newItem was already added to this control");
            }
            newItem.Changed += new EventHandler(this.OnItemChanged);
            newItem.RenderSlot.Value = this.items.Count;
            this.items.Add(newItem);
            if (newItem.Image == null)
            {
                this.StartBusyFrameAnimation();
            }
            base.PerformLayout();
            base.Invalidate();
        }

        private void AnimateToScrollOffset(int newScrollOffset)
        {
            int finalValue = this.ClampScrollOffset(newScrollOffset);
            if (this.scrollOffset.FinalValue != finalValue)
            {
                this.scrollOffset.AnimateValueTo(finalValue, 0.35, AnimationTransitionType.SmoothStop);
            }
        }

        private void CalculateVisibleScrollOffsets(int itemIndex, out int minOffset, out int maxOffset, out int minFullyShownOffset, out int maxFullyShownOffset)
        {
            RectInt32 num = this.ItemIndexToViewRect(itemIndex);
            minOffset = (num.Left + 1) - base.ClientSize.Width;
            maxOffset = num.Right - 1;
            minFullyShownOffset = num.Right - base.ClientSize.Width;
            maxFullyShownOffset = num.Left;
            if (this.leftScrollButton.Visible)
            {
                maxOffset -= this.leftScrollButton.Width;
                maxFullyShownOffset -= this.leftScrollButton.Width;
            }
            if (this.rightScrollButton.Visible)
            {
                minOffset += this.rightScrollButton.Width;
                minFullyShownOffset += this.rightScrollButton.Width;
            }
        }

        private int ClampScrollOffset(int scrollOffset) => 
            Int32Util.Clamp(scrollOffset, this.MinScrollOffset, this.MaxScrollOffset);

        public void ClearItems()
        {
            if (this.items.Count != 0)
            {
                this.VerifyThreadAccess();
                base.SuspendLayout();
                UIUtil.SuspendControlPainting(this);
                while (this.items.Count > 0)
                {
                    this.RemoveItem(this.items[this.items.Count - 1]);
                }
                UIUtil.ResumeControlPainting(this);
                base.ResumeLayout(true);
                base.Invalidate();
            }
        }

        public PointInt32 ClientPointToViewPoint(PointInt32 clientPt) => 
            new PointInt32(clientPt.X + this.ScrollOffset, clientPt.Y);

        public RectInt32 ClientRectToViewRect(RectInt32 clientRect) => 
            new RectInt32(this.ClientPointToViewPoint(clientRect.Location), clientRect.Size);

        public bool ContainsItem(Item item) => 
            this.items.Contains(item);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.StopBusyFrameAnimation();
                DisposableUtil.Free<SelectionHighlightRenderer>(ref this.selectionHighlightRenderer);
                DisposableUtil.Free<AnimatedDouble>(ref this.selectionHighlightRenderSlot);
                DisposableUtil.Free<DropShadowRenderer>(ref this.dropShadowRenderer);
                DisposableUtil.Free<AnimatedInt32>(ref this.scrollOffset);
                if (this.dropShadowDeviceBitmapCache != null)
                {
                    foreach (DeviceBitmap bitmap in this.dropShadowDeviceBitmapCache.Values)
                    {
                        bitmap.BitmapSource = null;
                    }
                    this.dropShadowDeviceBitmapCache.Clear();
                    this.dropShadowDeviceBitmapCache = null;
                }
                DisposableUtil.Free<AnimatedInt32>(ref this.busyAnimationFrame);
            }
            base.Dispose(disposing);
        }

        private void DrawItem(IDrawingContext dc, Item item, PointInt32 offset)
        {
            RectInt32 num;
            RectInt32 num2;
            RectInt32 num3;
            RectInt32 num4;
            RectInt32 num5;
            this.MeasureItemPartRectangles(item, out num, out num2, out num3, out num4, out num5);
            num.X += offset.X;
            num.Y += offset.Y;
            num2.X += offset.X;
            num2.Y += offset.Y;
            num3.X += offset.X;
            num3.Y += offset.Y;
            num4.X += offset.X;
            num4.Y += offset.Y;
            num5.X += offset.X;
            num5.Y += offset.Y;
            this.DrawItemBackground(dc, item, num);
            this.DrawItemForeground(dc, item, num, num2, num3, num4, num5);
        }

        protected virtual void DrawItemBackground(IDrawingContext dc, Item item, RectInt32 itemRect)
        {
            if (!item.IsSelected && (item.HoverHighlightOpacity.Value > 0.0))
            {
                this.hoverHighlightLayer.Size = new SizeDouble?(itemRect.Size);
                using (dc.UseLayer(this.hoverHighlightLayer, new RectDouble?(itemRect), null, AntialiasMode.PerPrimitive, null, item.HoverHighlightOpacity.Value, null, LayerOptions.None))
                {
                    this.selectionHighlightRenderer.HighlightState = HighlightState.Hover;
                    this.selectionHighlightRenderer.RenderBackground(dc, itemRect);
                }
            }
        }

        protected virtual void DrawItemCloseButton(IDrawingContext dc, Item item, RectInt32 itemRect, RectInt32 closeButtonRect)
        {
            switch (item.CloseButtonState)
            {
                case PushButtonState.Normal:
                case PushButtonState.Hot:
                case PushButtonState.Pressed:
                {
                    DeviceBitmap deviceBitmap = ImageResourceUtil.GetDeviceBitmap(ImageStripHelpers.GetCloseButtonImageResource(item.CloseButtonState));
                    float num = (float) DoubleUtil.Clamp(item.CloseButtonOpacity.Value, 0.0, 1.0);
                    dc.DrawBitmap(deviceBitmap, new RectDouble?(closeButtonRect), (double) num, BitmapInterpolationMode.Linear, null);
                    return;
                }
            }
        }

        protected virtual void DrawItemDirtyOverlay(IDrawingContext dc, Item item, RectInt32 itemRect, RectInt32 dirtyOverlayRect)
        {
            if (item.DirtyOverlayOpacity.Value > 0.0)
            {
                int num;
                if (dirtyOverlayRect.Width <= 11)
                {
                    num = 11;
                }
                else
                {
                    num = 0x12;
                }
                DeviceBitmap deviceBitmap = ImageResourceUtil.GetDeviceBitmap(PdnResources.GetImageResource($"Images.ImageStrip.DirtyOverlay.{num.ToString()}.png"));
                float num2 = (float) DoubleUtil.Clamp(item.DirtyOverlayOpacity.Value, 0.0, 1.0);
                dc.DrawBitmap(deviceBitmap, new RectDouble?(dirtyOverlayRect), (double) num2, BitmapInterpolationMode.Linear, null);
            }
        }

        protected virtual void DrawItemForeground(IDrawingContext dc, Item item, RectInt32 itemRect, RectInt32 imageRect, RectInt32 imageInsetRect, RectInt32 closeButtonRect, RectInt32 dirtyOverlayRect)
        {
            RectInt32 num = itemRect;
            if (this.drawShadow)
            {
                this.DrawItemImageShadow(dc, item, itemRect, imageRect, imageInsetRect);
            }
            this.DrawItemImage(dc, item, itemRect, imageRect, imageInsetRect);
            if (this.showCloseButtons)
            {
                this.DrawItemCloseButton(dc, item, itemRect, closeButtonRect);
            }
            if (this.drawDirtyOverlay)
            {
                this.DrawItemDirtyOverlay(dc, item, itemRect, dirtyOverlayRect);
            }
        }

        protected virtual void DrawItemImage(IDrawingContext dc, Item item, RectInt32 itemRect, RectInt32 imageRect, RectInt32 imageInsetRect)
        {
            if (item.Image == null)
            {
                int num = this.busyAnimationFrame.Value;
                DeviceBitmap bitmap = this.BusyAnimationFrames[num % this.BusyAnimationFrames.Length];
                RectInt32 num2 = new RectInt32(itemRect.X + ((imageRect.Width - bitmap.PixelSize.Width) / 2), itemRect.Y + ((imageRect.Height - bitmap.PixelSize.Height) / 2), bitmap.PixelSize.Width, bitmap.PixelSize.Height);
                dc.DrawBitmap(bitmap, new RectDouble?(num2), 1.0, BitmapInterpolationMode.Linear, null);
            }
            else
            {
                dc.DrawBitmap(item.DeviceImage, new RectDouble?(imageInsetRect), (double) ((float) item.ImageOpacity.Value), BitmapInterpolationMode.Linear, null);
            }
        }

        protected virtual void DrawItemImageShadow(IDrawingContext dc, Item item, RectInt32 itemRect, RectInt32 imageRect, RectInt32 imageInsetRect)
        {
            if (item.Image != null)
            {
                DeviceBitmap bitmap;
                SizeInt32 size = imageInsetRect.Size;
                int recommendedExtent = this.dropShadowRenderer.GetRecommendedExtent(size);
                if (!this.dropShadowDeviceBitmapCache.TryGetValue(size, out bitmap))
                {
                    SizeInt32 num4 = new SizeInt32(size.Width + recommendedExtent, size.Height + recommendedExtent);
                    using (IBitmap<ColorPbgra32> bitmap2 = BitmapAllocator.Pbgra32.Allocate(num4, AllocationOptions.Default))
                    {
                        using (IDrawingContext context = DrawingContext.FromBitmap(bitmap2, FactorySource.PerThread))
                        {
                            context.Clear(null);
                            this.dropShadowRenderer.RenderInside(context, new RectInt32(0, 0, num4.Width, num4.Height), recommendedExtent);
                        }
                        bitmap = new DeviceBitmap(bitmap2);
                        this.dropShadowDeviceBitmapCache.Add(size, bitmap);
                    }
                }
                RectInt32 num3 = RectInt32.Inflate(imageInsetRect, recommendedExtent, recommendedExtent);
                dc.DrawBitmap(bitmap, new RectDouble?(num3), (double) ((float) item.ImageOpacity.Value), BitmapInterpolationMode.NearestNeighbor, null);
            }
        }

        protected virtual void DrawSelectionHighlight(IDrawingContext dc, RectInt32 highlightRect)
        {
            this.selectionHighlightRenderer.HighlightState = HighlightState.Checked;
            this.selectionHighlightRenderer.RenderBackground(dc, highlightRect);
        }

        private void EnsureBusyAnimationFramesIsInitialized()
        {
            if (this.busyAnimationFrames == null)
            {
                this.busyAnimationFrames = AnimatedResources.Working;
                this.busyAnimationFramesDevice = new DeviceBitmap[this.busyAnimationFrames.Length];
                for (int i = 0; i < this.busyAnimationFramesDevice.Length; i++)
                {
                    this.busyAnimationFramesDevice[i] = this.busyAnimationFrames[i].CreateDeviceBitmap();
                }
            }
        }

        public void EnsureItemFullyVisible(Item item)
        {
            int index = this.items.IndexOf(item);
            this.EnsureItemFullyVisible(index);
        }

        public void EnsureItemFullyVisible(int index)
        {
            if (!this.IsItemFullyVisible(index))
            {
                int num;
                int num2;
                int num3;
                int num4;
                int num8;
                this.CalculateVisibleScrollOffsets(index, out num, out num2, out num3, out num4);
                int scrollOffset = this.ScrollOffset;
                int num6 = Math.Abs((int) (scrollOffset - num3));
                int num7 = Math.Abs((int) (scrollOffset - num4));
                if (num6 <= num7)
                {
                    num8 = num3;
                }
                else
                {
                    num8 = num4;
                }
                this.SmoothScrollToOffset(num8);
            }
        }

        private void ForceMouseMove()
        {
            PointInt32 num = base.PointToClient(Control.MousePosition).ToPointInt32();
            this.lastMouseMoveClientPt = new PointInt32(this.lastMouseMoveClientPt.X + 1, this.lastMouseMoveClientPt.Y + 1);
            MouseEventArgs e = new MouseEventArgs(MouseButtons.None, 0, num.X, num.Y, 0);
            this.OnMouseMove(e);
        }

        private void GetFocus()
        {
            if ((this.managedFocus && !MenuStripEx.IsAnyMenuActive) && (UIUtil.IsOurAppActive && !this.Focused))
            {
                base.Focus();
            }
        }

        private void InitializeComponent()
        {
            this.leftScrollButton = new PaintDotNet.Controls.ArrowButton();
            this.rightScrollButton = new PaintDotNet.Controls.ArrowButton();
            base.SuspendLayout();
            this.leftScrollButton.Name = "leftScrollButton";
            this.leftScrollButton.ArrowDirection = ArrowDirection.Left;
            this.leftScrollButton.ArrowOutlineWidth = 1f;
            this.leftScrollButton.Click += new EventHandler(this.OnLeftScrollButtonClick);
            this.rightScrollButton.Name = "rightScrollButton";
            this.rightScrollButton.ArrowDirection = ArrowDirection.Right;
            this.rightScrollButton.ArrowOutlineWidth = 1f;
            this.rightScrollButton.Click += new EventHandler(this.OnRightScrollButtonClick);
            base.Name = "ImageStrip";
            base.TabStop = false;
            base.Controls.Add(this.leftScrollButton);
            base.Controls.Add(this.rightScrollButton);
            base.ResumeLayout();
            base.PerformLayout();
        }

        public bool IsItemFullyVisible(int index)
        {
            RectInt32 a = this.ItemIndexToViewRect(index);
            RectInt32 scrolledViewRect = this.ScrolledViewRect;
            if (this.leftScrollButton.Visible)
            {
                scrolledViewRect.X += this.leftScrollButton.Width;
                scrolledViewRect.Width -= this.leftScrollButton.Width;
            }
            if (this.rightScrollButton.Visible)
            {
                scrolledViewRect.Width -= this.rightScrollButton.Width;
            }
            return (RectInt32.Intersect(a, scrolledViewRect) == a);
        }

        public bool IsItemVisible(int index)
        {
            RectInt32 num2 = RectInt32.Intersect(this.ItemIndexToViewRect(index), this.ScrolledViewRect);
            if (num2.Width <= 0)
            {
                return (num2.Height > 0);
            }
            return true;
        }

        private RectInt32 ItemIndexToClientRect(int itemIndex)
        {
            RectInt32 viewRect = this.ItemIndexToViewRect(itemIndex);
            return this.ViewRectToClientRect(viewRect);
        }

        public Item ItemIndexToItem(int index) => 
            this.items[index];

        private RectInt32 ItemIndexToViewRect(int itemIndex)
        {
            SizeInt32 itemSize = this.ItemSize;
            return new RectInt32(itemIndex * itemSize.Width, 0, itemSize.Width, itemSize.Height);
        }

        private ItemPart ItemPointToItemPart(Item item, PointInt32 pt)
        {
            RectInt32 num;
            RectInt32 num2;
            RectInt32 num3;
            RectInt32 num4;
            RectInt32 num5;
            this.MeasureItemPartRectangles(item, out num, out num2, out num3, out num4, out num5);
            if (num4.Contains(pt))
            {
                return ItemPart.CloseButton;
            }
            if (num2.Contains(pt))
            {
                return ItemPart.Image;
            }
            return ItemPart.None;
        }

        public int ItemToItemIndex(Item item) => 
            this.items.IndexOf(item);

        private void JumpToScrollOffset(int newScrollOffset)
        {
            int num = this.ClampScrollOffset(newScrollOffset);
            if (this.ScrollOffset != num)
            {
                this.scrollOffset.Value = num;
            }
        }

        private void MeasureItemPartRectangles(out RectInt32 itemRect, out RectInt32 imageRect)
        {
            itemRect = new RectInt32(0, 0, base.ClientSize.Height, base.ClientSize.Height);
            imageRect = new RectInt32(itemRect.Left, itemRect.Top, itemRect.Width, itemRect.Width);
        }

        private void MeasureItemPartRectangles(Item item, out RectInt32 itemRect, out RectInt32 imageRect, out RectInt32 imageInsetRect, out RectInt32 closeButtonRect, out RectInt32 dirtyOverlayRect)
        {
            SizeInt32 size;
            this.MeasureItemPartRectangles(out itemRect, out imageRect);
            RectInt32 num = new RectInt32(imageRect.Left + 4, imageRect.Top + 4, imageRect.Width - 8, imageRect.Height - 8);
            if (item.Image == null)
            {
                size = imageRect.Size;
            }
            else
            {
                size = ThumbnailHelpers.ComputeThumbnailSize(item.Image.Size.ToSizeInt32(), num.Width);
            }
            imageInsetRect = new RectInt32(num.Left + ((num.Width - size.Width) / 2), (num.Bottom - size.Height) - 1, size.Width, size.Height);
            int width = UIUtil.ScaleWidth(0x11);
            int num4 = UIUtil.ScaleWidth(0);
            closeButtonRect = new RectInt32((num.Right - width) - num4, num.Top + num4, width, width);
            int num5 = UIUtil.ScaleWidth(11);
            int num6 = UIUtil.ScaleWidth(2);
            int num7 = UIUtil.ScaleWidth(0);
            dirtyOverlayRect = new RectInt32(num.Left + num7, num.Top + num6, num5, num5);
        }

        private void MouseStatesToItemStates()
        {
            UIUtil.SuspendControlPainting(this);
            try
            {
                int? nullable = null;
                ItemPart none = ItemPart.None;
                PushButtonState normal = PushButtonState.Normal;
                if (this.mouseDownApplyRendering)
                {
                    if ((this.mouseDownIndex < 0) || (this.mouseDownIndex >= this.items.Count))
                    {
                        this.mouseDownApplyRendering = false;
                    }
                    else
                    {
                        nullable = new int?(this.mouseDownIndex);
                        none = this.mouseDownItemPart;
                        normal = PushButtonState.Pressed;
                    }
                }
                else if (this.mouseOverApplyRendering)
                {
                    if ((this.mouseOverIndex < 0) || (this.mouseOverIndex >= this.items.Count))
                    {
                        this.mouseOverApplyRendering = false;
                    }
                    else
                    {
                        nullable = new int?(this.mouseOverIndex);
                        none = this.mouseOverItemPart;
                        normal = PushButtonState.Hot;
                    }
                }
                for (int i = 0; i < this.items.Count; i++)
                {
                    double num2;
                    Item item = this.items[i];
                    if (!nullable.HasValue || (i != nullable.Value))
                    {
                        item.IsMouseOver = false;
                        item.HoverHighlightOpacity.AnimateValueTo(0.0, 0.05, AnimationTransitionType.SmoothStop);
                        item.CloseButtonState = PushButtonState.Normal;
                        num2 = 0.0;
                    }
                    else
                    {
                        item.IsMouseOver = true;
                        item.HoverHighlightOpacity.AnimateValueTo(1.0, 0.05, AnimationTransitionType.SmoothStop);
                        if (none == ItemPart.CloseButton)
                        {
                            item.CloseButtonState = normal;
                        }
                        else
                        {
                            item.CloseButtonState = PushButtonState.Normal;
                        }
                        if (item.IsSelected)
                        {
                            num2 = 1.0;
                        }
                        else
                        {
                            num2 = 0.0;
                        }
                    }
                    if (item.CloseButtonOpacity.FinalValue != num2)
                    {
                        double duration = (num2 == 1.0) ? 0.2 : 0.5;
                        item.CloseButtonOpacity.AnimateValueTo(num2, duration, AnimationTransitionType.SmoothStop);
                    }
                }
            }
            finally
            {
                UIUtil.ResumeControlPainting(this);
                base.Invalidate(true);
            }
        }

        public void MoveScrollByOffset(int deltaScrollOffset)
        {
            int newScrollOffset = this.scrollOffset.FinalValue + deltaScrollOffset;
            this.JumpToScrollOffset(newScrollOffset);
        }

        public void MoveScrollToOffset(int newScrollOffset)
        {
            this.VerifyThreadAccess();
            this.JumpToScrollOffset(newScrollOffset);
        }

        private void OnBusyAnimationFrameValueChanged(object sender, EventArgs e)
        {
            if (!(from doc in this.items
                where doc.Image == null
                select doc).Any<Item>())
            {
                this.StopBusyFrameAnimation();
            }
            else
            {
                for (int i = 0; i < this.items.Count; i++)
                {
                    Item item = this.items[i];
                    if ((item.Image == null) && this.IsItemVisible(i))
                    {
                        item.Update();
                    }
                }
                this.QueueUpdate();
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            if (Thread.CurrentThread.ManagedThreadId != this.ctorThreadID)
            {
                ExceptionUtil.ThrowInvalidOperationException("Control handle was created on a thread other than the one this object was constructed on");
            }
            base.OnHandleCreated(e);
        }

        private void OnItemChanged(object sender, EventArgs e)
        {
            if (!base.IsDisposed)
            {
                Item item = (Item) sender;
                if (item.Image == null)
                {
                    this.StartBusyFrameAnimation();
                }
                if (this.isReordering || item.RenderSlot.IsAnimating)
                {
                    base.Invalidate();
                }
                else
                {
                    RectInt32 a = this.RenderSlotToClientRect(item.RenderSlot.Value);
                    RectInt32 b = this.RenderSlotToClientRect(item.RenderSlot.PreviousValue);
                    RectInt32 rect = RectInt32.Union(a, b);
                    base.Invalidate(rect);
                }
                if (item.IsSelected)
                {
                    if (this.isReordering || item.RenderSlot.IsAnimating)
                    {
                        this.selectionHighlightRenderSlot.Value = item.RenderSlot.Value;
                    }
                    else if (this.selectionHighlightRenderSlot.FinalValue != item.RenderSlot.FinalValue)
                    {
                        this.selectionHighlightRenderSlot.AnimateValueTo(item.RenderSlot.FinalValue, 0.1, AnimationTransitionType.SmoothStop);
                    }
                }
                this.OnLayoutRequested();
            }
        }

        protected virtual void OnItemClicked(Item item, ItemPart itemPart, MouseButtons mouseButtons)
        {
            this.ItemClicked.Raise<Tuple<Item, ItemPart, MouseButtons>>(this, Tuple.Create<Item, ItemPart, MouseButtons>(item, itemPart, mouseButtons));
        }

        protected virtual void OnItemMoved(ImageStripItemMovedEventArgs e)
        {
            ImageStripItemMovedEventHandler itemMoved = this.ItemMoved;
            if (itemMoved != null)
            {
                itemMoved(this, e);
            }
        }

        protected virtual void OnItemMoving(ImageStripItemMovingEventArgs e)
        {
            ImageStripItemMovingEventHandler itemMoving = this.ItemMoving;
            if (itemMoving != null)
            {
                itemMoving(this, e);
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int width = UIUtil.ScaleWidth(0x10);
            Size clientSize = base.ClientSize;
            int scrollOffset = this.ScrollOffset;
            this.leftScrollButton.Bounds = new Rectangle(0, 0, width, clientSize.Height);
            this.rightScrollButton.Bounds = new Rectangle(clientSize.Width - this.rightScrollButton.Width, 0, width, clientSize.Height);
            bool flag = this.showScrollButtons && (this.ViewRectangle.Width > clientSize.Width);
            bool flag2 = (scrollOffset < this.MaxScrollOffset) & flag;
            bool flag3 = (scrollOffset > this.MinScrollOffset) & flag;
            this.rightScrollButton.Enabled = flag2;
            this.rightScrollButton.Visible = flag2;
            this.leftScrollButton.Enabled = flag3;
            this.leftScrollButton.Visible = flag3;
            base.OnLayout(levent);
        }

        private void OnLayoutRequested()
        {
            this.LayoutRequested.Raise(this);
        }

        private void OnLeftScrollButtonClick(object sender, EventArgs e)
        {
            base.Focus();
            this.OnScrollArrowClicked(ArrowDirection.Left);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (this.mouseDownButton == MouseButtons.None)
            {
                PointInt32 clientPt = new PointInt32(e.X, e.Y);
                PointInt32 viewPt = this.ClientPointToViewPoint(clientPt);
                this.mouseDownClientPt = clientPt;
                this.mouseDownViewPt = viewPt;
                this.mouseDownButton = e.Button;
                int itemIndex = this.ViewPointToItemIndex(viewPt);
                if ((itemIndex >= 0) && (itemIndex < this.items.Count))
                {
                    Item item = this.items[itemIndex];
                    PointInt32 pt = this.ViewPointToItemPoint(itemIndex, viewPt);
                    ItemPart itemPart = this.ItemPointToItemPart(item, pt);
                    if (itemPart == ItemPart.Image)
                    {
                        this.OnItemClicked(item, itemPart, e.Button);
                        this.mouseDownApplyRendering = false;
                        this.mouseOverIndex = itemIndex;
                        this.mouseOverItemPart = itemPart;
                        this.mouseOverApplyRendering = true;
                    }
                    else
                    {
                        this.mouseDownApplyRendering = true;
                        this.mouseDownIndex = itemIndex;
                        this.mouseDownItemPart = itemPart;
                        this.mouseOverApplyRendering = false;
                    }
                    this.MouseStatesToItemStates();
                }
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            this.GetFocus();
            this.isMouseEntered = true;
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            this.isMouseEntered = false;
            this.mouseDownApplyRendering = false;
            this.mouseOverApplyRendering = false;
            this.mouseDownButton = MouseButtons.None;
            this.MouseStatesToItemStates();
            if ((this.managedFocus && !MenuStripEx.IsAnyMenuActive) && UIUtil.IsOurAppActive)
            {
                this.OnRelinquishFocus();
            }
            base.OnMouseLeave(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (this.isMouseEntered)
            {
                this.GetFocus();
            }
            PointInt32 clientPt = new PointInt32(e.X, e.Y);
            PointInt32 viewPt = this.ClientPointToViewPoint(clientPt);
            bool capture = base.Capture;
            int reorderSourceIndex = this.reorderSourceIndex;
            int num4 = this.reorderSourceIndex;
            bool isReordering = this.isReordering;
            if ((!this.isReordering && this.AllowReorder) && ((this.mouseDownButton == MouseButtons.Left) && (this.mouseOverItemPart == ItemPart.Image)))
            {
                int num5 = this.mouseDownClientPt.X - clientPt.X;
                int num6 = this.mouseDownClientPt.Y - clientPt.Y;
                Size dragSize = SystemInformation.DragSize;
                dragSize.Width *= 2;
                dragSize.Height *= 2;
                if ((Math.Abs(num5) >= dragSize.Width) || (Math.Abs(num6) >= dragSize.Height))
                {
                    int num7 = this.ViewPointToItemIndex(this.mouseDownViewPt);
                    if ((num7 >= 0) && (num7 < this.items.Count))
                    {
                        this.reorderSourceIndex = num7;
                        this.reorderInsertIndex = num7;
                        this.isReordering = true;
                        base.Capture = true;
                    }
                }
            }
            if (this.isReordering)
            {
                this.OnReorderMouseMove(e.Location.ToPointInt32());
            }
            else if (clientPt != this.lastMouseMoveClientPt)
            {
                int itemIndex = this.ViewPointToItemIndex(viewPt);
                if (this.mouseDownButton == MouseButtons.None)
                {
                    if ((itemIndex >= 0) && (itemIndex < this.items.Count))
                    {
                        Item item = this.items[itemIndex];
                        PointInt32 pt = this.ViewPointToItemPoint(itemIndex, viewPt);
                        ItemPart part = this.ItemPointToItemPart(item, pt);
                        this.mouseOverIndex = itemIndex;
                        this.mouseOverItemPart = part;
                        this.mouseOverApplyRendering = true;
                    }
                    else
                    {
                        this.mouseOverApplyRendering = false;
                    }
                }
                else
                {
                    this.mouseOverApplyRendering = false;
                    if (itemIndex != this.mouseDownIndex)
                    {
                        this.mouseDownApplyRendering = false;
                    }
                    else if ((itemIndex < 0) || (itemIndex >= this.items.Count))
                    {
                        this.mouseDownApplyRendering = false;
                    }
                    else
                    {
                        Item item2 = this.Items[itemIndex];
                        PointInt32 num10 = this.ViewPointToItemPoint(itemIndex, viewPt);
                        if (this.ItemPointToItemPart(item2, num10) != this.mouseDownItemPart)
                        {
                            this.mouseDownApplyRendering = false;
                        }
                    }
                }
                this.MouseStatesToItemStates();
            }
            this.lastMouseMoveClientPt = clientPt;
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            bool flag = false;
            if (this.isReordering)
            {
                Item item = this.items[this.reorderSourceIndex];
                PointInt32 clientPt = e.Location.ToPointInt32();
                PointInt32 num2 = this.ClientPointToViewPoint(clientPt);
                int oldIndex = this.ItemToItemIndex(item);
                int newIndex = (int) DoubleUtil.SafeClamp(this.ViewXToSlotIndex((double) num2.X), 0.0, (double) (this.items.Count - 1));
                bool cancel = false;
                if (oldIndex != newIndex)
                {
                    ImageStripItemMovingEventArgs args = new ImageStripItemMovingEventArgs(item, oldIndex, newIndex, cancel);
                    this.OnItemMoving(args);
                    cancel = args.Cancel;
                    if (!cancel)
                    {
                        this.items.RemoveAt(oldIndex);
                        int index = Int32Util.Clamp(newIndex, 0, this.items.Count);
                        this.items.Insert(index, item);
                        ImageStripItemMovedEventArgs args2 = new ImageStripItemMovedEventArgs(item, oldIndex, newIndex);
                        this.OnItemMoved(args2);
                    }
                }
                if (!cancel)
                {
                    for (int i = 0; i < this.items.Count; i++)
                    {
                        Item item2 = this.items[i];
                        if (item2.RenderSlot.FinalValue != i)
                        {
                            item2.RenderSlot.AnimateValueTo((double) i, 0.25, AnimationTransitionType.SmoothStop);
                        }
                    }
                }
                this.reorderInsertIndex = -1;
                this.reorderSourceIndex = -1;
                this.mouseDownButton = MouseButtons.None;
                this.isReordering = false;
                base.Invalidate();
                base.Capture = false;
            }
            else if (this.mouseDownButton == e.Button)
            {
                PointInt32 num8 = new PointInt32(e.X, e.Y);
                PointInt32 viewPt = this.ClientPointToViewPoint(num8);
                int itemIndex = this.ViewPointToItemIndex(viewPt);
                if ((itemIndex >= 0) && (itemIndex < this.items.Count))
                {
                    Item item3 = this.items[itemIndex];
                    PointInt32 pt = this.ViewPointToItemPoint(itemIndex, viewPt);
                    ItemPart itemPart = this.ItemPointToItemPart(item3, pt);
                    if ((itemIndex == this.mouseDownIndex) && (itemPart == this.mouseDownItemPart))
                    {
                        if ((itemPart == ItemPart.CloseButton) && !item3.IsSelected)
                        {
                            itemPart = ItemPart.Image;
                        }
                        this.OnItemClicked(item3, itemPart, this.mouseDownButton);
                        flag = true;
                    }
                    this.mouseOverApplyRendering = true;
                    this.mouseOverItemPart = itemPart;
                    this.mouseOverIndex = itemIndex;
                }
                this.mouseDownApplyRendering = false;
                this.mouseDownButton = MouseButtons.None;
                this.MouseStatesToItemStates();
            }
            if (flag)
            {
                this.ForceMouseMove();
                this.GetFocus();
            }
            base.OnMouseUp(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            float num = ((float) e.Delta) / ((float) SystemInformation.MouseWheelScrollDelta);
            int deltaScrollOffset = -((int) (num * this.ItemSize.Width));
            this.SmoothScrollByOffset(deltaScrollOffset);
            this.ForceMouseMove();
            base.Invalidate();
            base.OnMouseWheel(e);
        }

        private void OnRelinquishFocus()
        {
            this.RelinquishFocus.Raise(this);
        }

        protected override void OnRender(IDrawingContext dc, RectFloat clipRect)
        {
            if (UIUtil.IsControlPaintingEnabled(this))
            {
                SizeInt32 itemSize = this.ItemSize;
                RectInt32 num2 = clipRect.Int32Bound;
                bool flag = this.items.Any<Item>(i => i.IsSelected);
                if (!this.isReordering & flag)
                {
                    RectInt32 highlightRect = this.RenderSlotToClientRect(this.selectionHighlightRenderSlot.Value);
                    this.DrawSelectionHighlight(dc, highlightRect);
                }
                for (int j = 0; j < this.items.Count; j++)
                {
                    Item item = this.items[j];
                    if ((j != this.reorderSourceIndex) && !item.IsSelected)
                    {
                        this.RenderItem(dc, num2, item);
                    }
                }
                if (this.isReordering & flag)
                {
                    RectInt32 num5 = this.RenderSlotToClientRect(this.selectionHighlightRenderSlot.Value);
                    this.DrawSelectionHighlight(dc, num5);
                }
                for (int k = 0; k < this.items.Count; k++)
                {
                    Item item2 = this.items[k];
                    if ((k == this.reorderSourceIndex) || item2.IsSelected)
                    {
                        RectInt32 num7 = this.RenderSlotToClientRect(item2.RenderSlot.Value);
                        this.RenderItem(dc, num2, item2);
                    }
                }
            }
            base.OnRender(dc, clipRect);
        }

        private void OnReorderMouseMove(PointInt32 mouseClientPt)
        {
            Item item = this.items[this.reorderSourceIndex];
            int num = this.ItemToItemIndex(item);
            PointInt32 num2 = this.ClientPointToViewPoint(mouseClientPt);
            PointInt32 mouseDownViewPt = this.mouseDownViewPt;
            int num4 = num2.X - mouseDownViewPt.X;
            double num5 = this.ViewXToSlotIndex((double) num4);
            double num6 = num + num5;
            double num7 = DoubleUtil.Clamp(num6, 0.0, (double) (this.items.Count - 1));
            item.RenderSlot.Value = num7;
            int num8 = (int) this.ViewXToSlotIndex((double) num2.X);
            int num9 = Int32Util.Clamp(num8, 0, this.items.Count - 1);
            if (num9 != this.reorderInsertIndex)
            {
                this.reorderInsertIndex = num9;
            }
            int width = this.ItemSize.Width;
            for (int i = 0; i < this.items.Count; i++)
            {
                if (i != num)
                {
                    int num13;
                    int num14;
                    if (i < this.reorderSourceIndex)
                    {
                        num13 = i;
                    }
                    else
                    {
                        num13 = i - 1;
                    }
                    if (num13 < this.reorderInsertIndex)
                    {
                        num14 = num13;
                    }
                    else
                    {
                        num14 = num13 + 1;
                    }
                    Item item2 = this.items[i];
                    if (item2.RenderSlot.FinalValue != num14)
                    {
                        item2.RenderSlot.AnimateValueTo((double) num14, 0.25, AnimationTransitionType.SmoothStop);
                    }
                }
            }
        }

        private void OnRightScrollButtonClick(object sender, EventArgs e)
        {
            base.Focus();
            this.OnScrollArrowClicked(ArrowDirection.Right);
        }

        protected virtual void OnScrollArrowClicked(ArrowDirection arrowDirection)
        {
            this.ScrollArrowClicked.Raise<ArrowDirection>(this, arrowDirection);
        }

        protected virtual void OnScrollOffsetChanged()
        {
            base.PerformLayout();
            base.Invalidate();
            this.ScrollOffsetChanged.Raise(this);
        }

        private void OnScrollOffsetValueChanged(object sender, EventArgs e)
        {
            if (this.isMouseEntered)
            {
                this.ForceMouseMove();
            }
            this.OnScrollOffsetChanged();
            this.QueueUpdate();
        }

        private void OnSelectionHighlightRenderSlotValueChanged(object sender, EventArgs e)
        {
            if (this.selectionHighlightRenderSlot.Value >= (this.items.Count - 1))
            {
                if (!this.isReordering)
                {
                    this.ForceMouseMove();
                }
                this.OnLayoutRequested();
            }
            base.Invalidate();
        }

        public void PerformItemClick(Item item, ItemPart itemPart, MouseButtons mouseButtons)
        {
            this.OnItemClicked(item, itemPart, mouseButtons);
        }

        public void PerformItemClick(int itemIndex, ItemPart itemPart, MouseButtons mouseButtons)
        {
            this.PerformItemClick(this.items[itemIndex], itemPart, mouseButtons);
        }

        public void RemoveItem(Item item)
        {
            if (!this.items.Contains(item))
            {
                throw new ArgumentException("item was never added to this control");
            }
            this.items.Remove(item);
            item.Changed -= new EventHandler(this.OnItemChanged);
            item.UncacheDeviceImage();
            for (int i = 0; i < this.items.Count; i++)
            {
                Item item2 = this.items[i];
                if (item2.RenderSlot.FinalValue != i)
                {
                    item2.RenderSlot.AnimateValueTo((double) i, 0.25, AnimationTransitionType.SmoothStop);
                }
            }
            base.PerformLayout();
            base.Invalidate();
        }

        public void RemoveItemAt(int index)
        {
            Item item = this.items[index];
            this.RemoveItem(item);
        }

        private void RenderItem(IDrawingContext dc, RectInt32 clipRect, Item item)
        {
            RectInt32 num = this.RenderSlotToClientRect(item.RenderSlot.Value);
            if (num.IntersectsWith(clipRect))
            {
                this.DrawItem(dc, item, num.Location);
            }
        }

        private RectInt32 RenderSlotToClientRect(double slotIndex)
        {
            int itemIndex = (int) Math.Floor(slotIndex);
            int num2 = (int) Math.Ceiling(slotIndex);
            double num3 = slotIndex - itemIndex;
            double num4 = 1.0 - num3;
            RectInt32 num5 = this.ItemIndexToClientRect(itemIndex);
            RectInt32 num6 = this.ItemIndexToClientRect(num2);
            return new RectInt32((int) ((num4 * num5.X) + (num3 * num6.X)), (int) ((num4 * num5.Y) + (num3 * num6.Y)), (int) ((num4 * num5.Width) + (num3 * num6.Width)), (int) ((num4 * num5.Height) + (num3 * num6.Height)));
        }

        private double SlotIndexToViewX(double slotIndex) => 
            (slotIndex * this.ItemSize.Width);

        public void SmoothScrollByOffset(int deltaScrollOffset)
        {
            int num3;
            int num = this.scrollOffset.Value;
            int finalValue = this.scrollOffset.FinalValue;
            if (((deltaScrollOffset < 0) && (num < finalValue)) || ((deltaScrollOffset > 0) && (num > finalValue)))
            {
                num3 = num;
            }
            else
            {
                num3 = finalValue;
            }
            int newScrollOffset = num3 + deltaScrollOffset;
            this.SmoothScrollToOffset(newScrollOffset);
        }

        public void SmoothScrollToOffset(int newScrollOffset)
        {
            this.VerifyThreadAccess();
            this.AnimateToScrollOffset(newScrollOffset);
        }

        private void StartBusyFrameAnimation()
        {
            if (!this.busyAnimationFrame.IsAnimating)
            {
                int frameCount = this.BusyAnimationFrames.Length;
                double num = ((double) this.BusyAnimationFrames.Length) / 20.0;
                this.busyAnimationFrame.AnimateRawValue((s, v) => AnimationStoryboard.InitializeLinearRepeating(s, v, 0.0, (double) frameCount, 1.0, -1), null);
            }
        }

        private void StopBusyFrameAnimation()
        {
            this.busyAnimationFrame.StopAnimation();
        }

        public PointInt32 ViewPointToClientPoint(PointInt32 viewPt) => 
            new PointInt32(viewPt.X - this.ScrollOffset, viewPt.Y);

        public int ViewPointToItemIndex(PointInt32 viewPt)
        {
            if (!this.ViewRectangle.Contains(viewPt))
            {
                return -1;
            }
            SizeInt32 itemSize = this.ItemSize;
            return (viewPt.X / itemSize.Width);
        }

        private PointInt32 ViewPointToItemPoint(int itemIndex, PointInt32 viewPt)
        {
            RectInt32 num = this.ItemIndexToViewRect(itemIndex);
            return new PointInt32(viewPt.X - num.X, viewPt.Y);
        }

        public RectInt32 ViewRectToClientRect(RectInt32 viewRect) => 
            new RectInt32(this.ViewPointToClientPoint(viewRect.Location), viewRect.Size);

        private double ViewXToSlotIndex(double viewX) => 
            (viewX / ((double) this.ItemSize.Width));

        public bool AllowReorder { get; set; }

        private DeviceBitmap[] BusyAnimationFrames
        {
            get
            {
                this.EnsureBusyAnimationFramesIsInitialized();
                return this.busyAnimationFramesDevice;
            }
        }

        public bool DrawDirtyOverlay
        {
            get => 
                this.drawDirtyOverlay;
            set
            {
                if (this.drawDirtyOverlay != value)
                {
                    this.drawDirtyOverlay = value;
                    base.Invalidate(true);
                }
            }
        }

        public bool DrawShadow
        {
            get => 
                this.drawShadow;
            set
            {
                if (this.drawShadow != value)
                {
                    this.drawShadow = value;
                    base.Invalidate(true);
                }
            }
        }

        public int ItemCount =>
            this.items.Count;

        public Item[] Items =>
            this.items.ToArrayEx<Item>();

        public SizeInt32 ItemSize
        {
            get
            {
                RectInt32 num;
                RectInt32 num2;
                this.MeasureItemPartRectangles(out num, out num2);
                return num.Size;
            }
        }

        protected PaintDotNet.Controls.ArrowButton LeftScrollButton =>
            this.leftScrollButton;

        public bool ManagedFocus
        {
            get => 
                this.managedFocus;
            set
            {
                this.managedFocus = value;
            }
        }

        public int MaxScrollOffset
        {
            get
            {
                SizeInt32 itemSize = this.ItemSize;
                int num3 = this.ViewRectangle.Width - base.ClientSize.Width;
                return Math.Max(0, num3);
            }
        }

        public int MinScrollOffset =>
            0;

        public SizeInt32 PreferredImageSize
        {
            get
            {
                RectInt32 num;
                RectInt32 num2;
                this.MeasureItemPartRectangles(out num, out num2);
                return new SizeInt32(num2.Width - 8, num2.Height - 8);
            }
        }

        public int PreferredMinClientWidth
        {
            get
            {
                if (this.items.Count == 0)
                {
                    return 0;
                }
                int width = this.ItemSize.Width;
                if (this.leftScrollButton.Visible || this.rightScrollButton.Visible)
                {
                    width += this.leftScrollButton.Width;
                    width += this.rightScrollButton.Width;
                }
                return Math.Min(width, this.ViewRectangle.Width);
            }
        }

        protected PaintDotNet.Controls.ArrowButton RightScrollButton =>
            this.rightScrollButton;

        public RectInt32 ScrolledViewRect =>
            new RectInt32(this.ScrollOffset, 0, base.ClientSize.Width, base.ClientSize.Height);

        public int ScrollOffset =>
            this.ClampScrollOffset(this.scrollOffset.Value);

        public bool ShowCloseButtons
        {
            get => 
                this.showCloseButtons;
            set
            {
                if (this.showCloseButtons != value)
                {
                    this.showCloseButtons = value;
                    base.PerformLayout();
                    base.Invalidate();
                }
            }
        }

        public bool ShowScrollButtons
        {
            get => 
                this.showScrollButtons;
            set
            {
                if (this.showScrollButtons != value)
                {
                    this.showScrollButtons = value;
                    base.PerformLayout();
                    base.Invalidate(true);
                }
            }
        }

        public RectInt32 ViewRectangle
        {
            get
            {
                double num2;
                this.VerifyThreadAccess();
                SizeInt32 itemSize = this.ItemSize;
                if (this.items.Count == 0)
                {
                    num2 = 0.0;
                }
                else
                {
                    num2 = this.items.Max<Item>((Func<Item, double>) (item => item.RenderSlot.Value));
                }
                double num4 = Math.Max(Math.Max(num2, this.selectionHighlightRenderSlot.Value), (double) (this.items.Count - 1));
                return new RectInt32(0, 0, (int) this.SlotIndexToViewX(num4 + 1.0), itemSize.Height);
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly ImageStrip.<>c <>9 = new ImageStrip.<>c();
            public static Func<ImageStrip.Item, double> <>9__141_0;
            public static Func<ImageStrip.Item, bool> <>9__181_0;
            public static Func<ImageStrip.Item, bool> <>9__80_0;

            internal double <get_ViewRectangle>b__141_0(ImageStrip.Item item) => 
                item.RenderSlot.Value;

            internal bool <OnBusyAnimationFrameValueChanged>b__80_0(ImageStrip.Item doc) => 
                (doc.Image == null);

            internal bool <OnRender>b__181_0(ImageStrip.Item i) => 
                i.IsSelected;
        }

        public sealed class Item
        {
            private AnimatedDouble closeButtonOpacity;
            private PushButtonState closeButtonState = PushButtonState.Normal;
            private DeviceBitmap deviceImage;
            private AnimatedDouble dirtyOverlayOpacity;
            private AnimatedDouble hoverHighlightOpacity;
            private System.Drawing.Image image;
            private AnimatedDouble imageOpacity;
            private bool isDirty;
            private int isDirtyValueLockCount;
            private bool isMouseOver;
            private bool isSelected;
            private bool lockedIsDirtyValue;
            private AnimatedDouble renderSlot;
            private object tag;

            [field: CompilerGenerated]
            public event EventHandler Changed;

            public Item(System.Drawing.Image image = null)
            {
                this.image = image;
                ValueChangedEventHandler<double> handler = (s, e) => this.OnChanged();
                this.imageOpacity = new AnimatedDouble(1.0);
                this.imageOpacity.ValueChanged += handler;
                this.hoverHighlightOpacity = new AnimatedDouble(0.0);
                this.hoverHighlightOpacity.ValueChanged += handler;
                this.dirtyOverlayOpacity = new AnimatedDouble(0.0);
                this.dirtyOverlayOpacity.ValueChanged += handler;
                this.closeButtonOpacity = new AnimatedDouble(0.0);
                this.closeButtonOpacity.ValueChanged += handler;
                this.renderSlot = new AnimatedDouble(0.0);
                this.renderSlot.ValueChanged += handler;
            }

            private void AnimateDirtyOverlayOpacity()
            {
                double finalValue = this.IsDirty ? 1.0 : 0.0;
                double duration = this.IsDirty ? 1.0 : 1.0;
                if (finalValue != this.dirtyOverlayOpacity.FinalValue)
                {
                    this.dirtyOverlayOpacity.AnimateValueTo(finalValue, duration, AnimationTransitionType.SmoothStop);
                }
            }

            public void LockDirtyValue(bool forceValue)
            {
                this.isDirtyValueLockCount++;
                if (this.isDirtyValueLockCount == 1)
                {
                    this.lockedIsDirtyValue = forceValue;
                    this.AnimateDirtyOverlayOpacity();
                }
            }

            private void OnChanged()
            {
                this.Changed.Raise(this);
            }

            public void UncacheDeviceImage()
            {
                if (this.deviceImage != null)
                {
                    this.deviceImage.BitmapSource = null;
                    this.deviceImage = null;
                }
            }

            public void UnlockDirtyValue()
            {
                this.isDirtyValueLockCount--;
                if (this.isDirtyValueLockCount == 0)
                {
                    this.OnChanged();
                    this.AnimateDirtyOverlayOpacity();
                }
                else if (this.isDirtyValueLockCount < 0)
                {
                    ExceptionUtil.ThrowInvalidOperationException("Calls to UnlockDirtyValue() must be matched by a preceding call to LockDirtyValue()");
                }
            }

            public void Update()
            {
                this.OnChanged();
            }

            public AnimatedDouble CloseButtonOpacity =>
                this.closeButtonOpacity;

            public PushButtonState CloseButtonState
            {
                get => 
                    this.closeButtonState;
                set
                {
                    if (this.closeButtonState != value)
                    {
                        this.closeButtonState = value;
                        this.OnChanged();
                    }
                }
            }

            public DeviceBitmap DeviceImage
            {
                get
                {
                    if (this.image == null)
                    {
                        return null;
                    }
                    if (this.deviceImage == null)
                    {
                        this.deviceImage = this.image.CreateDeviceBitmap();
                    }
                    return this.deviceImage;
                }
            }

            public AnimatedDouble DirtyOverlayOpacity =>
                this.dirtyOverlayOpacity;

            public AnimatedDouble HoverHighlightOpacity =>
                this.hoverHighlightOpacity;

            public System.Drawing.Image Image
            {
                get => 
                    this.image;
                set
                {
                    this.image = value;
                    if (this.deviceImage != null)
                    {
                        this.deviceImage.BitmapSource = null;
                        this.deviceImage = null;
                    }
                    this.OnChanged();
                }
            }

            public AnimatedDouble ImageOpacity =>
                this.imageOpacity;

            public bool IsDirty
            {
                get
                {
                    if (this.isDirtyValueLockCount > 0)
                    {
                        return this.lockedIsDirtyValue;
                    }
                    return this.isDirty;
                }
                set
                {
                    if (this.isDirty != value)
                    {
                        this.isDirty = value;
                        if (this.isDirtyValueLockCount <= 0)
                        {
                            this.OnChanged();
                        }
                        this.AnimateDirtyOverlayOpacity();
                    }
                }
            }

            public bool IsMouseOver
            {
                get => 
                    this.isMouseOver;
                set
                {
                    if (this.isMouseOver != value)
                    {
                        this.isMouseOver = value;
                        this.OnChanged();
                    }
                }
            }

            public bool IsSelected
            {
                get => 
                    this.isSelected;
                set
                {
                    if (value != this.isSelected)
                    {
                        this.isSelected = value;
                        this.OnChanged();
                    }
                }
            }

            public AnimatedDouble RenderSlot =>
                this.renderSlot;

            public object Tag
            {
                get => 
                    this.tag;
                set
                {
                    this.tag = value;
                    this.OnChanged();
                }
            }
        }

        public enum ItemPart
        {
            None,
            Image,
            CloseButton
        }
    }
}

