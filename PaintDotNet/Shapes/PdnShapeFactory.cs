namespace PaintDotNet.Shapes
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    internal sealed class PdnShapeFactory : ShapeFactory
    {
        public override Shape CreateShape(string id) => 
            ((Shape) Activator.CreateInstance(Type.GetType(id, true)));

        [IteratorStateMachine(typeof(<GetShapeIDs>d__2))]
        public override IEnumerable<string> GetShapeIDs()
        {
            yield return PdnShapeBase.GetShapeID<LineCurveShape>();
            yield return PdnShapeBase.GetShapeID<RectangleShape>();
            yield return PdnShapeBase.GetShapeID<RoundedRectangleShape>();
            yield return PdnShapeBase.GetShapeID<EllipseShape>();
            yield return PdnShapeBase.GetShapeID<DiamondShape>();
            yield return PdnShapeBase.GetShapeID<TrapezoidShape>();
            yield return PdnShapeBase.GetShapeID<ParallelogramShape>();
            yield return PdnShapeBase.GetShapeID<TriangleShape>();
            yield return PdnShapeBase.GetShapeID<RightTriangleShape>();
            yield return PdnShapeBase.GetShapeID<PentagonShape>();
            yield return PdnShapeBase.GetShapeID<HexagonShape>();
            yield return PdnShapeBase.GetShapeID<Star3PointShape>();
            yield return PdnShapeBase.GetShapeID<Star4PointShape>();
            yield return PdnShapeBase.GetShapeID<Star5PointShape>();
            yield return PdnShapeBase.GetShapeID<Star6PointShape>();
            yield return PdnShapeBase.GetShapeID<BlockArrowShape>();
            yield return PdnShapeBase.GetShapeID<NotchedArrowShape>();
            yield return PdnShapeBase.GetShapeID<PentagonArrowShape>();
            yield return PdnShapeBase.GetShapeID<ChevronArrowShape>();
            yield return PdnShapeBase.GetShapeID<RectangularCalloutShape>();
            yield return PdnShapeBase.GetShapeID<RoundedRectangularCalloutShape>();
            yield return PdnShapeBase.GetShapeID<EllipticalCalloutShape>();
            yield return PdnShapeBase.GetShapeID<CloudCalloutShape>();
            yield return PdnShapeBase.GetShapeID<LightningBoltShape>();
            yield return PdnShapeBase.GetShapeID<CheckMarkShape>();
            yield return PdnShapeBase.GetShapeID<MultiplyShape>();
            yield return PdnShapeBase.GetShapeID<GearShape>();
            yield return PdnShapeBase.GetShapeID<HeartShape>();
        }

    }
}

