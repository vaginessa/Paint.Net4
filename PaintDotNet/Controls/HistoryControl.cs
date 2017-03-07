namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Direct2D;
    using PaintDotNet.DirectWrite;
    using PaintDotNet.Drawing;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Imaging;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.Runtime;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.UI.Media;
    using PaintDotNet.VisualStyling;
    using System;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class HistoryControl : Direct2DControl
    {
        private SolidColorBrush backBrush;
        private PaintDotNet.HistoryStack historyStack;
        private int ignoreScrollOffsetSet;
        private int imageHeight;
        private int itemHeight;
        private PointInt32 lastMouseClientPt;
        private bool managedFocus;
        private int redoItemHighlight;
        private int scrollOffset;
        private SelectionHighlightRenderer selectionHighlightRenderer;
        private static readonly SolidColorBrush slateGrayBrush = new SolidColorBrush(Color.SlateGray).EnsureFrozen<SolidColorBrush>();
        private int undoItemHighlight;
        private VScrollBar vScrollBar;

        [field: CompilerGenerated]
        public event EventHandler HistoryChanged;

        [field: CompilerGenerated]
        public event EventHandler RelinquishFocus;

        [field: CompilerGenerated]
        public event EventHandler ScrollOffsetChanged;

        public HistoryControl() : base(FactorySource.PerThread)
        {
            this.undoItemHighlight = -1;
            this.redoItemHighlight = -1;
            this.lastMouseClientPt = new PointInt32(-1, -1);
            this.backBrush = new SolidColorBrush();
            this.selectionHighlightRenderer = new SelectionHighlightRenderer();
            this.itemHeight = UIUtil.ScaleHeight(20);
            this.imageHeight = UIUtil.ScaleHeight(0x10);
            base.SetStyle(ControlStyles.StandardDoubleClick, false);
            this.InitializeComponent();
        }

        private PointInt32 ClientPointToViewPoint(PointInt32 pt) => 
            new PointInt32(pt.X, pt.Y + this.ScrollOffset);

        public RectInt32 ClientRectangleToViewRectangle(RectInt32 clientRect) => 
            new RectInt32(this.ClientPointToViewPoint(clientRect.Location), clientRect.Size);

        private void EnsureItemIsFullyVisible(ItemType itemType, int itemIndex)
        {
            PointInt32 location = this.StackIndexToViewPoint(itemType, itemIndex);
            RectInt32 num2 = new RectInt32(location, new SizeInt32(this.ViewWidth, this.itemHeight));
            int num3 = num2.Bottom - base.ClientSize.Height;
            int top = num2.Top;
            this.ScrollOffset = Int32Util.ClampSafe(this.ScrollOffset, num3, top);
        }

        private void EnsureLastUndoItemIsFullyVisible()
        {
            int itemIndex = this.historyStack.UndoStack.Count - 1;
            this.EnsureItemIsFullyVisible(ItemType.Undo, itemIndex);
        }

        private void InitializeComponent()
        {
            this.vScrollBar = new VScrollBar();
            base.SuspendLayout();
            this.vScrollBar.Name = "vScrollBar";
            this.vScrollBar.ValueChanged += new EventHandler(this.OnVScrollBarValueChanged);
            base.Name = "HistoryControl";
            base.TabStop = false;
            base.Controls.Add(this.vScrollBar);
            base.ResizeRedraw = true;
            base.ResumeLayout();
            base.PerformLayout();
        }

        private void KeyUpHandler(object sender, KeyEventArgs e)
        {
            this.OnKeyUp(e);
        }

        protected override void OnClick(EventArgs e)
        {
            if (this.historyStack != null)
            {
                ItemType type;
                int num2;
                PointInt32 viewPt = this.ClientPointToViewPoint(this.lastMouseClientPt);
                this.ViewPointToStackIndex(viewPt, out type, out num2);
                this.OnItemClicked(type, num2);
            }
            base.OnClick(e);
        }

        private void OnHistoryChanged()
        {
            this.vScrollBar.Maximum = this.ViewHeight;
            this.HistoryChanged.Raise(this);
        }

        private void OnHistoryChanged(object sender, EventArgs e)
        {
            if (!base.IsDisposed)
            {
                this.PerformMouseMove();
                base.PerformLayout();
                this.Refresh();
                this.OnHistoryChanged();
            }
        }

        private void OnHistoryHistoryFlushed(object sender, EventArgs e)
        {
            if (!base.IsDisposed)
            {
                this.EnsureLastUndoItemIsFullyVisible();
                this.PerformMouseMove();
                base.PerformLayout();
                this.Refresh();
            }
        }

        private void OnHistoryNewHistoryMemento(object sender, EventArgs e)
        {
            if (!base.IsDisposed)
            {
                this.EnsureLastUndoItemIsFullyVisible();
                this.PerformMouseMove();
                base.PerformLayout();
                base.Invalidate();
            }
        }

        private void OnHistorySteppedBackward(object sender, EventArgs e)
        {
            if (!base.IsDisposed)
            {
                this.undoItemHighlight = -1;
                this.redoItemHighlight = -1;
                this.EnsureLastUndoItemIsFullyVisible();
                this.PerformMouseMove();
                base.PerformLayout();
                this.Refresh();
            }
        }

        private void OnHistorySteppedForward(object sender, EventArgs e)
        {
            if (!base.IsDisposed)
            {
                this.undoItemHighlight = -1;
                this.redoItemHighlight = -1;
                this.EnsureLastUndoItemIsFullyVisible();
                this.PerformMouseMove();
                base.PerformLayout();
                this.Refresh();
            }
        }

        private void OnItemClicked(ItemType itemType, HistoryMemento hm)
        {
            long iD = hm.ID;
            if (itemType == ItemType.Undo)
            {
                if (iD == this.historyStack.UndoStack[this.historyStack.UndoStack.Count - 1].ID)
                {
                    if (this.historyStack.UndoStack.Count > 1)
                    {
                        this.historyStack.StepBackward(this);
                    }
                }
                else
                {
                    this.SuspendScrollOffsetSet();
                    this.historyStack.BeginStepGroup();
                    using (new WaitCursorChanger(this))
                    {
                        while (this.historyStack.UndoStack[this.historyStack.UndoStack.Count - 1].ID != iD)
                        {
                            this.historyStack.StepBackward(this);
                        }
                    }
                    this.historyStack.EndStepGroup();
                    this.ResumeScrollOffsetSet();
                }
            }
            else
            {
                this.SuspendScrollOffsetSet();
                this.historyStack.BeginStepGroup();
                using (new WaitCursorChanger(this))
                {
                    while (this.historyStack.UndoStack[this.historyStack.UndoStack.Count - 1].ID != iD)
                    {
                        this.historyStack.StepForward(this);
                    }
                }
                this.historyStack.EndStepGroup();
                this.ResumeScrollOffsetSet();
            }
            CleanupManager.RequestCleanup();
            base.Focus();
        }

        private void OnItemClicked(ItemType itemType, int itemIndex)
        {
            HistoryMemento memento;
            if (itemType == ItemType.Undo)
            {
                if ((itemIndex >= 0) && (itemIndex < this.historyStack.UndoStack.Count))
                {
                    memento = this.historyStack.UndoStack[itemIndex];
                }
                else
                {
                    memento = null;
                }
            }
            else if ((itemIndex >= 0) && (itemIndex < this.historyStack.RedoStack.Count))
            {
                memento = this.historyStack.RedoStack[itemIndex];
            }
            else
            {
                memento = null;
            }
            if (memento != null)
            {
                this.EnsureItemIsFullyVisible(itemType, itemIndex);
                this.OnItemClicked(itemType, memento);
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int num;
            if (this.historyStack == null)
            {
                num = 0;
            }
            else
            {
                num = this.historyStack.UndoStack.Count + this.historyStack.RedoStack.Count;
            }
            int num2 = num * this.itemHeight;
            if (num2 > base.ClientSize.Height)
            {
                this.vScrollBar.Visible = true;
                this.vScrollBar.Location = new PointInt32(base.ClientSize.Width - this.vScrollBar.Width, 0).ToGdipPoint();
                this.vScrollBar.Height = base.ClientSize.Height;
                this.vScrollBar.Minimum = 0;
                this.vScrollBar.Maximum = num2;
                this.vScrollBar.LargeChange = base.ClientSize.Height;
                this.vScrollBar.SmallChange = this.itemHeight;
            }
            else
            {
                this.vScrollBar.Visible = false;
            }
            if (this.historyStack != null)
            {
                this.ScrollOffset = Int32Util.Clamp(this.ScrollOffset, this.MinScrollOffset, this.MaxScrollOffset);
            }
            base.OnLayout(levent);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            if (((this.historyStack != null) && this.managedFocus) && (!MenuStripEx.IsAnyMenuActive && UIUtil.IsOurAppActive))
            {
                base.Focus();
            }
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (this.historyStack != null)
            {
                this.undoItemHighlight = -1;
                this.redoItemHighlight = -1;
                this.Refresh();
                if (this.Focused && this.managedFocus)
                {
                    this.OnRelinquishFocus();
                }
            }
            base.OnMouseLeave(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            ItemType type;
            int num3;
            if (this.historyStack == null)
            {
                goto Label_00B3;
            }
            PointInt32 pt = new PointInt32(e.X, e.Y);
            PointInt32 viewPt = this.ClientPointToViewPoint(pt);
            this.ViewPointToStackIndex(viewPt, out type, out num3);
            if (type != ItemType.Undo)
            {
                if (type != ItemType.Redo)
                {
                    throw ExceptionUtil.InvalidEnumArgumentException<ItemType>(type, "itemType");
                }
            }
            else
            {
                if ((num3 >= 0) && (num3 < this.historyStack.UndoStack.Count))
                {
                    this.undoItemHighlight = num3;
                }
                else
                {
                    this.undoItemHighlight = -1;
                }
                this.redoItemHighlight = -1;
                goto Label_00A6;
            }
            this.undoItemHighlight = -1;
            if ((num3 >= 0) && (num3 < this.historyStack.RedoStack.Count))
            {
                this.redoItemHighlight = num3;
            }
            else
            {
                this.redoItemHighlight = -1;
            }
        Label_00A6:
            this.Refresh();
            this.lastMouseClientPt = pt;
        Label_00B3:
            base.OnMouseMove(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (this.historyStack != null)
            {
                int num = (e.Delta * SystemInformation.MouseWheelScrollLines) / SystemInformation.MouseWheelScrollDelta;
                int num2 = num * this.itemHeight;
                this.ScrollOffset -= num2;
                this.PerformMouseMove();
            }
            base.OnMouseWheel(e);
        }

        private void OnRelinquishFocus()
        {
            this.RelinquishFocus.Raise(this);
        }

        protected override void OnRender(IDrawingContext dc, RectFloat clipRect)
        {
            if (this.historyStack != null)
            {
                dc.Clear(new ColorRgba128Float?(this.BackColor));
                using (dc.UseTranslateTransform(0f, (float) -this.scrollOffset, MatrixMultiplyOrder.Prepend))
                {
                    int num7;
                    int num8;
                    int num11;
                    int num12;
                    RectDouble? nullable;
                    TextLayoutAlgorithm? nullable2;
                    int num = UIUtil.ScaleWidth(1);
                    int num2 = (this.itemHeight - this.imageHeight) / 2;
                    int num3 = UIUtil.ScaleWidth(2);
                    RectInt32 a = this.ClientRectangleToViewRectangle(base.ClientRectangle.ToRectInt32());
                    RectInt32 undoViewRectangle = this.UndoViewRectangle;
                    dc.FillRectangle(undoViewRectangle, PaintDotNet.UI.Media.SystemBrushes.Window);
                    RectInt32 num6 = RectInt32.Intersect(a, undoViewRectangle);
                    if ((num6.Width > 0) && (num6.Height > 0))
                    {
                        ItemType type;
                        this.ViewPointToStackIndex(num6.Location, out type, out num7);
                        this.ViewPointToStackIndex(new PointInt32(num6.Left, num6.Bottom - 1), out type, out num8);
                    }
                    else
                    {
                        num7 = 0;
                        num8 = -1;
                    }
                    for (int i = num7; i <= num8; i++)
                    {
                        DeviceBitmap deviceBitmap;
                        int imageHeight;
                        HighlightState hover;
                        ImageResource image = this.historyStack.UndoStack[i].Image;
                        if (image != null)
                        {
                            deviceBitmap = ImageResourceUtil.GetDeviceBitmap(image);
                        }
                        else
                        {
                            deviceBitmap = null;
                        }
                        if (deviceBitmap != null)
                        {
                            imageHeight = (deviceBitmap.PixelSize.Width * this.imageHeight) / deviceBitmap.PixelSize.Height;
                        }
                        else
                        {
                            imageHeight = this.imageHeight;
                        }
                        if (i == (this.historyStack.UndoStack.Count - 1))
                        {
                            hover = HighlightState.Checked;
                        }
                        else if (i == this.undoItemHighlight)
                        {
                            hover = HighlightState.Hover;
                        }
                        else
                        {
                            hover = HighlightState.Default;
                        }
                        RectInt32 bounds = new RectInt32(0, i * this.itemHeight, this.ViewWidth, this.itemHeight);
                        this.selectionHighlightRenderer.HighlightState = hover;
                        this.selectionHighlightRenderer.RenderBackground(dc, bounds);
                        PaintDotNet.UI.Media.Brush embeddedTextBrush = this.selectionHighlightRenderer.EmbeddedTextBrush;
                        if (deviceBitmap != null)
                        {
                            nullable = null;
                            dc.DrawBitmap(deviceBitmap, new RectDouble?(new RectInt32(bounds.X + num, bounds.Y + num2, imageHeight, this.imageHeight)), 1.0, BitmapInterpolationMode.Linear, nullable);
                        }
                        int x = (num + num3) + imageHeight;
                        RectInt32 num17 = new RectInt32(x, bounds.Y, this.ViewWidth - x, this.itemHeight);
                        nullable2 = null;
                        TextLayout textLayout = UIText.CreateLayout(dc, this.historyStack.UndoStack[i].Name, this.Font, nullable2, HotkeyRenderMode.Hide, (double) num17.Width, (double) num17.Height);
                        textLayout.WordWrapping = WordWrapping.Wrap;
                        textLayout.ParagraphAlignment = ParagraphAlignment.Center;
                        UIText.AdjustFontSizeToFitLayoutSize(dc, textLayout, (double) num17.Width, (double) num17.Height, 0.6);
                        dc.DrawTextLayout(num17.Location, textLayout, embeddedTextBrush, DrawTextOptions.None);
                    }
                    RectInt32 redoViewRectangle = this.RedoViewRectangle;
                    dc.FillRectangle(redoViewRectangle, slateGrayBrush);
                    RectInt32 num10 = RectInt32.Intersect(a, redoViewRectangle);
                    if ((num10.Width > 0) && (num10.Height > 0))
                    {
                        ItemType type2;
                        this.ViewPointToStackIndex(num10.Location, out type2, out num11);
                        this.ViewPointToStackIndex(new PointInt32(num10.Left, num10.Bottom - 1), out type2, out num12);
                    }
                    else
                    {
                        num11 = 0;
                        num12 = -1;
                    }
                    for (int j = num11; j <= num12; j++)
                    {
                        DeviceBitmap bitmap2;
                        int num20;
                        PaintDotNet.UI.Media.Brush inactiveCaptionText;
                        ImageResource imageResource = this.historyStack.RedoStack[j].Image;
                        if (imageResource != null)
                        {
                            bitmap2 = ImageResourceUtil.GetDeviceBitmap(imageResource);
                        }
                        else
                        {
                            bitmap2 = null;
                        }
                        if (bitmap2 != null)
                        {
                            num20 = (bitmap2.PixelSize.Width * this.imageHeight) / bitmap2.PixelSize.Height;
                        }
                        else
                        {
                            num20 = this.imageHeight;
                        }
                        RectInt32 num21 = new RectInt32(0, redoViewRectangle.Top + (j * this.itemHeight), this.ViewWidth, this.itemHeight);
                        if (j == this.redoItemHighlight)
                        {
                            this.selectionHighlightRenderer.HighlightState = HighlightState.Hover;
                            this.selectionHighlightRenderer.RenderBackground(dc, num21);
                            inactiveCaptionText = this.selectionHighlightRenderer.EmbeddedTextBrush;
                        }
                        else
                        {
                            inactiveCaptionText = PaintDotNet.UI.Media.SystemBrushes.InactiveCaptionText;
                        }
                        if (bitmap2 != null)
                        {
                            nullable = null;
                            dc.DrawBitmap(bitmap2, new RectDouble?(new RectInt32(num21.X + num, num21.Y + num2, num20, this.imageHeight)), 1.0, BitmapInterpolationMode.Linear, nullable);
                        }
                        int num22 = (num + num3) + num20;
                        RectInt32 num23 = new RectInt32(num22, num21.Y, this.ViewWidth - num22, this.itemHeight);
                        nullable2 = null;
                        TextLayout layout2 = UIText.CreateLayout(dc, this.historyStack.RedoStack[j].Name, this.Font, nullable2, HotkeyRenderMode.Hide, (double) num23.Width, (double) num23.Height);
                        layout2.WordWrapping = WordWrapping.NoWrap;
                        layout2.ParagraphAlignment = ParagraphAlignment.Center;
                        layout2.FontStyle = PaintDotNet.DirectWrite.FontStyle.Italic;
                        UIText.AdjustFontSizeToFitLayoutSize(dc, layout2, (double) num23.Width, (double) num23.Height, 0.6);
                        dc.DrawTextLayout(num23.Location, layout2, inactiveCaptionText, DrawTextOptions.None);
                    }
                }
            }
            base.OnRender(dc, clipRect);
        }

        protected override void OnResize(EventArgs e)
        {
            base.PerformLayout();
            base.OnResize(e);
        }

        private void OnScrollOffsetChanged()
        {
            this.vScrollBar.Value = Int32Util.Clamp(this.scrollOffset, this.vScrollBar.Minimum, this.vScrollBar.Maximum);
            this.ScrollOffsetChanged.Raise(this);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.PerformLayout();
            base.OnSizeChanged(e);
        }

        private void OnVScrollBarValueChanged(object sender, EventArgs e)
        {
            this.ScrollOffset = this.vScrollBar.Value;
        }

        private void PerformMouseMove()
        {
            if (base.IsHandleCreated)
            {
                Point pt = base.PointToClient(Control.MousePosition);
                if (base.ClientRectangle.Contains(pt))
                {
                    MouseEventArgs e = new MouseEventArgs(MouseButtons.None, 0, pt.X, pt.Y, 0);
                    this.OnMouseMove(e);
                }
            }
        }

        private void ResumeScrollOffsetSet()
        {
            this.ignoreScrollOffsetSet--;
        }

        private PointInt32 StackIndexToViewPoint(ItemType itemType, int itemIndex)
        {
            RectInt32 undoViewRectangle;
            if (itemType == ItemType.Undo)
            {
                undoViewRectangle = this.UndoViewRectangle;
            }
            else
            {
                undoViewRectangle = this.RedoViewRectangle;
            }
            return new PointInt32(0, (itemIndex * this.itemHeight) + undoViewRectangle.Top);
        }

        private void SuspendScrollOffsetSet()
        {
            this.ignoreScrollOffsetSet++;
        }

        private void ViewPointToStackIndex(PointInt32 viewPt, out ItemType itemType, out int itemIndex)
        {
            RectInt32 undoViewRectangle = this.UndoViewRectangle;
            if ((viewPt.Y >= undoViewRectangle.Top) && (viewPt.Y < undoViewRectangle.Bottom))
            {
                itemType = ItemType.Undo;
                itemIndex = (viewPt.Y - undoViewRectangle.Top) / this.itemHeight;
            }
            else
            {
                RectInt32 redoViewRectangle = this.RedoViewRectangle;
                itemType = ItemType.Redo;
                itemIndex = (viewPt.Y - redoViewRectangle.Top) / this.itemHeight;
            }
        }

        public PaintDotNet.HistoryStack HistoryStack
        {
            get => 
                this.historyStack;
            set
            {
                if (this.historyStack != null)
                {
                    this.historyStack.Changed -= new EventHandler(this.OnHistoryChanged);
                    this.historyStack.SteppedForward -= new EventHandler(this.OnHistorySteppedForward);
                    this.historyStack.SteppedBackward -= new EventHandler(this.OnHistorySteppedBackward);
                    this.historyStack.HistoryFlushed -= new EventHandler(this.OnHistoryHistoryFlushed);
                    this.historyStack.NewHistoryMemento -= new EventHandler(this.OnHistoryNewHistoryMemento);
                }
                this.historyStack = value;
                base.PerformLayout();
                if (this.historyStack != null)
                {
                    this.historyStack.Changed += new EventHandler(this.OnHistoryChanged);
                    this.historyStack.SteppedForward += new EventHandler(this.OnHistorySteppedForward);
                    this.historyStack.SteppedBackward += new EventHandler(this.OnHistorySteppedBackward);
                    this.historyStack.HistoryFlushed += new EventHandler(this.OnHistoryHistoryFlushed);
                    this.historyStack.NewHistoryMemento += new EventHandler(this.OnHistoryNewHistoryMemento);
                    this.EnsureLastUndoItemIsFullyVisible();
                }
                this.Refresh();
                this.OnHistoryChanged();
            }
        }

        private int ItemCount =>
            (this.historyStack?.UndoStack.Count + this.historyStack.RedoStack.Count);

        public bool ManagedFocus
        {
            get => 
                this.managedFocus;
            set
            {
                this.managedFocus = value;
            }
        }

        public int MaxScrollOffset =>
            Math.Max(0, this.ViewHeight - base.ClientSize.Height);

        public int MinScrollOffset =>
            0;

        private RectInt32 RedoViewRectangle =>
            new RectInt32(0, this.itemHeight * this.historyStack.UndoStack.Count, this.ViewWidth, this.itemHeight * this.historyStack.RedoStack.Count);

        public int ScrollOffset
        {
            get => 
                this.scrollOffset;
            set
            {
                if (this.ignoreScrollOffsetSet <= 0)
                {
                    int num = Int32Util.Clamp(value, this.MinScrollOffset, this.MaxScrollOffset);
                    if (this.scrollOffset != num)
                    {
                        this.scrollOffset = num;
                        this.OnScrollOffsetChanged();
                        base.Invalidate(false);
                    }
                }
            }
        }

        private RectInt32 UndoViewRectangle =>
            new RectInt32(0, 0, this.ViewWidth, this.itemHeight * this.historyStack.UndoStack.Count);

        private int ViewHeight =>
            (this.ItemCount * this.itemHeight);

        public RectInt32 ViewRectangle =>
            new RectInt32(0, 0, this.ViewWidth, this.ViewHeight);

        public int ViewWidth
        {
            get
            {
                if (this.vScrollBar.Visible)
                {
                    return (base.ClientSize.Width - this.vScrollBar.Width);
                }
                return base.ClientSize.Width;
            }
        }

        private enum ItemType
        {
            Undo,
            Redo
        }
    }
}

