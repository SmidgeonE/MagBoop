using System.Collections.Generic;
using FistVR;
using HarmonyLib;
using UnityEngine;

namespace MagBoop.ModFiles
{
    public class HandPatch
    {
        public static Dictionary<GameObject, AudioImpactController> impactControllers =
            new Dictionary<GameObject, AudioImpactController>();
        
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
            impactControllers.Add(__instance.gameObject, impactController);

            foreach (var comp in __instance.GetComponents<Component>()) Debug.Log(comp.GetType());

            var x = __instance.GetComponents<SphereCollider>();
            foreach (var col in x) col.isTrigger = false;
            
        }

        [HarmonyPatch(typeof(FVRFireArmMagazine), "Awake")]
        [HarmonyPostfix]
        private static void AddImpactProxy(FVRFireArmMagazine __instance)
        {
            __instance.gameObject.AddComponent<TriggerProxyScript>();
        }
        
    }
}