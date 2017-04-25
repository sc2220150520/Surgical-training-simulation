using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using Devdog.InventorySystem.Models;

namespace Devdog.InventorySystem
{
    [AddComponentMenu("InventorySystem/Windows/Crafting layout")]
    [RequireComponent(typeof(UIWindow))]
    public partial class CraftingWindowLayoutUI : ItemCollectionBase
    {
        #region Events

        public delegate void CraftSuccess(InventoryCraftingCategory category, InventoryCraftingBlueprint blueprint, InventoryItemBase result);
        public delegate void CraftFailed(InventoryCraftingCategory category, InventoryCraftingBlueprint blueprint);
        public delegate void CraftProgress(InventoryCraftingCategory category, InventoryCraftingBlueprint blueprint, float progress);
        public delegate void CraftCanceled(InventoryCraftingCategory category, InventoryCraftingBlueprint blueprint, float progress);

        public event CraftSuccess OnCraftSuccess;
        public event CraftFailed OnCraftFailed;
        public event CraftProgress OnCraftProgress;
        public event CraftCanceled OnCraftCanceled;

        #endregion

        //[Header("Behavior")] // Moved to custom editor
        public int craftingCategoryID = 0;

        public InventoryCraftingCategory craftingCategory
        {
            get { return ItemManager.instance.craftingCategories[craftingCategoryID]; }
        }

        public InventoryCraftingCategory currentCategory { get; protected set; }
        public InventoryCraftingBlueprint currentBlueprint { get; protected set; }
        public Dictionary<uint, uint> currentBlueprintItemsDict { get; protected set; }


        [SerializeField]
        protected uint _initialCollectionSize = 9;
        public override uint initialCollectionSize
        {
            get
            {
                return _initialCollectionSize;
            }
        }

        #region UI Elements

        [Header("UI elements")]
        public UnityEngine.UI.Text blueprintTitle;
        public UnityEngine.UI.Text blueprintDescription;

        public UnityEngine.UI.Text blueprintCraftCostText;

        public UnityEngine.UI.Slider blueprintCraftProgressSlider;

        public InventoryUIItemWrapperBase blueprintItemResult;
        public UnityEngine.UI.Button blueprintCraftButton;
    
        #endregion

        [Header("Audio & Visuals")]
        public Color enoughGoldColor;
        public Color notEnoughGoldColor;

    
        [HideInInspector]
        public float currentCraftProgress { get; protected set; }

        [System.NonSerialized]
        protected IEnumerator activeCraft;


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
            base.Awake();
            currentBlueprintItemsDict = new Dictionary<uint, uint>(9);
            SetCraftingCategory(craftingCategory);
        
            window.OnHide += () =>
            {
                CancelActiveCraft();
            };

            window.OnShow += () =>
            {
                foreach (var i in items)
                {
                    i.Repaint();
                }

                ValidateReferences();
                ValidateBlueprint();
            };

            foreach (var col in InventoryManager.GetLootToCollections())
            {
                col.OnRemovedItem += (uint itemID, uint slot, uint amount) =>
                {
                    if (window.isVisible == false)
                        return;

                    foreach (var i in items)
                    {
                        i.Repaint();
                    }

                    ValidateReferences();
                    ValidateBlueprint();
                };
                col.OnAddedItem += (InventoryItemBase item, uint slot, uint amount) =>
                {
                    if (window.isVisible == false)
                        return;

                    foreach (var i in items)
                    {
                        i.Repaint();
                    }

                    ValidateBlueprint();
                };
                col.OnUsedItem += (uint itemID, uint slot, uint amount) =>
                {
                    if (window.isVisible == false)
                        return;

                    foreach (var item in items)
                    {
                        if (item.item != null && item.item.ID == itemID)
                            item.Repaint();
                    }

                    if (currentBlueprintItemsDict.ContainsKey(itemID))
                    {
                        CancelActiveCraft(); // Used an item that we're using in crafting.
                        ValidateBlueprint();
                    }
                };
            }

            blueprintCraftButton.onClick.AddListener(() =>
            {
                if(currentBlueprint != null)
                    CraftItem(currentCategory, currentBlueprint, 1);
            });
        }

        protected void ValidateReferences()
        {
            foreach (var item in items)
            {
                if (item.item != null)
                {
                    if(item.item.itemCollection == null)
                    {
                        item.item = null;
                        item.Repaint();
                    }
                }
            }
        }

        public virtual void SetCraftingCategory(InventoryCraftingCategory category)
        {
            currentCategory = category;
            CancelActiveCraft(); // Just in case

            if(category.cols * category.rows > items.Length)
            {
                AddSlots((uint)(category.cols * category.rows - items.Length)); // Increase
            }
            else if (category.cols * category.rows < items.Length)
            {
                RemoveSlots((uint)(items.Length - category.cols * category.rows)); // Decrease
            }
        }

        protected virtual void SetBlueprint(InventoryCraftingBlueprint blueprint)
        {
            currentBlueprint = blueprint;

            // Set all the details for the blueprint.
            if (blueprintTitle != null)
                blueprintTitle.text = blueprint.name;

            if (blueprintDescription != null)
                blueprintDescription.text = blueprint.description;

            SetBlueprintResult(blueprint);

            if (blueprintCraftCostText != null)
            {
                if (InventoryManager.instance.inventory.gold < blueprint.craftCostPrice)
                    blueprintCraftCostText.color = notEnoughGoldColor;
                else
                    blueprintCraftCostText.color = enoughGoldColor;

                blueprintCraftCostText.text = InventorySettingsManager.instance.currencyFormatter.Format(blueprint.craftCostPrice);
            }
        }


        protected virtual void SetBlueprintResult(InventoryCraftingBlueprint blueprint)
        {
            if (blueprintItemResult != null)
            {
                if (blueprint != null)
                {
                    blueprintItemResult.item = blueprint.itemResult;
                    blueprintItemResult.item.currentStackSize = (uint)blueprint.itemResultCount;
                    blueprintItemResult.Repaint();
                    blueprintItemResult.item.currentStackSize = 1; // Reset
                }
                else
                {
                    bool nullBefore = blueprintItemResult.item == null;
                    blueprintItemResult.item = null;
                
                    if(nullBefore == false)
                        blueprintItemResult.Repaint();
                }
            }
        }


        /// <summary>
        /// Tries to find a blueprint based on the current layout / items inside the UI item wrappers (items).
        /// </summary>
        /// <param name="cat"></param>
        /// <returns>Returns blueprint if found one, null if not.</returns>
        public virtual InventoryCraftingBlueprint GetBlueprintFromCurrentLayout(InventoryCraftingCategory cat)
        {
            if(items.Length != cat.cols * cat.rows)
            {
                Debug.LogWarning("Updating blueprint but blueprint layout cols/rows don't match the collection");
            }

            int totalItemCountInLayout = 0; // Nr of items inside the UI wrappers.
            foreach (var item in items)
            {
                if (item.item != null)
                    totalItemCountInLayout++;
            }

            foreach (var b in cat.blueprints)
            {
                foreach (var a in b.blueprintLayouts)
                {
                    if (a.enabled)
                    {
                        var hasItems = new Dictionary<uint, uint>(); // ItemID, amount
                        //var requiredItems = new Dictionary<uint, uint>(); // ItemID, amount
                        currentBlueprintItemsDict.Clear();

                        int counter = 0; // Item index counter
                        int shouldHaveCount = 0; // Amount we should have..
                        int hasCount = 0; // How many slots in our layout
                        bool shouldBreak = false;
                        foreach (var r in a.rows)
                        {
                            if (shouldBreak)
                                break;

                            foreach (var c in r.columns)
                            {
                                if (shouldBreak)
                                    break;

                                if (c.item != null && c.amount > 0)
                                {
                                    if (currentBlueprintItemsDict.ContainsKey(c.item.ID) == false)
                                        currentBlueprintItemsDict.Add(c.item.ID, 0);

                                    currentBlueprintItemsDict[c.item.ID] += (uint)c.amount;
                                    shouldHaveCount++;

                                    if (items[counter].item != null)
                                    {
                                        if (items[counter].item.ID != c.item.ID)
                                        {
                                            shouldBreak = true;
                                            break; // Item in the wrong place...
                                        }

                                        if(hasItems.ContainsKey(c.item.ID) == false)
                                        {
                                            hasItems.Add(c.item.ID, InventoryManager.GetItemCount(c.item.ID, cat.alsoScanBankForRequiredItems));
                                        }

                                        hasCount++;
                                    }
                                    else if(items[counter].item == null && c != null)
                                    {
                                        shouldBreak = true;
                                        break;
                                    }
                                }

                                counter++;
                            }
                        }

                        if (shouldBreak)
                            break; // Onto the next one

                        // Filled slots test
                        if (totalItemCountInLayout != hasCount || shouldHaveCount != hasCount)
                            break;

                        // Check count
                        foreach (var item in currentBlueprintItemsDict)
                        {
                            if (hasItems.ContainsKey(item.Key) == false || hasItems[item.Key] < item.Value)
                                shouldBreak = true;
                        }

                        if (shouldBreak)
                            break;

                        return b;
                    }
                }
            }

            return null; // Nothing found
        }

        /// <summary>
        /// Check if the bluerint is still valid and craftable.
        /// </summary>
        protected virtual void ValidateBlueprint()
        {
            SetBlueprintResult(null); // Clear the old, check again
            var blueprint = GetBlueprintFromCurrentLayout(currentCategory);
            if (blueprint != null)
            {
                // Found something to craft!
                SetBlueprint(blueprint);
            }
            else
            {
                currentBlueprint = null;
                currentBlueprintItemsDict.Clear();
            }
        }

        public override bool SetItem(uint slot, InventoryItemBase item)
        {
            bool set = base.SetItem(slot, item);
            if(set)
            {
                ValidateBlueprint();
            }

            return set;
        }

        /// <summary>
        /// Called when an item is being crafted.
        /// </summary>
        /// <param name="progress"></param>
        protected virtual void NotifyCraftProgress(InventoryCraftingCategory category, InventoryCraftingBlueprint blueprint, float progress)
        {
            currentCraftProgress = progress;
            if (blueprintCraftProgressSlider != null)
                blueprintCraftProgressSlider.value = currentCraftProgress;

            if (OnCraftProgress != null)
                OnCraftProgress(category, blueprint, progress);
        }


        /// <summary>
        /// Crafts the item and triggers the coroutine method to handle the crafting itself.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="blueprint"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        protected virtual bool CraftItem(InventoryCraftingCategory category, InventoryCraftingBlueprint blueprint, int amount)
        {
            if (activeCraft != null)
                return false; // Already crafting

            activeCraft = _CraftItem(category, blueprint, amount, blueprint.craftingTimeDuration);
            StartCoroutine(activeCraft);

            return true;
        }

        private IEnumerator _CraftItem(InventoryCraftingCategory category, InventoryCraftingBlueprint blueprint, int amount, float currentCraftTime)
        {
            bool canCraft = CanCraftBlueprint(blueprint, category.alsoScanBankForRequiredItems, amount);
            if (canCraft)
            {
                float counter = currentCraftTime;
                while (true)
                {
                    yield return new WaitForSeconds(Time.deltaTime); // Update loop
                    counter -= Time.deltaTime;
                    NotifyCraftProgress(category, blueprint, 1.0f - Mathf.Clamp01(counter / currentCraftTime));

                    if (counter <= 0.0f)
                        break;
                }


                // Remove items from inventory
                uint[] keys = currentBlueprintItemsDict.Keys.ToArray();
                uint[] vals = currentBlueprintItemsDict.Values.ToArray();
                for (int i = 0; i < keys.Length; i++)
                {
                    InventoryManager.RemoveItem(keys[i], vals[i], category.alsoScanBankForRequiredItems); //  * GetCraftInputFieldAmount()
                }

                for (int i = 0; i < keys.Length; i++)
                {
                    uint c = InventoryManager.GetItemCount(keys.ElementAt(i), category.alsoScanBankForRequiredItems);
                    foreach (var item in items)
                    {
                        if(item.item != null && c == 0)
                        {
                            item.item = null;
                            item.Repaint();
                        }
                    }
                }


                // Remove gold
                InventoryManager.instance.inventory.gold -= blueprint.craftCostPrice;

                if (blueprint.successChanceFactor >= Random.value)
                {
                    // Store crafted item
                    var c = GameObject.Instantiate<InventoryItemBase>(blueprint.itemResult);
                    c.currentStackSize = (uint)(blueprint.itemResultCount); //  * GetCraftInputFieldAmount()
                    if (category.forceSaveInCollection != null)
                    {
                        category.forceSaveInCollection.AddItem(c);
                    }
                    else
                    {
                        InventoryManager.AddItem(c);
                    }

                    InventoryManager.instance.lang.craftedItem.Show(blueprint.itemResult.name, blueprint.itemResult.description);

                    if (OnCraftSuccess != null)
                        OnCraftSuccess(category, blueprint, c);
                }
                else
                {
                    InventoryManager.instance.lang.craftingFailed.Show(blueprint.itemResult.name, blueprint.itemResult.description);

                    if (OnCraftFailed != null)
                        OnCraftFailed(category, blueprint);
                }

                amount--;
                currentBlueprintItemsDict.Clear();

                if (amount > 0)
                {
                    activeCraft = _CraftItem(category, blueprint, amount, Mathf.Clamp(currentCraftTime / blueprint.craftingTimeSpeedupFactor, 0.0f, blueprint.craftingTimeDuration));
                    StartCoroutine(activeCraft);
                }
                else
                {
                    activeCraft = null; // All done
                }
            }
            else
            {
                //StopCoroutine(activeCraft);
                activeCraft = null;
            }
        }



        /// <summary>
        /// Cancels any crafting action that is active. For example when you're crafting an item with a timer, cancel it when you walk away.
        /// </summary>
        public void CancelActiveCraft()
        {
            if (activeCraft != null)
            {
                StopCoroutine(activeCraft);
                InventoryManager.instance.lang.craftingCanceled.Show(currentBlueprint.itemResult.name, currentBlueprint.itemResult.description);
                activeCraft = null;

                if (OnCraftCanceled != null)
                    OnCraftCanceled(currentCategory, currentBlueprint, currentCraftProgress);
            }
        }

        /// <summary>
        /// Does the inventory contain the required items?
        /// </summary>
        /// <param name="blueprint"></param>
        /// <param name="alsoScanBank"></param>
        /// <param name="craftCount"></param>
        /// <returns></returns>
        public virtual bool CanCraftBlueprint(InventoryCraftingBlueprint blueprint, bool alsoScanBank, int craftCount)
        {
            // Layout can only be triggered if one is visible -> selected. So no need to check it here.


            if (InventoryManager.instance.inventory.gold < blueprint.craftCostPrice * craftCount)
            {
                InventoryManager.instance.lang.userNotEnoughGold.Show(blueprint.itemResult.name, blueprint.itemResult.description, craftCount, blueprint.craftCostPrice * craftCount, InventoryManager.instance.inventory.gold);
                return false;
            }

            // Can the items be stored in the inventory / designated spot?
            if (currentCategory.forceSaveInCollection != null)
            {
                bool added = currentCategory.forceSaveInCollection.CanAddItem(blueprint.itemResult);
                if (added == false)
                    return false;
            }
            else
            {
                bool added = InventoryManager.CanAddItem(blueprint.itemResult);
                if (added == false)
                    return false;
            }

            return true;
        }
    }
}