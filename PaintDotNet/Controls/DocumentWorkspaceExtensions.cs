namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Dialogs;
    using PaintDotNet.HistoryFunctions;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Resources;
    using System;
    using System.Runtime.CompilerServices;

    internal static class DocumentWorkspaceExtensions
    {
        public static HistoryFunctionResult ApplyFunction(this DocumentWorkspace dw, HistoryFunction function)
        {
            HistoryFunctionResult successNoOp;
            bool flag = false;
            if ((function.ActionFlags & ActionFlags.KeepToolActive) != ActionFlags.KeepToolActive)
            {
                dw.PushNullTool();
                dw.Update();
                flag = true;
            }
            try
            {
                using (new WaitCursorChanger(dw))
                {
                    string localizedErrorText;
                    HistoryMemento memento = null;
                    Exception exception = null;
                    try
                    {
                        memento = function.Execute(dw);
                        if (memento == null)
                        {
                            successNoOp = HistoryFunctionResult.SuccessNoOp;
                        }
                        else
                        {
                            successNoOp = HistoryFunctionResult.Success;
                        }
                        localizedErrorText = null;
                    }
                    catch (HistoryFunctionNonFatalException exception2)
                    {
                        exception = exception2;
                        if (exception2.InnerException is OutOfMemoryException)
                        {
                            successNoOp = HistoryFunctionResult.OutOfMemory;
                        }
                        else
                        {
                            successNoOp = HistoryFunctionResult.NonFatalError;
                        }
                        if (exception2.LocalizedErrorText != null)
                        {
                            localizedErrorText = exception2.LocalizedErrorText;
                        }
                        else if (exception2.InnerException is OutOfMemoryException)
                        {
                            localizedErrorText = PdnResources.GetString("ExecuteFunction.GenericOutOfMemory");
                        }
                        else
                        {
                            localizedErrorText = PdnResources.GetString("ExecuteFunction.GenericError");
                        }
                    }
                    if ((localizedErrorText != null) && (exception != null))
                    {
                        ExceptionDialog.ShowErrorDialog(dw, localizedErrorText, exception);
                    }
                    else if ((localizedErrorText != null) && (exception == null))
                    {
                        MessageBoxUtil.ErrorBox(dw, localizedErrorText);
                    }
                    if (memento != null)
                    {
                        dw.History.PushNewMemento(memento);
                    }
                }
            }
            finally
            {
                if (flag)
                {
                    dw.PopNullTool();
                }
            }
            return successNoOp;
        }

        public static void FlushTool(this DocumentWorkspace dw)
        {
            using (new PushNullToolMode(dw))
            {
            }
        }
    }
}

