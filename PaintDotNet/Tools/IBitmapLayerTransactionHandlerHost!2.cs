namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using PaintDotNet.Settings.App;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    internal interface IBitmapLayerTransactionHandlerHost<TTool, TChanges> where TTool: TransactedTool<TTool, TChanges> where TChanges: TransactedToolChanges<TChanges, TTool>
    {
        IEnumerable<IMaskedRenderer<ColorBgra, ColorAlpha8>> CreateContentRenderers(BitmapLayer layer, TChanges changes);
        ContentBlendMode GetBlendMode(TChanges changes);
        void GetContentClip(TChanges changes, out RectInt32 clipRect, out IRenderer<ColorAlpha8> clipMaskRenderer);
        RectInt32 GetDifferentialMaxBounds(TChanges oldChanges, TChanges newChanges);

        BitmapLayer ActiveLayer { get; }

        int ActiveLayerIndex { get; }

        PaintDotNet.Document Document { get; }

        IHistoryWorkspace HistoryWorkspace { get; }

        PaintDotNet.Selection Selection { get; }

        AppSettings.ToolsSection ToolSettings { get; }
    }
}

