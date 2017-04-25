using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using Devdog.InventorySystem.Models;
using Devdog.InventorySystem.UI.Models;
using Random = UnityEngine.Random;

namespace Devdog.InventorySystem
{
    [AddComponentMenu("InventorySystem/Windows/Crafting standard")]
    [RequireComponent(typeof(UIWindow))]
    public partial class CraftingWindowStandardUI : MonoBehaviour
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


        public InventoryCraftingCategory defaultCategory
        {
            get { return ItemManager.instance.craftingCategories[defaultCategoryID]; }
        }
        [Header("Crafting")]
        public int defaultCategoryID;

        /// <summary>
        /// Crafting category title
        /// </summary>
        [Header("General UI references")]
        public UnityEngine.UI.Text currentCategoryTitle;

        /// <summary>
        /// Crafting category description
        /// </summary>
        public UnityEngine.UI.Text currentCategoryDescription;

        [InventoryRequired]
        public RectTransform blueprintsContainer;


        [Header("Blueprint prefabs")]
        public InventoryCraftingCategoryUI blueprintCategoryPrefab;
        
        /// <summary>
        /// The button used to select the prefab the user wishes to craft.
        /// </summary>
        [InventoryRequired] public InventoryCraftingBlueprintUI blueprintButtonPrefab;

        /// <summary>
        /// A single required item to be shown in the UI.
        /// </summary>
        [InventoryRequired] public InventoryUIItemWrapper blueprintRequiredItemPrefab;

        #region Crafting item page

        [Header("Craft blueprint UI References")]
        public InventoryUIItemWrapper blueprintIcon;
        public UnityEngine.UI.Text blueprintTitle;
        public UnityEngine.UI.Text blueprintDescription;

        [InventoryRequired]
        public RectTransform blueprintRequiredItemsContainer;
        public UnityEngine.UI.Slider blueprintCraftProgressSlider;


        public UnityEngine.UI.Text blueprintCraftCostText;

        /// <summary>
        /// Craft the selected item button
        /// </summary>
        [InventoryRequired]
        public UnityEngine.UI.Button blueprintCraftButton;
        public UnityEngine.UI.Button blueprintMinCraftButton;
        public UnityEngine.UI.InputField blueprintCraftAmountInput;
        public UnityEngine.UI.Button blueprintPlusCraftButton;

        #endregion

        [Header("UI window pages")]
        public UIWindowPage noBlueprintSelectedPage;
        public UIWindowPage blueprintCraftPage;


        [Header("Audio & Visuals")]
        public Color itemsAvailableColor = Color.white;
        public Color itemsNotAvailableColor = Color.red;

        public AudioClip successCraftItem;
        public AudioClip failedCraftItem;
        public AudioClip canceledCraftItem;

        public AnimationClip craftAnimation;



        #region Pools

        [NonSerialized]
        protected InventoryPool<InventoryCraftingCategoryUI> categoryPool;
        
        [NonSerialized]
        protected InventoryPool<InventoryCraftingBlueprintUI> blueprintPool;

        [NonSerialized]
        protected InventoryPool<InventoryUIItemWrapper> blueprintRequiredItemsPool;

        #endregion

        public InventoryCraftingCategory currentCategory { get; protected set; }
        public InventoryCraftingBlueprint currentBlueprint { get; protected set; }

        [NonSerialized]
        protected IEnumerator activeCraft;

        public float currentCraftProgress
        {
            get;
            protected set;
        }

        public virtual void Awake()
        {
            if (blueprintCategoryPrefab != null)
                categoryPool = new InventoryPool<InventoryCraftingCategoryUI>(blueprintCategoryPrefab, 16);
            
#if UNITY_EDITOR
            if (blueprintButtonPrefab == null)
                Debug.LogWarning("Blueprint button prefab is empty in CraftingWindowStandardUI", gameObject);

            if (blueprintRequiredItemPrefab == null)
                Debug.LogWarning("Blueprint required item prefab is empty in CraftingWindowStandardUI", gameObject);
#endif

            blueprintPool = new InventoryPool<InventoryCraftingBlueprintUI>(blueprintButtonPrefab, 128);
            blueprintRequiredItemsPool = new InventoryPool<InventoryUIItemWrapper>(blueprintRequiredItemPrefab, 8);

            if (defaultCategoryID >= 0 && defaultCategoryID <= ItemManager.instance.craftingCategories.Length - 1)
                currentCategory = defaultCategory;

            if(blueprintMinCraftButton != null)
            {
                blueprintMinCraftButton.onClick.AddListener(() =>
                {
                    if(Input.GetKey(KeyCode.LeftShift))
                        blueprintCraftAmountInput.text = (GetCraftInputFieldAmount() - 10).ToString();
                    else
                        blueprintCraftAmountInput.text = (GetCraftInputFieldAmount() - 1).ToString();

                    ValidateCraftInputFieldAmount();
                });
            }
            if(blueprintPlusCraftButton != null)
            {
                blueprintPlusCraftButton.onClick.AddListener(() =>
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                        blueprintCraftAmountInput.text = (GetCraftInputFieldAmount() + 10).ToString();
                    else
                        blueprintCraftAmountInput.text = (GetCraftInputFieldAmount() + 1).ToString();

                    ValidateCraftInputFieldAmount();
                });
            }

            blueprintCraftButton.onClick.AddListener(() => CraftItem(currentCategory, currentBlueprint, GetCraftInputFieldAmount()));

            window.OnShow += () =>
            {
                if(currentCategory != null)
                    SetCraftingCategory(currentCategory);

                if (currentBlueprint != null)
                    SetBlueprint(currentBlueprint);
            };

            window.OnHide += CancelActiveCraft;

            foreach (var col in InventoryManager.GetLootToCollections())
            {
                col.OnAddedItem += (InventoryItemBase item, uint slot, uint amount) =>
                {
                    if(currentBlueprint != null)
                        SetBlueprint(currentBlueprint);
                };
                col.OnRemovedItem += (uint itemID, uint slot, uint amount) =>
                {
                    if (currentBlueprint != null)
                        SetBlueprint(currentBlueprint);
                };
                col.OnDroppedItem += (InventoryItemBase item, uint slot, GameObject droppedObj) =>
                {
                    CancelActiveCraft(); // If the user drops something.

                    if (currentBlueprint != null)
                        SetBlueprint(currentBlueprint);
                };
            }

            InventoryManager.instance.inventory.OnGoldChanged += (float added) =>
            {
                if (currentBlueprint != null)
                    SetBlueprint(currentBlueprint);
            };
        }

        protected virtual int GetCraftInputFieldAmount()
        {
            if(blueprintCraftAmountInput != null)
                return int.Parse(blueprintCraftAmountInput.text);

            return 1;
        }

        protected virtual void ValidateCraftInputFieldAmount()
        {
            int amount = GetCraftInputFieldAmount();
            if (amount < 1)
                amount = 1;
            else if (amount > 999)
                amount = 999;

            blueprintCraftAmountInput.text = amount.ToString();
        }


        public virtual void SetCraftingCategory(InventoryCraftingCategory category)
        {
            categoryPool.DestroyAll();
            blueprintPool.DestroyAll();
            currentCategory = category;
            if (blueprintCraftAmountInput != null)
                blueprintCraftAmountInput.text = "1"; // Reset
            
            CancelActiveCraft(); // Just in case

            if(currentCategoryTitle != null)
                currentCategoryTitle.text = currentCategory.name;
        
            if (currentCategoryDescription != null)
                currentCategoryDescription.text = currentCategory.description;

            if (noBlueprintSelectedPage != null)
                noBlueprintSelectedPage.Show();

            if (blueprintCraftPage == null && currentCategory.blueprints.Length > 0)
                SetBlueprint(currentCategory.blueprints[0]); // Select first blueprint

            int lastItemCategory = -1;
            foreach (var b in currentCategory.blueprints)
            {
                if (b.playerLearnedBlueprint == false)
                    continue;

                var blue = blueprintPool.Get();
                blue.transform.SetParent(blueprintsContainer);
                blue.Set(b);

                if (blueprintCategoryPrefab != null)
                {
                    if (lastItemCategory != b.itemResult.category.ID)
                    {
                        lastItemCategory = (int)b.itemResult.category.ID;

                        var uiCategory = categoryPool.Get();
                        uiCategory.Set(b.itemResult.category.name);

                        uiCategory.transform.SetParent(blueprintsContainer);
                        blue.transform.SetParent(uiCategory.container);
                    }
                }

                var bTemp = b; // Store capture list, etc.
                blue.button.onClick.AddListener(() =>
                {
                    currentBlueprint = bTemp;
                    SetBlueprint(currentBlueprint);
                    CancelActiveCraft(); // Just in case         

                    if (blueprintCraftPage != null && blueprintCraftPage.isVisible == false)
                        blueprintCraftPage.Show();
                });
            }
        }

        protected virtual void SetBlueprint(InventoryCraftingBlueprint blueprint)
        {
            if (window.isVisible == false)
                return;

            // Set all the details for the blueprint.
            if (blueprintTitle != null)
                blueprintTitle.text = blueprint.name;

            if (blueprintDescription != null)
                blueprintDescription.text = blueprint.description;

            if (blueprintIcon != null)
            {
                blueprintIcon.item = blueprint.itemResult;
                blueprintIcon.item.currentStackSize = (uint)blueprint.itemResultCount;
                blueprintIcon.Repaint();
                blueprintIcon.item.currentStackSize = 1; // Reset
            }

            if (blueprintCraftProgressSlider)
                blueprintCraftProgressSlider.value = 0.0f; // Reset

            if (blueprintCraftCostText != null)
            {
                if (InventoryManager.instance.inventory.gold < blueprint.craftCostPrice)
                    blueprintCraftCostText.color = itemsNotAvailableColor;
                else
                    blueprintCraftCostText.color = itemsAvailableColor;

                blueprintCraftCostText.text = InventorySettingsManager.instance.currencyFormatter.Format(blueprint.craftCostPrice);
            }

            blueprintRequiredItemsPool.DestroyAll();
            foreach (var item in blueprint.requiredItems)
            {
                var ui = blueprintRequiredItemsPool.Get();
                item.item.currentStackSize = (uint)item.amount;
                ui.transform.SetParent(blueprintRequiredItemsContainer);
                ui.item = item.item;
                if (InventoryManager.GetItemCount(item.item.ID, currentCategory.alsoScanBankForRequiredItems) >= item.amount)
                    ui.icon.color = itemsAvailableColor;
                else
                    ui.icon.color = itemsNotAvailableColor;

                ui.Repaint();
                item.item.currentStackSize = 1; // Reset
            }
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
                foreach (var item in blueprint.requiredItems)
                {
                    InventoryManager.RemoveItem(item.item.ID, (uint)item.amount, category.alsoScanBankForRequiredItems); //  * GetCraftInputFieldAmount()
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

                if (amount > 0)
                {
                    if (blueprintCraftAmountInput != null)
                        blueprintCraftAmountInput.text = amount.ToString();

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

                if (canceledCraftItem != null)
                    InventoryUIUtility.AudioPlayOneShot(canceledCraftItem);

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
            foreach (var item in blueprint.requiredItems)
            {
                uint count = InventoryManager.GetItemCount(item.item.ID, alsoScanBank);
                if (count < item.amount * craftCount)
                {
                    InventoryManager.instance.lang.craftingDontHaveRequiredItems.Show(item.item.name, item.item.description, blueprint.name);
                    return false;
                }
            }

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