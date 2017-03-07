namespace PaintDotNet.Data
{
    using PaintDotNet;
    using PaintDotNet.IndirectUI;
    using PaintDotNet.PropertySystem;
    using PaintDotNet.Resources;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;

    internal sealed class JpegFileType : PropertyBasedFileType
    {
        public JpegFileType() : base("JPEG", FileTypeFlags.None | FileTypeFlags.SupportsLoading | FileTypeFlags.SupportsSaving, textArray1)
        {
            string[] textArray1 = new string[] { ".jpg", ".jpeg", ".jpe", ".jfif" };
        }

        public override ControlInfo OnCreateSaveConfigUI(PropertyCollection props)
        {
            ControlInfo info = PropertyBasedFileType.CreateDefaultSaveConfigUI(props);
            info.SetPropertyControlValue(PropertyNames.Quality, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("JpegFileType.ConfigUI.Quality.DisplayName") ?? "??");
            return info;
        }

        public override PropertyCollection OnCreateSavePropertyCollection() => 
            new PropertyCollection(new List<Property> { new Int32Property(PropertyNames.Quality, 0x5f, 0, 100) });

        protected override Document OnLoad(Stream input)
        {
            using (Image image = Image.FromStream(input, false, true))
            {
                return Document.FromGdipImage(image, false);
            }
        }

        protected override void OnSaveT(Document input, Stream output, PropertyBasedSaveConfigToken token, Surface scratchSurface, ProgressEventHandler progressCallback)
        {
            int num = token.GetProperty<Int32Property>(PropertyNames.Quality).Value;
            ImageCodecInfo imageCodecInfo = GdiPlusFileType.GetImageCodecInfo(ImageFormat.Jpeg);
            EncoderParameters encoderParams = new EncoderParameters(1);
            EncoderParameter parameter = new EncoderParameter(Encoder.Quality, (long) num);
            encoderParams.Param[0] = parameter;
            scratchSurface.Clear(ColorBgra.White);
            using (RenderArgs args = new RenderArgs(scratchSurface))
            {
                input.Render(args, false);
            }
            using (Bitmap bitmap = scratchSurface.CreateAliasedBitmap())
            {
                GdiPlusFileType.LoadProperties(bitmap, input);
                bitmap.Save(output, imageCodecInfo, encoderParams);
            }
        }

        public enum PropertyNames
        {
            Quality
        }
    }
}

