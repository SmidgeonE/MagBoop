using FistVR;
using HarmonyLib;

namespace MagBoop.ModFiles
{
    public class MagazinePatches
    {
        [HarmonyPatch(typeof(FVRFireArmMagazine), "Awake")]
        [HarmonyPostfix]
        private static void GenerateBoopTrigger(FVRFireArmMagazine __instance)
        {
            
        }
    }
}