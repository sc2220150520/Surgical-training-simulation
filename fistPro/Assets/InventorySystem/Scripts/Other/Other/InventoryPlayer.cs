using System;
using UnityEngine;

namespace Devdog.InventorySystem
{
    public partial class InventoryPlayer : MonoBehaviour
    {


        public void Awake()
        {
            InventorySettingsManager.instance.playerObject = transform;
        }


        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (ShowObjectTriggerer.instance != null && ShowObjectTriggerer.instance.itemTriggerOnPlayerCollision)
            {
                var c = hit.gameObject.GetComponent<InventoryItemBase>();
                if (c != null)
                {
                    hit.gameObject.GetComponent<InventoryItemBase>().PickupItem();
                }
            }
        }

        //// For collider based character stuff
        public virtual void OnCollisionEnter(Collision col)
        {
            if (ShowObjectTriggerer.instance != null && ShowObjectTriggerer.instance.itemTriggerOnPlayerCollision)
            {
                var item = col.gameObject.GetComponent<ObjectTriggererItem>();
                if (item != null)
                    item.item.PickupItem();
            }
        }
    }
}
