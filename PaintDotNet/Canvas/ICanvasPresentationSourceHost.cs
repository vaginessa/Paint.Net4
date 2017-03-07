namespace PaintDotNet.Canvas
{
    using PaintDotNet.Rendering;
    using PaintDotNet.UI.Input;
    using System;

    internal interface ICanvasPresentationSourceHost
    {
        bool RequestFocus();
        void Update();

        SizeDouble CanvasSize { get; }

        PaintDotNet.UI.Input.Cursor Cursor { set; }

        bool IsFocused { get; }
    }
}

