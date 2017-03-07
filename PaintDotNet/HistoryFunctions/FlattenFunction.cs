namespace PaintDotNet.HistoryFunctions
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Resources;
    using PaintDotNet.Runtime;
    using System;

    internal sealed class FlattenFunction : HistoryFunction
    {
        public FlattenFunction() : base(ActionFlags.None)
        {
        }

        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            SelectionData state = null;
            SegmentedList<HistoryMemento> actions = new SegmentedList<HistoryMemento>();
            if (!historyWorkspace.Selection.IsEmpty)
            {
                state = historyWorkspace.Selection.Save();
                HistoryMemento memento3 = new DeselectFunction().Execute(historyWorkspace);
                actions.Add(memento3);
            }
            ReplaceDocumentHistoryMemento item = new ReplaceDocumentHistoryMemento(null, null, historyWorkspace);
            actions.Add(item);
            CompoundHistoryMemento memento2 = new CompoundHistoryMemento(StaticName, PdnResources.GetImageResource("Icons.MenuImageFlattenIcon.png"), actions);
            Document document = RetryManager.RunMemorySensitiveOperation<Document>(() => historyWorkspace.Document.Flatten());
            base.EnterCriticalRegion();
            historyWorkspace.Document = document;
            if (state != null)
            {
                SelectionHistoryMemento newHA = new SelectionHistoryMemento(null, null, historyWorkspace);
                historyWorkspace.Selection.Restore(state);
                memento2.AddMemento(newHA);
            }
            return memento2;
        }

        public static string StaticName =>
            PdnResources.GetString("FlattenFunction.Name");
    }
}

