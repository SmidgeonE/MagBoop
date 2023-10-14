﻿using FistVR;
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
            if (!(Vector3.Dot(
                    hand.GetComponent<Rigidbody>().velocity - slideScript.Handgun.RootRigidbody.velocity,
                    transform.forward) > 0.002f)) return;

            hand.Buzz(hand.Buzzer.Buzz_BeginInteraction);
            var stoveData = transform.parent.GetComponent<StovepipeData>();
            var pitchShift = 1 + UnityEngine.Random.Range(0f, 0.4f);

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
    }
}