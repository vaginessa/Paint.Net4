namespace PaintDotNet.Data
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Resources;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;

    [Serializable]
    internal sealed class FileTypeCollection
    {
        private FileType[] fileTypes;

        public FileTypeCollection(IEnumerable<FileType> fileTypes)
        {
            this.fileTypes = fileTypes.ToArrayEx<FileType>();
        }

        public static FileType[] FilterFileTypeList(FileType[] input, bool excludeCantSave, bool excludeCantLoad)
        {
            List<FileType> items = new List<FileType>();
            foreach (FileType type in input)
            {
                if ((!excludeCantSave || type.SupportsSaving) && (!excludeCantLoad || type.SupportsLoading))
                {
                    items.Add(type);
                }
            }
            return items.ToArrayEx<FileType>();
        }

        public int IndexOfExtension(string findMeExt)
        {
            if (findMeExt != null)
            {
                for (int i = 0; i < this.fileTypes.Length; i++)
                {
                    foreach (string str in this.fileTypes[i].Extensions)
                    {
                        if (string.Equals(str, findMeExt, StringComparison.InvariantCultureIgnoreCase))
                        {
                            return i;
                        }
                    }
                }
            }
            return -1;
        }

        public int IndexOfFileType(FileType fileType)
        {
            if (fileType != null)
            {
                for (int i = 0; i < this.fileTypes.Length; i++)
                {
                    if (this.fileTypes[i].Name == fileType.Name)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public int IndexOfName(string name)
        {
            for (int i = 0; i < this.fileTypes.Length; i++)
            {
                if (this.fileTypes[i].Name == name)
                {
                    return i;
                }
            }
            return -1;
        }

        public string ToString(bool excludeCantSave, bool excludeCantLoad)
        {
            FileType[] typeArray = FilterFileTypeList(this.fileTypes, excludeCantSave, excludeCantLoad);
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < typeArray.Length; i++)
            {
                builder.Append(typeArray[i].ToString());
                if (i != (typeArray.Length - 1))
                {
                    builder.Append("|");
                }
            }
            return builder.ToString();
        }

        public string ToString(bool includeAll, string allName, bool excludeCantSave, bool excludeCantLoad)
        {
            if (allName == null)
            {
                allName = PdnResources.GetString("FileTypeCollection.AllImageTypes");
            }
            if (!includeAll)
            {
                return this.ToString(excludeCantSave, excludeCantLoad);
            }
            StringBuilder builder = new StringBuilder(allName);
            StringBuilder builder2 = new StringBuilder();
            bool flag = false;
            FileType[] typeArray = FilterFileTypeList(this.fileTypes, excludeCantSave, excludeCantLoad);
            for (int i = 0; i < typeArray.Length; i++)
            {
                if (!flag)
                {
                    flag = true;
                    builder.Append(" (");
                }
                string[] extensions = typeArray[i].Extensions;
                for (int j = 0; j < extensions.Length; j++)
                {
                    builder.Append("*");
                    builder.Append(extensions[j]);
                    builder2.Append("*");
                    builder2.Append(extensions[j]);
                    if ((j != (extensions.Length - 1)) || (i != (typeArray.Length - 1)))
                    {
                        builder.Append(", ");
                        builder2.Append(";");
                    }
                }
            }
            if (flag)
            {
                builder.Append(")");
            }
            string str = builder.ToString() + "|" + builder2.ToString();
            if (typeArray.Length != 0)
            {
                str = str + "|" + this.ToString(excludeCantSave, excludeCantLoad);
            }
            return str;
        }

        public string[] AllExtensions
        {
            get
            {
                List<string> items = new List<string>();
                foreach (FileType type in this.fileTypes)
                {
                    foreach (string str in type.Extensions)
                    {
                        items.Add(str);
                    }
                }
                return items.ToArrayEx<string>();
            }
        }

        public FileType[] FileTypes =>
            ((FileType[]) this.fileTypes.Clone());

        public FileType this[int index] =>
            this.fileTypes[index];

        public int Length =>
            this.fileTypes.Length;
    }
}

