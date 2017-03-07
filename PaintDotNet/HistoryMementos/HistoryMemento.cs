namespace PaintDotNet.HistoryMementos
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Runtime;
    using PaintDotNet.Threading;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal abstract class HistoryMemento
    {
        private readonly ProtectedRegion flushRegion = new ProtectedRegion("Flush", ProtectedRegionOptions.ErrorOnMultithreadedAccess | ProtectedRegionOptions.ErrorOnPerThreadReentrancy | ProtectedRegionOptions.DisablePumpingWhenEntered);
        private PersistedObject<HistoryMementoData> historyMementoData;
        private long id;
        private ImageResource image;
        private string name;
        private static long nextId;
        private Guid seriesGuid = Guid.Empty;

        public HistoryMemento(string name, ImageResource image)
        {
            this.name = name;
            this.image = image;
            this.id = Interlocked.Increment(ref nextId);
        }

        public static HistoryMemento Combine(string name, ImageResource image, IEnumerable<HistoryMemento> mementos)
        {
            HistoryMemento[] actions = (from m in mementos
                where m > null
                select m).ToArrayEx<HistoryMemento>();
            if (actions.Length == 0)
            {
                ExceptionUtil.ThrowArgumentException("One of the mementos must be non-null!");
            }
            return new CompoundHistoryMemento(name, image, actions);
        }

        public static HistoryMemento Combine(string name, ImageResource image, params HistoryMemento[] mementos) => 
            Combine(name, image, (IEnumerable<HistoryMemento>) mementos);

        public void Flush()
        {
            using (this.flushRegion.UseEnterScope())
            {
                if (this.historyMementoData != null)
                {
                    this.historyMementoData.Flush();
                }
                this.OnFlush();
            }
        }

        protected virtual void OnFlush()
        {
        }

        protected abstract HistoryMemento OnUndo(ProgressEventHandler progressCallback);
        public HistoryMemento PerformUndo(ProgressEventHandler progressCallback = null)
        {
            ProgressEventHandler handler = progressCallback ?? (<>c.<>9__25_0 ?? (<>c.<>9__25_0 = new ProgressEventHandler(<>c.<>9.<PerformUndo>b__25_0)));
            HistoryMemento memento = this.OnUndo(handler);
            memento.ID = this.ID;
            memento.SeriesGuid = this.SeriesGuid;
            return memento;
        }

        protected HistoryMementoData Data
        {
            get => 
                this.historyMementoData?.Object;
            set
            {
                this.historyMementoData = new PersistedObject<HistoryMementoData>(value, false);
            }
        }

        public long ID
        {
            get => 
                this.id;
            set
            {
                this.id = value;
            }
        }

        public ImageResource Image
        {
            get => 
                this.image;
            set
            {
                this.image = value;
            }
        }

        public string Name
        {
            get => 
                this.name;
            set
            {
                this.name = value;
            }
        }

        public Guid SeriesGuid
        {
            get => 
                this.seriesGuid;
            set
            {
                this.seriesGuid = value;
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly HistoryMemento.<>c <>9 = new HistoryMemento.<>c();
            public static ProgressEventHandler <>9__25_0;
            public static Func<HistoryMemento, bool> <>9__28_0;

            internal bool <Combine>b__28_0(HistoryMemento m) => 
                (m > null);

            internal void <PerformUndo>b__25_0(object s, ProgressEventArgs e)
            {
            }
        }
    }
}

