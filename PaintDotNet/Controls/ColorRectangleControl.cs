namespace PaintDotNet.Controls
{
    using PaintDotNet.VisualStyling;
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal class ColorRectangleControl : UserControl
    {
        private Color rectangleColor;

        public ColorRectangleControl()
        {
            base.ResizeRedraw = true;
            this.DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            ColorRectangle.Draw(e.Graphics, base.ClientRectangle, this.rectangleColor, base.Enabled, true);
        }

        public Color RectangleColor
        {
            get => 
                this.rectangleColor;
            set
            {
                this.rectangleColor = value;
                base.Invalidate(true);
            }
        }
    }
}

