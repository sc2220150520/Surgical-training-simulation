using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Devdog.InventorySystem
{
    [AddComponentMenu("InventorySystem/Windows/Inventory bags")]
    public partial class InventoryBagsUI : ItemCollectionBase
    {
        [SerializeField]
        private uint _initialCollectionSize = 4;
        public override uint initialCollectionSize { get { return _initialCollectionSize; } }

        public override void Awake()
        {
            base.Awake();
        }

        public override bool MoveItem(InventoryItemBase item, uint fromSlot, ItemCollectionBase toCollection, uint toSlot, bool clearOld, bool doRepaint = true)
        {
            if (item == null)
                return true;

            // No moving inside own collection
            if (this == toCollection)
                return false;

            var bag = (BagInventoryItem)item;
            if (toCollection[toSlot].item == null)
            {
                bool set = toCollection.SetItem(toSlot, item);
                if (set && InventoryManager.instance.inventory.CanRemoveSlots(bag.extendInventoryBySlots) == false)
                {
                    toCollection.SetItem(toSlot, null); // And reset
                    return false;
                }
            }
        
            bool moved = base.MoveItem(item, fromSlot, toCollection, toSlot, clearOld, doRepaint);
            if (moved == false)
                return false;

            if (toCollection != this)
            {
                bool unequip = bag.Unequip();
                if (unequip)
                {
                    SetItem(fromSlot, null);
                }
            }

            return true;
        }

        public override bool OverrideUseMethod(InventoryItemBase item)
        {
            var bag = item as BagInventoryItem;
            if (item.itemCollection == this)
            {
                // Used from inside
                if (bag != null)
                {
                    bool unequip = bag.Unequip();
                    if (unequip)
                    {
                        InventoryManager.AddItemAndRemove(bag);
                    }
                }
            }

            return true;
        }

        public override bool SetItem(uint slot, InventoryItemBase item)
        {
            // First check if the item wasn't empty.
            if (items[slot].item != null && item != null)
            {
                return false;
            }

            // Then set the item
            bool set = base.SetItem(slot, item);
            if (set == false)
                return false;

            if (item == null)
                return true;

            // Then handle the equip / inventory extending.
            var bag = (BagInventoryItem)item;
            bool equipped = bag.Equip();
            if (equipped)
            {
                //InventoryManager.instance.inventory.scrollBar
            }

            return equipped;
        }
    

        public override bool SwapOrMerge(uint slot1, ItemCollectionBase handler2, uint slot2, bool repaint = true)
        {
            return SwapSlots(slot1, handler2, slot2, repaint);
        }

        protected override bool SwapSlots(uint fromSlot, ItemCollectionBase toCollection, uint toSlot, bool repaint = true, bool fireEvents = true)
        {
            if (items[fromSlot].item != null && toCollection[toSlot].item != null)
                return false;

            return base.SwapSlots(fromSlot, toCollection, toSlot, repaint, fireEvents);
        }
    }
}