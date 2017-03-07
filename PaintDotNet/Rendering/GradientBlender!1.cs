namespace PaintDotNet.Rendering
{
    using System;
    using System.Runtime.CompilerServices;

    internal abstract class GradientBlender<TPixel> where TPixel: struct, INaturalPixelInfo
    {
        protected GradientBlender()
        {
        }

        public abstract TPixel GetGradientValue(double lerp, uint pixelId);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static uint XorShift(uint h)
        {
            h ^= h << 13;
            h ^= h >> 0x11;
            h ^= h << 5;
            return h;
        }
    }
}

