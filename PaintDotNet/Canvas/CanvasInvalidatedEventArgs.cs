namespace PaintDotNet.Canvas
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class CanvasInvalidatedEventArgs : PooledEventArgs<CanvasInvalidatedEventArgs, CalculateInvalidRectCallback, RectDouble>
    {
        private static readonly CalculateInvalidRectCallback identityCallback = new CalculateInvalidRectCallback(<>c.<>9.<.cctor>b__9_0);

        public RectDouble GetInvalidCanvasRect(CanvasView canvasView) => 
            this.Callback(canvasView, this.CanvasRect);

        public CalculateInvalidRectCallback Callback =>
            base.Value1;

        public RectDouble CanvasRect =>
            base.Value2;

        public static CalculateInvalidRectCallback IdentityCallback =>
            identityCallback;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly CanvasInvalidatedEventArgs.<>c <>9 = new CanvasInvalidatedEventArgs.<>c();

            internal RectDouble <.cctor>b__9_0(CanvasView cvi, RectDouble r) => 
                r;
        }
    }
}

