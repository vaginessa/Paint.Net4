namespace PaintDotNet.Settings
{
    using PaintDotNet.Diagnostics;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class SettingPath
    {
        public const char PathSeparatorChar = '/';
        private const string pathSeparatorString = "/";

        public static string Combine(string root, string path) => 
            Normalize(root + "/" + path);

        public static string CombinePathComponents(IEnumerable<string> pathComponents) => 
            string.Join("/", pathComponents);

        public static string GetLeafName(string settingPath)
        {
            string[] pathComponents = GetPathComponents(settingPath);
            return pathComponents[pathComponents.Length - 1];
        }

        public static string[] GetPathComponents(string settingPath)
        {
            Validate.IsNotNullOrWhiteSpace(settingPath, "settingPath");
            char[] separator = new char[] { '/' };
            return settingPath.Trim().Split(separator);
        }

        public static string GetSectionPath(string settingPath)
        {
            string[] pathComponents = GetPathComponents(settingPath);
            return CombinePathComponents(pathComponents.Take<string>(pathComponents.Length - 1));
        }

        public static bool IsInSection(string sectionPath, string path)
        {
            string[] pathComponents = GetPathComponents(sectionPath);
            string[] strArray2 = GetPathComponents(path);
            if (strArray2.Length == (pathComponents.Length + 1))
            {
                for (int i = 0; i < pathComponents.Length; i++)
                {
                    if (!string.Equals(pathComponents[i], strArray2[i], StringComparison.InvariantCultureIgnoreCase))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static bool IsInSectionTree(string sectionPath, string path)
        {
            string[] pathComponents = GetPathComponents(sectionPath);
            string[] strArray2 = GetPathComponents(path);
            if (strArray2.Length > pathComponents.Length)
            {
                for (int i = 0; i < pathComponents.Length; i++)
                {
                    if (!string.Equals(pathComponents[i], strArray2[i], StringComparison.InvariantCultureIgnoreCase))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static string Normalize(string path)
        {
            Validate.IsNotNull<string>(path, "path");
            if (string.IsNullOrWhiteSpace(path))
            {
                return path;
            }
            string str = path.Trim();
            string str2 = (path[0] == '/') ? str.Substring(1) : str;
            return path.Replace("//", "/");
        }

        public static StringComparer PathEqualityComparer =>
            StringComparer.InvariantCultureIgnoreCase;
    }
}

