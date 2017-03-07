namespace PaintDotNet.Controls
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal sealed class TransparentCheckBox : CheckBox
    {
        public TransparentCheckBox()
        {
            base.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            base.SetStyle(ControlStyles.Opaque, false);
            this.BackColor = Color.Transparent;
        }
    }
}

