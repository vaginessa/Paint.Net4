namespace PaintDotNet.AppModel
{
    using PaintDotNet;
    using PaintDotNet.Resources;
    using PaintDotNet.Settings.App;
    using System;

    internal sealed class ResourcesService : IDisposable
    {
        private static ResourcesService instance;

        private ResourcesService()
        {
            AppSettings.Instance.UI.Language.ValueChangedT += new ValueChangedEventHandler<CultureInfo>(this.OnLanguageValueChanged);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                AppSettings.Instance.UI.Language.ValueChangedT -= new ValueChangedEventHandler<CultureInfo>(this.OnLanguageValueChanged);
            }
        }

        ~ResourcesService()
        {
            this.Dispose(false);
        }

        public static void Initialize()
        {
            if (instance == null)
            {
                instance = new ResourcesService();
            }
        }

        private void OnLanguageValueChanged(object sender, ValueChangedEventArgs<CultureInfo> e)
        {
            PdnResources.Culture = e.NewValue;
        }

        public static ResourcesService Instance
        {
            get
            {
                if (instance == null)
                {
                    ExceptionUtil.ThrowInvalidOperationException("Must call Initialize() first");
                }
                return instance;
            }
        }
    }
}

