namespace PaintDotNet.Effects
{
    using PaintDotNet;
    using PaintDotNet.AppModel;
    using PaintDotNet.Collections;
    using PaintDotNet.Functional;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal sealed class EffectsCollection
    {
        private Assembly[] assemblies;
        private static readonly Tuple<string, string, Version, PluginBlockReason>[] blockedEffects = new Tuple<string, string, Version, PluginBlockReason>[] { 
            Tuple.Create<string, string, Version, PluginBlockReason>("PaintDotNet.Effects", "PDNBulkUpdaterEffect", new Version(1, 0, 0x7fffffff, 0x7fffffff), PluginBlockReason.Incompatible | PluginBlockReason.Unstable), Tuple.Create<string, string, Version, PluginBlockReason>("FilmEffect", "FilmEffect", new Version(1, 0, 0x7fffffff, 0x7fffffff), PluginBlockReason.NotBlocked | PluginBlockReason.Unstable | PluginBlockReason.UpdateRequired), Tuple.Create<string, string, Version, PluginBlockReason>("EdHarvey.Edfects.Effects", "ThresholdEffect", new Version(1, 0, 0x7fffffff, 0x7fffffff), PluginBlockReason.NotBlocked | PluginBlockReason.Unstable | PluginBlockReason.UpdateRequired), Tuple.Create<string, string, Version, PluginBlockReason>("InkSketch", "EffectPlugin", new Version(1, 0, 0x7fffffff, 0x7fffffff), PluginBlockReason.NotBlocked | PluginBlockReason.NowBuiltIn), Tuple.Create<string, string, Version, PluginBlockReason>("PortraitEffect", "EffectPlugin", new Version(1, 0, 0x7fffffff, 0x7fffffff), PluginBlockReason.NotBlocked | PluginBlockReason.NowBuiltIn), Tuple.Create<string, string, Version, PluginBlockReason>("HistogramEffects", "ReduceNoiseEffect", new Version(1, 1, 0x7fffffff, 0x7fffffff), PluginBlockReason.NotBlocked | PluginBlockReason.NowBuiltIn), Tuple.Create<string, string, Version, PluginBlockReason>("EdHarvey.Edfects.Effects", "FragmentEffect", new Version(3, 20, 0x7fffffff, 0x7fffffff), PluginBlockReason.NotBlocked | PluginBlockReason.NowBuiltIn), Tuple.Create<string, string, Version, PluginBlockReason>("EdHarvey.Edfects.Effects", "PosterizeEffect", new Version(3, 0x1f, 0x7fffffff, 0x7fffffff), PluginBlockReason.NotBlocked | PluginBlockReason.NowBuiltIn), Tuple.Create<string, string, Version, PluginBlockReason>("EdHarvey.Edfects.Effects", "LensBlurEffect", new Version(3, 0x24, 0x7fffffff, 0x7fffffff), PluginBlockReason.NotBlocked | PluginBlockReason.NowBuiltIn), Tuple.Create<string, string, Version, PluginBlockReason>("EdHarvey.Edfects.Effects", "DentsWarpPlusEffect", new Version(3, 0x24, 0x7fffffff, 0x7fffffff), PluginBlockReason.NotBlocked | PluginBlockReason.NowBuiltIn), Tuple.Create<string, string, Version, PluginBlockReason>("EdHarvey.Edfects.Effects", "CrystalizeEffect", new Version(3, 0x24, 0x7fffffff, 0x7fffffff), PluginBlockReason.NotBlocked | PluginBlockReason.NowBuiltIn), Tuple.Create<string, string, Version, PluginBlockReason>("EdHarvey.Edfects.Effects", "SurfaceBlurEffect", new Version(3, 0x24, 0x7fffffff, 0x7fffffff), PluginBlockReason.NotBlocked | PluginBlockReason.NowBuiltIn), Tuple.Create<string, string, Version, PluginBlockReason>("pyrochild.effects", "randomeffect", new Version(2, 0, 0x7fffffff, 0x7fffffff), PluginBlockReason.Incompatible | PluginBlockReason.UpdateRequired), Tuple.Create<string, string, Version, PluginBlockReason>("pyrochild.effects.splatter", "Splatter", new Version(1, 0, 0x7fffffff, 0x7fffffff), PluginBlockReason.NotBlocked | PluginBlockReason.Unstable | PluginBlockReason.UpdateRequired), Tuple.Create<string, string, Version, PluginBlockReason>("zachwalker.CurvesPlus", "CurvesPlus", new Version(2, 4, 0x7fffffff, 0x7fffffff), PluginBlockReason.NotBlocked | PluginBlockReason.Unstable | PluginBlockReason.UpdateRequired), Tuple.Create<string, string, Version, PluginBlockReason>("pyrochild.effects.smudge", "Smudge", new Version(3, 5, 0x7fffffff, 0x7fffffff), PluginBlockReason.Incompatible | PluginBlockReason.UpdateRequired),
            Tuple.Create<string, string, Version, PluginBlockReason>("pyrochild.effects.randomeffect", "RandomEffect", new Version(1, 2, 0x7fffffff, 0x7fffffff), PluginBlockReason.Incompatible), Tuple.Create<string, string, Version, PluginBlockReason>("pyrochild.effects.splatter", "Splatter", new Version(1, 5, 0x7fffffff, 0x7fffffff), PluginBlockReason.Incompatible), Tuple.Create<string, string, Version, PluginBlockReason>("pyrochild.effects.scriptlab", "ScriptLab", new Version(2, 4, 0x7fffffff, 0x7fffffff), PluginBlockReason.Incompatible), Tuple.Create<string, string, Version, PluginBlockReason>("pyrochild.effects.curvesplus", "CurvesPlus", new Version(2, 10, 0x7fffffff, 0x7fffffff), PluginBlockReason.Incompatible), Tuple.Create<string, string, Version, PluginBlockReason>("pyrochild.effects.gradientmapping", "GradientMapping", new Version(2, 2, 0x7fffffff, 0x7fffffff), PluginBlockReason.Incompatible), Tuple.Create<string, string, Version, PluginBlockReason>("pyrochild.effects.colormatch", "ColorMatch", new Version(1, 0, 0x7fffffff, 0x7fffffff), PluginBlockReason.Incompatible), Tuple.Create<string, string, Version, PluginBlockReason>("GREYCstoration", "BaseEffect", new Version(0, 9, 0x7fffffff, 0x7fffffff), PluginBlockReason.NotBlocked | PluginBlockReason.Unstable), Tuple.Create<string, string, Version, PluginBlockReason>("GREYCstoration", "RestoreEffect", new Version(0, 9, 0x7fffffff, 0x7fffffff), PluginBlockReason.NotBlocked | PluginBlockReason.Unstable), Tuple.Create<string, string, Version, PluginBlockReason>("CustomBrushes", "EffectPlugin", new Version(5, 1, 5, 0x7fffffff), PluginBlockReason.NotBlocked | PluginBlockReason.Unstable), Tuple.Create<string, string, Version, PluginBlockReason>("EffectCaller", "EffectPlugin", new Version(1, 0, 0x7fffffff, 0x7fffffff), PluginBlockReason.Incompatible), Tuple.Create<string, string, Version, PluginBlockReason>("StockLibrary", "EffectPlugin", new Version(1, 5, 0x7fffffff, 0x7fffffff), PluginBlockReason.NotBlocked | PluginBlockReason.Unstable), Tuple.Create<string, string, Version, PluginBlockReason>("OldPhoto", "EffectPlugin", new Version(1, 3, 0x7fffffff, 0x7fffffff), PluginBlockReason.Incompatible), Tuple.Create<string, string, Version, PluginBlockReason>("Flags", "EffectPlugin", new Version(1, 0, 0x7fffffff, 0x7fffffff), PluginBlockReason.NotBlocked | PluginBlockReason.Unstable), Tuple.Create<string, string, Version, PluginBlockReason>("PaintDotNet.Effects", "PDNOctagonal", new Version(1, 1, 0x7fffffff, 0x7fffffff), PluginBlockReason.NotBlocked | PluginBlockReason.Unstable), Tuple.Create<string, string, Version, PluginBlockReason>("ColorBalanceEffect", "ColorBalanceEffectPlugin", new Version(1, 1, 0xc6d, 0x7fffffff), PluginBlockReason.Incompatible | PluginBlockReason.UpdateRequired), Tuple.Create<string, string, Version, PluginBlockReason>("TransparencyEffect", "TransparencyEffectPlugin", new Version(1, 1, 0xc6d, 0x7fffffff), PluginBlockReason.Incompatible | PluginBlockReason.UpdateRequired),
            Tuple.Create<string, string, Version, PluginBlockReason>("Flip", "FlipHEffect", new Version(1, 0x7fffffff, 0x7fffffff, 0x7fffffff), PluginBlockReason.Incompatible | PluginBlockReason.UpdateRequired), Tuple.Create<string, string, Version, PluginBlockReason>("Flip", "FlipVEffect", new Version(1, 0x7fffffff, 0x7fffffff, 0x7fffffff), PluginBlockReason.Incompatible | PluginBlockReason.UpdateRequired), Tuple.Create<string, string, Version, PluginBlockReason>("Pastel", "PastelPlugin", new Version(1, 2, 0xc6d, 0x7fffffff), PluginBlockReason.Incompatible | PluginBlockReason.UpdateRequired), Tuple.Create<string, string, Version, PluginBlockReason>("GaussianBlurPlus", "GaussianBlurPlusPlugin", new Version(1, 1, 0xc6d, 0x7fffffff), PluginBlockReason.Incompatible | PluginBlockReason.UpdateRequired), Tuple.Create<string, string, Version, PluginBlockReason>("FeatherEffect", "FeatherPlugin", new Version(2, 3, 0xc6d, 0x7fffffff), PluginBlockReason.Incompatible | PluginBlockReason.UpdateRequired), Tuple.Create<string, string, Version, PluginBlockReason>("OutlineObjectEffect", "OutlineObjectEffectPlugin", new Version(1, 1, 0xc57, 0x7fffffff), PluginBlockReason.Incompatible | PluginBlockReason.UpdateRequired), Tuple.Create<string, string, Version, PluginBlockReason>("LomographyEffect", "LomographyEffectPlugin", new Version(1, 1, 0xc6d, 0x7fffffff), PluginBlockReason.Incompatible | PluginBlockReason.UpdateRequired), Tuple.Create<string, string, Version, PluginBlockReason>("Polygon", "EffectPlugin", new Version(1, 0, 0x7fffffff, 0x7fffffff), PluginBlockReason.Incompatible | PluginBlockReason.UpdateRequired), Tuple.Create<string, string, Version, PluginBlockReason>("BurninateEffect", "BurninateEffectPlugin", new Version(1, 1, 0xc6d, 0x7fffffff), PluginBlockReason.Incompatible | PluginBlockReason.UpdateRequired), Tuple.Create<string, string, Version, PluginBlockReason>("BevelSelection", "BevelSelectionPlugin", new Version(1, 0, 0xc33, 0x7fffffff), PluginBlockReason.Incompatible | PluginBlockReason.UpdateRequired), Tuple.Create<string, string, Version, PluginBlockReason>("SelectionTools", "BevelSelectionPlugin", new Version(1, 1, 0xc6d, 0x7fffffff), PluginBlockReason.Incompatible | PluginBlockReason.UpdateRequired), Tuple.Create<string, string, Version, PluginBlockReason>("SelectionTools", "FeatherSelectionEffectPlugin", new Version(1, 1, 0xc6d, 0x7fffffff), PluginBlockReason.Incompatible | PluginBlockReason.UpdateRequired), Tuple.Create<string, string, Version, PluginBlockReason>("SelectionTools", "OutlineSelectionEffectPlugin", new Version(1, 1, 0xc6d, 0x7fffffff), PluginBlockReason.Incompatible | PluginBlockReason.UpdateRequired), Tuple.Create<string, string, Version, PluginBlockReason>("Reveal1Effect", "Reveal1", new Version(1, 0, 0x7fffffff, 0x7fffffff), PluginBlockReason.Incompatible | PluginBlockReason.UpdateRequired), Tuple.Create<string, string, Version, PluginBlockReason>("Reveal3Effect", "Reveal3", new Version(1, 0, 0x7fffffff, 0x7fffffff), PluginBlockReason.Incompatible | PluginBlockReason.UpdateRequired), Tuple.Create<string, string, Version, PluginBlockReason>("PaintDotNet.Effects", "SteganographyFillEffect", new Version(1, 0, 0x7fffffff, 0x7fffffff), PluginBlockReason.Incompatible | PluginBlockReason.UpdateRequired),
            Tuple.Create<string, string, Version, PluginBlockReason>("ShadowHighlight", "EffectPlugin", new Version(1, 0, 0x7fffffff, 0x7fffffff), PluginBlockReason.Incompatible | PluginBlockReason.UpdateRequired), Tuple.Create<string, string, Version, PluginBlockReason>("ColorToAlpha", "EffectPlugin", new Version(1, 0, 0x7fffffff, 0x7fffffff), PluginBlockReason.Incompatible | PluginBlockReason.UpdateRequired), Tuple.Create<string, string, Version, PluginBlockReason>("ColorMixer", "EffectPlugin", new Version(1, 0, 0x7fffffff, 0x7fffffff), PluginBlockReason.Incompatible | PluginBlockReason.UpdateRequired), Tuple.Create<string, string, Version, PluginBlockReason>("LocalContrast", "EffectPlugin", new Version(1, 0, 0x7fffffff, 0x7fffffff), PluginBlockReason.Incompatible | PluginBlockReason.UpdateRequired)
        };
        private static readonly Guid[] deprecatedEffectGuids = new Guid[] { new Guid("9A1EB3D9-0A36-4d32-9BB2-707D6E5A9D2C"), new Guid("3154E367-6B4D-4960-B4D8-F6D06E1C9C24"), new Guid("1445F876-356D-4a7c-B726-50457F6E7AEF"), new Guid("270DCBF1-CE42-411e-9885-162E2BFA8265") };
        private List<Type> effects;
        private static Dictionary<string, List<Tuple<string, Version, PluginBlockReason>>> indexedBlockedEffects;
        private List<PluginErrorInfo> loaderExceptions;

        public EffectsCollection(List<Assembly> assemblies)
        {
            this.loaderExceptions = new List<PluginErrorInfo>();
            this.assemblies = assemblies.ToArrayEx<Assembly>();
            this.effects = null;
        }

        public EffectsCollection(List<Type> effects)
        {
            this.loaderExceptions = new List<PluginErrorInfo>();
            this.assemblies = null;
            this.effects = new List<Type>(effects);
        }

        private void AddLoaderException(PluginErrorInfo loaderException)
        {
            EffectsCollection effectss = this;
            lock (effectss)
            {
                this.loaderExceptions.Add(loaderException);
            }
        }

        private static bool CheckForAnyGuidOnType(Type type, IEnumerable<Guid> guids) => 
            guids.Any<Guid>(guid => CheckForGuidOnType(type, guid));

        private static bool CheckForGuidOnType(Type type, Guid guid)
        {
            Func<GuidAttribute, bool> <>9__1;
            return () => type.GetCustomAttributes(true).OfType<GuidAttribute>().Any<GuidAttribute>((<>9__1 ?? (<>9__1 = a => new Guid(a.Value) == guid))).Eval<bool>().Repair<bool>(ex => false).Value;
        }

        private static Version GetAssemblyVersionFromType(Type type)
        {
            try
            {
                AssemblyName name = new AssemblyName(type.Assembly.FullName);
                return name.Version;
            }
            catch (Exception)
            {
                return new Version(0, 0, 0, 0);
            }
        }

        private static List<Type> GetEffectsFromAssemblies(Assembly[] assemblies, IList<PluginErrorInfo> errorsResult)
        {
            List<Type> effectsResult = new List<Type>();
            foreach (Assembly assembly in assemblies)
            {
                GetEffectsFromAssembly(assembly, effectsResult, errorsResult);
            }
            List<Type> list2 = new List<Type>();
            foreach (Type type in effectsResult)
            {
                Exception error = IsBannedEffect(type);
                if (error != null)
                {
                    list2.Add(type);
                    errorsResult.Add(new PluginErrorInfo(type.Assembly, type, error));
                }
            }
            foreach (Type type2 in list2)
            {
                effectsResult.Remove(type2);
            }
            return effectsResult;
        }

        private static void GetEffectsFromAssembly(Assembly assembly, IList<Type> effectsResult, IList<PluginErrorInfo> errorsResult)
        {
            try
            {
                foreach (Type type in GetTypesFromAssembly(assembly, errorsResult))
                {
                    try
                    {
                        if ((type.IsSubclassOf(typeof(Effect)) && !type.IsAbstract) && !type.IsObsolete(false))
                        {
                            effectsResult.Add(type);
                        }
                    }
                    catch (Exception exception)
                    {
                        errorsResult.Add(new PluginErrorInfo(assembly, type, exception));
                    }
                }
            }
            catch (ReflectionTypeLoadException)
            {
            }
        }

        public PluginErrorInfo[] GetLoaderExceptions()
        {
            EffectsCollection effectss = this;
            lock (effectss)
            {
                return this.loaderExceptions.ToArrayEx<PluginErrorInfo>();
            }
        }

        private static Type[] GetTypesFromAssembly(Assembly assembly, IList<PluginErrorInfo> errorsResult)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                List<Type> items = new List<Type>();
                foreach (Type type in exception.Types)
                {
                    if (type != null)
                    {
                        items.Add(type);
                    }
                }
                foreach (Exception exception2 in exception.LoaderExceptions)
                {
                    if ((exception2 is TypeLoadException) || (exception2 is FileNotFoundException))
                    {
                        errorsResult.Add(new PluginErrorInfo(assembly, null, exception2));
                    }
                    else
                    {
                        Exception[] innerExceptions = new Exception[] { exception2, exception };
                        throw new InternalErrorException($"Unexpected: rex.LoaderException[n] is not a recognized Exception type! (assembly.Location={assembly.Location})", new AggregateException(innerExceptions));
                    }
                }
                return items.ToArrayEx<Type>();
            }
        }

        private static Exception IsBannedEffect(Type effectType)
        {
            List<Tuple<string, Version, PluginBlockReason>> list;
            if (effectType.Assembly == typeof(Effect).Assembly)
            {
                return null;
            }
            if (indexedBlockedEffects == null)
            {
                Tuple<string, string, Version, PluginBlockReason>[] blockedEffects = EffectsCollection.blockedEffects;
                lock (blockedEffects)
                {
                    indexedBlockedEffects = new Dictionary<string, List<Tuple<string, Version, PluginBlockReason>>>(StringComparer.InvariantCultureIgnoreCase);
                    foreach (Tuple<string, string, Version, PluginBlockReason> tuple in EffectsCollection.blockedEffects)
                    {
                        List<Tuple<string, Version, PluginBlockReason>> list2;
                        string str3 = tuple.Item1;
                        Tuple<string, Version, PluginBlockReason> item = tuple.GetTuple234<string, string, Version, PluginBlockReason>();
                        if (!indexedBlockedEffects.TryGetValue(str3, out list2))
                        {
                            list2 = new List<Tuple<string, Version, PluginBlockReason>>();
                            indexedBlockedEffects.Add(str3, list2);
                        }
                        list2.Add(item);
                    }
                }
            }
            Version assemblyVersionFromType = GetAssemblyVersionFromType(effectType);
            string key = effectType.Namespace;
            string name = effectType.Name;
            if (indexedBlockedEffects.TryGetValue(key, out list))
            {
                foreach (Tuple<string, Version, PluginBlockReason> tuple3 in list)
                {
                    if ((string.Compare(name, tuple3.Item1, StringComparison.InvariantCultureIgnoreCase) == 0) && (assemblyVersionFromType <= tuple3.Item2))
                    {
                        return new BlockedPluginException(tuple3.Item3);
                    }
                }
            }
            if (!CheckForAnyGuidOnType(effectType, deprecatedEffectGuids) || ((string.Compare(key, "GlowEffect", StringComparison.InvariantCultureIgnoreCase) != 0) && (string.Compare(key, "DistortionEffects", StringComparison.InvariantCultureIgnoreCase) != 0)))
            {
                return null;
            }
            return new BlockedPluginException(PluginBlockReason.NotBlocked | PluginBlockReason.NowBuiltIn);
        }

        public Type[] Effects
        {
            get
            {
                EffectsCollection effectss = this;
                lock (effectss)
                {
                    if (this.effects == null)
                    {
                        List<PluginErrorInfo> errorsResult = new List<PluginErrorInfo>();
                        this.effects = GetEffectsFromAssemblies(this.assemblies, errorsResult);
                        for (int i = 0; i < errorsResult.Count; i++)
                        {
                            this.AddLoaderException(errorsResult[i]);
                        }
                    }
                }
                return this.effects.ToArrayEx<Type>();
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly EffectsCollection.<>c <>9 = new EffectsCollection.<>c();
            public static Func<Exception, bool> <>9__10_2;

            internal bool <CheckForGuidOnType>b__10_2(Exception ex) => 
                false;
        }
    }
}

