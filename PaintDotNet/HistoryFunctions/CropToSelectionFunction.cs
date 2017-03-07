namespace PaintDotNet.HistoryFunctions
{
    using PaintDotNet;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.Runtime;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    internal sealed class CropToSelectionFunction : HistoryFunction
    {
        public CropToSelectionFunction() : base(ActionFlags.None)
        {
        }

        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            GeometryList cachedClippingMask = historyWorkspace.Selection.GetCachedClippingMask();
            if (historyWorkspace.Selection.IsEmpty || (cachedClippingMask.Bounds.Area < 1.0))
            {
                return null;
            }
            Document document = historyWorkspace.Document;
            List<HistoryMemento> mementos = new List<HistoryMemento>(document.Layers.Count);
            RectInt32 b = cachedClippingMask.Bounds.GetInt32Bound(1E-05);
            RectInt32 sourceRect = RectInt32.Intersect(document.Bounds(), b);
            Document document2 = new Document(sourceRect.Width, sourceRect.Height);
            document2.ReplaceMetadataFrom(document);
            RectInt32 rect = new RectInt32(0, 0, sourceRect.Width, sourceRect.Height);
            IRenderer<ColorAlpha8> cachedClippingMaskRenderer = historyWorkspace.Selection.GetCachedClippingMaskRenderer(historyWorkspace.ToolSettings.Selection.RenderingQuality.Value);
            IRenderer<ColorAlpha8> newClipMaskRenderer = new ClippedRenderer<ColorAlpha8>(cachedClippingMaskRenderer, sourceRect);
            SelectionHistoryMemento item = new SelectionHistoryMemento(null, null, historyWorkspace);
            mementos.Add(item);
            base.EnterCriticalRegion();
            int count = document.Layers.Count;
            while (document.Layers.Count > 0)
            {
                BitmapLayer layer = (BitmapLayer) document.Layers[0];
                Surface croppedSurface = layer.Surface.CreateWindow(sourceRect);
                BitmapLayer newLayer = RetryManager.RunMemorySensitiveOperation<BitmapLayer>(() => new BitmapLayer(croppedSurface));
                newLayer.LoadProperties(layer.SaveProperties());
                HistoryMemento deleteLayerMemento = new DeleteLayerFunction(0).Execute(historyWorkspace);
                mementos.Add(deleteLayerMemento);
                Task task = Task.Factory.StartNew(delegate {
                    deleteLayerMemento.Flush();
                }, TaskCreationOptions.LongRunning);
                Parallel.ForEach<RectInt32>(rect.GetTiles(TilingStrategy.Tiles, 7), delegate (RectInt32 newTileRect) {
                    ISurface<ColorBgra> surface = newLayer.Surface.CreateWindow(newTileRect);
                    IRenderer<ColorAlpha8> mask = new ClippedRenderer<ColorAlpha8>(newClipMaskRenderer, newTileRect);
                    surface.MultiplyAlphaChannel(mask);
                });
                document2.Layers.Add(newLayer);
                task.Wait();
                if (document2.Layers.Count > count)
                {
                    ExceptionUtil.ThrowInternalErrorException("newDocument.Layers.Count > oldLayerCount");
                }
            }
            ReplaceDocumentHistoryMemento memento2 = new ReplaceDocumentHistoryMemento(null, null, historyWorkspace);
            mementos.Add(memento2);
            historyWorkspace.Document = document2;
            return HistoryMemento.Combine(HistoryMementoName, HistoryMementoImage, mementos);
        }

        public static ImageResource HistoryMementoImage =>
            PdnResources.GetImageResource("Icons.MenuImageCropIcon.png");

        public static string HistoryMementoName =>
            PdnResources.GetString("CropAction.Name");
    }
}

