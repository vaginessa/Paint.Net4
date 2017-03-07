namespace PaintDotNet.Tools.Pencil
{
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Imaging;
    using PaintDotNet.Rendering;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    internal sealed class PencilToolContentRenderer : CancellableMaskedRendererBgraBase
    {
        private PencilToolChanges changes;

        public PencilToolContentRenderer(int width, int height, PencilToolChanges changes) : base(width, height, true)
        {
            Validate.IsNotNull<PencilToolChanges>(changes, "changes");
            this.changes = changes;
        }

        [IteratorStateMachine(typeof(<EnumerateLinePoints>d__2))]
        private static IEnumerable<PointInt32> EnumerateLinePoints(PointInt32 first, PointInt32 second)
        {
            int num8;
            int x = first.X;
            int y = first.Y;
            int num4 = second.X;
            int num5 = second.Y;
            int num6 = num4 - x;
            int num7 = num5 - y;
            this.<dxabs>5__3 = Math.Abs(num6);
            this.<dyabs>5__2 = Math.Abs(num7);
            this.<px>5__6 = x;
            this.<py>5__4 = y;
            this.<sdx>5__7 = Math.Sign(num6);
            this.<sdy>5__5 = Math.Sign(num7);
            this.<x>5__10 = 0;
            this.<y>5__1 = 0;
            if (this.<dxabs>5__3 <= this.<dyabs>5__2)
            {
                if (this.<dxabs>5__3 == this.<dyabs>5__2)
                {
                    this.<i>5__9 = 0;
                    while (this.<i>5__9 <= this.<dxabs>5__3)
                    {
                        yield return new PointInt32(this.<px>5__6, this.<py>5__4);
                        this.<px>5__6 += this.<sdx>5__7;
                        this.<py>5__4 += this.<sdy>5__5;
                        num8 = this.<i>5__9;
                        this.<i>5__9 = num8 + 1;
                    }
                }
                else
                {
                    this.<i>5__11 = 0;
                    while (this.<i>5__11 <= this.<dyabs>5__2)
                    {
                        this.<x>5__10 += this.<dxabs>5__3;
                        if (this.<x>5__10 >= this.<dyabs>5__2)
                        {
                            this.<x>5__10 -= this.<dyabs>5__2;
                            this.<px>5__6 += this.<sdx>5__7;
                        }
                        yield return new PointInt32(this.<px>5__6, this.<py>5__4);
                        this.<py>5__4 += this.<sdy>5__5;
                        num8 = this.<i>5__11;
                        this.<i>5__11 = num8 + 1;
                    }
                }
                goto Label_02B1;
            }
            this.<i>5__8 = 0;
        Label_PostSwitchInIterator:;
            if (this.<i>5__8 <= this.<dxabs>5__3)
            {
                this.<y>5__1 += this.<dyabs>5__2;
                if (this.<y>5__1 >= this.<dxabs>5__3)
                {
                    this.<y>5__1 -= this.<dxabs>5__3;
                    this.<py>5__4 += this.<sdy>5__5;
                }
                yield return new PointInt32(this.<px>5__6, this.<py>5__4);
                this.<px>5__6 += this.<sdx>5__7;
                num8 = this.<i>5__8;
                this.<i>5__8 = num8 + 1;
                goto Label_PostSwitchInIterator;
            }
        Label_02B1:;
        }

        protected override unsafe void OnRender(ISurface<ColorBgra> dstContent, ISurface<ColorAlpha8> dstMask, PointInt32 renderOffset)
        {
            base.ThrowIfCancellationRequested();
            dstContent.Clear(this.changes.Color);
            base.ThrowIfCancellationRequested();
            dstMask.Clear(ColorAlpha8.Transparent);
            IList<PointDouble> points = this.changes.Points;
            int count = points.Count;
            switch (count)
            {
                case 0:
                    return;

                case 1:
                {
                    PointDouble pt = points[0];
                    PointInt32 num4 = PointDouble.Truncate(pt).OffsetCopy(-renderOffset.X, -renderOffset.Y);
                    if (dstMask.CheckPointValue<ColorAlpha8>(num4.X, num4.Y))
                    {
                        ColorAlpha8* alphaPtr = (ColorAlpha8*) dstMask.GetPointPointer<ColorAlpha8>(num4.X, num4.Y);
                        alphaPtr.A = 0xff;
                        return;
                    }
                    break;
                }
                default:
                {
                    RectInt32 rect = new RectInt32(renderOffset, dstMask.Size<ColorAlpha8>());
                    for (int i = 0; i < (count - 1); i++)
                    {
                        PointDouble num7 = points[i];
                        PointInt32 a = PointDouble.Truncate(num7);
                        PointDouble num9 = points[i + 1];
                        PointInt32 b = PointDouble.Truncate(num9);
                        if (RectInt32Util.FromPixelPoints(a, b).IntersectsWith(rect))
                        {
                            foreach (PointInt32 num12 in EnumerateLinePoints(a, b))
                            {
                                PointInt32 num13 = num12.OffsetCopy(-renderOffset.X, -renderOffset.Y);
                                if (dstMask.CheckPointValue<ColorAlpha8>(num13.X, num13.Y))
                                {
                                    byte* numPtr = (byte*) dstMask.GetPointPointer<ColorAlpha8>(num13.X, num13.Y);
                                    numPtr[0] = 0xff;
                                }
                            }
                        }
                    }
                    break;
                }
            }
        }

    }
}

