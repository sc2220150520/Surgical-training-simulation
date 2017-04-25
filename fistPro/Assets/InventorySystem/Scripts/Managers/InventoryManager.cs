using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Devdog.InventorySystem.Models;

namespace Devdog.InventorySystem
{
    [RequireComponent(typeof(ItemManager))]
    [RequireComponent(typeof(InventorySettingsManager))]
    [AddComponentMenu("InventorySystem/Managers/InventoryManager")]
    public partial class InventoryManager : MonoBehaviour
    {
        #region Variables

        [Header("Windows")]
        public InventoryUI inventory;
        public SkillbarUI skillbar;
        public BankUI bank;
        public CharacterUI character;
        public LootUI loot;
        public VendorUI vendor;
        public NoticeUI notice;
        public CraftingWindowStandardUI craftingStandard;
        // Add crafting -> layout
        
        [Header("Databases")]
        public InventoryLangDatabase lang; // All languages, notifications, stuff like that.


        /// <summary>
        /// The parent holds all collection's objects to keep the scene clean.
        /// </summary>
        public Transform collectionObjectsParent { get; private set; } 

        /// <summary>
        /// Collections such as the Inventory are used to loot items.
        /// When an item is picked up the item will be moved to the inventory. You can create multiple Inventories and limit types per inventory.
        /// </summary>
        private static List<InventoryCollectionLookup> lootToCollections = new List<InventoryCollectionLookup>(4);
        private static List<InventoryCollectionLookup> equipToCollections = new List<InventoryCollectionLookup>(4);
        private static List<ItemCollectionBase> bankCollections = new List<ItemCollectionBase>(4);

        #endregion
    

        private static InventoryManager _instance;


        public static InventoryManager instance {
            get {
                return _instance;
            }
        }
        
        public void Awake()
        {
            _instance = this;
            collectionObjectsParent = new GameObject("__COLLECTION_OBJECTS").transform;
        }


        #region Collection stuff

        protected virtual InventoryCollectionLookup GetBestLootCollectionForItem(InventoryItemBase item)
        {
            InventoryCollectionLookup best = null;

            foreach (var lookup in lootToCollections)
            {
                if (lookup.collection.CanAddItem(item))
                {
                    if (best == null)
                        best = lookup;
                    else if (lookup.priority > best.priority)
                        best = lookup;
                }
            }

            return best;
        }
        protected virtual InventoryCollectionLookup GetBestLootCollectionForItem(InventoryItemBase item, bool hasToFitAll)
        {
            if (hasToFitAll)
                return GetBestLootCollectionForItem(item);


            InventoryCollectionLookup best = null;

            foreach (var lookup in lootToCollections)
            {
                if (lookup.collection.CanAddItemCount(item) > 0)
                {
                    if (best == null)
                        best = lookup;
                    else if (lookup.priority > best.priority)
                        best = lookup;
                }
            }

            return best;
        }


        /// <summary>
        /// Get the item count of all items in the lootable collections.
        /// </summary>
        /// <param name="itemID"></param>
        /// <returns>Item count in all lootable collections.</returns>
        public static uint GetItemCount(uint itemID, bool checkBank)
        {
            uint count = 0;
            foreach (var collection in lootToCollections)
                count += collection.collection.GetItemCount(itemID);

            if(checkBank)
            {
                foreach (var collection in bankCollections)
                    count += collection.GetItemCount(itemID);
            }

            return count;
        }

        /// <summary>
        /// Get the first item from all lootable collections.
        /// </summary>
        /// <param name="itemID">ID of the object your searching for</param>
        /// <returns></returns>
        public static InventoryItemBase Find(uint itemID, bool checkBank)
        {
            foreach (var col in lootToCollections)
            {
                var item = col.collection.Find(itemID);
                if(item != null)
                    return item;   
            }

            if(checkBank)
            {
                foreach (var col in bankCollections)
                {
                    var item = col.Find(itemID);
                    if (item != null)
                        return item;
                }
            }

            return null;
        }

        /// <summary>
        /// Get all items with a given ID
        /// </summary>
        /// <param name="itemID">ID of the object your searching for</param>
        /// <returns></returns>
        public static List<InventoryItemBase> FindAll(uint itemID, bool checkBank)
        {
            var list = new List<InventoryItemBase>(8);
            foreach (var col in lootToCollections)
            {
                // Linq.Concat doesn't seem to work.. :/
                foreach (var item in col.collection.FindAll(itemID))
                {
                    list.Add(item);
                }
            }
        
            if(checkBank)
            {
                foreach (var col in bankCollections)
                {
                    // Linq.Concat doesn't seem to work.. :/
                    foreach (var item in col.FindAll(itemID))
                    {
                        list.Add(item);
                    }
                }
            }

            return list;
        }


        /// <summary>
        /// Add an item to an inventory.
        /// </summary>
        /// <param name="item">The item to add</param>
        /// <returns></returns>
        public static bool AddItem(InventoryItemBase item, bool repaint = true)
        {
            if (CanAddItem(item) == false)
            {
                instance.lang.collectionFull.Show(item.name, item.description, instance.inventory.collectionName);
                return false;
            }

            //// All items fit in 1 collection
            //if (item.currentStackSize <= item.maxStackSize)
            //    return best.collection.AddItem(item, repaint);

            // Not all items fit in 1 collection, divide them, grab best collection after each iteration
            // Keep going until stack is divided over collections.
            while (item.currentStackSize > 0)
            {
                var bestCollection = instance.GetBestLootCollectionForItem(item, false).collection;
                uint canStoreInCollection = bestCollection.CanAddItemCount(item);

                var copy = GameObject.Instantiate<InventoryItemBase>(item);
                copy.currentStackSize = (uint)Mathf.Min(Mathf.Min(item.currentStackSize, item.maxStackSize), canStoreInCollection);
                bestCollection.AddItem(copy);

                item.currentStackSize -= copy.currentStackSize;
                //item.currentStackSize = (uint)Mathf.Max(item.currentStackSize, 0); // Make sure it's positive
            }

            Destroy(item.gameObject); // Item is divided over collections, no longer need it.

            return true;
        }

        /// <summary>
        /// Add items to an inventory.
        /// </summary>
        /// <param name="item">The item to add</param>
        /// <returns></returns>
        public static bool AddItems(IEnumerable<InventoryItemBase> items, bool storeAsMuchAsPossible, bool repaint = true)
        {
            var toDict = new Dictionary<ItemCollectionBase, List<InventoryItemBase>>();

            foreach (var item in items)
            {
                var best = instance.GetBestLootCollectionForItem(item);
                if (best != null)
                {
                    if (toDict.ContainsKey(best.collection) == false)
                        toDict.Add(best.collection, new List<InventoryItemBase>());

                    toDict[best.collection].Add(item);
                }
                else if (storeAsMuchAsPossible == false)
                {
                    instance.lang.collectionFull.Show(item.name, item.description, instance.inventory.collectionName);
                    return false; // Not all items can be stored.
                }
            }

            // Collection is filled
            foreach (var item in toDict)
            {
                item.Key.AddItems(item.Value, repaint);
            }
        
            return true;
        }

        /// <summary>
        /// Add an item to an inventory and remove it from the collection it was previously in.
        /// </summary>
        /// <param name="item">The item to add</param>
        /// <returns></returns>
        public static bool AddItemAndRemove(InventoryItemBase item, bool repaint = true)
        {
            var best = instance.GetBestLootCollectionForItem(item);

            if (best != null)
            {
                return best.collection.AddItemAndRemove(item, repaint);
            }

            InventoryManager.instance.lang.collectionFull.Show(item.name, item.description, instance.inventory.collectionName);
            return false;
        }

        public static bool CanAddItem(InventoryItemBase item)
        {
            return CanAddItemCount(item) >= item.currentStackSize;
        }

        public static uint CanAddItemCount(InventoryItemBase item)
        {
            uint count = 0;
            foreach (var lookup in lootToCollections)
                count += lookup.collection.CanAddItemCount(item);

            return count;
        }


        /// <summary>
        /// Remove an item from the inventories / bank when checkBank = true.
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="amount"></param>
        /// <param name="checkBank">Also search the bankf or items, bank items take priority over items in the inventories</param>
        public static void RemoveItem(uint itemID, uint amount, bool checkBank)
        {
            var allItems = GetItemCount(itemID, checkBank); // All the items in all looting collections
            if (allItems < amount)
            {
                Debug.LogWarningFormat("Tried to remove {0} items, only {1} items available, check with FindAll().Count first.", amount, allItems);
                return;
            }

            uint amountToRemove = amount;
            if (checkBank)
            {
                foreach (var bank in bankCollections)
                {
                    if (amountToRemove > 0)
                    {
                        amountToRemove -= bank.RemoveItem(itemID, amountToRemove);
                    }
                    else
                        break;
                }
            }

            foreach (var inventory in lootToCollections)
            {
                //var items = bank.FindAll(itemID);
                if (amountToRemove > 0)
                {
                    amountToRemove -= inventory.collection.RemoveItem(itemID, amountToRemove);
                }
                else
                    break;
            }

        }


        /// <summary>
        /// Add a collection that functions as an Inventory. Items will be looted to this collection.
        /// </summary>
        /// <param name="collection">The collection to add.</param>
        /// <param name="priority">
        /// How important is the collection, if you 2 collections can hold the item, which one should be chosen?
        /// Range of 0 to 100
        /// </param>
        public static void AddInventoryCollection(ItemCollectionBase collection, int priority)
        {
            lootToCollections.Add(new InventoryCollectionLookup(collection, priority));
        }


        /// <summary>
        /// Check if a given collection is a loot to collection.
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static bool IsInventoryCollection(ItemCollectionBase collection)
        {
            return lootToCollections.Any(col => col.collection == collection);
        }

        /// <summary>
        /// Check if a given collection is a equip to collection.
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static bool IsEquipToCollection(ItemCollectionBase collection)
        {
            return equipToCollections.Any(col => col.collection == collection);
        }

        /// <summary>
        /// Add a collection that functions as an Equippable collection. Items can be equipped to this collection.
        /// </summary>
        /// <param name="collection">The collection to add.</param>
        /// <param name="priority">
        /// How important is the collection, if you 2 collections can hold the item, which one should be chosen?
        /// Range of 0 to 100
        /// 
        /// Note: This method is not used yet, it only registers the Equippable collection, that's it.
        /// </param>
        public static void AddEquipCollection(ItemCollectionBase collection, int priority)
        {
            equipToCollections.Add(new InventoryCollectionLookup(collection, priority));
        }


        public static void AddBankCollection(ItemCollectionBase collection)
        {
            bankCollections.Add(collection);
        }

        /// <summary>
        /// Get all bank collections
        /// I casted it to an array (instead of list) to avoid messing with the internal list.
        /// </summary>
        /// <returns></returns>
        public static ItemCollectionBase[] GetBankCollections()
        {
            return bankCollections.ToArray();
        }

        public static ItemCollectionBase[] GetLootToCollections()
        {
            var l = new List<ItemCollectionBase>(lootToCollections.Count);
            foreach (var item in lootToCollections)
                l.Add(item.collection);

            return l.ToArray();
        }

        #endregion

    }
}