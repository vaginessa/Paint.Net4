namespace PaintDotNet.Settings
{
    using PaintDotNet.Rendering;
    using System;

    internal sealed class PointInt32Setting : Setting<PointInt32>
    {
        private RectInt32 validBounds;

        public PointInt32Setting(string path, SettingScope scope, PointInt32 defaultValue, RectInt32 validBounds) : base(path, scope, defaultValue, PointInt32SettingConverter.Instance)
        {
            this.validBounds = validBounds;
        }

        protected override Setting OnClone() => 
            new PointInt32Setting(base.Path, base.Scope, base.DefaultValue, this.ValidBounds);

        protected override bool OnValidateValueT(PointInt32 potentialValue) => 
            this.validBounds.Contains(potentialValue);

        public RectInt32 ValidBounds =>
            this.validBounds;
    }
}

