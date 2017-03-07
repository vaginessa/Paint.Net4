namespace PaintDotNet.Dialogs
{
    using PaintDotNet;
    using PaintDotNet.Actions;
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.Drawing;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.Settings.App;
    using PaintDotNet.Snap;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Threading;
    using PaintDotNet.Updates;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class MainForm : GdiPaintForm, ISnapManagerHost, IThreadAffinitizedObject
    {
        private AppWorkspace appWorkspace;
        private IContainer components;
        private Button defaultButton;
        private System.Windows.Forms.Timer deferredInitializationTimer;
        private FloatingToolForm[] floatingToolForms;
        private bool processingOpen;
        private SegmentedList<string> queuedInstanceMessages;
        private PaintDotNet.SystemLayer.SingleInstanceManager singleInstanceManager;
        private PaintDotNet.Snap.SnapManager snapManager;

        public MainForm() : this(Array.Empty<string>())
        {
        }

        public MainForm(string[] args)
        {
            this.queuedInstanceMessages = new SegmentedList<string>();
            bool flag = true;
            base.StartPosition = FormStartPosition.WindowsDefaultLocation;
            List<string> list = new List<string>();
            foreach (string str in args)
            {
                if ((str.Length > 0) && (str[0] != '/'))
                {
                    try
                    {
                        string fullPath = Path.GetFullPath(str);
                        list.Add(fullPath);
                    }
                    catch (Exception)
                    {
                        list.Add(str);
                        flag = false;
                    }
                }
            }
            if (flag)
            {
                try
                {
                    Environment.CurrentDirectory = PdnInfo.ApplicationDir;
                }
                catch (Exception)
                {
                }
            }
            base.SuspendLayout();
            this.LoadWindowState();
            base.ResumeLayout(false);
            this.InitializeComponent();
            base.IsGlassDesired = this.appWorkspace.IsGlassDesired;
            base.Icon = PdnInfo.AppIcon;
            PdnBaseForm.EnableAutoGlass = AppSettings.Instance.UI.GlassButtonFooters.Value;
            AppSettings.Instance.UI.GlassButtonFooters.ValueChangedT += new ValueChangedEventHandler<bool>(this.OnGlassButtonFootersValueChangedT);
            PdnBaseForm.EnableOpacity = AppSettings.Instance.UI.TranslucentWindows.Value;
            AppSettings.Instance.UI.TranslucentWindows.ValueChangedT += new ValueChangedEventHandler<bool>(this.OnTranslucentWindowsValueChangedT);
            foreach (string str3 in list)
            {
                this.queuedInstanceMessages.Add(str3);
            }
            MeasurementUnit defaultDpuUnit = Document.DefaultDpuUnit;
            double defaultDpu = Document.GetDefaultDpu(defaultDpuUnit);
            SizeInt32 newDocumentSize = this.appWorkspace.GetNewDocumentSize();
            this.appWorkspace.CreateBlankDocumentInNewWorkspace(newDocumentSize, defaultDpuUnit, defaultDpu, true);
            this.appWorkspace.ActiveDocumentWorkspace.Document.Dirty = false;
            this.deferredInitializationTimer.Enabled = true;
            Application.Idle += new EventHandler(this.OnApplicationIdle);
            if (base.WindowState == FormWindowState.Normal)
            {
                if (!base.IsHandleCreated)
                {
                    this.CreateHandle();
                }
                this.LoadWindowState();
            }
        }

        public IUpdatesServiceHost CreateUpdatesServiceHost() => 
            new AppWorkspaceUpdatesServiceHost(this.appWorkspace);

        private void DeferredInitialization(object sender, EventArgs e)
        {
            this.deferredInitializationTimer.Enabled = false;
            this.deferredInitializationTimer.Tick -= new EventHandler(this.DeferredInitialization);
            this.deferredInitializationTimer.Dispose();
            this.deferredInitializationTimer = null;
            this.appWorkspace.ToolBar.MainMenu.PopulateEffects();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.singleInstanceManager != null)
                {
                    PaintDotNet.SystemLayer.SingleInstanceManager singleInstanceManager = this.singleInstanceManager;
                    this.SingleInstanceManager = null;
                    singleInstanceManager.Dispose();
                    singleInstanceManager = null;
                }
                if (this.components != null)
                {
                    this.components.Dispose();
                    this.components = null;
                }
            }
            try
            {
                base.Dispose(disposing);
            }
            catch (RankException)
            {
            }
        }

        private void FloatingToolFormHideInsteadOfCloseHandler(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            ((Form) sender).Hide();
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            this.defaultButton = new Button();
            this.appWorkspace = new AppWorkspace();
            this.deferredInitializationTimer = new System.Windows.Forms.Timer(this.components);
            base.SuspendLayout();
            this.appWorkspace.SuspendLayout();
            this.appWorkspace.Name = "appWorkspace";
            this.appWorkspace.Dock = DockStyle.Fill;
            this.appWorkspace.TabIndex = 2;
            this.appWorkspace.ActiveDocumentWorkspaceChanging += new EventHandler(this.OnAppWorkspaceActiveDocumentWorkspaceChanging);
            this.appWorkspace.ActiveDocumentWorkspaceChanged += new EventHandler(this.OnAppWorkspaceActiveDocumentWorkspaceChanged);
            this.deferredInitializationTimer.Interval = 250;
            this.deferredInitializationTimer.Tick += new EventHandler(this.DeferredInitialization);
            this.defaultButton.Size = new Size(1, 1);
            this.defaultButton.Text = "";
            this.defaultButton.Location = new Point(-100, -100);
            this.defaultButton.TabStop = false;
            this.defaultButton.Click += new EventHandler(this.OnDefaultButtonClick);
            try
            {
                this.AllowDrop = true;
            }
            catch (InvalidOperationException)
            {
            }
            base.Controls.Add(this.appWorkspace);
            base.Controls.Add(this.defaultButton);
            base.AcceptButton = this.defaultButton;
            base.Name = "MainForm";
            base.ForceActiveTitleBar = true;
            base.KeyPreview = true;
            base.Controls.SetChildIndex(this.appWorkspace, 0);
            this.appWorkspace.ResumeLayout(false);
            base.ResumeLayout(false);
        }

        private void LoadWindowState()
        {
            FormWindowState state = AppSettings.Instance.Window.Main.FormWindowState.Value;
            if (state != FormWindowState.Minimized)
            {
                if ((state != FormWindowState.Maximized) && AppSettings.Instance.Window.Main.IsBoundsSpecified)
                {
                    base.Bounds = AppSettings.Instance.Window.Main.Bounds.Value.ToGdipRectangle();
                }
                base.WindowState = state;
            }
        }

        private void OnApplicationIdle(object sender, EventArgs e)
        {
            if ((((Startup.State != PaintDotNet.ApplicationState.Closing) && (Startup.State != PaintDotNet.ApplicationState.Exiting)) && (Startup.State != PaintDotNet.ApplicationState.Unknown)) && (!base.IsDisposed && ((this.queuedInstanceMessages.Count > 0) || ((this.singleInstanceManager != null) && this.singleInstanceManager.AreMessagesPending))))
            {
                this.ProcessQueuedInstanceMessages();
            }
        }

        private void OnAppWorkspaceActiveDocumentWorkspaceChanged(object sender, EventArgs e)
        {
            DocumentWorkspace activeDocumentWorkspace = this.appWorkspace.ActiveDocumentWorkspace;
            if (activeDocumentWorkspace != null)
            {
                activeDocumentWorkspace.CanvasLayout += new EventHandler(this.OnDocumentWorkspaceCanvasLayout);
                activeDocumentWorkspace.DocumentChanged += new EventHandler(this.OnDocumentWorkspaceDocumentChanged);
                activeDocumentWorkspace.SaveOptionsChanged += new EventHandler(this.OnDocumentWorkspaceSaveOptionsChanged);
            }
            this.SetTitleText();
            this.ReevaluteFloatingToolFormForceOpaque();
        }

        private void OnAppWorkspaceActiveDocumentWorkspaceChanging(object sender, EventArgs e)
        {
            DocumentWorkspace activeDocumentWorkspace = this.appWorkspace.ActiveDocumentWorkspace;
            if (activeDocumentWorkspace != null)
            {
                activeDocumentWorkspace.CanvasLayout -= new EventHandler(this.OnDocumentWorkspaceCanvasLayout);
                activeDocumentWorkspace.DocumentChanged -= new EventHandler(this.OnDocumentWorkspaceDocumentChanged);
                activeDocumentWorkspace.SaveOptionsChanged -= new EventHandler(this.OnDocumentWorkspaceSaveOptionsChanged);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (this.appWorkspace.ActiveDocumentWorkspace != null)
            {
                this.appWorkspace.ActiveDocumentWorkspace.ClearTool();
            }
            base.OnClosed(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if ((!e.Cancel && (this.appWorkspace != null)) && !this.appWorkspace.IsDisposed)
            {
                CloseAllWorkspacesAction performMe = new CloseAllWorkspacesAction();
                this.appWorkspace.PerformAction(performMe);
                e.Cancel = performMe.Cancelled;
            }
            if (!e.Cancel)
            {
                if (base.Visible)
                {
                    this.SaveSettings();
                }
                if (this.floatingToolForms != null)
                {
                    FloatingToolForm[] floatingToolForms = this.floatingToolForms;
                    for (int i = 0; i < floatingToolForms.Length; i++)
                    {
                        floatingToolForms[i].Hide();
                    }
                }
                base.Hide();
                if (this.queuedInstanceMessages != null)
                {
                    this.queuedInstanceMessages.Clear();
                }
                PaintDotNet.SystemLayer.SingleInstanceManager singleInstanceManager = this.singleInstanceManager;
                this.SingleInstanceManager = null;
                if (singleInstanceManager != null)
                {
                    singleInstanceManager.Dispose();
                    singleInstanceManager = null;
                }
            }
            base.OnClosing(e);
        }

        private void OnDefaultButtonClick(object sender, EventArgs e)
        {
            if (this.appWorkspace.ActiveDocumentWorkspace != null)
            {
                this.appWorkspace.ActiveDocumentWorkspace.Focus();
                if (this.appWorkspace.ActiveDocumentWorkspace.Tool != null)
                {
                    this.appWorkspace.ActiveDocumentWorkspace.Tool.PerformKeyPress(new KeyPressEventArgs('\r'));
                    this.appWorkspace.ActiveDocumentWorkspace.Tool.PerformKeyPress(Keys.Enter);
                }
            }
        }

        private void OnDocumentWorkspaceCanvasLayout(object sender, EventArgs e)
        {
            this.ReevaluteFloatingToolFormForceOpaque();
        }

        private void OnDocumentWorkspaceDocumentChanged(object sender, EventArgs e)
        {
            this.SetTitleText();
            this.OnResize(EventArgs.Empty);
        }

        private void OnDocumentWorkspaceSaveOptionsChanged(object sender, EventArgs e)
        {
            this.SetTitleText();
        }

        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            base.Activate();
            if ((!PdnBaseForm.IsInThreadModalLoop && base.Enabled) && drgevent.Data.GetDataPresent(System.Windows.Forms.DataFormats.FileDrop))
            {
                string[] data = drgevent.Data.GetData(System.Windows.Forms.DataFormats.FileDrop) as string[];
                if (data == null)
                {
                    return;
                }
                string[] fileNames = this.PruneDirectories(data);
                bool flag = true;
                if (fileNames.Length == 0)
                {
                    return;
                }
                if ((fileNames.Length == 1) && (this.appWorkspace.DocumentWorkspaces.Length == 0))
                {
                    flag = false;
                }
                else
                {
                    string str4;
                    string str = (fileNames.Length > 1) ? "Plural" : "Singular";
                    Icon icon = PdnResources.GetImageResource("Icons.DragDrop.OpenOrImport.FormIcon.png").Reference.ToIcon();
                    string str2 = PdnResources.GetString("DragDrop.OpenOrImport.Title");
                    string str3 = PdnResources.GetString("DragDrop.OpenOrImport.InfoText." + str);
                    TaskButton button = new TaskButton(PdnResources.GetImageResource("Icons.MenuFileOpenIcon.png").Reference, PdnResources.GetString("DragDrop.OpenOrImport.OpenButton.ActionText"), PdnResources.GetString("DragDrop.OpenOrImport.OpenButton.ExplanationText." + str));
                    if (this.appWorkspace.DocumentWorkspaces.Length == 0)
                    {
                        str4 = PdnResources.GetString("DragDrop.OpenOrImport.ImportLayers.ExplanationText.NoImagesYet.Plural");
                    }
                    else
                    {
                        str4 = PdnResources.GetString("DragDrop.OpenOrImport.ImportLayers.ExplanationText." + str);
                    }
                    TaskButton button2 = new TaskButton(PdnResources.GetImageResource("Icons.MenuLayersAddNewLayerIcon.png").Reference, PdnResources.GetString("DragDrop.OpenOrImport.ImportLayers.ActionText." + str), str4);
                    TaskButton button3 = new TaskButton(PdnResources.GetImageResource("Icons.CancelIcon.png").Reference, PdnResources.GetString("TaskButton.Cancel.ActionText"), PdnResources.GetString("TaskButton.Cancel.ExplanationText"));
                    TaskDialog dialog2 = new TaskDialog {
                        Icon = icon,
                        Title = str2,
                        ScaleTaskImageWithDpi = false,
                        IntroText = str3
                    };
                    dialog2.TaskButtons = new TaskButton[] { button, button2, button3 };
                    dialog2.CancelButton = button3;
                    TaskButton button4 = dialog2.Show(this);
                    if (button4 == button)
                    {
                        flag = false;
                    }
                    else if (button4 == button2)
                    {
                        flag = true;
                    }
                    else
                    {
                        return;
                    }
                }
                if (!flag)
                {
                    this.appWorkspace.OpenFilesInNewWorkspace(fileNames);
                }
                else
                {
                    if (this.appWorkspace.ActiveDocumentWorkspace == null)
                    {
                        SizeInt32 newDocumentSize = this.appWorkspace.GetNewDocumentSize();
                        this.appWorkspace.CreateBlankDocumentInNewWorkspace(newDocumentSize, Document.DefaultDpuUnit, Document.GetDefaultDpu(Document.DefaultDpuUnit), false);
                    }
                    HistoryMemento memento = new ImportFromFileAction().ImportMultipleFiles(this.appWorkspace.ActiveDocumentWorkspace, fileNames);
                    if (memento != null)
                    {
                        this.appWorkspace.ActiveDocumentWorkspace.History.PushNewMemento(memento);
                    }
                }
            }
            base.OnDragDrop(drgevent);
        }

        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            if (base.Enabled && drgevent.Data.GetDataPresent(System.Windows.Forms.DataFormats.FileDrop))
            {
                string[] data = (string[]) drgevent.Data.GetData(System.Windows.Forms.DataFormats.FileDrop);
                if (data != null)
                {
                    foreach (string str in data)
                    {
                        try
                        {
                            if ((File.GetAttributes(str) & FileAttributes.Directory) == 0)
                            {
                                drgevent.Effect = DragDropEffects.Copy;
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
            base.OnDragEnter(drgevent);
        }

        private void OnFloatingToolFormMoving(object sender, EventArgs e)
        {
            this.ReevaluteFloatingToolFormForceOpaque();
        }

        private void OnGlassButtonFootersValueChangedT(object sender, ValueChangedEventArgs<bool> e)
        {
            PdnBaseForm.EnableAutoGlass = e.NewValue;
        }

        protected override void OnHelpRequested(HelpEventArgs hevent)
        {
            hevent.Handled = true;
            base.OnHelpRequested(hevent);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            bool isGlassSupported = base.IsGlassSupported;
            this.appWorkspace.DrawCaptionArea = isGlassSupported;
            if (base.IsHandleCreated)
            {
                Padding padding = new Padding(0, 0, 0, 0);
                if (isGlassSupported && !base.ExtendClientIntoFrame)
                {
                    base.ExtendClientIntoFrame = true;
                }
                else if (!isGlassSupported && base.ExtendClientIntoFrame)
                {
                    base.ExtendClientIntoFrame = false;
                }
                if ((this.appWorkspace > null) & isGlassSupported)
                {
                    base.IsGlassDesired = true;
                    Padding glassInset = this.appWorkspace.GlassInset;
                    Padding padding3 = new Padding(padding.Left + glassInset.Left, padding.Top + glassInset.Top, padding.Right + glassInset.Right, padding.Bottom + glassInset.Bottom);
                    base.GlassInset = padding3;
                    Padding glassCaptionDragInset = this.appWorkspace.GlassCaptionDragInset;
                    Padding padding5 = new Padding(padding.Left + glassCaptionDragInset.Left, padding.Top + glassCaptionDragInset.Top, padding.Right + glassCaptionDragInset.Right, padding.Bottom + glassCaptionDragInset.Bottom);
                    base.GlassCaptionDragInset = new Padding?(padding5);
                    IMessageFilter glassWndProcFilter = base.GetGlassWndProcFilter();
                    this.appWorkspace.SetGlassWndProcFilter(glassWndProcFilter);
                }
                else
                {
                    base.IsGlassDesired = false;
                    base.GlassInset = new Padding(0);
                    this.appWorkspace.SetGlassWndProcFilter(null);
                }
            }
            base.OnLayout(levent);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.EnsureFormIsOnScreen();
            this.floatingToolForms = new FloatingToolForm[] { this.appWorkspace.Widgets.ToolsForm, this.appWorkspace.Widgets.ColorsForm, this.appWorkspace.Widgets.HistoryForm, this.appWorkspace.Widgets.LayersForm };
            foreach (FloatingToolForm form in this.floatingToolForms)
            {
                form.Closing += new CancelEventHandler(this.FloatingToolFormHideInsteadOfCloseHandler);
                form.Moving += new MovingEventHandler(this.OnFloatingToolFormMoving);
                form.Move += new EventHandler(this.OnFloatingToolFormMoving);
            }
            this.PositionFloatingForms();
            base.OnLoad(e);
        }

        protected override void OnNonClientActivated()
        {
            if (this.appWorkspace != null)
            {
                this.appWorkspace.OnParentNonClientActivated();
            }
            base.OnNonClientActivated();
        }

        protected override void OnNonClientDeactivate()
        {
            if (this.appWorkspace != null)
            {
                this.appWorkspace.OnParentNonClientDeactivate();
            }
            base.OnNonClientDeactivate();
        }

        protected override void OnQueryEndSession(CancelEventArgs e)
        {
            if (!PdnBaseForm.IsInThreadModalLoop)
            {
                this.OnClosing(e);
            }
            else
            {
                foreach (Form form in Application.OpenForms)
                {
                    PdnBaseForm form2 = form as PdnBaseForm;
                    if (form2 != null)
                    {
                        form2.Flash();
                    }
                }
                e.Cancel = true;
            }
            base.OnQueryEndSession(e);
        }

        protected override void OnShown(EventArgs e)
        {
            this.SetTitleText();
            base.OnShown(e);
            if (PdnInfo.IsExpired)
            {
                foreach (Form form in Application.OpenForms)
                {
                    form.Enabled = false;
                }
                TaskButton button = new TaskButton(PdnResources.GetImageResource("Icons.MenuUtilitiesCheckForUpdatesIcon.png").Reference, PdnResources.GetString("ExpiredTaskDialog.CheckForUpdatesTB.ActionText"), PdnResources.GetString("ExpiredTaskDialog.CheckForUpdatesTB.ExplanationText"));
                TaskButton button2 = new TaskButton(PdnResources.GetImageResource("Icons.MenuHelpPdnWebsiteIcon.png").Reference, PdnResources.GetString("ExpiredTaskDialog.GoToWebSiteTB.ActionText"), PdnResources.GetString("ExpiredTaskDialog.GoToWebSiteTB.ExplanationText"));
                TaskButton button3 = new TaskButton(PdnResources.GetImageResource("Icons.CancelIcon.png").Reference, PdnResources.GetString("ExpiredTaskDialog.DoNotCheckForUpdatesTB.ActionText"), PdnResources.GetString("ExpiredTaskDialog.DoNotCheckForUpdatesTB.ExplanationText"));
                TaskButton[] buttonArray = new TaskButton[] { button, button2, button3 };
                TaskDialog dialog1 = new TaskDialog {
                    Icon = base.Icon,
                    Title = PdnInfo.FullAppName,
                    TaskImage = PdnResources.GetImageResource("Icons.WarningIcon.png").Reference,
                    ScaleTaskImageWithDpi = true,
                    IntroText = PdnResources.GetString("ExpiredTaskDialog.InfoText"),
                    TaskButtons = buttonArray,
                    AcceptButton = button,
                    CancelButton = button3,
                    PixelWidth96Dpi = 450
                };
                TaskButton button4 = dialog1.Show(this);
                if (button4 == button)
                {
                    UpdatesService.Instance.PerformUpdateCheck();
                }
                else if (button4 == button2)
                {
                    PdnInfo.LaunchWebSite(this, "redirect/pdnexpired.html");
                }
                base.Close();
            }
            if (this.appWorkspace.ActiveDocumentWorkspace != null)
            {
                this.appWorkspace.ActiveDocumentWorkspace.Focus();
            }
            else
            {
                this.appWorkspace.Focus();
            }
        }

        private void OnSingleInstanceManagerInstanceMessageReceived(object sender, EventArgs e)
        {
            base.BeginInvoke(new Action(this.ProcessQueuedInstanceMessages), null);
        }

        private void OnTranslucentWindowsValueChangedT(object sender, ValueChangedEventArgs<bool> e)
        {
            PdnBaseForm.EnableOpacity = e.NewValue;
        }

        private void PositionFloatingForms()
        {
            this.appWorkspace.ResetFloatingForms();
            try
            {
                this.SnapManager.Load();
            }
            catch (Exception)
            {
                this.appWorkspace.ResetFloatingForms();
            }
            foreach (FloatingToolForm form in this.floatingToolForms)
            {
                base.AddOwnedForm(form);
            }
            if (AppSettings.Instance.Window.Tools.IsVisible.Value)
            {
                this.appWorkspace.Widgets.ToolsForm.Show();
            }
            if (AppSettings.Instance.Window.History.IsVisible.Value)
            {
                this.appWorkspace.Widgets.HistoryForm.Show();
            }
            if (AppSettings.Instance.Window.Layers.IsVisible.Value)
            {
                this.appWorkspace.Widgets.LayersForm.Show();
            }
            if (AppSettings.Instance.Window.Colors.IsVisible.Value)
            {
                this.appWorkspace.Widgets.ColorsForm.Show();
            }
            System.Windows.Forms.Screen[] allScreens = System.Windows.Forms.Screen.AllScreens;
            foreach (FloatingToolForm form2 in this.floatingToolForms)
            {
                if (form2.Visible)
                {
                    bool flag = false;
                    try
                    {
                        bool flag2 = false;
                        foreach (System.Windows.Forms.Screen screen in allScreens)
                        {
                            Rectangle rectangle = Rectangle.Intersect(screen.Bounds, form2.Bounds);
                            if ((rectangle.Width > 0) && (rectangle.Height > 0))
                            {
                                flag2 = true;
                                break;
                            }
                        }
                        if (!flag2)
                        {
                            flag = true;
                        }
                    }
                    catch (Exception)
                    {
                        flag = true;
                    }
                    if (flag)
                    {
                        this.appWorkspace.ResetFloatingForm(form2);
                    }
                }
            }
        }

        private bool ProcessMessage(string message)
        {
            ArgumentAction action;
            string str;
            if (base.IsDisposed)
            {
                return false;
            }
            bool flag = this.SplitMessage(message, out action, out str);
            if (!flag)
            {
                return true;
            }
            switch (action)
            {
                case ArgumentAction.Open:
                    if (!this.processingOpen)
                    {
                        base.Activate();
                        bool isCurrentModalForm = base.IsCurrentModalForm;
                        bool enabled = base.Enabled;
                        if (!(isCurrentModalForm & enabled))
                        {
                            return flag;
                        }
                        this.processingOpen = true;
                        try
                        {
                            return this.appWorkspace.OpenFileInNewWorkspace(str);
                        }
                        finally
                        {
                            this.processingOpen = false;
                        }
                        break;
                    }
                    Work.QueueWorkItem(delegate {
                        Thread.Sleep(150);
                        this.BeginInvoke(() => this.singleInstanceManager.SendInstanceMessage(message));
                    });
                    return true;

                case ArgumentAction.OpenUntitled:
                    break;

                case ArgumentAction.Print:
                    base.Activate();
                    if ((!string.IsNullOrEmpty(str) && base.IsCurrentModalForm) && base.Enabled)
                    {
                        flag = this.appWorkspace.OpenFileInNewWorkspace(str);
                        if (!flag)
                        {
                            return flag;
                        }
                        DocumentWorkspace activeDocumentWorkspace = this.appWorkspace.ActiveDocumentWorkspace;
                        PrintAction action2 = new PrintAction();
                        activeDocumentWorkspace.PerformAction(action2);
                        CloseWorkspaceAction performMe = new CloseWorkspaceAction(activeDocumentWorkspace);
                        this.appWorkspace.PerformAction(performMe);
                        if (this.appWorkspace.DocumentWorkspaces.Length == 0)
                        {
                            Startup.CloseApplication();
                        }
                    }
                    return flag;

                case ArgumentAction.NoOp:
                    return true;

                default:
                    throw ExceptionUtil.InvalidEnumArgumentException<ArgumentAction>(action, "action");
            }
            base.Activate();
            if ((!string.IsNullOrEmpty(str) && base.IsCurrentModalForm) && base.Enabled)
            {
                DocumentWorkspace workspace;
                flag = this.appWorkspace.OpenFileInNewWorkspace(str, false, out workspace);
                if (flag)
                {
                    workspace.SetDocumentSaveOptions(null, null, null);
                    workspace.Document.Dirty = true;
                }
            }
            return flag;
        }

        private void ProcessQueuedInstanceMessages()
        {
            if (!base.IsDisposed && ((base.IsHandleCreated && !PdnInfo.IsExpired) && (this.singleInstanceManager != null)))
            {
                string[] pendingInstanceMessages = this.singleInstanceManager.GetPendingInstanceMessages();
                string[] strArray2 = this.queuedInstanceMessages.ToArrayEx<string>();
                this.queuedInstanceMessages.Clear();
                string[] strArray3 = new string[pendingInstanceMessages.Length + strArray2.Length];
                for (int i = 0; i < pendingInstanceMessages.Length; i++)
                {
                    strArray3[i] = pendingInstanceMessages[i];
                }
                for (int j = 0; j < strArray2.Length; j++)
                {
                    strArray3[j + pendingInstanceMessages.Length] = strArray2[j];
                }
                foreach (string str in strArray3)
                {
                    if (!this.ProcessMessage(str))
                    {
                        break;
                    }
                }
            }
        }

        private string[] PruneDirectories(string[] fileNames)
        {
            List<string> items = new List<string>();
            foreach (string str in fileNames)
            {
                try
                {
                    if ((File.GetAttributes(str) & FileAttributes.Directory) == 0)
                    {
                        items.Add(str);
                    }
                }
                catch (Exception)
                {
                }
            }
            return items.ToArrayEx<string>();
        }

        private void ReevaluteFloatingToolFormForceOpaque()
        {
            this.VerifyThreadAccess();
            if (this.floatingToolForms != null)
            {
                foreach (FloatingToolForm form in this.floatingToolForms)
                {
                    this.ReevaluteFloatingToolFormForceOpaque(form);
                }
            }
        }

        private void ReevaluteFloatingToolFormForceOpaque(FloatingToolForm ftForm)
        {
            bool flag;
            this.VerifyThreadAccess();
            DocumentWorkspace activeDocumentWorkspace = this.appWorkspace.ActiveDocumentWorkspace;
            if (activeDocumentWorkspace == null)
            {
                flag = false;
            }
            else
            {
                RectDouble visibleDocumentBounds = activeDocumentWorkspace.VisibleDocumentBounds;
                if (ftForm.Bounds.ToRectDouble().IntersectsWith(visibleDocumentBounds))
                {
                    flag = false;
                }
                else
                {
                    flag = true;
                }
            }
            ftForm.ForceOpaque = flag;
        }

        private void SaveSettings()
        {
            AppSettings.Instance.UI.TranslucentWindows.Value = PdnBaseForm.EnableOpacity;
            AppSettings.Instance.UI.GlassButtonFooters.Value = PdnBaseForm.EnableAutoGlass;
            AppSettings.Instance.Window.Main.FormWindowState.Value = base.WindowState;
            AppSettings.Instance.Window.Main.Bounds.Value = UIUtil.GetWindowNormalBoundsFromHandle(base.Handle);
            if (base.WindowState != FormWindowState.Minimized)
            {
                AppSettings.Instance.Window.Tools.IsVisible.Value = this.appWorkspace.Widgets.ToolsForm.Visible;
                AppSettings.Instance.Window.History.IsVisible.Value = this.appWorkspace.Widgets.HistoryForm.Visible;
                AppSettings.Instance.Window.Layers.IsVisible.Value = this.appWorkspace.Widgets.LayersForm.Visible;
                AppSettings.Instance.Window.Colors.IsVisible.Value = this.appWorkspace.Widgets.ColorsForm.Visible;
            }
            this.SnapManager.Save();
            this.appWorkspace.SaveSettings();
        }

        private void SetTitleText()
        {
            if ((this.appWorkspace == null) || (this.appWorkspace.ActiveDocumentWorkspace == null))
            {
                this.Text = PdnInfo.AppName;
            }
            else
            {
                string windowTitle = this.appWorkspace.ActiveDocumentWorkspace.GetWindowTitle();
                this.Text = windowTitle;
            }
            this.appWorkspace.InvalidateTitle();
        }

        private bool SplitMessage(string message, out ArgumentAction action, out string actionParm)
        {
            if (message.Length == 0)
            {
                action = ArgumentAction.NoOp;
                actionParm = null;
                return false;
            }
            if (message.IndexOf("print:") == 0)
            {
                action = ArgumentAction.Print;
                actionParm = message.Substring("print:".Length);
                return true;
            }
            if (message.IndexOf("untitled:") == 0)
            {
                action = ArgumentAction.OpenUntitled;
                actionParm = message.Substring("untitled:".Length);
                return true;
            }
            action = ArgumentAction.Open;
            actionParm = message;
            return true;
        }

        protected override void WndProc(ref Message m)
        {
            if (this.singleInstanceManager != null)
            {
                this.singleInstanceManager.FilterMessage(ref m);
            }
            base.WndProc(ref m);
        }

        protected override Size DefaultSize =>
            new Size(950, 0x2e2);

        public PaintDotNet.SystemLayer.SingleInstanceManager SingleInstanceManager
        {
            get => 
                this.singleInstanceManager;
            set
            {
                if (this.singleInstanceManager != null)
                {
                    this.singleInstanceManager.InstanceMessageReceived -= new EventHandler(this.OnSingleInstanceManagerInstanceMessageReceived);
                    this.singleInstanceManager.SetWindow(null);
                }
                this.singleInstanceManager = value;
                if (this.singleInstanceManager != null)
                {
                    this.singleInstanceManager.SetWindow(this);
                    this.singleInstanceManager.InstanceMessageReceived += new EventHandler(this.OnSingleInstanceManagerInstanceMessageReceived);
                }
            }
        }

        public PaintDotNet.Snap.SnapManager SnapManager
        {
            get
            {
                this.VerifyThreadAccess();
                if (this.snapManager == null)
                {
                    this.snapManager = new PaintDotNet.Snap.SnapManager();
                }
                return this.snapManager;
            }
        }

        private enum ArgumentAction
        {
            Open,
            OpenUntitled,
            Print,
            NoOp
        }
    }
}

