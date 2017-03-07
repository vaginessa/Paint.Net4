namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.HistoryFunctions;
    using PaintDotNet.Rendering;
    using System;

    internal static class CropToSelectionAction
    {
        public static void PerformAction(DocumentWorkspace docWorkspace)
        {
            Validate.IsNotNull<DocumentWorkspace>(docWorkspace, "docWorkspace");
            using (new PushNullToolMode(docWorkspace))
            {
                GeometryList cachedClippingMask = docWorkspace.Selection.GetCachedClippingMask();
                if (!cachedClippingMask.IsEmpty)
                {
                    RectDouble bounds = cachedClippingMask.Bounds;
                    if (bounds.Area >= 1.0)
                    {
                        PointDouble viewportCanvasOffset = docWorkspace.CanvasView.ViewportCanvasOffset;
                        if ((docWorkspace.ApplyFunction(new CropToSelectionFunction()) == HistoryFunctionResult.Success) && (docWorkspace.ZoomBasis == ZoomBasis.ScaleFactor))
                        {
                            PointDouble num3 = viewportCanvasOffset - ((VectorDouble) bounds.TopLeft);
                            docWorkspace.CanvasView.ViewportCanvasOffset = num3;
                        }
                    }
                }
            }
        }
    }
}

