using System;
using GameData;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering;

namespace ColorBlindFilter;

public class Patches
{
    [HarmonyWrapSafe]
    [HarmonyPatch(typeof(GameDataInit), nameof(GameDataInit.Initialize))]
    public static class GameDataInit__Initialize__Patch
    {
        public static void Postfix()
        {
            Plugin.OnGameDataInit();
        }
    }

    [HarmonyWrapSafe]
    [HarmonyPatch(typeof(CM_Camera), nameof(CM_Camera.Awake))]
    public static class CM_Camera__Awake__Patch
    {
        public static void Postfix(CM_Camera __instance)
        {
            __instance.gameObject.AddComponent<ApplyShader>();
        }
    }
    
    [HarmonyWrapSafe]
    [HarmonyPatch(typeof(FPSCamera), nameof(FPSCamera.Setup), new Type[] {})]
    public static class ClusteredRendering__CollectCommands__Patch
    {
        //private static CommandBuffer commandBuffer;

        public static void Postfix(FPSCamera __instance)
        {
            __instance.gameObject.AddComponent<ApplyShader>();

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