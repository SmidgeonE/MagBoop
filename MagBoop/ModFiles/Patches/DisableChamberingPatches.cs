using FistVR;
using HarmonyLib;
using UnityEngine;

namespace MagBoop.ModFiles
{
    public class DisableChamberingPatches
    {
        [HarmonyPatch(typeof(ClosedBolt), "BoltEvent_ExtractRoundFromMag")]
        [HarmonyPrefix]
        private static bool StopFromLoadingRoundIfMagIsFullyUnSeated(ClosedBolt __instance)
        {
            if (__instance.Weapon.Magazine == null) return true;
            
            var magBoopComp = __instance.Weapon.Magazine.GetComponent<MagazineBoopComponent>();
            if (magBoopComp == null) return true;

            if (magBoopComp.thisTrigger.isUnSeated && !magBoopComp.thisTrigger.hasAlreadyTappedOnce &&
                UserConfig.EnableMagUnSeating.Value)
            {
                return false;
            }

            return true;
        }
        
        [HarmonyPatch(typeof(HandgunSlide), "SlideEvent_ExtractRoundFromMag")]
        [HarmonyPrefix]
        private static bool StopFromLoadingRoundIfMagIsFullyUnSeated(HandgunSlide __instance)
        {
            if (__instance.Handgun.Magazine == null) return true;
            
            var magBoopComp = __instance.Handgun.Magazine.GetComponent<MagazineBoopComponent>();
            if (magBoopComp == null) return true;

            if (magBoopComp.thisTrigger.isUnSeated && !magBoopComp.thisTrigger.hasAlreadyTappedOnce &&
                UserConfig.EnableMagUnSeating.Value)
            {
                return false;
            }

            return true;
        }
        
        [HarmonyPatch(typeof(OpenBoltReceiverBolt), "BoltEvent_BeginChamberingf")]
        [HarmonyPrefix]
        private static bool StopFromLoadingRoundIfMagIsFullyUnSeated(OpenBoltReceiverBolt __instance)
        {
            if (__instance.Receiver.Magazine == null) return true;
            
            var magBoopComp = __instance.Receiver.Magazine.GetComponent<MagazineBoopComponent>();
            if (magBoopComp == null) return true;

            if (magBoopComp.thisTrigger.isUnSeated && !magBoopComp.thisTrigger.hasAlreadyTappedOnce &&
                UserConfig.EnableMagUnSeating.Value)
            {
                return false;
            }

            return true;
        }
    }
}