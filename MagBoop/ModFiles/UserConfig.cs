﻿using BepInEx;
using BepInEx.Configuration;
using FistVR;
using HarmonyLib;
using UnityEngine;

namespace MagBoop.ModFiles
{
    public static class UserConfig
    {
        public static ConfigEntry<bool> EnableMagTapping;
        public static ConfigEntry<bool> EnableMagUnSeating;
        public static ConfigEntry<bool> EnableCustomSounds;
        public static ConfigEntry<bool> EnableTriggerDebug;
        public static ConfigEntry<bool> EnableSlideTapping;

        public static ConfigEntry<float> MagUnseatedProbability;
        public static ConfigEntry<float> SlowSpeedUnseatingProbability;

        public static ConfigEntry<float> DoubleFeedMultiplier;
        public static ConfigEntry<float> MagRequiresTwoTapsProbability;

        public static ConfigEntry<bool> UseOldSounds;

        public static ConfigEntry<float> HandgunProbability;
        public static ConfigEntry<float> ClosedBoltProbability;
        public static ConfigEntry<float> OpenBoltProbability;
        public static ConfigEntry<float> TubeFedShotgunProbability;

        public static ConfigEntry<bool> DisableForBeltFeds;

        public static ConfigEntry<float> HKProbBoltClosed;
        public static ConfigEntry<float> GenericProbBoltClosed;

        public static ConfigEntry<bool> UseThirdLaw;
        public static ConfigEntry<float> ThirdLawPower;
        public static ConfigEntry<float> ThirdLawRotationSpeedMultiplier;
    }
}