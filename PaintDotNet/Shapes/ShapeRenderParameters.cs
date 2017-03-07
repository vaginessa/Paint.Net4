namespace PaintDotNet.Shapes
{
    using PaintDotNet.Collections;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Rendering;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal sealed class ShapeRenderParameters
    {
        public ShapeRenderParameters(PointDouble startPoint, PointDouble endPoint, VectorDouble transformScale, IDictionary<string, object> settingValues, IDictionary<object, object> propertyValues)
        {
            Validate.Begin().IsFinite(startPoint.X, "startPoint.X").IsFinite(startPoint.Y, "startPoint.Y").IsFinite(endPoint.X, "endPoint.X").IsFinite(endPoint.Y, "endPoint.Y").IsNotNull<IDictionary<string, object>>(settingValues, "settingValues").Check();
            this.StartPoint = startPoint;
            this.EndPoint = endPoint;
            this.TransformScale = transformScale;
            this.SettingValues = settingValues.ToDictionary<string, object>().AsReadOnly<string, object>();
            if (propertyValues == null)
            {
                this.PropertyValues = null;
            }
            else
            {
                this.PropertyValues = propertyValues.ToDictionary<object, object>().AsReadOnly<object, object>();
            }
        }

        public RectDouble BaseBounds =>
            RectDouble.FromCorners(this.StartPoint, this.EndPoint);

        public PointDouble EndPoint { get; private set; }

        public bool HasPropertyValues =>
            (this.PropertyValues > null);

        public IDictionary<object, object> PropertyValues { get; private set; }

        public IDictionary<string, object> SettingValues { get; private set; }

        public PointDouble StartPoint { get; private set; }

        public VectorDouble TransformScale { get; private set; }
    }
}

