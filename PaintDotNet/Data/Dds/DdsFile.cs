namespace PaintDotNet.Data.Dds
{
    using PaintDotNet;
    using PaintDotNet.IO;
    using PaintDotNet.Rendering;
    using System;
    using System.IO;

    internal sealed class DdsFile
    {
        public DdsHeader m_header = new DdsHeader();
        private byte[] m_pixelData;

        public int GetHeight() => 
            ((int) this.m_header.m_height);

        public byte[] GetPixelData() => 
            this.m_pixelData;

        private int GetSquishFlags(DdsFileFormat fileFormat, DdsCompressorType compressorType, DdsErrorMetric errorMetric, bool weightColorByAlpha)
        {
            int num = 0;
            if (fileFormat == DdsFileFormat.DDS_FORMAT_DXT1)
            {
                num |= 1;
            }
            else if (fileFormat == DdsFileFormat.DDS_FORMAT_DXT3)
            {
                num |= 2;
            }
            else if (fileFormat == DdsFileFormat.DDS_FORMAT_DXT5)
            {
                num |= 4;
            }
            if (num != 0)
            {
                if (compressorType == DdsCompressorType.ClusterFit)
                {
                    num |= 8;
                }
                else if (compressorType == DdsCompressorType.RangeFit)
                {
                    num |= 0x10;
                }
                else
                {
                    num |= 0x100;
                }
                if (errorMetric == DdsErrorMetric.Perceptual)
                {
                    num |= 0x20;
                }
                else
                {
                    num |= 0x40;
                }
                if ((compressorType == DdsCompressorType.ClusterFit) & weightColorByAlpha)
                {
                    num |= 0x80;
                }
            }
            return num;
        }

        public int GetWidth() => 
            ((int) this.m_header.m_width);

        public void Load(Stream input)
        {
            uint num = (uint) input.ReadUInt32();
            if (num != 0x20534444)
            {
                throw new FormatException("File does not appear to be a DDS image");
            }
            this.m_header.Read(input);
            if ((this.m_header.m_pixelFormat.m_flags != 0) && ((this.m_header.m_pixelFormat.m_flags & 4) == 0))
            {
                DdsFileFormat format;
                if ((((this.m_header.m_pixelFormat.m_flags == 0x41) && (this.m_header.m_pixelFormat.m_rgbBitCount == 0x20)) && ((this.m_header.m_pixelFormat.m_rBitMask == 0xff0000) && (this.m_header.m_pixelFormat.m_gBitMask == 0xff00))) && ((this.m_header.m_pixelFormat.m_bBitMask == 0xff) && (this.m_header.m_pixelFormat.m_aBitMask == 0xff000000)))
                {
                    format = DdsFileFormat.DDS_FORMAT_A8R8G8B8;
                }
                else if ((((this.m_header.m_pixelFormat.m_flags == 0x40) && (this.m_header.m_pixelFormat.m_rgbBitCount == 0x20)) && ((this.m_header.m_pixelFormat.m_rBitMask == 0xff0000) && (this.m_header.m_pixelFormat.m_gBitMask == 0xff00))) && ((this.m_header.m_pixelFormat.m_bBitMask == 0xff) && (this.m_header.m_pixelFormat.m_aBitMask == 0)))
                {
                    format = DdsFileFormat.DDS_FORMAT_X8R8G8B8;
                }
                else if ((((this.m_header.m_pixelFormat.m_flags == 0x41) && (this.m_header.m_pixelFormat.m_rgbBitCount == 0x20)) && ((this.m_header.m_pixelFormat.m_rBitMask == 0xff) && (this.m_header.m_pixelFormat.m_gBitMask == 0xff00))) && ((this.m_header.m_pixelFormat.m_bBitMask == 0xff0000) && (this.m_header.m_pixelFormat.m_aBitMask == 0xff000000)))
                {
                    format = DdsFileFormat.DDS_FORMAT_A8B8G8R8;
                }
                else if ((((this.m_header.m_pixelFormat.m_flags == 0x40) && (this.m_header.m_pixelFormat.m_rgbBitCount == 0x20)) && ((this.m_header.m_pixelFormat.m_rBitMask == 0xff) && (this.m_header.m_pixelFormat.m_gBitMask == 0xff00))) && ((this.m_header.m_pixelFormat.m_bBitMask == 0xff0000) && (this.m_header.m_pixelFormat.m_aBitMask == 0)))
                {
                    format = DdsFileFormat.DDS_FORMAT_X8B8G8R8;
                }
                else if ((((this.m_header.m_pixelFormat.m_flags == 0x41) && (this.m_header.m_pixelFormat.m_rgbBitCount == 0x10)) && ((this.m_header.m_pixelFormat.m_rBitMask == 0x7c00) && (this.m_header.m_pixelFormat.m_gBitMask == 0x3e0))) && ((this.m_header.m_pixelFormat.m_bBitMask == 0x1f) && (this.m_header.m_pixelFormat.m_aBitMask == 0x8000)))
                {
                    format = DdsFileFormat.DDS_FORMAT_A1R5G5B5;
                }
                else if ((((this.m_header.m_pixelFormat.m_flags == 0x41) && (this.m_header.m_pixelFormat.m_rgbBitCount == 0x10)) && ((this.m_header.m_pixelFormat.m_rBitMask == 0xf00) && (this.m_header.m_pixelFormat.m_gBitMask == 240))) && ((this.m_header.m_pixelFormat.m_bBitMask == 15) && (this.m_header.m_pixelFormat.m_aBitMask == 0xf000)))
                {
                    format = DdsFileFormat.DDS_FORMAT_A4R4G4B4;
                }
                else if ((((this.m_header.m_pixelFormat.m_flags == 0x40) && (this.m_header.m_pixelFormat.m_rgbBitCount == 0x18)) && ((this.m_header.m_pixelFormat.m_rBitMask == 0xff0000) && (this.m_header.m_pixelFormat.m_gBitMask == 0xff00))) && ((this.m_header.m_pixelFormat.m_bBitMask == 0xff) && (this.m_header.m_pixelFormat.m_aBitMask == 0)))
                {
                    format = DdsFileFormat.DDS_FORMAT_R8G8B8;
                }
                else
                {
                    if ((((this.m_header.m_pixelFormat.m_flags != 0x40) || (this.m_header.m_pixelFormat.m_rgbBitCount != 0x10)) || ((this.m_header.m_pixelFormat.m_rBitMask != 0xf800) || (this.m_header.m_pixelFormat.m_gBitMask != 0x7e0))) || ((this.m_header.m_pixelFormat.m_bBitMask != 0x1f) || (this.m_header.m_pixelFormat.m_aBitMask != 0)))
                    {
                        throw new FormatException("File is not a supported DDS format");
                    }
                    format = DdsFileFormat.DDS_FORMAT_R5G6B5;
                }
                int num6 = (int) (this.m_header.m_pixelFormat.m_rgbBitCount / 8);
                int pitchOrLinearSize = 0;
                if ((this.m_header.m_headerFlags & 8) != 0)
                {
                    pitchOrLinearSize = (int) this.m_header.m_pitchOrLinearSize;
                }
                else if ((this.m_header.m_headerFlags & 0x80000) != 0)
                {
                    pitchOrLinearSize = (int) (this.m_header.m_pitchOrLinearSize / this.m_header.m_height);
                }
                else
                {
                    pitchOrLinearSize = (int) (this.m_header.m_width * num6);
                }
                byte[] buffer2 = new byte[pitchOrLinearSize * this.m_header.m_height];
                input.Read(buffer2, 0, buffer2.GetLength(0));
                this.m_pixelData = new byte[(this.m_header.m_width * this.m_header.m_height) * 4];
                for (int i = 0; i < this.m_header.m_height; i++)
                {
                    for (int j = 0; j < this.m_header.m_width; j++)
                    {
                        int num10 = (i * pitchOrLinearSize) + (j * num6);
                        uint num11 = 0;
                        uint num12 = 0;
                        uint num13 = 0;
                        uint num14 = 0;
                        uint num15 = 0;
                        for (int k = 0; k < num6; k++)
                        {
                            num11 |= (uint) (buffer2[num10 + k] << (8 * k));
                        }
                        switch (format)
                        {
                            case DdsFileFormat.DDS_FORMAT_A8R8G8B8:
                                num15 = (num11 >> 0x18) & 0xff;
                                num12 = (num11 >> 0x10) & 0xff;
                                num13 = (num11 >> 8) & 0xff;
                                num14 = num11 & 0xff;
                                break;

                            case DdsFileFormat.DDS_FORMAT_X8R8G8B8:
                                num15 = 0xff;
                                num12 = (num11 >> 0x10) & 0xff;
                                num13 = (num11 >> 8) & 0xff;
                                num14 = num11 & 0xff;
                                break;

                            case DdsFileFormat.DDS_FORMAT_A8B8G8R8:
                                num15 = (num11 >> 0x18) & 0xff;
                                num12 = num11 & 0xff;
                                num13 = (num11 >> 8) & 0xff;
                                num14 = (num11 >> 0x10) & 0xff;
                                break;

                            case DdsFileFormat.DDS_FORMAT_X8B8G8R8:
                                num15 = 0xff;
                                num12 = num11 & 0xff;
                                num13 = (num11 >> 8) & 0xff;
                                num14 = (num11 >> 0x10) & 0xff;
                                break;

                            case DdsFileFormat.DDS_FORMAT_A1R5G5B5:
                                num15 = (num11 >> 15) * 0xff;
                                num12 = (num11 >> 10) & 0x1f;
                                num13 = (num11 >> 5) & 0x1f;
                                num14 = num11 & 0x1f;
                                num12 = (num12 << 3) | (num12 >> 2);
                                num13 = (num13 << 3) | (num13 >> 2);
                                num14 = (num14 << 3) | (num14 >> 2);
                                break;

                            case DdsFileFormat.DDS_FORMAT_A4R4G4B4:
                                num15 = (num11 >> 12) & 0xff;
                                num12 = (num11 >> 8) & 15;
                                num13 = (num11 >> 4) & 15;
                                num14 = num11 & 15;
                                num15 = (num15 << 4) | num15;
                                num12 = (num12 << 4) | num12;
                                num13 = (num13 << 4) | num13;
                                num14 = (num14 << 4) | num14;
                                break;

                            case DdsFileFormat.DDS_FORMAT_R8G8B8:
                                num15 = 0xff;
                                num12 = (num11 >> 0x10) & 0xff;
                                num13 = (num11 >> 8) & 0xff;
                                num14 = num11 & 0xff;
                                break;

                            case DdsFileFormat.DDS_FORMAT_R5G6B5:
                                num15 = 0xff;
                                num12 = (num11 >> 11) & 0x1f;
                                num13 = (num11 >> 5) & 0x3f;
                                num14 = num11 & 0x1f;
                                num12 = (num12 << 3) | (num12 >> 2);
                                num13 = (num13 << 2) | (num13 >> 4);
                                num14 = (num14 << 3) | (num14 >> 2);
                                break;
                        }
                        int index = ((int) ((i * this.m_header.m_width) * 4)) + (j * 4);
                        this.m_pixelData[index] = (byte) num12;
                        this.m_pixelData[index + 1] = (byte) num13;
                        this.m_pixelData[index + 2] = (byte) num14;
                        this.m_pixelData[index + 3] = (byte) num15;
                    }
                }
            }
            else
            {
                int flags = 0;
                switch (this.m_header.m_pixelFormat.m_fourCC)
                {
                    case 0x31545844:
                        flags = 1;
                        break;

                    case 0x33545844:
                        flags = 2;
                        break;

                    case 0x35545844:
                        flags = 4;
                        break;

                    default:
                        throw new FormatException("File is not a supported DDS format");
                }
                int num3 = ((this.GetWidth() + 3) / 4) * ((this.GetHeight() + 3) / 4);
                int num4 = ((flags & 1) != 0) ? 8 : 0x10;
                byte[] buffer = new byte[num3 * num4];
                input.Read(buffer, 0, buffer.GetLength(0));
                this.m_pixelData = DdsSquish.DecompressImage(buffer, this.GetWidth(), this.GetHeight(), flags);
            }
        }

        public void Save(Stream output, Surface surface, DdsFileFormat fileFormat, DdsCompressorType compressorType, DdsErrorMetric errorMetric, bool generateMipMaps, ResamplingAlgorithm mipMapResamplingAlgorithm, bool weightColorByAlpha, ProgressEventHandler progressCallback)
        {
            int num21;
            int num = 0;
            bool flag = ((fileFormat == DdsFileFormat.DDS_FORMAT_DXT1) || (fileFormat == DdsFileFormat.DDS_FORMAT_DXT3)) || (fileFormat == DdsFileFormat.DDS_FORMAT_DXT5);
            int num2 = 1;
            int mipWidth = surface.Width;
            int height = surface.Height;
            if (generateMipMaps)
            {
                while ((mipWidth > 1) || (height > 1))
                {
                    num2++;
                    mipWidth /= 2;
                    height /= 2;
                }
            }
            this.m_header.m_size = this.m_header.Size();
            this.m_header.m_headerFlags = 0x1007;
            if (flag)
            {
                this.m_header.m_headerFlags |= 0x80000;
            }
            else
            {
                this.m_header.m_headerFlags |= 8;
            }
            if (num2 > 1)
            {
                this.m_header.m_headerFlags |= 0x20000;
            }
            this.m_header.m_height = (uint) surface.Height;
            this.m_header.m_width = (uint) surface.Width;
            if (flag)
            {
                int num5 = ((surface.Width + 3) / 4) * ((surface.Height + 3) / 4);
                int num6 = (fileFormat == DdsFileFormat.DDS_FORMAT_DXT1) ? 8 : 0x10;
                this.m_header.m_pitchOrLinearSize = (uint) (num5 * num6);
            }
            else
            {
                switch (fileFormat)
                {
                    case DdsFileFormat.DDS_FORMAT_A8R8G8B8:
                    case DdsFileFormat.DDS_FORMAT_X8R8G8B8:
                    case DdsFileFormat.DDS_FORMAT_A8B8G8R8:
                    case DdsFileFormat.DDS_FORMAT_X8B8G8R8:
                        num = 4;
                        break;

                    case DdsFileFormat.DDS_FORMAT_A1R5G5B5:
                    case DdsFileFormat.DDS_FORMAT_A4R4G4B4:
                    case DdsFileFormat.DDS_FORMAT_R5G6B5:
                        num = 2;
                        break;

                    case DdsFileFormat.DDS_FORMAT_R8G8B8:
                        num = 3;
                        break;
                }
                this.m_header.m_pitchOrLinearSize = (uint) (this.m_header.m_width * num);
            }
            this.m_header.m_depth = 0;
            this.m_header.m_mipMapCount = (num2 == 1) ? 0 : ((uint) num2);
            this.m_header.m_reserved1_0 = 0;
            this.m_header.m_reserved1_1 = 0;
            this.m_header.m_reserved1_2 = 0;
            this.m_header.m_reserved1_3 = 0;
            this.m_header.m_reserved1_4 = 0;
            this.m_header.m_reserved1_5 = 0;
            this.m_header.m_reserved1_6 = 0;
            this.m_header.m_reserved1_7 = 0;
            this.m_header.m_reserved1_8 = 0;
            this.m_header.m_reserved1_9 = 0;
            this.m_header.m_reserved1_10 = 0;
            this.m_header.m_pixelFormat.Initialise(fileFormat);
            this.m_header.m_surfaceFlags = 0x1000;
            if (num2 > 1)
            {
                this.m_header.m_surfaceFlags |= 0x400008;
            }
            this.m_header.m_cubemapFlags = 0;
            this.m_header.m_reserved2_0 = 0;
            this.m_header.m_reserved2_1 = 0;
            this.m_header.m_reserved2_2 = 0;
            output.WriteUInt32(0x20534444);
            this.m_header.Write(output);
            int squishFlags = this.GetSquishFlags(fileFormat, compressorType, errorMetric, weightColorByAlpha);
            mipWidth = surface.Width;
            height = surface.Height;
            SizeInt32[] numArray = new SizeInt32[num2];
            int[] numArray2 = new int[num2];
            int[] pixelsCompleted = new int[num2];
            long totalPixels = 0L;
            for (int i = 0; i < num2; i++)
            {
                SizeInt32 num8 = new SizeInt32((mipWidth > 0) ? mipWidth : 1, (height > 0) ? height : 1);
                numArray[i] = num8;
                int num9 = num8.Width * num8.Height;
                numArray2[i] = num9;
                if (i == 0)
                {
                    pixelsCompleted[i] = 0;
                }
                else
                {
                    pixelsCompleted[i] = pixelsCompleted[i - 1] + numArray2[i - 1];
                }
                totalPixels += num9;
                mipWidth /= 2;
                height /= 2;
            }
            mipWidth = surface.Width;
            height = surface.Height;
            for (int mipLoop = 0; mipLoop < num2; mipLoop = num21 + 1)
            {
                byte[] buffer;
                SizeInt32 size = numArray[mipLoop];
                Surface surface2 = new Surface(size);
                if (mipLoop == 0)
                {
                    surface2 = surface;
                }
                else
                {
                    IRenderer<ColorBgra> renderer;
                    SizeInt32 newSize = surface2.Size<ColorBgra>();
                    switch (mipMapResamplingAlgorithm)
                    {
                        case ResamplingAlgorithm.NearestNeighbor:
                            renderer = surface.ResizeNearestNeighbor(newSize);
                            break;

                        case ResamplingAlgorithm.Bilinear:
                            renderer = surface.ResizeBilinear(newSize);
                            break;

                        case ResamplingAlgorithm.Bicubic:
                            renderer = surface.ResizeBicubic(newSize);
                            break;

                        case ResamplingAlgorithm.SuperSampling:
                            renderer = surface.ResizeSuperSampling(newSize);
                            break;

                        case ResamplingAlgorithm.Fant:
                            renderer = surface.ResizeFant(newSize);
                            break;

                        default:
                            throw ExceptionUtil.InvalidEnumArgumentException<ResamplingAlgorithm>(mipMapResamplingAlgorithm, "mipMapResamplingAlgorithm");
                    }
                    renderer.Render<ColorBgra>(surface2);
                }
                DdsSquish.ProgressFn fn = delegate (int workDone, int workTotal) {
                    long num = workDone * mipWidth;
                    long num2 = pixelsCompleted[mipLoop];
                    double num3 = (num + num2) / ((double) totalPixels);
                    progressCallback(this, new ProgressEventArgs(DoubleUtil.Clamp(100.0 * num3, 0.0, 100.0)));
                };
                if ((fileFormat >= DdsFileFormat.DDS_FORMAT_DXT1) && (fileFormat <= DdsFileFormat.DDS_FORMAT_DXT5))
                {
                    buffer = DdsSquish.CompressImage(surface2, squishFlags, (progressCallback == null) ? null : fn);
                }
                else
                {
                    int num12 = num * surface2.Width;
                    buffer = new byte[num12 * surface2.Height];
                    buffer.Initialize();
                    for (int j = 0; j < surface2.Height; j++)
                    {
                        for (int k = 0; k < surface2.Width; k++)
                        {
                            ColorBgra point = surface2.GetPoint(k, j);
                            uint num15 = 0;
                            switch (fileFormat)
                            {
                                case DdsFileFormat.DDS_FORMAT_A8R8G8B8:
                                    num15 = (uint) ((((point.A << 0x18) | (point.R << 0x10)) | (point.G << 8)) | point.B);
                                    break;

                                case DdsFileFormat.DDS_FORMAT_X8R8G8B8:
                                    num15 = (uint) (((point.R << 0x10) | (point.G << 8)) | point.B);
                                    break;

                                case DdsFileFormat.DDS_FORMAT_A8B8G8R8:
                                    num15 = (uint) ((((point.A << 0x18) | (point.B << 0x10)) | (point.G << 8)) | point.R);
                                    break;

                                case DdsFileFormat.DDS_FORMAT_X8B8G8R8:
                                    num15 = (uint) (((point.B << 0x10) | (point.G << 8)) | point.R);
                                    break;

                                case DdsFileFormat.DDS_FORMAT_A1R5G5B5:
                                    num15 = (uint) ((((((point.A != null) ? 1 : 0) << 15) | ((point.R >> 3) << 10)) | ((point.G >> 3) << 5)) | (point.B >> 3));
                                    break;

                                case DdsFileFormat.DDS_FORMAT_A4R4G4B4:
                                    num15 = (uint) (((((point.A >> 4) << 12) | ((point.R >> 4) << 8)) | ((point.G >> 4) << 4)) | (point.B >> 4));
                                    break;

                                case DdsFileFormat.DDS_FORMAT_R8G8B8:
                                    num15 = (uint) (((point.R << 0x10) | (point.G << 8)) | point.B);
                                    break;

                                case DdsFileFormat.DDS_FORMAT_R5G6B5:
                                    num15 = (uint) ((((point.R >> 3) << 11) | ((point.G >> 2) << 5)) | (point.B >> 3));
                                    break;
                            }
                            int num16 = (j * num12) + (k * num);
                            for (int m = 0; m < num; m++)
                            {
                                buffer[num16 + m] = (byte) ((num15 >> (8 * m)) & 0xff);
                            }
                        }
                        if (progressCallback != null)
                        {
                            long num18 = (j + 1) * mipWidth;
                            long num19 = pixelsCompleted[mipLoop];
                            double num20 = (num18 + num19) / ((double) totalPixels);
                            progressCallback(this, new ProgressEventArgs(100.0 * num20));
                        }
                    }
                }
                output.Write(buffer, 0, buffer.GetLength(0));
                mipWidth /= 2;
                height /= 2;
                num21 = mipLoop;
            }
        }
    }
}

