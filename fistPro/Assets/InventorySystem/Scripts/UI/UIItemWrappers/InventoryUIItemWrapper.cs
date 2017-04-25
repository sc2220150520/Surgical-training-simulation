using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

namespace Devdog.InventorySystem
{
    [AddComponentMenu("InventorySystem/UI Wrappers/UI Wrapper")]
    public partial class InventoryUIItemWrapper : InventoryUIItemWrapperBase, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerUpHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {

        #region Variables


        /// <summary>
        /// The icon that was set when the object started.
        /// If null, the default will be chosen.
        /// </summary>
        [NonSerialized]
        private Sprite startIcon;

        #region UI Elements

        public UnityEngine.UI.Text amountText;
        public UnityEngine.UI.Text itemName;
        //public Text keyCombinationText;
        public UnityEngine.UI.Image icon;
        public UnityEngine.UI.Image cooldownImage;

        public override Material material
        {
            get
            {
                return icon.material;
            }
            set
            {
                icon.material = value;
            }
        }

        //public Button button;

        #endregion


    
        public virtual bool isEmpty
        {
            get
            {
                return item == null;
            }
        }




        [NonSerialized]
        protected static bool pointerDownOnUIElement = false;

        /// <summary>
        /// Last time the button was pressed, used to determine long presses.
        /// </summary>
        [NonSerialized]
        private static float lastDownTime;
        [NonSerialized]
        private bool pressing = false;

        #endregion


        public virtual void Awake()
        {
            // Only set it if it differs from the default.
            if(icon != null)
                startIcon = icon.sprite;
        }

        //private IEnumerator _RepaintCooldown()
        //{
        //    yield return new WaitForSeconds(Random.value); // Random offset to avoid hickups
        
        //    while(true)
        //    {
        //        yield return new WaitForSeconds(0.1f); // 10x a second
        //        RepaintCooldown();
        //    }
        //}

        public virtual void Update()
        {
            RepaintCooldown();

#if UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_BLACKBERRY

        if (pressing && InventoryUIUtility.isDraggingItem == false && lastDownTime != 0.0f && Time.timeSinceLevelLoad - InventorySettingsManager.instance.mobileLongPressTime > lastDownTime)
        {
            // Long press for mobile
            if (InventorySettingsManager.instance.mobileUnstackItemKey == MobileUIActions.LongPress)
            {
                TriggerUnstack();
                lastDownTime = 0.0f;
            }
            else if (InventorySettingsManager.instance.mobileUseItemButton == MobileUIActions.LongPress)
            {
                TriggerUse();
                lastDownTime = 0.0f;
            }
        }

#endif
        }


        #region Button handler UI events

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (itemCollection == null)
                return;

            if (item != null && eventData.button == PointerEventData.InputButton.Left && itemCollection.canDragInCollection)
            {
                // Create a copy
                var copy = GameObject.Instantiate<InventoryUIItemWrapper>(this);
                copy.index = index;
                copy.itemCollection = itemCollection;

                var copyComp = copy.GetComponent<RectTransform>();
                copyComp.SetParent(InventorySettingsManager.instance.guiRoot);
                copyComp.transform.localPosition = new Vector3(copyComp.transform.localPosition.x, copyComp.transform.localPosition.y, 0.0f);
                copyComp.sizeDelta = GetComponent<RectTransform>().sizeDelta;

                InventoryUIUtility.BeginDrag(copy, (uint)copy.index, itemCollection, eventData); // Make sure they're the same size, copy doesn't handle this.
            }
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (item != null && itemCollection != null && itemCollection.canDragInCollection) // Can only drag existing item
                InventoryUIUtility.Drag(this, index, itemCollection, eventData);
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (item != null && itemCollection != null && itemCollection.canDragInCollection)
            {
                var lookup = InventoryUIUtility.EndDrag(this, index, itemCollection, eventData);

                // Didn't end on a button or used wrong key.
                if (lookup == null)
                    return;

                if (lookup.endOnButton)
                {
                    // Place on a slot
                    lookup.startItemCollection.SwapOrMerge((uint)lookup.startIndex, lookup.endItemCollection, (uint)lookup.endIndex);
                }
                else if (lookup.startItemCollection.useReferences)
                {
                    lookup.startItemCollection.SetItem((uint)lookup.startIndex, null);
                    lookup.startItemCollection[lookup.startIndex].Repaint();
                }
                else if(InventoryUIUtility.clickedUIElement == false)
                {
                    TriggerDrop();
                }
            }
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (itemCollection == null)
                return;

            pointerDownOnUIElement = InventoryUIUtility.clickedUIElement;
            if (pointerDownOnUIElement == false)
                return;

            if (InventorySettingsManager.instance.useContextMenu && (eventData.button == InventorySettingsManager.instance.triggerContextMenuButton || Application.isMobilePlatform))
            {
                if (item != null)
                    TriggerContextMenu();

                return;
            }

            if (InventorySettingsManager.instance.mobileUnstackItemKey == MobileUIActions.SingleTap)
            {
                TriggerUnstack();
                return;
            }
            else if(InventorySettingsManager.instance.mobileUseItemButton == MobileUIActions.SingleTap)
            {
                TriggerUse();
                return;
            }

            if (item != null && pressing == false && Time.timeSinceLevelLoad - InventorySettingsManager.instance.mobileDoubleTapTime < lastDownTime)
            {
                // Did double tap
                if (InventorySettingsManager.instance.mobileUnstackItemKey == MobileUIActions.DoubleTap)
                {
                    TriggerUnstack();
                    return;
                }
                else if(InventorySettingsManager.instance.mobileUseItemButton == MobileUIActions.DoubleTap)
                {
                    TriggerUse();
                    return;
                }
            }

            lastDownTime = Time.timeSinceLevelLoad;
            pressing = true;
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            // Started on a UI element?
            if (pointerDownOnUIElement == false)
                return;

            pressing = false;

            if (InventorySettingsManager.instance.useContextMenu || itemCollection == null)
                return;

            if(item != null)
            {
                // Check if we're trying to unstack
                if (itemCollection.useReferences == false && eventData.button == InventorySettingsManager.instance.unstackItemButton && Input.GetKey(InventorySettingsManager.instance.unstackItemKey))
                {
                    TriggerUnstack();
                    return; // Stop the rest otherwise we'll do 2 actions at once.
                }

                // Pointer up inside an item, swap them
                // Debug.Log("Pointer up on item " + eventData.button);
                if (eventData.button == InventorySettingsManager.instance.useItemButton)
                {
                    TriggerUse();
                }
            }


            pointerDownOnUIElement = false;
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            InventoryUIUtility.EnterItem(this, index, itemCollection, eventData);
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            InventoryUIUtility.ExitItem(this, index, itemCollection, eventData);
        }

        #endregion

        #region Triggers
    
        public override void TriggerContextMenu()
        {
            if (item == null)
                return;

            var contextMenu = InventorySettingsManager.instance.contextMenu;

            // Show context menu
            contextMenu.ClearMenuOptions();

            var itemList = item.GetUsabilities();
            itemList = itemCollection.GetExtraItemUsabilities(itemList);
            foreach (var i in itemList)
            {
                contextMenu.AddMenuOption(i.actionName, item, i.useItemCallback);
            }

            contextMenu.window.Show();
        }

        public override void TriggerUnstack()
        {
            if (item == null || itemCollection.useReferences || itemCollection.canPutItemsInCollection == false)
                return;

            if (item.currentStackSize > 1)
            {
                var m = InventorySettingsManager.instance;
                if(m.useUnstackDialog)
                {
                    var d = InventoryManager.instance.lang.unstackDialog;
                    m.intValDialog.ShowDialog(d.title, d.message, m.defaultDialogPositiveButtonText, m.defaultDialogNegativeButtonText, 1, (int)item.currentStackSize - 1, item,
                        (int val) =>
                        {
                            itemCollection.UnstackSlot(index, (uint)val, true);
                        },
                        (int val) =>
                        {
                            // Canceled


                        });
                }
                else
                {
                    itemCollection.UnstackSlot(index, (uint)Mathf.Floor(item.currentStackSize / 2), true);
                }
            }
        }

        public override void TriggerDrop(bool useRaycast = true)
        {
            if (item == null || itemCollection.canDropFromCollection == false)
                return;

            if(item.isDroppable == false)
            {
                InventoryManager.instance.lang.itemCannotBeDropped.Show(item.name, item.description);
                return;
            }

            Vector3 dropPosition = InventorySettingsManager.instance.playerObject.transform.position;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, InventorySettingsManager.instance.maxDropDistance,
                InventorySettingsManager.instance.layersWhenDropping))
            {
                dropPosition = hit.point;
            }
            else
            {
                return; // Couldn't drop item
            }

            var s = InventorySettingsManager.instance;
            if (useRaycast && s.showConfirmationDialogWhenDroppingItem && s.showConfirmationDialogMinRarity.ID <= item.rarity.ID)
            {
                // Not on a button, drop it
                var tempItem = item; // Capture list stuff
                var msg = InventoryManager.instance.lang.confirmationDialogDrop;
                s.confirmationDialog.ShowDialog(msg.title, msg.message, s.defaultDialogPositiveButtonText, s.defaultDialogNegativeButtonText, item,
                    (dialog) =>
                    {
                        ItemCollectionBase startCollection = tempItem.itemCollection;
                        uint startIndex = tempItem.index;

                        var d = tempItem.Drop(dropPosition);
                        if (d != null)
                        {
                            startCollection[startIndex].Repaint();
                        }
                    },
                    (dialog) =>
                    {
                        //Debug.Log("No clicked");
                    });
            }
            else
            {
                var d = item.Drop(dropPosition);
                if (d != null)
                {
                    Repaint();
                }
            }
        }

        public override void TriggerUse()
        {
            if (item == null)
                return;

            if (itemCollection.canUseFromCollection == false)
                return;

            int used = item.Use();
            if (used >= 0)
            {
                Repaint();
            }
        }
    
        #endregion

        /// <summary>
        /// Repaints the item icon and amount.
        /// </summary>
        public override void Repaint()
        {
            if (item != null)
            {
                if (amountText != null)
                {
                    // Only show when we have more then 1 item.
                    if (item.currentStackSize > 1)
                        amountText.text = item.currentStackSize.ToString();
                    else
                        amountText.text = string.Empty;
                }

                if (itemName != null)
                    itemName.text = item.name;

                if(icon != null)
                    icon.sprite = item.icon;
            }
            else
            {
                if (amountText != null)
                    amountText.text = string.Empty;

                if (itemName != null)
                    itemName.text = string.Empty;

                if(icon != null)
                    icon.sprite = startIcon != null ? startIcon : InventorySettingsManager.instance.defaultSlotIcon;
            }

            //RepaintCooldown(); // Already called by update loop
        }

        public virtual void RepaintCooldown()
        {
            if (cooldownImage == null)
                return;

            if (item != null)
            {
                if(item.isInCooldown)
                {
                    cooldownImage.fillAmount = 1.0f - item.cooldownFactor;
                    return;
                }
            }

            // To avoid GC
            if (cooldownImage.fillAmount != 0.0f)
                cooldownImage.fillAmount = 0.0f;
        }
    }
}