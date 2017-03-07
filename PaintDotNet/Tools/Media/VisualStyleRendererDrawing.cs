namespace PaintDotNet.Tools.Media
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using System;
    using System.Drawing;
    using System.Windows;
    using System.Windows.Forms.VisualStyles;

    internal sealed class VisualStyleRendererDrawing : GdiPlusDrawing
    {
        public static readonly DependencyProperty ClassNameProperty = DependencyProperty.Register("ClassName", typeof(string), typeof(VisualStyleRendererDrawing), new PropertyMetadata(null));
        public static readonly DependencyProperty FallbackDrawingProperty = DependencyProperty.Register("FallbackDrawing", typeof(GdiPlusDrawing), typeof(VisualStyleRendererDrawing), new PropertyMetadata(null));
        public static readonly DependencyProperty PartProperty = DependencyProperty.Register("Part", typeof(int), typeof(VisualStyleRendererDrawing), new PropertyMetadata(Int32Util.GetBoxed(0)));
        public static readonly DependencyProperty StateProperty = DependencyProperty.Register("State", typeof(int), typeof(VisualStyleRendererDrawing), new PropertyMetadata(Int32Util.GetBoxed(0)));

        protected override Freezable CreateInstanceCore() => 
            new VisualStyleRendererDrawing();

        protected override void OnDraw(Graphics g)
        {
            if (string.IsNullOrWhiteSpace(this.ClassName))
            {
                g.Clear(Color.Transparent);
            }
            else
            {
                SizeInt32 size = base.Size;
                try
                {
                    VisualStyleRenderer renderer = new VisualStyleRenderer(this.ClassName, this.Part, this.State);
                    Rectangle bounds = new Rectangle(0, 0, size.Width, size.Height);
                    renderer.DrawBackground(g, bounds, bounds);
                }
                catch (InvalidOperationException)
                {
                    g.Clear(Color.Transparent);
                    GdiPlusDrawing fallbackDrawing = this.FallbackDrawing;
                    if (fallbackDrawing != null)
                    {
                        fallbackDrawing.Size = size;
                        fallbackDrawing.Draw(g);
                    }
                }
            }
        }

        public string ClassName
        {
            get => 
                ((string) base.GetValue(ClassNameProperty));
            set
            {
                base.SetValue(ClassNameProperty, value);
            }
        }

        public GdiPlusDrawing FallbackDrawing
        {
            get => 
                ((GdiPlusDrawing) base.GetValue(FallbackDrawingProperty));
            set
            {
                base.SetValue(FallbackDrawingProperty, value);
            }
        }

        public int Part
        {
            get => 
                ((int) base.GetValue(PartProperty));
            set
            {
                base.SetValue(PartProperty, Int32Util.GetBoxed(value));
            }
        }

        public int State
        {
            get => 
                ((int) base.GetValue(StateProperty));
            set
            {
                base.SetValue(StateProperty, Int32Util.GetBoxed(value));
            }
        }
    }
}

