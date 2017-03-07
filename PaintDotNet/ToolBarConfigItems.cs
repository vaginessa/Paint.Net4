namespace PaintDotNet
{
    using System;

    [Flags]
    internal enum ToolBarConfigItems : uint
    {
        All = 0xffffffff,
        Antialiasing = 1,
        BlendMode = 2,
        Brush = 4,
        ColorPickerBehavior = 8,
        FloodMode = 0x10,
        Gradient = 0x20,
        LineCurveShapeType = 0x40,
        None = 0,
        PenDashStyle = 0x80,
        PenEndCap = 0x100,
        PenHardness = 0x200,
        PenStartCap = 0x400,
        PenWidth = 0x800,
        PixelSampleMode = 0x1000,
        Radius = 0x2000,
        RecolorToolSamplingMode = 0x4000,
        Resampling = 0x200000,
        SampleImageOrLayer = 0x8000,
        SelectionCombineMode = 0x10000,
        SelectionDrawMode = 0x20000,
        SelectionRenderingQuality = 0x40000,
        ShapeDrawType = 0x80000,
        ShapeType = 0x100000,
        Text = 0x400000,
        Tolerance = 0x800000
    }
}

