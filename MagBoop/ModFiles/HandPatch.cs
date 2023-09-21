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
            
            var bottomCollider = FindMainCollider(__instance);

            var interactionObj = new GameObject("MagBoopObj")
            {
                transform =
                {
                    parent = __instance.transform,
                    localRotation = Quaternion.Euler(Vector3.zero)
                },
                layer = LayerMask.NameToLayer("Interactable")
            };


            var triggerCol = interactionObj.AddComponent<BoxCollider>();
            var magCollider = __instance.GetComponent<BoxCollider>();

            if (magCollider == null) return;
            
            var magSize = magCollider.size;

            triggerCol.center = Vector3.zero;
            triggerCol.size = new Vector3(magSize.x, 0.02f, magSize.z);
            triggerCol.isTrigger = true;

            // Debug cube
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.parent = interactionObj.transform;
            cube.transform.localScale = triggerCol.size;
            cube.transform.localPosition = Vector3.zero;
            cube.transform.localRotation = Quaternion.Euler(Vector3.zero);
            cube.GetComponent<Collider>().enabled = false;
            interactionObj.AddComponent<TriggerProxyScript>();
            
            // Now using the lowest mesh collider, we can set the local pos + rotation of the trigger object
            var bottomColTransform = bottomCollider.transform;
            
            interactionObj.transform.localRotation = bottomColTransform.localRotation;
            interactionObj.transform.localPosition = ColliderLocalLowestPos(bottomCollider);
        }

        private static Vector3 ColliderLocalLowestPos(BoxCollider boxCol)
        {
            // Returns the lowest point of a box collider in local coordinates to the magazine object its attached to.
            
            return boxCol.transform.localPosition + (boxCol.center - Vector3.up * boxCol.size.y / 2f) * boxCol.transform.localScale.y;
        }

        private static BoxCollider FindMainCollider(FVRFireArmMagazine mag)
        {
            BoxCollider currentBottomCollider = null;
            var colliders = mag.GetComponentsInChildren<BoxCollider>();

            foreach (var collider in colliders)
            {
                if (collider.transform == mag.transform) continue;
                if (currentBottomCollider is null)
                {
                    currentBottomCollider = collider;
                    continue;
                }
                
                Debug.Log("current obj name: " + collider.transform.name);

                if (ColliderLocalLowestPos(collider).y < ColliderLocalLowestPos(currentBottomCollider).y)
                {
                    currentBottomCollider = collider;
                    Debug.Log("this collider is lower");
                }
            }

            if (currentBottomCollider is null)
            {
                Debug.Log("Couldnt find lowest collider on a magazine");
                currentBottomCollider = new BoxCollider();
            }

            return currentBottomCollider;
        }
    }
}