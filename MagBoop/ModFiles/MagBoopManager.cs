using BepInEx;
using BepInEx.Configuration;
using FistVR;
using HarmonyLib;
using UnityEngine;
using Valve.VR;

namespace MagBoop.ModFiles
{
    [BepInPlugin("dll.smidgeon.magboop", "Mag Boop", "1.0.0")]
    [BepInProcess("h3vr.exe")]
    public class MagBoopManager : BaseUnityPlugin
    {
        private static FVRPooledAudioSource _currentMagSoundSource;
        private static float _timeLeft;
        private static bool _hasStoppedSound = true;
        
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
                Harmony.CreateAndPatchAll(typeof(DisableChamberingPatches));
            }
        }

        public static void StartMagSoundShorteningTimer(FVRPooledAudioSource source, float time)
        {
            Debug.Log("setting time for shortening to " + time);
            _currentMagSoundSource = source;
            _timeLeft = time;
            _hasStoppedSound = false;
        }


        private void Update()
        {
            // This update function allows the mag sound to be shortened if it has been unseated.
            
            if (!UserConfig.EnableMagUnSeating.Value) return;
            if (_hasStoppedSound) return;
            if (_timeLeft < 0f)
            {
                // Stop source sound
                Debug.Log("Stopping mag sound short");
                _currentMagSoundSource.Source.Stop();

                _hasStoppedSound = true;
                return;
            }

            _timeLeft -= Time.deltaTime;
        }
        
        
        private void GenerateUserConfigs()
        {
            UserConfig.EnableMagTapping = Config.Bind("Activation", "Enable Mag Tapping", true,
                "This enables the mag tapping aspect of the mod.");
            UserConfig.EnableMagUnSeating = Config.Bind("Activation", "Enable Mag Unseating", true,
                "This enables the mag to be unseated, based on random chance as well as how fast / hard you push the magazine in.");
            UserConfig.MagUnseatedProbability = Config.Bind("Probabilities", "Probability of Being Unseated", 0.4f,
                "This is the base probability that the magazine will be seated incorrectly even if you push it in with decent force.");
            UserConfig.DoubleFeedMultiplier = Config.Bind("Probabilities", "Double Feed Probability Multiplier", 5f,
                "This is the multiplier applied to the double feeding chance of a weapon that has a magazine which isn't seated properly.");
            UserConfig.MagRequiresTwoTapsProbability = Config.Bind("Probabilities", "Magazine Requires Two Taps Probability", 0.3f,
                "This is the probability a single tap will not fully seat the magazine back into place.");
        }
    }
}