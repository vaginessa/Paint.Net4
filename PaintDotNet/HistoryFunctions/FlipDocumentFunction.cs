namespace PaintDotNet.HistoryFunctions
{
    using PaintDotNet;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Resources;
    using System;
    using System.Collections.Generic;

    internal sealed class FlipDocumentFunction : HistoryFunction
    {
        private FlipType flipType;
        private ImageResource historyMementoImage;
        private string historyMementoName;

        public FlipDocumentFunction(FlipType flipType) : base(ActionFlags.None)
        {
            this.historyMementoName = GetHistoryMementoName(flipType);
            this.historyMementoImage = GetHistoryMementoImage(flipType);
            this.flipType = flipType;
        }

        private static ImageResource GetHistoryMementoImage(FlipType flipType)
        {
            if (flipType != FlipType.Horizontal)
            {
                if (flipType == FlipType.Vertical)
                {
                    return PdnResources.GetImageResource("Icons.MenuImageFlipVerticalIcon.png");
                }
            }
            else
            {
                return PdnResources.GetImageResource("Icons.MenuImageFlipHorizontalIcon.png");
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
                    return PdnResources.GetString("FlipDocumentVerticalAction.Name");
                }
            }
            else
            {
                return PdnResources.GetString("FlipDocumentHorizontalAction.Name");
            }
            ExceptionUtil.ThrowInvalidEnumArgumentException<FlipType>(flipType, "flipType");
            return null;
        }

        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            List<HistoryMemento> mementos = new List<HistoryMemento>();
            if (!historyWorkspace.Selection.IsEmpty)
            {
                DeselectFunction function = new DeselectFunction();
                base.EnterCriticalRegion();
                HistoryMemento item = function.Execute(historyWorkspace);
                mementos.Add(item);
            }
            base.EnterCriticalRegion();
            int count = historyWorkspace.Document.Layers.Count;
            for (int i = 0; i < count; i++)
            {
                HistoryMemento memento2 = new FlipLayerFunction(this.flipType, i).Execute(historyWorkspace);
                mementos.Add(memento2);
            }
            return HistoryMemento.Combine(this.historyMementoName, this.historyMementoImage, mementos);
        }
    }
}

