namespace PaintDotNet.Controls
{
    using System.Drawing;
    using System.Windows.Forms;

    internal sealed class PdnToolStripSeparator : ToolStripSeparator
    {
        public override Size GetPreferredSize(Size constrainingSize)
        {
            Size preferredSize = base.GetPreferredSize(constrainingSize);
            return new Size(11, preferredSize.Height);
        }
    }
}

