namespace PaintDotNet.AppModel
{
    using PaintDotNet;
    using PaintDotNet.Direct2D;
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal static class RenderingPriorityManager
    {
        private static PaintDotNet.Direct2D.RenderingPriority renderingPriority = PaintDotNet.Direct2D.RenderingPriority.Normal;
        private static readonly object sync = new object();

        [field: CompilerGenerated]
        public static  event ValueChangedEventHandler<PaintDotNet.Direct2D.RenderingPriority> RenderingPriorityChanged;

        public static PaintDotNet.Direct2D.RenderingPriority RenderingPriority
        {
            get
            {
                object sync = RenderingPriorityManager.sync;
                lock (sync)
                {
                    return renderingPriority;
                }
            }
            set
            {
                object sync = RenderingPriorityManager.sync;
                lock (sync)
                {
                    PaintDotNet.Direct2D.RenderingPriority renderingPriority = RenderingPriorityManager.renderingPriority;
                    if (renderingPriority != value)
                    {
                        RenderingPriorityManager.renderingPriority = value;
                        RenderingPriorityChanged.Raise<PaintDotNet.Direct2D.RenderingPriority>(null, renderingPriority, value);
                    }
                }
            }
        }
    }
}

