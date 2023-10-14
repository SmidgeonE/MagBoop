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
        [HarmonyPatch(typeof(FVRViveHand), "TestCollider")]
        [HarmonyPostfix]
        private static void CheckCol(FVRViveHand __instance, Collider collider, bool isEnter)
        {
            if (!isEnter) return;
            
            var magTrig = collider.GetComponent<MagTriggerScript>();
            if (magTrig != null)
            {
                if (magTrig.transform.parent.GetComponent<FVRFireArmMagazine>().FireArm ==
                    __instance.CurrentInteractable)
                    return;
            
                magTrig.CheckAndPlayBoopSound(__instance);
                return;
            }

            
            var slideTrig = collider.GetComponent<SlideTriggerScript>();
            if (slideTrig != null) slideTrig.CheckSlideBoop(__instance);
        }
    }
}