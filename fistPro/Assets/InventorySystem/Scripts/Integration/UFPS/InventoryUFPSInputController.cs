#if UFPS

using UnityEngine;
using System.Collections;
using Devdog.InventorySystem;
using Devdog.InventorySystem.Models;


namespace Devdog.InventorySystem.Integration.UFPS
{
    [AddComponentMenu("InventorySystem/Integration/UFPS/UFPS Input controller")]
    public class InventoryUFPSInputController : MonoBehaviour
    {
        public vp_FPInput input;

        /// <summary>
        /// Close the windows when you click on the world.
        /// </summary>
        public bool closeWindowsWhenClickWorld;

        private static int windowCounter;
        private static bool registered = true;
        private static UIWindow[] windows;
        private static float lastWindowShownTime = 0.0f;

        // Start, to make sure all Awakes are done.
        public virtual void Start()
        {
            windows = Resources.FindObjectsOfTypeAll<UIWindow>();
            foreach (var w in windows)
            {
                var window = w; // Capture list and all...
                if (window.blockUFPSInput)
                {
                    window.OnShow += () =>
                    {
                        lastWindowShownTime = Time.time;
                        windowCounter++;

                        if (windowCounter > 0 && registered)
                            UnRegisterUFPSEvents();
                    };

                    window.OnHide += () =>
                    {
                        windowCounter--;

                        if (windowCounter == 0 && registered == false)
                            RegisterUFPSEvents();
                    };   
                }
            }
        }

        public void Update()
        {
            // Auto close window when movement is pressed.
            if (vp_Input.GetAxisRaw("Horizontal") != 0.0f || vp_Input.GetAxisRaw("Vertical") != 0.0f)
            {
                if (Time.time > lastWindowShownTime + 0.4f)
                {
                    HideAllWindows();
                }
            }

            if (closeWindowsWhenClickWorld)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (InventoryUIUtility.clickedUIElement == false)
                    {
                        input.MouseCursorBlocksMouseLook = true;
                        HideAllWindows();
                    }
                }
            }
        }

        protected virtual void HideAllWindows()
        {
            foreach (var window in windows)
            {
                if (window.isVisible && window.blockUFPSInput)
                    window.Hide();
            }
        }

        protected virtual void RegisterUFPSEvents()
        {
            if (input.FPPlayer != null)
            {
                input.enabled = true;

                registered = true;
                vp_Utility.LockCursor = true;
            }
        }

        protected virtual void UnRegisterUFPSEvents()
        {
            if (input.FPPlayer != null)
            {
                input.enabled = false;
                
                registered = false;
                vp_Utility.LockCursor = false;
            }
        }
    }
}

#else

using UnityEngine;
using System.Collections;
using Devdog.InventorySystem;
using Devdog.InventorySystem.Models;

namespace Devdog.InventorySystem.Integration.UFPS
{
    public class InventoryUFPSInputController : MonoBehaviour
    {
        // No UFPS, No fun stuff...
    }
}

#endif