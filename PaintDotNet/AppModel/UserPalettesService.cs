namespace PaintDotNet.AppModel
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Resources;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    internal sealed class UserPalettesService
    {
        private static readonly ColorBgra[] defaultPalette = new ColorBgra[] { 
            ColorBgra.FromUInt32(0xff000000), ColorBgra.FromUInt32(0xff404040), ColorBgra.FromUInt32(0xffff0000), ColorBgra.FromUInt32(0xffff6a00), ColorBgra.FromUInt32(0xffffd800), ColorBgra.FromUInt32(0xffb6ff00), ColorBgra.FromUInt32(0xff4cff00), ColorBgra.FromUInt32(0xff00ff21), ColorBgra.FromUInt32(0xff00ff90), ColorBgra.FromUInt32(0xff00ffff), ColorBgra.FromUInt32(0xff0094ff), ColorBgra.FromUInt32(0xff0026ff), ColorBgra.FromUInt32(0xff4800ff), ColorBgra.FromUInt32(0xffb200ff), ColorBgra.FromUInt32(0xffff00dc), ColorBgra.FromUInt32(0xffff006e),
            ColorBgra.FromUInt32(uint.MaxValue), ColorBgra.FromUInt32(0xff808080), ColorBgra.FromUInt32(0xff7f0000), ColorBgra.FromUInt32(0xff7f3300), ColorBgra.FromUInt32(0xff7f6a00), ColorBgra.FromUInt32(0xff5b7f00), ColorBgra.FromUInt32(0xff267f00), ColorBgra.FromUInt32(0xff007f0e), ColorBgra.FromUInt32(0xff007f46), ColorBgra.FromUInt32(0xff007f7f), ColorBgra.FromUInt32(0xff004a7f), ColorBgra.FromUInt32(0xff00137f), ColorBgra.FromUInt32(0xff21007f), ColorBgra.FromUInt32(0xff57007f), ColorBgra.FromUInt32(0xff7f006e), ColorBgra.FromUInt32(0xff7f0037),
            ColorBgra.FromUInt32(0xffa0a0a0), ColorBgra.FromUInt32(0xff303030), ColorBgra.FromUInt32(0xffff7f7f), ColorBgra.FromUInt32(0xffffb27f), ColorBgra.FromUInt32(0xffffe97f), ColorBgra.FromUInt32(0xffdaff7f), ColorBgra.FromUInt32(0xffa5ff7f), ColorBgra.FromUInt32(0xff7fff8e), ColorBgra.FromUInt32(0xff7fffc5), ColorBgra.FromUInt32(0xff7fffff), ColorBgra.FromUInt32(0xff7fc9ff), ColorBgra.FromUInt32(0xff7f92ff), ColorBgra.FromUInt32(0xffa17fff), ColorBgra.FromUInt32(0xffd67fff), ColorBgra.FromUInt32(0xffff7fed), ColorBgra.FromUInt32(0xffff7fb6),
            ColorBgra.FromUInt32(0xffc0c0c0), ColorBgra.FromUInt32(0xff606060), ColorBgra.FromUInt32(0xff7f3f3f), ColorBgra.FromUInt32(0xff7f593f), ColorBgra.FromUInt32(0xff7f743f), ColorBgra.FromUInt32(0xff6d7f3f), ColorBgra.FromUInt32(0xff527f3f), ColorBgra.FromUInt32(0xff3f7f47), ColorBgra.FromUInt32(0xff3f7f62), ColorBgra.FromUInt32(0xff3f7f7f), ColorBgra.FromUInt32(0xff3f647f), ColorBgra.FromUInt32(0xff3f497f), ColorBgra.FromUInt32(0xff503f7f), ColorBgra.FromUInt32(0xff6b3f7f), ColorBgra.FromUInt32(0xff7f3f76), ColorBgra.FromUInt32(0xff7f3f5b),
            ColorBgra.FromUInt32(0x80000000), ColorBgra.FromUInt32(0x80404040), ColorBgra.FromUInt32(0x80ff0000), ColorBgra.FromUInt32(0x80ff6a00), ColorBgra.FromUInt32(0x80ffd800), ColorBgra.FromUInt32(0x80b6ff00), ColorBgra.FromUInt32(0x804cff00), ColorBgra.FromUInt32(0x8000ff21), ColorBgra.FromUInt32(0x8000ff90), ColorBgra.FromUInt32(0x8000ffff), ColorBgra.FromUInt32(0x800094ff), ColorBgra.FromUInt32(0x800026ff), ColorBgra.FromUInt32(0x804800ff), ColorBgra.FromUInt32(0x80b200ff), ColorBgra.FromUInt32(0x80ff00dc), ColorBgra.FromUInt32(0x80ff006e),
            ColorBgra.FromUInt32(0x80ffffff), ColorBgra.FromUInt32(0x80808080), ColorBgra.FromUInt32(0x807f0000), ColorBgra.FromUInt32(0x807f3300), ColorBgra.FromUInt32(0x807f6a00), ColorBgra.FromUInt32(0x805b7f00), ColorBgra.FromUInt32(0x80267f00), ColorBgra.FromUInt32(0x80007f0e), ColorBgra.FromUInt32(0x80007f46), ColorBgra.FromUInt32(0x80007f7f), ColorBgra.FromUInt32(0x80004a7f), ColorBgra.FromUInt32(0x8000137f), ColorBgra.FromUInt32(0x8021007f), ColorBgra.FromUInt32(0x8057007f), ColorBgra.FromUInt32(0x807f006e), ColorBgra.FromUInt32(0x807f0037)
        };
        private static readonly IReadOnlyList<ColorBgra> defaultPaletteRO = new ReadOnlyCollection<ColorBgra>(defaultPalette);
        private static UserPalettesService instance;
        private const char lineCommentChar = ';';
        private const int paletteColorCount = 0x60;
        private static readonly Encoding paletteFileEncoding = Encoding.UTF8;
        private Dictionary<string, ColorBgra[]> palettes;
        private const string palettesSubDirResName = "ColorPalettes.UserDataSubDirName";
        private object sync = new object();

        private UserPalettesService()
        {
            UserFilesService.Instance.RegisterLocalizedDirectory("ColorPalettes.UserDataSubDirName");
            this.palettes = new Dictionary<string, ColorBgra[]>(StringComparer.InvariantCultureIgnoreCase);
        }

        public void AddOrUpdate(string name, ICollection<ColorBgra> colors)
        {
            this.AddOrUpdate(name, colors.ToArrayEx<ColorBgra>());
        }

        public void AddOrUpdate(string name, ColorBgra[] colors)
        {
            if (colors.Length != this.PaletteColorCount)
            {
                string[] textArray1 = new string[] { "palette must have exactly ", this.PaletteColorCount.ToString(), " colors (actual: ", colors.Length.ToString(), ")" };
                throw new ArgumentException(string.Concat(textArray1));
            }
            this.Delete(name);
            this.palettes.Add(name, colors);
        }

        public bool Contains(string name, out string existingKeyName)
        {
            foreach (string str in this.palettes.Keys)
            {
                if (string.Compare(str, name, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    existingKeyName = str;
                    return true;
                }
            }
            existingKeyName = null;
            return false;
        }

        public bool Delete(string name)
        {
            string str;
            if (this.Contains(name, out str))
            {
                this.palettes.Remove(str);
                return true;
            }
            return false;
        }

        public void EnsurePalettesPathExists()
        {
            string palettesPath = this.PalettesPath;
            try
            {
                if (!Directory.Exists(palettesPath))
                {
                    Directory.CreateDirectory(palettesPath);
                }
            }
            catch (Exception)
            {
            }
        }

        public ColorBgra[] EnsureValidPaletteSize(ColorBgra[] colors)
        {
            ColorBgra[] bgraArray = new ColorBgra[this.PaletteColorCount];
            for (int i = 0; i < this.PaletteColorCount; i++)
            {
                if (i >= colors.Length)
                {
                    bgraArray[i] = this.DefaultColor;
                }
                else
                {
                    bgraArray[i] = colors[i];
                }
            }
            return bgraArray;
        }

        private string FormatColor(ColorBgra color) => 
            color.ToHexString();

        public ColorBgra[] Get(string name)
        {
            string str;
            if (this.Contains(name, out str))
            {
                ColorBgra[] bgraArray = this.palettes[str];
                return (ColorBgra[]) bgraArray.Clone();
            }
            return null;
        }

        public string GetPaletteSaveString(IEnumerable<ColorBgra> palette)
        {
            StringWriter writer = new StringWriter();
            string str = PdnResources.GetString("ColorPalette.SaveHeader");
            writer.WriteLine(str);
            foreach (ColorBgra bgra in palette)
            {
                string str2 = this.FormatColor(bgra);
                writer.WriteLine(str2);
            }
            return writer.ToString();
        }

        public static void Initialize()
        {
            Type type = typeof(UserPalettesService);
            lock (type)
            {
                if (instance != null)
                {
                    ExceptionUtil.ThrowInvalidOperationException("already initialized");
                }
                instance = new UserPalettesService();
            }
        }

        public void Load()
        {
            object sync = this.Sync;
            lock (sync)
            {
                if (!this.DoesPalettesPathExist)
                {
                    this.palettes = new Dictionary<string, ColorBgra[]>();
                    return;
                }
            }
            string[] files = Array.Empty<string>();
            try
            {
                files = Directory.GetFiles(this.PalettesPath, "*" + this.PalettesFileExtension);
            }
            catch (Exception)
            {
            }
            Dictionary<string, ColorBgra[]> dictionary = new Dictionary<string, ColorBgra[]>();
            foreach (string str in files)
            {
                ColorBgra[] colors = this.LoadPalette(str);
                ColorBgra[] bgraArray2 = this.EnsureValidPaletteSize(colors);
                string key = Path.ChangeExtension(Path.GetFileName(str), null);
                dictionary.Add(key, bgraArray2);
            }
            object obj3 = this.Sync;
            lock (obj3)
            {
                this.palettes = dictionary;
            }
        }

        public ColorBgra[] LoadPalette(string palettePath)
        {
            ColorBgra[] bgraArray = null;
            FileStream stream = new FileStream(palettePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            try
            {
                StreamReader reader = new StreamReader(stream, paletteFileEncoding);
                try
                {
                    string paletteString = reader.ReadToEnd();
                    bgraArray = this.ParsePaletteString(paletteString);
                }
                finally
                {
                    reader.Close();
                    reader = null;
                    stream = null;
                }
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream = null;
                }
            }
            if (bgraArray == null)
            {
                return Array.Empty<ColorBgra>();
            }
            return bgraArray;
        }

        private bool ParseColor(string colorString, out ColorBgra color)
        {
            try
            {
                color = ColorBgra.ParseHexString(colorString);
                return true;
            }
            catch (Exception)
            {
                color = this.DefaultColor;
                return false;
            }
        }

        public bool ParsePaletteLine(string line, out ColorBgra color)
        {
            color = this.DefaultColor;
            if (line == null)
            {
                return false;
            }
            string colorString = this.RemoveComments(line).Trim();
            if (colorString.Length == 0)
            {
                return false;
            }
            return this.ParseColor(colorString, out color);
        }

        public ColorBgra[] ParsePaletteString(string paletteString)
        {
            List<ColorBgra> items = new List<ColorBgra>();
            StringReader reader = new StringReader(paletteString);
            while (true)
            {
                ColorBgra bgra;
                string line = reader.ReadLine();
                if (line == null)
                {
                    return items.ToArrayEx<ColorBgra>();
                }
                if (this.ParsePaletteLine(line, out bgra) && (items.Count < this.PaletteColorCount))
                {
                    items.Add(bgra);
                }
            }
        }

        public string RemoveComments(string line)
        {
            int index = line.IndexOf(';');
            if (index != -1)
            {
                return line.Substring(0, index);
            }
            return line;
        }

        public void Save()
        {
            this.EnsurePalettesPathExists();
            string palettesPath = this.PalettesPath;
            foreach (string str2 in this.palettes.Keys)
            {
                ColorBgra[] colors = this.palettes[str2];
                ColorBgra[] palette = this.EnsureValidPaletteSize(colors);
                string str3 = Path.ChangeExtension(str2, this.PalettesFileExtension);
                string palettePath = Path.Combine(palettesPath, str3);
                this.SavePalette(palettePath, palette);
            }
        }

        public void SavePalette(string palettePath, IEnumerable<ColorBgra> palette)
        {
            FileStream stream = new FileStream(palettePath, FileMode.Create, FileAccess.Write, FileShare.Read);
            try
            {
                StreamWriter writer = new StreamWriter(stream, paletteFileEncoding);
                try
                {
                    string paletteSaveString = this.GetPaletteSaveString(palette);
                    writer.WriteLine(paletteSaveString);
                }
                finally
                {
                    writer.Close();
                    writer = null;
                    stream = null;
                }
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream = null;
                }
            }
        }

        public string TryGetPalettesPath()
        {
            try
            {
                return this.PalettesPath;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool ValidatePaletteName(string paletteName)
        {
            if (string.IsNullOrEmpty(paletteName))
            {
                return false;
            }
            try
            {
                string str = Path.ChangeExtension(paletteName, this.PalettesFileExtension);
                char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
                char[] invalidPathChars = Path.GetInvalidPathChars();
                if (str.IndexOfAny(invalidFileNameChars) != -1)
                {
                    return false;
                }
                return true;
            }
            catch (ArgumentNullException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        public ColorBgra DefaultColor =>
            ColorBgra.White;

        public IReadOnlyList<ColorBgra> DefaultPalette =>
            defaultPaletteRO;

        private bool DoesPalettesPathExist
        {
            get
            {
                string path = this.TryGetPalettesPath();
                if (path == null)
                {
                    return false;
                }
                try
                {
                    return Directory.Exists(path);
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public static UserPalettesService Instance =>
            instance;

        public int PaletteColorCount =>
            0x60;

        public IReadOnlyList<string> PaletteNames
        {
            get
            {
                object sync = this.sync;
                lock (sync)
                {
                    return this.palettes.Keys.ToArrayEx<string>();
                }
            }
        }

        public string PalettesFileExtension =>
            ".txt";

        public string PalettesPath =>
            UserFilesService.Instance.GetLocalizedDirectoryPath("ColorPalettes.UserDataSubDirName");

        private object Sync =>
            this.sync;
    }
}

