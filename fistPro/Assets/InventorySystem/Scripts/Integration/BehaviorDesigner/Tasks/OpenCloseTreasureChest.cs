﻿#if BEHAVIOR_DESIGNER

using UnityEngine;
using Devdog.InventorySystem;

namespace BehaviorDesigner.Runtime.Tasks.InventorySystem
{
    [TaskCategory("InventorySystem")]
    [TaskDescription("Use a given item.")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=127")]
    [TaskIcon("Assets/Behavior Designer/Third Party/Inventory Pro/Editor/InventoryProIcon.png")]
    public class OpenCloseTreasureChest : Action
    {
        public TreasureChest chest;
        public SharedBool open;

        public override TaskStatus OnUpdate()
        {
            if (chest == null) {
                return TaskStatus.Failure;
            }

            if (open.Value)
                chest.triggerer.Use();
            else
                chest.triggerer.UnUse();

            return TaskStatus.Success;
        }
        
        public override void OnReset()
        {
            chest = null;
            open = false;
        }
    }
}

#endif