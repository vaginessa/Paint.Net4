namespace PaintDotNet.Shapes
{
    using PaintDotNet;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal sealed class ShapeOptions : IEquatable<ShapeOptions>
    {
        private static readonly ShapeOptions defaultInstance = new ShapeOptions(ShapeConstraintOption.AxisAlignedAspectRatio, ShapeElideOption.ZeroWidthOrZeroHeight, ShapeTransformOption.FullTransform, ShapeSnappingOption.RectangleCorners);

        public ShapeOptions(ShapeConstraintOption constraint = 1, ShapeElideOption elide = 2, ShapeTransformOption transform = 2, ShapeSnappingOption snapping = 1)
        {
            this.Constraint = constraint;
            this.Elide = elide;
            this.Transform = transform;
            this.Snapping = snapping;
        }

        public bool Equals(ShapeOptions other)
        {
            if (other == null)
            {
                return false;
            }
            return ((((this.Constraint == other.Constraint) && (this.Elide == other.Elide)) && (this.Transform == other.Transform)) && (this.Snapping == other.Snapping));
        }

        public override bool Equals(object obj) => 
            EquatableUtil.Equals<ShapeOptions, object>(this, obj);

        public override int GetHashCode() => 
            HashCodeUtil.CombineHashCodes((int) this.Constraint, (int) this.Elide, (int) this.Transform, (int) this.Snapping);

        public ShapeConstraintOption Constraint { get; private set; }

        public static ShapeOptions Default =>
            defaultInstance;

        public ShapeElideOption Elide { get; private set; }

        public ShapeSnappingOption Snapping { get; private set; }

        public ShapeTransformOption Transform { get; private set; }
    }
}

