using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace Devdog.InventorySystem
{
    /// <summary>
    /// A page inside an UIWindow. When a tab is clicked the insides of the window are changed, this is a page.
    /// </summary>
    [AddComponentMenu("InventorySystem/UI Helpers/UIWindowPage")]
    public partial class UIWindowPage : UIWindow
    {

        [Header("Page specific")]
        public bool isDefaultPage = true;

        [SerializeField]
        protected bool _isEnabled = true;
        public bool isEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                _isEnabled = value;

                if(isEnabled)
                {
                    myButton.enabled = false;

                }
                else
                {
                    Hide();
                    myButton.enabled = false;
                }
            }
        }

        /// <summary>
        /// The button that "triggers" this page. leave empty if there is no button.
        /// </summary>
        public UnityEngine.UI.Button myButton;

        /// <summary>
        /// Container that olds the items, if any.
        /// </summary>
        public RectTransform itemContainer;
        public UIWindow windowParent { get; set; }

        public override void Awake()
        {
            base.Awake();

            windowParent = transform.parent.GetComponentInParent<UIWindow>();
            if (windowParent == null)
                Debug.LogWarning("No UIWindow found in parents", gameObject);

            // Register our page with the window parent
            windowParent.AddPage(this);
        }

        public override void Show()
        {
            if(isEnabled == false)
            {
                Debug.LogWarning("Trying to show a disabled UIWindowPage");
                return;
            }

            base.Show();

            windowParent.NotifyPageShown(this);
        }

        public override void HideFirst()
        {
            isVisible = false;
            SetChildrenActive(false);
        }
    }
}