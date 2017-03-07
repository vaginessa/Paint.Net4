namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryFunctions;
    using PaintDotNet.Rendering;
    using System;

    internal sealed class PasteInToNewLayerAction
    {
        private IPdnDataObject clipData;
        private DocumentWorkspace documentWorkspace;
        private MaskedSurface maskedSurface;

        public PasteInToNewLayerAction(DocumentWorkspace documentWorkspace) : this(documentWorkspace, null, null)
        {
        }

        public PasteInToNewLayerAction(DocumentWorkspace documentWorkspace, IPdnDataObject clipData, MaskedSurface maskedSurface)
        {
            this.documentWorkspace = documentWorkspace;
            this.clipData = clipData;
            this.maskedSurface = maskedSurface;
        }

        public bool PerformAction()
        {
            bool flag2;
            try
            {
                if (this.documentWorkspace.ApplyFunction(new AddNewBlankLayerFunction()) == HistoryFunctionResult.Success)
                {
                    PasteAction action = new PasteAction(this.documentWorkspace, this.clipData, this.maskedSurface);
                    if (!action.PerformAction())
                    {
                        using (new WaitCursorChanger(this.documentWorkspace))
                        {
                            this.documentWorkspace.History.StepBackward(this.documentWorkspace.AppWorkspace);
                            goto Label_006E;
                        }
                    }
                    return true;
                }
            Label_006E:
                flag2 = false;
            }
            finally
            {
                this.clipData = null;
                this.maskedSurface = null;
            }
            return flag2;
        }
    }
}

