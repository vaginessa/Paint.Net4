namespace PaintDotNet.Effects
{
    using PaintDotNet.AppModel;
    using PaintDotNet.Settings.App;
    using System;
    using System.Collections.Generic;

    internal sealed class PalettesServiceForEffects : IPalettesService
    {
        public IReadOnlyList<ColorBgra> CurrentPalette
        {
            get
            {
                try
                {
                    string paletteString = AppSettings.Instance.Workspace.CurrentPalette.Value;
                    return UserPalettesService.Instance.ParsePaletteString(paletteString);
                }
                catch (Exception)
                {
                    return this.DefaultPalette;
                }
            }
        }

        public IReadOnlyList<ColorBgra> DefaultPalette =>
            UserPalettesService.Instance.DefaultPalette;
    }
}

