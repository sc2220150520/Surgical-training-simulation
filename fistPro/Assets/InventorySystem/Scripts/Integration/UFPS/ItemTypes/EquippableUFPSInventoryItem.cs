#if UFPS

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Devdog.InventorySystem;
using Devdog.InventorySystem.Models;

namespace Devdog.InventorySystem.Integration.UFPS
{
    public partial class EquippableUFPSInventoryItem : EquippableInventoryItem
    {
        public bool useUFPSItemData = true;
        public vp_ItemType itemType;
        
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


        public override string name
        {
            get
            {
                if (useUFPSItemData && itemType != null)
                    return itemType.DisplayName;
                else
                    return base.name;
            }
            set { base.name = value; }
        }

        public override string description
        {
            get
            {
                if (useUFPSItemData && itemType != null)
                    return itemType.Description;
                else
                    return base.description;
            }
            set { base.description = value; }
        }


        protected ItemCollectionBase tempCollection { get; set; }

        public void Awake()
        {
            OnEquipped += to =>
            {
                tempCollection = itemCollection;
                //bool added = eventHandler.AddItem.Try(new object[] { itemType });
                bool added = ufpsInventory.TryGiveItem(itemType, 0);
                if (added)
                {
                    var bankType = itemType as vp_UnitBankType;
                    if (bankType != null)
                    {
                        SetAmmo(bankType);

                        tempCollection.OnAddedItem += AddedItemCollection;
                        tempCollection.OnRemovedItem += RemovedItemCollection;
                    }
                }


                eventHandler.Register(this); // Enable UFPS events
            };
            OnUnEquipped += () =>
            {
                //eventHandler.RemoveItem.Try(new object[] { itemType });
                ufpsInventory.TryRemoveItem(itemType, 0);

                var bankType = itemType as vp_UnitBankType;
                if (bankType != null)
                {
                    tempCollection.OnAddedItem -= AddedItemCollection;
                    tempCollection.OnRemovedItem -= RemovedItemCollection;
                }

                tempCollection = itemCollection;
                eventHandler.Unregister(this); // Disable UFPS events            
            };
        }


        //// UFPS EVENT
        //protected virtual void OnStop_Attack()
        //{
        //    UpdateAmmoCount();
        //}

        protected virtual void OnStop_Reload()
        {
            gameObject.SetActive(true);
            StartCoroutine(TestUpdateAmmoCount());
        }

        protected IEnumerator TestUpdateAmmoCount()
        {
            yield return new WaitForFixedUpdate();

            UpdateAmmoCount();
            gameObject.SetActive(false);
        }

        protected virtual void UpdateAmmoCount()
        {
            var bankType = itemType as vp_UnitBankType;
            if (bankType != null)
            {
                int count = ufpsInventory.GetUnitCount(bankType.Unit);
                foreach (var item in itemCollection.items)
                {
                    var i = item.item as UnitTypeUFPSInventoryItem;
                    if (i != null && i.unitType == bankType.Unit)
                    {
                        // It's ammo!
                        i.currentStackSize = (uint)count;
                        if (item.item.currentStackSize == 0)
                        {
                            Destroy(item.item.gameObject);
                            item.item = null;
                        }
                        item.Repaint();
                        break; // Currently only supporting 1 stack of same ammo type
                    }
                }
            }
        }

        private void RemovedItemCollection(uint itemid, uint slot, uint amount)
        {
            SetAmmo(itemType as vp_UnitBankType);
        }

        private void AddedItemCollection(InventoryItemBase item, uint slot, uint amount)
        {
            SetAmmo(itemType as vp_UnitBankType);
        }

        protected virtual void SetAmmo(vp_UnitBankType bankType)
        {
            ufpsInventory.TryRemoveUnits(bankType.Unit, 9999); // Remove all, then figure out how many we have
            uint bulletCount = 0;
            foreach (var item in itemCollection.items)
            {
                var i = item.item as UnitTypeUFPSInventoryItem;
                if (i != null && i.unitType == bankType.Unit)
                {
                    // It's ammo!
                    bulletCount += i.currentStackSize;
                    item.Repaint();
                    break; // Currently only supporting 1 stack of same ammo type
                }
            }

            ufpsInventory.TryGiveUnits(bankType.Unit, (int)bulletCount);
            //eventHandler.AddItem.Try(new object[] { bankType.Unit, (int)bulletCount });
        }


        public override LinkedList<InfoBox.Row[]> GetInfo()
        {
            var basic = base.GetInfo();
            basic.Remove(basic.First.Next);
            //basic.AddAfter(basic.First, new InfoBox.Row[]
            //{
            //    new InfoBox.Row("Ammo", )
            //});


            return basic;
        }
    }
}

#else

using UnityEngine;
using System.Collections;
using Devdog.InventorySystem;
using Devdog.InventorySystem.Models;

namespace Devdog.InventorySystem.Integration.UFPS
{
    public class EquippableUFPSInventoryItem : EquippableInventoryItem
    {
        // No UFPS, No fun stuff...
    }
}

#endif
