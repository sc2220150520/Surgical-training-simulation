using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Devdog.InventorySystem.Models;

namespace Devdog.InventorySystem
{
    /// <summary>
    /// Used to represent items that can be equipped by the user, this includes armor, weapons, etc.
    /// </summary>
    public partial class EquippableInventoryItem : InventoryItemBase
    {
        public delegate void Equipped(InventoryEquippableField equippedTo);
        public delegate void UnEquipped();

        public event Equipped OnEquipped;
        public event UnEquipped OnUnEquipped;



        /// <summary>
        /// Equip type ID's, Manage in InventoryManager/Item Editor/Equip types
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private int _equipType;
        public InventoryEquipType equipType
        {
            get
            {
                return ItemManager.instance.equipTypes[_equipType];
            }
        }

        //[InventoryStat]
        //public int strength;
    
        //[InventoryStat]
        //public int agility;
    
        //[InventoryStat]
        //public int defense;

        public AudioClip playOnEquip;



        /// <summary>
        /// Called by the collection once the item is successfully equipped.
        /// </summary>
        /// <param name="equipSlot"></param>
        public void NotifyItemEquipped(InventoryEquippableField equipSlot)
        {
            if (OnEquipped != null)
                OnEquipped(equipSlot);
        }


        /// <summary>
        /// Called by the collection once the item is successfully un-equipped
        /// </summary>
        public void NotifyItemUnEquipped()
        {
            if (OnUnEquipped != null)
                OnUnEquipped();
        }



        public override int Use()
        {
            int used = base.Use();
            if (used < 0)
                return used;

            // Currently in a equip to collection?
            if (InventoryManager.IsEquipToCollection(itemCollection))
            {
                bool unequipped = Unequip();
                if (unequipped)
                    return 1; // Used from inside the character, move back to inventory.

                return 0; // Couldn't un-unequip
            }

            var equipSlot = FindBestEquipSlot();
            if (equipSlot == null)
                return -2; // No slot found

            bool equipped = Equip(equipSlot, InventoryManager.instance.character);
            if (equipped)
                return 1;

            return -2;
        }

        public virtual bool Equip(InventoryEquippableField equipSlot, ItemCollectionBase equipToCollection)
        {
            bool handled = HandleLocks(equipSlot, itemCollection);
            if (handled == false)
                return false; // Other items cannot be unequipped

            // There was already an item in this slot, un-equip that one first
            if (equipToCollection[equipSlot.index].item != null)
            {
                var item = equipToCollection[equipSlot.index].item as EquippableInventoryItem;
                bool addedItem = InventoryManager.AddItemAndRemove(item);
                if (addedItem == false)
                    return false; // Can't un-equip item to equip item..

                item.NotifyItemUnEquipped();
            }

            // The values before the collection / slot changed.
            uint prevIndex = index;
            var fromCollection = itemCollection;

            // Equip the item -> Will swap as merge is not possible
            bool swapped = itemCollection.SwapOrMerge(index, equipToCollection, equipSlot.index);
            if (swapped)
            {
                NotifyItemUsed(1, false); // NOTE: Collection changed, collection - event no longer valid!
                fromCollection.NotifyItemUsed(ID, prevIndex, 1);

                if (fromCollection[prevIndex].item != null)
                    ((EquippableInventoryItem)fromCollection[prevIndex].item).NotifyItemUnEquipped();

                return true;
            }

            return false;
        }

        public virtual bool Unequip()
        {
            uint prevIndex = index;
            var fromCollection = itemCollection;

            bool added = InventoryManager.AddItemAndRemove(this);
            if (added == false)
                return false;

            //if (fromCollection != null)
            //    fromCollection.NotifyItemUnequipped(this, prevIndex);
            //else
            //    InventoryManager.instance.character.NotifyItemUnequipped(this, prevIndex);

            NotifyItemUsed(1, false); // NOTE: Collection changed, collection - event no longer valid!
            fromCollection.NotifyItemUsed(ID, prevIndex, 1);

            NotifyItemUnEquipped();

            return true;
        }

        protected virtual InventoryEquippableField FindBestEquipSlot()
        {
            if (InventoryManager.instance.character == null)
                return null;

            var equipSlots = InventoryManager.instance.character.GetEquippableSlots(this);
            if (equipSlots.Length == 0)
            {
                Debug.LogWarning("No suitable equip slot found for item " + name, gameObject);
                return null;
            }

            InventoryEquippableField equipSlot = equipSlots[0];
            foreach (var e in equipSlots)
            {
                if (InventoryManager.instance.character[e.index].item == null)
                {
                    equipSlot = e; // Prefer an empty slot over swapping a filled one.
                }
            }

            return equipSlot;
        }

        /// <summary>
        /// Some item's require multiple slots, for example a 2 handed item forces the left handed item to be empty.
        /// </summary>
        /// <returns>true if items were removed, false if items were not removed.</returns>
        public virtual bool HandleLocks(InventoryEquippableField equipSlot, ItemCollectionBase usedFromCollection)
        {
            var toBeRemoved = new List<uint>(8);

            // Loop through things we want to block
            foreach (var blockType in equipType.blockTypes)
            {
                // Check every slot against this block type
                foreach (var field in InventoryManager.instance.character.equipSlotFields)
                {
                    var item = InventoryManager.instance.character[field.index].item;
                    if(item != null)
                    {
                        var eq = (EquippableInventoryItem)item;

                        if(eq.equipType.ID == blockType && field.index != equipSlot.index)
                        {
                            toBeRemoved.Add(field.index);
                            bool canAdd = InventoryManager.CanAddItem(eq);
                            if (canAdd == false)
                                return false;
                        }
                    }
                }
            }

            //// There was already an item in this slot, un-equip that one first
            //if (InventoryManager.instance.character[equipSlot.index].item != null)
            //{
            //    // TODO:  FIX THIS .. !
            //    toBeRemoved.Add(equipSlot.index);
            //}
            
            foreach (uint i in toBeRemoved)
            {
                var item = InventoryManager.instance.character[i].item as EquippableInventoryItem;
                bool added = InventoryManager.AddItemAndRemove(item);
                if (added == false)
                {
                    Debug.LogError("Item could not be saved, even after check, please report this bug + stacktrace.");
                    return false;
                }

                item.NotifyItemUnEquipped();
                //InventoryManager.instance.character.SetItem(i, null);
                //InventoryManager.instance.character[i].Repaint();
            }

            return true;
        }


        //public override LinkedList<InfoBox.Row[]> GetInfo()
        //{
        //    var info = base.GetInfo();

        //    info.AddAfter(info.First, new InfoBox.Row[]
        //    {
        //        new InfoBox.Row("Strength", strength.ToString()),
        //        new InfoBox.Row("Agility", agility.ToString()),
        //        new InfoBox.Row("Defense", defense.ToString())
        //    });

        //    return info;
        //}
    }
}