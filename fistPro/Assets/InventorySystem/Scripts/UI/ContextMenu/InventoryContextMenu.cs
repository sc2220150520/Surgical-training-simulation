using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Devdog.InventorySystem.Models;
using UnityEngine.UI;

namespace Devdog.InventorySystem
{
    [RequireComponent(typeof(UIWindow))]
    [AddComponentMenu("InventorySystem/Windows Other/Context menu")]
    public partial class InventoryContextMenu : MonoBehaviour
    {
        /// <summary>
        /// An option that can be chosen (code only).
        /// </summary>
        public partial class InventoryContextMenuOption : InventoryItemUsability
        {
            public InventoryContextMenuItem uiElement;

            public InventoryContextMenuOption(string actionName, UseItemCallback useItemCallback, InventoryContextMenuItem uiElement)
                : base(actionName, useItemCallback)
            {
                this.uiElement = uiElement;
            }
        }


        private List<InventoryContextMenuOption> menuOptions = new List<InventoryContextMenuOption>(8);

        [Header("UI")]
        public RectTransform container;
        
        /// <summary>
        /// Single line / menu item inside the context menu.
        /// </summary>
        public InventoryContextMenuItem contextMenuItemPrefab;


        /// <summary>
        /// If there is only 1 action in the context menu, trigger it auto.
        /// </summary>
        [Header("Behavior")]
        public bool autoTriggerIfSingleAction = true;

        public bool closeWindowWhenClickedOutside = true;
        public bool positionAtMouse = true;
        
        public UIWindow window { get; set; }
        private InventoryPool<InventoryContextMenuItem> pool;


        public virtual void Awake()
        {
            window = GetComponent<UIWindow>();
            pool = new InventoryPool<InventoryContextMenuItem>(contextMenuItemPrefab, 8);

            window.OnShow += window_OnWindowShow;
        }

        public virtual void Update()
        {
            if (window.isVisible == false)
                return;

            if (Input.GetKeyUp(KeyCode.Mouse0) && closeWindowWhenClickedOutside && Vector2.Distance(Input.mousePosition, transform.position) > 50)
            {
                window.Hide();
            }
        }

        private void window_OnWindowShow()
        {
            // The context menu is being shown, update it
            if(positionAtMouse)
                transform.position = Input.mousePosition;

            if (menuOptions.Count == 1 && autoTriggerIfSingleAction)
            {
                // Do the default?
                menuOptions[0].useItemCallback(menuOptions[0].uiElement.item);
                window.Hide(); // No need for it anymore
            }
        }

        public virtual void ClearMenuOptions()
        {
            if (positionAtMouse)
                transform.position = Input.mousePosition;

            // Remove the old
            foreach (var item in menuOptions)
            {
                pool.Destroy(item.uiElement);
                //Destroy(item.uiElement.gameObject);
            }

            menuOptions.Clear();
        }

        public virtual void AddMenuOption(string name, InventoryItemBase item, UseItemCallback callback)
        {
            var obj = pool.Get();
            obj.transform.SetParent(container);

            obj.item = item;
            obj.text.text = name;

            obj.button.onClick.AddListener(() =>
            {
                callback(obj.item);
                window.Hide();
            });

            menuOptions.Add(new InventoryContextMenuOption(name, callback, obj));
        }
    }
}