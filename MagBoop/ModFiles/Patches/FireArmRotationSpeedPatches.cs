using System;
using FistVR;
using HarmonyLib;
using Mono.CompilerServices.SymbolWriter;
using UnityEngine;

namespace MagBoop.ModFiles
{
    public class FireArmRotationSpeedPatches
    {
        [HarmonyPatch(typeof(FVRFireArm), "Awake")]
        [HarmonyPostfix]
        private static void GrabDefaultRotationSpeedPatch(FVRFireArm __instance)
        {
            if (MagBoopManager.DefaultRotationIntensities.ContainsKey(__instance)) return;
            
            MagBoopManager.DefaultRotationIntensities.Add(__instance, __instance.RotIntensity);
        }

        [HarmonyPatch(typeof(FVRFireArm), "FVRUpdate")]
        [HarmonyPostfix]
        private static void ResetRotationSpeedToDefault(FVRFireArm __instance, float ___m_rot_interp_tick)
        {
            if (!__instance.IsHeld) return;
            
            if (__instance.RootRigidbody.angularVelocity.magnitude < 0.9f)
                __instance.RotIntensity = MagBoopManager.DefaultRotationIntensities[__instance];
        }
    }
}