namespace PaintDotNet.Controls
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    internal class ImageStripItemMovingEventArgs : CancelEventArgs
    {
        public ImageStripItemMovingEventArgs(PaintDotNet.Controls.ImageStrip.Item item, int oldIndex, int newIndex, bool cancel) : base(cancel)
        {
            this.Item = item;
            this.OldIndex = oldIndex;
            this.NewIndex = newIndex;
        }

        public PaintDotNet.Controls.ImageStrip.Item Item { get; private set; }

        public int NewIndex { get; private set; }

        public int OldIndex { get; private set; }
    }
}

