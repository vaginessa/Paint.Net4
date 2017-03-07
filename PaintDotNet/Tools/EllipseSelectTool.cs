namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.Drawing;
    using PaintDotNet.Rendering;
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;

    internal sealed class EllipseSelectTool : SelectionTool
    {
        public EllipseSelectTool(DocumentWorkspace documentWorkspace) : base(documentWorkspace, PdnResources.GetImageResource("Icons.EllipseSelectToolIcon.png"), PdnResources.GetString("EllipseSelectTool.Name"), PdnResources.GetString("EllipseSelectTool.HelpText"), 's', ToolBarConfigItems.None)
        {
        }

        protected override SegmentedList<PointDouble> CreateShape(SegmentedList<PointDouble> tracePoints)
        {
            RectDouble num5;
            PointDouble a = tracePoints[0];
            PointDouble b = tracePoints[tracePoints.Count - 1];
            PointDouble num3 = new PointDouble(b.X - a.X, b.Y - a.Y);
            double num4 = Math.Sqrt((num3.X * num3.X) + (num3.Y * num3.Y));
            if ((base.ModifierKeys & Keys.Shift) != Keys.None)
            {
                PointDouble center = new PointDouble((a.X + b.X) / 2.0, (a.Y + b.Y) / 2.0);
                double num7 = num4 / 2.0;
                num5 = RectDouble.FromCenter(center, (double) (num7 * 2.0));
            }
            else
            {
                num5 = RectDoubleUtil.FromPixelPoints(a, b);
            }
            PdnGraphicsPath path = new PdnGraphicsPath();
            path.AddEllipse(num5.ToGdipRectangleF());
            using (Matrix matrix = new Matrix())
            {
                path.Flatten(matrix, 0.1f);
            }
            SegmentedList<PointDouble> list = new SegmentedList<PointDouble>(path.PathPoints.Select<PointF, PointDouble>(pt => pt.ToDoublePoint()), 7);
            path.Dispose();
            return list;
        }

        protected override void OnActivate()
        {
            base.SetCursors("Cursors.EllipseSelectToolCursor.cur", "Cursors.EllipseSelectToolCursorMinus.cur", "Cursors.EllipseSelectToolCursorPlus.cur", "Cursors.EllipseSelectToolCursorMouseDown.cur");
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
        }

        protected override SegmentedList<PointDouble> TrimShapePath(SegmentedList<PointDouble> tracePoints)
        {
            SegmentedList<PointDouble> list = new SegmentedList<PointDouble>();
            if (tracePoints.Count > 0)
            {
                list.Add(tracePoints[0]);
                if (tracePoints.Count > 1)
                {
                    list.Add(tracePoints[tracePoints.Count - 1]);
                }
            }
            return list;
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly EllipseSelectTool.<>c <>9 = new EllipseSelectTool.<>c();
            public static Func<PointF, PointDouble> <>9__1_0;

            internal PointDouble <CreateShape>b__1_0(PointF pt) => 
                pt.ToDoublePoint();
        }
    }
}

