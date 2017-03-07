namespace PaintDotNet.HistoryFunctions
{
    using PaintDotNet;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Imaging;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    internal class FillSelectionFunction : ContentRendererBgraHistoryFunctionBase
    {
        private ColorBgra fillColor;

        public FillSelectionFunction(ColorBgra fillColor)
        {
            this.fillColor = fillColor;
        }

        protected override ContentBlendMode GetContentBlendMode() => 
            ContentBlendMode.Overwrite;

        [IteratorStateMachine(typeof(<OnCreateContentRenderers>d__8))]
        protected override IEnumerable<IMaskedRenderer<ColorBgra, ColorAlpha8>> OnCreateContentRenderers(IHistoryWorkspace historyWorkspace, int width, int height)
        {
            IRenderer<ColorBgra> contentRenderer = new SolidColorRendererBgra(width, height, this.fillColor);
            IRenderer<ColorAlpha8> maskRenderer = new FillRendererAlpha8(width, height, ColorAlpha8.Opaque);
            MultiplexedMaskedRenderer<ColorBgra, ColorAlpha8> renderer3 = new MultiplexedMaskedRenderer<ColorBgra, ColorAlpha8>(contentRenderer, maskRenderer, ColorAlpha8.Opaque);
            yield return renderer3;
        }

        protected override HistoryMemento OnPostRender(IHistoryWorkspace historyWorkspace) => 
            null;

        protected override HistoryMemento OnPreRender(IHistoryWorkspace historyWorkspace) => 
            null;

        protected override bool QueryCanExecute(IHistoryWorkspace historyWorkspace) => 
            !historyWorkspace.Selection.IsEmpty;

        protected override ImageResource HistoryMementoImage =>
            PdnResources.GetImageResource("Icons.MenuEditFillSelectionIcon.png");

        protected override string HistoryMementoName =>
            PdnResources.GetString("FillSelectionAction.Name");

    }
}

