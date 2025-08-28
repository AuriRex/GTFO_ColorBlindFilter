using System.Reflection;
using HarmonyLib;
using ColorBlindFilter;
using Il2CppInterop.Runtime.Injection;
using TheArchive.Core;
using TheArchive.Core.Attributes;
using TheArchive.Core.Localization;
using TheArchive.Interfaces;

[assembly: AssemblyVersion(ColorBlindFilterModule.VERSION)]
[assembly: AssemblyFileVersion(ColorBlindFilterModule.VERSION)]
[assembly: AssemblyInformationalVersion(ColorBlindFilterModule.VERSION)]

namespace ColorBlindFilter;

[ArchiveModule(GUID, MOD_NAME, VERSION)]
public class ColorBlindFilterModule : IArchiveModule
{
    public ILocalizationService LocalizationService { get; set; }
    
    public IArchiveLogger Logger { get; set; }

    public const string GUID = "dev.aurirex.gtfo.colorblindfilter";
    public const string MOD_NAME = ManifestInfo.TSName;
    public const string VERSION = ManifestInfo.TSVersion;

    private static readonly Harmony _harmony = new(GUID);

    public void Init()
    {
        ClassInjector.RegisterTypeInIl2Cpp<ApplyShader>();
    }
}