namespace PaintDotNet.Tools.Move
{
    using PaintDotNet;
    using PaintDotNet.Imaging;
    using PaintDotNet.Rendering;
    using PaintDotNet.Runtime;
    using PaintDotNet.Settings.App;
    using PaintDotNet.Tools;
    using PaintDotNet.Tools.Controls;
    using System;
    using System.Collections.Generic;

    [Serializable]
    internal sealed class MoveToolChanges : TransactedToolChanges<MoveToolChanges, MoveTool>
    {
        private readonly RectDouble baseBounds;
        private readonly Matrix3x2Double baseTransform;
        private readonly Guid bitmapSourcePersistenceKey;
        [NonSerialized]
        private PersistedObject<ISurface<ColorBgra>> bitmapSourcePO;
        private readonly Matrix3x2Double deltaTransform;
        private readonly TransformEditingMode editingMode;
        private readonly Matrix3x2Double editTransform;
        private readonly bool leaveCopyBehind;
        private readonly MoveToolPixelSource pixelSource;
        private readonly PointDouble? rotationAnchorOffset;

        public MoveToolChanges(IEnumerable<KeyValuePair<string, object>> drawingSettingsValues, MoveToolPixelSource pixelSource, ISurface<ColorBgra> bitmapSource, bool leaveCopyBehind, RectDouble baseBounds, Matrix3x2Double baseTransform, Matrix3x2Double deltaTransform, TransformEditingMode editingMode, Matrix3x2Double editTransform, PointDouble? rotationAnchorOffset) : base(drawingSettingsValues)
        {
            if (((pixelSource == MoveToolPixelSource.Bitmap) && (bitmapSource == null)) || ((pixelSource != MoveToolPixelSource.Bitmap) && (bitmapSource != null)))
            {
                ExceptionUtil.ThrowArgumentException($"MoveToolPixelSource.{pixelSource} specified, but bitmapSourcePersistenceKey={this.bitmapSourcePersistenceKey}");
            }
            if (bitmapSource == null)
            {
                this.bitmapSourcePO = null;
                this.bitmapSourcePersistenceKey = Guid.Empty;
            }
            else
            {
                this.bitmapSourcePO = new PersistedObject<ISurface<ColorBgra>>(bitmapSource, true);
                this.bitmapSourcePersistenceKey = PersistedObjectLocker.Add<ISurface<ColorBgra>>(this.bitmapSourcePO);
            }
            this.pixelSource = pixelSource;
            this.leaveCopyBehind = leaveCopyBehind;
            this.baseBounds = baseBounds;
            this.baseTransform = baseTransform;
            this.deltaTransform = deltaTransform;
            this.editingMode = editingMode;
            this.editTransform = editTransform;
            this.rotationAnchorOffset = rotationAnchorOffset;
            this.Initialize();
        }

        public MoveToolChanges(IEnumerable<KeyValuePair<string, object>> drawingSettingsValues, MoveToolPixelSource pixelSource, Guid bitmapSourcePersistenceKey, bool leaveCopyBehind, RectDouble baseBounds, Matrix3x2Double baseTransform, Matrix3x2Double deltaTransform, TransformEditingMode editingMode, Matrix3x2Double editTransform, PointDouble? rotationAnchorOffset) : base(drawingSettingsValues)
        {
            if (((pixelSource == MoveToolPixelSource.Bitmap) && (bitmapSourcePersistenceKey == Guid.Empty)) || ((pixelSource != MoveToolPixelSource.Bitmap) && (bitmapSourcePersistenceKey != Guid.Empty)))
            {
                ExceptionUtil.ThrowArgumentException($"MoveToolPixelSource.{pixelSource} specified, but bitmapSourcePersistenceKey={bitmapSourcePersistenceKey}");
            }
            this.pixelSource = pixelSource;
            this.bitmapSourcePersistenceKey = bitmapSourcePersistenceKey;
            this.leaveCopyBehind = leaveCopyBehind;
            this.baseBounds = baseBounds;
            this.baseTransform = baseTransform;
            this.deltaTransform = deltaTransform;
            this.editingMode = editingMode;
            this.editTransform = editTransform;
            this.rotationAnchorOffset = rotationAnchorOffset;
            this.Initialize();
        }

        private void Initialize()
        {
            if ((this.bitmapSourcePO == null) && (this.bitmapSourcePersistenceKey != Guid.Empty))
            {
                this.bitmapSourcePO = PersistedObjectLocker.TryGet<ISurface<ColorBgra>>(this.bitmapSourcePersistenceKey);
                if (this.bitmapSourcePO == null)
                {
                    throw new PaintDotNet.InternalErrorException("this.bitmapSource == null");
                }
            }
        }

        protected override void OnDeserializedGraph()
        {
            this.Initialize();
            base.OnDeserializedGraph();
        }

        protected override RectInt32 OnGetMaxRenderBounds()
        {
            RectDouble a = this.BaseTransform.Transform(this.baseBounds);
            RectDouble b = this.FinalTransform.Transform(this.baseBounds);
            return RectDouble.Union(a, b).Int32Bound;
        }

        public ColorBgra BackFillColor =>
            ColorBgra.FromUInt32(this.SecondaryColor.Bgra & 0xffffff);

        public RectDouble BaseBounds =>
            this.baseBounds;

        public Matrix3x2Double BaseTransform =>
            this.baseTransform;

        public PersistedObject<ISurface<ColorBgra>> BitmapSource
        {
            get
            {
                MoveToolChanges changes = this;
                lock (changes)
                {
                    if (this.bitmapSourcePO == null)
                    {
                        return null;
                    }
                    return this.bitmapSourcePO;
                }
            }
        }

        public Guid BitmapSourcePersistenceKey =>
            this.bitmapSourcePersistenceKey;

        public Matrix3x2Double DeltaTransform =>
            this.deltaTransform;

        public TransformEditingMode EditingMode =>
            this.editingMode;

        public Matrix3x2Double EditTransform =>
            this.editTransform;

        public Matrix3x2Double FinalTransform =>
            ((this.baseTransform * this.deltaTransform) * this.editTransform);

        public bool LeaveCopyBehind =>
            this.leaveCopyBehind;

        public ResamplingAlgorithm MoveToolResamplingAlgorithm =>
            base.GetDrawingSettingValue<ResamplingAlgorithm>(ToolSettings.Null.MoveToolResamplingAlgorithm);

        public MoveToolPixelSource PixelSource =>
            this.pixelSource;

        public PointDouble? RotationAnchorOffset =>
            this.rotationAnchorOffset;

        public ColorBgra SecondaryColor
        {
            get
            {
                ColorBgra32 bgra;
                bool flag = base.TryGetDrawingSettingValue<ColorBgra32>(ToolSettings.Null.SecondaryColor, out bgra);
                return bgra;
            }
        }

        public PaintDotNet.SelectionRenderingQuality SelectionRenderingQuality =>
            base.GetDrawingSettingValue<PaintDotNet.SelectionRenderingQuality>(ToolSettings.Null.Selection.RenderingQuality);
    }
}

