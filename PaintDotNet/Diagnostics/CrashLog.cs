namespace PaintDotNet.Diagnostics
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Dxgi;
    using PaintDotNet.Functional;
    using PaintDotNet.Resources;
    using PaintDotNet.Settings;
    using PaintDotNet.Settings.App;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.VisualStyling;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Windows.Forms;

    internal static class CrashLog
    {
        public static string GetCrashLogHeader()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter stream = new StringWriter(sb);
            WriteCrashLog(null, stream);
            return sb.ToString();
        }

        public static void WriteCrashLog(Exception crashEx, TextWriter stream)
        {
            string str;
            string str2;
            DateTime startupTime = Startup.StartupTime;
            try
            {
                str = PdnResources.GetString("CrashLog.HeaderText.Format");
            }
            catch (Exception exception)
            {
                str = "This text file was created because paint.net crashed.\r\nPlease e-mail this file to {0} so we can diagnose and fix the problem.\r\n, --- Exception while calling PdnResources.GetString(\"CrashLog.HeaderText.Format\"): " + exception.ToString() + Environment.NewLine;
            }
            try
            {
                str2 = string.Format(str, "crashlog4@getpaint.net");
            }
            catch (Exception)
            {
                str2 = string.Empty;
            }
            stream.WriteLine(str2);
            string fullAppName = "err";
            string str4 = "err";
            string str5 = "err";
            string str6 = "err";
            string str7 = "err";
            string str8 = "err";
            string str9 = "err";
            string currentDirectory = "err";
            string str11 = "err";
            string revision = "err";
            string str13 = "err";
            string str14 = "err";
            string str15 = "err";
            string str16 = "err";
            string str17 = "err";
            string cpuName = "err";
            string str19 = "err";
            string str20 = "err";
            string str21 = "err";
            string str22 = "err";
            string str23 = "err";
            string str24 = "err";
            string str25 = "err";
            string str26 = "err";
            string str27 = "err";
            string str28 = "err";
            string str29 = "err";
            string str30 = "err";
            string str31 = "err";
            string str32 = "err";
            try
            {
                try
                {
                    fullAppName = PdnInfo.FullAppName;
                }
                catch (Exception exception2)
                {
                    fullAppName = Application.ProductVersion + ", --- Exception while calling PdnInfo.GetFullAppName(): " + exception2.ToString() + Environment.NewLine;
                }
                try
                {
                    str4 = DateTime.Now.ToString();
                }
                catch (Exception exception3)
                {
                    str4 = "--- Exception while populating timeOfCrash: " + exception3.ToString() + Environment.NewLine;
                }
                try
                {
                    str5 = ((TimeSpan) (DateTime.Now - startupTime)).ToString();
                }
                catch (Exception exception4)
                {
                    str5 = "--- Exception while populating appUptime: " + exception4.ToString() + Environment.NewLine;
                }
                try
                {
                    bool hasShutdownStarted = Environment.HasShutdownStarted;
                    bool flag2 = AppDomain.CurrentDomain.IsFinalizingForUnload();
                    str6 = Startup.State.ToString() + " " + (hasShutdownStarted ? "Environment.HasShutdownStarted " : string.Empty) + (flag2 ? "AppDomain.IsFinalizingForUnload " : string.Empty).Trim();
                }
                catch (Exception exception5)
                {
                    str6 = "--- Exception while populating applicationState: " + exception5.ToString() + Environment.NewLine;
                }
                try
                {
                    str7 = ((((double) Environment.WorkingSet) / 1024.0)).ToString("N0") + " KiB";
                }
                catch (Exception exception6)
                {
                    str7 = "--- Exception while populating workingSet: " + exception6.ToString() + Environment.NewLine;
                }
                try
                {
                    int num4;
                    int num5;
                    Process currentProcess = Process.GetCurrentProcess();
                    int handleCount = currentProcess.HandleCount;
                    int count = currentProcess.Threads.Count;
                    ProcessStatus.GetCurrentProcessGuiResources(out num4, out num5);
                    str8 = $"{handleCount} handles, {count} threads, {num4} gdi, {num5} user";
                }
                catch (Exception exception7)
                {
                    str8 = "--- Exception while populating threadCount: " + exception7.ToString() + Environment.NewLine;
                }
                try
                {
                    currentDirectory = Environment.CurrentDirectory;
                }
                catch (Exception exception8)
                {
                    currentDirectory = "--- Exception while populating currentDir: " + exception8.ToString() + Environment.NewLine;
                }
                try
                {
                    str9 = RegistrySettings.SystemWide.GetString("TARGETDIR", "n/a");
                }
                catch (Exception exception9)
                {
                    str9 = "--- Exception while populating targetDir: " + exception9.ToString() + Environment.NewLine;
                }
                try
                {
                    str11 = Environment.OSVersion.Version.ToString();
                }
                catch (Exception exception10)
                {
                    str11 = "--- Exception while populating osVersion: " + exception10.ToString() + Environment.NewLine;
                }
                try
                {
                    revision = OS.Revision;
                }
                catch (Exception exception11)
                {
                    revision = "--- Exception while populating osRevision: " + exception11.ToString() + Environment.NewLine;
                }
                try
                {
                    str13 = OS.OSType.ToString();
                }
                catch (Exception exception12)
                {
                    str13 = "--- Exception while populating osType: " + exception12.ToString() + Environment.NewLine;
                }
                try
                {
                    str14 = Processor.NativeArchitecture.ToString().ToLower();
                }
                catch (Exception exception13)
                {
                    str14 = "--- Exception while populating processorNativeArchitecture: " + exception13.ToString() + Environment.NewLine;
                }
                try
                {
                    str15 = Environment.Version.ToString();
                }
                catch (Exception exception14)
                {
                    str15 = "--- Exception while populating clrVersion: " + exception14.ToString() + Environment.NewLine;
                }
                try
                {
                    bool flag3 = OS.VerifyFrameworkVersion(4, 6, 0, OS.FrameworkProfile.Client);
                    bool flag4 = OS.VerifyFrameworkVersion(4, 6, 0, OS.FrameworkProfile.Full);
                    str16 = ((flag3 | flag4) ? "4.6 " : string.Empty).Trim();
                }
                catch (Exception exception15)
                {
                    str16 = "--- Exception while populating fxInventory: " + exception15.ToString() + Environment.NewLine;
                }
                try
                {
                    str17 = Processor.Architecture.ToString().ToLower();
                }
                catch (Exception exception16)
                {
                    str17 = "--- Exception while populating processorArchitecture: " + exception16.ToString() + Environment.NewLine;
                }
                try
                {
                    cpuName = Processor.CpuName;
                }
                catch (Exception exception17)
                {
                    cpuName = "--- Exception while populating cpuName: " + exception17.ToString() + Environment.NewLine;
                }
                try
                {
                    LogicalProcessorInfo logicalProcessorInformation = Processor.GetLogicalProcessorInformation();
                    int num6 = logicalProcessorInformation.Packages.Count;
                    int physicalCoreCount = logicalProcessorInformation.GetPhysicalCoreCount();
                    int logicalCpuCount = Processor.LogicalCpuCount;
                    if (num6 > 1)
                    {
                        str19 = $"{num6.ToString()}S/{physicalCoreCount.ToString()}C/{logicalCpuCount.ToString()}T";
                    }
                    else
                    {
                        str19 = $"{physicalCoreCount.ToString()}C/{logicalCpuCount.ToString()}T";
                    }
                }
                catch (Exception exception18)
                {
                    str19 = "--- Exception while populating cpuCount: " + exception18.ToString() + Environment.NewLine;
                }
                try
                {
                    str20 = "@ ~" + Processor.ApproximateSpeedMhz.ToString() + "MHz";
                }
                catch (Exception exception19)
                {
                    str20 = "--- Exception while populating cpuSpeed: " + exception19.ToString() + Environment.NewLine;
                }
                try
                {
                    str21 = "(" + str19;
                    foreach (string str33 in Enum.GetNames(typeof(ProcessorFeature)))
                    {
                        ProcessorFeature feature = (ProcessorFeature) Enum.Parse(typeof(ProcessorFeature), str33);
                        if (Processor.IsFeaturePresent(feature))
                        {
                            str21 = str21 + ", ";
                            str21 = str21 + str33;
                        }
                    }
                    if (str21.Length > 0)
                    {
                        str21 = str21 + ")";
                    }
                }
                catch (Exception exception20)
                {
                    str21 = "--- Exception while populating cpuFeatures: " + exception20.ToString() + Environment.NewLine;
                }
                try
                {
                    str22 = ((Memory.TotalPhysicalBytes / ((ulong) 0x400L)) / ((ulong) 0x400L)) + " MB";
                }
                catch (Exception exception21)
                {
                    str22 = "--- Exception while populating totalPhysicalBytes: " + exception21.ToString() + Environment.NewLine;
                }
                try
                {
                    BooleanSetting enableHardwareAcceleration = AppSettings.Instance.UI.EnableHardwareAcceleration;
                    str23 = enableHardwareAcceleration.Value.ToString() + " (default: " + enableHardwareAcceleration.DefaultValue.ToString() + ")";
                }
                catch (Exception exception22)
                {
                    str23 = "--- Exception while populating hwAcceleration: " + exception22.ToString() + Environment.NewLine;
                }
                try
                {
                    using (IDxgiFactory1 factory = DxgiFactory1.CreateFactory1())
                    {
                        IDxgiAdapter1[] items = factory.EnumerateAdapters1().ToArrayEx<IDxgiAdapter1>();
                        str24 = items.Select<IDxgiAdapter1, AdapterDescription1>(a => a.Description1).Select<AdapterDescription1, string>(d => $"{d.Description} (v:{d.VendorID.ToString("X")}, d:{d.DeviceID.ToString("X")}, r:{d.Revision.ToString()})").Join(", ");
                        DisposableUtil.FreeContents<IDxgiAdapter1>(items);
                    }
                }
                catch (Exception exception23)
                {
                    str24 = "--- Exception while populating videoCardNames: " + exception23.ToString() + Environment.NewLine;
                }
                try
                {
                    str25 = AppSettings.Instance.UI.EnableAnimations.Value.ToString();
                }
                catch (Exception exception24)
                {
                    str25 = "--- Exception while populating animations: " + exception24.ToString() + Environment.NewLine;
                }
                try
                {
                    float xScaleFactor = UIUtil.GetXScaleFactor();
                    str26 = $"{(96f * xScaleFactor).ToString("F2")} dpi ({xScaleFactor.ToString("F2")}x scale)";
                }
                catch (Exception exception25)
                {
                    str26 = "--- Exception while populating dpiInfo: " + exception25.ToString() + Environment.NewLine;
                }
                try
                {
                    VisualStyleClass visualStyleClass = UIUtil.VisualStyleClass;
                    PdnTheme effectiveTheme = ThemeConfig.EffectiveTheme;
                    bool isDwmCompositionEnabled = UIUtil.IsDwmCompositionEnabled;
                    string themeFileName = UIUtil.ThemeFileName;
                    str27 = $"{visualStyleClass.ToString()}/{effectiveTheme.ToString()}{isDwmCompositionEnabled ? " + DWM" : ""} ({themeFileName})";
                }
                catch (Exception exception26)
                {
                    str27 = "--- Exception while populating themeInfo: " + exception26.ToString() + Environment.NewLine;
                }
                try
                {
                    string str36;
                    string str37;
                    string path = AppSettings.Instance.UI.Language.Path;
                    bool flag8 = AppSettings.Instance.StorageHandler.TryGet(SettingsHive.CurrentUser, path, out str36);
                    bool flag9 = AppSettings.Instance.StorageHandler.TryGet(SettingsHive.SystemWide, path, out str37);
                    string[] textArray1 = new string[] { "pdnr.c: ", PdnResources.Culture.Name, ", hklm: ", flag9 ? str37 : "n/a", ", hkcu: ", flag8 ? str36 : "n/a", ", cc: ", CultureInfo.CurrentCulture.Name, ", cuic: ", CultureInfo.CurrentUICulture.Name };
                    str29 = string.Concat(textArray1);
                }
                catch (Exception exception27)
                {
                    str29 = "--- Exception while populating localeName: " + exception27.ToString() + Environment.NewLine;
                }
                try
                {
                    bool flag10 = AppSettings.Instance.Updates.AutoCheck.Value;
                    str28 = $"{flag10}, {AppSettings.Instance.Updates.LastCheckTimeUtc.Value.ToShortDateString()}";
                }
                catch (Exception exception28)
                {
                    str28 = "--- Exception while populating updaterInfo: " + exception28.ToString() + Environment.NewLine;
                }
                try
                {
                    List<string> strings = new List<string>();
                    if (AppSettings.Instance.UI.ErrorFlagsAtStartup.Value != AppSettings.Instance.UI.ErrorFlags.Value)
                    {
                        strings.Add("ErrorFlagsAtStartup=(" + AppSettings.Instance.UI.ErrorFlagsAtStartup.Value.ToString() + ")");
                    }
                    if (AppSettings.Instance.UI.ErrorFlags.Value != AppSettings.Instance.UI.ErrorFlags.DefaultValue)
                    {
                        strings.Add("ErrorFlags=(" + AppSettings.Instance.UI.ErrorFlags.Value.ToString() + ")");
                    }
                    if (AppSettings.Instance.UI.DefaultTextAntialiasMode.Value != AppSettings.Instance.UI.DefaultTextAntialiasMode.DefaultValue)
                    {
                        strings.Add("DefaultTextAntialiasMode=" + AppSettings.Instance.UI.DefaultTextAntialiasMode.Value.ToString());
                    }
                    if (AppSettings.Instance.UI.DefaultTextRenderingMode.Value != AppSettings.Instance.UI.DefaultTextRenderingMode.DefaultValue)
                    {
                        strings.Add("DefaultTextRenderingMode=" + AppSettings.Instance.UI.DefaultTextRenderingMode.Value.ToString());
                    }
                    if (AppSettings.Instance.UI.EnableCanvasHwndRenderTarget.Value != AppSettings.Instance.UI.EnableCanvasHwndRenderTarget.DefaultValue)
                    {
                        strings.Add("EnableCanvasHwndRenderTarget=" + AppSettings.Instance.UI.EnableCanvasHwndRenderTarget.Value.ToString());
                    }
                    if (AppSettings.Instance.UI.EnableHighQualityScaling.Value != AppSettings.Instance.UI.EnableHighQualityScaling.DefaultValue)
                    {
                        strings.Add("EnableHighQualityScaling=" + AppSettings.Instance.UI.EnableHighQualityScaling.Value.ToString());
                    }
                    if (AppSettings.Instance.UI.EnableSmoothMouseInput.Value != AppSettings.Instance.UI.EnableSmoothMouseInput.DefaultValue)
                    {
                        strings.Add("EnableSmoothMouseInput=" + AppSettings.Instance.UI.EnableSmoothMouseInput.Value.ToString());
                    }
                    str30 = strings.Join(", ");
                }
                catch (Exception exception29)
                {
                    str30 = "--- Exception while populating flagsInfo: " + exception29.ToString() + Environment.NewLine;
                }
                try
                {
                    StringBuilder builder = new StringBuilder();
                    Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    for (int i = 0; i < assemblies.Length; i++)
                    {
                        Assembly assembly = assemblies[i];
                        string str38 = () => assembly.FullName.Eval<string>().GetValueOrDefault() ?? "<FullName?>";
                        string str39 = () => assembly.Location.Eval<string>().GetValueOrDefault() ?? "<Location?>";
                        builder.AppendFormat("{0}    {1} @ {2}", Environment.NewLine, str38, str39);
                    }
                    str31 = builder.ToString();
                }
                catch (Exception exception30)
                {
                    str31 = "--- Exception while populating assembliesInfo: " + exception30.ToString() + Environment.NewLine;
                }
                try
                {
                    StringBuilder builder2 = new StringBuilder();
                    int num14 = Processor.Architecture.ToBitness();
                    foreach (ProcessStatus.ModuleFileNameAndBitness bitness in ProcessStatus.GetCurrentProcessModuleNames())
                    {
                        string fileVersion;
                        string str40 = string.Empty;
                        if (bitness.Bitness != num14)
                        {
                            str40 = $" ({bitness.Bitness.ToString()}-bit)";
                        }
                        try
                        {
                            fileVersion = FileVersionInfo.GetVersionInfo(bitness.FileName).FileVersion;
                        }
                        catch (Exception exception31)
                        {
                            fileVersion = $"ex: {exception31.GetType().FullName}";
                        }
                        object[] args = new object[] { Environment.NewLine, bitness.FileName, str40, fileVersion };
                        builder2.AppendFormat("{0}    {1}{2}, version={3}", args);
                    }
                    str32 = builder2.ToString();
                }
                catch (Exception exception32)
                {
                    str32 = "--- Exception while populating nativeModulesInfo: " + exception32.ToString() + Environment.NewLine;
                }
            }
            catch (Exception exception33)
            {
                stream.WriteLine("Exception while gathering app and system info: " + exception33.ToString());
            }
            stream.WriteLine("Application version: " + fullAppName);
            stream.WriteLine("Time of crash: " + str4);
            stream.WriteLine("Application uptime: " + str5);
            stream.WriteLine("Application state: " + str6);
            stream.WriteLine("Working set: " + str7);
            stream.WriteLine("Handles and threads: " + str8);
            stream.WriteLine("Install directory: " + str9);
            stream.WriteLine("Current directory: " + currentDirectory);
            stream.WriteLine("OS Version: " + str11 + (string.IsNullOrEmpty(revision) ? "" : (" " + revision)) + " " + str13 + " " + str14);
            stream.WriteLine(".NET version: CLR " + str15 + " " + str17 + ", FX " + str16);
            stream.WriteLine("Processor: \"" + cpuName + "\" " + str20 + " " + str21);
            stream.WriteLine("Physical memory: " + str22);
            stream.WriteLine("Video card: " + str24);
            stream.WriteLine("Hardware acceleration: " + str23);
            stream.WriteLine("UI animations: " + str25);
            stream.WriteLine("UI DPI: " + str26);
            stream.WriteLine("UI theme: " + str27);
            stream.WriteLine("Updates: " + str28);
            stream.WriteLine("Locale: " + str29);
            stream.WriteLine("Flags: " + str30);
            stream.WriteLine();
            stream.WriteLine("Exception details:");
            if (crashEx == null)
            {
                stream.WriteLine("(null)");
            }
            else
            {
                stream.WriteLine(crashEx.ToString());
                Exception[] loaderExceptions = null;
                if (crashEx is ReflectionTypeLoadException)
                {
                    loaderExceptions = ((ReflectionTypeLoadException) crashEx).LoaderExceptions;
                }
                if (loaderExceptions != null)
                {
                    for (int j = 0; j < loaderExceptions.Length; j++)
                    {
                        stream.WriteLine();
                        stream.WriteLine("Secondary exception details:");
                        if (loaderExceptions[j] == null)
                        {
                            stream.WriteLine("(null)");
                        }
                        else
                        {
                            stream.WriteLine(loaderExceptions[j].ToString());
                        }
                    }
                }
            }
            stream.WriteLine();
            stream.WriteLine("Managed assemblies: " + str31);
            stream.WriteLine();
            stream.WriteLine("Native modules: " + str32);
            stream.WriteLine("------------------------------------------------------------------------------");
            stream.Flush();
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly CrashLog.<>c <>9 = new CrashLog.<>c();
            public static Func<IDxgiAdapter1, AdapterDescription1> <>9__1_0;
            public static Func<AdapterDescription1, string> <>9__1_1;

            internal AdapterDescription1 <WriteCrashLog>b__1_0(IDxgiAdapter1 a) => 
                a.Description1;

            internal string <WriteCrashLog>b__1_1(AdapterDescription1 d) => 
                $"{d.Description} (v:{d.VendorID.ToString("X")}, d:{d.DeviceID.ToString("X")}, r:{d.Revision.ToString()})";
        }
    }
}

