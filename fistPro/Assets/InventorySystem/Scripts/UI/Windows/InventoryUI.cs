﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using Devdog.InventorySystem.Models;

namespace Devdog.InventorySystem
{
    [AddComponentMenu("InventorySystem/Windows/Inventory")]
    [RequireComponent(typeof(UIWindow))]
    public partial class InventoryUI : ItemCollectionBase
    {
        protected float _gold;
        public float gold
        {
            get { return _gold; }
            set
            {
                float old = _gold;
                _gold = value;

                if (changeGoldAudioClip)
                    InventoryUIUtility.AudioPlayOneShot(changeGoldAudioClip);

                if (OnGoldChanged != null)
                    OnGoldChanged(value - old);
            }
        }
        public event GoldChanged OnGoldChanged;

        private UIWindow _window;
        public UIWindow window
        {
            get
            {
                if (_window == null)
                    _window = GetComponent<UIWindow>();

                return _window;
            }
            protected set { _window = value; }
        }


        //public AudioSource audioSource;
        //public UnityEngine.UI.Text goldText;
        [Header("Behavior")]
        public UnityEngine.UI.Button sortButton;

        /// <summary>
        /// The collection used to extend this bag, leave empty if there is none.
        /// </summary>
        public ItemCollectionBase inventoryExtenderCollection;

        /// <summary>
        /// Scrolling rect inside the inventory
        /// </summary>
        public ScrollRect scrollRect;

        /// <summary>
        /// When an item is looted, save into this inventory?
        /// </summary>
        public bool isLootToInventory = true;


        /// <summary>
        /// How much priority does this inventory have when looting? When an item can be stored in multiple inventories, which one should be chosen?
        /// Range of 0 to 100
        /// </summary>
        [Range(0, 100)]
        public int lootPriority = 50;

        [SerializeField]
        private uint _initialCollectionSize = 20;
        public override uint initialCollectionSize { get { return _initialCollectionSize; } }


        /// <summary>
        /// When the item is used and the bank is open the item will be stored.
        /// </summary>
        [Header("Item usage")]
        public bool useItemMoveToBank = true;

        /// <summary>
        /// When the item is used and the vendor window is open, should the item be sold.
        /// </summary>
        public bool useItemSell = true;

        [Header("Audio & Visuals")]
        public AudioClip swapItemAudioClip;
        public AudioClip changeGoldAudioClip;
        public AudioClip sortAudioClip;
        public AudioClip onAddItemAudioClip; // When an item is added to the inventory

    
        public override void Awake()
        {
            base.Awake();

            if(isLootToInventory)
                InventoryManager.AddInventoryCollection(this, lootPriority);

            if(sortButton != null)
            {
                sortButton.onClick.AddListener(() =>
                {
                    SortCollection();

                    if (sortAudioClip)
                        InventoryUIUtility.AudioPlayOneShot(sortAudioClip);
                });
            }
		
            // Listen for events
            OnAddedItem += (InventoryItemBase items, uint slot, uint amount) => 
            {
                if (onAddItemAudioClip != null)
                    InventoryUIUtility.AudioPlayOneShot(onAddItemAudioClip);
            };
            OnSwappedItems += (ItemCollectionBase fromCollection, uint fromSlot, ItemCollectionBase toCollection, uint toSlot) =>
            {
                if (swapItemAudioClip != null)
                    InventoryUIUtility.AudioPlayOneShot(swapItemAudioClip);
            };
            OnDroppedItem += (InventoryItemBase item, uint slot, GameObject droppedObj) => 
            {
            
            };
            OnResized += (uint fromSize, uint toSize) =>
            {
                StartCoroutine(SetScrollToZero());
            };
            OnSorted += () => 
            {
            
            };
            OnGoldChanged += (float goldAdded) =>
            {

            };
        }

        private IEnumerator SetScrollToZero()
        {
            yield return null; // Wait a frame for the UI to update... (UI Layout updates at the end of update cycle, so wait 1 cycle, then update)

            if (scrollRect != null && scrollRect.verticalScrollbar != null)
            {
                scrollRect.verticalNormalizedPosition = 1.0f;
            }
        }

        public override IList<InventoryItemUsability> GetExtraItemUsabilities(IList<InventoryItemUsability> basicList)
        {
            var basic = base.GetExtraItemUsabilities(basicList);

            if (InventoryManager.instance.bank != null)
            {
                if (InventoryManager.instance.bank.window.isVisible)
                {
                    basic.Add(new InventoryItemUsability("Store", (item) =>
                    {
                        InventoryManager.instance.bank.AddItemAndRemove(item);
                    }));
                }
            }

            if (InventoryManager.instance.vendor != null)
            {
                if (InventoryManager.instance.vendor.window.isVisible)
                {
                    basic.Add(new InventoryItemUsability("Sell", (item) =>
                    {
                        InventoryManager.instance.vendor.currentVendor.SellItemToVendor(item);
                    }));
                }
            }

            return basic;
        }

        public override bool OverrideUseMethod(InventoryItemBase item)
        {
            // If both bank and vendor are open bank will take priority, probably the safest action...
            if (InventorySettingsManager.instance.useContextMenu)
                return false;

            if(useItemMoveToBank)
            {
                if (InventoryManager.instance.bank != null && InventoryManager.instance.bank.window.isVisible)
                {
                    if(item.isStorable)
                    {
                        InventoryManager.instance.bank.AddItemAndRemove(item);
                        return true;
                    }
                }
            }

            if (useItemSell)
            {
                if (InventoryManager.instance.vendor != null && InventoryManager.instance.vendor.window.isVisible)
                {
                    InventoryManager.instance.vendor.currentVendor.SellItemToVendor(item);
                    return true;
                }
            }

            return false; // Didn't override anything
        }

    }
}