namespace PaintDotNet.Tools.MagicWand
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using PaintDotNet.Runtime;
    using PaintDotNet.Settings.App;
    using PaintDotNet.Tools.FloodFill;
    using System;
    using System.Collections.Generic;

    [Serializable]
    internal sealed class MagicWandToolChanges : FloodFillToolChangesBase<MagicWandToolChanges, MagicWandTool>
    {
        private Guid baseGeometryPersistenceKey;
        [NonSerialized]
        private PersistedObject<GeometryList> baseGeometryPO;
        private PaintDotNet.SelectionCombineMode? selectionCombineModeOverride;

        public MagicWandToolChanges(IEnumerable<KeyValuePair<string, object>> drawingSettingsValues, PointDouble originPoint, PaintDotNet.SelectionCombineMode? selectionCombineModeOverride, FloodMode? floodModeOverride, GeometryList baseGeometry) : base(drawingSettingsValues, originPoint, floodModeOverride)
        {
            this.selectionCombineModeOverride = selectionCombineModeOverride;
            if (baseGeometry == null)
            {
                this.baseGeometryPO = null;
                this.baseGeometryPersistenceKey = Guid.Empty;
            }
            else
            {
                this.baseGeometryPO = new PersistedObject<GeometryList>(baseGeometry, true);
                this.baseGeometryPersistenceKey = PersistedObjectLocker.Add<GeometryList>(this.baseGeometryPO);
            }
            this.Initialize();
        }

        public MagicWandToolChanges(IEnumerable<KeyValuePair<string, object>> drawingSettingsValues, PointDouble originPoint, PaintDotNet.SelectionCombineMode? selectionCombineModeOverride, FloodMode? floodModeOverride, Guid baseGeometryPersistenceKey) : base(drawingSettingsValues, originPoint, floodModeOverride)
        {
            this.selectionCombineModeOverride = selectionCombineModeOverride;
            this.baseGeometryPersistenceKey = baseGeometryPersistenceKey;
            this.Initialize();
        }

        private void Initialize()
        {
            if ((this.baseGeometryPO == null) && (this.baseGeometryPersistenceKey != Guid.Empty))
            {
                this.baseGeometryPO = PersistedObjectLocker.TryGet<GeometryList>(this.baseGeometryPersistenceKey);
                if (this.baseGeometryPO == null)
                {
                    throw new InternalErrorException("this.baseGeometry == null");
                }
            }
        }

        protected override void OnClonedWithNewDrawingSettingsValues(MagicWandToolChanges source)
        {
            if (this.selectionCombineModeOverride.HasValue && (source.SelectionCombineModeBase != this.SelectionCombineModeBase))
            {
                using (base.UseChangeScope())
                {
                    this.selectionCombineModeOverride = null;
                }
            }
            base.OnClonedWithNewDrawingSettingsValues(source);
        }

        protected override void OnDeserializedGraph()
        {
            this.Initialize();
            base.OnDeserializedGraph();
        }

        public GeometryList BaseGeometry
        {
            get
            {
                MagicWandToolChanges changes = this;
                lock (changes)
                {
                    return this.baseGeometryPO?.Object;
                }
            }
        }

        public Guid BaseGeometryPersistenceKey =>
            this.baseGeometryPersistenceKey;

        public PaintDotNet.SelectionCombineMode SelectionCombineMode
        {
            get
            {
                PaintDotNet.SelectionCombineMode? selectionCombineModeOverride = this.SelectionCombineModeOverride;
                if (!selectionCombineModeOverride.HasValue)
                {
                    return this.SelectionCombineModeBase;
                }
                return selectionCombineModeOverride.GetValueOrDefault();
            }
        }

        public PaintDotNet.SelectionCombineMode SelectionCombineModeBase =>
            base.GetDrawingSettingValue<PaintDotNet.SelectionCombineMode>(ToolSettings.Null.Selection.CombineMode);

        public PaintDotNet.SelectionCombineMode? SelectionCombineModeOverride =>
            this.selectionCombineModeOverride;
    }
}

