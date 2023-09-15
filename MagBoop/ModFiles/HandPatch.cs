using System;
using System.Collections.Generic;
using System.Linq;
using FistVR;
using HarmonyLib;
using UnityEngine;

namespace MagBoop.ModFiles
{
    public class HandPatch
    {
        public static Dictionary<GameObject, AudioImpactController> impactControllers =
            new Dictionary<GameObject, AudioImpactController>();
        
        [HarmonyPatch(typeof(FVRViveHand), "Start")]
        [HarmonyPostfix]
        private static void GenerateBoopAudioController(FVRViveHand __instance)
        {
            var impactController = __instance.gameObject.AddComponent<AudioImpactController>();
            impactController.ImpactType = ImpactType.MagSmallPlastic;
            impactController.Alts = new List<AudioImpactController.AltImpactType>();
            impactController.CausesSonicEventOnSoundPlay = true;
            impactController.IgnoreRBs = new List<Rigidbody>();
            impactController.SetIFF(0);
            impactControllers.Add(__instance.gameObject, impactController);

            foreach (var comp in __instance.GetComponents<Component>()) Debug.Log(comp.GetType());

            /*
            var x = __instance.GetComponents<SphereCollider>();
            foreach (var col in x) col.isTrigger = false;
            */
            
        }

        [HarmonyPatch(typeof(FVRViveHand), "TestCollider")]
        [HarmonyPostfix]
        private static void CheckCol(FVRViveHand __instance, Collider collider, bool isEnter)
        {
            if (!isEnter) return;
            
            var trigScript = collider.GetComponent<TriggerProxyScript>();
            if (trigScript == null) return;
            
            Debug.Log("hand touchy");

            trigScript.PlayBoopSound(__instance.gameObject);
        }

        [HarmonyPatch(typeof(FVRFireArmMagazine), "Awake")]
        [HarmonyPostfix]
        private static void AddImpactProxy(FVRFireArmMagazine __instance, Collider[] ___m_colliders)
        {
            if (__instance.IsIntegrated) return;
            if (__instance.IsEnBloc) return;

            var interactionObj = new GameObject("MagBoopObj")
            {
                transform =
                {
                    parent = __instance.transform,
                    localRotation = Quaternion.Euler(Vector3.zero)
                },
                layer = LayerMask.NameToLayer("Interactable")
            };


            var collider = interactionObj.AddComponent<BoxCollider>();
            var magCollider = __instance.GetComponent<BoxCollider>();

            if (magCollider == null) return;
            
            var magSize = magCollider.size;

            collider.center = Vector3.zero;
            collider.size = new Vector3(magSize.x, 0.02f, magSize.z);
            collider.isTrigger = true;

            // Debug cube
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.parent = interactionObj.transform;
            cube.transform.localScale = collider.size;
            cube.transform.localPosition = Vector3.zero;
            cube.transform.localRotation = Quaternion.Euler(Vector3.zero);
            cube.GetComponent<Collider>().enabled = false;
            interactionObj.AddComponent<TriggerProxyScript>();
            
            // Now using the lowest mesh collider, we can set the local pos + rotation of the trigger object
            var bottomTransform = FindBottomTransform(___m_colliders);

            interactionObj.transform.localRotation = bottomTransform.localRotation;
            interactionObj.transform.localPosition = magCollider.center
                                                     + Vector3.down * ((magSize.y / 2) + 0.02f)
                                                     - Vector3.forward * (float) Math.Tan(interactionObj.transform.localRotation.x) * ((magSize.y / 2) + 0.02f);
        }

        
        private static Transform FindBottomTransform(IList<Collider> colliders)
        {
            // We iterate through all collider, whichever one is lowest will be the highest in the z for some reason 
            // Thats how magazines are structured

            var currentBottomCollider = colliders[0];
            
            foreach (var collider in colliders)
            {
                if (collider.transform.localPosition.z > currentBottomCollider.transform.localPosition.z)
                    currentBottomCollider = collider;
            }
            
            return currentBottomCollider.transform;
        }
    }
}