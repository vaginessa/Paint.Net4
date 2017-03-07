namespace PaintDotNet.Updates
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Resources;
    using PaintDotNet.Settings.App;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Threading;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    internal class CheckingState : UpdatesState
    {
        private ManualResetEvent abortEvent;
        private const string betaVersionsName = "BetaVersions";
        private const int channelIndex = 8;
        private ManualResetEvent checkingEvent;
        private const char commentChar = ';';
        private const string downloadPageUrlName = "DownloadPageUrl";
        private Exception exception;
        private const string fullZipUrlListNameFormat = "{0}_FullZipUrlList";
        private const string infoUrlNameFormat = "{0}_InfoUrl";
        private int latestVersionIndex;
        private PdnVersionManifest manifest;
        private const string nameNameFormat = "{0}_Name";
        private const string netFxVersionNameFormat = "{0}_NetFxVersion";
        private const string stableVersionsName = "StableVersions";
        private const string versionManifestRelativeUrlFormat = "/updates/versions.{0}.{1}.{2}.{3}.{4}.txt";
        private static readonly string versionManifestTestRelativeUrl = ("/updates/versions." + 8.ToString(CultureInfo.InvariantCulture) + ".test.txt");
        private const string zipUrlListNameFormat = "{0}_ZipUrlList";

        public CheckingState() : base(false, false, MarqueeStyle.Marquee)
        {
            this.checkingEvent = new ManualResetEvent(false);
            this.abortEvent = new ManualResetEvent(false);
        }

        private static string[] BreakIntoLines(string text)
        {
            string str;
            StringReader reader = new StringReader(text);
            List<string> items = new List<string>();
            while ((str = reader.ReadLine()) != null)
            {
                if ((str.Length > 0) && (str[0] != ';'))
                {
                    items.Add(str);
                }
            }
            return items.ToArrayEx<string>();
        }

        private static string[] BuildVersionValueMapping(NameValueCollection nameValues, Version[] versions, string secondaryKeyFormat)
        {
            string[] strArray = new string[versions.Length];
            for (int i = 0; i < versions.Length; i++)
            {
                string str = versions[i].ToString();
                string str2 = string.Format(secondaryKeyFormat, str);
                strArray[i] = nameValues[str2];
            }
            return strArray;
        }

        private static void CheckForUpdates(out PdnVersionManifest manifestResult, out int latestVersionIndexResult, out Exception exception)
        {
            exception = null;
            PdnVersionManifest updatesManifest = null;
            manifestResult = null;
            latestVersionIndexResult = -1;
            int num = 2;
            while (num > 0)
            {
                try
                {
                    updatesManifest = GetUpdatesManifest(out exception);
                    num = 0;
                    continue;
                }
                catch (Exception exception2)
                {
                    exception = exception2;
                    num--;
                    if (num == 0)
                    {
                        updatesManifest = null;
                    }
                    continue;
                }
            }
            if (updatesManifest != null)
            {
                int latestStableVersionIndex = updatesManifest.GetLatestStableVersionIndex();
                int latestBetaVersionIndex = updatesManifest.GetLatestBetaVersionIndex();
                bool flag2 = AppSettings.Instance.Updates.AutoCheckForPrerelease.Value || PdnInfo.IsPrereleaseBuild;
                int index = latestStableVersionIndex;
                if ((flag2 && (latestBetaVersionIndex != -1)) && ((latestStableVersionIndex == -1) || (updatesManifest.VersionInfos[latestBetaVersionIndex].Version >= updatesManifest.VersionInfos[latestStableVersionIndex].Version)))
                {
                    index = latestBetaVersionIndex;
                }
                if ((index != -1) && (PdnInfo.IsTestMode || (updatesManifest.VersionInfos[index].Version > PdnInfo.Version)))
                {
                    manifestResult = updatesManifest;
                    latestVersionIndexResult = index;
                }
            }
        }

        private void DoCheckThreadProc(object ignored)
        {
            try
            {
                Thread.Sleep(0x5dc);
                CheckForUpdates(out this.manifest, out this.latestVersionIndex, out this.exception);
            }
            finally
            {
                this.checkingEvent.Set();
            }
        }

        private static string GetNeutralLocaleName(CultureInfo ci)
        {
            if (ci.IsNeutralCulture)
            {
                return ci.Name;
            }
            if (ci.Parent == null)
            {
                return ci.Name;
            }
            if (ci.Parent == ci)
            {
                return ci.Name;
            }
            return GetNeutralLocaleName(ci.Parent);
        }

        private static PdnVersionManifest GetUpdatesManifest(out Exception exception)
        {
            try
            {
                Uri uri = new Uri(VersionManifestUrl);
                byte[] bytes = WebHelpers.DownloadSmallFile(uri);
                NameValueCollection nameValues = LinesToNameValues(BreakIntoLines(Encoding.UTF8.GetString(bytes)));
                string downloadPageUrl = nameValues["DownloadPageUrl"];
                string versions = nameValues["StableVersions"];
                Version[] versionArray = VersionStringToArray(versions);
                string[] strArray2 = BuildVersionValueMapping(nameValues, versionArray, "{0}_Name");
                string[] strArray3 = BuildVersionValueMapping(nameValues, versionArray, "{0}_NetFxVersion");
                string[] strArray4 = BuildVersionValueMapping(nameValues, versionArray, "{0}_InfoUrl");
                string[] strArray5 = BuildVersionValueMapping(nameValues, versionArray, "{0}_ZipUrlList");
                string[] strArray6 = BuildVersionValueMapping(nameValues, versionArray, "{0}_FullZipUrlList");
                string str5 = nameValues["BetaVersions"];
                Version[] versionArray2 = VersionStringToArray(str5);
                string[] strArray7 = BuildVersionValueMapping(nameValues, versionArray2, "{0}_Name");
                string[] strArray8 = BuildVersionValueMapping(nameValues, versionArray2, "{0}_NetFxVersion");
                string[] strArray9 = BuildVersionValueMapping(nameValues, versionArray2, "{0}_InfoUrl");
                string[] strArray10 = BuildVersionValueMapping(nameValues, versionArray2, "{0}_ZipUrlList");
                string[] strArray11 = BuildVersionValueMapping(nameValues, versionArray2, "{0}_FullZipUrlList");
                PdnVersionInfo[] versionInfos = new PdnVersionInfo[versionArray2.Length + versionArray.Length];
                int index = 0;
                for (int i = 0; i < versionArray.Length; i++)
                {
                    List<string> urlsOutput = new List<string>();
                    SplitUrlList(strArray5[i], urlsOutput);
                    List<string> list2 = new List<string>();
                    SplitUrlList(strArray6[i], list2);
                    Version version = new Version(strArray3[i]);
                    if ((version.Major == 2) && (version.Minor == 0))
                    {
                        version = new Version(2, 0, 0);
                    }
                    versionInfos[index] = new PdnVersionInfo(versionArray[i], strArray2[i], version.Major, version.Minor, version.Build, strArray4[i], urlsOutput.ToArrayEx<string>(), list2.ToArrayEx<string>(), true);
                    index++;
                }
                for (int j = 0; j < versionArray2.Length; j++)
                {
                    List<string> list3 = new List<string>();
                    SplitUrlList(strArray10[j], list3);
                    List<string> list4 = new List<string>();
                    SplitUrlList(strArray11[j], list4);
                    Version version2 = new Version(strArray8[j]);
                    if ((version2.Major == 2) && (version2.Minor == 0))
                    {
                        version2 = new Version(2, 0, 0);
                    }
                    versionInfos[index] = new PdnVersionInfo(versionArray2[j], strArray7[j], version2.Major, version2.Minor, version2.Build, strArray9[j], list3.ToArrayEx<string>(), list4.ToArrayEx<string>(), false);
                    index++;
                }
                PdnVersionManifest manifest = new PdnVersionManifest(downloadPageUrl, versionInfos);
                exception = null;
                return manifest;
            }
            catch (Exception exception2)
            {
                exception = exception2;
                return null;
            }
        }

        private static NameValueCollection LinesToNameValues(string[] lines)
        {
            NameValueCollection values = new NameValueCollection();
            foreach (string str in lines)
            {
                string str2;
                string str3;
                LineToNameValue(str, out str2, out str3);
                values.Add(str2, str3);
            }
            return values;
        }

        private static void LineToNameValue(string line, out string name, out string value)
        {
            int index = line.IndexOf('=');
            if (index == -1)
            {
                throw new FormatException("Line had no equal sign (=) present");
            }
            name = line.Substring(0, index);
            if (((line.Length - index) - 1) == 0)
            {
                value = string.Empty;
            }
            else
            {
                value = line.Substring(index + 1, (line.Length - index) - 1);
            }
        }

        protected override void OnAbort()
        {
            this.abortEvent.Set();
            base.OnAbort();
        }

        public override void OnEnteredState()
        {
            this.checkingEvent.Reset();
            this.abortEvent.Reset();
            Work.QueueWorkItem(new WaitCallback(this.DoCheckThreadProc));
            int num = new WaitHandleArray(2) { 
                [0] = this.checkingEvent,
                [1] = this.abortEvent
            }.WaitAny();
            if (((num == 0) && (this.manifest != null)) && (this.latestVersionIndex != -1))
            {
                base.StateMachine.QueueInput(PrivateInput.GoToUpdateAvailable);
            }
            else if (num == 1)
            {
                base.StateMachine.QueueInput(PrivateInput.GoToAborted);
            }
            else if (this.exception != null)
            {
                base.StateMachine.QueueInput(PrivateInput.GoToError);
            }
            else
            {
                base.StateMachine.QueueInput(PrivateInput.GoToDone);
            }
        }

        public override void ProcessInput(object input, out PaintDotNet.Updates.State newState)
        {
            if (input.Equals(PrivateInput.GoToUpdateAvailable))
            {
                newState = new UpdateAvailableState(this.manifest.VersionInfos[this.latestVersionIndex]);
            }
            else if (input.Equals(PrivateInput.GoToError))
            {
                string str;
                if (this.exception is WebException)
                {
                    str = UpdatesState.WebExceptionToErrorMessage((WebException) this.exception);
                }
                else
                {
                    str = PdnResources.GetString("Updates.CheckingState.GenericError");
                }
                newState = new ErrorState(this.exception, str);
            }
            else if (input.Equals(PrivateInput.GoToDone))
            {
                newState = new DoneState();
            }
            else
            {
                if (!input.Equals(PrivateInput.GoToAborted))
                {
                    throw new ArgumentException();
                }
                newState = new AbortedState();
            }
        }

        private static void SplitUrlList(string urlList, List<string> urlsOutput)
        {
            if (!string.IsNullOrEmpty(urlList))
            {
                string str2;
                int num;
                string str3;
                string str = urlList.Trim();
                if (str[0] == '"')
                {
                    int index = str.IndexOf('"', 1);
                    num = str.IndexOf(',', index);
                    str2 = str.Substring(1, index - 1);
                }
                else
                {
                    num = str.IndexOf(',');
                    if (num == -1)
                    {
                        str2 = str;
                    }
                    else
                    {
                        str2 = str.Substring(0, num);
                    }
                }
                if (num == -1)
                {
                    str3 = null;
                }
                else
                {
                    str3 = str.Substring(num + 1);
                }
                urlsOutput.Add(str2);
                SplitUrlList(str3, urlsOutput);
            }
        }

        private static Version[] VersionStringToArray(string versions)
        {
            char[] separator = new char[] { ',' };
            string[] strArray = versions.Split(separator);
            if ((strArray.Length == 0) || ((strArray.Length == 1) && (strArray[0].Length == 0)))
            {
                return Array.Empty<Version>();
            }
            Version[] versionArray = new Version[strArray.Length];
            for (int i = 0; i < strArray.Length; i++)
            {
                versionArray[i] = new Version(strArray[i]);
            }
            return versionArray;
        }

        public override bool CanAbort =>
            true;

        private static string VersionManifestUrl
        {
            get
            {
                Uri baseUri = new Uri("http://www.getpaint.net");
                if (PdnInfo.IsTestMode)
                {
                    Uri uri2 = new Uri(baseUri, versionManifestTestRelativeUrl);
                    return uri2.ToString();
                }
                string str2 = 8.ToString(CultureInfo.InvariantCulture);
                Version version = Environment.OSVersion.Version;
                ProcessorArchitecture architecture = Processor.Architecture;
                OSType oSType = OS.OSType;
                if (((version.Major == 5) && (version.Minor == 2)) && ((architecture == ProcessorArchitecture.X64) && (oSType == OSType.Workstation)))
                {
                    version = new Version(5, 1, version.Build, version.Revision);
                }
                string str3 = ((version.Major * 100) + version.Minor).ToString(CultureInfo.InvariantCulture);
                string str4 = OS.ServicePackMajor.ToString(CultureInfo.InvariantCulture);
                string str5 = architecture.ToString().ToLower();
                string neutralLocaleName = GetNeutralLocaleName(AppSettings.Instance.UI.Language.Value);
                Uri uri3 = new Uri(baseUri, "/updates/versions.{0}.{1}.{2}.{3}.{4}.txt");
                return string.Format(uri3.ToString(), new object[] { str2, str3, str4, str5, neutralLocaleName });
            }
        }
    }
}

