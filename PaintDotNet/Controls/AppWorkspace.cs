namespace PaintDotNet.Controls
{
    using Microsoft.WindowsAPICodePack.Taskbar;
    using PaintDotNet;
    using PaintDotNet.Actions;
    using PaintDotNet.AppModel;
    using PaintDotNet.Collections;
    using PaintDotNet.Data;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Dialogs;
    using PaintDotNet.Drawing;
    using PaintDotNet.Functional;
    using PaintDotNet.HistoryFunctions;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.Settings;
    using PaintDotNet.Settings.App;
    using PaintDotNet.Snap;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Threading;
    using PaintDotNet.Threading.Tasks;
    using PaintDotNet.Tools;
    using PaintDotNet.VisualStyling;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;

    internal class AppWorkspace : UserControl, IDispatcherObject, ISnapObstacleHost, IThreadAffinitizedObject, IGlassyControl, IServiceProvider
    {
        private DocumentWorkspace activeDocumentWorkspace;
        private bool addedToSnapManager;
        private ThreadDispatcher backgroundThread;
        private ColorsForm colorsForm;
        private readonly string cursorInfoStatusBarFormat = PdnResources.GetString("StatusBar.CursorInfo.Format.2");
        private System.Type defaultToolTypeChoice;
        private IDispatcher dispatcher;
        private List<DocumentWorkspace> documentWorkspaces = new List<DocumentWorkspace>();
        private IMessageFilter glassWndProcFilter;
        private bool globalRulersChoice;
        private System.Type globalToolTypeChoice;
        private HistoryForm historyForm;
        private int ignoreUpdateSnapObstacle;
        private readonly string imageInfoStatusBarFormat = PdnResources.GetString("StatusBar.Size.Format.2");
        private DocumentWorkspace initialWorkspace;
        private LayerForm layerForm;
        private int lockActiveDocumentWorkspaceCount;
        private ToolsForm mainToolBarForm;
        private int mtid;
        private SnapObstacleController snapObstacle;
        private PdnStatusBar statusBar;
        private int suspendThumbnailUpdates;
        private PaintDotNet.Threading.Tasks.TaskManager taskManager;
        private PdnToolBar toolBar;
        private PaintDotNet.Settings.App.ToolSettings toolSettings;
        private WorkspaceWidgets widgets;
        private Panel workspacePanel;

        [field: CompilerGenerated]
        public event EventHandler ActiveDocumentWorkspaceChanged;

        [field: CompilerGenerated]
        public event EventHandler ActiveDocumentWorkspaceChanging;

        [field: CompilerGenerated]
        public event CmdKeysEventHandler ProcessCmdKeyEvent;

        [field: CompilerGenerated]
        public event EventHandler RulersEnabledChanged;

        [field: CompilerGenerated]
        public event EventHandler StatusChanged;

        [field: CompilerGenerated]
        public event EventHandler UnitsChanged;

        public AppWorkspace()
        {
            this.dispatcher = new ControlDispatcher(this);
            this.mtid = Thread.CurrentThread.ManagedThreadId;
            this.backgroundThread = new ThreadDispatcher();
            this.taskManager = new PaintDotNet.Threading.Tasks.TaskManager();
            base.SuspendLayout();
            this.toolSettings = new PaintDotNet.Settings.App.ToolSettings(new MemoryStorageHandler(false));
            this.widgets = new WorkspaceWidgets(this);
            this.InitializeComponent();
            this.SetThemedBackColor();
            this.InitializeFloatingForms();
            this.Widgets.ToolsForm = this.mainToolBarForm;
            this.Widgets.LayersForm = this.layerForm;
            this.Widgets.HistoryForm = this.historyForm;
            this.Widgets.ColorsForm = this.colorsForm;
            this.ToolBar.BindAuxMenuToAppWorkspace();
            this.Widgets.CommonActionsStrip = this.ToolBar.CommonActionsStrip;
            this.Widgets.ToolConfigStrip = this.ToolBar.ToolConfigStrip;
            this.Widgets.StatusBarProgress = this.statusBar;
            this.Widgets.DocumentStrip = this.ToolBar.DocumentStrip;
            this.Widgets.DocumentStrip.ItemMoved += new ImageStripItemMovedEventHandler(this.OnDocumentStripItemMoved);
            this.ToolBar.SuspendLayout();
            this.ToolBar.CommonActionsStrip.SuspendLayout();
            this.ToolBar.ToolConfigStrip.SuspendLayout();
            this.ToolBar.ToolChooserStrip.SuspendLayout();
            this.ToolBar.DocumentStrip.SuspendLayout();
            this.statusBar.SuspendLayout();
            this.mainToolBarForm.ToolsControl.SetTools(DocumentWorkspace.ToolInfos);
            this.mainToolBarForm.ToolsControl.ToolClicked += new ToolClickedEventHandler(this.OnMainToolBarToolClicked);
            this.ToolBar.ToolChooserStrip.SetTools(DocumentWorkspace.ToolInfos);
            this.ToolBar.ToolChooserStrip.ToolClicked += new ToolClickedEventHandler(this.OnMainToolBarToolClicked);
            this.LoadSettings();
            AppSettings.Instance.Workspace.ShowPixelGrid.ValueChangedT += new ValueChangedEventHandler<bool>(this.OnShowPixelGridValueChangedT);
            AppSettings.Instance.Workspace.ShowRulers.ValueChangedT += new ValueChangedEventHandler<bool>(this.OnShowRulersValueChangedT);
            AppSettings.Instance.Workspace.MeasurementUnit.ValueChangedT += new ValueChangedEventHandler<MeasurementUnit>(this.OnMeasurementUnitValueChangedT);
            this.ToolSettings.PrimaryColor.ValueChangedT += new ValueChangedEventHandler<ColorBgra32>(this.PrimaryColorChangedHandler);
            this.Widgets.ColorsForm.UserPrimaryColor = this.ToolSettings.PrimaryColor.Value;
            this.ToolSettings.SecondaryColor.ValueChangedT += new ValueChangedEventHandler<ColorBgra32>(this.SecondaryColorChangedHandler);
            this.Widgets.ColorsForm.UserSecondaryColor = this.ToolSettings.SecondaryColor.Value;
            this.Widgets.ColorsForm.WhichUserColor = WhichUserColor.Primary;
            this.ToolBar.DocumentStrip.RelinquishFocus += new EventHandler(this.RelinquishFocusHandler);
            this.ToolBar.ToolConfigStrip.RelinquishFocus += new EventHandler(this.RelinquishFocusHandler2);
            this.ToolBar.ToolConfigStrip.CommitButtonClicked += new EventHandler(this.OnToolConfigStripCommitButtonClicked);
            this.ToolBar.CommonActionsStrip.RelinquishFocus += new EventHandler(this.OnToolStripRelinquishFocus);
            this.ToolBar.CommonActionsStrip.MouseWheel += new MouseEventHandler(this.OnToolStripMouseWheel);
            this.ToolBar.CommonActionsStrip.ButtonClick += new ValueEventHandler<CommonAction>(this.OnCommonActionsStripButtonClick);
            this.ToolBar.CommonActionsStrip.DrawGridChanged += new EventHandler(this.OnCommonActionsStripDrawGridChanged);
            this.ToolBar.CommonActionsStrip.RulersEnabledChanged += new EventHandler(this.OnCommonActionsStripRulersEnabledChanged);
            this.statusBar.ZoomBasisChanged += new EventHandler(this.OnStatusBarZoomBasisChanged);
            this.statusBar.ZoomScaleChanged += new EventHandler(this.OnStatusBarZoomScaleChanged);
            this.statusBar.ZoomIn += new EventHandler(this.OnStatusBarZoomIn);
            this.statusBar.ZoomOut += new EventHandler(this.OnStatusBarZoomOut);
            this.statusBar.UnitsChanged += new EventHandler(this.OnStatusBarUnitsChanged);
            this.ToolBar.ToolConfigStrip.RelinquishFocus += new EventHandler(this.OnToolStripRelinquishFocus);
            this.ToolBar.ToolConfigStrip.MouseWheel += new MouseEventHandler(this.OnToolStripMouseWheel);
            this.ToolBar.DocumentStrip.RelinquishFocus += new EventHandler(this.OnToolStripRelinquishFocus);
            this.ToolBar.DocumentStrip.DocumentClicked += new ValueEventHandler<Tuple<DocumentWorkspace, DocumentClickAction>>(this.OnDocumentStripDocumentTabClicked);
            this.ToolBar.DocumentStrip.DocumentListChanged += new EventHandler(this.OnDocumentStripDocumentListChanged);
            this.GlobalToolTypeChoice = this.defaultToolTypeChoice;
            this.ToolBar.ToolConfigStrip.ToolBarConfigItems = ToolBarConfigItems.None | ToolBarConfigItems.SelectionRenderingQuality;
            foreach (Setting setting in this.toolSettings)
            {
                setting.RaiseValueChangedEvent();
            }
            PdnToolStripRenderer renderer = new PdnToolStripRenderer();
            this.statusBar.Renderer = renderer;
            this.statusBar.ResumeLayout(false);
            this.ToolBar.CommonActionsStrip.ResumeLayout(false);
            this.ToolBar.ToolConfigStrip.ResumeLayout(false);
            this.ToolBar.ToolChooserStrip.ResumeLayout(false);
            this.ToolBar.DocumentStrip.ResumeLayout(false);
            this.ToolBar.ResumeLayout(false);
            base.ResumeLayout(false);
            this.ToolSettings.ActiveToolName.ValueChangedT += new ValueChangedEventHandler<string>(this.OnActiveToolNameValueChangedT);
        }

        public DocumentWorkspace AddNewDocumentWorkspace()
        {
            DocumentWorkspace workspace2;
            this.VerifyThreadAccess();
            bool flag = false;
            try
            {
                if (this.initialWorkspace != null)
                {
                    bool flag2;
                    if (this.initialWorkspace.Document == null)
                    {
                        flag2 = true;
                    }
                    else if ((!this.initialWorkspace.Document.Dirty && (this.initialWorkspace.History.UndoStack.Count == 1)) && ((this.initialWorkspace.History.RedoStack.Count == 0) && (this.initialWorkspace.History.UndoStack[0] is NullHistoryMemento)))
                    {
                        flag2 = true;
                    }
                    else if ((!this.initialWorkspace.Document.Dirty && (this.initialWorkspace.History.UndoStack.Count == 0)) && (this.initialWorkspace.History.RedoStack.Count == 0))
                    {
                        flag2 = true;
                    }
                    else
                    {
                        flag2 = false;
                    }
                    if (flag2)
                    {
                        this.globalToolTypeChoice = this.initialWorkspace.GetToolType();
                        flag = true;
                        UIUtil.SuspendControlPainting(this);
                        this.RemoveDocumentWorkspace(this.initialWorkspace);
                        this.initialWorkspace = null;
                    }
                }
                DocumentWorkspace item = new DocumentWorkspace {
                    AppWorkspace = this
                };
                item.PushCacheStandby();
                this.documentWorkspaces.Add(item);
                this.ToolBar.DocumentStrip.AddDocumentWorkspace(item);
                item.RequestActivate += new CancelEventHandler(this.OnDocumentWorkspaceRequestActivate);
                item.RequestClose += new CancelEventHandler(this.OnDocumentWorkspaceRequestClose);
                item.TabbedThumbnailChanged += new ValueChangedEventHandler<TabbedThumbnail>(this.OnDocumentWorkspaceTabbedThumbnailChanged);
                if (item.TabbedThumbnail != null)
                {
                    ValueChangedEventArgs<TabbedThumbnail> e = ValueChangedEventArgs.Get<TabbedThumbnail>(null, item.TabbedThumbnail);
                    this.OnDocumentWorkspaceTabbedThumbnailChanged(this, e);
                    e.Return();
                }
                workspace2 = item;
            }
            finally
            {
                if (flag)
                {
                    UIUtil.ResumeControlPainting(this);
                    base.Invalidate(true);
                }
            }
            return workspace2;
        }

        public bool CheckAccess() => 
            (Thread.CurrentThread.ManagedThreadId == this.mtid);

        private void CoordinatesToStrings(int x, int y, out string xString, out string yString, out string unitsString)
        {
            this.ActiveDocumentWorkspace.Document.CoordinatesToStrings(this.Units, x, y, out xString, out yString, out unitsString);
        }

        public bool CreateBlankDocumentInNewWorkspace(SizeInt32 size, MeasurementUnit dpuUnit, double dpu, bool isInitial)
        {
            if (!this.CanSetActiveWorkspace)
            {
                return false;
            }
            DocumentWorkspace activeDocumentWorkspace = this.ActiveDocumentWorkspace;
            if (activeDocumentWorkspace != null)
            {
                activeDocumentWorkspace.SuspendRefresh();
            }
            try
            {
                BitmapLayer layer;
                Document document = new Document(size.Width, size.Height) {
                    DpuUnit = dpuUnit,
                    DpuX = dpu,
                    DpuY = dpu
                };
                try
                {
                    using (new WaitCursorChanger(this))
                    {
                        layer = Layer.CreateBackgroundLayer(size.Width, size.Height);
                    }
                }
                catch (OutOfMemoryException exception)
                {
                    ExceptionDialog.ShowErrorDialog(this, PdnResources.GetString("NewImageAction.Error.OutOfMemory"), exception);
                    return false;
                }
                using (new WaitCursorChanger(this))
                {
                    bool flag2 = false;
                    if ((this.ActiveDocumentWorkspace != null) && this.ActiveDocumentWorkspace.Focused)
                    {
                        flag2 = true;
                    }
                    document.Layers.Add(layer);
                    DocumentWorkspace lockMe = this.AddNewDocumentWorkspace();
                    this.Widgets.DocumentStrip.LockDocumentWorkspaceDirtyValue(lockMe, false);
                    lockMe.SuspendRefresh();
                    try
                    {
                        lockMe.Document = document;
                    }
                    catch (OutOfMemoryException exception2)
                    {
                        ExceptionDialog.ShowErrorDialog(this, PdnResources.GetString("NewImageAction.Error.OutOfMemory"), exception2);
                        this.RemoveDocumentWorkspace(lockMe);
                        document.Dispose();
                        return false;
                    }
                    lockMe.ActiveLayer = (Layer) lockMe.Document.Layers[0];
                    this.ActiveDocumentWorkspace = lockMe;
                    lockMe.SetDocumentSaveOptions(null, null, null);
                    lockMe.History.ClearAll();
                    lockMe.History.PushNewMemento(new NullHistoryMemento(PdnResources.GetString("NewImageAction.Name"), this.FileNewIcon));
                    lockMe.Document.Dirty = false;
                    lockMe.ResumeRefresh();
                    if (isInitial)
                    {
                        this.initialWorkspace = lockMe;
                    }
                    if (flag2)
                    {
                        this.ActiveDocumentWorkspace.Focus();
                    }
                    this.Widgets.DocumentStrip.UnlockDocumentWorkspaceDirtyValue(lockMe);
                }
            }
            finally
            {
                if (activeDocumentWorkspace != null)
                {
                    activeDocumentWorkspace.ResumeRefresh();
                }
            }
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.ToolSettings.ActiveToolName.ValueChangedT -= new ValueChangedEventHandler<string>(this.OnActiveToolNameValueChangedT);
                if (this.taskManager != null)
                {
                    this.taskManager.Dispose();
                    this.taskManager = null;
                }
                if (this.backgroundThread != null)
                {
                    this.backgroundThread.Dispose();
                    this.backgroundThread = null;
                }
            }
            base.Dispose(disposing);
        }

        private void DocumenKeyUp(object sender, KeyEventArgs e)
        {
            if ((this.ActiveDocumentWorkspace != null) && (this.ActiveDocumentWorkspace.Tool != null))
            {
                this.ActiveDocumentWorkspace.Tool.PerformKeyUp(e);
            }
        }

        private void DocumentClick(object sender, EventArgs e)
        {
            if ((this.ActiveDocumentWorkspace != null) && (this.ActiveDocumentWorkspace.Tool != null))
            {
                this.ActiveDocumentWorkspace.Tool.PerformClick();
            }
        }

        private void DocumentEnter(object sender, EventArgs e)
        {
            if ((this.ActiveDocumentWorkspace != null) && (this.ActiveDocumentWorkspace.Tool != null))
            {
                this.ActiveDocumentWorkspace.Tool.PerformEnter();
            }
        }

        private void DocumentKeyDown(object sender, KeyEventArgs e)
        {
            if ((this.ActiveDocumentWorkspace != null) && (this.ActiveDocumentWorkspace.Tool != null))
            {
                this.ActiveDocumentWorkspace.Tool.PerformKeyDown(e);
            }
        }

        private void DocumentKeyPress(object sender, KeyPressEventArgs e)
        {
            if ((this.ActiveDocumentWorkspace != null) && (this.ActiveDocumentWorkspace.Tool != null))
            {
                this.ActiveDocumentWorkspace.Tool.PerformKeyPress(e);
            }
        }

        private void DocumentLeave(object sender, EventArgs e)
        {
            if ((this.ActiveDocumentWorkspace != null) && (this.ActiveDocumentWorkspace.Tool != null))
            {
                this.ActiveDocumentWorkspace.Tool.PerformLeave();
            }
        }

        private void DocumentMouseDownHandler(object sender, MouseEventArgsF e)
        {
            if ((this.ActiveDocumentWorkspace != null) && (this.ActiveDocumentWorkspace.Tool != null))
            {
                this.ActiveDocumentWorkspace.Tool.PerformMouseDown(e);
            }
        }

        private void DocumentMouseEnterHandler(object sender, EventArgs e)
        {
            if ((this.ActiveDocumentWorkspace != null) && (this.ActiveDocumentWorkspace.Tool != null))
            {
                this.ActiveDocumentWorkspace.Tool.PerformMouseEnter();
            }
        }

        private void DocumentMouseLeaveHandler(object sender, EventArgs e)
        {
            if ((this.ActiveDocumentWorkspace != null) && (this.ActiveDocumentWorkspace.Tool != null))
            {
                this.ActiveDocumentWorkspace.Tool.PerformMouseLeave();
            }
        }

        private void DocumentMouseMoveHandler(object sender, MouseEventArgsF e)
        {
            if ((this.ActiveDocumentWorkspace != null) && (this.ActiveDocumentWorkspace.Tool != null))
            {
                this.ActiveDocumentWorkspace.Tool.PerformMouseMove(e);
            }
            this.UpdateCursorInfoInStatusBar(e.X, e.Y);
        }

        private void DocumentMouseUpHandler(object sender, MouseEventArgsF e)
        {
            if ((this.ActiveDocumentWorkspace != null) && (this.ActiveDocumentWorkspace.Tool != null))
            {
                this.ActiveDocumentWorkspace.Tool.PerformMouseUp(e);
            }
        }

        [IteratorStateMachine(typeof(<EnumerateOwnedFormsTree>d__113))]
        private static IEnumerable<Form> EnumerateOwnedFormsTree(Form form)
        {
            this.<>7__wrap1 = form.OwnedForms;
            this.<>7__wrap2 = 0;
            while (this.<>7__wrap2 < this.<>7__wrap1.Length)
            {
                this.<ownedForm>5__1 = this.<>7__wrap1[this.<>7__wrap2];
                yield return this.<ownedForm>5__1;
                using (this.<>7__wrap3 = EnumerateOwnedFormsTree(this.<ownedForm>5__1).GetEnumerator())
                {
                    while (this.<>7__wrap3.MoveNext())
                    {
                        Form current = this.<>7__wrap3.Current;
                        yield return current;
                    }
                }
                this.<>7__wrap3 = null;
                this.<ownedForm>5__1 = null;
                this.<>7__wrap2++;
            }
            this.<>7__wrap1 = null;
        }

        public SizeInt32 GetNewDocumentSize()
        {
            PdnBaseForm form = base.FindForm() as PdnBaseForm;
            if ((form != null) && (form.ScreenAspect < 1.0))
            {
                return new SizeInt32(600, 800);
            }
            return new SizeInt32(800, 600);
        }

        public object GetService(System.Type serviceType)
        {
            if (serviceType == typeof(IPluginErrorService))
            {
                return PluginErrorService.Instance;
            }
            return null;
        }

        private void HookToolEvents()
        {
            this.UnhookToolEvents();
            if ((this.ActiveDocumentWorkspace != null) && (this.ActiveDocumentWorkspace.Tool != null))
            {
                this.ActiveDocumentWorkspace.Tool.ToolBarConfigItemsChanged += new EventHandler(this.OnActiveToolToolBarConfigItemsChanged);
                this.ActiveDocumentWorkspace.Tool.IsCommitSupportedChanged += new EventHandler(this.OnActiveToolIsCommitSupportedChanged);
                this.ActiveDocumentWorkspace.Tool.CanCommitChanged += new EventHandler(this.OnActiveToolCanCommitChanged);
            }
        }

        private void InitializeComponent()
        {
            this.toolBar = new PdnToolBar(this);
            this.statusBar = new PdnStatusBar(this);
            this.workspacePanel = new Panel();
            base.SuspendLayout();
            this.toolBar.SuspendLayout();
            this.statusBar.SuspendLayout();
            this.workspacePanel.SuspendLayout();
            this.toolBar.Name = "toolBar";
            this.toolBar.Dock = DockStyle.Top;
            this.statusBar.Name = "statusBar";
            this.statusBar.Dock = DockStyle.None;
            this.statusBar.AutoSize = false;
            this.workspacePanel.Name = "workspacePanel";
            this.workspacePanel.Dock = DockStyle.None;
            base.Controls.Add(this.toolBar);
            base.Controls.Add(this.workspacePanel);
            base.Controls.Add(this.statusBar);
            base.Name = "AppWorkspace";
            this.workspacePanel.ResumeLayout(false);
            this.statusBar.ResumeLayout(false);
            this.toolBar.ResumeLayout(false);
            base.ResumeLayout(false);
        }

        private void InitializeFloatingForms()
        {
            this.mainToolBarForm = new ToolsForm();
            this.mainToolBarForm.RelinquishFocus += new EventHandler(this.RelinquishFocusHandler);
            this.mainToolBarForm.ProcessCmdKeyEvent += new CmdKeysEventHandler(this.OnToolFormProcessCmdKeyEvent);
            this.layerForm = new LayerForm();
            this.layerForm.AppWorkspace = this;
            this.layerForm.NewLayerButtonClick += new EventHandler(this.OnLayerFormNewLayerButtonClicked);
            this.layerForm.DeleteLayerButtonClick += new EventHandler(this.OnLayerFormDeleteLayerButtonClicked);
            this.layerForm.DuplicateLayerButtonClick += new EventHandler(this.OnLayerFormDuplicateLayerButtonClick);
            this.layerForm.MergeLayerDownClick += new EventHandler(this.OnLayerFormMergeLayerDownClick);
            this.layerForm.MoveLayerUpButtonClick += new EventHandler(this.OnLayerFormMoveLayerUpButtonClicked);
            this.layerForm.MoveLayerDownButtonClick += new EventHandler(this.OnLayerFormMoveLayerDownButtonClicked);
            this.layerForm.MoveLayerToTopButtonClick += new EventHandler(this.OnLayerFormMoveLayerToTopButtonClicked);
            this.layerForm.MoveLayerToBottomButtonClick += new EventHandler(this.OnLayerFormMoveLayerToBottomButtonClicked);
            this.layerForm.PropertiesButtonClick += new EventHandler(this.OnLayerFormPropertiesButtonClick);
            this.layerForm.RelinquishFocus += new EventHandler(this.RelinquishFocusHandler);
            this.layerForm.ProcessCmdKeyEvent += new CmdKeysEventHandler(this.OnToolFormProcessCmdKeyEvent);
            this.historyForm = new HistoryForm();
            this.historyForm.RewindButtonClicked += new EventHandler(this.OnHistoryFormRewindButtonClicked);
            this.historyForm.UndoButtonClicked += new EventHandler(this.OnHistoryFormUndoButtonClicked);
            this.historyForm.RedoButtonClicked += new EventHandler(this.OnHistoryFormRedoButtonClicked);
            this.historyForm.FastForwardButtonClicked += new EventHandler(this.OnHistoryFormFastForwardButtonClicked);
            this.historyForm.RelinquishFocus += new EventHandler(this.RelinquishFocusHandler);
            this.historyForm.ProcessCmdKeyEvent += new CmdKeysEventHandler(this.OnToolFormProcessCmdKeyEvent);
            this.colorsForm = new ColorsForm();
            this.colorsForm.WhichUserColor = WhichUserColor.Primary;
            this.colorsForm.UserPrimaryColorChanged += new ColorEventHandler(this.OnColorsFormUserPrimaryColorChanged);
            this.colorsForm.UserSecondaryColorChanged += new ColorEventHandler(this.OnColorsFormUserSecondaryColorChanged);
            this.colorsForm.RelinquishFocus += new EventHandler(this.RelinquishFocusHandler);
            this.colorsForm.ProcessCmdKeyEvent += new CmdKeysEventHandler(this.OnToolFormProcessCmdKeyEvent);
        }

        internal void InvalidateTitle()
        {
            this.ToolBar.InvalidateTitle();
        }

        private void LoadDefaultToolType()
        {
            string defaultToolTypeName = AppSettings.Instance.ToolDefaults.ActiveToolName.Value;
            ToolInfo info = Array.Find<ToolInfo>(DocumentWorkspace.ToolInfos, check => string.Compare(defaultToolTypeName, check.ToolType.Name, StringComparison.InvariantCultureIgnoreCase) == 0);
            if (info == null)
            {
                this.defaultToolTypeChoice = PaintDotNet.Tools.Tool.DefaultToolType;
            }
            else
            {
                this.defaultToolTypeChoice = info.ToolType;
            }
        }

        public void LoadSettings()
        {
            try
            {
                this.LoadDefaultToolType();
                this.GlobalToolTypeChoice = this.defaultToolTypeChoice;
                this.globalRulersChoice = AppSettings.Instance.Workspace.ShowRulers.Value;
                this.DrawGrid = AppSettings.Instance.Workspace.ShowPixelGrid.Value;
                this.ToolSettings.LoadFrom(AppSettings.Instance.ToolDefaults);
                this.statusBar.Units = AppSettings.Instance.Workspace.MeasurementUnit.Value;
            }
            catch (Exception)
            {
            }
            this.ToolBar.ToolConfigStrip.LoadFromSettings(this.ToolSettings);
        }

        public IDisposable LockActiveDocumentWorkspace()
        {
            this.VerifyThreadAccess();
            this.lockActiveDocumentWorkspaceCount++;
            return Disposable.FromAction(delegate {
                this.UnlockActiveDocumentWorkspace();
            }, false);
        }

        protected virtual void OnActiveDocumentWorkspaceChanged()
        {
            this.SuspendUpdateSnapObstacle();
            if (this.ActiveDocumentWorkspace == null)
            {
                this.ToolBar.CommonActionsStrip.SetButtonEnabled(CommonAction.Print, false);
                this.ToolBar.CommonActionsStrip.SetButtonEnabled(CommonAction.Save, false);
                this.UpdateSelectionToolbarButtons();
                this.ToolBar.CommonActionsStrip.SetButtonEnabled(CommonAction.ToggleGrid, false);
                this.ToolBar.CommonActionsStrip.SetButtonEnabled(CommonAction.ToggleRulers, false);
                this.statusBar.MaxDocScaleFactor = ScaleFactor.MaxValue;
                this.statusBar.MinDocScaleFactor = ScaleFactor.MinValue;
                this.statusBar.ScaleFactor = ScaleFactor.OneToOne;
                this.statusBar.ZoomBasis = ZoomBasis.ScaleFactor;
                this.statusBar.Enabled = false;
            }
            else
            {
                this.ActiveDocumentWorkspace.PopCacheStandby();
                this.ActiveDocumentWorkspace.SuspendLayout();
                this.ToolBar.CommonActionsStrip.SetButtonEnabled(CommonAction.Print, true);
                this.ToolBar.CommonActionsStrip.SetButtonEnabled(CommonAction.Save, true);
                this.ToolBar.CommonActionsStrip.SetButtonEnabled(CommonAction.ToggleGrid, true);
                this.ToolBar.CommonActionsStrip.SetButtonEnabled(CommonAction.ToggleRulers, true);
                this.ActiveDocumentWorkspace.Dock = DockStyle.Fill;
                this.ActiveDocumentWorkspace.DrawGrid = this.DrawGrid;
                this.ActiveDocumentWorkspace.RulersEnabled = this.globalRulersChoice;
                this.ActiveDocumentWorkspace.TabIndex = 0;
                this.ActiveDocumentWorkspace.TabStop = false;
                this.ActiveDocumentWorkspace.RulersEnabledChanged += new EventHandler(this.OnDocumentWorkspaceRulersEnabledChanged);
                this.ActiveDocumentWorkspace.DocumentMouseEnter += new EventHandler(this.DocumentMouseEnterHandler);
                this.ActiveDocumentWorkspace.DocumentMouseMove += new EventHandler<MouseEventArgsF>(this.DocumentMouseMoveHandler);
                this.ActiveDocumentWorkspace.DocumentMouseDown += new EventHandler<MouseEventArgsF>(this.DocumentMouseDownHandler);
                this.ActiveDocumentWorkspace.DocumentMouseUp += new EventHandler<MouseEventArgsF>(this.DocumentMouseUpHandler);
                this.ActiveDocumentWorkspace.DocumentMouseLeave += new EventHandler(this.DocumentMouseLeaveHandler);
                this.ActiveDocumentWorkspace.Scroll += new ScrollEventHandler(this.OnDocumentWorkspaceScroll);
                this.ActiveDocumentWorkspace.DrawGridChanged += new EventHandler(this.OnDocumentWorkspaceDrawGridChanged);
                this.ActiveDocumentWorkspace.DocumentClick += new EventHandler(this.DocumentClick);
                this.ActiveDocumentWorkspace.DocumentEnter += new EventHandler(this.DocumentEnter);
                this.ActiveDocumentWorkspace.DocumentLeave += new EventHandler(this.DocumentLeave);
                this.ActiveDocumentWorkspace.DocumentKeyPress += new KeyPressEventHandler(this.DocumentKeyPress);
                this.ActiveDocumentWorkspace.DocumentKeyUp += new KeyEventHandler(this.DocumenKeyUp);
                this.ActiveDocumentWorkspace.DocumentKeyDown += new KeyEventHandler(this.DocumentKeyDown);
                this.ActiveDocumentWorkspace.Visible = true;
                if (!this.workspacePanel.Controls.Contains(this.ActiveDocumentWorkspace))
                {
                    this.ActiveDocumentWorkspace.Dock = DockStyle.Fill;
                    this.workspacePanel.Controls.Add(this.ActiveDocumentWorkspace);
                }
                for (int i = 0; i < this.documentWorkspaces.Count; i++)
                {
                    if (this.documentWorkspaces[i] != this.ActiveDocumentWorkspace)
                    {
                        this.documentWorkspaces[i].Visible = false;
                    }
                }
                this.workspacePanel.Controls.SetChildIndex(this.ActiveDocumentWorkspace, 0);
                this.ActiveDocumentWorkspace.Layout += new LayoutEventHandler(this.OnDocumentWorkspaceLayout);
                this.statusBar.SuspendEvents();
                this.statusBar.MaxDocScaleFactor = this.ActiveDocumentWorkspace.MaxScaleFactor;
                this.statusBar.MinDocScaleFactor = this.ActiveDocumentWorkspace.MinScaleFactor;
                this.statusBar.ScaleFactor = this.ActiveDocumentWorkspace.ScaleFactor;
                this.statusBar.ZoomBasis = this.ActiveDocumentWorkspace.ZoomBasis;
                this.statusBar.Enabled = true;
                this.statusBar.ResumeEvents();
                this.ActiveDocumentWorkspace.AppWorkspace = this;
                this.ActiveDocumentWorkspace.History.Changed += new EventHandler(this.OnHistoryChangedHandler);
                this.ActiveDocumentWorkspace.StatusChanged += new EventHandler(this.OnDocumentWorkspaceStatusChanged);
                this.ActiveDocumentWorkspace.DocumentChanging += new ValueEventHandler<Document>(this.OnDocumentWorkspaceDocumentChanging);
                this.ActiveDocumentWorkspace.DocumentChanged += new EventHandler(this.OnDocumentWorkspaceDocumentChanged);
                this.ActiveDocumentWorkspace.Selection.Changing += new EventHandler(this.SelectedPathChangingHandler);
                this.ActiveDocumentWorkspace.Selection.Changed += new EventHandler<SelectionChangedEventArgs>(this.SelectedPathChangedHandler);
                this.ActiveDocumentWorkspace.ScaleFactorChanged += new EventHandler(this.ZoomChangedHandler);
                this.ActiveDocumentWorkspace.ZoomBasisChanged += new EventHandler(this.OnDocumentWorkspaceZoomBasisChanged);
                this.ActiveDocumentWorkspace.Units = this.statusBar.Units;
                this.historyForm.HistoryControl.HistoryStack = this.ActiveDocumentWorkspace.History;
                this.ActiveDocumentWorkspace.ToolChanging += new EventHandler(this.ToolChangingHandler);
                this.ActiveDocumentWorkspace.ToolChanged += new EventHandler(this.ToolChangedHandler);
                this.HookToolEvents();
                this.Widgets.CommonActionsStrip.RulersEnabled = this.ActiveDocumentWorkspace.RulersEnabled;
                this.ToolBar.DocumentStrip.SelectDocumentWorkspace(this.ActiveDocumentWorkspace);
                this.ActiveDocumentWorkspace.SetToolFromType(this.GlobalToolTypeChoice);
                this.UpdateSelectionToolbarButtons();
                this.UpdateHistoryButtons();
                this.UpdateDocInfoInStatusBar();
                this.ActiveDocumentWorkspace.ResumeLayout();
                this.ActiveDocumentWorkspace.PerformLayout();
                this.ActiveDocumentWorkspace.FirstInputAfterGotFocus += new EventHandler(this.OnActiveDocumentWorkspaceFirstInputAfterGotFocus);
            }
            this.UpdateToolBarCommitButton();
            this.ActiveDocumentWorkspaceChanged.Raise(this);
            this.UpdateStatusBarContextStatus();
            this.ResumeUpdateSnapObstacle();
            this.UpdateSnapObstacle();
        }

        protected virtual void OnActiveDocumentWorkspaceChanging()
        {
            this.SuspendUpdateSnapObstacle();
            this.ActiveDocumentWorkspaceChanging.Raise(this);
            if (this.ActiveDocumentWorkspace != null)
            {
                this.ActiveDocumentWorkspace.FirstInputAfterGotFocus -= new EventHandler(this.OnActiveDocumentWorkspaceFirstInputAfterGotFocus);
                this.ActiveDocumentWorkspace.RulersEnabledChanged -= new EventHandler(this.OnDocumentWorkspaceRulersEnabledChanged);
                this.ActiveDocumentWorkspace.DocumentMouseEnter -= new EventHandler(this.DocumentMouseEnterHandler);
                this.ActiveDocumentWorkspace.DocumentMouseLeave -= new EventHandler(this.DocumentMouseLeaveHandler);
                this.ActiveDocumentWorkspace.DocumentMouseMove -= new EventHandler<MouseEventArgsF>(this.DocumentMouseMoveHandler);
                this.ActiveDocumentWorkspace.DocumentMouseDown -= new EventHandler<MouseEventArgsF>(this.DocumentMouseDownHandler);
                this.ActiveDocumentWorkspace.Scroll -= new ScrollEventHandler(this.OnDocumentWorkspaceScroll);
                this.ActiveDocumentWorkspace.Layout -= new LayoutEventHandler(this.OnDocumentWorkspaceLayout);
                this.ActiveDocumentWorkspace.DrawGridChanged -= new EventHandler(this.OnDocumentWorkspaceDrawGridChanged);
                this.ActiveDocumentWorkspace.DocumentClick -= new EventHandler(this.DocumentClick);
                this.ActiveDocumentWorkspace.DocumentMouseUp -= new EventHandler<MouseEventArgsF>(this.DocumentMouseUpHandler);
                this.ActiveDocumentWorkspace.DocumentKeyPress -= new KeyPressEventHandler(this.DocumentKeyPress);
                this.ActiveDocumentWorkspace.DocumentKeyUp -= new KeyEventHandler(this.DocumenKeyUp);
                this.ActiveDocumentWorkspace.DocumentKeyDown -= new KeyEventHandler(this.DocumentKeyDown);
                this.ActiveDocumentWorkspace.History.Changed -= new EventHandler(this.OnHistoryChangedHandler);
                this.ActiveDocumentWorkspace.StatusChanged -= new EventHandler(this.OnDocumentWorkspaceStatusChanged);
                this.ActiveDocumentWorkspace.DocumentChanging -= new ValueEventHandler<Document>(this.OnDocumentWorkspaceDocumentChanging);
                this.ActiveDocumentWorkspace.DocumentChanged -= new EventHandler(this.OnDocumentWorkspaceDocumentChanged);
                this.ActiveDocumentWorkspace.Selection.Changing -= new EventHandler(this.SelectedPathChangingHandler);
                this.ActiveDocumentWorkspace.Selection.Changed -= new EventHandler<SelectionChangedEventArgs>(this.SelectedPathChangedHandler);
                this.ActiveDocumentWorkspace.ScaleFactorChanged -= new EventHandler(this.ZoomChangedHandler);
                this.ActiveDocumentWorkspace.ZoomBasisChanged -= new EventHandler(this.OnDocumentWorkspaceZoomBasisChanged);
                this.ActiveDocumentWorkspace.Visible = false;
                this.historyForm.HistoryControl.HistoryStack = null;
                this.UnhookToolEvents();
                this.ActiveDocumentWorkspace.ToolChanging -= new EventHandler(this.ToolChangingHandler);
                this.ActiveDocumentWorkspace.ToolChanged -= new EventHandler(this.ToolChangedHandler);
                if (this.ActiveDocumentWorkspace.Tool != null)
                {
                    while (this.ActiveDocumentWorkspace.Tool.IsMouseEntered)
                    {
                        this.ActiveDocumentWorkspace.Tool.PerformMouseLeave();
                    }
                }
                if (this.ActiveDocumentWorkspace.GetToolType() != null)
                {
                    this.GlobalToolTypeChoice = this.ActiveDocumentWorkspace.GetToolType();
                }
                this.ActiveDocumentWorkspace.PushCacheStandby();
            }
            this.ResumeUpdateSnapObstacle();
            this.UpdateSnapObstacle();
        }

        private void OnActiveDocumentWorkspaceFirstInputAfterGotFocus(object sender, EventArgs e)
        {
            this.ToolBar.DocumentStrip.EnsureItemFullyVisible(this.ToolBar.DocumentStrip.SelectedDocumentIndex);
        }

        private void OnActiveToolCanCommitChanged(object sender, EventArgs e)
        {
            this.UpdateToolBarCommitButton();
        }

        private void OnActiveToolIsCommitSupportedChanged(object sender, EventArgs e)
        {
            this.UpdateToolBarCommitButton();
        }

        private void OnActiveToolNameValueChangedT(object sender, ValueChangedEventArgs<string> e)
        {
            if (!string.IsNullOrWhiteSpace(e.NewValue))
            {
                string name = this.globalToolTypeChoice?.Name;
                if (!string.Equals(e.NewValue, name, StringComparison.InvariantCultureIgnoreCase))
                {
                    System.Type type = DocumentWorkspace.ToolInfos.FindByName(e.NewValue);
                    if (type != null)
                    {
                        this.GlobalToolTypeChoice = type;
                    }
                }
            }
        }

        private void OnActiveToolToolBarConfigItemsChanged(object sender, EventArgs e)
        {
            this.UpdateToolBarConfigItems();
        }

        private void OnAppWorkspaceShown(object sender, EventArgs e)
        {
            this.UpdateSnapObstacle();
        }

        private void OnColorDisplayUserPrimaryAndSecondaryColorsChanged(object sender, EventArgs e)
        {
            if (this.Widgets.ColorsForm.WhichUserColor == WhichUserColor.Primary)
            {
                this.Widgets.ColorsForm.SetColorControlsRedraw(false);
                this.SecondaryColorChangedHandler(sender, e);
                this.PrimaryColorChangedHandler(sender, e);
                this.Widgets.ColorsForm.SetColorControlsRedraw(true);
                this.Widgets.ColorsForm.WhichUserColor = WhichUserColor.Primary;
            }
            else
            {
                this.Widgets.ColorsForm.SetColorControlsRedraw(false);
                this.PrimaryColorChangedHandler(sender, e);
                this.SecondaryColorChangedHandler(sender, e);
                this.Widgets.ColorsForm.SetColorControlsRedraw(true);
                this.Widgets.ColorsForm.WhichUserColor = WhichUserColor.Secondary;
            }
        }

        private void OnColorsFormUserPrimaryColorChanged(object sender, ColorEventArgs e)
        {
            this.ToolSettings.PrimaryColor.Value = (ColorBgra32) e.Color;
        }

        private void OnColorsFormUserSecondaryColorChanged(object sender, ColorEventArgs e)
        {
            this.ToolSettings.SecondaryColor.Value = (ColorBgra32) e.Color;
        }

        private void OnCommonActionsStripButtonClick(object sender, ValueEventArgs<CommonAction> e)
        {
            CommonAction action = e.Value;
            switch (action)
            {
                case CommonAction.New:
                    this.PerformAction(new NewImageAction());
                    break;

                case CommonAction.Open:
                    this.PerformAction(new OpenFileAction());
                    break;

                case CommonAction.Save:
                    if (this.ActiveDocumentWorkspace != null)
                    {
                        this.ActiveDocumentWorkspace.DoSave();
                    }
                    break;

                case CommonAction.Print:
                    if (this.ActiveDocumentWorkspace != null)
                    {
                        PrintAction action2 = new PrintAction();
                        this.ActiveDocumentWorkspace.PerformAction(action2);
                    }
                    break;

                case CommonAction.Cut:
                    if (this.ActiveDocumentWorkspace != null)
                    {
                        new CutAction().PerformAction(this.ActiveDocumentWorkspace);
                    }
                    break;

                case CommonAction.Copy:
                    if (this.ActiveDocumentWorkspace != null)
                    {
                        new CopyToClipboardAction(this.ActiveDocumentWorkspace).PerformAction();
                    }
                    break;

                case CommonAction.Paste:
                    if (this.ActiveDocumentWorkspace != null)
                    {
                        new PasteAction(this.ActiveDocumentWorkspace).PerformAction();
                    }
                    break;

                case CommonAction.CropToSelection:
                    if (this.ActiveDocumentWorkspace != null)
                    {
                        CropToSelectionAction.PerformAction(this.ActiveDocumentWorkspace);
                    }
                    break;

                case CommonAction.Deselect:
                    if (this.ActiveDocumentWorkspace != null)
                    {
                        this.ActiveDocumentWorkspace.ApplyFunction(new DeselectFunction());
                    }
                    break;

                case CommonAction.Undo:
                    if (this.ActiveDocumentWorkspace != null)
                    {
                        this.ActiveDocumentWorkspace.PerformAction(new HistoryUndoAction());
                    }
                    break;

                case CommonAction.Redo:
                    if (this.ActiveDocumentWorkspace != null)
                    {
                        this.ActiveDocumentWorkspace.PerformAction(new HistoryRedoAction());
                    }
                    break;

                case CommonAction.ToggleRulers:
                    this.RulersEnabled = !this.RulersEnabled;
                    break;

                case CommonAction.ToggleGrid:
                    this.DrawGrid = !this.DrawGrid;
                    break;

                default:
                    throw ExceptionUtil.InvalidEnumArgumentException<CommonAction>(action, "ca");
            }
            if (this.ActiveDocumentWorkspace != null)
            {
                this.ActiveDocumentWorkspace.Focus();
            }
        }

        private void OnCommonActionsStripDrawGridChanged(object sender, EventArgs e)
        {
            this.DrawGrid = ((CommonActionsStrip) sender).DrawGrid;
        }

        private void OnCommonActionsStripRulersEnabledChanged(object sender, EventArgs e)
        {
            if (this.ActiveDocumentWorkspace != null)
            {
                this.ActiveDocumentWorkspace.RulersEnabled = this.Widgets.CommonActionsStrip.RulersEnabled;
            }
        }

        private void OnDocumentStripDocumentListChanged(object sender, EventArgs e)
        {
            bool enabled = this.Widgets.DocumentStrip.DocumentCount > 0;
            this.Widgets.ToolsForm.Enabled = enabled;
            this.Widgets.HistoryForm.Enabled = enabled;
            this.Widgets.LayersForm.Enabled = enabled;
            this.Widgets.ColorsForm.Enabled = enabled;
            this.Widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Paste, enabled);
            this.UpdateHistoryButtons();
            this.UpdateDocInfoInStatusBar();
            this.UpdateCursorInfoInStatusBar(0, 0);
        }

        private void OnDocumentStripDocumentTabClicked(object sender, ValueEventArgs<Tuple<DocumentWorkspace, DocumentClickAction>> e)
        {
            DocumentClickAction action = e.Value.Item2;
            if (action != DocumentClickAction.Select)
            {
                if (action != DocumentClickAction.Close)
                {
                    throw new NotImplementedException("Code for DocumentClickAction." + e.Value.Item2.ToString() + " not implemented");
                }
            }
            else
            {
                if (this.CanSetActiveWorkspace)
                {
                    this.ActiveDocumentWorkspace = e.Value.Item1;
                }
                goto Label_0078;
            }
            CloseWorkspaceAction performMe = new CloseWorkspaceAction(e.Value.Item1);
            this.PerformAction(performMe);
        Label_0078:
            this.QueueUpdate();
        }

        private void OnDocumentStripItemMoved(object sender, ImageStripItemMovedEventArgs e)
        {
            DocumentWorkspace item = this.documentWorkspaces[e.OldIndex];
            this.documentWorkspaces.RemoveAt(e.OldIndex);
            this.documentWorkspaces.Insert(e.NewIndex, item);
            this.UpdateDocumentWorkspaceTabbedThumbnailOrder(item);
        }

        private void OnDocumentWorkspaceDocumentChanged(object sender, EventArgs e)
        {
            this.UpdateDocInfoInStatusBar();
            UIUtil.ResumeControlPainting(this);
            base.Invalidate(true);
        }

        private void OnDocumentWorkspaceDocumentChanging(object sender, ValueEventArgs<Document> e)
        {
            UIUtil.SuspendControlPainting(this);
        }

        private void OnDocumentWorkspaceDrawGridChanged(object sender, EventArgs e)
        {
            if (this.ActiveDocumentWorkspace != null)
            {
                this.DrawGrid = this.ActiveDocumentWorkspace.DrawGrid;
            }
        }

        private void OnDocumentWorkspaceLayout(object sender, LayoutEventArgs e)
        {
            this.UpdateSnapObstacle();
        }

        private void OnDocumentWorkspaceRequestActivate(object sender, CancelEventArgs e)
        {
            DocumentWorkspace dw = (DocumentWorkspace) sender;
            e.Cancel = !this.TrySetActiveDocumentWorkspace(dw);
        }

        private void OnDocumentWorkspaceRequestClose(object sender, CancelEventArgs e)
        {
            if (PdnBaseForm.IsInThreadModalLoop)
            {
                e.Cancel = true;
            }
            else
            {
                DocumentWorkspace closeMe = (DocumentWorkspace) sender;
                CloseWorkspaceAction performMe = new CloseWorkspaceAction(closeMe);
                this.PerformAction(performMe);
                e.Cancel = performMe.Cancelled;
            }
        }

        private void OnDocumentWorkspaceRulersEnabledChanged(object sender, EventArgs e)
        {
            this.Widgets.CommonActionsStrip.RulersEnabled = this.ActiveDocumentWorkspace.RulersEnabled;
            this.globalRulersChoice = this.ActiveDocumentWorkspace.RulersEnabled;
            base.PerformLayout();
            this.ActiveDocumentWorkspace.UpdateRulerSelectionTinting();
            AppSettings.Instance.Workspace.ShowRulers.Value = this.ActiveDocumentWorkspace.RulersEnabled;
        }

        private void OnDocumentWorkspaceScroll(object sender, ScrollEventArgs e)
        {
            this.OnScroll(e);
            this.UpdateSnapObstacle();
        }

        private void OnDocumentWorkspaceStatusChanged(object sender, EventArgs e)
        {
            this.OnStatusChanged();
            this.UpdateStatusBarContextStatus();
        }

        private void OnDocumentWorkspaceTabbedThumbnailChanged(object sender, ValueChangedEventArgs<TabbedThumbnail> e)
        {
            DocumentWorkspace dw = (DocumentWorkspace) sender;
            this.UpdateDocumentWorkspaceTabbedThumbnailOrder(dw);
            if ((dw == this.ActiveDocumentWorkspace) && (dw.TabbedThumbnail != null))
            {
                try
                {
                    TaskbarManager.Instance.TabbedThumbnail.SetActiveTab(dw.TabbedThumbnail);
                }
                catch (Exception)
                {
                }
            }
        }

        private void OnDocumentWorkspaceZoomBasisChanged(object sender, EventArgs e)
        {
            if ((this.ActiveDocumentWorkspace != null) && (this.ActiveDocumentWorkspace.ZoomBasis != this.statusBar.ZoomBasis))
            {
                this.statusBar.ZoomBasis = this.ActiveDocumentWorkspace.ZoomBasis;
            }
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            this.UpdateSnapObstacle();
            base.OnEnabledChanged(e);
        }

        private void OnHistoryChangedHandler(object sender, EventArgs e)
        {
            this.UpdateHistoryButtons();
            this.UpdateDocInfoInStatusBar();
        }

        private void OnHistoryFormFastForwardButtonClicked(object sender, EventArgs e)
        {
            if (this.ActiveDocumentWorkspace != null)
            {
                this.ActiveDocumentWorkspace.PerformAction(new HistoryFastForwardAction());
            }
        }

        private void OnHistoryFormRedoButtonClicked(object sender, EventArgs e)
        {
            if (this.ActiveDocumentWorkspace != null)
            {
                this.ActiveDocumentWorkspace.PerformAction(new HistoryRedoAction());
            }
        }

        private void OnHistoryFormRewindButtonClicked(object sender, EventArgs e)
        {
            if (this.ActiveDocumentWorkspace != null)
            {
                this.ActiveDocumentWorkspace.PerformAction(new HistoryRewindAction());
            }
        }

        private void OnHistoryFormUndoButtonClicked(object sender, EventArgs e)
        {
            if (this.ActiveDocumentWorkspace != null)
            {
                this.ActiveDocumentWorkspace.PerformAction(new HistoryUndoAction());
            }
        }

        private void OnLayerFormDeleteLayerButtonClicked(object sender, EventArgs e)
        {
            if (((this.ActiveDocumentWorkspace != null) && (this.ActiveDocumentWorkspace.Document != null)) && (this.ActiveDocumentWorkspace.Document.Layers.Count > 1))
            {
                this.ActiveDocumentWorkspace.ApplyFunction(new DeleteLayerFunction(this.ActiveDocumentWorkspace.ActiveLayerIndex));
            }
        }

        private void OnLayerFormDuplicateLayerButtonClick(object sender, EventArgs e)
        {
            if (this.ActiveDocumentWorkspace != null)
            {
                this.ActiveDocumentWorkspace.ApplyFunction(new DuplicateLayerFunction(this.ActiveDocumentWorkspace.ActiveLayerIndex));
            }
        }

        private void OnLayerFormMergeLayerDownClick(object sender, EventArgs e)
        {
            if ((this.ActiveDocumentWorkspace != null) && (this.ActiveDocumentWorkspace.ActiveLayerIndex > 0))
            {
                int num = (this.ActiveDocumentWorkspace.ActiveLayerIndex - 1).Clamp(0, this.ActiveDocumentWorkspace.Document.Layers.Count - 1);
                this.ActiveDocumentWorkspace.ApplyFunction(new MergeLayerDownFunction(this.ActiveDocumentWorkspace.ActiveLayerIndex));
                this.ActiveDocumentWorkspace.ActiveLayerIndex = num;
            }
        }

        private void OnLayerFormMoveLayerDownButtonClicked(object sender, EventArgs e)
        {
            if (((this.ActiveDocumentWorkspace != null) && (this.ActiveDocumentWorkspace.Document != null)) && (this.ActiveDocumentWorkspace.Document.Layers.Count >= 2))
            {
                this.ActiveDocumentWorkspace.PerformAction(new MoveActiveLayerDownAction());
            }
        }

        private void OnLayerFormMoveLayerToBottomButtonClicked(object sender, EventArgs e)
        {
            if (((this.ActiveDocumentWorkspace != null) && (this.ActiveDocumentWorkspace.Document != null)) && (this.ActiveDocumentWorkspace.Document.Layers.Count >= 2))
            {
                this.ActiveDocumentWorkspace.PerformAction(new MoveActiveLayerToBottomAction());
            }
        }

        private void OnLayerFormMoveLayerToTopButtonClicked(object sender, EventArgs e)
        {
            if (((this.ActiveDocumentWorkspace != null) && (this.ActiveDocumentWorkspace.Document != null)) && (this.ActiveDocumentWorkspace.Document.Layers.Count >= 2))
            {
                this.ActiveDocumentWorkspace.PerformAction(new MoveActiveLayerToTopAction());
            }
        }

        private void OnLayerFormMoveLayerUpButtonClicked(object sender, EventArgs e)
        {
            if (((this.ActiveDocumentWorkspace != null) && (this.ActiveDocumentWorkspace.Document != null)) && (this.ActiveDocumentWorkspace.Document.Layers.Count >= 2))
            {
                this.ActiveDocumentWorkspace.PerformAction(new MoveActiveLayerUpAction());
            }
        }

        private void OnLayerFormNewLayerButtonClicked(object sender, EventArgs e)
        {
            if (this.ActiveDocumentWorkspace != null)
            {
                this.ActiveDocumentWorkspace.ApplyFunction(new AddNewBlankLayerFunction());
            }
        }

        private void OnLayerFormPropertiesButtonClick(object sender, EventArgs e)
        {
            if (this.ActiveDocumentWorkspace != null)
            {
                this.ActiveDocumentWorkspace.PerformAction(new OpenActiveLayerPropertiesAction());
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int height = this.statusBar.PreferredSize.Height;
            Size clientSize = base.ClientSize;
            this.statusBar.Bounds = new Rectangle(new Point(0, clientSize.Height - height), new Size(clientSize.Width, height));
            base.OnLayout(levent);
            Size size3 = base.ClientSize;
            this.workspacePanel.Bounds = new Rectangle(new Point(0, this.ToolBar.Bottom), new Size(size3.Width, (size3.Height - this.ToolBar.Height) - height));
            this.UpdateSnapObstacle();
        }

        protected override void OnLoad(EventArgs e)
        {
            if (this.ActiveDocumentWorkspace != null)
            {
                this.ActiveDocumentWorkspace.Select();
            }
            this.UpdateSnapObstacle();
            base.OnLoad(e);
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            this.UpdateSnapObstacle();
            base.OnLocationChanged(e);
        }

        private void OnMainToolBarToolClicked(object sender, ToolClickedEventArgs e)
        {
            if (this.ActiveDocumentWorkspace != null)
            {
                this.ActiveDocumentWorkspace.Focus();
                this.ActiveDocumentWorkspace.SetToolFromType(e.ToolType);
            }
        }

        private void OnMeasurementUnitValueChangedT(object sender, ValueChangedEventArgs<MeasurementUnit> e)
        {
            this.statusBar.Units = e.NewValue;
        }

        private void OnParentFormLayout(object sender, LayoutEventArgs e)
        {
            this.UpdateSnapObstacle();
        }

        private void OnParentFormMove(object sender, EventArgs e)
        {
            this.UpdateSnapObstacle();
        }

        private void OnParentFormMoving(object sender, MovingEventArgs e)
        {
            this.UpdateSnapObstacle();
        }

        private void OnParentFormResizeEnd(object sender, EventArgs e)
        {
            this.UpdateSnapObstacle();
        }

        private void OnParentFormSizeChanged(object sender, EventArgs e)
        {
            this.UpdateSnapObstacle();
        }

        internal void OnParentNonClientActivated()
        {
            this.ToolBar.OnParentNonClientActivated();
        }

        internal void OnParentNonClientDeactivate()
        {
            this.ToolBar.OnParentNonClientDeactivate();
        }

        protected override void OnResize(EventArgs e)
        {
            this.UpdateSnapObstacle();
            base.OnResize(e);
            if ((base.ParentForm != null) && (this.ActiveDocumentWorkspace != null))
            {
                if (base.ParentForm.WindowState == FormWindowState.Minimized)
                {
                    this.ActiveDocumentWorkspace.IsToolPulseEnabled = false;
                }
                else
                {
                    this.ActiveDocumentWorkspace.IsToolPulseEnabled = true;
                }
            }
        }

        protected virtual void OnRulersEnabledChanged()
        {
            this.RulersEnabledChanged.Raise(this);
        }

        private void OnShowPixelGridValueChangedT(object sender, ValueChangedEventArgs<bool> e)
        {
            this.DrawGrid = e.NewValue;
        }

        private void OnShowRulersValueChangedT(object sender, ValueChangedEventArgs<bool> e)
        {
            this.RulersEnabled = e.NewValue;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            this.UpdateSnapObstacle();
            base.OnSizeChanged(e);
        }

        private void OnStatusBarUnitsChanged(object sender, EventArgs e)
        {
            if (this.statusBar.Units != MeasurementUnit.Pixel)
            {
                AppSettings.Instance.Workspace.LastNonPixelUnits.Value = this.statusBar.Units;
            }
            if (this.ActiveDocumentWorkspace != null)
            {
                this.ActiveDocumentWorkspace.Units = this.Units;
            }
            AppSettings.Instance.Workspace.MeasurementUnit.Value = this.statusBar.Units;
            this.UpdateDocInfoInStatusBar();
            this.statusBar.CursorInfoText = string.Empty;
            this.OnUnitsChanged();
        }

        private void OnStatusBarZoomBasisChanged(object sender, EventArgs e)
        {
            if ((this.ActiveDocumentWorkspace != null) && (this.ActiveDocumentWorkspace.ZoomBasis != this.statusBar.ZoomBasis))
            {
                this.ActiveDocumentWorkspace.ZoomBasis = this.statusBar.ZoomBasis;
            }
        }

        private void OnStatusBarZoomIn(object sender, EventArgs e)
        {
            if (this.ActiveDocumentWorkspace != null)
            {
                this.ActiveDocumentWorkspace.ZoomIn();
            }
        }

        private void OnStatusBarZoomOut(object sender, EventArgs e)
        {
            if (this.ActiveDocumentWorkspace != null)
            {
                this.ActiveDocumentWorkspace.ZoomOut();
            }
        }

        private void OnStatusBarZoomScaleChanged(object sender, EventArgs e)
        {
            if (this.ActiveDocumentWorkspace != null)
            {
                this.ActiveDocumentWorkspace.ZoomBasis = ZoomBasis.ScaleFactor;
                this.ActiveDocumentWorkspace.ScaleFactor = this.statusBar.ScaleFactor;
            }
        }

        private void OnStatusChanged()
        {
            this.StatusChanged.Raise(this);
        }

        protected override void OnSystemColorsChanged(EventArgs e)
        {
            this.SetThemedBackColor();
            base.OnSystemColorsChanged(e);
        }

        private void OnToolConfigStripCommitButtonClicked(object sender, EventArgs e)
        {
            if (((this.ActiveDocumentWorkspace != null) && (this.ActiveDocumentWorkspace.Tool != null)) && (this.ActiveDocumentWorkspace.Tool.IsCommitSupported && this.ActiveDocumentWorkspace.Tool.CanCommit))
            {
                this.ActiveDocumentWorkspace.Tool.Commit();
            }
        }

        private void OnToolConfigStripSelectionDrawModeUnitsChanging(object sender, EventArgs e)
        {
            if ((this.ActiveDocumentWorkspace != null) && (this.ActiveDocumentWorkspace.Document != null))
            {
                new ToolConfigStripSelectionDrawModeUnitsChangeHandler(this.ToolBar.ToolConfigStrip, this.ActiveDocumentWorkspace.Document).Initialize();
            }
        }

        private bool OnToolFormProcessCmdKeyEvent(object sender, ref Message msg, Keys keyData) => 
            ((this.ProcessCmdKeyEvent != null) && this.ProcessCmdKeyEvent(sender, ref msg, keyData));

        private void OnToolStripMouseWheel(object sender, MouseEventArgs e)
        {
            if (this.ActiveDocumentWorkspace != null)
            {
                this.ActiveDocumentWorkspace.PerformMouseWheel((Control) sender, e);
            }
        }

        private void OnToolStripRelinquishFocus(object sender, EventArgs e)
        {
            if (this.ActiveDocumentWorkspace != null)
            {
                this.ActiveDocumentWorkspace.Focus();
            }
        }

        protected virtual void OnUnitsChanged()
        {
            this.UnitsChanged.Raise(this);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            this.UpdateSnapObstacle();
            base.OnVisibleChanged(e);
        }

        public bool OpenFileInNewWorkspace(string fileName)
        {
            DocumentWorkspace workspace;
            return this.OpenFileInNewWorkspace(fileName, true, out workspace);
        }

        public bool OpenFileInNewWorkspace(string fileName, bool addToMruList, out DocumentWorkspace dwResult)
        {
            FileType type;
            Validate.IsNotNullOrWhiteSpace(fileName, "fileName");
            PdnBaseForm.UpdateAllForms();
            if (!this.CanSetActiveWorkspace)
            {
                dwResult = null;
                return false;
            }
            this.Widgets.StatusBarProgress.ResetProgressStatusBar();
            ProgressEventHandler progressCallback = delegate (object s, ProgressEventArgs e) {
                this.Widgets.StatusBarProgress.SetProgressStatusBar(new double?(e.Percent));
            };
            Document document = DocumentWorkspace.LoadDocument(this, fileName, out type, progressCallback);
            this.Widgets.StatusBarProgress.EraseProgressStatusBar();
            if (document == null)
            {
                this.Cursor = Cursors.Default;
                dwResult = null;
            }
            else
            {
                using (new WaitCursorChanger(this))
                {
                    DocumentWorkspace lockMe = this.AddNewDocumentWorkspace();
                    this.Widgets.DocumentStrip.LockDocumentWorkspaceDirtyValue(lockMe, false);
                    try
                    {
                        lockMe.Document = document;
                    }
                    catch (OutOfMemoryException exception)
                    {
                        ExceptionDialog.ShowErrorDialog(this, PdnResources.GetString("LoadImage.Error.OutOfMemoryException"), exception);
                        this.RemoveDocumentWorkspace(lockMe);
                        document.Dispose();
                        dwResult = null;
                        return false;
                    }
                    lockMe.ActiveLayer = (Layer) document.Layers[0];
                    lockMe.SetDocumentSaveOptions(fileName, type, null);
                    this.ActiveDocumentWorkspace = lockMe;
                    lockMe.History.ClearAll();
                    lockMe.History.PushNewMemento(new NullHistoryMemento(PdnResources.GetString("OpenImageAction.Name"), this.ImageFromDiskIcon));
                    document.Dirty = false;
                    this.Widgets.DocumentStrip.UnlockDocumentWorkspaceDirtyValue(lockMe);
                }
                dwResult = this.ActiveDocumentWorkspace;
                this.ActiveDocumentWorkspace.ZoomBasis = ZoomBasis.FitToWindow;
                if (addToMruList)
                {
                    Task task = this.ActiveDocumentWorkspace.AddToMruList();
                    UIUtil.BeginFrame(this, true, delegate (UIUtil.IFrame frame) {
                        task.ResultAsync().Receive(_ => frame.Close());
                    });
                }
            }
            if (this.ActiveDocumentWorkspace != null)
            {
                this.ActiveDocumentWorkspace.Focus();
            }
            this.Widgets.StatusBarProgress.EraseProgressStatusBar();
            return (document > null);
        }

        public bool OpenFilesInNewWorkspace(string[] fileNames)
        {
            if (base.IsDisposed)
            {
                return false;
            }
            bool flag = true;
            List<Task> taskList = new List<Task>();
            for (int i = 0; i < fileNames.Length; i++)
            {
                DocumentWorkspace workspace;
                flag &= this.OpenFileInNewWorkspace(fileNames[i], false, out workspace);
                if (flag && (i >= (fileNames.Length - MostRecentFilesService.Instance.MaxCount)))
                {
                    Task item = workspace.AddToMruList();
                    taskList.Add(item);
                }
                if (!flag)
                {
                    break;
                }
            }
            if (taskList.Count > 0)
            {
                int mruDoneCount = 0;
                UIUtil.BeginFrame(this, true, delegate (UIUtil.IFrame frame) {
                    foreach (Task task in taskList)
                    {
                        Action<Result> <>9__1;
                        task.ResultAsync().Receive(<>9__1 ?? (<>9__1 = delegate (Result <obj>) {
                            if (Interlocked.Increment(ref mruDoneCount) == taskList.Count)
                            {
                                frame.Close();
                            }
                        }));
                    }
                });
            }
            return flag;
        }

        public void PerformAction(AppWorkspaceAction performMe)
        {
            this.QueueUpdate();
            using (new WaitCursorChanger(this))
            {
                performMe.PerformAction(this);
            }
            this.QueueUpdate();
        }

        private void PrimaryColorChangedHandler(object sender, EventArgs e)
        {
            if (sender == this.ToolSettings.PrimaryColor)
            {
                this.Widgets.ColorsForm.UserPrimaryColor = this.ToolSettings.PrimaryColor.Value;
            }
        }

        public void RefreshTool()
        {
            if (this.ActiveDocumentWorkspace != null)
            {
                System.Type toolType = this.ActiveDocumentWorkspace.GetToolType();
                this.Widgets.ToolsControl.SelectTool(toolType);
            }
        }

        private void RelinquishFocusHandler(object sender, EventArgs e)
        {
            base.Focus();
        }

        private void RelinquishFocusHandler2(object sender, EventArgs e)
        {
            if (this.ActiveDocumentWorkspace != null)
            {
                this.ActiveDocumentWorkspace.Focus();
            }
        }

        public void RemoveDocumentWorkspace(DocumentWorkspace documentWorkspace)
        {
            bool flag;
            int index = this.documentWorkspaces.IndexOf(documentWorkspace);
            if (index == -1)
            {
                throw new ArgumentException("DocumentWorkspace was not created with AddNewDocumentWorkspace");
            }
            if (this.ActiveDocumentWorkspace == documentWorkspace)
            {
                flag = true;
                this.GlobalToolTypeChoice = documentWorkspace.GetToolType();
            }
            else
            {
                flag = false;
            }
            if (flag && !this.CanSetActiveWorkspace)
            {
                ExceptionUtil.ThrowInvalidOperationException("Cannot remove the active DocumentWorkspace when CanSetActiveWorkspace is false");
            }
            TransactedTool tool = documentWorkspace.Tool as TransactedTool;
            if (tool != null)
            {
                tool.ForceCancelDrawingOrEditing();
            }
            documentWorkspace.ClearTool();
            if (flag)
            {
                if (this.documentWorkspaces.Count == 1)
                {
                    this.ActiveDocumentWorkspace = null;
                }
                else if (index == (this.documentWorkspaces.Count - 1))
                {
                    this.ActiveDocumentWorkspace = this.documentWorkspaces[this.documentWorkspaces.Count - 2];
                }
                else
                {
                    this.ActiveDocumentWorkspace = this.documentWorkspaces[index + 1];
                }
            }
            this.documentWorkspaces.Remove(documentWorkspace);
            this.ToolBar.DocumentStrip.RemoveDocumentWorkspace(documentWorkspace);
            if (this.initialWorkspace == documentWorkspace)
            {
                this.initialWorkspace = null;
            }
            documentWorkspace.RequestActivate -= new CancelEventHandler(this.OnDocumentWorkspaceRequestActivate);
            documentWorkspace.RequestClose -= new CancelEventHandler(this.OnDocumentWorkspaceRequestClose);
            documentWorkspace.TabbedThumbnailChanged -= new ValueChangedEventHandler<TabbedThumbnail>(this.OnDocumentWorkspaceTabbedThumbnailChanged);
            if (documentWorkspace.TabbedThumbnail != null)
            {
                try
                {
                    TaskbarManager.Instance.TabbedThumbnail.RemoveThumbnailPreview(documentWorkspace.TabbedThumbnail);
                }
                catch (Exception)
                {
                }
            }
            Document document = documentWorkspace.Document;
            documentWorkspace.Document = null;
            this.workspacePanel.Controls.Remove(documentWorkspace);
            documentWorkspace.Dispose();
            document.Dispose();
            documentWorkspace = null;
        }

        public void ResetFloatingForm(FloatingToolForm ftf)
        {
            SnapManager manager = SnapManager.FindMySnapManager(this);
            if (ftf == this.Widgets.ToolsForm)
            {
                manager.ParkObstacle(this.Widgets.ToolsForm, this, HorizontalSnapEdge.Top, VerticalSnapEdge.Left);
            }
            else if (ftf == this.Widgets.HistoryForm)
            {
                manager.ParkObstacle(this.Widgets.HistoryForm, this, HorizontalSnapEdge.Top, VerticalSnapEdge.Right);
            }
            else if (ftf == this.Widgets.LayersForm)
            {
                manager.ParkObstacle(this.Widgets.LayersForm, this, HorizontalSnapEdge.Bottom, VerticalSnapEdge.Right);
            }
            else
            {
                if (ftf != this.Widgets.ColorsForm)
                {
                    throw new ArgumentException();
                }
                manager.ParkObstacle(this.Widgets.ColorsForm, this, HorizontalSnapEdge.Bottom, VerticalSnapEdge.Left);
            }
        }

        public void ResetFloatingForms()
        {
            this.ResetFloatingForm(this.Widgets.ToolsForm);
            this.ResetFloatingForm(this.Widgets.HistoryForm);
            this.ResetFloatingForm(this.Widgets.LayersForm);
            this.ResetFloatingForm(this.Widgets.ColorsForm);
        }

        private void ResumeThumbnailUpdates()
        {
            this.suspendThumbnailUpdates--;
            if (this.suspendThumbnailUpdates == 0)
            {
                this.Widgets.DocumentStrip.ResumeThumbnailUpdates();
            }
        }

        private void ResumeUpdateSnapObstacle()
        {
            this.ignoreUpdateSnapObstacle--;
        }

        public void RunEffect(System.Type effectType)
        {
            this.ToolBar.MainMenu.RunEffect(effectType);
        }

        public void SaveSettings()
        {
            AppSettings.Instance.Workspace.ShowRulers.Value = this.globalRulersChoice;
            AppSettings.Instance.Workspace.ShowPixelGrid.Value = this.DrawGrid;
            MostRecentFilesService.Instance.SaveMruList();
        }

        private void SecondaryColorChangedHandler(object sender, EventArgs e)
        {
            if (sender == this.ToolSettings.SecondaryColor)
            {
                this.Widgets.ColorsForm.UserSecondaryColor = this.ToolSettings.SecondaryColor.Value;
            }
        }

        private void SelectedPathChangedHandler(object sender, EventArgs e)
        {
            this.UpdateSelectionToolbarButtons();
        }

        private void SelectedPathChangingHandler(object sender, EventArgs e)
        {
        }

        public void SetGlassWndProcFilter(IMessageFilter filter)
        {
            this.glassWndProcFilter = filter;
            this.ToolBar.SetGlassWndProcFilter(filter);
        }

        private void SetThemedBackColor()
        {
            if (ThemeConfig.EffectiveTheme == PdnTheme.Aero)
            {
                this.BackColor = AeroColors.CanvasBackFillColor;
            }
            else if (SystemInformation.HighContrast)
            {
                this.BackColor = Control.DefaultBackColor;
            }
            else
            {
                this.BackColor = ClassicColors.CanvasBackFillColor;
            }
        }

        public IDisposable SuspendThumbnailUpdates()
        {
            IDisposable disposable = Disposable.FromAction(new Action(this.ResumeThumbnailUpdates), false);
            this.suspendThumbnailUpdates++;
            if (this.suspendThumbnailUpdates == 1)
            {
                this.Widgets.DocumentStrip.SuspendThumbnailUpdates();
            }
            return disposable;
        }

        private void SuspendUpdateSnapObstacle()
        {
            this.ignoreUpdateSnapObstacle++;
        }

        private void ToolChangedHandler(object sender, EventArgs e)
        {
            if ((this.ActiveDocumentWorkspace != null) && (this.ActiveDocumentWorkspace.Tool != null))
            {
                this.Widgets.ToolsControl.SelectTool(this.ActiveDocumentWorkspace.GetToolType(), false);
                this.ToolBar.ToolChooserStrip.SelectTool(this.ActiveDocumentWorkspace.GetToolType(), false);
                this.GlobalToolTypeChoice = this.ActiveDocumentWorkspace.GetToolType();
                this.UpdateToolBarConfigItems();
                this.UpdateToolBarCommitButton();
                this.HookToolEvents();
            }
            this.UpdateStatusBarContextStatus();
            UIUtil.ResumeControlPainting(this.ToolBar);
            this.ToolBar.PerformLayout();
            this.ToolBar.Invalidate();
        }

        private void ToolChangingHandler(object sender, EventArgs e)
        {
            UIUtil.SuspendControlPainting(this.ToolBar);
            this.UnhookToolEvents();
        }

        public bool TrySetActiveDocumentWorkspace(DocumentWorkspace dw)
        {
            this.VerifyThreadAccess();
            if (!this.CanSetActiveWorkspace)
            {
                return false;
            }
            if (dw != this.ActiveDocumentWorkspace)
            {
                if ((dw != null) && (this.documentWorkspaces.IndexOf(dw) == -1))
                {
                    throw new ArgumentException("DocumentWorkspace was not created with AddNewDocumentWorkspace");
                }
                bool focused = false;
                if (this.ActiveDocumentWorkspace != null)
                {
                    focused = this.ActiveDocumentWorkspace.Focused;
                }
                UIUtil.SuspendControlPainting(this);
                this.OnActiveDocumentWorkspaceChanging();
                this.activeDocumentWorkspace = dw;
                this.OnActiveDocumentWorkspaceChanged();
                UIUtil.ResumeControlPainting(this);
                this.Refresh();
            }
            if (dw != null)
            {
                dw.Focus();
                if (dw.TabbedThumbnail != null)
                {
                    try
                    {
                        TaskbarManager.Instance.TabbedThumbnail.SetActiveTab(dw.TabbedThumbnail);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            return true;
        }

        private void UnhookToolEvents()
        {
            if ((this.ActiveDocumentWorkspace != null) && (this.ActiveDocumentWorkspace.Tool != null))
            {
                this.ActiveDocumentWorkspace.Tool.ToolBarConfigItemsChanged -= new EventHandler(this.OnActiveToolToolBarConfigItemsChanged);
                this.ActiveDocumentWorkspace.Tool.IsCommitSupportedChanged -= new EventHandler(this.OnActiveToolIsCommitSupportedChanged);
                this.ActiveDocumentWorkspace.Tool.CanCommitChanged -= new EventHandler(this.OnActiveToolCanCommitChanged);
            }
        }

        private void UnlockActiveDocumentWorkspace()
        {
            this.VerifyThreadAccess();
            this.lockActiveDocumentWorkspaceCount--;
        }

        private void UpdateCursorInfoInStatusBar(int cursorX, int cursorY)
        {
            base.SuspendLayout();
            if ((this.ActiveDocumentWorkspace == null) || (this.ActiveDocumentWorkspace.Document == null))
            {
                this.statusBar.CursorInfoText = string.Empty;
            }
            else
            {
                string str;
                string str2;
                string str3;
                this.CoordinatesToStrings(cursorX, cursorY, out str, out str2, out str3);
                string str4 = string.Format(CultureInfo.InvariantCulture, this.cursorInfoStatusBarFormat, str, str2);
                this.statusBar.CursorInfoText = str4;
            }
            base.ResumeLayout(false);
        }

        private void UpdateDocInfoInStatusBar()
        {
            if ((this.ActiveDocumentWorkspace == null) || (this.ActiveDocumentWorkspace.Document == null))
            {
                this.statusBar.ImageInfoStatusText = string.Empty;
            }
            else if ((this.ActiveDocumentWorkspace != null) && (this.ActiveDocumentWorkspace.Document != null))
            {
                string str;
                string str2;
                string str3;
                this.CoordinatesToStrings(this.ActiveDocumentWorkspace.Document.Width, this.ActiveDocumentWorkspace.Document.Height, out str, out str2, out str3);
                string str4 = string.Format(CultureInfo.InvariantCulture, this.imageInfoStatusBarFormat, str, str2);
                this.statusBar.ImageInfoStatusText = str4;
            }
        }

        private void UpdateDocumentWorkspaceTabbedThumbnailOrder(DocumentWorkspace dw)
        {
            Validate.IsNotNull<DocumentWorkspace>(dw, "dw");
            int index = this.documentWorkspaces.IndexOf(dw);
            if ((index != -1) && (dw.TabbedThumbnail != null))
            {
                TabbedThumbnail tabbedThumbnail;
                if (index == (this.documentWorkspaces.Count - 1))
                {
                    tabbedThumbnail = null;
                }
                else
                {
                    tabbedThumbnail = this.documentWorkspaces[index + 1].TabbedThumbnail;
                }
                try
                {
                    TabbedThumbnailManager.SetTabOrder(dw.TabbedThumbnail, tabbedThumbnail);
                }
                catch (Exception)
                {
                }
            }
        }

        private void UpdateHistoryButtons()
        {
            if (this.ActiveDocumentWorkspace == null)
            {
                this.Widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Undo, false);
                this.Widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Redo, false);
            }
            else
            {
                if (this.ActiveDocumentWorkspace.History.UndoStack.Count > 1)
                {
                    this.Widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Undo, true);
                }
                else
                {
                    this.Widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Undo, false);
                }
                if (this.ActiveDocumentWorkspace.History.RedoStack.Count > 0)
                {
                    this.Widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Redo, true);
                }
                else
                {
                    this.Widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Redo, false);
                }
            }
        }

        private void UpdateSelectionToolbarButtons()
        {
            if ((this.ActiveDocumentWorkspace == null) || this.ActiveDocumentWorkspace.Selection.IsEmpty)
            {
                this.Widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Cut, false);
                this.Widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Copy, false);
                this.Widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Deselect, false);
                this.Widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.CropToSelection, false);
            }
            else
            {
                this.Widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Cut, true);
                this.Widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Copy, true);
                this.Widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Deselect, true);
                this.Widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.CropToSelection, true);
            }
        }

        private void UpdateSnapObstacle()
        {
            if ((this.ignoreUpdateSnapObstacle <= 0) && (this.snapObstacle != null))
            {
                if (!this.addedToSnapManager)
                {
                    SnapManager manager = SnapManager.FindMySnapManager(this);
                    if (manager != null)
                    {
                        PaintDotNet.Snap.SnapObstacle snapObstacle = this.SnapObstacle;
                        if (!this.addedToSnapManager)
                        {
                            manager.AddSnapObstacle(this.SnapObstacle);
                            this.addedToSnapManager = true;
                            base.FindForm().Shown += new EventHandler(this.OnAppWorkspaceShown);
                        }
                    }
                }
                if (this.snapObstacle != null)
                {
                    RectInt32 visibleViewRect;
                    RectInt32 num3;
                    if (this.ActiveDocumentWorkspace != null)
                    {
                        visibleViewRect = this.ActiveDocumentWorkspace.VisibleViewRect;
                    }
                    else
                    {
                        visibleViewRect = this.workspacePanel.ClientRectangle.ToRectInt32();
                    }
                    RectInt32 num2 = this.workspacePanel.RectangleToScreen(visibleViewRect);
                    if (OS.IsWin10OrLater && !SystemInformation.HighContrast)
                    {
                        int num4 = (num2.Top + SystemMetrics.PaddedBorderExtent) + SystemMetrics.SizeFrameHeight;
                        int bottom = Math.Max(num4, num2.Bottom);
                        num3 = RectInt32.FromEdges(num2.Left, num4, num2.Right, bottom);
                    }
                    else
                    {
                        num3 = num2;
                    }
                    this.snapObstacle.SetBounds(num3);
                    this.snapObstacle.Enabled = base.Visible && base.Enabled;
                }
            }
        }

        private void UpdateStatusBarContextStatus()
        {
            if (this.ActiveDocumentWorkspace != null)
            {
                this.statusBar.ContextStatusText = this.ActiveDocumentWorkspace.StatusText;
                this.statusBar.ContextStatusImage = this.ActiveDocumentWorkspace.StatusIcon;
            }
            else
            {
                this.statusBar.ContextStatusText = string.Empty;
                this.statusBar.ContextStatusImage = null;
            }
        }

        private void UpdateToolBarCommitButton()
        {
            this.VerifyAccess();
            bool isCommitSupported = false;
            bool canCommit = false;
            if ((this.ActiveDocumentWorkspace != null) && (this.ActiveDocumentWorkspace.Tool != null))
            {
                isCommitSupported = this.ActiveDocumentWorkspace.Tool.IsCommitSupported;
                canCommit = this.ActiveDocumentWorkspace.Tool.CanCommit;
            }
            this.ToolBar.ToolConfigStrip.IsCommitButtonVisible = isCommitSupported;
            this.ToolBar.ToolConfigStrip.IsCommitButtonEnabled = canCommit;
        }

        private void UpdateToolBarConfigItems()
        {
            ToolBarConfigItems toolBarConfigItems;
            this.VerifyAccess();
            if ((this.ActiveDocumentWorkspace != null) && (this.ActiveDocumentWorkspace.Tool != null))
            {
                toolBarConfigItems = this.ActiveDocumentWorkspace.Tool.ToolBarConfigItems;
            }
            else
            {
                toolBarConfigItems = ToolBarConfigItems.None;
            }
            this.ToolBar.ToolConfigStrip.ToolBarConfigItems = toolBarConfigItems | (ToolBarConfigItems.None | ToolBarConfigItems.SelectionRenderingQuality);
        }

        public void VerifyAccess()
        {
            if (!this.CheckAccess())
            {
                ExceptionUtil.ThrowInvalidOperationException("The object may not be accessed from this thread");
            }
        }

        protected override void WndProc(ref Message m)
        {
            bool flag = false;
            if (this.glassWndProcFilter != null)
            {
                flag = this.glassWndProcFilter.PreFilterMessage(ref m);
            }
            if (!flag)
            {
                base.WndProc(ref m);
            }
        }

        private void ZoomChangedHandler(object sender, EventArgs e)
        {
            if (this.ActiveDocumentWorkspace != null)
            {
                ScaleFactor scaleFactor = this.ActiveDocumentWorkspace.ScaleFactor;
                this.statusBar.SuspendEvents();
                this.statusBar.ZoomBasis = this.ActiveDocumentWorkspace.ZoomBasis;
                this.statusBar.ScaleFactor = scaleFactor;
                this.statusBar.ResumeEvents();
            }
        }

        public DocumentWorkspace ActiveDocumentWorkspace
        {
            get => 
                this.activeDocumentWorkspace;
            set
            {
                if (!this.CanSetActiveWorkspace)
                {
                    ExceptionUtil.ThrowInvalidOperationException("CanSetActiveWorkspace must be true");
                }
                if (!this.TrySetActiveDocumentWorkspace(value))
                {
                    throw new InternalErrorException("CanSetActiveWorkspace was true, but TrySetActiveWorkspace returned false");
                }
            }
        }

        public IDispatcher BackgroundThread =>
            this.backgroundThread;

        public bool CanSetActiveWorkspace
        {
            get
            {
                this.VerifyThreadAccess();
                return (this.lockActiveDocumentWorkspaceCount == 0);
            }
        }

        protected override Size DefaultSize =>
            new Size(0x500, 0x300);

        public System.Type DefaultToolType
        {
            get => 
                this.defaultToolTypeChoice;
            set
            {
                this.defaultToolTypeChoice = value;
                if (value != null)
                {
                    AppSettings.Instance.ToolDefaults.ActiveToolName.Value = value.Name;
                }
            }
        }

        public IDispatcher Dispatcher =>
            this.dispatcher;

        public DocumentWorkspace[] DocumentWorkspaces
        {
            get
            {
                this.VerifyThreadAccess();
                return this.documentWorkspaces.ToArrayEx<DocumentWorkspace>();
            }
        }

        public bool DrawCaptionArea
        {
            get => 
                this.ToolBar.DrawCaptionArea;
            set
            {
                this.ToolBar.DrawCaptionArea = value;
            }
        }

        private bool DrawGrid
        {
            get => 
                this.Widgets.CommonActionsStrip.DrawGrid;
            set
            {
                if (this.Widgets.CommonActionsStrip.DrawGrid != value)
                {
                    this.Widgets.CommonActionsStrip.DrawGrid = value;
                }
                if ((this.ActiveDocumentWorkspace != null) && (this.ActiveDocumentWorkspace.DrawGrid != value))
                {
                    this.ActiveDocumentWorkspace.DrawGrid = value;
                }
                AppSettings.Instance.Workspace.ShowPixelGrid.Value = value;
            }
        }

        private ImageResource FileNewIcon =>
            PdnResources.GetImageResource("Icons.MenuFileNewIcon.png");

        public Padding GlassCaptionDragInset
        {
            get
            {
                Padding glassCaptionDragInset = this.ToolBar.GlassCaptionDragInset;
                return new Padding(0, glassCaptionDragInset.Top, 0, 0);
            }
        }

        public Padding GlassInset
        {
            get
            {
                Padding glassInset = this.ToolBar.GlassInset;
                return new Padding(0, glassInset.Top, 0, 0);
            }
        }

        public System.Type GlobalToolTypeChoice
        {
            get => 
                this.globalToolTypeChoice;
            set
            {
                this.globalToolTypeChoice = value;
                if (this.ActiveDocumentWorkspace != null)
                {
                    this.ActiveDocumentWorkspace.SetToolFromType(value);
                }
                if (value != null)
                {
                    this.ToolSettings.ActiveToolName.Value = value.Name;
                }
            }
        }

        private ImageResource ImageFromDiskIcon =>
            PdnResources.GetImageResource("Icons.ImageFromDiskIcon.png");

        public DocumentWorkspace InitialWorkspace
        {
            set
            {
                this.initialWorkspace = value;
            }
        }

        public bool IsGlassDesired =>
            this.ToolBar.IsGlassDesired;

        Size IGlassyControl.ClientSize =>
            base.ClientSize;

        public bool RulersEnabled
        {
            get => 
                this.globalRulersChoice;
            set
            {
                if (this.globalRulersChoice != value)
                {
                    this.globalRulersChoice = value;
                    if (this.ActiveDocumentWorkspace != null)
                    {
                        this.ActiveDocumentWorkspace.RulersEnabled = value;
                    }
                    this.OnRulersEnabledChanged();
                }
            }
        }

        public PaintDotNet.Snap.SnapObstacle SnapObstacle
        {
            get
            {
                if (this.snapObstacle == null)
                {
                    this.snapObstacle = new SnapObstacleController("AppWorkspace", RectInt32.Zero, SnapRegion.Interior, true, null);
                    PdnBaseForm form = base.FindForm() as PdnBaseForm;
                    form.Moving += new MovingEventHandler(this.OnParentFormMoving);
                    form.Move += new EventHandler(this.OnParentFormMove);
                    form.ResizeEnd += new EventHandler(this.OnParentFormResizeEnd);
                    form.Layout += new LayoutEventHandler(this.OnParentFormLayout);
                    form.SizeChanged += new EventHandler(this.OnParentFormSizeChanged);
                    this.UpdateSnapObstacle();
                }
                return this.snapObstacle;
            }
        }

        public PaintDotNet.Threading.Tasks.TaskManager TaskManager =>
            this.taskManager;

        public PdnToolBar ToolBar =>
            this.toolBar;

        public AppSettings.ToolsSection ToolSettings =>
            this.toolSettings.Tools;

        public MeasurementUnit Units
        {
            get => 
                this.statusBar.Units;
            set
            {
                this.statusBar.Units = value;
            }
        }

        public WorkspaceWidgets Widgets =>
            this.widgets;


        private sealed class ToolConfigStripSelectionDrawModeUnitsChangeHandler
        {
            private Document activeDocument;
            private ToolConfigStrip toolConfigStrip;

            public ToolConfigStripSelectionDrawModeUnitsChangeHandler(ToolConfigStrip toolConfigStrip, Document activeDocument)
            {
                this.toolConfigStrip = toolConfigStrip;
                this.activeDocument = activeDocument;
            }

            public void Initialize()
            {
                this.toolConfigStrip.ToolSettings.Selection.DrawUnits.ValueChangedT += new ValueChangedEventHandler<MeasurementUnit>(this.OnToolConfigStripSelectionDrawModeUnitsChanged);
            }

            public void OnToolConfigStripSelectionDrawModeUnitsChanged(object sender, ValueChangedEventArgs<MeasurementUnit> e)
            {
                try
                {
                    if (!this.toolConfigStrip.IsDisposed && !this.activeDocument.IsDisposed)
                    {
                        double sourceLength = this.toolConfigStrip.ToolSettings.Selection.DrawWidth.Value;
                        double num2 = this.toolConfigStrip.ToolSettings.Selection.DrawHeight.Value;
                        double num3 = Document.ConvertMeasurement(sourceLength, e.OldValue, this.activeDocument.DpuUnit, this.activeDocument.DpuX, e.NewValue);
                        double num4 = Document.ConvertMeasurement(num2, e.OldValue, this.activeDocument.DpuUnit, this.activeDocument.DpuY, e.NewValue);
                        using (this.toolConfigStrip.ToolSettings.Selection.DrawWidth.SuspendValueChangedEvent())
                        {
                            using (this.toolConfigStrip.ToolSettings.Selection.DrawHeight.SuspendValueChangedEvent())
                            {
                                this.toolConfigStrip.ToolSettings.Selection.DrawWidth.Value = num3;
                                this.toolConfigStrip.ToolSettings.Selection.DrawHeight.Value = num4;
                            }
                        }
                    }
                }
                finally
                {
                    this.toolConfigStrip.ToolSettings.Selection.DrawUnits.ValueChangedT -= new ValueChangedEventHandler<MeasurementUnit>(this.OnToolConfigStripSelectionDrawModeUnitsChanged);
                }
            }
        }
    }
}

