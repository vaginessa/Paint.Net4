namespace PaintDotNet.Tools
{
    using System;
    using System.Runtime.CompilerServices;

    internal static class ToolInfoExtensions
    {
        public static Type FindByName(this ToolInfo[] toolInfos, string toolName)
        {
            foreach (ToolInfo info in toolInfos)
            {
                if (string.Equals(info.ToolType.Name, toolName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return info.ToolType;
                }
            }
            return null;
        }
    }
}

