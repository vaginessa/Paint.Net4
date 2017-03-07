namespace PaintDotNet.Rendering
{
    using PaintDotNet;
    using PaintDotNet.ObjectModel;
    using System;

    internal abstract class RendererSource<TPixel> : DependencyResource where TPixel: struct, INaturalPixelInfo
    {
        protected RendererSource()
        {
        }

        public IRenderer<TPixel> CreateRenderer(int width, int height)
        {
            base.VerifyAccess();
            IRenderer<TPixel> renderer = this.OnCreateRenderer(width, height);
            if ((renderer.Width != width) || (renderer.Height != height))
            {
                throw new InternalErrorException($"Created renderer's size ({new SizeInt32(renderer.Width, renderer.Height)}) does not match requested size ({new SizeInt32(width, height)})");
            }
            return renderer;
        }

        protected abstract IRenderer<TPixel> OnCreateRenderer(int width, int height);
    }
}

