namespace PaintDotNet.IndirectUI
{
    using System;

    internal abstract class PropertyControl<TValue, TProperty> : PropertyControl where TProperty: Property<TValue>
    {
        internal PropertyControl(PropertyControlInfo propInfo) : base(propInfo)
        {
        }

        public TProperty Property =>
            ((TProperty) base.Property);
    }
}

