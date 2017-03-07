namespace PaintDotNet.Tools.Media
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using System;
    using System.Drawing;
    using System.Windows;
    using System.Windows.Forms;

    internal sealed class ControlPaintButtonDrawing : GdiPlusDrawing
    {
        public static readonly DependencyProperty StateProperty = DependencyProperty.Register("State", typeof(ButtonState), typeof(ControlPaintButtonDrawing), new PropertyMetadata(EnumUtil.GetBoxed<ButtonState>(ButtonState.Normal)));

        protected override Freezable CreateInstanceCore() => 
            new ControlPaintButtonDrawing();

        protected override void OnDraw(Graphics g)
        {
            SizeInt32 size = base.Size;
            ControlPaint.DrawButton(g, new Rectangle(0, 0, size.Width, size.Height), this.State);
        }

        public ButtonState State
        {
            get => 
                ((ButtonState) base.GetValue(StateProperty));
            set
            {
                base.SetValue(StateProperty, EnumUtil.GetBoxed<ButtonState>(value));
            }
        }
    }
}

