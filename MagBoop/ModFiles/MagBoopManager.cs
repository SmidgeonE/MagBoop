using BepInEx;
using BepInEx.Configuration;
using FistVR;
using HarmonyLib;
using UnityEngine;
using Valve.VR;

namespace MagBoop.ModFiles
{
    [BepInPlugin("dll.smidgeon.magboop", "Mag Boop", "1.1.0")]
    [BepInProcess("h3vr.exe")]
    public class MagBoopManager : BaseUnityPlugin
    {
        private static FVRPooledAudioSource _currentMagSoundSource;
        private static float _defaultMagSoundVolume;
        private static float _timeLeft;
        private static float _timeUntilFadeOut;
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

            if (UserConfig.EnableSlideTapping.Value)
            {
                Harmony.CreateAndPatchAll(typeof(SlideTriggerPatches));
            }
        }

        private void Update()
        {
            // This update function allows the mag sound to be shortened if it has been unseated.
            
            if (!UserConfig.EnableMagUnSeating.Value) return;
            if (_hasStoppedSound) return;
            if (_timeLeft < 0f)
            {
                _currentMagSoundSource.Source.Stop();
                _currentMagSoundSource.Source.volume = _defaultMagSoundVolume;
                _hasStoppedSound = true;
            }

            if (_timeLeft < _timeUntilFadeOut)
                _currentMagSoundSource.Source.volume = _defaultMagSoundVolume * Time.deltaTime / _timeUntilFadeOut;

            _timeLeft -= Time.deltaTime;
        }
        
        
        private void GenerateUserConfigs()
        {
            UserConfig.EnableMagTapping = Config.Bind("Activation", "Enable Mag Tapping", true,
                "This enables the mag tapping aspect of the mod.");
            UserConfig.EnableMagUnSeating = Config.Bind("Activation", "Enable Mag Unseating", true,
                "This enables the mag to be unseated, based on random chance as well as how fast / hard you push the magazine in.");
            UserConfig.EnableCustomSounds = Config.Bind("Activation", "Enable Custom Sounds", true,
                "This enables the custom sounds, which should hopefully be better than the default sounds.");
            UserConfig.EnableTriggerDebug = Config.Bind("Activation", "Enable Slide Tapping", true,
                "This is enables the ability to tap the slide to make sure its totally forward. Useful when using Stovepipe + it's new failure to enter battery");
            UserConfig.EnableSlideTapping = Config.Bind("Debug", "Enable Mag Debug Cubes", false,
                "This is enables the weird white cubes where the boop trigger is located, can be used to see if the magazine you are using has it properly located.");
            
            UserConfig.MagUnseatedProbability = Config.Bind("Probabilities", "Probability of Being Unseated", 0.1f,
                "This is the base probability that the magazine will be seated incorrectly even if you push it in with decent force.");
            UserConfig.SlowSpeedUnseatingProbability = Config.Bind("Probabilities", "Probability of mag being unseated when inserted SLOWLY", 0.9f,
                "This is the base probability that the magazine will be seated incorrectly if you insert it really slowly.");
            UserConfig.DoubleFeedMultiplier = Config.Bind("Probabilities", "Double Feed Probability Multiplier", 5f,
                "This is the multiplier applied to the double feeding chance of a weapon that has a magazine which isn't seated properly.");
            UserConfig.MagRequiresTwoTapsProbability = Config.Bind("Probabilities", "Magazine Requires Two Taps Probability", 0.3f,
                "This is the probability a single tap will not fully seat the magazine back into place.");
            
            UserConfig.UseOldSounds = Config.Bind("Old Sounds", "Use Old Sounds", false,
                "This enables the old sounds.");
            
            UserConfig.HandgunProbability = Config.Bind("Specific Weapon Type Probabilities", "Handgun", 0.5f,
                "This is a modifier for how often the mag should be unseated for each type of weapon.");
            UserConfig.OpenBoltProbability = Config.Bind("Specific Weapon Type Probabilities", "Open Bolt", 1f,
                "This is a modifier for how often the mag should be unseated for each type of weapon.");
            UserConfig.ClosedBoltProbability = Config.Bind("Specific Weapon Type Probabilities", "Closed Bolt", 1f,
                "This is a modifier for how often the mag should be unseated for each type of weapon.");
            UserConfig.TubeFedShotgunProbability = Config.Bind("Specific Weapon Type Probabilities", "Tube Fed Shotguns", 1f,
                "This is a modifier for how often the mag should be unseated for each type of weapon.");
        }
        
        public static void StartMagNoiseTimer(FVRPooledAudioSource source, float time)
        {
            _currentMagSoundSource = source;
            _timeLeft = time;
            _hasStoppedSound = false;
            _timeUntilFadeOut = time / 2f;
            _defaultMagSoundVolume = source.Source.volume;
        }
    }
}