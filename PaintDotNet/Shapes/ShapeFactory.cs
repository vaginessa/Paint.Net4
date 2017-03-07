namespace PaintDotNet.Shapes
{
    using System;
    using System.Collections.Generic;

    internal abstract class ShapeFactory
    {
        protected ShapeFactory()
        {
        }

        public abstract Shape CreateShape(string id);
        public abstract IEnumerable<string> GetShapeIDs();
    }
}

