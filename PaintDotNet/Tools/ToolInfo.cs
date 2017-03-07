namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using System;

    internal sealed class ToolInfo
    {
        private string displayName;
        private string helpText;
        private char hotKey;
        private ImageResource image;
        private ImageResource largeImage;
        private bool skipIfActiveOnHotKey;
        private Type toolType;

        public ToolInfo(string displayName, string helpText, ImageResource image, ImageResource largeImage, char hotKey, bool skipIfActiveOnHotKey, Type toolType)
        {
            this.displayName = displayName;
            this.helpText = helpText;
            this.image = image;
            this.largeImage = largeImage;
            this.hotKey = hotKey;
            this.skipIfActiveOnHotKey = skipIfActiveOnHotKey;
            this.toolType = toolType;
        }

        public override bool Equals(object obj)
        {
            ToolInfo info = obj as ToolInfo;
            if (info == null)
            {
                return false;
            }
            return ((((this.displayName == info.displayName) && (this.helpText == info.helpText)) && ((this.hotKey == info.hotKey) && (this.skipIfActiveOnHotKey == info.skipIfActiveOnHotKey))) && (this.toolType == info.toolType));
        }

        public override int GetHashCode() => 
            this.displayName.GetHashCode();

        public string DisplayName =>
            this.displayName;

        public string HelpText =>
            this.helpText;

        public char HotKey =>
            this.hotKey;

        public ImageResource Image =>
            this.image;

        public ImageResource LargeImage =>
            this.largeImage;

        public bool SkipIfActiveOnHotKey =>
            this.skipIfActiveOnHotKey;

        public Type ToolType =>
            this.toolType;
    }
}

