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

    internal sealed class GifFileType : InternalFileType
    {
        public GifFileType() : base("GIF", FileTypeFlags.None | FileTypeFlags.SavesWithProgress | FileTypeFlags.SupportsLoading | FileTypeFlags.SupportsSaving, textArray1)
        {
            string[] textArray1 = new string[] { ".gif" };
        }

        internal override HashSet<InternalFileType.SavableBitDepths> CreateAllowedBitDepthListFromToken(PropertyBasedSaveConfigToken token) => 
            new HashSet<InternalFileType.SavableBitDepths> { 
                InternalFileType.SavableBitDepths.Rgb8,
                InternalFileType.SavableBitDepths.Rgba8
            };

        internal override int GetDitherLevelFromToken(PropertyBasedSaveConfigToken token) => 
            token.GetProperty<Int32Property>(PropertyNames.DitherLevel).Value;

        internal override int GetThresholdFromToken(PropertyBasedSaveConfigToken token) => 
            token.GetProperty<Int32Property>(PropertyNames.Threshold).Value;

        public override ControlInfo OnCreateSaveConfigUI(PropertyCollection props)
        {
            ControlInfo info = PropertyBasedFileType.CreateDefaultSaveConfigUI(props);
            info.SetPropertyControlValue(PropertyNames.DitherLevel, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("GifFileType.ConfigUI.DitherLevel.DisplayName"));
            info.SetPropertyControlValue(PropertyNames.Threshold, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("GifFileType.ConfigUI.Threshold.DisplayName"));
            info.SetPropertyControlValue(PropertyNames.Threshold, ControlInfoPropertyNames.Description, PdnResources.GetString("GifFileType.ConfigUI.Threshold.Description"));
            return info;
        }

        public override PropertyCollection OnCreateSavePropertyCollection() => 
            new PropertyCollection(new List<Property> { 
                new Int32Property(PropertyNames.DitherLevel, 7, 0, 8),
                new Int32Property(PropertyNames.Threshold, 0x80, 0, 0xff)
            });

        internal override void OnFinalSave(Document input, Stream output, Surface scratchSurface, int ditherLevel, InternalFileType.SavableBitDepths bitDepth, PropertyBasedSaveConfigToken token, ProgressEventHandler progressCallback)
        {
            bool flag;
            if (bitDepth != InternalFileType.SavableBitDepths.Rgb8)
            {
                if (bitDepth != InternalFileType.SavableBitDepths.Rgba8)
                {
                    throw ExceptionUtil.InvalidEnumArgumentException<InternalFileType.SavableBitDepths>(bitDepth, "bitDepth");
                }
            }
            else
            {
                flag = false;
                goto Label_0021;
            }
            flag = true;
        Label_0021:
            using (Bitmap bitmap = base.Quantize(scratchSurface, ditherLevel, 0x100, flag, progressCallback))
            {
                bitmap.Save(output, ImageFormat.Gif);
            }
        }

        protected override Document OnLoad(Stream input)
        {
            using (Image image = Image.FromStream(input, false, true))
            {
                return Document.FromGdipImage(image, false);
            }
        }

        public enum PropertyNames
        {
            Threshold,
            DitherLevel
        }
    }
}

