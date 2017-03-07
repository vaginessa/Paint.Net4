namespace PaintDotNet.Settings
{
    using PaintDotNet.Collections;
    using PaintDotNet.Diagnostics;
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal abstract class StaticListChoiceSetting<T> : Setting<T>
    {
        private readonly IEqualityComparer<T> equalityComparer;
        private Lazy<ReadOnlyIndexedList<T>> lazyValueChoicesIndex;

        protected StaticListChoiceSetting(string path, SettingScope scope, T defaultValue, SettingConverter converter) : this(path, scope, defaultValue, EqualityComparer<T>.Default, converter)
        {
        }

        protected StaticListChoiceSetting(string path, SettingScope scope, T defaultValue, IEqualityComparer<T> equalityComparer, SettingConverter converter) : base(path, scope, defaultValue, converter)
        {
            Validate.IsNotNull<IEqualityComparer<T>>(equalityComparer, "equalityComparer");
            this.equalityComparer = equalityComparer;
            this.InvalidateValueChoices();
        }

        protected void InvalidateValueChoices()
        {
            this.lazyValueChoicesIndex = new Lazy<ReadOnlyIndexedList<T>>(() => new ReadOnlyIndexedList<T>(this.OnGetValueChoices().ToArrayEx<T>(), base.equalityComparer), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        protected abstract IEnumerable<T> OnGetValueChoices();
        protected sealed override bool OnValidateValueT(T potentialValue) => 
            this.lazyValueChoicesIndex.Value.Contains(potentialValue);

        public IList<T> ValueChoices =>
            this.lazyValueChoicesIndex.Value;

        public IEqualityComparer<T> ValueComparer =>
            this.equalityComparer;
    }
}

