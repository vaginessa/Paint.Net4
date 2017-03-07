namespace PaintDotNet.Tools.Shapes
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Direct2D;
    using PaintDotNet.Imaging;
    using PaintDotNet.ObjectModel;
    using PaintDotNet.Rendering;
    using PaintDotNet.UI.Media;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal sealed class ShapesToolContentLayerRenderer : CancellableMaskedRendererBgraBase, IPreparableRenderer
    {
        private readonly ShapesToolChanges changes;
        private readonly PdnLegacyBrush contentBrush;
        private readonly IRenderer<ColorBgra> contentRenderer;
        private readonly ShapesToolContentLayer layer;
        private int nextPrepareStage;
        private bool[] penTracingMask;
        private static readonly SolidColorBrush whiteBrush = SolidColorBrushCache.Get((ColorRgba128Float) Colors.White);

        public ShapesToolContentLayerRenderer(int width, int height, ShapesToolContentLayer layer, ShapesToolChanges changes) : base(width, height, true)
        {
            ColorBgra outlineForegroundColor;
            ColorBgra outlineBackgroundColor;
            Validate.IsNotNull<ShapesToolChanges>(changes, "changes");
            this.changes = changes;
            this.layer = layer;
            ShapesToolContentLayer layer2 = this.layer;
            if (layer2 != ShapesToolContentLayer.Interior)
            {
                if (layer2 != ShapesToolContentLayer.Outline)
                {
                    throw ExceptionUtil.InvalidEnumArgumentException<ShapesToolContentLayer>(this.layer, "this.layer");
                }
            }
            else
            {
                outlineForegroundColor = this.changes.InteriorForegroundColor;
                outlineBackgroundColor = this.changes.InteriorBackgroundColor;
                goto Label_007A;
            }
            outlineForegroundColor = this.changes.OutlineForegroundColor;
            outlineBackgroundColor = this.changes.OutlineBackgroundColor;
        Label_007A:
            this.contentBrush = new PdnLegacyBrush(this.changes.BrushType, this.changes.HatchStyle, outlineForegroundColor, outlineBackgroundColor);
            this.contentRenderer = this.contentBrush.CreateRenderer(width, height);
        }

        private bool CanCustomRenderPen(Pen pen)
        {
            if (!pen.DashStyle.IsSolid)
            {
                if ((pen.DashCap != PenLineCap.Flat) && (pen.DashCap != PenLineCap.Square))
                {
                    return false;
                }
                if (pen.DashStyle.Offset != ((int) pen.DashStyle.Offset))
                {
                    return false;
                }
                foreach (double num in pen.DashStyle.Dashes)
                {
                    if (num != ((int) num))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        [IteratorStateMachine(typeof(<EnumeratePolyLinePixels>d__14))]
        private static IEnumerable<PointInt32> EnumeratePolyLinePixels(IEnumerable<PointDouble> points) => 
            new <EnumeratePolyLinePixels>d__14(-2) { <>3__points = points };

        [IteratorStateMachine(typeof(<GetTracingDashesForPenWithoutOffset>d__13))]
        private IEnumerable<int> GetTracingDashesForPenWithoutOffset(Pen pen)
        {
            this.<dashStyle>5__1 = pen.DashStyle;
            if (!this.<dashStyle>5__1.IsSolid)
            {
                if (pen.DashCap == PenLineCap.Square)
                {
                    this.<dashLengthOffset>5__3 = 1;
                    this.<gapLengthOffset>5__4 = -1;
                }
                else
                {
                    this.<dashLengthOffset>5__3 = 0;
                    this.<gapLengthOffset>5__4 = 0;
                }
                this.<isDash>5__5 = true;
                this.<dashes>5__2 = this.<dashStyle>5__1.Dashes;
                this.<dashesCount>5__7 = this.<dashes>5__2.Count;
                this.<loops>5__9 = ((this.<dashesCount>5__7 & 1) == 1) ? 2 : 1;
                this.<loop>5__8 = 0;
                while (this.<loop>5__8 < this.<loops>5__9)
                {
                    int num4;
                    this.<i>5__6 = 0;
                    while (this.<i>5__6 < this.<dashesCount>5__7)
                    {
                        double num2 = this.<dashes>5__2[this.<i>5__6];
                        int num3 = (int) num2;
                        yield return (this.<isDash>5__5 ? (num3 + this.<dashLengthOffset>5__3) : (num3 + this.<gapLengthOffset>5__4));
                        this.<isDash>5__5 = !this.<isDash>5__5;
                        num4 = this.<i>5__6 + 1;
                        this.<i>5__6 = num4;
                    }
                    num4 = this.<loop>5__8 + 1;
                    this.<loop>5__8 = num4;
                }
            }
            yield return 1;
        }

        [IteratorStateMachine(typeof(<GetTracingMaskForPenWithoutOffset>d__12))]
        private IEnumerable<bool> GetTracingMaskForPenWithoutOffset(Pen pen) => 
            new <GetTracingMaskForPenWithoutOffset>d__12(-2) { 
                <>4__this = this,
                <>3__pen = pen
            };

        private bool MustUseCustom1pxAliasedOutlineRenderer() => 
            ((((((!this.changes.Antialiasing && (this.layer == ShapesToolContentLayer.Outline)) && (this.changes.ShapeRenderData.OutlineDraw != null)) && ((this.changes.PenEndCap == LineCap2.Flat) || (this.changes.PenEndCap == LineCap2.Rounded))) && ((this.changes.PenStartCap == LineCap2.Flat) || (this.changes.PenStartCap == LineCap2.Rounded))) && (this.changes.OutlinePen.Thickness == 1.0)) && this.CanCustomRenderPen(this.changes.OutlinePen));

        protected override unsafe void OnRender(ISurface<ColorBgra> dstContent, ISurface<ColorAlpha8> dstMask, PointInt32 renderOffset)
        {
            RectInt32 num = new RectInt32(renderOffset, dstMask.Size<ColorAlpha8>());
            if (this.MustUseCustom1pxAliasedOutlineRenderer())
            {
                bool[] penTracingMask = this.penTracingMask;
                if (penTracingMask == null)
                {
                    penTracingMask = this.GetTracingMaskForPenWithoutOffset(this.changes.OutlinePen).ToArrayEx<bool>();
                    this.penTracingMask = penTracingMask;
                }
                dstMask.Clear(ColorAlpha8.Transparent);
                base.ThrowIfCancellationRequested();
                IList<PointDouble[]> flattenedPolyPoly = this.changes.ShapeRenderData.OutlineDraw.GetFlattenedPolyPoly();
                base.ThrowIfCancellationRequested();
                int num2 = 0;
                byte* numPtr = (byte*) dstMask.Scan0;
                int stride = dstMask.Stride;
                int offset = (int) this.changes.OutlinePen.DashStyle.Offset;
                foreach (IEnumerable<PointDouble> enumerable in flattenedPolyPoly)
                {
                    foreach (PointInt32 num5 in EnumeratePolyLinePixels(enumerable))
                    {
                        if (penTracingMask[offset] && num.Contains(num5))
                        {
                            PointInt32 num6 = new PointInt32(num5.X - renderOffset.X, num5.Y - renderOffset.Y);
                            (numPtr + (num6.Y * stride))[num6.X] = 0xff;
                        }
                        offset++;
                        if (offset == penTracingMask.Length)
                        {
                            offset = 0;
                        }
                        num2++;
                        if ((num2 & 0x3ff) == 0)
                        {
                            base.ThrowIfCancellationRequested();
                        }
                    }
                }
            }
            else
            {
                using (IDrawingContext context = DrawingContext.FromSurface(dstMask, FactorySource.PerThread))
                {
                    IBrush cachedOrCreateResource = context.GetCachedOrCreateResource<IBrush>(whiteBrush);
                    context.Clear(null);
                    using (context.UseTranslateTransform((float) -renderOffset.X, (float) -renderOffset.Y, MatrixMultiplyOrder.Prepend))
                    {
                        using (context.UseAntialiasMode(this.changes.Antialiasing ? AntialiasMode.PerPrimitive : AntialiasMode.Aliased))
                        {
                            base.ThrowIfCancellationRequested();
                            ShapesToolContentLayer layer = this.layer;
                            if (layer != ShapesToolContentLayer.Interior)
                            {
                                if (layer != ShapesToolContentLayer.Outline)
                                {
                                    throw new PaintDotNet.InternalErrorException(ExceptionUtil.InvalidEnumArgumentException<ShapesToolContentLayer>(this.layer, "this.layer"));
                                }
                            }
                            else
                            {
                                if (this.changes.ShapeRenderData.InteriorFill != null)
                                {
                                    this.changes.ShapeRenderData.InteriorFill.Fill(context, cachedOrCreateResource);
                                }
                                goto Label_02E6;
                            }
                            if (this.changes.ShapeRenderData.OutlineFill != null)
                            {
                                this.changes.ShapeRenderData.OutlineFill.Fill(context, cachedOrCreateResource);
                            }
                            else if (this.changes.ShapeRenderData.OutlineDraw != null)
                            {
                                IStrokeStyle strokeStyle = context.SafeGetCachedOrCreateResource<IStrokeStyle>(this.changes.OutlinePen);
                                float thickness = (float) this.changes.OutlinePen.Thickness;
                                this.changes.ShapeRenderData.OutlineDraw.Draw(context, cachedOrCreateResource, thickness, strokeStyle);
                            }
                        }
                    }
                Label_02E6:
                    base.ThrowIfCancellationRequested();
                }
            }
            base.ThrowIfCancellationRequested();
            this.contentRenderer.Render(dstContent, renderOffset);
            base.ThrowIfCancellationRequested();
        }

        bool IPreparableRenderer.TryPrepareNextStage()
        {
            if (Interlocked.Increment(ref this.nextPrepareStage) == 1)
            {
                if (this.MustUseCustom1pxAliasedOutlineRenderer())
                {
                    object flattenedPolyPoly = this.changes.ShapeRenderData.OutlineDraw.GetFlattenedPolyPoly();
                    return true;
                }
                switch (this.layer)
                {
                    case ShapesToolContentLayer.Interior:
                        return this.changes.ShapeRenderData.InteriorFill?.EnsureFillPrepared();

                    case ShapesToolContentLayer.Outline:
                        if (this.changes.ShapeRenderData.OutlineFill != null)
                        {
                            return this.changes.ShapeRenderData.OutlineFill.EnsureFillPrepared();
                        }
                        if (this.changes.ShapeRenderData.OutlineDraw != null)
                        {
                            return this.changes.ShapeRenderData.OutlineDraw.EnsureDrawPrepared();
                        }
                        break;
                }
            }
            return false;
        }

        [CompilerGenerated]
        private sealed class <EnumeratePolyLinePixels>d__14 : IEnumerable<PointInt32>, IEnumerable, IEnumerator<PointInt32>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private PointInt32 <>2__current;
            public IEnumerable<PointDouble> <>3__points;
            private int <>l__initialThreadId;
            private double <dxdu>5__3;
            private double <dydu>5__4;
            private double <lengthU>5__6;
            private IEnumerator<PointDouble> <pointsEnum>5__1;
            private PointDouble <pt0>5__2;
            private PointDouble <pt1>5__7;
            private double <u>5__5;
            private IEnumerable<PointDouble> points;

            [DebuggerHidden]
            public <EnumeratePolyLinePixels>d__14(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private void <>m__Finally1()
            {
                this.<>1__state = -1;
                if (this.<pointsEnum>5__1 != null)
                {
                    this.<pointsEnum>5__1.Dispose();
                }
            }

            private bool MoveNext()
            {
                try
                {
                    int num = this.<>1__state;
                    if (num == 0)
                    {
                        bool flag;
                        this.<>1__state = -1;
                        this.<pointsEnum>5__1 = this.points.GetEnumerator();
                        this.<>1__state = -3;
                        if (!this.<pointsEnum>5__1.MoveNext())
                        {
                            flag = false;
                        }
                        else
                        {
                            this.<pt0>5__2 = this.<pointsEnum>5__1.Current;
                            double num2 = 0.0;
                            while (this.<pointsEnum>5__1.MoveNext())
                            {
                                this.<pt1>5__7 = this.<pointsEnum>5__1.Current;
                                double num3 = this.<pt1>5__7.X - this.<pt0>5__2.X;
                                double num4 = this.<pt1>5__7.Y - this.<pt0>5__2.Y;
                                double num5 = Math.Abs(num3);
                                double num6 = Math.Abs(num4);
                                if (num5 > num6)
                                {
                                    this.<lengthU>5__6 = num5;
                                    this.<dxdu>5__3 = Math.Sign(num3);
                                    this.<dydu>5__4 = num4 / num5;
                                }
                                else if (num5 < num6)
                                {
                                    this.<lengthU>5__6 = num6;
                                    this.<dxdu>5__3 = num3 / num6;
                                    this.<dydu>5__4 = Math.Sign(num4);
                                }
                                else
                                {
                                    this.<lengthU>5__6 = num5;
                                    this.<dxdu>5__3 = Math.Sign(num3);
                                    this.<dydu>5__4 = Math.Sign(num4);
                                }
                                this.<u>5__5 = num2;
                                while (this.<u>5__5 < this.<lengthU>5__6)
                                {
                                    PointDouble pt = new PointDouble(this.<pt0>5__2.X + (this.<dxdu>5__3 * this.<u>5__5), this.<pt0>5__2.Y + (this.<dydu>5__4 * this.<u>5__5));
                                    this.<>2__current = PointDouble.Truncate(pt);
                                    this.<>1__state = 1;
                                    return true;
                                Label_018F:
                                    this.<>1__state = -3;
                                    this.<u>5__5++;
                                }
                                num2 = this.<u>5__5 - this.<lengthU>5__6;
                                this.<pt0>5__2 = this.<pt1>5__7;
                                this.<pt1>5__7 = new PointDouble();
                            }
                            this.<pt0>5__2 = new PointDouble();
                            this.<>m__Finally1();
                            goto Label_020D;
                        }
                        this.<>m__Finally1();
                        return flag;
                    }
                    if (num != 1)
                    {
                        return false;
                    }
                    goto Label_018F;
                Label_020D:
                    this.<pointsEnum>5__1 = null;
                    return false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
            }

            [DebuggerHidden]
            IEnumerator<PointInt32> IEnumerable<PointInt32>.GetEnumerator()
            {
                ShapesToolContentLayerRenderer.<EnumeratePolyLinePixels>d__14 d__;
                if ((this.<>1__state == -2) && (this.<>l__initialThreadId == Environment.CurrentManagedThreadId))
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                else
                {
                    d__ = new ShapesToolContentLayerRenderer.<EnumeratePolyLinePixels>d__14(0);
                }
                d__.points = this.<>3__points;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<PaintDotNet.Rendering.PointInt32>.GetEnumerator();

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
                switch (this.<>1__state)
                {
                    case -3:
                    case 1:
                        try
                        {
                        }
                        finally
                        {
                            this.<>m__Finally1();
                        }
                        break;
                }
            }

            PointInt32 IEnumerator<PointInt32>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }


        [CompilerGenerated]
        private sealed class <GetTracingMaskForPenWithoutOffset>d__12 : IEnumerable<bool>, IEnumerable, IEnumerator<bool>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private bool <>2__current;
            public Pen <>3__pen;
            public ShapesToolContentLayerRenderer <>4__this;
            private IEnumerator<int> <>7__wrap1;
            private int <>l__initialThreadId;
            private int <dash>5__3;
            private int <i>5__2;
            private bool <isDash>5__1;
            private Pen pen;

            [DebuggerHidden]
            public <GetTracingMaskForPenWithoutOffset>d__12(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private void <>m__Finally1()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap1 != null)
                {
                    this.<>7__wrap1.Dispose();
                }
            }

            private bool MoveNext()
            {
                try
                {
                    int num = this.<>1__state;
                    if (num == 0)
                    {
                        this.<>1__state = -1;
                        this.<isDash>5__1 = true;
                        this.<>7__wrap1 = this.<>4__this.GetTracingDashesForPenWithoutOffset(this.pen).GetEnumerator();
                        this.<>1__state = -3;
                        while (this.<>7__wrap1.MoveNext())
                        {
                            this.<dash>5__3 = this.<>7__wrap1.Current;
                            this.<i>5__2 = 0;
                            while (this.<i>5__2 < this.<dash>5__3)
                            {
                                this.<>2__current = this.<isDash>5__1;
                                this.<>1__state = 1;
                                return true;
                            Label_007A:
                                this.<>1__state = -3;
                                int num2 = this.<i>5__2 + 1;
                                this.<i>5__2 = num2;
                            }
                            this.<isDash>5__1 = !this.<isDash>5__1;
                        }
                        this.<>m__Finally1();
                        this.<>7__wrap1 = null;
                        return false;
                    }
                    if (num != 1)
                    {
                        return false;
                    }
                    goto Label_007A;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
            }

            [DebuggerHidden]
            IEnumerator<bool> IEnumerable<bool>.GetEnumerator()
            {
                ShapesToolContentLayerRenderer.<GetTracingMaskForPenWithoutOffset>d__12 d__;
                if ((this.<>1__state == -2) && (this.<>l__initialThreadId == Environment.CurrentManagedThreadId))
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                else
                {
                    d__ = new ShapesToolContentLayerRenderer.<GetTracingMaskForPenWithoutOffset>d__12(0) {
                        <>4__this = this.<>4__this
                    };
                }
                d__.pen = this.<>3__pen;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<System.Boolean>.GetEnumerator();

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
                switch (this.<>1__state)
                {
                    case -3:
                    case 1:
                        try
                        {
                        }
                        finally
                        {
                            this.<>m__Finally1();
                        }
                        break;
                }
            }

            bool IEnumerator<bool>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }
    }
}

