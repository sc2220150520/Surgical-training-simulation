using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

namespace Devdog.InventorySystem.Dialogs
{
    public enum ItemBuySellDialogAction
    {
        Selling,
        Buying,
        BuyingBack
    }


    public partial class ItemBuySellDialog : ItemIntValDialog
    {
        public UnityEngine.UI.Text price;
        protected ItemBuySellDialogAction action;
        protected VendorTriggerer vendor;

        [Header("Audio & Visuals")]
        public Color affordableColor = Color.white;
        public Color unAffordableColor = Color.red;
    

        public override void ShowDialog(string title, string description, string yes, string no, int minValue, int maxValue, IntValDialogCallback yesCallback, IntValDialogCallback noCallback)
        {
            base.ShowDialog(title, description, yes, no, minValue, maxValue, yesCallback, noCallback);

            inputField.onValueChange.AddListener((string result) =>
            {
                uint amount = uint.Parse(result); // Let's trust Unity on this...

                float finalPrice = 0.0f;
                if (action == ItemBuySellDialogAction.Buying)
                    finalPrice = vendor.GetBuyPrice(inventoryItem, amount);
                else if (action == ItemBuySellDialogAction.Selling)
                    finalPrice = vendor.GetSellPrice(inventoryItem, amount);
                else if (action == ItemBuySellDialogAction.BuyingBack)
                    finalPrice = vendor.GetBuyBackPrice(inventoryItem, amount);

                price.text = InventorySettingsManager.instance.currencyFormatter.Format(finalPrice);
                if(action == ItemBuySellDialogAction.Buying || action == ItemBuySellDialogAction.BuyingBack)
                {
                    if (finalPrice > InventoryManager.instance.inventory.gold)
                        price.color = unAffordableColor;
                    else
                        price.color = affordableColor;
                }
                else
                    price.color = affordableColor;                   

            });
            inputField.onValueChange.Invoke(inputField.text); // Hit the event on start
        }

    
        public override void ShowDialog(string title, string description, string yes, string no, int minValue, int maxValue, InventoryItemBase item, IntValDialogCallback yesCallback, IntValDialogCallback noCallback)
        {
            inventoryItem = item;
            ShowDialog(string.Format(string.Format(title, item.name, item.description)), string.Format(description, item.name, item.description), yes, no, minValue, maxValue, yesCallback, noCallback);
        }

        public virtual void ShowDialog(string title, string description, string yes, string no, int minValue, int maxValue, InventoryItemBase item, ItemBuySellDialogAction action, VendorTriggerer vendor, IntValDialogCallback yesCallback, IntValDialogCallback noCallback)
        {
            // Don't call base class going directly to this.ShowDialog()
            inventoryItem = item;
            this.action = action;
            this.vendor = vendor;
            ShowDialog(string.Format(string.Format(title, item.name, item.description)), string.Format(description, item.name, item.description), yes, no, minValue, maxValue, yesCallback, noCallback);
        }
    }
}
