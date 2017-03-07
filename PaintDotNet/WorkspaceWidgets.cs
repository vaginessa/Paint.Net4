namespace PaintDotNet
{
    using PaintDotNet.Controls;
    using PaintDotNet.Dialogs;
    using System;

    internal class WorkspaceWidgets
    {
        private PaintDotNet.Dialogs.ColorsForm colorsForm;
        private PaintDotNet.Controls.CommonActionsStrip commonActionsStrip;
        private PaintDotNet.Controls.DocumentStrip documentStrip;
        private PaintDotNet.Dialogs.HistoryForm historyForm;
        private LayerForm layersForm;
        private IStatusBarProgress statusBarProgress;
        private PaintDotNet.Controls.ToolConfigStrip toolConfigStrip;
        private PaintDotNet.Dialogs.ToolsForm toolsForm;
        private AppWorkspace workspace;

        public WorkspaceWidgets(AppWorkspace workspace)
        {
            this.workspace = workspace;
        }

        public PaintDotNet.Dialogs.ColorsForm ColorsForm
        {
            get => 
                this.colorsForm;
            set
            {
                this.colorsForm = value;
            }
        }

        public PaintDotNet.Controls.CommonActionsStrip CommonActionsStrip
        {
            get => 
                this.commonActionsStrip;
            set
            {
                this.commonActionsStrip = value;
            }
        }

        public PaintDotNet.Controls.DocumentStrip DocumentStrip
        {
            get => 
                this.documentStrip;
            set
            {
                this.documentStrip = value;
            }
        }

        public PaintDotNet.Controls.HistoryControl HistoryControl =>
            this.historyForm.HistoryControl;

        public PaintDotNet.Dialogs.HistoryForm HistoryForm
        {
            get => 
                this.historyForm;
            set
            {
                this.historyForm = value;
            }
        }

        public LayerForm LayersForm
        {
            get => 
                this.layersForm;
            set
            {
                this.layersForm = value;
            }
        }

        public IStatusBarProgress StatusBarProgress
        {
            get => 
                this.statusBarProgress;
            set
            {
                this.statusBarProgress = value;
            }
        }

        public PaintDotNet.Controls.ToolConfigStrip ToolConfigStrip
        {
            get => 
                this.toolConfigStrip;
            set
            {
                this.toolConfigStrip = value;
            }
        }

        public PaintDotNet.Controls.ToolsControl ToolsControl =>
            this.toolsForm.ToolsControl;

        public PaintDotNet.Dialogs.ToolsForm ToolsForm
        {
            get => 
                this.toolsForm;
            set
            {
                this.toolsForm = value;
            }
        }
    }
}

