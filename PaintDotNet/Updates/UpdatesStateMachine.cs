namespace PaintDotNet.Updates
{
    using System;
    using System.Windows.Forms;

    internal class UpdatesStateMachine : StateMachine
    {
        private IWin32Window uiContext;

        public UpdatesStateMachine() : base(new StartupState(), objArray1)
        {
            object[] objArray1 = new object[] { UpdatesAction.Continue, UpdatesAction.Cancel };
        }

        public IWin32Window UIContext
        {
            get => 
                this.uiContext;
            set
            {
                this.uiContext = value;
            }
        }
    }
}

