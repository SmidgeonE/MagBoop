using System;
using System.Collections.Generic;
using System.Linq;
using FistVR;
using HarmonyLib;
using RenderHeads.Media.AVProVideo;
using Stovepipe;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MagBoop.ModFiles
{
    public class MagazineUnseatingPatches
    {
        private static FVRFireArm _currentUnSeatedWeapon;

        [HarmonyPatch(typeof(FVRFireArmMagazine), "Load", typeof(FVRFireArm))]
        [HarmonyPrefix]
        private static void UnSeatMagDiceRoll(FVRFireArmMagazine __instance, FVRFireArm fireArm)
        {
            if (!__instance.IsExtractable) return;
            if (__instance.IsEnBloc) return;

            var magBoopComp = __instance.GetComponent<MagazineBoopComponent>();
            if (magBoopComp is null) return;
            if (magBoopComp.thisTrigger.isUnSeated) return;

            
            // Modulating the probability based on the speed of insertion
            var hand = __instance.m_hand;
            if (hand is null) return;
            
            var magSpeedRelativeToWeapon = (hand.GetComponent<Rigidbody>().velocity
                                            - fireArm.RootRigidbody.velocity).magnitude;
            
            const float slowSpeed = 0.02f;
            const float quickSpeed = 0.2f;

            var speedLerp = Mathf.InverseLerp(slowSpeed, quickSpeed, Mathf.Abs(magSpeedRelativeToWeapon));
            var lerpedProbability = Mathf.Lerp(UserConfig.SlowSpeedUnseatingProbability.Value,
                UserConfig.MagUnseatedProbability.Value, speedLerp);
            
            if (Random.Range(0f, 1f) < lerpedProbability)
            {
                magBoopComp.thisTrigger.isUnSeated = true;
                magBoopComp.thisTrigger.hasStartedMagNoiseTimer = false;
                _currentUnSeatedWeapon = fireArm;
                magBoopComp.thisTrigger.hasAlreadyTappedOnce = false;
            }
        }
        
        [HarmonyPatch(typeof(FVRFireArmMagazine), "Load", typeof(FVRFireArm))]
        [HarmonyPostfix]
        private static void UnSeatMag(FVRFireArmMagazine __instance)
        {
            var magBoopComp = __instance.GetComponent<MagazineBoopComponent>();
            if (magBoopComp is null) return;
            if (!magBoopComp.thisTrigger.isUnSeated) return;

            // Unseat Mag

            var magTransform = __instance.transform;

            magBoopComp.thisTrigger.insertsAboveWeapon =
                Vector3.Dot(__instance.transform.up, __instance.FireArm.transform.up) < -0.2f;
            
            // Check if it is physically above the weapon..

            if (magBoopComp.thisTrigger.insertsAboveWeapon == false)
                magBoopComp.thisTrigger.insertsAboveWeapon = __instance.transform.localPosition.y > 0.004f;

            // Check if it actually just inserts to the side...
            
            if (Mathf.Abs(Vector3.Dot(__instance.transform.up, __instance.FireArm.transform.right)) > 0.2f)
                magBoopComp.thisTrigger.insertsAboveWeapon = false;
            
            if (magBoopComp.thisTrigger.insertsAboveWeapon)
                magTransform.position += magTransform.up * 0.015f;
            else
                magTransform.position -= magTransform.up * 0.015f;

            var doubleFeedData = __instance.FireArm.GetComponent<DoubleFeedData>();
            if (doubleFeedData != null)
            {
                /*Debug.Log("increaseing double feed mutli;liers");
                    Debug.Log("from " + doubleFeedData.doubleFeedChance);
                    Debug.Log("to " + doubleFeedData.doubleFeedChance * UserConfig.DoubleFeedMultiplier.Value);*/
                doubleFeedData.doubleFeedChance *= UserConfig.DoubleFeedMultiplier.Value;
                doubleFeedData.doubleFeedMaxChance *= UserConfig.DoubleFeedMultiplier.Value;
            }
        }

        [HarmonyPatch(typeof(FVRFireArmMagazine), "Release")]
        [HarmonyPatch(typeof(FVRFireArmMagazine), "ReleaseFromAttachableFireArm")]
        [HarmonyPatch(typeof(FVRFireArmMagazine), "ReleaseFromSecondarySlot")]
        [HarmonyPrefix]
        private static void ReduceDoubleFeedMultsIfReleasedMag(FVRFireArmMagazine __instance)
        {
            var magBoopComp = __instance.GetComponent<MagazineBoopComponent>();
            if (magBoopComp is null) return;

            var doubleFeedData = __instance.FireArm.GetComponent<DoubleFeedData>();
            if (doubleFeedData is null) return;

            doubleFeedData.doubleFeedChance /= UserConfig.DoubleFeedMultiplier.Value;
            doubleFeedData.doubleFeedMaxChance /= UserConfig.DoubleFeedMultiplier.Value;
        }

        [HarmonyPatch(typeof(FVRPooledAudioSource), "Play")]
        [HarmonyPostfix]
        private static void MagInsertionNoiseAlteration(FVRPooledAudioSource __instance, Vector3 pos, 
            AudioEvent audioEvent)
        {
            if (_currentUnSeatedWeapon == null) return;
            if (pos != _currentUnSeatedWeapon.transform.position) return;
            if (_currentUnSeatedWeapon.Magazine == null) return;
            
            var magBoopComp = _currentUnSeatedWeapon.Magazine.GetComponent<MagazineBoopComponent>();

            // This is the first boop noise, the very short one
            
            if (!magBoopComp.thisTrigger.hasStartedMagNoiseTimer && magBoopComp.thisTrigger.isUnSeated)
            {
                // Making is quieter
                __instance.Source.Stop();
                __instance.Source.volume *= 0.8f;
                __instance.Source.Play();

                // Making it stop sooner
                MagBoopManager.StartMagNoiseTimer(__instance, 0.12f);
                magBoopComp.thisTrigger.hasStartedMagNoiseTimer = true;
            }
        }

        [HarmonyPatch(typeof(AudioImpactController), "ProcessCollision")]
        [HarmonyPostfix]
        private static void EnvironmentMagBoopPatch(AudioImpactController __instance, Collision col
            , bool ___m_hasPlayedAudioThisFrame)
        {
            if (col.relativeVelocity.magnitude < __instance.HitThreshold_Ignore) return;
            if (__instance.transform.parent == null) return;
            
            var weapon = __instance.GetComponent<FVRFireArm>();
            if (weapon == null) return;

            var mag = weapon.Magazine;
            if (mag == null) return;

            var boopComp = mag.GetComponent<MagazineBoopComponent>();
            if (boopComp == null) return;

            var magColliders = mag.GetComponentsInChildren<Collider>().ToArray();

            foreach (var contactPoint in col.contacts)
            {
                var contactCols =
                    magColliders.Where(x => x == contactPoint.otherCollider || x == contactPoint.thisCollider).ToArray();

                if (!contactCols.Any()) return;

                if (Vector3.Dot(col.relativeVelocity, contactCols[0].transform.position) <= 0f) return;
                    
                boopComp.thisTrigger.ReSeatMagazine();
                return;
            }
        }
    }
}