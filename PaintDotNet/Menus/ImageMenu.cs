namespace PaintDotNet.Menus
{
    using PaintDotNet;
    using PaintDotNet.Actions;
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryFunctions;
    using PaintDotNet.Resources;
    using System;
    using System.Windows.Forms;

    internal sealed class ImageMenu : PdnMenuItem
    {
        private PdnMenuItem menuImageCanvasSize;
        private PdnMenuItem menuImageCrop;
        private PdnMenuItem menuImageFlatten;
        private PdnMenuItem menuImageFlipHorizontal;
        private PdnMenuItem menuImageFlipVertical;
        private PdnMenuItem menuImageResize;
        private PdnMenuItem menuImageRotate180;
        private PdnMenuItem menuImageRotate90CCW;
        private PdnMenuItem menuImageRotate90CW;
        private ToolStripSeparator menuImageSeparator1;
        private ToolStripSeparator menuImageSeparator2;
        private ToolStripSeparator menuImageSeparator3;

        public ImageMenu()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.menuImageCrop = new PdnMenuItem();
            this.menuImageResize = new PdnMenuItem();
            this.menuImageCanvasSize = new PdnMenuItem();
            this.menuImageSeparator1 = new ToolStripSeparator();
            this.menuImageFlipHorizontal = new PdnMenuItem();
            this.menuImageFlipVertical = new PdnMenuItem();
            this.menuImageSeparator2 = new ToolStripSeparator();
            this.menuImageRotate90CW = new PdnMenuItem();
            this.menuImageRotate90CCW = new PdnMenuItem();
            this.menuImageRotate180 = new PdnMenuItem();
            this.menuImageSeparator3 = new ToolStripSeparator();
            this.menuImageFlatten = new PdnMenuItem();
            ToolStripItem[] toolStripItems = new ToolStripItem[] { this.menuImageCrop, this.menuImageResize, this.menuImageCanvasSize, this.menuImageSeparator1, this.menuImageFlipHorizontal, this.menuImageFlipVertical, this.menuImageSeparator2, this.menuImageRotate90CW, this.menuImageRotate90CCW, this.menuImageRotate180, this.menuImageSeparator3, this.menuImageFlatten };
            base.DropDownItems.AddRange(toolStripItems);
            base.Name = "Menu.Image";
            this.Text = PdnResources.GetString("Menu.Image.Text");
            this.menuImageCrop.Name = "Crop";
            this.menuImageCrop.Click += new EventHandler(this.OnMenuImageCropClick);
            this.menuImageCrop.ShortcutKeys = Keys.Control | Keys.Shift | Keys.X;
            this.menuImageResize.Name = "Resize";
            this.menuImageResize.ShortcutKeys = Keys.Control | Keys.R;
            this.menuImageResize.Click += new EventHandler(this.OnMenuImageResizeClick);
            this.menuImageCanvasSize.Name = "CanvasSize";
            this.menuImageCanvasSize.ShortcutKeys = Keys.Control | Keys.Shift | Keys.R;
            this.menuImageCanvasSize.Click += new EventHandler(this.OnMenuImageCanvasSizeClick);
            this.menuImageFlipHorizontal.Name = "FlipHorizontal";
            this.menuImageFlipHorizontal.Click += new EventHandler(this.OnMenuImageFlipHorizontalClick);
            this.menuImageFlipVertical.Name = "FlipVertical";
            this.menuImageFlipVertical.Click += new EventHandler(this.OnMenuImageFlipVerticalClick);
            this.menuImageRotate90CW.Name = "Rotate90CW";
            this.menuImageRotate90CW.ShortcutKeys = Keys.Control | Keys.H;
            this.menuImageRotate90CW.Click += new EventHandler(this.OnMenuImageRotate90CWClick);
            this.menuImageRotate90CCW.Name = "Rotate90CCW";
            this.menuImageRotate90CCW.ShortcutKeys = Keys.Control | Keys.G;
            this.menuImageRotate90CCW.Click += new EventHandler(this.OnMenuImageRotate90CCWClick);
            this.menuImageRotate180.Name = "Rotate180";
            this.menuImageRotate180.Click += new EventHandler(this.OnMenuImageRotate180Click);
            this.menuImageRotate180.ShortcutKeys = Keys.Control | Keys.J;
            this.menuImageFlatten.Name = "Flatten";
            this.menuImageFlatten.ShortcutKeys = Keys.Control | Keys.Shift | Keys.F;
            this.menuImageFlatten.Click += new EventHandler(this.OnMenuImageFlattenClick);
        }

        protected override void OnDropDownOpening(EventArgs e)
        {
            if (base.AppWorkspace.ActiveDocumentWorkspace == null)
            {
                this.menuImageCrop.Enabled = false;
                this.menuImageResize.Enabled = false;
                this.menuImageCanvasSize.Enabled = false;
                this.menuImageFlipHorizontal.Enabled = false;
                this.menuImageFlipVertical.Enabled = false;
                this.menuImageRotate90CW.Enabled = false;
                this.menuImageRotate90CCW.Enabled = false;
                this.menuImageRotate180.Enabled = false;
                this.menuImageFlatten.Enabled = false;
            }
            else
            {
                this.menuImageCrop.Enabled = !base.AppWorkspace.ActiveDocumentWorkspace.Selection.IsEmpty;
                this.menuImageResize.Enabled = true;
                this.menuImageCanvasSize.Enabled = true;
                this.menuImageFlipHorizontal.Enabled = true;
                this.menuImageFlipVertical.Enabled = true;
                this.menuImageRotate90CW.Enabled = true;
                this.menuImageRotate90CCW.Enabled = true;
                this.menuImageRotate180.Enabled = true;
                this.menuImageFlatten.Enabled = base.AppWorkspace.ActiveDocumentWorkspace.Document.Layers.Count > 1;
            }
            base.OnDropDownOpening(e);
        }

        private void OnMenuImageCanvasSizeClick(object sender, EventArgs e)
        {
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                base.AppWorkspace.ActiveDocumentWorkspace.PerformAction(new CanvasSizeAction());
            }
        }

        private void OnMenuImageCropClick(object sender, EventArgs e)
        {
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                CropToSelectionAction.PerformAction(base.AppWorkspace.ActiveDocumentWorkspace);
            }
        }

        private void OnMenuImageFlattenClick(object sender, EventArgs e)
        {
            if ((base.AppWorkspace.ActiveDocumentWorkspace != null) && (base.AppWorkspace.ActiveDocumentWorkspace.Document.Layers.Count > 1))
            {
                base.AppWorkspace.ActiveDocumentWorkspace.ApplyFunction(new FlattenFunction());
            }
        }

        private void OnMenuImageFlipHorizontalClick(object sender, EventArgs e)
        {
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                base.AppWorkspace.ActiveDocumentWorkspace.ApplyFunction(new FlipDocumentFunction(FlipType.Horizontal));
            }
        }

        private void OnMenuImageFlipVerticalClick(object sender, EventArgs e)
        {
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                base.AppWorkspace.ActiveDocumentWorkspace.ApplyFunction(new FlipDocumentFunction(FlipType.Vertical));
            }
        }

        private void OnMenuImageResizeClick(object sender, EventArgs e)
        {
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                base.AppWorkspace.ActiveDocumentWorkspace.PerformAction(new ResizeAction());
            }
        }

        private void OnMenuImageRotate180Click(object sender, EventArgs e)
        {
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                HistoryFunction function = new RotateDocumentFunction(RotateType.Rotate180);
                base.AppWorkspace.ActiveDocumentWorkspace.ApplyFunction(function);
            }
        }

        private void OnMenuImageRotate90CCWClick(object sender, EventArgs e)
        {
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                HistoryFunction function = new RotateDocumentFunction(RotateType.CounterClockwise90);
                base.AppWorkspace.ActiveDocumentWorkspace.ApplyFunction(function);
            }
        }

        private void OnMenuImageRotate90CWClick(object sender, EventArgs e)
        {
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                HistoryFunction function = new RotateDocumentFunction(RotateType.Clockwise90);
                base.AppWorkspace.ActiveDocumentWorkspace.ApplyFunction(function);
            }
        }
    }
}

