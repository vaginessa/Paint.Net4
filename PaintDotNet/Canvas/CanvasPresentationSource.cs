namespace PaintDotNet.Canvas
{
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Rendering;
    using PaintDotNet.UI;
    using PaintDotNet.UI.Input;
    using System;

    internal sealed class CanvasPresentationSource : PresentationSource
    {
        private ICanvasPresentationSourceHost host;
        private WinFormsKeyboardDevice keyboardDevice;
        private CanvasMouseDevice mouseDevice;

        public CanvasPresentationSource(ICanvasPresentationSourceHost host, CompositionTarget compositionTarget) : base(compositionTarget)
        {
            Validate.IsNotNull<ICanvasPresentationSourceHost>(host, "host");
            this.host = host;
            this.mouseDevice = new CanvasMouseDevice(this);
            this.keyboardDevice = new WinFormsKeyboardDevice(this);
        }

        protected override SizeDouble GetAvailableSizeForMeasure() => 
            this.host.CanvasSize;

        protected override InputDevice[] GetInputDevices() => 
            new InputDevice[] { this.keyboardDevice, this.mouseDevice };

        protected override void OnCursorChanged(Cursor cursor)
        {
            this.host.Cursor = cursor;
            base.OnCursorChanged(cursor);
        }

        protected override void OnLayoutUpdated()
        {
            this.host.Update();
            base.OnLayoutUpdated();
        }

        internal override bool RequestFocus() => 
            this.host.RequestFocus();

        public WinFormsKeyboardDevice KeyboardDevice =>
            this.keyboardDevice;

        public CanvasMouseDevice MouseDevice =>
            this.mouseDevice;
    }
}

