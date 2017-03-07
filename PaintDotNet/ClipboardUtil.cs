namespace PaintDotNet
{
    using PaintDotNet.Rendering;
    using PaintDotNet.Runtime;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.IO;
    using System.Windows.Forms;

    internal static class ClipboardUtil
    {
        private static readonly string[] fileDropImageExtensions = new string[] { ".bmp", ".png", ".jpg", ".jpe", ".jpeg", ".jfif", ".gif" };

        public static MaskedSurface GetClipboardImage(IWin32Window currentWindow, IPdnDataObject clipData)
        {
            try
            {
                return GetClipboardImageImpl(currentWindow, clipData);
            }
            finally
            {
            }
        }

        private static unsafe Surface GetClipboardImageAsSurface(IWin32Window currentWindow, IPdnDataObject clipData)
        {
            Image image = null;
            Surface surface = null;
            if (((image == null) && (surface == null)) && clipData.GetDataPresent(PdnDataObjectFormats.FileDrop))
            {
                try
                {
                    string[] strArray = null;
                    using (PaintDotNet.SystemLayer.Clipboard.Transaction transaction = PaintDotNet.SystemLayer.Clipboard.Open(currentWindow))
                    {
                        strArray = transaction.TryGetFileDropData();
                    }
                    if ((strArray != null) && (strArray.Length == 1))
                    {
                        string fileName = strArray[0];
                        if (IsImageFileName(fileName) && File.Exists(fileName))
                        {
                            image = Image.FromFile(fileName);
                            surface = Surface.CopyFromGdipImage(image, false);
                            image.Dispose();
                            image = null;
                        }
                    }
                }
                catch (OutOfMemoryException)
                {
                    throw;
                }
                catch (Exception)
                {
                }
            }
            if (((image == null) && (surface == null)) && clipData.GetDataPresent(PdnDataObjectFormats.Dib, true))
            {
                try
                {
                    using (PaintDotNet.SystemLayer.Clipboard.Transaction transaction2 = PaintDotNet.SystemLayer.Clipboard.Open(currentWindow))
                    {
                        bool flag = transaction2.TryGetRawNativeData(8, delegate (UnsafeBufferLock buffer) {
                            Size size;
                            byte* pBitmapInfo = (byte*) buffer.Address;
                            int ncbBitmapInfo = (int) buffer.Size;
                            if (PdnGraphics.TryGetBitmapInfoSize(pBitmapInfo, ncbBitmapInfo, out size))
                            {
                                surface = new Surface(size.Width, size.Height);
                                bool flag = false;
                                try
                                {
                                    using (Bitmap bitmap = surface.CreateAliasedBitmap(true))
                                    {
                                        flag = PdnGraphics.TryCopyFromBitmapInfo(bitmap, pBitmapInfo, ncbBitmapInfo);
                                    }
                                    surface.DetectAndFixDishonestAlpha();
                                }
                                finally
                                {
                                    if ((surface != null) && !flag)
                                    {
                                        surface.Dispose();
                                        surface = null;
                                    }
                                }
                            }
                        });
                    }
                }
                catch (OutOfMemoryException)
                {
                    throw;
                }
                catch (Exception)
                {
                }
            }
            if (((image == null) && (surface == null)) && (clipData.GetDataPresent(PdnDataObjectFormats.Bitmap, true) || clipData.GetDataPresent(PdnDataObjectFormats.EnhancedMetafile, true)))
            {
                try
                {
                    image = clipData.GetData(PdnDataObjectFormats.Bitmap, true) as Image;
                }
                catch (OutOfMemoryException)
                {
                    throw;
                }
                catch (Exception)
                {
                }
                if (image == null)
                {
                    try
                    {
                        using (PaintDotNet.SystemLayer.Clipboard.Transaction transaction3 = PaintDotNet.SystemLayer.Clipboard.Open(currentWindow))
                        {
                            image = transaction3.TryGetEmf();
                            Image image1 = image;
                        }
                    }
                    catch (OutOfMemoryException)
                    {
                        throw;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            if (((image == null) && (surface == null)) && clipData.GetDataPresent("PNG", false))
            {
                try
                {
                    bool flag2 = false;
                    using (PaintDotNet.SystemLayer.Clipboard.Transaction transaction4 = PaintDotNet.SystemLayer.Clipboard.Open(currentWindow))
                    {
                        uint formatID = PaintDotNet.SystemLayer.Clipboard.RegisterFormat("PNG");
                        flag2 = transaction4.TryGetRawNativeData(formatID, delegate (Stream stream) {
                            image = Image.FromStream(stream, false, true);
                        });
                    }
                    if (flag2 && (image != null))
                    {
                        surface = Surface.CopyFromGdipImage(image, false);
                        DisposableUtil.Free<Image>(ref image);
                    }
                }
                catch (OutOfMemoryException)
                {
                    throw;
                }
                catch (Exception)
                {
                }
            }
            if ((surface != null) && (image != null))
            {
                throw new InternalErrorException("both surface and image are non-null");
            }
            if ((surface == null) && (image != null))
            {
                surface = Surface.CopyFromGdipImage(image, true);
            }
            return surface;
        }

        private static MaskedSurface GetClipboardImageImpl(IWin32Window currentWindow, IPdnDataObject clipData)
        {
            CleanupManager.RequestCleanup();
            using (PaintDotNet.SystemLayer.Clipboard.Transaction transaction = PaintDotNet.SystemLayer.Clipboard.Open(currentWindow))
            {
                if (transaction.IsManagedDataPresent(typeof(MaskedSurface)))
                {
                    try
                    {
                        MaskedSurface surface2 = transaction.TryGetManagedData(typeof(MaskedSurface)) as MaskedSurface;
                        if ((surface2 != null) && !surface2.IsDisposed)
                        {
                            return surface2;
                        }
                        if (surface2 != null)
                        {
                            bool isDisposed = surface2.IsDisposed;
                        }
                    }
                    catch (OutOfMemoryException)
                    {
                        throw;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            Surface clipboardImageAsSurface = GetClipboardImageAsSurface(currentWindow, clipData);
            if (clipboardImageAsSurface != null)
            {
                return new MaskedSurface(ref clipboardImageAsSurface, true);
            }
            return null;
        }

        public static SizeInt32? GetClipboardImageSize(IWin32Window currentWindow, IPdnDataObject clipData)
        {
            try
            {
                return GetClipboardImageSizeImpl(currentWindow, clipData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static SizeInt32? GetClipboardImageSizeImpl(IWin32Window currentWindow, IPdnDataObject clipData)
        {
            CleanupManager.RequestCleanup();
            using (MaskedSurface surface = GetClipboardImage(currentWindow, clipData))
            {
                if (surface != null)
                {
                    return new SizeInt32?(surface.GetCachedGeometryMaskScansBounds().Size);
                }
            }
            return null;
        }

        public static bool IsClipboardImageMaybeAvailable(IWin32Window currentWindow, IPdnDataObject clipData)
        {
            try
            {
                bool flag3;
                bool flag = false;
                using (PaintDotNet.SystemLayer.Clipboard.Transaction transaction = PaintDotNet.SystemLayer.Clipboard.Open(currentWindow))
                {
                    if (transaction.IsManagedDataPresent(typeof(MaskedSurface)))
                    {
                        flag = clipData.GetDataPresent(typeof(MaskedSurface));
                    }
                }
                if (!clipData.GetDataPresent(PdnDataObjectFormats.FileDrop))
                {
                    flag3 = false;
                }
                else
                {
                    string[] data = clipData.GetData(PdnDataObjectFormats.FileDrop) as string[];
                    if (((data != null) && (data.Length == 1)) && (IsImageFileName(data[0]) && File.Exists(data[0])))
                    {
                        flag3 = true;
                    }
                    else
                    {
                        flag3 = false;
                    }
                }
                bool dataPresent = clipData.GetDataPresent(PdnDataObjectFormats.Bitmap, true);
                bool flag5 = clipData.GetDataPresent(PdnDataObjectFormats.Dib, true);
                bool flag6 = clipData.GetDataPresent(PdnDataObjectFormats.EnhancedMetafile, true);
                bool flag7 = clipData.GetDataPresent("PNG", false);
                return (((((flag | flag3) | dataPresent) | flag5) | flag6) | flag7);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool IsImageFileName(string fileName)
        {
            try
            {
                foreach (string str in fileDropImageExtensions)
                {
                    if (Path.HasExtension(str))
                    {
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }
    }
}

