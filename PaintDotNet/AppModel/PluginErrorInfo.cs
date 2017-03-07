namespace PaintDotNet.AppModel
{
    using PaintDotNet;
    using PaintDotNet.Diagnostics;
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal sealed class PluginErrorInfo : IEquatable<PluginErrorInfo>
    {
        private System.Reflection.Assembly assembly;
        private string filePath;

        internal PluginErrorInfo(System.Reflection.Assembly assembly, System.Type type, Exception error) : this(assembly, null, type, error)
        {
        }

        internal PluginErrorInfo(string filePath, System.Type type, Exception error) : this(null, filePath, type, error)
        {
        }

        private PluginErrorInfo(System.Reflection.Assembly assembly, string filePath, System.Type type, Exception error)
        {
            Validate.IsNotNull<Exception>(error, "error");
            if ((assembly == null) && (filePath == null))
            {
                throw new ArgumentNullException((assembly == null) ? "assembly" : "filePath");
            }
            if ((assembly != null) && (filePath != null))
            {
                throw new ArgumentException("only assembly or filePath may be specified, not both");
            }
            if ((!(error is TypeLoadException) && !(error is FileNotFoundException)) && (assembly != null))
            {
                Validate.IsNotNull<System.Type>(type, "type");
            }
            this.assembly = assembly;
            this.filePath = filePath;
            this.Type = type;
            this.Error = error;
            this.ErrorString = error.ToString();
        }

        public bool Equals(PluginErrorInfo other)
        {
            if (other == null)
            {
                return false;
            }
            return ((((this.assembly == other.assembly) && (this.filePath == other.filePath)) && (this.Type == other.Type)) && (this.ErrorString == other.ErrorString));
        }

        public override bool Equals(object obj) => 
            EquatableUtil.Equals<PluginErrorInfo, object>(this, obj);

        public override int GetHashCode() => 
            HashCodeUtil.CombineHashCodes((this.assembly == null) ? 0 : this.assembly.GetHashCode(), (this.filePath == null) ? 0 : this.filePath.GetHashCode(), (this.Type == null) ? 0 : this.Type.GetHashCode(), this.ErrorString.GetHashCode());

        public System.Reflection.Assembly Assembly =>
            this.assembly;

        public Exception Error { get; private set; }

        public string ErrorString { get; private set; }

        public string FilePath
        {
            get
            {
                if (this.filePath != null)
                {
                    return this.filePath;
                }
                if (this.assembly != null)
                {
                    return this.assembly.Location;
                }
                return null;
            }
        }

        public bool HasAssembly =>
            (this.assembly != null);

        public bool HasType =>
            (this.Type != null);

        public bool HasTypeName =>
            (this.TypeName > null);

        public System.Type Type { get; private set; }

        public string TypeName
        {
            get
            {
                if (this.Type != null)
                {
                    return this.Type.FullName;
                }
                if (this.Error is TypeLoadException)
                {
                    return ((TypeLoadException) this.Error).TypeName;
                }
                return null;
            }
        }
    }
}

