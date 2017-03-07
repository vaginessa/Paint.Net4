namespace PaintDotNet.Tools.Controls
{
    using PaintDotNet;
    using PaintDotNet.Animation;
    using PaintDotNet.ObjectModel;
    using PaintDotNet.Rendering;
    using PaintDotNet.Tools.Media;
    using PaintDotNet.UI;
    using PaintDotNet.UI.Controls;
    using PaintDotNet.UI.Input;
    using PaintDotNet.UI.Media;
    using System;
    using System.Runtime.CompilerServices;
    using System.Windows;

    internal sealed class TransformHandlePanel : Panel
    {
        public static readonly DependencyProperty AllowBackgroundClickProperty = FrameworkProperty.Register("AllowBackgroundClick", typeof(bool), typeof(TransformHandlePanel), new PaintDotNet.UI.FrameworkPropertyMetadata(BooleanUtil.GetBoxed(false)));
        public static readonly DependencyProperty AreScaleHandlesVisibleProperty = FrameworkProperty.Register("AreScaleHandlesVisible", typeof(bool), typeof(TransformHandlePanel), new PaintDotNet.UI.FrameworkPropertyMetadata(BooleanUtil.GetBoxed(true)));
        public static readonly DependencyProperty BackgroundCursorProperty = FrameworkProperty.Register("BackgroundCursor", typeof(Cursor), typeof(TransformHandlePanel), new PaintDotNet.UI.FrameworkPropertyMetadata(Cursors.Arrow));
        public static readonly DependencyProperty BaseBoundsProperty = FrameworkProperty.Register("BaseBounds", typeof(RectDouble), typeof(TransformHandlePanel), new PaintDotNet.UI.FrameworkPropertyMetadata(RectDouble.Empty, PaintDotNet.UI.FrameworkPropertyMetadataOptions.AffectsArrange));
        public static readonly DependencyProperty HairWidthProperty = FrameworkProperty.Register("HairWidth", typeof(double), typeof(TransformHandlePanel), new PaintDotNet.UI.FrameworkPropertyMetadata(DoubleUtil.GetBoxed(1.0), PaintDotNet.UI.FrameworkPropertyMetadataOptions.AffectsMeasure | PaintDotNet.UI.FrameworkPropertyMetadataOptions.AffectsArrange));
        private Cursor handCursor;
        private Cursor handDownCursor;
        private AnimationStateHelper[] handleAnimationHelpers;
        private const double handleHotRadiusAnimationDuration = 0.2;
        private const double handleHotRadiusFactor = 1.4;
        private AnimatedDouble[] handleOpacities;
        private HandleElement[] handles;
        public static readonly DependencyProperty HitTestPaddingProperty = FrameworkProperty.Register("HitTestPadding", typeof(double), typeof(TransformHandlePanel), new PaintDotNet.UI.FrameworkPropertyMetadata(DoubleUtil.GetBoxed(3.0), PaintDotNet.UI.FrameworkPropertyMetadataOptions.AffectsMeasure | PaintDotNet.UI.FrameworkPropertyMetadataOptions.AffectsArrange));
        public static readonly DependencyProperty IsHandleAnimationEnabledProperty = FrameworkProperty.Register("IsHandleAnimationEnabled", typeof(bool), typeof(TransformHandlePanel), new PaintDotNet.UI.FrameworkPropertyMetadata(BooleanUtil.GetBoxed(true), new PropertyChangedCallback(<>c.<>9.<.cctor>b__117_0)));
        public static readonly DependencyProperty IsRotationAnchorVisibleProperty = FrameworkProperty.Register("IsRotationAnchorVisible", typeof(bool), typeof(TransformHandlePanel), new PaintDotNet.UI.FrameworkPropertyMetadata(BooleanUtil.GetBoxed(true)));
        private DependencyFunc<RectDouble, PaintDotNet.UI.Media.Transform, PointDouble, double> mouseFollowAngle;
        private RotateCursorDrawing mouseFollowDrawing;
        private HandleElement mouseFollowElement;
        private DependencyFunc<bool, Visibility?, Visibility> mouseFollowElementVisibility;
        private DependencyValue<PointDouble> mouseFollowOffset;
        private RotateTransform mouseFollowTransform;
        public static readonly DependencyProperty RotateBoxClipProperty = FrameworkProperty.Register("RotateBoxClip", typeof(Geometry), typeof(TransformHandlePanel), new PaintDotNet.UI.FrameworkPropertyMetadata(Geometry.Infinite, null, null, Geometry.CoerceGeometryPropertyInfiniteCallback));
        public static readonly DependencyProperty RotateBoxClipToBoundsProperty = FrameworkProperty.Register("RotateBoxClipToBounds", typeof(bool), typeof(TransformHandlePanel), new PaintDotNet.UI.FrameworkPropertyMetadata(BooleanUtil.GetBoxed(true)));
        public static readonly DependencyProperty RotateBoxPaddingFactorProperty = FrameworkProperty.Register("RotateBoxPaddingFactor", typeof(double), typeof(TransformHandlePanel), new PaintDotNet.UI.FrameworkPropertyMetadata(DoubleUtil.GetBoxed(3.0), PaintDotNet.UI.FrameworkPropertyMetadataOptions.AffectsMeasure | PaintDotNet.UI.FrameworkPropertyMetadataOptions.AffectsArrange));
        public static readonly DependencyProperty RotationAnchorOffsetProperty = FrameworkProperty.Register("RotationAnchorOffset", typeof(PointDouble?), typeof(TransformHandlePanel), new PaintDotNet.UI.FrameworkPropertyMetadata(null, PaintDotNet.UI.FrameworkPropertyMetadataOptions.AffectsArrange), ValidateValueCallbacks.NullablePointDoubleIsNullOrFinite);
        private DependencyFunc<PointDouble?, bool, Visibility> rotationAnchorResetButtonVisibility;
        public static readonly DependencyProperty RotationIndicatorVisibilityProperty = FrameworkProperty.Register("RotationIndicatorVisibility", typeof(Visibility?), typeof(TransformHandlePanel), new PaintDotNet.UI.FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty TransformHandleTypeProperty = FrameworkProperty.RegisterAttached("TransformHandleType", typeof(TransformHandleType), typeof(UIElement), typeof(TransformHandlePanel), new PaintDotNet.UI.FrameworkPropertyMetadata(EnumUtil.GetBoxed<TransformHandleType>(TransformHandleType.Invalid)));
        public static readonly DependencyProperty TransformProperty = FrameworkProperty.Register("Transform", typeof(PaintDotNet.UI.Media.Transform), typeof(TransformHandlePanel), new PaintDotNet.UI.FrameworkPropertyMetadata(PaintDotNet.UI.Media.Transform.Identity, PaintDotNet.UI.FrameworkPropertyMetadataOptions.AffectsArrange, null, null, PaintDotNet.UI.Media.Transform.CoerceTransformPropertyCallback));
        public static readonly DependencyProperty TranslateBoxClipProperty = FrameworkProperty.Register("TranslateBoxClip", typeof(Geometry), typeof(TransformHandlePanel), new PaintDotNet.UI.FrameworkPropertyMetadata(Geometry.Infinite, null, null, Geometry.CoerceGeometryPropertyInfiniteCallback));
        public static readonly DependencyProperty TranslateBoxClipToBoundsProperty = FrameworkProperty.Register("TranslateBoxClipToBounds", typeof(bool), typeof(TransformHandlePanel), new PaintDotNet.UI.FrameworkPropertyMetadata(BooleanUtil.GetBoxed(true)));
        public static readonly DependencyProperty TranslateCursorProperty = FrameworkProperty.Register("TranslateCursor", typeof(Cursor), typeof(TransformHandlePanel), new PaintDotNet.UI.FrameworkPropertyMetadata(Cursors.SizeAll));

        public TransformHandlePanel()
        {
            base.ClipToBounds = false;
            this.handCursor = CursorUtil.LoadResource("Cursors.PanToolCursor.cur");
            this.handDownCursor = CursorUtil.LoadResource("Cursors.PanToolCursorMouseDown.cur");
            this.handles = new HandleElement[0x12];
            for (int i = 0; i < this.handles.Length; i++)
            {
                if (i == 3)
                {
                    this.handles[i] = new RotationAnchorResetButton();
                }
                else
                {
                    this.handles[i] = new HandleElement();
                }
                SetTransformHandleType(this.handles[i], (TransformHandleType) i);
                ClickDragBehavior.SetIsEnabled(this.handles[i], true);
                ClickDragBehavior.SetAllowClick(this.handles[i], false);
                this.handles[i].SetBinding(DrawingElement.ScaleProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HairWidthProperty), BindingMode.OneWay);
            }
            this.GetHandle(TransformHandleType.Background).ClipToBounds = false;
            this.GetHandle(TransformHandleType.Background).SetBinding(ClickDragBehavior.AllowClickProperty, this, new PaintDotNet.ObjectModel.PropertyPath(AllowBackgroundClickProperty), BindingMode.OneWay);
            this.GetHandle(TransformHandleType.RotationAnchor).SetBinding<bool, Visibility>(UIElement.VisibilityProperty, this, new PaintDotNet.ObjectModel.PropertyPath(IsRotationAnchorVisibleProperty), BindingMode.OneWay, delegate (bool b) {
                if (!b)
                {
                    return Visibility.Hidden;
                }
                return Visibility.Visible;
            });
            ClickDragBehavior.SetAllowClick(this.GetHandle(TransformHandleType.RotationAnchorResetButton), true);
            ClickDragBehavior.SetAllowDrag(this.GetHandle(TransformHandleType.RotationAnchorResetButton), false);
            this.GetHandle(TransformHandleType.RotationAnchorResetButton).RenderTransform = new RotateTransform();
            this.GetHandle(TransformHandleType.RotationAnchorResetButton).RenderTransformOrigin = new PointDouble(0.5, 0.5);
            this.rotationAnchorResetButtonVisibility = new DependencyFunc<PointDouble?, bool, Visibility>(new Func<PointDouble?, bool, Visibility>(TransformHandlePanel.GetRotationAnchorResetButtonVisibility));
            this.rotationAnchorResetButtonVisibility.SetArgInput(1, this, new PaintDotNet.ObjectModel.PropertyPath(RotationAnchorOffsetProperty));
            this.rotationAnchorResetButtonVisibility.SetArgInput(2, this, new PaintDotNet.ObjectModel.PropertyPath(IsRotationAnchorVisibleProperty));
            this.GetHandle(TransformHandleType.RotationAnchorResetButton).SetBinding(UIElement.VisibilityProperty, this.rotationAnchorResetButtonVisibility, new PaintDotNet.ObjectModel.PropertyPath(this.rotationAnchorResetButtonVisibility.GetValueProperty()), BindingMode.OneWay);
            this.GetHandle(TransformHandleType.TranslateBox).RenderTransform = new RotateTransform();
            this.GetHandle(TransformHandleType.TranslateBox).SetBinding(UIElement.ClipProperty, this, new PaintDotNet.ObjectModel.PropertyPath(TranslateBoxClipProperty), BindingMode.OneWay);
            this.GetHandle(TransformHandleType.TranslateBox).SetBinding(UIElement.ClipToBoundsProperty, this, new PaintDotNet.ObjectModel.PropertyPath(TranslateBoxClipToBoundsProperty), BindingMode.OneWay);
            this.GetHandle(TransformHandleType.RotateBox).RenderTransform = new RotateTransform();
            this.GetHandle(TransformHandleType.RotateBox).SetBinding(UIElement.ClipProperty, this, new PaintDotNet.ObjectModel.PropertyPath(RotateBoxClipProperty), BindingMode.OneWay);
            this.GetHandle(TransformHandleType.RotateBox).SetBinding(UIElement.ClipToBoundsProperty, this, new PaintDotNet.ObjectModel.PropertyPath(RotateBoxClipToBoundsProperty), BindingMode.OneWay);
            for (int j = 6; j <= 9; j++)
            {
                this.handles[j].RenderTransform = new RotateTransform();
            }
            for (int k = 6; k <= 0x11; k++)
            {
                this.handles[k].SetBinding<bool, Visibility>(UIElement.VisibilityProperty, this, new PaintDotNet.ObjectModel.PropertyPath(AreScaleHandlesVisibleProperty), BindingMode.OneWay, delegate (bool b) {
                    if (!b)
                    {
                        return Visibility.Hidden;
                    }
                    return Visibility.Visible;
                });
            }
            for (int m = 4; m <= 0x11; m++)
            {
                if (m != 5)
                {
                    this.handles[m].RenderTransformOrigin = new PointDouble(0.5, 0.5);
                    RotateTransform targetObject = new RotateTransform();
                    targetObject.SetBinding<PaintDotNet.UI.Media.Transform, double>(RotateTransform.AngleProperty, this, new PaintDotNet.ObjectModel.PropertyPath(TransformProperty), BindingMode.OneWay, tx => tx.Value.GetRotationAngle());
                    this.handles[m].RenderTransform = targetObject;
                }
            }
            this.GetHandle(TransformHandleType.RotationAnchor).SetBinding(HandleElement.IsHotProperty, this.GetHandle(TransformHandleType.RotateBox), new PaintDotNet.ObjectModel.PropertyPath(UIElement.IsMouseOverProperty), BindingMode.OneWay);
            this.GetHandle(TransformHandleType.TranslateHandle).SetBinding(HandleElement.IsHotProperty, this.GetHandle(TransformHandleType.TranslateHandle), new PaintDotNet.ObjectModel.PropertyPath(UIElement.IsMouseOverProperty), BindingMode.OneWay);
            this.GetHandle(TransformHandleType.ScaleW).SetBinding(HandleElement.IsHotProperty, this.GetHandle(TransformHandleType.ScaleEdgeW), new PaintDotNet.ObjectModel.PropertyPath(UIElement.IsMouseOverProperty), BindingMode.OneWay);
            this.GetHandle(TransformHandleType.ScaleN).SetBinding(HandleElement.IsHotProperty, this.GetHandle(TransformHandleType.ScaleEdgeN), new PaintDotNet.ObjectModel.PropertyPath(UIElement.IsMouseOverProperty), BindingMode.OneWay);
            this.GetHandle(TransformHandleType.ScaleE).SetBinding(HandleElement.IsHotProperty, this.GetHandle(TransformHandleType.ScaleEdgeE), new PaintDotNet.ObjectModel.PropertyPath(UIElement.IsMouseOverProperty), BindingMode.OneWay);
            this.GetHandle(TransformHandleType.ScaleS).SetBinding(HandleElement.IsHotProperty, this.GetHandle(TransformHandleType.ScaleEdgeS), new PaintDotNet.ObjectModel.PropertyPath(UIElement.IsMouseOverProperty), BindingMode.OneWay);
            for (int n = 4; n <= 0x11; n++)
            {
                this.SetHandCursorBinding(this.handles[n]);
            }
            this.SetHandCursorBinding(this.GetHandle(TransformHandleType.RotateBox));
            this.GetHandle(TransformHandleType.Background).SetBinding(PaintDotNet.UI.FrameworkElement.CursorProperty, this, new PaintDotNet.ObjectModel.PropertyPath(BackgroundCursorProperty), BindingMode.OneWay);
            this.GetHandle(TransformHandleType.TranslateBox).SetBinding(PaintDotNet.UI.FrameworkElement.CursorProperty, this, new PaintDotNet.ObjectModel.PropertyPath(TranslateCursorProperty), BindingMode.OneWay);
            this.GetHandle(TransformHandleType.TranslateHandle).Drawing = new CompassHandleDrawing();
            this.GetHandle(TransformHandleType.ScaleW).Drawing = new CircleHandleDrawing();
            this.GetHandle(TransformHandleType.ScaleN).Drawing = new CircleHandleDrawing();
            this.GetHandle(TransformHandleType.ScaleE).Drawing = new CircleHandleDrawing();
            this.GetHandle(TransformHandleType.ScaleS).Drawing = new CircleHandleDrawing();
            this.GetHandle(TransformHandleType.ScaleNW).Drawing = new CircleHandleDrawing();
            this.GetHandle(TransformHandleType.ScaleNE).Drawing = new CircleHandleDrawing();
            this.GetHandle(TransformHandleType.ScaleSW).Drawing = new CircleHandleDrawing();
            this.GetHandle(TransformHandleType.ScaleSE).Drawing = new CircleHandleDrawing();
            this.GetHandle(TransformHandleType.RotationAnchor).Drawing = new ScrewHandleDrawing();
            for (int num6 = 0; num6 < this.handles.Length; num6++)
            {
                base.Children.Add(this.handles[num6]);
            }
            this.mouseFollowElement = new HandleElement();
            this.mouseFollowElementVisibility = new DependencyFunc<bool, Visibility?, Visibility>(new Func<bool, Visibility?, Visibility>(this.GetMouseFollowElementVisibility));
            this.mouseFollowElementVisibility.SetArgInput(1, this.GetHandle(TransformHandleType.RotateBox), new PaintDotNet.ObjectModel.PropertyPath(UIElement.IsMouseOverProperty));
            this.mouseFollowElementVisibility.SetArgInput(2, this, new PaintDotNet.ObjectModel.PropertyPath(RotationIndicatorVisibilityProperty));
            this.mouseFollowElement.SetBinding(UIElement.VisibilityProperty, this.mouseFollowElementVisibility, new PaintDotNet.ObjectModel.PropertyPath(this.mouseFollowElementVisibility.GetValueProperty()), BindingMode.OneWay);
            this.mouseFollowElement.SetBinding(DrawingElement.ScaleProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HairWidthProperty), BindingMode.OneWay);
            this.mouseFollowElement.IsHitTestVisible = false;
            this.mouseFollowOffset = new DependencyValue<PointDouble>();
            this.mouseFollowOffset.ValueChanged += (s, e) => base.InvalidateArrange();
            this.mouseFollowAngle = new DependencyFunc<RectDouble, PaintDotNet.UI.Media.Transform, PointDouble, double>(new Func<RectDouble, PaintDotNet.UI.Media.Transform, PointDouble, double>(this.GetMouseFollowDrawingAngle));
            this.mouseFollowAngle.SetArgInput(1, this, new PaintDotNet.ObjectModel.PropertyPath(BaseBoundsProperty));
            this.mouseFollowAngle.SetArgInput(2, this, new PaintDotNet.ObjectModel.PropertyPath(TransformProperty));
            this.mouseFollowAngle.SetArgInput(3, this.mouseFollowOffset, new PaintDotNet.ObjectModel.PropertyPath(this.mouseFollowOffset.GetValueProperty()));
            this.mouseFollowDrawing = new RotateCursorDrawing();
            this.mouseFollowDrawing.Radius = 16.0;
            this.mouseFollowDrawing.BigRadius = 50.0;
            this.mouseFollowTransform = new RotateTransform();
            this.mouseFollowTransform.SetBinding(RotateTransform.AngleProperty, this.mouseFollowAngle, new PaintDotNet.ObjectModel.PropertyPath(this.mouseFollowAngle.GetValueProperty()), BindingMode.OneWay);
            this.mouseFollowElement.RenderTransform = this.mouseFollowTransform;
            this.mouseFollowElement.RenderTransformOrigin = new PointDouble(0.5, 0.5);
            this.mouseFollowElement.Drawing = this.mouseFollowDrawing;
            base.Children.Add(this.mouseFollowElement);
            this.handleAnimationHelpers = new AnimationStateHelper[0x12];
            for (int num7 = 4; num7 <= 0x11; num7++)
            {
                this.handleAnimationHelpers[num7] = new AnimationStateHelper();
                this.handleAnimationHelpers[num7].Element = this.GetHandle((TransformHandleType) num7);
                this.handleAnimationHelpers[num7].ShouldEnableAnimationsChanged += new ValueChangedEventHandler<bool>(this.OnHandleShouldEnableAnimationsChanged);
            }
        }

        private static void ArrangeBoxHandle(UIElement handle, RectDouble baseBounds, SizeDouble inflationSize, SizeDouble deflationSize, Matrix3x2Double tx)
        {
            RectDouble zero;
            PointDouble num = tx.Transform(baseBounds.TopLeft);
            PointDouble num2 = tx.Transform(baseBounds.TopRight);
            PointDouble num3 = tx.Transform(baseBounds.BottomLeft);
            PointDouble num4 = tx.Transform(baseBounds.BottomRight);
            VectorDouble num5 = (VectorDouble) (num2 - num);
            VectorDouble num6 = (VectorDouble) (num4 - num2);
            bool flag = tx.IsFlipped();
            double length = num5.Length;
            double num8 = flag ? ((double) (-1)) : ((double) 1);
            double num9 = num6.Length * num8;
            PointDouble num10 = num;
            RectDouble rect = RectDouble.FromEdges(num10.X, num10.Y, num10.X + length, num10.Y + num9);
            double dx = inflationSize.Width - deflationSize.Width;
            double dy = inflationSize.Height - deflationSize.Height;
            double num14 = rect.Width + (dx * 2.0);
            double num15 = rect.Height + (dy * 2.0);
            if ((num14 <= double.Epsilon) || (num15 <= double.Epsilon))
            {
                zero = RectDouble.Zero;
            }
            else
            {
                zero = RectDouble.Inflate(rect, dx, dy);
                handle.RenderTransformOriginUnits = RenderTransformOriginUnits.Absolute;
                handle.RenderTransformOrigin = new PointDouble(num10.X - zero.TopLeft.X, num10.Y - zero.TopLeft.Y);
                double num18 = MathUtil.RadiansToDegrees(Math.Atan2(num5.Y, num5.X));
                ((RotateTransform) handle.RenderTransform).Angle = num18;
            }
            handle.Arrange(zero);
        }

        private void ArrangeByCenter(UIElement element, PointDouble centerPt, SizeDouble? size, Matrix3x2Double tx)
        {
            SizeDouble num;
            if (size.HasValue)
            {
                num = size.Value;
            }
            else
            {
                SizeDouble desiredSize = element.DesiredSize;
                double hairWidth = this.HairWidth;
                double hitTestPadding = this.HitTestPadding;
                double num9 = hairWidth * hitTestPadding;
                num = new SizeDouble(desiredSize.Width + num9, desiredSize.Height + num9);
            }
            PointDouble location = tx.Transform(centerPt);
            RectDouble rect = new RectDouble(location, num);
            VectorDouble offset = (VectorDouble) -(((VectorDouble) num) / 2.0);
            RectDouble finalRect = RectDouble.Offset(rect, offset);
            element.Arrange(finalRect);
        }

        private void ArrangeCornerOffsetHandle(HandleElement cornerOffsetHandle, PointDouble baseTopLeftPt, PointDouble baseBottomRightPt, Matrix3x2Double tx, double distanceFromBottomRightPt)
        {
            PointDouble num = tx.Transform(baseTopLeftPt);
            PointDouble num2 = tx.Transform(baseBottomRightPt);
            VectorDouble vec = (VectorDouble) (num2 - num);
            VectorDouble num4 = VectorDouble.Normalize(vec);
            PointDouble centerPt = num2 + ((PointDouble) (num4 * distanceFromBottomRightPt));
            this.ArrangeByCenter(cornerOffsetHandle, centerPt, null, Matrix3x2Double.Identity);
        }

        private static void ArrangeEdgeScaleHandle(UIElement edgeScaleHandle, PointDouble baseCorner1, PointDouble baseCorner2, SizeDouble handleSize, Matrix3x2Double tx)
        {
            VectorDouble vec = (VectorDouble) (baseCorner2 - baseCorner1);
            double num3 = MathUtil.RadiansToDegrees(Math.Atan2(vec.Y, vec.X));
            VectorDouble num4 = VectorDouble.Normalize(vec);
            PointDouble num5 = tx.Transform(baseCorner1);
            VectorDouble num7 = (VectorDouble) (tx.Transform(baseCorner2) - num5);
            double length = num7.Length;
            double num10 = MathUtil.RadiansToDegrees(Math.Atan2(num7.Y, num7.X));
            PointDouble location = num5;
            PointDouble num12 = new PointDouble(location.X + (length * num4.X), location.Y + (length * num4.Y));
            RectDouble finalRect = new RectDouble(location, new SizeDouble(length * num4.X, length * num4.Y));
            finalRect.Inflate((double) ((0.5 * handleSize.Width) * num4.Y), (double) ((0.5 * handleSize.Height) * num4.X));
            edgeScaleHandle.Arrange(finalRect);
            PointDouble num14 = new PointDouble(0.5 * num4.Y, 0.5 * num4.X);
            PointDouble num15 = num14.IsFinite ? num14 : new PointDouble(0.0, 0.0);
            edgeScaleHandle.RenderTransformOrigin = num15;
            double num16 = num10 - num3;
            ((RotateTransform) edgeScaleHandle.RenderTransform).Angle = num16;
        }

        protected override SizeDouble ArrangeOverride(SizeDouble finalSize)
        {
            double hairWidth = this.HairWidth;
            double hitTestPadding = this.HitTestPadding;
            double num3 = hairWidth * hitTestPadding;
            RectDouble baseBounds = this.BaseBounds;
            Matrix3x2Double tx = this.Transform.Value;
            RectDouble transformedBounds = tx.Transform(baseBounds);
            if ((baseBounds.IsEmpty || !transformedBounds.IsFinite) || (((baseBounds.X == 0.0) && (baseBounds.Y == 0.0)) && ((baseBounds.Width == 0.0) && (baseBounds.Height == 0.0))))
            {
                RectDouble finalRect = transformedBounds.IsFinite ? transformedBounds : RectDouble.Zero;
                foreach (UIElement element in base.Children)
                {
                    element.Arrange(finalRect);
                }
            }
            else
            {
                SizeDouble positiveInfinity = SizeDouble.PositiveInfinity;
                for (int i = 10; i <= 0x11; i++)
                {
                    SizeDouble desiredSize = this.GetHandle((TransformHandleType) i).DesiredSize;
                    double width = Math.Min(positiveInfinity.Width, desiredSize.Width);
                    positiveInfinity = new SizeDouble(width, Math.Min(positiveInfinity.Height, desiredSize.Height));
                }
                SizeDouble otherHandleSize = new SizeDouble(positiveInfinity.Width + num3, positiveInfinity.Height + num3);
                this.GetHandle(TransformHandleType.Background).Arrange(new RectDouble(PointDouble.Zero, finalSize));
                SizeDouble? size = null;
                this.ArrangeByCenter(this.GetHandle(TransformHandleType.ScaleNW), baseBounds.TopLeft, size, tx);
                size = null;
                this.ArrangeByCenter(this.GetHandle(TransformHandleType.ScaleNE), baseBounds.TopRight, size, tx);
                size = null;
                this.ArrangeByCenter(this.GetHandle(TransformHandleType.ScaleSW), baseBounds.BottomLeft, size, tx);
                size = null;
                this.ArrangeByCenter(this.GetHandle(TransformHandleType.ScaleSE), baseBounds.BottomRight, size, tx);
                size = null;
                this.ArrangeByCenter(this.GetHandle(TransformHandleType.ScaleW), baseBounds.LeftCenter(), size, tx);
                size = null;
                this.ArrangeByCenter(this.GetHandle(TransformHandleType.ScaleN), baseBounds.TopCenter(), size, tx);
                size = null;
                this.ArrangeByCenter(this.GetHandle(TransformHandleType.ScaleE), baseBounds.RightCenter(), size, tx);
                size = null;
                this.ArrangeByCenter(this.GetHandle(TransformHandleType.ScaleS), baseBounds.BottomCenter(), size, tx);
                PointDouble effectiveRotationAnchorOffset = this.EffectiveRotationAnchorOffset;
                this.ArrangeByCenter(this.GetHandle(TransformHandleType.RotationAnchor), effectiveRotationAnchorOffset, null, Matrix3x2Double.Identity);
                HandleElement handle = this.GetHandle(TransformHandleType.RotationAnchorResetButton);
                this.ArrangeRotationAnchorResetButton(handle, transformedBounds, otherHandleSize, effectiveRotationAnchorOffset);
                ArrangeEdgeScaleHandle(this.GetHandle(TransformHandleType.ScaleEdgeW), baseBounds.TopLeft, baseBounds.BottomLeft, otherHandleSize, tx);
                ArrangeEdgeScaleHandle(this.GetHandle(TransformHandleType.ScaleEdgeN), baseBounds.TopLeft, baseBounds.TopRight, otherHandleSize, tx);
                ArrangeEdgeScaleHandle(this.GetHandle(TransformHandleType.ScaleEdgeE), baseBounds.TopRight, baseBounds.BottomRight, otherHandleSize, tx);
                ArrangeEdgeScaleHandle(this.GetHandle(TransformHandleType.ScaleEdgeS), baseBounds.BottomLeft, baseBounds.BottomRight, otherHandleSize, tx);
                double rotateBoxPaddingFactor = this.RotateBoxPaddingFactor;
                SizeDouble inflationSize = new SizeDouble(rotateBoxPaddingFactor * otherHandleSize.Width, rotateBoxPaddingFactor * otherHandleSize.Height);
                ArrangeBoxHandle(this.GetHandle(TransformHandleType.RotateBox), baseBounds, inflationSize, SizeDouble.Zero, tx);
                SizeDouble num13 = inflationSize;
                ArrangeBoxHandle(this.GetHandle(TransformHandleType.TranslateBox), baseBounds, SizeDouble.Zero, SizeDouble.Zero, tx);
                HandleElement cornerOffsetHandle = this.GetHandle(TransformHandleType.TranslateHandle);
                this.ArrangeCornerOffsetHandle(cornerOffsetHandle, baseBounds.TopLeft, baseBounds.BottomRight, tx, 45.0 * hairWidth);
                this.ArrangeByCenter(this.mouseFollowElement, this.mouseFollowOffset.Value, new SizeDouble?(this.mouseFollowElement.DesiredSize), Matrix3x2Double.Identity);
            }
            return base.ArrangeOverride(finalSize);
        }

        private void ArrangeRotationAnchorResetButton(HandleElement rotationAnchorResetButton, RectDouble transformedBounds, SizeDouble otherHandleSize, PointDouble rotationAnchorOffset)
        {
            VectorDouble vec = (VectorDouble) (rotationAnchorOffset - transformedBounds.Center);
            VectorDouble num2 = vec.NormalizeOrZeroCopy();
            PointDouble centerPt = (transformedBounds.Center + vec) + ((PointDouble) ((num2 * otherHandleSize.Width) * 2.0));
            this.ArrangeByCenter(rotationAnchorResetButton, centerPt, new SizeDouble?(rotationAnchorResetButton.DesiredSize), Matrix3x2Double.Identity);
            double num5 = MathUtil.RadiansToDegrees(Math.Atan2(vec.Y, vec.X));
            ((RotateTransform) rotationAnchorResetButton.RenderTransform).Angle = num5;
        }

        private void DisableHandleAnimation(TransformHandleType handleType)
        {
            base.VerifyAccess();
            int index = (int) handleType;
            if ((index < 4) || (index > 0x11))
            {
                throw new ArgumentOutOfRangeException();
            }
            if ((GetHandleOpacityIndex(handleType) != -1) && ((this.handleOpacities != null) && (this.handleOpacities[index] != null)))
            {
                this.handles[index].ClearBinding(UIElement.OpacityProperty);
                this.handles[index].ClearValue(UIElement.OpacityProperty);
                this.handleOpacities[index].StopAnimation();
                DisposableUtil.Free<AnimatedDouble>(ref this.handleOpacities[index]);
            }
        }

        private void EnableHandleAnimation(TransformHandleType handleType)
        {
            double offset;
            base.VerifyAccess();
            int index = (int) handleType;
            if ((index < 4) || (index > 0x11))
            {
                throw new ArgumentOutOfRangeException();
            }
            int handleOpacityIndex = GetHandleOpacityIndex(handleType);
            if (handleOpacityIndex != -1)
            {
                if (this.handleOpacities == null)
                {
                    this.handleOpacities = new AnimatedDouble[this.handles.Length];
                }
                if (this.handleOpacities[index] == null)
                {
                    this.handleOpacities[index] = new AnimatedDouble(1.0);
                    offset = ((double) handleOpacityIndex) / 10.0;
                    this.handleOpacities[index].AnimateRawValue((s, v) => InitializeHandleOpacityStoryboard(s, v, offset), null);
                    this.handles[index].SetBinding(UIElement.OpacityProperty, this.handleOpacities[index], new PaintDotNet.ObjectModel.PropertyPath(AnimatedValue<double>.ValuePropertyName, Array.Empty<object>()), BindingMode.OneWay);
                }
            }
        }

        private HandleElement GetHandle(TransformHandleType handleType) => 
            this.handles[(int) handleType];

        private static int GetHandleOpacityIndex(TransformHandleType handleType)
        {
            switch (handleType)
            {
                case TransformHandleType.RotationAnchor:
                    return 0;

                case TransformHandleType.TranslateHandle:
                    return 9;

                case TransformHandleType.ScaleW:
                    return 8;

                case TransformHandleType.ScaleN:
                    return 2;

                case TransformHandleType.ScaleE:
                    return 4;

                case TransformHandleType.ScaleS:
                    return 6;

                case TransformHandleType.ScaleNW:
                    return 1;

                case TransformHandleType.ScaleNE:
                    return 3;

                case TransformHandleType.ScaleSW:
                    return 7;

                case TransformHandleType.ScaleSE:
                    return 5;
            }
            return 0;
        }

        private double GetMouseFollowDrawingAngle(RectDouble baseBounds, PaintDotNet.UI.Media.Transform transform, PointDouble mousePosition)
        {
            if ((baseBounds.IsEmpty || !baseBounds.IsFinite) || (transform == null))
            {
                return 0.0;
            }
            PointDouble transformedCenter = this.GetTransformedCenter(baseBounds, transform);
            VectorDouble num2 = (VectorDouble) (mousePosition - transformedCenter);
            double radians = Math.Atan2(num2.Y, num2.X) + 3.1415926535897931;
            return MathUtil.RadiansToDegrees(radians);
        }

        private double GetMouseFollowDrawingBigRadius(RectDouble baseBounds, PaintDotNet.UI.Media.Transform transform, PointDouble mousePosition)
        {
            if ((baseBounds.IsEmpty || !baseBounds.IsFinite) || (transform == null))
            {
                return 1.0;
            }
            PointDouble transformedCenter = this.GetTransformedCenter(baseBounds, transform);
            VectorDouble num2 = (VectorDouble) (mousePosition - transformedCenter);
            return num2.Length;
        }

        private Visibility GetMouseFollowElementVisibility(bool isMouseOver, Visibility? rotationIndicatorVisibility)
        {
            if (rotationIndicatorVisibility.HasValue)
            {
                return rotationIndicatorVisibility.Value;
            }
            if (!isMouseOver)
            {
                return Visibility.Hidden;
            }
            return Visibility.Visible;
        }

        private static Visibility GetRotationAnchorResetButtonVisibility(PointDouble? offset, bool isVisible)
        {
            if (offset.HasValue & isVisible)
            {
                return Visibility.Visible;
            }
            return Visibility.Hidden;
        }

        internal static VectorDouble GetScaleHandleDirectionVector(TransformHandleType scaleHandleType)
        {
            switch (scaleHandleType)
            {
                case TransformHandleType.ScaleEdgeW:
                case TransformHandleType.ScaleW:
                    return new VectorDouble(-1.0, 0.0);

                case TransformHandleType.ScaleEdgeN:
                case TransformHandleType.ScaleN:
                    return new VectorDouble(0.0, -1.0);

                case TransformHandleType.ScaleEdgeE:
                case TransformHandleType.ScaleE:
                    return new VectorDouble(1.0, 0.0);

                case TransformHandleType.ScaleEdgeS:
                case TransformHandleType.ScaleS:
                    return new VectorDouble(0.0, 1.0);

                case TransformHandleType.ScaleNW:
                    return new VectorDouble(-1.0, -1.0);

                case TransformHandleType.ScaleNE:
                    return new VectorDouble(1.0, -1.0);

                case TransformHandleType.ScaleSW:
                    return new VectorDouble(-1.0, 1.0);

                case TransformHandleType.ScaleSE:
                    return new VectorDouble(1.0, 1.0);
            }
            throw ExceptionUtil.InvalidEnumArgumentException<TransformHandleType>(scaleHandleType, "scaleHandleType");
        }

        private PointDouble GetTransformedCenter(RectDouble baseBounds, PaintDotNet.UI.Media.Transform transform)
        {
            if ((!baseBounds.IsEmpty && baseBounds.IsFinite) && (transform != null))
            {
                return this.EffectiveRotationAnchorOffset;
            }
            return PointDouble.Zero;
        }

        public static TransformHandleType GetTransformHandleType(UIElement target) => 
            ((TransformHandleType) target.GetValue(TransformHandleTypeProperty));

        private static void InitializeHandleOpacityStoryboard(IAnimationStoryboard storyboard, IAnimationVariable variable, double startOffsetPeriodFraction)
        {
            AnimationSeconds duration = 2.0;
            AnimationSeconds seconds2 = duration * startOffsetPeriodFraction;
            if (seconds2 != 0.0)
            {
                AnimationTransition transition2 = new ConstantAnimationTransition(seconds2);
                storyboard.AddTransition(variable, transition2);
            }
            AnimationKeyFrame startKeyFrame = storyboard.AddKeyFrameAtOffset(AnimationKeyFrame.StoryboardStart, seconds2);
            AnimationTransition transition = new SinusoidalFromRangeAnimationTransition(duration, 0.25, 1.5, duration, AnimationSlope.Decreasing);
            storyboard.AddTransition(variable, transition);
            AnimationKeyFrame endKeyFrame = storyboard.AddKeyFrameAfterTransition(transition);
            storyboard.RepeatBetweenKeyFrames(startKeyFrame, endKeyFrame, -1);
        }

        private void IsHandleAnimationEnabledPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            this.UpdateAllHandleAnimationStates();
        }

        protected override SizeDouble MeasureOverride(SizeDouble constraintSize)
        {
            foreach (UIElement element in base.Children)
            {
                if (element != null)
                {
                    element.Measure(SizeDouble.PositiveInfinity);
                }
            }
            return base.MeasureOverride(constraintSize);
        }

        private void OnHandleIsMouseOverChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            TransformHandleType rotationAnchor;
            HandleElement target = (HandleElement) sender;
            TransformHandleType transformHandleType = GetTransformHandleType(target);
            switch (transformHandleType)
            {
                case TransformHandleType.RotateBox:
                    rotationAnchor = TransformHandleType.RotationAnchor;
                    break;

                case TransformHandleType.ScaleEdgeW:
                    rotationAnchor = TransformHandleType.ScaleW;
                    break;

                case TransformHandleType.ScaleEdgeN:
                    rotationAnchor = TransformHandleType.ScaleN;
                    break;

                case TransformHandleType.ScaleEdgeE:
                    rotationAnchor = TransformHandleType.ScaleE;
                    break;

                case TransformHandleType.ScaleEdgeS:
                    rotationAnchor = TransformHandleType.ScaleS;
                    break;

                default:
                    rotationAnchor = transformHandleType;
                    break;
            }
            HandleDrawing drawing = this.GetHandle(rotationAnchor).Drawing;
        }

        private void OnHandleShouldEnableAnimationsChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            this.UpdateHandleAnimationState(((AnimationStateHelper) sender).Element);
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            PointDouble position = e.GetPosition(this);
            this.mouseFollowOffset.Value = position;
            base.OnPreviewMouseMove(e);
        }

        private void SetHandCursorBinding(UIElement element)
        {
            object[] pathParameters = new object[] { ClickDragBehavior.IsPressedProperty };
            element.SetBinding<bool, Cursor>(PaintDotNet.UI.FrameworkElement.CursorProperty, element, new PaintDotNet.ObjectModel.PropertyPath("(0)", pathParameters), BindingMode.OneWay, delegate (bool ip) {
                if (!ip)
                {
                    return this.handCursor;
                }
                return this.handDownCursor;
            });
        }

        public static void SetTransformHandleType(UIElement target, TransformHandleType value)
        {
            target.SetValue(TransformHandleTypeProperty, EnumUtil.GetBoxed<TransformHandleType>(value));
        }

        private void UpdateAllHandleAnimationStates()
        {
            base.VerifyAccess();
            for (int i = 4; i <= 0x11; i++)
            {
                this.UpdateHandleAnimationState((TransformHandleType) i);
            }
        }

        private void UpdateHandleAnimationState(HandleElement handle)
        {
            if (handle != null)
            {
                TransformHandleType transformHandleType = GetTransformHandleType(handle);
                this.UpdateHandleAnimationState(transformHandleType);
            }
        }

        private void UpdateHandleAnimationState(TransformHandleType handleType)
        {
            base.VerifyAccess();
            int index = (int) handleType;
            if ((index < 4) || (index > 0x11))
            {
                throw new ArgumentOutOfRangeException();
            }
            if (this.handleAnimationHelpers[index].ShouldEnableAnimations && this.IsHandleAnimationEnabled)
            {
                this.EnableHandleAnimation(handleType);
            }
            else
            {
                this.DisableHandleAnimation(handleType);
            }
        }

        private void UpdateHandleAnimationState(object handle)
        {
            this.UpdateHandleAnimationState((HandleElement) handle);
        }

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

        public PointDouble EffectiveRotationAnchorOffset
        {
            get
            {
                PointDouble? rotationAnchorOffset = this.RotationAnchorOffset;
                if (rotationAnchorOffset.HasValue)
                {
                    return rotationAnchorOffset.Value;
                }
                return this.TransformedBoundsCenter;
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

        public bool IsHandleAnimationEnabled
        {
            get => 
                ((bool) base.GetValue(IsHandleAnimationEnabledProperty));
            set
            {
                base.SetValue(IsHandleAnimationEnabledProperty, BooleanUtil.GetBoxed(value));
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

        public double RotateBoxPaddingFactor
        {
            get => 
                ((double) base.GetValue(RotateBoxPaddingFactorProperty));
            set
            {
                base.SetValue(RotateBoxPaddingFactorProperty, DoubleUtil.GetBoxed(value));
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

        public Visibility? RotationIndicatorVisibility
        {
            get => 
                ((Visibility?) base.GetValue(RotationIndicatorVisibilityProperty));
            set
            {
                base.SetValue(RotationIndicatorVisibilityProperty, value);
            }
        }

        public PaintDotNet.UI.Media.Transform Transform
        {
            get => 
                ((PaintDotNet.UI.Media.Transform) base.GetValue(TransformProperty));
            set
            {
                base.SetValue(TransformProperty, value);
            }
        }

        public PointDouble TransformedBoundsCenter
        {
            get
            {
                RectDouble baseBounds = this.BaseBounds;
                return this.Transform.Value.Transform(baseBounds.Center);
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
            public static readonly TransformHandlePanel.<>c <>9 = new TransformHandlePanel.<>c();
            public static Func<bool, Visibility> <>9__14_0;
            public static Func<bool, Visibility> <>9__14_1;
            public static Func<Transform, double> <>9__14_2;

            internal void <.cctor>b__117_0(DependencyObject s, DependencyPropertyChangedEventArgs e)
            {
                ((TransformHandlePanel) s).IsHandleAnimationEnabledPropertyChanged(e);
            }

            internal Visibility <.ctor>b__14_0(bool b)
            {
                if (!b)
                {
                    return Visibility.Hidden;
                }
                return Visibility.Visible;
            }

            internal Visibility <.ctor>b__14_1(bool b)
            {
                if (!b)
                {
                    return Visibility.Hidden;
                }
                return Visibility.Visible;
            }

            internal double <.ctor>b__14_2(Transform tx) => 
                tx.Value.GetRotationAngle();
        }
    }
}

