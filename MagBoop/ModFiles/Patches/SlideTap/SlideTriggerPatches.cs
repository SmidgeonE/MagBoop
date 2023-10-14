using FistVR;
using HarmonyLib;
using UnityEngine;

namespace MagBoop.ModFiles
{
    public class SlideTriggerPatches
    {
        [HarmonyPatch(typeof(HandgunSlide), "Awake")]
        [HarmonyPostfix]
        private static void AddSlideBoopTrigger(HandgunSlide __instance)
        {
            var slideCollider = __instance.GetComponent<BoxCollider>();

            if (slideCollider is null) return;
            var slideTransform = __instance.transform;
            
            var interactionObj = new GameObject("SlideBoopObj")
            {
                transform =
                {
                    parent = slideTransform,
                    localRotation = Quaternion.Euler(Vector3.zero),
                    localPosition = Vector3.zero
                },
                layer = LayerMask.NameToLayer("Interactable")
            };
            
            var triggerCol = interactionObj.AddComponent<BoxCollider>();
            interactionObj.AddComponent<SlideTriggerScript>();
            
            // setting trigger collider size
            
            triggerCol.center = Vector3.zero;
            triggerCol.size = new Vector3(slideCollider.size.x * slideTransform.localScale.x * 1.2f,
                slideCollider.size.y * slideTransform.localScale.y * 1.5f, 
                0.04f);
            
            // Setting pos relative to slide of the trigger object

            var centre = Vector3.Scale(slideCollider.center, slideTransform.localScale);
            var offsetBack = slideCollider.size.z * 0.5f * slideTransform.localScale.z - 0.02f;

            interactionObj.transform.position = slideTransform.position + centre
                                                - offsetBack * slideTransform.forward;

            
            // For some reason the y axis always starts wrong, this just sets it to zero

            interactionObj.transform.localPosition = new Vector3(
                interactionObj.transform.localPosition.x,
                0, 
                interactionObj.transform.localPosition.z);

            if (UserConfig.EnableTriggerDebug.Value)
                MagazineTriggerPatches.GenerateDebugCube(interactionObj, triggerCol);
        }
    }
}