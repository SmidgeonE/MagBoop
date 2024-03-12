using FistVR;
using Stovepipe;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace MagBoop.ModFiles
{
    public class SlideTriggerScript : MonoBehaviour
    {
        private HandgunSlide slideScript;

        private void Start()
        {
            slideScript = transform.parent.GetComponent<HandgunSlide>();
        }
        
        public void CheckSlideBoop(FVRViveHand hand)
        {
            if (slideScript.Handgun.QuickbeltSlot != null
                && !slideScript.Handgun.IsHeld 
                && GM.Options.MovementOptions.CurrentMovementMode == FVRMovementManager.MovementMode.Armswinger) return;
            if (hand.CurrentInteractable != null) return;
            if (hand.Input.GripPressed) return;

            var handRb = hand.GetComponent<Rigidbody>();
            var relativeVelocity = handRb.velocity - slideScript.Handgun.RootRigidbody.velocity;
            
            if (!(Vector3.Dot(relativeVelocity, transform.forward) > 0.003f)) return;
            if (Vector3.Dot(relativeVelocity, transform.forward) < 0.5f * relativeVelocity.magnitude) return;

            hand.Buzz(hand.Buzzer.Buzz_BeginInteraction);
            var stoveData = transform.parent.GetComponent<StovepipeData>();
            var pitchShift = 1 + UnityEngine.Random.Range(0f, 0.4f);
            
            // If we have the 3rd law thing enabled, we boop
            if (UserConfig.UseThirdLaw.Value)
                ImpartTorqueOnWeapon(handRb, relativeVelocity.magnitude, transform.forward);


            if (stoveData != null)
            {
                if (stoveData.isWeaponBatteryFailing)
                    slideScript.Handgun.PlayAudioEvent(FirearmAudioEventType.BoltSlideForward, pitchShift);
                else
                    slideScript.Handgun.PlayAudioEvent(FirearmAudioEventType.BoltSlideForwardHeld, pitchShift);
                
                stoveData.isWeaponBatteryFailing = false;
            }
            else
                slideScript.Handgun.PlayAudioEvent(FirearmAudioEventType.BoltSlideForwardHeld, pitchShift);
        }
        
        private void ImpartTorqueOnWeapon(Rigidbody handRb, float handVelocityMag, Vector3 boopDir)
        {
            var weaponRb = slideScript.Handgun.RootRigidbody;
            
            if (!weaponRb) return;

            // torque = distance x force
            var force = UserConfig.ThirdLawPower.Value * 30f * handVelocityMag * boopDir;
            var torque = Vector3.Cross(weaponRb.transform.position - handRb.transform.position, force);
            
            weaponRb.AddTorque(torque, ForceMode.Force);
            slideScript.Handgun.RotIntensity *= UserConfig.ThirdLawRotationSpeedMultiplier.Value;
            
        }
    }
}