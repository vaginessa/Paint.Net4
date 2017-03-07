namespace PaintDotNet.Data.Dds
{
    using PaintDotNet.IO;
    using System;
    using System.IO;

    internal class DdsHeader
    {
        public uint m_cubemapFlags;
        public uint m_depth;
        public uint m_headerFlags;
        public uint m_height;
        public uint m_mipMapCount;
        public uint m_pitchOrLinearSize;
        public DdsPixelFormat m_pixelFormat = new DdsPixelFormat();
        public uint m_reserved1_0;
        public uint m_reserved1_1;
        public uint m_reserved1_10;
        public uint m_reserved1_2;
        public uint m_reserved1_3;
        public uint m_reserved1_4;
        public uint m_reserved1_5;
        public uint m_reserved1_6;
        public uint m_reserved1_7;
        public uint m_reserved1_8;
        public uint m_reserved1_9;
        public uint m_reserved2_0;
        public uint m_reserved2_1;
        public uint m_reserved2_2;
        public uint m_size;
        public uint m_surfaceFlags;
        public uint m_width;

        public void Read(Stream input)
        {
            this.m_size = (uint) input.ReadUInt32();
            this.m_headerFlags = (uint) input.ReadUInt32();
            this.m_height = (uint) input.ReadUInt32();
            this.m_width = (uint) input.ReadUInt32();
            this.m_pitchOrLinearSize = (uint) input.ReadUInt32();
            this.m_depth = (uint) input.ReadUInt32();
            this.m_mipMapCount = (uint) input.ReadUInt32();
            this.m_reserved1_0 = (uint) input.ReadUInt32();
            this.m_reserved1_1 = (uint) input.ReadUInt32();
            this.m_reserved1_2 = (uint) input.ReadUInt32();
            this.m_reserved1_3 = (uint) input.ReadUInt32();
            this.m_reserved1_4 = (uint) input.ReadUInt32();
            this.m_reserved1_5 = (uint) input.ReadUInt32();
            this.m_reserved1_6 = (uint) input.ReadUInt32();
            this.m_reserved1_7 = (uint) input.ReadUInt32();
            this.m_reserved1_8 = (uint) input.ReadUInt32();
            this.m_reserved1_9 = (uint) input.ReadUInt32();
            this.m_reserved1_10 = (uint) input.ReadUInt32();
            this.m_pixelFormat.Read(input);
            this.m_surfaceFlags = (uint) input.ReadUInt32();
            this.m_cubemapFlags = (uint) input.ReadUInt32();
            this.m_reserved2_0 = (uint) input.ReadUInt32();
            this.m_reserved2_1 = (uint) input.ReadUInt32();
            this.m_reserved2_2 = (uint) input.ReadUInt32();
        }

        public uint Size() => 
            ((0x48 + this.m_pixelFormat.Size()) + 20);

        public void Write(Stream output)
        {
            output.WriteUInt32(this.m_size);
            output.WriteUInt32(this.m_headerFlags);
            output.WriteUInt32(this.m_height);
            output.WriteUInt32(this.m_width);
            output.WriteUInt32(this.m_pitchOrLinearSize);
            output.WriteUInt32(this.m_depth);
            output.WriteUInt32(this.m_mipMapCount);
            output.WriteUInt32(this.m_reserved1_0);
            output.WriteUInt32(this.m_reserved1_1);
            output.WriteUInt32(this.m_reserved1_2);
            output.WriteUInt32(this.m_reserved1_3);
            output.WriteUInt32(this.m_reserved1_4);
            output.WriteUInt32(this.m_reserved1_5);
            output.WriteUInt32(this.m_reserved1_6);
            output.WriteUInt32(this.m_reserved1_7);
            output.WriteUInt32(this.m_reserved1_8);
            output.WriteUInt32(this.m_reserved1_9);
            output.WriteUInt32(this.m_reserved1_10);
            this.m_pixelFormat.Write(output);
            output.WriteUInt32(this.m_surfaceFlags);
            output.WriteUInt32(this.m_cubemapFlags);
            output.WriteUInt32(this.m_reserved2_0);
            output.WriteUInt32(this.m_reserved2_1);
            output.WriteUInt32(this.m_reserved2_2);
        }

        public enum CubemapFlags
        {
            DDS_CUBEMAP_ALLFACES = 0xfe00,
            DDS_CUBEMAP_NEGATIVEX = 0xa00,
            DDS_CUBEMAP_NEGATIVEY = 0x2200,
            DDS_CUBEMAP_NEGATIVEZ = 0x8200,
            DDS_CUBEMAP_POSITIVEX = 0x600,
            DDS_CUBEMAP_POSITIVEY = 0x1200,
            DDS_CUBEMAP_POSITIVEZ = 0x4200
        }

        public enum HeaderFlags
        {
            DDS_HEADER_FLAGS_LINEARSIZE = 0x80000,
            DDS_HEADER_FLAGS_MIPMAP = 0x20000,
            DDS_HEADER_FLAGS_PITCH = 8,
            DDS_HEADER_FLAGS_TEXTURE = 0x1007,
            DDS_HEADER_FLAGS_VOLUME = 0x800000
        }

        public enum SurfaceFlags
        {
            DDS_SURFACE_FLAGS_CUBEMAP = 8,
            DDS_SURFACE_FLAGS_MIPMAP = 0x400008,
            DDS_SURFACE_FLAGS_TEXTURE = 0x1000
        }

        public enum VolumeFlags
        {
            DDS_FLAGS_VOLUME = 0x200000
        }
    }
}

