namespace PaintDotNet.Tools.Controls
{
    using PaintDotNet.UI;
    using System;
    using System.Windows;

    internal sealed class TransformEditingBeginEventArgs : CancelRoutedEventArgs
    {
        private TransformEditingMode editingMode;
        private TransformHandleType? triggerHandle;

        public TransformEditingBeginEventArgs(RoutedEvent routedEvent, DependencyObject source, TransformEditingMode editingMode, TransformHandleType? triggerHandle) : base(routedEvent, source)
        {
            this.editingMode = editingMode;
            this.triggerHandle = triggerHandle;
        }

        public TransformEditingMode EditingMode =>
            this.editingMode;

        public TransformHandleType? TriggerHandle =>
            this.triggerHandle;
    }
}

