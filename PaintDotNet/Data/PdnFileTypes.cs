namespace PaintDotNet.Data
{
    using PaintDotNet;
    using PaintDotNet.Data.Dds;
    using System;
    using System.Drawing.Imaging;

    internal sealed class PdnFileTypes : IFileTypeFactory
    {
        public static readonly BmpFileType Bmp = new BmpFileType();
        public static readonly DdsFileType Dds;
        private static FileType[] fileTypes;
        public static readonly GifFileType Gif = new GifFileType();
        public static readonly JpegFileType Jpeg = new JpegFileType();
        public static readonly PdnFileType Pdn;
        public static readonly PngFileType Png;
        public static readonly TgaFileType Tga;
        public static readonly GdiPlusFileType Tiff;

        static PdnFileTypes()
        {
            string[] extensions = new string[] { ".tif", ".tiff" };
            Tiff = new GdiPlusFileType("TIFF", ImageFormat.Tiff, false, extensions);
            Png = new PngFileType();
            Pdn = new PdnFileType();
            Tga = new TgaFileType();
            Dds = new DdsFileType();
            fileTypes = new FileType[] { Pdn, Bmp, Gif, Jpeg, Png, Tiff, Tga, Dds };
        }

        internal FileTypeCollection GetFileTypeCollection() => 
            new FileTypeCollection(fileTypes);

        public FileType[] GetFileTypeInstances() => 
            ((FileType[]) fileTypes.Clone());
    }
}

