using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Devdog.InventorySystem.Dialogs;

namespace Devdog.InventorySystem
{
    /// <summary>
    /// Represents a vendor that sells / buys something
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(ObjectTriggerer))]
    [AddComponentMenu("InventorySystem/Triggers/Vendor triggerer")]
    public partial class VendorTriggerer : MonoBehaviour, IObjectTriggerUser
    {
        [Header("Vendor")]
        public string vendorName;
        public bool canSellToVendor;

        [Header("Items")]
        public InventoryItemBase[] forSale;

        public bool generateItems = false;
        public int generateItemsCount = 10;
    
        /// <summary>
        /// All prices are multiplied with this value. If you want to make items 10% more expensive in a certain are, or on a certain vendor, use this.
        /// </summary>
        [Range(0.0f, 10.0f)]
        [Header("Buying / Selling")]
        public float buyPriceFactor = 1.0f;

        /// <summary>
        /// All sell prices are multiplied with this value. If you want to make items 10% more expensive in a certain are, or on a certain vendor, use this.
        /// </summary>
        [Range(0.0f, 10.0f)]
        public float sellPriceFactor = 1.0f;

        [Range(1, 999)]
        public uint maxBuyItemCount = 999;
        public bool removeItemAfterPurchase = false;

        /// <summary>
        /// Can items be bought back after they're sold?
        /// </summary>
        [Header("Buy back")]
        public bool enableBuyBack = true;

        /// <summary>
        /// How expensive is the item to buy back. item.sellPrice * buyBackCostFactor = the final price to buy back an item.
        /// </summary>
        [Range(0.0f, 10.0f)]
        public float buyBackPriceFactor = 1.0f;

        /// <summary>
        /// Max number of items in buy back window.
        /// </summary>
        public uint maxBuyBackItemSlotsCount = 10;

        /// <summary>
        /// Is buyback shared between all vendors with the same category?
        /// </summary>
        public bool buyBackIsShared = false;

        /// <summary>
        /// The category this vendor belongs to, used for sharing the buyback.
        /// Shared buyback is shared based on the vendor categeory, all vendors with the same category will have the same buy back items.
        /// </summary>
        [Tooltip("Shared buyback is shared based on the vendor categeory, all vendors with the same category will have the same buy back items.")]
        public string vendorCategory = "Default";

        /// <summary>
        /// Generator used to generate a random set of items for this vendor
        /// </summary>
        public IItemGenerator itemGenerator { get; set; }

        protected VendorUI vendorUI;
        protected Animator animator;


        /// <summary>
        /// List of items that can be bought back, not static and is vendor specific.
        /// Only used if buyBackIsShared = false
        /// if buyBackIsShared = true the static buyBackDist will be used.
        /// </summary>
        [HideInInspector]
        [NonSerialized]
        public List<InventoryItemBase> buyBackList = new List<InventoryItemBase>();

        /// <summary>
        /// A dictionary of all items that have been sold to vendors. It's static so items are shared by catergory.
        /// Key: name / category of the buyback
        /// Value: list of all items that can be bought back.
        /// </summary>
        public static Dictionary<string, List<InventoryItemBase>> buyBackDict = new Dictionary<string, List<InventoryItemBase>>();

        [NonSerialized]
        protected Transform buyBackParent;
        
        [NonSerialized]
        protected ObjectTriggerer triggerer;


        public void Awake()
        {
            vendorUI = InventoryManager.instance.vendor;
            animator = GetComponent<Animator>();
            triggerer = GetComponent<ObjectTriggerer>();
            triggerer.window = vendorUI.window;
            triggerer.handleWindowDirectly = false; // We're in charge now :)

            buyBackParent = new GameObject("Vendor_BuyBackContainer").transform;
            buyBackParent.SetParent(InventoryManager.instance.collectionObjectsParent);
            
            CreateGenerator();


            triggerer.OnTriggerUse += () =>
            {
                // Set items
                vendorUI.SetItems(forSale, false);
                vendorUI.currentVendor = this;
                vendorUI.OnRemovedItem += vendorUI_OnRemovedItem;

                if (vendorUI.window.isVisible == false)
                {
                    vendorUI.window.Show();
                }
            };
            triggerer.OnTriggerUnUse += () =>
            {
                vendorUI.window.Hide();
                vendorUI.currentVendor = null;
                vendorUI.OnRemovedItem -= vendorUI_OnRemovedItem;
            };
        }

        protected void vendorUI_OnRemovedItem(uint itemID, uint slot, uint amount)
        {
            //forSale[slot] = null;
        }

        public virtual void CreateGenerator()
        {
            var generator = new BasicItemGenerator();
            generator.SetItems(ItemManager.instance.items, 1.0f);
            generator.onlyOfType = new List<System.Type>(vendorUI.onlyAllowTypes);
            //generator.maxBuyPrice = 50;
            //generator.onlyOfType.Add(typeof(ConsumableInventoryItem));
            //generator.onlyOfType.Add(typeof(UnusableInventoryItem));
            itemGenerator = generator;

            if(generateItems)
                forSale = itemGenerator.Generate(generateItemsCount);
        }


        /// <summary>
        /// Sell an item to this vendor.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="amount"></param>
        public virtual void SellItemToVendor(InventoryItemBase item)
        {
            uint max = InventoryManager.GetItemCount(item.ID, false);

            if (CanSellItemToVendor(item, max) == false)
            {
                InventoryManager.instance.lang.vendorCannotSellItem.Show(item.name, item.description, max);
                return;
            }

            InventorySettingsManager.instance.buySellDialog.ShowDialog("Sell " + name, "Are you sure you want to sell " + name, "Sell", "Cancel", 1, (int)max, item, ItemBuySellDialogAction.Selling, this, (amount) =>
            {
                // Sell items
                SellItemToVendorNow(item, (uint)amount);

            }, (amount) =>
            {
                // Canceled

            });
        }

        /// <summary>
        /// Sell item now to this vendor. The vendor doesn't sell the object, the user sells to this vendor.
        /// Note that this does not show any UI or warnings and immediately handles the action.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public virtual bool SellItemToVendorNow(InventoryItemBase item, uint amount)
        {
            if (CanSellItemToVendor(item, amount) == false)
                return false;

            if (enableBuyBack)
            {
                var copy = GameObject.Instantiate<InventoryItemBase>(item);
                copy.currentStackSize = (uint)amount;
                copy.transform.SetParent(buyBackParent.transform);

                if (buyBackIsShared)
                {
                    if (buyBackDict.ContainsKey(vendorCategory) == false)
                        buyBackDict.Add(vendorCategory, new List<InventoryItemBase>());

                    buyBackDict[vendorCategory].Add(copy);
                    if (buyBackDict[vendorCategory].Count > maxBuyBackItemSlotsCount)
                    {
                        Destroy(buyBackDict[vendorCategory][0].gameObject);
                        buyBackDict[vendorCategory].RemoveAt(0);
                    }
                }
                else
                {
                    // Not shared drop in object array
                    buyBackList.Add(copy);
                    if (buyBackList.Count > maxBuyBackItemSlotsCount)
                    {
                        Destroy(buyBackList[0].gameObject);
                        buyBackList.RemoveAt(0);
                    }
                }
            }

            InventoryManager.instance.inventory.gold += GetSellPrice(item, (uint)amount);
            InventoryManager.RemoveItem(item.ID, amount, false);

            vendorUI.NotifyItemSoldToVendor(item, amount);
            return true;
        }

        public virtual bool CanSellItemToVendor(InventoryItemBase item, uint amount)
        {
            if (canSellToVendor == false)
                return false;

            if (item.isSellable == false)
                return false;

            return true;
        }


        public virtual bool CanBuyItemFromVendor(InventoryItemBase item, uint amount, bool isBuyBack = false)
        {
            float totalCost = GetBuyPrice(item, amount);
            if (isBuyBack)
                totalCost = GetBuyBackPrice(item, amount);

            string totalCostString = InventorySettingsManager.instance.currencyFormatter.Format(totalCost);
            if (InventoryManager.instance.inventory.gold < totalCost)
            {
                InventoryManager.instance.lang.userNotEnoughGold.Show(item.name, item.description, amount, totalCostString, InventoryManager.instance.inventory.gold);
                return false; // Not enough gold for this many
            }

            if (CanAddItemsToInventory(item, amount) == false)
            {
                InventoryManager.instance.lang.collectionFull.Show(item.name, item.description, vendorUI.collectionName);
                return false;
            }

            return true;
        }

        public virtual void BuyItemFromVendor(InventoryItemBase item, bool isBuyBack = false)
        {
            ItemBuySellDialogAction action = ItemBuySellDialogAction.Buying;
            uint maxAmount = maxBuyItemCount;
            if (isBuyBack)
            {
                action = ItemBuySellDialogAction.BuyingBack;
                maxAmount = item.currentStackSize;
            }

            InventorySettingsManager.instance.buySellDialog.ShowDialog("Buy item " + item.name, "How many items do you want to buy?", "Buy", "Cancel", 1, (int)maxAmount, item, action, this, (amount) =>
            {
                // Clicked yes!
                BuyItemFromVendorNow(item, (uint)amount, isBuyBack);

            }, (amount) =>
            {
                // Clicked cancel...

            });
        }

        /// <summary>
        /// Buy an item from this vendor, this does not display a dialog, but moves the item directly to the inventory.
        /// Note that this does not show any UI or warnings and immediately handles the action.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="amount"></param>
        public virtual bool BuyItemFromVendorNow(InventoryItemBase item, uint amount, bool isBuyBack = false)
        {
            if (CanBuyItemFromVendor(item, amount, isBuyBack) == false)
                return false;


            var c1 = GameObject.Instantiate<InventoryItemBase>(item);
            c1.currentStackSize = amount;
            InventoryManager.AddItem(c1); // Will handle unstacking if the stack size goes out of bounds.


            if(isBuyBack)
            {
                InventoryManager.instance.inventory.gold -= GetBuyBackPrice(item, amount);

                if (buyBackIsShared)
                {
                    buyBackDict[vendorCategory][(int)item.index].currentStackSize -= amount;
                    if (buyBackDict[vendorCategory][(int)item.index].currentStackSize < 1)
                    {
                        Destroy(buyBackDict[vendorCategory][(int)item.index].gameObject);
                        buyBackDict[vendorCategory].RemoveAt((int)item.index);
                    }
                }
                else
                {
                    buyBackList[(int)item.index].currentStackSize -= amount;
                    if (buyBackList[(int)item.index].currentStackSize < 1)
                    {
                        Destroy(buyBackList[(int)item.index].gameObject);
                        buyBackList.RemoveAt((int)item.index);
                    }
                }

                vendorUI.NotifyItemBoughtBackFromVendor(item, amount);
            }
            else
            {
                InventoryManager.instance.inventory.gold -= GetBuyPrice(item, amount);
            
                if(removeItemAfterPurchase)
                {
                    item.itemCollection.SetItem(item.index, null);
                    item.itemCollection.NotifyItemRemoved(item.ID, item.index, item.currentStackSize);
                    item.itemCollection[item.index].Repaint();
                    //Destroy(item.gameObject);
                }
            }

            vendorUI.NotifyItemBoughtFromVendor(item, amount);

            return true;
        }


        /// <summary>
        /// Can this item * amount be added to the inventory, is there room?
        /// </summary>
        /// <param name="item"></param>
        /// <param name="amount"></param>
        /// <returns>True if items can be placed, false is not.</returns>
        protected virtual bool CanAddItemsToInventory(InventoryItemBase item, uint amount)
        {
            uint originalStackSize = item.currentStackSize;

            item.currentStackSize = amount;
            bool can = InventoryManager.CanAddItem(item);
            item.currentStackSize = originalStackSize; // Reset

            return can;
        }

        public virtual float GetBuyBackPrice(InventoryItemBase item, uint amount)
        {
            return (float)System.Math.Round(item.sellPrice * buyBackPriceFactor, vendorUI.roundPriceToDecimals) * amount;
        }
        public virtual float GetBuyPrice(InventoryItemBase item, uint amount)
        {
            return (float)System.Math.Round(item.buyPrice * buyPriceFactor, vendorUI.roundPriceToDecimals) * amount;
        }
        public virtual float GetSellPrice(InventoryItemBase item, uint amount)
        {
            return (float)System.Math.Round(item.sellPrice * sellPriceFactor, vendorUI.roundPriceToDecimals) * amount;
        }
    }
}