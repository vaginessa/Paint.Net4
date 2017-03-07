namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.Dialogs;
    using PaintDotNet.Drawing;
    using PaintDotNet.HistoryFunctions;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Resources;
    using PaintDotNet.Runtime;
    using PaintDotNet.Tools.Move;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    internal sealed class ImportFromFileAction : DocumentWorkspaceAction
    {
        public ImportFromFileAction() : base(ActionFlags.KeepToolActive)
        {
        }

        private HistoryMemento DoCanvasResize(DocumentWorkspace documentWorkspace, Size newLayerSize)
        {
            Document document;
            int activeLayerIndex = documentWorkspace.ActiveLayerIndex;
            Size newSize = new Size(Math.Max(newLayerSize.Width, documentWorkspace.Document.Width), Math.Max(newLayerSize.Height, documentWorkspace.Document.Height));
            try
            {
                using (new WaitCursorChanger(documentWorkspace))
                {
                    CleanupManager.RequestCleanup();
                    document = CanvasSizeAction.ResizeDocument(documentWorkspace.Document, newSize, AnchorEdge.TopLeft, documentWorkspace.ToolSettings.SecondaryColor.Value);
                }
            }
            catch (OutOfMemoryException exception)
            {
                ExceptionDialog.ShowErrorDialog(documentWorkspace, PdnResources.GetString("ImportFromFileAction.AskForCanvasResize.OutOfMemory"), exception);
                document = null;
            }
            if (document == null)
            {
                return null;
            }
            HistoryMemento memento = new ReplaceDocumentHistoryMemento(string.Empty, null, documentWorkspace);
            using (new WaitCursorChanger(documentWorkspace))
            {
                documentWorkspace.Document = document;
            }
            documentWorkspace.ActiveLayer = (Layer) documentWorkspace.Document.Layers[activeLayerIndex];
            return memento;
        }

        private HistoryMemento ImportDocument(DocumentWorkspace documentWorkspace, Document document, out Rectangle lastLayerBounds)
        {
            List<HistoryMemento> historyMementos = new List<HistoryMemento>();
            bool[] flagArray = new bool[document.Layers.Count];
            for (int i = 0; i < flagArray.Length; i++)
            {
                flagArray[i] = true;
            }
            lastLayerBounds = Rectangle.Empty;
            if (flagArray != null)
            {
                List<Layer> list2 = new List<Layer>();
                for (int j = 0; j < flagArray.Length; j++)
                {
                    if (flagArray[j])
                    {
                        list2.Add((Layer) document.Layers[j]);
                    }
                }
                foreach (Layer layer in list2)
                {
                    document.Layers.Remove(layer);
                }
                document.Dispose();
                document = null;
                foreach (Layer layer2 in list2)
                {
                    lastLayerBounds = layer2.Bounds;
                    HistoryMemento item = this.ImportOneLayer(documentWorkspace, (BitmapLayer) layer2);
                    if (item != null)
                    {
                        historyMementos.Add(item);
                    }
                    else
                    {
                        this.Rollback(historyMementos);
                        historyMementos.Clear();
                        break;
                    }
                }
            }
            if (document != null)
            {
                document.Dispose();
                document = null;
            }
            if (historyMementos.Count > 0)
            {
                return new CompoundHistoryMemento(string.Empty, null, historyMementos.ToArrayEx<HistoryMemento>());
            }
            lastLayerBounds = Rectangle.Empty;
            return null;
        }

        public HistoryMemento ImportMultipleFiles(DocumentWorkspace documentWorkspace, string[] fileNames)
        {
            HistoryMemento memento = null;
            SegmentedList<HistoryMemento> historyMementos = new SegmentedList<HistoryMemento>();
            Rectangle empty = Rectangle.Empty;
            foreach (string str in fileNames)
            {
                HistoryMemento item = this.ImportOneFile(documentWorkspace, str, out empty);
                if (item != null)
                {
                    historyMementos.Add(item);
                }
                else
                {
                    this.Rollback(historyMementos);
                    historyMementos.Clear();
                    break;
                }
            }
            if ((empty.Width > 0) && (empty.Height > 0))
            {
                SelectionHistoryMemento memento3 = new SelectionHistoryMemento(null, null, documentWorkspace);
                historyMementos.Add(memento3);
                using (documentWorkspace.Selection.UseChangeScope())
                {
                    documentWorkspace.Selection.Reset();
                    documentWorkspace.Selection.SetContinuation(empty.ToRectInt32(), SelectionCombineMode.Replace);
                    documentWorkspace.Selection.CommitContinuation();
                }
            }
            if (historyMementos.Count > 0)
            {
                HistoryMemento[] actions = historyMementos.ToArrayEx<HistoryMemento>();
                memento = new CompoundHistoryMemento(StaticName, StaticImage, actions);
            }
            return memento;
        }

        private HistoryMemento ImportOneFile(DocumentWorkspace documentWorkspace, string fileName, out Rectangle lastLayerBounds)
        {
            FileType type;
            documentWorkspace.AppWorkspace.Widgets.StatusBarProgress.ResetProgressStatusBar();
            ProgressEventHandler progressCallback = delegate (object s, ProgressEventArgs e) {
                documentWorkspace.AppWorkspace.Widgets.StatusBarProgress.SetProgressStatusBar(new double?(e.Percent));
            };
            Document document = DocumentWorkspace.LoadDocument(documentWorkspace, fileName, out type, progressCallback);
            documentWorkspace.AppWorkspace.Widgets.StatusBarProgress.EraseProgressStatusBar();
            if (document != null)
            {
                string str = Path.ChangeExtension(Path.GetFileName(fileName), null);
                string format = PdnResources.GetString("ImportFromFileAction.ImportOneFile.NewLayer.Format");
                foreach (Layer layer in document.Layers)
                {
                    layer.Name = string.Format(format, str, layer.Name);
                    layer.IsBackground = false;
                }
                return this.ImportDocument(documentWorkspace, document, out lastLayerBounds);
            }
            lastLayerBounds = Rectangle.Empty;
            return null;
        }

        private HistoryMemento ImportOneLayer(DocumentWorkspace documentWorkspace, BitmapLayer layer)
        {
            SegmentedList<HistoryMemento> items = new SegmentedList<HistoryMemento>();
            bool flag = true;
            if (flag && !documentWorkspace.Selection.IsEmpty)
            {
                HistoryMemento item = new DeselectFunction().Execute(documentWorkspace);
                items.Add(item);
            }
            if (flag && ((layer.Width > documentWorkspace.Document.Width) || (layer.Height > documentWorkspace.Document.Height)))
            {
                HistoryMemento memento3 = this.DoCanvasResize(documentWorkspace, layer.Size);
                if (memento3 == null)
                {
                    flag = false;
                }
                else
                {
                    items.Add(memento3);
                }
            }
            if (flag && (layer.Size != documentWorkspace.Document.Size))
            {
                BitmapLayer layer2;
                try
                {
                    using (new WaitCursorChanger(documentWorkspace))
                    {
                        CleanupManager.RequestCleanup();
                        layer2 = CanvasSizeAction.ResizeLayer(layer, documentWorkspace.Document.Size, AnchorEdge.TopLeft, ColorBgra.White.NewAlpha(0));
                    }
                }
                catch (OutOfMemoryException exception)
                {
                    ExceptionDialog.ShowErrorDialog(documentWorkspace, PdnResources.GetString("ImportFromFileAction.ImportOneLayer.OutOfMemory"), exception);
                    flag = false;
                    layer2 = null;
                }
                if (layer2 != null)
                {
                    layer.Dispose();
                    layer = layer2;
                }
            }
            if (flag)
            {
                NewLayerHistoryMemento memento4 = new NewLayerHistoryMemento(string.Empty, null, documentWorkspace, documentWorkspace.Document.Layers.Count);
                documentWorkspace.Document.Layers.Add(layer);
                items.Add(memento4);
            }
            if (flag)
            {
                return new CompoundHistoryMemento(string.Empty, null, items.ToArrayEx<HistoryMemento>());
            }
            this.Rollback(items);
            return null;
        }

        public override HistoryMemento PerformAction(DocumentWorkspace documentWorkspace)
        {
            string[] strArray;
            string directoryName = Path.GetDirectoryName(documentWorkspace.FilePath);
            DialogResult result = DocumentWorkspace.ChooseFiles(documentWorkspace, out strArray, true, directoryName);
            HistoryMemento memento = null;
            if (result == DialogResult.OK)
            {
                System.Type type2;
                System.Type toolType = documentWorkspace.GetToolType();
                documentWorkspace.ClearTool();
                memento = this.ImportMultipleFiles(documentWorkspace, strArray);
                if (memento != null)
                {
                    HistoryMemento[] actions = new HistoryMemento[] { memento };
                    CompoundHistoryMemento memento2 = new CompoundHistoryMemento(StaticName, StaticImage, actions);
                    memento = memento2;
                    type2 = typeof(MoveSelectedPixelsTool);
                }
                else
                {
                    type2 = toolType;
                }
                documentWorkspace.SetToolFromType(type2);
            }
            return memento;
        }

        private void Rollback(IList<HistoryMemento> historyMementos)
        {
            for (int i = historyMementos.Count - 1; i >= 0; i--)
            {
                historyMementos[i].PerformUndo(null);
            }
        }

        public static ImageResource StaticImage =>
            PdnResources.GetImageResource("Icons.MenuLayersImportFromFileIcon.png");

        public static string StaticName =>
            PdnResources.GetString("ImportFromFileAction.Name");
    }
}

