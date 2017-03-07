namespace PaintDotNet.Snap
{
    using PaintDotNet.Threading;

    internal interface ISnapObstacleHost : IThreadAffinitizedObject
    {
        PaintDotNet.Snap.SnapObstacle SnapObstacle { get; }
    }
}

