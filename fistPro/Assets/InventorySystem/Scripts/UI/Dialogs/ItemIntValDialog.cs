using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Devdog.InventorySystem.Dialogs
{
    public partial class ItemIntValDialog : IntValDialog
    {
        public UnityEngine.UI.Image itemIcon;
        protected InventoryItemBase inventoryItem { get; set; }

        public override void ShowDialog(string title, string description, string yes, string no, int minValue, int maxValue, IntValDialogCallback yesCallback, IntValDialogCallback noCallback)
        {
            base.ShowDialog(title, description, yes, no, minValue, maxValue, yesCallback, noCallback);

            if(itemIcon != null && inventoryItem != null)
            {
                if (inventoryItem != null)
                {
                    itemIcon.sprite = inventoryItem.icon;
                }
                else
                    itemIcon.sprite = InventorySettingsManager.instance.defaultSlotIcon;
            }
        }


        public override void ShowDialog(string title, string description, string yes, string no, int minValue, int maxValue, InventoryItemBase item, IntValDialogCallback yesCallback, IntValDialogCallback noCallback)
        {
            // Don't call base class going directly to this.ShowDialog()
            inventoryItem = item;
            ShowDialog(string.Format(string.Format(title, item.name, item.description)), string.Format(description, item.name, item.description), yes, no, minValue, maxValue, yesCallback, noCallback);
        }
    }
}