namespace PaintDotNet.Controls
{
    using System;
    using System.Windows.Forms;

    internal abstract class UserControl2 : UserControl
    {
        protected UserControl2()
        {
        }

        public abstract bool IsMouseCaptured();
    }
}

