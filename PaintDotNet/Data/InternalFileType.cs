namespace PaintDotNet.Data
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.IO;
    using PaintDotNet.Rendering;
    using PaintDotNet.Threading;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;

    internal abstract class InternalFileType : PropertyBasedFileType
    {
        internal InternalFileType(string name, FileTypeFlags flags, string[] extensions) : base(name, flags, extensions)
        {
        }

        private unsafe void Analyze(Surface scratchSurface, out bool allOpaque, out bool all0or255Alpha, out int uniqueColorCount)
        {
            allOpaque = true;
            all0or255Alpha = true;
            HashSet<ColorBgra> set = new HashSet<ColorBgra>();
            for (int i = 0; i < scratchSurface.Height; i++)
            {
                ColorBgra* rowAddress = scratchSurface.GetRowAddress(i);
                ColorBgra* bgraPtr2 = rowAddress + scratchSurface.Width;
                while (rowAddress < bgraPtr2)
                {
                    ColorBgra item = rowAddress[0];
                    if (item.A != 0xff)
                    {
                        allOpaque = false;
                    }
                    if ((item.A > 0) && (item.A < 0xff))
                    {
                        all0or255Alpha = false;
                    }
                    if (((item.A == 0xff) && !set.Contains(item)) && (set.Count < 300))
                    {
                        set.Add(rowAddress[0]);
                    }
                    rowAddress++;
                }
            }
            uniqueColorCount = set.Count;
        }

        internal SavableBitDepths ChooseBitDepth(Document input, Surface scratchSurface, int ditherLevel, int threshold, PropertyBasedSaveConfigToken token, ProgressEventHandler progressCallback, double progressStart, double progressEnd, HashSet<SavableBitDepths> allowedBitDepths, HashSet<SavableBitDepths> losslessBitDepths, bool allOpaque, bool all0Or255Alpha, int uniqueColorCount)
        {
            if (allowedBitDepths.Count == 0)
            {
                throw new ArgumentException("Count must be 1 or more", "allowedBitDepths");
            }
            try
            {
                HashSet<SavableBitDepths> set2;
                if (allowedBitDepths.Count == 1)
                {
                    return allowedBitDepths.First<SavableBitDepths>();
                }
                HashSet<SavableBitDepths> source = HashSetUtil.Intersect<SavableBitDepths>(allowedBitDepths, losslessBitDepths);
                if (source.Count == 1)
                {
                    return source.First<SavableBitDepths>();
                }
                if (source.Count == 0)
                {
                    set2 = allowedBitDepths;
                }
                else
                {
                    set2 = source;
                }
                long num = input.Width * input.Height;
                if (((all0Or255Alpha && (uniqueColorCount <= 0xff)) && ((num <= 0x10000L) && set2.Contains(SavableBitDepths.Rgb8))) && set2.Contains(SavableBitDepths.Rgb24))
                {
                    long num2 = 0L;
                    long num3 = 0L;
                    try
                    {
                        num2 = this.GetSavableBitDepthFileLength(SavableBitDepths.Rgb8, input, scratchSurface, ditherLevel, threshold, token, progressCallback, 0.0, 50.0);
                        num3 = this.GetSavableBitDepthFileLength(SavableBitDepths.Rgb24, input, scratchSurface, ditherLevel, threshold, token, progressCallback, 50.0, 100.0);
                    }
                    catch (OutOfMemoryException)
                    {
                        return SavableBitDepths.Rgb8;
                    }
                    if (num2 < num3)
                    {
                        return SavableBitDepths.Rgb8;
                    }
                    return SavableBitDepths.Rgb24;
                }
                if (((all0Or255Alpha && (uniqueColorCount <= 0xff)) && ((num < 0x10000L) && set2.Contains(SavableBitDepths.Rgba8))) && set2.Contains(SavableBitDepths.Rgba32))
                {
                    long num4 = 0L;
                    long num5 = 0L;
                    try
                    {
                        num4 = this.GetSavableBitDepthFileLength(SavableBitDepths.Rgba8, input, scratchSurface, ditherLevel, threshold, token, progressCallback, 0.0, 50.0);
                        num5 = this.GetSavableBitDepthFileLength(SavableBitDepths.Rgba32, input, scratchSurface, ditherLevel, threshold, token, progressCallback, 50.0, 100.0);
                    }
                    catch (OutOfMemoryException)
                    {
                        return SavableBitDepths.Rgba8;
                    }
                    if (num4 < num5)
                    {
                        return SavableBitDepths.Rgba8;
                    }
                    return SavableBitDepths.Rgba32;
                }
                if ((set2.Contains(SavableBitDepths.Rgb8) & allOpaque) && (uniqueColorCount <= 0x100))
                {
                    return SavableBitDepths.Rgb8;
                }
                if ((set2.Contains(SavableBitDepths.Rgba8) & all0Or255Alpha) && (uniqueColorCount <= 0xff))
                {
                    return SavableBitDepths.Rgba8;
                }
                if (!(set2.Contains(SavableBitDepths.Rgb24) & allOpaque))
                {
                    if (set2.Contains(SavableBitDepths.Rgba32))
                    {
                        return SavableBitDepths.Rgba32;
                    }
                    SavableBitDepths[] items = new SavableBitDepths[] { SavableBitDepths.Rgb8, SavableBitDepths.Rgb24 };
                    if (set2.SetEquals(HashSetUtil.Create<SavableBitDepths>(items)))
                    {
                        return SavableBitDepths.Rgb24;
                    }
                    SavableBitDepths[] depthsArray2 = new SavableBitDepths[] { SavableBitDepths.Rgb8, SavableBitDepths.Rgba8 };
                    if (set2.SetEquals(HashSetUtil.Create<SavableBitDepths>(depthsArray2)))
                    {
                        return SavableBitDepths.Rgba8;
                    }
                    SavableBitDepths[] depthsArray3 = new SavableBitDepths[] { SavableBitDepths.Rgba8, SavableBitDepths.Rgb24 };
                    if (!set2.SetEquals(HashSetUtil.Create<SavableBitDepths>(depthsArray3)))
                    {
                        throw new ArgumentException("Could not accomodate input values -- internal error?");
                    }
                }
                return SavableBitDepths.Rgb24;
            }
            finally
            {
            }
        }

        protected unsafe Bitmap CreateAliased24BppBitmap(Surface surface)
        {
            int num = surface.Width * 3;
            return new Bitmap(surface.Width, surface.Height, ((num + 3) / 4) * 4, PixelFormat.Format24bppRgb, new IntPtr(surface.Scan0.VoidStar));
        }

        internal abstract HashSet<SavableBitDepths> CreateAllowedBitDepthListFromToken(PropertyBasedSaveConfigToken token);
        private unsafe void FinalSave(Document input, Stream output, Surface scratchSurface, int ditherLevel, int threshold, SavableBitDepths bitDepth, PropertyBasedSaveConfigToken token, ProgressEventHandler progressCallback, double progressStart, double progressEnd)
        {
            this.RenderFlattenedDocument(input, scratchSurface);
            if (((bitDepth == SavableBitDepths.Rgb8) || (bitDepth == SavableBitDepths.Rgba8)) || (bitDepth == SavableBitDepths.Rgb24))
            {
                ColorBgra white = ColorBgra.White;
                Work.ParallelFor(WaitType.Blocking, 0, scratchSurface.Height, delegate (int y) {
                    int width = scratchSurface.Width;
                    ColorBgra* rowAddress = scratchSurface.GetRowAddress(y);
                    for (int j = 0; j < width; j++)
                    {
                        ColorBgra rhs = rowAddress[0];
                        if ((bitDepth == SavableBitDepths.Rgba8) && (rhs.A < threshold))
                        {
                            rowAddress->Bgra = 0;
                        }
                        else
                        {
                            rowAddress->Bgra = CompositionOps.Normal.ApplyStatic(white, rhs).Bgra;
                        }
                        rowAddress++;
                    }
                }, WorkItemQueuePriority.Normal, null);
            }
            ProgressEventHandler handler = (sender, e) => progressCallback(sender, new ProgressEventArgs(progressStart + ((progressEnd - progressStart) * (e.Percent / 100.0))));
            this.OnFinalSave(input, output, scratchSurface, ditherLevel, bitDepth, token, handler);
        }

        internal abstract int GetDitherLevelFromToken(PropertyBasedSaveConfigToken token);
        private long GetSavableBitDepthFileLength(SavableBitDepths bitDepth, Document input, Surface scratchSurface, int ditherLevel, int threshold, PropertyBasedSaveConfigToken token, ProgressEventHandler progressCallback, double progressStart, double progressEnd)
        {
            scratchSurface.Clear(ColorBgra.Zero);
            using (SegmentedMemoryStream stream = new SegmentedMemoryStream())
            {
                this.FinalSave(input, stream, scratchSurface, ditherLevel, threshold, bitDepth, token, progressCallback, progressStart, progressEnd);
                stream.Flush();
                return stream.Length;
            }
        }

        internal abstract int GetThresholdFromToken(PropertyBasedSaveConfigToken token);
        internal abstract void OnFinalSave(Document input, Stream output, Surface flattenedImage, int ditherLevel, SavableBitDepths bitDepth, PropertyBasedSaveConfigToken token, ProgressEventHandler progressCallback);
        protected sealed override void OnSaveT(Document input, Stream output, PropertyBasedSaveConfigToken token, Surface scratchSurface, ProgressEventHandler progressCallback)
        {
            try
            {
                int num3;
                bool flag;
                bool flag2;
                int num4;
                int thresholdFromToken = this.GetThresholdFromToken(token);
                int ditherLevelFromToken = this.GetDitherLevelFromToken(token);
                HashSet<SavableBitDepths> allowedBitDepths = this.CreateAllowedBitDepthListFromToken(token);
                if (allowedBitDepths.Count == 0)
                {
                    throw new ArgumentException("there must be at least 1 element returned from CreateAllowedBitDepthListFromToken()");
                }
                SavableBitDepths[] items = new SavableBitDepths[] { SavableBitDepths.Rgb8, SavableBitDepths.Rgba8 };
                if (allowedBitDepths.IsSubsetOf(HashSetUtil.Create<SavableBitDepths>(items)))
                {
                    num3 = thresholdFromToken;
                }
                else
                {
                    num3 = 1;
                }
                this.RenderFlattenedDocument(input, scratchSurface);
                this.Analyze(scratchSurface, out flag, out flag2, out num4);
                HashSet<SavableBitDepths> losslessBitDepths = new HashSet<SavableBitDepths> {
                    SavableBitDepths.Rgba32
                };
                if (flag)
                {
                    losslessBitDepths.Add(SavableBitDepths.Rgb24);
                    if (num4 <= 0x100)
                    {
                        losslessBitDepths.Add(SavableBitDepths.Rgb8);
                    }
                }
                else if (flag2 && (num4 < 0x100))
                {
                    losslessBitDepths.Add(SavableBitDepths.Rgba8);
                }
                double chooseBitDepthProgressLast = 0.0;
                ProgressEventHandler handler = delegate (object sender, ProgressEventArgs e) {
                    chooseBitDepthProgressLast = e.Percent;
                    progressCallback(sender, e);
                };
                SavableBitDepths bitDepth = this.ChooseBitDepth(input, scratchSurface, ditherLevelFromToken, num3, token, handler, 0.0, 66.666666666666671, allowedBitDepths, losslessBitDepths, flag, flag2, num4);
                if (((bitDepth == SavableBitDepths.Rgba8) && (num3 == 0)) && (allowedBitDepths.Contains(SavableBitDepths.Rgba8) && allowedBitDepths.Contains(SavableBitDepths.Rgb8)))
                {
                    bitDepth = SavableBitDepths.Rgb8;
                }
                this.FinalSave(input, output, scratchSurface, ditherLevelFromToken, num3, bitDepth, token, progressCallback, chooseBitDepthProgressLast, 100.0);
            }
            finally
            {
            }
        }

        private string PrintSet<T>(HashSet<T> set)
        {
            StringBuilder builder = new StringBuilder();
            bool flag = true;
            foreach (T local in set)
            {
                if (!flag)
                {
                    builder.Append(", ");
                }
                flag = false;
                builder.Append(local.ToString());
            }
            return builder.ToString();
        }

        private void RenderFlattenedDocument(Document input, ISurface<ColorBgra> dst)
        {
            dst.Clear(ColorBgra.Zero);
            input.CreateRenderer().Parallelize<ColorBgra>(TilingStrategy.HorizontalSlices, 7, WorkItemQueuePriority.Normal).Render<ColorBgra>(dst);
        }

        protected unsafe void SquishSurfaceTo24Bpp(Surface surface)
        {
            byte* rowAddress = (byte*) surface.GetRowAddress(0);
            int num = surface.Width * 3;
            int num2 = ((num + 3) / 4) * 4;
            int num3 = num2 - num;
            for (int i = 0; i < surface.Height; i++)
            {
                ColorBgra* bgraPtr = surface.GetRowAddress(i);
                ColorBgra* bgraPtr2 = bgraPtr + surface.Width;
                while (bgraPtr < bgraPtr2)
                {
                    rowAddress[0] = bgraPtr->B;
                    rowAddress[1] = bgraPtr->G;
                    rowAddress[2] = bgraPtr->R;
                    bgraPtr++;
                    rowAddress += 3;
                }
                rowAddress += num3;
            }
        }

        internal enum SavableBitDepths
        {
            Rgba32,
            Rgb24,
            Rgb8,
            Rgba8
        }
    }
}

