using Player;
using TheArchive.Core;
using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.Attributes.Feature.Members;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Settings;
using TheArchive.Core.Models;
using TheArchive.Interfaces;
using TheArchive.Utilities;
using UnityEngine;

namespace ColorBlindFilter;

[EnableFeatureByDefault]
public class ColorblindFeature : Feature
{
    public override string Name => "Color Blind Filter";

    public override string Description => "Apply a color blind filter on the game.";

    public override FeatureGroup Group => FeatureGroups.Accessibility;

    public new static IArchiveLogger FeatureLogger { get; set; }
    
    [FeatureConfig]
    public static ColorBlindSettings Settings { get; set; }

    public class ColorBlindSettings
    {
        [FSDisplayName("Color Blind Filter For Menus")]
        [FSDescription("Apply Color Blind Filter for menu scenes.")]
        public bool ApplyInMenus { get; set; } = true;
        
        [FSDisplayName("Color Blind Filter In Game")]
        [FSDescription("Apply Color Blind Filter when playing the game.")]
        public bool ApplyDuringGameplay { get; set; } = true;
        
        [FSHide]
        [FSDisplayName("Brightness Multiplier")]
        [FSDescription("Multiplier for overall image brightness. (0.82 is 'default', aka 100%)")]
        [FSSlider(0.1f, 1.2f, FSSlider.SliderStyle.FloatTwoDecimal)]
        public float Multiplier { get; set; } = 1.0f;
        
        [FSDisplayName("Color Blind Mode")]
        [FSDescription("The mode to use.")]
        public ColorBlindMode Mode { get; set; } = ColorBlindMode.Normal;

        [FSDisplayName("Luminance Boost")]
        [FSDescription("Boosts the overall color values that get applied.\nTry disabling this if the image is too bright!\n\n(Does <u>not</u> apply to Custom!)")]
        public bool Boost { get; set; } = true;
        
        [FSHeader(":// Custom Color Mixer Settings")]
        [FSDisplayName("Red Channel")]
        [FSDescription("How to transform the red color channel.\n\nMake sure to select the 'Custom' Mode above!")]
        [FSIdentifier("Color.Red")]
        public SColor CustomRed { get; set; } = new SColor(1, 0, 0);
        
        [FSDisplayName("Green Channel")]
        [FSDescription("How to transform the green color channel.\n\nMake sure to select the 'Custom' Mode above!")]
        [FSIdentifier("Color.Green")]
        public SColor CustomGreen { get; set; } = new SColor(0, 1, 0);
        
        [FSDisplayName("Blue Channel")]
        [FSDescription("How to transform the blue color channel.\n\nMake sure to select the 'Custom' Mode above!")]
        [FSIdentifier("Color.Blue")]
        public SColor CustomBlue { get; set; } = new SColor(0, 0, 1);
    }
    
    private static ApplyShader _menuCBApplier;
    private static ApplyShader _gameCBApplier;
    
    // https://www.alanzucconi.com/2015/12/16/color-blindness/
    public enum ColorBlindMode
    {
        Normal,
        Custom,
        Protanopia,
        Protanomaly,
        Deuteranopia,
        Deuteranomaly,
        Tritanopia,
        Tritanomaly,
        Achromatopsia,
        Achromatomaly,
    }

    public override void Init()
    {
        var info = new Attribution.AttributionInfo("Color Blind Shader",
            "Alan Zucconi\n\nhttps://www.alanzucconi.com/2015/12/16/color-blindness/",
            "",
            "ColorBlindFilter");
        Attribution.Add(info);
    }

    public override void OnGameDataInitialized()
    {
        TryLoadMaterial();
        
        SetCustomColors();

        ApplySettings(Settings);
    }

    private static void SetCustomColors()
    {
        ColorBlindnessValues.CustomColors[0] = Settings.CustomRed.ToUnityColor();
        ColorBlindnessValues.CustomColors[1] = Settings.CustomGreen.ToUnityColor();
        ColorBlindnessValues.CustomColors[2] = Settings.CustomBlue.ToUnityColor();
    }

    public override void OnEnable()
    {
        TryLoadMaterial();
        
        SetCustomColors();
        ApplySettings(Settings);
        
        ApplyShaderTo(CM_Camera.Current);
        
        if (!PlayerManager.TryGetLocalPlayerAgent(out var player))
            return;
        
        ApplyShaderTo(player.FPSCamera);
    }

    public override void OnDisable()
    {
        _gameCBApplier?.SafeDestroy();
        _menuCBApplier?.SafeDestroy();
    }

    private static void TryLoadMaterial()
    {
        var mat = ColorBlindnessValues.Material;
        
        if (mat != null)
            return;
        
        var bundle = AssetBundle.LoadFromMemory(Resources.Data.colorblindshader);
        mat = bundle.LoadAsset("assets/_shaderthingie/hidden_colorblindness.mat").Cast<Material>();
        bundle.Unload(unloadAllLoadedObjects: false);
        
        UnityEngine.Object.DontDestroyOnLoad(mat);
        mat.hideFlags = HideFlags.DontUnloadUnusedAsset | HideFlags.HideAndDontSave;

        ColorBlindnessValues.Material = mat;
    }

    public override void OnFeatureSettingChanged(FeatureSetting setting)
    {
        if (setting.Identifier.StartsWith("Color."))
        {
            SetCustomColors();
        }
        
        ApplySettings(Settings);
    }

    private static void ApplySettings(ColorBlindSettings settings)
    {
        FeatureLogger.Debug($"{nameof(ApplySettings)} called: {settings.Mode}, GamePlay: {settings.ApplyDuringGameplay} ({_gameCBApplier}), Menu: {settings.ApplyInMenus} ({_menuCBApplier})");
        
        ColorBlindnessValues.Boost = settings.Boost;
        ColorBlindnessValues.GlobalMultiplier = settings.Multiplier;
        ColorBlindnessValues.ApplyMode(settings.Mode);
        
        if (settings.Mode == ColorBlindMode.Normal
            && Mathf.Approximately(settings.Multiplier, 1f))
        {
            _gameCBApplier?.SafeDestroy();
            _menuCBApplier?.SafeDestroy();
            return;
        }
        
        if (!settings.ApplyDuringGameplay)
        {
            _gameCBApplier?.SafeDestroy();
        }
        else
        {
            if (PlayerManager.TryGetLocalPlayerAgent(out var player))
            {
                ApplyShaderTo(player.FPSCamera);
            }
        }

        if (!settings.ApplyInMenus)
        {
            _menuCBApplier?.SafeDestroy();
        }
        else
        {
            ApplyShaderTo(CM_Camera.Current);
        }
    }

    private static void ApplyShaderTo(CM_Camera camera)
    {
        if (!Settings.ApplyInMenus)
            return;

        if (camera == null)
            return;
        
        var go = camera.Camera.gameObject;
        
        if (go.GetComponent<ApplyShader>() == null)
            _menuCBApplier = go.AddComponent<ApplyShader>();
    }
    
    private static void ApplyShaderTo(FPSCamera camera)
    {
        if (!Settings.ApplyDuringGameplay)
            return;
        
        if (camera == null)
            return;
        
        var go = camera.gameObject;
        
        if (go.GetComponent<ApplyShader>() == null)
            _gameCBApplier = go.AddComponent<ApplyShader>();
    }
    
    [ArchivePatch(typeof(CM_Camera), nameof(CM_Camera.Awake))]
    public static class CM_Camera__Awake__Patch
    {
        public static void Postfix(CM_Camera __instance)
        {
            ApplyShaderTo(__instance);
        }
    }
    
    [ArchivePatch(typeof(FPSCamera), nameof(FPSCamera.Setup), [])]
    public static class FPSCamera__Setup__Patch
    {
        //private static CommandBuffer commandBuffer;

        public static void Postfix(FPSCamera __instance)
        {
            ApplyShaderTo(__instance);

            //var camera = __instance.GetComponent<Camera>();
            //
            // commandBuffer = new CommandBuffer
            // {
            //     name = "COLORBLIND PP RENDERING COMMANDS"
            // };
            //
            // var test = BuiltinRenderTextureType.CameraTarget;
            // var currentActive = BuiltinRenderTextureType.CurrentActive;
            // commandBuffer.Blit(test, currentActive, Plugin.material);
            //
            // camera.AddCommandBuffer(CameraEvent.AfterImageEffects, commandBuffer);
        }
        
        // public static void Postfix(ClusteredRendering __instance, CommandBuffer beforeGBuffer, CommandBuffer beforeLighting)
        // {
        //     var currentActive = BuiltinRenderTextureType.CurrentActive;
        //     
        //     beforeGBuffer.Blit(currentActive, currentActive, Plugin.material);
        // }
    }
}