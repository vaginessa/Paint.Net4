namespace PaintDotNet.Tools
{
    using System;

    internal abstract class TransactedToolEditingToken<TChanges> : TransactedToolChangesToken<TChanges>
    {
        protected TransactedToolEditingToken()
        {
        }
    }
}

