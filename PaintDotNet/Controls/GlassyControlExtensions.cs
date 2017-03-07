namespace PaintDotNet.Controls
{
    using System;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;

    internal static class GlassyControlExtensions
    {
        public static Rectangle GlassInsetBottomRect(this IGlassyControl control)
        {
            Size clientSize = control.ClientSize;
            Padding glassInset = control.GlassInset;
            return new Rectangle(0, clientSize.Height - glassInset.Bottom, clientSize.Width, glassInset.Bottom);
        }

        public static Rectangle GlassInsetLeftRect(this IGlassyControl control)
        {
            Padding glassInset = control.GlassInset;
            return new Rectangle(0, 0, glassInset.Left, control.ClientSize.Height);
        }

        public static Rectangle GlassInsetRightRect(this IGlassyControl control)
        {
            Size clientSize = control.ClientSize;
            Padding glassInset = control.GlassInset;
            return new Rectangle(clientSize.Width - glassInset.Right, 0, glassInset.Right, clientSize.Height);
        }

        public static Rectangle GlassInsetTopRect(this IGlassyControl control)
        {
            Padding glassInset = control.GlassInset;
            return new Rectangle(0, 0, control.ClientSize.Width, glassInset.Top);
        }
    }
}

