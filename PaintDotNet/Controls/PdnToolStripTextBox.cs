namespace PaintDotNet.Controls
{
    using System;
    using System.Windows.Forms;

    internal class PdnToolStripTextBox : ToolStripTextBox
    {
        public PdnToolStripTextBox()
        {
            base.BorderStyle = BorderStyle.FixedSingle;
        }
    }
}

