namespace PaintDotNet.Tools.Controls
{
    using PaintDotNet;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.ObjectModel;
    using PaintDotNet.Rendering;
    using PaintDotNet.Threading;
    using PaintDotNet.UI;
    using PaintDotNet.UI.Controls;
    using PaintDotNet.UI.Input;
    using PaintDotNet.UI.Media;
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Threading;

    internal sealed class TransformControl : Panel
    {
        private OurEditTransformToken activeEditToken;
        public static readonly DependencyProperty AllowBackgroundClickProperty = FrameworkProperty.Register("AllowBackgroundClick", typeof(bool), typeof(TransformControl), new PaintDotNet.UI.FrameworkPropertyMetadata(BooleanUtil.GetBoxed(false)));
        public static readonly DependencyProperty AreScaleHandlesVisibleEffectiveProperty = AreScaleHandlesVisibleEffectivePropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey AreScaleHandlesVisibleEffectivePropertyKey = FrameworkProperty.RegisterReadOnly("AreScaleHandlesVisibleEffective", typeof(bool), typeof(TransformControl), new PaintDotNet.UI.FrameworkPropertyMetadata(BooleanUtil.GetBoxed(true), null, null, new CoerceValueCallback(<>c.<>9.<.cctor>b__193_4)));
        public static readonly DependencyProperty AreScaleHandlesVisibleProperty = FrameworkProperty.Register("AreScaleHandlesVisible", typeof(bool), typeof(TransformControl), new PaintDotNet.UI.FrameworkPropertyMetadata(BooleanUtil.GetBoxed(true), new PropertyChangedCallback(<>c.<>9.<.cctor>b__193_3)));
        public static readonly DependencyProperty BackgroundCursorProperty = FrameworkProperty.Register("BackgroundCursor", typeof(Cursor), typeof(TransformControl), new PaintDotNet.UI.FrameworkPropertyMetadata(Cursors.Arrow));
        public static readonly DependencyProperty BaseBoundsProperty = FrameworkProperty.Register("BaseBounds", typeof(RectDouble), typeof(TransformControl), new PaintDotNet.UI.FrameworkPropertyMetadata(RectDouble.BoxedZero, new PropertyChangedCallback(<>c.<>9.<.cctor>b__193_7)));
        private ContainerTransform baseTransformContainer;
        public static readonly DependencyProperty BaseTransformProperty = FrameworkProperty.Register("BaseTransform", typeof(Transform), typeof(TransformControl), new PaintDotNet.UI.FrameworkPropertyMetadata(Transform.Identity, new PropertyChangedCallback(<>c.<>9.<.cctor>b__193_8), null, Transform.CoerceTransformPropertyCallback));
        private readonly ProtectedRegion beginEditingRegion = new ProtectedRegion("BeginEditing", ProtectedRegionOptions.ErrorOnPerThreadReentrancy);
        private ContainerTransform deltaTransformContainer;
        public static readonly DependencyProperty DeltaTransformProperty = FrameworkProperty.Register("DeltaTransform", typeof(Transform), typeof(TransformControl), new PaintDotNet.UI.FrameworkPropertyMetadata(Transform.Identity, new PropertyChangedCallback(<>c.<>9.<.cctor>b__193_11), new SubPropertyChangedCallback(<>c.<>9.<.cctor>b__193_12), Transform.CoerceTransformPropertyCallback));
        private Matrix3x2Double deltaTxBeforeEditing = Matrix3x2Double.NaN;
        private PointDouble dragBeginPt = PointDouble.PositiveInfinity;
        private PointDouble dragBeginRotationAnchorOffset = PointDouble.PositiveInfinity;
        private PaintDotNet.UI.FrameworkElement dragHandle;
        public static readonly RoutedEvent EditChangedEvent = RoutedEvent.Register("EditChanged", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(TransformControl));
        private int editChangedSuspendCount;
        public static readonly RoutedEvent EditingBeginEvent = RoutedEvent.Register("EditingBegin", RoutingStrategy.Direct, typeof(TransformEditingBeginEventHandler), typeof(TransformControl));
        public static readonly RoutedEvent EditingCancelledEvent = RoutedEvent.Register("EditingCancelled", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(TransformControl));
        public static readonly RoutedEvent EditingFinishedEvent = RoutedEvent.Register("EditingFinished", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(TransformControl));
        public static readonly DependencyProperty EditingModeProperty = EditingModePropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey EditingModePropertyKey = FrameworkProperty.RegisterReadOnly("EditingMode", typeof(TransformEditingMode), typeof(TransformControl), new PaintDotNet.UI.FrameworkPropertyMetadata(EnumUtil.GetBoxed<TransformEditingMode>(TransformEditingMode.None), new PropertyChangedCallback(<>c.<>9.<.cctor>b__193_0)));
        private ContainerTransform editTransformContainer;
        public static readonly DependencyProperty EditTransformProperty = EditTransformPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey EditTransformPropertyKey = FrameworkProperty.RegisterReadOnly("EditTransform", typeof(Transform), typeof(TransformControl), new PaintDotNet.UI.FrameworkPropertyMetadata(Transform.Identity, new PropertyChangedCallback(<>c.<>9.<.cctor>b__193_9), new SubPropertyChangedCallback(<>c.<>9.<.cctor>b__193_10), Transform.CoerceTransformPropertyCallback));
        private TransformGroup finalTransform;
        public static readonly DependencyProperty FinalTransformProperty = FinalTransformPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey FinalTransformPropertyKey = FrameworkProperty.RegisterReadOnly("FinalTransform", typeof(Transform), typeof(TransformControl), new PaintDotNet.UI.FrameworkPropertyMetadata(Transform.Identity, null, null, Transform.CoerceTransformPropertyCallback));
        private Matrix3x2Double finalTxBeforeEditing = Matrix3x2Double.NaN;
        private PaintDotNet.UI.FrameworkElement gestureHandle;
        public static readonly DependencyProperty HairWidthProperty = FrameworkProperty.Register("HairWidth", typeof(double), typeof(TransformControl), new PaintDotNet.UI.FrameworkPropertyMetadata(DoubleUtil.GetBoxed(1.0), PaintDotNet.UI.FrameworkPropertyMetadataOptions.AffectsArrange));
        public static readonly DependencyProperty HitTestPaddingProperty = FrameworkProperty.Register("HitTestPadding", typeof(double), typeof(TransformControl), new PaintDotNet.UI.FrameworkPropertyMetadata(DoubleUtil.GetBoxed(0.0), PaintDotNet.UI.FrameworkPropertyMetadataOptions.AffectsArrange));
        private bool internalMaySetDeltaTransform;
        public static readonly DependencyProperty IsEditingProperty = IsEditingPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey IsEditingPropertyKey = FrameworkProperty.RegisterReadOnly("IsEditing", typeof(bool), typeof(TransformControl), new PaintDotNet.UI.FrameworkPropertyMetadata(BooleanUtil.GetBoxed(false), new PropertyChangedCallback(<>c.<>9.<.cctor>b__193_1), null, new CoerceValueCallback(<>c.<>9.<.cctor>b__193_2)));
        public static readonly DependencyProperty IsInputPixelSnappingEnabledProperty = FrameworkProperty.Register("IsInputPixelSnappingEnabled", typeof(bool), typeof(TransformControl), new PaintDotNet.UI.FrameworkPropertyMetadata(BooleanUtil.GetBoxed(true)));
        private bool isPreparingToEdit;
        public static readonly DependencyProperty IsRotationAnchorVisibleEffectiveProperty = IsRotationAnchorVisibleEffectivePropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey IsRotationAnchorVisibleEffectivePropertyKey = FrameworkProperty.RegisterReadOnly("IsRotationAnchorVisibleEffective", typeof(bool), typeof(TransformControl), new PaintDotNet.UI.FrameworkPropertyMetadata(BooleanUtil.GetBoxed(true), null, null, new CoerceValueCallback(<>c.<>9.<.cctor>b__193_6)));
        public static readonly DependencyProperty IsRotationAnchorVisibleProperty = FrameworkProperty.Register("IsRotationAnchorVisible", typeof(bool), typeof(TransformControl), new PaintDotNet.UI.FrameworkPropertyMetadata(BooleanUtil.GetBoxed(true), new PropertyChangedCallback(<>c.<>9.<.cctor>b__193_5)));
        private static readonly DependencyProperty IsScaleTransformProperty = DependencyProperty.RegisterAttached("IsScaleTransform", typeof(bool), typeof(TransformControl), new PropertyMetadata(BooleanUtil.GetBoxed(false)));
        private DispatcherTimer keyboardEditTimer;
        private EditTransformToken keyboardEditToken;
        private static readonly object keyboardEditTokenTag = (typeof(TransformControl).FullName + ".KeyboardEditTokenTag");
        private PointDouble latestDragMovePt = PointDouble.PositiveInfinity;
        public const double MinScale = 1E-07;
        private EditTransformToken mouseEditToken;
        private static readonly object mouseEditTokenTag = (typeof(TransformControl).FullName + ".MouseEditTokenTag");
        private readonly ProtectedRegion onHandleGestureEventRegion = new ProtectedRegion("OnHandleGestureEvent", ProtectedRegionOptions.ErrorOnPerThreadReentrancy);
        private TransformHandlePanel panel;
        public static readonly DependencyProperty RotateBoxClipProperty = FrameworkProperty.Register("RotateBoxClip", typeof(Geometry), typeof(TransformControl), new PaintDotNet.UI.FrameworkPropertyMetadata(Geometry.Infinite, null, null, Geometry.CoerceGeometryPropertyInfiniteCallback));
        public static readonly DependencyProperty RotateBoxClipToBoundsProperty = FrameworkProperty.Register("RotateBoxClipToBounds", typeof(bool), typeof(TransformControl), new PaintDotNet.UI.FrameworkPropertyMetadata(BooleanUtil.GetBoxed(true)));
        public static readonly DependencyProperty RotationAnchorOffsetProperty = FrameworkProperty.Register("RotationAnchorOffset", typeof(PointDouble?), typeof(TransformControl), new PaintDotNet.UI.FrameworkPropertyMetadata(null, PaintDotNet.UI.FrameworkPropertyMetadataOptions.AffectsArrange, new PropertyChangedCallback(<>c.<>9.<.cctor>b__193_13)), ValidateValueCallbacks.NullablePointDoubleIsNullOrFinite);
        private PointDouble rotationCenter = PointDouble.PositiveInfinity;
        private int suppressedEditChangedEventCount;
        public static readonly DependencyProperty TransformHandleTypeProperty = TransformHandlePanel.TransformHandleTypeProperty;
        public static readonly DependencyProperty TranslateBoxClipProperty = FrameworkProperty.Register("TranslateBoxClip", typeof(Geometry), typeof(TransformControl), new PaintDotNet.UI.FrameworkPropertyMetadata(Geometry.Infinite, null, null, Geometry.CoerceGeometryPropertyInfiniteCallback));
        public static readonly DependencyProperty TranslateBoxClipToBoundsProperty = FrameworkProperty.Register("TranslateBoxClipToBounds", typeof(bool), typeof(TransformControl), new PaintDotNet.UI.FrameworkPropertyMetadata(BooleanUtil.GetBoxed(true)));
        public static readonly DependencyProperty TranslateCursorProperty = FrameworkProperty.Register("TranslateCursor", typeof(Cursor), typeof(TransformControl), new PaintDotNet.UI.FrameworkPropertyMetadata(Cursors.SizeAll));

        public event RoutedEventHandler EditChanged
        {
            add
            {
                base.AddHandler(EditChangedEvent, value);
            }
            remove
            {
                base.RemoveHandler(EditChangedEvent, value);
            }
        }

        public event TransformEditingBeginEventHandler EditingBegin
        {
            add
            {
                base.AddHandler(EditingBeginEvent, value);
            }
            remove
            {
                base.RemoveHandler(EditingBeginEvent, value);
            }
        }

        public event RoutedEventHandler EditingCancelled
        {
            add
            {
                base.AddHandler(EditingCancelledEvent, value);
            }
            remove
            {
                base.RemoveHandler(EditingCancelledEvent, value);
            }
        }

        public event RoutedEventHandler EditingFinished
        {
            add
            {
                base.AddHandler(EditingFinishedEvent, value);
            }
            remove
            {
                base.RemoveHandler(EditingFinishedEvent, value);
            }
        }

        public TransformControl()
        {
            base.ClipToBounds = false;
            base.Focusable = true;
            this.finalTransform = new TransformGroup();
            this.baseTransformContainer = new ContainerTransform();
            this.baseTransformContainer.SetBinding(ContainerTransform.TransformProperty, this, new PaintDotNet.ObjectModel.PropertyPath(BaseTransformProperty), BindingMode.OneWay);
            this.finalTransform.Children.Add(this.baseTransformContainer);
            this.deltaTransformContainer = new ContainerTransform();
            this.deltaTransformContainer.SetBinding(ContainerTransform.TransformProperty, this, new PaintDotNet.ObjectModel.PropertyPath(DeltaTransformProperty), BindingMode.OneWay);
            this.finalTransform.Children.Add(this.deltaTransformContainer);
            this.editTransformContainer = new ContainerTransform();
            this.editTransformContainer.SetBinding(ContainerTransform.TransformProperty, this, new PaintDotNet.ObjectModel.PropertyPath(EditTransformProperty), BindingMode.OneWay);
            this.finalTransform.Children.Add(this.editTransformContainer);
            this.finalTransform.Changed += new EventHandler(this.OnInternalFinalTransformChanged);
            this.panel = new TransformHandlePanel();
            this.panel.SetBinding(TransformHandlePanel.AllowBackgroundClickProperty, this, new PaintDotNet.ObjectModel.PropertyPath(AllowBackgroundClickProperty), BindingMode.OneWay);
            this.panel.SetBinding(TransformHandlePanel.BackgroundCursorProperty, this, new PaintDotNet.ObjectModel.PropertyPath(BackgroundCursorProperty), BindingMode.OneWay);
            this.panel.SetBinding(TransformHandlePanel.TranslateCursorProperty, this, new PaintDotNet.ObjectModel.PropertyPath(TranslateCursorProperty), BindingMode.OneWay);
            this.panel.SetBinding(TransformHandlePanel.TranslateBoxClipProperty, this, new PaintDotNet.ObjectModel.PropertyPath(TranslateBoxClipProperty), BindingMode.OneWay);
            this.panel.SetBinding(TransformHandlePanel.RotateBoxClipProperty, this, new PaintDotNet.ObjectModel.PropertyPath(RotateBoxClipProperty), BindingMode.OneWay);
            this.panel.SetBinding(TransformHandlePanel.RotateBoxClipToBoundsProperty, this, new PaintDotNet.ObjectModel.PropertyPath(RotateBoxClipToBoundsProperty), BindingMode.OneWay);
            this.panel.SetBinding(TransformHandlePanel.TranslateBoxClipToBoundsProperty, this, new PaintDotNet.ObjectModel.PropertyPath(TranslateBoxClipToBoundsProperty), BindingMode.OneWay);
            this.panel.SetBinding(TransformHandlePanel.AreScaleHandlesVisibleProperty, this, new PaintDotNet.ObjectModel.PropertyPath(AreScaleHandlesVisibleEffectiveProperty), BindingMode.OneWay);
            this.panel.SetBinding(TransformHandlePanel.IsRotationAnchorVisibleProperty, this, new PaintDotNet.ObjectModel.PropertyPath(IsRotationAnchorVisibleEffectiveProperty), BindingMode.OneWay);
            this.panel.SetBinding(TransformHandlePanel.BaseBoundsProperty, this, new PaintDotNet.ObjectModel.PropertyPath(BaseBoundsProperty), BindingMode.OneWay);
            this.panel.SetBinding(TransformHandlePanel.TransformProperty, this, new PaintDotNet.ObjectModel.PropertyPath(FinalTransformProperty), BindingMode.OneWay);
            this.panel.SetBinding(TransformHandlePanel.HairWidthProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HairWidthProperty), BindingMode.OneWay);
            this.panel.SetBinding(TransformHandlePanel.HitTestPaddingProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HitTestPaddingProperty), BindingMode.OneWay);
            this.panel.SetBinding(TransformHandlePanel.RotationAnchorOffsetProperty, this, new PaintDotNet.ObjectModel.PropertyPath(RotationAnchorOffsetProperty), BindingMode.TwoWay);
            this.panel.SetBinding<TransformEditingMode, Visibility?>(TransformHandlePanel.RotationIndicatorVisibilityProperty, this, new PaintDotNet.ObjectModel.PropertyPath(EditingModeProperty), BindingMode.OneWay, delegate (TransformEditingMode em) {
                if (em != TransformEditingMode.Rotate)
                {
                    return null;
                }
                return 0;
            });
            base.Children.Add(this.panel);
            base.AddHandler(ClickDragBehavior.GestureBeginEvent, new MouseEventHandler(this.OnHandleGestureBegin));
            base.AddHandler(ClickDragBehavior.ClickedEvent, new MouseEventHandler(this.OnHandleClicked));
            base.AddHandler(ClickDragBehavior.DragBeginEvent, new MouseEventHandler(this.OnHandleDragBegin));
            base.AddHandler(ClickDragBehavior.DragMoveEvent, new MouseEventHandler(this.OnHandleDragMove));
            base.AddHandler(ClickDragBehavior.DragEndEvent, new MouseEventHandler(this.OnHandleDragEnd));
            base.AddHandler(ClickDragBehavior.GestureEndEvent, new RoutedEventHandler(this.OnHandleGestureEnd));
            this.UpdateFinalTransform();
            base.Unloaded += new EventHandler(this.OnUnloaded);
        }

        private void AreScaleHandlesVisiblePropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.CoerceValue(AreScaleHandlesVisibleEffectiveProperty);
        }

        protected override SizeDouble ArrangeOverride(SizeDouble finalSize)
        {
            this.panel.Arrange(new RectDouble(PointDouble.Zero, finalSize));
            return base.ArrangeOverride(finalSize);
        }

        private void BaseBoundsPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (this.IsEditing)
            {
                throw new InvalidOperationException("BaseBounds may only be set when IsEditing = false");
            }
        }

        private void BaseTransformPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (this.IsEditing)
            {
                throw new InvalidOperationException("BaseTransform may not be set when IsEditing = true");
            }
        }

        public void BeginEditChanges()
        {
            base.VerifyAccess();
            this.editChangedSuspendCount++;
        }

        private void BeginPreparingToEdit()
        {
            base.VerifyAccess();
            this.isPreparingToEdit = true;
        }

        private void CancelEditing(OurEditTransformToken sourceToken)
        {
            Validate.IsNotNull<OurEditTransformToken>(sourceToken, "sourceToken");
            base.VerifyAccess();
            if (sourceToken != this.activeEditToken)
            {
                throw new InvalidOperationException("can only use an EditTransformToken that is active");
            }
            if (!this.IsEditing)
            {
                throw new InvalidOperationException("May only call CancelEditing() when IsEditing = true");
            }
            this.BeginEditChanges();
            this.RotationAnchorOffset = sourceToken.OldRotationAnchorOffset;
            base.ClearValue(EditTransformPropertyKey);
            base.CoerceValue(EditTransformProperty);
            OurEditTransformToken activeEditToken = this.activeEditToken;
            this.activeEditToken = null;
            activeEditToken.NotifyDeactivated();
            this.EndEditChanges(true);
            this.EditingMode = TransformEditingMode.None;
            base.RaiseEvent(new RoutedEventArgs(EditingCancelledEvent, this));
        }

        private object CoerceAreScaleHandlesVisibleEffectiveProperty(object baseValue)
        {
            if (!this.AreScaleHandlesVisible)
            {
                return BooleanUtil.GetBoxed(false);
            }
            TransformEditingMode editingMode = this.EditingMode;
            switch (editingMode)
            {
                case TransformEditingMode.None:
                case TransformEditingMode.Scale:
                    return BooleanUtil.GetBoxed(true);

                case TransformEditingMode.Rotate:
                case TransformEditingMode.Translate:
                case TransformEditingMode.MoveRotationAnchor:
                case TransformEditingMode.Custom:
                    return BooleanUtil.GetBoxed(false);
            }
            throw ExceptionUtil.InvalidEnumArgumentException<TransformEditingMode>(editingMode, "editingMode");
        }

        private object CoerceIsEditingProperty(object baseValue)
        {
            if (this.EditingMode == TransformEditingMode.None)
            {
                return BooleanUtil.GetBoxed(false);
            }
            return BooleanUtil.GetBoxed(true);
        }

        private object CoerceIsRotationAnchorVisibleEffectiveProperty(object baseValue)
        {
            if (!this.IsRotationAnchorVisible)
            {
                return BooleanUtil.GetBoxed(false);
            }
            TransformEditingMode editingMode = this.EditingMode;
            switch (editingMode)
            {
                case TransformEditingMode.None:
                case TransformEditingMode.Rotate:
                case TransformEditingMode.MoveRotationAnchor:
                    return BooleanUtil.GetBoxed(true);

                case TransformEditingMode.Scale:
                case TransformEditingMode.Translate:
                case TransformEditingMode.Custom:
                    return BooleanUtil.GetBoxed(false);
            }
            throw ExceptionUtil.InvalidEnumArgumentException<TransformEditingMode>(editingMode, "editingMode");
        }

        private void CommitEdits(OurEditTransformToken sourceToken)
        {
            Validate.IsNotNull<OurEditTransformToken>(sourceToken, "sourceToken");
            base.VerifyAccess();
            if (sourceToken != this.activeEditToken)
            {
                throw new InvalidOperationException("can only use an EditTransformToken that is active and that was returned from BeginEditing()");
            }
            this.BeginEditChanges();
            Transform transform = this.EditTransform.ToFrozen<Transform>();
            base.ClearValue(EditTransformPropertyKey);
            base.CoerceValue(EditTransformProperty);
            Matrix3x2Double deltaTx = this.DeltaTransform.Value * transform.Value;
            this.SetDeltaTransform(deltaTx);
            OurEditTransformToken activeEditToken = this.activeEditToken;
            this.activeEditToken = null;
            activeEditToken.NotifyDeactivated();
            this.EndEditChanges(false);
            this.EditingMode = TransformEditingMode.None;
            base.RaiseEvent(new RoutedEventArgs(EditingFinishedEvent, this));
        }

        private static double ConstrainAngle(double angle)
        {
            double num6;
            while (angle < 0.0)
            {
                angle += 360.0;
            }
            int num = (int) angle;
            int num2 = (num / 15) * 15;
            int num3 = num2 + 15;
            double num4 = Math.Abs((double) (angle - num2));
            double num5 = Math.Abs((double) (angle - num3));
            if (num4 < num5)
            {
                num6 = num2;
            }
            else
            {
                num6 = num3;
            }
            if (num6 > 180.0)
            {
                num6 -= 360.0;
            }
            return num6;
        }

        private void DeltaTransformPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            this.OnDeltaTransformChanged();
        }

        private void DeltaTransformSubPropertyChanged(DependencyProperty property)
        {
            this.OnDeltaTransformChanged();
        }

        private void EditingModePropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.CoerceValue(IsEditingProperty);
            base.CoerceValue(AreScaleHandlesVisibleEffectiveProperty);
            base.CoerceValue(IsRotationAnchorVisibleEffectiveProperty);
        }

        private void EditTransformPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!this.IsEditing && !this.isPreparingToEdit)
            {
                throw new InvalidOperationException("EditTransform may only be set when IsEditing = true");
            }
            this.OnEditTransformPropertyChanged();
        }

        private void EditTransformSubPropertyChanged(DependencyProperty property)
        {
            this.OnEditTransformPropertyChanged();
        }

        public void EndEditChanges()
        {
            this.EndEditChanges(true);
        }

        public void EndEditChanges(bool raiseEvent)
        {
            base.VerifyAccess();
            this.editChangedSuspendCount--;
            if ((raiseEvent && this.IsEditing) && ((this.editChangedSuspendCount == 0) && (this.suppressedEditChangedEventCount != 0)))
            {
                this.RaiseEditChangedEvent();
            }
        }

        private void EndPreparingToEdit()
        {
            base.VerifyAccess();
            this.isPreparingToEdit = false;
        }

        private static double EpsilonFloor(double x, double epsilon)
        {
            double num = Math.Floor(x);
            double num2 = Math.Ceiling(x);
            if (Math.Abs((double) (x - num2)) < epsilon)
            {
                return num2;
            }
            return num;
        }

        private static void GetEdgeScalers(TransformHandleType scaleHandleType, out int leftScale, out int topScale, out int rightScale, out int bottomScale)
        {
            switch (scaleHandleType)
            {
                case TransformHandleType.ScaleEdgeW:
                case TransformHandleType.ScaleW:
                    leftScale = 1;
                    topScale = 0;
                    rightScale = 0;
                    bottomScale = 0;
                    return;

                case TransformHandleType.ScaleEdgeN:
                case TransformHandleType.ScaleN:
                    leftScale = 0;
                    topScale = 1;
                    rightScale = 0;
                    bottomScale = 0;
                    return;

                case TransformHandleType.ScaleEdgeE:
                case TransformHandleType.ScaleE:
                    leftScale = 0;
                    topScale = 0;
                    rightScale = 1;
                    bottomScale = 0;
                    return;

                case TransformHandleType.ScaleEdgeS:
                case TransformHandleType.ScaleS:
                    leftScale = 0;
                    topScale = 0;
                    rightScale = 0;
                    bottomScale = 1;
                    return;

                case TransformHandleType.ScaleNW:
                    leftScale = 1;
                    topScale = 1;
                    rightScale = 0;
                    bottomScale = 0;
                    return;

                case TransformHandleType.ScaleNE:
                    leftScale = 0;
                    topScale = 1;
                    rightScale = 1;
                    bottomScale = 0;
                    return;

                case TransformHandleType.ScaleSW:
                    leftScale = 1;
                    topScale = 0;
                    rightScale = 0;
                    bottomScale = 1;
                    return;

                case TransformHandleType.ScaleSE:
                    leftScale = 0;
                    topScale = 0;
                    rightScale = 1;
                    bottomScale = 1;
                    return;
            }
            throw ExceptionUtil.InvalidEnumArgumentException<TransformHandleType>(scaleHandleType, "scaleHandleType");
        }

        private static bool GetIsScaleTransform(Transform transform) => 
            ((bool) transform.GetValue(IsScaleTransformProperty));

        private static PointDouble GetScaleHandleUnitOffset(TransformHandleType scaleHandleType)
        {
            VectorDouble scaleHandleDirectionVector = TransformHandlePanel.GetScaleHandleDirectionVector(scaleHandleType);
            return new PointDouble((scaleHandleDirectionVector.X + 1.0) / 2.0, (scaleHandleDirectionVector.Y + 1.0) / 2.0);
        }

        public static TransformHandleType GetTransformHandleType(UIElement target) => 
            TransformHandlePanel.GetTransformHandleType(target);

        private void IsEditingPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (this.isPreparingToEdit)
            {
                throw new InternalErrorException("Must call EndPreparingToEdit() before setting IsEditing to true");
            }
        }

        private void IsRotationAnchorVisiblePropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.CoerceValue(IsRotationAnchorVisibleEffectiveProperty);
        }

        protected override SizeDouble MeasureOverride(SizeDouble availableSize)
        {
            this.panel.Measure(availableSize);
            return base.MeasureOverride(availableSize);
        }

        private void NotifyTokenDeactivated(OurEditTransformToken token)
        {
            if (token.Tag == keyboardEditTokenTag)
            {
                this.keyboardEditToken = null;
                this.keyboardEditTimer.Stop();
                this.keyboardEditTimer = null;
            }
            else if (token.Tag == mouseEditTokenTag)
            {
                this.mouseEditToken = null;
                this.dragHandle = null;
                this.finalTxBeforeEditing = Matrix3x2Double.NaN;
                this.deltaTxBeforeEditing = Matrix3x2Double.NaN;
                this.dragBeginPt = PointDouble.PositiveInfinity;
                this.latestDragMovePt = PointDouble.PositiveInfinity;
                this.rotationCenter = PointDouble.PositiveInfinity;
                this.dragBeginRotationAnchorOffset = PointDouble.PositiveInfinity;
            }
        }

        private void OnDeltaTransformChanged()
        {
            if (this.IsEditing && !this.internalMaySetDeltaTransform)
            {
                throw new InvalidOperationException("DeltaTransform may only be set when IsEditing is false");
            }
            this.UpdateFinalTransform();
        }

        private void OnEditTransformPropertyChanged()
        {
            this.UpdateFinalTransform();
            this.RaiseEditChangedEvent();
        }

        private void OnHandleClicked(object sender, MouseEventArgs e)
        {
            using (this.onHandleGestureEventRegion.UseEnterScope())
            {
                PaintDotNet.UI.FrameworkElement source = (PaintDotNet.UI.FrameworkElement) e.Source;
                if ((GetTransformHandleType(source) == TransformHandleType.RotationAnchorResetButton) && !this.IsEditing)
                {
                    EditTransformToken token = this.TryBeginEditing(TransformEditingMode.MoveRotationAnchor, 3);
                    if (token != null)
                    {
                        token.RotationAnchorOffset = null;
                        token.Commit();
                    }
                }
            }
        }

        private void OnHandleDragBegin(object sender, MouseEventArgs e)
        {
            using (this.onHandleGestureEventRegion.UseEnterScope())
            {
                TransformEditingMode rotate;
                TransformHandleType? nullable;
                EditTransformToken token;
                PaintDotNet.UI.FrameworkElement source = (PaintDotNet.UI.FrameworkElement) e.Source;
                TransformHandleType transformHandleType = GetTransformHandleType(source);
                if (this.IsEditing && (this.activeEditToken == this.keyboardEditToken))
                {
                    this.keyboardEditToken.Commit();
                }
                if (e.RightButton == MouseButtonState.Pressed)
                {
                    rotate = TransformEditingMode.Rotate;
                    nullable = null;
                }
                else
                {
                    nullable = new TransformHandleType?(transformHandleType);
                    switch (transformHandleType)
                    {
                        case TransformHandleType.Background:
                        case TransformHandleType.TranslateBox:
                        case TransformHandleType.TranslateHandle:
                            rotate = TransformEditingMode.Translate;
                            goto Label_00C5;

                        case TransformHandleType.RotateBox:
                            rotate = TransformEditingMode.Rotate;
                            goto Label_00C5;

                        case TransformHandleType.RotationAnchorResetButton:
                            throw new InternalErrorException();

                        case TransformHandleType.RotationAnchor:
                            rotate = TransformEditingMode.MoveRotationAnchor;
                            goto Label_00C5;

                        case TransformHandleType.ScaleEdgeW:
                        case TransformHandleType.ScaleEdgeN:
                        case TransformHandleType.ScaleEdgeE:
                        case TransformHandleType.ScaleEdgeS:
                        case TransformHandleType.ScaleW:
                        case TransformHandleType.ScaleN:
                        case TransformHandleType.ScaleE:
                        case TransformHandleType.ScaleS:
                        case TransformHandleType.ScaleNW:
                        case TransformHandleType.ScaleNE:
                        case TransformHandleType.ScaleSW:
                        case TransformHandleType.ScaleSE:
                            rotate = TransformEditingMode.Scale;
                            goto Label_00C5;
                    }
                    rotate = TransformEditingMode.Translate;
                }
            Label_00C5:
                token = this.TryBeginEditing(rotate, nullable);
                if (token != null)
                {
                    token.Tag = mouseEditTokenTag;
                    this.dragHandle = source;
                    this.dragBeginPt = e.GetPosition(this);
                    this.latestDragMovePt = this.dragBeginPt;
                    Matrix3x2Double num = this.DeltaTransform.Value;
                    this.deltaTxBeforeEditing = num;
                    Matrix3x2Double matrix = this.FinalTransform.Value;
                    this.finalTxBeforeEditing = matrix;
                    switch (rotate)
                    {
                        case TransformEditingMode.Rotate:
                        {
                            PointDouble center = this.BaseBounds.Center;
                            PointDouble effectiveRotationAnchorOffset = this.panel.EffectiveRotationAnchorOffset;
                            this.rotationCenter = effectiveRotationAnchorOffset;
                            token.EditTransform = new RotateTransform(0.0, effectiveRotationAnchorOffset.X, effectiveRotationAnchorOffset.Y);
                            break;
                        }
                        case TransformEditingMode.Scale:
                        {
                            TransformGroup group = new TransformGroup();
                            Transform item = new MatrixTransform(matrix.Inverse).EnsureFrozen<MatrixTransform>();
                            group.Children.Add(item);
                            RectDouble baseBounds = this.BaseBounds;
                            PointDouble scaleHandleUnitOffset = GetScaleHandleUnitOffset(transformHandleType);
                            PointDouble num8 = new PointDouble(baseBounds.X + ((1.0 - scaleHandleUnitOffset.X) * baseBounds.Width), baseBounds.Y + ((1.0 - scaleHandleUnitOffset.Y) * baseBounds.Height));
                            TranslateTransform transform2 = new TranslateTransform(-num8.X, -num8.Y);
                            group.Children.Add(transform2);
                            ScaleTransform transform = new ScaleTransform();
                            SetIsScaleTransform(transform, true);
                            group.Children.Add(transform);
                            group.Children.Add(new TranslateTransform(num8.X, num8.Y));
                            group.Children.Add(new MatrixTransform(matrix).EnsureFrozen<MatrixTransform>());
                            token.EditTransform = group;
                            break;
                        }
                        case TransformEditingMode.Translate:
                            token.EditTransform = new TranslateTransform();
                            break;

                        case TransformEditingMode.MoveRotationAnchor:
                            this.dragBeginRotationAnchorOffset = this.panel.EffectiveRotationAnchorOffset;
                            break;

                        default:
                            throw new InternalErrorException(ExceptionUtil.InvalidEnumArgumentException<TransformEditingMode>(rotate, "editMode"));
                    }
                    this.mouseEditToken = token;
                }
            }
        }

        private void OnHandleDragEnd(object sender, MouseEventArgs e)
        {
            using (this.onHandleGestureEventRegion.UseEnterScope())
            {
                if (this.mouseEditToken != null)
                {
                    this.mouseEditToken.Commit();
                }
            }
        }

        private void OnHandleDragMove(PointDouble dragMovePt)
        {
            if (this.mouseEditToken != null)
            {
                RotateTransform editTransform;
                double num10;
                this.latestDragMovePt = dragMovePt;
                bool flag = (base.GetKeyboardDevice().Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
                TransformHandleType transformHandleType = GetTransformHandleType(this.dragHandle);
                VectorDouble num = (VectorDouble) (dragMovePt - this.dragBeginPt);
                double x = Math.Truncate(num.X);
                VectorDouble num2 = new VectorDouble(x, Math.Truncate(num.Y));
                VectorDouble num3 = this.IsInputPixelSnappingEnabled ? num2 : num;
                switch (this.mouseEditToken.EditingMode)
                {
                    case TransformEditingMode.Rotate:
                    {
                        editTransform = (RotateTransform) this.EditTransform;
                        VectorDouble num4 = (VectorDouble) (this.dragBeginPt - this.rotationCenter);
                        VectorDouble num5 = (VectorDouble) (dragMovePt - this.rotationCenter);
                        double num6 = Math.Atan2(num4.Y, num4.X);
                        double radians = Math.Atan2(num5.Y, num5.X) - num6;
                        double num9 = MathUtil.RadiansToDegrees(radians);
                        if (!flag)
                        {
                            num10 = num9;
                            break;
                        }
                        Matrix3x2Double matrix = this.BaseTransform.Value * this.deltaTxBeforeEditing;
                        double angle = matrix.GetRotationAngle() + num9;
                        double num15 = ConstrainAngle(angle) - angle;
                        num10 = num9 + num15;
                        break;
                    }
                    case TransformEditingMode.Scale:
                    {
                        int num22;
                        int num23;
                        int num24;
                        int num25;
                        double num39;
                        double num40;
                        Matrix3x2Double inverse = this.finalTxBeforeEditing.Inverse;
                        TransformGroup group = (TransformGroup) this.EditTransform;
                        ScaleTransform transform3 = (ScaleTransform) group.Children.First<Transform>(tx => GetIsScaleTransform(tx));
                        RectDouble baseBounds = this.BaseBounds;
                        PointDouble num19 = inverse.Transform(this.dragBeginPt);
                        VectorDouble num21 = (VectorDouble) (inverse.Transform(dragMovePt) - num19);
                        GetEdgeScalers(transformHandleType, out num22, out num23, out num24, out num25);
                        double num26 = num22 * num21.X;
                        double num27 = num23 * num21.Y;
                        double num28 = num24 * num21.X;
                        double num29 = num25 * num21.Y;
                        RectDouble num30 = RectDouble.FromEdges(baseBounds.Left + num26, baseBounds.Top + num27, baseBounds.Right + num28, baseBounds.Bottom + num29);
                        double num31 = num30.Width / baseBounds.Width;
                        double num32 = num30.Height / baseBounds.Height;
                        double num33 = num31;
                        double num34 = num32;
                        if (flag && baseBounds.HasPositiveArea)
                        {
                            double num46 = (Math.Max(num22, num24) == 0) ? double.PositiveInfinity : 1.0;
                            double num48 = (Math.Max(num23, num25) == 0) ? double.PositiveInfinity : 1.0;
                            double num49 = baseBounds.Width / baseBounds.Height;
                            VectorDouble num51 = VectorDouble.Abs(this.finalTxBeforeEditing.GetScale());
                            double num52 = baseBounds.Width * num51.X;
                            double num53 = baseBounds.Height * num51.Y;
                            double num54 = num52 * num31;
                            double num55 = num53 * num32;
                            double num56 = (num54 / baseBounds.Width) * num46;
                            double num57 = (num55 / baseBounds.Height) * num48;
                            double num58 = Math.Min(num56, num57);
                            double num59 = baseBounds.Width * num58;
                            double num60 = baseBounds.Height * num58;
                            num33 = num59 / num52;
                            num34 = num60 / num53;
                        }
                        double num35 = num33;
                        double num36 = num34;
                        if (this.IsInputPixelSnappingEnabled)
                        {
                            VectorDouble num62 = VectorDouble.Abs(this.finalTxBeforeEditing.GetScale());
                            int num63 = num22 + num24;
                            int num64 = num23 + num25;
                            if (baseBounds.Width != 0.0)
                            {
                                int num65 = (flag && (num63 == 0)) ? 2 : 1;
                                double num66 = baseBounds.Width * num62.X;
                                int num67 = (int) Math.Round(num66, MidpointRounding.AwayFromZero);
                                int num68 = num67 & (num65 - 1);
                                double num69 = ((double) num65) / num66;
                                num35 = (EpsilonFloor(num33 / num69, 0.01) + (((double) num68) / 2.0)) * num69;
                            }
                            if (baseBounds.Height != 0.0)
                            {
                                int num70 = (flag && (num64 == 0)) ? 2 : 1;
                                double num71 = baseBounds.Height * num62.Y;
                                int num72 = (int) Math.Round(num71, MidpointRounding.AwayFromZero);
                                int num73 = num72 & (num70 - 1);
                                double num74 = ((double) num70) / num71;
                                num36 = (EpsilonFloor(num34 / num74, 0.01) + (((double) num73) / 2.0)) * num74;
                            }
                        }
                        double num37 = Math.Max(1E-07, num35);
                        double num38 = Math.Max(1E-07, num36);
                        if (((num22 != 1.0) || (num30.Left < baseBounds.Right)) && ((num24 != 1.0) || (num30.Right > baseBounds.Left)))
                        {
                            num39 = 1.0;
                        }
                        else
                        {
                            num39 = -1.0;
                        }
                        if (((num23 == 1.0) && (num30.Top >= baseBounds.Bottom)) || ((num25 == 1.0) && (num30.Bottom <= baseBounds.Top)))
                        {
                            num40 = -1.0;
                        }
                        else
                        {
                            num40 = 1.0;
                        }
                        double num41 = num37 * num39;
                        double num42 = num38 * num40;
                        double num43 = num41;
                        double num44 = num42;
                        transform3.ScaleX = num43;
                        transform3.ScaleY = num44;
                        return;
                    }
                    case TransformEditingMode.Translate:
                    {
                        TranslateTransform transform = (TranslateTransform) this.EditTransform;
                        transform.X = num3.X;
                        transform.Y = num3.Y;
                        return;
                    }
                    case TransformEditingMode.MoveRotationAnchor:
                    {
                        PointDouble num16 = new PointDouble(this.dragBeginRotationAnchorOffset.X + num.X, this.dragBeginRotationAnchorOffset.Y + num.Y);
                        this.panel.RotationAnchorOffset = new PointDouble?(num16);
                        return;
                    }
                    default:
                        throw new InternalErrorException(ExceptionUtil.InvalidEnumArgumentException<TransformEditingMode>(this.mouseEditToken.EditingMode, "this.mouseEditToken.EditingMode"));
                }
                editTransform.Angle = num10;
            }
        }

        private void OnHandleDragMove(object sender, MouseEventArgs e)
        {
            using (this.onHandleGestureEventRegion.UseEnterScope())
            {
                PaintDotNet.UI.FrameworkElement source = (PaintDotNet.UI.FrameworkElement) e.Source;
                TransformHandleType transformHandleType = GetTransformHandleType(source);
                PointDouble position = e.GetPosition(this);
                this.OnHandleDragMove(position);
            }
        }

        private void OnHandleGestureBegin(object sender, MouseEventArgs e)
        {
            using (this.onHandleGestureEventRegion.UseEnterScope())
            {
                PaintDotNet.UI.FrameworkElement source = (PaintDotNet.UI.FrameworkElement) e.Source;
                if (this.keyboardEditToken != null)
                {
                    this.keyboardEditToken.Commit();
                }
                if (this.mouseEditToken != null)
                {
                    this.mouseEditToken.Commit();
                }
                this.gestureHandle = source;
            }
        }

        private void OnHandleGestureEnd(object sender, RoutedEventArgs e)
        {
            using (this.onHandleGestureEventRegion.UseEnterScope())
            {
                PaintDotNet.UI.FrameworkElement source = (PaintDotNet.UI.FrameworkElement) e.Source;
                this.gestureHandle = null;
            }
        }

        private void OnInternalFinalTransformChanged(object sender, EventArgs e)
        {
            this.UpdateFinalTransform();
        }

        private void OnKeyboardEditTimerTick(object sender, EventArgs e)
        {
            if (this.keyboardEditToken != null)
            {
                this.keyboardEditToken.Commit();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            this.OnKeyUpOrDown(e);
            if (!e.Handled)
            {
                switch (e.Key)
                {
                    case Key.Left:
                    case Key.Up:
                    case Key.Right:
                    case Key.Down:
                        if (!this.IsEditing || (this.activeEditToken != this.keyboardEditToken))
                        {
                            if (!this.IsEditing && base.IsVisible)
                            {
                                this.keyboardEditToken = this.TryBeginEditing(TransformEditingMode.Translate);
                                if (this.keyboardEditToken != null)
                                {
                                    this.keyboardEditToken.Tag = keyboardEditTokenTag;
                                    this.keyboardEditToken.EditTransform = new TranslateTransform();
                                    int num2 = (PaintDotNet.UI.SystemParameters.KeyboardDelayMs * 3) / 2;
                                    int num3 = Math.Max(500, num2);
                                    this.keyboardEditTimer = new DispatcherTimer(TimeSpan.FromMilliseconds((double) num3), DispatcherPriority.Loaded, new EventHandler(this.OnKeyboardEditTimerTick), base.Dispatcher);
                                    this.ProcessArrowKey(e.Key, e.KeyboardDevice.Modifiers);
                                    e.Handled = true;
                                }
                            }
                            break;
                        }
                        this.ProcessArrowKey(e.Key, e.KeyboardDevice.Modifiers);
                        e.Handled = true;
                        break;

                    case Key.Escape:
                        if (this.mouseEditToken != null)
                        {
                            this.mouseEditToken.Cancel();
                            e.Handled = true;
                        }
                        break;
                }
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            this.OnKeyUpOrDown(e);
            base.OnKeyUp(e);
        }

        private void OnKeyUpOrDown(KeyEventArgs e)
        {
            if (!e.Handled && (this.mouseEditToken != null))
            {
                this.OnHandleDragMove(this.latestDragMovePt);
            }
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            if (!e.Handled)
            {
                base.Focus();
            }
        }

        private void OnUnloaded(object sender, EventArgs e)
        {
            if (this.activeEditToken != null)
            {
                this.activeEditToken.Cancel();
            }
        }

        private void ProcessArrowKey(Key key, ModifierKeys modifiers)
        {
            VectorDouble num;
            VectorDouble num2;
            switch (key)
            {
                case Key.Left:
                    num = new VectorDouble(-1.0, 0.0);
                    break;

                case Key.Up:
                    num = new VectorDouble(0.0, -1.0);
                    break;

                case Key.Right:
                    num = new VectorDouble(1.0, 0.0);
                    break;

                case Key.Down:
                    num = new VectorDouble(0.0, 1.0);
                    break;

                default:
                    throw ExceptionUtil.InvalidEnumArgumentException<Key>(key, "key");
            }
            if ((modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                num2 = (VectorDouble) (num * 10.0);
            }
            else
            {
                num2 = num;
            }
            TranslateTransform editTransform = (TranslateTransform) this.keyboardEditToken.EditTransform;
            editTransform.X += num2.X;
            editTransform.Y += num2.Y;
            this.keyboardEditTimer.Stop();
            this.keyboardEditTimer.Start();
        }

        private void RaiseEditChangedEvent()
        {
            base.VerifyAccess();
            if (!this.IsEditing)
            {
                throw new InvalidOperationException("IsEditing must be true");
            }
            if (this.editChangedSuspendCount == 0)
            {
                base.RaiseEvent(new RoutedEventArgs(EditChangedEvent, this));
            }
            else
            {
                this.suppressedEditChangedEventCount++;
            }
        }

        private void RotationAnchorOffsetPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (this.IsEditing || this.isPreparingToEdit)
            {
                this.RaiseEditChangedEvent();
            }
        }

        private void SetDeltaTransform(Matrix3x2Double deltaTx)
        {
            this.internalMaySetDeltaTransform = true;
            try
            {
                this.DeltaTransform = new MatrixTransform(deltaTx).EnsureFrozen<MatrixTransform>();
            }
            finally
            {
                this.internalMaySetDeltaTransform = false;
            }
        }

        private static void SetIsScaleTransform(ScaleTransform transform, bool value)
        {
            transform.SetValue(IsScaleTransformProperty, BooleanUtil.GetBoxed(value));
        }

        public static void SetTransformHandleType(UIElement target, TransformHandleType value)
        {
            TransformHandlePanel.SetTransformHandleType(target, value);
        }

        public EditTransformToken TryBeginEditing(TransformEditingMode editingMode) => 
            this.TryBeginEditing(editingMode, null);

        private EditTransformToken TryBeginEditing(TransformEditingMode editingMode, TransformHandleType? triggerHandle)
        {
            base.VerifyAccess();
            switch (editingMode)
            {
                case TransformEditingMode.Rotate:
                case TransformEditingMode.Scale:
                case TransformEditingMode.Translate:
                case TransformEditingMode.MoveRotationAnchor:
                case TransformEditingMode.Custom:
                    if (this.IsEditing)
                    {
                        throw new InvalidOperationException("Cannot call BeginEdit() when IsEditing is true");
                    }
                    break;

                default:
                    throw ExceptionUtil.InvalidEnumArgumentException<TransformEditingMode>(editingMode, "editingMode");
            }
            if (this.activeEditToken != null)
            {
                throw new InternalErrorException("this.activeEditToken is non-null");
            }
            using (this.beginEditingRegion.UseEnterScope())
            {
                this.BeginPreparingToEdit();
                TransformEditingBeginEventArgs e = new TransformEditingBeginEventArgs(EditingBeginEvent, this, editingMode, triggerHandle);
                base.RaiseEvent(e);
                if (e.Handled && e.Cancel)
                {
                    this.EndPreparingToEdit();
                    base.RaiseEvent(new RoutedEventArgs(EditingCancelledEvent, this));
                    return null;
                }
                this.EndPreparingToEdit();
                OurEditTransformToken token2 = new OurEditTransformToken(this, editingMode);
                this.EditingMode = editingMode;
                this.activeEditToken = token2;
                return token2;
            }
        }

        private void UpdateFinalTransform()
        {
            this.FinalTransform = new MatrixTransform(this.finalTransform.Value).EnsureFrozen<MatrixTransform>();
        }

        internal EditTransformToken ActiveEditToken =>
            this.activeEditToken;

        public bool AllowBackgroundClick
        {
            get => 
                ((bool) base.GetValue(AllowBackgroundClickProperty));
            set
            {
                base.SetValue(AllowBackgroundClickProperty, BooleanUtil.GetBoxed(value));
            }
        }

        public bool AreScaleHandlesVisible
        {
            get => 
                ((bool) base.GetValue(AreScaleHandlesVisibleProperty));
            set
            {
                base.SetValue(AreScaleHandlesVisibleProperty, BooleanUtil.GetBoxed(value));
            }
        }

        public bool AreScaleHandlesVisibleEffective =>
            ((bool) base.GetValue(AreScaleHandlesVisibleEffectiveProperty));

        public Cursor BackgroundCursor
        {
            get => 
                ((Cursor) base.GetValue(BackgroundCursorProperty));
            set
            {
                base.SetValue(BackgroundCursorProperty, value);
            }
        }

        public RectDouble BaseBounds
        {
            get => 
                ((RectDouble) base.GetValue(BaseBoundsProperty));
            set
            {
                base.SetValue(BaseBoundsProperty, value);
            }
        }

        public Transform BaseTransform
        {
            get => 
                ((Transform) base.GetValue(BaseTransformProperty));
            set
            {
                base.SetValue(BaseTransformProperty, value);
            }
        }

        public Transform DeltaTransform
        {
            get => 
                ((Transform) base.GetValue(DeltaTransformProperty));
            set
            {
                base.SetValue(DeltaTransformProperty, value);
            }
        }

        public TransformEditingMode EditingMode
        {
            get => 
                ((TransformEditingMode) base.GetValue(EditingModeProperty));
            private set
            {
                base.SetValue(EditingModePropertyKey, EnumUtil.GetBoxed<TransformEditingMode>(value));
            }
        }

        public Transform EditTransform
        {
            get => 
                ((Transform) base.GetValue(EditTransformProperty));
            private set
            {
                base.SetValue(EditTransformPropertyKey, value);
            }
        }

        public Transform FinalTransform
        {
            get => 
                ((Transform) base.GetValue(FinalTransformProperty));
            private set
            {
                base.SetValue(FinalTransformPropertyKey, value);
            }
        }

        public double HairWidth
        {
            get => 
                ((double) base.GetValue(HairWidthProperty));
            set
            {
                base.SetValue(HairWidthProperty, DoubleUtil.GetBoxed(value));
            }
        }

        public double HitTestPadding
        {
            get => 
                ((double) base.GetValue(HitTestPaddingProperty));
            set
            {
                base.SetValue(HitTestPaddingProperty, DoubleUtil.GetBoxed(value));
            }
        }

        public bool IsEditing =>
            ((bool) base.GetValue(IsEditingProperty));

        public bool IsInputPixelSnappingEnabled
        {
            get => 
                ((bool) base.GetValue(IsInputPixelSnappingEnabledProperty));
            set
            {
                base.SetValue(IsInputPixelSnappingEnabledProperty, BooleanUtil.GetBoxed(value));
            }
        }

        public bool IsRotationAnchorVisible
        {
            get => 
                ((bool) base.GetValue(IsRotationAnchorVisibleProperty));
            set
            {
                base.SetValue(IsRotationAnchorVisibleProperty, BooleanUtil.GetBoxed(value));
            }
        }

        public bool IsRotationAnchorVisibleEffective =>
            ((bool) base.GetValue(IsRotationAnchorVisibleEffectiveProperty));

        public Geometry RotateBoxClip
        {
            get => 
                ((Geometry) base.GetValue(RotateBoxClipProperty));
            set
            {
                base.SetValue(RotateBoxClipProperty, value);
            }
        }

        public bool RotateBoxClipToBounds
        {
            get => 
                ((bool) base.GetValue(RotateBoxClipToBoundsProperty));
            set
            {
                base.SetValue(RotateBoxClipToBoundsProperty, BooleanUtil.GetBoxed(value));
            }
        }

        public PointDouble? RotationAnchorOffset
        {
            get => 
                ((PointDouble?) base.GetValue(RotationAnchorOffsetProperty));
            set
            {
                base.SetValue(RotationAnchorOffsetProperty, value);
            }
        }

        public Geometry TranslateBoxClip
        {
            get => 
                ((Geometry) base.GetValue(TranslateBoxClipProperty));
            set
            {
                base.SetValue(TranslateBoxClipProperty, value);
            }
        }

        public bool TranslateBoxClipToBounds
        {
            get => 
                ((bool) base.GetValue(TranslateBoxClipToBoundsProperty));
            set
            {
                base.SetValue(TranslateBoxClipToBoundsProperty, BooleanUtil.GetBoxed(value));
            }
        }

        public Cursor TranslateCursor
        {
            get => 
                ((Cursor) base.GetValue(TranslateCursorProperty));
            set
            {
                base.SetValue(TranslateCursorProperty, value);
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly TransformControl.<>c <>9 = new TransformControl.<>c();
            public static Func<Transform, bool> <>9__184_0;
            public static Func<TransformEditingMode, Visibility?> <>9__23_0;

            internal void <.cctor>b__193_0(DependencyObject s, DependencyPropertyChangedEventArgs e)
            {
                ((TransformControl) s).EditingModePropertyChanged(e);
            }

            internal void <.cctor>b__193_1(DependencyObject s, DependencyPropertyChangedEventArgs e)
            {
                ((TransformControl) s).IsEditingPropertyChanged(e);
            }

            internal void <.cctor>b__193_10(DependencyObject s, DependencyProperty e)
            {
                ((TransformControl) s).EditTransformSubPropertyChanged(e);
            }

            internal void <.cctor>b__193_11(DependencyObject s, DependencyPropertyChangedEventArgs e)
            {
                ((TransformControl) s).DeltaTransformPropertyChanged(e);
            }

            internal void <.cctor>b__193_12(DependencyObject s, DependencyProperty p)
            {
                ((TransformControl) s).DeltaTransformSubPropertyChanged(p);
            }

            internal void <.cctor>b__193_13(DependencyObject s, DependencyPropertyChangedEventArgs e)
            {
                ((TransformControl) s).RotationAnchorOffsetPropertyChanged(e);
            }

            internal object <.cctor>b__193_2(DependencyObject dO, object bV) => 
                ((TransformControl) dO).CoerceIsEditingProperty(bV);

            internal void <.cctor>b__193_3(DependencyObject s, DependencyPropertyChangedEventArgs e)
            {
                ((TransformControl) s).AreScaleHandlesVisiblePropertyChanged(e);
            }

            internal object <.cctor>b__193_4(DependencyObject dO, object bV) => 
                ((TransformControl) dO).CoerceAreScaleHandlesVisibleEffectiveProperty(bV);

            internal void <.cctor>b__193_5(DependencyObject s, DependencyPropertyChangedEventArgs e)
            {
                ((TransformControl) s).IsRotationAnchorVisiblePropertyChanged(e);
            }

            internal object <.cctor>b__193_6(DependencyObject dO, object bV) => 
                ((TransformControl) dO).CoerceIsRotationAnchorVisibleEffectiveProperty(bV);

            internal void <.cctor>b__193_7(DependencyObject s, DependencyPropertyChangedEventArgs e)
            {
                ((TransformControl) s).BaseBoundsPropertyChanged(e);
            }

            internal void <.cctor>b__193_8(DependencyObject s, DependencyPropertyChangedEventArgs e)
            {
                ((TransformControl) s).BaseTransformPropertyChanged(e);
            }

            internal void <.cctor>b__193_9(DependencyObject s, DependencyPropertyChangedEventArgs e)
            {
                ((TransformControl) s).EditTransformPropertyChanged(e);
            }

            internal Visibility? <.ctor>b__23_0(TransformEditingMode em)
            {
                if (em != TransformEditingMode.Rotate)
                {
                    return null;
                }
                return 0;
            }

            internal bool <OnHandleDragMove>b__184_0(Transform tx) => 
                TransformControl.GetIsScaleTransform(tx);
        }

        private sealed class OurEditTransformToken : EditTransformToken
        {
            private TransformControl owner;

            public OurEditTransformToken(TransformControl owner, TransformEditingMode editingMode) : base(editingMode, owner.DeltaTransform, owner.RotationAnchorOffset)
            {
                this.owner = owner;
            }

            protected override Transform GetEditTransform() => 
                this.owner.EditTransform;

            protected override bool GetIsActive() => 
                ((this.owner != null) && (this == this.owner.activeEditToken));

            protected override PointDouble? GetRotationAnchorOffset() => 
                this.owner.RotationAnchorOffset;

            protected override void OnCancel()
            {
                this.owner.CancelEditing(this);
            }

            protected override void OnCommit()
            {
                this.owner.CommitEdits(this);
            }

            protected override void OnDeactivated()
            {
                this.owner.NotifyTokenDeactivated(this);
            }

            protected override void SetEditTransform(Transform value)
            {
                this.owner.EditTransform = value;
            }

            protected override void SetRotationAnchorOffset(PointDouble? value)
            {
                this.owner.RotationAnchorOffset = value;
            }

            protected override void VerifyIsActive()
            {
                base.VerifyIsActive();
                if (!this.owner.IsEditing)
                {
                    throw new InternalErrorException("this.owner.IsEditing is false");
                }
            }
        }
    }
}

