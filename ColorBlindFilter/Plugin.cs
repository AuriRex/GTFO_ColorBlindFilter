using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using ColorBlindFilter;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

[assembly: AssemblyVersion(Plugin.VERSION)]
[assembly: AssemblyFileVersion(Plugin.VERSION)]
[assembly: AssemblyInformationalVersion(Plugin.VERSION)]

namespace ColorBlindFilter;

[BepInPlugin(GUID, MOD_NAME, VERSION)]
public class Plugin : BasePlugin
{
    private static AssetBundle _bundle;

    public const string GUID = "dev.AuriRex.gtfo.ColorBlindFilter";
    public const string MOD_NAME = ManifestInfo.TSName;
    public const string VERSION = ManifestInfo.TSVersion;

    internal static ManualLogSource L;

    private static readonly Harmony _harmony = new(GUID);

    internal static Material Material;
    
    public override void Load()
    {
        L = Log;

        ClassInjector.RegisterTypeInIl2Cpp<ApplyShader>();
        _harmony.PatchAll(Assembly.GetExecutingAssembly());

        L.LogInfo("Plugin loaded!");
    }

    public static void OnGameDataInit()
    {
        if (Material != null)
            return;
        
        _bundle = AssetBundle.LoadFromMemory(Resources.Data.colorblindshader);
        Material = _bundle.LoadAsset("assets/_shaderthingie/hidden_colorblindness.mat").Cast<Material>();
        _bundle.Unload(unloadAllLoadedObjects: false);
        
        Object.DontDestroyOnLoad(Material);
        Material.hideFlags = HideFlags.DontUnloadUnusedAsset | HideFlags.HideAndDontSave;
        
        ColorBlindnessValues.ApplyMode(ColorBlindMode.Deuteranopia);
    }
}