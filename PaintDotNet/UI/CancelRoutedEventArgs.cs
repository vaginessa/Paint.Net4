namespace PaintDotNet.UI
{
    using System;
    using System.Windows;

    internal class CancelRoutedEventArgs : RoutedEventArgs
    {
        private bool cancel;

        public CancelRoutedEventArgs(RoutedEvent routedEvent, DependencyObject source) : base(routedEvent, source)
        {
        }

        public bool Cancel
        {
            get => 
                this.cancel;
            set
            {
                this.cancel = value;
            }
        }
    }
}

