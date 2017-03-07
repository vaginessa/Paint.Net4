namespace PaintDotNet.AppModel
{
    using PaintDotNet;
    using PaintDotNet.Effects;
    using PaintDotNet.Settings.App;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal sealed class ServiceProviderForEffects : MarshalByRefObject, IServiceProvider, IDisposable, IIsDisposed
    {
        private bool disposed;
        private Dictionary<Type, object> serviceMap = new Dictionary<Type, object>();
        private object sync = new object();

        private object CreateService(Type serviceType)
        {
            if (serviceType == typeof(ISettingsService))
            {
                return new SettingsService(AppSettings.Instance, new string[] { AppSettings.Instance.Effects.DefaultQualityLevel.Path });
            }
            if (serviceType == typeof(IAppInfoService))
            {
                return new AppInfoService();
            }
            if (serviceType == typeof(IUserFilesService))
            {
                return UserFilesService.Instance;
            }
            if (serviceType == typeof(IShellService))
            {
                return new ShellService();
            }
            if (serviceType == typeof(IEnumLocalizerFactory))
            {
                return new EnumLocalizerFactory(t => t == typeof(LayerBlendMode));
            }
            if (serviceType == typeof(IExceptionDialogService))
            {
                return new ExceptionDialogService();
            }
            if (serviceType != typeof(IPalettesService))
            {
                throw new KeyNotFoundException();
            }
            return new PalettesServiceForEffects();
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            object sync = this.sync;
            lock (sync)
            {
                if (this.serviceMap != null)
                {
                    foreach (object obj3 in this.serviceMap.Values)
                    {
                        IDisposable disposable = obj3 as IDisposable;
                        if (disposable != null)
                        {
                            disposable.Dispose();
                        }
                    }
                    this.serviceMap.Clear();
                }
            }
            this.disposed = true;
        }

        public object GetService(Type serviceType)
        {
            this.VerifyNotDisposed();
            object obj2 = null;
            object sync = this.sync;
            lock (sync)
            {
                if (this.serviceMap.TryGetValue(serviceType, out obj2))
                {
                    return obj2;
                }
                try
                {
                    obj2 = this.CreateService(serviceType);
                }
                catch (KeyNotFoundException)
                {
                    return null;
                }
                this.serviceMap.Add(serviceType, obj2);
            }
            return obj2;
        }

        private void VerifyNotDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("ServiceProviderForEffects");
            }
        }

        public bool IsDisposed =>
            this.disposed;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly ServiceProviderForEffects.<>c <>9 = new ServiceProviderForEffects.<>c();
            public static Func<Type, bool> <>9__6_0;

            internal bool <CreateService>b__6_0(Type t) => 
                (t == typeof(LayerBlendMode));
        }
    }
}

