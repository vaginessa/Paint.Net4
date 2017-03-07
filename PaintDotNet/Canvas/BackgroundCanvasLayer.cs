namespace PaintDotNet.Canvas
{
    using PaintDotNet.Direct2D;
    using PaintDotNet.Imaging;
    using PaintDotNet.Rendering;
    using PaintDotNet.VisualStyling;
    using System;

    internal sealed class BackgroundCanvasLayer : CanvasLayer
    {
        protected override void OnRender(IDrawingContext dc, RectFloat clipRect, CanvasView canvasView)
        {
            SizeDouble canvasSize = canvasView.CanvasSize;
            RectDouble num2 = RectDouble.FromEdges(-131072.0, 0.0, 0.0, canvasSize.Height);
            RectDouble num3 = RectDouble.FromEdges(-131072.0, -131072.0, 131072.0, 0.0);
            RectDouble num4 = RectDouble.FromEdges(canvasSize.Width, 0.0, 131072.0, canvasSize.Height);
            RectDouble num5 = RectDouble.FromEdges(-131072.0, canvasSize.Height, 131072.0, 131072.0);
            ColorRgba128Float num6 = (ThemeConfig.EffectiveTheme == PdnTheme.Aero) ? AeroColors.CanvasBackFillColor : ClassicColors.CanvasBackFillColor;
            dc.Clear((RectFloat) num2, AntialiasMode.Aliased, new ColorRgba128Float?(num6));
            dc.Clear((RectFloat) num3, AntialiasMode.Aliased, new ColorRgba128Float?(num6));
            dc.Clear((RectFloat) num4, AntialiasMode.Aliased, new ColorRgba128Float?(num6));
            dc.Clear((RectFloat) num5, AntialiasMode.Aliased, new ColorRgba128Float?(num6));
            base.OnRender(dc, clipRect, canvasView);
        }
    }
}

