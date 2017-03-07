namespace PaintDotNet.Snap
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Threading;
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;

    internal abstract class SnapObstacle : ThreadAffinitizedObjectBase
    {
        protected RectInt32 bounds;
        private bool enabled;
        private string name;
        private ISnapObstaclePersist persistenceHandler;
        protected RectInt32 previousBounds;
        protected int snapDistance;
        private int snapProximity;
        private PaintDotNet.Snap.SnapRegion snapRegion;
        private bool stickyEdges;

        [field: CompilerGenerated]
        public event ValueChangedEventHandler<RectInt32> BoundsChanged;

        [field: CompilerGenerated]
        public event ValueEventHandler<RectInt32> BoundsChanging;

        internal SnapObstacle(string name, RectInt32 bounds, PaintDotNet.Snap.SnapRegion snapRegion, bool stickyEdges, ISnapObstaclePersist persistenceHandler) : this(name, bounds, snapRegion, stickyEdges, DefaultSnapProximity, DefaultSnapDistance, persistenceHandler)
        {
        }

        internal SnapObstacle(string name, RectInt32 bounds, PaintDotNet.Snap.SnapRegion snapRegion, bool stickyEdges, int snapProximity, int snapDistance, ISnapObstaclePersist persistenceHandler)
        {
            this.name = name;
            this.bounds = bounds;
            this.previousBounds = bounds;
            this.snapRegion = snapRegion;
            this.stickyEdges = stickyEdges;
            this.snapProximity = snapProximity;
            this.snapDistance = snapDistance;
            this.enabled = true;
            this.persistenceHandler = persistenceHandler;
        }

        protected virtual void OnBoundsChanged()
        {
            base.VerifyAccess();
            this.BoundsChanged.Raise<RectInt32>(this, this.previousBounds, this.bounds);
        }

        protected virtual void OnBoundsChangeRequested(RectInt32 newBounds, ref bool handled)
        {
            base.VerifyAccess();
        }

        protected virtual void OnBoundsChanging()
        {
            base.VerifyAccess();
            this.BoundsChanging.Raise<RectInt32>(this, this.Bounds);
        }

        public bool RequestBoundsChange(RectInt32 newBounds)
        {
            base.VerifyAccess();
            bool handled = false;
            this.OnBoundsChangeRequested(newBounds, ref handled);
            return handled;
        }

        public RectInt32 Bounds
        {
            get
            {
                base.VerifyAccess();
                return this.bounds;
            }
        }

        public static int DefaultSnapDistance
        {
            get
            {
                if (OS.IsWin10OrLater && !SystemInformation.HighContrast)
                {
                    return -3;
                }
                return 3;
            }
        }

        public static int DefaultSnapProximity =>
            15;

        public bool Enabled
        {
            get
            {
                base.VerifyAccess();
                return this.enabled;
            }
            set
            {
                base.VerifyAccess();
                this.enabled = value;
            }
        }

        public bool IsPersistenceEnabled
        {
            get
            {
                base.VerifyAccess();
                return (this.persistenceHandler > null);
            }
        }

        public string Name =>
            this.name;

        public ISnapObstaclePersist PersistenceHandler =>
            this.persistenceHandler;

        public int SnapDistance
        {
            get
            {
                base.VerifyAccess();
                return this.snapDistance;
            }
        }

        public int SnapProximity
        {
            get
            {
                base.VerifyAccess();
                return this.snapProximity;
            }
        }

        public PaintDotNet.Snap.SnapRegion SnapRegion
        {
            get
            {
                base.VerifyAccess();
                return this.snapRegion;
            }
        }

        public bool StickyEdges
        {
            get
            {
                base.VerifyAccess();
                return this.stickyEdges;
            }
        }
    }
}

