﻿using System;
using FistVR;
using HarmonyLib;
using RenderHeads.Media.AVProVideo;
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

            /*Debug.Log("increaseing double feed mutli;liers");
            Debug.Log("from " + doubleFeedData.doubleFeedChance);
            Debug.Log("to " + doubleFeedData.doubleFeedChance * UserConfig.DoubleFeedMultiplier.Value);*/
            doubleFeedData.doubleFeedChance *= UserConfig.DoubleFeedMultiplier.Value;
            doubleFeedData.doubleFeedMaxChance *= UserConfig.DoubleFeedMultiplier.Value;
        }

        [HarmonyPatch(typeof(FVRFireArmMagazine), "Release")]
        [HarmonyPatch(typeof(FVRFireArmMagazine), "ReleaseFromAttachableFireArm")]
        [HarmonyPatch(typeof(FVRFireArmMagazine), "ReleaseFromSecondarySlot")]
        [HarmonyPrefix]
        private static void StopMagReleasingMakingSoundAndReducingDoubleFeed(FVRFireArmMagazine __instance)
        {
            var magBoopComp = __instance.GetComponent<MagazineBoopComponent>();
            if (magBoopComp is null) return;

            magBoopComp.thisTrigger.StartCooldownTimer();

            if (!magBoopComp.thisTrigger.isUnSeated) return;

            magBoopComp.thisTrigger.isUnSeated = false;

            var doubleFeedData = __instance.FireArm.GetComponent<DoubleFeedData>();
            if (doubleFeedData is null) return;

            doubleFeedData.doubleFeedChance /= UserConfig.DoubleFeedMultiplier.Value;
            doubleFeedData.doubleFeedMaxChance /= UserConfig.DoubleFeedMultiplier.Value;
        }

        [HarmonyPatch(typeof(FVRPooledAudioSource), "Play")]
        [HarmonyPostfix]
        private static void MagInsertionNoiseTimer(FVRPooledAudioSource __instance, Vector3 pos)
        {
            if (pos != TriggerProxyScript.CurrentMagSoundPosition) return;
            
            Debug.Log("Playing at boop location");

            MagBoopManager.StartMagSoundShorteningTimer(__instance, __instance.Source.clip.length / 2f);
        }


        /*[HarmonyPatch(typeof(AudioImpactController), "ProcessCollision")]
        [HarmonyPostfix]*/
        private static void EnvironmentMagBoopPatch(AudioImpactController __instance, Collision col
            , bool ___m_hasPlayedAudioThisFrame)
        {
            if (___m_hasPlayedAudioThisFrame)
            {
                Debug.Log("a");
                return;
            }
            
            if (col.relativeVelocity.magnitude < __instance.HitThreshold_Ignore)
            {
                Debug.Log("c");
                return;
            }
            
            if (__instance.transform.parent == null)
            {
                Debug.Log("d");
                return;
            }
            
            var mag = __instance.GetComponent<FVRFireArmMagazine>();
            if (mag == null)
            {
                Debug.Log("e");
                return;
            }
            
            var boopComp = mag.GetComponent<MagazineBoopComponent>();
            if (boopComp == null)
            {
                Debug.Log("f");
                return;
            }
            
            boopComp.thisTrigger.ReSeatMagazine();
            Debug.Log("checking for sound from environment");
        }
    }
}