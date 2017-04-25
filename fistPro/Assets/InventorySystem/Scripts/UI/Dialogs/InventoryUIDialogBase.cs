using UnityEngine;
using System.Collections;
using Devdog.InventorySystem.Dialogs;
using UnityEngine.UI;

namespace Devdog.InventorySystem.Dialogs
{

    public delegate void InventoryUIDialogCallback(InventoryUIDialogBase dialog);

    /// <summary>
    /// The abstract base class used to create all dialogs. If you want to create your own dialog, extend from this class.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(UIWindow))]
    public abstract partial class InventoryUIDialogBase : MonoBehaviour
    {
        [Header("UI")]
        public Text titleText;
        public Text descriptionText;

        public UnityEngine.UI.Button yesButton;
        public UnityEngine.UI.Button noButton;

        /// <summary>
        /// The item that should be selected by default when the dialog opens.
        /// </summary>
        [Header("Behavior")]
        public Selectable selectOnOpenDialog;

        /// <summary>
        /// Disables the items defined in InventorySettingsManager.disabledWhileDialogActive if set to true.
        /// </summary>
        public bool disableElementsWhileActive = true;
    
        protected CanvasGroup canvasGroup { get; set; }
        protected Animator animator { get; set; }
        public UIWindow window { get; protected set; }

        public virtual void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            animator = GetComponent<Animator>();
            window = GetComponent<UIWindow>();


            window.OnShow += () =>
            {
                SetEnabledWhileActive(false); // Disable other UI elements

                if (selectOnOpenDialog != null)
                    selectOnOpenDialog.Select();
            };
            window.OnHide += () =>
            {
                SetEnabledWhileActive(true); // Enable other UI elements
            };
        }

        public void Toggle()
        {
            window.Toggle();
            if(window.isVisible)
                SetEnabledWhileActive(false); // Disable other UI elements
            else
                SetEnabledWhileActive(true); // Enable other UI elements
        }

        /// <summary>
        /// Disables elements of the UI when a dialog is active. Useful to block user actions while presented with a dialog.
        /// </summary>
        /// <param name="enabled">Should the items be disabled?</param>
        protected virtual void SetEnabledWhileActive(bool enabled)
        {
            if (disableElementsWhileActive == false)
                return;

            foreach (var item in InventorySettingsManager.instance.disabledWhileDialogActive)
            {
                var group = item.gameObject.GetComponent<CanvasGroup>();
                if (group == null)
                    group = item.gameObject.AddComponent<CanvasGroup>();

                group.blocksRaycasts = enabled;
                group.interactable = enabled;
            }
        }
    }
}