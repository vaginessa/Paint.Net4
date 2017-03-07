namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Dialogs;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.Runtime;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Threading;
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Threading;

    internal abstract class CopyToClipboardActionBase
    {
        private PaintDotNet.Controls.DocumentWorkspace documentWorkspace;

        public CopyToClipboardActionBase(PaintDotNet.Controls.DocumentWorkspace documentWorkspace)
        {
            this.documentWorkspace = documentWorkspace;
        }

        protected abstract IRenderer<ColorBgra> GetSource();
        public bool PerformAction()
        {
            bool flag = true;
            if (!this.QueryCanPerformAction())
            {
                return false;
            }
            try
            {
                using (new WaitCursorChanger(this.documentWorkspace))
                {
                    IRenderer<ColorBgra> source = this.GetSource();
                    RectInt32 num = source.Bounds<ColorBgra>();
                    GeometryList cachedClippingMask = this.documentWorkspace.Selection.GetCachedClippingMask();
                    RectInt32 a = cachedClippingMask.Bounds.Int32Bound;
                    IRenderer<ColorAlpha8> cachedClippingMaskRenderer = this.documentWorkspace.Selection.GetCachedClippingMaskRenderer(this.documentWorkspace.ToolSettings.Selection.RenderingQuality.Value);
                    RectInt32 sourceRect = RectInt32.Intersect(a, source.Bounds<ColorBgra>());
                    IRenderer<ColorBgra> renderer3 = new ClippedRenderer<ColorBgra>(source, sourceRect);
                    IRenderer<ColorAlpha8> alpha = new ClippedRenderer<ColorAlpha8>(cachedClippingMaskRenderer, sourceRect);
                    IRenderer<ColorBgra> sourceRHS = new MultiplyAlphaChannelRendererBgra32(renderer3, alpha);
                    IRenderer<ColorBgra> sourceLHS = new SolidColorRendererBgra(sourceRHS.Width, sourceRHS.Height, ColorBgra.White);
                    IRenderer<ColorBgra> renderer = new BlendRendererBgra(sourceLHS, CompositionOps.Normal.Static, sourceRHS);
                    if ((a.Width > 0) && (a.Height > 0))
                    {
                        int num5 = 10;
                        while (num5 >= 0)
                        {
                            try
                            {
                                try
                                {
                                    using (Clipboard.Transaction transaction = Clipboard.Open(this.documentWorkspace))
                                    {
                                        transaction.Empty();
                                        using (MaskedSurface surface = MaskedSurface.CopyFrom(source, cachedClippingMask))
                                        {
                                            transaction.AddManagedData(surface);
                                            using (Surface surface2 = surface.Surface.CreateWindow(new Rectangle(0, 0, a.Width, a.Height)))
                                            {
                                                sourceRHS.Parallelize<ColorBgra>(TilingStrategy.Tiles, 7, WorkItemQueuePriority.Normal).Render<ColorBgra>(surface2);
                                                using (Bitmap copyBitmap = surface2.CreateAliasedBitmap(true))
                                                {
                                                    transaction.AddRawNativeData("PNG", delegate (Stream dstStream) {
                                                        copyBitmap.Save(dstStream, ImageFormat.Png);
                                                    });
                                                }
                                                renderer.Parallelize<ColorBgra>(TilingStrategy.Tiles, 7, WorkItemQueuePriority.Normal).Render<ColorBgra>(surface2);
                                                using (Bitmap bitmap = surface2.CreateAliasedBitmap(false))
                                                {
                                                    transaction.AddDibV5(bitmap);
                                                }
                                            }
                                            goto Label_0292;
                                        }
                                    }
                                }
                                catch (Exception exception)
                                {
                                    if (num5 == 0)
                                    {
                                        flag = false;
                                        ExceptionDialog.ShowErrorDialog(this.documentWorkspace, PdnResources.GetString("CopyAction.Error.TransferToClipboard"), exception.ToString());
                                    }
                                    else
                                    {
                                        CleanupManager.RequestCleanup();
                                        CleanupManager.WaitForPendingCleanup(50);
                                        Thread.Sleep(50);
                                    }
                                }
                                continue;
                            }
                            finally
                            {
                                num5--;
                            }
                        }
                    }
                }
            }
            catch (OutOfMemoryException exception2)
            {
                flag = false;
                ExceptionDialog.ShowErrorDialog(this.documentWorkspace, PdnResources.GetString("CopyAction.Error.OutOfMemory"), exception2.ToString());
            }
            catch (Exception exception3)
            {
                flag = false;
                ExceptionDialog.ShowErrorDialog(this.documentWorkspace, PdnResources.GetString("CopyAction.Error.Generic"), exception3.ToString());
            }
        Label_0292:
            CleanupManager.RequestCleanup();
            return flag;
        }

        protected virtual bool QueryCanPerformAction()
        {
            RectDouble bounds = this.documentWorkspace.Selection.GetCachedClippingMask().Bounds;
            return ((bounds.Width >= 1.0) && (bounds.Height >= 1.0));
        }

        protected PaintDotNet.Controls.DocumentWorkspace DocumentWorkspace =>
            this.documentWorkspace;
    }
}

