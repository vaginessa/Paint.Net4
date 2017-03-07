namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Resources;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.VisualStyling;
    using System;
    using System.Windows.Forms.VisualStyles;

    internal static class ImageStripHelpers
    {
        public static ImageResource GetCloseButtonImageResource(PushButtonState closeButtonState)
        {
            string str;
            if (OS.IsWin8OrLater)
            {
                str = "Metro";
            }
            else
            {
                PdnTheme effectiveTheme = ThemeConfig.EffectiveTheme;
                if (effectiveTheme != PdnTheme.Classic)
                {
                    if (effectiveTheme != PdnTheme.Aero)
                    {
                        throw ExceptionUtil.InvalidEnumArgumentException<PdnTheme>(ThemeConfig.EffectiveTheme, "ThemeConfig.EffectiveTheme");
                    }
                    str = "Aero";
                }
                else
                {
                    str = "Classic";
                }
            }
            string str2 = "." + str + ".png";
            string str3 = closeButtonState.ToString();
            return PdnResources.GetImageResource("Images.ImageStrip.CloseButton." + str3 + str2);
        }
    }
}

