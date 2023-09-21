using BepInEx.Configuration;

namespace MagBoop.ModFiles
{
    public static class UserConfig
    {
        public static ConfigEntry<bool> EnableMagTapping;
        public static ConfigEntry<bool> EnableMagUnSeating;
        
        public static ConfigEntry<float> MagUnseatedProbability;
        public static ConfigEntry<float> DoubleFeedMultiplier;
    }
}