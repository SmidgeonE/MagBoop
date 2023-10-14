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
            var offsetBack = slideCollider.size.z * 0.5f * slideTransform.localScale.z - 0.01f;
            var offsetUp = slideCollider.size.y * slideTransform.localScale.y;

            interactionObj.transform.position = slideTransform.position + centre
                                                - offsetBack * slideTransform.forward
                                                + offsetUp * slideTransform.up;
            
            if (UserConfig.EnableTriggerDebug.Value)
                MagazineTriggerPatches.GenerateDebugCube(interactionObj, triggerCol);
        }
    }
}