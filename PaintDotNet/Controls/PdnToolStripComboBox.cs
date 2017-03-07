namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Direct2D;
    using PaintDotNet.DirectWrite;
    using PaintDotNet.Drawing;
    using PaintDotNet.Imaging;
    using PaintDotNet.MemoryManagement;
    using PaintDotNet.Rendering;
    using PaintDotNet.UI.Media;
    using PaintDotNet.VisualStyling;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    internal class PdnToolStripComboBox : ToolStripComboBox
    {
        private Dictionary<object, DeviceBitmap> itemBitmapCache = new Dictionary<object, DeviceBitmap>();
        private ISurface<ColorBgra> renderBuffer;
        private SelectionHighlightRenderer selectionHighlightRenderer = new SelectionHighlightRenderer();

        public PdnToolStripComboBox(bool enableOwnerDraw)
        {
            base.ComboBox.Select(0, 0);
            base.ComboBox.FlatStyle = FlatStyle.Standard;
            if (!enableOwnerDraw)
            {
                base.ComboBox.HandleCreated += new EventHandler(this.OnComboBoxFirstHandleCreated);
                base.ComboBox.LostFocus += (s, e) => base.ComboBox.Select(0, 0);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposableUtil.Free<SelectionHighlightRenderer>(ref this.selectionHighlightRenderer);
                DisposableUtil.Free<ISurface<ColorBgra>>(ref this.renderBuffer);
            }
            base.Dispose(disposing);
        }

        protected virtual DeviceBitmap GetItemBitmap(object item, int maxHeight) => 
            null;

        protected virtual string GetItemText(object item) => 
            base.ComboBox.GetItemText(item);

        private void InitializeCustomDrawing()
        {
            base.ComboBox.DrawMode = DrawMode.OwnerDrawFixed;
            base.ComboBox.DrawItem += new DrawItemEventHandler(this.OnComboBoxDrawItem);
            base.ComboBox.Select(0, 0);
        }

        private void OnComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index != -1)
            {
                DeviceBitmap itemBitmap;
                object item = base.Items[e.Index];
                string itemText = this.GetItemText(item);
                if (!this.itemBitmapCache.TryGetValue(item, out itemBitmap))
                {
                    itemBitmap = this.GetItemBitmap(item, 0x10);
                    this.itemBitmapCache.Add(item, itemBitmap);
                }
                HighlightState state = ((e.State & DrawItemState.Selected) == DrawItemState.Selected) ? HighlightState.Hover : HighlightState.Default;
                if (((this.renderBuffer == null) || (this.renderBuffer.Width < e.Bounds.Width)) || (this.renderBuffer.Height < e.Bounds.Height))
                {
                    DisposableUtil.Free<ISurface<ColorBgra>>(ref this.renderBuffer);
                    this.renderBuffer = SurfaceAllocator.Bgra.Allocate(e.Bounds.Width, e.Bounds.Height, AllocationOptions.ZeroFillNotRequired);
                }
                using (IDrawingContext context = DrawingContext.FromSurface(this.renderBuffer, AlphaMode.Ignore, FactorySource.PerThread))
                {
                    context.Clear(new ColorRgba128Float?(System.Drawing.SystemColors.Window));
                    this.selectionHighlightRenderer.HighlightState = state;
                    this.selectionHighlightRenderer.RenderBackground(context, new RectFloat(0f, 0f, (float) e.Bounds.Width, (float) e.Bounds.Height));
                    double x = 2.0;
                    if (itemBitmap != null)
                    {
                        double height = Math.Min((double) e.Bounds.Height, itemBitmap.Size.Height);
                        double width = (itemBitmap.Size.Width * height) / itemBitmap.Size.Height;
                        double y = (e.Bounds.Height - height) / 2.0;
                        RectDouble num6 = new RectDouble(2.0, y, width, height);
                        context.DrawBitmap(itemBitmap, new RectDouble?(num6), 1.0, BitmapInterpolationMode.Linear, null);
                        x = num6.Right + 2.0;
                    }
                    RectDouble num2 = new RectDouble(x, 0.0, e.Bounds.Width - x, (double) e.Bounds.Height);
                    TextLayout textLayout = UIText.CreateLayout(context, itemText, this.Font, null, HotkeyRenderMode.Ignore, num2.Width, num2.Height);
                    textLayout.WordWrapping = WordWrapping.NoWrap;
                    textLayout.TrimmingGranularity = TrimmingGranularity.Character;
                    textLayout.TrimmingStyle = TextTrimmingStyle.Ellipsis;
                    context.DrawTextLayout(num2.Location, textLayout, this.selectionHighlightRenderer.EmbeddedTextBrush, DrawTextOptions.None);
                }
                using (e.Graphics.UseCompositingMode(CompositingMode.SourceCopy))
                {
                    e.Graphics.DrawSurface(this.renderBuffer, false, e.Bounds, new Rectangle(0, 0, e.Bounds.Width, e.Bounds.Height), GraphicsUnit.Pixel);
                }
            }
        }

        private void OnComboBoxFirstHandleCreated(object sender, EventArgs e)
        {
            base.ComboBox.HandleCreated -= new EventHandler(this.OnComboBoxFirstHandleCreated);
            if (base.ComboBox.DrawMode == DrawMode.Normal)
            {
                base.ComboBox.BeginInvoke(new Action(this.InitializeCustomDrawing));
            }
        }
    }
}

