﻿#if PLAYMAKER

using UnityEngine;
using System.Collections;
using Devdog.InventorySystem.Dialogs;
using Devdog.InventorySystem.Models;
using HutongGames.PlayMaker;

namespace Devdog.InventorySystem.Integration.PlayMaker
{

    [ActionCategory("InventorySystem")]
    [HutongGames.PlayMaker.Tooltip("Show a notice message")]
    public class UIShowNoticeMessage : FsmStateAction
    {
        public FsmString title;
        public FsmString message;
        public FsmColor color;
        public NoticeDuration duration = NoticeDuration.Medium;

        [Note("If the message is regarding an object you can specify said object here, and use {0} for the item name and {1} for the item description inside your message.")]
        //[UIHint(UIHint.Variable)]
        public InventoryItemBase item;


        public override void Reset()
        {

        }

        public override void OnEnter()
        {
            var m = new InventoryNoticeMessage(title.Value, message.Value, duration, color.Value);

            if (item != null)
                m.Show(item.name, item.description);
            else
                m.Show();
        }
    }
}

#endif