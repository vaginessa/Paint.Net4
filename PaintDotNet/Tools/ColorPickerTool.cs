namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Canvas;
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.Tools.Pencil;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;

    internal sealed class ColorPickerTool : PaintDotNet.Tools.Tool
    {
        private IRenderer<ColorBgra> allLayersDocumentRenderer;
        private Surface allLayersRenderSurface;
        private Cursor colorPickerToolCursor;
        private int currentSampleSize;
        private bool mouseDown;
        private PickerPreviewCanvasLayer previewRenderer;

        public ColorPickerTool(DocumentWorkspace documentWorkspace) : base(documentWorkspace, PdnResources.GetImageResource("Icons.ColorPickerToolIcon.png"), PdnResources.GetString("ColorPickerTool.Name"), PdnResources.GetString("ColorPickerTool.HelpText"), 'k', true, ToolBarConfigItems.ColorPickerBehavior | ToolBarConfigItems.PixelSampleMode | ToolBarConfigItems.SampleImageOrLayer)
        {
            this.mouseDown = false;
        }

        private void CreateAllLayersRenderSurface(int sampleSize)
        {
            DisposableUtil.Free<Surface>(ref this.allLayersRenderSurface);
            int width = (base.Document.Bounds.Width < sampleSize) ? base.Document.Bounds.Width : sampleSize;
            int height = (base.Document.Bounds.Height < sampleSize) ? base.Document.Bounds.Height : sampleSize;
            this.allLayersRenderSurface = new Surface(width, height);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposableUtil.Free<Surface>(ref this.allLayersRenderSurface);
            }
            base.Dispose(disposing);
        }

        private ColorBgra LiftColor(int x, int y)
        {
            if (base.ToolSettings.SampleAllLayers.Value)
            {
                return this.SampleAllLayers(x, y);
            }
            return this.SampleCurrentLayer(x, y);
        }

        protected override void OnActivate()
        {
            this.colorPickerToolCursor = PdnResources.GetCursor("Cursors.ColorPickerToolCursor.cur");
            base.Cursor = this.colorPickerToolCursor;
            this.previewRenderer = new PickerPreviewCanvasLayer();
            base.DocumentCanvas.CanvasLayers.Add(this.previewRenderer);
            this.allLayersDocumentRenderer = base.Document.CreateRenderer();
            this.currentSampleSize = base.ToolSettings.PixelSampleMode.Value;
            this.CreateAllLayersRenderSurface(this.currentSampleSize);
            base.ToolSettings.PixelSampleMode.ValueChangedT += new ValueChangedEventHandler<PixelSampleMode>(this.OnPixelSampleModeValueChangedT);
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            base.ToolSettings.PixelSampleMode.ValueChangedT -= new ValueChangedEventHandler<PixelSampleMode>(this.OnPixelSampleModeValueChangedT);
            DisposableUtil.Free<Surface>(ref this.allLayersRenderSurface);
            base.DocumentCanvas.CanvasLayers.Remove(this.previewRenderer);
            DisposableUtil.Free<PickerPreviewCanvasLayer>(ref this.previewRenderer);
            DisposableUtil.Free<Cursor>(ref this.colorPickerToolCursor);
            base.OnDeactivate();
        }

        protected override void OnMouseDown(MouseEventArgsF e)
        {
            base.OnMouseDown(e);
            this.mouseDown = true;
            this.PickColor(e);
        }

        protected override void OnMouseEnter()
        {
            this.previewRenderer.IsVisible = true;
            base.OnMouseEnter();
        }

        protected override void OnMouseLeave()
        {
            this.previewRenderer.IsVisible = false;
            base.OnMouseLeave();
        }

        protected override void OnMouseMove(MouseEventArgsF e)
        {
            this.previewRenderer.BrushLocation = new PointDouble(e.Fx, e.Fy);
            this.previewRenderer.BrushSize = (double) base.ToolSettings.PixelSampleMode.Value;
            base.OnMouseMove(e);
            if (this.mouseDown)
            {
                this.PickColor(e);
            }
        }

        protected override void OnMouseUp(MouseEventArgsF e)
        {
            base.OnMouseUp(e);
            this.mouseDown = false;
            switch (base.ToolSettings.ColorPickerClickBehavior.Value)
            {
                case ColorPickerClickBehavior.NoToolSwitch:
                    break;

                case ColorPickerClickBehavior.SwitchToLastTool:
                    base.DocumentWorkspace.SetToolFromType(base.DocumentWorkspace.PreviousActiveToolType);
                    return;

                case ColorPickerClickBehavior.SwitchToPencilTool:
                    base.DocumentWorkspace.SetToolFromType(typeof(PencilTool));
                    return;

                default:
                    ExceptionUtil.ThrowInternalErrorException();
                    break;
            }
        }

        private void OnPixelSampleModeValueChangedT(object sender, ValueChangedEventArgs<PixelSampleMode> e)
        {
            this.currentSampleSize = e.NewValue;
            this.CreateAllLayersRenderSurface(this.currentSampleSize);
        }

        private void PickColor(MouseEventArgsF e)
        {
            if (this.currentSampleSize == 1)
            {
                if (!base.Document.Bounds.Contains(e.X, e.Y))
                {
                    return;
                }
            }
            else
            {
                Rectangle bounds = base.Document.Bounds;
                int width = (this.currentSampleSize - 1) / 2;
                bounds.Inflate(width, width);
                if (!bounds.Contains(e.X, e.Y))
                {
                    return;
                }
            }
            ColorBgra bgra = this.LiftColor(e.X, e.Y);
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                base.ToolSettings.PrimaryColor.Value = (ColorBgra32) bgra;
            }
            else if ((e.Button & MouseButtons.Right) == MouseButtons.Right)
            {
                base.ToolSettings.SecondaryColor.Value = (ColorBgra32) bgra;
            }
        }

        private ColorBgra SampleAllLayers(int x, int y)
        {
            if (this.currentSampleSize == 1)
            {
                this.allLayersDocumentRenderer.Render(this.allLayersRenderSurface, new PointInt32(x, y));
                return this.allLayersRenderSurface[0, 0];
            }
            int num = (this.currentSampleSize - 1) / 2;
            RectInt32 num2 = new RectInt32(x - num, y - num, this.currentSampleSize, this.currentSampleSize);
            Rectangle bounds = base.Document.Bounds;
            num2.Intersect(new RectInt32(bounds.X, bounds.Y, bounds.Width, bounds.Height));
            PointInt32 location = num2.Location;
            this.allLayersDocumentRenderer.Render(this.allLayersRenderSurface, location);
            return SampleArea(this.allLayersRenderSurface, new RectInt32(PointInt32.Zero, num2.Size));
        }

        private static unsafe ColorBgra SampleArea(Surface surface, RectInt32 sampleArea)
        {
            List<ColorBgra> items = new List<ColorBgra>();
            for (int i = sampleArea.Top; i < sampleArea.Bottom; i++)
            {
                ColorBgra* pointAddressUnchecked = surface.GetPointAddressUnchecked(sampleArea.Left, i);
                for (int j = sampleArea.Left; j < sampleArea.Right; j++)
                {
                    items.Add(pointAddressUnchecked[0]);
                    pointAddressUnchecked++;
                }
            }
            if (items.Count == 0)
            {
                return ColorBgra.TransparentBlack;
            }
            return ColorBgra.Blend(items.ToArrayEx<ColorBgra>());
        }

        private ColorBgra SampleCurrentLayer(int x, int y)
        {
            Surface surface = ((BitmapLayer) base.ActiveLayer).Surface;
            if (this.currentSampleSize == 1)
            {
                return surface[x, y];
            }
            int num = (this.currentSampleSize - 1) / 2;
            RectInt32 sampleArea = new RectInt32(x - num, y - num, this.currentSampleSize, this.currentSampleSize);
            Rectangle bounds = surface.Bounds;
            sampleArea.Intersect(new RectInt32(bounds.X, bounds.Y, bounds.Width, bounds.Height));
            return SampleArea(surface, sampleArea);
        }
    }
}

