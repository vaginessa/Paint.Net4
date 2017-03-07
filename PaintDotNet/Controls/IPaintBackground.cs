namespace PaintDotNet.Controls
{
    using System;
    using System.Drawing;

    internal interface IPaintBackground
    {
        void PaintBackground(Graphics g, Rectangle clipRect);
    }
}

