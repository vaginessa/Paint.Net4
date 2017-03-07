namespace PaintDotNet.Settings.App
{
    using PaintDotNet;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.DirectWrite;
    using PaintDotNet.Imaging;
    using PaintDotNet.Settings;
    using PaintDotNet.UI.Media;
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal static class AppSettingsToolsSectionExtensions
    {
        public static PaintDotNet.UI.Media.Brush CreateBrush(this AppSettings.ToolsSection toolSettings, bool swapColors)
        {
            ColorBgra32 a = toolSettings.PrimaryColor.Value;
            ColorBgra32 b = toolSettings.SecondaryColor.Value;
            if (swapColors)
            {
                ObjectUtil.Swap<ColorBgra32>(ref a, ref b);
            }
            return toolSettings.CreateBrush(a, b);
        }

        public static PaintDotNet.UI.Media.Brush CreateBrush(this AppSettings.ToolsSection toolSettings, ColorBgra32 foreColor, ColorBgra32 backColor)
        {
            PaintDotNet.BrushType type = toolSettings.Brush.Type.Value;
            PaintDotNet.UI.Media.HatchStyle hatchStyle = toolSettings.Brush.HatchStyle.Value;
            switch (type)
            {
                case PaintDotNet.BrushType.Solid:
                    return new SolidColorBrush((ColorRgba128Float) foreColor);

                case PaintDotNet.BrushType.Hatch:
                    return new PaintDotNet.UI.Media.HatchBrush(hatchStyle, (ColorRgba128Float) foreColor, (ColorRgba128Float) backColor);
            }
            throw new InvalidOperationException("BrushType is invalid");
        }

        public static System.Drawing.Brush CreateGdipBrush(this AppSettings.ToolsSection toolSettings, bool swapColors)
        {
            ColorBgra32 a = toolSettings.PrimaryColor.Value;
            ColorBgra32 b = toolSettings.SecondaryColor.Value;
            if (swapColors)
            {
                ObjectUtil.Swap<ColorBgra32>(ref a, ref b);
            }
            return toolSettings.CreateGdipBrush(a, b);
        }

        public static System.Drawing.Brush CreateGdipBrush(this AppSettings.ToolsSection toolSettings, ColorBgra32 foreColor, ColorBgra32 backColor)
        {
            PaintDotNet.BrushType type = toolSettings.Brush.Type.Value;
            System.Drawing.Drawing2D.HatchStyle hatchstyle = toolSettings.Brush.HatchStyle.Value;
            switch (type)
            {
                case PaintDotNet.BrushType.Solid:
                    return new SolidBrush((Color) foreColor);

                case PaintDotNet.BrushType.Hatch:
                    return new System.Drawing.Drawing2D.HatchBrush(hatchstyle, (Color) foreColor, (Color) backColor);
            }
            throw new InvalidOperationException("BrushType is invalid");
        }

        public static System.Drawing.Pen CreatePen(this AppSettings.ToolsSection toolSettings, ColorBgra32 foreColor, ColorBgra32 backColor)
        {
            System.Drawing.Pen pen;
            LineCap cap3;
            CustomLineCap cap4;
            LineCap cap5;
            CustomLineCap cap6;
            float width = toolSettings.Pen.Width.Value;
            LineCap2 cap = toolSettings.Pen.StartCap.Value;
            LineCap2 cap2 = toolSettings.Pen.EndCap.Value;
            System.Drawing.Drawing2D.DashStyle style = toolSettings.Pen.DashStyle.Value;
            if (((PaintDotNet.BrushType) toolSettings.Brush.Type.Value) == PaintDotNet.BrushType.None)
            {
                pen = new System.Drawing.Pen((Color) foreColor, width);
            }
            else
            {
                pen = new System.Drawing.Pen(toolSettings.CreateGdipBrush(foreColor, backColor), width);
            }
            LineCapToLineCap2(cap, out cap3, out cap4);
            if (cap4 != null)
            {
                pen.CustomStartCap = cap4;
            }
            else
            {
                pen.StartCap = cap3;
            }
            LineCapToLineCap2(cap2, out cap5, out cap6);
            if (cap6 != null)
            {
                pen.CustomEndCap = cap6;
            }
            else
            {
                pen.EndCap = cap5;
            }
            pen.DashStyle = style;
            return pen;
        }

        public static SizedFontProperties CreateSizedFontProperties(this AppSettings.ToolsSection toolSettings, IFontMap fontMap)
        {
            FontProperties fontProperties;
            FontWeight bold;
            PaintDotNet.DirectWrite.FontStyle italic;
            string displayName = toolSettings.Text.FontFamilyName.Value;
            string[] namesToTry = new string[] { displayName, "Segoe UI", "Arial" };
            try
            {
                fontProperties = fontMap.GetFontProperties(namesToTry);
            }
            catch (NoFontException)
            {
                fontProperties = new FontProperties(displayName, string.Empty, FontWeight.Normal, FontStretch.Normal, PaintDotNet.DirectWrite.FontStyle.Normal, TextDecorations.None);
            }
            System.Drawing.FontStyle style = toolSettings.Text.FontStyle.Value;
            FontWeight weight = fontProperties.Weight;
            if (style.HasFlag(System.Drawing.FontStyle.Bold) && (weight <= FontWeight.DemiBold))
            {
                bold = FontWeight.Bold;
            }
            else
            {
                bold = weight;
            }
            FontStretch stretch = fontProperties.Stretch;
            PaintDotNet.DirectWrite.FontStyle style2 = fontProperties.Style;
            if (style.HasFlag(System.Drawing.FontStyle.Italic) && (style2 == PaintDotNet.DirectWrite.FontStyle.Normal))
            {
                italic = PaintDotNet.DirectWrite.FontStyle.Italic;
            }
            else
            {
                italic = style2;
            }
            TextDecorations decorations = (fontProperties.Decorations | (style.HasFlag(System.Drawing.FontStyle.Underline) ? TextDecorations.Underline : TextDecorations.None)) | (style.HasFlag(System.Drawing.FontStyle.Strikeout) ? TextDecorations.Strikethrough : TextDecorations.None);
            float dipSize = (toolSettings.Text.FontSize.Value * 96f) / 72f;
            return new SizedFontProperties(new FontProperties(fontProperties.DisplayName, fontProperties.FontFamilyName, bold, stretch, italic, decorations), dipSize);
        }

        private static void LineCapToLineCap2(LineCap2 cap2, out LineCap capResult, out CustomLineCap customCapResult)
        {
            switch (cap2)
            {
                case LineCap2.Flat:
                    capResult = LineCap.Flat;
                    customCapResult = null;
                    return;

                case LineCap2.Arrow:
                    capResult = LineCap.ArrowAnchor;
                    customCapResult = new AdjustableArrowCap(5f, 5f, false);
                    return;

                case LineCap2.ArrowFilled:
                    capResult = LineCap.ArrowAnchor;
                    customCapResult = new AdjustableArrowCap(5f, 5f, true);
                    return;

                case LineCap2.Rounded:
                    capResult = LineCap.Round;
                    customCapResult = null;
                    return;
            }
            throw ExceptionUtil.InvalidEnumArgumentException<LineCap2>(cap2, "cap2");
        }

        public static void LoadFrom(this AppSettings.ToolsSection dstSettings, AppSettings.ToolsSection srcSettings)
        {
            Validate.IsNotNull<AppSettings.ToolsSection>(srcSettings, "srcSettings");
            if (dstSettings != srcSettings)
            {
                string[] pathComponents = SettingPath.GetPathComponents(dstSettings.Path);
                string[] strArray2 = SettingPath.GetPathComponents(srcSettings.Path);
                foreach (Setting setting in dstSettings.Settings)
                {
                    string[] strArray3 = SettingPath.GetPathComponents(setting.Path);
                    string[] strArray4 = new string[(strArray3.Length - pathComponents.Length) + strArray2.Length];
                    int index = 0;
                    for (int i = 0; i < strArray2.Length; i++)
                    {
                        strArray4[index] = strArray2[i];
                        index++;
                    }
                    for (int j = pathComponents.Length; j < strArray3.Length; j++)
                    {
                        strArray4[index] = strArray3[j];
                        index++;
                    }
                    string str3 = SettingPath.CombinePathComponents(strArray4);
                    Setting setting2 = srcSettings[str3];
                    setting.Value = setting2.Value;
                }
            }
        }
    }
}

