namespace PaintDotNet.Controls
{
    using System;
    using System.Runtime.CompilerServices;

    internal class VerticalImageStripItemMovedEventArgs : EventArgs
    {
        public VerticalImageStripItemMovedEventArgs(PaintDotNet.Controls.VerticalImageStrip.Item item, int oldIndex, int newIndex)
        {
            this.Item = item;
            this.OldIndex = oldIndex;
            this.NewIndex = newIndex;
        }

        public PaintDotNet.Controls.VerticalImageStrip.Item Item { get; private set; }

        public int NewIndex { get; private set; }

        public int OldIndex { get; private set; }
    }
}

