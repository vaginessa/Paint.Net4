namespace PaintDotNet.Rendering
{
    using PaintDotNet;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Imaging;
    using System;

    internal sealed class TransformedNearestNeighborContentRenderer : CancellableMaskedRendererBgraBase
    {
        private ISurface<ColorBgra> content;
        private int contentHeight;
        private unsafe ColorBgra* contentScan0;
        private int contentStride;
        private int contentWidth;
        private const long fpCeilingInc = 0xffffffL;
        private const long fpFactor = 0x1000000L;
        private const int fpFactorLog2 = 0x18;
        private const long fpFracMask = 0xffffffL;
        private Matrix3x2Double invMatrix;
        private Matrix3x2Double matrix;
        private long srcOffsetDxDxFp;
        private long srcOffsetDxDyFp;
        private long srcOffsetDyDxFp;
        private long srcOffsetDyDyFp;
        private long srcOffsetOriginXFp;
        private long srcOffsetOriginYFp;

        public unsafe TransformedNearestNeighborContentRenderer(SizeInt32 size, ISurface<ColorBgra> content, Matrix3x2Double matrix) : base(size.Width, size.Height, true)
        {
            Validate.Begin().IsPositive(size.Width, "size.Width").IsPositive(size.Height, "size.Height").IsTrue(matrix.HasInverse, "matrix.HasInverse").Check();
            this.content = content;
            this.contentWidth = this.content.Width;
            this.contentHeight = this.content.Height;
            this.contentScan0 = (ColorBgra*) this.content.Scan0;
            this.contentStride = this.content.Stride;
            this.matrix = matrix;
            this.invMatrix = this.matrix.Inverse;
            PointDouble pt = new PointDouble(0.5, 0.5);
            PointDouble num2 = new PointDouble(0.5, 1.5);
            PointDouble num3 = new PointDouble(1.5, 0.5);
            PointDouble num4 = this.invMatrix.Transform(pt);
            PointDouble num5 = this.invMatrix.Transform(num2);
            PointDouble num6 = this.invMatrix.Transform(num3);
            PointDouble num7 = new PointDouble(num4.X, num4.Y);
            PointDouble num8 = new PointDouble(num5.X, num5.Y);
            PointDouble num9 = new PointDouble(num6.X, num6.Y);
            double num10 = num9.X - num7.X;
            double num11 = num9.Y - num7.Y;
            double num12 = num8.X - num7.X;
            double num13 = num8.Y - num7.Y;
            this.srcOffsetOriginXFp = DoubleUtil.ClampToInt64(num7.X * 16777216.0);
            this.srcOffsetOriginYFp = DoubleUtil.ClampToInt64(num7.Y * 16777216.0);
            this.srcOffsetDxDxFp = DoubleUtil.ClampToInt64(num10 * 16777216.0);
            this.srcOffsetDyDxFp = DoubleUtil.ClampToInt64(num11 * 16777216.0);
            this.srcOffsetDxDyFp = DoubleUtil.ClampToInt64(num12 * 16777216.0);
            this.srcOffsetDyDyFp = DoubleUtil.ClampToInt64(num13 * 16777216.0);
        }

        protected override unsafe void OnRender(ISurface<ColorBgra> dstContent, ISurface<ColorAlpha8> dstMask, PointInt32 renderOffset)
        {
            base.ThrowIfCancellationRequested();
            int width = dstContent.Width;
            int height = dstContent.Height;
            int stride = dstContent.Stride;
            int num4 = stride - (width * 4);
            int num6 = dstMask.Stride - (width * ColorAlpha8.SizeOf);
            long num7 = (this.srcOffsetOriginXFp + (renderOffset.X * this.srcOffsetDxDxFp)) + (renderOffset.Y * this.srcOffsetDxDyFp);
            long num8 = (this.srcOffsetOriginYFp + (renderOffset.X * this.srcOffsetDyDxFp)) + (renderOffset.Y * this.srcOffsetDyDyFp);
            uint* numPtr = (uint*) (((void*) dstContent.Scan0) + (width * 4));
            byte* numPtr2 = (byte*) dstMask.Scan0;
            for (int i = 0; i < height; i++)
            {
                int num12;
                base.ThrowIfCancellationRequested();
                long num10 = num7;
                long num11 = num8;
                uint* numPtr3 = numPtr - width;
                int num13 = (int) (num10 >> 0x18);
                int num14 = (int) (num11 >> 0x18);
                if (((num13 < 0) || (num14 < 0)) || ((num13 >= this.contentWidth) || (num14 >= this.contentHeight)))
                {
                    num12 = 0;
                }
                else
                {
                    int num15 = (int) ((num10 + (this.srcOffsetDxDxFp * width)) >> 0x18);
                    int num16 = (int) ((num11 + (this.srcOffsetDyDxFp * width)) >> 0x18);
                    if (((num15 < 0) || (num16 < 0)) || ((num15 >= this.contentWidth) || (num16 >= this.contentHeight)))
                    {
                        num12 = 0;
                    }
                    else
                    {
                        num12 = width;
                    }
                }
                while ((numPtr3 < numPtr) && (num12 > 0))
                {
                    int num17 = (int) (num10 >> 0x18);
                    int num18 = (int) (num11 >> 0x18);
                    numPtr3[0] = *((uint*) ((this.contentScan0 + (num18 * this.contentStride)) + (num17 * 4)));
                    numPtr2[0] = 0xff;
                    num10 += this.srcOffsetDxDxFp;
                    num11 += this.srcOffsetDyDxFp;
                    numPtr2++;
                    numPtr3++;
                    num12--;
                }
                while (numPtr3 < numPtr)
                {
                    int num19 = (int) (num10 >> 0x18);
                    int num20 = (int) (num11 >> 0x18);
                    if (((num19 < 0) || (num20 < 0)) || ((num19 >= this.contentWidth) || (num20 >= this.contentHeight)))
                    {
                        numPtr3[0] = 0;
                        numPtr2[0] = 0;
                    }
                    else
                    {
                        numPtr3[0] = *((uint*) ((this.contentScan0 + (num20 * this.contentStride)) + (num19 * 4)));
                        numPtr2[0] = 0xff;
                    }
                    num10 += this.srcOffsetDxDxFp;
                    num11 += this.srcOffsetDyDxFp;
                    numPtr2++;
                    numPtr3++;
                }
                num7 += this.srcOffsetDxDyFp;
                num8 += this.srcOffsetDyDyFp;
                numPtr += stride;
                numPtr2 += num6;
            }
        }
    }
}

