namespace PaintDotNet.Data
{
    using PaintDotNet;
    using PaintDotNet.IndirectUI;
    using PaintDotNet.IO;
    using PaintDotNet.PropertySystem;
    using PaintDotNet.Resources;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;

    internal sealed class TgaFileType : InternalFileType
    {
        public TgaFileType() : base("TGA", FileTypeFlags.None | FileTypeFlags.SavesWithProgress | FileTypeFlags.SupportsLoading | FileTypeFlags.SupportsSaving, textArray1)
        {
            string[] textArray1 = new string[] { ".tga" };
        }

        internal override HashSet<InternalFileType.SavableBitDepths> CreateAllowedBitDepthListFromToken(PropertyBasedSaveConfigToken token)
        {
            TgaBitDepthUIChoices choices = (TgaBitDepthUIChoices) token.GetProperty<StaticListChoiceProperty>(PropertyNames.BitDepth).Value;
            HashSet<InternalFileType.SavableBitDepths> set = new HashSet<InternalFileType.SavableBitDepths>();
            switch (choices)
            {
                case TgaBitDepthUIChoices.AutoDetect:
                    set.Add(InternalFileType.SavableBitDepths.Rgb24);
                    set.Add(InternalFileType.SavableBitDepths.Rgba32);
                    return set;

                case TgaBitDepthUIChoices.Bpp32:
                    set.Add(InternalFileType.SavableBitDepths.Rgba32);
                    return set;

                case TgaBitDepthUIChoices.Bpp24:
                    set.Add(InternalFileType.SavableBitDepths.Rgb24);
                    return set;
            }
            throw ExceptionUtil.InvalidEnumArgumentException<TgaBitDepthUIChoices>(choices, "bitDepth");
        }

        private ColorBgra[] CreateGrayPalette()
        {
            ColorBgra[] bgraArray = new ColorBgra[0x100];
            for (int i = 0; i < bgraArray.Length; i++)
            {
                bgraArray[i] = ColorBgra.FromBgra((byte) i, (byte) i, (byte) i, 0xff);
            }
            return bgraArray;
        }

        private byte ExpandCompressedLine(MemoryBlock dst, int dstIndex, ref TgaHeader header, Stream input, int width, int y, byte rleLeftOver, ColorBgra[] palette)
        {
            byte num;
            long position = 0L;
            for (int i = 0; i < width; i += num)
            {
                if (rleLeftOver != 0xff)
                {
                    num = rleLeftOver;
                    rleLeftOver = 0xff;
                }
                else
                {
                    int num4 = input.ReadByte();
                    if (num4 == -1)
                    {
                        throw new EndOfStreamException();
                    }
                    num = (byte) num4;
                }
                if ((num & 0x80) != 0)
                {
                    num = (byte) (num - 0x7f);
                    if ((i + num) > width)
                    {
                        rleLeftOver = (byte) (0x80 + ((num - (width - i)) - 1));
                        position = input.Position;
                        num = (byte) (width - i);
                    }
                    ColorBgra bgra = this.ReadColor(input, header.pixelDepth, palette);
                    for (int j = 0; j < num; j++)
                    {
                        int num6 = dstIndex + (j * 4);
                        dst[(long) num6] = bgra[0];
                        dst[(long) (1 + num6)] = bgra[1];
                        dst[(long) (2 + num6)] = bgra[2];
                        dst[(long) (3 + num6)] = bgra[3];
                    }
                    if (rleLeftOver != 0xff)
                    {
                        input.Position = position;
                    }
                }
                else
                {
                    num = (byte) (num + 1);
                    if ((i + num) > width)
                    {
                        rleLeftOver = (byte) ((num - (width - i)) - 1);
                        num = (byte) (width - i);
                    }
                    this.ExpandUncompressedLine(dst, dstIndex, ref header, input, num, y, i, palette);
                }
                dstIndex += num * 4;
            }
            return rleLeftOver;
        }

        private void ExpandUncompressedLine(MemoryBlock dst, int dstIndex, ref TgaHeader header, Stream input, int width, int y, int xoffset, ColorBgra[] palette)
        {
            for (int i = 0; i < width; i++)
            {
                ColorBgra bgra = this.ReadColor(input, header.pixelDepth, palette);
                dst[(long) dstIndex] = bgra[0];
                dst[(long) (1 + dstIndex)] = bgra[1];
                dst[(long) (2 + dstIndex)] = bgra[2];
                dst[(long) (3 + dstIndex)] = bgra[3];
                dstIndex += 4;
            }
        }

        internal override int GetDitherLevelFromToken(PropertyBasedSaveConfigToken token) => 
            0;

        internal override int GetThresholdFromToken(PropertyBasedSaveConfigToken token) => 
            0;

        protected override bool IsReflexive(PropertyBasedSaveConfigToken token) => 
            ((((TgaBitDepthUIChoices) token.GetProperty<StaticListChoiceProperty>(PropertyNames.BitDepth).Value) == TgaBitDepthUIChoices.Bpp32) || base.IsReflexive(token));

        private ColorBgra[] LoadPalette(Stream input, int count)
        {
            ColorBgra[] bgraArray = new ColorBgra[count];
            for (int i = 0; i < bgraArray.Length; i++)
            {
                int num2 = input.ReadByte();
                if (num2 == -1)
                {
                    throw new EndOfStreamException();
                }
                int num3 = input.ReadByte();
                if (num3 == -1)
                {
                    throw new EndOfStreamException();
                }
                int num4 = input.ReadByte();
                if (num4 == -1)
                {
                    throw new EndOfStreamException();
                }
                bgraArray[i] = ColorBgra.FromBgra((byte) num2, (byte) num3, (byte) num4, 0xff);
            }
            return bgraArray;
        }

        private void MirrorX(Surface surface)
        {
            for (int i = 0; i < surface.Height; i++)
            {
                for (int j = 0; j < (surface.Width / 2); j++)
                {
                    ColorBgra bgra = surface[(surface.Width - j) - 1, i];
                    surface[(surface.Width - j) - 1, i] = surface[j, i];
                    surface[j, i] = bgra;
                }
            }
        }

        public override ControlInfo OnCreateSaveConfigUI(PropertyCollection props)
        {
            ControlInfo info = PropertyBasedFileType.CreateDefaultSaveConfigUI(props);
            info.SetPropertyControlValue(PropertyNames.BitDepth, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("TgaFileType.ConfigUI.BitDepth.DisplayName"));
            info.SetPropertyControlType(PropertyNames.BitDepth, PropertyControlType.RadioButton);
            PropertyControlInfo info2 = info.FindControlForPropertyName(PropertyNames.BitDepth);
            info2.SetValueDisplayName(TgaBitDepthUIChoices.AutoDetect, PdnResources.GetString("TgaFileType.ConfigUI.BitDepth.AutoDetect.DisplayName"));
            info2.SetValueDisplayName(TgaBitDepthUIChoices.Bpp24, PdnResources.GetString("TgaFileType.ConfigUI.BitDepth.Bpp24.DisplayName"));
            info2.SetValueDisplayName(TgaBitDepthUIChoices.Bpp32, PdnResources.GetString("TgaFileType.ConfigUI.BitDepth.Bpp32.DisplayName"));
            info.SetPropertyControlValue(PropertyNames.RleCompress, ControlInfoPropertyNames.DisplayName, string.Empty);
            info.SetPropertyControlValue(PropertyNames.RleCompress, ControlInfoPropertyNames.Description, PdnResources.GetString("TgaFileType.ConfigUI.RleCompress.Description"));
            return info;
        }

        public override PropertyCollection OnCreateSavePropertyCollection() => 
            new PropertyCollection(new List<Property> { 
                StaticListChoiceProperty.CreateForEnum<TgaBitDepthUIChoices>(PropertyNames.BitDepth, TgaBitDepthUIChoices.AutoDetect, false),
                new BooleanProperty(PropertyNames.RleCompress, true)
            });

        internal override void OnFinalSave(Document input, Stream output, Surface scratchSurface, int ditherLevel, InternalFileType.SavableBitDepths bitDepth, PropertyBasedSaveConfigToken token, ProgressEventHandler progressCallback)
        {
            bool rleCompress = token.GetProperty<BooleanProperty>(PropertyNames.RleCompress).Value;
            this.SaveTga(scratchSurface, output, bitDepth, rleCompress, progressCallback);
        }

        protected override Document OnLoad(Stream input)
        {
            bool flag;
            Document document2;
            TgaHeader header = new TgaHeader(input);
            switch (header.imageType)
            {
                case TgaType.Map:
                case TgaType.Rgb:
                case TgaType.Mono:
                    flag = false;
                    break;

                case TgaType.RleMap:
                case TgaType.RleRgb:
                case TgaType.RleMono:
                    flag = true;
                    break;

                default:
                    throw new FormatException("unknown TGA image type");
            }
            if (((header.imageWidth == 0) || (header.imageHeight == 0)) || ((header.pixelDepth == 0) || (header.cmapLength > 0x100)))
            {
                throw new FormatException("bad TGA header");
            }
            if ((((header.pixelDepth != 8) && (header.pixelDepth != 15)) && ((header.pixelDepth != 0x10) && (header.pixelDepth != 0x18))) && (header.pixelDepth != 0x20))
            {
                throw new FormatException("bad TGA header: pixelDepth not one of {8, 15, 16, 24, 32}");
            }
            if (header.idLength > 0)
            {
                input.Position += header.idLength;
            }
            BitmapLayer layer = Layer.CreateBackgroundLayer(header.imageWidth, header.imageHeight);
            try
            {
                Surface surface = layer.Surface;
                surface.Clear((ColorBgra) (-1));
                ColorBgra[] palette = null;
                if (header.cmapType != 0)
                {
                    palette = this.LoadPalette(input, header.cmapLength);
                }
                if ((header.imageType == TgaType.Mono) || (header.imageType == TgaType.RleMono))
                {
                    palette = this.CreateGrayPalette();
                }
                bool flag2 = (header.imageDesc & 0x10) == 0x10;
                bool flag3 = (header.imageDesc & 0x20) == 0x20;
                byte rleLeftOver = 0xff;
                for (int i = 0; i < header.imageHeight; i++)
                {
                    MemoryBlock row;
                    if (flag3)
                    {
                        row = surface.GetRow(i);
                    }
                    else
                    {
                        row = surface.GetRow((header.imageHeight - i) - 1);
                    }
                    if (flag)
                    {
                        rleLeftOver = this.ExpandCompressedLine(row, 0, ref header, input, header.imageWidth, i, rleLeftOver, palette);
                    }
                    else
                    {
                        this.ExpandUncompressedLine(row, 0, ref header, input, header.imageWidth, i, 0, palette);
                    }
                }
                if (flag2)
                {
                    this.MirrorX(surface);
                }
                Document document = new Document(surface.Width, surface.Height);
                document.Layers.Add(layer);
                document2 = document;
            }
            catch
            {
                if (layer != null)
                {
                    layer.Dispose();
                    layer = null;
                }
                throw;
            }
            return document2;
        }

        private ColorBgra ReadColor(Stream input, int pixelDepth, ColorBgra[] palette)
        {
            int num3;
            if (pixelDepth <= 15)
            {
                if (pixelDepth == 8)
                {
                    int index = input.ReadByte();
                    if (index == -1)
                    {
                        throw new EndOfStreamException();
                    }
                    if (index >= palette.Length)
                    {
                        throw new FormatException("color index was outside the bounds of the palette");
                    }
                    return palette[index];
                }
                if (pixelDepth == 15)
                {
                    goto Label_006D;
                }
                goto Label_00D6;
            }
            if (pixelDepth != 0x10)
            {
                if (pixelDepth == 0x18)
                {
                    int num2 = input.ReadUInt24();
                    if (num2 == -1)
                    {
                        throw new EndOfStreamException();
                    }
                    ColorBgra bgra = ColorBgra.FromUInt32((uint) num2);
                    bgra.A = 0xff;
                    return bgra;
                }
                if (pixelDepth == 0x20)
                {
                    long num = input.ReadUInt32();
                    if (num == -1L)
                    {
                        throw new EndOfStreamException();
                    }
                    return ColorBgra.FromUInt32((uint) num);
                }
                goto Label_00D6;
            }
        Label_006D:
            num3 = input.ReadUInt16();
            if (num3 == -1)
            {
                throw new EndOfStreamException();
            }
            return ColorBgra.FromBgra((byte) ((num3 & 0x1f) * 8), (byte) ((num3 >> 2) & 0xf8), (byte) ((num3 >> 7) & 0xf8), 0xff);
        Label_00D6:
            throw new FormatException("colorDepth was not one of {8, 15, 16, 24, 32}");
        }

        private void SaveTga(Surface input, Stream output, InternalFileType.SavableBitDepths bitDepth, bool rleCompress, ProgressEventHandler progressCallback)
        {
            TgaHeader header = new TgaHeader {
                idLength = 0,
                cmapType = 0,
                imageType = rleCompress ? TgaType.RleRgb : TgaType.Rgb,
                cmapIndex = 0,
                cmapLength = 0,
                cmapEntrySize = 0,
                xOrigin = 0,
                yOrigin = 0,
                imageWidth = (ushort) input.Width,
                imageHeight = (ushort) input.Height,
                imageDesc = 0
            };
            if (bitDepth != InternalFileType.SavableBitDepths.Rgba32)
            {
                if (bitDepth != InternalFileType.SavableBitDepths.Rgb24)
                {
                    throw ExceptionUtil.InvalidEnumArgumentException<InternalFileType.SavableBitDepths>(bitDepth, "bitDepth");
                }
            }
            else
            {
                header.pixelDepth = 0x20;
                header.imageDesc = (byte) (header.imageDesc | 8);
                goto Label_00AC;
            }
            header.pixelDepth = 0x18;
        Label_00AC:
            header.Write(output);
            for (int i = input.Height - 1; i >= 0; i--)
            {
                if (rleCompress)
                {
                    SaveTgaRowRle(output, input, ref header, i);
                }
                else
                {
                    SaveTgaRowRaw(output, input, ref header, i);
                }
                if (progressCallback != null)
                {
                    progressCallback(this, new ProgressEventArgs(100.0 * (((double) (input.Height - i)) / ((double) input.Height))));
                }
            }
        }

        private static void SaveTgaRowRaw(Stream output, Surface input, ref TgaHeader header, int y)
        {
            for (int i = 0; i < input.Width; i++)
            {
                ColorBgra color = input[i, y];
                WriteColor(output, color, header.pixelDepth);
            }
        }

        private static void SaveTgaRowRle(Stream output, Surface input, ref TgaHeader header, int y)
        {
            TgaPacketStateMachine machine = new TgaPacketStateMachine(output, header.pixelDepth);
            for (int i = 0; i < input.Width; i++)
            {
                machine.Push(input[i, y]);
            }
            machine.Flush();
        }

        private static void WriteColor(Stream output, ColorBgra color, int bitDepth)
        {
            if (bitDepth != 0x18)
            {
                if (bitDepth != 0x20)
                {
                    return;
                }
            }
            else
            {
                int num = ((color.R * color.A) + (0xff * (0xff - color.A))) / 0xff;
                int num2 = ((color.G * color.A) + (0xff * (0xff - color.A))) / 0xff;
                int num3 = ((color.B * color.A) + (0xff * (0xff - color.A))) / 0xff;
                int num4 = (num3 + (num2 << 8)) + (num << 0x10);
                output.WriteUInt24(num4);
                return;
            }
            output.WriteUInt32(color.Bgra);
        }

        public enum PropertyNames
        {
            BitDepth,
            RleCompress
        }

        public enum TgaBitDepthUIChoices
        {
            AutoDetect,
            Bpp32,
            Bpp24
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TgaHeader
        {
            public byte idLength;
            public byte cmapType;
            public TgaFileType.TgaType imageType;
            public ushort cmapIndex;
            public ushort cmapLength;
            public byte cmapEntrySize;
            public ushort xOrigin;
            public ushort yOrigin;
            public ushort imageWidth;
            public ushort imageHeight;
            public byte pixelDepth;
            public byte imageDesc;
            public void Write(Stream output)
            {
                output.WriteByte(this.idLength);
                output.WriteByte(this.cmapType);
                output.WriteByte((byte) this.imageType);
                output.WriteUInt16(this.cmapIndex);
                output.WriteUInt16(this.cmapLength);
                output.WriteByte(this.cmapEntrySize);
                output.WriteUInt16(this.xOrigin);
                output.WriteUInt16(this.yOrigin);
                output.WriteUInt16(this.imageWidth);
                output.WriteUInt16(this.imageHeight);
                output.WriteByte(this.pixelDepth);
                output.WriteByte(this.imageDesc);
            }

            public TgaHeader(Stream input)
            {
                int num = input.ReadByte();
                if (num == -1)
                {
                    throw new EndOfStreamException();
                }
                this.idLength = (byte) num;
                num = input.ReadByte();
                if (num == -1)
                {
                    throw new EndOfStreamException();
                }
                this.cmapType = (byte) num;
                num = input.ReadByte();
                if (num == -1)
                {
                    throw new EndOfStreamException();
                }
                this.imageType = (TgaFileType.TgaType) ((byte) num);
                int num2 = input.ReadUInt16();
                if (num2 == -1)
                {
                    throw new EndOfStreamException();
                }
                this.cmapIndex = (ushort) num2;
                num2 = input.ReadUInt16();
                if (num2 == -1)
                {
                    throw new EndOfStreamException();
                }
                this.cmapLength = (ushort) num2;
                num = input.ReadByte();
                if (num == -1)
                {
                    throw new EndOfStreamException();
                }
                this.cmapEntrySize = (byte) num;
                num2 = input.ReadUInt16();
                if (num2 == -1)
                {
                    throw new EndOfStreamException();
                }
                this.xOrigin = (ushort) num2;
                num2 = input.ReadUInt16();
                if (num2 == -1)
                {
                    throw new EndOfStreamException();
                }
                this.yOrigin = (ushort) num2;
                num2 = input.ReadUInt16();
                if (num2 == -1)
                {
                    throw new EndOfStreamException();
                }
                this.imageWidth = (ushort) num2;
                num2 = input.ReadUInt16();
                if (num2 == -1)
                {
                    throw new EndOfStreamException();
                }
                this.imageHeight = (ushort) num2;
                num = input.ReadByte();
                if (num == -1)
                {
                    throw new EndOfStreamException();
                }
                this.pixelDepth = (byte) num;
                num = input.ReadByte();
                if (num == -1)
                {
                    throw new EndOfStreamException();
                }
                this.imageDesc = (byte) num;
            }
        }

        private class TgaPacketStateMachine
        {
            private int bitDepth;
            private Stream output;
            private ColorBgra[] packetColors = new ColorBgra[0x80];
            private int packetLength;
            private bool rlePacket;

            public TgaPacketStateMachine(Stream output, int bitDepth)
            {
                this.output = output;
                this.bitDepth = bitDepth;
            }

            public void Flush()
            {
                byte num = (byte) ((this.rlePacket ? 0x80 : 0) + ((byte) (this.packetLength - 1)));
                this.output.WriteByte(num);
                int num2 = this.rlePacket ? 1 : this.packetLength;
                for (int i = 0; i < num2; i++)
                {
                    TgaFileType.WriteColor(this.output, this.packetColors[i], this.bitDepth);
                }
                this.packetLength = 0;
            }

            public void Push(ColorBgra color)
            {
                if (this.packetLength == 0)
                {
                    this.rlePacket = false;
                    this.packetColors[0] = color;
                    this.packetLength = 1;
                }
                else if (this.packetLength == 1)
                {
                    this.rlePacket = color == this.packetColors[0];
                    this.packetColors[1] = color;
                    this.packetLength = 2;
                }
                else if (this.packetLength == this.packetColors.Length)
                {
                    this.Flush();
                    this.Push(color);
                }
                else if (((this.packetLength >= 2) && this.rlePacket) && (color != this.packetColors[this.packetLength - 1]))
                {
                    this.Flush();
                    this.Push(color);
                }
                else if (((this.packetLength >= 2) && this.rlePacket) && (color == this.packetColors[this.packetLength - 1]))
                {
                    this.packetLength++;
                    this.packetColors[this.packetLength - 1] = color;
                }
                else if (((this.packetLength >= 2) && !this.rlePacket) && (color != this.packetColors[this.packetLength - 1]))
                {
                    this.packetLength++;
                    this.packetColors[this.packetLength - 1] = color;
                }
                else if (((this.packetLength >= 2) && !this.rlePacket) && (color == this.packetColors[this.packetLength - 1]))
                {
                    this.packetLength--;
                    this.Flush();
                    this.Push(color);
                    this.Push(color);
                }
            }
        }

        private enum TgaType : byte
        {
            CompMap = 0x20,
            CompMap4 = 0x21,
            Map = 1,
            Mono = 3,
            Null = 0,
            Rgb = 2,
            RleMap = 9,
            RleMono = 11,
            RleRgb = 10
        }
    }
}

