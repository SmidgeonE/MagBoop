using System;
using FistVR;
using HarmonyLib;
using Stovepipe;
using UnityEngine;
using Random = System.Random;

namespace MagBoop.ModFiles
{
    public class MagTriggerScript : FVRInteractiveObject
    {
        public FVRFireArmMagazine thisMagScript;
        public AudioImpactController thisController;
        public bool insertsAboveWeapon;

        private const float MinSpeed = 0.001f;
        private const float MaxSpeed = 0.05f;
        private const float VolumeVariance = 2f;
        public const float PitchVariance = 0.4f;
        private static float _currentInvLerpOfSpeed;

        public bool isUnSeated;
        public bool hasStartedMagNoiseTimer;

        public const float SoundCooldown = 0.2f;
        public float soundCooldownTimer;
        
        private float _timeTillResetAudioTimer;
        private FVRPooledAudioSource _pooledAudioSource;
        private bool _hasFoundPooledAudioSource;
        private bool _hasResetAudioTimer;
        
        public bool hasAlreadyTappedOnce;

        protected override void Start()
        {
            var magTransform = transform.parent;
            
            thisMagScript = magTransform.GetComponent<FVRFireArmMagazine>();
            thisController = magTransform.GetComponent<AudioImpactController>();
            
            base.Start();
        }

        public void StartCooldownTimer()
        {
            soundCooldownTimer = SoundCooldown;
        }

        public override bool IsInteractable()
        {
            return false;
        }

        private void Update()
        {
            if (soundCooldownTimer > 0) soundCooldownTimer -= Time.fixedDeltaTime;
            
            if (_timeTillResetAudioTimer > 0f) _timeTillResetAudioTimer -= Time.fixedDeltaTime;
            else if (!_hasResetAudioTimer && _hasFoundPooledAudioSource) ResetAudioAndAudioTimer();
        }

        private void ResetAudioAndAudioTimer()
        {
            if (_pooledAudioSource == null) return;
            
            _pooledAudioSource.Source.Stop();
            _pooledAudioSource.Source.time = 0f;
            _pooledAudioSource.Source.pitch = 1f;
            _hasResetAudioTimer = true;
        }

        public void ReSeatMagazine()
        {
            if (!isUnSeated) return;
            if (thisMagScript == null) return;
            
            var magSeatedPos = thisMagScript.FireArm.GetMagMountPos(thisMagScript.IsBeltBox).position;
            
            if (UnityEngine.Random.Range(0f, 1f) < UserConfig.MagRequiresTwoTapsProbability.Value && !hasAlreadyTappedOnce)
            {
                // This is the half-way mag boop
                
                thisMagScript.transform.position = Vector3.Lerp(thisMagScript.transform.position, magSeatedPos, 0.5f);
                hasAlreadyTappedOnce = true;
                return;
            }

            thisMagScript.transform.position = magSeatedPos;
            isUnSeated = false;
            
            // If double feed data can be found, we reduce the double feed chance again

        }
        
        public void CheckAndPlayBoopSound(FVRViveHand hand)
        {
            if (soundCooldownTimer > 0) return;
            if (hand.Input.GripPressed) return;
            
            var handRb = hand.GetComponent<Rigidbody>();
            if (thisMagScript.FireArm == null) return;
            if (thisMagScript.FireArm.QuickbeltSlot != null
                && !thisMagScript.FireArm.IsHeld 
                && GM.Options.MovementOptions.CurrentMovementMode == FVRMovementManager.MovementMode.Armswinger) return;
            if (hand.CurrentInteractable != null) return;


            var weaponRb = thisMagScript.FireArm.RootRigidbody;
            var upwardsSpeed = Vector3.Dot(handRb.velocity - weaponRb.velocity, thisMagScript.FireArm.transform.up);

            if (insertsAboveWeapon) upwardsSpeed *= -1;
            
            if (upwardsSpeed < 0.002f) return;
            if (upwardsSpeed < 0) return;
            
            if (UserConfig.EnableMagUnSeating.Value) ReSeatMagazine();
            
            hand.Buzz(hand.Buzzer.Buzz_BeginInteraction);
            
            var impactIntensity = AudioImpactIntensity.Medium;
            if (upwardsSpeed > 0.015f)
                impactIntensity = AudioImpactIntensity.Hard;
            
            // If the majority of the speed is in the wrong direction, the boop will also fail

            var totalVelocityMagnitude = (handRb.velocity - weaponRb.velocity).magnitude;

            if (upwardsSpeed < 0.5 * totalVelocityMagnitude) return;

            // Gather how loud the sound should be based on the movement of the hand.
            // If this boop is the one to make the mag fully seated, it will be lower pitch so the user knows
            
            _currentInvLerpOfSpeed = Mathf.InverseLerp(MinSpeed, MaxSpeed, upwardsSpeed);
            var movementBasedVolume = 3f + _currentInvLerpOfSpeed * VolumeVariance;
            var randomPitch = 1 + UnityEngine.Random.Range(0f, PitchVariance);
            
            // Do the third law physics thing
            if (insertsAboveWeapon && UserConfig.UseThirdLaw.Value)
                ImpartTorqueOnWeapon(handRb, totalVelocityMagnitude, -thisMagScript.FireArm.transform.up);
            else if (UserConfig.UseThirdLaw.Value)
                ImpartTorqueOnWeapon(handRb, totalVelocityMagnitude, thisMagScript.FireArm.transform.up);

            if (UserConfig.UseOldSounds.Value)
                SM.PlayImpactSound(thisController.ImpactType, MatSoundType.SoftSurface, impactIntensity, transform.parent.position,
                    thisController.PoolToUse, thisController.DistLimit, movementBasedVolume, randomPitch);
            else
                PlayEndOfMagInsertionNoise(randomPitch);

            StartCooldownTimer();
        }

        private void ImpartTorqueOnWeapon(Rigidbody handRb, float handVelocityMag, Vector3 boopDir)
        {
            var weaponRb = thisMagScript.FireArm.RootRigidbody;
            
            // torque = distance x force
            var force = 10f * UserConfig.ThirdLawPower.Value * handVelocityMag * boopDir;
            var torque = Vector3.Cross(weaponRb.transform.position - handRb.transform.position, force);

            if (!weaponRb)
                return;

            weaponRb.AddTorque(torque, ForceMode.Force);
        }

        public void PlayEndOfMagInsertionNoise(float randomPitch)
        {
            if (thisMagScript.ProfileOverride == null)
                _pooledAudioSource = thisMagScript.FireArm.PlayAudioAsHandling(thisMagScript.Profile.MagazineIn,
                    thisMagScript.FireArm.transform.position);
            else
                _pooledAudioSource = thisMagScript.FireArm.PlayAudioAsHandling(thisMagScript.ProfileOverride.MagazineIn,
                    thisMagScript.FireArm.transform.position);

            if (_pooledAudioSource is null) return;

            _hasFoundPooledAudioSource = true;
            _pooledAudioSource.Source.Stop();
            _pooledAudioSource.Source.volume = 1f;
            _pooledAudioSource.Source.time = 0.1f * _pooledAudioSource.Source.clip.length;
            _pooledAudioSource.Source.pitch = randomPitch;
            _pooledAudioSource.Source.Play();

            _timeTillResetAudioTimer = _pooledAudioSource.Source.clip.length * 0.9f;
            
            _hasResetAudioTimer = false;
        }
    }
}