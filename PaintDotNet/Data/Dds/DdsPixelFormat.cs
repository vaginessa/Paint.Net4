namespace PaintDotNet.Data.Dds
{
    using PaintDotNet.IO;
    using System;
    using System.IO;

    internal sealed class DdsPixelFormat
    {
        public uint m_aBitMask;
        public uint m_bBitMask;
        public uint m_flags;
        public uint m_fourCC;
        public uint m_gBitMask;
        public uint m_rBitMask;
        public uint m_rgbBitCount;
        public uint m_size;

        public void Initialise(DdsFileFormat fileFormat)
        {
            this.m_size = this.Size();
            switch (fileFormat)
            {
                case DdsFileFormat.DDS_FORMAT_DXT1:
                case DdsFileFormat.DDS_FORMAT_DXT3:
                case DdsFileFormat.DDS_FORMAT_DXT5:
                    this.m_flags = 4;
                    this.m_rgbBitCount = 0;
                    this.m_rBitMask = 0;
                    this.m_gBitMask = 0;
                    this.m_bBitMask = 0;
                    this.m_aBitMask = 0;
                    if (fileFormat == DdsFileFormat.DDS_FORMAT_DXT1)
                    {
                        this.m_fourCC = 0x31545844;
                    }
                    if (fileFormat == DdsFileFormat.DDS_FORMAT_DXT3)
                    {
                        this.m_fourCC = 0x33545844;
                    }
                    if (fileFormat != DdsFileFormat.DDS_FORMAT_DXT5)
                    {
                        break;
                    }
                    this.m_fourCC = 0x35545844;
                    return;

                case DdsFileFormat.DDS_FORMAT_A8R8G8B8:
                    this.m_flags = 0x41;
                    this.m_rgbBitCount = 0x20;
                    this.m_fourCC = 0;
                    this.m_rBitMask = 0xff0000;
                    this.m_gBitMask = 0xff00;
                    this.m_bBitMask = 0xff;
                    this.m_aBitMask = 0xff000000;
                    return;

                case DdsFileFormat.DDS_FORMAT_X8R8G8B8:
                    this.m_flags = 0x40;
                    this.m_rgbBitCount = 0x20;
                    this.m_fourCC = 0;
                    this.m_rBitMask = 0xff0000;
                    this.m_gBitMask = 0xff00;
                    this.m_bBitMask = 0xff;
                    this.m_aBitMask = 0;
                    return;

                case DdsFileFormat.DDS_FORMAT_A8B8G8R8:
                    this.m_flags = 0x41;
                    this.m_rgbBitCount = 0x20;
                    this.m_fourCC = 0;
                    this.m_rBitMask = 0xff;
                    this.m_gBitMask = 0xff00;
                    this.m_bBitMask = 0xff0000;
                    this.m_aBitMask = 0xff000000;
                    return;

                case DdsFileFormat.DDS_FORMAT_X8B8G8R8:
                    this.m_flags = 0x40;
                    this.m_rgbBitCount = 0x20;
                    this.m_fourCC = 0;
                    this.m_rBitMask = 0xff;
                    this.m_gBitMask = 0xff00;
                    this.m_bBitMask = 0xff0000;
                    this.m_aBitMask = 0;
                    return;

                case DdsFileFormat.DDS_FORMAT_A1R5G5B5:
                    this.m_flags = 0x41;
                    this.m_rgbBitCount = 0x10;
                    this.m_fourCC = 0;
                    this.m_rBitMask = 0x7c00;
                    this.m_gBitMask = 0x3e0;
                    this.m_bBitMask = 0x1f;
                    this.m_aBitMask = 0x8000;
                    return;

                case DdsFileFormat.DDS_FORMAT_A4R4G4B4:
                    this.m_flags = 0x41;
                    this.m_rgbBitCount = 0x10;
                    this.m_fourCC = 0;
                    this.m_rBitMask = 0xf00;
                    this.m_gBitMask = 240;
                    this.m_bBitMask = 15;
                    this.m_aBitMask = 0xf000;
                    return;

                case DdsFileFormat.DDS_FORMAT_R8G8B8:
                    this.m_flags = 0x40;
                    this.m_fourCC = 0;
                    this.m_rgbBitCount = 0x18;
                    this.m_rBitMask = 0xff0000;
                    this.m_gBitMask = 0xff00;
                    this.m_bBitMask = 0xff;
                    this.m_aBitMask = 0;
                    return;

                case DdsFileFormat.DDS_FORMAT_R5G6B5:
                    this.m_flags = 0x40;
                    this.m_fourCC = 0;
                    this.m_rgbBitCount = 0x10;
                    this.m_rBitMask = 0xf800;
                    this.m_gBitMask = 0x7e0;
                    this.m_bBitMask = 0x1f;
                    this.m_aBitMask = 0;
                    break;

                default:
                    return;
            }
        }

        public void Read(Stream input)
        {
            this.m_size = (uint) input.ReadUInt32();
            this.m_flags = (uint) input.ReadUInt32();
            this.m_fourCC = (uint) input.ReadUInt32();
            this.m_rgbBitCount = (uint) input.ReadUInt32();
            this.m_rBitMask = (uint) input.ReadUInt32();
            this.m_gBitMask = (uint) input.ReadUInt32();
            this.m_bBitMask = (uint) input.ReadUInt32();
            this.m_aBitMask = (uint) input.ReadUInt32();
        }

        public uint Size() => 
            0x20;

        public void Write(Stream output)
        {
            output.WriteUInt32(this.m_size);
            output.WriteUInt32(this.m_flags);
            output.WriteUInt32(this.m_fourCC);
            output.WriteUInt32(this.m_rgbBitCount);
            output.WriteUInt32(this.m_rBitMask);
            output.WriteUInt32(this.m_gBitMask);
            output.WriteUInt32(this.m_bBitMask);
            output.WriteUInt32(this.m_aBitMask);
        }

        public enum PixelFormatFlags
        {
            DDS_FOURCC = 4,
            DDS_RGB = 0x40,
            DDS_RGBA = 0x41
        }
    }
}

