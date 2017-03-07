namespace PaintDotNet.Controls.ToolConfigUI
{
    using System;
    using System.Windows.Forms;

    internal class ShapeCategoryLabel : ToolStripLabel
    {
        public ShapeCategoryLabel(string text) : base(" " + text)
        {
        }

        protected override Padding DefaultPadding =>
            new Padding(0, 0, 0, 2);
    }
}

