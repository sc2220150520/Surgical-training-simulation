using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace Devdog.InventorySystem
{
    public partial class InventoryUIUtility
    {
        public class InventoryUIDragLookup
        {
            public int startIndex = -1;
            public ItemCollectionBase startItemCollection;

            public int endIndex = -1;
            public ItemCollectionBase endItemCollection;

            public bool endOnButton
            {
                get
                {
                    return endItemCollection != null;
                }
            }
        }


        #region Variables 

        private static InventoryUIItemWrapper draggingItem;
        public static InventoryUIItemWrapper hoveringItem { get; private set; }
        public static bool isDraggingItem
        {
            get
            {
                return draggingItem != null;
            }
        }

        public static bool clickedUIElement
        {
            get
            {
                return EventSystem.current.IsPointerOverGameObject();
            }
        }


        public static bool isFocusedOnInput
        {
            get
            {
                if (EventSystem.current.currentSelectedGameObject != null)
                    if (EventSystem.current.currentSelectedGameObject.GetComponent<UnityEngine.UI.InputField>() != null)
                        return true;

                return false;
            }
        }

        #endregion



        public static InventoryUIDragLookup BeginDrag(InventoryUIItemWrapper toDrag, uint startIndex, ItemCollectionBase collection, PointerEventData eventData)
        {
            if (draggingItem != null)
            {
                Debug.LogWarning("Item still attached to cursor, can only drag one item at a time", draggingItem.gameObject);
                return null; // Can only drag one item at a time
            }

            if (eventData.button != PointerEventData.InputButton.Left)
                return null;


            draggingItem = toDrag;
            //draggingButtonCollection = collection;

            // Canvas group allows object to ignore raycasts.
            CanvasGroup group = draggingItem.gameObject.GetComponent<CanvasGroup>();
            if(group == null)
                group = draggingItem.gameObject.AddComponent<CanvasGroup>();

            group.blocksRaycasts = false; // Allows rays to go through so we can hover over the empty slots.
            group.interactable = false;

            var lookup = new InventoryUIDragLookup();
            lookup.startIndex = (int)startIndex;
            lookup.startItemCollection = collection;

            return lookup;
        }

        public static void Drag(InventoryUIItemWrapper toDrag, uint startSlot, ItemCollectionBase handler, PointerEventData eventData)
        {
            if(eventData.button == PointerEventData.InputButton.Left)
                draggingItem.transform.position = new Vector3(eventData.position.x, eventData.position.y, 0.0f);
        }

        public static InventoryUIDragLookup EndDrag(InventoryUIItemWrapper toDrag, uint startSlot, ItemCollectionBase handler, PointerEventData eventData)
        {
            if(eventData.button == PointerEventData.InputButton.Left)
            {
                var lookup = new InventoryUIDragLookup();
                lookup.startIndex = (int)draggingItem.index;
                lookup.startItemCollection = draggingItem.itemCollection;

                if (hoveringItem != null)
                {
                    lookup.endIndex = (int)hoveringItem.index;
                    lookup.endItemCollection = hoveringItem.itemCollection;
                }

                Object.Destroy(draggingItem.gameObject); // No longer need it

                draggingItem = null;
                //draggingButtonCollection = null;

                return lookup;
            }

            return null;
        }

        /// <summary>
        /// When the cursor enters an item
        /// </summary>
        public static void EnterItem(InventoryUIItemWrapper item, uint slot, ItemCollectionBase handler, PointerEventData eventData)
        {
            hoveringItem = item;
            //hoveringItemCollection = handler;
        }

        /// <summary>
        /// When the cursor exits an item
        /// </summary>
        /// <param name="item"></param>
        /// <param name="slot">The slot is the IButtonHandler index not the inventory index.</param>
        /// <param name="handler"></param>
        /// <param name="eventData"></param>
        public static void ExitItem(InventoryUIItemWrapper item, uint slot, ItemCollectionBase handler, PointerEventData eventData)
        {
            hoveringItem = null;
            //hoveringItemCollection = null;
        }
    
        /// <summary>
        /// Plays an audio clip, only use this for the UI, it is not pooled so performance isn't superb.
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="volume"></param>
        public static void AudioPlayOneShot(AudioClip clip, float volume = 1.0f)
        {
            var obj = new GameObject("TEMP_AUDIO_SOURCE_UI");
            var source = obj.AddComponent<AudioSource>();

            source.PlayOneShot(clip, volume);
            Object.Destroy(obj, clip.length + 0.1f);
        }
    }
}