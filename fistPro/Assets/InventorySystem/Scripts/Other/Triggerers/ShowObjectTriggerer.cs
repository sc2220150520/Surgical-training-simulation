using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Devdog.InventorySystem
{
    /// <summary>
    /// Handles the raycast based triggering of an object, as well as trigger the repaint.
    /// </summary>
    [AddComponentMenu("InventorySystem/Triggers/Show object triggerer")]
    public class ShowObjectTriggerer : MonoBehaviour
    {
        [Header("UI")]
        public ObjectTriggererUI uiElement;

        public Sprite pickupSprite;
        public Sprite useSprite;

        ///// <summary>
        ///// When the item is clicked, should it trigger?
        ///// </summary>
        [Header("Behavior")]
        public bool itemTriggerMouseClick = true;

        ///// <summary>
        ///// When the item is hovered over (center of screen) and a certain key is tapped, should it trigger?
        ///// </summary>
        public KeyCode itemTriggerHoverKeyCode = KeyCode.F;

        /// <summary>
        /// Trigger the item when the player collides with it.
        /// </summary>
        public bool itemTriggerOnPlayerCollision = false;

        public static ShowObjectTriggerer instance { get; protected set; }

        public void Awake()
        {
            instance = this;
        }


        public virtual void Update()
        {
            // Raycast from center of screen
            Debug.DrawRay(Camera.main.transform.position, (Camera.main.transform.forward * InventorySettingsManager.instance.useObjectDistance), Color.red, 0.1f);
            //Profiler.BeginSample("ShowObjectPickerSAMPLE_TEST");

            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, InventorySettingsManager.instance.useObjectDistance))
            {
                if (itemTriggerHoverKeyCode != KeyCode.None)
                {
                    var item = hit.transform.GetComponent<ObjectTriggererItem>();
                    if (item != null)
                    {
                        uiElement.Repaint(pickupSprite, itemTriggerHoverKeyCode.ToString());
                        if (Input.GetKeyDown(itemTriggerHoverKeyCode))
                        {
                            // Pickup
                            item.item.PickupItem();
                        }
                        return;
                    }                    
                }

                var objectTrigger = hit.transform.GetComponent<ObjectTriggerer>();
                if (objectTrigger != null)
                {
                    if (objectTrigger.triggerHoverKeyCode != KeyCode.None)
                    {
                        uiElement.Repaint(useSprite, objectTrigger.triggerHoverKeyCode.ToString());
                        if (Input.GetKeyDown(objectTrigger.triggerHoverKeyCode))
                        {
                            // Pickup
                            if (objectTrigger.toggleWhenTriggered)
                                objectTrigger.Toggle();
                            else
                                objectTrigger.Use();
                        }
                        return;                        
                    }
                }

                // Didn't hit a object that is triggerable
                Hide();
            }
            else
            {
                Hide();
            }

            //Profiler.EndSample();
        }

        protected virtual void Hide()
        {
            if (uiElement != null && uiElement.window.isVisible)
            {
                uiElement.window.Hide();
            }
        }

        //var colliders = hit.transform.GetComponentsInChildren<Collider>();
        //foreach (var col in colliders)
        //{
        //    if (col.transform.IsChildOf(transform))
        //    {
        //        if ((window != null && window.isVisible) && toggleWhenTriggered && isOpen)
        //            Hide();
        //        else
        //            Show();

        //        break;
        //    }
        //}
    }   
}
