using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Devdog.InventorySystem
{
    [AddComponentMenu("InventorySystem/Windows/Vendor buy back")]
    public partial class VendorUIBuyBack : ItemCollectionBase
    {
        private UIWindowPage _window;
        public UIWindowPage window
        {
            get
            {
                if (_window == null)
                    _window = GetComponent<UIWindowPage>();

                return _window;
            }
            protected set { _window = value; }
        }

        public VendorUI vendorUI;
    

        [SerializeField]
        protected uint _initialCollectionSize = 10;
        public override uint initialCollectionSize
        {
            get
            {
                return _initialCollectionSize;
            }
        }

    
        public override void Awake()
        {
            base.Awake();
            vendorUI = InventoryManager.instance.vendor;

            // Assuming we're using InventoryUIItemWrapperVendor here...
            for (int i = 0; i < items.Length; i++)
            {
                var s = ((InventoryUIItemWrapperVendor)items[i]);
                s.vendor = vendorUI;
                s.isInBuyBack = true;
            }

            InventoryManager.instance.inventory.OnGoldChanged += (float amountAdded) =>
            {
                foreach (var item in items)
                {
                    item.Repaint();
                }
            };

            window.OnShow += () =>
            {
                UpdateItems();
            };

            vendorUI.OnSoldItemToVendor += (InventoryItemBase item, uint amount, VendorTriggerer vendor) =>
            {
                UpdateItems();
            };

            vendorUI.OnBoughtItemBackFromVendor += (InventoryItemBase item, uint amount, VendorTriggerer vendor) =>
            {
                UpdateItems();
            };
        }

        protected virtual void UpdateItems()
        {
            if (vendorUI.currentVendor == null)
                return;

            if (vendorUI.currentVendor.enableBuyBack)
            {
                if (vendorUI.currentVendor.buyBackIsShared)
                {
                    if (VendorTriggerer.buyBackDict.ContainsKey(vendorUI.currentVendor.vendorCategory))
                        SetItems(VendorTriggerer.buyBackDict[vendorUI.currentVendor.vendorCategory].ToArray(), true);
                }
                else
                    SetItems(vendorUI.currentVendor.buyBackList.ToArray(), true);
            }
        }

        public override void SetItems(InventoryItemBase[] toSet, bool setParent, bool repaint = true)
        {
            if (vendorUI.currentVendor == null || vendorUI.currentVendor.enableBuyBack == false)
                return;

            if (window.isVisible == false)
                return;

            base.SetItems(toSet, setParent, false);

            for (int i = 0; i < items.Length; i++)
                items[i].Repaint();
        }

        public override bool CanMergeSlots(uint slot1, ItemCollectionBase collection2, uint slot2)
        {
            return false;
        }
        public override bool SwapOrMerge(uint slot1, ItemCollectionBase handler2, uint slot2, bool repaint = true)
        {
            return SwapSlots(slot1, handler2, slot2, repaint);    
        }
    }
}