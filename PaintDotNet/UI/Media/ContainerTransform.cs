namespace PaintDotNet.UI.Media
{
    using PaintDotNet.Rendering;
    using System;
    using System.Windows;

    internal sealed class ContainerTransform : PaintDotNet.UI.Media.Transform
    {
        public static readonly DependencyProperty TransformProperty = DependencyProperty.Register("Transform", typeof(PaintDotNet.UI.Media.Transform), typeof(ContainerTransform), new PropertyMetadata(null));

        public ContainerTransform()
        {
        }

        public ContainerTransform(PaintDotNet.UI.Media.Transform transform)
        {
            this.Transform = transform;
        }

        protected override Freezable CreateInstanceCore() => 
            new ContainerTransform();

        public PaintDotNet.UI.Media.Transform Transform
        {
            get => 
                ((PaintDotNet.UI.Media.Transform) base.GetValue(TransformProperty));
            set
            {
                base.SetValue(TransformProperty, value);
            }
        }

        public override Matrix3x2Double Value
        {
            get
            {
                PaintDotNet.UI.Media.Transform transform = this.Transform;
                return transform?.Value;
            }
        }
    }
}

