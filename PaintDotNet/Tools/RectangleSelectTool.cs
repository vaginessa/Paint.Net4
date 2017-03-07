namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.Rendering;
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;

    internal sealed class RectangleSelectTool : SelectionTool
    {
        public RectangleSelectTool(DocumentWorkspace documentWorkspace) : base(documentWorkspace, PdnResources.GetImageResource("Icons.RectangleSelectToolIcon.png"), PdnResources.GetString("RectangleSelectTool.Name"), PdnResources.GetString("RectangleSelectTool.HelpText"), 's', ToolBarConfigItems.None | ToolBarConfigItems.SelectionDrawMode)
        {
        }

        private static bool AreTheSameGeometry(GeometryList geometry1, PointDouble[] polygon2)
        {
            if (geometry1.PolygonCount != 1)
            {
                return false;
            }
            if (geometry1.TotalPointCount != polygon2.Length)
            {
                return false;
            }
            RectDouble num = polygon2.Bounds();
            if (geometry1.Bounds != num)
            {
                return false;
            }
            IList<PointDouble[]> polygonList = geometry1.GetPolygonList();
            if (polygonList.Count != 1)
            {
                throw new InternalErrorException();
            }
            PointDouble[] numArray = polygonList[0];
            for (int i = 0; i < numArray.Length; i++)
            {
                if (numArray[i] != polygon2[i])
                {
                    return false;
                }
            }
            return true;
        }

        protected override SegmentedList<PointDouble> CreateShape(SegmentedList<PointDouble> tracePoints)
        {
            RectDouble num3;
            double num6;
            double num7;
            PointDouble a = tracePoints[0];
            PointDouble b = tracePoints[tracePoints.Count - 1];
            SelectionDrawMode mode = base.ToolSettings.Selection.DrawMode.Value;
            double num4 = base.ToolSettings.Selection.DrawWidth.Value;
            double num5 = base.ToolSettings.Selection.DrawHeight.Value;
            MeasurementUnit sourceUnits = base.ToolSettings.Selection.DrawUnits.Value;
            switch (mode)
            {
                case SelectionDrawMode.FixedRatio:
                case SelectionDrawMode.FixedSize:
                    num6 = Math.Abs(num4);
                    num7 = Math.Abs(num5);
                    break;

                default:
                    num6 = num4;
                    num7 = num5;
                    break;
            }
            switch (mode)
            {
                case SelectionDrawMode.Normal:
                    if ((base.ModifierKeys & Keys.Shift) == Keys.None)
                    {
                        num3 = RectDoubleUtil.FromPixelPoints(a, b);
                        break;
                    }
                    num3 = RectDoubleUtil.FromPixelPointsConstrained(a, b);
                    break;

                case SelectionDrawMode.FixedRatio:
                    try
                    {
                        double num13 = b.X - a.X;
                        double num14 = b.Y - a.Y;
                        double num15 = num13 / num6;
                        double num16 = Math.Sign(num15);
                        double num17 = num14 / num7;
                        double num18 = Math.Sign(num17);
                        double num19 = num6 / num7;
                        if (num15 < num17)
                        {
                            double x = a.X;
                            double y = a.Y;
                            double right = a.X + num13;
                            double bottom = a.Y + (num18 * Math.Abs((double) (num13 / num19)));
                            num3 = RectDouble.FromEdges(x, y, right, bottom);
                        }
                        else
                        {
                            double left = a.X;
                            double top = a.Y;
                            double num26 = a.X + (num16 * Math.Abs((double) (num14 * num19)));
                            double num27 = a.Y + num14;
                            num3 = RectDouble.FromEdges(left, top, num26, num27);
                        }
                    }
                    catch (ArithmeticException)
                    {
                        num3 = new RectDouble(a.X, a.Y, 0.0, 0.0);
                    }
                    break;

                case SelectionDrawMode.FixedSize:
                {
                    double width = Document.ConvertMeasurement(num6, sourceUnits, base.Document.DpuUnit, base.Document.DpuX, MeasurementUnit.Pixel);
                    double height = Document.ConvertMeasurement(num7, sourceUnits, base.Document.DpuUnit, base.Document.DpuY, MeasurementUnit.Pixel);
                    num3 = new RectDouble(b.X, b.Y, width, height);
                    break;
                }
                default:
                    throw ExceptionUtil.InvalidEnumArgumentException<SelectionDrawMode>(mode, "drawMode");
            }
            RectDouble num8 = RectDouble.Intersect(num3, base.Document.Bounds());
            RectDouble num10 = num8.Int32Bound;
            if (num8.HasPositiveArea)
            {
                SegmentedList<PointDouble> list = new SegmentedList<PointDouble>(5, 7) {
                    new PointDouble(num10.Left, num10.Top),
                    new PointDouble(num10.Right, num10.Top),
                    new PointDouble(num10.Right, num10.Bottom),
                    new PointDouble(num10.Left, num10.Bottom)
                };
                list.Add(list[0]);
                return list;
            }
            return new SegmentedList<PointDouble>(0, 7);
        }

        protected override SelectionTool.WhatToDo GetWhatToDo(SelectionTool.WhatToDo plannedAction, bool appending, GeometryList oldSelectionGeometry, PointDouble[] newPolygon)
        {
            if (((((SelectionDrawMode) base.ToolSettings.Selection.DrawMode.Value) == SelectionDrawMode.FixedSize) && (plannedAction == SelectionTool.WhatToDo.Emit)) && (!appending && AreTheSameGeometry(oldSelectionGeometry, newPolygon)))
            {
                return SelectionTool.WhatToDo.Clear;
            }
            return base.GetWhatToDo(plannedAction, appending, oldSelectionGeometry, newPolygon);
        }

        protected override void OnActivate()
        {
            base.SetCursors("Cursors.RectangleSelectToolCursor.cur", "Cursors.RectangleSelectToolCursorMinus.cur", "Cursors.RectangleSelectToolCursorPlus.cur", "Cursors.RectangleSelectToolCursorMouseDown.cur");
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

        protected override bool WasClickTooQuick(double milliseconds)
        {
            if (((SelectionDrawMode) base.ToolSettings.Selection.DrawMode.Value) == SelectionDrawMode.FixedSize)
            {
                return false;
            }
            return base.WasClickTooQuick(milliseconds);
        }

        protected override int MinPointsForRender
        {
            get
            {
                if (((SelectionDrawMode) base.ToolSettings.Selection.DrawMode.Value) == SelectionDrawMode.FixedSize)
                {
                    return 1;
                }
                return base.MinPointsForRender;
            }
        }

        protected override bool MustMoveForEmit =>
            (((SelectionDrawMode) base.ToolSettings.Selection.DrawMode.Value) != SelectionDrawMode.FixedSize);
    }
}

