namespace PaintDotNet.Tools.MagicWand
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Functional;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.Settings;
    using PaintDotNet.Threading;
    using PaintDotNet.Tools;
    using PaintDotNet.Tools.Controls;
    using PaintDotNet.Tools.FloodFill;
    using PaintDotNet.UI;
    using PaintDotNet.UI.Input;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class MagicWandTool : AsyncSelectionToolBase<MagicWandTool, MagicWandToolChanges>
    {
        private PointDouble gestureBeginCanvasPt;
        private TransactedToolDrawingAgent<MagicWandToolChanges> mouseInputDrawingAgent;
        private TransactedToolEditingAgent<MagicWandToolChanges> mouseInputEditingAgent;
        private readonly ProtectedRegion onUIGestureRegion;

        public MagicWandTool(DocumentWorkspace documentWorkspace) : base(documentWorkspace, PdnResources.GetImageResource("Icons.MagicWandToolIcon.png"), PdnResources.GetString("MagicWandTool.Name"), PdnResources.GetString("MagicWandTool.HelpText"), 's', false, ToolBarConfigItems.FloodMode | ToolBarConfigItems.SampleImageOrLayer | ToolBarConfigItems.SelectionCombineMode | ToolBarConfigItems.Tolerance)
        {
            this.onUIGestureRegion = new ProtectedRegion("OnUIGesture*", ProtectedRegionOptions.None);
        }

        protected override GeometryList CreateSelectionGeometry(MagicWandToolChanges changes, AsyncSelectionToolCreateGeometryContext context, CancellationToken cancellationToken)
        {
            GeometryList list;
            Result<BitVector2D> lazyBaseStencil;
            IRenderer<ColorBgra> sampleSource = ((MagicWandToolCreateGeometryContext) context).SampleSource;
            byte x = (byte) Math.Round((double) (changes.Tolerance * 255.0), MidpointRounding.AwayFromZero);
            byte tolerance = ByteUtil.FastScale(x, x);
            PointInt32 pt = changes.OriginPointInt32;
            if (!sampleSource.Bounds<ColorBgra>().Contains(pt))
            {
                switch (changes.SelectionCombineMode)
                {
                    case SelectionCombineMode.Replace:
                    case SelectionCombineMode.Intersect:
                        return new GeometryList();

                    case SelectionCombineMode.Union:
                    case SelectionCombineMode.Exclude:
                    case SelectionCombineMode.Xor:
                        return changes.BaseGeometry;
                }
                throw ExceptionUtil.InvalidEnumArgumentException<SelectionCombineMode>(changes.SelectionCombineMode, "changes.SelectionCombineMode");
            }
            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }
            Func<bool> isCancellationRequestedFn = () => cancellationToken.IsCancellationRequested;
            ColorBgra basis = sampleSource.GetPointSlow(pt);
            int width = ((sampleSource.Width + 0x1f) / 0x20) * 0x20;
            BitVector2D newStencil = new BitVector2D(width, sampleSource.Height);
            BitVector2DStruct newStencilWrapper = new BitVector2DStruct(newStencil);
            if (((changes.SelectionCombineMode != SelectionCombineMode.Replace) && sampleSource.Bounds<ColorBgra>().Contains(changes.BaseGeometry.Bounds.Int32Bound)) && changes.BaseGeometry.IsPixelated)
            {
                lazyBaseStencil = LazyResult.New<BitVector2D>(() => PixelatedGeometryListToBitVector2D(changes.BaseGeometry, newStencil.Width, newStencil.Height, cancellationToken), LazyThreadSafetyMode.ExecutionAndPublication, new SingleUseCriticalSection());
                ThreadPool.QueueUserWorkItem(delegate (object _) {
                    lazyBaseStencil.EnsureEvaluated();
                });
            }
            else
            {
                lazyBaseStencil = null;
            }
            FloodMode floodMode = changes.FloodMode;
            if (floodMode != FloodMode.Local)
            {
                if (floodMode != FloodMode.Global)
                {
                    throw ExceptionUtil.InvalidEnumArgumentException<FloodMode>(changes.FloodMode, "changes.FloodMode");
                }
            }
            else
            {
                RectInt32 num4;
                FloodFillAlgorithm.FillStencilFromPoint<BitVector2DStruct>(sampleSource, newStencilWrapper, pt, tolerance, isCancellationRequestedFn, out num4);
                goto Label_0293;
            }
            TileMathHelper tileMathHelper = new TileMathHelper(sampleSource.Width, sampleSource.Height, 7);
            Work.ParallelForEach<PointInt32>(WaitType.Pumping, tileMathHelper.EnumerateTileOffsets(), delegate (PointInt32 tileOffset) {
                if (!cancellationToken.IsCancellationRequested)
                {
                    RectInt32 clipRect = tileMathHelper.GetTileSourceRect(tileOffset);
                    FloodFillAlgorithm.FillStencilByColor<BitVector2DStruct>(sampleSource, newStencilWrapper, basis, tolerance, isCancellationRequestedFn, clipRect);
                }
            }, WorkItemQueuePriority.Normal, null);
        Label_0293:
            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }
            if (changes.SelectionCombineMode == SelectionCombineMode.Replace)
            {
                list = GeometryList.FromStencil<BitVector2DStruct>(newStencilWrapper, cancellationToken);
            }
            else if (lazyBaseStencil == null)
            {
                GeometryList rhs = GeometryList.FromStencil<BitVector2DStruct>(newStencilWrapper, cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }
                list = GeometryList.Combine(changes.BaseGeometry, changes.SelectionCombineMode.ToGeometryCombineMode(), rhs);
            }
            else
            {
                BitVector2D other = lazyBaseStencil.Value;
                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }
                switch (changes.SelectionCombineMode)
                {
                    case SelectionCombineMode.Replace:
                        throw new InternalErrorException();

                    case SelectionCombineMode.Union:
                        newStencil.Or(other);
                        break;

                    case SelectionCombineMode.Exclude:
                        newStencil.Invert();
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return null;
                        }
                        newStencil.And(other);
                        break;

                    case SelectionCombineMode.Intersect:
                        newStencil.And(other);
                        break;

                    case SelectionCombineMode.Xor:
                        newStencil.Xor(other);
                        break;

                    default:
                        throw ExceptionUtil.InvalidEnumArgumentException<SelectionCombineMode>(changes.SelectionCombineMode, "changes.SelectionCombineMode");
                }
                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }
                list = GeometryList.FromStencil<BitVector2DStruct>(newStencilWrapper, cancellationToken);
            }
            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }
            list.Freeze();
            return list;
        }

        protected override string GetCommitChangesHistoryMementoName(MagicWandToolChanges changes) => 
            PdnResources.GetString("MagicWandTool.CommitChanges.HistoryMementoName");

        protected override AsyncSelectionToolCreateGeometryContext GetCreateGeometryContext(MagicWandToolChanges changes) => 
            new MagicWandToolCreateGeometryContext(base.GetSampleSource((BitmapLayer) base.ActiveLayer, changes.SampleAllLayers));

        protected override void OnActivated()
        {
            this.mouseInputDrawingAgent = new TransactedToolDrawingAgent<MagicWandToolChanges>("MagicWandTool.mouseInputDrawingAgent");
            this.mouseInputDrawingAgent.CancelRequested += new HandledEventHandler(this.OnMouseInputDrawingAgentCancelRequested);
            this.mouseInputDrawingAgent.EndRequested += new HandledEventHandler(this.OnMouseInputDrawingAgentEndRequested);
            this.mouseInputEditingAgent = new TransactedToolEditingAgent<MagicWandToolChanges>("MagicWandTool.mouseInputEditingAgent");
            this.mouseInputEditingAgent.CancelRequested += new HandledEventHandler(this.OnMouseInputEditingAgentCancelRequested);
            this.mouseInputEditingAgent.EndRequested += new HandledEventHandler(this.OnMouseInputEditingAgentEndRequested);
            base.OnActivated();
        }

        protected override void OnDeactivated()
        {
            this.mouseInputDrawingAgent = null;
            this.mouseInputEditingAgent = null;
            this.UI.RemoveHandler(ToolUICanvas.GestureBeginEvent, new MouseEventHandler(this.OnUIGestureBegin));
            this.UI.RemoveHandler(ToolUICanvas.ClickedEvent, new MouseEventHandler(this.OnUIClicked));
            this.UI.RemoveHandler(ToolUICanvas.DragBeginEvent, new MouseEventHandler(this.OnUIDragBegin));
            this.UI.RemoveHandler(ToolUICanvas.DragMoveEvent, new MouseEventHandler(this.OnUIDragMove));
            this.UI.RemoveHandler(ToolUICanvas.DragEndEvent, new MouseEventHandler(this.OnUIDragEnd));
            this.UI.RemoveHandler(ToolUICanvas.GestureEndEvent, new RoutedEventHandler(this.OnUIGestureEnd));
            base.OnDeactivated();
        }

        protected override void OnDeactivating()
        {
            base.OnDeactivating();
        }

        [IteratorStateMachine(typeof(<OnGetDrawingSettings>d__6))]
        protected override IEnumerable<Setting> OnGetDrawingSettings()
        {
            yield return this.ToolSettings.FloodMode;
            yield return this.ToolSettings.SampleAllLayers;
            yield return this.ToolSettings.Selection.CombineMode;
            yield return this.ToolSettings.Tolerance;
        }

        protected override string OnGetHistoryMementoNameForChanges(MagicWandToolChanges oldChanges, MagicWandToolChanges newChanges)
        {
            if ((oldChanges == null) && (newChanges != null))
            {
                return PdnResources.GetString("MagicWandTool.EndDrawing.HistoryMementoName");
            }
            string genericHistoryMementoName = PdnResources.GetString("MagicWandTool.Changed.Generic.HistoryMementoName");
            string historyMementoName = null;
            if (oldChanges.OriginPoint != newChanges.OriginPoint)
            {
                historyMementoName = TransactedTool<MagicWandTool, MagicWandToolChanges>.FoldHistoryMementoName(historyMementoName, genericHistoryMementoName, PdnResources.GetString("FloodFillToolBase.Changed.OriginPoint.HistoryMementoName"));
            }
            if (oldChanges.SelectionCombineMode != newChanges.SelectionCombineMode)
            {
                historyMementoName = TransactedTool<MagicWandTool, MagicWandToolChanges>.FoldHistoryMementoName(historyMementoName, genericHistoryMementoName, PdnResources.GetString("TransactedTool.Changed.SelectionCombineMode.HistoryMementoName"));
            }
            if (oldChanges.FloodMode != newChanges.FloodMode)
            {
                historyMementoName = TransactedTool<MagicWandTool, MagicWandToolChanges>.FoldHistoryMementoName(historyMementoName, genericHistoryMementoName, PdnResources.GetString("TransactedTool.Changed.FloodMode.HistoryMementoName"));
            }
            if (oldChanges.Tolerance != newChanges.Tolerance)
            {
                historyMementoName = TransactedTool<MagicWandTool, MagicWandToolChanges>.FoldHistoryMementoName(historyMementoName, genericHistoryMementoName, PdnResources.GetString("TransactedTool.Changed.Tolerance.HistoryMementoName"));
            }
            if (oldChanges.SampleAllLayers != newChanges.SampleAllLayers)
            {
                historyMementoName = TransactedTool<MagicWandTool, MagicWandToolChanges>.FoldHistoryMementoName(historyMementoName, genericHistoryMementoName, PdnResources.GetString("TransactedTool.Changed.SampleAllLayers.HistoryMementoName"));
            }
            return (historyMementoName ?? genericHistoryMementoName);
        }

        protected override void OnGetStatus(out ImageResource image, out string text)
        {
            if (!base.DocumentWorkspace.Selection.IsEmpty)
            {
                base.DocumentWorkspace.GetLatestSelectionInfo(out text, out image);
                if ((text != null) && (image != null))
                {
                    return;
                }
            }
            base.OnGetStatus(out image, out text);
        }

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

        private void OnMouseInputEditingAgentCancelRequested(object sender, HandledEventArgs e)
        {
            if (!e.Handled)
            {
                this.mouseInputEditingAgent.TransactionToken.Cancel();
                e.Handled = true;
            }
        }

        private void OnMouseInputEditingAgentEndRequested(object sender, HandledEventArgs e)
        {
            if (!e.Handled)
            {
                this.mouseInputEditingAgent.TransactionToken.End();
                e.Handled = true;
            }
        }

        protected override void OnPresentationSourceInitialized()
        {
            MagicWandToolUI lui = new MagicWandToolUI {
                Tool = this
            };
            lui.AddHandler(ToolUICanvas.GestureBeginEvent, new MouseEventHandler(this.OnUIGestureBegin));
            lui.AddHandler(ToolUICanvas.ClickedEvent, new MouseEventHandler(this.OnUIClicked));
            lui.AddHandler(ToolUICanvas.DragBeginEvent, new MouseEventHandler(this.OnUIDragBegin));
            lui.AddHandler(ToolUICanvas.DragMoveEvent, new MouseEventHandler(this.OnUIDragMove));
            lui.AddHandler(ToolUICanvas.DragEndEvent, new MouseEventHandler(this.OnUIDragEnd));
            lui.AddHandler(ToolUICanvas.GestureEndEvent, new RoutedEventHandler(this.OnUIGestureEnd));
            this.UI = lui;
            base.OnPresentationSourceInitialized();
        }

        private void OnUIClicked(object sender, MouseEventArgs e)
        {
            using (this.onUIGestureRegion.UseEnterScope())
            {
                UIElement source = e.Source as UIElement;
                if (source != null)
                {
                    FloodFillToolHandleType handleType = FloodFillToolUIBase<MagicWandTool, MagicWandToolChanges>.GetHandleType(source);
                }
            }
        }

        private void OnUIDragBegin(object sender, MouseEventArgs e)
        {
            using (this.onUIGestureRegion.UseEnterScope())
            {
                UIElement source = e.Source as UIElement;
                if (source != null)
                {
                    FloodFillToolHandleType handleType = FloodFillToolUIBase<MagicWandTool, MagicWandToolChanges>.GetHandleType(source);
                }
            }
        }

        private void OnUIDragEnd(object sender, MouseEventArgs e)
        {
            using (this.onUIGestureRegion.UseEnterScope())
            {
                UIElement source = e.Source as UIElement;
                if (source != null)
                {
                    FloodFillToolHandleType handleType = FloodFillToolUIBase<MagicWandTool, MagicWandToolChanges>.GetHandleType(source);
                }
            }
        }

        private void OnUIDragMove(object sender, MouseEventArgs e)
        {
            using (this.onUIGestureRegion.UseEnterScope())
            {
                MagicWandToolChanges changes2;
                UIElement source = e.Source as UIElement;
                if (source != null)
                {
                    PointDouble position = e.GetPosition(this.UI);
                    FloodFillToolHandleType handleType = FloodFillToolUIBase<MagicWandTool, MagicWandToolChanges>.GetHandleType(source);
                    PointDouble? nullable = null;
                    switch (handleType)
                    {
                        case FloodFillToolHandleType.Canvas:
                            if ((this.State == TransactedToolState.Drawing) && this.mouseInputDrawingAgent.IsActive)
                            {
                                nullable = new PointDouble?(this.gestureBeginCanvasPt);
                            }
                            break;

                        case FloodFillToolHandleType.Origin:
                        case FloodFillToolHandleType.Move:
                            if ((this.State == TransactedToolState.Editing) && this.mouseInputEditingAgent.IsActive)
                            {
                                nullable = new PointDouble?(base.ChangesBeforeEditing.OriginPoint);
                            }
                            break;

                        default:
                            throw ExceptionUtil.InvalidEnumArgumentException<FloodFillToolHandleType>(handleType, "sender.(MagicWandToolUI.HandleType)");
                    }
                    if (nullable.HasValue)
                    {
                        VectorDouble num2 = (VectorDouble) (position - this.gestureBeginCanvasPt);
                        PointDouble originPoint = nullable.Value + num2;
                        MagicWandToolChanges changes = base.Changes;
                        changes2 = new MagicWandToolChanges(changes.DrawingSettingsValues, originPoint, changes.SelectionCombineModeOverride, changes.FloodModeOverride, changes.BaseGeometryPersistenceKey);
                        if (!changes.Equals((ReferenceValue) changes2))
                        {
                            switch (handleType)
                            {
                                case FloodFillToolHandleType.Canvas:
                                    this.mouseInputDrawingAgent.TransactionToken.Changes = changes2;
                                    break;

                                case FloodFillToolHandleType.Origin:
                                case FloodFillToolHandleType.Move:
                                    goto Label_0139;
                            }
                        }
                    }
                }
                return;
            Label_0139:
                this.mouseInputEditingAgent.TransactionToken.Changes = changes2;
            }
        }

        private void OnUIGestureBegin(object sender, MouseEventArgs e)
        {
            using (this.onUIGestureRegion.UseEnterScope())
            {
                UIElement source = e.Source as UIElement;
                if (source != null)
                {
                    PointDouble position = e.GetPosition(this.UI);
                    this.gestureBeginCanvasPt = position;
                    if ((this.State == TransactedToolState.Drawing) && !this.mouseInputDrawingAgent.IsActive)
                    {
                        this.RequestCancelDrawing();
                        base.VerifyState(TransactedToolState.Idle);
                    }
                    if ((this.State == TransactedToolState.Editing) && !this.mouseInputEditingAgent.IsActive)
                    {
                        this.RequestEndEditing();
                        base.VerifyState(TransactedToolState.Dirty);
                    }
                    FloodFillToolHandleType handleType = FloodFillToolUIBase<MagicWandTool, MagicWandToolChanges>.GetHandleType(source);
                    switch (handleType)
                    {
                        case FloodFillToolHandleType.Canvas:
                            if (this.State == TransactedToolState.Dirty)
                            {
                                base.CommitChanges();
                                base.VerifyState(TransactedToolState.Idle);
                            }
                            if (this.State == TransactedToolState.Idle)
                            {
                                SelectionHistoryMemento selectionHistoryMementoAndPrepareForBeginDrawing = base.GetSelectionHistoryMementoAndPrepareForBeginDrawing();
                                GeometryList cachedGeometryList = base.Selection.GetCachedGeometryList();
                                SelectionCombineMode? selectionCombineModeOverride = base.GetSelectionCombineModeOverride();
                                FloodMode? floodModeOverride = ((base.PresentationSource.PrimaryKeyboardDevice.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) ? ((FloodMode?) 1) : null;
                                MagicWandToolChanges initialChanges = new MagicWandToolChanges(base.DrawingSettingsValues, position, selectionCombineModeOverride, floodModeOverride, cachedGeometryList);
                                base.BeginDrawing(this.mouseInputDrawingAgent, initialChanges, selectionHistoryMementoAndPrepareForBeginDrawing);
                            }
                            return;

                        case FloodFillToolHandleType.Origin:
                        case FloodFillToolHandleType.Move:
                            if (this.State == TransactedToolState.Dirty)
                            {
                                base.BeginEditing(this.mouseInputEditingAgent);
                            }
                            return;
                    }
                    throw ExceptionUtil.InvalidEnumArgumentException<FloodFillToolHandleType>(handleType, "sender.(MagicWandToolUI.HandleType)");
                }
            }
        }

        private void OnUIGestureEnd(object sender, RoutedEventArgs e)
        {
            using (this.onUIGestureRegion.UseEnterScope())
            {
                UIElement source = e.Source as UIElement;
                if (source != null)
                {
                    FloodFillToolHandleType handleType = FloodFillToolUIBase<MagicWandTool, MagicWandToolChanges>.GetHandleType(source);
                    switch (handleType)
                    {
                        case FloodFillToolHandleType.Canvas:
                        case FloodFillToolHandleType.Origin:
                        case FloodFillToolHandleType.Move:
                            if ((this.State != TransactedToolState.Drawing) || !this.mouseInputDrawingAgent.IsActive)
                            {
                                break;
                            }
                            this.mouseInputDrawingAgent.TransactionToken.End();
                            base.VerifyState(TransactedToolState.Dirty);
                            return;

                        default:
                            throw ExceptionUtil.InvalidEnumArgumentException<FloodFillToolHandleType>(handleType, "sender.(MagicWandToolUI.HandleType)");
                    }
                    if ((this.State == TransactedToolState.Editing) && this.mouseInputEditingAgent.IsActive)
                    {
                        this.mouseInputEditingAgent.TransactionToken.End();
                        base.VerifyState(TransactedToolState.Dirty);
                    }
                }
            }
        }

        private static BitVector2D PixelatedGeometryListToBitVector2D(GeometryList geometry, int width, int height, CancellationToken cancellationToken)
        {
            BitVector2D vectord = new BitVector2D(width, height);
            foreach (RectInt32 num in geometry.EnumerateInteriorScans())
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }
                vectord.Set(num, true);
            }
            return vectord;
        }

        public MagicWandToolUI UI
        {
            get => 
                ((MagicWandToolUI) base.UI);
            set
            {
                base.UI = value;
            }
        }

    }
}

