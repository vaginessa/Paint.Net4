namespace PaintDotNet.AppModel
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Dialogs;
    using PaintDotNet.Resources;
    using PaintDotNet.Settings.App;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Collections.Generic;
    using System.IO;

    internal sealed class UserFilesService : Disposable, IUserFilesService
    {
        private string currentUserFilesPath;
        private static UserFilesService instance;
        private Dictionary<string, string> resNameToSubDirNameMap = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        private object sync = new object();

        public UserFilesService()
        {
            AppSettings.Instance.UI.Language.ValueChangedT += new ValueChangedEventHandler<CultureInfo>(this.OnLanguageChanged);
            this.currentUserFilesPath = this.TryGetUserFilesPath();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                AppSettings.Instance.UI.Language.ValueChangedT -= new ValueChangedEventHandler<CultureInfo>(this.OnLanguageChanged);
            }
            base.Dispose(disposing);
        }

        public string GetLocalizedDirectoryPath(string subDirResName)
        {
            object sync = this.Sync;
            lock (sync)
            {
                if (!this.resNameToSubDirNameMap.ContainsKey(subDirResName))
                {
                    throw new ArgumentException("a localized path must first be registered with RegisterLocalizedDirectory", "subDirResName");
                }
                string userFilesPath = this.UserFilesPath;
                string str2 = PdnResources.GetString(subDirResName);
                return Path.Combine(userFilesPath, str2);
            }
        }

        public static void Initialize()
        {
            if (instance == null)
            {
                instance = new UserFilesService();
            }
        }

        private void OnLanguageChanged(object sender, ValueChangedEventArgs<CultureInfo> e)
        {
            List<TupleStruct<string, string>> list = new List<TupleStruct<string, string>>();
            object sync = this.Sync;
            lock (sync)
            {
                string b = this.TryGetUserFilesPath();
                if ((this.currentUserFilesPath != null) && (b != null))
                {
                    if (!string.Equals(this.currentUserFilesPath, b, StringComparison.InvariantCultureIgnoreCase))
                    {
                        list.Add(TupleStruct.Create<string, string>(this.currentUserFilesPath, b));
                        this.currentUserFilesPath = b;
                    }
                    foreach (string str2 in this.resNameToSubDirNameMap.Keys.ToArrayEx<string>())
                    {
                        string a = this.resNameToSubDirNameMap[str2];
                        string str4 = PdnResources.GetString(str2);
                        if (!string.Equals(a, str4, StringComparison.InvariantCultureIgnoreCase))
                        {
                            string str5 = Path.Combine(b, a);
                            string str6 = Path.Combine(b, str4);
                            this.resNameToSubDirNameMap[str2] = str4;
                            list.Add(TupleStruct.Create<string, string>(str5, str6));
                        }
                    }
                }
            }
            foreach (TupleStruct<string, string> struct2 in list)
            {
                string path = struct2.Item1;
                string str8 = struct2.Item2;
                try
                {
                    if (Directory.Exists(path) && !Directory.Exists(str8))
                    {
                        Directory.Move(path, str8);
                    }
                }
                catch (Exception exception)
                {
                    ExceptionDialog.ShowErrorDialog(null, exception);
                }
            }
        }

        public void RegisterLocalizedDirectory(string subDirResName)
        {
            object sync = this.Sync;
            lock (sync)
            {
                if (!this.resNameToSubDirNameMap.ContainsKey(subDirResName))
                {
                    string str = PdnResources.GetString(subDirResName);
                    this.resNameToSubDirNameMap.Add(subDirResName, str);
                }
            }
        }

        public string TryGetUserFilesPath()
        {
            try
            {
                return this.UserFilesPath;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static UserFilesService Instance
        {
            get
            {
                if (instance == null)
                {
                    ExceptionUtil.ThrowInvalidOperationException("Must call Initialize() first");
                }
                return instance;
            }
        }

        private object Sync =>
            this.sync;

        public string UserFilesPath
        {
            get
            {
                string virtualPath = ShellUtil.GetVirtualPath(VirtualFolderName.UserDocuments, true);
                string str2 = PdnResources.GetString("SystemLayer.UserDataDirName");
                return Path.Combine(virtualPath, str2);
            }
        }
    }
}

