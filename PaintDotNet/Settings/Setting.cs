namespace PaintDotNet.Settings
{
    using PaintDotNet;
    using PaintDotNet.AppModel;
    using PaintDotNet.ComponentModel;
    using PaintDotNet.Diagnostics;
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal abstract class Setting : ISetting, ICloneable, INotifyPropertyChanged
    {
        private SettingsContainer container;
        private SettingConverter converter;
        private object defaultValue;
        private bool isInitialized;
        private object oldValueBeforeEventSuspension;
        private string path;
        private SettingScope scope;
        private int valueChangedEventSuspensionCount;
        private static readonly PropertyChangedEventArgs valuePropertyChangedEventArgs = new PropertyChangedEventArgs("Value");
        private Type valueType;

        [field: CompilerGenerated]
        public event PropertyChangedEventHandler PropertyChanged;

        [field: CompilerGenerated]
        public event ValueChangedEventHandler<object> ValueChanged;

        internal Setting(Setting cloneMe)
        {
            this.path = cloneMe.path;
            this.valueType = cloneMe.valueType;
            this.defaultValue = cloneMe.defaultValue;
            this.scope = cloneMe.scope;
        }

        internal Setting(string path, SettingScope scope, Type valueType, SettingConverter converter)
        {
            Validate.Begin().IsNotNullOrWhiteSpace(path, "path").IsNotNull<Type>(valueType, "valueType").IsNotNull<SettingConverter>(converter, "converter").Check();
            if (!converter.IsValidValueType(valueType))
            {
                throw new ArgumentException("valueType is not a supported Type");
            }
            this.path = path;
            this.scope = scope;
            this.valueType = valueType;
            this.converter = converter;
        }

        protected virtual bool AreNonNullValuesEqual(object value1, object value2) => 
            value1.Equals(value2);

        protected bool AreValuesEqual(object value1, object value2)
        {
            bool flag = value1 == null;
            bool flag2 = value2 == null;
            if (flag & flag2)
            {
                return true;
            }
            if (flag | flag2)
            {
                return false;
            }
            return this.AreNonNullValuesEqual(value1, value2);
        }

        public object Clone() => 
            this.OnClone();

        protected object CoerceValue(object value) => 
            this.OnCoerceValue(value);

        protected void Initialize(object defaultValue)
        {
            if (this.isInitialized)
            {
                ExceptionUtil.ThrowInvalidOperationException("Initialize() has already been called");
            }
            if (!this.converter.IsValidValue(this.valueType, defaultValue))
            {
                throw new ArgumentException("defaultValue is not suitable for this.valueType");
            }
            this.defaultValue = defaultValue;
            this.isInitialized = true;
        }

        public bool IsValidValue(object potentialValue)
        {
            if ((potentialValue == null) && this.valueType.IsValueType)
            {
                return false;
            }
            if (potentialValue != null)
            {
                Type c = potentialValue.GetType();
                if (!this.valueType.IsAssignableFrom(c))
                {
                    return false;
                }
            }
            return this.OnValidateValue(potentialValue);
        }

        protected abstract Setting OnClone();
        protected virtual object OnCoerceValue(object baseValue) => 
            baseValue;

        protected virtual object OnRepairValue(object valueFromStorage) => 
            this.defaultValue;

        protected virtual bool OnValidateValue(object potentialValue) => 
            true;

        protected virtual void OnValueChanged(object oldValue, object newValue)
        {
            this.ValueChanged.Raise<object>(this, oldValue, newValue);
            this.RaisePropertyChanged(valuePropertyChangedEventArgs);
        }

        protected void RaisePropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged.Raise(this, e);
        }

        public void RaiseValueChangedEvent()
        {
            object obj2;
            object sync = this.Sync;
            lock (sync)
            {
                obj2 = this.Value;
            }
            this.OnValueChanged(obj2, obj2);
        }

        protected object RepairValue(object valueFromStorage) => 
            this.OnRepairValue(valueFromStorage);

        public void Reset()
        {
            this.Value = this.DefaultValue;
        }

        private void ResumeValueChangedEvent()
        {
            object oldValueBeforeEventSuspension;
            object obj3;
            bool flag;
            object sync = this.Sync;
            lock (sync)
            {
                oldValueBeforeEventSuspension = this.oldValueBeforeEventSuspension;
                obj3 = this.Value;
                this.valueChangedEventSuspensionCount--;
                if ((this.valueChangedEventSuspensionCount == 0) && !this.AreValuesEqual(oldValueBeforeEventSuspension, obj3))
                {
                    flag = true;
                    this.oldValueBeforeEventSuspension = null;
                }
                else
                {
                    flag = false;
                }
            }
            if (flag)
            {
                this.OnValueChanged(oldValueBeforeEventSuspension, obj3);
            }
        }

        public IDisposable SuspendValueChangedEvent()
        {
            object sync = this.Sync;
            lock (sync)
            {
                this.valueChangedEventSuspensionCount++;
                if (this.valueChangedEventSuspensionCount == 1)
                {
                    this.oldValueBeforeEventSuspension = this.Value;
                }
                return new ValueChangedEventSpring(this);
            }
        }

        protected void VerifyIsInitialized()
        {
            if (!this.isInitialized)
            {
                ExceptionUtil.ThrowInvalidOperationException();
            }
        }

        internal SettingsContainer Container
        {
            get => 
                this.container;
            set
            {
                if (this.container != null)
                {
                    ExceptionUtil.ThrowInvalidOperationException("Can only set Container once");
                }
                this.container = value;
            }
        }

        public SettingConverter Converter =>
            this.converter;

        public object DefaultValue
        {
            get
            {
                this.VerifyIsInitialized();
                return this.defaultValue;
            }
        }

        public bool IsReadOnly =>
            false;

        protected bool IsValueChangedEventSuspended
        {
            get
            {
                object sync = this.Sync;
                lock (sync)
                {
                    return (this.valueChangedEventSuspensionCount > 0);
                }
            }
        }

        public string Path =>
            this.path;

        public SettingScope Scope =>
            this.scope;

        protected SettingsStorageHandler StorageHandler =>
            this.container.StorageHandler;

        protected object Sync =>
            this;

        public object Value
        {
            get
            {
                object sync = this.Sync;
                lock (sync)
                {
                    object potentialValue = this.StorageHandler.Get(this);
                    object obj4 = this.IsValidValue(potentialValue) ? potentialValue : this.RepairValue(potentialValue);
                    return this.CoerceValue(obj4);
                }
            }
            set
            {
                object obj2;
                object obj3;
                bool flag;
                object sync = this.Sync;
                lock (sync)
                {
                    obj2 = this.Value;
                    if (!this.IsValidValue(value))
                    {
                        throw new ArgumentException();
                    }
                    this.StorageHandler.TrySet(this, value);
                    obj3 = this.Value;
                    if (!this.IsValueChangedEventSuspended && !this.AreValuesEqual(obj3, obj2))
                    {
                        flag = true;
                    }
                    else
                    {
                        flag = false;
                    }
                }
                if (flag)
                {
                    this.OnValueChanged(obj2, obj3);
                }
            }
        }

        public Type ValueType =>
            this.valueType;

        private sealed class ValueChangedEventSpring : IDisposable
        {
            private Setting appSetting;

            public ValueChangedEventSpring(Setting appSetting)
            {
                Validate.IsNotNull<Setting>(appSetting, "appSetting");
                this.appSetting = appSetting;
            }

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (disposing && (this.appSetting != null))
                {
                    this.appSetting.ResumeValueChangedEvent();
                    this.appSetting = null;
                }
            }

            ~ValueChangedEventSpring()
            {
                ExceptionUtil.ThrowInvalidOperationException("This object must be disposed");
            }
        }
    }
}

