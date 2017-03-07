namespace PaintDotNet.Menus
{
    using PaintDotNet.Effects;
    using PaintDotNet.Resources;
    using System;

    internal sealed class EffectsMenu : EffectMenuBase
    {
        public EffectsMenu()
        {
            this.InitializeComponent();
        }

        protected override bool FilterEffects(Effect effect) => 
            (effect.Category == EffectCategory.Effect);

        private void InitializeComponent()
        {
            base.Name = "Menu.Effects";
            this.Text = PdnResources.GetString("Menu.Effects.Text");
        }

        protected override bool EnableEffectShortcuts =>
            false;

        protected override bool EnableRepeatEffectMenuItem =>
            true;
    }
}

