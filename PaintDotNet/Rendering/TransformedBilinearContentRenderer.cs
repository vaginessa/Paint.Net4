namespace PaintDotNet.Rendering
{
    using PaintDotNet;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Direct2D;
    using PaintDotNet.Imaging;
    using PaintDotNet.UI.Media;
    using System;

    internal sealed class TransformedBilinearContentRenderer : CancellableMaskedRendererBgraBase
    {
        private ISurface<ColorBgra> content;
        private int contentHeight;
        private unsafe ColorBgra* contentScan0;
        private int contentStride;
        private int contentWidth;
        private const long fpCeilingInc64 = 0xffffffL;
        private const int fpFactor32 = 0x1000000;
        private const long fpFactor64 = 0x1000000L;
        private const int fpFactorLog2 = 0x18;
        private const long fpFracMask64 = 0xffffffL;
        private Matrix3x2Double invMatrix;
        private Matrix3x2Double matrix;
        private SizeInt32 size;
        private RectInt32 srcCoverageBounds;
        private long srcOffsetDxDxFp;
        private long srcOffsetDxDyFp;
        private long srcOffsetDyDxFp;
        private long srcOffsetDyDyFp;
        private long srcOffsetOriginXFp;
        private long srcOffsetOriginYFp;
        private static readonly SolidColorBrush whiteBrush = SolidColorBrushCache.Get((ColorRgba128Float) Colors.White);

        public unsafe TransformedBilinearContentRenderer(SizeInt32 size, ISurface<ColorBgra> content, RectInt32 srcCoverageBounds, Matrix3x2Double matrix) : base(size.Width, size.Height, true)
        {
            Validate.Begin().IsPositive(size.Width, "size.Width").IsPositive(size.Height, "size.Height").IsTrue(matrix.HasInverse, "matrix.HasInverse").Check();
            this.size = size;
            this.content = content;
            this.srcCoverageBounds = srcCoverageBounds;
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
            PointDouble num7 = new PointDouble(num4.X - 0.5, num4.Y - 0.5);
            PointDouble num8 = new PointDouble(num5.X - 0.5, num5.Y - 0.5);
            PointDouble num9 = new PointDouble(num6.X - 0.5, num6.Y - 0.5);
            double num10 = num7.X * 16777216.0;
            double num11 = num7.Y * 16777216.0;
            this.srcOffsetOriginXFp = DoubleUtil.ClampToInt64(num10);
            this.srcOffsetOriginYFp = DoubleUtil.ClampToInt64(num11);
            double num12 = num9.X - num7.X;
            double num13 = num9.Y - num7.Y;
            double num14 = num8.X - num7.X;
            double num15 = num8.Y - num7.Y;
            double num16 = num12 * 16777216.0;
            double num17 = num13 * 16777216.0;
            double num18 = num14 * 16777216.0;
            double num19 = num15 * 16777216.0;
            this.srcOffsetDxDxFp = DoubleUtil.ClampToInt64(num16);
            this.srcOffsetDyDxFp = DoubleUtil.ClampToInt64(num17);
            this.srcOffsetDxDyFp = DoubleUtil.ClampToInt64(num18);
            this.srcOffsetDyDyFp = DoubleUtil.ClampToInt64(num19);
        }

        protected override void OnRender(ISurface<ColorBgra> dstContent, ISurface<ColorAlpha8> dstMask, PointInt32 renderOffset)
        {
            base.ThrowIfCancellationRequested();
            this.RenderContent(dstContent, renderOffset);
            base.ThrowIfCancellationRequested();
            this.RenderMask(dstMask, renderOffset);
            base.ThrowIfCancellationRequested();
        }

        private unsafe void RenderContent(ISurface<ColorBgra> dstContent, PointInt32 renderOffset)
        {
            int width = dstContent.Width;
            int height = dstContent.Height;
            int stride = dstContent.Stride;
            int num4 = stride - (width * 4);
            RectInt32 num5 = RectInt32.Intersect(this.srcCoverageBounds, new RectInt32(0, 0, this.contentWidth, this.contentHeight));
            int left = num5.Left;
            int top = num5.Top;
            int right = num5.Right;
            int bottom = num5.Bottom;
            long num10 = (this.srcOffsetOriginXFp + (renderOffset.X * this.srcOffsetDxDxFp)) + (renderOffset.Y * this.srcOffsetDxDyFp);
            long num11 = (this.srcOffsetOriginYFp + (renderOffset.X * this.srcOffsetDyDxFp)) + (renderOffset.Y * this.srcOffsetDyDyFp);
            ColorBgra* bgraPtr = this.contentScan0;
            int contentStride = this.contentStride;
            ColorBgra* bgraPtr2 = (ColorBgra*) dstContent.Scan0;
            ColorBgra* bgraPtr3 = bgraPtr2 + width;
            for (int i = 0; i < height; i++)
            {
                uint num16;
                int num17;
                base.ThrowIfCancellationRequested();
                long num14 = num10;
                long num15 = num11;
                ColorBgra* bgraPtr4 = bgraPtr3 - width;
                int num18 = (int) ((long) ((bgraPtr3 - bgraPtr4) / sizeof(ColorBgra)));
                int num19 = (int) (num14 >> 0x18);
                int num20 = (int) (num15 >> 0x18);
                int num21 = ((num19 >= left) && (num19 < right)) ? 1 : 0;
                int num22 = ((num20 >= top) && (num20 < bottom)) ? 1 : 0;
                int num23 = ((num19 >= (left - 1)) && (num19 < (right - 1))) ? 1 : 0;
                int num24 = ((num20 >= (top - 1)) && (num20 < (bottom - 1))) ? 1 : 0;
                long num25 = num14 + (this.srcOffsetDxDxFp * num18);
                long num26 = num15 + (this.srcOffsetDyDxFp * num18);
                int num27 = (int) (num25 >> 0x18);
                int num28 = (int) (num26 >> 0x18);
                int num29 = ((num27 >= left) && (num27 < right)) ? 1 : 0;
                int num30 = ((num28 >= top) && (num28 < bottom)) ? 1 : 0;
                int num31 = ((num27 >= (left - 1)) && (num27 < (right - 1))) ? 1 : 0;
                int num32 = ((num28 >= (top - 1)) && (num28 < (bottom - 1))) ? 1 : 0;
                if ((((((((num21 + num22) + num23) + num24) + num29) + num30) + num31) + num32) == 8)
                {
                    num17 = num18;
                }
                else
                {
                    num17 = 0;
                }
                while ((bgraPtr4 < bgraPtr3) && (num17 > 0))
                {
                    num16 = 0;
                    int num33 = (int) (num14 >> 0x18);
                    int num34 = (int) (num15 >> 0x18);
                    int num35 = 0x100 - (((int) (num14 & 0xffffffL)) >> 0x10);
                    int num36 = 0x100 - (((int) (num15 & 0xffffffL)) >> 0x10);
                    int num37 = (num35 * num36) >> 8;
                    int num38 = ((0x100 - num35) * num36) >> 8;
                    int num39 = (num35 * (0x100 - num36)) >> 8;
                    int num40 = ((0x100 - num35) * (0x100 - num36)) >> 8;
                    int num41 = num37;
                    int num42 = num38;
                    int num43 = num39;
                    int num44 = num40;
                    ushort d = (ushort) (((num41 + num42) + num43) + num44);
                    long num46 = (num34 * contentStride) + (num33 * 4);
                    ColorBgra32 bgra = *((ColorBgra32*) (bgraPtr + num46));
                    ColorBgra32 bgra2 = *((ColorBgra32*) (bgraPtr + (num46 + 4L)));
                    ColorBgra32 bgra3 = *((ColorBgra32*) (bgraPtr + (num46 + contentStride)));
                    ColorBgra32 bgra4 = *((ColorBgra32*) (bgraPtr + ((num46 + contentStride) + 4L)));
                    int num47 = bgra.A * num41;
                    int num48 = bgra2.A * num42;
                    int num49 = bgra3.A * num43;
                    int num50 = bgra4.A * num44;
                    ushort n = (ushort) (((num47 + num48) + num49) + num50);
                    if (n != 0)
                    {
                        uint num52 = UInt32Util.FastDivideByUInt16(n, d);
                        uint num53 = UInt32Util.FastDivideByUInt16((uint) ((((num47 * bgra.B) + (num48 * bgra2.B)) + (num49 * bgra3.B)) + (num50 * bgra4.B)), n);
                        uint num54 = UInt32Util.FastDivideByUInt16((uint) ((((num47 * bgra.G) + (num48 * bgra2.G)) + (num49 * bgra3.G)) + (num50 * bgra4.G)), n);
                        uint num55 = UInt32Util.FastDivideByUInt16((uint) ((((num47 * bgra.R) + (num48 * bgra2.R)) + (num49 * bgra3.R)) + (num50 * bgra4.R)), n);
                        num16 = ColorBgra.BgraToUInt32((byte) num53, (byte) num54, (byte) num55, (byte) num52);
                    }
                    bgraPtr4->Bgra = num16;
                    num14 += this.srcOffsetDxDxFp;
                    num15 += this.srcOffsetDyDxFp;
                    bgraPtr4++;
                    num17--;
                }
                while (bgraPtr4 < bgraPtr3)
                {
                    num16 = 0;
                    int num56 = (int) (num14 >> 0x18);
                    int num57 = (int) (num15 >> 0x18);
                    int num58 = ((num56 >= left) && (num56 < right)) ? 1 : 0;
                    int num59 = ((num57 >= top) && (num57 < bottom)) ? 1 : 0;
                    int num60 = ((num56 >= (left - 1)) && (num56 < (right - 1))) ? 1 : 0;
                    int num61 = ((num57 >= (top - 1)) && (num57 < (bottom - 1))) ? 1 : 0;
                    int num62 = num59 * num58;
                    int num63 = num59 * num60;
                    int num64 = num61 * num58;
                    int num65 = num61 * num60;
                    if ((((num62 + num63) + num64) + num65) != 0)
                    {
                        int num67 = 0x100 - (((int) (num14 & 0xffffffL)) >> 0x10);
                        int num68 = 0x100 - (((int) (num15 & 0xffffffL)) >> 0x10);
                        int num69 = (num67 * num68) >> 8;
                        int num70 = ((0x100 - num67) * num68) >> 8;
                        int num71 = (num67 * (0x100 - num68)) >> 8;
                        int num72 = ((0x100 - num67) * (0x100 - num68)) >> 8;
                        int num73 = num62 * num69;
                        int num74 = num63 * num70;
                        int num75 = num64 * num71;
                        int num76 = num65 * num72;
                        ushort num77 = (ushort) (((num73 + num74) + num75) + num76);
                        long num78 = (num57 * contentStride) + (num56 * 4);
                        ColorBgra32 bgra5 = *((ColorBgra32*) (bgraPtr + (num78 * num62)));
                        ColorBgra32 bgra6 = *((ColorBgra32*) (bgraPtr + ((num78 + 4L) * num63)));
                        ColorBgra32 bgra7 = *((ColorBgra32*) (bgraPtr + ((num78 + contentStride) * num64)));
                        ColorBgra32 bgra8 = *((ColorBgra32*) (bgraPtr + (((num78 + contentStride) + 4L) * num65)));
                        int num79 = bgra5.A * num73;
                        int num80 = bgra6.A * num74;
                        int num81 = bgra7.A * num75;
                        int num82 = bgra8.A * num76;
                        ushort num83 = (ushort) (((num79 + num80) + num81) + num82);
                        if (num83 != 0)
                        {
                            uint num84 = UInt32Util.FastDivideByUInt16(num83, num77);
                            uint num85 = UInt32Util.FastDivideByUInt16((uint) ((((num79 * bgra5.B) + (num80 * bgra6.B)) + (num81 * bgra7.B)) + (num82 * bgra8.B)), num83);
                            uint num86 = UInt32Util.FastDivideByUInt16((uint) ((((num79 * bgra5.G) + (num80 * bgra6.G)) + (num81 * bgra7.G)) + (num82 * bgra8.G)), num83);
                            uint num87 = UInt32Util.FastDivideByUInt16((uint) ((((num79 * bgra5.R) + (num80 * bgra6.R)) + (num81 * bgra7.R)) + (num82 * bgra8.R)), num83);
                            num16 = ColorBgra.BgraToUInt32((byte) num85, (byte) num86, (byte) num87, (byte) num84);
                        }
                    }
                    bgraPtr4->Bgra = num16;
                    num14 += this.srcOffsetDxDxFp;
                    num15 += this.srcOffsetDyDxFp;
                    bgraPtr4++;
                }
                num10 += this.srcOffsetDxDyFp;
                num11 += this.srcOffsetDyDyFp;
                bgraPtr3 += stride;
            }
        }

        private void RenderMask(ISurface<ColorAlpha8> dstMask, PointInt32 renderOffset)
        {
            int width = dstMask.Width;
            int height = dstMask.Height;
            bool flag = false;
            SizeInt32 size = new SizeInt32(width, height);
            RectInt32 rectangle = new RectInt32(renderOffset, size);
            if (!flag && this.matrix.HasInverse)
            {
                Matrix3x2Double inverse = this.matrix.Inverse;
                PointDouble pt = inverse.Transform(rectangle.TopLeft);
                PointDouble num7 = inverse.Transform(rectangle.TopRight);
                PointDouble num8 = inverse.Transform(rectangle.BottomLeft);
                PointDouble num9 = inverse.Transform(rectangle.BottomRight);
                RectDouble srcCoverageBounds = this.srcCoverageBounds;
                if ((srcCoverageBounds.Contains(pt) && srcCoverageBounds.Contains(num7)) && (srcCoverageBounds.Contains(num8) && srcCoverageBounds.Contains(num9)))
                {
                    dstMask.Clear(ColorAlpha8.Opaque);
                    flag = true;
                }
            }
            if (!flag)
            {
                GeometryRelation relation;
                IDirect2DFactory perThread = Direct2DFactory.PerThread;
                using (IRectangleGeometry geometry = perThread.CreateRectangleGeometry(rectangle))
                {
                    base.ThrowIfCancellationRequested();
                    using (IRectangleGeometry geometry2 = perThread.CreateRectangleGeometry(this.srcCoverageBounds))
                    {
                        base.ThrowIfCancellationRequested();
                        relation = geometry.CompareWithGeometry(geometry2, new Matrix3x2Float?((Matrix3x2Float) this.matrix), null);
                        base.ThrowIfCancellationRequested();
                    }
                }
                switch (relation)
                {
                    case GeometryRelation.IsContained:
                        dstMask.Clear(ColorAlpha8.Opaque);
                        flag = true;
                        break;

                    case GeometryRelation.Disjoint:
                        dstMask.Clear(ColorAlpha8.Transparent);
                        flag = true;
                        break;
                }
            }
            if (!flag)
            {
                using (IDrawingContext context = DrawingContext.FromSurface(dstMask, FactorySource.PerThread))
                {
                    base.ThrowIfCancellationRequested();
                    context.Clear(null);
                    context.AntialiasMode = AntialiasMode.PerPrimitive;
                    using (context.UseTranslateTransform((float) -renderOffset.X, (float) -renderOffset.Y, MatrixMultiplyOrder.Prepend))
                    {
                        using (context.UseTransformMultiply((Matrix3x2Float) this.matrix, MatrixMultiplyOrder.Prepend))
                        {
                            context.FillRectangle(this.srcCoverageBounds, whiteBrush);
                        }
                    }
                }
            }
        }
    }
}

