﻿#if PLY_GAME

using System;
using System.Collections.Generic;
using System.Linq;
using plyBloxKit;
using UnityEngine;

namespace Devdog.InventorySystem.Integration.plyGame.plyBlox
{
    [plyBlock("InventorySystem", "Collections", "Pickup item", BlockType.Action, ReturnValueType = typeof(Bool_Value), ReturnValueString = "Return - Boolean")]
    public class PickupItem : Bool_Value
    {
        [plyBlockField("Item", ShowName = true, ShowValue = true, DefaultObject = typeof(InventoryItemBase), EmptyValueName = "-error-", SubName = "InventorySystem item", Description = "Item to pickup, make sure it's a scene object, and not a prefab.")]
        public InventoryItemBase item;

        public override void Created()
        {
            blockIsValid = item != null;
        }

        public override BlockReturn Run(BlockReturn param)
        {
            item.PickupItem();
            return BlockReturn.OK;
        }
    }
}

#endif