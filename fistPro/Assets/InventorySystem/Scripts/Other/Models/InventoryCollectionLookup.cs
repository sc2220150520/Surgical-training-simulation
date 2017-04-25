using System;
using System.Collections.Generic;
using UnityEngine;

namespace Devdog.InventorySystem.Models
{
    /// <summary>
    /// A lookup for the InvenetoryManager to keep track of inventories.
    /// !! Does not require serialization, it's runtime only
    /// </summary>
    public partial class InventoryCollectionLookup
    {
        public ItemCollectionBase collection { get; set; }

        /// <summary>
        /// Range of 0-100
        /// </summary>
        public int priority { get; set; }
    

        public InventoryCollectionLookup(ItemCollectionBase collection, int priority)
        {
            this.collection = collection;
            this.priority = priority;
        }
    }
}