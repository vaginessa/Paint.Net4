namespace PaintDotNet
{
    using Microsoft.Win32;
    using PaintDotNet.Animation;
    using PaintDotNet.AppModel;
    using PaintDotNet.Base;
    using PaintDotNet.Collections;
    using PaintDotNet.ComponentModel;
    using PaintDotNet.Core;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Dialogs;
    using PaintDotNet.Direct2D;
    using PaintDotNet.DirectWrite;
    using PaintDotNet.Effects;
    using PaintDotNet.Framework;
    using PaintDotNet.Functional;
    using PaintDotNet.IndirectUI;
    using PaintDotNet.Interop;
    using PaintDotNet.Resources;
    using PaintDotNet.Settings.App;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Threading;
    using PaintDotNet.Updates;
    using PaintDotNet.VisualStyling;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class Startup
    {
        private readonly string[] args;
        private bool canUseCrashDialog;
        private static Startup instance;
        private MainForm mainForm;
        private static DateTime startupTime;

        private Startup(string[] args)
        {
            this.args = args.ToArrayEx<string>();
        }

        private bool CheckForImportantFiles()
        {
            string[] head = new string[] { "PaintDotNet.Base.dll", "PaintDotNet.Core.dll", "PaintDotNet.Data.dll", "PaintDotNet.Effects.dll", "PaintDotNet.Framework.dll", "PaintDotNet.Resources.dll", "PaintDotNet.SystemLayer.dll", "PaintDotNet.SystemLayer.Native.x86.dll", "PaintDotNet.SystemLayer.Native.x64.dll" };
            string[] tails = new string[] { "Interop.WIA.dll", "PaintDotNet.exe.config", "SetupNgen.exe", "SetupNgen.exe.config", "ShellExtension_x64.dll", "ShellExtension_x86.dll", "UpdateMonitor.exe", "UpdateMonitor.exe.config" };
            string[] items = new string[] { 
                "cs", "da", "de", "es", "fa", "fi", "fr", "hi", "hu", "it", "ja", "ko", "lt", "nl", "pl", "pt-BR",
                "pt-PT", "ru", "sv", "zh-CN", "zh-TW"
            };
            string[] strArray4 = items.Select<string, string>(c => $"PaintDotNet.Strings.3.{c}.resources").Concat<string>("PaintDotNet.Strings.3.resources").ToArrayEx<string>();
            string[] strArray5 = head.Concat<string>(tails).Concat<string>(strArray4).OrderBySelf<string>().ToArrayEx<string>();
            string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            List<string> list = null;
            foreach (string str4 in strArray5)
            {
                bool flag;
                try
                {
                    FileInfo info2 = new FileInfo(Path.Combine(directoryName, str4));
                    flag = !info2.Exists;
                }
                catch (Exception)
                {
                    flag = true;
                }
                if (flag)
                {
                    if (list == null)
                    {
                        list = new List<string>();
                    }
                    list.Add(str4);
                }
            }
            Uri uri = new Uri(Assembly.GetEntryAssembly().CodeBase);
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(uri.LocalPath);
            foreach (string str6 in head)
            {
                bool flag2 = false;
                try
                {
                    if (!string.Equals(FileVersionInfo.GetVersionInfo(Path.Combine(directoryName, str6)).FileVersion, versionInfo.FileVersion, StringComparison.OrdinalIgnoreCase))
                    {
                        flag2 = true;
                    }
                }
                catch (Exception)
                {
                    flag2 = true;
                }
                if (flag2)
                {
                    if (list == null)
                    {
                        list = new List<string>();
                    }
                    list.Add(str6);
                }
            }
            if (list != null)
            {
                if (ShellUtil.ReplaceMissingFiles(list.ToArrayEx<string>()))
                {
                    return true;
                }
                Process.GetCurrentProcess().Kill();
            }
            return false;
        }

        public static bool CloseApplication()
        {
            List<Form> list = new List<Form>();
            foreach (Form form in Application.OpenForms)
            {
                if (form.Modal && (form != instance.mainForm))
                {
                    list.Add(form);
                }
            }
            if (list.Count > 0)
            {
                return false;
            }
            return CloseForm(instance.mainForm);
        }

        private static bool CloseForm(Form form)
        {
            ArrayList list = new ArrayList(Application.OpenForms);
            if (list.IndexOf(form) == -1)
            {
                return false;
            }
            form.Close();
            ArrayList list2 = new ArrayList(Application.OpenForms);
            return (list2.IndexOf(form) == -1);
        }

        [STAThread]
        public static int Main(string[] args)
        {
            State = PaintDotNet.ApplicationState.Starting;
            try
            {
                startupTime = DateTime.Now;
                try
                {
                    AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(Startup.OnCurrentDomainAssemblyResolve);
                    instance = new Startup(args);
                    instance.Start();
                }
                catch (Exception exception)
                {
                    try
                    {
                        if ((instance != null) && instance.canUseCrashDialog)
                        {
                            UnhandledException(exception);
                        }
                        else
                        {
                            UIUtil.MessageBox(null, exception.ToString(), null, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        }
                        Process.GetCurrentProcess().Kill();
                    }
                    catch (Exception)
                    {
                        try
                        {
                            UIUtil.MessageBox(null, exception.ToString(), null, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                            Process.GetCurrentProcess().Kill();
                        }
                        catch (Exception)
                        {
                            Environment.FailFast(null, exception);
                        }
                    }
                }
            }
            finally
            {
                State = PaintDotNet.ApplicationState.Exiting;
            }
            return 0;
        }

        private static void OnApplicationThreadException(object sender, ThreadExceptionEventArgs e)
        {
            UnhandledException(e.Exception);
        }

        private static Assembly OnCurrentDomainAssemblyResolve(object sender, ResolveEventArgs args)
        {
            int index = args.Name.IndexOf("PdnLib", StringComparison.InvariantCultureIgnoreCase);
            Assembly assembly = null;
            if (index == 0)
            {
                assembly = typeof(PaintDotNet.Core.AssemblyServices).Assembly;
            }
            return assembly;
        }

        private static void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            UnhandledException((Exception) e.ExceptionObject);
        }

        public void Start()
        {
            string str2;
            string str3;
            int num;
            try
            {
                string path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "paint.net"), "Optimization");
                Directory.CreateDirectory(path);
                string profile = ((this.args != null) && (this.args.Length >= 1)) ? "Startup.1.profile" : "Startup.0.profile";
                ProfileOptimization.SetProfileRoot(path);
                ProfileOptimization.StartProfile(profile);
            }
            catch (Exception)
            {
            }
            try
            {
                new Uri(Environment.GetEnvironmentVariable("windir"), UriKind.Absolute);
            }
            catch (Exception exception)
            {
                if (!(exception is UriFormatException) && !(exception is ArgumentNullException))
                {
                    throw;
                }
                string environmentVariable = Environment.GetEnvironmentVariable("systemroot");
                Environment.SetEnvironmentVariable("windir", environmentVariable);
            }
            string sxsRootDirPath = Path.Combine(Application.StartupPath, "SxS");
            ShellUtil.LoadUniversalCrt(sxsRootDirPath);
            ShellUtil.LoadVisualCppRuntime(sxsRootDirPath);
            ShellUtil.LoadOpenMP(sxsRootDirPath);
            Control.CheckForIllegalCrossThreadCalls = true;
            Application.SetCompatibleTextRenderingDefault(false);
            Application.EnableVisualStyles();
            Processor.LockLogicalCpuCount();
            string[] args = TryRemoveArg(this.args, "/sleep=", out str2);
            if (!string.IsNullOrWhiteSpace(str2) && int.TryParse(str2, out num))
            {
                Thread.Sleep(num);
            }
            args = TryRemoveArg(args, "/skipRepairAttempt", out str3);
            if ((str3 == null) && this.CheckForImportantFiles())
            {
                StartNewInstance(null, false, args);
            }
            else
            {
                string str4;
                args = TryRemoveArg(args, "/mutexName=", out str4);
                string mutexName = string.IsNullOrWhiteSpace(str4) ? "PaintDotNet" : str4;
                this.StartPart2(mutexName, args);
            }
        }

        public static void StartNewInstance(IWin32Window parent, string fileName)
        {
            string str;
            if ((fileName != null) && (fileName.Length != 0))
            {
                str = "\"" + fileName + "\"";
            }
            else
            {
                str = "";
            }
            string[] args = new string[] { str };
            StartNewInstance(parent, false, args);
        }

        public static void StartNewInstance(IWin32Window parent, bool requireAdmin, params string[] args)
        {
            string str;
            StringBuilder builder = new StringBuilder();
            foreach (string str2 in args)
            {
                builder.Append(' ');
                if (str2.IndexOf(' ') != -1)
                {
                    builder.Append('"');
                }
                builder.Append(str2);
                if (str2.IndexOf(' ') != -1)
                {
                    builder.Append('"');
                }
            }
            if (builder.Length > 0)
            {
                str = builder.ToString(1, builder.Length - 1);
            }
            else
            {
                str = null;
            }
            ShellUtil.Execute(parent, Application.ExecutablePath, str, requireAdmin ? ExecutePrivilege.RequireAdmin : ExecutePrivilege.AsInvokerOrAsManifest, ExecuteWaitType.ReturnImmediately);
        }

        private void StartPart2(string mutexName, string[] remainingArgs)
        {
            IAnimationService animationService;
            Memory.Initialize();
            CultureInfo info = AppSettings.Instance.UI.Language.Value;
            Thread.CurrentThread.CurrentUICulture = info;
            CultureInfo.DefaultThreadCurrentUICulture = info;
            AppSettings.Instance.UI.Language.Value = info;
            PdnResources.Culture = info;
            AppSettings.Instance.UI.ErrorFlagsAtStartup.Value = AppSettings.Instance.UI.ErrorFlags.Value;
            UIUtil.IsGetMouseMoveScreenPointsEnabled = AppSettings.Instance.UI.EnableSmoothMouseInput.Value;
            if (!OS.VerifyFrameworkVersion(4, 6, 0, OS.FrameworkProfile.Full))
            {
                string message = PdnResources.GetString("Error.FXRequirement");
                MessageBoxUtil.ErrorBox(null, message);
                string fxurl = "http://www.microsoft.com/en-us/download/details.aspx?id=40773";
                () => ShellUtil.LaunchUrl2(null, fxurl).Eval<bool>().Observe();
            }
            else if (!OS.VerifyWindowsVersion(6, 1, 1))
            {
                string str4 = PdnResources.GetString("Error.OSRequirement");
                MessageBoxUtil.ErrorBox(null, str4);
            }
            else if (!Processor.IsFeaturePresent(ProcessorFeature.SSE))
            {
                string str5 = PdnResources.GetString("Error.SSERequirement");
                MessageBoxUtil.ErrorBox(null, str5);
            }
            else
            {
                string str;
                if (MultithreadedWorkItemDispatcher.IsSingleThreadForced && PdnInfo.IsFinalBuild)
                {
                    throw new PaintDotNet.InternalErrorException("MultithreadedWorkItemDispatcher.IsSingleThreadForced shouldn't be true for Final builds!");
                }
                if (RefTrackedObject.IsFullRefTrackingEnabled && PdnInfo.IsFinalBuild)
                {
                    throw new PaintDotNet.InternalErrorException("Full ref tracking should not be enabled for non-expiring builds!");
                }
                PaintDotNet.Base.AssemblyServices.RegisterProxies();
                PaintDotNet.Core.AssemblyServices.RegisterProxies();
                PaintDotNet.Framework.AssemblyServices.RegisterProxies();
                ObjectRefProxy.CloseRegistration();
                remainingArgs = TryRemoveArg(remainingArgs, "/showCrashLog=", out str);
                if (!string.IsNullOrWhiteSpace(str))
                {
                    DialogResult? nullable = null;
                    try
                    {
                        string fullPath = Path.GetFullPath(str);
                        if (File.Exists(fullPath))
                        {
                            nullable = new DialogResult?(CrashManager.ShowCrashLogDialog(fullPath));
                        }
                    }
                    catch (Exception exception)
                    {
                        try
                        {
                            MessageBox.Show(exception.ToString(), null, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        }
                        catch (Exception exception2)
                        {
                            Environment.FailFast(null, exception2);
                        }
                    }
                    DialogResult? nullable2 = nullable;
                    DialogResult oK = DialogResult.OK;
                    if ((((DialogResult) nullable2.GetValueOrDefault()) == oK) ? nullable2.HasValue : false)
                    {
                        string[] args = new string[] { "/sleep=1000" };
                        StartNewInstance(null, false, args);
                    }
                }
                else
                {
                    string str2;
                    AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Startup.OnCurrentDomainUnhandledException);
                    Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException, true);
                    Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException, false);
                    Application.ThreadException += new ThreadExceptionEventHandler(Startup.OnApplicationThreadException);
                    this.canUseCrashDialog = true;
                    remainingArgs = TryRemoveArg(remainingArgs, "/test", out str2);
                    if (str2 != null)
                    {
                        PdnInfo.IsTestMode = true;
                    }
                    SingleInstanceManager disposeMe = new SingleInstanceManager(mutexName);
                    animationService = null;
                    try
                    {
                        DirectWriteFactory.DefaultDirectWriteSettingsController defaultDirectWriteSettingsController;
                        DrawingContext.DefaultDrawingContextSettingsController defaultDrawingContextSettingsController;
                        IDisposable updatesServiceShutdown;
                        IUpdatesServiceHost updatesServiceHost;
                        PdnSynchronizationContext pdnSyncContext;
                        if (!disposeMe.IsFirstInstance)
                        {
                            disposeMe.FocusFirstInstance();
                            foreach (string str7 in remainingArgs)
                            {
                                disposeMe.SendInstanceMessage(str7, 30);
                            }
                            disposeMe.Dispose();
                            disposeMe = null;
                        }
                        else
                        {
                            CleanupService.Initialize();
                            ResourcesService.Initialize();
                            UserFilesService.Initialize();
                            UserPalettesService.Initialize();
                            animationService = AnimationService.Initialize();
                            animationService.IsEnabled = true;
                            Document.Initialize(PdnInfo.Version);
                            Layer.Initialize(PdnResources.GetString("Layer.BackgroundLayer.DefaultName"));
                            Effect.SetDefaultServiceProviderValueFactory(effect => new ServiceProviderForEffects());
                            defaultDirectWriteSettingsController = DirectWriteFactory.GetDefaultSettingsController();
                            defaultDirectWriteSettingsController.DefaultCulture = AppSettings.Instance.UI.Language.Value;
                            AppSettings.Instance.UI.Language.ValueChangedT += (sender, e) => (defaultDirectWriteSettingsController.DefaultCulture = e.NewValue);
                            AeroColors.CurrentScheme = AppSettings.Instance.UI.AeroColorScheme.Value;
                            AppSettings.Instance.UI.AeroColorScheme.ValueChangedT += (sender, e) => (AeroColors.CurrentScheme = e.NewValue);
                            defaultDrawingContextSettingsController = DrawingContext.GetDefaultSettingsController();
                            defaultDrawingContextSettingsController.DefaultTextAntialiasMode = AppSettings.Instance.UI.DefaultTextAntialiasMode.Value;
                            AppSettings.Instance.UI.DefaultTextAntialiasMode.ValueChangedT += delegate (object sender, ValueChangedEventArgs<TextAntialiasMode> e) {
                                defaultDrawingContextSettingsController.DefaultTextAntialiasMode = e.NewValue;
                                foreach (Form form in Application.OpenForms)
                                {
                                    form.PerformLayoutRecursiveDepthFirst("TextAntialiasMode");
                                    form.Invalidate(true);
                                }
                            };
                            defaultDrawingContextSettingsController.DefaultTextRenderingMode = AppSettings.Instance.UI.DefaultTextRenderingMode.Value;
                            AppSettings.Instance.UI.DefaultTextRenderingMode.ValueChangedT += delegate (object sender, ValueChangedEventArgs<TextRenderingMode> e) {
                                defaultDrawingContextSettingsController.DefaultTextRenderingMode = e.NewValue;
                                foreach (Form form in Application.OpenForms)
                                {
                                    form.PerformLayoutRecursiveDepthFirst("TextRenderingMode");
                                    form.Invalidate(true);
                                }
                            };
                            PaintDotNet.IndirectUI.ControlInfo.Initialize(Assembly.GetExecutingAssembly());
                            PdnBaseForm.SetStaticHelpRequestedHandler(delegate (object sender, HelpEventArgs e) {
                                HelpService.Instance.ShowHelp(sender as IWin32Window);
                                e.Handled = true;
                            });
                            Control control = new Control();
                            SynchronizationContext current = SynchronizationContext.Current;
                            PdnSynchronizationContextController controller = PdnSynchronizationContext.Install(new WaitForMultipleObjectsExDelegate(WaitHelper.WaitForMultipleObjectsEx), new SleepExDelegate(WaitHelper.SleepEx));
                            pdnSyncContext = controller.Instance;
                            this.mainForm = new MainForm(remainingArgs);
                            updatesServiceHost = this.mainForm.CreateUpdatesServiceHost();
                            updatesServiceShutdown = null;
                            EventHandler initUpdatesOnShown = null;
                            initUpdatesOnShown = delegate (object sender, EventArgs e) {
                                this.mainForm.Shown -= initUpdatesOnShown;
                                updatesServiceShutdown = UpdatesService.Initialize(updatesServiceHost);
                            };
                            this.mainForm.Shown += initUpdatesOnShown;
                            this.mainForm.SingleInstanceManager = disposeMe;
                            disposeMe = null;
                            int num = (int) Math.Floor((double) 8.3333333333333339);
                            int intervalMs = (int) Math.Floor((double) 50.0);
                            using (AnimationTimerUpdateThread timerThread = new AnimationTimerUpdateThread(intervalMs, false))
                            {
                                <>c__DisplayClass20_1 class_3;
                                <>c__DisplayClass20_5 class_5;
                                animationService.SetAnimationMode(AppSettings.Instance.UI.EnableAnimations.Value ? AnimationMode.Enabled : AnimationMode.Disabled);
                                AppSettings.Instance.UI.EnableAnimations.ValueChangedT += new ValueChangedEventHandler<bool>(class_3.<StartPart2>b__8);
                                Action<AnimationManagerStatus> updateAnimationTimerIsEnabled = new Action<AnimationManagerStatus>(class_5.<StartPart2>b__10);
                                Action updateAnimationTimerInterval = new Action(class_5.<StartPart2>b__11);
                                bool? isAppActivated = null;
                                Action updateRenderingPriority = delegate {
                                    if ((animationService.IsDisposed || this.mainForm.IsDisposed) || timerThread.IsDisposed)
                                    {
                                        return;
                                    }
                                    RenderingPriority low = RenderingPriority.Normal;
                                    if (isAppActivated.HasValue && !isAppActivated.Value)
                                    {
                                        low = RenderingPriority.Low;
                                    }
                                    else if (this.mainForm.WindowState == FormWindowState.Minimized)
                                    {
                                        low = RenderingPriority.Low;
                                    }
                                    else
                                    {
                                        SessionSwitchReason? lastSessionSwitchReason = SessionSwitchHelpers.LastSessionSwitchReason;
                                        SessionSwitchReason? nullable2 = lastSessionSwitchReason;
                                        SessionSwitchReason consoleDisconnect = SessionSwitchReason.SessionLock;
                                        if (!((((SessionSwitchReason) nullable2.GetValueOrDefault()) == consoleDisconnect) ? nullable2.HasValue : false))
                                        {
                                            nullable2 = lastSessionSwitchReason;
                                            consoleDisconnect = SessionSwitchReason.ConsoleDisconnect;
                                            if (!((((SessionSwitchReason) nullable2.GetValueOrDefault()) == consoleDisconnect) ? nullable2.HasValue : false))
                                            {
                                                nullable2 = lastSessionSwitchReason;
                                                consoleDisconnect = SessionSwitchReason.RemoteDisconnect;
                                                if (!((((SessionSwitchReason) nullable2.GetValueOrDefault()) == consoleDisconnect) ? nullable2.HasValue : false))
                                                {
                                                    goto Label_00EC;
                                                }
                                            }
                                        }
                                        low = RenderingPriority.Low;
                                    }
                                Label_00EC:
                                    RenderingPriorityManager.RenderingPriority = low;
                                };
                                TimerThreadTickEventHandler timerThreadTickHandler = (sender, e) => updateAnimationTimerInterval();
                                this.mainForm.IsAppActivatedChanged += delegate (object sender, ValueChangedEventArgs<bool> e) {
                                    isAppActivated = new bool?(e.NewValue);
                                    updateRenderingPriority();
                                };
                                this.mainForm.WindowStateChanged += delegate (object sender, ValueChangedEventArgs<FormWindowState> e) {
                                    if ((((FormWindowState) e.OldValue) == FormWindowState.Minimized) || (((FormWindowState) e.NewValue) == FormWindowState.Minimized))
                                    {
                                        updateAnimationTimerIsEnabled(animationService.Status);
                                        updateRenderingPriority();
                                    }
                                };
                                SessionSwitchEventHandler handler = delegate (object sender, SessionSwitchEventArgs e) {
                                    SessionSwitchReason reason = e.Reason;
                                    pdnSyncContext.Post(delegate {
                                        if ((!animationService.IsDisposed && !this.mainForm.IsDisposed) && !timerThread.IsDisposed)
                                        {
                                            switch (reason)
                                            {
                                                case SessionSwitchReason.ConsoleConnect:
                                                case SessionSwitchReason.RemoteConnect:
                                                case SessionSwitchReason.SessionUnlock:
                                                    updateAnimationTimerIsEnabled(animationService.Status);
                                                    break;

                                                case SessionSwitchReason.ConsoleDisconnect:
                                                case SessionSwitchReason.RemoteDisconnect:
                                                case SessionSwitchReason.SessionLock:
                                                    updateAnimationTimerIsEnabled(AnimationManagerStatus.Idle);
                                                    break;
                                            }
                                            updateRenderingPriority();
                                        }
                                    });
                                };
                                SessionSwitchHelpers.Initialize();
                                SystemEvents.SessionSwitch += handler;
                                ValueChangedEventHandler<AnimationManagerStatus> animationManagerStatusChangedHandler = (s, e) => updateAnimationTimerIsEnabled(e.NewValue);
                                EventHandler initAnimationTimerAfterMainFormShownHandler = null;
                                initAnimationTimerAfterMainFormShownHandler = delegate (object <sender>, EventArgs <e>) {
                                    this.mainForm.Shown -= initAnimationTimerAfterMainFormShownHandler;
                                    updateAnimationTimerIsEnabled(animationService.Status);
                                    animationService.StatusChanged += animationManagerStatusChangedHandler;
                                    timerThread.Tick += timerThreadTickHandler;
                                    updateAnimationTimerInterval();
                                    updateRenderingPriority();
                                };
                                this.mainForm.Shown += initAnimationTimerAfterMainFormShownHandler;
                                State = PaintDotNet.ApplicationState.Running;
                                try
                                {
                                    PdnBaseForm.PushModalLoopCount();
                                    Application.Run(this.mainForm);
                                }
                                finally
                                {
                                    State = PaintDotNet.ApplicationState.Closing;
                                }
                                Control.CheckForIllegalCrossThreadCalls = false;
                                SystemEvents.SessionSwitch -= handler;
                                timerThread.Tick -= timerThreadTickHandler;
                                this.mainForm.Shown -= initAnimationTimerAfterMainFormShownHandler;
                                animationService.StatusChanged -= animationManagerStatusChangedHandler;
                                DisposableUtil.Free<IDisposable>(ref updatesServiceShutdown);
                                controller.Uninstall();
                                try
                                {
                                    this.mainForm.Dispose();
                                }
                                catch (Exception)
                                {
                                }
                                this.mainForm = null;
                            }
                        }
                    }
                    finally
                    {
                        DisposableUtil.Free<IAnimationService>(ref animationService);
                        DisposableUtil.Free<SingleInstanceManager>(ref disposeMe);
                    }
                }
            }
        }

        private static string[] TryRemoveArg(string[] args, string argPrefix, out string argSuffix)
        {
            int index = args.IndexOf<string>(arg => arg.StartsWith(argPrefix, StringComparison.OrdinalIgnoreCase));
            if (index != -1)
            {
                argSuffix = args[index].Substring(argPrefix.Length);
                return args.Take<string>(index).Concat<string>(args.Skip<string>((index + 1))).ToArrayEx<string>();
            }
            argSuffix = null;
            return args;
        }

        private static void UnhandledException(Exception ex)
        {
            CrashManager.ReportUnhandledException(ex);
        }

        public static DateTime StartupTime =>
            startupTime;

        public static PaintDotNet.ApplicationState State
        {
            [CompilerGenerated]
            get => 
                <State>k__BackingField;
            [CompilerGenerated]
            private set
            {
                <State>k__BackingField = value;
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly Startup.<>c <>9 = new Startup.<>c();
            public static Func<string, string> <>9__16_0;
            public static Func<Effect, IServiceProvider> <>9__20_1;
            public static ValueChangedEventHandler<AeroColorScheme> <>9__20_3;
            public static HelpEventHandler <>9__20_6;

            internal string <CheckForImportantFiles>b__16_0(string c) => 
                $"PaintDotNet.Strings.3.{c}.resources";

            internal IServiceProvider <StartPart2>b__20_1(Effect effect) => 
                new ServiceProviderForEffects();

            internal void <StartPart2>b__20_3(object sender, ValueChangedEventArgs<AeroColorScheme> e)
            {
                AeroColors.CurrentScheme = e.NewValue;
            }

            internal void <StartPart2>b__20_6(object sender, HelpEventArgs e)
            {
                HelpService.Instance.ShowHelp(sender as IWin32Window);
                e.Handled = true;
            }
        }

        private sealed class PdnTraceListener : DefaultTraceListener
        {
            public override void Fail(string message)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }

            public override void Fail(string message, string detailMessage)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
        }
    }
}

