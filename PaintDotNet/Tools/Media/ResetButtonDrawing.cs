namespace PaintDotNet.Tools.Media
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.ObjectModel;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.UI.Media;
    using System;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;

    internal sealed class ResetButtonDrawing : HandleDrawing
    {
        private VisualStyleRendererDrawing buttonBackgroundDrawing = new VisualStyleRendererDrawing();
        private ControlPaintButtonDrawing buttonBackgroundDrawing2;
        public static readonly DependencyProperty ButtonStateProperty = DependencyProperty.Register("ButtonState", typeof(PdnPushButtonState), typeof(ResetButtonDrawing), new PropertyMetadata(EnumUtil.GetBoxed<PdnPushButtonState>(PdnPushButtonState.Default)));
        private const int iconPadding = 2;
        private const int iconSize = 0x10;
        private DeviceBitmapDrawing resetIconDrawing;
        private DrawingGroup resetIconDrawingContainer;

        public ResetButtonDrawing()
        {
            this.buttonBackgroundDrawing.ClassName = VisualStyleElement.Button.PushButton.Normal.ClassName;
            this.buttonBackgroundDrawing.Part = VisualStyleElement.Button.PushButton.Normal.Part;
            this.buttonBackgroundDrawing.SetBinding<PdnPushButtonState, int>(VisualStyleRendererDrawing.StateProperty, this, new PaintDotNet.ObjectModel.PropertyPath(ButtonStateProperty.Name, Array.Empty<object>()), BindingMode.OneWay, ppbs => (int) ppbs);
            this.buttonBackgroundDrawing2 = new ControlPaintButtonDrawing();
            this.buttonBackgroundDrawing2.SetBinding<PdnPushButtonState, System.Windows.Forms.ButtonState>(ControlPaintButtonDrawing.StateProperty, this, new PaintDotNet.ObjectModel.PropertyPath(ButtonStateProperty.Name, Array.Empty<object>()), BindingMode.OneWay, new Func<PdnPushButtonState, System.Windows.Forms.ButtonState>(ResetButtonDrawing.ConvertPdnPushButtonStateToButtonState));
            this.buttonBackgroundDrawing.FallbackDrawing = this.buttonBackgroundDrawing2;
            this.resetIconDrawing = new DeviceBitmapDrawing(ImageResourceUtil.GetDeviceBitmap(PdnResources.GetImageResource("Icons.ResetIcon.png")));
            this.resetIconDrawingContainer = new DrawingGroup();
            Transform[] items = new Transform[] { new ScaleTransform(16.0 / this.resetIconDrawing.Bounds.Width, 16.0 / this.resetIconDrawing.Bounds.Height), new TranslateTransform(2.0, 2.0) };
            this.resetIconDrawingContainer.Transform = new TransformGroup(ArrayUtil.Infer<Transform>(items));
            this.resetIconDrawingContainer.Children.Add(this.resetIconDrawing);
            this.buttonBackgroundDrawing.Size = new SizeInt32(((int) Math.Round(this.resetIconDrawingContainer.Bounds.Width, MidpointRounding.AwayFromZero)) + 4, ((int) Math.Round(this.resetIconDrawingContainer.Bounds.Height, MidpointRounding.AwayFromZero)) + 4);
            base.DrawingGroup.Children.Add(this.buttonBackgroundDrawing);
            base.DrawingGroup.Children.Add(this.resetIconDrawingContainer);
        }

        private static System.Windows.Forms.ButtonState ConvertPdnPushButtonStateToButtonState(PdnPushButtonState state)
        {
            switch (state)
            {
                case PdnPushButtonState.Normal:
                    return System.Windows.Forms.ButtonState.Normal;

                case PdnPushButtonState.Hot:
                    return System.Windows.Forms.ButtonState.Normal;

                case PdnPushButtonState.Pressed:
                    return System.Windows.Forms.ButtonState.Pushed;

                case PdnPushButtonState.Disabled:
                    return System.Windows.Forms.ButtonState.Inactive;

                case PdnPushButtonState.Default:
                case PdnPushButtonState.DefaultAnimate:
                    return System.Windows.Forms.ButtonState.Normal;
            }
            return System.Windows.Forms.ButtonState.Normal;
        }

        public PdnPushButtonState ButtonState
        {
            get => 
                ((PdnPushButtonState) base.GetValue(ButtonStateProperty));
            set
            {
                base.SetValue(ButtonStateProperty, EnumUtil.GetBoxed<PdnPushButtonState>(value));
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly ResetButtonDrawing.<>c <>9 = new ResetButtonDrawing.<>c();
            public static Func<PdnPushButtonState, int> <>9__6_0;

            internal int <.ctor>b__6_0(PdnPushButtonState ppbs) => 
                ((int) ppbs);
        }
    }
}

