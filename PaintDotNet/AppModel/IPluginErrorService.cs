namespace PaintDotNet.AppModel
{
    using System;
    using System.Reflection;

    internal interface IPluginErrorService
    {
        string GetLocalizedEffectErrorMessage(PluginErrorInfo errorInfo);
        PluginErrorInfo[] GetPluginLoadErrors();
        void ReportEffectLoadError(Assembly assembly, Type type, Exception error);
        void ReportShapeLoadError(string filePath, Exception error);
    }
}

