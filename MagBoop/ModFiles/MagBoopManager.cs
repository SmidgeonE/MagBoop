﻿using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using Valve.VR;

namespace MagBoop.ModFiles
{
    [BepInPlugin("dll.smidgeon.magboop", "Mag Boop", "1.0.0")]
    [BepInProcess("h3vr.exe")]
    public class MagBoopManager : BaseUnityPlugin
    {
    
        private void Start()
        {
            GenerateUserConfigs();
            
            if (UserConfig.EnableMagTapping.Value)
            {
                Harmony.CreateAndPatchAll(typeof(HandPatch));
                Harmony.CreateAndPatchAll(typeof(MagazineTriggerPatches));
            }
            if (UserConfig.EnableMagUnSeating.Value)
            {
                Harmony.CreateAndPatchAll(typeof(MagazineUnseatingPatches));
            }
        }




        private void GenerateUserConfigs()
        {
            UserConfig.EnableMagTapping = Config.Bind("Activation", "Enable Mag Tapping", true,
                "This enables the mag tapping aspect of the mod.");
            UserConfig.EnableMagUnSeating = Config.Bind("Activation", "Enable Mag Unseating", true,
                "This enables the mag to be unseated, based on random chance as well as how fast / hard you push the magazine in.");
            UserConfig.MagUnseatedProbability = Config.Bind("Probabilities", "Probability of Being Unseated", 0.2f,
                "This is the base probability that the magazine will be seated incorrectly even if you push it in with decent force.");
            UserConfig.MagUnseatedProbability = Config.Bind("Probabilities", "Double Feed Probability Multiplier", 3f,
                "This is the multiplier applied to the double feeding chance of a weapon that has a magazine which isn't seated properly.");
        }
    }
}