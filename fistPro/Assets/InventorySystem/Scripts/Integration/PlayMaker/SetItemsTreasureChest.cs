#if PLAYMAKER

using UnityEngine;
using System.Collections;
using HutongGames.PlayMaker;


namespace Devdog.InventorySystem.Integration.PlayMaker
{
    [ActionCategory("InventorySystem")]
    [HutongGames.PlayMaker.Tooltip("Set items on a treasure chest, replaces the old items that were in the collection before it.")]
    public class SetItemsTreasureChest : FsmStateAction
    {
        public InventoryItemBase[] items;
        public TreasureChest chest;

        public override void Reset()
        {

        }

        public override void OnEnter()
        {
            chest.items = items;
            Finish();
        }
    }
}

#endif