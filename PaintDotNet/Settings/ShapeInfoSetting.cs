namespace PaintDotNet.Settings
{
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Shapes;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal sealed class ShapeInfoSetting : StaticListChoiceSetting<ShapeInfo>
    {
        private Func<IEnumerable<ShapeInfo>> shapeInfoListSource;

        public ShapeInfoSetting(string path, SettingScope scope, ShapeInfo defaultValue) : this(path, scope, defaultValue, () => ShapeManager.GetShapeInfos())
        {
        }

        public ShapeInfoSetting(string path, SettingScope scope, ShapeInfo defaultValue, Func<IEnumerable<ShapeInfo>> shapeInfoListSource) : base(path, scope, defaultValue, SerializableObjectSettingConverter<ShapeInfo>.Instance)
        {
            Validate.IsNotNull<Func<IEnumerable<ShapeInfo>>>(shapeInfoListSource, "shapeInfoListSource");
            this.shapeInfoListSource = shapeInfoListSource;
        }

        protected override Setting OnClone() => 
            new ShapeInfoSetting(base.Path, base.Scope, base.DefaultValue, this.shapeInfoListSource);

        protected override IEnumerable<ShapeInfo> OnGetValueChoices() => 
            this.shapeInfoListSource();

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly ShapeInfoSetting.<>c <>9 = new ShapeInfoSetting.<>c();
            public static Func<IEnumerable<ShapeInfo>> <>9__3_0;

            internal IEnumerable<ShapeInfo> <.ctor>b__3_0() => 
                ShapeManager.GetShapeInfos();
        }
    }
}

