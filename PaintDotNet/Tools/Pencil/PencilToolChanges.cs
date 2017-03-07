namespace PaintDotNet.Tools.Pencil
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Rendering;
    using PaintDotNet.Settings.App;
    using PaintDotNet.Tools;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;

    [Serializable]
    internal sealed class PencilToolChanges : TransactedToolChanges<PencilToolChanges, PencilTool>
    {
        public PencilToolChanges(IEnumerable<KeyValuePair<string, object>> drawingSettingsValues, PaintDotNet.WhichUserColor whichUserColor, IEnumerable<PointDouble> points) : base(drawingSettingsValues)
        {
            this.WhichUserColor = whichUserColor;
            this.Points = new ReadOnlyCollection<PointDouble>(points.ToArrayEx<PointDouble>());
        }

        protected override RectInt32 OnGetMaxRenderBounds() => 
            RectInt32.Inflate(this.Points.Bounds().Int32Bound, 1, 1);

        public ContentBlendMode BlendMode =>
            base.GetDrawingSettingValue<ContentBlendMode>(ToolSettings.Null.BlendMode);

        public ColorBgra Color
        {
            get
            {
                if (this.WhichUserColor != PaintDotNet.WhichUserColor.Primary)
                {
                    return this.SecondaryColor;
                }
                return this.PrimaryColor;
            }
        }

        public IList<PointDouble> Points { get; private set; }

        public ColorBgra PrimaryColor =>
            base.GetDrawingSettingValue<ColorBgra32>(ToolSettings.Null.PrimaryColor);

        public ColorBgra SecondaryColor =>
            base.GetDrawingSettingValue<ColorBgra32>(ToolSettings.Null.SecondaryColor);

        public PaintDotNet.SelectionRenderingQuality SelectionRenderingQuality =>
            base.GetDrawingSettingValue<PaintDotNet.SelectionRenderingQuality>(ToolSettings.Null.Selection.RenderingQuality);

        public PaintDotNet.WhichUserColor WhichUserColor { get; private set; }
    }
}

