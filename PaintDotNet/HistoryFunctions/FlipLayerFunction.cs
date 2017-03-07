namespace PaintDotNet.HistoryFunctions
{
    using PaintDotNet;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Resources;
    using System;

    internal sealed class FlipLayerFunction : HistoryFunction
    {
        private FlipType flipType;
        private ImageResource historyMementoImage;
        private string historyMementoName;
        private int layerIndex;

        public FlipLayerFunction(FlipType flipType, int layerIndex) : base(ActionFlags.None)
        {
            this.historyMementoName = GetHistoryMementoName(flipType);
            this.historyMementoImage = GetHistoryMementoImage(flipType);
            this.flipType = flipType;
            this.layerIndex = layerIndex;
        }

        private static ImageResource GetHistoryMementoImage(FlipType flipType)
        {
            if (flipType != FlipType.Horizontal)
            {
                if (flipType == FlipType.Vertical)
                {
                    return PdnResources.GetImageResource("Icons.MenuLayersFlipVerticalIcon.png");
                }
            }
            else
            {
                return PdnResources.GetImageResource("Icons.MenuLayersFlipHorizontalIcon.png");
            }
            ExceptionUtil.ThrowInvalidEnumArgumentException<FlipType>(flipType, "flipType");
            return null;
        }

        private static string GetHistoryMementoName(FlipType flipType)
        {
            if (flipType != FlipType.Horizontal)
            {
                if (flipType == FlipType.Vertical)
                {
                    return PdnResources.GetString("FlipLayerVerticalAction.Name");
                }
            }
            else
            {
                return PdnResources.GetString("FlipLayerHorizontalAction.Name");
            }
            ExceptionUtil.ThrowInvalidEnumArgumentException<FlipType>(flipType, "flipType");
            return null;
        }

        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            CompoundHistoryMemento memento = new CompoundHistoryMemento(this.historyMementoName, this.historyMementoImage);
            if (!historyWorkspace.Selection.IsEmpty)
            {
                DeselectFunction function = new DeselectFunction();
                base.EnterCriticalRegion();
                HistoryMemento memento4 = function.Execute(historyWorkspace);
                memento.AddMemento(memento4);
            }
            FlipLayerHistoryMemento newHA = new FlipLayerHistoryMemento(null, null, historyWorkspace, this.layerIndex, this.flipType);
            base.EnterCriticalRegion();
            HistoryMemento memento3 = newHA.PerformUndo(null);
            memento.AddMemento(newHA);
            return memento;
        }
    }
}

