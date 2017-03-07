namespace PaintDotNet.Settings
{
    using PaintDotNet.Rendering;
    using System;

    internal sealed class RectInt32Setting : Setting<RectInt32>
    {
        private RectInt32 validBounds;

        public RectInt32Setting(string path, SettingScope scope, RectInt32 defaultValue, RectInt32 validBounds) : base(path, scope, defaultValue, RectInt32SettingConverter.Instance)
        {
            this.validBounds = validBounds;
        }

        protected override Setting OnClone() => 
            new RectInt32Setting(base.Path, base.Scope, base.DefaultValue, this.ValidBounds);

        protected override bool OnValidateValueT(RectInt32 potentialValue) => 
            potentialValue.IntersectsWith(this.validBounds);

        public RectInt32 ValidBounds =>
            this.validBounds;
    }
}

