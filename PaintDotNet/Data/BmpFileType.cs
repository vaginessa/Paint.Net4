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

    internal sealed class BmpFileType : InternalFileType
    {
        public BmpFileType() : base("BMP", FileTypeFlags.None | FileTypeFlags.SupportsLoading | FileTypeFlags.SupportsSaving, textArray1)
        {
            string[] textArray1 = new string[] { ".bmp" };
        }

        internal override HashSet<InternalFileType.SavableBitDepths> CreateAllowedBitDepthListFromToken(PropertyBasedSaveConfigToken token)
        {
            BmpBitDepthUIChoices choices = (BmpBitDepthUIChoices) token.GetProperty<StaticListChoiceProperty>(PropertyNames.BitDepth).Value;
            HashSet<InternalFileType.SavableBitDepths> set = new HashSet<InternalFileType.SavableBitDepths>();
            switch (choices)
            {
                case BmpBitDepthUIChoices.AutoDetect:
                    set.Add(InternalFileType.SavableBitDepths.Rgb24);
                    set.Add(InternalFileType.SavableBitDepths.Rgb8);
                    return set;

                case BmpBitDepthUIChoices.Bpp24:
                    set.Add(InternalFileType.SavableBitDepths.Rgb24);
                    return set;

                case BmpBitDepthUIChoices.Bpp8:
                    set.Add(InternalFileType.SavableBitDepths.Rgb8);
                    return set;
            }
            throw ExceptionUtil.InvalidEnumArgumentException<BmpBitDepthUIChoices>(choices, "bitDepth");
        }

        internal override int GetDitherLevelFromToken(PropertyBasedSaveConfigToken token) => 
            token.GetProperty<Int32Property>(PropertyNames.DitherLevel).Value;

        internal override int GetThresholdFromToken(PropertyBasedSaveConfigToken token) => 
            0;

        public override ControlInfo OnCreateSaveConfigUI(PropertyCollection props)
        {
            ControlInfo info = PropertyBasedFileType.CreateDefaultSaveConfigUI(props);
            info.SetPropertyControlValue(PropertyNames.BitDepth, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("BmpFileType.ConfigUI.BitDepth.DisplayName"));
            PropertyControlInfo info2 = info.FindControlForPropertyName(PropertyNames.BitDepth);
            info2.SetValueDisplayName(BmpBitDepthUIChoices.AutoDetect, PdnResources.GetString("BmpFileType.ConfigUI.BitDepth.AutoDetect.DisplayName"));
            info2.SetValueDisplayName(BmpBitDepthUIChoices.Bpp24, PdnResources.GetString("BmpFileType.ConfigUI.BitDepth.Bpp24.DisplayName"));
            info2.SetValueDisplayName(BmpBitDepthUIChoices.Bpp8, PdnResources.GetString("BmpFileType.ConfigUI.BitDepth.Bpp8.DisplayName"));
            info.SetPropertyControlType(PropertyNames.BitDepth, PropertyControlType.RadioButton);
            info.SetPropertyControlValue(PropertyNames.DitherLevel, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("BmpFileType.ConfigUI.DitherLevel.DisplayName"));
            return info;
        }

        public override PropertyCollection OnCreateSavePropertyCollection()
        {
            List<Property> properties = new List<Property> {
                StaticListChoiceProperty.CreateForEnum<BmpBitDepthUIChoices>(PropertyNames.BitDepth, BmpBitDepthUIChoices.AutoDetect, false),
                new Int32Property(PropertyNames.DitherLevel, 7, 0, 8)
            };
            return new PropertyCollection(properties, new List<PropertyCollectionRule> { new ReadOnlyBoundToValueRule<object, StaticListChoiceProperty>(PropertyNames.DitherLevel, PropertyNames.BitDepth, BmpBitDepthUIChoices.Bpp8, true) });
        }

        internal override void OnFinalSave(Document input, Stream output, Surface scratchSurface, int ditherLevel, InternalFileType.SavableBitDepths bitDepth, PropertyBasedSaveConfigToken token, ProgressEventHandler progressCallback)
        {
            if (bitDepth == InternalFileType.SavableBitDepths.Rgb24)
            {
                base.SquishSurfaceTo24Bpp(scratchSurface);
                ImageCodecInfo imageCodecInfo = GdiPlusFileType.GetImageCodecInfo(ImageFormat.Bmp);
                EncoderParameters encoderParams = new EncoderParameters(1);
                EncoderParameter parameter = new EncoderParameter(Encoder.ColorDepth, 0x18);
                encoderParams.Param[0] = parameter;
                using (Bitmap bitmap = base.CreateAliased24BppBitmap(scratchSurface))
                {
                    GdiPlusFileType.LoadProperties(bitmap, input);
                    bitmap.Save(output, imageCodecInfo, encoderParams);
                    return;
                }
            }
            if (bitDepth == InternalFileType.SavableBitDepths.Rgb8)
            {
                using (Bitmap bitmap2 = base.Quantize(scratchSurface, ditherLevel, 0x100, false, progressCallback))
                {
                    ImageCodecInfo encoder = GdiPlusFileType.GetImageCodecInfo(ImageFormat.Bmp);
                    EncoderParameters parameters2 = new EncoderParameters(1);
                    EncoderParameter parameter2 = new EncoderParameter(Encoder.ColorDepth, 8);
                    parameters2.Param[0] = parameter2;
                    GdiPlusFileType.LoadProperties(bitmap2, input);
                    bitmap2.Save(output, encoder, parameters2);
                    return;
                }
            }
            throw ExceptionUtil.InvalidEnumArgumentException<InternalFileType.SavableBitDepths>(bitDepth, "bitDepth");
        }

        protected override Document OnLoad(Stream input)
        {
            if (input.Length == 0)
            {
                Document document = new Document(800, 600);
                Layer layer = Layer.CreateBackgroundLayer(document.Width, document.Height);
                document.Layers.Add(layer);
                return document;
            }
            using (Image image = Image.FromStream(input, false, true))
            {
                return Document.FromGdipImage(image, false);
            }
        }

        public enum BmpBitDepthUIChoices
        {
            AutoDetect,
            Bpp24,
            Bpp8
        }

        public enum PropertyNames
        {
            BitDepth,
            DitherLevel
        }
    }
}

