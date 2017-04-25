#if PLAYMAKER

using UnityEngine;
using System.Collections;
using Devdog.InventorySystem.Dialogs;
using Devdog.InventorySystem.Models;
using HutongGames.PlayMaker;

namespace Devdog.InventorySystem.Integration.PlayMaker
{

    [ActionCategory("InventorySystem")]
    [HutongGames.PlayMaker.Tooltip("Show or hide a dialog")]
    public class UIShowHideUIWindow : FsmStateAction
    {
        public FsmBool show;
        public UIWindow window;

        public override void Reset()
        {

        }

        public override void OnEnter()
        {
            if (show.Value)
                window.Show();
            else
                window.Hide();
        }
    }
}

#endif