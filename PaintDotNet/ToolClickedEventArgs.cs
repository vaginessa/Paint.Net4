namespace PaintDotNet
{
    using PaintDotNet.Tools;
    using System;

    internal class ToolClickedEventArgs : EventArgs
    {
        private Type toolType;

        public ToolClickedEventArgs(Tool tool)
        {
            this.toolType = tool.GetType();
        }

        public ToolClickedEventArgs(Type toolType)
        {
            this.toolType = toolType;
        }

        public Type ToolType =>
            this.toolType;
    }
}

