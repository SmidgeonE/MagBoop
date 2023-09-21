using System;
using System.Collections.Generic;
using System.Linq;
using FistVR;
using HarmonyLib;
using UnityEngine;

namespace MagBoop.ModFiles
{
    public class HandPatch
    {

        [HarmonyPatch(typeof(FVRViveHand), "Start")]
        [HarmonyPostfix]
        private static void GenerateBoopAudioController(FVRViveHand __instance)
        {
            var impactController = __instance.gameObject.AddComponent<AudioImpactController>();
            impactController.ImpactType = ImpactType.MagSmallPlastic;
            impactController.Alts = new List<AudioImpactController.AltImpactType>();
            impactController.CausesSonicEventOnSoundPlay = true;
            impactController.IgnoreRBs = new List<Rigidbody>();
            impactController.SetIFF(0);
        }

        [HarmonyPatch(typeof(FVRViveHand), "TestCollider")]
        [HarmonyPostfix]
        private static void CheckCol(FVRViveHand __instance, Collider collider, bool isEnter)
        {
            if (!isEnter) return;
            
            var trigScript = collider.GetComponent<TriggerProxyScript>();
            if (trigScript == null) return;
            
            trigScript.PlayBoopSound(__instance.gameObject);
        }
    }
}