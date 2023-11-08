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
            // List of various exclusions for mag unseating...
            
            if (!__instance.IsExtractable) return;
            if (__instance.IsEnBloc) return;
            if (UserConfig.DisableForBeltFeds.Value && fireArm.UsesBeltBoxes) return;
            if (fireArm is ClosedBoltWeapon cb && cb.Bolt.UsesAKSafetyLock) return;
            if (MagBoopManager.ExcludedWeaponNames.Contains(fireArm.name.Remove(fireArm.name.Length - 7))) return;
            
            var magBoopComp = __instance.GetComponent<MagazineBoopComponent>();
            if (magBoopComp is null) return;
            if (magBoopComp.thisMagTrigger.isUnSeated) return;

            var ignoreHandVelocityEffect = false;

            // Grabbing user defined probability modifiers...
            var weaponTypeProbabilityModifier = 1f;

            switch (fireArm)
            {
                case Handgun _:
                    weaponTypeProbabilityModifier = UserConfig.HandgunProbability.Value;
                    break;
                
                case OpenBoltReceiver _:
                    weaponTypeProbabilityModifier = UserConfig.OpenBoltProbability.Value;
                    break;
                
                case ClosedBoltWeapon cbw:
                    if (!cbw.Handle.IsSlappable)
                    {
                        weaponTypeProbabilityModifier = UserConfig.ClosedBoltProbability.Value;
                        break;
                    }

                    if (cbw.Bolt.CurPos == ClosedBolt.BoltPos.Forward)
                        weaponTypeProbabilityModifier = UserConfig.HKProbBoltClosed.Value;
                    else
                        weaponTypeProbabilityModifier = UserConfig.HKProbBoltOpen.Value;

                    ignoreHandVelocityEffect = true;

                    break;
                
                case TubeFedShotgun _:
                    weaponTypeProbabilityModifier = UserConfig.TubeFedShotgunProbability.Value;
                    break;
            }
            
            
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

            if (ignoreHandVelocityEffect) lerpedProbability = 1f;
            
            if (Random.Range(0f, 1f) < lerpedProbability * weaponTypeProbabilityModifier)
            {
                magBoopComp.thisMagTrigger.isUnSeated = true;
                magBoopComp.thisMagTrigger.hasStartedMagNoiseTimer = false;
                _currentUnSeatedWeapon = fireArm;
                magBoopComp.thisMagTrigger.hasAlreadyTappedOnce = false;
            }
        }
        
        [HarmonyPatch(typeof(FVRFireArmMagazine), "Load", typeof(FVRFireArm))]
        [HarmonyPostfix]
        private static void UnSeatMag(FVRFireArmMagazine __instance)
        {
            var magBoopComp = __instance.GetComponent<MagazineBoopComponent>();
            if (magBoopComp is null) return;
            if (!magBoopComp.thisMagTrigger.isUnSeated) return;

            // Unseat Mag

            var magTransform = __instance.transform;

            magBoopComp.thisMagTrigger.insertsAboveWeapon =
                Vector3.Dot(__instance.transform.up, __instance.FireArm.transform.up) < -0.2f;
            
            // Check if it is physically above the weapon..

            if (magBoopComp.thisMagTrigger.insertsAboveWeapon == false)
                magBoopComp.thisMagTrigger.insertsAboveWeapon = __instance.transform.localPosition.y > 0.004f;

            // Check if it actually just inserts to the side...
            
            if (Mathf.Abs(Vector3.Dot(__instance.transform.up, __instance.FireArm.transform.right)) > 0.2f)
                magBoopComp.thisMagTrigger.insertsAboveWeapon = false;
            
            if (magBoopComp.thisMagTrigger.insertsAboveWeapon)
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
            
            if (!magBoopComp.thisMagTrigger.hasStartedMagNoiseTimer && magBoopComp.thisMagTrigger.isUnSeated)
            {
                // Making is quieter
                __instance.Source.Stop();
                __instance.Source.volume *= 0.8f;
                __instance.Source.Play();

                // Making it stop sooner
                MagBoopManager.StartMagNoiseTimer(__instance, 0.12f);
                magBoopComp.thisMagTrigger.hasStartedMagNoiseTimer = true;
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
                    
                // Re seat mag and play appropriate noise
                
                boopComp.thisMagTrigger.ReSeatMagazine();
                
                if (UserConfig.UseOldSounds.Value || boopComp.thisMagTrigger.isUnSeated)
                    SM.PlayImpactSound(boopComp.thisMagTrigger.thisController.ImpactType, MatSoundType.SoftSurface,
                        AudioImpactIntensity.Hard, boopComp.thisMagTrigger.thisMagScript.FireArm.transform.position,
                        boopComp.thisMagTrigger.thisController.PoolToUse, boopComp.thisMagTrigger.thisController.DistLimit, 
                        1f, 1f);
                else
                    boopComp.thisMagTrigger.PlayEndOfMagInsertionNoise(1f + 
                                                                    Random.Range(0f, MagTriggerScript.PitchVariance));

                return;
            }
        }
    }
}