namespace PaintDotNet.Data
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.IndirectUI;
    using PaintDotNet.PropertySystem;
    using PaintDotNet.Resources;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Runtime.CompilerServices;

    internal sealed class PngFileType : InternalFileType
    {
        public PngFileType() : base("PNG", FileTypeFlags.None | FileTypeFlags.SupportsLoading | FileTypeFlags.SupportsSaving, textArray1)
        {
            string[] textArray1 = new string[] { ".png" };
        }

        internal override HashSet<InternalFileType.SavableBitDepths> CreateAllowedBitDepthListFromToken(PropertyBasedSaveConfigToken token)
        {
            PngBitDepthUIChoices choices = (PngBitDepthUIChoices) token.GetProperty<StaticListChoiceProperty>(PropertyNames.BitDepth).Value;
            HashSet<InternalFileType.SavableBitDepths> collection = new HashSet<InternalFileType.SavableBitDepths>();
            switch (choices)
            {
                case PngBitDepthUIChoices.AutoDetect:
                    collection.AddRange<InternalFileType.SavableBitDepths>(new InternalFileType.SavableBitDepths[] { InternalFileType.SavableBitDepths.Rgb24 });
                    return collection;

                case PngBitDepthUIChoices.Bpp32:
                    collection.AddRange<InternalFileType.SavableBitDepths>(new InternalFileType.SavableBitDepths[1]);
                    return collection;

                case PngBitDepthUIChoices.Bpp24:
                {
                    InternalFileType.SavableBitDepths[] items = new InternalFileType.SavableBitDepths[] { InternalFileType.SavableBitDepths.Rgb24 };
                    collection.AddRange<InternalFileType.SavableBitDepths>(items);
                    return collection;
                }
                case PngBitDepthUIChoices.Bpp8:
                {
                    InternalFileType.SavableBitDepths[] depthsArray2 = new InternalFileType.SavableBitDepths[] { InternalFileType.SavableBitDepths.Rgb8, InternalFileType.SavableBitDepths.Rgba8 };
                    collection.AddRange<InternalFileType.SavableBitDepths>(depthsArray2);
                    return collection;
                }
            }
            throw ExceptionUtil.InvalidEnumArgumentException<PngBitDepthUIChoices>(choices, "bitDepthFromToken");
        }

        internal override int GetDitherLevelFromToken(PropertyBasedSaveConfigToken token) => 
            token.GetProperty<Int32Property>(PropertyNames.DitherLevel).Value;

        internal override int GetThresholdFromToken(PropertyBasedSaveConfigToken token) => 
            token.GetProperty<Int32Property>(PropertyNames.Threshold).Value;

        protected override bool IsReflexive(PropertyBasedSaveConfigToken token)
        {
            PngBitDepthUIChoices choices = (PngBitDepthUIChoices) token.GetProperty<StaticListChoiceProperty>(PropertyNames.BitDepth).Value;
            return (choices == PngBitDepthUIChoices.Bpp32);
        }

        public override ControlInfo OnCreateSaveConfigUI(PropertyCollection props)
        {
            ControlInfo info = PropertyBasedFileType.CreateDefaultSaveConfigUI(props);
            info.SetPropertyControlValue(PropertyNames.BitDepth, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("PngFileType.ConfigUI.BitDepth.DisplayName"));
            PropertyControlInfo info2 = info.FindControlForPropertyName(PropertyNames.BitDepth);
            info2.SetValueDisplayName(PngBitDepthUIChoices.AutoDetect, PdnResources.GetString("PngFileType.ConfigUI.BitDepth.AutoDetect.DisplayName"));
            info2.SetValueDisplayName(PngBitDepthUIChoices.Bpp32, PdnResources.GetString("PngFileType.ConfigUI.BitDepth.Bpp32.DisplayName"));
            info2.SetValueDisplayName(PngBitDepthUIChoices.Bpp24, PdnResources.GetString("PngFileType.ConfigUI.BitDepth.Bpp24.DisplayName"));
            info2.SetValueDisplayName(PngBitDepthUIChoices.Bpp8, PdnResources.GetString("PngFileType.ConfigUI.BitDepth.Bpp8.DisplayName"));
            info.SetPropertyControlType(PropertyNames.BitDepth, PropertyControlType.RadioButton);
            info.SetPropertyControlValue(PropertyNames.DitherLevel, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("PngFileType.ConfigUI.DitherLevel.DisplayName"));
            info.SetPropertyControlValue(PropertyNames.Threshold, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("PngFileType.ConfigUI.Threshold.DisplayName"));
            info.SetPropertyControlValue(PropertyNames.Threshold, ControlInfoPropertyNames.Description, PdnResources.GetString("PngFileType.ConfigUI.Threshold.Description"));
            return info;
        }

        public override PropertyCollection OnCreateSavePropertyCollection()
        {
            List<Property> properties = new List<Property> {
                StaticListChoiceProperty.CreateForEnum<PngBitDepthUIChoices>(PropertyNames.BitDepth, PngBitDepthUIChoices.AutoDetect, false),
                new Int32Property(PropertyNames.DitherLevel, 7, 0, 8),
                new Int32Property(PropertyNames.Threshold, 0x80, 0, 0xff)
            };
            return new PropertyCollection(properties, new List<PropertyCollectionRule> { 
                new ReadOnlyBoundToValueRule<object, StaticListChoiceProperty>(PropertyNames.Threshold, PropertyNames.BitDepth, PngBitDepthUIChoices.Bpp8, true),
                new ReadOnlyBoundToValueRule<object, StaticListChoiceProperty>(PropertyNames.DitherLevel, PropertyNames.BitDepth, PngBitDepthUIChoices.Bpp8, true)
            });
        }

        internal override void OnFinalSave(Document input, Stream output, Surface scratchSurface, int ditherLevel, InternalFileType.SavableBitDepths bitDepth, PropertyBasedSaveConfigToken token, ProgressEventHandler progressCallback)
        {
            int num;
            Bitmap bitmap;
            Func<PropertyItem, bool> func;
            ImageCodecInfo imageCodecInfo = GdiPlusFileType.GetImageCodecInfo(ImageFormat.Png);
            EncoderParameters encoderParams = new EncoderParameters(1);
            if (bitDepth == InternalFileType.SavableBitDepths.Rgba32)
            {
                num = 0x20;
                bitmap = scratchSurface.CreateAliasedBitmap();
            }
            else if (bitDepth == InternalFileType.SavableBitDepths.Rgb24)
            {
                base.SquishSurfaceTo24Bpp(scratchSurface);
                num = 0x18;
                bitmap = base.CreateAliased24BppBitmap(scratchSurface);
            }
            else if (bitDepth == InternalFileType.SavableBitDepths.Rgb8)
            {
                num = 8;
                bitmap = base.Quantize(scratchSurface, ditherLevel, 0x100, false, progressCallback);
            }
            else
            {
                if (bitDepth != InternalFileType.SavableBitDepths.Rgba8)
                {
                    throw ExceptionUtil.InvalidEnumArgumentException<InternalFileType.SavableBitDepths>(bitDepth, "bitDepth");
                }
                num = 8;
                bitmap = base.Quantize(scratchSurface, ditherLevel, 0x100, true, progressCallback);
            }
            EncoderParameter parameter = new EncoderParameter(Encoder.ColorDepth, (long) num);
            encoderParams.Param[0] = parameter;
            if (num == 0x20)
            {
                func = pi => true;
            }
            else
            {
                int iccProfileDataID = 0x8773;
                func = pi => pi.Id != iccProfileDataID;
            }
            GdiPlusFileType.LoadProperties(bitmap, input, func);
            bitmap.Save(output, imageCodecInfo, encoderParams);
            bitmap.Dispose();
            bitmap = null;
        }

        protected override Document OnLoad(Stream input)
        {
            using (Image image = Image.FromStream(input, false, true))
            {
                return Document.FromGdipImage(image, false);
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly PngFileType.<>c <>9 = new PngFileType.<>c();
            public static Func<PropertyItem, bool> <>9__10_0;

            internal bool <OnFinalSave>b__10_0(PropertyItem pi) => 
                true;
        }

        public enum PngBitDepthUIChoices
        {
            AutoDetect,
            Bpp32,
            Bpp24,
            Bpp8
        }

        public enum PropertyNames
        {
            BitDepth,
            DitherLevel,
            Threshold
        }
    }
}

