namespace PaintDotNet.Tools.BrushBase
{
    using PaintDotNet;
    using PaintDotNet.Brushes;
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Rendering;
    using PaintDotNet.Settings;
    using PaintDotNet.Settings.App;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Threading;
    using PaintDotNet.Tools;
    using PaintDotNet.Tools.Controls;
    using PaintDotNet.UI;
    using PaintDotNet.UI.Input;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal abstract class BrushToolBase<TDerived, TChanges, TUI> : PresentationBasedTool<TDerived, TChanges>, IBitmapLayerTransactionHandlerHost<TDerived, TChanges> where TDerived: BrushToolBase<TDerived, TChanges, TUI> where TChanges: BrushToolChangesBase<TChanges, TDerived> where TUI: BrushToolUIBase<TUI, TDerived, TChanges>, new()
    {
        private TransactedToolDrawingAgent<TChanges> mouseInputDrawingAgent;
        private TimerResolutionScope timerResolutionScope;
        private BitmapLayerTransactionHandler<TDerived, TChanges> txHandler;

        protected BrushToolBase(DocumentWorkspace documentWorkspace, ImageResource toolBarImage, string name, string helpText, char hotKey, bool skipIfActiveOnHotKey, ToolBarConfigItems toolBarConfigItems) : base(documentWorkspace, toolBarImage, name, helpText, hotKey, skipIfActiveOnHotKey, (((toolBarConfigItems | (ToolBarConfigItems.None | ToolBarConfigItems.PenWidth)) | (ToolBarConfigItems.None | ToolBarConfigItems.PenHardness)) | ToolBarConfigItems.Antialiasing) | (ToolBarConfigItems.None | ToolBarConfigItems.SelectionRenderingQuality), false)
        {
            this.txHandler = new BitmapLayerTransactionHandler<TDerived, TChanges>((TDerived) this);
        }

        protected abstract TChanges CreateChanges(TChanges oldChanges, IEnumerable<BrushInputPoint> inputPoints);
        protected abstract TChanges CreateChanges(IEnumerable<KeyValuePair<string, object>> drawingSettingsValues, IEnumerable<BrushInputPoint> inputPoints, MouseButtonState rightButtonState);
        protected abstract IEnumerable<IMaskedRenderer<ColorBgra, ColorAlpha8>> CreateContentRenderers(BitmapLayer layer, TChanges changes);
        protected abstract ContentBlendMode GetBlendMode(TChanges changes);
        protected static PointDouble GetMouseCenterPt(PointDouble mouseTopLeftPt, double hairWidth) => 
            BrushToolBase<TDerived, TChanges, TUI>.GetMouseCoverageRect(mouseTopLeftPt, hairWidth).Center;

        protected static RectDouble GetMouseCoverageRect(PointDouble mouseTopLeftPt, double hairWidth) => 
            new RectDouble(mouseTopLeftPt, new SizeDouble(hairWidth, hairWidth));

        protected override void OnActivated()
        {
            this.mouseInputDrawingAgent = new TransactedToolDrawingAgent<TChanges>(base.GetType().Name + ".mouseInputDrawingAgent");
            this.mouseInputDrawingAgent.CancelRequested += new HandledEventHandler(this.OnMouseInputDrawingAgentCancelRequested);
            this.mouseInputDrawingAgent.EndRequested += new HandledEventHandler(this.OnMouseInputDrawingAgentEndRequested);
            this.txHandler.RelayActivated();
            base.OnActivated();
        }

        protected override void OnChangesChanged(TChanges oldChanges, TChanges newChanges)
        {
            this.txHandler.RelayChangesChanged(oldChanges, newChanges);
            base.OnChangesChanged(oldChanges, newChanges);
        }

        protected override HistoryMemento OnCommitChanges(TChanges changes) => 
            this.txHandler.RelayCommitChanges(changes, base.Name, base.Image);

        protected override void OnDeactivated()
        {
            this.mouseInputDrawingAgent = null;
            this.txHandler.RelayDeactivated();
            DisposableUtil.Free<TimerResolutionScope>(ref this.timerResolutionScope);
            base.OnDeactivated();
        }

        protected override void OnDeactivating()
        {
            this.txHandler.RelayDeactivating();
            base.OnDeactivating();
        }

        protected override IEnumerable<Setting> OnGetDrawingSettings()
        {
            Setting[] tails = new Setting[] { base.ToolSettings.Pen.Width, base.ToolSettings.Pen.Hardness, base.ToolSettings.Antialiasing };
            return this.txHandler.RelayGetDrawingSettings().Concat<Setting>(tails);
        }

        protected override string OnGetHistoryMementoNameForChanges(TChanges oldChanges, TChanges newChanges) => 
            base.Name;

        private void OnMouseInputDrawingAgentCancelRequested(object sender, HandledEventArgs e)
        {
            if (!e.Handled)
            {
                this.mouseInputDrawingAgent.TransactionToken.Cancel();
                e.Handled = true;
            }
        }

        private void OnMouseInputDrawingAgentEndRequested(object sender, HandledEventArgs e)
        {
            if (!e.Handled)
            {
                this.mouseInputDrawingAgent.TransactionToken.End();
                e.Handled = true;
            }
        }

        protected override void OnPresentationSourceInitialized()
        {
            TUI local = Activator.CreateInstance<TUI>();
            local.Tool = (TDerived) this;
            local.AddHandler(ToolUICanvas.DragBeginEvent, new MouseEventHandler(this.OnUIDragBegin));
            local.AddHandler(ToolUICanvas.DragMoveEvent, new MouseEventHandler(this.OnUIDragMove));
            local.AddHandler(ToolUICanvas.DragEndEvent, new MouseEventHandler(this.OnUIDragEnd));
            this.UI = local;
            this.OnUIInitialized();
            base.OnPresentationSourceInitialized();
        }

        protected override void OnStateChanged(TransactedToolState oldValue, TransactedToolState newValue)
        {
            if (newValue != TransactedToolState.Idle)
            {
                if (newValue == TransactedToolState.Drawing)
                {
                    this.PushTimerScope();
                }
            }
            else if (this.IsTimerScopeActive)
            {
                this.PopTimerScope();
            }
            base.OnStateChanged(oldValue, newValue);
        }

        protected override bool OnToolConfigStripHotKey(ToolConfigStripHotKey key)
        {
            switch (key)
            {
                case ToolConfigStripHotKey.DecrementPenSize:
                case ToolConfigStripHotKey.DecrementPenSizeBy5:
                case ToolConfigStripHotKey.IncrementPenSize:
                case ToolConfigStripHotKey.IncrementPenSizeBy5:
                    if (((this.State != TransactedToolState.Drawing) && (this.State != TransactedToolState.Editing)) && !base.IsCommitting)
                    {
                        break;
                    }
                    return true;
            }
            return base.OnToolConfigStripHotKey(key);
        }

        protected virtual void OnUIDragBegin(object sender, MouseEventArgs e)
        {
            if (!e.Handled && (e.Source is UIElement))
            {
                PointDouble mouseCenterPt = BrushToolBase<TDerived, TChanges, TUI>.GetMouseCenterPt(e.GetPosition(this.UI), base.CanvasView.CanvasHairWidth);
                BrushInputPoint point = new BrushInputPoint(mouseCenterPt);
                if ((this.State == TransactedToolState.Idle) && !this.mouseInputDrawingAgent.IsActive)
                {
                    BrushInputPoint[] inputPoints = new BrushInputPoint[] { point };
                    TChanges initialChanges = this.CreateChanges(base.DrawingSettingsValues, inputPoints, e.RightButton);
                    base.BeginDrawing(this.mouseInputDrawingAgent, initialChanges);
                }
            }
        }

        protected virtual void OnUIDragEnd(object sender, MouseEventArgs e)
        {
            if ((!e.Handled && (e.Source is UIElement)) && ((this.State == TransactedToolState.Drawing) && this.mouseInputDrawingAgent.IsActive))
            {
                this.mouseInputDrawingAgent.TransactionToken.Commit();
            }
        }

        protected virtual void OnUIDragMove(object sender, MouseEventArgs e)
        {
            double canvasHairWidth;
            if ((!e.Handled && (e.Source is UIElement)) && ((this.State == TransactedToolState.Drawing) && this.mouseInputDrawingAgent.IsActive))
            {
                IList<PointDouble> intermediatePoints = e.GetIntermediatePoints(this.UI);
                canvasHairWidth = base.CanvasView.CanvasHairWidth;
                IList<BrushInputPoint> inputPoints = intermediatePoints.Select<PointDouble, PointDouble>(pt => BrushToolBase<TDerived, TChanges, TUI>.GetMouseCenterPt(pt, canvasHairWidth)).Select<PointDouble, BrushInputPoint>(pt => new BrushInputPoint(pt));
                TChanges changes = this.mouseInputDrawingAgent.TransactionToken.Changes;
                TChanges local2 = this.CreateChanges(changes, inputPoints);
                this.mouseInputDrawingAgent.TransactionToken.Changes = local2;
            }
        }

        protected virtual void OnUIInitialized()
        {
        }

        IEnumerable<IMaskedRenderer<ColorBgra, ColorAlpha8>> IBitmapLayerTransactionHandlerHost<TDerived, TChanges>.CreateContentRenderers(BitmapLayer layer, TChanges changes) => 
            this.CreateContentRenderers(layer, changes);

        ContentBlendMode IBitmapLayerTransactionHandlerHost<TDerived, TChanges>.GetBlendMode(TChanges changes) => 
            this.GetBlendMode(changes);

        void IBitmapLayerTransactionHandlerHost<TDerived, TChanges>.GetContentClip(TChanges changes, out RectInt32 clipRect, out IRenderer<ColorAlpha8> clipMaskRenderer)
        {
            throw new NotSupportedException();
        }

        RectInt32 IBitmapLayerTransactionHandlerHost<TDerived, TChanges>.GetDifferentialMaxBounds(TChanges oldChanges, TChanges newChanges)
        {
            object renderDataCurrencyToken = oldChanges.RenderDataCurrencyToken;
            object newCurrencyToken = newChanges.RenderDataCurrencyToken;
            IList<int> list = newChanges.RenderData.EnumerateStrokeSampleIndicesBetweenCurrencyTokens(renderDataCurrencyToken, newCurrencyToken);
            if (list.Count == 0)
            {
                return newChanges.GetMaxRenderBounds();
            }
            SizeDouble size = newChanges.Stamp.Size;
            RectDouble bounds = newChanges.RenderData.StrokeSamples[list[0]].GetBounds(size);
            int count = list.Count;
            for (int i = 1; i < count; i++)
            {
                int num5 = list[i];
                RectDouble b = newChanges.RenderData.StrokeSamples[num5].GetBounds(size);
                bounds = RectDouble.Union(bounds, b);
            }
            return bounds.Int32Bound;
        }

        private void PopTimerScope()
        {
            ((BrushToolBase<TDerived, TChanges, TUI>) this).VerifyAccess<BrushToolBase<TDerived, TChanges, TUI>>();
            if (this.timerResolutionScope == null)
            {
                throw new InvalidOperationException("mismatched push/pop");
            }
            DisposableUtil.Free<TimerResolutionScope>(ref this.timerResolutionScope);
        }

        private void PushTimerScope()
        {
            ((BrushToolBase<TDerived, TChanges, TUI>) this).VerifyAccess<BrushToolBase<TDerived, TChanges, TUI>>();
            if (this.timerResolutionScope != null)
            {
                throw new InvalidOperationException("mismatched push/pop");
            }
            if (!base.Active)
            {
                throw new InvalidOperationException("Tool is not in the active state");
            }
            this.timerResolutionScope = new TimerResolutionScope(1);
        }

        private bool IsTimerScopeActive
        {
            get
            {
                ((BrushToolBase<TDerived, TChanges, TUI>) this).VerifyAccess<BrushToolBase<TDerived, TChanges, TUI>>();
                return (this.timerResolutionScope > null);
            }
        }

        BitmapLayer IBitmapLayerTransactionHandlerHost<TDerived, TChanges>.ActiveLayer =>
            ((BitmapLayer) base.ActiveLayer);

        int IBitmapLayerTransactionHandlerHost<TDerived, TChanges>.ActiveLayerIndex =>
            base.ActiveLayerIndex;

        Document IBitmapLayerTransactionHandlerHost<TDerived, TChanges>.Document =>
            base.Document;

        IHistoryWorkspace IBitmapLayerTransactionHandlerHost<TDerived, TChanges>.HistoryWorkspace =>
            base.DocumentWorkspace;

        Selection IBitmapLayerTransactionHandlerHost<TDerived, TChanges>.Selection =>
            base.Selection;

        AppSettings.ToolsSection IBitmapLayerTransactionHandlerHost<TDerived, TChanges>.ToolSettings =>
            base.ToolSettings;

        protected TUI UI
        {
            get => 
                ((TUI) base.UI);
            set
            {
                base.UI = value;
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly BrushToolBase<TDerived, TChanges, TUI>.<>c <>9;
            public static Func<PointDouble, BrushInputPoint> <>9__28_1;

            static <>c()
            {
                BrushToolBase<TDerived, TChanges, TUI>.<>c.<>9 = new BrushToolBase<TDerived, TChanges, TUI>.<>c();
            }

            internal BrushInputPoint <OnUIDragMove>b__28_1(PointDouble pt) => 
                new BrushInputPoint(pt);
        }
    }
}

