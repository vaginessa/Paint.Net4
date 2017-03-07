namespace PaintDotNet.Diagnostics
{
    using PaintDotNet.Dialogs;
    using PaintDotNet.Resources;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    internal static class CrashManager
    {
        private static bool alreadyReported = false;
        private static readonly object sync = new object();

        private static int GetExitCodeForException(Exception ex)
        {
            int errorCode;
            if (ex is Win32Exception)
            {
                errorCode = ((Win32Exception) ex).ErrorCode;
            }
            else if (ex is COMException)
            {
                errorCode = ((COMException) ex).ErrorCode;
            }
            else
            {
                errorCode = Marshal.GetHRForException(ex);
            }
            if (errorCode == 0)
            {
                errorCode = -1;
            }
            return errorCode;
        }

        public static string GetSecondParagraphForMessage(string crashLogText)
        {
            if (IsCrashCausedByMirillisAction(crashLogText))
            {
                string format = PdnResources.GetString("CrashLogDialog.Message.MayBeCausedBy.DisableOrRemove.Format");
                string str2 = PdnResources.GetString("CrashLogDialog.MayBeCausedBy.MirillisAction");
                return string.Format(format, str2);
            }
            return string.Empty;
        }

        private static bool IsCrashCausedByMirillisAction(string crashLogText) => 
            ((crashLogText.Contains("System.AccessViolationException") || crashLogText.Contains("PaintDotNet.Direct2D.RecreateTargetException")) && (crashLogText.Contains(@"\Mirillis\Action!\action_x64.dll,") || crashLogText.Contains(@"\ActionRecorder\action_x64.dll,")));

        public static void ReportUnhandledException(Exception ex)
        {
            Validate.IsNotNull<Exception>(ex, "ex");
            object sync = CrashManager.sync;
            lock (sync)
            {
                if (alreadyReported)
                {
                    return;
                }
                alreadyReported = true;
            }
            ReportUnhandledExceptionImpl(ex);
        }

        private static void ReportUnhandledExceptionImpl(Exception ex)
        {
            try
            {
                string str = SaveCrashLog(ex);
                Uri uri = new Uri(Assembly.GetEntryAssembly().CodeBase);
                string localPath = uri.LocalPath;
                string arguments = "\"/showCrashLog=" + str + "\"";
                ProcessStartInfo startInfo = new ProcessStartInfo(localPath, arguments) {
                    UseShellExecute = false
                };
                Process.Start(startInfo);
                Process.GetCurrentProcess().Kill();
            }
            catch (Exception exception)
            {
                try
                {
                    MessageBox.Show(exception.ToString(), "paint.net", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    Process.GetCurrentProcess().Kill();
                }
                catch (Exception exception2)
                {
                    Environment.FailFast("paint.net", exception2);
                }
            }
        }

        public static string SaveCrashLog(Exception ex)
        {
            string searchPattern = "pdncrash.*.log";
            string crashLogDir = CrashLogDir;
            Directory.CreateDirectory(crashLogDir);
            try
            {
                FileSystem.EnableCompression(crashLogDir);
            }
            catch (Exception)
            {
            }
            string[] first = Directory.GetFiles(crashLogDir, searchPattern, SearchOption.TopDirectoryOnly);
            var enumerable = (from filePath in first
                orderby File.GetCreationTimeUtc(filePath) descending
                let fileName = Path.GetFileName(filePath)
                let fileNameNoExt = Path.ChangeExtension(fileName, null)
                let ordinalStr = fileNameNoExt.Substring("pdncrash.".Length)
                let ordinal = long.Parse(ordinalStr)
                select new { 
                    FilePath = filePath,
                    Ordinal = ordinal
                }).Take(0x13);
            long num = (from filePathAndOrdinal in enumerable select filePathAndOrdinal.Ordinal).FirstOrDefault<long>();
            long num2 = 1L + num;
            IEnumerable<string> enumerable2 = first.Except<string>(from p in enumerable select p.FilePath);
            string str3 = $"pdncrash.{num2}.log";
            string path = Path.Combine(crashLogDir, str3);
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.AutoFlush = true;
                CrashLog.WriteCrashLog(ex, writer);
            }
            FileSystem.EnableCompression(path);
            foreach (string str5 in enumerable2)
            {
                FileSystem.TryDeleteFile(str5);
            }
            return path;
        }

        public static DialogResult ShowCrashLogDialog(string crashLogPath)
        {
            object sync = CrashManager.sync;
            lock (sync)
            {
                alreadyReported = true;
            }
            string directoryName = Path.GetDirectoryName(crashLogPath);
            string crashLogText = File.ReadAllText(crashLogPath);
            using (ExceptionDialog dialog = new ExceptionDialog())
            {
                dialog.Text = PdnResources.GetString("CrashLogDialog.Title.Text");
                dialog.Message = PdnResources.GetString("CrashLogDialog.Message");
                dialog.Message2 = GetSecondParagraphForMessage(crashLogText);
                dialog.Button1Text = PdnResources.GetString("CrashLogDialog.RestartButton.Text");
                dialog.IsButton1Visible = true;
                dialog.Button2Text = PdnResources.GetString("CrashLogDialog.QuitButton.Text");
                dialog.ExceptionText = crashLogText;
                dialog.CrashLogDirectory = directoryName;
                dialog.ShowInTaskbar = true;
                return dialog.ShowDialog();
            }
        }

        public static string CrashLogDir =>
            Path.Combine(Path.Combine(ShellUtil.GetVirtualPath(VirtualFolderName.UserLocalAppData, true), "paint.net"), "CrashLogs");

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly CrashManager.<>c <>9 = new CrashManager.<>c();
            public static Func<string, DateTime> <>9__10_0;
            public static Func<string, <>f__AnonymousType0<string, string>> <>9__10_1;
            public static Func<<>f__AnonymousType0<string, string>, <>f__AnonymousType1<<>f__AnonymousType0<string, string>, string>> <>9__10_2;
            public static Func<<>f__AnonymousType1<<>f__AnonymousType0<string, string>, string>, <>f__AnonymousType2<<>f__AnonymousType1<<>f__AnonymousType0<string, string>, string>, string>> <>9__10_3;
            public static Func<<>f__AnonymousType2<<>f__AnonymousType1<<>f__AnonymousType0<string, string>, string>, string>, <>f__AnonymousType3<<>f__AnonymousType2<<>f__AnonymousType1<<>f__AnonymousType0<string, string>, string>, string>, long>> <>9__10_4;
            public static Func<<>f__AnonymousType3<<>f__AnonymousType2<<>f__AnonymousType1<<>f__AnonymousType0<string, string>, string>, string>, long>, <>f__AnonymousType4<string, long>> <>9__10_5;
            public static Func<<>f__AnonymousType4<string, long>, long> <>9__10_6;
            public static Func<<>f__AnonymousType4<string, long>, string> <>9__10_7;

            internal DateTime <SaveCrashLog>b__10_0(string filePath) => 
                File.GetCreationTimeUtc(filePath);

            internal <>f__AnonymousType0<string, string> <SaveCrashLog>b__10_1(string filePath) => 
                new { 
                    filePath = filePath,
                    fileName = Path.GetFileName(filePath)
                };

            internal <>f__AnonymousType1<<>f__AnonymousType0<string, string>, string> <SaveCrashLog>b__10_2(<>f__AnonymousType0<string, string> <>h__TransparentIdentifier0) => 
                new { 
                    <>h__TransparentIdentifier0 = <>h__TransparentIdentifier0,
                    fileNameNoExt = Path.ChangeExtension(<>h__TransparentIdentifier0.fileName, null)
                };

            internal <>f__AnonymousType2<<>f__AnonymousType1<<>f__AnonymousType0<string, string>, string>, string> <SaveCrashLog>b__10_3(<>f__AnonymousType1<<>f__AnonymousType0<string, string>, string> <>h__TransparentIdentifier1) => 
                new { 
                    <>h__TransparentIdentifier1 = <>h__TransparentIdentifier1,
                    ordinalStr = <>h__TransparentIdentifier1.fileNameNoExt.Substring("pdncrash.".Length)
                };

            internal <>f__AnonymousType3<<>f__AnonymousType2<<>f__AnonymousType1<<>f__AnonymousType0<string, string>, string>, string>, long> <SaveCrashLog>b__10_4(<>f__AnonymousType2<<>f__AnonymousType1<<>f__AnonymousType0<string, string>, string>, string> <>h__TransparentIdentifier2) => 
                new { 
                    <>h__TransparentIdentifier2 = <>h__TransparentIdentifier2,
                    ordinal = long.Parse(<>h__TransparentIdentifier2.ordinalStr)
                };

            internal <>f__AnonymousType4<string, long> <SaveCrashLog>b__10_5(<>f__AnonymousType3<<>f__AnonymousType2<<>f__AnonymousType1<<>f__AnonymousType0<string, string>, string>, string>, long> <>h__TransparentIdentifier3) => 
                new { 
                    FilePath = <>h__TransparentIdentifier3.<>h__TransparentIdentifier2.<>h__TransparentIdentifier1.<>h__TransparentIdentifier0.filePath,
                    Ordinal = <>h__TransparentIdentifier3.ordinal
                };

            internal long <SaveCrashLog>b__10_6(<>f__AnonymousType4<string, long> filePathAndOrdinal) => 
                filePathAndOrdinal.Ordinal;

            internal string <SaveCrashLog>b__10_7(<>f__AnonymousType4<string, long> p) => 
                p.FilePath;
        }
    }
}

