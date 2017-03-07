namespace PaintDotNet.Rendering
{
    using System;
    using System.Runtime.InteropServices;

    internal interface IMaskFromScansRenderer
    {
        void Render(ISurfaceAllocator<ColorAlpha8> allocator, int dstWidth, int dstHeight, PointInt32 renderOffset, ref ISurface<ColorAlpha8> dst, out int? fill255Count);
    }
}

