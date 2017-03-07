namespace PaintDotNet.Controls
{
    using Microsoft.WindowsAPICodePack.Taskbar;
    using PaintDotNet;
    using PaintDotNet.Actions;
    using PaintDotNet.AppModel;
    using PaintDotNet.Canvas;
    using PaintDotNet.Collections;
    using PaintDotNet.Concurrency;
    using PaintDotNet.Data;
    using PaintDotNet.Dialogs;
    using PaintDotNet.Direct2D;
    using PaintDotNet.Drawing;
    using PaintDotNet.Functional;
    using PaintDotNet.HistoryFunctions;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Imaging;
    using PaintDotNet.IO;
    using PaintDotNet.MemoryManagement;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.Runtime;
    using PaintDotNet.Settings.App;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Threading;
    using PaintDotNet.Threading.Tasks;
    using PaintDotNet.Threading.Tasks.IterativeTaskDirectives;
    using PaintDotNet.Tools;
    using PaintDotNet.Tools.CloneStamp;
    using PaintDotNet.Tools.Eraser;
    using PaintDotNet.Tools.Gradient;
    using PaintDotNet.Tools.MagicWand;
    using PaintDotNet.Tools.Move;
    using PaintDotNet.Tools.PaintBrush;
    using PaintDotNet.Tools.PaintBucket;
    using PaintDotNet.Tools.Pencil;
    using PaintDotNet.Tools.Recolor;
    using PaintDotNet.Tools.Shapes;
    using PaintDotNet.VisualStyling;
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Threading;
    using System.Windows.Forms;

    internal class DocumentWorkspace : DocumentView, IDispatcherObject, IThreadAffinitizedObject, IHistoryWorkspace, IThumbnailProvider
    {
        private Layer activeLayer;
        private PaintDotNet.Tools.Tool activeTool;
        private PaintDotNet.Controls.AppWorkspace appWorkspace;
        private string borrowScratchSurfaceReason = string.Empty;
        private readonly string contextStatusBarFormat = PdnResources.GetString("StatusBar.Context.SelectedArea.Text.Format");
        private readonly string contextStatusBarWithAngleFormat = PdnResources.GetString("StatusBar.Context.SelectedArea.Text.WithAngle.Format");
        private IDispatcher dispatcher;
        private bool doesSelectionInfoNeedUpdate;
        private string filePath;
        private PaintDotNet.FileType fileType;
        private HistoryStack history;
        private bool isScratchSurfaceBorrowed;
        private bool isSelectionInfoUpdating;
        private bool isToolPulseEnabled;
        private bool isUpdateTabbedThumbnailQueued;
        private DateTime lastSaveTime = neverSavedDateTime;
        private ImageResource latestSelectionInfoImage;
        private string latestSelectionInfoText;
        private static readonly DateTime neverSavedDateTime = DateTime.MinValue;
        private int nullToolCount;
        private System.Type preNullTool;
        private System.Type previousActiveToolType;
        private PaintDotNet.SaveConfigToken saveConfigToken;
        private int savedAli;
        private ScaleFactor savedSf;
        private PaintDotNet.ZoomBasis savedZb;
        private Surface scratchSurface;
        private PaintDotNet.Selection selection;
        private long? selectionArea;
        private RectInt32? selectionBounds;
        private ConcurrentDictionary<System.Type, object> staticToolData = new ConcurrentDictionary<System.Type, object>();
        private ImageResource statusIcon;
        private string statusText;
        private int suspendToolCursorChanges;
        private Microsoft.WindowsAPICodePack.Taskbar.TabbedThumbnail tabbedThumbnail;
        private PaintDotNet.Threading.Tasks.TaskManager taskManager;
        private static ToolInfo[] toolInfos;
        private System.Windows.Forms.Timer toolPulseTimer;
        private static System.Type[] tools;
        private int zoomChangesCount;

        [field: CompilerGenerated]
        public event EventHandler ActiveLayerChanged;

        [field: CompilerGenerated]
        public event EventHandler ActiveLayerChanging;

        [field: CompilerGenerated]
        public event EventHandler FilePathChanged;

        [field: CompilerGenerated]
        public event CancelEventHandler RequestActivate;

        [field: CompilerGenerated]
        public event CancelEventHandler RequestClose;

        [field: CompilerGenerated]
        public event EventHandler SaveOptionsChanged;

        [field: CompilerGenerated]
        public event EventHandler StatusChanged;

        [field: CompilerGenerated]
        public event ValueChangedEventHandler<Microsoft.WindowsAPICodePack.Taskbar.TabbedThumbnail> TabbedThumbnailChanged;

        [field: CompilerGenerated]
        public event EventHandler ToolChanged;

        [field: CompilerGenerated]
        public event EventHandler ToolChanging;

        [field: CompilerGenerated]
        public event EventHandler ZoomBasisChanged;

        [field: CompilerGenerated]
        public event EventHandler ZoomBasisChanging;

        static DocumentWorkspace()
        {
            InitializeTools();
            InitializeToolInfos();
        }

        public DocumentWorkspace()
        {
            this.dispatcher = new ControlDispatcher(this);
            this.taskManager = new PaintDotNet.Threading.Tasks.TaskManager();
            this.selection = new PaintDotNet.Selection();
            base.DocumentCanvas.Selection = this.selection;
            this.activeLayer = null;
            this.history = new HistoryStack(this);
            this.InitializeComponent();
            this.selection.Changed += new EventHandler<SelectionChangedEventArgs>(this.OnSelectionChanged);
            base.CanvasView.ScaleBasis = ScaleBasis.FitToViewport;
            DocumentCanvasLayer.SetIsHighQualityScalingEnabled(base.CanvasView, AppSettings.Instance.UI.EnableHighQualityScaling.Value);
            AppSettings.Instance.UI.EnableHighQualityScaling.ValueChangedT += new ValueChangedEventHandler<bool>(this.OnEnableHighQualityScalingValueChanged);
            SelectionCanvasLayer.SetIsAntialiasedOutlineEnabled(base.CanvasView, AppSettings.Instance.UI.EnableAntialiasedSelectionOutline.Value);
            AppSettings.Instance.UI.EnableAntialiasedSelectionOutline.ValueChangedT += new ValueChangedEventHandler<bool>(this.OnEnableAntialiasedSelectionOutlineValueChanged);
            SelectionCanvasLayer.SetIsAnimatedOutlineEnabled(base.CanvasView, AppSettings.Instance.UI.EnableAnimations.Value);
            AppSettings.Instance.UI.EnableAnimations.ValueChangedT += new ValueChangedEventHandler<bool>(this.OnEnableAnimationsValueChanged);
            AppSettings.Instance.UI.ShowTaskbarPreviews.ValueChangedT += new ValueChangedEventHandler<bool>(this.OnShowTaskbarPreviewsSettingChanged);
        }

        public bool Activate()
        {
            this.VerifyThreadAccess();
            CancelEventArgs e = new CancelEventArgs(true);
            this.OnRequestActivate(e);
            return !e.Cancel;
        }

        public Task AddToMruList()
        {
            string fullPath = Path.GetFullPath(this.FilePath);
            ShellUtil.AddToRecentDocumentsList(fullPath);
            MostRecentFile mrf = new MostRecentFile(fullPath, new System.Drawing.Bitmap(1, 1));
            if (MostRecentFilesService.Instance.Contains(fullPath))
            {
                MostRecentFilesService.Instance.Remove(fullPath);
            }
            MostRecentFilesService.Instance.Add(mrf);
            MostRecentFilesService.Instance.SaveMruList();
            IterativeTask task = this.TaskManager.CreateIterativeTask(_ => this.AddToMruListTask());
            task.Start(this.appWorkspace.Dispatcher);
            return task;
        }

        [IteratorStateMachine(typeof(<AddToMruListTask>d__228))]
        public IEnumerator<Directive> AddToMruListTask()
        {
            this.<fullFileName>5__3 = Path.GetFullPath(this.FilePath);
            this.<edgeLength>5__1 = MostRecentFilesService.Instance.IconSize;
            yield return Directive.DispatchTo(this.appWorkspace.BackgroundThread);
            IRenderer<ColorBgra> renderer = this.CreateThumbnailRenderer(this.<edgeLength>5__1);
            int shadowExtent = DropShadow.GetRecommendedExtent(renderer.Size<ColorBgra>().ToGdipSize());
            ShadowDecorationRenderer renderer2 = new ShadowDecorationRenderer(renderer, shadowExtent);
            IRenderer<ColorBgra> renderer3 = renderer2.Inset(new SizeInt32(this.<edgeLength>5__1 + (shadowExtent * 2), this.<edgeLength>5__1 + (shadowExtent * 2)), new PointInt32(((this.<edgeLength>5__1 + (shadowExtent * 2)) - renderer2.Width) / 2, ((this.<edgeLength>5__1 + (shadowExtent * 2)) - renderer2.Height) / 2));
            try
            {
                this.<thumb>5__2 = new Surface(renderer3.Width, renderer3.Height);
                renderer3.Parallelize<ColorBgra>(TilingStrategy.HorizontalSlices, 3, WorkItemQueuePriority.AboveNormal).Render<ColorBgra>(this.<thumb>5__2);
            }
            catch (Exception)
            {
            }
            using (this.<ra>5__5 = new RenderArgs(this.<thumb>5__2))
            {
                System.Drawing.Bitmap image = new System.Drawing.Bitmap(this.<ra>5__5.Bitmap.Width, this.<ra>5__5.Bitmap.Height, this.<ra>5__5.Bitmap.PixelFormat);
                using (Graphics graphics = Graphics.FromImage(image))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.DrawImage(this.<ra>5__5.Bitmap, 0, 0, this.<ra>5__5.Bitmap.Width, this.<ra>5__5.Bitmap.Height);
                }
                this.<mrf2>5__4 = new MostRecentFile(this.<fullFileName>5__3, image);
                yield return Directive.DispatchTo(this.appWorkspace.Dispatcher);
                if (MostRecentFilesService.Instance.Contains(this.<fullFileName>5__3))
                {
                    MostRecentFilesService.Instance.Remove(this.<fullFileName>5__3);
                }
                MostRecentFilesService.Instance.Add(this.<mrf2>5__4);
                MostRecentFilesService.Instance.SaveMruList();
                this.<mrf2>5__4 = null;
            }
            this.<ra>5__5 = null;
            this.<thumb>5__2.Dispose();
        }

        private void BeginUpdateSelectionInfo()
        {
            this.VerifyAccess<DocumentWorkspace>();
            if (this.isSelectionInfoUpdating)
            {
                throw new InvalidOperationException();
            }
            this.isSelectionInfoUpdating = true;
            this.doesSelectionInfoNeedUpdate = false;
            RectInt32 docBounds = base.Document.Bounds();
            Result<GeometryList> lazyClippingMask = this.Selection.GetCachedLazyGeometryList();
            Result<IReadOnlyList<RectInt32>> lazyScans = this.Selection.GetCachedLazyGeometryListScans();
            SynchronizationContext syncContext = PdnSynchronizationContext.Instance;
            WorkItemDispatcher.Default.Enqueue(() => this.UpdateSelectionInfo(syncContext, docBounds, lazyClippingMask, lazyScans), WorkItemQueuePriority.Low);
        }

        private void BeginZoomChanges()
        {
            this.zoomChangesCount++;
        }

        public Surface BorrowScratchSurface(string reason)
        {
            if (this.isScratchSurfaceBorrowed)
            {
                ExceptionUtil.ThrowInvalidOperationException("ScratchSurface already borrowed: '" + this.borrowScratchSurfaceReason + "' (trying to borrow for: '" + reason + "')");
            }
            this.isScratchSurfaceBorrowed = true;
            this.borrowScratchSurfaceReason = reason;
            this.scratchSurface.Clear();
            return this.scratchSurface;
        }

        public static DialogResult ChooseFile(Control parent, out string fileName) => 
            ChooseFile(parent, out fileName, null);

        public static DialogResult ChooseFile(Control parent, out string fileName, string startingDir)
        {
            string[] strArray;
            DialogResult result = ChooseFiles(parent, out strArray, false, startingDir);
            if (result == DialogResult.OK)
            {
                fileName = strArray[0];
                return result;
            }
            fileName = null;
            return result;
        }

        public static DialogResult ChooseFiles(Control owner, out string[] fileNames, bool multiselect) => 
            ChooseFiles(owner, out fileNames, multiselect, null);

        public static DialogResult ChooseFiles(Control owner, out string[] fileNames, bool multiselect, string startingDir)
        {
            FileTypeCollection fileTypes = FileTypes.GetFileTypes();
            using (PaintDotNet.SystemLayer.IFileOpenDialog dialog = CommonDialogs.CreateFileOpenDialog())
            {
                if (startingDir != null)
                {
                    dialog.InitialDirectory = startingDir;
                }
                else
                {
                    dialog.InitialDirectory = GetDefaultSavePath();
                }
                dialog.CheckFileExists = true;
                dialog.CheckPathExists = true;
                dialog.Multiselect = multiselect;
                dialog.Filter = fileTypes.ToString(true, PdnResources.GetString("FileDialog.Types.AllImages"), false, true);
                dialog.FilterIndex = 0;
                DialogResult result = ShowFileDialog(owner, dialog, true);
                if (result == DialogResult.OK)
                {
                    fileNames = dialog.FileNames;
                }
                else
                {
                    fileNames = Array.Empty<string>();
                }
                return result;
            }
        }

        public void ClearTool()
        {
            this.SetTool(null);
        }

        public bool Close()
        {
            this.VerifyThreadAccess();
            CancelEventArgs e = new CancelEventArgs(true);
            this.OnRequestClose(e);
            return !e.Cancel;
        }

        public IRenderer<ColorBgra> CreateThumbnailRenderer(SizeInt32 maxThumbSize)
        {
            if (base.Document == null)
            {
                return new SolidColorRendererBgra(1, 1, ColorBgra.Red);
            }
            IRenderer<ColorBgra> source = base.DocumentCanvas.DocumentCanvasLayer.DocumentRenderer.CreateRenderer();
            SizeInt32 size = ThumbnailHelpers.ComputeThumbnailSize(base.Document.Size(), maxThumbSize);
            IRenderer<ColorBgra> sourceLHS = RendererBgra.Checkers(size);
            IRenderer<ColorBgra> sourceRHS = source.ResizeSuperSampling(size);
            return sourceLHS.DrawBlend(CompositionOps.Normal.Static, sourceRHS);
        }

        public IRenderer<ColorBgra> CreateThumbnailRenderer(int maxEdgeLength) => 
            this.CreateThumbnailRenderer(new SizeInt32(maxEdgeLength, maxEdgeLength));

        public PaintDotNet.Tools.Tool CreateTool(System.Type toolType) => 
            CreateTool(toolType, this);

        private static PaintDotNet.Tools.Tool CreateTool(System.Type toolType, DocumentWorkspace dc)
        {
            System.Type[] types = new System.Type[] { typeof(DocumentWorkspace) };
            object[] parameters = new object[] { dc };
            return (PaintDotNet.Tools.Tool) toolType.GetConstructor(types).Invoke(parameters);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                AppSettings.Instance.UI.ShowTaskbarPreviews.ValueChangedT -= new ValueChangedEventHandler<bool>(this.OnShowTaskbarPreviewsSettingChanged);
                AppSettings.Instance.UI.EnableHighQualityScaling.ValueChangedT -= new ValueChangedEventHandler<bool>(this.OnEnableHighQualityScalingValueChanged);
                AppSettings.Instance.UI.EnableAntialiasedSelectionOutline.ValueChangedT -= new ValueChangedEventHandler<bool>(this.OnEnableAntialiasedSelectionOutlineValueChanged);
                AppSettings.Instance.UI.EnableAnimations.ValueChangedT -= new ValueChangedEventHandler<bool>(this.OnEnableAnimationsValueChanged);
                this.appWorkspace.ToolSettings.Selection.RenderingQuality.ValueChangedT -= new ValueChangedEventHandler<SelectionRenderingQuality>(this.OnToolSettingsSelectionRenderingQualityChanged);
                if (this.taskManager != null)
                {
                    this.taskManager.BeginShutdown();
                    this.taskManager.Dispose();
                    this.taskManager = null;
                }
                DisposableUtil.Free<PaintDotNet.Tools.Tool>(ref this.activeTool);
                DisposableUtil.Free<System.Windows.Forms.Timer>(ref this.toolPulseTimer);
                DisposableUtil.Free<Microsoft.WindowsAPICodePack.Taskbar.TabbedThumbnail>(ref this.tabbedThumbnail);
            }
            base.Dispose(disposing);
        }

        public bool DoSave() => 
            this.DoSave(false);

        protected bool DoSave(bool tryToFlatten)
        {
            using (new PushNullToolMode(this))
            {
                string str;
                PaintDotNet.FileType newFileType;
                PaintDotNet.SaveConfigToken newSaveConfigToken;
                this.GetDocumentSaveOptions(out str, out newFileType, out newSaveConfigToken);
                if (str == null)
                {
                    return this.DoSaveAs();
                }
                if ((newFileType != null) && !newFileType.SupportsSaving)
                {
                    return this.DoSaveAs();
                }
                if (newFileType == null)
                {
                    FileTypeCollection fileTypes = FileTypes.GetFileTypes();
                    string extension = Path.GetExtension(str);
                    int num = fileTypes.IndexOfExtension(extension);
                    newFileType = fileTypes[num];
                }
                if ((base.Document.Layers.Count > 1) && !newFileType.SupportsLayers)
                {
                    if (!tryToFlatten)
                    {
                        return this.DoSaveAs();
                    }
                    if (this.WarnAboutFlattening() != DialogResult.Yes)
                    {
                        return false;
                    }
                    this.ApplyFunction(new FlattenFunction());
                }
                if (newSaveConfigToken == null)
                {
                    bool flag2;
                    Surface saveScratchSurface = this.BorrowScratchSurface(base.GetType().Name + ".DoSave() calling GetSaveConfigToken()");
                    try
                    {
                        flag2 = this.GetSaveConfigToken(newFileType, newSaveConfigToken, out newSaveConfigToken, saveScratchSurface);
                    }
                    finally
                    {
                        this.ReturnScratchSurface(saveScratchSurface);
                    }
                    if (!flag2)
                    {
                        return false;
                    }
                }
                if (newFileType.SupportsCustomHeaders)
                {
                    using (new WaitCursorChanger(this))
                    {
                        ISurface<ColorBgra> surface2;
                        byte[] buffer;
                        CleanupManager.RequestCleanup();
                        if ((base.Document.Width > 0x100) || (base.Document.Height > 0x100))
                        {
                            SizeInt32 newSize = ThumbnailHelpers.ComputeThumbnailSize(base.Document.Size(), 0x100);
                            surface2 = base.Document.CreateRenderer().ResizeFant(newSize).Parallelize<ColorBgra>(TilingStrategy.HorizontalSlices, 5, WorkItemQueuePriority.Normal).ToSurface();
                        }
                        else
                        {
                            surface2 = base.Document.CreateRenderer().Parallelize<ColorBgra>(TilingStrategy.Tiles, 7, WorkItemQueuePriority.Normal).ToSurface();
                        }
                        using (MemoryStream stream = new MemoryStream())
                        {
                            using (System.Drawing.Bitmap bitmap = surface2.CreateAliasedGdipBitmap())
                            {
                                bitmap.Save(stream, ImageFormat.Png);
                            }
                            stream.Flush();
                            buffer = stream.ToArrayEx();
                        }
                        surface2.Dispose();
                        string str3 = Convert.ToBase64String(buffer, Base64FormattingOptions.None);
                        string str4 = "<thumb png=\"" + str3 + "\" />";
                        base.Document.CustomHeaders = str4;
                    }
                }
                Result saveResult = null;
                Surface saveScratch = this.BorrowScratchSurface(base.GetType().Name + ".DoSave() for purposes of saving");
                try
                {
                    using (SaveTransaction saveTx = new SaveTransaction(str, FileMode.Create, FileAccess.ReadWrite, FileShare.None, FileOptions.None))
                    {
                        VirtualTask<Unit> saveTask = this.TaskManager.CreateVirtualTask(TaskState.Running);
                        TaskProgressDialog progressDialog = new TaskProgressDialog {
                            Task = saveTask,
                            CloseOnFinished = true,
                            ShowCancelButton = false,
                            Text = PdnResources.GetString("SaveProgressDialog.Title")
                        };
                        string str5 = PdnResources.GetString("TaskProgressDialog.Initializing.Text");
                        string savingText = PdnResources.GetString("SaveProgressDialog.Description");
                        string savingWithPercentTextFormat = PdnResources.GetString("SaveProgressDialog.DescriptionWithPercent.Format");
                        progressDialog.HeaderText = str5;
                        progressDialog.Icon = PdnResources.GetImageResource("Icons.MenuFileSaveIcon.png").Reference.ToIcon();
                        progressDialog.Shown += delegate (object <sender>, EventArgs <e>) {
                            try
                            {
                                Work.QueueWorkItem(delegate {
                                    try
                                    {
                                        progressDialog.SetHeaderTextAsync(savingText);
                                        saveResult = delegate {
                                            try
                                            {
                                                newFileType.Save(this.Document, saveTx.Stream, newSaveConfigToken, saveScratch, delegate (object s2, ProgressEventArgs e2) {
                                                    saveTask.Progress = new double?(DoubleUtil.Clamp(e2.Percent / 100.0, 0.0, 1.0));
                                                    progressDialog.SetHeaderTextAsync(string.Format(savingWithPercentTextFormat, ((int) e2.Percent).Clamp(0, 100)));
                                                }, true);
                                                saveTx.Stream.Flush();
                                                Exception[] innerExceptions = saveTx.Stream.Exceptions.ToArrayEx<Exception>();
                                                if (innerExceptions.Length != 0)
                                                {
                                                    saveTx.Rollback();
                                                    throw new AggregateException(innerExceptions);
                                                }
                                                saveTx.Commit();
                                            }
                                            catch (Exception)
                                            {
                                                switch (saveTx.State)
                                                {
                                                    case SaveTransactionState.Initialized:
                                                    case SaveTransactionState.FailedCommit:
                                                        saveTx.Rollback();
                                                        break;
                                                }
                                                throw;
                                            }
                                        }.Try();
                                    }
                                    finally
                                    {
                                        saveTask.SetState(TaskState.Finished);
                                    }
                                });
                            }
                            catch (Exception exception)
                            {
                                saveResult = Result.NewError(exception);
                                saveTask.SetState(TaskState.Finished);
                            }
                        };
                        progressDialog.ShowDialog(this);
                        DisposableUtil.Free<TaskProgressDialog>(ref progressDialog);
                        this.lastSaveTime = DateTime.Now;
                    }
                }
                catch (Exception exception)
                {
                    if ((saveResult == null) || !saveResult.IsError)
                    {
                        saveResult = Result.NewError(exception);
                    }
                    else
                    {
                        saveResult.Observe();
                        Exception[] exceptionArray1 = new Exception[] { exception, saveResult.Error };
                        saveResult = Result.NewError(new AggregateException(exceptionArray1));
                    }
                }
                this.ReturnScratchSurface(saveScratch);
                if (saveResult == null)
                {
                    MessageBoxUtil.ErrorBox(this, PdnResources.GetString("SaveImage.Error.Exception"));
                    return false;
                }
                if (saveResult.IsError)
                {
                    string str6;
                    if (saveResult.Error is UnauthorizedAccessException)
                    {
                        str6 = PdnResources.GetString("SaveImage.Error.UnauthorizedAccessException");
                    }
                    else if (saveResult.Error is SecurityException)
                    {
                        str6 = PdnResources.GetString("SaveImage.Error.SecurityException");
                    }
                    else if (saveResult.Error is DirectoryNotFoundException)
                    {
                        str6 = PdnResources.GetString("SaveImage.Error.DirectoryNotFoundException");
                    }
                    else if (saveResult.Error is IOException)
                    {
                        str6 = PdnResources.GetString("SaveImage.Error.IOException");
                    }
                    else if (saveResult.Error is OutOfMemoryException)
                    {
                        str6 = PdnResources.GetString("SaveImage.Error.OutOfMemoryException");
                    }
                    else
                    {
                        str6 = PdnResources.GetString("SaveImage.Error.Exception");
                    }
                    ExceptionDialog.ShowErrorDialog(this, str6, saveResult.Error);
                    saveResult.Observe();
                    return false;
                }
                base.Document.Dirty = false;
                this.SetDocumentSaveOptions(str, newFileType, newSaveConfigToken);
                Task task = this.AddToMruList();
                UIUtil.BeginFrame(this, true, delegate (UIUtil.IFrame frame) {
                    task.ResultAsync().Receive(delegate (Result r) {
                        r.Observe();
                        frame.Close();
                    }).Observe();
                });
                return true;
            }
        }

        public bool DoSaveAs()
        {
            using (new PushNullToolMode(this))
            {
                string str;
                PaintDotNet.FileType type;
                PaintDotNet.SaveConfigToken token;
                bool flag;
                Surface saveScratchSurface = this.BorrowScratchSurface(base.GetType() + ".DoSaveAs() handing off scratch surface to DoSaveAsDialog()");
                try
                {
                    flag = this.DoSaveAsDialog(out str, out type, out token, saveScratchSurface);
                }
                finally
                {
                    this.ReturnScratchSurface(saveScratchSurface);
                }
                if (flag)
                {
                    string str2;
                    PaintDotNet.FileType type2;
                    PaintDotNet.SaveConfigToken token2;
                    this.GetDocumentSaveOptions(out str2, out type2, out token2);
                    this.SetDocumentSaveOptions(str, type, token);
                    bool flag2 = this.DoSave(true);
                    if (!flag2)
                    {
                        this.SetDocumentSaveOptions(str2, type2, token2);
                    }
                    return flag2;
                }
                return false;
            }
        }

        private bool DoSaveAsDialog(out string newFileName, out PaintDotNet.FileType newFileType, out PaintDotNet.SaveConfigToken newSaveConfigToken, Surface saveScratchSurface)
        {
            FileTypeCollection types = new FileTypeCollection(from ft in FileTypes.GetFileTypes().FileTypes
                where ft.SupportsSaving
                select ft);
            using (PaintDotNet.SystemLayer.IFileSaveDialog dialog = CommonDialogs.CreateFileSaveDialog())
            {
                string str2;
                PaintDotNet.FileType type;
                PaintDotNet.SaveConfigToken token;
                string defaultSavePath;
                bool flag;
                string defaultSaveName;
                string defaultExtension;
                PaintDotNet.FileType pdn;
                PaintDotNet.SaveConfigToken token2;
                string str6;
                bool flag2;
                string str7;
                PaintDotNet.FileType type3;
                PaintDotNet.SaveConfigToken token3;
                dialog.AddExtension = true;
                dialog.CheckPathExists = true;
                dialog.OverwritePrompt = true;
                string str = types.ToString(false, null, true, false);
                dialog.Filter = str;
                this.GetDocumentSaveOptions(out str2, out type, out token);
                if (((base.Document.Layers.Count > 1) && (type != null)) && !type.SupportsLayers)
                {
                    pdn = PdnFileTypes.Pdn;
                    token2 = null;
                }
                else if (type == null)
                {
                    if (base.Document.Layers.Count == 1)
                    {
                        pdn = PdnFileTypes.Png;
                    }
                    else
                    {
                        pdn = PdnFileTypes.Pdn;
                    }
                    token2 = null;
                }
                else
                {
                    pdn = type;
                    token2 = token;
                }
                if (str2 == null)
                {
                    defaultSaveName = GetDefaultSaveName();
                    if ((defaultSaveName.Length > 0) && (defaultSaveName[0] == '.'))
                    {
                        defaultSaveName = defaultSaveName.Substring(1);
                        flag = true;
                    }
                    else
                    {
                        flag = false;
                    }
                    defaultSavePath = GetDefaultSavePath();
                    defaultExtension = pdn.DefaultExtension;
                }
                else
                {
                    string fullPath = Path.GetFullPath(str2);
                    defaultSavePath = Path.GetDirectoryName(fullPath);
                    string fileName = Path.GetFileName(fullPath);
                    if ((fileName == null) || (fileName.Length == 0))
                    {
                        defaultSaveName = GetDefaultSaveName();
                    }
                    if (fileName[0] == '.')
                    {
                        flag = true;
                        fileName = fileName.Substring(1);
                    }
                    else
                    {
                        flag = false;
                    }
                    string extension = Path.GetExtension(fileName);
                    if (pdn.SupportsExtension(extension))
                    {
                        defaultSaveName = Path.ChangeExtension(fileName, null);
                        defaultExtension = extension;
                    }
                    else if (types.IndexOfExtension(extension) != -1)
                    {
                        defaultSaveName = Path.ChangeExtension(fileName, null);
                        defaultExtension = pdn.DefaultExtension;
                    }
                    else
                    {
                        defaultSaveName = fileName;
                        defaultExtension = pdn.DefaultExtension;
                    }
                }
                if (pdn.SupportsExtension(defaultExtension))
                {
                    str6 = (flag ? "." : "") + defaultSaveName;
                }
                else
                {
                    str6 = (flag ? "." : "") + defaultSaveName + defaultExtension;
                }
                dialog.InitialDirectory = defaultSavePath;
                dialog.FileName = str6;
                dialog.FilterIndex = 1 + types.IndexOfFileType(pdn);
                dialog.Title = PdnResources.GetString("SaveAsDialog.Title");
                if (ShowFileDialog(this, dialog, false) != DialogResult.OK)
                {
                    flag2 = false;
                    str7 = null;
                    type3 = null;
                    token3 = null;
                }
                else
                {
                    str7 = dialog.FileName;
                    type3 = types[dialog.FilterIndex - 1];
                    flag2 = this.GetSaveConfigToken(type3, token2, out token3, saveScratchSurface);
                }
                if (flag2)
                {
                    newFileName = str7;
                    newFileType = type3;
                    newSaveConfigToken = token3;
                }
                else
                {
                    newFileName = null;
                    newFileType = null;
                    newSaveConfigToken = null;
                }
                return flag2;
            }
        }

        private void EndUpdateSelectionInfo(long area, RectInt32 bounds)
        {
            this.VerifyAccess<DocumentWorkspace>();
            this.isSelectionInfoUpdating = false;
            if (!base.IsDisposed)
            {
                this.selectionArea = new long?(area);
                this.selectionBounds = new RectInt32?(bounds);
                this.UpdateSelectionInfoInStatusBar();
                if (this.doesSelectionInfoNeedUpdate)
                {
                    this.BeginUpdateSelectionInfo();
                }
            }
        }

        private void EndZoomChanges()
        {
            this.zoomChangesCount--;
            if (this.zoomChangesCount == 0)
            {
                base.RaiseCanvasLayout();
            }
        }

        private static void GetBoundsAndArea<TList>(TList scans, RectInt32 areaClipRect, out RectInt32 bounds, out long area) where TList: IReadOnlyList<RectInt32>
        {
            int count = scans.Count;
            if (count == 0)
            {
                bounds = RectInt32.Empty;
                area = 0L;
            }
            else
            {
                RectInt32 a = scans[0];
                bounds = a;
                RectInt32 num3 = RectInt32.Intersect(a, areaClipRect);
                area = num3.Area;
                for (int i = 1; i < count; i++)
                {
                    RectInt32 b = scans[i];
                    bounds = RectInt32.Union(bounds, b);
                    RectInt32 num6 = RectInt32.Intersect(b, areaClipRect);
                    area += num6.Area;
                }
            }
        }

        private static string GetDefaultSaveName() => 
            PdnResources.GetString("Untitled.FriendlyName");

        private static string GetDefaultSavePath()
        {
            string virtualPath;
            try
            {
                virtualPath = ShellUtil.GetVirtualPath(VirtualFolderName.UserPictures, false);
                DirectoryInfo info = new DirectoryInfo(virtualPath);
            }
            catch (Exception)
            {
                virtualPath = "";
            }
            string str2 = AppSettings.Instance.File.DialogDirectory.Value;
            if (string.IsNullOrWhiteSpace(str2))
            {
                return virtualPath;
            }
            try
            {
                DirectoryInfo info2 = new DirectoryInfo(str2);
                if (!info2.Exists)
                {
                    str2 = virtualPath;
                }
            }
            catch (Exception)
            {
                str2 = virtualPath;
            }
            return str2;
        }

        public void GetDocumentSaveOptions(out string filePathResult, out PaintDotNet.FileType fileTypeResult, out PaintDotNet.SaveConfigToken saveConfigTokenResult)
        {
            filePathResult = this.filePath;
            fileTypeResult = this.fileType;
            if (this.saveConfigToken == null)
            {
                saveConfigTokenResult = null;
            }
            else
            {
                saveConfigTokenResult = (PaintDotNet.SaveConfigToken) this.saveConfigToken.Clone();
            }
        }

        public string GetFileFriendlyName()
        {
            if (this.filePath != null)
            {
                return Path.GetFileName(this.filePath);
            }
            return PdnResources.GetString("Untitled.FriendlyName");
        }

        public void GetLatestSelectionInfo(out string selectionInfoText, out ImageResource selectionInfoImage)
        {
            selectionInfoText = this.latestSelectionInfoText;
            selectionInfoImage = this.latestSelectionInfoImage;
        }

        private unsafe System.Drawing.Bitmap GetLivePreviewBitmap()
        {
            System.Drawing.Bitmap bitmap;
            try
            {
                bitmap = new System.Drawing.Bitmap(base.Width, base.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            }
            catch (Exception)
            {
                return null;
            }
            CanvasView canvasView = new CanvasView {
                CanvasExtentPadding = base.CanvasView.CanvasExtentPadding,
                ScaleRatio = base.CanvasView.ScaleRatio,
                ViewportCanvasOffset = base.CanvasView.ViewportCanvasOffset,
                ViewportSize = base.CanvasView.ViewportSize
            };
            DocumentCanvasLayer.SetIsHighQualityScalingEnabled(canvasView, DocumentCanvasLayer.GetIsHighQualityScalingEnabled(base.CanvasView));
            SelectionCanvasLayer.SetIsAntialiasedOutlineEnabled(canvasView, SelectionCanvasLayer.GetIsAntialiasedOutlineEnabled(base.CanvasView));
            SelectionCanvasLayer.SetIsAnimatedOutlineEnabled(canvasView, SelectionCanvasLayer.GetIsAnimatedOutlineEnabled(base.CanvasView));
            PixelGridCanvasLayer.SetIsPixelGridEnabled(canvasView, PixelGridCanvasLayer.GetIsPixelGridEnabled(base.CanvasView));
            canvasView.Canvas = base.DocumentCanvas;
            canvasView.IsVisible = true;
            BitmapData bitmapdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            try
            {
                using (SharedBitmap bitmap3 = new SharedBitmap(bitmap, bitmap.Width, bitmap.Height, PixelFormats.Bgr32, bitmapdata.Scan0, bitmapdata.Stride, 96.0, 96.0))
                {
                    using (IRenderTarget target = RenderTarget.FromBitmap(Direct2DFactory.PerThread, bitmap3))
                    {
                        using (IDrawingContext context = DrawingContext.FromRenderTarget(target))
                        {
                            canvasView.RenderTarget = target;
                            base.DocumentCanvas.PreRenderSync(canvasView);
                            RectFloat viewportCanvasBounds = (RectFloat) canvasView.ViewportCanvasBounds;
                            base.DocumentCanvas.BeforeRender(viewportCanvasBounds, canvasView);
                            base.DocumentCanvas.Render(context, viewportCanvasBounds, canvasView);
                            base.DocumentCanvas.AfterRender(viewportCanvasBounds, canvasView);
                        }
                    }
                }
                for (int i = 0; i < bitmapdata.Height; i++)
                {
                    uint* numPtr = (uint*) (bitmapdata.Scan0.ToPointer() + (i * bitmapdata.Stride));
                    uint* numPtr2 = numPtr + bitmapdata.Width;
                    while (numPtr < numPtr2)
                    {
                        numPtr[0] |= 0xff000000;
                        numPtr++;
                    }
                }
            }
            finally
            {
                bitmap.UnlockBits(bitmapdata);
                canvasView.Canvas = null;
            }
            return bitmap;
        }

        private bool GetSaveConfigToken(PaintDotNet.FileType currentFileType, PaintDotNet.SaveConfigToken currentSaveConfigToken, out PaintDotNet.SaveConfigToken newSaveConfigToken, Surface saveScratchSurface)
        {
            if (currentFileType.SupportsConfiguration)
            {
                using (SaveConfigDialog dialog = new SaveConfigDialog())
                {
                    dialog.ScratchSurface = saveScratchSurface;
                    ProgressEventHandler handler = delegate (object sender, ProgressEventArgs e) {
                        if ((e.Percent < 1.0) || (e.Percent >= 100.0))
                        {
                            this.AppWorkspace.Widgets.StatusBarProgress.ResetProgressStatusBar();
                            this.AppWorkspace.Widgets.StatusBarProgress.EraseProgressStatusBar();
                        }
                        else
                        {
                            this.AppWorkspace.Widgets.StatusBarProgress.SetProgressStatusBar(new double?(e.Percent));
                        }
                    };
                    dialog.Progress += handler;
                    dialog.Document = base.Document;
                    dialog.FileType = currentFileType;
                    PaintDotNet.SaveConfigToken lastSaveConfigToken = currentFileType.GetLastSaveConfigToken();
                    if ((currentSaveConfigToken != null) && (lastSaveConfigToken.GetType() == currentSaveConfigToken.GetType()))
                    {
                        dialog.SaveConfigToken = currentSaveConfigToken;
                    }
                    dialog.EnableInstanceOpacity = false;
                    DialogResult result = dialog.ShowDialog(this);
                    dialog.Progress -= handler;
                    this.AppWorkspace.Widgets.StatusBarProgress.ResetProgressStatusBar();
                    this.AppWorkspace.Widgets.StatusBarProgress.EraseProgressStatusBar();
                    if (result == DialogResult.OK)
                    {
                        newSaveConfigToken = dialog.SaveConfigToken;
                        return true;
                    }
                    newSaveConfigToken = null;
                    return false;
                }
            }
            newSaveConfigToken = currentFileType.GetLastSaveConfigToken();
            return true;
        }

        public object GetStaticToolData(System.Type toolType)
        {
            object obj2;
            this.staticToolData.TryGetValue(toolType, out obj2);
            return obj2;
        }

        public System.Type GetToolType()
        {
            if (this.Tool != null)
            {
                return this.Tool.GetType();
            }
            return null;
        }

        public string GetWindowTitle()
        {
            string appName = PdnInfo.AppName;
            string fileFriendlyName = this.GetFileFriendlyName();
            string str5 = string.Format(PdnResources.GetString("MainForm.Title.Format"), fileFriendlyName, appName);
            if (base.Document != null)
            {
                return str5;
            }
            return appName;
        }

        private void InitializeComponent()
        {
            this.toolPulseTimer = new System.Windows.Forms.Timer();
            this.toolPulseTimer.Interval = 0x10;
            this.toolPulseTimer.Tick += new EventHandler(this.OnToolPulseTimerTick);
            this.ReevaluateToolPulseTimerEnabled();
        }

        private static void InitializeToolInfos()
        {
            int index = 0;
            toolInfos = new ToolInfo[tools.Length];
            foreach (System.Type type in tools)
            {
                using (PaintDotNet.Tools.Tool tool = CreateTool(type, null))
                {
                    toolInfos[index] = tool.Info;
                    index++;
                }
            }
        }

        private static void InitializeTools()
        {
            tools = new System.Type[] { 
                typeof(RectangleSelectTool), typeof(MoveSelectedPixelsTool), typeof(LassoSelectTool), typeof(MoveSelectionTool), typeof(EllipseSelectTool), typeof(ZoomTool), typeof(MagicWandTool), typeof(PanTool), typeof(PaintBucketTool), typeof(GradientTool), typeof(PaintBrushTool), typeof(EraserTool), typeof(PencilTool), typeof(ColorPickerTool), typeof(CloneStampTool), typeof(RecolorTool),
                typeof(TextTool), typeof(LineCurveTool), typeof(ShapesTool)
            };
        }

        private void InvalidateSelectionInfo()
        {
            this.VerifyAccess<DocumentWorkspace>();
            if (!base.IsDisposed)
            {
                this.doesSelectionInfoNeedUpdate = true;
                if (!this.isSelectionInfoUpdating)
                {
                    this.BeginUpdateSelectionInfo();
                }
            }
        }

        private void InvalidateTabbedThumbnail()
        {
            this.VerifyThreadAccess();
            if (!this.isUpdateTabbedThumbnailQueued && base.IsHandleCreated)
            {
                this.isUpdateTabbedThumbnailQueued = true;
                base.BeginInvoke(delegate {
                    this.isUpdateTabbedThumbnailQueued = false;
                    this.UpdateTabbedThumbnail();
                });
            }
        }

        private void LayerInsertedHandler(object sender, IndexEventArgs e)
        {
            Layer layer = (Layer) base.Document.Layers[e.Index];
            this.ActiveLayer = layer;
            layer.PropertyChanging += new PropertyEventHandler(this.LayerPropertyChangingHandler);
            layer.PropertyChanged += new PropertyEventHandler(this.LayerPropertyChangedHandler);
        }

        private void LayerPropertyChangedHandler(object sender, PropertyEventArgs e)
        {
            Layer layer = (Layer) sender;
            if ((!layer.Visible && (layer == this.ActiveLayer)) && ((base.Document.Layers.Count > 1) && !this.History.IsExecutingMemento))
            {
                this.SelectClosestVisibleLayer(layer);
            }
        }

        private void LayerPropertyChangingHandler(object sender, PropertyEventArgs e)
        {
            string format = PdnResources.GetString("LayerPropertyChanging.HistoryMementoNameFormat");
            string str2 = "Layer.Properties.{0}.Name";
            string str4 = PdnResources.GetString(string.Format(str2, e.PropertyName));
            string name = string.Format(format, str4);
            if (e.PropertyName == Layer.VisiblePropertyName)
            {
                using (new PushNullToolMode(this))
                {
                }
            }
            LayerPropertyHistoryMemento memento = new LayerPropertyHistoryMemento(name, PdnResources.GetImageResource("Icons.MenuLayersLayerPropertiesIcon.png"), this, base.Document.Layers.IndexOf(sender));
            this.History.PushNewMemento(memento);
        }

        private void LayerRemovedHandler(object sender, IndexEventArgs e)
        {
        }

        private void LayerRemovingHandler(object sender, IndexEventArgs e)
        {
            int num;
            Layer layer = (Layer) base.Document.Layers[e.Index];
            layer.PropertyChanging -= new PropertyEventHandler(this.LayerPropertyChangingHandler);
            layer.PropertyChanged -= new PropertyEventHandler(this.LayerPropertyChangedHandler);
            if (e.Index == 0)
            {
                num = 1;
            }
            else if (e.Index == (base.Document.Layers.Count - 1))
            {
                num = e.Index - 1;
            }
            else
            {
                num = e.Index - 1;
            }
            if ((num >= 0) && (num < base.Document.Layers.Count))
            {
                this.ActiveLayer = (Layer) base.Document.Layers[num];
            }
            else if (base.Document.Layers.Count == 0)
            {
                this.ActiveLayer = null;
            }
            else
            {
                this.ActiveLayer = (Layer) base.Document.Layers[0];
            }
        }

        public static Document LoadDocument(Control owner, string fileName, out PaintDotNet.FileType fileTypeResult, ProgressEventHandler progressCallback)
        {
            PaintDotNet.FileType fileType;
            fileTypeResult = null;
            try
            {
                FileTypeCollection fileTypes = FileTypes.GetFileTypes();
                int num = fileTypes.IndexOfExtension(Path.GetExtension(fileName));
                if (num == -1)
                {
                    MessageBoxUtil.ErrorBox(owner, PdnResources.GetString("LoadImage.Error.ImageTypeNotRecognized"));
                    return null;
                }
                fileType = fileTypes[num];
                fileTypeResult = fileType;
            }
            catch (ArgumentException exception)
            {
                string message = string.Format(PdnResources.GetString("LoadImage.Error.InvalidFileName.Format"), fileName);
                ExceptionDialog.ShowErrorDialog(owner, message, exception);
                return null;
            }
            Stream underlyingStream = null;
            Result<Document> docResult = Result.NewError<Document>(new Exception(), false);
            try
            {
                bool sendProgressEvents = true;
                underlyingStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                long streamLength = underlyingStream.Length;
                long totalBytes = 0L;
                SiphonStream siphonStream = new SiphonStream(underlyingStream);
                IOEventHandler handler = null;
                handler = delegate (object sender, IOEventArgs e) {
                    () => owner.BeginInvoke(() => delegate {
                        if ((progressCallback > null) & sendProgressEvents)
                        {
                            totalBytes += e.Count;
                            double percent = (100.0 * (((double) totalBytes) / ((double) streamLength))).Clamp(0.0, 100.0);
                            progressCallback(null, new ProgressEventArgs(percent));
                        }
                    }.Try().Observe()).Try().Observe();
                };
                siphonStream.IOFinished += handler;
                UIUtil.BeginFrame(owner, true, delegate (UIUtil.IFrame ifc) {
                    try
                    {
                        Work.QueueWorkItem(delegate {
                            try
                            {
                                docResult = new Func<Stream, Document>(fileType.Load).Eval<Stream, Document>(siphonStream);
                            }
                            finally
                            {
                                ifc.Close();
                            }
                        });
                    }
                    catch (Exception exception)
                    {
                        docResult = Result.NewError<Document>(exception);
                        ifc.Close();
                    }
                });
                sendProgressEvents = false;
                if (progressCallback != null)
                {
                    progressCallback(null, new ProgressEventArgs(100.0));
                }
                siphonStream.IOFinished -= handler;
                siphonStream.Close();
            }
            catch (Exception exception2)
            {
                docResult = Result.NewError<Document>(exception2);
            }
            if (underlyingStream != null)
            {
                underlyingStream.Close();
                underlyingStream = null;
            }
            if (docResult.IsValue)
            {
                Metadata metadata = docResult.Value.Metadata;
                metadata.RemoveExifValues(ExifTagID.JPEGInterchangeFormat);
                metadata.RemoveExifValues(ExifTagID.JPEGInterchangeFormatLength);
                metadata.RemoveExifValues(ExifTagID.ThumbnailData);
                metadata.RemoveExifValues(ExifTagID.Orientation);
                return docResult.Value;
            }
            string stringName = Result.NewError<string>(docResult.Error, false).Repair<string, ArgumentException>(delegate (ArgumentException ex) {
                if (fileName.Length == 0)
                {
                    return "LoadImage.Error.BlankFileName";
                }
                return "LoadImage.Error.ArgumentException";
            }).Repair<string, UnauthorizedAccessException>(ex => "LoadImage.Error.UnauthorizedAccessException").Repair<string, SecurityException>(ex => "LoadImage.Error.SecurityException").Repair<string, FileNotFoundException>(ex => "LoadImage.Error.FileNotFoundException").Repair<string, DirectoryNotFoundException>(ex => "LoadImage.Error.DirectoryNotFoundException").Repair<string, PathTooLongException>(ex => "LoadImage.Error.PathTooLongException").Repair<string, IOException>(ex => "LoadImage.Error.IOException").Repair<string, SerializationException>(ex => "LoadImage.Error.SerializationException").Repair<string, OutOfMemoryException>(ex => "LoadImage.Error.OutOfMemoryException").Repair<string>(ex => "LoadImage.Error.Exception").Value;
            ExceptionDialog.ShowErrorDialog(owner, PdnResources.GetString(stringName), docResult.Error);
            return null;
        }

        protected override void OnCompositionUpdated()
        {
            this.InvalidateTabbedThumbnail();
            base.OnCompositionUpdated();
        }

        protected override void OnDocumentChanged()
        {
            if (base.Document == null)
            {
                this.ActiveLayer = null;
            }
            else
            {
                if (this.activeTool != null)
                {
                    throw new InvalidOperationException($"Tool ({this.activeTool.GetType().Name}) was not deactivated while Document was being changed");
                }
                if (this.scratchSurface != null)
                {
                    if (this.isScratchSurfaceBorrowed)
                    {
                        throw new InvalidOperationException("scratchSurface is currently borrowed: " + this.borrowScratchSurfaceReason);
                    }
                    if ((base.Document == null) || (this.scratchSurface.Size != base.Document.Size))
                    {
                        this.scratchSurface.Dispose();
                        this.scratchSurface = null;
                    }
                }
                if (this.scratchSurface == null)
                {
                    this.scratchSurface = new Surface(base.Document.Size, SurfaceCreationFlags.DoNotZeroFillHint);
                }
                this.Selection.ClipRectangle = base.Document.Bounds();
                base.Document.Metadata.Changed += new EventHandler(this.OnDocumentMetadataChanged);
                foreach (Layer layer in base.Document.Layers)
                {
                    layer.PropertyChanging += new PropertyEventHandler(this.LayerPropertyChangingHandler);
                    layer.PropertyChanged += new PropertyEventHandler(this.LayerPropertyChangedHandler);
                }
                base.Document.Layers.RemovingAt += new IndexEventHandler(this.LayerRemovingHandler);
                base.Document.Layers.RemovedAt += new IndexEventHandler(this.LayerRemovedHandler);
                base.Document.Layers.Inserted += new IndexEventHandler(this.LayerInsertedHandler);
                if (!base.Document.Layers.Contains(this.ActiveLayer))
                {
                    if (base.Document.Layers.Count > 0)
                    {
                        if ((this.savedAli >= 0) && (this.savedAli < base.Document.Layers.Count))
                        {
                            this.ActiveLayer = (Layer) base.Document.Layers[this.savedAli];
                        }
                        else
                        {
                            this.ActiveLayer = (Layer) base.Document.Layers[0];
                        }
                    }
                    else
                    {
                        this.ActiveLayer = null;
                    }
                }
                foreach (Layer layer2 in base.Document.Layers)
                {
                    layer2.Invalidate();
                }
                bool dirty = base.Document.Dirty;
                base.Document.Invalidate();
                base.Document.Dirty = dirty;
                this.ZoomBasis = this.savedZb;
                if (this.savedZb == PaintDotNet.ZoomBasis.ScaleFactor)
                {
                    base.ScaleFactor = this.savedSf;
                }
            }
            this.PopNullTool();
            base.AutoScrollPosition = new Point(0, 0);
            base.OnDocumentChanged();
        }

        protected override void OnDocumentChanging(Document newDocument)
        {
            base.OnDocumentChanging(newDocument);
            this.savedZb = this.ZoomBasis;
            this.savedSf = base.ScaleFactor;
            if (this.ActiveLayer != null)
            {
                this.savedAli = this.ActiveLayerIndex;
            }
            else
            {
                this.savedAli = -1;
            }
            if (newDocument != null)
            {
                this.UpdateExifTags(newDocument);
            }
            if (base.Document != null)
            {
                base.Document.Metadata.Changed -= new EventHandler(this.OnDocumentMetadataChanged);
                foreach (Layer layer in base.Document.Layers)
                {
                    layer.PropertyChanging -= new PropertyEventHandler(this.LayerPropertyChangingHandler);
                    layer.PropertyChanged -= new PropertyEventHandler(this.LayerPropertyChangedHandler);
                }
                base.Document.Layers.RemovingAt -= new IndexEventHandler(this.LayerRemovingHandler);
                base.Document.Layers.RemovedAt -= new IndexEventHandler(this.LayerRemovedHandler);
                base.Document.Layers.Inserted -= new IndexEventHandler(this.LayerInsertedHandler);
            }
            this.staticToolData.Clear();
            this.PushNullTool();
            this.ActiveLayer = null;
            if (this.scratchSurface != null)
            {
                if (this.isScratchSurfaceBorrowed)
                {
                    ExceptionUtil.ThrowInvalidOperationException("scratchSurface is currently borrowed: " + this.borrowScratchSurfaceReason);
                }
                if ((newDocument == null) || (newDocument.Size != this.scratchSurface.Size))
                {
                    this.scratchSurface.Dispose();
                    this.scratchSurface = null;
                }
            }
            if (!this.Selection.IsEmpty)
            {
                this.Selection.Reset();
            }
        }

        private void OnDocumentMetadataChanged(object sender, EventArgs e)
        {
            if (base.InvokeRequired)
            {
                try
                {
                    base.BeginInvoke(new Action(this.InvalidateSelectionInfo));
                }
                catch (Exception)
                {
                }
            }
            else
            {
                this.InvalidateSelectionInfo();
            }
        }

        private void OnEnableAnimationsValueChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            SelectionCanvasLayer.SetIsAnimatedOutlineEnabled(base.CanvasView, e.NewValue);
        }

        private void OnEnableAntialiasedSelectionOutlineValueChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            SelectionCanvasLayer.SetIsAntialiasedOutlineEnabled(base.CanvasView, e.NewValue);
        }

        private void OnEnableHighQualityScalingValueChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            DocumentCanvasLayer.SetIsHighQualityScalingEnabled(base.CanvasView, e.NewValue);
        }

        protected virtual void OnFilePathChanged()
        {
            this.InvalidateTabbedThumbnail();
            this.FilePathChanged.Raise(this);
        }

        protected void OnLayerChanged()
        {
            base.Focus();
            this.ActiveLayerChanged.Raise(this);
        }

        protected void OnLayerChanging()
        {
            this.ActiveLayerChanging.Raise(this);
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            base.RaiseCanvasLayout();
        }

        protected override void OnLoad(EventArgs e)
        {
            if (this.appWorkspace == null)
            {
                ExceptionUtil.ThrowInvalidOperationException("Must set the Workspace property");
            }
            base.OnLoad(e);
        }

        private void OnRequestActivate(CancelEventArgs e)
        {
            CancelEventHandler requestActivate = this.RequestActivate;
            if (requestActivate != null)
            {
                requestActivate(this, e);
            }
        }

        private void OnRequestClose(CancelEventArgs e)
        {
            CancelEventHandler requestClose = this.RequestClose;
            if (requestClose != null)
            {
                requestClose(this, e);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            base.RaiseCanvasLayout();
        }

        protected virtual void OnSaveOptionsChanged()
        {
            this.InvalidateTabbedThumbnail();
            this.SaveOptionsChanged.Raise(this);
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            this.InvalidateSelectionInfo();
            this.UpdateRulerSelectionTinting();
            this.UpdateSelectionInfoInStatusBar();
        }

        private void OnShowTaskbarPreviewsSettingChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            if (base.InvokeRequired)
            {
                try
                {
                    base.BeginInvoke(new Action(this.InvalidateTabbedThumbnail));
                }
                catch (Exception)
                {
                }
            }
            else
            {
                this.InvalidateTabbedThumbnail();
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.PerformLayout();
            base.OnSizeChanged(e);
            base.RaiseCanvasLayout();
        }

        protected virtual void OnStatusChanged()
        {
            this.StatusChanged.Raise(this);
        }

        private void OnTabbedThumbnailActivated(object sender, TabbedThumbnailEventArgs e)
        {
            PdnBaseForm form = base.FindForm() as PdnBaseForm;
            if (form != null)
            {
                base.BeginInvoke(() => form.RestoreWindow());
            }
            this.Activate();
        }

        private void OnTabbedThumbnailBitmapRequested(object sender, TabbedThumbnailBitmapRequestedEventArgs e)
        {
            if ((this.TabbedThumbnail != null) && (e.RequestedSize.HasValue && (base.Document != null)))
            {
                try
                {
                    IRenderer<ColorBgra> renderer = this.CreateThumbnailRenderer(e.RequestedSize.Value.ToSizeInt32());
                    using (ISurface<ColorBgra> surface = SurfaceAllocator.Bgra.Allocate<ColorBgra>(renderer.Size<ColorBgra>(), AllocationOptions.Default))
                    {
                        renderer.Parallelize<ColorBgra>(TilingStrategy.HorizontalSlices, 4, WorkItemQueuePriority.Normal).Render<ColorBgra>(surface);
                        System.Drawing.Bitmap bitmap = surface.ToGdipBitmap();
                        e.Bitmap = bitmap;
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        private void OnTabbedThumbnailChanged(Microsoft.WindowsAPICodePack.Taskbar.TabbedThumbnail oldValue, Microsoft.WindowsAPICodePack.Taskbar.TabbedThumbnail newValue)
        {
            this.VerifyThreadAccess();
            if (oldValue != null)
            {
                oldValue.TabbedThumbnailActivated -= new EventHandler<TabbedThumbnailEventArgs>(this.OnTabbedThumbnailActivated);
                oldValue.TabbedThumbnailBitmapRequested -= new EventHandler<TabbedThumbnailBitmapRequestedEventArgs>(this.OnTabbedThumbnailBitmapRequested);
                oldValue.TabbedThumbnailLivePreviewBitmapRequested -= new EventHandler<TabbedThumbnailBitmapRequestedEventArgs>(this.OnTabbedThumbnailLivePreviewBitmapRequested);
                oldValue.TabbedThumbnailClosed -= new EventHandler<TabbedThumbnailClosedEventArgs>(this.OnTabbedThumbnailClosed);
                TaskbarManager.Instance.TabbedThumbnail.RemoveThumbnailPreview(oldValue);
                oldValue.Dispose();
            }
            if (newValue != null)
            {
                newValue.TabbedThumbnailActivated += new EventHandler<TabbedThumbnailEventArgs>(this.OnTabbedThumbnailActivated);
                newValue.TabbedThumbnailBitmapRequested += new EventHandler<TabbedThumbnailBitmapRequestedEventArgs>(this.OnTabbedThumbnailBitmapRequested);
                newValue.TabbedThumbnailLivePreviewBitmapRequested += new EventHandler<TabbedThumbnailBitmapRequestedEventArgs>(this.OnTabbedThumbnailLivePreviewBitmapRequested);
                newValue.TabbedThumbnailClosed += new EventHandler<TabbedThumbnailClosedEventArgs>(this.OnTabbedThumbnailClosed);
                try
                {
                    TaskbarManager.Instance.TabbedThumbnail.AddThumbnailPreview(newValue);
                }
                catch (FileNotFoundException)
                {
                }
                catch (OutOfMemoryException)
                {
                }
            }
            this.TabbedThumbnailChanged.Raise<Microsoft.WindowsAPICodePack.Taskbar.TabbedThumbnail>(this, oldValue, newValue);
        }

        private void OnTabbedThumbnailClosed(object sender, TabbedThumbnailClosedEventArgs e)
        {
            base.FindForm().Activate();
            e.Cancel = !this.Close();
        }

        private void OnTabbedThumbnailLivePreviewBitmapRequested(object sender, TabbedThumbnailBitmapRequestedEventArgs e)
        {
            System.Drawing.Bitmap livePreviewBitmap;
            try
            {
                livePreviewBitmap = this.GetLivePreviewBitmap();
            }
            catch (Exception)
            {
                return;
            }
            e.Bitmap = livePreviewBitmap;
        }

        protected void OnToolChanged()
        {
            if (base.ContainsFocus)
            {
                base.OnDocumentEnter(EventArgs.Empty);
            }
            this.ToolChanged.Raise(this);
        }

        protected void OnToolChanging()
        {
            this.ToolChanging.Raise(this);
            if (base.ContainsFocus)
            {
                base.OnDocumentLeave(EventArgs.Empty);
            }
        }

        private void OnToolPulseTimerTick(object sender, EventArgs e)
        {
            if ((base.FindForm() == null) || (base.FindForm().WindowState == FormWindowState.Minimized))
            {
                this.ReevaluateToolPulseTimerEnabled();
            }
            else if (((this.Tool != null) && this.Tool.Active) && this.Tool.IsPulseEnabled)
            {
                this.Tool.PerformPulse();
            }
        }

        private void OnToolSettingsSelectionRenderingQualityChanged(object sender, ValueChangedEventArgs<SelectionRenderingQuality> e)
        {
            SelectionCanvasLayer.SetSelectionRenderingQuality(base.CanvasView, e.NewValue);
        }

        protected override void OnUnitsChanged()
        {
            if (!this.Selection.IsEmpty)
            {
                this.UpdateSelectionInfoInStatusBar();
            }
            base.OnUnitsChanged();
        }

        protected virtual void OnZoomBasisChanged()
        {
            this.ZoomBasisChanged.Raise(this);
        }

        protected virtual void OnZoomBasisChanging()
        {
            this.ZoomBasisChanging.Raise(this);
        }

        bool IThreadAffinitizedObject.CheckAccess() => 
            this.CheckThreadAccess();

        void IThreadAffinitizedObject.VerifyAccess()
        {
            this.VerifyThreadAccess();
        }

        public void PerformAction(DocumentWorkspaceAction action)
        {
            bool flag = false;
            if ((action.ActionFlags & ActionFlags.KeepToolActive) != ActionFlags.KeepToolActive)
            {
                this.PushNullTool();
                base.Update();
                flag = true;
            }
            try
            {
                using (new WaitCursorChanger(this))
                {
                    HistoryMemento memento = action.PerformAction(this);
                    if (memento != null)
                    {
                        this.History.PushNewMemento(memento);
                    }
                }
            }
            finally
            {
                if (flag)
                {
                    this.PopNullTool();
                }
            }
        }

        public void PerformAction(System.Type actionType, string newName, ImageResource icon)
        {
            using (new WaitCursorChanger(this))
            {
                System.Type[] types = new System.Type[] { typeof(DocumentWorkspace) };
                object[] parameters = new object[] { this };
                DocumentWorkspaceAction action = actionType.GetConstructor(types).Invoke(parameters) as DocumentWorkspaceAction;
                if (action != null)
                {
                    bool flag = false;
                    if ((action.ActionFlags & ActionFlags.KeepToolActive) != ActionFlags.KeepToolActive)
                    {
                        this.PushNullTool();
                        base.Update();
                        flag = true;
                    }
                    try
                    {
                        HistoryMemento memento = action.PerformAction(this);
                        if (memento != null)
                        {
                            memento.Name = newName;
                            memento.Image = icon;
                            this.History.PushNewMemento(memento);
                        }
                    }
                    finally
                    {
                        if (flag)
                        {
                            this.PopNullTool();
                        }
                    }
                }
            }
        }

        public void PopNullTool()
        {
            this.nullToolCount--;
            if (this.nullToolCount == 0)
            {
                this.SetToolFromType(this.preNullTool);
                this.preNullTool = null;
            }
            else if (this.nullToolCount < 0)
            {
                ExceptionUtil.ThrowInvalidOperationException("PopNullTool() call was not matched with PushNullTool()");
            }
        }

        public void PushNullTool()
        {
            if (this.nullToolCount == 0)
            {
                this.preNullTool = this.GetToolType();
                this.ClearTool();
                this.nullToolCount = 1;
            }
            else
            {
                this.nullToolCount++;
            }
        }

        private void ReevaluateToolPulseTimerEnabled()
        {
            this.VerifyAccess<DocumentWorkspace>();
            bool flag = (this.toolPulseTimer != null) && this.toolPulseTimer.Enabled;
            if (this.toolPulseTimer != null)
            {
                if ((base.FindForm() == null) || (base.FindForm().WindowState == FormWindowState.Minimized))
                {
                    this.toolPulseTimer.Enabled = false;
                }
                else if ((((this.toolPulseTimer != null) && this.IsToolPulseEnabled) && ((this.Tool != null) && this.Tool.Active)) && this.Tool.IsPulseEnabled)
                {
                    this.toolPulseTimer.Enabled = true;
                }
                else
                {
                    this.toolPulseTimer.Enabled = false;
                }
            }
            bool flag2 = (this.toolPulseTimer != null) && this.toolPulseTimer.Enabled;
        }

        public void ResumeToolCursorChanges()
        {
            this.suspendToolCursorChanges--;
            if ((this.suspendToolCursorChanges <= 0) && (this.activeTool != null))
            {
                this.Cursor = this.activeTool.Cursor;
            }
        }

        public void ReturnScratchSurface(Surface borrowedScratchSurface)
        {
            if (!this.isScratchSurfaceBorrowed)
            {
                ExceptionUtil.ThrowInvalidOperationException("ScratchSurface wasn't borrowed");
            }
            if (this.scratchSurface != borrowedScratchSurface)
            {
                ExceptionUtil.ThrowInvalidOperationException("returned ScratchSurface doesn't match the real one");
            }
            this.isScratchSurfaceBorrowed = false;
            this.borrowScratchSurfaceReason = string.Empty;
        }

        public void SelectClosestVisibleLayer(Layer layer)
        {
            int index = base.Document.Layers.IndexOf(layer);
            int num2 = index;
            for (int i = 0; i < base.Document.Layers.Count; i++)
            {
                int num4 = index - i;
                int num5 = index + i;
                if (((num4 >= 0) && (num4 < base.Document.Layers.Count)) && ((Layer) base.Document.Layers[num4]).Visible)
                {
                    num2 = num4;
                    break;
                }
                if (((num5 >= 0) && (num5 < base.Document.Layers.Count)) && ((Layer) base.Document.Layers[num5]).Visible)
                {
                    num2 = num5;
                    break;
                }
            }
            if (num2 != index)
            {
                this.ActiveLayer = (Layer) base.Document.Layers[num2];
            }
        }

        public void SetDocumentSaveOptions(string newFilePath, PaintDotNet.FileType newFileType, PaintDotNet.SaveConfigToken newSaveConfigToken)
        {
            this.filePath = newFilePath;
            this.OnFilePathChanged();
            this.fileType = newFileType;
            if (newSaveConfigToken == null)
            {
                this.saveConfigToken = null;
            }
            else
            {
                this.saveConfigToken = (PaintDotNet.SaveConfigToken) newSaveConfigToken.Clone();
            }
            this.OnSaveOptionsChanged();
        }

        public void SetStaticToolData(System.Type toolType, object data)
        {
            this.staticToolData[toolType] = data;
        }

        public void SetStatus(string newStatusText, ImageResource newStatusIcon)
        {
            this.statusText = newStatusText;
            this.statusIcon = newStatusIcon;
            this.OnStatusChanged();
        }

        public void SetTool(PaintDotNet.Tools.Tool copyMe)
        {
            this.OnToolChanging();
            if (this.activeTool != null)
            {
                this.previousActiveToolType = this.activeTool.GetType();
                this.activeTool.CursorChanged -= new EventHandler(this.ToolCursorChangedHandler);
                this.activeTool.IsPulseEnabledChanged -= new EventHandler(this.ToolIsPulseEnabledChangedHandler);
                this.activeTool.PerformDeactivate();
                this.activeTool.Dispose();
                this.activeTool = null;
            }
            if (copyMe == null)
            {
                this.IsToolPulseEnabled = false;
            }
            else
            {
                this.activeTool = this.CreateTool(copyMe.GetType());
                this.activeTool.PerformActivate();
                this.activeTool.CursorChanged += new EventHandler(this.ToolCursorChangedHandler);
                this.activeTool.IsPulseEnabledChanged += new EventHandler(this.ToolIsPulseEnabledChangedHandler);
                if (this.suspendToolCursorChanges <= 0)
                {
                    this.Cursor = this.activeTool.Cursor;
                }
                this.IsToolPulseEnabled = true;
            }
            this.ReevaluateToolPulseTimerEnabled();
            this.OnToolChanged();
        }

        public void SetToolFromType(System.Type toolType)
        {
            if (toolType != this.GetToolType())
            {
                if (toolType == null)
                {
                    this.ClearTool();
                }
                else
                {
                    PaintDotNet.Tools.Tool copyMe = this.CreateTool(toolType);
                    this.SetTool(copyMe);
                }
            }
        }

        public static DialogResult ShowFileDialog(Control owner, PaintDotNet.SystemLayer.IFileDialog fd, bool populateInitialDir)
        {
            string fileName;
            if (populateInitialDir)
            {
                string path = AppSettings.Instance.File.DialogDirectory.Value;
                bool exists = false;
                try
                {
                    DirectoryInfo info = new DirectoryInfo(path);
                    using (new WaitCursorChanger(owner))
                    {
                        exists = info.Exists;
                        if (!info.Exists)
                        {
                            path = fd.InitialDirectory;
                        }
                    }
                }
                catch (Exception)
                {
                    path = fd.InitialDirectory;
                }
                fd.InitialDirectory = path;
            }
            DialogResult result = fd.ShowDialog(owner);
            if (result != DialogResult.OK)
            {
                return result;
            }
            if (fd is PaintDotNet.SystemLayer.IFileOpenDialog)
            {
                string[] fileNames;
                try
                {
                    fileNames = ((PaintDotNet.SystemLayer.IFileOpenDialog) fd).FileNames;
                }
                catch (PathTooLongException exception)
                {
                    ExceptionDialog.ShowErrorDialog(owner, exception);
                    return DialogResult.Cancel;
                }
                if (fileNames.Length != 0)
                {
                    fileName = fileNames[0];
                }
                else
                {
                    fileName = null;
                }
            }
            else
            {
                if (fd is PaintDotNet.SystemLayer.IFileSaveDialog)
                {
                    try
                    {
                        fileName = ((PaintDotNet.SystemLayer.IFileSaveDialog) fd).FileName;
                        goto Label_00D3;
                    }
                    catch (PathTooLongException exception2)
                    {
                        ExceptionDialog.ShowErrorDialog(owner, exception2);
                        return DialogResult.Cancel;
                    }
                }
                throw new InvalidOperationException();
            }
        Label_00D3:
            if (fileName == null)
            {
                throw new FileNotFoundException();
            }
            string directoryName = Path.GetDirectoryName(fileName);
            AppSettings.Instance.File.DialogDirectory.Value = directoryName;
            return result;
        }

        public void SuspendToolCursorChanges()
        {
            this.suspendToolCursorChanges++;
        }

        private void ToolCursorChangedHandler(object sender, EventArgs e)
        {
            if (this.suspendToolCursorChanges <= 0)
            {
                this.Cursor = this.activeTool.Cursor;
            }
        }

        private void ToolIsPulseEnabledChangedHandler(object sender, EventArgs e)
        {
            this.ReevaluateToolPulseTimerEnabled();
        }

        private void UpdateExifTags(Document document)
        {
            System.Drawing.Imaging.PropertyItem item = Exif.CreateAscii(ExifTagID.Software, PdnInfo.GetInvariantProductName());
            System.Drawing.Imaging.PropertyItem[] items = new System.Drawing.Imaging.PropertyItem[] { item };
            document.Metadata.ReplaceExifValues(ExifTagID.Software, items);
        }

        public void UpdateRulerSelectionTinting()
        {
            if (base.RulersEnabled)
            {
                RectDouble boundsF = this.Selection.GetBoundsF();
                base.SetHighlightRectangle(boundsF);
            }
        }

        private void UpdateSelectionInfo(SynchronizationContext syncContext, RectInt32 docBounds, Result<GeometryList> lazyClippingMask, Result<IReadOnlyList<RectInt32>> lazyScans)
        {
            if (this.CheckAccess<DocumentWorkspace>())
            {
                throw new InvalidOperationException();
            }
            long area = 0L;
            RectInt32 bounds = RectInt32.Zero;
            IReadOnlyList<RectInt32> scans = lazyScans.Value;
            if (scans.Count == 0)
            {
                RectDouble num3 = RectDouble.Intersect(lazyClippingMask.Value.Bounds, docBounds);
                bounds = new RectInt32((int) Math.Floor(num3.X), (int) Math.Floor(num3.Y), 0, 0);
            }
            else
            {
                GetBoundsAndArea<IReadOnlyList<RectInt32>>(scans, docBounds, out bounds, out area);
            }
            syncContext.Post((Action) (() => this.EndUpdateSelectionInfo(area, bounds)));
        }

        private void UpdateSelectionInfoInStatusBar()
        {
            string str;
            string str2;
            string str3;
            string str4;
            string str5;
            string str6;
            string str7;
            string str8;
            double? nullable4;
            double num4;
            double? nullable5;
            if (this.Selection.IsEmpty)
            {
                this.latestSelectionInfoText = null;
                this.latestSelectionInfoImage = null;
                this.UpdateStatusBarToToolHelpText();
                return;
            }
            if (!this.selectionArea.HasValue || !this.selectionBounds.HasValue)
            {
                this.InvalidateSelectionInfo();
            }
            long? selectionArea = this.selectionArea;
            long num = selectionArea.HasValue ? selectionArea.GetValueOrDefault() : -1L;
            RectInt32? selectionBounds = this.selectionBounds;
            RectInt32 num2 = selectionBounds.HasValue ? selectionBounds.GetValueOrDefault() : RectInt32.Zero;
            base.Document.CoordinatesToStrings(base.Units, num2.X, num2.Y, out str3, out str4, out str2);
            base.Document.CoordinatesToStrings(base.Units, num2.Width, num2.Height, out str6, out str7, out str5);
            NumberFormatInfo provider = (NumberFormatInfo) CultureInfo.CurrentCulture.NumberFormat.Clone();
            if (base.Units == MeasurementUnit.Pixel)
            {
                provider.NumberDecimalDigits = 0;
                str8 = num.ToString("N", provider);
            }
            else
            {
                provider.NumberDecimalDigits = 2;
                str8 = base.Document.PixelAreaToPhysicalArea((double) num, base.Units).ToString("N", provider);
            }
            string str9 = PdnResources.GetString("MeasurementUnit." + base.Units.ToString() + ".Plural");
            double? nullable = null;
            if (!nullable.HasValue)
            {
                MoveTool tool = this.Tool as MoveTool;
                if ((tool != null) && tool.HostShouldShowAngle)
                {
                    nullable = new double?(tool.HostAngle);
                }
            }
            if (!nullable.HasValue)
            {
                str = string.Format(this.contextStatusBarFormat, new object[] { str3, str2, str4, str2, str6, str5, str7, str5, str8, str9.ToLower() });
                goto Label_030A;
            }
            NumberFormatInfo info2 = (NumberFormatInfo) provider.Clone();
            info2.NumberDecimalDigits = 2;
            while (nullable.Value > 180.0)
            {
                nullable4 = nullable;
                num4 = 360.0;
                nullable = nullable4.HasValue ? new double?(nullable4.GetValueOrDefault() - num4) : ((double?) (nullable5 = null));
            }
        Label_0235:
            nullable4 = nullable;
            num4 = -180.0;
            if ((nullable4.GetValueOrDefault() < num4) ? nullable4.HasValue : false)
            {
                nullable4 = nullable;
                num4 = 360.0;
                nullable = nullable4.HasValue ? new double?(nullable4.GetValueOrDefault() + num4) : ((double?) (nullable5 = null));
                goto Label_0235;
            }
            str = string.Format(this.contextStatusBarWithAngleFormat, new object[] { str3, str2, str4, str2, str6, str5, str7, str5, str8, str9.ToLower(), nullable.Value.ToString("N", info2) });
        Label_030A:
            this.latestSelectionInfoText = str;
            this.latestSelectionInfoImage = PdnResources.GetImageResource("Icons.SelectionIcon.png");
            this.SetStatus(str, PdnResources.GetImageResource("Icons.SelectionIcon.png"));
        }

        public void UpdateStatusBarToToolHelpText()
        {
            this.UpdateStatusBarToToolHelpText(this.activeTool);
        }

        public void UpdateStatusBarToToolHelpText(PaintDotNet.Tools.Tool tool)
        {
            if (tool == null)
            {
                this.SetStatus(string.Empty, null);
            }
            else
            {
                string name = tool.Name;
                string helpText = tool.HelpText;
                string newStatusText = string.Format(PdnResources.GetString("StatusBar.Context.Help.Text.Format"), name, helpText);
                this.SetStatus(newStatusText, PdnResources.GetImageResource("Icons.MenuHelpHelpTopicsIcon.png"));
            }
        }

        private void UpdateTabbedThumbnail()
        {
            this.VerifyThreadAccess();
            if (!base.IsDisposed && OS.IsWin7OrLater)
            {
                if (!base.IsHandleCreated)
                {
                    this.TabbedThumbnail = null;
                }
                else if (!AppSettings.Instance.UI.ShowTaskbarPreviews.Value)
                {
                    this.TabbedThumbnail = null;
                }
                else
                {
                    Form form = base.FindForm();
                    if ((form != null) && form.IsHandleCreated)
                    {
                        if (this.TabbedThumbnail == null)
                        {
                            this.TabbedThumbnail = new Microsoft.WindowsAPICodePack.Taskbar.TabbedThumbnail(form.Handle, this);
                            this.TabbedThumbnail.SetWindowIcon(PdnInfo.AppIcon.CloneT<Icon>());
                        }
                        this.TabbedThumbnail.Title = this.GetFileFriendlyName();
                        this.TabbedThumbnail.Tooltip = this.GetWindowTitle();
                        this.TabbedThumbnail.InvalidatePreview();
                    }
                }
            }
        }

        private DialogResult WarnAboutFlattening()
        {
            Icon icon = PdnResources.GetImageResource("Icons.MenuFileSaveIcon.png").Reference.ToIcon();
            string str = PdnResources.GetString("WarnAboutFlattening.Title");
            string str2 = PdnResources.GetString("WarnAboutFlattening.IntroText");
            Image image = null;
            TaskButton button = new TaskButton(PdnResources.GetImageResource("Icons.MenuImageFlattenIcon.png").Reference, PdnResources.GetString("WarnAboutFlattening.FlattenTB.ActionText"), PdnResources.GetString("WarnAboutFlattening.FlattenTB.ExplanationText"));
            TaskButton button2 = new TaskButton(PdnResources.GetImageResource("Icons.CancelIcon.png").Reference, PdnResources.GetString("WarnAboutFlattening.CancelTB.ActionText"), PdnResources.GetString("WarnAboutFlattening.CancelTB.ExplanationText"));
            TaskDialog dialog2 = new TaskDialog {
                Icon = icon,
                Title = str,
                TaskImage = image,
                IntroText = str2
            };
            dialog2.TaskButtons = new TaskButton[] { button, button2 };
            dialog2.AcceptButton = button;
            dialog2.CancelButton = button2;
            dialog2.PixelWidth96Dpi = (TaskDialog.DefaultPixelWidth96Dpi * 5) / 4;
            TaskDialog dialog = dialog2;
            if (dialog.Show(this.AppWorkspace) == button)
            {
                return DialogResult.Yes;
            }
            return DialogResult.No;
        }

        public void ZoomToRectangle(RectInt32 selectionBounds)
        {
            this.ZoomToRectangle(selectionBounds.ToGdipRectangle());
        }

        public void ZoomToRectangle(Rectangle selectionBounds)
        {
            PointDouble center = selectionBounds.ToRectDouble().Center;
            RectInt32 clientRectangleMin = base.ClientRectangleMin;
            ScaleFactor factor = ScaleFactor.Min(clientRectangleMin.Width, selectionBounds.Width + 2, clientRectangleMin.Height, selectionBounds.Height + 2, ScaleFactor.MinValue);
            this.ZoomBasis = PaintDotNet.ZoomBasis.ScaleFactor;
            base.ScaleFactor = factor;
            SizeDouble size = base.CanvasView.ViewportCanvasBounds.Size;
            PointDouble viewportCanvasOffset = base.CanvasView.ViewportCanvasOffset;
            PointDouble num5 = new PointDouble(center.X - (size.Width / 2.0), center.Y - (size.Height / 2.0));
            base.CanvasView.ViewportCanvasOffset = num5;
        }

        public void ZoomToSelection()
        {
            if (!this.Selection.IsEmpty)
            {
                GeometryList cachedGeometryList = this.Selection.GetCachedGeometryList();
                this.ZoomToRectangle(cachedGeometryList.Bounds.Int32Bound);
            }
        }

        protected override void ZoomToWithCentering(ScaleFactor newScaleFactor, Func<PointDouble> canvasAnchorPtFn)
        {
            if (this.ZoomBasis == PaintDotNet.ZoomBasis.FitToWindow)
            {
                this.ZoomBasis = PaintDotNet.ZoomBasis.ScaleFactor;
                base.ZoomToWithCentering(newScaleFactor, canvasAnchorPtFn);
            }
            else
            {
                base.ZoomToWithCentering(newScaleFactor, canvasAnchorPtFn);
            }
        }

        public Layer ActiveLayer
        {
            get => 
                this.activeLayer;
            set
            {
                bool deactivateOnLayerChange;
                this.OnLayerChanging();
                if (this.Tool != null)
                {
                    deactivateOnLayerChange = this.Tool.DeactivateOnLayerChange;
                }
                else
                {
                    deactivateOnLayerChange = false;
                }
                if (deactivateOnLayerChange)
                {
                    this.PushNullTool();
                    this.IsToolPulseEnabled = false;
                }
                try
                {
                    if (base.Document != null)
                    {
                        if ((value != null) && !base.Document.Layers.Contains(value))
                        {
                            ExceptionUtil.ThrowInvalidOperationException("ActiveLayer was changed to a layer that is not contained within the Document");
                        }
                    }
                    else if (value != null)
                    {
                        ExceptionUtil.ThrowInvalidOperationException("ActiveLayer was set to non-null while Document was null");
                    }
                    this.activeLayer = value;
                }
                finally
                {
                    if (deactivateOnLayerChange)
                    {
                        this.PopNullTool();
                        this.IsToolPulseEnabled = true;
                    }
                }
                this.OnLayerChanged();
            }
        }

        public int ActiveLayerIndex
        {
            get => 
                base.Document.Layers.IndexOf(this.ActiveLayer);
            set
            {
                this.ActiveLayer = (Layer) base.Document.Layers[value];
            }
        }

        public PaintDotNet.Controls.AppWorkspace AppWorkspace
        {
            get => 
                this.appWorkspace;
            set
            {
                if (value != this.appWorkspace)
                {
                    if (this.appWorkspace != null)
                    {
                        ExceptionUtil.ThrowInvalidOperationException("Once a DocumentWorkspace is assigned to an AppWorkspace, it may not be reassigned");
                    }
                    this.appWorkspace = value;
                    SelectionCanvasLayer.SetSelectionRenderingQuality(base.CanvasView, this.appWorkspace.ToolSettings.Selection.RenderingQuality.Value);
                    this.ToolSettings.Selection.RenderingQuality.ValueChangedT += new ValueChangedEventHandler<SelectionRenderingQuality>(this.OnToolSettingsSelectionRenderingQualityChanged);
                }
            }
        }

        public IDispatcher BackgroundThread =>
            this.appWorkspace.BackgroundThread;

        public IDispatcher Dispatcher =>
            this.dispatcher;

        public bool EnableSelectionOutline
        {
            get => 
                base.DocumentCanvas.SelectionCanvasLayer.IsOutlineEnabled;
            set
            {
                if ((base.DocumentCanvas != null) && (base.DocumentCanvas.SelectionCanvasLayer != null))
                {
                    base.DocumentCanvas.SelectionCanvasLayer.IsOutlineEnabled = value;
                }
            }
        }

        public bool EnableSelectionTinting
        {
            get => 
                base.DocumentCanvas.SelectionCanvasLayer.IsInteriorFilled;
            set
            {
                if ((base.DocumentCanvas != null) && (base.DocumentCanvas.SelectionCanvasLayer != null))
                {
                    base.DocumentCanvas.SelectionCanvasLayer.IsInteriorFilled = value;
                }
            }
        }

        public string FilePath =>
            this.filePath;

        public PaintDotNet.FileType FileType =>
            this.fileType;

        public HistoryStack History =>
            this.history;

        public bool IsToolPulseEnabled
        {
            get => 
                this.isToolPulseEnabled;
            set
            {
                this.VerifyAccess<DocumentWorkspace>();
                if (value != this.isToolPulseEnabled)
                {
                    this.isToolPulseEnabled = value;
                    this.ReevaluateToolPulseTimerEnabled();
                }
            }
        }

        public bool IsZoomChanging =>
            (this.zoomChangesCount > 0);

        public DateTime LastSaveTime =>
            this.lastSaveTime;

        IWin32Window IHistoryWorkspace.Window =>
            this;

        public System.Type PreviousActiveToolType =>
            this.previousActiveToolType;

        public PaintDotNet.SaveConfigToken SaveConfigToken
        {
            get
            {
                if (this.saveConfigToken == null)
                {
                    return null;
                }
                return (PaintDotNet.SaveConfigToken) this.saveConfigToken.Clone();
            }
        }

        public PaintDotNet.Selection Selection =>
            this.selection;

        public ImageResource StatusIcon =>
            this.statusIcon;

        public string StatusText =>
            this.statusText;

        public Microsoft.WindowsAPICodePack.Taskbar.TabbedThumbnail TabbedThumbnail
        {
            get
            {
                this.VerifyThreadAccess();
                return this.tabbedThumbnail;
            }
            private set
            {
                this.VerifyThreadAccess();
                if (value != this.tabbedThumbnail)
                {
                    Microsoft.WindowsAPICodePack.Taskbar.TabbedThumbnail tabbedThumbnail = this.tabbedThumbnail;
                    this.tabbedThumbnail = value;
                    this.OnTabbedThumbnailChanged(tabbedThumbnail, value);
                }
            }
        }

        public PaintDotNet.Threading.Tasks.TaskManager TaskManager =>
            this.taskManager;

        public PaintDotNet.Tools.Tool Tool =>
            this.activeTool;

        public static ToolInfo[] ToolInfos =>
            ((ToolInfo[]) toolInfos.Clone());

        public static System.Type[] Tools =>
            ((System.Type[]) tools.Clone());

        public AppSettings.ToolsSection ToolSettings =>
            this.appWorkspace.ToolSettings;

        public PaintDotNet.ZoomBasis ZoomBasis
        {
            get
            {
                ScaleBasis scaleBasis = base.CanvasView.ScaleBasis;
                if (scaleBasis != ScaleBasis.Ratio)
                {
                    if (scaleBasis != ScaleBasis.FitToViewport)
                    {
                        throw new PaintDotNet.InternalErrorException();
                    }
                    return PaintDotNet.ZoomBasis.FitToWindow;
                }
                return PaintDotNet.ZoomBasis.ScaleFactor;
            }
            set
            {
                if (this.ZoomBasis == value)
                {
                    return;
                }
                PointDouble documentCenterPoint = base.DocumentCenterPoint;
                this.OnZoomBasisChanging();
                if (value != PaintDotNet.ZoomBasis.FitToWindow)
                {
                    if (value != PaintDotNet.ZoomBasis.ScaleFactor)
                    {
                        throw ExceptionUtil.InvalidEnumArgumentException<PaintDotNet.ZoomBasis>(value, "value");
                    }
                }
                else
                {
                    base.CanvasView.ScaleBasis = ScaleBasis.FitToViewport;
                    goto Label_0047;
                }
                base.CanvasView.ScaleBasis = ScaleBasis.Ratio;
            Label_0047:
                base.DocumentCenterPoint = documentCenterPoint;
                this.OnZoomBasisChanged();
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly DocumentWorkspace.<>c <>9 = new DocumentWorkspace.<>c();
            public static Func<FileType, bool> <>9__235_0;
            public static Func<DirectoryNotFoundException, string> <>9__242_10;
            public static Func<PathTooLongException, string> <>9__242_11;
            public static Func<IOException, string> <>9__242_12;
            public static Func<SerializationException, string> <>9__242_13;
            public static Func<OutOfMemoryException, string> <>9__242_14;
            public static Func<Exception, string> <>9__242_15;
            public static Func<UnauthorizedAccessException, string> <>9__242_7;
            public static Func<SecurityException, string> <>9__242_8;
            public static Func<FileNotFoundException, string> <>9__242_9;

            internal bool <DoSaveAsDialog>b__235_0(FileType ft) => 
                ft.SupportsSaving;

            internal string <LoadDocument>b__242_10(DirectoryNotFoundException ex) => 
                "LoadImage.Error.DirectoryNotFoundException";

            internal string <LoadDocument>b__242_11(PathTooLongException ex) => 
                "LoadImage.Error.PathTooLongException";

            internal string <LoadDocument>b__242_12(IOException ex) => 
                "LoadImage.Error.IOException";

            internal string <LoadDocument>b__242_13(SerializationException ex) => 
                "LoadImage.Error.SerializationException";

            internal string <LoadDocument>b__242_14(OutOfMemoryException ex) => 
                "LoadImage.Error.OutOfMemoryException";

            internal string <LoadDocument>b__242_15(Exception ex) => 
                "LoadImage.Error.Exception";

            internal string <LoadDocument>b__242_7(UnauthorizedAccessException ex) => 
                "LoadImage.Error.UnauthorizedAccessException";

            internal string <LoadDocument>b__242_8(SecurityException ex) => 
                "LoadImage.Error.SecurityException";

            internal string <LoadDocument>b__242_9(FileNotFoundException ex) => 
                "LoadImage.Error.FileNotFoundException";
        }

    }
}

