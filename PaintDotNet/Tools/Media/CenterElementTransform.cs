namespace PaintDotNet.Tools.Media
{
    using PaintDotNet.ObjectModel;
    using PaintDotNet.Rendering;
    using PaintDotNet.UI;
    using PaintDotNet.UI.Media;
    using System;
    using System.Runtime.CompilerServices;
    using System.Windows;

    internal sealed class CenterElementTransform : Transform
    {
        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register("Target", typeof(PaintDotNet.UI.FrameworkElement), typeof(CenterElementTransform), new PropertyMetadata(null));
        private TranslateTransform transform;

        public CenterElementTransform()
        {
            this.transform = new TranslateTransform();
            this.transform.SetBinding(TranslateTransform.XProperty, this, PropertyPathUtil.Combine(TargetProperty, PaintDotNet.UI.FrameworkElement.ActualWidthProperty), BindingMode.OneWay, x => -((double) x) / 2.0);
            this.transform.SetBinding(TranslateTransform.YProperty, this, PropertyPathUtil.Combine(TargetProperty, PaintDotNet.UI.FrameworkElement.ActualHeightProperty), BindingMode.OneWay, y => -((double) y) / 2.0);
            base.OnFreezablePropertyChanged(null, this.transform);
        }

        public CenterElementTransform(PaintDotNet.UI.FrameworkElement target) : this()
        {
            this.Target = target;
        }

        protected override Freezable CreateInstanceCore() => 
            new CenterElementTransform();

        public override bool HasInverse =>
            this.transform.HasInverse;

        public override Transform Inverse =>
            this.transform.Inverse;

        public override bool IsIdentity =>
            this.transform.IsIdentity;

        public PaintDotNet.UI.FrameworkElement Target
        {
            get => 
                ((PaintDotNet.UI.FrameworkElement) base.GetValue(TargetProperty));
            set
            {
                base.SetValue(TargetProperty, value);
            }
        }

        public override Matrix3x2Double Value =>
            this.transform.Value;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly CenterElementTransform.<>c <>9 = new CenterElementTransform.<>c();
            public static Func<object, object> <>9__1_0;
            public static Func<object, object> <>9__1_1;

            internal object <.ctor>b__1_0(object x) => 
                (-((double) x) / 2.0);

            internal object <.ctor>b__1_1(object y) => 
                (-((double) y) / 2.0);
        }
    }
}

