#if PLAYMAKER

using UnityEngine;
using System.Collections;
using HutongGames.PlayMaker;


namespace Devdog.InventorySystem.Integration.PlayMaker
{
    [ActionCategory("InventorySystem")]
    [HutongGames.PlayMaker.Tooltip("Use a given item.")]
    public class OpenCloseTreasureChest : FsmStateAction
    {
        public TreasureChest chest;
        public FsmBool open;
        
        public override void Reset()
        {

        }

        public override void OnEnter()
        {
            if (open.Value)
                chest.triggerer.Use();
            else
                chest.triggerer.UnUse();

            Finish();
        }
    }
}

#endif