using System;
using FistVR;
using HarmonyLib;
using UnityEngine;

namespace MagBoop.ModFiles
{
    public class MagazineTriggerPatches
    {
        [HarmonyPatch(typeof(FVRFireArmMagazine), "Load", typeof(FVRFireArm))]
        [HarmonyPostfix]
        private static void StopInteractionOnMagLoad(FVRFireArmMagazine __instance)
        {
            var magBoopComp = __instance.GetComponent<MagazineBoopComponent>();
            if (magBoopComp is null) return;
            
            magBoopComp.thisTrigger.soundCooldownTimer = TriggerProxyScript.SoundCooldown;
        }
        
        [HarmonyPatch(typeof(FVRFireArmMagazine), "Awake")]
        [HarmonyPostfix]
        private static void AddImpactProxy(FVRFireArmMagazine __instance)
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
            var triggerScript = interactionObj.AddComponent<TriggerProxyScript>();
            __instance.gameObject.AddComponent<MagazineBoopComponent>().thisTrigger = triggerScript;


            if (magCollider == null) return;
            
            var magSize = magCollider.size;

            triggerCol.center = Vector3.zero;
            triggerCol.size = new Vector3(magSize.x, 0.02f, magSize.z);
            triggerCol.isTrigger = true;

            // Debug cube
            /*
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.parent = interactionObj.transform;
            cube.transform.localScale = triggerCol.size;
            cube.transform.localPosition = Vector3.zero;
            cube.transform.localRotation = Quaternion.Euler(Vector3.zero);
            cube.GetComponent<Collider>().enabled = false;
            */

            
            // Now using the lowest mesh collider, we can set the local pos + rotation of the trigger object
            // Shift forward factor accounts for the fact the bottom of the magazine will be curved, so a simple 
            // translation downwards causes the trigger to be quite far back.

            var shiftForwardFactor = -Vector3.forward * (float)Math.Tan(interactionObj.transform.localRotation.x) *
                                     ((magSize.y / 2) + 0.02f);
            interactionObj.transform.localRotation = bottomCollider.transform.localRotation;
            interactionObj.transform.localPosition = ColliderLocalLowestPos(bottomCollider) + shiftForwardFactor;
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
                
                if (ColliderLocalLowestPos(collider).y < ColliderLocalLowestPos(currentBottomCollider).y)
                    currentBottomCollider = collider;
            }

            return currentBottomCollider ? currentBottomCollider : new BoxCollider();
        }
    }
}