using System;
using FistVR;
using HarmonyLib;
using Stovepipe;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MagBoop.ModFiles
{
    public class MagazineUnseatingPatches
    {
        [HarmonyPatch(typeof(FVRFireArmMagazine), "Load", typeof(FVRFireArm))]
        [HarmonyPostfix]
        private static void UnSeatMagDiceRoll(FVRFireArmMagazine __instance)
        {
            if (!__instance.IsExtractable) return;
            if (__instance.IsEnBloc) return;
            if (__instance.FireArm is null) return;
            
            var magBoopComp = __instance.GetComponent<MagazineBoopComponent>();
            if (magBoopComp is null) return;
            if (magBoopComp.thisTrigger.isUnSeated) return;
            
            if (Random.Range(0f, 1f) >= UserConfig.MagUnseatedProbability.Value) return;
            
            // Unseat Mag

            __instance.transform.position -= __instance.transform.up * 0.015f;
            magBoopComp.thisTrigger.isUnSeated = true;
            magBoopComp.thisTrigger.hasAlreadyTappedOnce = false;
            
            var doubleFeedData = __instance.FireArm.GetComponent<DoubleFeedData>();
            if (doubleFeedData == null) return;
            
            doubleFeedData.doubleFeedChance *= UserConfig.DoubleFeedMultiplier.Value;
            doubleFeedData.doubleFeedMaxChance *= UserConfig.DoubleFeedMultiplier.Value;
        }

        [HarmonyPatch(typeof(FVRFireArmMagazine), "Release")]
        [HarmonyPatch(typeof(FVRFireArmMagazine), "ReleaseFromAttachableFireArm")]
        [HarmonyPatch(typeof(FVRFireArmMagazine), "ReleaseFromSecondarySlot")]
        [HarmonyPostfix]
        private static void StopFromMakingSound(FVRFireArmMagazine __instance)
        {
            var magBoopComp = __instance.GetComponent<MagazineBoopComponent>();
            if (magBoopComp is null) return;

            magBoopComp.thisTrigger.StartCooldownTimer();
        }
    }
}