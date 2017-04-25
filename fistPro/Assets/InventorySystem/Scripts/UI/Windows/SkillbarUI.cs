using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace Devdog.InventorySystem
{
    [System.Serializable]
    public class SkillbarSlot
    {
        public KeyCode[] keyCombination;
        public string name;
    }


    [AddComponentMenu("InventorySystem/Windows/Skillbar")]
    public partial class SkillbarUI : ItemCollectionBase
    {
        [Header("General")]
        public SkillbarSlot[] keys;
    
        /// <summary>
        /// The grayscale material used for references of which there are none in the referenced collection.
        /// </summary>
        [Header("Audio & Visuals")]
        public Material grayMaterial;

        /// <summary>
        /// The default icon material.
        /// </summary>
        public Material defaultMaterial;


        public override uint initialCollectionSize
        {
            get
            {
                return (uint)keys.Length;
            }
        }


        public override void Awake()
        {
            base.Awake();

            // Fill the container on startup, can add / remove later on
            for (uint i = 0; i < initialCollectionSize; i++)
            {
                ((InventoryUIItemWrapperKeyTrigger)items[i]).keyCombination = keys[i].name;
                items[i].Repaint(); // First time
            }

            foreach (var i in InventoryManager.GetLootToCollections())
            {
                // Item added to inventory
                i.OnAddedItem += (InventoryItemBase item, uint slot, uint amount) =>
                {
                    RepaintOfID(item.ID);
                };
                i.OnRemovedItem += (uint itemID, uint slot, uint amount) =>
                {
                    RepaintOfID(itemID);
                };
                i.OnUsedItem += (uint itemID, uint slot, uint amount) =>
                {
                    RepaintOfID(itemID);
                };
            }
        }
    
        public void Update()
        {
            if (InventoryUIUtility.isFocusedOnInput)
                return;

            for (int i = 0; i < keys.Length; i++)
            {
                uint keysDown = 0;
                foreach (var k in keys[i].keyCombination)
                {
                    if(Input.GetKeyDown(k))
                        keysDown++;
                }

                if(keysDown == keys[i].keyCombination.Length && keys[i].keyCombination.Length > 0)
                {
                    // All keys down
                    items[i].TriggerUse();
                    //items[i].Repaint();
                }
            }
        }


        private void RepaintOfID(uint id)
        {
            foreach (var item in items)
            {
                if(item.item != null && item.item.ID == id)
                {
                    item.Repaint();
                }
            }
        }


        public override bool SetItem(uint slot, InventoryItemBase item)
        {
            bool set = base.SetItem(slot, item);

            //items[slot].Repaint();
            //return set;
            return set;
        }


        protected override bool SwapSlots(uint slot1, ItemCollectionBase collection2, uint slot2, bool repaint = true, bool fireEvents = true)
        {
            return base.SwapSlots(slot1, collection2, slot2, true, fireEvents); // last var false for no repainting.
        }

        public override bool SwapOrMerge(uint slot1, ItemCollectionBase handler2, uint slot2, bool repaint = true)
        {
            // References cannot be merged, so all thats left is swapping
            bool swapped = SwapSlots(slot1, handler2, slot2, repaint);

            //if(swapped)
            //{
            //    items[slot1].Repaint();

            //    if (this == handler2)
            //        items[slot2].Repaint();

            //    return true;
            //}

            return swapped;
        }

        public override bool CanMergeSlots(uint slot1, ItemCollectionBase collection2, uint slot2)
        {
            return false;
        }
    }
}