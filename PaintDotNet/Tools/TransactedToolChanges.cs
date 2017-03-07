namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using System;

    [Serializable]
    internal abstract class TransactedToolChanges : ReferenceValue
    {
        private static readonly RectInt32 maxMaxRenderBounds = new RectInt32(0, 0, 0x10000, 0x10000);

        protected TransactedToolChanges()
        {
        }

        public static RectInt32 MaxMaxRenderBounds =>
            maxMaxRenderBounds;
    }
}

