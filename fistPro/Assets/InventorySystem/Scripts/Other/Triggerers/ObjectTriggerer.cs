using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Devdog.InventorySystem.Dialogs;
using UnityEditor;

namespace Devdog.InventorySystem
{
    /// <summary>
    /// Used to trigger a physical object such as vendor, treasure chests etc.
    /// </summary>
    [AddComponentMenu("InventorySystem/Triggers/Object triggerer")]
    public partial class ObjectTriggerer : MonoBehaviour
    {
        #region Events

        public delegate void TriggerUse();
        public delegate void TriggerUnUse();

        public event TriggerUse OnTriggerUse;
        public event TriggerUnUse OnTriggerUnUse;

        #endregion

        /// <summary>
        /// When the item is clicked, should it trigger?
        /// </summary>
        [Header("Triggers")]
        public bool triggerMouseClick = true;

        /// <summary>
        /// When the item is hovered over (center of screen) and a certain key is tapped, should it trigger?
        /// </summary>
        public KeyCode triggerHoverKeyCode = KeyCode.None;

        /// <summary>
        /// When true the window will be triggered directly, if false, a 2nd party will have to handle it through events.
        /// </summary>
        [HideInInspector]
        [NonSerialized]
        public bool handleWindowDirectly = true;

        /// <summary>
        /// Toggle when triggered
        /// </summary>
        public bool toggleWhenTriggered = true;

        /// <summary>
        /// Only required if handling the window directly
        /// </summary>
        [Header("The window")]
        public UIWindow window;

        [Header("Animations & Audio")]
        public AnimationClip useAnimation;
        public AnimationClip unUseAnimation;

        public AudioClip useAudioClip;
        public AudioClip unUseAudioClip;


        public Animator animator { get; protected set; }

        public bool isActive { get; protected set; }


        public bool inRange
        {
            get
            {
                return Vector3.Distance(InventorySettingsManager.instance.playerObject.transform.position, transform.position) < InventorySettingsManager.instance.useObjectDistance;
            }
        }

        public void Awake()
        {
            animator = GetComponent<Animator>();

            if (window != null)
            {
                window.OnHide += () =>
                {
                    UnUse();
                };                
            }

            ObjectTriggererChecker.objectTriggerers.Add(this);
            ObjectTriggererChecker.Init(this);
        }

        public virtual void OnMouseDown()
        {
            if (triggerMouseClick && InventoryUIUtility.clickedUIElement == false && inRange)
            {
                if (toggleWhenTriggered)
                    Toggle();
                else
                    Use();
            }
        }

        public virtual void Toggle()
        {
            if (window != null && window.isVisible && isActive)
            {
                UnUse();
            }
            else
            {
                Use();
            }
        }

        public virtual void Use(bool fireEvents = true)
        {
            if (isActive)
                return;

            if (handleWindowDirectly && fireEvents)
            {
                if (toggleWhenTriggered)
                    window.Toggle();
                else if (window.isVisible == false)
                    window.Show();
            }

            if (useAnimation != null)
                animator.Play(useAnimation.name);

            if (useAudioClip != null)
                InventoryUIUtility.AudioPlayOneShot(useAudioClip);

            isActive = true;

            if (OnTriggerUse != null && fireEvents)
                OnTriggerUse();
        }

        public virtual void UnUse(bool fireEvents = true)
        {
            if (isActive == false)
                return;
            
            if (handleWindowDirectly && fireEvents)
            {
                window.Hide();
            }

            if (unUseAnimation != null && animator != null)
                animator.Play(unUseAnimation.name);

            if (unUseAudioClip != null)
                InventoryUIUtility.AudioPlayOneShot(unUseAudioClip);

            isActive = false;
            
            if (OnTriggerUnUse != null && fireEvents)
                OnTriggerUnUse();
        }
        
    }
}