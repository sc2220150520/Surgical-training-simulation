﻿#if BEHAVIOR_DESIGNER

using UnityEngine;
using Devdog.InventorySystem;

namespace BehaviorDesigner.Runtime.Tasks.InventorySystem
{
    [TaskCategory("InventorySystem")]
    [TaskDescription("Set items for a vendor, replaces the old items that were in the collection before it.")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=127")]
    [TaskIcon("Assets/Behavior Designer/Third Party/Inventory Pro/Editor/InventoryProIcon.png")]
    public class SetItemsVendor : Action
    {
        public InventoryItemBase[] items;
        public VendorTriggerer vendor;

        public override TaskStatus OnUpdate()
        {
            if (vendor == null) {
                return TaskStatus.Failure;
            }
            vendor.forSale = items;
            return TaskStatus.Success;
        }

        public override void OnReset()
        {
            items = null;
            vendor = null;
        }
    }
}

#endif