namespace PaintDotNet.Tools.PaintBucket
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using PaintDotNet.Runtime;
    using PaintDotNet.Settings.App;
    using PaintDotNet.Tools.FloodFill;
    using System;
    using System.Collections.Generic;
    using System.Drawing.Drawing2D;

    [Serializable]
    internal sealed class PaintBucketToolChanges : FloodFillToolChangesBase<PaintBucketToolChanges, PaintBucketTool>
    {
        private Guid clippingMaskPersistenceKey;
        [NonSerialized]
        private PersistedObject<GeometryList> clippingMaskPO;
        private PaintDotNet.WhichUserColor whichUserColor;

        public PaintBucketToolChanges(IEnumerable<KeyValuePair<string, object>> drawingSettingsValues, PointDouble originPoint, FloodMode? floodModeOverride, PaintDotNet.WhichUserColor whichUserColor, GeometryList clippingMask) : base(drawingSettingsValues, originPoint, floodModeOverride)
        {
            this.whichUserColor = whichUserColor;
            if (clippingMask == null)
            {
                this.clippingMaskPO = null;
                this.clippingMaskPersistenceKey = Guid.Empty;
            }
            else
            {
                this.clippingMaskPO = new PersistedObject<GeometryList>(clippingMask, true);
                this.clippingMaskPersistenceKey = PersistedObjectLocker.Add<GeometryList>(this.clippingMaskPO);
            }
            this.Initialize();
        }

        public PaintBucketToolChanges(IEnumerable<KeyValuePair<string, object>> drawingSettingsValues, PointDouble originPoint, FloodMode? floodModeOverride, PaintDotNet.WhichUserColor whichUserColor, Guid clippingMaskPersistenceKey) : base(drawingSettingsValues, originPoint, floodModeOverride)
        {
            this.whichUserColor = whichUserColor;
            this.clippingMaskPersistenceKey = clippingMaskPersistenceKey;
            this.Initialize();
        }

        private void Initialize()
        {
            if ((this.clippingMaskPO == null) && (this.clippingMaskPersistenceKey != Guid.Empty))
            {
                this.clippingMaskPO = PersistedObjectLocker.TryGet<GeometryList>(this.clippingMaskPersistenceKey);
                if (this.clippingMaskPO == null)
                {
                    throw new InternalErrorException("this.clippingMask == null");
                }
            }
        }

        protected override void OnDeserializedGraph()
        {
            this.Initialize();
            base.OnDeserializedGraph();
        }

        public bool Antialiasing =>
            base.GetDrawingSettingValue<bool>(ToolSettings.Null.Antialiasing);

        public ColorBgra BackgroundColor
        {
            get
            {
                if (this.WhichUserColor != PaintDotNet.WhichUserColor.Primary)
                {
                    return this.PrimaryColor;
                }
                return this.SecondaryColor;
            }
        }

        public ContentBlendMode BlendMode =>
            base.GetDrawingSettingValue<ContentBlendMode>(ToolSettings.Null.BlendMode);

        public PaintDotNet.BrushType BrushType =>
            base.GetDrawingSettingValue<PaintDotNet.BrushType>(ToolSettings.Null.Brush.Type);

        public GeometryList ClippingMask
        {
            get
            {
                PaintBucketToolChanges changes = this;
                lock (changes)
                {
                    return this.clippingMaskPO?.Object;
                }
            }
        }

        public Guid ClippingMaskPersistenceKey =>
            this.clippingMaskPersistenceKey;

        public ColorBgra ForegroundColor
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

        public System.Drawing.Drawing2D.HatchStyle HatchStyle =>
            base.GetDrawingSettingValue<System.Drawing.Drawing2D.HatchStyle>(ToolSettings.Null.Brush.HatchStyle);

        public ColorBgra PrimaryColor =>
            base.GetDrawingSettingValue<ColorBgra32>(ToolSettings.Null.PrimaryColor);

        public ColorBgra SecondaryColor =>
            base.GetDrawingSettingValue<ColorBgra32>(ToolSettings.Null.SecondaryColor);

        public PaintDotNet.SelectionRenderingQuality SelectionRenderingQuality =>
            base.GetDrawingSettingValue<PaintDotNet.SelectionRenderingQuality>(ToolSettings.Null.Selection.RenderingQuality);

        public PaintDotNet.WhichUserColor WhichUserColor =>
            this.whichUserColor;
    }
}

