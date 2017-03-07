namespace PaintDotNet.Data.Dds
{
    using PaintDotNet;
    using PaintDotNet.IndirectUI;
    using PaintDotNet.PropertySystem;
    using PaintDotNet.Resources;
    using System;
    using System.Collections.Generic;
    using System.IO;

    internal sealed class DdsFileType : PropertyBasedFileType
    {
        public DdsFileType() : base(PdnResources.GetString("DdsFileType.Name"), FileTypeFlags.None | FileTypeFlags.SavesWithProgress | FileTypeFlags.SupportsLoading | FileTypeFlags.SupportsSaving, textArray1)
        {
            string[] textArray1 = new string[] { ".dds" };
        }

        public override ControlInfo OnCreateSaveConfigUI(PropertyCollection props)
        {
            ControlInfo info = PropertyBasedFileType.CreateDefaultSaveConfigUI(props);
            info.SetPropertyControlValue(PropertyNames.FileFormat, ControlInfoPropertyNames.DisplayName, string.Empty);
            info.FindControlForPropertyName(PropertyNames.FileFormat).SetValueDisplayName(DdsFileFormat.DDS_FORMAT_DXT1, PdnResources.GetString("DdsFileType.SaveConfigWidget.FileFormatList.DXT1"));
            info.FindControlForPropertyName(PropertyNames.FileFormat).SetValueDisplayName(DdsFileFormat.DDS_FORMAT_DXT3, PdnResources.GetString("DdsFileType.SaveConfigWidget.FileFormatList.DXT3"));
            info.FindControlForPropertyName(PropertyNames.FileFormat).SetValueDisplayName(DdsFileFormat.DDS_FORMAT_DXT5, PdnResources.GetString("DdsFileType.SaveConfigWidget.FileFormatList.DXT5"));
            info.FindControlForPropertyName(PropertyNames.FileFormat).SetValueDisplayName(DdsFileFormat.DDS_FORMAT_A8R8G8B8, PdnResources.GetString("DdsFileType.SaveConfigWidget.FileFormatList.A8R8G8B8"));
            info.FindControlForPropertyName(PropertyNames.FileFormat).SetValueDisplayName(DdsFileFormat.DDS_FORMAT_X8R8G8B8, PdnResources.GetString("DdsFileType.SaveConfigWidget.FileFormatList.X8R8G8B8"));
            info.FindControlForPropertyName(PropertyNames.FileFormat).SetValueDisplayName(DdsFileFormat.DDS_FORMAT_A8B8G8R8, PdnResources.GetString("DdsFileType.SaveConfigWidget.FileFormatList.A8B8G8R8"));
            info.FindControlForPropertyName(PropertyNames.FileFormat).SetValueDisplayName(DdsFileFormat.DDS_FORMAT_X8B8G8R8, PdnResources.GetString("DdsFileType.SaveConfigWidget.FileFormatList.X8B8G8R8"));
            info.FindControlForPropertyName(PropertyNames.FileFormat).SetValueDisplayName(DdsFileFormat.DDS_FORMAT_A1R5G5B5, PdnResources.GetString("DdsFileType.SaveConfigWidget.FileFormatList.A1R5G5B5"));
            info.FindControlForPropertyName(PropertyNames.FileFormat).SetValueDisplayName(DdsFileFormat.DDS_FORMAT_A4R4G4B4, PdnResources.GetString("DdsFileType.SaveConfigWidget.FileFormatList.A4R4G4B4"));
            info.FindControlForPropertyName(PropertyNames.FileFormat).SetValueDisplayName(DdsFileFormat.DDS_FORMAT_R8G8B8, PdnResources.GetString("DdsFileType.SaveConfigWidget.FileFormatList.R8G8B8"));
            info.FindControlForPropertyName(PropertyNames.FileFormat).SetValueDisplayName(DdsFileFormat.DDS_FORMAT_R5G6B5, PdnResources.GetString("DdsFileType.SaveConfigWidget.FileFormatList.R5G6B5"));
            info.SetPropertyControlValue(PropertyNames.CompressorType, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("DdsFileType.SaveConfigWidget.CompressorTypeLabel.Text"));
            info.SetPropertyControlType(PropertyNames.CompressorType, PropertyControlType.RadioButton);
            info.FindControlForPropertyName(PropertyNames.CompressorType).SetValueDisplayName(DdsCompressorType.RangeFit, PdnResources.GetString("DdsFileType.SaveConfigWidget.RangeFit.Text"));
            info.FindControlForPropertyName(PropertyNames.CompressorType).SetValueDisplayName(DdsCompressorType.ClusterFit, PdnResources.GetString("DdsFileType.SaveConfigWidget.ClusterFit.Text"));
            info.FindControlForPropertyName(PropertyNames.CompressorType).SetValueDisplayName(DdsCompressorType.IterativeFit, PdnResources.GetString("DdsFileType.SaveConfigWidget.IterativeFit.Text"));
            info.SetPropertyControlValue(PropertyNames.ErrorMetric, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("DdsFileType.SaveConfigWidget.ErrorMetricLabel.Text"));
            info.SetPropertyControlType(PropertyNames.ErrorMetric, PropertyControlType.RadioButton);
            info.FindControlForPropertyName(PropertyNames.ErrorMetric).SetValueDisplayName(DdsErrorMetric.Perceptual, PdnResources.GetString("DdsFileType.SaveConfigWidget.Perceptual.Text"));
            info.FindControlForPropertyName(PropertyNames.ErrorMetric).SetValueDisplayName(DdsErrorMetric.Uniform, PdnResources.GetString("DdsFileType.SaveConfigWidget.Uniform.Text"));
            info.SetPropertyControlValue(PropertyNames.GenerateMipMaps, ControlInfoPropertyNames.DisplayName, string.Empty);
            info.SetPropertyControlValue(PropertyNames.GenerateMipMaps, ControlInfoPropertyNames.Description, PdnResources.GetString("DdsFileType.SaveConfigWidget.GenerateMipMaps.Text"));
            info.SetPropertyControlValue(PropertyNames.WeightColorByAlpha, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("DdsFileType.SaveConfigWidget.AdditionalOptions.Text"));
            info.SetPropertyControlValue(PropertyNames.WeightColorByAlpha, ControlInfoPropertyNames.Description, PdnResources.GetString("DdsFileType.SaveConfigWidget.WeightColourByAlpha"));
            info.SetPropertyControlValue(PropertyNames.MipMapResamplingAlgorithm, ControlInfoPropertyNames.DisplayName, string.Empty);
            info.FindControlForPropertyName(PropertyNames.MipMapResamplingAlgorithm).SetValueDisplayName(ResamplingAlgorithm.Fant, PdnResources.GetString("DdsFileType.SaveConfigWidget.MipMapResamplingAlgorithm.Fant"));
            info.FindControlForPropertyName(PropertyNames.MipMapResamplingAlgorithm).SetValueDisplayName(ResamplingAlgorithm.SuperSampling, PdnResources.GetString("DdsFileType.SaveConfigWidget.MipMapResamplingAlgorithm.SuperSampling"));
            info.FindControlForPropertyName(PropertyNames.MipMapResamplingAlgorithm).SetValueDisplayName(ResamplingAlgorithm.Bicubic, PdnResources.GetString("DdsFileType.SaveConfigWidget.MipMapResamplingAlgorithm.Bicubic"));
            info.FindControlForPropertyName(PropertyNames.MipMapResamplingAlgorithm).SetValueDisplayName(ResamplingAlgorithm.Bilinear, PdnResources.GetString("DdsFileType.SaveConfigWidget.MipMapResamplingAlgorithm.Bilinear"));
            info.FindControlForPropertyName(PropertyNames.MipMapResamplingAlgorithm).SetValueDisplayName(ResamplingAlgorithm.NearestNeighbor, PdnResources.GetString("DdsFileType.SaveConfigWidget.MipMapResamplingAlgorithm.NearestNeighbor"));
            return info;
        }

        public override PropertyCollection OnCreateSavePropertyCollection()
        {
            List<Property> properties = new List<Property> {
                StaticListChoiceProperty.CreateForEnum<DdsFileFormat>(PropertyNames.FileFormat, DdsFileFormat.DDS_FORMAT_DXT1, false)
            };
            object[] valueChoices = new object[] { DdsCompressorType.RangeFit, DdsCompressorType.ClusterFit, DdsCompressorType.IterativeFit };
            properties.Add(new StaticListChoiceProperty(PropertyNames.CompressorType, valueChoices, 1));
            object[] objArray2 = new object[] { DdsErrorMetric.Uniform, DdsErrorMetric.Perceptual };
            properties.Add(new StaticListChoiceProperty(PropertyNames.ErrorMetric, objArray2, 1));
            properties.Add(new BooleanProperty(PropertyNames.WeightColorByAlpha, false, true));
            properties.Add(new BooleanProperty(PropertyNames.GenerateMipMaps, false));
            object[] objArray3 = new object[] { ResamplingAlgorithm.Fant, ResamplingAlgorithm.SuperSampling, ResamplingAlgorithm.Bilinear, ResamplingAlgorithm.Bicubic, ResamplingAlgorithm.NearestNeighbor };
            properties.Add(new StaticListChoiceProperty(PropertyNames.MipMapResamplingAlgorithm, objArray3, 0));
            List<PropertyCollectionRule> rules = new List<PropertyCollectionRule>();
            object[] valuesForReadOnly = new object[] { DdsFileFormat.DDS_FORMAT_A8B8G8R8, DdsFileFormat.DDS_FORMAT_A8R8G8B8, DdsFileFormat.DDS_FORMAT_A4R4G4B4, DdsFileFormat.DDS_FORMAT_A1R5G5B5, DdsFileFormat.DDS_FORMAT_R5G6B5, DdsFileFormat.DDS_FORMAT_R8G8B8, DdsFileFormat.DDS_FORMAT_X8B8G8R8, DdsFileFormat.DDS_FORMAT_X8R8G8B8 };
            rules.Add(new ReadOnlyBoundToValueRule<object, StaticListChoiceProperty>(PropertyNames.ErrorMetric, PropertyNames.FileFormat, valuesForReadOnly, false));
            object[] objArray5 = new object[] { DdsFileFormat.DDS_FORMAT_A8B8G8R8, DdsFileFormat.DDS_FORMAT_A8R8G8B8, DdsFileFormat.DDS_FORMAT_A4R4G4B4, DdsFileFormat.DDS_FORMAT_A1R5G5B5, DdsFileFormat.DDS_FORMAT_R5G6B5, DdsFileFormat.DDS_FORMAT_R8G8B8, DdsFileFormat.DDS_FORMAT_X8B8G8R8, DdsFileFormat.DDS_FORMAT_X8R8G8B8 };
            rules.Add(new ReadOnlyBoundToValueRule<object, StaticListChoiceProperty>(PropertyNames.CompressorType, PropertyNames.FileFormat, objArray5, false));
            TupleStruct<object, object>[] sourcePropertyNameValuePairs = new TupleStruct<object, object>[] { Tuple.Create<object, object>(PropertyNames.FileFormat, DdsFileFormat.DDS_FORMAT_A8B8G8R8), Tuple.Create<object, object>(PropertyNames.FileFormat, DdsFileFormat.DDS_FORMAT_A8R8G8B8), Tuple.Create<object, object>(PropertyNames.FileFormat, DdsFileFormat.DDS_FORMAT_A4R4G4B4), Tuple.Create<object, object>(PropertyNames.FileFormat, DdsFileFormat.DDS_FORMAT_A1R5G5B5), Tuple.Create<object, object>(PropertyNames.FileFormat, DdsFileFormat.DDS_FORMAT_R5G6B5), Tuple.Create<object, object>(PropertyNames.FileFormat, DdsFileFormat.DDS_FORMAT_R8G8B8), Tuple.Create<object, object>(PropertyNames.FileFormat, DdsFileFormat.DDS_FORMAT_X8B8G8R8), Tuple.Create<object, object>(PropertyNames.FileFormat, DdsFileFormat.DDS_FORMAT_X8R8G8B8), Tuple.Create<object, object>(PropertyNames.CompressorType, DdsCompressorType.RangeFit) };
            rules.Add(new ReadOnlyBoundToNameValuesRule(PropertyNames.WeightColorByAlpha, false, sourcePropertyNameValuePairs));
            rules.Add(new ReadOnlyBoundToBooleanRule(PropertyNames.MipMapResamplingAlgorithm, PropertyNames.GenerateMipMaps, true));
            return new PropertyCollection(properties, rules);
        }

        protected override Document OnLoad(Stream input)
        {
            DdsFile file = new DdsFile();
            file.Load(input);
            BitmapLayer layer = Layer.CreateBackgroundLayer(file.GetWidth(), file.GetHeight());
            Surface surface = layer.Surface;
            ColorBgra bgra = new ColorBgra();
            byte[] pixelData = file.GetPixelData();
            for (int i = 0; i < file.GetHeight(); i++)
            {
                for (int j = 0; j < file.GetWidth(); j++)
                {
                    int index = ((i * file.GetWidth()) * 4) + (j * 4);
                    bgra.R = pixelData[index];
                    bgra.G = pixelData[index + 1];
                    bgra.B = pixelData[index + 2];
                    bgra.A = pixelData[index + 3];
                    surface[j, i] = bgra;
                }
            }
            Document document = new Document(surface.Width, surface.Height);
            document.Layers.Add(layer);
            return document;
        }

        protected override void OnSaveT(Document input, Stream output, PropertyBasedSaveConfigToken token, Surface scratchSurface, ProgressEventHandler callback)
        {
            scratchSurface.Clear(ColorBgra.Zero);
            using (RenderArgs args = new RenderArgs(scratchSurface))
            {
                input.Render(args, true);
            }
            DdsFileFormat fileFormat = (DdsFileFormat) token.GetProperty<StaticListChoiceProperty>(PropertyNames.FileFormat).Value;
            DdsCompressorType compressorType = (DdsCompressorType) token.GetProperty<StaticListChoiceProperty>(PropertyNames.CompressorType).Value;
            DdsErrorMetric errorMetric = (DdsErrorMetric) token.GetProperty<StaticListChoiceProperty>(PropertyNames.ErrorMetric).Value;
            bool weightColorByAlpha = token.GetProperty<BooleanProperty>(PropertyNames.WeightColorByAlpha).Value;
            bool generateMipMaps = token.GetProperty<BooleanProperty>(PropertyNames.GenerateMipMaps).Value;
            ResamplingAlgorithm mipMapResamplingAlgorithm = (ResamplingAlgorithm) token.GetProperty<StaticListChoiceProperty>(PropertyNames.MipMapResamplingAlgorithm).Value;
            new DdsFile().Save(output, scratchSurface, fileFormat, compressorType, errorMetric, generateMipMaps, mipMapResamplingAlgorithm, weightColorByAlpha, callback);
        }

        public enum PropertyNames
        {
            FileFormat,
            CompressorType,
            ErrorMetric,
            WeightColorByAlpha,
            GenerateMipMaps,
            MipMapResamplingAlgorithm
        }
    }
}

