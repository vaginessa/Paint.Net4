namespace PaintDotNet.AppModel
{
    using System;
    using System.Drawing;

    internal class MostRecentFile
    {
        private string path;
        private Image thumb;

        public MostRecentFile(string path, Image thumb)
        {
            this.path = path;
            this.thumb = thumb;
        }

        public string Path =>
            this.path;

        public Image Thumb =>
            this.thumb;
    }
}

