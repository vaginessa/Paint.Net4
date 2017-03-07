namespace PaintDotNet.Canvas
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Direct2D;
    using PaintDotNet.Rendering;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;

    internal class Canvas : DependencyObject, IDisposable, IIsDisposed
    {
        private HashSet<CanvasLayer> canvasLayersCopy;
        public static readonly DependencyProperty CanvasLayersProperty = CanvasLayersPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey CanvasLayersPropertyKey = DependencyProperty.RegisterReadOnly("CanvasLayers", typeof(ObservableCollection<CanvasLayer>), typeof(PaintDotNet.Canvas.Canvas), PropertyMetadataUtil.Null);
        private static readonly RectDouble canvasMaxBounds = RectDouble.FromEdges(-131072.0, -131072.0, 131072.0, 131072.0);
        public const double CanvasMaximumX = 131072.0;
        public const double CanvasMaximumY = 131072.0;
        private static readonly SizeDouble canvasMaxSize = new SizeDouble(131072.0, 131072.0);
        public const double CanvasMinimumX = -131072.0;
        public const double CanvasMinimumY = -131072.0;
        public static readonly DependencyProperty CanvasSizeProperty = DependencyProperty.Register("CanvasSize", typeof(SizeDouble), typeof(PaintDotNet.Canvas.Canvas), new PropertyMetadata(SizeDouble.Zero, new PropertyChangedCallback(<>c.<>9.<.cctor>b__70_0), new CoerceValueCallback(<>c.<>9.<.cctor>b__70_1)));
        private bool isDisposed;
        private bool isRendering;
        private HashSet<CanvasView> registeredViews;
        private ReadOnlyHashSet<CanvasView> registeredViewsRO;
        private int visibleViewsCount;

        [field: CompilerGenerated]
        public event ValueChangedEventHandler<SizeDouble> CanvasSizeChanged;

        [field: CompilerGenerated]
        public event EventHandler<CanvasInvalidatedEventArgs> Invalidated;

        [field: CompilerGenerated]
        public event ValueChangedEventHandler<int> VisibleViewsCountChanged;

        public Canvas()
        {
            this.CanvasLayers = new ObservableCollection<CanvasLayer>();
            this.CanvasLayers.CollectionChanged += new NotifyCollectionChangedEventHandler(this.OnCanvasLayersCollectionChanged);
            this.canvasLayersCopy = new HashSet<CanvasLayer>();
            this.registeredViews = new HashSet<CanvasView>();
            this.registeredViewsRO = this.registeredViews.AsReadOnly<CanvasView>();
        }

        public void AfterRender(RectFloat clipRect, CanvasView canvasView)
        {
            if (!this.registeredViews.Contains(canvasView))
            {
                ExceptionUtil.ThrowInvalidOperationException("The CanvasView must be registered first using RegisterView()");
            }
            if (canvasView.IsVisible)
            {
                foreach (CanvasLayer layer in this.CanvasLayers)
                {
                    if (layer.IsVisible && !layer.IsTopMost)
                    {
                        layer.AfterRender(clipRect, canvasView);
                    }
                }
                foreach (CanvasLayer layer2 in this.CanvasLayers)
                {
                    if (layer2.IsVisible && layer2.IsTopMost)
                    {
                        layer2.AfterRender(clipRect, canvasView);
                    }
                }
            }
        }

        public void BeforeRender(RectFloat clipRect, CanvasView canvasView)
        {
            if (!this.registeredViews.Contains(canvasView))
            {
                ExceptionUtil.ThrowInvalidOperationException("The CanvasView must be registered first using RegisterView()");
            }
            if (canvasView.IsVisible)
            {
                foreach (CanvasLayer layer in this.CanvasLayers)
                {
                    if (layer.IsVisible && !layer.IsTopMost)
                    {
                        layer.BeforeRender(clipRect, canvasView);
                    }
                }
                foreach (CanvasLayer layer2 in this.CanvasLayers)
                {
                    if (layer2.IsVisible && layer2.IsTopMost)
                    {
                        layer2.BeforeRender(clipRect, canvasView);
                    }
                }
            }
        }

        public void Dispose()
        {
            base.VerifyAccess();
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                for (int i = this.registeredViews.Count - 1; i >= 0; i--)
                {
                    this.UnregisterView(this.registeredViews.First<CanvasView>());
                }
            }
            this.isDisposed = true;
        }

        ~Canvas()
        {
            this.Dispose(false);
        }

        public void Invalidate()
        {
            this.Invalidate(CanvasMaxBounds);
        }

        protected void Invalidate(CanvasInvalidatedEventArgs e)
        {
            base.VerifyAccess();
            this.OnInvalidated(e);
        }

        public void Invalidate(RectDouble canvasRect)
        {
            base.VerifyAccess();
            this.OnInvalidated(CanvasInvalidatedEventArgs.IdentityCallback, canvasRect);
        }

        public void Invalidate(CalculateInvalidRectCallback callback, RectDouble canvasRect)
        {
            base.VerifyAccess();
            this.OnInvalidated(callback, canvasRect);
        }

        public void InvalidateDeviceResources(CanvasView canvasView)
        {
            base.VerifyAccess();
            this.OnInvalidateDeviceResources(canvasView);
            foreach (CanvasLayer layer in this.CanvasLayers)
            {
                layer.InvalidateDeviceResources(canvasView);
            }
        }

        private void OnCanvasLayerAdded(CanvasLayer canvasLayer)
        {
            canvasLayer.Owner = this;
            canvasLayer.Invalidated += new EventHandler<CanvasInvalidatedEventArgs>(this.OnCanvasLayerInvalidated);
            foreach (CanvasView view in this.registeredViews)
            {
                canvasLayer.RegisterView(view);
            }
        }

        private void OnCanvasLayerInvalidated(object sender, CanvasInvalidatedEventArgs e)
        {
            base.VerifyAccess();
            this.Invalidate(e);
        }

        private void OnCanvasLayerRemoved(CanvasLayer canvasLayer)
        {
            foreach (CanvasView view in this.registeredViews)
            {
                canvasLayer.UnregisterView(view);
            }
            canvasLayer.Owner = null;
            canvasLayer.Invalidated -= new EventHandler<CanvasInvalidatedEventArgs>(this.OnCanvasLayerInvalidated);
        }

        private void OnCanvasLayersAdded(IEnumerable<CanvasLayer> canvasLayers)
        {
            foreach (CanvasLayer layer in canvasLayers)
            {
                this.OnCanvasLayerAdded(layer);
            }
        }

        private void OnCanvasLayersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.VerifyAccess();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    this.OnCanvasLayersAdded(e.NewItems.Cast<CanvasLayer>());
                    break;

                case NotifyCollectionChangedAction.Remove:
                    this.OnCanvasLayersRemoved(e.OldItems.Cast<CanvasLayer>());
                    break;

                case NotifyCollectionChangedAction.Replace:
                    this.OnCanvasLayersRemoved(e.OldItems.Cast<CanvasLayer>());
                    this.OnCanvasLayersAdded(e.NewItems.Cast<CanvasLayer>());
                    break;

                case NotifyCollectionChangedAction.Reset:
                    foreach (CanvasLayer layer in this.canvasLayersCopy)
                    {
                        this.OnCanvasLayerRemoved(layer);
                    }
                    this.canvasLayersCopy.Clear();
                    break;
            }
            this.Invalidate();
        }

        private void OnCanvasLayersRemoved(IEnumerable<CanvasLayer> canvasLayers)
        {
            foreach (CanvasLayer layer in canvasLayers)
            {
                this.OnCanvasLayerRemoved(layer);
            }
        }

        protected virtual void OnCanvasSizePropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            this.CanvasSizeChanged.Raise<SizeDouble>(this, e);
            this.Invalidate();
        }

        private void OnCanvasViewIsVisibleChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            int num;
            if (e.NewValue && !e.OldValue)
            {
                num = this.VisibleViewsCount + 1;
                this.VisibleViewsCount = num;
            }
            else if (!e.NewValue && e.OldValue)
            {
                num = this.VisibleViewsCount - 1;
                this.VisibleViewsCount = num;
            }
            else
            {
                ExceptionUtil.ThrowInternalErrorException();
            }
        }

        protected virtual object OnCoerceCanvasSizeProperty(object baseValue) => 
            baseValue;

        private void OnInvalidated(CanvasInvalidatedEventArgs e)
        {
            this.Invalidated.Raise<CanvasInvalidatedEventArgs>(this, e);
        }

        private void OnInvalidated(CalculateInvalidRectCallback callback, RectDouble canvasRect)
        {
            this.Invalidated.RaisePooled<CanvasInvalidatedEventArgs, CalculateInvalidRectCallback, RectDouble>(this, callback, canvasRect);
        }

        protected virtual void OnInvalidateDeviceResources(CanvasView canvasView)
        {
        }

        protected virtual void OnViewRegistered(CanvasView canvasView)
        {
        }

        protected virtual void OnViewUnregistered(CanvasView canvasView)
        {
        }

        protected virtual void OnVisibleViewsCountChanged(int oldValue, int newValue)
        {
            this.VisibleViewsCountChanged.Raise<int>(this, oldValue, newValue);
        }

        public void RegisterView(CanvasView canvasView)
        {
            base.VerifyAccess();
            if (this.registeredViews.Add(canvasView))
            {
                foreach (CanvasLayer layer in this.CanvasLayers)
                {
                    layer.RegisterView(canvasView);
                }
                canvasView.IsVisibleChanged += new ValueChangedEventHandler<bool>(this.OnCanvasViewIsVisibleChanged);
                if (canvasView.IsVisible)
                {
                    int num = this.VisibleViewsCount + 1;
                    this.VisibleViewsCount = num;
                }
                this.OnViewRegistered(canvasView);
            }
        }

        public void Render(IDrawingContext dc, RectFloat clipRect, CanvasView canvasView)
        {
            base.VerifyAccess();
            if (this.isRendering)
            {
                ExceptionUtil.ThrowInvalidOperationException("Render() is not re-entrant");
            }
            if (!this.registeredViews.Contains(canvasView))
            {
                ExceptionUtil.ThrowInvalidOperationException("The CanvasView must be registered first using RegisterView()");
            }
            if (canvasView.IsVisible)
            {
                this.isRendering = true;
                try
                {
                    PointDouble viewportCanvasOffset = canvasView.ViewportCanvasOffset;
                    float scaleRatio = (float) canvasView.ScaleRatio;
                    float x = (float) viewportCanvasOffset.X;
                    float y = (float) viewportCanvasOffset.Y;
                    using (dc.UseScaleTransform(scaleRatio, scaleRatio, MatrixMultiplyOrder.Prepend))
                    {
                        using (dc.UseTranslateTransform(-x, -y, MatrixMultiplyOrder.Prepend))
                        {
                            foreach (CanvasLayer layer in this.CanvasLayers)
                            {
                                if (layer.IsVisible && !layer.IsTopMost)
                                {
                                    using (dc.UseSaveDrawingState())
                                    {
                                        layer.Render(dc, clipRect, canvasView);
                                    }
                                }
                            }
                            foreach (CanvasLayer layer2 in this.CanvasLayers)
                            {
                                if (layer2.IsVisible && layer2.IsTopMost)
                                {
                                    using (dc.UseSaveDrawingState())
                                    {
                                        layer2.Render(dc, clipRect, canvasView);
                                    }
                                }
                            }
                        }
                    }
                }
                finally
                {
                    this.isRendering = false;
                }
            }
        }

        public void UnregisterView(CanvasView canvasView)
        {
            base.VerifyAccess();
            if (this.registeredViews.Remove(canvasView))
            {
                foreach (CanvasLayer layer in this.CanvasLayers)
                {
                    layer.UnregisterView(canvasView);
                }
                canvasView.IsVisibleChanged -= new ValueChangedEventHandler<bool>(this.OnCanvasViewIsVisibleChanged);
                if (canvasView.IsVisible)
                {
                    int num = this.VisibleViewsCount - 1;
                    this.VisibleViewsCount = num;
                }
                this.OnViewUnregistered(canvasView);
            }
        }

        public ObservableCollection<CanvasLayer> CanvasLayers
        {
            get => 
                ((ObservableCollection<CanvasLayer>) base.GetValue(CanvasLayersProperty));
            private set
            {
                base.SetValue(CanvasLayersPropertyKey, value);
            }
        }

        public static RectDouble CanvasMaxBounds =>
            canvasMaxBounds;

        public static SizeDouble CanvasMaxSize =>
            canvasMaxSize;

        public SizeDouble CanvasSize
        {
            get => 
                ((SizeDouble) base.GetValue(CanvasSizeProperty));
            set
            {
                base.SetValue(CanvasSizeProperty, value);
            }
        }

        public bool IsDisposed =>
            this.isDisposed;

        public ReadOnlyHashSet<CanvasView> RegisteredViews
        {
            get
            {
                base.VerifyAccess();
                return this.registeredViewsRO;
            }
        }

        public int VisibleViewsCount
        {
            get => 
                this.visibleViewsCount;
            private set
            {
                base.VerifyAccess();
                int visibleViewsCount = this.visibleViewsCount;
                if (value != visibleViewsCount)
                {
                    this.visibleViewsCount = value;
                    this.OnVisibleViewsCountChanged(visibleViewsCount, value);
                }
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly PaintDotNet.Canvas.Canvas.<>c <>9 = new PaintDotNet.Canvas.Canvas.<>c();

            internal void <.cctor>b__70_0(DependencyObject s, DependencyPropertyChangedEventArgs e)
            {
                ((PaintDotNet.Canvas.Canvas) s).OnCanvasSizePropertyChanged(e);
            }

            internal object <.cctor>b__70_1(DependencyObject dO, object bV) => 
                ((PaintDotNet.Canvas.Canvas) dO).OnCoerceCanvasSizeProperty(bV);
        }
    }
}

