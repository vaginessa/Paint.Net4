namespace PaintDotNet.Rendering
{
    using System;
    using System.Runtime.CompilerServices;

    internal static class PdnBrushExtensions
    {
        public static IRenderer<ColorBgra> CreateRenderer(this PdnBrush brush, SizeInt32 size) => 
            brush.CreateRenderer(size.Width, size.Height);
    }
}

