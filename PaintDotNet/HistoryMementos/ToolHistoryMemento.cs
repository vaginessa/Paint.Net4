namespace PaintDotNet.HistoryMementos
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using System;

    internal abstract class ToolHistoryMemento : HistoryMemento
    {
        private int activeLayerIndex;
        private PaintDotNet.Controls.DocumentWorkspace documentWorkspace;
        private Type toolType;

        public ToolHistoryMemento(PaintDotNet.Controls.DocumentWorkspace documentWorkspace, string name, ImageResource image) : base(name, image)
        {
            this.documentWorkspace = documentWorkspace;
            this.activeLayerIndex = this.documentWorkspace.ActiveLayerIndex;
            this.toolType = documentWorkspace.GetToolType();
        }

        protected abstract HistoryMemento OnToolUndo(ProgressEventHandler progressCallback);
        protected sealed override HistoryMemento OnUndo(ProgressEventHandler progressCallback)
        {
            if (this.documentWorkspace.GetToolType() != this.toolType)
            {
                this.documentWorkspace.SetToolFromType(this.toolType);
            }
            if (this.documentWorkspace.ActiveLayerIndex != this.activeLayerIndex)
            {
                this.documentWorkspace.ActiveLayerIndex = this.activeLayerIndex;
            }
            return this.OnToolUndo(progressCallback);
        }

        protected PaintDotNet.Controls.DocumentWorkspace DocumentWorkspace =>
            this.documentWorkspace;

        public Type ToolType =>
            this.toolType;
    }
}

