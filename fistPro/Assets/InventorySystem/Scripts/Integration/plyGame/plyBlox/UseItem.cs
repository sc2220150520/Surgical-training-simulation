﻿#if PLY_GAME

using System;
using System.Collections.Generic;
using System.Linq;
using plyBloxKit;
using UnityEngine;

namespace Devdog.InventorySystem.Integration.plyGame.plyBlox
{
    [plyBlock("InventorySystem", "Items", "Use item", BlockType.Action, ReturnValueType = typeof(UseItem))]
    public class UseItem : Bool_Value
    {
        [plyBlockField("Item", ShowName = true, ShowValue = true, DefaultObject = typeof(InventoryItemBase), EmptyValueName = "-error-", SubName = "InventorySystem item", Description = "Item to use, make sure it's a scene object, and not a prefab.")]
        public InventoryItemBase item;

        public override void Created()
        {
            blockIsValid = item != null;
        }

        public override BlockReturn Run(BlockReturn param)
        {
            item.Use();
            return BlockReturn.OK;
        }
    }
}

#endif