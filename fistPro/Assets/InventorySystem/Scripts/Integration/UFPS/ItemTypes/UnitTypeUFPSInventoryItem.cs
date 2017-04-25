#if UFPS

using UnityEngine;
using System.Collections;
using Devdog.InventorySystem;
using Devdog.InventorySystem.Models;

namespace Devdog.InventorySystem.Integration.UFPS
{
    public partial class UnitTypeUFPSInventoryItem : EquippableInventoryItem
    {
        public vp_UnitType unitType;
        public uint unitAmount = 1;
        public AudioClip pickupSound;
        public bool useUFPSItemData = true;

        
        public bool addDirectlyToWeapon = true;

        public override string name
        {
            get
            {
                if (useUFPSItemData && unitType != null)
                    return unitType.DisplayName;

                return base.name;
            }
            set { base.name = value; }
        }

        public override string description
        {
            get
            {
                if (useUFPSItemData && unitType != null)
                    return unitType.Description;
                
                return base.description;
            }
            set { base.description = value; }
        }
        
        protected vp_PlayerEventHandler eventHandler
        {
            get
            {
                return InventorySettingsManager.instance.playerObject.GetComponent<vp_PlayerEventHandler>();
            }
        }
        protected vp_PlayerInventory ufpsInventory
        {
            get
            {
                return InventorySettingsManager.instance.playerObject.GetComponent<vp_PlayerInventory>();
            }
        }

        public void Awake()
        {
            OnUsedItem += to => AddToUFPSAmmo();
            OnUnEquipped += () => RemoveFromUFPSAmmo();

            eventHandler.Register(this);
        }

        public void OnDestroy()
        {
            eventHandler.Unregister(this);            
        }

        //protected virtual void OnStop_Attack()
        //{
        //    uint counter = 0;
        //    foreach (var item in itemCollection.items)
        //    {
        //        var i = item.item as UnitTypeUFPSInventoryItem;
        //        if (i != null && i.unitType == unitType)
        //        {
        //            // It's ammo!
        //            counter += i.currentStackSize;
        //        }
        //    }

        //    currentStackSize = counter;
        //    itemCollection[index].Repaint();
        //}


        protected virtual bool AddToUFPSAmmo()
        {
            return true;
            //return eventHandler.AddItem.Try(new object[] { unitType, (int)currentStackSize });
        }

        protected virtual bool RemoveFromUFPSAmmo()
        {
            return true;
            //return eventHandler.RemoveItem.Try(new object[] { unitType, (int)currentStackSize });
            //return true;
            //eventHandler.CurrentWeaponClipCount.Set(0);
            //eventHandler.CurrentWeaponAmmoCount.Set(0);
        }

        //public override int Use()
        //{
        //    if (addDirectlyToWeapon)
        //        return -1;

        //    int used = base.Use();
        //    if (used < 0)
        //        return used;

        //    // Un-Equipping
        //    bool added = AddToUFPSAmmo();
        //    if (added)
        //    {
        //        currentStackSize = 0; // TODO: Fix this, if UFPS accepts less units...
        //        NotifyItemUsed(currentStackSize, true);
        //        return (int)currentStackSize;
        //    }

        //    return 0;
        //}

        public override bool PickupItem(bool addToInventory = true)
        {
            currentStackSize = unitAmount;
            if (addDirectlyToWeapon)
            {
                // Add bullets directly to weapon
                //unitType.Space 
                bool added = AddToUFPSAmmo();
                if (added)
                {
                    if (pickupSound != null)
                        InventoryUIUtility.AudioPlayOneShot(pickupSound);

                    Destroy(gameObject); // Get rid of object
                    return true;
                }
            }
            else
            {
                bool pickedup = base.PickupItem(addToInventory); // Add to inventory instead.
                if (pickedup)
                {
                    if (pickupSound != null)
                        InventoryUIUtility.AudioPlayOneShot(pickupSound);

                    return true;
                }
            }
            
            return false;
        }
    }
}

#endif