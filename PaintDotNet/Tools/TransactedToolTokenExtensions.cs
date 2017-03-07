namespace PaintDotNet.Tools
{
    using System;
    using System.Runtime.CompilerServices;

    internal static class TransactedToolTokenExtensions
    {
        public static bool IsActive(this TransactedToolToken token) => 
            (((token != null) && !token.HasResponded) && !token.IsDisposed);
    }
}

