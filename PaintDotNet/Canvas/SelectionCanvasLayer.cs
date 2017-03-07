namespace PaintDotNet.Canvas
{
    using PaintDotNet;
    using PaintDotNet.Animation;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Functional;
    using PaintDotNet.Imaging;
    using PaintDotNet.Rendering;
    using PaintDotNet.UI.Media;
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;

    internal sealed class SelectionCanvasLayer : CanvasLayer<SelectionCanvasLayer, SelectionCanvasLayerView>
    {
        private static readonly double[] dashes = new double[] { ((4.0 * dashMultiplier) / 2.0), ((4.0 * dashMultiplier) / 2.0) };
        private const int dashLength = 4;
        private static readonly double dashMultiplier = (UIUtil.GetXScaleFactor() + UIUtil.GetYScaleFactor());
        private AnimatedInt32 dashOffsetProperty = new AnimatedInt32(0, AnimationRoundingMode.Floor);
        private static readonly Func<int, StrokeStyle> dashOffsetToDashedStrokeStyleFn = Func.Memoize<int, StrokeStyle>(new Func<int, StrokeStyle>(<>c.<>9.<.cctor>b__79_0));
        private static readonly PaintDotNet.Canvas.SelectionSnapshot emptySelectionSnapshot = new PaintDotNet.Canvas.SelectionSnapshot(Result.New<GeometryList>(new GeometryList()), Result.New<IReadOnlyList<RectInt32>>(Array.Empty<RectInt32>()), Matrix3x2Double.Identity, RectDouble.Empty, true, -1);
        private PaintDotNet.UI.Media.Brush interiorBrush;
        public static readonly DependencyProperty IsAnimatedOutlineEnabledProperty = DependencyProperty.RegisterAttached("IsAnimatedOutlineEnabled", typeof(bool), typeof(SelectionCanvasLayer), new PropertyMetadata(BooleanUtil.GetBoxed(true), new PropertyChangedCallback(SelectionCanvasLayer.OnIsAnimatedOutlineEnabledPropertyChanged)));
        public static readonly DependencyProperty IsAntialiasedOutlineEnabledProperty = DependencyProperty.RegisterAttached("IsAntialiasedOutlineEnabled", typeof(bool), typeof(SelectionCanvasLayer), new PropertyMetadata(BooleanUtil.GetBoxed(true), new PropertyChangedCallback(SelectionCanvasLayer.OnIsAntialiasedOutlineEnabledPropertyChanged)));
        private bool isInteriorFilled;
        private bool isOutlineEnabled = true;
        private PaintDotNet.Selection selection;
        public static readonly DependencyProperty SelectionRenderingQualityProperty = DependencyProperty.RegisterAttached("SelectionRenderingQuality", typeof(SelectionRenderingQuality), typeof(SelectionCanvasLayer), new PropertyMetadata(EnumUtil.GetBoxed<SelectionRenderingQuality>(SelectionRenderingQuality.Aliased), new PropertyChangedCallback(SelectionCanvasLayer.OnSelectionRenderingQualityPropertyChanged)));
        private PaintDotNet.Canvas.SelectionSnapshot selectionSnapshot;
        private int selectionSnapshotGeometryVersion;
        private bool useSystemTinting = true;

        [field: CompilerGenerated]
        public static  event ValueChangedEventHandler<bool> CanvasViewIsAnimatedOutlineEnabledChanged;

        [field: CompilerGenerated]
        public static  event ValueChangedEventHandler<bool> CanvasViewIsAntialiasedOutlineEnabledChanged;

        [field: CompilerGenerated]
        public static  event ValueChangedEventHandler<SelectionRenderingQuality> CanvasViewSelectionRenderingQualityChanged;

        public SelectionCanvasLayer()
        {
            this.dashOffsetProperty.ValueChanged += new ValueChangedEventHandler<int>(this.OnDashOffsetPropertyValueChanged);
            CanvasViewIsAnimatedOutlineEnabledChanged += new ValueChangedEventHandler<bool>(this.OnCanvasViewIsAnimatedOutlineEnabledChanged);
            CanvasViewIsAntialiasedOutlineEnabledChanged += new ValueChangedEventHandler<bool>(this.OnCanvasViewIsAntialiasedOutlineEnabledChanged);
            CanvasViewSelectionRenderingQualityChanged += new ValueChangedEventHandler<SelectionRenderingQuality>(this.OnCanvasViewSelectionRenderingQualityChanged);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Selection = null;
                CanvasViewIsAnimatedOutlineEnabledChanged -= new ValueChangedEventHandler<bool>(this.OnCanvasViewIsAnimatedOutlineEnabledChanged);
                CanvasViewIsAntialiasedOutlineEnabledChanged -= new ValueChangedEventHandler<bool>(this.OnCanvasViewIsAntialiasedOutlineEnabledChanged);
                CanvasViewSelectionRenderingQualityChanged -= new ValueChangedEventHandler<SelectionRenderingQuality>(this.OnCanvasViewSelectionRenderingQualityChanged);
            }
            base.Dispose(disposing);
        }

        private void EnsureAnimating()
        {
            base.VerifyAccess();
            if (((base.Owner != null) && (base.Owner.VisibleViewsCount > 0)) && (base.IsVisible && !this.dashOffsetProperty.IsAnimating))
            {
                if (this.IsAnimatedOutlineEnabledForAnyCanvasView())
                {
                    double num = (double) this.dashOffsetProperty.Value;
                    this.dashOffsetProperty.AnimateRawValue(new Action<IAnimationStoryboard, IAnimationVariable>(this.InitializeDashOffsetStoryboard), null);
                }
                else
                {
                    this.StopAnimation();
                }
            }
            else if (!this.IsAnimatedOutlineEnabledForAnyCanvasView())
            {
                this.StopAnimation();
            }
        }

        internal static StrokeStyle GetDashedStrokeStyle(int dashOffset) => 
            dashOffsetToDashedStrokeStyleFn(dashOffset);

        public static bool GetIsAnimatedOutlineEnabled(CanvasView canvasView) => 
            ((bool) canvasView.GetValue(IsAnimatedOutlineEnabledProperty));

        public static bool GetIsAntialiasedOutlineEnabled(CanvasView canvasView) => 
            ((bool) canvasView.GetValue(IsAntialiasedOutlineEnabledProperty));

        internal int GetOutlineDashOffset(CanvasView canvasView)
        {
            base.VerifyAccess();
            if (GetIsAnimatedOutlineEnabled(canvasView))
            {
                return this.dashOffsetProperty.Value;
            }
            return 0;
        }

        internal SelectionRenderParameters GetRenderParameters(CanvasView canvasView)
        {
            base.VerifyAccess();
            return new SelectionRenderParameters(this.SelectionSnapshot, canvasView.CanvasSize, canvasView.ViewportSize, canvasView.ViewportCanvasBounds, canvasView.ScaleRatio, this.IsInteriorFilled, this.EffectiveInteriorBrush, this.IsOutlineEnabled, GetIsAntialiasedOutlineEnabled(canvasView), GetIsAnimatedOutlineEnabled(canvasView), GetSelectionRenderingQuality(canvasView));
        }

        public static SelectionRenderingQuality GetSelectionRenderingQuality(CanvasView canvasView) => 
            ((SelectionRenderingQuality) canvasView.GetValue(SelectionRenderingQualityProperty));

        private void InitializeDashOffsetStoryboard(IAnimationStoryboard storyboard, IAnimationVariable variable)
        {
            AnimationStoryboard.InitializeLinearRepeating(storyboard, variable, 0.0, 4.0, 0.25, -1);
        }

        private void InvalidateSelectionArea()
        {
            base.VerifyAccess();
            foreach (SelectionCanvasLayerView view in base.GetCanvasLayerViews())
            {
                view.InvalidateSelectionArea();
            }
        }

        private void InvalidateSelectionSnapshot()
        {
            base.VerifyAccess();
            this.selectionSnapshot = null;
            foreach (SelectionCanvasLayerView view in base.GetCanvasLayerViews())
            {
                view.NotifySelectionSnapshotInvalidated();
            }
        }

        private bool IsAnimatedOutlineEnabledForAnyCanvasView()
        {
            base.VerifyAccess();
            if (base.Owner == null)
            {
                return false;
            }
            return base.Owner.RegisteredViews.Any<CanvasView>(cv => GetIsAnimatedOutlineEnabled(cv));
        }

        protected override void OnBeforeRender(RectFloat clipRect, CanvasView canvasView)
        {
            if ((this.selection == null) || this.selection.IsEmpty)
            {
                this.StopAnimation();
            }
            else if ((this.selection != null) && !this.selection.IsEmpty)
            {
                if (this.IsOutlineEnabled)
                {
                    this.EnsureAnimating();
                }
                else
                {
                    this.StopAnimation();
                }
            }
            base.OnBeforeRender(clipRect, canvasView);
        }

        protected override void OnCanvasChanged(PaintDotNet.Canvas.Canvas oldValue, PaintDotNet.Canvas.Canvas newValue)
        {
            if (oldValue != null)
            {
                oldValue.VisibleViewsCountChanged -= new ValueChangedEventHandler<int>(this.OnCanvasVisibleViewsCountChanged);
            }
            if (newValue != null)
            {
                newValue.VisibleViewsCountChanged += new ValueChangedEventHandler<int>(this.OnCanvasVisibleViewsCountChanged);
                if (newValue.VisibleViewsCount == 0)
                {
                    this.StopAnimation();
                }
            }
            else
            {
                this.StopAnimation();
            }
            base.OnCanvasChanged(oldValue, newValue);
        }

        private void OnCanvasViewIsAnimatedOutlineEnabledChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            CanvasView canvasView = (CanvasView) sender;
            this.QueueBeginRedraw(canvasView);
        }

        private void OnCanvasViewIsAntialiasedOutlineEnabledChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            CanvasView canvasView = (CanvasView) sender;
            this.QueueBeginRedraw(canvasView);
        }

        private void OnCanvasViewSelectionRenderingQualityChanged(object sender, ValueChangedEventArgs<SelectionRenderingQuality> e)
        {
            CanvasView canvasView = (CanvasView) sender;
            this.QueueBeginRedraw(canvasView);
        }

        private void OnCanvasVisibleViewsCountChanged(object sender, ValueChangedEventArgs<int> e)
        {
            if (e.NewValue == 0)
            {
                this.StopAnimation();
            }
        }

        private void OnDashOffsetPropertyValueChanged(object sender, EventArgs e)
        {
            base.VerifyAccess();
            this.InvalidateSelectionArea();
        }

        private static void OnIsAnimatedOutlineEnabledPropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            CanvasViewIsAnimatedOutlineEnabledChanged.Raise<bool>(target, e);
        }

        private static void OnIsAntialiasedOutlineEnabledPropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            CanvasViewIsAntialiasedOutlineEnabledChanged.Raise<bool>(target, e);
        }

        protected override void OnIsVisibleChanged(bool oldValue, bool newValue)
        {
            if (newValue)
            {
                foreach (CanvasView view in base.Owner.RegisteredViews)
                {
                    base.TryRecreateCanvasLayerView(view);
                }
                this.InvalidateSelectionArea();
            }
            else
            {
                foreach (CanvasView view2 in base.Owner.RegisteredViews)
                {
                    base.TryRemoveCanvasLayerView(view2);
                }
            }
            base.OnIsVisibleChanged(oldValue, newValue);
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            base.VerifyAccess();
            if ((e.ChangeFlags != SelectionChangeFlags.InterimTransform) && (e.ChangeFlags != SelectionChangeFlags.None))
            {
                this.selectionSnapshotGeometryVersion++;
            }
            this.InvalidateSelectionSnapshot();
        }

        private void OnSelectionChanging(object sender, EventArgs e)
        {
            base.VerifyAccess();
            this.InvalidateSelectionSnapshot();
        }

        private static void OnSelectionRenderingQualityPropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            CanvasViewSelectionRenderingQualityChanged.Raise<SelectionRenderingQuality>(target, e);
        }

        protected override SelectionCanvasLayerView OnTryCreateCanvasLayerView(CanvasView canvasView) => 
            new SelectionCanvasLayerView(this, canvasView);

        private void QueueBeginRedraw()
        {
            base.VerifyAccess();
            foreach (SelectionCanvasLayerView view in base.GetCanvasLayerViews())
            {
                view.QueueBeginRedraw();
            }
        }

        private void QueueBeginRedraw(CanvasView canvasView)
        {
            base.VerifyAccess();
            SelectionCanvasLayerView view = base.TryGetCanvasLayerView(canvasView);
            if (view != null)
            {
                view.QueueBeginRedraw();
            }
        }

        public static void SetIsAnimatedOutlineEnabled(CanvasView canvasView, bool value)
        {
            canvasView.SetValue(IsAnimatedOutlineEnabledProperty, BooleanUtil.GetBoxed(value));
        }

        public static void SetIsAntialiasedOutlineEnabled(CanvasView canvasView, bool value)
        {
            canvasView.SetValue(IsAntialiasedOutlineEnabledProperty, BooleanUtil.GetBoxed(value));
        }

        public static void SetSelectionRenderingQuality(CanvasView canvasView, SelectionRenderingQuality value)
        {
            canvasView.SetValue(SelectionRenderingQualityProperty, EnumUtil.GetBoxed<SelectionRenderingQuality>(value));
        }

        private void StopAnimation()
        {
            base.VerifyAccess();
            if (this.dashOffsetProperty.IsAnimating || !this.IsAnimatedOutlineEnabledForAnyCanvasView())
            {
                this.dashOffsetProperty.StopAnimation();
            }
        }

        internal static int DashLength =>
            4;

        private PaintDotNet.UI.Media.Brush EffectiveInteriorBrush
        {
            get
            {
                if (!this.useSystemTinting && (this.interiorBrush != null))
                {
                    return this.interiorBrush;
                }
                return SolidColorBrushCache.Get((ColorRgba128Float) ColorBgra.FromColor(Color.FromArgb(0x38, System.Drawing.SystemColors.Highlight)));
            }
        }

        public PaintDotNet.UI.Media.Brush InteriorBrush
        {
            get => 
                this.interiorBrush;
            set
            {
                base.VerifyAccess();
                if (value != this.interiorBrush)
                {
                    this.interiorBrush = value;
                    this.QueueBeginRedraw();
                }
            }
        }

        public bool IsInteriorFilled
        {
            get => 
                this.isInteriorFilled;
            set
            {
                base.VerifyAccess();
                if (value != this.isInteriorFilled)
                {
                    this.isInteriorFilled = value;
                    this.QueueBeginRedraw();
                }
            }
        }

        public bool IsOutlineEnabled
        {
            get => 
                this.isOutlineEnabled;
            set
            {
                base.VerifyAccess();
                if (value != this.isOutlineEnabled)
                {
                    this.isOutlineEnabled = value;
                    this.QueueBeginRedraw();
                }
            }
        }

        public PaintDotNet.Selection Selection
        {
            get
            {
                base.VerifyAccess();
                return this.selection;
            }
            set
            {
                base.VerifyAccess();
                PaintDotNet.Selection selection = this.selection;
                if (value != selection)
                {
                    if (this.selection != null)
                    {
                        this.selection.Changing -= new EventHandler(this.OnSelectionChanging);
                        this.selection.Changed -= new EventHandler<SelectionChangedEventArgs>(this.OnSelectionChanged);
                    }
                    this.selection = value;
                    if (this.selection != null)
                    {
                        this.selection.Changing += new EventHandler(this.OnSelectionChanging);
                        this.selection.Changed += new EventHandler<SelectionChangedEventArgs>(this.OnSelectionChanged);
                    }
                    this.InvalidateSelectionSnapshot();
                }
            }
        }

        public PaintDotNet.Canvas.SelectionSnapshot SelectionSnapshot
        {
            get
            {
                base.VerifyAccess();
                if (this.selectionSnapshot == null)
                {
                    if ((this.selection == null) || this.selection.IsEmpty)
                    {
                        this.selectionSnapshot = emptySelectionSnapshot;
                    }
                    else
                    {
                        Result<GeometryList> cachedLazyGeometryList = this.selection.GetCachedLazyGeometryList();
                        Result<IReadOnlyList<RectInt32>> cachedLazyGeometryListScans = this.selection.GetCachedLazyGeometryListScans();
                        Matrix3x2Double interimTransform = this.selection.GetInterimTransform();
                        RectDouble fastMaxBounds = this.selection.GetFastMaxBounds();
                        bool isEmpty = this.selection.IsEmpty;
                        this.selectionSnapshot = new PaintDotNet.Canvas.SelectionSnapshot(cachedLazyGeometryList, cachedLazyGeometryListScans, interimTransform, fastMaxBounds, isEmpty, this.selectionSnapshotGeometryVersion);
                    }
                }
                return this.selectionSnapshot;
            }
        }

        public bool UseSystemTinting
        {
            get => 
                this.useSystemTinting;
            set
            {
                base.VerifyAccess();
                if (value != this.useSystemTinting)
                {
                    this.useSystemTinting = value;
                    this.QueueBeginRedraw();
                }
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly SelectionCanvasLayer.<>c <>9 = new SelectionCanvasLayer.<>c();
            public static Func<CanvasView, bool> <>9__71_0;

            internal StrokeStyle <.cctor>b__79_0(int dashOffset)
            {
                Validate.IsClamped(dashOffset, 0, 4, "dashOffset");
                StrokeStyle style = new StrokeStyle {
                    StartLineCap = PenLineCap.Flat,
                    EndLineCap = PenLineCap.Flat,
                    DashCap = PenLineCap.Flat,
                    LineJoin = PenLineJoin.Bevel,
                    MiterLimit = StrokeStyle.DefaultMiterLimit,
                    DashStyle = new DashStyle(SelectionCanvasLayer.dashes, -dashOffset * SelectionCanvasLayer.dashMultiplier)
                };
                style.Freeze();
                return style;
            }

            internal bool <IsAnimatedOutlineEnabledForAnyCanvasView>b__71_0(CanvasView cv) => 
                SelectionCanvasLayer.GetIsAnimatedOutlineEnabled(cv);
        }
    }
}

