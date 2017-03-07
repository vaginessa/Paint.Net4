namespace PaintDotNet
{
    using System;

    internal enum PixelSampleMode
    {
        Average11x11 = 11,
        Average31x31 = 0x1f,
        Average3x3 = 3,
        Average51x51 = 0x33,
        Average5x5 = 5,
        PointSample = 1
    }
}

