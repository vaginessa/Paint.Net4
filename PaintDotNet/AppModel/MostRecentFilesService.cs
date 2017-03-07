namespace PaintDotNet.AppModel
{
    using PaintDotNet.Collections;
    using PaintDotNet.Settings.App;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;

    internal sealed class MostRecentFilesService
    {
        private Queue<MostRecentFile> files = new Queue<MostRecentFile>();
        private const int iconSize = 0x38;
        private static readonly MostRecentFilesService instance = new MostRecentFilesService();
        private bool isLoaded;
        private int maxCount = AppSettings.Instance.File.MostRecent.MaxCount;
        private object sync = new object();

        private MostRecentFilesService()
        {
        }

        public void Add(MostRecentFile mrf)
        {
            object sync = this.Sync;
            lock (sync)
            {
                if (!this.IsLoaded)
                {
                    this.LoadMruList();
                }
                if (!this.Contains(mrf.Path))
                {
                    this.files.Enqueue(mrf);
                    while (this.files.Count > this.maxCount)
                    {
                        this.files.Dequeue();
                    }
                }
            }
        }

        public void Clear()
        {
            object sync = this.Sync;
            lock (sync)
            {
                if (!this.IsLoaded)
                {
                    this.LoadMruList();
                }
                foreach (MostRecentFile file in this.GetFileList())
                {
                    this.Remove(file.Path);
                }
            }
        }

        public bool Contains(string fileName)
        {
            object sync = this.Sync;
            lock (sync)
            {
                if (!this.IsLoaded)
                {
                    this.LoadMruList();
                }
                foreach (MostRecentFile file in this.files)
                {
                    if (string.Equals(fileName, file.Path, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public MostRecentFile[] GetFileList()
        {
            object sync = this.Sync;
            lock (sync)
            {
                if (!this.IsLoaded)
                {
                    this.LoadMruList();
                }
                return this.files.ToArrayEx<MostRecentFile>();
            }
        }

        public void LoadMruList()
        {
            object sync = this.Sync;
            lock (sync)
            {
                try
                {
                    this.isLoaded = true;
                    this.Clear();
                    for (int i = 0; i < this.MaxCount; i++)
                    {
                        try
                        {
                            string str = AppSettings.Instance.File.MostRecent.Paths[i].Value;
                            if (!string.IsNullOrWhiteSpace(str))
                            {
                                byte[] buffer = AppSettings.Instance.File.MostRecent.Thumbnails[i].Value;
                                if (buffer.Length != 0)
                                {
                                    using (MemoryStream stream = new MemoryStream(buffer))
                                    {
                                        Image thumb = Image.FromStream(stream);
                                        MostRecentFile mrf = new MostRecentFile(str, thumb);
                                        this.Add(mrf);
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                catch (Exception)
                {
                    this.Clear();
                }
            }
        }

        public void Remove(string fileName)
        {
            object sync = this.Sync;
            lock (sync)
            {
                if (!this.IsLoaded)
                {
                    this.LoadMruList();
                }
                if (this.Contains(fileName))
                {
                    Queue<MostRecentFile> queue = new Queue<MostRecentFile>();
                    foreach (MostRecentFile file in this.files)
                    {
                        if (string.Compare(file.Path, fileName, StringComparison.InvariantCultureIgnoreCase) != 0)
                        {
                            queue.Enqueue(file);
                        }
                    }
                    this.files = queue;
                }
            }
        }

        public void SaveMruList()
        {
            object sync = this.Sync;
            lock (sync)
            {
                if (this.IsLoaded)
                {
                    MostRecentFile[] fileList = this.GetFileList();
                    for (int i = 0; i < AppSettings.Instance.File.MostRecent.MaxCount; i++)
                    {
                        AppSettings.Instance.File.MostRecent.Paths[i].Reset();
                        AppSettings.Instance.File.MostRecent.Thumbnails[i].Reset();
                    }
                    for (int j = 0; j < this.Count; j++)
                    {
                        try
                        {
                            AppSettings.Instance.File.MostRecent.Paths[j].Value = fileList[j].Path;
                            using (MemoryStream stream = new MemoryStream())
                            {
                                fileList[j].Thumb.Save(stream, ImageFormat.Png);
                                stream.Flush();
                                AppSettings.Instance.File.MostRecent.Thumbnails[j].Value = stream.GetBuffer();
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
        }

        public int Count
        {
            get
            {
                object sync = this.Sync;
                lock (sync)
                {
                    if (!this.IsLoaded)
                    {
                        this.LoadMruList();
                    }
                    return this.files.Count;
                }
            }
        }

        public int IconSize =>
            UIUtil.ScaleWidth(0x38);

        public static MostRecentFilesService Instance =>
            instance;

        public bool IsLoaded
        {
            get
            {
                object sync = this.Sync;
                lock (sync)
                {
                    return this.isLoaded;
                }
            }
        }

        public int MaxCount =>
            this.maxCount;

        private object Sync =>
            this.sync;
    }
}

