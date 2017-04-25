using UnityEngine;
using System.Collections;
using Devdog.InventorySystem.Models;

namespace Devdog.InventorySystem
{
    /// <summary>
    /// This is used to represent gold as an item, once the item is picked up gold will be added to the inventory collection.
    /// </summary>
    public partial class GoldInventoryItem : InventoryItemBase
    {

        [InventoryStat]
        public float amount;


        public override bool PickupItem (bool addToInventory = true)
        {
            if(addToInventory)
                InventoryManager.instance.inventory.gold += amount;

            Destroy (gameObject); // Don't need to store gold objects
            return true;
        }

        public override int Use()
        {
            return -2; // Can't use gold
        }
    }
}