namespace PaintDotNet.Runtime
{
    using PaintDotNet.AppModel;
    using PaintDotNet.Collections;
    using System;
    using System.Collections.Generic;

    internal static class PersistedObjectLocker
    {
        private static readonly Dictionary<Guid, WeakReference> guidToPO = new Dictionary<Guid, WeakReference>();
        private static readonly object sync = new object();

        static PersistedObjectLocker()
        {
            CleanupService.AddCleanupSource(new OurCleanupSource());
        }

        public static Guid Add<T>(PersistedObject<T> po) where T: class
        {
            Guid key = Guid.NewGuid();
            WeakReference reference = new WeakReference(po);
            object sync = PersistedObjectLocker.sync;
            lock (sync)
            {
                guidToPO.Add(key, reference);
            }
            return key;
        }

        private static void Cleanup()
        {
            object sync = PersistedObjectLocker.sync;
            lock (sync)
            {
                foreach (KeyValuePair<Guid, WeakReference> pair in guidToPO.ToArrayEx<KeyValuePair<Guid, WeakReference>>())
                {
                    if ((pair.Value.Target == null) && !pair.Value.IsAlive)
                    {
                        guidToPO.Remove(pair.Key);
                    }
                }
            }
        }

        public static void Remove(Guid guid)
        {
            object sync = PersistedObjectLocker.sync;
            lock (sync)
            {
                guidToPO.Remove(guid);
            }
        }

        public static PersistedObject<T> TryGet<T>(Guid guid) where T: class
        {
            WeakReference reference;
            object sync = PersistedObjectLocker.sync;
            lock (sync)
            {
                guidToPO.TryGetValue(guid, out reference);
            }
            if (reference == null)
            {
                return null;
            }
            object target = reference.Target;
            if (target == null)
            {
                Remove(guid);
                return null;
            }
            return (PersistedObject<T>) target;
        }

        private sealed class OurCleanupSource : CleanupSource
        {
            protected override void OnPerformCleanup()
            {
                PersistedObjectLocker.Cleanup();
            }
        }
    }
}

