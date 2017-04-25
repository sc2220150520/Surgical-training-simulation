using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Devdog.InventorySystem.Models;

namespace Devdog.InventorySystem
{
    [AddComponentMenu("InventorySystem/Windows/Bank")]
    [RequireComponent(typeof(UIWindow))]
    public partial class BankUI : ItemCollectionBase
    {
        private float _gold;
        public float gold
        {
            get { return _gold; }
            set
            {
                if (OnGoldChanged != null)
                    OnGoldChanged(value - _gold);

                if (changeGoldAudioClip)
                    InventoryUIUtility.AudioPlayOneShot(changeGoldAudioClip);

                _gold = value;
            }
        }
        public event GoldChanged OnGoldChanged;

        [Header("Behavior")]
        public UnityEngine.UI.Button sortButton;

        [SerializeField]
        private uint _initialCollectionSize = 80;
        public override uint initialCollectionSize { get { return _initialCollectionSize; } }

        /// <summary>
        /// When the item is used from this collection, should the item be moved to the inventory?
        /// </summary>
        [Header("Item usage")]
        public bool useMoveToInventory = true;

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


        [Header("Audio & Visuals")]
        public AudioClip swapItemAudioClip;
        public AudioClip changeGoldAudioClip;
        public AudioClip sortAudioClip;
        public AudioClip onAddItemAudioClip; // When an item is added to the inventory


        public override void Awake()
        {
            base.Awake();

            InventoryManager.AddBankCollection(this);

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
            OnAddedItem += (InventoryItemBase item, uint slot, uint amount) =>
            {
                if (onAddItemAudioClip != null)
                    InventoryUIUtility.AudioPlayOneShot(onAddItemAudioClip);
            };
            OnSwappedItems += (ItemCollectionBase fromCollection, uint fromSlot, ItemCollectionBase toCollection, uint toSlot) =>
            {
                if (swapItemAudioClip != null)
                    InventoryUIUtility.AudioPlayOneShot(swapItemAudioClip);
            };
        }

        // Items from the bank go straight to the inventory
        public override bool OverrideUseMethod(InventoryItemBase item)
        {
            if (InventorySettingsManager.instance.useContextMenu)
                return false;

            if(useMoveToInventory)
                InventoryManager.AddItemAndRemove(item);

            return useMoveToInventory;
        }

        public override IList<InventoryItemUsability> GetExtraItemUsabilities(IList<InventoryItemUsability> basic)
        {
            var l = base.GetExtraItemUsabilities(basic);

            l.Add(new InventoryItemUsability("To inventory", (item) =>
            {
                var oldCollection = item.itemCollection;
                uint oldIndex = item.index;

                bool added = InventoryManager.AddItem(item);
                if(added)
                {
                    oldCollection.SetItem(oldIndex, null);
                    oldCollection[oldIndex].Repaint();

                    oldCollection.NotifyItemRemoved(item.ID, oldIndex, item.currentStackSize);
                }
            }));

            return l;
        }
    
        public override bool CanSetItem(uint slot, InventoryItemBase item)
        {
            bool set = base.CanSetItem(slot, item);
            if (set == false)
                return false;

            if (item == null)
                return true;

            return item.isStorable;
        }
    }
}