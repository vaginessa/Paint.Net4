namespace PaintDotNet.HistoryFunctions
{
    using PaintDotNet;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Resources;
    using PaintDotNet.Runtime;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    internal sealed class RotateDocumentFunction : HistoryFunction
    {
        private RotateType rotation;

        public RotateDocumentFunction(RotateType rotation) : base(ActionFlags.None)
        {
            this.rotation = rotation;
        }

        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            int height;
            int width;
            string str;
            string str2;
            switch (this.rotation)
            {
                case RotateType.Clockwise90:
                case RotateType.CounterClockwise90:
                    height = historyWorkspace.Document.Height;
                    width = historyWorkspace.Document.Width;
                    break;

                case RotateType.Rotate180:
                    height = historyWorkspace.Document.Width;
                    width = historyWorkspace.Document.Height;
                    break;

                default:
                    throw ExceptionUtil.InvalidEnumArgumentException<RotateType>(this.rotation, "this.rotation");
            }
            switch (this.rotation)
            {
                case RotateType.Clockwise90:
                    str = "Icons.MenuImageRotate90CWIcon.png";
                    str2 = PdnResources.GetString("RotateAction.90CW");
                    break;

                case RotateType.CounterClockwise90:
                    str = "Icons.MenuImageRotate90CCWIcon.png";
                    str2 = PdnResources.GetString("RotateAction.90CCW");
                    break;

                case RotateType.Rotate180:
                    str = "Icons.MenuImageRotate180Icon.png";
                    str2 = PdnResources.GetString("RotateAction.180");
                    break;

                default:
                    throw ExceptionUtil.InvalidEnumArgumentException<RotateType>(this.rotation, "this.rotation");
            }
            string name = string.Format(PdnResources.GetString("RotateAction.HistoryMementoName.Format"), StaticName, str2);
            ImageResource imageResource = PdnResources.GetImageResource(str);
            List<HistoryMemento> actions = new List<HistoryMemento>();
            Document document = new Document(height, width);
            if (!historyWorkspace.Selection.IsEmpty)
            {
                DeselectFunction function = new DeselectFunction();
                base.EnterCriticalRegion();
                HistoryMemento memento3 = function.Execute(historyWorkspace);
                actions.Add(memento3);
            }
            ReplaceDocumentHistoryMemento item = new ReplaceDocumentHistoryMemento(null, null, historyWorkspace);
            actions.Add(item);
            document.ReplaceMetadataFrom(historyWorkspace.Document);
            for (int i = 0; i < historyWorkspace.Document.Layers.Count; i++)
            {
                Layer at = historyWorkspace.Document.Layers.GetAt(i);
                if (!(at is BitmapLayer))
                {
                    throw new InvalidOperationException("Cannot Rotate non-BitmapLayers");
                }
                Layer layer2 = this.RotateLayer((BitmapLayer) at, this.rotation, height, width);
                document.Layers.Add(layer2);
            }
            CompoundHistoryMemento memento2 = new CompoundHistoryMemento(name, imageResource, actions);
            base.EnterCriticalRegion();
            historyWorkspace.Document = document;
            return memento2;
        }

        private BitmapLayer RotateLayer(BitmapLayer layer, RotateType rotationType, int width, int height)
        {
            Surface surface = RetryManager.RunMemorySensitiveOperation<Surface>(() => new Surface(width, height));
            if (rotationType == RotateType.Rotate180)
            {
                Parallel.For(0, height, delegate (int y) {
                    for (int j = 0; j < width; j++)
                    {
                        surface[j, y] = layer.Surface[(width - j) - 1, (height - y) - 1];
                    }
                });
            }
            else if (rotationType == RotateType.CounterClockwise90)
            {
                Parallel.For(0, height, delegate (int y) {
                    for (int k = 0; k < width; k++)
                    {
                        surface[k, y] = layer.Surface[(height - y) - 1, k];
                    }
                });
            }
            else if (rotationType == RotateType.Clockwise90)
            {
                Parallel.For(0, height, delegate (int y) {
                    for (int m = 0; m < width; m++)
                    {
                        surface[m, y] = layer.Surface[y, (width - 1) - m];
                    }
                });
            }
            BitmapLayer layer2 = new BitmapLayer(surface, true);
            layer2.LoadProperties(layer.SaveProperties());
            return layer2;
        }

        public static string StaticName =>
            PdnResources.GetString("RotateAction.Name");
    }
}

