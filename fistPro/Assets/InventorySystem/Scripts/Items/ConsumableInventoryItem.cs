using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Devdog.InventorySystem.Models;

namespace Devdog.InventorySystem
{
    /// <summary>
    /// Used to represent items that can be used by the player, and once used reduce 1 in stack size. This includes potions, food, scrolls, etc.
    /// </summary>
    public partial class ConsumableInventoryItem : InventoryItemBase
    {
        /// <summary>
        /// When the item is used, play this sound.
        /// </summary>
        public AudioClip audioClipWhenUsed;


        //[SerializeField]
        //[InventoryStat]
        //private uint _restoreHealth;
        //public uint restoreHealth {
        //    get {
        //        return _restoreHealth;
        //    }
        //    set {
        //        _restoreHealth = value;
        //    }
        //}

        //[SerializeField]
        //[InventoryStat]
        //private uint _restoreMana;
        //public uint restoreMana {
        //    get {
        //        return _restoreMana;
        //    }
        //    set {
        //        _restoreMana = value;
        //    }
        //}

        public override LinkedList<InfoBox.Row[]> GetInfo()
        {
            var basic = base.GetInfo();
            //basic.AddAfter(basic.First, new InfoBox.Row[]{
            //    new InfoBox.Row("Restore health", restoreHealth.ToString(), Color.green, Color.green),
            //    new InfoBox.Row("Restore mana", restoreMana.ToString(), Color.green, Color.green)
            //});


            return basic;
        }


        public override int Use()
        {
            int used = base.Use();
            if(used < 0)
                return used;

            // Do something with item
            currentStackSize--; // Remove 1
        
            // TODO: Add some health or something

            NotifyItemUsed(1, true);

            if (audioClipWhenUsed != null)
                InventoryUIUtility.AudioPlayOneShot(audioClipWhenUsed);
        
            return 1; // 1 item used
        }
    }
}