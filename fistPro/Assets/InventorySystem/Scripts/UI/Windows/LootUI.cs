using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using Devdog.InventorySystem.Models;

namespace Devdog.InventorySystem
{
    [AddComponentMenu("InventorySystem/Windows/Loot")]
    [RequireComponent(typeof(UIWindow))]
    public partial class LootUI : ItemCollectionBase
    {
        public override uint initialCollectionSize
        {
            get { return 0; }
        }


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


        public override void Awake()
        {
            //base.Awake();


            //window.OnHide += () =>
            //    {
            //        // Window is closed.
            //        if(lootingObject != null)
            //        {
            //            lootingObject.SendMessage("OnLootWindowClosed", SendMessageOptions.DontRequireReceiver);
            //        }

            //        lootingObject = null;
            //    };

            // Closes the window if no objects are left.
            OnRemovedItem += (uint itemID, uint slot, uint amount) =>
            {
                foreach (var item in items)
                {
                    if (item.item != null)
                        return;
                }
                
                // All slots are empty
                window.Hide();
            };
        }

        /// <summary>
        /// Set the items for the Loot UI
        /// </summary>
        /// <param name="items">The items that we can loot.</param>
        /// <param name="lootingObject">The object that we're currently looting, treasure chest, creature, etc.</param>
        public override void SetItems(InventoryItemBase[] items, bool setParent, bool repaint = true)
        {
            // Switched loot objects, notify the old one.
            //if (this.lootingObject != null)
            //    this.lootingObject.SendMessage("OnLootWindowClosed", SendMessageOptions.DontRequireReceiver);            

            foreach (var item in this.items)
                Destroy(item.gameObject); // Get rid of the old.


            //this.lootingObject = lootingObject;
            this.items = new InventoryUIItemWrapper[items.Length];
            for (uint i = 0; i < items.Length; i++)
            {
                this.items[i] = CreateUIItem<InventoryUIItemWrapperLoot>(i, itemButtonPrefab != null ? itemButtonPrefab : InventorySettingsManager.instance.itemButtonPrefab);
                this.items[i].item = items[i];
            }
        
            foreach (var item in this.items)
            {
                item.Repaint();
            }
        }
    
        public override bool CanSetItem(uint slot, InventoryItemBase item)
        {
            return item == null;
        }

        public virtual void TakeAll()
        {
            foreach (var item in this.items)
            {
                if(item != null && item.item != null)
                {
                    ((InventoryUIItemWrapperLoot)item).PickupItem();
                }
            }
        }

        public override IList<InventoryItemUsability> GetExtraItemUsabilities(IList<InventoryItemUsability> basic)
        {
            var l = base.GetExtraItemUsabilities(basic);
        
            l.Add(new InventoryItemUsability("Loot", (item) =>
            {
                var oldCollection = item.itemCollection;
                uint oldIndex = item.index;

                bool added = InventoryManager.AddItem(item);
                if (added)
                {
                    oldCollection.SetItem(oldIndex, null);
                    oldCollection[oldIndex].Repaint();

                    oldCollection.NotifyItemRemoved(item.ID, oldIndex, item.currentStackSize);
                }
            }));

            return l;
        }


        public override bool CanMergeSlots(uint slot1, ItemCollectionBase collection2, uint slot2)
        {
            return false;    
        }
        public override bool SwapOrMerge(uint slot1, ItemCollectionBase handler2, uint slot2, bool repaint = true)
        {
            return false;    
        }
    }
}