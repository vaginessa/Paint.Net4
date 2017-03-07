namespace PaintDotNet.Controls
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal sealed class IconBox : UserControl
    {
        private Bitmap icon;
        private Bitmap renderSurface;

        public IconBox()
        {
            base.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            this.InitializeComponent();
            base.ResizeRedraw = true;
        }

        private void InitializeComponent()
        {
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(this.BackColor);
            Rectangle srcRect = new Rectangle(new Point(0, 0), this.icon.Size);
            Rectangle destRect = new Rectangle(new Point(0, 0), base.ClientSize);
            e.Graphics.DrawImage(this.Icon, destRect, srcRect, GraphicsUnit.Pixel);
            base.OnPaint(e);
        }

        public Bitmap Icon
        {
            get => 
                this.icon;
            set
            {
                if (value == null)
                {
                    value = new Bitmap(1, 1);
                    using (Graphics graphics = Graphics.FromImage(value))
                    {
                        graphics.Clear(Color.Transparent);
                    }
                }
                this.icon = value;
                if (this.renderSurface != null)
                {
                    this.renderSurface.Dispose();
                }
                this.renderSurface = null;
                base.Invalidate();
            }
        }
    }
}

