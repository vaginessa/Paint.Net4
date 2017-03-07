namespace PaintDotNet.Data
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Windows.Forms;

    internal sealed class FileTypes
    {
        private static FileTypeCollection collection;

        private FileTypes()
        {
        }

        private static System.Type[] GetFileTypeFactoriesFromAssemblies(ICollection assemblies)
        {
            List<System.Type> items = new List<System.Type>();
            foreach (Assembly assembly in assemblies)
            {
                System.Type[] fileTypeFactoriesFromAssembly;
                try
                {
                    fileTypeFactoriesFromAssembly = GetFileTypeFactoriesFromAssembly(assembly);
                }
                catch (Exception)
                {
                    continue;
                }
                foreach (System.Type type in fileTypeFactoriesFromAssembly)
                {
                    items.Add(type);
                }
            }
            return items.ToArrayEx<System.Type>();
        }

        private static System.Type[] GetFileTypeFactoriesFromAssembly(Assembly assembly)
        {
            List<System.Type> items = new List<System.Type>();
            foreach (System.Type type in assembly.GetTypes())
            {
                if (IsInterfaceImplemented(type, typeof(IFileTypeFactory)) && !type.IsAbstract)
                {
                    items.Add(type);
                }
            }
            return items.ToArrayEx<System.Type>();
        }

        public static FileTypeCollection GetFileTypes()
        {
            if (collection == null)
            {
                collection = LoadFileTypes();
            }
            return collection;
        }

        private static bool IsInterfaceImplemented(System.Type derivedType, System.Type interfaceType) => 
            (-1 != Array.IndexOf<System.Type>(derivedType.GetInterfaces(), interfaceType));

        private static FileTypeCollection LoadFileTypes()
        {
            bool exists;
            List<Assembly> assemblies = new List<Assembly> {
                typeof(FileTypes).Assembly
            };
            string path = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "FileTypes");
            try
            {
                DirectoryInfo info = new DirectoryInfo(path);
                exists = info.Exists;
            }
            catch (Exception)
            {
                exists = false;
            }
            if (exists)
            {
                foreach (string str3 in Directory.GetFiles(path, "*.dll"))
                {
                    if (!Path.GetFileName(str3).Equals("DdsFileType.dll", StringComparison.InvariantCultureIgnoreCase))
                    {
                        bool flag2;
                        Assembly item = null;
                        try
                        {
                            item = Assembly.LoadFrom(str3);
                            flag2 = true;
                        }
                        catch (Exception)
                        {
                            flag2 = false;
                        }
                        if (flag2)
                        {
                            assemblies.Add(item);
                        }
                    }
                }
            }
            System.Type[] fileTypeFactoriesFromAssemblies = GetFileTypeFactoriesFromAssemblies(assemblies);
            List<FileType> fileTypes = new List<FileType>(10);
            foreach (System.Type type in fileTypeFactoriesFromAssemblies)
            {
                IFileTypeFactory factory;
                FileType[] fileTypeInstances;
                ConstructorInfo constructor = type.GetConstructor(System.Type.EmptyTypes);
                try
                {
                    factory = (IFileTypeFactory) constructor.Invoke(null);
                }
                catch (Exception)
                {
                    continue;
                }
                try
                {
                    fileTypeInstances = factory.GetFileTypeInstances();
                }
                catch (Exception)
                {
                    continue;
                }
                if (fileTypeInstances != null)
                {
                    foreach (FileType type2 in fileTypeInstances)
                    {
                        fileTypes.Add(type2);
                    }
                }
            }
            return new FileTypeCollection(fileTypes);
        }
    }
}

