using System;
using System.Collections.Generic;
using FistVR;
using HarmonyLib;
using UnityEngine;
using Object = System.Object;

namespace MagBoop.ModFiles
{
    public class MagazineTriggerPatches
    {
        [HarmonyPatch(typeof(FVRFireArmMagazine), "Load", typeof(FVRFireArm))]
        [HarmonyPostfix]
        private static void StopFromBoopingOnMagLoad(FVRFireArmMagazine __instance)
        {
            var magBoopComp = __instance.GetComponent<MagazineBoopComponent>();
            if (magBoopComp is null) return;
            
            magBoopComp.thisMagTrigger.StartCooldownTimer();
        }
        
        [HarmonyPatch(typeof(FVRFireArmMagazine), "Release")]
        [HarmonyPatch(typeof(FVRFireArmMagazine), "ReleaseFromAttachableFireArm")]
        [HarmonyPatch(typeof(FVRFireArmMagazine), "ReleaseFromSecondarySlot")]
        [HarmonyPrefix]
        private static void StopFromBoopingAfterMagRelease(FVRFireArmMagazine __instance)
        {
            var magBoopComp = __instance.GetComponent<MagazineBoopComponent>();
            if (magBoopComp is null) return;

            magBoopComp.thisMagTrigger.StartCooldownTimer();

            if (!magBoopComp.thisMagTrigger.isUnSeated) return;

            magBoopComp.thisMagTrigger.isUnSeated = false;
        }

        [HarmonyPatch(typeof(FVRFireArm), "Fire")]
        [HarmonyPostfix]
        private static void StopFromBoopingIfJustShot(FVRFireArm __instance)
        {
            if (__instance.Magazine == null) return;
            
            var magBoopComp = __instance.Magazine.GetComponent<MagazineBoopComponent>();
            if (magBoopComp is null) return;
            
            magBoopComp.thisMagTrigger.StartCooldownTimer();
        }

        [HarmonyPatch(typeof(FVRFireArmMagazine), "Awake")]
        [HarmonyPostfix]
        public static void AddOrSwapOrientationOfImpactProxy(FVRFireArmMagazine __instance)
        {
            if (__instance.IsIntegrated) return;
            if (__instance.IsEnBloc) return;
            if (__instance.gameObject.name == "G11_Mag(Clone)") return;
            
            // If The interaction object already exists, we destroy it and recreate is with the highest collider instead

            var priorBoopComp = __instance.GetComponent<MagazineBoopComponent>();
            if (priorBoopComp != null && priorBoopComp.hasAlreadyAdjustedTrigger) return;

            var topOrBottomCol = FindLowestCollider(__instance);

            if (topOrBottomCol == null)
            {
                Debug.Log("No magboop trigger could be identified for " + __instance.name);
                return;
            }

            var alreadyMadeTrigger = __instance.transform.FindChild("MagBoopObj");
            if (alreadyMadeTrigger != null)
            {
                UnityEngine.Object.Destroy(alreadyMadeTrigger.gameObject);
                topOrBottomCol = FindHighestCollider(__instance);
                __instance.GetComponent<MagazineBoopComponent>().hasAlreadyAdjustedTrigger = true;
            }

            var interactionObj = GenerateInteractionObjAndAddMagDataClass(__instance, 
                out var triggerCol, out var magBoxCollider, out var magBoopComp);

            if (magBoxCollider == null)
            {
                // This means it is a capsule collider or a sphere collider, if the box collider returns null
                var magCollider = __instance.GetComponent<Collider>();
                
                if (magCollider is CapsuleCollider) GenerateCapsuleMagazineTrigger(__instance, triggerCol, interactionObj);
                else if (magCollider is SphereCollider) GenerateSphereMagazineTrigger(__instance, triggerCol, interactionObj);

                if (UserConfig.EnableTriggerDebug.Value) GenerateDebugCube(interactionObj, triggerCol);
                return;
            }

            GenerateBoxMagazineTrigger(triggerCol, magBoxCollider.size, interactionObj, magBoopComp, topOrBottomCol);
            if (UserConfig.EnableTriggerDebug.Value) GenerateDebugCube(interactionObj, triggerCol);
        }
        
        [HarmonyPatch(typeof(FVRFireArmMagazine), "Load", typeof(FVRFireArm))]
        [HarmonyPostfix]
        private static void AdjustPositionOfTriggerIfTopLoad(FVRFireArmMagazine __instance)
        {
            var magBoopComp = __instance.GetComponent<MagazineBoopComponent>();
            if (magBoopComp is null) return;
            
            if (magBoopComp.thisMagTrigger.insertsAboveWeapon)
                AddOrSwapOrientationOfImpactProxy(__instance);
        }

        private static void GenerateBoxMagazineTrigger(BoxCollider triggerCol, Vector3 magSize, 
            GameObject interactionObj, MagazineBoopComponent magBoopComp, BoxCollider topOrBottomCol)
        {
            triggerCol.center = Vector3.zero;
            triggerCol.size = new Vector3(magSize.x, 0.045f, magSize.z);
            triggerCol.isTrigger = true;

            if (GM.CurrentPlayerBody.LeftHand != null &&
                GM.CurrentPlayerBody.LeftHand.GetComponent<FVRViveHand>().DMode == DisplayMode.Index)
            {
                triggerCol.size = new Vector3(triggerCol.size.x, 0.17f, triggerCol.size.z);
            }
            
            // Now using the lowest mesh collider, we can set the local pos + rotation of the trigger object
            // Shift forward factor accounts for the fact the bottom of the magazine will be curved, so a simple 
            // translation downwards causes the trigger to be quite far back.

            var shiftForwardFactor = -Vector3.forward * (float)Math.Tan(interactionObj.transform.localRotation.x) *
                                     ((magSize.y / 2) + 0.02f);
            
            interactionObj.transform.localRotation = topOrBottomCol.transform.localRotation;
            
            if (!magBoopComp.hasAlreadyAdjustedTrigger) interactionObj.transform.localPosition = ColliderLocalLowestPos(topOrBottomCol) + shiftForwardFactor;
            else interactionObj.transform.localPosition = ColliderLocalHighestPos(topOrBottomCol) - shiftForwardFactor;
        }

        public static void GenerateDebugCube(GameObject interactionObj, BoxCollider triggerCol)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.parent = interactionObj.transform;
            cube.transform.localScale = triggerCol.size;
            cube.transform.localPosition = Vector3.zero;
            cube.transform.localRotation = Quaternion.Euler(Vector3.zero);
            cube.GetComponent<Collider>().enabled = false;
        }

        private static GameObject GenerateInteractionObjAndAddMagDataClass(FVRFireArmMagazine mag,
            out BoxCollider triggerCol, out BoxCollider magCollider, out MagazineBoopComponent magBoopComp)
        {
            var interactionObj = new GameObject("MagBoopObj")
            {
                transform =
                {
                    parent = mag.transform,
                    localRotation = Quaternion.Euler(Vector3.zero)
                },
                layer = LayerMask.NameToLayer("Interactable")
            };

            triggerCol = interactionObj.AddComponent<BoxCollider>();
            magCollider = mag.GetComponent<BoxCollider>();
            var triggerScript = interactionObj.AddComponent<MagTriggerScript>();
            magBoopComp = mag.gameObject.GetComponent<MagazineBoopComponent>() ?? 
                          mag.gameObject.AddComponent<MagazineBoopComponent>();

            magBoopComp.thisMagTrigger = triggerScript;
            return interactionObj;
        }

        private static void GenerateCapsuleMagazineTrigger(FVRFireArmMagazine mag, BoxCollider triggerCol,
            GameObject interactionObj)
        {
            var capsuleCollider = mag.GetComponent<CapsuleCollider>();

            triggerCol.center = Vector3.zero;
            triggerCol.size = new Vector3(capsuleCollider.radius, 0.02f, capsuleCollider.height / 2f);
            triggerCol.isTrigger = true;

            interactionObj.transform.localPosition = Vector3.zero -
                                                     Vector3.up * capsuleCollider.radius *
                                                     interactionObj.transform.localScale.y -
                                                     Vector3.forward * capsuleCollider.height *
                                                     interactionObj.transform.localScale.y / 2f;
                                                     ;
        }
        
        private static void GenerateSphereMagazineTrigger(FVRFireArmMagazine mag, BoxCollider triggerCol,
            GameObject interactionObj)
        {
            var capsuleCollider = mag.GetComponent<SphereCollider>();

            triggerCol.center = Vector3.zero;
            var radius = capsuleCollider.radius;
            triggerCol.size = new Vector3(radius, 0.02f, radius);
            triggerCol.isTrigger = true;

            interactionObj.transform.localPosition = Vector3.zero -
                                                     Vector3.up * radius *
                                                     interactionObj.transform.localScale.y;
        }

        private static Vector3 ColliderLocalLowestPos(BoxCollider boxCol)
        {
            // Returns the lowest point of a box collider in local coordinates to the magazine object its attached to.
            
            return boxCol.transform.localPosition + (boxCol.center - Vector3.up * boxCol.size.y / 2f) * boxCol.transform.localScale.y;
        }
        
        private static Vector3 ColliderLocalHighestPos(BoxCollider boxCol)
        {
            // Returns the highest point of a box collider in local coordinates to the magazine object its attached to.
            
            return boxCol.transform.localPosition + (boxCol.center + Vector3.up * boxCol.size.y / 2f) * boxCol.transform.localScale.y;
        }

        private static BoxCollider FindLowestCollider(FVRFireArmMagazine mag)
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

            return currentBottomCollider;
        }
        
        private static BoxCollider FindHighestCollider(FVRFireArmMagazine mag)
        {
            BoxCollider currentTopCollider = null;
            var colliders = mag.GetComponentsInChildren<BoxCollider>();

            foreach (var collider in colliders)
            {
                if (currentTopCollider is null)
                {
                    currentTopCollider = collider;
                    continue;
                }
                
                if (ColliderLocalLowestPos(collider).y > ColliderLocalLowestPos(currentTopCollider).y)
                    currentTopCollider = collider;
            }
            
            return currentTopCollider;
        }
    }
}