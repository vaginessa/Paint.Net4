namespace PaintDotNet.VisualStyling
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Controls.ToolConfigUI;
    using PaintDotNet.Direct2D;
    using PaintDotNet.DirectWrite;
    using PaintDotNet.Drawing;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.UI.Media;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    internal class PdnToolStripRenderer : ToolStripProfessionalRenderer
    {
        private bool drawToolStripExBackgroundTopSeparatorLine = true;
        private Dictionary<ToolStrip, int> dropDownToHSepLeftMap = new Dictionary<ToolStrip, int>();
        private readonly Point[] onRenderArrowPoints3 = new Point[3];
        private PenBrushCache penBrushCache;
        private static readonly ImageResource pluginIndicator = PdnResources.GetImageResource("Icons.PluginIndicator.png");
        private readonly Point[] renderOverflowArrowPoints3 = new Point[3];
        private SolidColorBrush textBrush = new SolidColorBrush();

        public PdnToolStripRenderer()
        {
            base.RoundedEdges = false;
            this.penBrushCache = PenBrushCache.ThreadInstance;
        }

        private void DrawAeroSeparator(Graphics g, Rectangle contentRect)
        {
            int x = contentRect.Left + (contentRect.Width / 2);
            int y = contentRect.Top + 4;
            int num3 = contentRect.Bottom - 5;
            if ((num3 - y) >= 1)
            {
                Point point = new Point(x, y);
                Point point2 = new Point(x, num3);
                using (System.Drawing.Drawing2D.LinearGradientBrush brush = new System.Drawing.Drawing2D.LinearGradientBrush(point, point2, this.AeroColorTheme.SeparatorTopColor, this.AeroColorTheme.SeparatorBottomColor))
                {
                    g.FillRectangle(brush, new Rectangle(x, y, 1, num3 - y));
                }
            }
        }

        public void DrawItemPluginIndicator(Graphics g, ToolStripItem item)
        {
            if (pluginIndicator.Reference != null)
            {
                int height = UIUtil.ScaleHeight(0x10);
                int width = UIUtil.ScaleWidth(10);
                Rectangle destRect = new Rectangle((item.Bounds.Width - width) - 9, (item.Bounds.Height - height) / 2, width, height);
                Image image = null;
                if (!item.Enabled)
                {
                    image = ToolStripRenderer.CreateDisabledImage(pluginIndicator.Reference);
                }
                Image image2 = image ?? pluginIndicator.Reference;
                Rectangle srcRect = new Rectangle(Point.Empty, image2.Size);
                g.DrawImage(image2, destRect, srcRect, GraphicsUnit.Pixel);
                if (image != null)
                {
                    image.Dispose();
                    image = null;
                }
            }
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            Graphics graphics = e.Graphics;
            Rectangle arrowRectangle = e.ArrowRectangle;
            Color color = e.Item.Enabled ? e.ArrowColor : DisabledRendering.GetDisabledColor(e.ArrowColor);
            using (System.Drawing.Brush brush = new SolidBrush(color))
            {
                Point point = new Point(arrowRectangle.Left + (arrowRectangle.Width / 2), arrowRectangle.Top + (arrowRectangle.Height / 2));
                switch (e.Direction)
                {
                    case ArrowDirection.Right:
                        this.onRenderArrowPoints3[0] = new Point(-2, -4);
                        this.onRenderArrowPoints3[1] = new Point(-2, 4);
                        this.onRenderArrowPoints3[2] = new Point(2, 0);
                        break;

                    case ArrowDirection.Left:
                        this.onRenderArrowPoints3[0] = new Point(2, -4);
                        this.onRenderArrowPoints3[1] = new Point(2, 4);
                        this.onRenderArrowPoints3[2] = new Point(-2, 0);
                        break;

                    case ArrowDirection.Up:
                        this.onRenderArrowPoints3[0] = new Point(-3, 2);
                        this.onRenderArrowPoints3[1] = new Point(3, 2);
                        this.onRenderArrowPoints3[2] = new Point(0, -2);
                        break;

                    default:
                        this.onRenderArrowPoints3[0] = new Point(-3, -1);
                        this.onRenderArrowPoints3[1] = new Point(3, -1);
                        this.onRenderArrowPoints3[2] = new Point(0, 2);
                        break;
                }
                for (int i = 0; i < this.onRenderArrowPoints3.Length; i++)
                {
                    this.onRenderArrowPoints3[i] = new Point(point.X + UIUtil.ScaleWidth(this.onRenderArrowPoints3[i].X), point.Y + UIUtil.ScaleHeight(this.onRenderArrowPoints3[i].Y));
                }
                graphics.FillPolygon(brush, this.onRenderArrowPoints3);
            }
        }

        protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
        {
            if (ThemeConfig.EffectiveTheme != PdnTheme.Aero)
            {
                base.OnRenderButtonBackground(e);
            }
            else
            {
                ToolStripButton item = (ToolStripButton) e.Item;
                Rectangle rect = new Rectangle(Point.Empty, e.Item.Size);
                this.RenderAeroButtonBackground(e.Graphics, rect, item.Enabled, item.Selected, item.Pressed, item.Checked);
            }
        }

        protected override void OnRenderDropDownButtonBackground(ToolStripItemRenderEventArgs e)
        {
            if (ThemeConfig.EffectiveTheme != PdnTheme.Aero)
            {
                base.OnRenderDropDownButtonBackground(e);
            }
            else
            {
                ToolStripDropDownButton item = (ToolStripDropDownButton) e.Item;
                Rectangle rect = new Rectangle(Point.Empty, item.Size);
                this.RenderAeroButtonBackground(e.Graphics, rect, true, item.Selected, item.Pressed, false);
            }
        }

        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
        {
            if (ThemeConfig.EffectiveTheme != PdnTheme.Aero)
            {
                base.OnRenderImageMargin(e);
            }
            else if (e.ToolStrip is ToolStripDropDown)
            {
                if (this.dropDownToHSepLeftMap.Count > 100)
                {
                    this.dropDownToHSepLeftMap.Clear();
                }
                int right = e.AffectedBounds.Right;
                e.Graphics.FillRectangle(this.penBrushCache.GetSolidBrush(this.AeroColorTheme.ImageMarginBackgroundColor), e.AffectedBounds);
                int num2 = right;
                e.Graphics.DrawLine(this.penBrushCache.GetPen(this.AeroColorTheme.ImageMarginSeparatorColor1), num2, e.AffectedBounds.Top, num2, e.AffectedBounds.Bottom);
                int num3 = right + 1;
                e.Graphics.DrawLine(this.penBrushCache.GetPen(this.AeroColorTheme.ImageMarginSeparatorColor2), num3, e.AffectedBounds.Top, num3, e.AffectedBounds.Bottom);
                this.dropDownToHSepLeftMap[e.ToolStrip] = num3 - 1;
            }
            else
            {
                base.OnRenderImageMargin(e);
            }
        }

        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            if (ThemeConfig.EffectiveTheme != PdnTheme.Aero)
            {
                base.OnRenderItemCheck(e);
            }
            else
            {
                Rectangle imageRectangle = e.ImageRectangle;
                ToolStripItem item = e.Item;
                Image image = item.Image;
                ToolStripMenuItem item2 = item as ToolStripMenuItem;
                if (item2 != null)
                {
                    Rectangle rect = imageRectangle;
                    rect.Inflate(2, 2);
                    HighlightState state = item.Enabled ? HighlightState.Hover : HighlightState.Disabled;
                    SelectionHighlight.DrawBackground(e.Graphics, this.penBrushCache, rect, state);
                    bool flag = false;
                    if ((image == null) && item2.Checked)
                    {
                        Image reference = PdnResources.GetImageResource("Icons.ToolStrip.Checked.png").Reference;
                        if (item.Enabled)
                        {
                            image = reference;
                            flag = false;
                        }
                        else
                        {
                            image = ToolStripRenderer.CreateDisabledImage(reference);
                            flag = true;
                        }
                    }
                    if (image != null)
                    {
                        Rectangle srcRect = new Rectangle(Point.Empty, image.Size);
                        e.Graphics.DrawImage(image, imageRectangle, srcRect, GraphicsUnit.Pixel);
                    }
                    if (flag)
                    {
                        image.Dispose();
                        image = null;
                    }
                }
            }
        }

        protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
        {
            if (ThemeConfig.EffectiveTheme != PdnTheme.Aero)
            {
                base.OnRenderItemImage(e);
            }
            else
            {
                Rectangle imageRectangle = e.ImageRectangle;
                Image normalImage = e.Image;
                ToolStripItem item = e.Item;
                if (normalImage != null)
                {
                    Image image2 = null;
                    if (!item.Enabled)
                    {
                        image2 = ToolStripRenderer.CreateDisabledImage(normalImage);
                    }
                    Image image = image2 ?? normalImage;
                    Rectangle srcRect = new Rectangle(Point.Empty, image.Size);
                    e.Graphics.DrawImage(image, imageRectangle, srcRect, GraphicsUnit.Pixel);
                    if (image2 != null)
                    {
                        image2.Dispose();
                        image2 = null;
                    }
                }
            }
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Text))
            {
                int num;
                Rectangle textRectangle = e.TextRectangle;
                Rectangle rect = textRectangle;
                if ((e.TextFormat & (TextFormatFlags.Right | TextFormatFlags.HorizontalCenter)) == TextFormatFlags.Default)
                {
                    num = 10;
                }
                else
                {
                    num = 0;
                }
                rect.Width += num;
                using (IDrawingContext context = DrawingContextUtil.FromGraphics(e.Graphics, rect, false, FactorySource.PerThread))
                {
                    HotkeyRenderMode ignore;
                    Color c = (ThemeConfig.EffectiveTheme == PdnTheme.Aero) ? this.AeroColorTheme.MenuTextColor : e.Item.ForeColor;
                    Color color2 = e.Item.Enabled ? c : DisabledRendering.GetDisabledColor(c);
                    this.textBrush.Color = color2;
                    if ((e.TextFormat & TextFormatFlags.NoPrefix) == TextFormatFlags.NoPrefix)
                    {
                        ignore = HotkeyRenderMode.Ignore;
                    }
                    else if ((e.TextFormat & TextFormatFlags.HidePrefix) == TextFormatFlags.HidePrefix)
                    {
                        ignore = HotkeyRenderMode.Hide;
                    }
                    else
                    {
                        ignore = HotkeyRenderMode.Show;
                    }
                    TextLayout textLayout = UIText.CreateLayout(context, e.Text, e.TextFont, null, ignore, (double) textRectangle.Width, (double) textRectangle.Height);
                    if ((e.TextFormat & TextFormatFlags.Right) == TextFormatFlags.Right)
                    {
                        textLayout.TextAlignment = PaintDotNet.DirectWrite.TextAlignment.Trailing;
                    }
                    else if ((e.TextFormat & TextFormatFlags.HorizontalCenter) == TextFormatFlags.HorizontalCenter)
                    {
                        textLayout.TextAlignment = PaintDotNet.DirectWrite.TextAlignment.Center;
                    }
                    if ((e.TextFormat & TextFormatFlags.Bottom) == TextFormatFlags.Bottom)
                    {
                        textLayout.ParagraphAlignment = ParagraphAlignment.Far;
                    }
                    else if ((e.TextFormat & TextFormatFlags.VerticalCenter) == TextFormatFlags.VerticalCenter)
                    {
                        textLayout.ParagraphAlignment = ParagraphAlignment.Center;
                    }
                    textLayout.WordWrapping = WordWrapping.NoWrap;
                    UIText.AdjustFontSizeToFitLayoutSize(context, textLayout, (double) textRectangle.Width, 65535.0, 0.6);
                    if (e.Item is PdnToolStripStatusLabel)
                    {
                        textLayout.TrimmingGranularity = TrimmingGranularity.Character;
                        textLayout.TrimmingStyle = TextTrimmingStyle.Ellipsis;
                    }
                    context.DrawTextLayout((double) rect.X, (double) rect.Y, textLayout, this.textBrush, DrawTextOptions.None);
                }
            }
        }

        protected override void OnRenderLabelBackground(ToolStripItemRenderEventArgs e)
        {
            if (e.Item is ShapeCategoryLabel)
            {
                Rectangle rect = new Rectangle(Point.Empty, e.Item.Size);
                using (System.Drawing.Drawing2D.LinearGradientBrush brush = new System.Drawing.Drawing2D.LinearGradientBrush(rect, this.AeroColorTheme.StatusBackFillGradBottomColor, this.AeroColorTheme.StatusBackFillGradBottomColor, LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(brush, rect);
                }
                e.Graphics.DrawLine(this.penBrushCache.GetPen(this.AeroColorTheme.StatusBorderColor1), new Point(rect.Left, rect.Bottom), new Point(rect.Right, rect.Bottom));
            }
            else
            {
                base.OnRenderLabelBackground(e);
            }
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (ThemeConfig.EffectiveTheme != PdnTheme.Aero)
            {
                base.OnRenderMenuItemBackground(e);
            }
            else
            {
                HighlightState disabled;
                ToolStripItem item = e.Item;
                Rectangle rect = new Rectangle(0, 0, item.Width, item.Height);
                if (item.IsOnDropDown)
                {
                    rect = Rectangle.Inflate(rect, -1, -1);
                    rect.X += 2;
                    rect.Width -= 3;
                }
                if (!e.Item.Enabled && e.Item.Selected)
                {
                    disabled = HighlightState.Disabled;
                }
                else if (e.Item.Pressed && e.Item.IsOnDropDown)
                {
                    disabled = HighlightState.Hover;
                }
                else if (e.Item.Pressed)
                {
                    disabled = HighlightState.Hover;
                }
                else if (e.Item.Selected)
                {
                    disabled = HighlightState.Hover;
                }
                else
                {
                    disabled = HighlightState.Default;
                }
                SelectionHighlight.DrawBackground(e.Graphics, this.penBrushCache, rect, disabled);
            }
        }

        protected override void OnRenderOverflowButtonBackground(ToolStripItemRenderEventArgs e)
        {
            if (ThemeConfig.EffectiveTheme != PdnTheme.Aero)
            {
                base.OnRenderOverflowButtonBackground(e);
            }
            else
            {
                this.RenderAeroButtonBackground(e.Graphics, new Rectangle(1, 0, e.Item.Width - 1, e.Item.Height), true, e.Item.Selected, e.Item.Pressed, false);
                this.RenderOverflowArrowGlyph(e);
            }
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            if (ThemeConfig.EffectiveTheme != PdnTheme.Aero)
            {
                base.OnRenderSeparator(e);
            }
            else if (!(e.Item.Owner is StatusStrip) || (e.Item.DisplayStyle != ToolStripItemDisplayStyle.None))
            {
                if ((e.Item.IsOnDropDown && (e.ToolStrip is ToolStripDropDownMenu)) && !e.Vertical)
                {
                    int left;
                    if (!this.dropDownToHSepLeftMap.TryGetValue(e.ToolStrip, out left))
                    {
                        left = e.Item.ContentRectangle.Left;
                    }
                    int right = e.Item.ContentRectangle.Right;
                    int num3 = ((e.Item.ContentRectangle.Top + e.Item.ContentRectangle.Bottom) / 2) - 1;
                    e.Graphics.DrawLine(this.penBrushCache.GetPen(this.AeroColorTheme.MenuSeparatorColor1), left, num3, right, num3);
                    int num4 = num3 + 1;
                    e.Graphics.DrawLine(this.penBrushCache.GetPen(this.AeroColorTheme.MenuSeparatorColor2), left, num4, right, num4);
                }
                else if (e.Vertical && !e.Item.IsOnDropDown)
                {
                    this.DrawAeroSeparator(e.Graphics, e.Item.ContentRectangle);
                }
                else
                {
                    base.OnRenderSeparator(e);
                }
            }
        }

        protected override void OnRenderSplitButtonBackground(ToolStripItemRenderEventArgs e)
        {
            if (ThemeConfig.EffectiveTheme != PdnTheme.Aero)
            {
                base.OnRenderSplitButtonBackground(e);
            }
            else
            {
                ToolStripSplitButton item = (ToolStripSplitButton) e.Item;
                Rectangle rect = new Rectangle(Point.Empty, item.Size);
                this.RenderAeroButtonBackground(e.Graphics, rect, true, item.Selected, item.Pressed, false);
                ArrowDirection arrowDirection = (item.Owner is StatusStrip) ? ArrowDirection.Up : ArrowDirection.Down;
                base.DrawArrow(new ToolStripArrowRenderEventArgs(e.Graphics, item, item.DropDownButtonBounds, this.AeroColorTheme.SplitButtonArrowColor, arrowDirection));
                if (item.Selected || item.Pressed)
                {
                    this.DrawAeroSeparator(e.Graphics, new Rectangle(item.DropDownButtonBounds.Left, item.DropDownButtonBounds.Top, 1, item.DropDownButtonBounds.Height));
                }
            }
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            if (ThemeConfig.EffectiveTheme != PdnTheme.Aero)
            {
                base.OnRenderToolStripBackground(e);
            }
            else if (e.ToolStrip is ToolStripDropDown)
            {
                e.Graphics.FillRectangle(this.penBrushCache.GetSolidBrush(this.AeroColorTheme.MenuItemBackFillColor), e.AffectedBounds);
            }
            else
            {
                if (e.ToolStrip is StatusStrip)
                {
                    Rectangle affectedBounds = e.AffectedBounds;
                    int num = affectedBounds.Y + 1;
                    affectedBounds.Y = num;
                    num = affectedBounds.Height - 1;
                    affectedBounds.Height = num;
                    using (System.Drawing.Drawing2D.LinearGradientBrush brush = new System.Drawing.Drawing2D.LinearGradientBrush(e.AffectedBounds, this.AeroColorTheme.StatusBackFillGradTopColor, this.AeroColorTheme.StatusBackFillGradBottomColor, LinearGradientMode.Vertical))
                    {
                        e.Graphics.FillRectangle(brush, e.AffectedBounds);
                        return;
                    }
                }
                if (e.ToolStrip is ToolStripEx)
                {
                    Rectangle rect = e.AffectedBounds;
                    e.Graphics.TryFillRectangle(this.penBrushCache.GetSolidBrush(this.AeroColorTheme.ToolBarBackFillGradMidColor), rect);
                    if (this.DrawToolStripExBackgroundTopSeparatorLine)
                    {
                        e.Graphics.DrawLine(this.penBrushCache.GetPen(this.AeroColorTheme.ToolBarOutlineColor), e.AffectedBounds.Left, e.AffectedBounds.Top, e.AffectedBounds.Right, e.AffectedBounds.Top);
                    }
                }
                else
                {
                    base.OnRenderToolStripBackground(e);
                }
            }
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            if (ThemeConfig.EffectiveTheme != PdnTheme.Aero)
            {
                base.OnRenderToolStripBorder(e);
            }
            else if (e.ToolStrip is ToolStripDropDown)
            {
                Rectangle affectedBounds = e.AffectedBounds;
                int num = affectedBounds.Width - 1;
                affectedBounds.Width = num;
                num = affectedBounds.Height - 1;
                affectedBounds.Height = num;
                e.Graphics.DrawRectangle(this.penBrushCache.GetPen(this.AeroColorTheme.BorderOuterColor), affectedBounds);
                Rectangle rect = e.AffectedBounds;
                rect.Inflate(-1, -1);
                num = rect.Width - 1;
                rect.Width = num;
                num = rect.Height - 1;
                rect.Height = num;
                e.Graphics.DrawRectangle(this.penBrushCache.GetPen(this.AeroColorTheme.BorderInnerColor), rect);
            }
            else
            {
                StatusStrip toolStrip = e.ToolStrip as StatusStrip;
            }
        }

        private void RenderAeroButtonBackground(Graphics g, Rectangle rect, bool isEnabled, bool isSelected, bool isPressed, bool isChecked)
        {
            HighlightState hover;
            if (isPressed)
            {
                if (isEnabled)
                {
                    hover = HighlightState.Checked;
                }
                else
                {
                    hover = HighlightState.Default;
                }
            }
            else if (isSelected)
            {
                if (isEnabled)
                {
                    hover = HighlightState.Hover;
                }
                else
                {
                    hover = HighlightState.Disabled;
                }
            }
            else if (isChecked)
            {
                if (isEnabled)
                {
                    hover = HighlightState.Checked;
                }
                else
                {
                    hover = HighlightState.Disabled;
                }
            }
            else
            {
                hover = HighlightState.Default;
            }
            SelectionHighlight.DrawBackground(g, this.penBrushCache, rect, hover);
        }

        private void RenderOverflowArrow(Graphics g, Rectangle glyphRect, System.Drawing.Brush brush)
        {
            Point point = new Point(glyphRect.Left + (glyphRect.Width / 2), glyphRect.Top + (glyphRect.Height / 2));
            point.X += glyphRect.Width % 2;
            this.renderOverflowArrowPoints3[0] = new Point(point.X - 2, point.Y - 1);
            this.renderOverflowArrowPoints3[1] = new Point(point.X + 3, point.Y - 1);
            this.renderOverflowArrowPoints3[2] = new Point(point.X, point.Y + 2);
            g.FillPolygon(brush, this.renderOverflowArrowPoints3);
        }

        private void RenderOverflowArrowGlyph(ToolStripItemRenderEventArgs e)
        {
            Graphics g = e.Graphics;
            ToolStripItem item = e.Item;
            Rectangle glyphRect = new Rectangle(item.Width - 12, item.Height - 8, 9, 5);
            glyphRect.Offset(1, 1);
            this.RenderOverflowArrow(g, glyphRect, System.Drawing.SystemBrushes.ButtonHighlight);
            g.DrawLine(SystemPens.ButtonHighlight, (int) (glyphRect.Right - 6), (int) (glyphRect.Y - 2), (int) (glyphRect.Right - 2), (int) (glyphRect.Y - 2));
            glyphRect.Offset(-1, -1);
            this.RenderOverflowArrow(g, glyphRect, System.Drawing.SystemBrushes.ControlText);
            g.DrawLine(SystemPens.ControlText, (int) (glyphRect.Right - 6), (int) (glyphRect.Y - 2), (int) (glyphRect.Right - 2), (int) (glyphRect.Y - 2));
        }

        public virtual PaintDotNet.VisualStyling.AeroColorTheme AeroColorTheme =>
            AeroColors.CurrentTheme;

        public bool DrawToolStripExBackgroundTopSeparatorLine
        {
            get => 
                this.drawToolStripExBackgroundTopSeparatorLine;
            set
            {
                this.drawToolStripExBackgroundTopSeparatorLine = value;
            }
        }
    }
}

