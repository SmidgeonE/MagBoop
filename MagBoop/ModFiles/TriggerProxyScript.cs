using System;
using FistVR;
using HarmonyLib;
using UnityEngine;

namespace MagBoop.ModFiles
{
    public class TriggerProxyScript : FVRInteractiveObject
    {
        private FVRFireArmMagazine thisMagScript;
        private AudioImpactController thisController;

        protected override void Start()
        {
            thisMagScript = transform.parent.GetComponent<FVRFireArmMagazine>();
            thisController = transform.parent.GetComponent<AudioImpactController>();
            base.Start();

        }

        public void PlayBoopSound(GameObject hand)
        {
            var handRb = hand.GetComponent<Rigidbody>();
            Debug.Log("a");
            if (thisMagScript.FireArm == null)
            {
                Debug.Log("ifrearm is null");
                return;
            }
            var weaponRb = thisMagScript.FireArm.RootRigidbody;
            Debug.Log("b");
            var upwardsSpeed = Vector3.Dot(handRb.velocity - weaponRb.velocity,
                weaponRb.transform.position - transform.position);
            Debug.Log("upwards speed was" + upwardsSpeed);
            
            
            var impactIntensity = AudioImpactIntensity.Light;
            if (upwardsSpeed > 0.025f)
                impactIntensity = AudioImpactIntensity.Hard;
            else if (upwardsSpeed > 0.005f)
                impactIntensity = AudioImpactIntensity.Medium;

            var magMat = transform.parent.GetComponent<PMat>();
            var impactMat = MatSoundType.SoftSurface;
            if (magMat == null)
            {
                Debug.Log("hand doesnt have a mat!");
                magMat = handRb.GetComponent<PMat>();
            }
            if (magMat != null && magMat.MatDef == null) Debug.Log("mat def is null!");

            if (magMat != null && magMat.MatDef != null)
            {
                Debug.Log("setting material to specific sound trpe ");
                impactMat = magMat.MatDef.SoundType;
            }
            
            
            SM.PlayImpactSound(thisController.ImpactType, impactMat, impactIntensity, transform.position,
                thisController.PoolToUse, thisController.DistLimit);
        }
    }
}