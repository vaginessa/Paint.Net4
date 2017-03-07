namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Drawing;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.Threading;
    using PaintDotNet.VisualStyling;
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class PanControl : UserControl
    {
        private Bitmap cachedUnderlay;
        private Rectangle dragAreaRect;
        private Cursor handCursor;
        private Cursor handMouseDownCursor;
        private bool mouseDown;
        private PointF position = new PointF(0f, 0f);
        private Bitmap renderSurface;
        private Point startMouse = new Point(0, 0);
        private PointF startPosition = new PointF(0f, 0f);
        private ImageResource staticImageUnderlay;

        [field: CompilerGenerated]
        public event EventHandler PositionChanged;

        public PanControl()
        {
            if (!base.DesignMode)
            {
                this.handCursor = PdnResources.GetCursor("Cursors.PanToolCursor.cur");
                this.handMouseDownCursor = PdnResources.GetCursor("Cursors.PanToolCursorMouseDown.cur");
                this.Cursor = this.handCursor;
            }
            this.InitializeComponent();
            this.RefreshDragAreaRect();
        }

        private void CheckRenderSurface()
        {
            if ((this.renderSurface != null) && (this.renderSurface.Size != base.Size))
            {
                this.renderSurface.Dispose();
                this.renderSurface = null;
            }
            if (this.renderSurface == null)
            {
                this.renderSurface = new Bitmap(base.Width, base.Height);
                using (Graphics graphics = Graphics.FromImage(this.renderSurface))
                {
                    this.DrawToGraphics(graphics);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.handCursor != null)
                {
                    this.handCursor.Dispose();
                    this.handCursor = null;
                }
                if (this.handMouseDownCursor != null)
                {
                    this.handMouseDownCursor.Dispose();
                    this.handMouseDownCursor = null;
                }
            }
            base.Dispose(disposing);
        }

        private void DoPaint(Graphics g)
        {
            this.CheckRenderSurface();
            g.DrawImage(this.renderSurface, base.ClientRectangle, base.ClientRectangle, GraphicsUnit.Pixel);
        }

        private void DrawToGraphics(Graphics g)
        {
            PointF tf4;
            Size clientSize = base.ClientSize;
            PointF tf = new PointF((this.position.X * this.dragAreaRect.Width) / ((float) clientSize.Width), (this.position.Y * this.dragAreaRect.Height) / ((float) clientSize.Height));
            PointF tf2 = new PointF(((float) clientSize.Width) / 2f, ((float) clientSize.Height) / 2f);
            PointF tf3 = new PointF(((1f + tf.X) * clientSize.Width) / 2f, ((1f + tf.Y) * clientSize.Height) / 2f);
            if (((-1f <= tf.X) && (tf.X <= 1f)) && ((-1f <= tf.Y) && (tf.Y <= 1f)))
            {
                tf4 = new PointF(((1f + tf.X) * clientSize.Width) / 2f, ((1f + tf.Y) * clientSize.Height) / 2f);
            }
            else
            {
                tf4 = new PointF(((1f + tf.X) * clientSize.Width) / 2f, ((1f + tf.Y) * clientSize.Height) / 2f);
                float introduced26 = Math.Abs(tf.X);
                if (introduced26 > Math.Abs(tf.Y))
                {
                    if (tf.X > 0f)
                    {
                        tf4.X = clientSize.Width - 1;
                        tf4.Y = ((1f + (tf.Y / tf.X)) * clientSize.Height) / 2f;
                    }
                    else
                    {
                        tf4.X = 0f;
                        tf4.Y = ((1f - (tf.Y / tf.X)) * clientSize.Height) / 2f;
                    }
                }
                else if (tf.Y > 0f)
                {
                    tf4.X = ((1f + (tf.X / tf.Y)) * clientSize.Width) / 2f;
                    tf4.Y = clientSize.Height - 1;
                }
                else
                {
                    tf4.X = ((1f - (tf.X / tf.Y)) * clientSize.Width) / 2f;
                    tf4.Y = 0f;
                }
            }
            using (g.UseCompositingMode(CompositingMode.SourceCopy))
            {
                g.Clear(this.BackColor);
            }
            using (g.UseCompositingMode(CompositingMode.SourceOver))
            {
                SizeInt32 num;
                Rectangle rectangle;
                if (this.staticImageUnderlay == null)
                {
                    goto Label_05A8;
                }
                if (this.cachedUnderlay != null)
                {
                    num = new SizeInt32(this.cachedUnderlay.Width, this.cachedUnderlay.Height);
                    goto Label_052E;
                }
                Image reference = this.staticImageUnderlay.Reference;
                Rectangle srcRect = new Rectangle(0, 0, reference.Width, reference.Height);
                SizeInt32 num2 = new SizeInt32(Math.Max(1, Math.Min(clientSize.Width - 4, srcRect.Width)), Math.Max(1, Math.Min(clientSize.Height - 4, srcRect.Height)));
                num = ThumbnailHelpers.ComputeThumbnailSize(reference.Size.ToSizeInt32(), Math.Min(num2.Width, num2.Height));
                this.cachedUnderlay = new Bitmap(num.Width, num.Height, PixelFormat.Format24bppRgb);
                ISurface<ColorBgra> surface = RendererBgra.Checkers(num).ToSurface();
                Bitmap image = surface.CreateAliasedGdipBitmap();
                Rectangle destRect = new Rectangle(0, 0, this.cachedUnderlay.Width, this.cachedUnderlay.Height);
                using (Graphics graphics = Graphics.FromImage(this.cachedUnderlay))
                {
                    graphics.CompositingMode = CompositingMode.SourceOver;
                    graphics.DrawImage(image, destRect, new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
                    Bitmap bitmap2 = reference as Bitmap;
                    if ((bitmap2 != null) && (bitmap2.PixelFormat == PixelFormat.Format32bppArgb))
                    {
                        BitmapData bitmapdata = bitmap2.LockBits(new Rectangle(new Point(0, 0), bitmap2.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                        try
                        {
                            using (SharedSurface<ColorBgra> surface2 = new SharedSurface<ColorBgra>(bitmapdata.Scan0, bitmapdata.Width, bitmapdata.Height, bitmapdata.Stride))
                            {
                                using (ISurface<ColorBgra> surface3 = surface2.ResizeSuperSampling(num).Parallelize<ColorBgra>(TilingStrategy.HorizontalSlices, 3, WorkItemQueuePriority.Normal).ToSurface())
                                {
                                    using (Bitmap bitmap3 = surface3.CreateAliasedGdipBitmap())
                                    {
                                        graphics.DrawImage(bitmap3, destRect, surface3.Bounds<ColorBgra>().ToGdipRectangle(), GraphicsUnit.Pixel);
                                        goto Label_051A;
                                    }
                                }
                            }
                        }
                        finally
                        {
                            bitmap2.UnlockBits(bitmapdata);
                        }
                    }
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    RectangleF ef = RectangleF.Inflate(destRect, 0.5f, 0.5f);
                    graphics.DrawImage(reference, ef, srcRect, GraphicsUnit.Pixel);
                }
            Label_051A:
                image.Dispose();
                image = null;
                surface.Dispose();
                surface = null;
            Label_052E:
                rectangle = new Rectangle((clientSize.Width - num.Width) / 2, (clientSize.Height - num.Height) / 2, num.Width, num.Height);
                g.DrawImage(this.cachedUnderlay, rectangle, new Rectangle(0, 0, this.cachedUnderlay.Width, this.cachedUnderlay.Height), GraphicsUnit.Pixel);
                DropShadow.DrawOutside(g, rectangle, 2);
            }
        Label_05A8:
            using (g.UsePixelOffsetMode(PixelOffsetMode.Half))
            {
                using (g.UseSmoothingMode(SmoothingMode.HighQuality))
                {
                    using (Pen pen = (Pen) Pens.Black.Clone())
                    {
                        pen.SetLineCap(LineCap.Round, LineCap.DiamondAnchor, DashCap.Flat);
                        pen.EndCap = LineCap.ArrowAnchor;
                        pen.Width = 2f;
                        pen.Color = SystemColors.ControlDark;
                        g.DrawLine(pen, tf2, tf4);
                    }
                    using (Pen pen2 = new Pen(Color.White))
                    {
                        pen2.SetLineCap(LineCap.DiamondAnchor, LineCap.DiamondAnchor, DashCap.Flat);
                        pen2.Width = 3f;
                        pen2.Color = Color.White;
                        g.DrawLine(pen2, tf3.X - 5f, tf3.Y, tf3.X + 5f, tf3.Y);
                        g.DrawLine(pen2, tf3.X, tf3.Y - 5f, tf3.X, tf3.Y + 5f);
                        pen2.Width = 2f;
                        pen2.Color = Color.Black;
                        g.DrawLine(pen2, tf3.X - 5f, tf3.Y, tf3.X + 5f, tf3.Y);
                        g.DrawLine(pen2, tf3.X, tf3.Y - 5f, tf3.X, tf3.Y + 5f);
                    }
                }
            }
        }

        private void InitializeComponent()
        {
            base.Name = "PanControl";
            base.Size = new Size(0xb8, 0xa8);
            base.TabStop = false;
        }

        private PointF MousePtToPosition(Point clientMousePt)
        {
            float num = base.ClientRectangle.Left + (((float) (base.ClientRectangle.Right - base.ClientRectangle.Left)) / 2f);
            float num2 = base.ClientRectangle.Top + (((float) (base.ClientRectangle.Bottom - base.ClientRectangle.Top)) / 2f);
            float num3 = clientMousePt.X - num;
            float num4 = clientMousePt.Y - num2;
            float x = num3 / (((float) this.dragAreaRect.Width) / 2f);
            return new PointF(x, num4 / (((float) this.dragAreaRect.Height) / 2f));
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if ((base.Enabled && !this.mouseDown) && (e.Button == MouseButtons.Left))
            {
                this.mouseDown = true;
                this.startPosition = this.position;
                this.startMouse = new Point(e.X, e.Y);
                this.Cursor = this.handMouseDownCursor;
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (this.mouseDown && (e.Button == MouseButtons.Left))
            {
                this.Position = this.MousePtToPosition(new Point(e.X, e.Y));
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (this.mouseDown)
            {
                if (e.Button == MouseButtons.Left)
                {
                    this.Position = this.MousePtToPosition(new Point(e.X, e.Y));
                }
                this.Cursor = this.handCursor;
                this.mouseDown = false;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            this.renderSurface = null;
            this.DoPaint(e.Graphics);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            this.DoPaint(pevent.Graphics);
        }

        private void OnPositionChanged()
        {
            this.PositionChanged.Raise(this);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            if (this.cachedUnderlay != null)
            {
                this.cachedUnderlay.Dispose();
                this.cachedUnderlay = null;
            }
            this.RefreshDragAreaRect();
            base.OnSizeChanged(e);
        }

        private PointF PositionToClientPt(PointF pos)
        {
            float num = base.ClientRectangle.Left + (((float) (base.ClientRectangle.Right - base.ClientRectangle.Left)) / 2f);
            float num2 = base.ClientRectangle.Top + (((float) (base.ClientRectangle.Bottom - base.ClientRectangle.Top)) / 2f);
            float num3 = ((float) this.dragAreaRect.Width) / 2f;
            float num4 = ((float) this.dragAreaRect.Height) / 2f;
            float x = num + (pos.X * num3);
            return new PointF(x, num2 + (pos.Y * num4));
        }

        private void RefreshDragAreaRect()
        {
            if (this.staticImageUnderlay == null)
            {
                this.dragAreaRect = base.ClientRectangle;
            }
            else
            {
                Image reference = this.staticImageUnderlay.Reference;
                Rectangle rectangle = new Rectangle(0, 0, reference.Width, reference.Height);
                Size size = new Size(Math.Min(base.ClientSize.Width - 4, rectangle.Width), Math.Min(base.ClientSize.Height - 4, rectangle.Height));
                SizeInt32 num = ThumbnailHelpers.ComputeThumbnailSize(reference.Size.ToSizeInt32(), Math.Min(size.Width, size.Height));
                Rectangle rectangle2 = new Rectangle((base.ClientSize.Width - num.Width) / 2, (base.ClientSize.Height - num.Height) / 2, num.Width, num.Height);
                this.dragAreaRect = new Rectangle(base.ClientRectangle.Left + rectangle2.Left, base.ClientRectangle.Top + rectangle2.Top, rectangle2.Width, rectangle2.Height);
            }
        }

        public PointF Position
        {
            get => 
                this.position;
            set
            {
                if (this.position != value)
                {
                    this.position = value;
                    base.Invalidate();
                    this.OnPositionChanged();
                    this.QueueUpdate();
                }
            }
        }

        public ImageResource StaticImageUnderlay
        {
            get => 
                this.staticImageUnderlay;
            set
            {
                this.staticImageUnderlay = value;
                if (this.cachedUnderlay != null)
                {
                    this.cachedUnderlay.Dispose();
                    this.cachedUnderlay = null;
                }
                this.RefreshDragAreaRect();
                base.Invalidate(true);
            }
        }
    }
}

