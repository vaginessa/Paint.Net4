namespace PaintDotNet.Tools.Shapes
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Drawing;
    using PaintDotNet.Functional;
    using PaintDotNet.Imaging;
    using PaintDotNet.ObjectModel;
    using PaintDotNet.PropertySystem;
    using PaintDotNet.Rendering;
    using PaintDotNet.Settings.App;
    using PaintDotNet.Shapes;
    using PaintDotNet.Threading;
    using PaintDotNet.Tools;
    using PaintDotNet.UI.Media;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing.Drawing2D;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    [Serializable]
    internal sealed class ShapesToolChanges : TransactedToolChanges<ShapesToolChanges, ShapesToolBase>
    {
        private TupleStruct<PaintDotNet.Shapes.ShapeInfo, object, object>[] allShapePropertyValuesArray;
        [NonSerialized]
        private IDictionary<PaintDotNet.Shapes.ShapeInfo, IDictionary<object, object>> allShapePropertyValuesMap;
        private static readonly IDictionary<object, object> emptyShapePropertyValuesRO = new Dictionary<object, object>().AsReadOnly<object, object>();
        private bool isEditingEndPoint;
        private bool isEditingStartPoint;
        private PointDouble mouseEndPoint;
        private PointDouble mouseStartPoint;
        [NonSerialized]
        private Pen outlinePen;
        private PointDouble? rotationAnchorOffset;
        private readonly string shapeInfoSettingPath;
        [NonSerialized]
        private PropertyCollection shapePropertySchema;
        [NonSerialized]
        private Result<PaintDotNet.Shapes.ShapeRenderData> shapeRenderData;
        [NonSerialized]
        private PaintDotNet.Shapes.ShapeRenderParameters shapeRenderParams;
        private bool shouldApplyConstraint;
        private bool shouldApplySnapping;
        private Matrix3x2Double transform;
        private PaintDotNet.WhichUserColor whichUserColor;

        public ShapesToolChanges(IEnumerable<KeyValuePair<string, object>> drawingSettingsValues, string shapeInfoSettingPath, IDictionary<PaintDotNet.Shapes.ShapeInfo, IDictionary<object, object>> allShapePropertyValues, PointDouble mouseStartPoint, PointDouble mouseEndPoint, bool shouldApplySnapping, bool shouldApplyConstraint, bool isEditingStartPoint, bool isEditingEndPoint, PaintDotNet.WhichUserColor whichUserColor, Matrix3x2Double transform, PointDouble? rotationAnchorOffset) : base(drawingSettingsValues)
        {
            this.shapeInfoSettingPath = shapeInfoSettingPath;
            this.InitializeAllShapePropertyValues(allShapePropertyValues);
            this.mouseStartPoint = mouseStartPoint;
            this.mouseEndPoint = mouseEndPoint;
            this.shouldApplySnapping = shouldApplySnapping;
            this.shouldApplyConstraint = shouldApplyConstraint;
            this.isEditingStartPoint = isEditingStartPoint;
            this.isEditingEndPoint = isEditingEndPoint;
            this.whichUserColor = whichUserColor;
            this.transform = transform;
            this.rotationAnchorOffset = rotationAnchorOffset;
            this.Initialize();
        }

        public ShapesToolChanges CloneWithNewRotationAnchorOffset(PointDouble? newRotationAnchorOffset)
        {
            PointDouble? nullable = newRotationAnchorOffset;
            PointDouble? rotationAnchorOffset = this.rotationAnchorOffset;
            if ((nullable.HasValue == rotationAnchorOffset.HasValue) ? (nullable.HasValue ? (nullable.GetValueOrDefault() == rotationAnchorOffset.GetValueOrDefault()) : true) : false)
            {
                return this;
            }
            ShapesToolChanges changes = (ShapesToolChanges) base.MemberwiseClone();
            using (changes.UseChangeScope())
            {
                changes.rotationAnchorOffset = this.rotationAnchorOffset;
            }
            return changes;
        }

        public ShapesToolChanges CloneWithNewTransform(Matrix3x2Double newTransform)
        {
            if (newTransform == this.transform)
            {
                return this;
            }
            if (!(this.transform.GetScale() == newTransform.GetScale()))
            {
                return new ShapesToolChanges(base.DrawingSettingsValues, this.ShapeInfoSettingPath, this.AllShapePropertyValues, this.MouseStartPoint, this.MouseEndPoint, this.ShouldApplySnapping, this.ShouldApplyConstraint, this.IsEditingStartPoint, this.IsEditingEndPoint, this.WhichUserColor, newTransform, this.RotationAnchorOffset);
            }
            Matrix3x2Double relativeTx = this.transform.Inverse * newTransform;
            ShapesToolChanges changes = (ShapesToolChanges) base.MemberwiseClone();
            using (changes.UseChangeScope())
            {
                changes.transform = newTransform;
                changes.shapeRenderData = LazyResult.New<PaintDotNet.Shapes.ShapeRenderData>(() => PaintDotNet.Shapes.ShapeRenderData.Transform(this.shapeRenderData.Value, relativeTx), LazyThreadSafetyMode.ExecutionAndPublication, new SingleUseCriticalSection());
                changes.InvalidateCachedMaxRenderBounds();
            }
            return changes;
        }

        private static void ConstrainPoints15Degrees(PointDouble a, PointDouble b, out PointDouble bP)
        {
            double num5;
            VectorDouble num = (VectorDouble) (b - a);
            double num2 = Math.Atan2(num.Y, num.X);
            double length = num.Length;
            double theta = (Math.Round((double) ((12.0 * num2) / 3.1415926535897931), MidpointRounding.AwayFromZero) * 3.1415926535897931) / 12.0;
            if (IsThetaMultipleOf90Degress(theta))
            {
                num5 = Math.Round(length, MidpointRounding.AwayFromZero);
            }
            else if (IsThetaMultipleOf45Degrees(theta))
            {
                num5 = Math.Round((double) (length / 1.4142135623730951), MidpointRounding.AwayFromZero) * 1.4142135623730951;
            }
            else
            {
                num5 = length;
            }
            double x = a.X + (num5 * Math.Cos(theta));
            double y = a.Y + (num5 * Math.Sin(theta));
            bP = new PointDouble(x, y);
        }

        private void ConstrainPointsToAspectRatio(PointDouble a, PointDouble b, out PointDouble bP)
        {
            double num6;
            double num7;
            int num = Math.Sign((double) (b.X - a.X));
            int num2 = Math.Sign((double) (b.Y - a.Y));
            double num3 = Math.Abs((double) (b.X - a.X));
            double num4 = Math.Abs((double) (b.Y - a.Y));
            double aspectRatio = this.Shape.AspectRatio;
            if (aspectRatio == 1.0)
            {
                num6 = Math.Min(num3, num4);
                num7 = num6;
            }
            else
            {
                num6 = num3;
                num7 = num3 / aspectRatio;
                if (num7 > num4)
                {
                    num6 = num4 * aspectRatio;
                    num7 = num4;
                }
            }
            double x = a.X + (num6 * num);
            double y = a.Y + (num7 * num2);
            bP = new PointDouble(x, y);
        }

        [IteratorStateMachine(typeof(<GetMaximumBoundsRects>d__111))]
        private IEnumerable<RectDouble> GetMaximumBoundsRects()
        {
            if (this.shapeRenderData.Value.InteriorFill != null)
            {
                yield return this.shapeRenderData.Value.InteriorFill.FastMaxBounds;
            }
            if (this.shapeRenderData.Value.OutlineFill != null)
            {
                yield return this.shapeRenderData.Value.OutlineFill.FastMaxBounds;
            }
            else if (this.shapeRenderData.Value.OutlineDraw != null)
            {
                yield return this.shapeRenderData.Value.OutlineDraw.GetRenderBounds(this.PenWidth, this.outlinePen);
            }
        }

        private void Initialize()
        {
            if ((this.allShapePropertyValuesMap == null) && (this.allShapePropertyValuesArray != null))
            {
                this.allShapePropertyValuesMap = ShapePropertyValuesUtil.ToReadOnlyMap(this.allShapePropertyValuesArray);
            }
            PaintDotNet.UI.Media.DashStyle style = DashStyleUtil.ToMedia(this.PenDashStyle);
            this.outlinePen = new Pen { 
                Brush = SolidColorBrushCache.Get((ColorRgba128Float) Colors.White),
                Thickness = this.PenWidth,
                LineJoin = PenLineJoin.Miter,
                DashStyle = style
            }.EnsureFrozen<Pen>();
            RectDouble baseBounds = this.BaseBounds;
            VectorDouble txScale = this.Transform.GetScale();
            VectorDouble num3 = (VectorDouble) (this.EndPoint - this.StartPoint);
            PointDouble endPoint = new PointDouble(this.StartPoint.X + (num3.X * txScale.X), this.StartPoint.Y + (num3.Y * txScale.Y));
            Dictionary<string, object> settingValues = (from gsp in this.Shape.RenderSettingPaths select KeyValuePairUtil.Create<string, object>(gsp, base.GetDrawingSettingValue(gsp))).ToDictionary<string, object>();
            PaintDotNet.Shapes.ShapeRenderParameters renderParams = new PaintDotNet.Shapes.ShapeRenderParameters(this.StartPoint, endPoint, txScale, settingValues, null);
            this.shapePropertySchema = this.Shape.CreatePropertyCollection(renderParams);
            foreach (Property property in this.shapePropertySchema)
            {
                property.ValueChanged += delegate (object s, ValueEventArgs<object> e) {
                    throw new ReadOnlyException();
                };
                property.ReadOnlyChanged += delegate (object s, ValueEventArgs<bool> e) {
                    throw new ReadOnlyException();
                };
            }
            this.shapeRenderParams = new PaintDotNet.Shapes.ShapeRenderParameters(this.StartPoint, endPoint, txScale, settingValues, this.ShapePropertyValues);
            this.shapeRenderData = LazyResult.New<PaintDotNet.Shapes.ShapeRenderData>(delegate {
                PaintDotNet.Shapes.ShapeRenderData renderData = this.Shape.CreateRenderData(this.shapeRenderParams);
                Matrix3x2Double matrix = Matrix3x2Double.ScalingAt(1.0 / Math.Abs(txScale.X), 1.0 / Math.Abs(txScale.Y), this.StartPoint.X, this.StartPoint.Y);
                return PaintDotNet.Shapes.ShapeRenderData.Transform(PaintDotNet.Shapes.ShapeRenderData.Transform(renderData, matrix), this.transform);
            }, LazyThreadSafetyMode.ExecutionAndPublication, new SingleUseCriticalSection());
        }

        private void InitializeAllShapePropertyValues(IEnumerable<KeyValuePair<PaintDotNet.Shapes.ShapeInfo, IDictionary<object, object>>> allShapePropertyValues)
        {
            if (allShapePropertyValues == null)
            {
                this.allShapePropertyValuesArray = null;
                this.allShapePropertyValuesMap = null;
            }
            else
            {
                this.allShapePropertyValuesArray = ShapePropertyValuesUtil.ToTable(allShapePropertyValues).ToArrayEx<TupleStruct<PaintDotNet.Shapes.ShapeInfo, object, object>>();
                this.allShapePropertyValuesMap = ShapePropertyValuesUtil.ToReadOnlyMap(this.allShapePropertyValuesArray);
            }
        }

        private static bool IsThetaMultipleOf45Degrees(double theta) => 
            (IsThetaMultipleOf90Degress(theta) || DoubleUtil.IsCloseToOne(Math.Abs(Math.Tan(theta))));

        private static bool IsThetaMultipleOf90Degress(double theta)
        {
            double x = Math.Abs(Math.Sin(theta));
            double num4 = Math.Abs(Math.Cos(theta));
            if (!DoubleUtil.IsCloseToZero(x) && !DoubleUtil.IsCloseToZero(num4))
            {
                return false;
            }
            return true;
        }

        protected override void OnDeserializedGraph()
        {
            this.Initialize();
            base.OnDeserializedGraph();
        }

        protected override RectInt32 OnGetMaxRenderBounds() => 
            RectDouble.Inflate(this.GetMaximumBoundsRects().Bounds(), 1.0, 1.0).Int32Bound;

        private static void SnapMouseCanvasCoordinates(double startT, double endT, out double snappedStartT, out double snappedEndT)
        {
            if (startT <= endT)
            {
                snappedStartT = Math.Floor(startT);
                snappedEndT = Math.Ceiling(endT);
            }
            else
            {
                snappedStartT = Math.Ceiling(startT);
                snappedEndT = Math.Floor(endT);
            }
        }

        private static void SnapMouseCanvasCoordinatesToRectangleCorners(PointDouble startPoint, PointDouble endPoint, out PointDouble snappedStartPoint, out PointDouble snappedEndPoint)
        {
            double num;
            double num2;
            double num3;
            double num4;
            SnapMouseCanvasCoordinates(startPoint.X, endPoint.X, out num, out num2);
            SnapMouseCanvasCoordinates(startPoint.Y, endPoint.Y, out num3, out num4);
            PointDouble num5 = new PointDouble(num, num3);
            PointDouble num6 = new PointDouble(num2, num4);
            snappedStartPoint = num5;
            snappedEndPoint = num6;
            if (snappedStartPoint.X == snappedEndPoint.X)
            {
                snappedEndPoint.X++;
            }
            if (snappedStartPoint.Y == snappedEndPoint.Y)
            {
                snappedEndPoint.Y++;
            }
        }

        private static void SnapMouseCanvasPoints(ShapeSnappingOption snappingOption, PointDouble startPoint, PointDouble endPoint, out PointDouble snappedStartPoint, out PointDouble snappedEndPoint)
        {
            switch (snappingOption)
            {
                case ShapeSnappingOption.None:
                    snappedStartPoint = startPoint;
                    snappedEndPoint = endPoint;
                    return;

                case ShapeSnappingOption.RectangleCorners:
                    SnapMouseCanvasCoordinatesToRectangleCorners(startPoint, endPoint, out snappedStartPoint, out snappedEndPoint);
                    return;

                case ShapeSnappingOption.PixelCenters:
                    snappedStartPoint = PointDouble.Truncate(startPoint) + new VectorDouble(0.5, 0.5);
                    snappedEndPoint = PointDouble.Truncate(endPoint) + new VectorDouble(0.5, 0.5);
                    return;
            }
            throw ExceptionUtil.InvalidEnumArgumentException<ShapeSnappingOption>(snappingOption, "snappingOption");
        }

        public IDictionary<PaintDotNet.Shapes.ShapeInfo, IDictionary<object, object>> AllShapePropertyValues =>
            this.allShapePropertyValuesMap;

        public bool Antialiasing =>
            base.GetDrawingSettingValue<bool>(ToolSettings.Null.Antialiasing);

        public ColorBgra BackgroundColor
        {
            get
            {
                if (this.WhichUserColor != PaintDotNet.WhichUserColor.Primary)
                {
                    return this.PrimaryColor;
                }
                return this.SecondaryColor;
            }
        }

        public RectDouble BaseBounds =>
            RectDouble.FromCorners(this.StartPoint, this.EndPoint);

        public ContentBlendMode BlendMode =>
            base.GetDrawingSettingValue<ContentBlendMode>(ToolSettings.Null.BlendMode);

        public PaintDotNet.BrushType BrushType =>
            base.GetDrawingSettingValue<PaintDotNet.BrushType>(ToolSettings.Null.Brush.Type);

        public PointDouble EndPoint
        {
            get
            {
                if (!this.ShouldApplyConstraint || !this.IsEditingEndPoint)
                {
                    return this.MouseEndPointSnapped;
                }
                switch (this.Shape.Options.Constraint)
                {
                    case ShapeConstraintOption.None:
                        return this.MouseEndPointSnapped;

                    case ShapeConstraintOption.AxisAlignedAspectRatio:
                        PointDouble num;
                        this.ConstrainPointsToAspectRatio(this.MouseStartPointSnapped, this.MouseEndPointSnapped, out num);
                        return num;

                    case ShapeConstraintOption.HypotenuseMultipleOf15Degrees:
                    {
                        PointDouble num6;
                        PointDouble mouseStartPointSnapped = this.MouseStartPointSnapped;
                        PointDouble mouseEndPointSnapped = this.MouseEndPointSnapped;
                        PointDouble a = this.Transform.Transform(mouseStartPointSnapped);
                        PointDouble b = this.Transform.Transform(mouseEndPointSnapped);
                        ConstrainPoints15Degrees(a, b, out num6);
                        return this.Transform.Inverse.Transform(num6);
                    }
                }
                throw ExceptionUtil.InvalidEnumArgumentException<ShapeConstraintOption>(this.Shape.Options.Constraint, "this.Shape.Options.Constraint");
            }
        }

        public ColorBgra ForegroundColor
        {
            get
            {
                if (this.WhichUserColor != PaintDotNet.WhichUserColor.Primary)
                {
                    return this.SecondaryColor;
                }
                return this.PrimaryColor;
            }
        }

        public bool HasAllShapePropertyValues =>
            (this.allShapePropertyValuesMap > null);

        public System.Drawing.Drawing2D.HatchStyle HatchStyle =>
            base.GetDrawingSettingValue<System.Drawing.Drawing2D.HatchStyle>(ToolSettings.Null.Brush.HatchStyle);

        public ColorBgra InteriorBackgroundColor
        {
            get
            {
                if ((this.ShapeDrawType & PaintDotNet.ShapeDrawType.Outline) != PaintDotNet.ShapeDrawType.Outline)
                {
                    return this.BackgroundColor;
                }
                return this.ForegroundColor;
            }
        }

        public ColorBgra InteriorForegroundColor
        {
            get
            {
                if ((this.ShapeDrawType & PaintDotNet.ShapeDrawType.Outline) != PaintDotNet.ShapeDrawType.Outline)
                {
                    return this.ForegroundColor;
                }
                return this.BackgroundColor;
            }
        }

        public bool IsEditingEndPoint =>
            this.isEditingEndPoint;

        public bool IsEditingStartPoint =>
            this.isEditingStartPoint;

        public PointDouble MouseEndPoint =>
            this.mouseEndPoint;

        public PointDouble MouseEndPointSnapped
        {
            get
            {
                if (this.ShouldApplySnapping)
                {
                    PointDouble num;
                    PointDouble num2;
                    SnapMouseCanvasPoints(this.Shape.Options.Snapping, this.MouseStartPoint, this.MouseEndPoint, out num, out num2);
                    return num2;
                }
                return this.MouseEndPoint;
            }
        }

        public PointDouble MouseStartPoint =>
            this.mouseStartPoint;

        public PointDouble MouseStartPointSnapped
        {
            get
            {
                if (this.ShouldApplySnapping)
                {
                    PointDouble num;
                    PointDouble num2;
                    SnapMouseCanvasPoints(this.Shape.Options.Snapping, this.MouseStartPoint, this.MouseEndPoint, out num, out num2);
                    return num;
                }
                return this.MouseStartPoint;
            }
        }

        public ColorBgra OutlineBackgroundColor =>
            this.BackgroundColor;

        public ColorBgra OutlineForegroundColor =>
            this.ForegroundColor;

        public Pen OutlinePen =>
            this.outlinePen;

        public System.Drawing.Drawing2D.DashStyle PenDashStyle =>
            base.GetDrawingSettingValue<System.Drawing.Drawing2D.DashStyle>(ToolSettings.Null.Pen.DashStyle);

        public LineCap2 PenEndCap =>
            base.GetDrawingSettingValue<LineCap2>(ToolSettings.Null.Pen.EndCap);

        public LineCap2 PenStartCap =>
            base.GetDrawingSettingValue<LineCap2>(ToolSettings.Null.Pen.StartCap);

        public float PenWidth =>
            base.GetDrawingSettingValue<float>(ToolSettings.Null.Pen.Width);

        public ColorBgra PrimaryColor =>
            base.GetDrawingSettingValue<ColorBgra32>(ToolSettings.Null.PrimaryColor);

        public float Radius =>
            base.GetDrawingSettingValue<float>(ToolSettings.Null.Radius);

        public PointDouble? RotationAnchorOffset =>
            this.rotationAnchorOffset;

        public ColorBgra SecondaryColor =>
            base.GetDrawingSettingValue<ColorBgra32>(ToolSettings.Null.SecondaryColor);

        public PaintDotNet.SelectionRenderingQuality SelectionRenderingQuality =>
            base.GetDrawingSettingValue<PaintDotNet.SelectionRenderingQuality>(ToolSettings.Null.Selection.RenderingQuality);

        public PaintDotNet.Shapes.Shape Shape =>
            ShapeManager.GetShape(this.ShapeInfo);

        public PaintDotNet.ShapeDrawType ShapeDrawType =>
            base.GetDrawingSettingValue<PaintDotNet.ShapeDrawType>(ToolSettings.Null.Shapes.DrawType);

        public PaintDotNet.Shapes.ShapeInfo ShapeInfo =>
            base.GetDrawingSettingValue<PaintDotNet.Shapes.ShapeInfo>(this.shapeInfoSettingPath);

        public string ShapeInfoSettingPath =>
            this.shapeInfoSettingPath;

        public PropertyCollection ShapePropertySchema =>
            this.shapePropertySchema;

        public IDictionary<object, object> ShapePropertyValues
        {
            get
            {
                IDictionary<object, object> dictionary;
                if (!this.HasAllShapePropertyValues)
                {
                    return null;
                }
                if (!this.allShapePropertyValuesMap.TryGetValue(this.ShapeInfo, out dictionary))
                {
                    return emptyShapePropertyValuesRO;
                }
                return dictionary;
            }
        }

        public PaintDotNet.Shapes.ShapeRenderData ShapeRenderData =>
            this.shapeRenderData.Value;

        public PaintDotNet.Shapes.ShapeRenderParameters ShapeRenderParameters =>
            this.shapeRenderParams;

        public bool ShouldApplyConstraint =>
            ((this.Shape.Options.Constraint != ShapeConstraintOption.None) && this.shouldApplyConstraint);

        public bool ShouldApplySnapping =>
            ((this.Shape.Options.Snapping != ShapeSnappingOption.None) && this.shouldApplySnapping);

        public PointDouble StartPoint
        {
            get
            {
                if (!this.ShouldApplyConstraint || !this.IsEditingStartPoint)
                {
                    return this.MouseStartPointSnapped;
                }
                switch (this.Shape.Options.Constraint)
                {
                    case ShapeConstraintOption.None:
                        return this.MouseStartPointSnapped;

                    case ShapeConstraintOption.AxisAlignedAspectRatio:
                        PointDouble num;
                        this.ConstrainPointsToAspectRatio(this.MouseEndPointSnapped, this.MouseStartPointSnapped, out num);
                        return num;

                    case ShapeConstraintOption.HypotenuseMultipleOf15Degrees:
                    {
                        PointDouble num6;
                        PointDouble mouseStartPointSnapped = this.MouseStartPointSnapped;
                        PointDouble mouseEndPointSnapped = this.MouseEndPointSnapped;
                        PointDouble b = this.Transform.Transform(mouseStartPointSnapped);
                        ConstrainPoints15Degrees(this.Transform.Transform(mouseEndPointSnapped), b, out num6);
                        return this.Transform.Inverse.Transform(num6);
                    }
                }
                throw ExceptionUtil.InvalidEnumArgumentException<ShapeConstraintOption>(this.Shape.Options.Constraint, "this.Shape.Options.Constraint");
            }
        }

        public Matrix3x2Double Transform =>
            this.transform;

        public PaintDotNet.WhichUserColor WhichUserColor =>
            this.whichUserColor;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly ShapesToolChanges.<>c <>9 = new ShapesToolChanges.<>c();
            public static ValueEventHandler<object> <>9__117_1;
            public static ValueEventHandler<bool> <>9__117_2;

            internal void <Initialize>b__117_1(object s, ValueEventArgs<object> e)
            {
                throw new ReadOnlyException();
            }

            internal void <Initialize>b__117_2(object s, ValueEventArgs<bool> e)
            {
                throw new ReadOnlyException();
            }
        }

    }
}

