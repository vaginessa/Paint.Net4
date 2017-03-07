namespace PaintDotNet
{
    using PaintDotNet.Settings.App;
    using PaintDotNet.Threading;
    using PaintDotNet.Threading.Tasks;
    using System;
    using System.Windows.Forms;

    internal interface IHistoryWorkspace : IThreadAffinitizedObject
    {
        Layer ActiveLayer { get; }

        int ActiveLayerIndex { get; }

        PaintDotNet.Document Document { get; set; }

        PaintDotNet.Selection Selection { get; }

        PaintDotNet.Threading.Tasks.TaskManager TaskManager { get; }

        AppSettings.ToolsSection ToolSettings { get; }

        IWin32Window Window { get; }
    }
}

