namespace PaintDotNet.Tools.Controls
{
    using System;

    internal abstract class ToolUICanvas<TTool, TChanges> : ToolUICanvas where TTool: PresentationBasedTool<TTool, TChanges> where TChanges: TransactedToolChanges<TChanges, TTool>
    {
        protected ToolUICanvas()
        {
        }

        public TTool Tool
        {
            get => 
                ((TTool) base.Tool);
            set
            {
                base.Tool = value;
            }
        }
    }
}

