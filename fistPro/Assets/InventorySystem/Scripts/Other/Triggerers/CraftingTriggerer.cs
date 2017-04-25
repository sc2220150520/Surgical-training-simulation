using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Devdog.InventorySystem.Models;

namespace Devdog.InventorySystem
{
    /// <summary>
    /// A physical representation of a crafting station.
    /// </summary>
    [AddComponentMenu("InventorySystem/Triggers/Crafting triggerer")]
    [RequireComponent(typeof(ObjectTriggerer))]
    public class CraftingTriggerer : MonoBehaviour, IObjectTriggerUser
    {
        public int craftingCategoryID = 0; // What category can we craft from?
        protected InventoryCraftingCategory category
        {
            get
            {
                return ItemManager.instance.craftingCategories[craftingCategoryID];
            }
        }

        [NonSerialized]
        protected UIWindow window;
        protected static CraftingTriggerer currentCraftingStation;

        [NonSerialized]
        protected ObjectTriggerer triggerer;

        public void Awake()
        {
            window = InventoryManager.instance.craftingStandard.window;
            triggerer = GetComponent<ObjectTriggerer>();
            triggerer.window = window;
            triggerer.handleWindowDirectly = false; // We're in charge now :)

            window.OnHide += () =>
            {
                currentCraftingStation = null;
            };

            triggerer.OnTriggerUse += () =>
            {
                window.Toggle();

                if (window.isVisible)
                {
                    currentCraftingStation = this;
                    InventoryManager.instance.craftingStandard.SetCraftingCategory(category);
                }
            };
            triggerer.OnTriggerUnUse += () =>
            {
                if (currentCraftingStation == this)
                    window.Hide();
            };
        }
    }
}