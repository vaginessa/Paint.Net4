namespace PaintDotNet.Canvas
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using PaintDotNet.UI.Input;
    using System;
    using System.Collections.Generic;

    internal sealed class CanvasMouseDevice : MouseDevice
    {
        public CanvasMouseDevice(CanvasPresentationSource presentationSource) : base(presentationSource)
        {
        }

        public void RelayMouseDown(MouseEventArgsF e)
        {
            foreach (MouseButton button in WinFormsInputHelpers.FromWinFormsMouseButton(e.Button))
            {
                base.ProcessMouseDown(button);
            }
        }

        public void RelayMouseEnter()
        {
            base.ProcessMouseEnter();
        }

        public void RelayMouseLeave()
        {
            base.ProcessMouseLeave();
        }

        public void RelayMouseMove(MouseEventArgsF e)
        {
            base.ProcessMouseMove(e.Point, e.IntermediatePoints);
        }

        public void RelayMouseMove(PointDouble point)
        {
            PointDouble[] intermediatePoints = new PointDouble[] { point };
            this.RelayMouseMove(point, intermediatePoints);
        }

        public void RelayMouseMove(PointDouble point, IList<PointDouble> intermediatePoints)
        {
            base.ProcessMouseMove(point, intermediatePoints);
        }

        public void RelayMouseUp(MouseEventArgsF e)
        {
            foreach (MouseButton button in WinFormsInputHelpers.FromWinFormsMouseButton(e.Button))
            {
                base.ProcessMouseUp(button);
            }
        }

        public void RelayMouseUp(MouseButton mouseButton)
        {
            base.ProcessMouseUp(mouseButton);
        }
    }
}

