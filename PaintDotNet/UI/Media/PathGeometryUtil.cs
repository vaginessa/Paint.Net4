namespace PaintDotNet.UI.Media
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Rendering;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    internal static class PathGeometryUtil
    {
        public static IEnumerable<PointDouble> EnumeratePathFigurePoints(PathFigure figure) => 
            EnumeratePathFigurePointsInner(figure).SequentialDistinct<PointDouble>();

        [IteratorStateMachine(typeof(<EnumeratePathFigurePointsInner>d__3))]
        private static IEnumerable<PointDouble> EnumeratePathFigurePointsInner(PathFigure figure)
        {
            yield return figure.StartPoint;
            using (this.<>7__wrap1 = figure.Segments.GetEnumerator())
            {
                while (this.<>7__wrap1.MoveNext())
                {
                    this.<segment>5__1 = this.<>7__wrap1.Current;
                    LineSegment segment = this.<segment>5__1 as LineSegment;
                    if (segment != null)
                    {
                        yield return segment.Point;
                    }
                    else
                    {
                        PolyLineSegment segment2 = this.<segment>5__1 as PolyLineSegment;
                        if (segment2 != null)
                        {
                            using (this.<>7__wrap2 = segment2.Points.GetEnumerator())
                            {
                                while (this.<>7__wrap2.MoveNext())
                                {
                                    PointDouble current = this.<>7__wrap2.Current;
                                    yield return current;
                                }
                            }
                            this.<>7__wrap2 = null;
                        }
                        else
                        {
                            this.<segment>5__1 = null;
                        }
                    }
                }
            }
            this.<>7__wrap1 = null;
            if (figure.IsClosed)
            {
                yield return figure.StartPoint;
            }
        }

        [IteratorStateMachine(typeof(<EnumeratePathGeometryPolygons>d__1))]
        public static IEnumerable<IEnumerable<PointDouble>> EnumeratePathGeometryPolygons(PathGeometry pathGeometry) => 
            new <EnumeratePathGeometryPolygons>d__1(-2) { <>3__pathGeometry = pathGeometry };

        public static void SetFirstAndLastPointsOfPathGeometry(PathGeometry pathGeometry, PointDouble pt1, PointDouble ptN)
        {
            pathGeometry.Figures[0].StartPoint = pt1;
            PathFigure figure = pathGeometry.Figures[pathGeometry.Figures.Count - 1];
            PathSegment segment = figure.Segments[figure.Segments.Count - 1];
            LineSegment segment2 = segment as LineSegment;
            if (segment2 != null)
            {
                segment2.Point = ptN;
            }
            else
            {
                PolyLineSegment segment3 = segment as PolyLineSegment;
                if (segment3 == null)
                {
                    throw new InternalErrorException();
                }
                segment3.Points[segment3.Points.Count - 1] = ptN;
            }
        }


        [CompilerGenerated]
        private sealed class <EnumeratePathGeometryPolygons>d__1 : IEnumerable<IEnumerable<PointDouble>>, IEnumerable, IEnumerator<IEnumerable<PointDouble>>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private IEnumerable<PointDouble> <>2__current;
            public PathGeometry <>3__pathGeometry;
            private IEnumerator<PathFigure> <>7__wrap1;
            private int <>l__initialThreadId;
            private PathGeometry pathGeometry;

            [DebuggerHidden]
            public <EnumeratePathGeometryPolygons>d__1(int <>1__state)
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
                        if (this.pathGeometry.MayHaveCurves())
                        {
                            throw new ArgumentException("pathGeometry has curves");
                        }
                        this.<>7__wrap1 = this.pathGeometry.Figures.GetEnumerator();
                        this.<>1__state = -3;
                        while (this.<>7__wrap1.MoveNext())
                        {
                            PathFigure current = this.<>7__wrap1.Current;
                            this.<>2__current = PathGeometryUtil.EnumeratePathFigurePoints(current);
                            this.<>1__state = 1;
                            return true;
                        Label_0077:
                            this.<>1__state = -3;
                        }
                        this.<>m__Finally1();
                        this.<>7__wrap1 = null;
                        return false;
                    }
                    if (num != 1)
                    {
                        return false;
                    }
                    goto Label_0077;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
            }

            [DebuggerHidden]
            IEnumerator<IEnumerable<PointDouble>> IEnumerable<IEnumerable<PointDouble>>.GetEnumerator()
            {
                PathGeometryUtil.<EnumeratePathGeometryPolygons>d__1 d__;
                if ((this.<>1__state == -2) && (this.<>l__initialThreadId == Environment.CurrentManagedThreadId))
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                else
                {
                    d__ = new PathGeometryUtil.<EnumeratePathGeometryPolygons>d__1(0);
                }
                d__.pathGeometry = this.<>3__pathGeometry;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<PaintDotNet.Rendering.PointDouble>>.GetEnumerator();

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

            IEnumerable<PointDouble> IEnumerator<IEnumerable<PointDouble>>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }
    }
}

