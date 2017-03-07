namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Functional;
    using PaintDotNet.HistoryFunctions;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Rendering;
    using PaintDotNet.Settings;
    using PaintDotNet.Threading;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal sealed class BitmapLayerTransactionHandler<TTool, TChanges> where TTool: TransactedTool<TTool, TChanges>, IBitmapLayerTransactionHandlerHost<TTool, TChanges> where TChanges: TransactedToolChanges<TChanges, TTool>
    {
        private bool autoClipToSelection;
        private bool isActivated;
        private BitmapLayerToolLayerOverlay<TTool, TChanges> layerOverlay;
        private TTool tool;
        private readonly ProtectedRegion updateLayerOverlayRegion;

        public BitmapLayerTransactionHandler(TTool tool) : this(tool, true)
        {
        }

        public BitmapLayerTransactionHandler(TTool tool, bool autoClipToSelection)
        {
            this.updateLayerOverlayRegion = new ProtectedRegion("UpdateLayerOverlay", ProtectedRegionOptions.ErrorOnPerThreadReentrancy | ProtectedRegionOptions.DisablePumpingWhenEntered);
            Validate.IsNotNull<TTool>(tool, "tool");
            this.tool = tool;
            this.autoClipToSelection = autoClipToSelection;
        }

        private BitmapLayerToolLayerOverlay<TTool, TChanges> CreateLayerOverlay(BitmapLayer layer, TChanges changes)
        {
            IRenderer<ColorAlpha8> renderer;
            RectInt32 num;
            this.tool.VerifyAccess<TTool>();
            if (this.autoClipToSelection)
            {
                this.GetDefaultContentClip(changes, out num, out renderer);
            }
            else
            {
                this.tool.GetContentClip(changes, out num, out renderer);
            }
            ContentBlendMode blendMode = this.tool.GetBlendMode(changes);
            IEnumerable<IMaskedRenderer<ColorBgra, ColorAlpha8>> contentRenderers = this.tool.CreateContentRenderers(layer, changes);
            if (contentRenderers == null)
            {
                ExceptionUtil.ThrowInternalErrorException("OnCreateContentRenderer() returned null");
            }
            RectInt32 maxRenderBounds = changes.GetMaxRenderBounds();
            return new BitmapLayerToolLayerOverlay<TTool, TChanges>(layer, RectInt32.Intersect(layer.Bounds(), RectInt32.Intersect(num, maxRenderBounds)), changes, blendMode, contentRenderers, renderer);
        }

        public void GetDefaultContentClip(TChanges changes, out RectInt32 clipRect, out IRenderer<ColorAlpha8> clipMaskRenderer)
        {
            if (this.tool.Selection.IsEmpty)
            {
                clipMaskRenderer = null;
                clipRect = this.tool.Document.Bounds();
            }
            else
            {
                GeometryList cachedClippingMask = this.tool.Selection.GetCachedClippingMask();
                clipRect = RectInt32.Intersect(cachedClippingMask.Bounds.Int32Bound, this.tool.Document.Bounds());
                SelectionRenderingQuality drawingSettingValue = changes.GetDrawingSettingValue<SelectionRenderingQuality>(this.tool.ToolSettings.Selection.RenderingQuality);
                Result<IRenderer<ColorAlpha8>> cachedLazyClippingMaskRenderer = this.tool.Selection.GetCachedLazyClippingMaskRenderer(drawingSettingValue);
                SizeInt32 size = this.tool.Document.Size();
                clipMaskRenderer = LazyRenderer.Create<ColorAlpha8>(size, cachedLazyClippingMaskRenderer);
            }
        }

        public RectInt32 GetDefaultDifferentialMaxBounds(TChanges oldChanges, TChanges newChanges)
        {
            if (oldChanges.Equals((ReferenceValue) newChanges))
            {
                return new RectInt32(newChanges.GetMaxRenderBounds().Location, SizeInt32.Zero);
            }
            RectInt32 maxRenderBounds = oldChanges.GetMaxRenderBounds();
            RectInt32 b = newChanges.GetMaxRenderBounds();
            return RectInt32.Union(maxRenderBounds, b);
        }

        public void RelayActivated()
        {
            if (this.isActivated)
            {
                ExceptionUtil.ThrowInvalidOperationException("already activated");
            }
            this.isActivated = true;
        }

        public void RelayChangesChanged(TChanges oldChanges, TChanges newChanges)
        {
            this.UpdateLayerOverlay();
        }

        public HistoryMemento RelayCommitChanges(TChanges changes, string mementoName, ImageResource mementoImage)
        {
            this.tool.VerifyAccess<TTool>();
            HistoryMemento memento = new ApplyRendererToBitmapLayerHistoryFunction(mementoName, mementoImage, this.tool.ActiveLayerIndex, this.layerOverlay, this.layerOverlay.AffectedBounds, 4, 0x7d0, ActionFlags.KeepToolActive).Execute(this.tool.HistoryWorkspace);
            if (memento != null)
            {
                return memento;
            }
            return new EmptyHistoryMemento(mementoName, mementoImage);
        }

        public void RelayDeactivated()
        {
            if (!this.isActivated)
            {
                ExceptionUtil.ThrowInvalidOperationException("not activated");
            }
            this.isActivated = false;
        }

        public void RelayDeactivating()
        {
        }

        [IteratorStateMachine(typeof(<RelayGetDrawingSettings>d__9))]
        public IEnumerable<Setting> RelayGetDrawingSettings()
        {
            yield return this.tool.ToolSettings.Selection.RenderingQuality;
        }

        private void UpdateLayerOverlay()
        {
            this.tool.VerifyAccess<TTool>();
            using (this.updateLayerOverlayRegion.UseEnterScope())
            {
                TChanges local;
                RectInt32? nullable;
                RectInt32? nullable2;
                RectInt32? nullable3;
                RectInt32? nullable4;
                BitmapLayerToolLayerOverlay<TTool, TChanges> overlay2;
                RectInt32 differentialMaxBounds;
                RectInt32 num3;
                BitmapLayerToolLayerOverlay<TTool, TChanges> layerOverlay = this.layerOverlay;
                if (layerOverlay == null)
                {
                    local = default(TChanges);
                    nullable = null;
                    nullable2 = null;
                }
                else
                {
                    local = layerOverlay.Changes;
                    nullable = new RectInt32?(local.GetMaxRenderBounds());
                    nullable2 = new RectInt32?(layerOverlay.AffectedBounds);
                }
                TChanges changes = this.tool.Changes;
                if (changes == null)
                {
                    nullable3 = null;
                    nullable4 = null;
                    overlay2 = null;
                }
                else
                {
                    nullable3 = new RectInt32?(changes.GetMaxRenderBounds());
                    overlay2 = this.CreateLayerOverlay(this.tool.ActiveLayer, changes);
                    if (overlay2 == null)
                    {
                        ExceptionUtil.ThrowInternalErrorException("CreateLayerOverlay() -> null");
                    }
                    nullable4 = new RectInt32?(overlay2.AffectedBounds);
                }
                RectInt32 num = this.tool.Document.Bounds();
                if ((local == null) && (changes != null))
                {
                    differentialMaxBounds = nullable3.Value;
                    num3 = nullable4.Value;
                }
                else if ((local != null) && (changes == null))
                {
                    differentialMaxBounds = nullable.Value;
                    num3 = nullable2.Value;
                }
                else
                {
                    differentialMaxBounds = this.tool.GetDifferentialMaxBounds(local, changes);
                    num3 = RectInt32.Union(nullable2.Value, nullable4.Value);
                }
                RectInt32 num4 = RectInt32.Intersect(differentialMaxBounds, num3);
                this.layerOverlay = overlay2;
                this.tool.DocumentCanvas.ReplaceLayerOverlay(this.tool.ActiveLayer, layerOverlay, overlay2, new RectInt32?(num4));
            }
        }

        [CompilerGenerated]
        private sealed class <RelayGetDrawingSettings>d__9 : IEnumerable<Setting>, IEnumerable, IEnumerator<Setting>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private Setting <>2__current;
            public BitmapLayerTransactionHandler<TTool, TChanges> <>4__this;
            private int <>l__initialThreadId;

            [DebuggerHidden]
            public <RelayGetDrawingSettings>d__9(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private bool MoveNext()
            {
                switch (this.<>1__state)
                {
                    case 0:
                        this.<>1__state = -1;
                        this.<>2__current = this.<>4__this.tool.ToolSettings.Selection.RenderingQuality;
                        this.<>1__state = 1;
                        return true;

                    case 1:
                        this.<>1__state = -1;
                        break;
                }
                return false;
            }

            [DebuggerHidden]
            IEnumerator<Setting> IEnumerable<Setting>.GetEnumerator()
            {
                if ((this.<>1__state == -2) && (this.<>l__initialThreadId == Environment.CurrentManagedThreadId))
                {
                    this.<>1__state = 0;
                    return (BitmapLayerTransactionHandler<TTool, TChanges>.<RelayGetDrawingSettings>d__9) this;
                }
                return new BitmapLayerTransactionHandler<TTool, TChanges>.<RelayGetDrawingSettings>d__9(0) { <>4__this = this.<>4__this };
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<PaintDotNet.Settings.Setting>.GetEnumerator();

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
            }

            Setting IEnumerator<Setting>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }
    }
}

