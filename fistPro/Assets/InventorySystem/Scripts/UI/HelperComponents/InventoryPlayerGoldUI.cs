using UnityEngine;
using System.Collections;
using Devdog.InventorySystem;

namespace Devdog.InventorySystem
{
    [AddComponentMenu("InventorySystem/UI Helpers/Inventory gold")]
    [RequireComponent(typeof(UIWindow))]
    public class InventoryPlayerGoldUI : MonoBehaviour
    {

        [Tooltip("The UI element that displays the amount of gold the player has in his inventory")]
        public UnityEngine.UI.Text playerGoldText;

        protected UIWindow window;

        // Use this for initialization
        void Awake()
        {
            window = GetComponent<UIWindow>();
            
            InventoryManager.instance.inventory.OnGoldChanged += (float added) =>
            {
                if (window.isVisible)
                    Repaint();
            };

            window.OnShow += () =>
            {
                Repaint();
            };
        }

        protected virtual void Repaint()
        {
            if (playerGoldText != null)
                playerGoldText.text = InventorySettingsManager.instance.currencyFormatter.Format(InventoryManager.instance.inventory.gold);
        }
    }
}