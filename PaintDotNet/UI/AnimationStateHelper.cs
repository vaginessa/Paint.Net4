namespace PaintDotNet.UI
{
    using PaintDotNet;
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;

    internal sealed class AnimationStateHelper : Disposable
    {
        private PaintDotNet.UI.FrameworkElement element;
        private bool shouldEnableAnimations;

        [field: CompilerGenerated]
        public event EventHandler DisableAnimations;

        [field: CompilerGenerated]
        public event EventHandler EnableAnimations;

        [field: CompilerGenerated]
        public event ValueChangedEventHandler<bool> ShouldEnableAnimationsChanged;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Element = null;
            }
            this.ShouldEnableAnimationsChanged = null;
            this.EnableAnimations = null;
            this.DisableAnimations = null;
            base.Dispose(disposing);
        }

        private void OnElementIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.UpdateShouldEnableAnimations();
        }

        private void OnElementLoaded(object sender, EventArgs e)
        {
            this.UpdateShouldEnableAnimations();
        }

        private void OnElementUnloaded(object sender, EventArgs e)
        {
            this.UpdateShouldEnableAnimations();
        }

        private void UpdateShouldEnableAnimations()
        {
            this.VerifyAccess();
            if (((this.element != null) && this.element.IsLoaded) && this.element.IsVisible)
            {
                this.ShouldEnableAnimations = true;
            }
            else
            {
                this.ShouldEnableAnimations = false;
            }
        }

        private void VerifyAccess()
        {
            if (this.element != null)
            {
                this.element.VerifyAccess();
            }
        }

        public PaintDotNet.UI.FrameworkElement Element
        {
            get => 
                this.element;
            set
            {
                this.VerifyAccess();
                if (value != this.element)
                {
                    this.ShouldEnableAnimations = false;
                    if (this.element != null)
                    {
                        this.element.IsVisibleChanged -= new DependencyPropertyChangedEventHandler(this.OnElementIsVisibleChanged);
                        this.element.Loaded -= new EventHandler(this.OnElementLoaded);
                        this.element.Unloaded -= new EventHandler(this.OnElementUnloaded);
                    }
                    this.element = value;
                    if (this.element != null)
                    {
                        this.element.IsVisibleChanged += new DependencyPropertyChangedEventHandler(this.OnElementIsVisibleChanged);
                        this.element.Loaded += new EventHandler(this.OnElementLoaded);
                        this.element.Unloaded += new EventHandler(this.OnElementUnloaded);
                    }
                    this.UpdateShouldEnableAnimations();
                }
            }
        }

        public bool ShouldEnableAnimations
        {
            get => 
                this.shouldEnableAnimations;
            private set
            {
                this.VerifyAccess();
                if (value != this.shouldEnableAnimations)
                {
                    this.shouldEnableAnimations = value;
                    this.ShouldEnableAnimationsChanged.Raise<bool>(this, !value, value);
                    if (value)
                    {
                        this.EnableAnimations.Raise(this);
                    }
                    else
                    {
                        this.DisableAnimations.Raise(this);
                    }
                }
            }
        }
    }
}

