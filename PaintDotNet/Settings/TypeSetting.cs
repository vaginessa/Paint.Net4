namespace PaintDotNet.Settings
{
    using PaintDotNet.Diagnostics;
    using System;

    internal sealed class TypeSetting : Setting<Type>
    {
        private Type baseType;
        private bool isBaseTypeAllowed;

        public TypeSetting(string path, SettingScope scope, Type defaultValue, Type baseType, bool isBaseTypeAllowed) : base(path, scope, defaultValue, TypeSettingConverter.Instance)
        {
            Validate.IsNotNull<Type>(baseType, "baseType");
            this.baseType = baseType;
            this.isBaseTypeAllowed = isBaseTypeAllowed;
        }

        protected override Setting OnClone() => 
            new TypeSetting(base.Path, base.Scope, base.DefaultValue, this.baseType, this.isBaseTypeAllowed);

        public Type BaseType =>
            this.baseType;

        public bool IsBaseTypeAllowed =>
            this.isBaseTypeAllowed;
    }
}

