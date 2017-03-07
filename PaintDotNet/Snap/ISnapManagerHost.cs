namespace PaintDotNet.Snap
{
    using PaintDotNet.Threading;

    internal interface ISnapManagerHost : IThreadAffinitizedObject
    {
        PaintDotNet.Snap.SnapManager SnapManager { get; }
    }
}

