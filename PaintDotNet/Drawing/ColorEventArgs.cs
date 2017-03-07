namespace PaintDotNet.Drawing
{
    using PaintDotNet;
    using System;

    [Serializable]
    internal class ColorEventArgs : EventArgs
    {
        private ColorBgra color;

        public ColorEventArgs(ColorBgra color)
        {
            this.color = color;
        }

        public ColorBgra Color =>
            this.color;
    }
}

