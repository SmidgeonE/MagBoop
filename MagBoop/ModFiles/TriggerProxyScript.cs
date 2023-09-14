using System;
using FistVR;
using UnityEngine;

namespace MagBoop.ModFiles
{
    public class TriggerProxyScript : MonoBehaviour
    {
        private FVRFireArmMagazine thisMagScript;
        private AudioImpactController thisController;

        private void Start()
        {
            thisMagScript = gameObject.GetComponent<FVRFireArmMagazine>();
            thisController = gameObject.GetComponent<AudioImpactController>();
        }
        
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("BEEEEP");
            
            if (HandPatch.impactControllers.TryGetValue(other.gameObject, out var controller))
            {
                Debug.Log("collision with hand detected!");
                if (thisMagScript.FireArm != null) PlayBoopSound(other.gameObject);
            }
        }

        private void PlayBoopSound(GameObject hand)
        {
            var handRb = hand.GetComponent<Rigidbody>();
            var weaponRb = thisMagScript.FireArm.RootRigidbody;

            var upwardsSpeed = Vector3.Dot(handRb.velocity - weaponRb.velocity,
                weaponRb.transform.position - transform.position);
            
            Debug.Log("upwards speed was" + upwardsSpeed);
            
            
            if (upwardsSpeed < thisController.HitThreshold_Ignore) return;
            
            var impactIntensity = AudioImpactIntensity.Light;
            if (upwardsSpeed > thisController.HitThreshold_High)
                impactIntensity = AudioImpactIntensity.Hard;
            else if (upwardsSpeed > thisController.HitThreshold_Medium)
                impactIntensity = AudioImpactIntensity.Medium;

            var handMat = hand.GetComponent<PMat>();
            var impactMat = MatSoundType.Meat;
            if (handMat != null && handMat.MatDef != null) impactMat = handMat.MatDef.SoundType;

            SM.PlayImpactSound(thisController.ImpactType, impactMat, impactIntensity, transform.position,
                thisController.PoolToUse, thisController.DistLimit);
        }
    }
}