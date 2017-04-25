using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Devdog.InventorySystem
{
    [AddComponentMenu("InventorySystem/UI Wrappers/UI Wrapper static")]
    public partial class InventoryUIItemWrapperStatic : InventoryUIItemWrapper
    {
        public override void Update()
        {
            //base.Update();
        }

        #region Button handler UI events

   
        public override void OnPointerUp(PointerEventData eventData)
        {

        }

        public virtual void PickupItem()
        {
        
        }

        #endregion


        public override void Repaint()
        {
            base.Repaint();

            if (item != null)
            {
                //itemName.text = item.name;
                itemName.color = item.rarity.color;
            }
            else
            {
                //itemName.text = string.Empty;
            }
        }

        public override void RepaintCooldown()
        {
            //base.RepaintCooldown();
        }
    }
}