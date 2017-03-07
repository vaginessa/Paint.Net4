namespace PaintDotNet.Controls
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal interface IGlassyControl
    {
        void SetGlassWndProcFilter(IMessageFilter wndProcFilter);

        Size ClientSize { get; }

        Padding GlassCaptionDragInset { get; }

        Padding GlassInset { get; }

        bool IsGlassDesired { get; }
    }
}

