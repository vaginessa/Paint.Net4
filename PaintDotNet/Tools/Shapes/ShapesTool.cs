namespace PaintDotNet.Tools.Shapes
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Resources;
    using PaintDotNet.Settings;
    using PaintDotNet.Shapes;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class ShapesTool : ShapesToolBase
    {
        public ShapesTool(DocumentWorkspace docWorkspace) : base(docWorkspace, PdnResources.GetImageResource("Icons.ShapesToolIcon.16.png"), PdnResources.GetString("ShapesTool.Name"), PdnResources.GetString("ShapesTool.HelpText"), 'o', ToolBarConfigItems.None | ToolBarConfigItems.ShapeDrawType | ToolBarConfigItems.ShapeType)
        {
        }

        public static IEnumerable<ShapeInfo> GetShapesCatalog() => 
            ShapeManager.GetShapeInfos().Except<ShapeInfo>(LineCurveTool.GetShapesCatalog());

        public override ImageResource LargeImage =>
            PdnResources.GetImageResource("Icons.ShapesToolIcon.png");

        protected override StaticListChoiceSetting<ShapeInfo> ShapeSetting =>
            base.ToolSettings.Shapes.Shape;
    }
}

