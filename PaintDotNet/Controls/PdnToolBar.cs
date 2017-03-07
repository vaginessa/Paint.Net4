namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Direct2D;
    using PaintDotNet.DirectWrite;
    using PaintDotNet.Drawing;
    using PaintDotNet.Imaging;
    using PaintDotNet.Menus;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.UI.Media;
    using PaintDotNet.VisualStyling;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;

    internal class PdnToolBar : Control, IPaintBackground, IGlassyControl
    {
        private PaintDotNet.Controls.AppWorkspace appWorkspace;
        private PdnAuxMenu auxMenu;
        private CaptionControl captionControl;
        private PaintDotNet.Controls.CommonActionsStrip commonActionsStrip;
        private PaintDotNet.Controls.ArrowButton documentListButton;
        private PaintDotNet.Controls.DocumentStrip documentStrip;
        private IMessageFilter glassWndProcFilter;
        private DateTime ignoreShowDocumentListUntil = DateTime.MinValue;
        private ImageListMenu imageListMenu;
        private PdnMainMenu mainMenu;
        private GraphicsPath outlinePath;
        private Rectangle outlinePathClientRect = Rectangle.Empty;
        private int outlinePathHInset;
        private int outlinePathOpaqueWidth;
        private Region outlineRegion;
        private PenBrushCache penBrushCache = PenBrushCache.ThreadInstance;
        private Size prevSize;
        private ISystemFonts systemFonts;
        private PdnToolBarStripRenderer toolBarStripRenderer;
        private PaintDotNet.Controls.ToolChooserStrip toolChooserStrip;
        private PaintDotNet.Controls.ToolConfigStrip toolConfigStrip;
        private const ToolStripGripStyle toolStripsGripStyle = ToolStripGripStyle.Hidden;

        public PdnToolBar(PaintDotNet.Controls.AppWorkspace appWorkspace)
        {
            this.appWorkspace = appWorkspace;
            base.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            base.SuspendLayout();
            this.InitializeComponent();
            this.mainMenu.AppWorkspace = appWorkspace;
            this.toolBarStripRenderer = new PdnToolBarStripRenderer();
            this.commonActionsStrip.Renderer = this.toolBarStripRenderer;
            this.toolChooserStrip.Renderer = this.toolBarStripRenderer;
            this.toolConfigStrip.Renderer = this.toolBarStripRenderer;
            this.mainMenu.Renderer = this.toolBarStripRenderer;
            this.auxMenu.Renderer = this.toolBarStripRenderer;
            this.documentListButton.ArrowImage = PdnResources.GetImageResource("Images.ToolBar.ImageListMenu.OpenButton.png").Reference;
            base.ResumeLayout(false);
            this.systemFonts = new PaintDotNet.DirectWrite.SystemFonts();
        }

        public void BindAuxMenuToAppWorkspace()
        {
            this.auxMenu.AppWorkspace = this.appWorkspace;
        }

        protected override void Dispose(bool disposing)
        {
            DisposableUtil.Free<ISystemFonts>(ref this.systemFonts, disposing);
            base.Dispose(disposing);
        }

        public void HideDocumentList()
        {
            this.imageListMenu.HideImageList();
        }

        private void InitializeComponent()
        {
            base.SuspendLayout();
            this.mainMenu = new PdnMainMenu();
            this.auxMenu = new PdnAuxMenu();
            this.commonActionsStrip = new PaintDotNet.Controls.CommonActionsStrip();
            this.toolChooserStrip = new PaintDotNet.Controls.ToolChooserStrip();
            this.toolConfigStrip = new PaintDotNet.Controls.ToolConfigStrip(this.appWorkspace.ToolSettings);
            this.documentStrip = new PaintDotNet.Controls.DocumentStrip();
            this.documentListButton = new PaintDotNet.Controls.ArrowButton();
            this.imageListMenu = new ImageListMenu();
            this.captionControl = new CaptionControl(this);
            this.mainMenu.SuspendLayout();
            this.auxMenu.SuspendLayout();
            this.commonActionsStrip.SuspendLayout();
            this.toolChooserStrip.SuspendLayout();
            this.toolConfigStrip.SuspendLayout();
            this.mainMenu.AutoSize = false;
            this.mainMenu.Dock = DockStyle.None;
            this.mainMenu.BackColor = Color.Transparent;
            this.auxMenu.AutoSize = false;
            this.auxMenu.Dock = DockStyle.None;
            this.auxMenu.DefaultDropDownDirection = ToolStripDropDownDirection.BelowLeft;
            this.auxMenu.BackColor = Color.Transparent;
            this.commonActionsStrip.Name = "commonActionsStrip";
            this.commonActionsStrip.BackColor = Color.Transparent;
            this.commonActionsStrip.AutoSize = false;
            this.commonActionsStrip.TabIndex = 0;
            this.commonActionsStrip.Dock = DockStyle.None;
            this.commonActionsStrip.GripStyle = ToolStripGripStyle.Hidden;
            this.toolChooserStrip.Name = "toolChooserStrip";
            this.toolChooserStrip.AutoSize = false;
            this.toolChooserStrip.TabIndex = 2;
            this.toolChooserStrip.Dock = DockStyle.None;
            this.toolChooserStrip.GripStyle = ToolStripGripStyle.Hidden;
            this.toolConfigStrip.Name = "drawConfigStrip";
            this.toolConfigStrip.AutoSize = false;
            this.toolConfigStrip.TabIndex = 3;
            this.toolConfigStrip.Dock = DockStyle.None;
            this.toolConfigStrip.GripStyle = ToolStripGripStyle.Hidden;
            this.documentStrip.AutoSize = false;
            this.documentStrip.BackColor = Color.FromArgb(0, 0, 0, 0);
            this.documentStrip.Name = "documentStrip";
            this.documentStrip.TabIndex = 5;
            this.documentStrip.ShowScrollButtons = true;
            this.documentStrip.DocumentListChanged += new EventHandler(this.OnDocumentStripDocumentListChanged);
            this.documentStrip.DocumentClicked += new ValueEventHandler<Tuple<DocumentWorkspace, DocumentClickAction>>(this.OnDocumentStripDocumentClicked);
            this.documentStrip.ManagedFocus = true;
            this.documentStrip.AllowReorder = true;
            this.documentStrip.LayoutRequested += new EventHandler(this.OnDocumentStripRequestLayout);
            this.documentListButton.Name = "documentListButton";
            this.documentListButton.ArrowDirection = ArrowDirection.Down;
            this.documentListButton.ReverseArrowColors = true;
            this.documentListButton.Click += new EventHandler(this.OnDocumentListButtonClick);
            this.imageListMenu.Name = "imageListMenu";
            this.imageListMenu.Closed += new EventHandler(this.OnImageListMenuClosed);
            this.imageListMenu.ItemClicked += new ValueEventHandler<ImageListMenu.Item>(this.OnImageListMenuItemClicked);
            this.captionControl.Name = "captionControl";
            this.captionControl.Paint += new PaintEventHandler(this.OnCaptionControlPaint);
            base.Controls.Add(this.documentListButton);
            base.Controls.Add(this.documentStrip);
            base.Controls.Add(this.mainMenu);
            base.Controls.Add(this.auxMenu);
            base.Controls.Add(this.imageListMenu);
            base.Controls.Add(this.captionControl);
            base.Controls.Add(this.commonActionsStrip);
            base.Controls.Add(this.toolChooserStrip);
            base.Controls.Add(this.toolConfigStrip);
            this.mainMenu.ResumeLayout(false);
            this.auxMenu.ResumeLayout(false);
            this.commonActionsStrip.ResumeLayout(false);
            this.toolChooserStrip.ResumeLayout(false);
            this.toolConfigStrip.ResumeLayout(false);
            base.ResumeLayout(false);
        }

        internal void InvalidateTitle()
        {
            base.Invalidate();
            this.captionControl.Invalidate();
        }

        private void OnCaptionControlPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.CompositingMode = CompositingMode.SourceCopy;
            e.Graphics.TryFillRectangle(this.penBrushCache.GetSolidBrush(Color.Transparent), e.ClipRectangle);
            this.PaintBackground(e.Graphics, e.ClipRectangle);
        }

        private void OnDocumentListButtonClick(object sender, EventArgs e)
        {
            if (this.imageListMenu.IsImageListVisible)
            {
                this.HideDocumentList();
            }
            else
            {
                this.ShowDocumentList();
            }
        }

        private void OnDocumentStripDocumentClicked(object sender, ValueEventArgs<Tuple<DocumentWorkspace, DocumentClickAction>> e)
        {
            if (((DocumentClickAction) e.Value.Item2) == DocumentClickAction.Select)
            {
                base.PerformLayout();
            }
        }

        private void OnDocumentStripDocumentListChanged(object sender, EventArgs e)
        {
            base.PerformLayout();
            if (this.documentStrip.DocumentCount == 0)
            {
                this.toolChooserStrip.Enabled = false;
                this.toolConfigStrip.Enabled = false;
            }
            else
            {
                this.toolChooserStrip.Enabled = true;
                this.toolConfigStrip.Enabled = true;
            }
        }

        private void OnDocumentStripRequestLayout(object sender, EventArgs e)
        {
            base.PerformLayout((Control) sender, "Layout");
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            PdnBaseForm form = base.FindForm() as PdnBaseForm;
            if (((form != null) && form.MaximizeBox) && ((form.WindowState != FormWindowState.Minimized) && form.IsGlassEffectivelyEnabled))
            {
                using (PdnRegion region = form.CreateGlassInsetRegion())
                {
                    Point mousePosition = Control.MousePosition;
                    Point location = base.PointToClient(mousePosition);
                    if (region.IsVisible(new Rectangle(location, new Size(1, 1))))
                    {
                        FormWindowState normal;
                        FormWindowState windowState = form.WindowState;
                        if (windowState != FormWindowState.Normal)
                        {
                            if (windowState != FormWindowState.Maximized)
                            {
                                throw new PaintDotNet.InternalErrorException(new InvalidEnumArgumentException("form.WindowState", (int) form.WindowState, typeof(FormWindowState)));
                            }
                            normal = FormWindowState.Normal;
                        }
                        else
                        {
                            normal = FormWindowState.Maximized;
                        }
                        form.WindowState = normal;
                    }
                }
            }
            base.OnDoubleClick(e);
        }

        private void OnImageListMenuClosed(object sender, EventArgs e)
        {
            this.documentListButton.ForcedPushedAppearance = false;
            this.ignoreShowDocumentListUntil = DateTime.Now + new TimeSpan(0, 0, 0, 0, 250);
        }

        private void OnImageListMenuItemClicked(object sender, ValueEventArgs<ImageListMenu.Item> e)
        {
            DocumentWorkspace tag = (DocumentWorkspace) e.Value.Tag;
            if (!tag.IsDisposed)
            {
                this.documentStrip.SelectedDocument = tag;
            }
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            int captionAreaHeight = this.CaptionAreaHeight;
            int captionHeight = SystemMetrics.CaptionHeight;
            int sizeFrameHeight = SystemMetrics.SizeFrameHeight;
            int paddedBorderExtent = SystemMetrics.PaddedBorderExtent;
            int num5 = sizeFrameHeight + paddedBorderExtent;
            int y = this.DrawCaptionArea ? num5 : 0;
            bool flag = ThemeConfig.EffectiveTheme == PdnTheme.Aero;
            Size clientSize = base.ClientSize;
            int height = clientSize.Height;
            Rectangle bounds = this.mainMenu.Bounds;
            Size preferredSize = this.mainMenu.PreferredSize;
            Padding padding = this.mainMenu.Padding;
            Padding padding2 = padding;
            Rectangle rectangle4 = this.auxMenu.Bounds;
            Size size3 = this.auxMenu.PreferredSize;
            Padding padding4 = this.auxMenu.Padding;
            Rectangle rectangle5 = this.documentStrip.Bounds;
            Rectangle rectangle6 = rectangle5;
            Rectangle rectangle8 = this.commonActionsStrip.Bounds;
            Size size4 = this.commonActionsStrip.PreferredSize;
            Rectangle rectangle10 = this.toolChooserStrip.Bounds;
            Size size5 = this.toolChooserStrip.PreferredSize;
            Rectangle rectangle12 = this.toolChooserStrip.Bounds;
            Size size6 = this.toolConfigStrip.PreferredSize;
            Rectangle rectangle14 = this.documentListButton.Bounds;
            Rectangle rectangle16 = this.imageListMenu.Bounds;
            bounds.Location = new Point(0, captionAreaHeight);
            bounds.Height = preferredSize.Height + (flag ? 3 : 0);
            rectangle4.Height = size3.Height;
            padding2 = new Padding(flag ? 5 : 0, padding.Top, 0, padding.Bottom - (flag ? 1 : 0));
            padding4.Top = 0;
            padding4.Bottom = padding2.Bottom;
            rectangle4.Width = size3.Width;
            rectangle4.Location = new Point(clientSize.Width - rectangle4.Width, bounds.Top);
            rectangle8.Height = size4.Height + 1;
            int num8 = Math.Max(size5.Height, size6.Height) + 1;
            rectangle10.Height = num8;
            rectangle12.Height = num8;
            rectangle8.Width = size4.Width;
            rectangle10.Width = size5.Width;
            rectangle12.Width = clientSize.Width - rectangle10.Width;
            rectangle8.Location = new Point(3, bounds.Bottom + 1);
            rectangle10.Location = new Point(3, rectangle8.Bottom + 1);
            rectangle12.Location = new Point(rectangle10.Right, rectangle10.Top);
            bounds.Width = preferredSize.Width;
            int right = bounds.Right;
            int num10 = (rectangle8.Left + size4.Width) + this.commonActionsStrip.Margin.Horizontal;
            int num11 = (((rectangle10.Left + size5.Width) + this.toolChooserStrip.Margin.Horizontal) + size6.Width) + this.toolConfigStrip.Margin.Horizontal;
            int num12 = Math.Max(right, num10);
            bool flag2 = this.documentStrip.DocumentCount > 0;
            if (flag2)
            {
                int num17 = UIUtil.ScaleWidth(0x12);
                rectangle14.Width = num17;
            }
            else
            {
                rectangle14.Width = 0;
            }
            int left = rectangle4.Left;
            if (flag)
            {
                int aeroCaptionBottonsWidth = UIUtil.GetAeroCaptionBottonsWidth();
                left = Math.Min(left, clientSize.Width - aeroCaptionBottonsWidth);
            }
            int preferredMinClientWidth = this.documentStrip.PreferredMinClientWidth;
            Size size7 = this.documentStrip.PreferredSize;
            if (this.documentStrip.DocumentCount == 0)
            {
                rectangle6.Width = 0;
            }
            else
            {
                rectangle6.Width = Math.Max(preferredMinClientWidth, Math.Min(this.documentStrip.PreferredSize.Width, (left - num12) - rectangle14.Width));
            }
            rectangle6.Height = (rectangle8.Bottom - y) - 1;
            rectangle14.Height = rectangle6.Height;
            int num15 = left - (rectangle6.Width + rectangle14.Width);
            bounds.Width = Math.Min(num15, preferredSize.Width);
            rectangle8.Width = Math.Min(num12, num15);
            rectangle6.Location = new Point(Math.Max(0, rectangle8.Right + 1), y);
            rectangle14.Location = new Point(rectangle6.Right, y);
            rectangle16.Location = new Point(rectangle14.Left, rectangle14.Bottom - 1);
            rectangle16.Width = rectangle14.Width;
            rectangle16.Height = 0;
            int num16 = UIUtil.ScaleHeight(1);
            height = rectangle12.Bottom + num16;
            base.ClientSize = new Size(clientSize.Width, height);
            this.mainMenu.Padding = padding2;
            this.mainMenu.Bounds = bounds;
            this.auxMenu.Padding = padding4;
            this.auxMenu.Bounds = rectangle4;
            this.commonActionsStrip.Bounds = rectangle8;
            this.toolChooserStrip.Bounds = rectangle10;
            this.toolConfigStrip.Bounds = rectangle12;
            this.documentListButton.Bounds = rectangle14;
            this.documentListButton.Visible = flag2;
            this.documentListButton.Enabled = flag2;
            this.documentStrip.Bounds = rectangle6;
            this.imageListMenu.Bounds = rectangle16;
            Rectangle rectangle17 = new Rectangle(0, 0, rectangle6.Left, captionHeight);
            this.captionControl.Bounds = rectangle17;
            this.documentStrip.PerformLayout();
            if (rectangle6 != rectangle5)
            {
                int width = Math.Max(rectangle5.Left, rectangle14.Left);
                base.Invalidate(new Rectangle(0, 0, width, this.mainMenu.Top), false);
            }
            if (rectangle6.Width == 0)
            {
                this.mainMenu.Invalidate();
            }
            if (rectangle6.Height != rectangle5.Height)
            {
                this.documentStrip.RefreshAllThumbnails();
            }
            if (this.IsGlassDesired)
            {
                this.documentStrip.ClearTopInset = new int?(((this.GlassInset.Top - SystemMetrics.SizeFrameHeight) - SystemMetrics.PaddedBorderExtent) - 1);
            }
            else
            {
                this.documentStrip.ClearTopInset = null;
            }
            base.OnLayout(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (UIUtil.IsControlPaintingEnabled(this))
            {
                this.PaintBackground(e.Graphics, e.ClipRectangle);
            }
            base.OnPaint(e);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
        }

        internal void OnParentNonClientActivated()
        {
            this.UpdateTitle();
        }

        internal void OnParentNonClientDeactivate()
        {
            this.UpdateTitle();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            Size size = base.Size;
            if (size.Width != this.prevSize.Width)
            {
                int num = Math.Min(size.Width, this.prevSize.Width);
                int num2 = Math.Max(size.Width, this.prevSize.Width);
                base.Invalidate(new Rectangle(num - 3, 0, (num2 - num) + 3, size.Height), true);
            }
            this.prevSize = size;
            base.OnSizeChanged(e);
        }

        public void PaintBackground(Graphics g, Rectangle clipRect)
        {
            if (clipRect.HasPositiveArea())
            {
                int paddedBorderExtent = SystemMetrics.PaddedBorderExtent;
                int sizeFrameWidth = SystemMetrics.SizeFrameWidth;
                int sizeFrameHeight = SystemMetrics.SizeFrameHeight;
                int captionHeight = SystemMetrics.CaptionHeight;
                int num5 = sizeFrameWidth + paddedBorderExtent;
                int num6 = sizeFrameHeight + paddedBorderExtent;
                PdnBaseForm window = base.FindForm() as PdnBaseForm;
                bool flag = (window != null) ? window.IsNonClientActive : UIUtil.IsOurAppActiveNative;
                FormWindowState state = (window != null) ? window.WindowState : FormWindowState.Normal;
                using (g.TryUseSmoothingMode(SmoothingMode.None))
                {
                    using (g.TryUsePixelOffsetMode(PixelOffsetMode.None))
                    {
                        using (g.TryUseInterpolationMode(InterpolationMode.NearestNeighbor))
                        {
                            using (g.TryUseClip(clipRect, CombineMode.Replace))
                            {
                                ColorBgra? nullable;
                                Color toolBarOutlineColor;
                                Color color3;
                                int num27;
                                bool flag2 = ThemeConfig.EffectiveTheme == PdnTheme.Aero;
                                if (!flag2)
                                {
                                    Color menuStripGradientEnd = ProfessionalColors.MenuStripGradientEnd;
                                    TryFillRectangleClipped(g, this.penBrushCache.GetSolidBrush(menuStripGradientEnd), clipRect, clipRect);
                                    return;
                                }
                                bool isDwmCompositionEnabled = UIUtil.IsDwmCompositionEnabled;
                                Rectangle rectangle = new Rectangle(Point.Empty, base.ClientSize);
                                int top = this.mainMenu.Top;
                                int x = this.mainMenu.Padding.Left - 2;
                                int num9 = Math.Min(this.documentStrip.Left, this.mainMenu.Left + this.mainMenu.PreferredSize.Width);
                                int num10 = isDwmCompositionEnabled ? -1 : 0;
                                if (((rectangle != this.outlinePathClientRect) || (num9 != this.outlinePathOpaqueWidth)) || (num10 != this.outlinePathHInset))
                                {
                                    DisposableUtil.Free<GraphicsPath>(ref this.outlinePath);
                                    DisposableUtil.Free<Region>(ref this.outlineRegion);
                                }
                                if (this.outlinePath == null)
                                {
                                    this.outlinePathClientRect = rectangle;
                                    this.outlinePathOpaqueWidth = num9;
                                    this.outlinePathHInset = num10;
                                    this.outlinePath = new GraphicsPath();
                                    int y = ((this.CaptionAreaHeight + this.mainMenu.PreferredSize.Height) + (flag2 ? 3 : 0)) - 1;
                                    int num13 = rectangle.Height - y;
                                    int num14 = num10;
                                    int num15 = (rectangle.Right - 1) - num10;
                                    Point[] points = new Point[] { new Point(num10, rectangle.Bottom - 1), new Point(num10, y), new Point(x, y), new Point(x, top), new Point(num9 - 1, top), new Point(num9 - 1, y), new Point(num15, y), new Point(num15, rectangle.Bottom - 1), new Point(num10, rectangle.Bottom - 1) };
                                    this.outlinePath.AddLines(points);
                                    this.outlineRegion = new Region(this.outlinePath);
                                }
                                if (isDwmCompositionEnabled)
                                {
                                    using (g.TryUseCompositingMode(CompositingMode.SourceCopy))
                                    {
                                        Rectangle rectangle8 = new Rectangle(0, 0, x, this.mainMenu.Bottom);
                                        Rectangle rectangle9 = new Rectangle(x, 0, rectangle.Right - x, top);
                                        Rectangle rectangle10 = new Rectangle(num9, 0, rectangle.Right - num9, this.mainMenu.Bottom);
                                        Rectangle rectangle11 = (window != null) ? window.GlassInsetTopRect : this.GlassInsetTopRect();
                                        Rectangle rect = Rectangle.Intersect(clipRect, rectangle11);
                                        if (rect.HasPositiveArea())
                                        {
                                            System.Drawing.Brush solidBrush = this.penBrushCache.GetSolidBrush(Color.FromArgb(0));
                                            TryFillRectangleClipped(g, solidBrush, rect, rectangle8);
                                            TryFillRectangleClipped(g, solidBrush, rect, rectangle9);
                                            TryFillRectangleClipped(g, solidBrush, rect, rectangle10);
                                        }
                                        ColorBgra formBackColor = AeroColors.FormBackColor;
                                        if (flag && !UIUtil.IsImmersiveTabletModeEnabled)
                                        {
                                            UIUtil.WindowColorizationInfo? windowColorizationInfo = UIUtil.GetWindowColorizationInfo();
                                            if (windowColorizationInfo.HasValue && windowColorizationInfo.Value.ColorPrevalence)
                                            {
                                                formBackColor = windowColorizationInfo.Value.AccentColor;
                                                formBackColor.A = 0xff;
                                            }
                                        }
                                        nullable = new ColorBgra?(formBackColor);
                                        Rectangle rectangle13 = Rectangle.FromLTRB(0, rectangle11.Bottom, rectangle11.Right, rectangle10.Bottom);
                                        Rectangle rectangle14 = Rectangle.Intersect(clipRect, rectangle13);
                                        if (rectangle14.HasPositiveArea())
                                        {
                                            System.Drawing.Brush brush = this.penBrushCache.GetSolidBrush((Color) formBackColor);
                                            TryFillRectangleClipped(g, brush, rectangle14, rectangle8);
                                            TryFillRectangleClipped(g, brush, rectangle14, rectangle9);
                                            TryFillRectangleClipped(g, brush, rectangle14, rectangle10);
                                        }
                                        goto Label_0517;
                                    }
                                }
                                Rectangle bounds = new Rectangle(-num5, 0, base.ClientSize.Width + (2 * num5), this.DrawCaptionArea ? (num6 + captionHeight) : 0);
                                if (this.DrawCaptionArea)
                                {
                                    VisualStyleElement element = flag ? VisualStyleElement.Window.Caption.Active : VisualStyleElement.Window.Caption.Disabled;
                                    try
                                    {
                                        new VisualStyleRenderer(element).DrawBackground(g, bounds, clipRect);
                                    }
                                    catch (InvalidOperationException)
                                    {
                                    }
                                }
                                Rectangle fillRect = new Rectangle(0, bounds.Bottom, base.ClientSize.Width, base.ClientSize.Height - bounds.Bottom);
                                if (OS.IsWin8OrLater)
                                {
                                    color3 = AeroColors.FormBackColor;
                                }
                                else
                                {
                                    color3 = flag ? System.Drawing.SystemColors.GradientActiveCaption : System.Drawing.SystemColors.GradientInactiveCaption;
                                }
                                TryFillRectangleClipped(g, this.penBrushCache.GetSolidBrush(color3), clipRect, fillRect);
                                nullable = new ColorBgra?(color3);
                            Label_0517:
                                if (this.DrawCaptionArea)
                                {
                                    ImageResource imageResource;
                                    int smallIconWidth = SystemMetrics.SmallIconWidth;
                                    if (smallIconWidth < 0x18)
                                    {
                                        imageResource = PdnResources.GetImageResource("Icons.PaintDotNet.16.png");
                                    }
                                    else
                                    {
                                        imageResource = PdnResources.GetImageResource("Icons.PaintDotNet.32.png");
                                    }
                                    Image reference = imageResource.Reference;
                                    int num17 = (state == FormWindowState.Maximized) ? num6 : 0;
                                    int bottom = num6 + captionHeight;
                                    int num19 = (state == FormWindowState.Maximized) ? (num6 + ((captionHeight - smallIconWidth) / 2)) : (((num6 + captionHeight) - smallIconWidth) / 2);
                                    Rectangle destRect = new Rectangle(2, num19, smallIconWidth, smallIconWidth);
                                    g.TryDrawImage(reference, destRect, new Rectangle(0, 0, reference.Width, reference.Height), GraphicsUnit.Pixel);
                                    if (window != null)
                                    {
                                        string text = window.Text;
                                        RectInt32 num20 = RectInt32.FromEdges(destRect.Right, num17, this.documentStrip.Left - 1, bottom);
                                        if (RectInt32.Intersect(clipRect.ToRectInt32(), num20).HasPositiveArea)
                                        {
                                            bool flag4 = true;
                                            bool flag5 = true;
                                            if (OS.IsWin10OrLater)
                                            {
                                                flag4 = true;
                                                flag5 = false;
                                            }
                                            else if (isDwmCompositionEnabled)
                                            {
                                                string str2 = "  " + text + "  ";
                                                Font captionFont = System.Drawing.SystemFonts.CaptionFont;
                                                TextFormatFlags flags = TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter;
                                                Font disposeMe = null;
                                                float size = captionFont.Size;
                                                float num23 = size * 0.75f;
                                                while (size > num23)
                                                {
                                                    DisposableUtil.Free<Font>(ref disposeMe);
                                                    disposeMe = new Font(captionFont.FontFamily, size, captionFont.Style);
                                                    if (TextRenderer.MeasureText(str2, disposeMe, new Size(0xffff, 0xffff), flags).Width < num20.Width)
                                                    {
                                                        break;
                                                    }
                                                    size = Math.Max(num23, size - 0.25f);
                                                }
                                                VisualStyleElement element2 = flag ? VisualStyleElement.Window.Caption.Active : VisualStyleElement.Window.Caption.Inactive;
                                                Color? textColor = null;
                                                textColor = null;
                                                if (VisualStyleRendererUtil.TryDrawTextEx(window, g, str2, disposeMe, flags, num20.ToGdipRectangle(), element2.Part, element2.State, textColor, textColor, null, 1, null, null, true, 10))
                                                {
                                                    flag4 = false;
                                                }
                                                else
                                                {
                                                    flag4 = true;
                                                    flag5 = false;
                                                }
                                                DisposableUtil.Free<Font>(ref disposeMe);
                                                DisposableUtil.Free<Font>(ref captionFont);
                                            }
                                            if (flag4)
                                            {
                                                int num24 = 5;
                                                int num25 = num20.Width - num24;
                                                using (IDrawingContext context = DrawingContextUtil.FromGraphics(g, num20, true, FactorySource.PerThread))
                                                {
                                                    SizedFontProperties caption = this.systemFonts.Caption;
                                                    TextLayout textLayout = UIText.CreateLayout(context, text, caption, null, HotkeyRenderMode.Ignore, 65535.0, (double) num20.Height);
                                                    textLayout.Locale = PdnResources.Culture;
                                                    UIText.AdjustFontSizeToFitLayoutSize(context, textLayout, (double) num25, 65535.0, 0.6);
                                                    textLayout.TrimmingGranularity = TrimmingGranularity.Character;
                                                    textLayout.TrimmingStyle = TextTrimmingStyle.Ellipsis;
                                                    textLayout.ParagraphAlignment = ParagraphAlignment.Center;
                                                    textLayout.MaxWidth = num25;
                                                    textLayout.WordWrapping = WordWrapping.NoWrap;
                                                    ColorBgra bgra2 = flag ? System.Drawing.SystemColors.ActiveCaptionText : System.Drawing.SystemColors.InactiveCaptionText;
                                                    if ((OS.IsWin10OrLater && !SystemInformation.HighContrast) && !UIUtil.IsImmersiveTabletModeEnabled)
                                                    {
                                                        UIUtil.WindowColorizationInfo? nullable7 = UIUtil.GetWindowColorizationInfo();
                                                        if (nullable7.HasValue)
                                                        {
                                                            UIUtil.WindowColorizationInfo valueOrDefault = nullable7.GetValueOrDefault();
                                                            if (valueOrDefault.ColorPrevalence)
                                                            {
                                                                bgra2 = flag ? valueOrDefault.CaptionTextColor : valueOrDefault.InactiveCaptionTextColor;
                                                            }
                                                        }
                                                    }
                                                    TextAntialiasMode textAntialiasMode = context.TextAntialiasMode;
                                                    TextAntialiasMode aaMode = (!flag5 && (textAntialiasMode == TextAntialiasMode.ClearType)) ? TextAntialiasMode.Grayscale : textAntialiasMode;
                                                    RectInt32 num26 = new RectInt32(num20.X + num24, Math.Max(UIUtil.ScaleHeight(1), num20.Top), 0xffff, 0xffff);
                                                    using (context.UseAxisAlignedClip(num26, AntialiasMode.Aliased))
                                                    {
                                                        using (context.UseTextAntialiasMode(aaMode))
                                                        {
                                                            if (nullable.HasValue)
                                                            {
                                                                context.Clear(new ColorRgba128Float?(nullable.Value));
                                                            }
                                                            context.DrawTextLayout((double) (num20.X + num24), (double) num20.Top, textLayout, SolidColorBrushCache.Get((ColorRgba128Float) bgra2), DrawTextOptions.None);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                Rectangle b = new Rectangle(0, this.mainMenu.Top, num9, this.mainMenu.Height);
                                if (Rectangle.Intersect(clipRect, b).HasPositiveArea())
                                {
                                    using (Region region = this.outlineRegion.Clone())
                                    {
                                        region.Intersect(b);
                                        g.TryFillRegion(this.penBrushCache.GetSolidBrush(AeroColors.ToolBarBackFillGradTopColor), region);
                                    }
                                }
                                int height = 40;
                                Rectangle rectangle4 = new Rectangle(0, b.Bottom, rectangle.Width, height);
                                if (Rectangle.Intersect(clipRect, rectangle4).HasPositiveArea())
                                {
                                    using (Region region2 = this.outlineRegion.Clone())
                                    {
                                        System.Drawing.Brush brush3;
                                        bool flag6;
                                        region2.Intersect(rectangle4);
                                        if (AeroColors.ToolBarBackFillGradTopColor == AeroColors.ToolBarBackFillGradMidColor)
                                        {
                                            brush3 = this.penBrushCache.GetSolidBrush(AeroColors.ToolBarBackFillGradTopColor);
                                            flag6 = false;
                                        }
                                        else
                                        {
                                            Rectangle rectangle18 = rectangle4;
                                            num27 = rectangle18.Y - 1;
                                            rectangle18.Y = num27;
                                            num27 = rectangle18.Height + 1;
                                            rectangle18.Height = num27;
                                            brush3 = new System.Drawing.Drawing2D.LinearGradientBrush(rectangle18, AeroColors.ToolBarBackFillGradTopColor, AeroColors.ToolBarBackFillGradMidColor, LinearGradientMode.Vertical);
                                            flag6 = true;
                                        }
                                        g.TryFillRegion(brush3, region2);
                                        if (flag6)
                                        {
                                            brush3.Dispose();
                                            brush3 = null;
                                        }
                                    }
                                }
                                Rectangle rectangle6 = new Rectangle(rectangle4.Left, rectangle4.Bottom, rectangle4.Width, base.ClientSize.Height - rectangle4.Bottom);
                                if (Rectangle.Intersect(clipRect, rectangle6).HasPositiveArea())
                                {
                                    using (Region region3 = this.outlineRegion.Clone())
                                    {
                                        System.Drawing.Brush brush4;
                                        bool flag7;
                                        region3.Intersect(rectangle6);
                                        if (AeroColors.ToolBarBackFillGradMidColor == AeroColors.ToolBarBackFillGradBottomColor)
                                        {
                                            brush4 = this.penBrushCache.GetSolidBrush(AeroColors.ToolBarBackFillGradMidColor);
                                            flag7 = false;
                                        }
                                        else
                                        {
                                            Rectangle rectangle19 = rectangle6;
                                            num27 = rectangle19.Y - 1;
                                            rectangle19.Y = num27;
                                            num27 = rectangle19.Height + 1;
                                            rectangle19.Height = num27;
                                            brush4 = new System.Drawing.Drawing2D.LinearGradientBrush(rectangle19, AeroColors.ToolBarBackFillGradMidColor, AeroColors.ToolBarBackFillGradBottomColor, LinearGradientMode.Vertical);
                                            flag7 = true;
                                        }
                                        g.TryFillRegion(brush4, region3);
                                        if (flag7)
                                        {
                                            brush4.Dispose();
                                            brush4 = null;
                                        }
                                    }
                                }
                                if (OS.IsWin8OrLater && SystemMetrics.IsRemoteSession)
                                {
                                    toolBarOutlineColor = Color.FromArgb(0xff, AeroColors.ToolBarOutlineColor);
                                }
                                else
                                {
                                    toolBarOutlineColor = AeroColors.ToolBarOutlineColor;
                                }
                                using (g.TryUseCompositingMode(CompositingMode.SourceCopy))
                                {
                                    g.TryDrawPath(this.penBrushCache.GetPen(toolBarOutlineColor), this.outlinePath);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void SetGlassWndProcFilter(IMessageFilter filter)
        {
            this.glassWndProcFilter = filter;
        }

        public void ShowDocumentList()
        {
            if (((this.documentStrip.DocumentCount >= 1) && (DateTime.Now >= this.ignoreShowDocumentListUntil)) && !this.imageListMenu.IsImageListVisible)
            {
                DocumentWorkspace[] documentList = this.documentStrip.DocumentList;
                Image[] documentThumbnails = this.documentStrip.DocumentThumbnails;
                ImageListMenu.Item[] items = new ImageListMenu.Item[this.documentStrip.DocumentCount];
                for (int i = 0; i < items.Length; i++)
                {
                    bool selected = documentList[i] == this.documentStrip.SelectedDocument;
                    items[i] = new ImageListMenu.Item(documentThumbnails[i], documentList[i].GetFileFriendlyName(), selected);
                    items[i].Tag = documentList[i];
                }
                Cursor.Current = Cursors.Default;
                this.documentListButton.ForcedPushedAppearance = true;
                this.imageListMenu.ShowImageList(items);
            }
        }

        private static void TryFillRectangleClipped(Graphics g, System.Drawing.Brush brush, Rectangle clipRect, Rectangle fillRect)
        {
            Rectangle rect = Rectangle.Intersect(clipRect, fillRect);
            if (rect.HasPositiveArea())
            {
                g.TryFillRectangle(brush, rect);
            }
        }

        internal void UpdateTitle()
        {
            this.InvalidateTitle();
            base.Update();
        }

        protected override void WndProc(ref Message m)
        {
            bool flag = false;
            if (this.glassWndProcFilter != null)
            {
                flag = this.glassWndProcFilter.PreFilterMessage(ref m);
            }
            if (!flag)
            {
                base.WndProc(ref m);
            }
        }

        public PaintDotNet.Controls.AppWorkspace AppWorkspace =>
            this.appWorkspace;

        public PdnAuxMenu AuxMenu =>
            this.auxMenu;

        private int CaptionAreaHeight
        {
            get
            {
                if (!this.DrawCaptionArea)
                {
                    return 0;
                }
                return ((SystemMetrics.SizeFrameHeight + SystemMetrics.PaddedBorderExtent) + SystemMetrics.CaptionHeight);
            }
        }

        public PaintDotNet.Controls.CommonActionsStrip CommonActionsStrip =>
            this.commonActionsStrip;

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                UIUtil.AddCompositedExStyleToCreateParams(createParams);
                return createParams;
            }
        }

        public PaintDotNet.Controls.DocumentStrip DocumentStrip =>
            this.documentStrip;

        public bool DrawCaptionArea { get; set; }

        public Padding GlassCaptionDragInset
        {
            get
            {
                int num = this.mainMenu.PreferredSize.Height + ((ThemeConfig.EffectiveTheme == PdnTheme.Aero) ? 3 : 0);
                return new Padding(0, this.CaptionAreaHeight + num, 0, 0);
            }
        }

        public Padding GlassInset
        {
            get
            {
                if (OS.IsWin10OrLater)
                {
                    return new Padding(0, this.CaptionAreaHeight, 0, 0);
                }
                return this.GlassCaptionDragInset;
            }
        }

        public bool IsGlassDesired =>
            ((ThemeConfig.EffectiveTheme == PdnTheme.Aero) && UIUtil.IsDwmCompositionEnabled);

        public PdnMainMenu MainMenu =>
            this.mainMenu;

        Size IGlassyControl.ClientSize =>
            base.ClientSize;

        public PaintDotNet.Controls.ToolChooserStrip ToolChooserStrip =>
            this.toolChooserStrip;

        public PaintDotNet.Controls.ToolConfigStrip ToolConfigStrip =>
            this.toolConfigStrip;

        private sealed class CaptionControl : Control
        {
            private PdnToolBar owner;

            public CaptionControl(PdnToolBar owner)
            {
                base.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                base.ResizeRedraw = true;
                this.owner = owner;
            }

            protected override void WndProc(ref Message m)
            {
                bool flag = false;
                if (this.owner.glassWndProcFilter != null)
                {
                    flag = this.owner.glassWndProcFilter.PreFilterMessage(ref m);
                }
                if (!flag)
                {
                    base.WndProc(ref m);
                }
            }

            protected override System.Windows.Forms.CreateParams CreateParams
            {
                get
                {
                    System.Windows.Forms.CreateParams createParams = base.CreateParams;
                    UIUtil.AddCompositedExStyleToCreateParams(createParams);
                    return createParams;
                }
            }
        }

        private sealed class PdnToolBarStripRenderer : PdnToolStripRenderer
        {
            private SelectionHighlightRenderer selectionHighlightRenderer = new SelectionHighlightRenderer();
            private SolidColorBrush textBrush = new SolidColorBrush();

            public PdnToolBarStripRenderer()
            {
                base.RoundedEdges = false;
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                if (!string.IsNullOrWhiteSpace(e.Text))
                {
                    if (((e.Item is PdnMenuItem) && !e.Item.IsOnDropDown) && (e.TextDirection == ToolStripTextDirection.Horizontal))
                    {
                        PdnMenuItem item = (PdnMenuItem) e.Item;
                        using (IDrawingContext context = DrawingContextUtil.FromGraphics(e.Graphics, e.TextRectangle, false, FactorySource.PerThread))
                        {
                            Color textColor;
                            HotkeyRenderMode ignore;
                            if (ThemeConfig.EffectiveTheme == PdnTheme.Aero)
                            {
                                context.Clear(new ColorRgba128Float?(AeroColors.ToolBarBackFillGradTopColor));
                            }
                            if ((ThemeConfig.EffectiveTheme == PdnTheme.Aero) && (item.Selected || item.Pressed))
                            {
                                this.selectionHighlightRenderer.HighlightState = HighlightState.Hover;
                                this.selectionHighlightRenderer.RenderBackground(context, new RectInt32(0, 0, e.Item.Width, e.Item.Height));
                            }
                            if (ThemeConfig.EffectiveTheme == PdnTheme.Aero)
                            {
                                textColor = e.Item.Enabled ? AeroColors.MenuTextColor : DisabledRendering.GetDisabledColor(AeroColors.MenuTextColor);
                            }
                            else
                            {
                                textColor = e.TextColor;
                            }
                            this.textBrush.Color = textColor;
                            if ((e.TextFormat & TextFormatFlags.NoPrefix) == TextFormatFlags.NoPrefix)
                            {
                                ignore = HotkeyRenderMode.Ignore;
                            }
                            else if ((e.TextFormat & TextFormatFlags.HidePrefix) == TextFormatFlags.HidePrefix)
                            {
                                ignore = HotkeyRenderMode.Hide;
                            }
                            else
                            {
                                ignore = HotkeyRenderMode.Show;
                            }
                            TextLayout textLayout = UIText.CreateLayout(context, e.Text, e.TextFont, null, ignore, (double) e.TextRectangle.Width, (double) e.TextRectangle.Height);
                            UIText.AdjustFontSizeToFitLayoutSize(context, textLayout, (double) e.TextRectangle.Width, (double) (e.TextRectangle.Height + 1), 0.6);
                            textLayout.ParagraphAlignment = ParagraphAlignment.Center;
                            textLayout.TextAlignment = PaintDotNet.DirectWrite.TextAlignment.Center;
                            context.DrawTextLayout((double) e.TextRectangle.X, (double) e.TextRectangle.Y, textLayout, this.textBrush, DrawTextOptions.None);
                            return;
                        }
                    }
                    base.OnRenderItemText(e);
                }
            }

            protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
            {
                bool flag = false;
                if (((ThemeConfig.EffectiveTheme == PdnTheme.Aero) || (e.ToolStrip.GetType() == typeof(ToolStrip))) || ((e.ToolStrip.GetType() == typeof(ToolStripEx)) || (e.ToolStrip.GetType() == typeof(PdnMainMenu))))
                {
                    flag = this.PaintBackground(e.Graphics, e.ToolStrip, e.AffectedBounds);
                }
                if (!flag)
                {
                    base.OnRenderToolStripBackground(e);
                }
            }

            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
                if ((ThemeConfig.EffectiveTheme != PdnTheme.Aero) || (e.ToolStrip is ToolStripDropDown))
                {
                    base.OnRenderToolStripBorder(e);
                }
            }

            protected override void OnRenderToolStripPanelBackground(ToolStripPanelRenderEventArgs e)
            {
                this.PaintBackground(e.Graphics, e.ToolStripPanel, new Rectangle(new Point(0, 0), e.ToolStripPanel.Size));
                e.Handled = true;
            }

            private bool PaintBackground(Graphics g, Control control, Rectangle clipRect)
            {
                Control parent = control;
                IPaintBackground background = null;
                do
                {
                    parent = parent.Parent;
                    if (parent == null)
                    {
                        break;
                    }
                    background = parent as IPaintBackground;
                }
                while (background == null);
                if (background == null)
                {
                    return false;
                }
                Rectangle r = control.RectangleToScreen(clipRect);
                Rectangle rectangle2 = parent.RectangleToClient(r);
                int num = rectangle2.Left - clipRect.Left;
                int num2 = rectangle2.Top - clipRect.Top;
                g.TranslateTransform((float) -num, (float) -num2, MatrixOrder.Append);
                background.PaintBackground(g, rectangle2);
                g.TranslateTransform((float) num, (float) num2, MatrixOrder.Append);
                return true;
            }
        }
    }
}

