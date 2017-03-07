namespace PaintDotNet.AppModel
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Resources;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;

    internal sealed class PluginErrorService : IPluginErrorService
    {
        private static readonly PluginErrorService instance = new PluginErrorService();
        private HashSet<PluginErrorInfo> pluginLoadErrors = new HashSet<PluginErrorInfo>();

        private PluginErrorService()
        {
        }

        private void AddPluginLoadError(PluginErrorInfo errorInfo)
        {
            HashSet<PluginErrorInfo> pluginLoadErrors = this.pluginLoadErrors;
            lock (pluginLoadErrors)
            {
                if (!this.pluginLoadErrors.Contains(errorInfo))
                {
                    this.pluginLoadErrors.Add(errorInfo);
                }
            }
        }

        public string GetLocalizedEffectErrorMessage(PluginErrorInfo errorInfo) => 
            GetLocalizedEffectErrorMessage(errorInfo.Assembly, errorInfo.Type, errorInfo.Error);

        private static string GetLocalizedEffectErrorMessage(Assembly assembly, string typeName, Exception exception)
        {
            IPluginSupportInfo pluginSupportInfo = PluginSupportInfo.GetPluginSupportInfo(assembly);
            return GetLocalizedEffectErrorMessage(assembly, typeName, pluginSupportInfo, exception);
        }

        private static string GetLocalizedEffectErrorMessage(Assembly assembly, Type type, Exception exception)
        {
            IPluginSupportInfo pluginSupportInfo;
            string fullName;
            if (type != null)
            {
                fullName = type.FullName;
                pluginSupportInfo = PluginSupportInfo.GetPluginSupportInfo(type);
            }
            else if (exception is TypeLoadException)
            {
                TypeLoadException exception2 = exception as TypeLoadException;
                fullName = exception2.TypeName;
                pluginSupportInfo = PluginSupportInfo.GetPluginSupportInfo(assembly);
            }
            else
            {
                pluginSupportInfo = PluginSupportInfo.GetPluginSupportInfo(assembly);
                fullName = null;
            }
            return GetLocalizedEffectErrorMessage(assembly, fullName, pluginSupportInfo, exception);
        }

        private static string GetLocalizedEffectErrorMessage(Assembly assembly, string typeName, IPluginSupportInfo supportInfo, Exception exception)
        {
            string pluginBlockReasonString;
            string location = assembly.Location;
            string format = PdnResources.GetString("EffectErrorMessage.ShortFormat");
            string str3 = PdnResources.GetString("EffectErrorMessage.FullFormat");
            string str4 = PdnResources.GetString("EffectErrorMessage.InfoNotSupplied");
            if (exception is BlockedPluginException)
            {
                pluginBlockReasonString = GetPluginBlockReasonString(((BlockedPluginException) exception).Reason);
            }
            else
            {
                pluginBlockReasonString = exception.ToString();
            }
            if (supportInfo == null)
            {
                return string.Format(format, location ?? str4, typeName ?? str4, pluginBlockReasonString);
            }
            return string.Format(str3, new object[] { location ?? str4, typeName ?? (supportInfo.DisplayName ?? str4), (supportInfo.Version ?? new Version()).ToString(), supportInfo.Author ?? str4, supportInfo.Copyright ?? str4, (supportInfo.WebsiteUri == null) ? str4 : supportInfo.WebsiteUri.ToString(), pluginBlockReasonString });
        }

        private static string GetPluginBlockReasonString(PluginBlockReason reason)
        {
            StringBuilder builder = new StringBuilder();
            IEnumerable<PluginBlockReason> enumerable2 = from v in Enum.GetValues(typeof(PluginBlockReason)).Cast<PluginBlockReason>()
                where (reason & v) > PluginBlockReason.NotBlocked
                select v;
            EnumLocalizer localizer = EnumLocalizer.Create(typeof(PluginBlockReason));
            return (from r in enumerable2 select localizer.GetLocalizedEnumValue(r).LocalizedName).Aggregate<string, string>(string.Empty, (sa, s) => (sa + ((sa.Length > 0) ? Environment.NewLine : string.Empty) + s));
        }

        public PluginErrorInfo[] GetPluginLoadErrors()
        {
            HashSet<PluginErrorInfo> pluginLoadErrors = this.pluginLoadErrors;
            lock (pluginLoadErrors)
            {
                return this.pluginLoadErrors.ToArrayEx<PluginErrorInfo>();
            }
        }

        public void ReportEffectLoadError(Assembly assembly, Type type, Exception error)
        {
            PluginErrorInfo errorInfo = new PluginErrorInfo(assembly, type, error);
            this.AddPluginLoadError(errorInfo);
        }

        public void ReportShapeLoadError(string filePath, Exception error)
        {
            PluginErrorInfo errorInfo = new PluginErrorInfo(filePath, null, error);
            this.AddPluginLoadError(errorInfo);
        }

        public static IPluginErrorService Instance =>
            instance;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly PluginErrorService.<>c <>9 = new PluginErrorService.<>c();
            public static Func<string, string, string> <>9__12_2;

            internal string <GetPluginBlockReasonString>b__12_2(string sa, string s) => 
                (sa + ((sa.Length > 0) ? Environment.NewLine : string.Empty) + s);
        }
    }
}

