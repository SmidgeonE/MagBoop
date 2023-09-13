using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace MagBoop.ModFiles
{
    [BepInPlugin("dll.smidgeon.magboop", "Mag Boop", "1.0.0")]
    [BepInProcess("h3vr.exe")]
    public class MagBoopManager : BaseUnityPlugin
    {
    
        private void Start()
        {
            Debug.Log("starting mag boop");

            Harmony.CreateAndPatchAll(typeof(MagazinePatches));

        }
    }
}