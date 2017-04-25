using UnityEngine;
using System.Collections;
using Devdog.InventorySystem.Models;

namespace Devdog.InventorySystem
{
    [AddComponentMenu("InventorySystem/Managers/Item manager")]
    [RequireComponent(typeof(InventorySettingsManager))]
    [RequireComponent(typeof(InventoryManager))]
    public partial class ItemManager : MonoBehaviour
    {
        public InventoryItemDatabase itemDatabase;


        #region Convenience properties

        public InventoryItemBase[] items { get { return itemDatabase.items; } set { itemDatabase.items = value; }}
        public InventoryItemRarity[] itemRaritys { get { return itemDatabase.itemRaritys; } set { itemDatabase.itemRaritys = value; } }
        public InventoryItemCategory[] itemCategories { get { return itemDatabase.itemCategories; } set { itemDatabase.itemCategories = value; } }
        public InventoryItemProperty[] properties { get { return itemDatabase.properties; } set { itemDatabase.properties = value; } }
        public InventoryEquipStat[] equipStats { get { return itemDatabase.equipStats; } set { itemDatabase.equipStats = value; } }
        public string[] equipStatTypes { get { return itemDatabase.equipStatTypes; } set { itemDatabase.equipStatTypes = value; } }
        public InventoryEquipType[] equipTypes { get { return itemDatabase.equipTypes; } set { itemDatabase.equipTypes = value; } }
        public InventoryCraftingCategory[] craftingCategories { get { return itemDatabase.craftingCategories; } set { itemDatabase.craftingCategories = value; } }

        #endregion

        private static ItemManager _instance;
        public static ItemManager instance
        {
            get
            {
                return _instance;
            }
        }



        public void Awake()
        {
            _instance = this;
        }

    }
}

// using UnityEditor;