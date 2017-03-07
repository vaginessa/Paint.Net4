namespace PaintDotNet.Dialogs
{
    using PaintDotNet.Drawing;
    using PaintDotNet.Resources;
    using System;

    internal sealed class ResizeDialog : ImageSizeDialog
    {
        public ResizeDialog() : base(ImageSizeDialogType.ImageResize)
        {
            this.Text = PdnResources.GetString("ResizeDialog.Text");
            base.Icon = PdnResources.GetImageResource("Icons.MenuImageResizeIcon.png").Reference.ToIcon();
        }
    }
}

