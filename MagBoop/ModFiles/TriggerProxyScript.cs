using System;
using FistVR;
using HarmonyLib;
using UnityEngine;
using Random = System.Random;

namespace MagBoop.ModFiles
{
    public class TriggerProxyScript : FVRInteractiveObject
    {
        private FVRFireArmMagazine _thisMagScript;
        private AudioImpactController _thisController;

        private const float MinSpeed = 0.001f;
        private const float MaxSpeed = 0.05f;
        private const float VolumeVariance = 1.5f;
        private const float PitchVariance = 0.2f;
        private static float _currentInvLerpOfSpeed;

        protected override void Start()
        {
            var magTransform = transform.parent;
            
            _thisMagScript = magTransform.GetComponent<FVRFireArmMagazine>();
            _thisController = magTransform.GetComponent<AudioImpactController>();
            base.Start();
        }

        public void PlayBoopSound(GameObject hand)
        {
            var handRb = hand.GetComponent<Rigidbody>();
            if (_thisMagScript.FireArm == null) return;
            
            var weaponRb = _thisMagScript.FireArm.RootRigidbody;
            var upwardsSpeed = Vector3.Dot(handRb.velocity - weaponRb.velocity,
                weaponRb.transform.position - transform.position);
            
            
            var impactIntensity = AudioImpactIntensity.Medium;
            if (upwardsSpeed > 0.015f)
                impactIntensity = AudioImpactIntensity.Hard;

            var magMat = transform.parent.GetComponent<PMat>();
            var impactMat = MatSoundType.SoftSurface;
            
            // Try loads of things to find the correct material
            if (magMat == null) magMat = handRb.GetComponent<PMat>();
            if (magMat != null && magMat.MatDef != null) impactMat = magMat.MatDef.SoundType;

            
            // Gather how loud the sound should be based on the movement of the hand.
            
            _currentInvLerpOfSpeed = Mathf.InverseLerp(MinSpeed, MaxSpeed, upwardsSpeed);
            var movementBasedVolume = 2.5f + _currentInvLerpOfSpeed * VolumeVariance;
            var randomPitch = 1 + UnityEngine.Random.Range(0f, PitchVariance);
            
            SM.PlayImpactSound(_thisController.ImpactType, impactMat, impactIntensity, transform.parent.position,
                _thisController.PoolToUse, _thisController.DistLimit, movementBasedVolume, randomPitch);
        }
    }
}