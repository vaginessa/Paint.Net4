namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.ComponentModel;
    using PaintDotNet.Concurrency;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Dialogs;
    using PaintDotNet.Direct2D;
    using PaintDotNet.DirectWrite;
    using PaintDotNet.Drawing;
    using PaintDotNet.Imaging;
    using PaintDotNet.Imaging.Proxies;
    using PaintDotNet.ObjectModel;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Threading;
    using PaintDotNet.Threading.Tasks;
    using PaintDotNet.UI.Media;
    using PaintDotNet.VisualStyling;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;
    using System.Windows.Threading;

    internal sealed class FontListComboBoxHandler : ComboBoxHandler
    {
        private ThreadDispatcher backgroundThread;
        private static readonly TimeSpan clearCacheDelay = new TimeSpan(0, 0, 30);
        private System.Windows.Forms.Timer clearCacheTimer;
        private string defaultFontName;
        private IFontMap fontMap;
        private Dictionary<TupleStruct<string, float, PaintDotNet.UI.Media.Brush>, PlacedBitmap> fontPreviewRenderCache;
        private readonly string fontSampleText;
        private bool hasBeenShown;
        private bool isPopulated;
        private SelectionHighlightRenderer selectionHighlightRenderer;
        private ISystemFonts systemFonts;
        private SolidColorBrush textBrush;

        public FontListComboBoxHandler(System.Windows.Forms.ComboBox comboBox, IFontMap fontMap, string defaultFontName) : base(comboBox, DrawMode.OwnerDrawVariable)
        {
            this.textBrush = new SolidColorBrush();
            this.fontPreviewRenderCache = new Dictionary<TupleStruct<string, float, PaintDotNet.UI.Media.Brush>, PlacedBitmap>();
            this.selectionHighlightRenderer = new SelectionHighlightRenderer();
            Validate.IsNotNull<IFontMap>(fontMap, "fontMap");
            this.fontMap = fontMap.CreateRef();
            this.fontMap.CollectionChanged += new EventHandler(this.OnFontFamilyNameCollectionChanged);
            this.systemFonts = new PaintDotNet.DirectWrite.SystemFonts();
            this.defaultFontName = defaultFontName ?? this.systemFonts.Message.FontProperties.DisplayName;
            base.ComboBox.Sorted = true;
            base.ComboBox.Items.Add(defaultFontName);
            base.ComboBox.SelectedItem = defaultFontName;
            base.ComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.fontSampleText = PdnResources.GetString("ToolConfigStrip.FontFamilyComboBox.FontSampleText");
        }

        public void AsyncPrefetchFontNames()
        {
            IFontMap fontMapP = this.fontMap.CreateRef();
            Work.QueueWorkItem(delegate {
                try
                {
                    string[] strArray = fontMapP.ToArrayEx<string>();
                }
                catch (Exception)
                {
                }
                finally
                {
                    fontMapP.Dispose();
                }
            });
        }

        private PlacedBitmap CreateFontPreview(string gdiFontName, float fontSize, PaintDotNet.UI.Media.Brush textBrush)
        {
            FontProperties fontProperties;
            TextMetrics metrics;
            OverhangMetrics overhangMetrics;
            try
            {
                fontProperties = this.fontMap.GetFontProperties(gdiFontName);
                using (IDrawingContext context = DrawingContext.CreateNull(FactorySource.PerThread))
                {
                    TextLayoutAlgorithm? layoutAlgorithm = null;
                    TextLayout resourceSource = UIText.CreateLayout(context, this.fontSampleText, fontProperties, (double) fontSize, layoutAlgorithm, HotkeyRenderMode.Ignore, 65535.0, 65535.0);
                    ITextLayout cachedOrCreateResource = context.GetCachedOrCreateResource<ITextLayout>(resourceSource);
                    metrics = cachedOrCreateResource.Metrics;
                    overhangMetrics = cachedOrCreateResource.OverhangMetrics;
                }
            }
            catch (Exception exception)
            {
                if ((!(exception is NoFontException) && !(exception is FontFileAccessException)) && (!(exception is FontFileFormatException) && !(exception is FontFileNotFoundException)))
                {
                    throw;
                }
                Surface cleanupObject = Surface.CopyFromGdipImage(PdnResources.GetImageResource("Icons.WarningIcon.png").Reference);
                BitmapProxy proxy = new BitmapProxy(cleanupObject.CreateAliasedImagingBitmap(), ObjectRefProxyOptions.AssumeOwnership);
                proxy.AddCleanupObject(cleanupObject);
                return new PlacedBitmap(proxy, new RectDouble(0.0, 0.0, (double) proxy.Size.Width, (double) proxy.Size.Height), true);
            }
            RectDouble a = new RectDouble((double) metrics.Left, (double) metrics.Top, (double) (metrics.Left + metrics.WidthMax), (double) (metrics.Top + metrics.Height));
            RectDouble b = RectDouble.FromEdges((double) (metrics.Left - overhangMetrics.Left), (double) (metrics.Top - overhangMetrics.Top), (double) (metrics.LayoutWidth + overhangMetrics.Right), (double) (metrics.LayoutHeight + overhangMetrics.Bottom));
            RectInt32 num4 = RectDouble.Union(a, b).Int32Bound;
            IBitmap bitmap = new PaintDotNet.Imaging.Bitmap(num4.Width, num4.Height, PixelFormats.Pbgra32, BitmapCreateCacheOption.CacheOnLoad);
            using (IDrawingContext context2 = DrawingContext.FromBitmap(bitmap, FactorySource.PerThread))
            {
                context2.Clear(null);
                using (context2.UseTranslateTransform((float) -num4.X, (float) -num4.Y, MatrixMultiplyOrder.Prepend))
                {
                    using (context2.UseTextRenderingMode(TextRenderingMode.Outline))
                    {
                        TextLayout textLayout = UIText.CreateLayout(context2, this.fontSampleText, fontProperties, (double) fontSize, null, HotkeyRenderMode.Ignore, 65535.0, 65535.0);
                        context2.TextAntialiasMode = TextAntialiasMode.Grayscale;
                        context2.DrawTextLayout(0.0, 0.0, textLayout, textBrush, DrawTextOptions.None);
                    }
                }
            }
            return new PlacedBitmap(bitmap, b, true);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposableUtil.Free<ThreadDispatcher>(ref this.backgroundThread);
                DisposableUtil.Free<ISystemFonts>(ref this.systemFonts);
                DisposableUtil.Free<System.Windows.Forms.Timer>(ref this.clearCacheTimer);
                DisposableUtil.Free<SelectionHighlightRenderer>(ref this.selectionHighlightRenderer);
                if (this.fontPreviewRenderCache != null)
                {
                    Dictionary<TupleStruct<string, float, PaintDotNet.UI.Media.Brush>, PlacedBitmap> fontPreviewRenderCache = this.fontPreviewRenderCache;
                    lock (fontPreviewRenderCache)
                    {
                        this.fontPreviewRenderCache.Values.ForEach<PlacedBitmap>(pb => pb.Dispose());
                        this.fontPreviewRenderCache = null;
                    }
                }
                if (this.fontMap != null)
                {
                    this.fontMap.CollectionChanged -= new EventHandler(this.OnFontFamilyNameCollectionChanged);
                    this.fontMap.Dispose();
                    this.fontMap = null;
                }
                DisposableUtil.Free<ISystemFonts>(ref this.systemFonts);
            }
            base.Dispose(disposing);
        }

        private void DrawComboBoxItem(DrawItemEventArgs e)
        {
            HighlightState hover;
            string fontName = (string) base.ComboBox.Items[e.Index];
            bool flag = (e.State & DrawItemState.Selected) > DrawItemState.None;
            bool flag2 = this.hasBeenShown && (e.Bounds.Width >= (base.ComboBox.DropDownWidth / 2));
            if (!flag2)
            {
                hover = HighlightState.Default;
            }
            else if (flag)
            {
                hover = HighlightState.Hover;
            }
            else
            {
                hover = HighlightState.Default;
            }
            Color selectionBackColor = SelectionHighlight.GetSelectionBackColor(hover);
            int num = UIUtil.ScaleWidth(3);
            int num2 = num;
            int num3 = -1;
            this.selectionHighlightRenderer.HighlightState = hover;
            using (IDrawingContext context = DrawingContextUtil.FromGraphics(e.Graphics, e.Bounds, false, FactorySource.PerThread))
            {
                RenderLayer layer = new RenderLayer();
                context.Clear(new ColorRgba128Float?((ColorRgba128Float) PaintDotNet.Imaging.SystemColors.Window));
                this.selectionHighlightRenderer.RenderBackground(context, e.Bounds.ToRectFloat());
                SizedFontProperties menu = this.systemFonts.Menu;
                PaintDotNet.UI.Media.Brush textBrush = this.selectionHighlightRenderer.EmbeddedTextBrush;
                TextLayout textLayout = UIText.CreateLayout(context, fontName, menu, null, HotkeyRenderMode.Ignore, (double) (e.Bounds.Width - num2), (double) e.Bounds.Height);
                textLayout.ParagraphAlignment = ParagraphAlignment.Center;
                textLayout.WordWrapping = WordWrapping.NoWrap;
                int num5 = num + e.Bounds.X;
                context.DrawTextLayout((double) num5, (double) (e.Bounds.Y + num3), textLayout, textBrush, DrawTextOptions.None);
                ITextLayout cachedOrCreateResource = context.GetCachedOrCreateResource<ITextLayout>(textLayout);
                int num4 = (int) Math.Ceiling((double) (num5 + cachedOrCreateResource.Metrics.WidthMax));
                PlacedBitmap bitmap = this.TryGetFontPreview(fontName, 16f, textBrush);
                if (bitmap == null)
                {
                    IntPtr listHwnd = UIUtil.GetListBoxHwnd(base.ComboBox);
                    Action callback = delegate {
                        try
                        {
                            PlacedBitmap bitmap = this.GetOrCreateFontPreview(fontName, 16f, textBrush);
                            if (listHwnd != IntPtr.Zero)
                            {
                                this.ComboBox.BeginInvoke(() => UIUtil.InvalidateHwnd(listHwnd));
                            }
                        }
                        catch (Exception)
                        {
                        }
                    };
                    if (this.backgroundThread == null)
                    {
                        this.backgroundThread = new ThreadDispatcher(ApartmentState.MTA);
                    }
                    this.backgroundThread.Enqueue(QueueSide.Front, callback).Observe();
                }
                if (flag2 && (bitmap != null))
                {
                    PaintDotNet.UI.Media.Brush brush;
                    RectFloat num7;
                    RectFloat num6 = new RectFloat((float) ((e.Bounds.Right - num) - bitmap.Bitmap.Size.Width), num3 + ((float) Math.Floor((double) ((e.Bounds.Y + ((e.Bounds.Height - bitmap.LayoutRect.Height) / 2.0)) - bitmap.LayoutRect.Top))), (float) bitmap.Bitmap.Size.Width, (float) bitmap.Bitmap.Size.Height);
                    if (num6.Left > num4)
                    {
                        num7 = num6;
                        brush = null;
                    }
                    else
                    {
                        num7 = num6;
                        num7.X += num4 - num6.X;
                        num7.X = (float) Math.Ceiling((double) num7.X);
                        LinearGradientBrush brush2 = new LinearGradientBrush {
                            ColorInterpolationMode = ColorInterpolationMode.SRgbLinearInterpolation,
                            SpreadMethod = GradientSpreadMethod.Pad
                        };
                        brush2.GradientStops.Add(new GradientStop(Color.White, 0.0));
                        brush2.GradientStops.Add(new GradientStop(Color.White, ((double) (e.Bounds.Width - UIUtil.ScaleWidth(0x18))) / ((double) e.Bounds.Width)));
                        brush2.GradientStops.Add(new GradientStop(Color.Transparent, 1.0));
                        brush2.EndPoint = new PointDouble((double) e.Bounds.Width, 0.0);
                        brush = brush2;
                    }
                    using (context.CreateLayer(null))
                    {
                        context.PushLayer(layer, new RectDouble?(num7), null, AntialiasMode.Aliased, new Matrix3x2Double?(Matrix3x2Float.Identity), 1.0, brush, LayerOptions.None);
                        context.DrawBitmap(bitmap.DeviceBitmap, new RectDouble?(num7), 1.0, BitmapInterpolationMode.Linear, null);
                        context.PopLayer();
                    }
                }
            }
        }

        private PlacedBitmap GetOrCreateFontPreview(string gdiFontName, float fontSize, PaintDotNet.UI.Media.Brush textBrush)
        {
            PlacedBitmap bitmap;
            TupleStruct<string, float, PaintDotNet.UI.Media.Brush> key = TupleStruct.Create<string, float, PaintDotNet.UI.Media.Brush>(gdiFontName, fontSize, textBrush);
            Dictionary<TupleStruct<string, float, PaintDotNet.UI.Media.Brush>, PlacedBitmap> fontPreviewRenderCache = this.fontPreviewRenderCache;
            lock (fontPreviewRenderCache)
            {
                this.fontPreviewRenderCache.TryGetValue(key, out bitmap);
            }
            if (bitmap == null)
            {
                bitmap = this.CreateFontPreview(gdiFontName, fontSize, textBrush);
                Dictionary<TupleStruct<string, float, PaintDotNet.UI.Media.Brush>, PlacedBitmap> dictionary2 = this.fontPreviewRenderCache;
                lock (dictionary2)
                {
                    if (this.fontPreviewRenderCache.ContainsKey(key))
                    {
                        bitmap.Dispose();
                        return this.fontPreviewRenderCache[key];
                    }
                    this.fontPreviewRenderCache.Add(key, bitmap);
                }
            }
            return bitmap;
        }

        private void OnClearCacheTimerTick(object sender, EventArgs e)
        {
            try
            {
                if (!base.IsDisposed && (this.fontPreviewRenderCache != null))
                {
                    using (Profiling.UseEnter("Clearing the font preview cache"))
                    {
                        Dictionary<TupleStruct<string, float, PaintDotNet.UI.Media.Brush>, PlacedBitmap> fontPreviewRenderCache = this.fontPreviewRenderCache;
                        lock (fontPreviewRenderCache)
                        {
                            this.fontPreviewRenderCache.Clear();
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                try
                {
                    DisposableUtil.Free<System.Windows.Forms.Timer>(ref this.clearCacheTimer);
                }
                catch (Exception)
                {
                }
            }
        }

        protected override void OnComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index != -1)
            {
                this.DrawComboBoxItem(e);
            }
            base.OnComboBoxDrawItem(sender, e);
        }

        protected override void OnComboBoxDropDown(object sender, EventArgs e)
        {
            this.hasBeenShown = true;
            DisposableUtil.Free<System.Windows.Forms.Timer>(ref this.clearCacheTimer);
            base.OnComboBoxDropDown(sender, e);
        }

        protected override void OnComboBoxDropDownClosed(object sender, EventArgs e)
        {
            ThreadDispatcher backgroundThreadP = this.backgroundThread;
            if (backgroundThreadP != null)
            {
                this.backgroundThread = null;
                backgroundThreadP.Enqueue(QueueSide.Front, () => backgroundThreadP.Dispose()).Observe();
            }
            DisposableUtil.Free<System.Windows.Forms.Timer>(ref this.clearCacheTimer);
            this.clearCacheTimer = new System.Windows.Forms.Timer();
            this.clearCacheTimer.Interval = (int) clearCacheDelay.TotalMilliseconds;
            this.clearCacheTimer.Tick += new EventHandler(this.OnClearCacheTimerTick);
            this.clearCacheTimer.Enabled = true;
            base.OnComboBoxDropDownClosed(sender, e);
        }

        protected override void OnComboBoxGotFocus(object sender, EventArgs e)
        {
            if (!this.isPopulated)
            {
                this.isPopulated = true;
                using (new WaitCursorChanger(base.ComboBox))
                {
                    string selectedItem = (string) base.ComboBox.SelectedItem;
                    string str2 = null;
                    ManualResetEventSlim gotFamilies = new ManualResetEventSlim(false);
                    string[] fontNames = null;
                    VirtualTask<Unit> task = TaskManager.Global.CreateVirtualTask(TaskState.Running);
                    IFontMap fontMapP = this.fontMap.CreateRef();
                    Work.QueueWorkItem(delegate {
                        try
                        {
                            fontNames = fontMapP.ToArrayEx<string>();
                        }
                        finally
                        {
                            fontMapP.Dispose();
                            try
                            {
                                gotFamilies.Set();
                            }
                            finally
                            {
                                task.SetState(TaskState.Finished);
                            }
                        }
                    });
                    if (!gotFamilies.Wait(0x3e8))
                    {
                        new TaskProgressDialog { 
                            Task = task,
                            CloseOnFinished = true,
                            ShowCancelButton = false,
                            Text = PdnInfo.BareProductName,
                            Icon = PdnInfo.AppIcon,
                            Text = PdnResources.GetString("TextConfigWidget.LoadingFontsList.Text")
                        }.ShowDialog(base.ComboBox);
                    }
                    gotFamilies.Wait();
                    base.ComboBox.BeginUpdate();
                    base.ComboBox.Items.Clear();
                    foreach (string str3 in fontNames)
                    {
                        int num2 = base.ComboBox.Items.Add(str3);
                        if ((selectedItem != null) && DirectWriteFactory.FontNameComparer.Equals(selectedItem, str3))
                        {
                            str2 = str3;
                        }
                    }
                    if (str2 != null)
                    {
                        base.ComboBox.SelectedItem = str2;
                    }
                    else
                    {
                        base.ComboBox.SelectedItem = this.defaultFontName;
                    }
                    base.ComboBox.EndUpdate();
                }
            }
            base.OnComboBoxGotFocus(sender, e);
        }

        protected override void OnComboBoxHandleCreated(object sender, EventArgs e)
        {
            base.OnComboBoxHandleCreated(sender, e);
        }

        protected override void OnComboBoxMeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = (e.ItemHeight * 4) / 3;
            base.OnComboBoxMeasureItem(sender, e);
        }

        private void OnFontFamilyNameCollectionChanged(object sender, EventArgs e)
        {
            if (base.ComboBox.IsHandleCreated)
            {
                try
                {
                    base.ComboBox.BeginInvoke(() => this.isPopulated = false);
                }
                catch (Exception)
                {
                }
            }
        }

        private PlacedBitmap TryGetFontPreview(string gdiFontName, float fontSize, PaintDotNet.UI.Media.Brush textBrush)
        {
            PlacedBitmap bitmap;
            TupleStruct<string, float, PaintDotNet.UI.Media.Brush> key = TupleStruct.Create<string, float, PaintDotNet.UI.Media.Brush>(gdiFontName, fontSize, textBrush);
            Dictionary<TupleStruct<string, float, PaintDotNet.UI.Media.Brush>, PlacedBitmap> fontPreviewRenderCache = this.fontPreviewRenderCache;
            lock (fontPreviewRenderCache)
            {
                bool flag2 = this.fontPreviewRenderCache.TryGetValue(key, out bitmap);
            }
            return bitmap;
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly FontListComboBoxHandler.<>c <>9 = new FontListComboBoxHandler.<>c();
            public static Action<FontListComboBoxHandler.PlacedBitmap> <>9__26_0;

            internal void <Dispose>b__26_0(FontListComboBoxHandler.PlacedBitmap pb)
            {
                pb.Dispose();
            }
        }

        private sealed class PlacedBitmap : IDisposable
        {
            private IBitmap bitmap;
            private PaintDotNet.UI.Media.DeviceBitmap deviceBitmap;
            private RectDouble layoutRect;

            public PlacedBitmap(IBitmap bitmap, RectDouble layoutRect, bool assumeOwnership)
            {
                this.bitmap = assumeOwnership ? bitmap : bitmap.CreateRef();
                this.layoutRect = layoutRect;
            }

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (disposing)
                {
                    DisposableUtil.Free<IBitmap>(ref this.bitmap);
                    if (this.deviceBitmap != null)
                    {
                        if (this.deviceBitmap.CheckAccess())
                        {
                            this.deviceBitmap.BitmapSource = null;
                        }
                        else
                        {
                            PaintDotNet.UI.Media.DeviceBitmap deviceBitmapLocal = this.deviceBitmap;
                            deviceBitmapLocal.Dispatcher.BeginInvoke(() => deviceBitmapLocal.BitmapSource = null, DispatcherPriority.ApplicationIdle, Array.Empty<object>());
                        }
                        this.deviceBitmap = null;
                    }
                }
            }

            public IBitmap Bitmap =>
                this.bitmap;

            public PaintDotNet.UI.Media.DeviceBitmap DeviceBitmap
            {
                get
                {
                    if (this.deviceBitmap == null)
                    {
                        this.deviceBitmap = new PaintDotNet.UI.Media.DeviceBitmap(this.bitmap);
                    }
                    return this.deviceBitmap;
                }
            }

            public RectDouble LayoutRect =>
                this.layoutRect;
        }
    }
}

