namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Resources;
    using PaintDotNet.SystemLayer;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;

    internal class ColorDisplayWidget : UserControl
    {
        private IconBox blackAndWhiteIconBox;
        private IContainer components;
        private ColorRectangleControl primaryColorRectangle;
        private ColorRectangleControl secondaryColorRectangle;
        private IconBox swapIconBox;
        private ToolTip toolTip;
        private ColorBgra userPrimaryColor;
        private ColorBgra userSecondaryColor;

        [field: CompilerGenerated]
        public event EventHandler BlackAndWhiteButtonClicked;

        [field: CompilerGenerated]
        public event EventHandler SwapColorsClicked;

        [field: CompilerGenerated]
        public event EventHandler UserPrimaryColorChanged;

        [field: CompilerGenerated]
        public event EventHandler UserPrimaryColorClick;

        [field: CompilerGenerated]
        public event EventHandler UserSecondaryColorChanged;

        [field: CompilerGenerated]
        public event EventHandler UserSecondaryColorClick;

        public ColorDisplayWidget()
        {
            this.InitializeComponent();
            this.swapIconBox.Icon = new Bitmap(PdnResources.GetImageResource("Icons.SwapIcon.png").Reference);
            this.blackAndWhiteIconBox.Icon = new Bitmap(PdnResources.GetImageResource("Icons.BlackAndWhiteIcon.png").Reference);
            if (!base.DesignMode)
            {
                this.toolTip.SetToolTip(this.swapIconBox, PdnResources.GetString("ColorDisplayWidget.SwapIconBox.ToolTipText"));
                this.toolTip.SetToolTip(this.blackAndWhiteIconBox, PdnResources.GetString("ColorDisplayWidget.BlackAndWhiteIconBox.ToolTipText"));
                this.toolTip.SetToolTip(this.primaryColorRectangle, PdnResources.GetString("ColorDisplayWidget.ForeColorRectangle.ToolTipText"));
                this.toolTip.SetToolTip(this.secondaryColorRectangle, PdnResources.GetString("ColorDisplayWidget.BackColorRectangle.ToolTipText"));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
                this.components = null;
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            this.primaryColorRectangle = new ColorRectangleControl();
            this.secondaryColorRectangle = new ColorRectangleControl();
            this.swapIconBox = new IconBox();
            this.blackAndWhiteIconBox = new IconBox();
            this.toolTip = new ToolTip(this.components);
            base.SuspendLayout();
            this.primaryColorRectangle.Name = "foreColorRectangle";
            this.primaryColorRectangle.RectangleColor = Color.FromArgb(0, 0, 0xc0);
            this.primaryColorRectangle.Size = new Size(0x1c, 0x1c);
            this.primaryColorRectangle.TabIndex = 0;
            this.primaryColorRectangle.Click += new EventHandler(this.OnPrimaryColorRectangleClick);
            this.primaryColorRectangle.KeyUp += new KeyEventHandler(this.OnControlKeyUp);
            this.secondaryColorRectangle.Name = "backColorRectangle";
            this.secondaryColorRectangle.RectangleColor = Color.Magenta;
            this.secondaryColorRectangle.Size = new Size(0x1c, 0x1c);
            this.secondaryColorRectangle.TabIndex = 1;
            this.secondaryColorRectangle.Click += new EventHandler(this.OnSecondaryColorRectangleClick);
            this.secondaryColorRectangle.KeyUp += new KeyEventHandler(this.OnControlKeyUp);
            this.swapIconBox.Icon = null;
            this.swapIconBox.Name = "swapIconBox";
            this.swapIconBox.Size = new Size(0x10, 0x10);
            this.swapIconBox.TabIndex = 2;
            this.swapIconBox.TabStop = false;
            this.swapIconBox.Click += new EventHandler(this.OnSwapIconBoxClick);
            this.swapIconBox.KeyUp += new KeyEventHandler(this.OnControlKeyUp);
            this.swapIconBox.DoubleClick += new EventHandler(this.OnSwapIconBoxClick);
            this.blackAndWhiteIconBox.Icon = null;
            this.blackAndWhiteIconBox.Name = "blackAndWhiteIconBox";
            this.blackAndWhiteIconBox.Size = new Size(0x10, 0x10);
            this.blackAndWhiteIconBox.TabIndex = 3;
            this.blackAndWhiteIconBox.TabStop = false;
            this.blackAndWhiteIconBox.Click += new EventHandler(this.OnBlackAndWhiteIconBoxClick);
            this.blackAndWhiteIconBox.KeyUp += new KeyEventHandler(this.OnControlKeyUp);
            this.blackAndWhiteIconBox.DoubleClick += new EventHandler(this.OnBlackAndWhiteIconBoxClick);
            this.toolTip.ShowAlways = true;
            base.Controls.Add(this.blackAndWhiteIconBox);
            base.Controls.Add(this.swapIconBox);
            base.Controls.Add(this.primaryColorRectangle);
            base.Controls.Add(this.secondaryColorRectangle);
            base.AutoScaleDimensions = new SizeF(96f, 96f);
            base.AutoScaleMode = AutoScaleMode.Dpi;
            base.Name = "ColorDisplayWidget";
            base.Size = new Size(0x30, 0x30);
            base.ResumeLayout(false);
        }

        protected virtual void OnBlackAndWhiteButtonClicked()
        {
            this.BlackAndWhiteButtonClicked.Raise(this);
        }

        private void OnBlackAndWhiteIconBoxClick(object sender, EventArgs e)
        {
            this.OnBlackAndWhiteButtonClicked();
        }

        private void OnControlKeyUp(object sender, KeyEventArgs e)
        {
            this.OnKeyUp(e);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int num = (base.ClientRectangle.Width - UIUtil.ScaleWidth(this.DefaultSize.Width)) / 2;
            int num2 = (base.ClientRectangle.Height - UIUtil.ScaleHeight(this.DefaultSize.Height)) / 2;
            this.primaryColorRectangle.Location = new Point(UIUtil.ScaleWidth((int) (num + 2)), UIUtil.ScaleHeight((int) (num2 + 2)));
            this.secondaryColorRectangle.Location = new Point(UIUtil.ScaleWidth((int) (num + 0x12)), UIUtil.ScaleHeight((int) (num2 + 0x12)));
            this.swapIconBox.Location = new Point(UIUtil.ScaleWidth((int) (num + 30)), UIUtil.ScaleHeight((int) (num2 + 2)));
            this.blackAndWhiteIconBox.Location = new Point(UIUtil.ScaleWidth((int) (num + 2)), UIUtil.ScaleHeight((int) (num2 + 0x1f)));
            base.OnLayout(levent);
        }

        private void OnPrimaryColorRectangleClick(object sender, EventArgs e)
        {
            this.OnUserPrimaryColorClick();
        }

        private void OnSecondaryColorRectangleClick(object sender, EventArgs e)
        {
            this.OnUserSecondaryColorClick();
        }

        protected virtual void OnSwapColorsClicked()
        {
            this.SwapColorsClicked.Raise(this);
        }

        private void OnSwapIconBoxClick(object sender, EventArgs e)
        {
            this.OnSwapColorsClicked();
        }

        protected virtual void OnUserPrimaryColorChanged()
        {
            this.UserPrimaryColorChanged.Raise(this);
        }

        protected virtual void OnUserPrimaryColorClick()
        {
            this.UserPrimaryColorClick.Raise(this);
        }

        protected virtual void OnUserSecondaryColorChanged()
        {
            this.UserSecondaryColorChanged.Raise(this);
        }

        protected virtual void OnUserSecondaryColorClick()
        {
            this.UserSecondaryColorClick.Raise(this);
        }

        protected override Size DefaultSize =>
            new Size(0x30, 0x30);

        public ColorBgra UserPrimaryColor
        {
            get => 
                this.userPrimaryColor;
            set
            {
                ColorBgra userPrimaryColor = this.userPrimaryColor;
                this.userPrimaryColor = value;
                this.primaryColorRectangle.RectangleColor = (Color) value;
                base.Invalidate();
                this.QueueUpdate();
            }
        }

        public ColorBgra UserSecondaryColor
        {
            get => 
                this.userSecondaryColor;
            set
            {
                ColorBgra userSecondaryColor = this.userSecondaryColor;
                this.userSecondaryColor = value;
                this.secondaryColorRectangle.RectangleColor = (Color) value;
                base.Invalidate();
                this.QueueUpdate();
            }
        }
    }
}

