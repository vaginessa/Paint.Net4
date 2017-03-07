namespace PaintDotNet.Snap
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Rendering;
    using PaintDotNet.Threading;
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;

    internal sealed class SnapManager : ThreadAffinitizedObjectBase
    {
        private Dictionary<SnapObstacle, SnapDescription> obstacles = new Dictionary<SnapObstacle, SnapDescription>();

        public void AddSnapObstacle(ISnapObstacleHost snapObstacleHost)
        {
            base.VerifyAccess();
            snapObstacleHost.VerifyAccess();
            this.AddSnapObstacle(snapObstacleHost.SnapObstacle);
        }

        public void AddSnapObstacle(SnapObstacle snapObstacle)
        {
            base.VerifyAccess();
            snapObstacle.VerifyAccess();
            if (!this.obstacles.ContainsKey(snapObstacle))
            {
                this.obstacles.Add(snapObstacle, null);
                if (snapObstacle.StickyEdges)
                {
                    snapObstacle.BoundsChanging += new ValueEventHandler<RectInt32>(this.OnSnapObstacleBoundsChanging);
                    snapObstacle.BoundsChanged += new ValueChangedEventHandler<RectInt32>(this.OnSnapObstacleBoundsChanged);
                }
            }
        }

        private static PointInt32 AdjustNewLocation(SnapObstacle obstacle, PointInt32 newLocation, SnapDescription snapDescription)
        {
            obstacle.VerifyAccess();
            if ((snapDescription == null) || ((snapDescription.HorizontalEdge == HorizontalSnapEdge.Neither) && (snapDescription.VerticalEdge == VerticalSnapEdge.Neither)))
            {
                return obstacle.Bounds.Location;
            }
            RectInt32 num = new RectInt32(newLocation, obstacle.Bounds.Size);
            RectInt32 bounds = snapDescription.SnappedTo.Bounds;
            HorizontalSnapEdge horizontalEdge = snapDescription.HorizontalEdge;
            VerticalSnapEdge verticalEdge = snapDescription.VerticalEdge;
            SnapRegion snapRegion = snapDescription.SnappedTo.SnapRegion;
            int num3 = 0;
            if ((horizontalEdge == HorizontalSnapEdge.Top) && (snapRegion == SnapRegion.Exterior))
            {
                int num7 = bounds.Top - snapDescription.YOffset;
                num3 = num.Bottom - num7;
            }
            else if ((horizontalEdge == HorizontalSnapEdge.Bottom) && (snapRegion == SnapRegion.Exterior))
            {
                int num8 = bounds.Bottom + snapDescription.YOffset;
                num3 = num.Top - num8;
            }
            else if ((horizontalEdge == HorizontalSnapEdge.Top) && (snapRegion == SnapRegion.Interior))
            {
                int num9 = Math.Min(bounds.Bottom, bounds.Top + snapDescription.YOffset);
                num3 = num.Top - num9;
            }
            else if ((horizontalEdge == HorizontalSnapEdge.Bottom) && (snapRegion == SnapRegion.Interior))
            {
                int num10 = Math.Max(bounds.Top, bounds.Bottom - snapDescription.YOffset);
                num3 = num.Bottom - num10;
            }
            int num4 = 0;
            if ((verticalEdge == VerticalSnapEdge.Left) && (snapRegion == SnapRegion.Exterior))
            {
                int num11 = bounds.Left - snapDescription.XOffset;
                num4 = num.Right - num11;
            }
            else if ((verticalEdge == VerticalSnapEdge.Right) && (snapRegion == SnapRegion.Exterior))
            {
                int num12 = bounds.Right + snapDescription.XOffset;
                num4 = num.Left - num12;
            }
            else if ((verticalEdge == VerticalSnapEdge.Left) && (snapRegion == SnapRegion.Interior))
            {
                int num13 = Math.Min(bounds.Right, bounds.Left + snapDescription.XOffset);
                num4 = num.Left - num13;
            }
            else if ((verticalEdge == VerticalSnapEdge.Right) && (snapRegion == SnapRegion.Interior))
            {
                int num14 = Math.Max(bounds.Left, bounds.Right - snapDescription.XOffset);
                num4 = num.Right - num14;
            }
            return new PointInt32(num.Left - num4, num.Top - num3);
        }

        public PointInt32 AdjustObstacleDestination(SnapObstacle movingObstacle, PointInt32 newLocation)
        {
            base.VerifyAccess();
            movingObstacle.VerifyAccess();
            PointInt32 num = this.AdjustObstacleDestination(movingObstacle, newLocation, false);
            return this.AdjustObstacleDestination(movingObstacle, num, true);
        }

        public PointInt32 AdjustObstacleDestination(SnapObstacle movingObstacle, PointInt32 newLocation, bool considerStickies)
        {
            base.VerifyAccess();
            movingObstacle.VerifyAccess();
            PointInt32 num = newLocation;
            SnapDescription description = this.obstacles[movingObstacle];
            SnapDescription currentSnapDescription = null;
            foreach (SnapObstacle obstacle in this.obstacles.Keys)
            {
                if (((obstacle.StickyEdges == considerStickies) && obstacle.Enabled) && (obstacle != movingObstacle))
                {
                    SnapDescription snapDescription = this.DetermineNewSnapDescription(movingObstacle, num, obstacle, currentSnapDescription);
                    if (snapDescription != null)
                    {
                        PointInt32 num2 = AdjustNewLocation(movingObstacle, num, snapDescription);
                        currentSnapDescription = snapDescription;
                        num = num2;
                        RectInt32 num3 = new RectInt32(num, movingObstacle.Bounds.Size);
                    }
                }
            }
            if (((description == null) || !description.SnappedTo.StickyEdges) || ((currentSnapDescription == null) || currentSnapDescription.SnappedTo.StickyEdges))
            {
                this.obstacles[movingObstacle] = currentSnapDescription;
            }
            return num;
        }

        private static bool AreEdgesClose(int l1, int r1, int l2, int r2)
        {
            if (r1 >= l2)
            {
                if (r2 < l1)
                {
                    return false;
                }
                if (((l1 <= l2) && (l2 <= r1)) && (r1 <= r2))
                {
                    return true;
                }
                if (((l2 <= l1) && (l1 <= r2)) && (r2 <= r1))
                {
                    return true;
                }
                if ((l1 <= l2) && (r2 <= r1))
                {
                    return true;
                }
                if ((l2 <= l1) && (l1 <= r2))
                {
                    return true;
                }
                ExceptionUtil.ThrowInvalidOperationException();
            }
            return false;
        }

        public bool ContainsSnapObstacle(ISnapObstacleHost snapObstacleHost)
        {
            base.VerifyAccess();
            snapObstacleHost.VerifyAccess();
            return this.ContainsSnapObstacle(snapObstacleHost.SnapObstacle);
        }

        public bool ContainsSnapObstacle(SnapObstacle snapObstacle)
        {
            base.VerifyAccess();
            snapObstacle.VerifyAccess();
            return this.obstacles.ContainsKey(snapObstacle);
        }

        private SnapDescription DetermineNewSnapDescription(SnapObstacle avoider, PointInt32 newLocation, SnapObstacle avoidee, SnapDescription currentSnapDescription)
        {
            int snapProximity;
            int num4;
            int num5;
            int num6;
            int num7;
            bool flag3;
            base.VerifyAccess();
            avoider.VerifyAccess();
            avoidee.VerifyAccess();
            if ((currentSnapDescription != null) && ((currentSnapDescription.HorizontalEdge != HorizontalSnapEdge.Neither) || (currentSnapDescription.VerticalEdge != VerticalSnapEdge.Neither)))
            {
                snapProximity = avoidee.SnapProximity * 2;
            }
            else
            {
                snapProximity = avoidee.SnapProximity;
            }
            RectInt32 bounds = avoider.Bounds;
            bounds.X = newLocation.X;
            bounds.Y = newLocation.Y;
            RectInt32 num3 = avoidee.Bounds;
            bool flag = AreEdgesClose(bounds.Top, bounds.Bottom, num3.Top, num3.Bottom);
            bool flag2 = AreEdgesClose(bounds.Left, bounds.Right, num3.Left, num3.Right);
            SnapRegion snapRegion = avoidee.SnapRegion;
            if (snapRegion != SnapRegion.Interior)
            {
                if (snapRegion != SnapRegion.Exterior)
                {
                    throw ExceptionUtil.InvalidEnumArgumentException<SnapRegion>(avoidee.SnapRegion, "avoidee.SnapRegion");
                }
            }
            else
            {
                num4 = Math.Abs((int) (bounds.Left - num3.Left));
                num5 = Math.Abs((int) (bounds.Right - num3.Right));
                num6 = Math.Abs((int) (bounds.Top - num3.Top));
                num7 = Math.Abs((int) (bounds.Bottom - num3.Bottom));
                goto Label_0184;
            }
            num4 = Math.Abs((int) (bounds.Left - num3.Right));
            num5 = Math.Abs((int) (bounds.Right - num3.Left));
            num6 = Math.Abs((int) (bounds.Top - num3.Bottom));
            num7 = Math.Abs((int) (bounds.Bottom - num3.Top));
        Label_0184:
            flag3 = num4 < snapProximity;
            bool flag4 = num5 < snapProximity;
            bool flag5 = num6 < snapProximity;
            bool flag6 = num7 < snapProximity;
            VerticalSnapEdge neither = VerticalSnapEdge.Neither;
            if (flag)
            {
                if ((flag3 && (avoidee.SnapRegion == SnapRegion.Exterior)) || (flag4 && (avoidee.SnapRegion == SnapRegion.Interior)))
                {
                    neither = VerticalSnapEdge.Right;
                }
                else if ((flag4 && (avoidee.SnapRegion == SnapRegion.Exterior)) || (flag3 && (avoidee.SnapRegion == SnapRegion.Interior)))
                {
                    neither = VerticalSnapEdge.Left;
                }
            }
            HorizontalSnapEdge horizontalEdge = HorizontalSnapEdge.Neither;
            if (flag2)
            {
                if ((flag5 && (avoidee.SnapRegion == SnapRegion.Exterior)) || (flag6 && (avoidee.SnapRegion == SnapRegion.Interior)))
                {
                    horizontalEdge = HorizontalSnapEdge.Bottom;
                }
                else if ((flag6 && (avoidee.SnapRegion == SnapRegion.Exterior)) || (flag5 && (avoidee.SnapRegion == SnapRegion.Interior)))
                {
                    horizontalEdge = HorizontalSnapEdge.Top;
                }
            }
            if ((horizontalEdge != HorizontalSnapEdge.Neither) || (neither != VerticalSnapEdge.Neither))
            {
                int snapDistance = avoider.SnapDistance;
                int yOffset = avoider.SnapDistance;
                if ((horizontalEdge == HorizontalSnapEdge.Neither) && (avoidee.SnapRegion == SnapRegion.Interior))
                {
                    yOffset = bounds.Top - num3.Top;
                    horizontalEdge = HorizontalSnapEdge.Top;
                }
                if ((neither == VerticalSnapEdge.Neither) && (avoidee.SnapRegion == SnapRegion.Interior))
                {
                    snapDistance = bounds.Left - num3.Left;
                    neither = VerticalSnapEdge.Left;
                }
                return new SnapDescription(avoidee, horizontalEdge, neither, snapDistance, yOffset);
            }
            return null;
        }

        public static SnapManager FindMySnapManager(Control me)
        {
            if (!(me is ISnapObstacleHost))
            {
                throw new ArgumentException("must be called with a Control that implements ISnapObstacleHost");
            }
            ISnapManagerHost host = me as ISnapManagerHost;
            if (host == null)
            {
                host = me.FindForm() as ISnapManagerHost;
            }
            if (host != null)
            {
                return host.SnapManager;
            }
            return null;
        }

        public SnapObstacle FindObstacle(string name)
        {
            base.VerifyAccess();
            foreach (SnapObstacle obstacle in this.obstacles.Keys)
            {
                if (string.Compare(obstacle.Name, name, true) == 0)
                {
                    return obstacle;
                }
            }
            return null;
        }

        public void Load()
        {
            base.VerifyAccess();
            foreach (SnapObstacle obstacle in this.obstacles.Keys.ToArrayEx<SnapObstacle>())
            {
                if (obstacle.IsPersistenceEnabled)
                {
                    this.LoadSnapObstacleData(obstacle);
                }
            }
        }

        private void LoadSnapObstacleData(SnapObstacle so)
        {
            if (so.PersistenceHandler.IsDataAvailable)
            {
                SnapDescription description;
                if (!so.PersistenceHandler.IsSnapped)
                {
                    description = null;
                }
                else
                {
                    string snappedToName = so.PersistenceHandler.SnappedToName;
                    description = new SnapDescription(this.FindObstacle(snappedToName), so.PersistenceHandler.HorizontalEdge, so.PersistenceHandler.VerticalEdge, so.PersistenceHandler.Offset);
                }
                this.obstacles[so] = description;
                RectInt32 bounds = so.PersistenceHandler.Bounds;
                so.RequestBoundsChange(bounds);
                if (description != null)
                {
                    ParkObstacle(so, description);
                }
            }
        }

        private void OnSnapObstacleBoundsChanged(object sender, ValueChangedEventArgs<RectInt32> e)
        {
            base.VerifyAccess();
            SnapObstacle senderSO = (SnapObstacle) sender;
            senderSO.VerifyAccess();
            RectInt32 oldValue = e.OldValue;
            RectInt32 bounds = senderSO.Bounds;
            this.UpdateDependentObstacles(senderSO, oldValue, bounds);
        }

        private void OnSnapObstacleBoundsChanging(object sender, ValueEventArgs<RectInt32> e)
        {
            base.VerifyAccess();
        }

        private static void ParkObstacle(SnapObstacle avoider, SnapDescription snapDescription)
        {
            avoider.VerifyAccess();
            PointInt32 location = avoider.Bounds.Location;
            PointInt32 num2 = AdjustNewLocation(avoider, location, snapDescription);
            RectInt32 newBounds = new RectInt32(num2, avoider.Bounds.Size);
            avoider.RequestBoundsChange(newBounds);
        }

        public void ParkObstacle(ISnapObstacleHost obstacle, ISnapObstacleHost snappedTo, HorizontalSnapEdge hEdge, VerticalSnapEdge vEdge)
        {
            base.VerifyAccess();
            obstacle.VerifyAccess();
            snappedTo.VerifyAccess();
            this.ParkObstacle(obstacle.SnapObstacle, snappedTo.SnapObstacle, hEdge, vEdge);
        }

        public void ParkObstacle(SnapObstacle obstacle, SnapObstacle snappedTo, HorizontalSnapEdge hEdge, VerticalSnapEdge vEdge)
        {
            base.VerifyAccess();
            obstacle.VerifyAccess();
            snappedTo.VerifyAccess();
            SnapDescription snapDescription = new SnapDescription(snappedTo, hEdge, vEdge, obstacle.SnapDistance, obstacle.SnapDistance);
            this.obstacles[obstacle] = snapDescription;
            ParkObstacle(obstacle, snapDescription);
        }

        public void RemoveSnapObstacle(ISnapObstacleHost snapObstacleHost)
        {
            base.VerifyAccess();
            snapObstacleHost.VerifyAccess();
            this.RemoveSnapObstacle(snapObstacleHost.SnapObstacle);
        }

        public void RemoveSnapObstacle(SnapObstacle snapObstacle)
        {
            base.VerifyAccess();
            snapObstacle.VerifyAccess();
            if (this.obstacles.ContainsKey(snapObstacle))
            {
                this.obstacles.Remove(snapObstacle);
                if (snapObstacle.StickyEdges)
                {
                    snapObstacle.BoundsChanging -= new ValueEventHandler<RectInt32>(this.OnSnapObstacleBoundsChanging);
                    snapObstacle.BoundsChanged -= new ValueChangedEventHandler<RectInt32>(this.OnSnapObstacleBoundsChanged);
                }
            }
        }

        public void ReparkObstacle(ISnapObstacleHost obstacle)
        {
            base.VerifyAccess();
            obstacle.VerifyAccess();
            this.ReparkObstacle(obstacle.SnapObstacle);
        }

        public void ReparkObstacle(SnapObstacle obstacle)
        {
            base.VerifyAccess();
            obstacle.VerifyAccess();
            if (this.obstacles.ContainsKey(obstacle))
            {
                SnapDescription snapDescription = this.obstacles[obstacle];
                if (snapDescription != null)
                {
                    ParkObstacle(obstacle, snapDescription);
                }
            }
        }

        public void Save()
        {
            base.VerifyAccess();
            foreach (SnapObstacle obstacle in this.obstacles.Keys)
            {
                if (obstacle.IsPersistenceEnabled)
                {
                    this.SaveSnapObstacleData(obstacle);
                }
            }
        }

        private void SaveSnapObstacleData(SnapObstacle so)
        {
            base.VerifyAccess();
            so.VerifyAccess();
            SnapDescription description = this.obstacles[so];
            bool flag = description > null;
            so.PersistenceHandler.IsSnapped = flag;
            if (flag)
            {
                so.PersistenceHandler.SnappedToName = description.SnappedTo.Name;
                so.PersistenceHandler.HorizontalEdge = description.HorizontalEdge;
                so.PersistenceHandler.VerticalEdge = description.VerticalEdge;
                so.PersistenceHandler.Offset = new PointInt32(description.XOffset, description.YOffset);
            }
            so.PersistenceHandler.Bounds = so.Bounds;
        }

        private void UpdateDependentObstacles(SnapObstacle senderSO, RectInt32 fromRect, RectInt32 toRect)
        {
            base.VerifyAccess();
            senderSO.VerifyAccess();
            int num = toRect.Left - fromRect.Left;
            int num2 = toRect.Top - fromRect.Top;
            int num3 = toRect.Right - fromRect.Right;
            int num4 = toRect.Bottom - fromRect.Bottom;
            foreach (SnapObstacle obstacle in this.obstacles.Keys)
            {
                if (senderSO != obstacle)
                {
                    SnapDescription snapDescription = this.obstacles[obstacle];
                    if ((snapDescription != null) && (snapDescription.SnappedTo == senderSO))
                    {
                        int num5;
                        int num6;
                        if (snapDescription.VerticalEdge == VerticalSnapEdge.Right)
                        {
                            num5 = num3;
                        }
                        else
                        {
                            num5 = num;
                        }
                        if (snapDescription.HorizontalEdge == HorizontalSnapEdge.Bottom)
                        {
                            num6 = num4;
                        }
                        else
                        {
                            num6 = num2;
                        }
                        RectInt32 bounds = obstacle.Bounds;
                        PointInt32 newLocation = new PointInt32(bounds.Left + num5, bounds.Top + num6);
                        PointInt32 location = AdjustNewLocation(obstacle, newLocation, snapDescription);
                        RectInt32 newBounds = new RectInt32(location, bounds.Size);
                        obstacle.RequestBoundsChange(newBounds);
                        this.UpdateDependentObstacles(obstacle, bounds, newBounds);
                    }
                }
            }
        }
    }
}

