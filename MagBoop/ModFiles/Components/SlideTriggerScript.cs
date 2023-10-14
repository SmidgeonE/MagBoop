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
            Debug.Log("Checking slide boop");

            if (!(Vector3.Dot(
                    hand.GetComponent<Rigidbody>().velocity - slideScript.Handgun.RootRigidbody.velocity,
                    transform.forward) > 0.002f)) return;

            Debug.Log("satisfactorially booped");

            var stoveData = transform.parent.GetComponent<StovepipeData>();

            if (stoveData != null)
            {
                if (stoveData.isWeaponBatteryFailing)
                    slideScript.Handgun.PlayAudioEvent(FirearmAudioEventType.BoltSlideForward);
                else
                    slideScript.Handgun.PlayAudioEvent(FirearmAudioEventType.BoltSlideForwardHeld);
                
                stoveData.isWeaponBatteryFailing = false;
            }
            else
                slideScript.Handgun.PlayAudioEvent(FirearmAudioEventType.BoltSlideForwardHeld);
        }
    }
}