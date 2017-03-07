namespace PaintDotNet.Settings
{
    using PaintDotNet;
    using PaintDotNet.AppModel;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal abstract class Setting<TValue> : Setting, ISetting<TValue>, ISetting
    {
        private static IBoxPolicy<TValue> boxPolicy;
        private static IEqualityComparer<TValue> equalityComparer;

        [field: CompilerGenerated]
        public event ValueChangedEventHandler<TValue> ValueChangedT;

        public Setting(string path, SettingScope scope, TValue defaultValue, SettingConverter converter) : base(path, scope, typeof(TValue), converter)
        {
            if (Setting<TValue>.boxPolicy == null)
            {
                Setting<TValue>.boxPolicy = this.OnGetStaticBoxPolicy();
            }
            if (Setting<TValue>.equalityComparer == null)
            {
                Setting<TValue>.equalityComparer = this.OnGetStaticEqualityComparer();
            }
            base.Initialize(this.BoxValue(defaultValue));
        }

        protected sealed override bool AreNonNullValuesEqual(object value1, object value2) => 
            this.AreValuesEqualT((TValue) value1, (TValue) value2);

        protected virtual bool AreValuesEqualT(TValue value1, TValue value2) => 
            Setting<TValue>.equalityComparer.Equals(value1, value2);

        protected object BoxValue(TValue value) => 
            Setting<TValue>.boxPolicy.BoxValue(value);

        private IBoxPolicy<TValue> GetBoxPolicy()
        {
            IBoxPolicy<TValue> policy = this.OnGetStaticBoxPolicy();
            if (policy == null)
            {
                throw new InternalErrorException("OnGetBoxPolicy() returned null");
            }
            return policy;
        }

        public bool IsValidValue(TValue value)
        {
            object obj2 = this.BoxValue(value);
            return base.IsValidValue(value);
        }

        protected virtual IBoxPolicy<TValue> OnGetStaticBoxPolicy() => 
            BoxPolicy.Default<TValue>.Instance;

        protected virtual IEqualityComparer<TValue> OnGetStaticEqualityComparer() => 
            EqualityComparer<TValue>.Default;

        protected sealed override bool OnValidateValue(object potentialValue) => 
            this.OnValidateValueT((TValue) potentialValue);

        protected virtual bool OnValidateValueT(TValue potentialValue) => 
            true;

        protected sealed override void OnValueChanged(object oldValue, object newValue)
        {
            base.OnValueChanged(oldValue, newValue);
            this.OnValueChangedT((TValue) oldValue, (TValue) newValue);
        }

        protected virtual void OnValueChangedT(TValue oldValue, TValue newValue)
        {
            this.ValueChangedT.Raise<TValue>(this, oldValue, newValue);
        }

        public TValue DefaultValue =>
            ((TValue) base.DefaultValue);

        public TValue Value
        {
            get => 
                ((TValue) base.Value);
            set
            {
                base.Value = this.BoxValue(value);
            }
        }
    }
}

