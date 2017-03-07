namespace PaintDotNet.Snap
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal sealed class SnapObstacleController : SnapObstacle
    {
        [field: CompilerGenerated]
        public event HandledEventHandler<RectInt32> BoundsChangeRequested;

        public SnapObstacleController(string name, RectInt32 bounds, SnapRegion snapRegion, bool stickyEdges, ISnapObstaclePersist persistenceHandler) : base(name, bounds, snapRegion, stickyEdges, persistenceHandler)
        {
        }

        public SnapObstacleController(string name, RectInt32 bounds, SnapRegion snapRegion, bool stickyEdges, int snapProximity, int snapDistance, ISnapObstaclePersist persistenceHandler) : base(name, bounds, snapRegion, stickyEdges, snapProximity, snapDistance, persistenceHandler)
        {
        }

        protected override void OnBoundsChangeRequested(RectInt32 newBounds, ref bool handled)
        {
            base.VerifyAccess();
            if (this.BoundsChangeRequested != null)
            {
                HandledEventArgs<RectInt32> e = new HandledEventArgs<RectInt32>(handled, newBounds);
                this.BoundsChangeRequested(this, e);
                handled = e.Handled;
            }
            base.OnBoundsChangeRequested(newBounds, ref handled);
        }

        public void SetBounds(RectInt32 bounds)
        {
            base.VerifyAccess();
            if (base.bounds != bounds)
            {
                this.OnBoundsChanging();
                base.previousBounds = base.bounds;
                base.bounds = bounds;
                this.OnBoundsChanged();
            }
        }

        public void SetSnapDistance(int newSnapDistance)
        {
            base.VerifyAccess();
            if (base.snapDistance != newSnapDistance)
            {
                base.snapDistance = newSnapDistance;
            }
        }
    }
}

