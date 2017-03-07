namespace PaintDotNet.Dialogs
{
    using PaintDotNet.Drawing;
    using PaintDotNet.Resources;
    using System;

    internal sealed class CanvasSizeDialog : ImageSizeDialog
    {
        public CanvasSizeDialog() : base(ImageSizeDialogType.ImageCanvasSize)
        {
            this.Text = PdnResources.GetString("CanvasSizeDialog.Text");
            base.Icon = PdnResources.GetImageResource("Icons.MenuImageCanvasSizeIcon.png").Reference.ToIcon();
        }
    }
}

