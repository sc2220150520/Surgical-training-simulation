﻿#if PLAYMAKER

using UnityEngine;
using System.Collections;
using HutongGames.PlayMaker;


namespace Devdog.InventorySystem.Integration.PlayMaker
{
    [ActionCategory("InventorySystem")]
    [HutongGames.PlayMaker.Tooltip("Use a given item.")]
    public class UseThisItem : FsmStateAction
    {
       
        public override void Reset()
        {

        }

        public override void OnEnter()
        {
            this.Owner.SendMessage("Use", SendMessageOptions.RequireReceiver);
            Finish();
        }
    }
}

#endif