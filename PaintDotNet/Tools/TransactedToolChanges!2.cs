namespace PaintDotNet.Tools
{
    using PaintDotNet.Collections;
    using PaintDotNet.Rendering;
    using PaintDotNet.Settings;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;

    [Serializable]
    internal abstract class TransactedToolChanges<TDerived, TTool> : TransactedToolChanges where TDerived: TransactedToolChanges<TDerived, TTool> where TTool: TransactedTool<TTool, TDerived>
    {
        [NonSerialized]
        private RectInt32? cachedMaxRenderBounds;
        private KeyValuePair<string, object>[] drawingSettingsValues;

        protected TransactedToolChanges(IEnumerable<KeyValuePair<string, object>> drawingSettingsValues)
        {
            this.drawingSettingsValues = drawingSettingsValues.ToArrayEx<KeyValuePair<string, object>>();
        }

        public TDerived Clone() => 
            base.CloneCore<TDerived>();

        public TDerived CloneWithNewDrawingSettingsValues(IEnumerable<KeyValuePair<string, object>> newDrawingSettingsValues)
        {
            TDerived local = this.Clone();
            KeyValuePair<string, object>[] pairArray = newDrawingSettingsValues.ToArrayEx<KeyValuePair<string, object>>();
            using (local.UseChangeScope())
            {
                local.drawingSettingsValues = pairArray;
                local.OnClonedWithNewDrawingSettingsValues((TDerived) this);
            }
            return local.Clone();
        }

        public object GetDrawingSettingValue(Setting setting) => 
            this.drawingSettingsValues.First<KeyValuePair<string, object>>(s => SettingPath.PathEqualityComparer.Equals(s.Key, setting.Path)).Value;

        public T GetDrawingSettingValue<T>(Setting<T> setting) => 
            ((T) this.drawingSettingsValues.First<KeyValuePair<string, object>>(s => SettingPath.PathEqualityComparer.Equals(s.Key, this.setting.Path)).Value);

        public object GetDrawingSettingValue(string settingPath) => 
            this.drawingSettingsValues.First<KeyValuePair<string, object>>(s => SettingPath.PathEqualityComparer.Equals(s.Key, settingPath)).Value;

        public T GetDrawingSettingValue<T>(string settingPath) => 
            ((T) this.drawingSettingsValues.First<KeyValuePair<string, object>>(s => SettingPath.PathEqualityComparer.Equals(s.Key, this.settingPath)).Value);

        public RectInt32 GetMaxRenderBounds()
        {
            RectInt32? cachedMaxRenderBounds;
            lock (changes)
            {
                cachedMaxRenderBounds = this.cachedMaxRenderBounds;
            }
            RectInt32? nullable2 = cachedMaxRenderBounds;
            RectInt32 num = nullable2.HasValue ? nullable2.GetValueOrDefault() : this.OnGetMaxRenderBounds();
            lock (changes2)
            {
                this.cachedMaxRenderBounds = new RectInt32?(num);
            }
            if (num.IsEmpty)
            {
                return TransactedToolChanges.MaxMaxRenderBounds;
            }
            return RectInt32.Intersect(num, TransactedToolChanges.MaxMaxRenderBounds);
        }

        protected void InvalidateCachedMaxRenderBounds()
        {
            lock (changes)
            {
                this.cachedMaxRenderBounds = null;
            }
        }

        protected virtual void OnClonedWithNewDrawingSettingsValues(TDerived source)
        {
        }

        protected virtual RectInt32 OnGetMaxRenderBounds() => 
            TransactedToolChanges.MaxMaxRenderBounds;

        public bool TryGetDrawingSettingValue(Setting setting, out object value) => 
            this.TryGetDrawingSettingValue(setting.Path, out value);

        public bool TryGetDrawingSettingValue<T>(Setting<T> setting, out T value) => 
            this.TryGetDrawingSettingValue<T>(setting.Path, out value);

        public bool TryGetDrawingSettingValue(string settingPath, out object value)
        {
            IEqualityComparer<string> pathEqualityComparer = SettingPath.PathEqualityComparer;
            for (int i = 0; i < this.drawingSettingsValues.Length; i++)
            {
                KeyValuePair<string, object> pair = this.drawingSettingsValues[i];
                if (pathEqualityComparer.Equals(settingPath, pair.Key))
                {
                    value = pair.Value;
                    return true;
                }
            }
            value = null;
            return false;
        }

        public bool TryGetDrawingSettingValue<T>(string settingPath, out T value)
        {
            object obj2;
            bool flag = this.TryGetDrawingSettingValue(settingPath, out obj2);
            value = flag ? ((T) obj2) : default(T);
            return flag;
        }

        public IEnumerable<KeyValuePair<string, object>> DrawingSettingsValues =>
            this.drawingSettingsValues;
    }
}

