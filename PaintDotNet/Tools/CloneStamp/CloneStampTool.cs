namespace PaintDotNet.Tools.CloneStamp
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.Settings;
    using PaintDotNet.Tools.BrushBase;
    using PaintDotNet.UI;
    using PaintDotNet.UI.Input;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;

    internal sealed class CloneStampTool : BrushToolBase<CloneStampTool, CloneStampToolChanges, CloneStampToolUI>
    {
        private bool isHandlingDrag;

        public CloneStampTool(DocumentWorkspace documentWorkspace) : base(documentWorkspace, PdnResources.GetImageResource("Icons.CloneStampToolIcon.png"), PdnResources.GetString("CloneStampTool.Name"), PdnResources.GetString("CloneStampTool.HelpText"), 'l', false, ToolBarConfigItems.BlendMode)
        {
        }

        protected override CloneStampToolChanges CreateChanges(CloneStampToolChanges oldChanges, IEnumerable<BrushInputPoint> inputPoints) => 
            new CloneStampToolChanges(oldChanges, inputPoints);

        protected override CloneStampToolChanges CreateChanges(IEnumerable<KeyValuePair<string, object>> drawingSettingsValues, IEnumerable<BrushInputPoint> inputPoints, MouseButtonState rightButtonState)
        {
            WhichUserColor whichUserColor = (rightButtonState == MouseButtonState.Pressed) ? WhichUserColor.Secondary : WhichUserColor.Primary;
            int anchorLayerIndex = this.GetAnchorLayerIndex();
            PointDouble pt = (PointDouble) -this.GetStaticData().anchorOffset;
            return new CloneStampToolChanges(drawingSettingsValues, whichUserColor, inputPoints, anchorLayerIndex, PointDouble.Truncate(pt));
        }

        [IteratorStateMachine(typeof(<CreateContentRenderers>d__21))]
        protected override IEnumerable<IMaskedRenderer<ColorBgra, ColorAlpha8>> CreateContentRenderers(BitmapLayer layer, CloneStampToolChanges changes)
        {
            BitmapLayer layer = (BitmapLayer) this.Document.Layers[changes.SourceLayerIndex];
            IRenderer<ColorBgra> sampleSource = layer.Surface;
            yield return new CloneStampToolContentRenderer(sampleSource, changes);
        }

        private int GetAnchorLayerIndex()
        {
            int index = -1;
            if (this.GetStaticData().anchorLayerWeak != null)
            {
                Layer target = this.GetStaticData().anchorLayerWeak.Target;
                if (((target != null) && this.GetStaticData().anchorLayerWeak.IsAlive) && !target.IsDisposed)
                {
                    index = base.Document.Layers.IndexOf(target);
                }
            }
            if (index == -1)
            {
                index = base.ActiveLayerIndex;
            }
            return index;
        }

        protected override ContentBlendMode GetBlendMode(CloneStampToolChanges changes) => 
            changes.BlendMode;

        private CloneStampToolStaticData GetStaticData()
        {
            CloneStampToolStaticData staticData = (CloneStampToolStaticData) base.GetStaticData();
            if (staticData == null)
            {
                base.SetStaticData(new CloneStampToolStaticData());
                return this.GetStaticData();
            }
            return staticData;
        }

        protected override void OnActivated()
        {
            if (this.GetAnchorLayerIndex() == -1)
            {
                this.GetStaticData().anchorOffsetMode = AnchorOffsetMode.NotSet;
            }
            this.UpdateAnchor();
            base.ToolSettings.Pen.Width.ValueChangedT += new ValueChangedEventHandler<float>(this.OnToolSettingsPenWidthValueChangedT);
            base.OnActivated();
        }

        protected override void OnDeactivated()
        {
            this.UpdateAnchor();
            base.ToolSettings.Pen.Width.ValueChangedT -= new ValueChangedEventHandler<float>(this.OnToolSettingsPenWidthValueChangedT);
            base.OnDeactivated();
        }

        protected override IEnumerable<Setting> OnGetDrawingSettings()
        {
            Setting[] tails = new Setting[] { base.ToolSettings.BlendMode, base.ToolSettings.PrimaryColor, base.ToolSettings.SecondaryColor };
            return base.OnGetDrawingSettings().Concat<Setting>(tails);
        }

        protected override void OnKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            this.UpdateUIIsSettingAnchor();
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(System.Windows.Forms.KeyEventArgs e)
        {
            this.UpdateUIIsSettingAnchor();
            base.OnKeyUp(e);
        }

        protected override void OnMouseEnter()
        {
            this.UpdateUIIsSettingAnchor();
            base.OnMouseEnter();
        }

        protected override void OnMouseLeave()
        {
            this.UpdateUIIsSettingAnchor();
            base.OnMouseLeave();
        }

        protected override void OnMouseMove(MouseEventArgsF e)
        {
            this.UpdateAnchor();
            base.OnMouseMove(e);
        }

        private void OnToolSettingsPenWidthValueChangedT(object sender, ValueChangedEventArgs<float> e)
        {
            this.UpdateAnchor();
        }

        protected override void OnUIDragBegin(object sender, PaintDotNet.UI.Input.MouseEventArgs e)
        {
            if (e.Source is UIElement)
            {
                bool flag = (base.PresentationSource.PrimaryKeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
                PointDouble mouseCenterPt = BrushToolBase<CloneStampTool, CloneStampToolChanges, CloneStampToolUI>.GetMouseCenterPt(e.GetPosition(base.UI), base.CanvasView.CanvasHairWidth);
                if (flag)
                {
                    this.isHandlingDrag = true;
                    this.GetStaticData().anchorOffsetMode = AnchorOffsetMode.Absolute;
                    this.GetStaticData().anchorOffset = (VectorDouble) mouseCenterPt;
                    this.GetStaticData().anchorLayerWeak = new WeakReferenceT<Layer>(base.ActiveLayer);
                    this.UpdateAnchor();
                    e.Handled = true;
                }
                else if (this.GetStaticData().anchorOffsetMode == AnchorOffsetMode.NotSet)
                {
                    this.isHandlingDrag = true;
                    e.Handled = true;
                }
                else if (this.GetStaticData().anchorOffsetMode == AnchorOffsetMode.Absolute)
                {
                    PointDouble anchorCenter = this.GetStaticData().anchorCenter;
                    this.GetStaticData().anchorOffsetMode = AnchorOffsetMode.Relative;
                    VectorDouble num4 = (VectorDouble) (((PointDouble) this.GetStaticData().anchorOffset) - mouseCenterPt);
                    this.GetStaticData().anchorOffset = num4;
                    this.UpdateAnchor();
                    PointDouble num5 = this.GetStaticData().anchorCenter;
                }
            }
            base.OnUIDragBegin(sender, e);
        }

        protected override void OnUIDragEnd(object sender, PaintDotNet.UI.Input.MouseEventArgs e)
        {
            if (this.isHandlingDrag)
            {
                if (e.Source is UIElement)
                {
                    PointDouble mouseCenterPt = BrushToolBase<CloneStampTool, CloneStampToolChanges, CloneStampToolUI>.GetMouseCenterPt(e.GetPosition(base.UI), base.CanvasView.CanvasHairWidth);
                    if (this.GetStaticData().anchorOffsetMode == AnchorOffsetMode.Absolute)
                    {
                        this.GetStaticData().anchorOffset = (VectorDouble) mouseCenterPt;
                        this.UpdateAnchor();
                    }
                    else if (this.GetStaticData().anchorOffsetMode == AnchorOffsetMode.NotSet)
                    {
                        string message = PdnResources.GetString("CloneStampTool.Error.TriedToDrawWithoutAnchor.Text");
                        MessageBoxUtil.ErrorBox(base.DocumentWorkspace, message);
                    }
                }
                e.Handled = true;
                this.isHandlingDrag = false;
            }
            base.OnUIDragEnd(sender, e);
        }

        protected override void OnUIDragMove(object sender, PaintDotNet.UI.Input.MouseEventArgs e)
        {
            if (this.isHandlingDrag && (e.Source is UIElement))
            {
                PointDouble mouseCenterPt = BrushToolBase<CloneStampTool, CloneStampToolChanges, CloneStampToolUI>.GetMouseCenterPt(e.GetPosition(base.UI), base.CanvasView.CanvasHairWidth);
                if (this.GetStaticData().anchorOffsetMode == AnchorOffsetMode.Absolute)
                {
                    this.GetStaticData().anchorOffset = (VectorDouble) mouseCenterPt;
                    this.UpdateAnchor();
                }
                e.Handled = true;
            }
            this.UpdateAnchor();
            base.OnUIDragMove(sender, e);
        }

        protected override void OnUIInitialized()
        {
            this.UpdateAnchor();
            this.UpdateUIIsSettingAnchor();
            base.OnUIInitialized();
        }

        private void UpdateAnchor()
        {
            if (base.UI != null)
            {
                base.UI.AnchorRadius = ((double) base.ToolSettings.Pen.Width.Value) / 2.0;
                switch (this.GetStaticData().anchorOffsetMode)
                {
                    case AnchorOffsetMode.NotSet:
                        this.GetStaticData().anchorCenter = new PointDouble(-131072.0, -131072.0);
                        break;

                    case AnchorOffsetMode.Absolute:
                        this.GetStaticData().anchorCenter = (PointDouble) this.GetStaticData().anchorOffset;
                        break;

                    case AnchorOffsetMode.Relative:
                    {
                        PointDouble? nullable = base.PresentationSource.PrimaryMouseDevice.TryGetPosition(base.UI);
                        PointDouble mouseTopLeftPt = nullable.HasValue ? nullable.GetValueOrDefault() : PointDouble.Zero;
                        PointDouble mouseCenterPt = BrushToolBase<CloneStampTool, CloneStampToolChanges, CloneStampToolUI>.GetMouseCenterPt(mouseTopLeftPt, base.CanvasView.CanvasHairWidth);
                        this.GetStaticData().anchorCenter = mouseCenterPt + this.GetStaticData().anchorOffset;
                        break;
                    }
                    default:
                        throw new InternalErrorException(ExceptionUtil.InvalidEnumArgumentException<AnchorOffsetMode>(this.GetStaticData().anchorOffsetMode, "this.anchorOffsetMode"));
                }
                base.UI.AnchorCenter = this.GetStaticData().anchorCenter;
                base.UI.IsAnchorVisible = this.GetStaticData().anchorOffsetMode > AnchorOffsetMode.NotSet;
            }
        }

        private void UpdateUIIsSettingAnchor()
        {
            bool flag;
            if (!base.Active)
            {
                flag = false;
            }
            else if (!base.IsMouseEntered)
            {
                flag = false;
            }
            else if ((base.ModifierKeys & Keys.Control) == Keys.Control)
            {
                flag = true;
            }
            else
            {
                flag = false;
            }
            if (base.UI != null)
            {
                base.UI.IsSettingAnchor = flag;
            }
        }


        private enum AnchorOffsetMode
        {
            NotSet,
            Absolute,
            Relative
        }

        private sealed class CloneStampToolStaticData
        {
            public PointDouble anchorCenter;
            public WeakReferenceT<Layer> anchorLayerWeak;
            public VectorDouble anchorOffset;
            public CloneStampTool.AnchorOffsetMode anchorOffsetMode;
        }
    }
}

