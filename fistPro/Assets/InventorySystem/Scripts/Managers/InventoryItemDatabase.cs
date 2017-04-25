using System;
using System.Collections;
using System.Collections.Generic;
using Devdog.InventorySystem.Models;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
#endif

namespace Devdog.InventorySystem
{
    [System.Serializable]
    public partial class InventoryItemDatabase : ScriptableObject
    {
#if UNITY_EDITOR
        [MenuItem("Assets/Create/InventorySystem/Item database", false, -5)]
        protected static void CreatePrefab()
        {
            var selected = Selection.activeObject;
            string path = AssetDatabase.GetAssetPath(selected);

            if (path == "")
            {
                path = "Assets";
            }

            string pathName = AssetDatabase.GenerateUniqueAssetPath(path + "/ItemDatabase.asset");
            var o = ScriptableObject.CreateInstance<InventoryItemDatabase>();
            AssetDatabase.CreateAsset(o, pathName);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = o;
            //AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath(pathName, typeof(InventoryItemDatabase)));

            Debug.Log("Item database created at path " + pathName);
        }
#endif

        [Header("Items")]
        public InventoryItemBase[] items = new InventoryItemBase[0];
        public InventoryItemRarity[] itemRaritys = new InventoryItemRarity[0];
        public InventoryItemCategory[] itemCategories = new InventoryItemCategory[] { new InventoryItemCategory() { ID = 0, name = "None", cooldownTime = 0.0f } };
        public InventoryItemProperty[] properties = new InventoryItemProperty[0];
        
        [Header("Equipment")]
        public InventoryEquipStat[] equipStats = new InventoryEquipStat[0];
        public string[] equipStatTypes = new string[0];
        public InventoryEquipType[] equipTypes = new InventoryEquipType[0];
        
        [Header("Crafting")]
        public InventoryCraftingCategory[] craftingCategories = new InventoryCraftingCategory[0];
    }
}
