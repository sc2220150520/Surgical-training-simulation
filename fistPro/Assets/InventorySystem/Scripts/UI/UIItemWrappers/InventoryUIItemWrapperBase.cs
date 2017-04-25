using System;
using System.Collections.Generic;
using Devdog.InventorySystem.Models;
using UnityEngine;

namespace Devdog.InventorySystem
{
    public abstract class InventoryUIItemWrapperBase : MonoBehaviour, IPoolableObject
    {
        /// <summary>
        /// Don't ever use this directly! Public because of the lack of friend classes...
        /// </summary>
        [NonSerialized]
        private InventoryItemBase _item;
        /// <summary>
        /// The item we're wrapping.
        /// </summary>
        public virtual InventoryItemBase item
        {
            get
            {
                return _item;
            }
            set
            {
                _item = value;
                if (_item != null && itemCollection != null && itemCollection.useReferences == false)
                {
                    _item.itemCollection = itemCollection;
                    _item.index = index;
                }
            }
        }


        [NonSerialized]
        private uint _index;
        /// <summary>
        /// Index of ItemCollection
        /// </summary>
        [HideInInspector]
        public virtual uint index
        {
            get
            {
                return _index;
            }
            set
            {
                _index = value;
                if (item != null && itemCollection && itemCollection.useReferences == false)
                    item.index = value;
            }
        }

        [NonSerialized]
        private ItemCollectionBase _itemCollection;
        /// <summary>
        /// The collection that holds this item.
        /// this == itemCollection[index]
        /// </summary>
        public virtual ItemCollectionBase itemCollection
        {
            get
            {
                return _itemCollection;
            }
            set
            {
                _itemCollection = value;
                if (item != null && itemCollection != null && itemCollection.useReferences == false)
                    item.itemCollection = value;
            }
        }

        public abstract Material material { get; set; }
    

        /// <summary>
        /// Repaint this object, make sure to only call it when absolutely necessary as it is a rather heavy method.
        /// </summary>
        public abstract void Repaint();


        public abstract void TriggerContextMenu();
        public abstract void TriggerUnstack();
        public abstract void TriggerDrop(bool useRaycast = true);
        public abstract void TriggerUse();

        public virtual void Reset()
        {
            //index = 0;
            //itemCollection = null;
        }
    }
}