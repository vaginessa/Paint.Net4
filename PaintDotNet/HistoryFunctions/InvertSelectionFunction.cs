namespace PaintDotNet.HistoryFunctions
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Concurrency;
    using PaintDotNet.Dialogs;
    using PaintDotNet.Drawing;
    using PaintDotNet.Functional;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.Threading.Tasks;
    using System;
    using System.Threading;

    internal sealed class InvertSelectionFunction : HistoryFunction
    {
        public InvertSelectionFunction() : base(ActionFlags.None)
        {
        }

        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            if (historyWorkspace.Selection.IsEmpty)
            {
                return null;
            }
            SelectionHistoryMemento memento = new SelectionHistoryMemento(StaticName, StaticImage, historyWorkspace);
            GeometryList selectedPath = historyWorkspace.Selection.GetCachedGeometryList();
            SelectionRenderingQuality selectionRenderingQuality = historyWorkspace.ToolSettings.Selection.RenderingQuality.Value;
            Result<IReadOnlyList<RectInt32>> selectedPathScansLazy = historyWorkspace.Selection.GetCachedLazyClippingMaskScans();
            RectInt32 documentBounds = historyWorkspace.Document.Bounds();
            Func<GeometryList> invertedPathFn = delegate {
                if ((selectionRenderingQuality == SelectionRenderingQuality.Aliased) || selectedPath.IsPixelated)
                {
                    GeometryList list2 = GeometryList.FromNonOverlappingSortedScans(selectedPathScansLazy.Value);
                    list2.AddRect(documentBounds);
                    SegmentedList<RectInt32> scans = new SegmentedList<RectInt32>();
                    foreach (RectInt32 num in list2.EnumerateInteriorScans())
                    {
                        if (documentBounds.Contains(num))
                        {
                            scans.Add(num);
                        }
                        else if (documentBounds.IntersectsWith(num))
                        {
                            scans.Add(RectInt32.Intersect(documentBounds, num));
                        }
                    }
                    return GeometryList.FromNonOverlappingScans(scans);
                }
                GeometryList lhs = documentBounds.Contains(selectedPath.Bounds) ? selectedPath : GeometryList.ClipToRect(selectedPath, documentBounds);
                return GeometryList.Combine(lhs, GeometryCombineMode.Xor, documentBounds);
            };
            ThreadTask<GeometryList> task = historyWorkspace.TaskManager.StartNewThreadTask<GeometryList>(task => invertedPathFn(), ApartmentState.MTA);
            ManualResetEvent taskFinishedEvent = new ManualResetEvent(false);
            task.ResultAsync<GeometryList>().Receive(delegate (Result<GeometryList> r) {
                taskFinishedEvent.Set();
            }).Observe();
            if (!taskFinishedEvent.WaitOne(0x3e8))
            {
                using (TaskProgressDialog dialog = new TaskProgressDialog())
                {
                    dialog.Task = task;
                    dialog.Text = StaticName;
                    dialog.Icon = StaticImage.Reference.ToIcon();
                    dialog.HeaderText = PdnResources.GetString("SaveConfigDialog.Finishing.Text");
                    dialog.ShowDialog(historyWorkspace.Window);
                }
            }
            Result<GeometryList> taskResult = task.TaskResult;
            if (taskResult.IsError)
            {
                if (taskResult.Error is OutOfMemoryException)
                {
                    throw new OutOfMemoryException(null, taskResult.Error);
                }
                throw new AggregateException(null, taskResult.Error);
            }
            GeometryList geometry = task.TaskResult.Value;
            base.EnterCriticalRegion();
            using (historyWorkspace.Selection.UseChangeScope())
            {
                historyWorkspace.Selection.Reset();
                geometry.Freeze();
                historyWorkspace.Selection.SetContinuation(geometry, SelectionCombineMode.Replace);
                historyWorkspace.Selection.CommitContinuation();
            }
            return memento;
        }

        public static ImageResource StaticImage =>
            PdnResources.GetImageResource("Icons.MenuEditInvertSelectionIcon.png");

        public static string StaticName =>
            PdnResources.GetString("InvertSelectionAction.Name");
    }
}

