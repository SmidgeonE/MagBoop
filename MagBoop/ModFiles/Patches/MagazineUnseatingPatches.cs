﻿using System;
using FistVR;
using HarmonyLib;
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

            if (Random.Range(0f, 1f) >= UserConfig.MagUnseatedProbability.Value) return;
            
            // Unseat Mag
            
            Debug.Log("Unseating Mag");

            __instance.transform.position -= __instance.transform.up * 0.04f;

            var magBoopComp = __instance.GetComponent<MagazineBoopComponent>();
            if (magBoopComp is null) return;

            magBoopComp.thisTrigger.isUnSeated = true;
        }
    }
}