namespace PaintDotNet.UI.Input
{
    using PaintDotNet.Resources;
    using System;
    using System.IO;

    internal static class CursorUtil
    {
        public static Cursor LoadResource(string resourceName)
        {
            Cursor arrow;
            try
            {
                using (Stream stream = PdnResources.CreateResourceStream(resourceName))
                {
                    arrow = new Cursor(stream, resourceName);
                }
            }
            catch (OutOfMemoryException)
            {
                arrow = Cursors.Arrow;
            }
            return arrow;
        }
    }
}

