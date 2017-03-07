namespace PaintDotNet.Dialogs
{
    using PaintDotNet.Drawing;
    using PaintDotNet.Resources;
    using System;

    internal sealed class NewFileDialog : ImageSizeDialog
    {
        public NewFileDialog() : base(ImageSizeDialogType.FileNew)
        {
            this.Text = PdnResources.GetString("NewFileDialog.Text");
            base.Icon = PdnResources.GetImageResource("Icons.MenuFileNewIcon.png").Reference.ToIcon();
        }
    }
}

