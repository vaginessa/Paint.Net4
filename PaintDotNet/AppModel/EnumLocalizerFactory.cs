namespace PaintDotNet.AppModel
{
    using PaintDotNet.Collections;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Resources;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal sealed class EnumLocalizerFactory : IEnumLocalizerFactory
    {
        private readonly Func<Type, bool> allowedEnumTypePredicate;

        public EnumLocalizerFactory(Func<Type, bool> allowedEnumTypePredicate)
        {
            Validate.IsNotNull<Func<Type, bool>>(allowedEnumTypePredicate, "allowedEnumTypePredicate");
            this.allowedEnumTypePredicate = allowedEnumTypePredicate;
        }

        public IEnumLocalizer Create(Type enumType)
        {
            Validate.IsNotNull<Type>(enumType, "enumType");
            if (!this.allowedEnumTypePredicate(enumType))
            {
                throw new ArgumentException();
            }
            return new EnumLocalizerWrapper(EnumLocalizer.Create(enumType));
        }

        private sealed class EnumLocalizerWrapper : IEnumLocalizer
        {
            private readonly EnumLocalizer enumLocalizer;

            public EnumLocalizerWrapper(EnumLocalizer enumLocalizer)
            {
                Validate.IsNotNull<EnumLocalizer>(enumLocalizer, "enumLocalizer");
                this.enumLocalizer = enumLocalizer;
            }

            public ILocalizedEnumValue GetLocalizedEnumValue(object enumValue)
            {
                Validate.IsNotNull<object>(enumValue, "enumValue");
                return new EnumLocalizerFactory.LocalizedEnumValueWrapper(this.enumLocalizer.GetLocalizedEnumValue((Enum) enumValue));
            }

            public IList<ILocalizedEnumValue> GetLocalizedEnumValues() => 
                this.enumLocalizer.GetLocalizedEnumValues().Select<LocalizedEnumValue, EnumLocalizerFactory.LocalizedEnumValueWrapper>(lev => new EnumLocalizerFactory.LocalizedEnumValueWrapper(lev)).ToArrayEx<EnumLocalizerFactory.LocalizedEnumValueWrapper>();

            public Type EnumType =>
                this.enumLocalizer.EnumType;

            [Serializable, CompilerGenerated]
            private sealed class <>c
            {
                public static readonly EnumLocalizerFactory.EnumLocalizerWrapper.<>c <>9 = new EnumLocalizerFactory.EnumLocalizerWrapper.<>c();
                public static Func<LocalizedEnumValue, EnumLocalizerFactory.LocalizedEnumValueWrapper> <>9__4_0;

                internal EnumLocalizerFactory.LocalizedEnumValueWrapper <GetLocalizedEnumValues>b__4_0(LocalizedEnumValue lev) => 
                    new EnumLocalizerFactory.LocalizedEnumValueWrapper(lev);
            }
        }

        private sealed class LocalizedEnumValueWrapper : ILocalizedEnumValue
        {
            private readonly LocalizedEnumValue localizedEnumValue;

            public LocalizedEnumValueWrapper(LocalizedEnumValue localizedEnumValue)
            {
                Validate.IsNotNull<LocalizedEnumValue>(localizedEnumValue, "localizedEnumValue");
                this.localizedEnumValue = localizedEnumValue;
            }

            public Type EnumType =>
                this.localizedEnumValue.EnumType;

            public object EnumValue =>
                this.localizedEnumValue.EnumValue;

            public string LocalizedName =>
                this.localizedEnumValue.LocalizedName;
        }
    }
}

