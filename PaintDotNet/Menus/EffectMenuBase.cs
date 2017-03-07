namespace PaintDotNet.Menus
{
    using PaintDotNet;
    using PaintDotNet.Actions;
    using PaintDotNet.AppModel;
    using PaintDotNet.Concurrency;
    using PaintDotNet.Controls;
    using PaintDotNet.Dialogs;
    using PaintDotNet.Drawing;
    using PaintDotNet.Effects;
    using PaintDotNet.Functional;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.Runtime;
    using PaintDotNet.Settings.App;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Threading;
    using PaintDotNet.Threading.Tasks;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;

    internal abstract class EffectMenuBase : PdnMenuItem
    {
        private Container components;
        private const int effectRefreshInterval = 0x10;
        private EffectsCollection effects;
        private Dictionary<System.Type, EffectConfigToken> effectTokens = new Dictionary<System.Type, EffectConfigToken>();
        private System.Windows.Forms.Timer invalidateTimer;
        private Image lastEffectImage;
        private string lastEffectName;
        private EffectConfigToken lastEffectToken;
        private System.Type lastEffectType;
        private bool menuPopulated;
        private PdnRegion[] progressRegions;
        private int progressRegionsStartIndex;
        private readonly int renderingThreadCount = Math.Max(2, WorkItemDispatcher.Default.MaxThreadCount);
        private PdnMenuItem sentinel;
        private bool showProgressInStatusBar;
        private const int tilesPerCpu = 0x19;

        public EffectMenuBase()
        {
            this.InitializeComponent();
        }

        private void AddEffectsToMenu()
        {
            EffectsCollection effects = this.Effects;
            System.Type[] typeArray = effects.Effects;
            bool enableEffectShortcuts = this.EnableEffectShortcuts;
            List<Effect> list = new List<Effect>();
            foreach (System.Type type in effects.Effects)
            {
                try
                {
                    Effect effect = (Effect) type.GetConstructor(System.Type.EmptyTypes).Invoke(null);
                    if (this.FilterEffects(effect))
                    {
                        list.Add(effect);
                    }
                }
                catch (Exception exception)
                {
                    base.AppWorkspace.GetService<IPluginErrorService>().ReportEffectLoadError(type.Assembly, type, exception);
                }
            }
            list.Sort((Comparison<Effect>) ((lhs, rhs) => string.Compare(lhs.Name, rhs.Name, true)));
            List<string> list2 = new List<string>();
            foreach (Effect effect2 in list)
            {
                if (!string.IsNullOrEmpty(effect2.SubMenuName))
                {
                    list2.Add(effect2.SubMenuName);
                }
            }
            list2.Sort((Comparison<string>) ((lhs, rhs) => string.Compare(lhs, rhs, true)));
            string str = null;
            foreach (string str2 in list2)
            {
                if (str2 != str)
                {
                    PdnMenuItem item = new PdnMenuItem(str2, null, null);
                    base.DropDownItems.Add(item);
                    str = str2;
                }
            }
            foreach (Effect effect3 in list)
            {
                this.AddEffectToMenu(effect3, enableEffectShortcuts);
                effect3.Dispose();
            }
        }

        private void AddEffectToMenu(Effect effect, bool withShortcut)
        {
            System.Type effectType;
            if (this.FilterEffects(effect))
            {
                Image image;
                string name = effect.Name;
                if (effect.CheckForEffectFlags(EffectFlags.Configurable))
                {
                    name = string.Format(PdnResources.GetString("Effects.Name.Format.Configurable"), name);
                }
                if (effect.Image == null)
                {
                    image = null;
                }
                else
                {
                    try
                    {
                        image = effect.Image.CloneT<Image>();
                    }
                    catch (Exception)
                    {
                        image = null;
                    }
                }
                PdnMenuItem item = new PdnMenuItem(name, image, new EventHandler(this.OnEffectMenuItemClick));
                if (withShortcut)
                {
                    item.ShortcutKeys = this.GetEffectShortcutKeys(effect);
                }
                else
                {
                    item.ShortcutKeys = Keys.None;
                }
                effectType = effect.GetType();
                item.Tag = effectType;
                item.Name = "Effect(" + effect.GetType().FullName + ")";
                item.IsPlugin = !this.IsBuiltInEffect(effect);
                if (item.IsPlugin)
                {
                    Result<string> result;
                    Result<Version> result2;
                    Result<string> result3;
                    Result<string> result4;
                    Result<Uri> result5;
                    string location = effectType.Assembly.Location;
                    Result<IPluginSupportInfo> pluginSupportInfo = () => PluginSupportInfo.GetPluginSupportInfo(effectType).Eval<IPluginSupportInfo>();
                    if (pluginSupportInfo.IsValue && (pluginSupportInfo.Value != null))
                    {
                        result = () => pluginSupportInfo.Value.DisplayName.Eval<string>();
                        result2 = () => pluginSupportInfo.Value.Version.Eval<Version>();
                        result3 = () => pluginSupportInfo.Value.Author.Eval<string>();
                        result4 = () => pluginSupportInfo.Value.Copyright.Eval<string>();
                        result5 = () => pluginSupportInfo.Value.WebsiteUri.Eval<Uri>();
                    }
                    else
                    {
                        result = null;
                        result2 = null;
                        result3 = null;
                        result4 = null;
                        result5 = null;
                    }
                    StringBuilder builder = new StringBuilder();
                    if (((result != null) && result.IsValue) && !string.IsNullOrWhiteSpace(result.Value))
                    {
                        if (((result2 != null) && result2.IsValue) && (result2.Value != null))
                        {
                            builder.AppendLine(string.Format(PdnResources.GetString("Effect.PluginToolTip.DisplayNameAndVersion.Format"), result.Value, result2.Value.ToString()));
                        }
                        else
                        {
                            builder.AppendLine(string.Format(PdnResources.GetString("Effect.PluginToolTip.DisplayName.Format"), result.Value));
                        }
                    }
                    else if (((result2 != null) && result2.IsValue) && (result2.Value != null))
                    {
                        builder.AppendLine(string.Format(PdnResources.GetString("Effect.PluginToolTip.Version.Format"), result2.Value.ToString(4)));
                    }
                    if (((result3 != null) && result3.IsValue) && !string.IsNullOrWhiteSpace(result3.Value))
                    {
                        builder.AppendLine(string.Format(PdnResources.GetString("Effect.PluginToolTip.Author.Format"), result3.Value));
                    }
                    if (((result4 != null) && result4.IsValue) && !string.IsNullOrWhiteSpace(result4.Value))
                    {
                        builder.AppendLine(result4.Value);
                    }
                    if (((result5 != null) && result5.IsValue) && (result5.Value != null))
                    {
                        builder.AppendLine(result5.Value.ToString());
                    }
                    if (!string.IsNullOrWhiteSpace(location))
                    {
                        builder.AppendLine(string.Format(PdnResources.GetString("Effect.PluginToolTip.Location.Format"), location));
                    }
                    if (builder.Length > 0)
                    {
                        item.ToolTipText = builder.ToString();
                    }
                }
                PdnMenuItem item2 = this;
                if (effect.SubMenuName != null)
                {
                    PdnMenuItem item3 = null;
                    foreach (ToolStripItem item4 in base.DropDownItems)
                    {
                        PdnMenuItem item5 = item4 as PdnMenuItem;
                        if ((item5 != null) && (item5.Text == effect.SubMenuName))
                        {
                            item3 = item5;
                            break;
                        }
                    }
                    if (item3 == null)
                    {
                        item3 = new PdnMenuItem(effect.SubMenuName, null, null);
                        base.DropDownItems.Add(item3);
                    }
                    item2 = item3;
                }
                item2.DropDownItems.Add(item);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.lastEffectImage != null)
                {
                    this.lastEffectImage.Dispose();
                    this.lastEffectImage = null;
                }
                if (this.components != null)
                {
                    this.components.Dispose();
                    this.components = null;
                }
            }
            base.Dispose(disposing);
        }

        private bool DoEffect(Effect effect, EffectConfigToken token, PdnRegion selectedRegion, PdnRegion regionToRender, IRenderer<ColorAlpha8> clipMaskRenderer, Surface originalSurface, out Exception exception)
        {
            exception = null;
            DocumentWorkspace activeDocumentWorkspace = base.AppWorkspace.ActiveDocumentWorkspace;
            bool dirty = activeDocumentWorkspace.Document.Dirty;
            bool flag2 = false;
            try
            {
                VirtualTask<Unit> renderTask = activeDocumentWorkspace.TaskManager.CreateVirtualTask(TaskState.NotYetRunning);
                using (TaskProgressDialog progressDialog = new TaskProgressDialog())
                {
                    if (effect.Image != null)
                    {
                        progressDialog.Icon = effect.Image.ToIcon();
                    }
                    progressDialog.Text = effect.Name;
                    string str = PdnResources.GetString("Effects.ApplyingDialog.Description");
                    string renderintTextPercentFormat = PdnResources.GetString("Effects.ApplyingDialog.Description.WithPercent.Format");
                    string cancelingText = PdnResources.GetString("TaskProgressDialog.Canceling.Text");
                    string str2 = PdnResources.GetString("TaskProgressDialog.Initializing.Text");
                    progressDialog.HeaderText = str2;
                    this.showProgressInStatusBar = false;
                    this.invalidateTimer.Enabled = true;
                    using (new WaitCursorChanger(base.AppWorkspace))
                    {
                        HistoryMemento memento = null;
                        DialogResult none = DialogResult.None;
                        try
                        {
                            <>c__DisplayClass47_3 class_2;
                            <>c__DisplayClass47_5 class_4;
                            ManualResetEvent saveEvent = new ManualResetEvent(false);
                            BitmapHistoryMemento bha = null;
                            GeometryList selectedGeometry = GeometryList.FromNonOverlappingScans(from r in selectedRegion.GetRegionScansReadOnlyInt() select r.ToRectInt32());
                            Work.QueueWorkItem(delegate {
                                try
                                {
                                    ImageResource resource;
                                    if (effect.Image == null)
                                    {
                                        resource = null;
                                    }
                                    else
                                    {
                                        resource = ImageResource.FromImage(effect.Image);
                                    }
                                    bha = new BitmapHistoryMemento(effect.Name, resource, this.AppWorkspace.ActiveDocumentWorkspace, this.AppWorkspace.ActiveDocumentWorkspace.ActiveLayerIndex, selectedGeometry.EnumerateInteriorScans(), originalSurface);
                                }
                                finally
                                {
                                    saveEvent.Set();
                                    selectedGeometry = null;
                                }
                            });
                            BackgroundEffectRenderer ber = new BackgroundEffectRenderer(effect, token, new RenderArgs(((BitmapLayer) base.AppWorkspace.ActiveDocumentWorkspace.ActiveLayer).Surface), new RenderArgs(originalSurface), regionToRender, clipMaskRenderer, 0x19 * this.renderingThreadCount, this.renderingThreadCount);
                            int tileCount = 0;
                            ber.RenderedTile += delegate (object s, RenderedTileEventArgs e) {
                                progressDialog.Dispatcher.BeginTry(delegate {
                                    if (!renderTask.IsCancelRequested)
                                    {
                                        tileCount += 1;
                                        double num = ((double) (tileCount + 1)) / ((double) e.TileCount);
                                        renderTask.Progress = new double?(num.Clamp(0.0, 1.0));
                                        int num2 = (int) (100.0 * num);
                                        string str = string.Format(renderintTextPercentFormat, num2);
                                        progressDialog.HeaderText = str;
                                    }
                                }).Observe();
                            };
                            ber.RenderedTile += new RenderedTileEventHandler(this.RenderedTileHandler);
                            ber.StartingRendering += new EventHandler(this.StartingRenderingHandler);
                            ber.FinishedRendering += new EventHandler(class_2.<DoEffect>b__4);
                            ber.FinishedRendering += new EventHandler(this.FinishedRenderingHandler);
                            renderTask.CancelRequested += delegate (object sender, EventArgs e) {
                                ber.AbortAsync();
                                progressDialog.Dispatcher.BeginTry(new Action(class_4.<DoEffect>b__6)).Observe();
                            };
                            renderTask.SetState(TaskState.Running);
                            progressDialog.Shown += delegate (object s, EventArgs e) {
                                ber.Start();
                            };
                            progressDialog.Task = renderTask;
                            progressDialog.CloseOnFinished = true;
                            progressDialog.ShowDialog(base.AppWorkspace);
                            if (!renderTask.IsCancelRequested)
                            {
                                none = DialogResult.OK;
                            }
                            else
                            {
                                none = DialogResult.Cancel;
                                flag2 = true;
                                using (new WaitCursorChanger(base.AppWorkspace))
                                {
                                    try
                                    {
                                        ber.Abort();
                                        ber.Join();
                                    }
                                    catch (Exception exception2)
                                    {
                                        exception = exception2;
                                    }
                                    try
                                    {
                                        if (originalSurface.Scan0.MaySetAllowWrites)
                                        {
                                            originalSurface.Scan0.AllowWrites = true;
                                        }
                                    }
                                    catch (Exception)
                                    {
                                    }
                                }
                                originalSurface.Parallelize<ColorBgra>(TilingStrategy.HorizontalSlices, 7, WorkItemQueuePriority.Normal).Render(((BitmapLayer) base.AppWorkspace.ActiveDocumentWorkspace.ActiveLayer).Surface, PointInt32.Zero);
                            }
                            this.invalidateTimer.Enabled = false;
                            try
                            {
                                ber.Join();
                            }
                            catch (Exception exception3)
                            {
                                exception = exception3;
                            }
                            ber.Dispose();
                            this.WaitWithUI(base.AppWorkspace, effect, WaitWithUIType.Finishing, saveEvent);
                            saveEvent.Close();
                            saveEvent = null;
                            memento = bha;
                        }
                        catch (Exception)
                        {
                            using (new WaitCursorChanger(base.AppWorkspace))
                            {
                                Surface dst = ((BitmapLayer) base.AppWorkspace.ActiveDocumentWorkspace.ActiveLayer).Surface;
                                originalSurface.Parallelize<ColorBgra>(TilingStrategy.HorizontalSlices, 7, WorkItemQueuePriority.Normal).Render<ColorBgra>(dst);
                                memento = null;
                            }
                        }
                        base.AppWorkspace.ActiveDocumentWorkspace.ActiveLayer.Invalidate(RectInt32.Inflate(selectedRegion.GetBoundsRectInt32(), 1, 1));
                        using (new WaitCursorChanger(base.AppWorkspace))
                        {
                            if (none == DialogResult.OK)
                            {
                                if (memento != null)
                                {
                                    base.AppWorkspace.ActiveDocumentWorkspace.History.PushNewMemento(memento);
                                }
                                base.AppWorkspace.Update();
                                return true;
                            }
                            CleanupManager.RequestCleanup();
                        }
                    }
                }
            }
            finally
            {
                if (flag2)
                {
                    base.AppWorkspace.ActiveDocumentWorkspace.Document.Dirty = dirty;
                }
                this.invalidateTimer.Enabled = false;
            }
            return false;
        }

        protected abstract bool FilterEffects(Effect effect);
        private void FinishedRenderingHandler(object sender, EventArgs e)
        {
            if (base.AppWorkspace.InvokeRequired)
            {
                object[] args = new object[] { sender, e };
                base.AppWorkspace.BeginInvoke(new EventHandler(this.FinishedRenderingHandler), args);
            }
        }

        private static EffectsCollection GatherEffects()
        {
            bool flag;
            List<Assembly> assemblies = new List<Assembly> {
                Assembly.GetAssembly(typeof(Effect))
            };
            string path = Path.Combine(PdnInfo.ApplicationDir, "Effects");
            try
            {
                flag = Directory.Exists(path);
            }
            catch (Exception)
            {
                flag = false;
            }
            if (flag)
            {
                string searchPattern = "*.dll";
                foreach (string str4 in Directory.GetFiles(path, searchPattern))
                {
                    Assembly item = null;
                    try
                    {
                        item = Assembly.LoadFrom(str4);
                        assemblies.Add(item);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            return new EffectsCollection(assemblies);
        }

        private static SelectionRenderingQuality GetEffectiveSelectionRenderingQuality(Effect effect, AppSettings.ToolsSection toolSettings)
        {
            if (effect.CheckForEffectFlags(EffectFlags.ForceAliasedSelectionQuality))
            {
                return SelectionRenderingQuality.Aliased;
            }
            return toolSettings.Selection.RenderingQuality.Value;
        }

        protected virtual Keys GetEffectShortcutKeys(Effect effect) => 
            Keys.None;

        private void HandleEffectException(AppWorkspace appWorkspace, Effect effect, Exception ex)
        {
            try
            {
                base.AppWorkspace.Widgets.StatusBarProgress.ResetProgressStatusBar();
                base.AppWorkspace.Widgets.StatusBarProgress.EraseProgressStatusBar();
            }
            catch (Exception)
            {
            }
            if (this.IsBuiltInEffect(effect))
            {
                throw new ApplicationException("Effect threw an exception", ex);
            }
            Icon icon = PdnResources.GetImageResource("Icons.BugWarning.png").Reference.ToIcon();
            string str = PdnResources.GetString("Effect.PluginErrorDialog.Title");
            Image image = null;
            string str2 = PdnResources.GetString("Effect.PluginErrorDialog.IntroText");
            TaskButton button = new TaskButton(PdnResources.GetImageResource("Icons.RightArrowBlue.png").Reference, PdnResources.GetString("Effect.PluginErrorDialog.RestartTB.ActionText"), PdnResources.GetString("Effect.PluginErrorDialog.RestartTB.ExplanationText"));
            TaskButton button2 = new TaskButton(PdnResources.GetImageResource("Icons.WarningIcon.png").Reference, PdnResources.GetString("Effect.PluginErrorDialog.DoNotRestartTB.ActionText"), PdnResources.GetString("Effect.PluginErrorDialog.DoNotRestartTB.ExplanationText"));
            string str3 = PdnResources.GetString("Effect.PluginErrorDialog.AuxButton1.Text");
            Action auxButtonClickHandler = delegate {
                using (PdnBaseForm form = new PdnBaseForm())
                {
                    form.Name = "EffectCrash";
                    TextBox box = new TextBox();
                    form.Icon = PdnResources.GetImageResource("Icons.WarningIcon.png").Reference.ToIcon();
                    form.Text = PdnResources.GetString("Effect.PluginErrorDialog.Title");
                    box.Dock = DockStyle.Fill;
                    box.ReadOnly = true;
                    box.Multiline = true;
                    PluginErrorInfo errorInfo = new PluginErrorInfo(effect.GetType().Assembly, effect.GetType(), ex);
                    string localizedEffectErrorMessage = this.AppWorkspace.GetService<IPluginErrorService>().GetLocalizedEffectErrorMessage(errorInfo);
                    box.Font = new Font(FontFamily.GenericMonospace, box.Font.Size);
                    box.Text = localizedEffectErrorMessage;
                    box.ScrollBars = ScrollBars.Vertical;
                    form.StartPosition = FormStartPosition.CenterParent;
                    form.ShowInTaskbar = false;
                    form.MinimizeBox = false;
                    form.Controls.Add(box);
                    form.Width = UIUtil.ScaleWidth(700);
                    form.ShowDialog();
                }
            };
            TaskAuxButton button3 = new TaskAuxButton {
                Text = str3
            };
            button3.Clicked += (s, e) => auxButtonClickHandler();
            TaskDialog dialog2 = new TaskDialog {
                Icon = icon,
                Title = str,
                TaskImage = image,
                IntroText = str2
            };
            dialog2.TaskButtons = new TaskButton[] { button, button2 };
            dialog2.AcceptButton = button;
            dialog2.CancelButton = button2;
            dialog2.PixelWidth96Dpi = TaskDialog.DefaultPixelWidth96Dpi * 2;
            dialog2.AuxControls = new TaskAuxControl[] { button3 };
            TaskDialog dialog = dialog2;
            if (dialog.Show(appWorkspace) == button)
            {
                if (ShellUtil.IsActivityQueuedForRestart)
                {
                    MessageBoxUtil.ErrorBox(appWorkspace, PdnResources.GetString("Effect.PluginErrorDialog.CantQueue2ndRestart"));
                }
                else
                {
                    CloseAllWorkspacesAction action = new CloseAllWorkspacesAction();
                    action.PerformAction(appWorkspace);
                    if (!action.Cancelled)
                    {
                        ShellUtil.RestartApplication();
                        Startup.CloseApplication();
                    }
                }
            }
        }

        private void InitializeComponent()
        {
            this.sentinel = new PdnMenuItem();
            this.sentinel.Name = null;
            this.components = new Container();
            this.invalidateTimer = new System.Windows.Forms.Timer(this.components);
            this.invalidateTimer.Enabled = false;
            this.invalidateTimer.Tick += new EventHandler(this.OnInvalidateTimerTick);
            this.invalidateTimer.Interval = 0x10;
            base.DropDownItems.Add(this.sentinel);
        }

        private bool IsBuiltInEffect(Effect effect)
        {
            if (effect == null)
            {
                return true;
            }
            System.Type type = effect.GetType();
            System.Type type2 = typeof(Effect);
            return (type.Assembly == type2.Assembly);
        }

        protected override void OnDropDownOpening(EventArgs e)
        {
            if (!this.menuPopulated)
            {
                this.PopulateMenu();
            }
            bool flag = base.AppWorkspace.ActiveDocumentWorkspace > null;
            foreach (ToolStripItem item in base.DropDownItems)
            {
                item.Enabled = flag;
            }
            base.OnDropDownOpening(e);
        }

        private void OnEffectMenuItemClick(object sender, EventArgs e)
        {
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                PdnMenuItem item = (PdnMenuItem) sender;
                System.Type tag = (System.Type) item.Tag;
                this.RunEffect(tag);
            }
        }

        private void OnInvalidateTimerTick(object sender, EventArgs e)
        {
            if ((base.AppWorkspace.FindForm().WindowState != FormWindowState.Minimized) && (this.progressRegions != null))
            {
                PdnRegion[] progressRegions = this.progressRegions;
                lock (progressRegions)
                {
                    int progressRegionsStartIndex = this.progressRegionsStartIndex;
                    int index = progressRegionsStartIndex;
                    while (index < this.progressRegions.Length)
                    {
                        if (this.progressRegions[index] == null)
                        {
                            break;
                        }
                        index++;
                    }
                    if (progressRegionsStartIndex != index)
                    {
                        RectInt32? nullable = null;
                        for (int i = progressRegionsStartIndex; i < index; i++)
                        {
                            RectInt32 b = this.progressRegions[i].GetBoundsRectInt32();
                            nullable = new RectInt32?(nullable.HasValue ? RectInt32.Union(nullable.Value, b) : b);
                        }
                        if (nullable.HasValue)
                        {
                            base.AppWorkspace.ActiveDocumentWorkspace.ActiveLayer.Invalidate(nullable.Value);
                        }
                        this.progressRegionsStartIndex = index;
                    }
                    if (this.showProgressInStatusBar)
                    {
                        double num5 = (100.0 * index) / ((double) this.progressRegions.Length);
                        base.AppWorkspace.Widgets.StatusBarProgress.SetProgressStatusBar(new double?(num5));
                    }
                    else
                    {
                        base.AppWorkspace.Widgets.StatusBarProgress.EraseProgressStatusBar();
                    }
                }
            }
        }

        private void OnRepeatEffectMenuItemClick(object sender, EventArgs e)
        {
            Exception exception = null;
            Effect effect = null;
            DocumentWorkspace activeDocumentWorkspace = base.AppWorkspace.ActiveDocumentWorkspace;
            if (activeDocumentWorkspace != null)
            {
                using (new PushNullToolMode(activeDocumentWorkspace))
                {
                    Surface dst = activeDocumentWorkspace.BorrowScratchSurface(base.GetType() + ".OnRepeatEffectMenuItemClick() utilizing scratch for rendering");
                    try
                    {
                        EffectConfigToken token;
                        using (new WaitCursorChanger(base.AppWorkspace))
                        {
                            ((BitmapLayer) activeDocumentWorkspace.ActiveLayer).Surface.Parallelize<ColorBgra>(TilingStrategy.HorizontalSlices, 7, WorkItemQueuePriority.Normal).Render<ColorBgra>(dst);
                        }
                        effect = (Effect) Activator.CreateInstance(this.lastEffectType);
                        SelectionRenderingQuality effectiveSelectionRenderingQuality = GetEffectiveSelectionRenderingQuality(effect, activeDocumentWorkspace.ToolSettings);
                        PdnRegion selection = PdnRegion.FromRectangles(activeDocumentWorkspace.Selection.GetCachedClippingMaskBiLevelCoverageScans(effectiveSelectionRenderingQuality));
                        IRenderer<ColorAlpha8> cachedClippingMaskRenderer = activeDocumentWorkspace.Selection.GetCachedClippingMaskRenderer(effectiveSelectionRenderingQuality);
                        EffectEnvironmentParameters parameters = new EffectEnvironmentParameters(base.AppWorkspace.ToolSettings.PrimaryColor.Value, base.AppWorkspace.ToolSettings.SecondaryColor.Value, base.AppWorkspace.ToolSettings.Pen.Width.Value, selection, dst);
                        effect.EnvironmentParameters = parameters;
                        if (this.lastEffectToken == null)
                        {
                            token = null;
                        }
                        else
                        {
                            token = (EffectConfigToken) this.lastEffectToken.Clone();
                        }
                        this.DoEffect(effect, token, selection, selection, cachedClippingMaskRenderer, dst, out exception);
                    }
                    finally
                    {
                        activeDocumentWorkspace.ReturnScratchSurface(dst);
                    }
                }
            }
            if (exception != null)
            {
                this.HandleEffectException(base.AppWorkspace, effect, exception);
            }
            if (effect != null)
            {
                effect.Dispose();
                effect = null;
            }
        }

        public void PopulateEffects()
        {
            this.PopulateMenu(false);
        }

        private void PopulateMenu()
        {
            base.DropDownItems.Clear();
            if (this.EnableRepeatEffectMenuItem && (this.lastEffectType != null))
            {
                PdnMenuItem item = new PdnMenuItem(string.Format(PdnResources.GetString("Effects.RepeatMenuItem.Format"), this.lastEffectName), this.lastEffectImage, new EventHandler(this.OnRepeatEffectMenuItemClick)) {
                    Name = "RepeatEffect(" + this.lastEffectType.FullName + ")",
                    ShortcutKeys = Keys.Control | Keys.F
                };
                base.DropDownItems.Add(item);
                ToolStripSeparator separator = new ToolStripSeparator();
                base.DropDownItems.Add(separator);
            }
            this.AddEffectsToMenu();
            PluginErrorInfo[] loaderExceptions = this.Effects.GetLoaderExceptions();
            for (int i = 0; i < loaderExceptions.Length; i++)
            {
                base.AppWorkspace.GetService<IPluginErrorService>().ReportEffectLoadError(loaderExceptions[i].Assembly, loaderExceptions[i].Type, loaderExceptions[i].Error);
            }
        }

        private void PopulateMenu(bool forceRepopulate)
        {
            if (forceRepopulate)
            {
                this.menuPopulated = false;
            }
            this.PopulateMenu();
        }

        private void RenderedTileHandler(object sender, RenderedTileEventArgs e)
        {
            if (base.AppWorkspace.InvokeRequired)
            {
                object[] args = new object[] { sender, e };
                base.AppWorkspace.BeginInvoke(new RenderedTileEventHandler(this.RenderedTileHandler), args);
            }
            else
            {
                PdnRegion[] progressRegions = this.progressRegions;
                lock (progressRegions)
                {
                    if (this.progressRegions[e.TileNumber] == null)
                    {
                        this.progressRegions[e.TileNumber] = e.RenderedRegion;
                    }
                }
            }
        }

        public void RunEffect(System.Type effectType)
        {
            ThreadPriority priority = Thread.CurrentThread.Priority;
            Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
            try
            {
                this.RunEffectImpl(effectType);
            }
            finally
            {
                Thread.CurrentThread.Priority = priority;
            }
        }

        private void RunEffectImpl(System.Type effectType)
        {
            bool dirty = base.AppWorkspace.ActiveDocumentWorkspace.Document.Dirty;
            bool flag2 = false;
            base.AppWorkspace.Update();
            base.AppWorkspace.Widgets.StatusBarProgress.ResetProgressStatusBar();
            DocumentWorkspace activeDocumentWorkspace = base.AppWorkspace.ActiveDocumentWorkspace;
            Exception exception = null;
            Effect effect = null;
            BitmapLayer activeLayer = (BitmapLayer) activeDocumentWorkspace.ActiveLayer;
            ThreadDispatcher backThread = new ThreadDispatcher();
            PdnRegion selection = null;
            using (new PushNullToolMode(activeDocumentWorkspace))
            {
                try
                {
                    IRenderer<ColorAlpha8> cachedClippingMaskRenderer;
                    effect = (Effect) Activator.CreateInstance(effectType);
                    string name = effect.Name;
                    EffectConfigToken token = null;
                    SelectionRenderingQuality effectiveSelectionRenderingQuality = GetEffectiveSelectionRenderingQuality(effect, activeDocumentWorkspace.ToolSettings);
                    if (effectiveSelectionRenderingQuality == SelectionRenderingQuality.Aliased)
                    {
                        cachedClippingMaskRenderer = null;
                        selection = activeDocumentWorkspace.Selection.CreateRegion();
                    }
                    else
                    {
                        cachedClippingMaskRenderer = activeDocumentWorkspace.Selection.GetCachedClippingMaskRenderer(effectiveSelectionRenderingQuality);
                        selection = PdnRegion.FromRectangles(activeDocumentWorkspace.Selection.GetCachedClippingMaskBiLevelCoverageScans(effectiveSelectionRenderingQuality));
                    }
                    if (!effect.CheckForEffectFlags(EffectFlags.Configurable))
                    {
                        Surface dst = activeDocumentWorkspace.BorrowScratchSurface(base.GetType() + ".RunEffect() using scratch surface for non-configurable rendering");
                        try
                        {
                            using (new WaitCursorChanger(base.AppWorkspace))
                            {
                                activeLayer.Surface.Parallelize<ColorBgra>(TilingStrategy.HorizontalSlices, 7, WorkItemQueuePriority.Normal).Render<ColorBgra>(dst);
                            }
                            EffectEnvironmentParameters parameters = new EffectEnvironmentParameters(base.AppWorkspace.ToolSettings.PrimaryColor.Value, base.AppWorkspace.ToolSettings.SecondaryColor.Value, base.AppWorkspace.ToolSettings.Pen.Width.Value, selection, dst);
                            effect.EnvironmentParameters = parameters;
                            this.DoEffect(effect, null, selection, selection, cachedClippingMaskRenderer, dst, out exception);
                        }
                        finally
                        {
                            activeDocumentWorkspace.ReturnScratchSurface(dst);
                        }
                    }
                    else
                    {
                        PdnRegion renderRegion = selection.Clone();
                        renderRegion.Intersect(RectDouble.Inflate(activeDocumentWorkspace.VisibleDocumentRect, 1.0, 1.0).ToGdipRectangleF());
                        Surface surface2 = activeDocumentWorkspace.BorrowScratchSurface(base.GetType() + ".RunEffect() using scratch surface for rendering during configuration");
                        try
                        {
                            using (new WaitCursorChanger(base.AppWorkspace))
                            {
                                activeLayer.Surface.Parallelize<ColorBgra>(TilingStrategy.HorizontalSlices, 7, WorkItemQueuePriority.Normal).Render<ColorBgra>(surface2);
                            }
                            EffectEnvironmentParameters parameters2 = new EffectEnvironmentParameters(base.AppWorkspace.ToolSettings.PrimaryColor.Value, base.AppWorkspace.ToolSettings.SecondaryColor.Value, base.AppWorkspace.ToolSettings.Pen.Width.Value, selection, surface2);
                            effect.EnvironmentParameters = parameters2;
                            IDisposable disposable = base.AppWorkspace.SuspendThumbnailUpdates();
                            long asyncVersion = 0L;
                            using (EffectConfigDialog configDialog = effect.CreateConfigDialog())
                            {
                                DialogResult none;
                                configDialog.Effect = effect;
                                configDialog.EffectSourceSurface = surface2;
                                configDialog.Selection = selection;
                                this.showProgressInStatusBar = true;
                                Action resetProgressStatusBar = delegate {
                                    if (this.showProgressInStatusBar)
                                    {
                                        base.AppWorkspace.Widgets.StatusBarProgress.ResetProgressStatusBar();
                                    }
                                };
                                BackgroundEffectRenderer ber = null;
                                EventHandler handler = delegate (object sender, EventArgs e) {
                                    EffectConfigDialog ecf = (EffectConfigDialog) sender;
                                    if (ber != null)
                                    {
                                        if (this.showProgressInStatusBar)
                                        {
                                            PdnSynchronizationContext.Instance.EnsurePosted(resetProgressStatusBar);
                                        }
                                        asyncVersion += 1L;
                                        long ourRunVersion = asyncVersion;
                                        backThread.BeginTry(delegate {
                                            try
                                            {
                                                if (ourRunVersion == asyncVersion)
                                                {
                                                    ber.Start();
                                                }
                                            }
                                            catch (Exception exception1)
                                            {
                                                exception = exception1;
                                                configDialog.BeginInvoke(new Action(ecf.Close));
                                            }
                                        });
                                    }
                                };
                                configDialog.EffectTokenChanged += handler;
                                if (this.effectTokens.ContainsKey(effectType))
                                {
                                    EffectConfigToken token2 = (EffectConfigToken) this.effectTokens[effectType].Clone();
                                    configDialog.EffectToken = token2;
                                }
                                ber = new BackgroundEffectRenderer(effect, configDialog.EffectToken, new RenderArgs(activeLayer.Surface), new RenderArgs(surface2), renderRegion, cachedClippingMaskRenderer, 0x19 * this.renderingThreadCount, this.renderingThreadCount);
                                ber.RenderedTile += new RenderedTileEventHandler(this.RenderedTileHandler);
                                ber.StartingRendering += new EventHandler(this.StartingRenderingHandler);
                                ber.FinishedRendering += new EventHandler(this.FinishedRenderingHandler);
                                this.invalidateTimer.Enabled = true;
                                try
                                {
                                    configDialog.Shown += (sender, e) => backThread.BeginTry(delegate {
                                        EffectConfigDialog dialog1 = (EffectConfigDialog) sender;
                                        try
                                        {
                                            ber.Start();
                                        }
                                        catch (Exception exception1)
                                        {
                                            exception = exception1;
                                            configDialog.BeginInvoke(new Action(dialog1.Close));
                                        }
                                    });
                                    try
                                    {
                                        none = configDialog.ShowDialog(base.AppWorkspace);
                                    }
                                    catch (Exception exception)
                                    {
                                        none = DialogResult.None;
                                        exception = exception;
                                    }
                                    configDialog.EffectTokenChanged -= handler;
                                    asyncVersion += 1L;
                                }
                                finally
                                {
                                    this.showProgressInStatusBar = false;
                                    this.invalidateTimer.Enabled = false;
                                }
                                this.OnInvalidateTimerTick(this.invalidateTimer, EventArgs.Empty);
                                if (none == DialogResult.OK)
                                {
                                    this.effectTokens[effectType] = (EffectConfigToken) configDialog.EffectToken.Clone();
                                }
                                using (new WaitCursorChanger(base.AppWorkspace))
                                {
                                    using (ManualResetEvent stopDone = new ManualResetEvent(false))
                                    {
                                        WaitWithUIType canceling;
                                        backThread.BeginTry(delegate {
                                            try
                                            {
                                                ber.Abort();
                                                ber.Join();
                                                ber.Dispose();
                                                ber = null;
                                            }
                                            catch (Exception exception1)
                                            {
                                                exception = exception1;
                                            }
                                            finally
                                            {
                                                stopDone.Set();
                                            }
                                        });
                                        if (none == DialogResult.Cancel)
                                        {
                                            canceling = WaitWithUIType.Canceling;
                                        }
                                        else
                                        {
                                            canceling = WaitWithUIType.Finishing;
                                        }
                                        this.WaitWithUI(base.AppWorkspace, effect, canceling, stopDone);
                                    }
                                    try
                                    {
                                        if (surface2.Scan0.MaySetAllowWrites)
                                        {
                                            surface2.Scan0.AllowWrites = true;
                                        }
                                    }
                                    catch (Exception)
                                    {
                                    }
                                    if (none != DialogResult.OK)
                                    {
                                        Layer layer2 = activeDocumentWorkspace.ActiveLayer;
                                        BitmapLayer layer3 = (BitmapLayer) layer2;
                                        Surface surface = layer3.Surface;
                                        surface2.Parallelize<ColorBgra>(TilingStrategy.HorizontalSlices, 7, WorkItemQueuePriority.Normal).Render<ColorBgra>(surface);
                                        layer2.Invalidate();
                                    }
                                    configDialog.EffectTokenChanged -= handler;
                                    configDialog.Hide();
                                    base.AppWorkspace.Update();
                                    renderRegion.Dispose();
                                }
                                disposable.Dispose();
                                disposable = null;
                                if (none == DialogResult.OK)
                                {
                                    PdnRegion regionToRender = selection.Clone();
                                    PdnRegion roi = PdnRegion.CreateEmpty();
                                    for (int i = 0; i < this.progressRegions.Length; i++)
                                    {
                                        if (this.progressRegions[i] == null)
                                        {
                                            break;
                                        }
                                        regionToRender.Exclude(this.progressRegions[i]);
                                        roi.Union(this.progressRegions[i]);
                                    }
                                    activeDocumentWorkspace.ActiveLayer.Invalidate(roi);
                                    token = (EffectConfigToken) configDialog.EffectToken.Clone();
                                    base.AppWorkspace.Widgets.StatusBarProgress.ResetProgressStatusBar();
                                    this.DoEffect(effect, token, selection, regionToRender, cachedClippingMaskRenderer, surface2, out exception);
                                }
                                else
                                {
                                    using (new WaitCursorChanger(base.AppWorkspace))
                                    {
                                        activeDocumentWorkspace.ActiveLayer.Invalidate();
                                        CleanupManager.RequestCleanup();
                                    }
                                    flag2 = true;
                                    return;
                                }
                            }
                        }
                        catch (Exception exception2)
                        {
                            exception = exception2;
                        }
                        finally
                        {
                            activeDocumentWorkspace.ReturnScratchSurface(surface2);
                        }
                    }
                    if (effect.Category == EffectCategory.Effect)
                    {
                        if (this.lastEffectImage != null)
                        {
                            this.lastEffectImage.Dispose();
                            this.lastEffectImage = null;
                        }
                        this.lastEffectType = effect.GetType();
                        this.lastEffectName = effect.Name;
                        this.lastEffectImage = (effect.Image == null) ? null : effect.Image.CloneT<Image>();
                        if (token == null)
                        {
                            this.lastEffectToken = null;
                        }
                        else
                        {
                            this.lastEffectToken = (EffectConfigToken) token.Clone();
                        }
                        this.PopulateMenu(true);
                    }
                }
                catch (Exception exception3)
                {
                    exception = exception3;
                }
                finally
                {
                    DisposableUtil.Free<PdnRegion>(ref selection);
                    base.AppWorkspace.Widgets.StatusBarProgress.ResetProgressStatusBar();
                    base.AppWorkspace.Widgets.StatusBarProgress.EraseProgressStatusBar();
                    if (this.progressRegions != null)
                    {
                        for (int j = 0; j < this.progressRegions.Length; j++)
                        {
                            DisposableUtil.Free<PdnRegion>(ref this.progressRegions[j]);
                        }
                    }
                    if (flag2)
                    {
                        base.AppWorkspace.ActiveDocumentWorkspace.Document.Dirty = dirty;
                    }
                    if (exception != null)
                    {
                        this.HandleEffectException(base.AppWorkspace, effect, exception);
                    }
                    backThread.BeginTry(() => DisposableUtil.Free<ThreadDispatcher>(ref backThread)).Observe();
                    if (effect != null)
                    {
                        effect.Dispose();
                        effect = null;
                    }
                }
            }
        }

        private void StartingRenderingHandler(object sender, EventArgs e)
        {
            if (base.AppWorkspace.InvokeRequired)
            {
                object[] args = new object[] { sender, e };
                base.AppWorkspace.BeginInvoke(new EventHandler(this.StartingRenderingHandler), args);
            }
            else
            {
                if (this.progressRegions == null)
                {
                    this.progressRegions = new PdnRegion[0x19 * this.renderingThreadCount];
                }
                PdnRegion[] progressRegions = this.progressRegions;
                lock (progressRegions)
                {
                    for (int i = 0; i < this.progressRegions.Length; i++)
                    {
                        this.progressRegions[i] = null;
                    }
                    this.progressRegionsStartIndex = 0;
                }
            }
        }

        private void WaitWithUI(IWin32Window owner, Effect effect, WaitWithUIType waitType, WaitHandle doneSignal)
        {
            if (!doneSignal.WaitOne(0, false))
            {
                using (TaskProgressDialog dialog = new TaskProgressDialog())
                {
                    dialog.CloseOnFinished = true;
                    dialog.ShowCancelButton = false;
                    if (waitType != WaitWithUIType.Canceling)
                    {
                        if (waitType == WaitWithUIType.Finishing)
                        {
                        }
                    }
                    else
                    {
                        dialog.HeaderText = PdnResources.GetString("TaskProgressDialog.Canceling.Text");
                        goto Label_006A;
                    }
                    dialog.HeaderText = PdnResources.GetString("SaveConfigDialog.Finishing.Text");
                Label_006A:
                    dialog.Text = effect.Name;
                    dialog.Icon = null;
                    if (effect.Image != null)
                    {
                        Icon icon;
                        try
                        {
                            icon = effect.Image.ToIcon();
                        }
                        catch (Exception)
                        {
                            icon = null;
                        }
                        if (icon != null)
                        {
                            dialog.Icon = icon;
                        }
                    }
                    VirtualTask<Unit> vTask = TaskManager.Global.CreateVirtualTask(TaskState.Running);
                    dialog.Task = vTask;
                    Work.QueueWorkItem(WorkItemQueuePriority.High, delegate {
                        try
                        {
                            doneSignal.WaitOne();
                        }
                        finally
                        {
                            vTask.SetState(TaskState.Finished);
                        }
                    });
                    dialog.ShowDialog(owner);
                }
            }
        }

        public EffectsCollection Effects
        {
            get
            {
                if (this.effects == null)
                {
                    this.effects = GatherEffects();
                }
                return this.effects;
            }
        }

        protected abstract bool EnableEffectShortcuts { get; }

        protected abstract bool EnableRepeatEffectMenuItem { get; }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly EffectMenuBase.<>c <>9 = new EffectMenuBase.<>c();
            public static Comparison<Effect> <>9__37_0;
            public static Comparison<string> <>9__37_1;
            public static Func<Rectangle, RectInt32> <>9__47_0;

            internal int <AddEffectsToMenu>b__37_0(Effect lhs, Effect rhs) => 
                string.Compare(lhs.Name, rhs.Name, true);

            internal int <AddEffectsToMenu>b__37_1(string lhs, string rhs) => 
                string.Compare(lhs, rhs, true);

            internal RectInt32 <DoEffect>b__47_0(Rectangle r) => 
                r.ToRectInt32();
        }

        private enum WaitWithUIType
        {
            Canceling,
            Finishing
        }
    }
}

