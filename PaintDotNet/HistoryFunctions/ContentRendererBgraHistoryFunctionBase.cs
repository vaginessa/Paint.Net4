namespace PaintDotNet.HistoryFunctions
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Rendering;
    using System;
    using System.Collections.Generic;

    internal abstract class ContentRendererBgraHistoryFunctionBase : HistoryFunction
    {
        protected ContentRendererBgraHistoryFunctionBase() : base(ActionFlags.None)
        {
        }

        protected virtual ContentBlendMode GetContentBlendMode() => 
            ContentBlendMode.Normal;

        protected abstract IEnumerable<IMaskedRenderer<ColorBgra, ColorAlpha8>> OnCreateContentRenderers(IHistoryWorkspace historyWorkspace, int width, int height);
        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            if (!this.QueryCanExecute(historyWorkspace))
            {
                return null;
            }
            HistoryMemento memento = this.OnPreRender(historyWorkspace);
            int activeLayerIndex = historyWorkspace.ActiveLayerIndex;
            BitmapLayer activeLayer = (BitmapLayer) historyWorkspace.ActiveLayer;
            RectInt32 num2 = activeLayer.Bounds();
            RectDouble bounds = historyWorkspace.Selection.GetCachedClippingMask().Bounds;
            IRenderer<ColorAlpha8> cachedClippingMaskRenderer = historyWorkspace.Selection.GetCachedClippingMaskRenderer(historyWorkspace.ToolSettings.Selection.RenderingQuality.Value);
            IEnumerable<IMaskedRenderer<ColorBgra, ColorAlpha8>> contentRenderers = this.OnCreateContentRenderers(historyWorkspace, num2.Width, num2.Height);
            ContentBlendMode contentBlendMode = this.GetContentBlendMode();
            ContentRendererBgra renderer = new ContentRendererBgra(activeLayer.Surface, contentBlendMode, contentRenderers, cachedClippingMaskRenderer);
            base.EnterCriticalRegion();
            HistoryMemento memento2 = new ApplyRendererToBitmapLayerHistoryFunction(this.HistoryMementoName, this.HistoryMementoImage, activeLayerIndex, renderer, bounds.Int32Bound, 4, 0x7d0, ActionFlags.None).Execute(historyWorkspace);
            HistoryMemento memento3 = this.OnPostRender(historyWorkspace);
            HistoryMemento[] items = new HistoryMemento[] { memento, memento2, memento3 };
            HistoryMemento[] actions = ArrayUtil.Infer<HistoryMemento>(items).WhereNotNull<HistoryMemento>().ToArrayEx<HistoryMemento>();
            if (actions.Length == 0)
            {
                return null;
            }
            return new CompoundHistoryMemento(this.HistoryMementoName, this.HistoryMementoImage, actions);
        }

        protected abstract HistoryMemento OnPostRender(IHistoryWorkspace historyWorkspace);
        protected abstract HistoryMemento OnPreRender(IHistoryWorkspace historyWorkspace);
        protected virtual bool QueryCanExecute(IHistoryWorkspace historyWorkspace) => 
            true;

        protected abstract ImageResource HistoryMementoImage { get; }

        protected abstract string HistoryMementoName { get; }
    }
}

