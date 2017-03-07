namespace PaintDotNet.HistoryFunctions
{
    using System;

    internal enum HistoryFunctionResult
    {
        Success,
        SuccessNoOp,
        Cancelled,
        OutOfMemory,
        NonFatalError
    }
}

