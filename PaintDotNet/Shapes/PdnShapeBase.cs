namespace PaintDotNet.Shapes
{
    using PaintDotNet.Diagnostics;
    using System;

    internal abstract class PdnShapeBase : Shape
    {
        protected PdnShapeBase(string displayName, ShapeCategory category) : base(displayName, category)
        {
        }

        protected PdnShapeBase(string displayName, ShapeCategory category, ShapeOptions options) : base(displayName, category, options)
        {
        }

        public static string GetShapeID<TShape>() where TShape: PdnShapeBase => 
            typeof(TShape).FullName;

        public static string GetShapeID(Type shapeType)
        {
            Validate.IsNotNull<Type>(shapeType, "shapeType");
            if (shapeType.IsAbstract || !typeof(PdnShapeBase).IsAssignableFrom(shapeType))
            {
                throw new ArgumentException($"shapeType ({shapeType.FullName})");
            }
            return shapeType.FullName;
        }

        public static ShapeInfo GetShapeInfo<TShape>() where TShape: PdnShapeBase => 
            new ShapeInfo(typeof(PdnShapeFactory), GetShapeID<TShape>());

        public static ShapeInfo GetShapeInfo(Type shapeType)
        {
            Validate.IsNotNull<Type>(shapeType, "shapeType");
            return new ShapeInfo(typeof(PdnShapeFactory), GetShapeID(shapeType));
        }

        public sealed override string ID =>
            GetShapeID(base.GetType());

        public sealed override string ToolTipText =>
            base.DisplayName;
    }
}

