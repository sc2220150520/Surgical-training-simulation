using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Devdog.InventorySystem
{
    [AddComponentMenu("InventorySystem/Windows/Vendor UI")]
    [RequireComponent(typeof(UIWindow))]
    public partial class VendorUI : ItemCollectionBase
    {

        #region Events

        public delegate void SoldItemToVendor(InventoryItemBase item, uint amount, VendorTriggerer vendor);
        public delegate void BoughtItemFromVendor(InventoryItemBase item, uint amount, VendorTriggerer vendor);
        public delegate void BoughtItemBackFromVendor(InventoryItemBase item, uint amount, VendorTriggerer vendor);
    

        /// <summary>
        /// Fired when an item is sold.
        /// </summary>
        public event SoldItemToVendor OnSoldItemToVendor;

        /// <summary>
        /// Fired when an item is bought, also fired when an item is bought back.
        /// </summary>
        public event BoughtItemFromVendor OnBoughtItemFromVendor;

        /// <summary>
        /// Fired when an item is bought back from a vendor.
        /// </summary>
        public event BoughtItemBackFromVendor OnBoughtItemBackFromVendor;

        #endregion


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


        protected VendorTriggerer _currentVendor;
        public VendorTriggerer currentVendor
        {
            get
            {
                return _currentVendor;
            }
            set
            {
                _currentVendor = value;
                if (_currentVendor != null)
                {
                    foreach (var item in items)
                        item.Repaint();

                    if (vendorNameText != null)
                        vendorNameText.text = _currentVendor.vendorName;
                }
            }
        }


        public VendorUIBuyBack buyBackCollection;
        public UnityEngine.UI.Text vendorNameText;


        /// <summary>
        /// Prices can be modified per vendor, 0 will generate an int 1 will generate a value of 1.1 and so forth.
        /// </summary>
        public int roundPriceToDecimals = 0;

        public AudioClip audioWhenSoldItemToVendor;
        public AudioClip audioWhenBoughtItemFromVendor;


        [SerializeField]
        protected uint _initialCollectionSize = 20;
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

            // Assuming we're using InventoryUIItemWrapperVendor here...
            for (int i = 0; i < items.Length; i++)
                ((InventoryUIItemWrapperVendor)items[i]).vendor = this;

            InventoryManager.instance.inventory.OnGoldChanged += (float amountAdded) =>
            {
                if (window.isVisible == false)
                    return;

                RepaintWindow();
            };

            window.OnShow += () =>
            {
                RepaintWindow();
            };
        }


        protected virtual void RepaintWindow()
        {
            foreach (var item in items)
            {
                item.Repaint();
            }
        }

        #region Notifies 

        public virtual void NotifyItemSoldToVendor(InventoryItemBase item, uint amount)
        {
            InventoryManager.instance.lang.vendorSoldItemToVendor.Show(item.name, item.description, amount, currentVendor.vendorName, InventorySettingsManager.instance.currencyFormatter.Format(item.sellPrice * amount));

            if (audioWhenSoldItemToVendor != null)
                InventoryUIUtility.AudioPlayOneShot(audioWhenSoldItemToVendor);

            if (OnSoldItemToVendor != null)
                OnSoldItemToVendor(item, amount, currentVendor);
        }

        public virtual void NotifyItemBoughtFromVendor(InventoryItemBase item, uint amount)
        {
            InventoryManager.instance.lang.vendorBoughtItemFromVendor.Show(item.name, item.description, amount, currentVendor.vendorName, InventorySettingsManager.instance.currencyFormatter.Format(item.buyPrice * amount));

            if (audioWhenBoughtItemFromVendor != null)
                InventoryUIUtility.AudioPlayOneShot(audioWhenBoughtItemFromVendor);

            if (OnBoughtItemFromVendor != null)
                OnBoughtItemFromVendor(item, amount, currentVendor);
        }

        public virtual void NotifyItemBoughtBackFromVendor(InventoryItemBase item, uint amount)
        {
            if (OnBoughtItemBackFromVendor != null)
                OnBoughtItemBackFromVendor(item, amount, currentVendor);
        }

        #endregion

        public override void SetItems(InventoryItemBase[] toSet, bool setParent, bool repaint = true)
        {
            for (int i = 0; i < items.Length; i++)
                items[i].item = null;

            ResizeSlots(toSet.Length);

            base.SetItems(toSet, setParent, repaint);
        }

        protected virtual void ResizeSlots(int newSize)
        {
            if (newSize > items.Length)
            {
                int startSize = items.Length;
                AddSlots((uint)(newSize - items.Length));

                for (int i = startSize; i < newSize; i++)
                    ((InventoryUIItemWrapperVendor)items[i]).vendor = this;
            }
            else if (newSize < items.Length)
            {
                if(newSize >= initialCollectionSize)
                    RemoveSlots((uint)(items.Length - newSize));
            }
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