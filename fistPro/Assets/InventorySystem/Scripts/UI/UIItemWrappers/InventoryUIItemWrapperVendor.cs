using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Devdog.InventorySystem
{
    [AddComponentMenu("InventorySystem/UI Wrappers/UI Wrapper Vendor")]
    public partial class InventoryUIItemWrapperVendor : InventoryUIItemWrapper
    {
        public UnityEngine.UI.Text buyPrice;

        public Color affordableColor = Color.red;
        public Color notAffordableColor = Color.white;

        [HideInInspector]
        public VendorUI vendor;

        [HideInInspector]
        public bool isInBuyBack = false;

        public static bool hideWhenEmpty = true;


        public override void Awake()
        {
            base.Awake();
            Repaint();
        }

        public override void Update()
        {
            //base.Update();
        }


        #region Button handler UI events

   
        public override void OnPointerUp(PointerEventData eventData)
        {
            if (pointerDownOnUIElement == false)
                return;

            if (InventorySettingsManager.instance.useContextMenu)
            {
                base.OnPointerDown(eventData);
                return;
            }

            if (item != null)
                vendor.currentVendor.BuyItemFromVendor(item, isInBuyBack);
        }

        public override void TriggerContextMenu()
        {
            //base.TriggerContextMenu();

            var contextMenu = InventorySettingsManager.instance.contextMenu;
            contextMenu.ClearMenuOptions();
            contextMenu.AddMenuOption("Buy", item, (i) =>
            {
                vendor.currentVendor.BuyItemFromVendor(i, isInBuyBack);
            });

            contextMenu.window.Show();
        }



        #endregion


        public override void Repaint()
        {
            base.Repaint();
        
            if (item != null)
            {
                if (hideWhenEmpty)
                    gameObject.SetActive(true);

                //itemName.text = item.name;
                itemName.color = item.rarity.color;

                float finalPrice = item.buyPrice;
                if (vendor.currentVendor != null)
                {
                    if (isInBuyBack)
                        finalPrice = vendor.currentVendor.GetBuyBackPrice(item, 1);
                    else
                        finalPrice = vendor.currentVendor.GetBuyPrice(item, 1);
                }

                buyPrice.text = InventorySettingsManager.instance.currencyFormatter.Format(finalPrice);
                if (finalPrice > InventoryManager.instance.inventory.gold)
                    buyPrice.color = notAffordableColor;
                else
                    buyPrice.color = affordableColor;
            }
            else
            {
                //itemName.text = string.Empty;
                buyPrice.text = string.Empty;
            
                if (hideWhenEmpty)
                    gameObject.SetActive(false);
            }
        }

        public override void RepaintCooldown()
        {
            //base.RepaintCooldown();
        }
    }
}