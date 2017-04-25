// /**
// * Written By Joris Huijbregts
// * Some legal stuff --- Copyright!
// */
using UnityEngine;
using System.Collections;
using Devdog.InventorySystem.Dialogs;
using Devdog.InventorySystem.Models;
using UnityEngine.EventSystems;

namespace Devdog.InventorySystem
{
    public enum MobileUIActions
    {
        LongPress,
        DoubleTap,
        SingleTap
    }


    [AddComponentMenu("InventorySystem/Managers/Settings manager")]
    [RequireComponent(typeof(ItemManager))]
    [RequireComponent(typeof(InventoryManager))]
    public partial class InventorySettingsManager : MonoBehaviour
    {
        /// <summary>
        /// The default icon used when a slot is empty.
        /// </summary>
        [Header("UI")]
        //[InventoryRequired]
        public Sprite defaultSlotIcon;

        /// <summary>
        /// The default UI Item.
        /// </summary>
        [InventoryRequired]
        public GameObject itemButtonPrefab;

        /// <summary>
        /// Use the context menu or not?
        /// </summary>
        public bool useContextMenu;

        /// <summary>
        /// The context menu used if the itemButtonPrefab supports it.
        /// </summary>
        public InventoryContextMenu contextMenu;

        /// <summary>
        /// The root of the GUI.
        /// </summary>
        [InventoryRequired]
        public RectTransform guiRoot;

        /// <summary>
        /// The default collection sorter, sorts by category.
        /// </summary>
        public ICollectionSorter collectionSorter;


        
        /// <summary>
        /// The player / camera / whatever represents the player. Used to calculate the distance to objects.
        /// </summary>
        [Header("Dropping")]
        //[InventoryRequired]
        //[HideInInspector]
        public Transform playerObject;

        /// <summary>
        /// Drop an object at the cursor / mouse position position
        /// If false, the item will be dropped using the offset vector.
        /// </summary>
        public bool dropAtMousePosition = true;

        /// <summary>
        /// The offset by which an item is dropped into the world
        /// Only used when dropAtMousePosition = false;
        /// </summary>
        public Vector3 dropOffsetVector = new Vector3(0.0f, 0.0f, 0.0f);

        /// <summary>
        /// The maxmimum distance an item can be dropped
        /// </summary>
        public float maxDropDistance = 10.0f;

        /// <summary>
        /// Layers to consider when ray casting for item dropping.
        /// </summary>
        public LayerMask layersWhenDropping;

        /// <summary>
        /// When dropping an item, do you want it placed precisely on the ground?
        /// </summary>
        public bool dropItemRaycastToGround = false;



        /// <summary>
        /// The default confirmation dialog box used to verify actions with the user.
        /// </summary>
        [Header("Dialogs")]
        public ConfirmationDialog confirmationDialog;

        /// <summary>
        /// Do you want to show a confirmation dialog, when an item is being dropped?
        /// </summary>
        public bool showConfirmationDialogWhenDroppingItem = true;


        public int _showConfirmationDialogMinRarity = 0;
        /// <summary>
        /// The minimal rarity an item should have before the dialog appears?
        /// </summary>
        public InventoryItemRarity showConfirmationDialogMinRarity
        {
            get
            {
                return ItemManager.instance.itemRaritys[_showConfirmationDialogMinRarity];
            }
        }

        /// <summary>
        /// The default buying / selling dialog
        /// </summary>
        public ItemBuySellDialog buySellDialog;

        /// <summary>
        /// The default confirmation dialog box used to verify actions with the user.
        /// </summary>
        public IntValDialog intValDialog;


        /// <summary>
        /// If true a dialog is displayed, if false the stack will be split in half.
        /// </summary>
        public bool useUnstackDialog = true;

        /// <summary>
        /// Disables these UI elements while the dialog is active, useful when you want to disable the inventory when the user sees a dialog box.
        /// </summary>
        [Tooltip("Disables these UI elements while the dialog is active, useful when you want to disable the inventory when the user sees a dialog box")]
        public RectTransform[] disabledWhileDialogActive = new RectTransform[0];

        public string defaultDialogPositiveButtonText = "Yes";
        public string defaultDialogNegativeButtonText = "No";


        /// <summary>
        /// The key combination used to unstack an item
        /// </summary>
        [Header("User actions")]
        public PointerEventData.InputButton unstackItemButton = PointerEventData.InputButton.Right;
    
        /// <summary>
        /// Key used to unstack the item, forms a combination with the unstackItemButton
        /// </summary>
        public KeyCode unstackItemKey = KeyCode.LeftShift;

        /// <summary>
        /// The button used to "use" an item.
        /// </summary>
        public PointerEventData.InputButton useItemButton = PointerEventData.InputButton.Right;

        /// <summary>
        /// Trigger the context menu using, the following button
        /// </summary>
        public PointerEventData.InputButton triggerContextMenuButton = PointerEventData.InputButton.Right;

        /// <summary>
        /// The currency formatter formats a value to a string. If you want custom formatting, simply write your own and set it.
        /// </summary>
        public ICurrencyFormatter currencyFormatter;

        /// <summary>
        /// The distance items can be used, and windows should be auto closed.
        /// </summary>
        public float useObjectDistance = 10.0f;


        /// <summary>
        /// How long should the LongPress time be on mobile devices?
        /// </summary>
        [Header("Mobile actions")]
        public float mobileLongPressTime = 0.3f;

        /// <summary>
        /// How long 2 taps can be appart from one another to trigger the double tap event.
        /// </summary>
        public float mobileDoubleTapTime = 0.4f;

        /// <summary>
        /// Action used to unstack on mobile devices
        /// </summary>
        public MobileUIActions mobileUnstackItemKey = MobileUIActions.LongPress;

        /// <summary>
        /// Action to use an item on mobile devices
        /// </summary>
        public MobileUIActions mobileUseItemButton = MobileUIActions.DoubleTap;

        /// <summary>
        /// The default formatter used to 
        /// </summary>
        [Header("Other")]
        public CharacterStatFormatterBase defaultCharacterStatFormatter;


        private bool _isUIWorldSpace;
        public bool isUIWorldSpace
        {
            get
            {
                return _isUIWorldSpace;
            }
        }


        private static InventorySettingsManager _instance;
        public static InventorySettingsManager instance
        {
            get
            {
                return _instance;
            }
        }


        public void Awake()
        {
            _instance = this;
            collectionSorter = new BasicCollectionSorter();
            currencyFormatter = new BasicCurrencyFormatter();

            _isUIWorldSpace = guiRoot.GetComponent<Canvas>().renderMode == RenderMode.WorldSpace;
        }
    }
}