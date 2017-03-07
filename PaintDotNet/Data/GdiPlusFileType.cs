namespace PaintDotNet.Data
{
    using PaintDotNet;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Runtime.CompilerServices;

    internal class GdiPlusFileType : FileType
    {
        private System.Drawing.Imaging.ImageFormat imageFormat;

        public GdiPlusFileType(string name, System.Drawing.Imaging.ImageFormat imageFormat, bool supportsLayers, string[] extensions) : this(name, imageFormat, supportsLayers, extensions, false)
        {
        }

        public GdiPlusFileType(string name, System.Drawing.Imaging.ImageFormat imageFormat, bool supportsLayers, string[] extensions, bool savesWithProgress) : base(name, (((supportsLayers ? (FileTypeFlags.None | FileTypeFlags.SupportsLayers) : FileTypeFlags.None) | (FileTypeFlags.None | FileTypeFlags.SupportsLoading)) | (FileTypeFlags.None | FileTypeFlags.SupportsSaving)) | (savesWithProgress ? (FileTypeFlags.None | FileTypeFlags.SavesWithProgress) : FileTypeFlags.None), extensions)
        {
            this.imageFormat = imageFormat;
        }

        public static ImageCodecInfo GetImageCodecInfo(System.Drawing.Imaging.ImageFormat format)
        {
            foreach (ImageCodecInfo info in ImageCodecInfo.GetImageEncoders())
            {
                if (info.FormatID == format.Guid)
                {
                    return info;
                }
            }
            return null;
        }

        public static void LoadProperties(Image dstImage, Document srcDoc)
        {
            LoadProperties(dstImage, srcDoc, _ => true);
        }

        public static void LoadProperties(Image dstImage, Document srcDoc, Func<System.Drawing.Imaging.PropertyItem, bool> selectorFn)
        {
            Bitmap bitmap = dstImage as Bitmap;
            if (bitmap != null)
            {
                float dpuX;
                float dpuY;
                switch (srcDoc.DpuUnit)
                {
                    case MeasurementUnit.Inch:
                        dpuX = (float) srcDoc.DpuX;
                        dpuY = (float) srcDoc.DpuY;
                        break;

                    case MeasurementUnit.Centimeter:
                        dpuX = (float) Document.DotsPerCmToDotsPerInch(srcDoc.DpuX);
                        dpuY = (float) Document.DotsPerCmToDotsPerInch(srcDoc.DpuY);
                        break;

                    default:
                        dpuX = 1f;
                        dpuY = 1f;
                        break;
                }
                try
                {
                    bitmap.SetResolution(dpuX, dpuY);
                }
                catch (Exception)
                {
                }
            }
            Metadata metadata = srcDoc.Metadata;
            foreach (string str in metadata.GetKeys("$exif"))
            {
                System.Drawing.Imaging.PropertyItem arg = PdnGraphics.DeserializePropertyItem(metadata.GetValue("$exif", str));
                if (selectorFn(arg))
                {
                    try
                    {
                        dstImage.SetPropertyItem(arg);
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }
        }

        protected override Document OnLoad(Stream input)
        {
            using (Image image = Image.FromStream(input, false, true))
            {
                return Document.FromGdipImage(image, false);
            }
        }

        protected override void OnSave(Document input, Stream output, SaveConfigToken token, Surface scratchSurface, ProgressEventHandler callback)
        {
            Save(input, output, scratchSurface, this.ImageFormat, callback);
        }

        public static void Save(Document input, Stream output, Surface scratchSurface, System.Drawing.Imaging.ImageFormat format, ProgressEventHandler callback)
        {
            scratchSurface.Clear(ColorBgra.FromBgra(0, 0, 0, 0));
            using (RenderArgs args = new RenderArgs(scratchSurface))
            {
                input.Render(args, true);
            }
            using (Bitmap bitmap = scratchSurface.CreateAliasedBitmap())
            {
                LoadProperties(bitmap, input);
                bitmap.Save(output, format);
            }
        }

        public System.Drawing.Imaging.ImageFormat ImageFormat =>
            this.imageFormat;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly GdiPlusFileType.<>c <>9 = new GdiPlusFileType.<>c();
            public static Func<System.Drawing.Imaging.PropertyItem, bool> <>9__5_0;

            internal bool <LoadProperties>b__5_0(System.Drawing.Imaging.PropertyItem _) => 
                true;
        }
    }
}

