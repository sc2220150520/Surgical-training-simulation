﻿using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Devdog.InventorySystem
{
    [AddComponentMenu("InventorySystem/UI Wrappers/UI Wrapper reference sum")]
    public partial class InventoryUIItemWrapperKeyTrigger : InventoryUIItemWrapper
    {
        public UnityEngine.UI.Text keyCombinationText;
        public string keyCombination;
        
        public override void Repaint()
        {
            base.Repaint();

            if (item != null)
            {
                if (keyCombinationText != null)
                    keyCombinationText.text = keyCombination;
            }
            else
            {
                if (keyCombinationText != null)
                    keyCombinationText.text = keyCombination;
            }
        }
    }
}