namespace PaintDotNet.Runtime
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Serialization;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Threading;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class PersistedObject<T> : IDisposable where T: class
    {
        private IntPtr bstrTempFileName;
        private bool disposed;
        private static readonly ConcurrentSet<string> fileNames;
        private WeakReference objectRef;
        private static readonly object persistBackgroundSync;
        private readonly ProtectedRegion persistedToDiskRegion;
        private string tempFileName;
        private ManualResetEvent theObjectSaved;
        private readonly ProtectedRegion waitForObjectSavedRegion;

        static PersistedObject()
        {
            PersistedObject<T>.persistBackgroundSync = new object();
            PersistedObject<T>.fileNames = new ConcurrentSet<string>();
            Application.ApplicationExit += new EventHandler(PersistedObject<T>.OnApplicationExit);
        }

        public PersistedObject(T theObject, bool background)
        {
            this.bstrTempFileName = IntPtr.Zero;
            this.theObjectSaved = new ManualResetEvent(false);
            this.waitForObjectSavedRegion = new ProtectedRegion("WaitForObjectSaved", ProtectedRegionOptions.DisablePumpingWhenEntered);
            this.persistedToDiskRegion = new ProtectedRegion("PersistToDisk", ProtectedRegionOptions.DisablePumpingWhenEntered);
            this.objectRef = new WeakReference(theObject);
            this.tempFileName = FileSystem.GetTempFileName();
            PersistedObject<T>.fileNames.Add(this.tempFileName);
            this.bstrTempFileName = Marshal.StringToBSTR(this.tempFileName);
            if (background)
            {
                ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(this.PersistToDiskThread), theObject);
            }
            else
            {
                this.PersistToDisk(theObject);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                this.disposed = true;
                if (disposing)
                {
                    this.WaitForObjectSaved(0x3e8);
                }
                string fileName = Marshal.PtrToStringBSTR(this.bstrTempFileName);
                Marshal.FreeBSTR(this.bstrTempFileName);
                this.bstrTempFileName = IntPtr.Zero;
                FileInfo info = new FileInfo(fileName);
                if (info.Exists)
                {
                    bool flag = FileSystem.TryDeleteFile(info.FullName);
                    try
                    {
                        PersistedObject<T>.fileNames.Remove(fileName);
                    }
                    catch (Exception)
                    {
                    }
                }
                this.theObjectSaved = null;
            }
        }

        ~PersistedObject()
        {
            this.Dispose(false);
        }

        public void Flush()
        {
            this.WaitForObjectSaved();
            object weakObject = this.WeakObject;
            IDisposable disposable = weakObject as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
                disposable = null;
            }
            this.objectRef = null;
        }

        private static void OnApplicationExit(object sender, EventArgs e)
        {
            string[] fileNames = PersistedObject<T>.FileNames;
            if (fileNames.Length != 0)
            {
                foreach (string str in fileNames)
                {
                    try
                    {
                        FileInfo info = new FileInfo(str);
                        if (info.Exists)
                        {
                            bool flag = FileSystem.TryDeleteFile(info.FullName);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        private void PersistToDisk(object theObject)
        {
            using (this.persistedToDiskRegion.UseEnterScope())
            {
                try
                {
                    object persistBackgroundSync = PersistedObject<T>.persistBackgroundSync;
                    lock (persistBackgroundSync)
                    {
                        using (FileStream stream = new FileStream(this.tempFileName, FileMode.Create, FileAccess.Write, FileShare.Read))
                        {
                            BinaryFormatter formatter = new BinaryFormatter();
                            KeyValuePair<object, object>[] items = new KeyValuePair<object, object>[] { KeyValuePairUtil.Create<object, object>(MemoryBlock.UseCompressionBooleanDeferredFormatterKey, true) };
                            DeferredFormatter additional = new DeferredFormatter(null, ArrayUtil.Infer<KeyValuePair<object, object>>(items));
                            StreamingContext context = new StreamingContext(formatter.Context.State, additional);
                            formatter.Context = context;
                            formatter.Serialize(stream, theObject);
                            additional.FinishSerialization(stream);
                            stream.Flush();
                        }
                    }
                }
                finally
                {
                    this.theObjectSaved.Set();
                    this.theObjectSaved = null;
                }
                GC.KeepAlive(theObject);
            }
        }

        private void PersistToDiskThread(object theObject)
        {
            this.PersistToDisk(theObject);
        }

        private void WaitForObjectSaved()
        {
            using (this.waitForObjectSavedRegion.UseEnterScope())
            {
                ManualResetEvent theObjectSaved = this.theObjectSaved;
                if (theObjectSaved != null)
                {
                    theObjectSaved.WaitOne();
                }
            }
        }

        private void WaitForObjectSaved(int timeoutMs)
        {
            using (this.waitForObjectSavedRegion.UseEnterScope())
            {
                ManualResetEvent theObjectSaved = this.theObjectSaved;
                if (theObjectSaved != null)
                {
                    theObjectSaved.WaitOne(timeoutMs, false);
                }
            }
        }

        internal static string[] FileNames =>
            PersistedObject<T>.fileNames.ToArrayEx<string>();

        public T Object
        {
            get
            {
                T target;
                if (this.disposed)
                {
                    throw new ObjectDisposedException("PersistedObject");
                }
                if (this.objectRef == null)
                {
                    target = default(T);
                }
                else
                {
                    target = (T) this.objectRef.Target;
                }
                if (target != null)
                {
                    return target;
                }
                using (FileStream stream = new FileStream(Marshal.PtrToStringBSTR(this.bstrTempFileName), FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    SerializationFallbackBinder binder = new SerializationFallbackBinder();
                    binder.SetNextRequiredBaseType(typeof(T));
                    formatter.Binder = binder;
                    DeferredFormatter additional = new DeferredFormatter();
                    StreamingContext context = new StreamingContext(formatter.Context.State, additional);
                    formatter.Context = context;
                    T local2 = (T) formatter.Deserialize(stream);
                    additional.FinishDeserialization(stream);
                    this.objectRef = new WeakReference(local2);
                    return local2;
                }
            }
        }

        public T WeakObject
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException("PersistedObject");
                }
                if (this.objectRef == null)
                {
                    return default(T);
                }
                return (T) this.objectRef.Target;
            }
        }
    }
}

