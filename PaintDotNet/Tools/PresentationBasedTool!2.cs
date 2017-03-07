namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Canvas;
    using PaintDotNet.Controls;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Rendering;
    using PaintDotNet.Threading;
    using PaintDotNet.Tools.Controls;
    using PaintDotNet.UI;
    using PaintDotNet.UI.Input;
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    internal abstract class PresentationBasedTool<TDerived, TChanges> : TransactedTool<TDerived, TChanges>, ICanvasPresentationSourceHost, INotifyPropertyChanged where TDerived: PresentationBasedTool<TDerived, TChanges> where TChanges: TransactedToolChanges<TChanges, TDerived>
    {
        private bool allowSelectionChanges;
        private static readonly PropertyChangedEventArgs changesPropertyChangedEventArgs;
        private PresentationCanvasLayer presentationCanvasLayer;
        private CanvasPresentationSource presentationSource;
        private static readonly PropertyChangedEventArgs statePropertyChangedEventArgs;

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged;

        static PresentationBasedTool()
        {
            PresentationBasedTool<TDerived, TChanges>.changesPropertyChangedEventArgs = new PropertyChangedEventArgs("Changes");
            PresentationBasedTool<TDerived, TChanges>.statePropertyChangedEventArgs = new PropertyChangedEventArgs("State");
        }

        protected PresentationBasedTool(DocumentWorkspace documentWorkspace, ImageResource toolBarImage, string name, string helpText, char hotKey, bool skipIfActiveOnHotKey, ToolBarConfigItems toolBarConfigItems, bool isCommitSupported) : base(documentWorkspace, toolBarImage, name, helpText, hotKey, skipIfActiveOnHotKey, toolBarConfigItems, isCommitSupported)
        {
        }

        protected override void Dispose(bool disposing)
        {
            this.propertyChanged = null;
            base.Dispose(disposing);
        }

        private void EnsurePresentationSourceMouseEntered()
        {
            if (!this.presentationSource.MouseDevice.IsMouseEntered)
            {
                this.presentationSource.MouseDevice.RelayMouseEnter();
            }
        }

        protected IRenderer<ColorBgra> GetSampleSource(BitmapLayer layer, bool sampleAllLayers)
        {
            if (sampleAllLayers)
            {
                return base.Document.CreateRenderer();
            }
            return layer.Surface;
        }

        protected SelectionCombineMode? GetSelectionCombineModeOverride()
        {
            ModifierKeys modifiers = this.presentationSource.PrimaryKeyboardDevice.Modifiers;
            MouseDevice primaryMouseDevice = this.presentationSource.PrimaryMouseDevice;
            return PresentationBasedTool<TDerived, TChanges>.GetSelectionCombineModeOverride((modifiers & ModifierKeys.Control) == ModifierKeys.Control, (modifiers & ModifierKeys.Alt) == ModifierKeys.Alt, primaryMouseDevice.LeftButton == MouseButtonState.Pressed, primaryMouseDevice.RightButton == MouseButtonState.Pressed);
        }

        protected static SelectionCombineMode? GetSelectionCombineModeOverride(bool isCtrlKeyDown, bool isAltKeyDown, bool isLeftMouseDown, bool isRightMouseDown)
        {
            if (isCtrlKeyDown & isLeftMouseDown)
            {
                return 1;
            }
            if (isAltKeyDown & isLeftMouseDown)
            {
                return 2;
            }
            if (isCtrlKeyDown & isRightMouseDown)
            {
                return 4;
            }
            if (isAltKeyDown & isRightMouseDown)
            {
                return 3;
            }
            return null;
        }

        protected override bool IsSelectionChangeAllowed() => 
            this.allowSelectionChanges;

        protected override void OnActivated()
        {
            base.DocumentWorkspace.VisibleChanged += new EventHandler(this.OnDocumentWorkspaceVisibleChanged);
            this.presentationCanvasLayer = new PresentationCanvasLayer();
            base.DocumentCanvas.CanvasLayers.Add(this.presentationCanvasLayer);
            this.presentationSource = new CanvasPresentationSource(this, this.presentationCanvasLayer.CompositionTarget);
            this.OnPresentationSourceInitialized();
            base.DocumentCanvas.CanvasSizeChanged += new ValueChangedEventHandler<SizeDouble>(this.OnDocumentCanvasCanvasSizeChanged);
            base.OnActivated();
        }

        protected override void OnChangesChanged(TChanges oldChanges, TChanges newChanges)
        {
            this.RaisePropertyChanged(PresentationBasedTool<TDerived, TChanges>.changesPropertyChangedEventArgs);
            base.OnChangesChanged(oldChanges, newChanges);
        }

        protected override void OnDeactivated()
        {
            base.DocumentWorkspace.VisibleChanged -= new EventHandler(this.OnDocumentWorkspaceVisibleChanged);
            base.DocumentCanvas.CanvasSizeChanged -= new ValueChangedEventHandler<SizeDouble>(this.OnDocumentCanvasCanvasSizeChanged);
            this.OnUnloadingUI();
            this.UI = null;
            DisposableUtil.Free<CanvasPresentationSource>(ref this.presentationSource);
            base.DocumentCanvas.CanvasLayers.Remove(this.presentationCanvasLayer);
            DisposableUtil.Free<PresentationCanvasLayer>(ref this.presentationCanvasLayer);
            this.OnUnloadedUI();
            base.OnDeactivated();
        }

        private void OnDocumentCanvasCanvasSizeChanged(object sender, ValueChangedEventArgs<SizeDouble> e)
        {
            this.presentationSource.InvalidateLayout();
        }

        private void OnDocumentWorkspaceVisibleChanged(object sender, EventArgs e)
        {
            this.UpdateUIVisibility();
        }

        protected override void OnEnter()
        {
            this.presentationSource.KeyboardDevice.RelayEnter(base.ModifierKeys);
            base.OnEnter();
        }

        protected override void OnKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            if (!e.Handled)
            {
                this.presentationSource.KeyboardDevice.RelayKeyDown(e);
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(System.Windows.Forms.KeyEventArgs e)
        {
            if (!e.Handled)
            {
                this.presentationSource.KeyboardDevice.RelayKeyUp(e);
            }
            base.OnKeyUp(e);
        }

        protected override void OnLeave()
        {
            this.presentationSource.KeyboardDevice.RelayLeave();
            base.OnLeave();
        }

        protected override void OnMouseDown(MouseEventArgsF e)
        {
            this.EnsurePresentationSourceMouseEntered();
            this.presentationSource.MouseDevice.RelayMouseDown(e);
            base.OnMouseDown(e);
        }

        protected override void OnMouseEnter()
        {
            this.presentationSource.MouseDevice.RelayMouseEnter();
            base.OnMouseEnter();
        }

        protected override void OnMouseLeave()
        {
            if (this.presentationSource.MouseDevice.Captured != null)
            {
                this.presentationSource.MouseDevice.Capture(null);
            }
            foreach (MouseButton button in this.presentationSource.MouseDevice.EnumerateMouseButtons())
            {
                if (this.presentationSource.MouseDevice.GetMouseButtonState(button) == MouseButtonState.Pressed)
                {
                    this.presentationSource.MouseDevice.RelayMouseUp(button);
                }
            }
            this.presentationSource.MouseDevice.RelayMouseLeave();
            base.OnMouseLeave();
        }

        protected override void OnMouseMove(MouseEventArgsF e)
        {
            this.EnsurePresentationSourceMouseEntered();
            this.presentationSource.MouseDevice.RelayMouseMove(e);
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgsF e)
        {
            this.EnsurePresentationSourceMouseEntered();
            this.presentationSource.MouseDevice.RelayMouseUp(e);
            base.OnMouseUp(e);
        }

        protected virtual void OnPresentationSourceInitialized()
        {
        }

        protected override void OnStateChanged(TransactedToolState oldValue, TransactedToolState newValue)
        {
            this.RaisePropertyChanged(PresentationBasedTool<TDerived, TChanges>.statePropertyChangedEventArgs);
            base.OnStateChanged(oldValue, newValue);
        }

        protected virtual void OnUnloadedUI()
        {
        }

        protected virtual void OnUnloadingUI()
        {
        }

        bool ICanvasPresentationSourceHost.RequestFocus() => 
            base.DocumentWorkspace.Focus();

        void ICanvasPresentationSourceHost.Update()
        {
            if (base.DocumentWorkspace.Visible && base.DocumentWorkspace.Enabled)
            {
                Form activeForm = Form.ActiveForm;
                Form form2 = base.DocumentWorkspace.FindForm();
                if ((activeForm != null) && (activeForm == form2))
                {
                    base.DocumentWorkspace.Update();
                }
            }
        }

        protected void RaisePropertyChanged(PropertyChangedEventArgs e)
        {
            ((PresentationBasedTool<TDerived, TChanges>) this).VerifyAccess<PresentationBasedTool<TDerived, TChanges>>();
            if (this.propertyChanged != null)
            {
                this.propertyChanged(this, e);
            }
        }

        protected void RaisePropertyChanged(string propertyName)
        {
            this.RaisePropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        private void UpdateUIVisibility()
        {
            ((PresentationBasedTool<TDerived, TChanges>) this).VerifyAccess<PresentationBasedTool<TDerived, TChanges>>();
            if (this.UI != null)
            {
                this.UI.Visibility = base.DocumentWorkspace.Visible ? Visibility.Visible : Visibility.Hidden;
            }
        }

        SizeDouble ICanvasPresentationSourceHost.CanvasSize =>
            base.DocumentCanvas.CanvasSize;

        PaintDotNet.UI.Input.Cursor ICanvasPresentationSourceHost.Cursor
        {
            set
            {
                if (value == null)
                {
                    base.Cursor = null;
                }
                else
                {
                    base.Cursor = value.WinFormsCursor;
                }
            }
        }

        bool ICanvasPresentationSourceHost.IsFocused =>
            base.DocumentWorkspace.ContainsFocus;

        protected PaintDotNet.UI.PresentationSource PresentationSource =>
            this.presentationSource;

        protected ToolUICanvas<TDerived, TChanges> UI
        {
            get
            {
                if (this.presentationSource == null)
                {
                    return null;
                }
                return (ToolUICanvas<TDerived, TChanges>) this.presentationSource.RootVisual;
            }
            set
            {
                this.presentationSource.RootVisual = value;
                this.UpdateUIVisibility();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        protected struct AllowSelectionChangesScope : IDisposable
        {
            private TDerived owner;
            public AllowSelectionChangesScope(TDerived owner)
            {
                Validate.IsNotNull<TDerived>(owner, "owner");
                if (owner.allowSelectionChanges)
                {
                    ExceptionUtil.ThrowInvalidOperationException();
                }
                this.owner = owner;
                this.owner.allowSelectionChanges = true;
            }

            public void Dispose()
            {
                if (this.owner != null)
                {
                    if (!this.owner.allowSelectionChanges)
                    {
                        ExceptionUtil.ThrowInternalErrorException();
                    }
                    this.owner.allowSelectionChanges = false;
                    this.owner = default(TDerived);
                }
            }
        }
    }
}

