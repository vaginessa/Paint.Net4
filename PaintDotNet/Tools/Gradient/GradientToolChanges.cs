namespace PaintDotNet.Tools.Gradient
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using PaintDotNet.Settings.App;
    using PaintDotNet.Tools;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    [Serializable]
    internal sealed class GradientToolChanges : TransactedToolChanges<GradientToolChanges, GradientTool>
    {
        public GradientToolChanges(IEnumerable<KeyValuePair<string, object>> drawingSettingsValues, PointDouble mouseStartPoint, PointDouble mouseEndPoint, bool reverseColors, bool isAngleConstrained, bool isEditingStartPoint, bool isEditingEndPoint, bool isDragging) : base(drawingSettingsValues)
        {
            this.MouseStartPoint = mouseStartPoint;
            this.MouseEndPoint = mouseEndPoint;
            this.ReverseColors = reverseColors;
            this.IsAngleConstrained = isAngleConstrained;
            this.IsEditingStartPoint = isEditingStartPoint;
            this.IsEditingEndPoint = isEditingEndPoint;
            this.IsDragging = isDragging;
        }

        private static PointDouble ConstrainPoints(PointDouble a, PointDouble b)
        {
            VectorDouble num = (VectorDouble) (b - a);
            double d = Math.Atan2(num.Y, num.X);
            double num3 = Math.Sqrt((num.X * num.X) + (num.Y * num.Y));
            d = (Math.Round((double) ((12.0 * d) / 3.1415926535897931), MidpointRounding.AwayFromZero) * 3.1415926535897931) / 12.0;
            return new PointDouble(a.X + (num3 * Math.Cos(d)), a.Y + (num3 * Math.Sin(d)));
        }

        protected override RectInt32 OnGetMaxRenderBounds() => 
            TransactedToolChanges.MaxMaxRenderBounds;

        public bool Antialiasing =>
            base.GetDrawingSettingValue<bool>(ToolSettings.Null.Antialiasing);

        public ContentBlendMode BlendMode =>
            base.GetDrawingSettingValue<ContentBlendMode>(ToolSettings.Null.BlendMode);

        public PointDouble GradientEndPoint
        {
            get
            {
                if (this.IsAngleConstrained && this.IsEditingEndPoint)
                {
                    return ConstrainPoints(this.MouseStartPoint, this.MouseEndPoint);
                }
                return this.MouseEndPoint;
            }
        }

        public PointDouble GradientStartPoint
        {
            get
            {
                if (this.IsAngleConstrained && this.IsEditingStartPoint)
                {
                    return ConstrainPoints(this.MouseEndPoint, this.MouseStartPoint);
                }
                return this.MouseStartPoint;
            }
        }

        public PaintDotNet.Rendering.GradientType GradientType =>
            base.GetDrawingSettingValue<PaintDotNet.Rendering.GradientType>(ToolSettings.Null.Gradient.Type);

        public bool IsAlphaOnly =>
            base.GetDrawingSettingValue<bool>(ToolSettings.Null.Gradient.IsAlphaOnly);

        public bool IsAngleConstrained { get; private set; }

        public bool IsDragging { get; private set; }

        public bool IsEditingEndPoint { get; private set; }

        public bool IsEditingStartPoint { get; private set; }

        public PointDouble MouseEndPoint { get; private set; }

        public PointDouble MouseStartPoint { get; private set; }

        public ColorBgra PrimaryColor =>
            base.GetDrawingSettingValue<ColorBgra32>(ToolSettings.Null.PrimaryColor);

        public GradientRepeatType RepeatType =>
            base.GetDrawingSettingValue<GradientRepeatType>(ToolSettings.Null.Gradient.RepeatType);

        public bool ReverseColors { get; private set; }

        public ColorBgra SecondaryColor =>
            base.GetDrawingSettingValue<ColorBgra32>(ToolSettings.Null.SecondaryColor);

        public PaintDotNet.SelectionRenderingQuality SelectionRenderingQuality =>
            base.GetDrawingSettingValue<PaintDotNet.SelectionRenderingQuality>(ToolSettings.Null.Selection.RenderingQuality);
    }
}

