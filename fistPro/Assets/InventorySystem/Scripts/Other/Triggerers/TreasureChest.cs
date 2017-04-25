using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Devdog.InventorySystem
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(ObjectTriggerer))]
    [AddComponentMenu("InventorySystem/Triggers/Treasure chest")]
    public partial class TreasureChest : MonoBehaviour, IObjectTriggerUser
    {
        public InventoryItemBase[] items;
        protected LootUI lootWindow;
        protected UIWindow window;

        protected bool open = false;
        protected Animator animator;

        public bool generateItems = false;
        public int minAmount = 3;
        public int maxAmount = 7;

        protected IItemGenerator itemGenerator;
        protected static TreasureChest lastChest;

        public ObjectTriggerer triggerer { get; protected set; }

        public virtual void Awake()
        {
            //base.Awake();
            lootWindow = InventoryManager.instance.loot;
            window = lootWindow.window;
            triggerer = GetComponent<ObjectTriggerer>();
            triggerer.window = window;
            triggerer.handleWindowDirectly = false; // We're in charge now :)

            animator = GetComponent<Animator>();


            triggerer.OnTriggerUse += () =>
            {
                OpenTreasureChest();
            };
            triggerer.OnTriggerUnUse += () =>
            {
                if (open)
                {
                    window.Hide();
                    
                    open = false;
                }
            };


            CreateGenerator();
        }

        protected void window_OnHide()
        {
            CloseTreasureChest();
        }

        protected void lootWindow_OnRemovedItem(uint itemID, uint slot, uint amount)
        {
            items[slot] = null;
        }


        public virtual void CreateGenerator()
        {
            var generator = new BasicItemGenerator();
            generator.SetItems(ItemManager.instance.items, 1.0f);
            generator.onlyOfType = new List<System.Type>(InventoryManager.instance.vendor.onlyAllowTypes);
            //generator.maxBuyPrice = 50;
            //generator.onlyOfType.Add(typeof(ConsumableInventoryItem));
            //generator.onlyOfType.Add(typeof(UnusableInventoryItem));
            itemGenerator = generator;

            if (generateItems)
            {
                var t = itemGenerator.Generate(minAmount, maxAmount);
                items = new InventoryItemBase[t.Length];
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = GameObject.Instantiate<InventoryItemBase>(t[i]);
                    items[i].gameObject.SetActive(false);
                    items[i].transform.SetParent(transform);
                }
            }
        }


        /// <summary>
        /// Called by window event.
        /// From an external call use the triggerer. UnUse() instead.
        /// </summary>
        protected virtual void CloseTreasureChest()
        {
            open = false;
            if (lastChest == this)
                lastChest = null;

            triggerer.UnUse(); // Bit of a special case

            window.OnHide -= window_OnHide;
            lootWindow.OnRemovedItem -= lootWindow_OnRemovedItem;
        }


        /// <summary>
        /// Open this treasure chest, auto. closes the previous one.
        /// From an external call use the triggerer. Use() instead.
        /// </summary>
        protected virtual void OpenTreasureChest()
        {
            if (this != lastChest && lastChest != null)
                lastChest.CloseTreasureChest();

            lastChest = this;

            // Set items
            lootWindow.SetItems(items, true);
            window.OnHide += window_OnHide;
            lootWindow.OnRemovedItem += lootWindow_OnRemovedItem;
            
            if (open == false)
            {
                window.Show();
                open = true;
            }
        }
    }
}