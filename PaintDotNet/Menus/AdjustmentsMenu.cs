namespace PaintDotNet.Menus
{
    using PaintDotNet.Effects;
    using PaintDotNet.Resources;
    using System;
    using System.Windows.Forms;

    internal sealed class AdjustmentsMenu : EffectMenuBase
    {
        public AdjustmentsMenu()
        {
            this.InitializeComponent();
        }

        protected override bool FilterEffects(Effect effect) => 
            (effect.Category == EffectCategory.Adjustment);

        protected override Keys GetEffectShortcutKeys(Effect effect)
        {
            if (effect is DesaturateEffect)
            {
                return (Keys.Control | Keys.Shift | Keys.G);
            }
            if (effect is AutoLevelEffect)
            {
                return (Keys.Control | Keys.Shift | Keys.L);
            }
            if (effect is InvertColorsEffect)
            {
                return (Keys.Control | Keys.Shift | Keys.I);
            }
            if (effect is HueAndSaturationAdjustment)
            {
                return (Keys.Control | Keys.Shift | Keys.U);
            }
            if (effect is SepiaEffect)
            {
                return (Keys.Control | Keys.Shift | Keys.E);
            }
            if (effect is BrightnessAndContrastAdjustment)
            {
                return (Keys.Control | Keys.Shift | Keys.T);
            }
            if (effect is LevelsEffect)
            {
                return (Keys.Control | Keys.L);
            }
            if (effect is CurvesEffect)
            {
                return (Keys.Control | Keys.Shift | Keys.M);
            }
            if (effect is PosterizeAdjustment)
            {
                return (Keys.Control | Keys.Shift | Keys.P);
            }
            return Keys.None;
        }

        private void InitializeComponent()
        {
            base.Name = "Menu.Layers.Adjustments";
            this.Text = PdnResources.GetString("Menu.Layers.Adjustments.Text");
        }

        protected override bool EnableEffectShortcuts =>
            true;

        protected override bool EnableRepeatEffectMenuItem =>
            false;
    }
}

