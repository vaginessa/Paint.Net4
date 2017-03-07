namespace PaintDotNet.Tools.MagicWand
{
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Rendering;
    using PaintDotNet.Tools;
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class MagicWandToolCreateGeometryContext : AsyncSelectionToolCreateGeometryContext
    {
        public MagicWandToolCreateGeometryContext(IRenderer<ColorBgra> sampleSource)
        {
            Validate.IsNotNull<IRenderer<ColorBgra>>(sampleSource, "sampleSource");
            this.SampleSource = sampleSource;
        }

        public IRenderer<ColorBgra> SampleSource { get; private set; }
    }
}

