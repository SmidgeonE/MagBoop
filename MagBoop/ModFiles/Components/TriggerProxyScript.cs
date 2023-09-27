using System;
using FistVR;
using HarmonyLib;
using Stovepipe;
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
        private const float VolumeVariance = 2f;
        private const float PitchVariance = 0.5f;
        private static float _currentInvLerpOfSpeed;

        public bool isUnSeated;
        public bool hasStartedMagNoiseTimer;

        public const float SoundCooldown = 0.2f;
        public float soundCooldownTimer;
        public bool hasAlreadyTappedOnce;

        protected override void Start()
        {
            var magTransform = transform.parent;
            
            _thisMagScript = magTransform.GetComponent<FVRFireArmMagazine>();
            _thisController = magTransform.GetComponent<AudioImpactController>();
            
            base.Start();
        }

        public void StartCooldownTimer()
        {
            soundCooldownTimer = SoundCooldown;
        }

        public override void BeginInteraction(FVRViveHand hand)
        {
            ForceBreakInteraction();
        }

        public override bool IsInteractable()
        {
            return false;
        }

        private void Update()
        {
            if (soundCooldownTimer > 0) soundCooldownTimer -= Time.fixedDeltaTime;
        }
        
        public void ReSeatMagazine()
        {
            if (!isUnSeated) return;
            if (_thisMagScript == null) return;

            var magSeatedPos = _thisMagScript.FireArm.GetMagMountPos(_thisMagScript.IsBeltBox).position;

            if (UnityEngine.Random.Range(0f, 1f) < UserConfig.MagRequiresTwoTapsProbability.Value && !hasAlreadyTappedOnce)
            {
                _thisMagScript.transform.position = Vector3.Lerp(_thisMagScript.transform.position, magSeatedPos, 0.5f);
                hasAlreadyTappedOnce = true;
                return;
            }
            
            _thisMagScript.transform.position = magSeatedPos;
            isUnSeated = false;
            
            var doubleFeedData = _thisMagScript.FireArm.GetComponent<DoubleFeedData>();
            if (doubleFeedData == null) return;
            
            /*Debug.Log("reduing duble feed multiplier");*/
            doubleFeedData.doubleFeedChance /= UserConfig.DoubleFeedMultiplier.Value;
            doubleFeedData.doubleFeedMaxChance /= UserConfig.DoubleFeedMultiplier.Value;
        }
        
        public void CheckAndPlayBoopSound(FVRViveHand hand)
        {
            if (soundCooldownTimer > 0) return;
            if (hand.Input.GripPressed) return;
            
            var handRb = hand.GetComponent<Rigidbody>();
            if (_thisMagScript.FireArm == null) return;
            if (_thisMagScript.FireArm.QuickbeltSlot != null
                && !_thisMagScript.FireArm.IsHeld 
                && GM.Options.MovementOptions.CurrentMovementMode == FVRMovementManager.MovementMode.Armswinger) return;
            if (hand.CurrentInteractable != null) return;


            var weaponRb = _thisMagScript.FireArm.RootRigidbody;
            var upwardsSpeed = Vector3.Dot(handRb.velocity - weaponRb.velocity, _thisMagScript.transform.up);

            if (upwardsSpeed < 0) return;
            
            if (UserConfig.EnableMagUnSeating.Value) ReSeatMagazine();
            
            var impactIntensity = AudioImpactIntensity.Medium;
            if (upwardsSpeed > 0.015f)
                impactIntensity = AudioImpactIntensity.Hard;

            var magMat = transform.parent.GetComponent<PMat>();
            var impactMat = MatSoundType.SoftSurface;
            
            // Try loads of things to find the correct material
            if (magMat == null) magMat = handRb.GetComponent<PMat>();
            if (magMat != null && magMat.MatDef != null) impactMat = magMat.MatDef.SoundType;

            
            // Gather how loud the sound should be based on the movement of the hand.
            // If this boop is the one to make the mag fully seated, it will be lower pitch so the user knows
            
            _currentInvLerpOfSpeed = Mathf.InverseLerp(MinSpeed, MaxSpeed, upwardsSpeed);
            var movementBasedVolume = 3f + _currentInvLerpOfSpeed * VolumeVariance;
            var randomPitch = 1 + UnityEngine.Random.Range(0f, PitchVariance);
            if (!isUnSeated) randomPitch *= 0.75f;

            SM.PlayImpactSound(_thisController.ImpactType, impactMat, impactIntensity, transform.parent.position,
                _thisController.PoolToUse, _thisController.DistLimit, movementBasedVolume, randomPitch);
            

            StartCooldownTimer();
        }
    }
}