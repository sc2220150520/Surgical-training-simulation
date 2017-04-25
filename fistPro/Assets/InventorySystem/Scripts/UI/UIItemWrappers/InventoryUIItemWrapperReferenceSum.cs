﻿using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Devdog.InventorySystem
{
    [AddComponentMenu("InventorySystem/UI Wrappers/UI Wrapper reference sum")]
    public partial class InventoryUIItemWrapperReferenceSum : InventoryUIItemWrapperKeyTrigger
    {    
        public override void Repaint()
        {
            base.Repaint();

            if (item != null)
            {
                uint count = InventoryManager.GetItemCount(item.ID, false);
                amountText.text = count.ToString();
            
                if (count == 0)
                    icon.material = InventoryManager.instance.skillbar.grayMaterial;
                else
                    icon.material = InventoryManager.instance.skillbar.defaultMaterial;
            }
            else
            {
                amountText.text = string.Empty;
                icon.material = InventoryManager.instance.skillbar.defaultMaterial;
            }
        }

        public override void TriggerUse()
        {
            if (item == null)
                return;

            if (itemCollection.canUseFromCollection == false)
                return;

            var found = InventoryManager.Find(item.ID, false);
            if (found != null)
            {
                int used = found.Use();
                if (used >= 0)
                    found.itemCollection[found.index].Repaint();
            }
        }
    }
}