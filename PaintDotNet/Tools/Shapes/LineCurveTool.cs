namespace PaintDotNet.Tools.Shapes
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.Settings;
    using PaintDotNet.Shapes;
    using PaintDotNet.Tools;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal sealed class LineCurveTool : ShapesToolBase
    {
        public LineCurveTool(DocumentWorkspace docWorkspace) : base(docWorkspace, PdnResources.GetImageResource("Icons.LineToolIcon.png"), PdnResources.GetString("LineTool.Name"), PdnResources.GetString("LineTool.HelpText"), 'o', ToolBarConfigItems.LineCurveShapeType)
        {
        }

        protected override ShapeDrawType GetContentRendererLayers(ShapesToolChanges changes) => 
            ShapeDrawType.Outline;

        public static IEnumerable<ShapeInfo> GetShapesCatalog() => 
            (from si in ShapeManager.GetShapeInfos()
                where IsLineCurveShapeInfo(si)
                select si);

        private static bool IsLineCurveShapeInfo(ShapeInfo shapeInfo) => 
            (shapeInfo == PdnShapeBase.GetShapeInfo<LineCurveShape>());

        protected override IEnumerable<Setting> OnGetDrawingSettings() => 
            base.OnGetDrawingSettings().Concat<Setting>(base.ToolSettings.Shapes.CurveType);

        protected override void OnGetStatus(out ImageResource image, out string text)
        {
            string str;
            PointDouble startPoint;
            PointDouble endPoint;
            double length;
            double num4;
            string str2;
            string str3;
            ShapesToolChanges changes = base.Changes;
            switch (this.State)
            {
                case TransactedToolState.Drawing:
                    if (changes == null)
                    {
                        break;
                    }
                    goto Label_003A;

                case TransactedToolState.Dirty:
                case TransactedToolState.Editing:
                    goto Label_003A;
            }
            base.OnGetStatus(out image, out text);
            return;
        Label_003A:
            if (this.State == TransactedToolState.Dirty)
            {
                str = PdnResources.GetString("LineTool.StatusText.NotAdjusting.Format");
            }
            else
            {
                str = PdnResources.GetString("LineTool.StatusText.Format");
            }
            if (changes.ShapeRenderData.OutlineDraw.IsEmpty)
            {
                startPoint = changes.StartPoint;
                endPoint = changes.EndPoint;
                length = 0.0;
                num4 = 0.0;
            }
            else
            {
                PointAndTangentDouble pointAtLength = changes.ShapeRenderData.OutlineDraw.GetPointAtLength(0.0);
                PointAndTangentDouble num12 = changes.ShapeRenderData.OutlineDraw.GetPointAtLength(double.PositiveInfinity);
                startPoint = pointAtLength.Point;
                endPoint = num12.Point;
                length = changes.ShapeRenderData.OutlineDraw.GetLength();
                num4 = (-180.0 * Math.Atan2(endPoint.Y - startPoint.Y, endPoint.X - startPoint.X)) / 3.1415926535897931;
            }
            MeasurementUnit units = base.AppWorkspace.Units;
            PointDouble num5 = new PointDouble(base.Document.PixelToPhysicalX(startPoint.X, units), base.Document.PixelToPhysicalY(startPoint.Y, units));
            PointDouble num6 = new PointDouble(base.Document.PixelToPhysicalX(endPoint.X, units), base.Document.PixelToPhysicalY(endPoint.Y, units));
            VectorDouble num7 = (VectorDouble) (num6 - num5);
            double num8 = base.Document.PixelToPhysicalX(length, units);
            double num9 = base.Document.PixelToPhysicalY(length, units);
            double num10 = (num8 + num9) / 2.0;
            if (units != MeasurementUnit.Pixel)
            {
                str3 = PdnResources.GetString("MeasurementUnit." + units.ToString() + ".Abbreviation");
                str2 = "F2";
            }
            else
            {
                str3 = string.Empty;
                str2 = "F0";
            }
            string str4 = PdnResources.GetString("MeasurementUnit." + units.ToString() + ".Plural");
            string str5 = string.Format(str, new object[] { num7.X.ToString(str2), str3, num7.Y.ToString(str2), str3, num10.ToString("F2"), str4, num4.ToString("F2") });
            image = base.Image;
            text = str5;
        }

        protected override StaticListChoiceSetting<ShapeInfo> ShapeSetting =>
            base.ToolSettings.Shapes.LineCurveShape;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly LineCurveTool.<>c <>9 = new LineCurveTool.<>c();
            public static Func<ShapeInfo, bool> <>9__1_0;

            internal bool <GetShapesCatalog>b__1_0(ShapeInfo si) => 
                LineCurveTool.IsLineCurveShapeInfo(si);
        }
    }
}

