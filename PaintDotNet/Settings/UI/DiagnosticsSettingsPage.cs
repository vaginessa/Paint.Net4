namespace PaintDotNet.Settings.UI
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Dialogs;
    using PaintDotNet.Dxgi;
    using PaintDotNet.Resources;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    internal sealed class DiagnosticsSettingsPage : SettingsDialogPage
    {
        private PdnPushButton copyToClipboardButton;
        private PictureBox folderIconBox;
        private static readonly Func<int, string> getIndentStringFn = Func.Memoize<int, string>(new Func<int, string>(<>c.<>9.<.cctor>b__20_0));
        private const int indentSize = 4;
        private ColumnHeader keyColumnHeader;
        private DoubleBufferedListView listView;
        private PdnLinkLabel openCrashLogFolderLink;
        private readonly DiagnosticsSettingsSection section;
        private ColumnHeader valueColumnHeader;

        public DiagnosticsSettingsPage(DiagnosticsSettingsSection section) : base(section)
        {
            this.section = section;
            base.SuspendLayout();
            this.listView = new DoubleBufferedListView();
            this.listView.SuspendLayout();
            this.listView.BeginUpdate();
            this.listView.Name = "listView";
            this.listView.Scrollable = true;
            this.listView.View = View.Details;
            this.listView.AllowColumnReorder = false;
            this.keyColumnHeader = new ColumnHeader();
            this.keyColumnHeader.Text = "Item";
            this.keyColumnHeader.Width = 100;
            this.listView.Columns.Add(this.keyColumnHeader);
            this.valueColumnHeader = new ColumnHeader();
            this.valueColumnHeader.Text = "Value";
            this.listView.Columns.Add(this.valueColumnHeader);
            foreach (KeyValuePair<string, string> pair in this.GetRows())
            {
                if (pair.Key == null)
                {
                    this.listView.Items.Add(string.Empty);
                }
                else
                {
                    ListViewItem item = new ListViewItem(pair.Key) {
                        SubItems = { pair.Value }
                    };
                    this.listView.Items.Add(item);
                }
            }
            this.listView.EndUpdate();
            this.copyToClipboardButton = new PdnPushButton();
            this.copyToClipboardButton.AutoSize = true;
            this.copyToClipboardButton.Text = PdnResources.GetString("ExceptionDialog.CopyToClipboardButton.Text");
            this.copyToClipboardButton.Click += new EventHandler(this.OnCopyToClipboardButtonClick);
            this.folderIconBox = new PictureBox();
            this.folderIconBox.SizeMode = PictureBoxSizeMode.StretchImage;
            this.folderIconBox.Image = PdnResources.GetImageResource("Icons.FolderShortcut.png").Reference;
            this.openCrashLogFolderLink = new PdnLinkLabel();
            this.openCrashLogFolderLink.Text = PdnResources.GetString("ExceptionDialog.OpenFolderLink.Text");
            this.openCrashLogFolderLink.LinkClicked += new LinkLabelLinkClickedEventHandler(this.OnOpenCrashLogFolderLinkLinkClicked);
            this.openCrashLogFolderLink.AutoSize = true;
            base.Controls.Add(this.listView);
            base.Controls.Add(this.copyToClipboardButton);
            base.Controls.Add(this.folderIconBox);
            base.Controls.Add(this.openCrashLogFolderLink);
            this.listView.ResumeLayout();
            base.ResumeLayout(true);
            this.listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private static KeyValuePair<string, string> CreateBlankRow() => 
            new KeyValuePair<string, string>(null, null);

        private static KeyValuePair<string, string> CreateRow(int indentLevel, string key, string value) => 
            new KeyValuePair<string, string>(GetIndentString(indentLevel) + key, value);

        private static string GetIndentString(int indentLevel) => 
            getIndentStringFn(indentLevel);

        private static string GetOSName(int majorVersion, int minorVersion, OSType type)
        {
            if (type != OSType.Workstation)
            {
                if ((type == OSType.Server) && (majorVersion == 6))
                {
                    switch (minorVersion)
                    {
                        case 1:
                            return "Windows 2008 R2";

                        case 2:
                            return "Windows Server 2012";

                        case 3:
                            return "Windows Server 2012 R2";
                    }
                }
            }
            else if (majorVersion != 6)
            {
                if ((majorVersion == 10) && (minorVersion == 0))
                {
                    return "Windows 10";
                }
            }
            else
            {
                switch (minorVersion)
                {
                    case 1:
                        return "Windows 7";

                    case 2:
                        return "Windows 8";

                    case 3:
                        return "Windows 8.1";
                }
            }
            return "Windows";
        }

        private static void GetProcessorCounts(out int socketCount, out int coreCount, out int threadCount)
        {
            threadCount = Environment.ProcessorCount;
            LogicalProcessorInfo logicalProcessorInformation = Processor.GetLogicalProcessorInformation();
            socketCount = logicalProcessorInformation.Packages.Count;
            coreCount = logicalProcessorInformation.GetPhysicalCoreCount();
        }

        [IteratorStateMachine(typeof(<GetRows>d__14))]
        private IEnumerable<KeyValuePair<string, string>> GetRows()
        {
            yield return CreateRow(0, "Application", PdnInfo.FullAppName);
            yield return CreateRow(0, "Build Date", PdnInfo.BuildTime.ToLongDateString());
            if (PdnInfo.WillExpire)
            {
                yield return CreateRow(0, "Expiration Date", PdnInfo.ExpirationDate.ToLongDateString());
            }
            yield return CreateBlankRow();
            yield return CreateRow(0, PdnResources.GetString("SettingsDialog.UI.EnableHardwareAcceleration.Description"), this.Section.AppSettings.UI.EnableHardwareAcceleration.Value.ToString(CultureInfo.CurrentUICulture));
            yield return CreateRow(0, PdnResources.GetString("SettingsDialog.UI.EnableAnimations.Description"), this.Section.AppSettings.UI.EnableAnimations.Value.ToString(CultureInfo.CurrentUICulture));
            float num2 = 96f * UIUtil.GetXScaleFactor();
            yield return CreateRow(0, "DPI", $"{num2.ToString("F2")} ({UIUtil.GetXScaleFactor().ToString("F2")}x scale)");
            yield return CreateRow(0, PdnResources.GetString("SettingsDialog.UI.Language.DisplayName"), this.Section.AppSettings.UI.Language.Value.Name);
            yield return CreateBlankRow();
            OperatingSystem oSVersion = Environment.OSVersion;
            string str = GetOSName(oSVersion.Version.Major, oSVersion.Version.Minor, OS.OSType);
            yield return CreateRow(0, "OS", $"{str}{string.IsNullOrWhiteSpace(OS.Revision) ? string.Empty : (" " + OS.Revision)} ({oSVersion.Version.ToString(4)})");
            yield return CreateRow(0, ".NET Runtime", Environment.Version.ToString(4));
            ulong num3 = Memory.TotalPhysicalBytes >> 20;
            yield return CreateRow(0, "Physical Memory", $"{num3.ToString("N0")} MB");
            this.<threadCount>5__4 = Environment.ProcessorCount;
            LogicalProcessorInfo processorInfo = Processor.GetLogicalProcessorInformation();
            this.<socketCount>5__1 = processorInfo.Packages.Count;
            this.<coreCount>5__3 = processorInfo.GetPhysicalCoreCount();
            yield return CreateBlankRow();
            if (this.<socketCount>5__1 < 2)
            {
                yield return CreateRow(0, "CPU", Processor.CpuName);
            }
            else
            {
                yield return CreateRow(0, "CPU", $"{this.<socketCount>5__1}x {Processor.CpuName}");
            }
            ProcessorArchitecture nativeArchitecture = Processor.NativeArchitecture;
            if (nativeArchitecture == ProcessorArchitecture.X86)
            {
                this.<procArchStr>5__2 = "x86 (32-bit)";
            }
            else if (nativeArchitecture == ProcessorArchitecture.X64)
            {
                this.<procArchStr>5__2 = "x64 (64-bit)";
            }
            else
            {
                this.<procArchStr>5__2 = "Unknown";
            }
            yield return CreateRow(1, "Architecture", this.<procArchStr>5__2);
            yield return CreateRow(1, "Process Mode", $"{Processor.Architecture.ToBitness()}-bit");
            yield return CreateRow(1, "Speed", $"~{Processor.ApproximateSpeedMhz} MHz");
            yield return CreateRow(1, "Cores / Threads", $"{this.<coreCount>5__3} / {this.<threadCount>5__4}");
            string str2 = Enum.GetValues(typeof(ProcessorFeature)).Cast<ProcessorFeature>().Where<ProcessorFeature>(new Func<ProcessorFeature, bool>(Processor.IsFeaturePresent)).Select<ProcessorFeature, string>((<>c.<>9__14_0 ?? (<>c.<>9__14_0 = new Func<ProcessorFeature, string>(<>c.<>9.<GetRows>b__14_0)))).Join(", ");
            yield return CreateRow(1, "Features", str2);
            yield return CreateBlankRow();
            using (this.<dxgiFactory1>5__11 = DxgiFactory1.CreateFactory1())
            {
                this.<adapters>5__5 = this.<dxgiFactory1>5__11.EnumerateAdapters1().ToArrayEx<IDxgiAdapter1>();
                this.<i>5__10 = 0;
                while (this.<i>5__10 < this.<adapters>5__5.Length)
                {
                    int length;
                    this.<adapter>5__7 = this.<adapters>5__5[this.<i>5__10];
                    this.<description>5__6 = this.<adapter>5__7.Description1;
                    if (this.<i>5__10 != 0)
                    {
                        yield return CreateBlankRow();
                    }
                    yield return CreateRow(0, "Video Card", this.<description>5__6.Description);
                    long num4 = this.<description>5__6.DedicatedVideoMemory >> 20;
                    yield return CreateRow(1, "Dedicated Video RAM", $"{num4.ToString("N0")} MB");
                    num4 = this.<description>5__6.DedicatedSystemMemory >> 20;
                    yield return CreateRow(1, "Dedicated System RAM", $"{num4.ToString("N0")} MB");
                    num4 = this.<description>5__6.SharedSystemMemory >> 20;
                    yield return CreateRow(1, "Shared System RAM", $"{num4.ToString("N0")} MB");
                    yield return CreateRow(1, "Vendor ID", $"0x{this.<description>5__6.VendorID.ToString("X4")}");
                    yield return CreateRow(1, "Device ID", $"0x{this.<description>5__6.DeviceID.ToString("X4")}");
                    yield return CreateRow(1, "Subsystem ID", $"0x{this.<description>5__6.SubSystemID.ToString("X8")}");
                    yield return CreateRow(1, "Revision", this.<description>5__6.Revision.ToString());
                    yield return CreateRow(1, "LUID", $"0x{this.<description>5__6.AdapterLuid.ToString("X8")}");
                    yield return CreateRow(1, "Flags", this.<description>5__6.Flags.ToString());
                    this.<outputs>5__8 = this.<adapter>5__7.EnumerateOutputs().ToArrayEx<IDxgiOutput>();
                    this.<attachedOutputs>5__9 = this.<outputs>5__8.Count<IDxgiOutput>(<>c.<>9__14_1 ?? (<>c.<>9__14_1 = new Func<IDxgiOutput, bool>(<>c.<>9.<GetRows>b__14_1)));
                    if (this.<attachedOutputs>5__9 == this.<outputs>5__8.Length)
                    {
                        yield return CreateRow(1, "Outputs", this.<outputs>5__8.Length.ToString());
                    }
                    else
                    {
                        length = this.<outputs>5__8.Length;
                        yield return CreateRow(1, "Outputs", $"{length.ToString()} ({this.<attachedOutputs>5__9.ToString()} attached)");
                    }
                    DisposableUtil.FreeContents<IDxgiOutput>(this.<outputs>5__8);
                    this.<adapter>5__7 = null;
                    this.<description>5__6 = new AdapterDescription1();
                    this.<outputs>5__8 = null;
                    length = this.<i>5__10 + 1;
                    this.<i>5__10 = length;
                }
                DisposableUtil.FreeContents<IDxgiAdapter1>(this.<adapters>5__5);
                this.<adapters>5__5 = null;
            }
            this.<dxgiFactory1>5__11 = null;
        }

        private void OnCopyToClipboardButtonClick(object sender, EventArgs e)
        {
            try
            {
                PdnClipboard.SetText((from r in this.GetRows() select $"{r.Key}	{r.Value}").Join(Environment.NewLine));
            }
            catch (Exception exception)
            {
                ExceptionDialog.ShowErrorDialog(this, exception);
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int num = UIUtil.ScaleHeight(8);
            int num2 = UIUtil.ScaleHeight(8);
            Size size = UIUtil.ScaleSize(0x4b, 0x18);
            Size size2 = new Size(base.ClientSize.Width, base.PanelHeight);
            base.ClientSize = size2;
            this.copyToClipboardButton.Size = size;
            this.copyToClipboardButton.PerformLayout();
            this.copyToClipboardButton.Location = new Point(0, size2.Height - this.copyToClipboardButton.Height);
            this.folderIconBox.Size = UIUtil.ScaleSize(this.folderIconBox.Image.Size);
            this.folderIconBox.Location = new Point(this.copyToClipboardButton.Right + ((num * 3) / 2), this.copyToClipboardButton.Top + ((this.copyToClipboardButton.Height - this.folderIconBox.Height) / 2));
            this.openCrashLogFolderLink.Size = this.openCrashLogFolderLink.GetPreferredSize(new Size(1, 1));
            this.openCrashLogFolderLink.Location = new Point(this.folderIconBox.Right + (num / 2), this.folderIconBox.Top + ((this.folderIconBox.Height - this.openCrashLogFolderLink.Height) / 2));
            int height = MathUtil.Min(this.copyToClipboardButton.Top, this.folderIconBox.Top, this.openCrashLogFolderLink.Top) - num2;
            this.listView.Bounds = new Rectangle(0, 0, size2.Width, height);
            base.OnLayout(levent);
        }

        private void OnOpenCrashLogFolderLinkLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                string crashLogDir = CrashManager.CrashLogDir;
                if (!Directory.Exists(crashLogDir))
                {
                    Directory.CreateDirectory(crashLogDir);
                }
                ShellUtil.BrowseFolder2(this, crashLogDir);
            }
            catch (Exception exception)
            {
                ExceptionDialog.ShowErrorDialog(this, exception);
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly DiagnosticsSettingsPage.<>c <>9 = new DiagnosticsSettingsPage.<>c();
            public static Func<ProcessorFeature, string> <>9__14_0;
            public static Func<IDxgiOutput, bool> <>9__14_1;
            public static Func<KeyValuePair<string, string>, string> <>9__18_0;

            internal string <.cctor>b__20_0(int i) => 
                new string(' ', i * 4);

            internal string <GetRows>b__14_0(ProcessorFeature pf) => 
                pf.ToString();

            internal bool <GetRows>b__14_1(IDxgiOutput o) => 
                o.Description.IsAttachedToDesktop;

            internal string <OnCopyToClipboardButtonClick>b__18_0(KeyValuePair<string, string> r) => 
                $"{r.Key}	{r.Value}";
        }


        private class DoubleBufferedListView : ListView
        {
            public DoubleBufferedListView()
            {
                this.DoubleBuffered = true;
            }
        }
    }
}

