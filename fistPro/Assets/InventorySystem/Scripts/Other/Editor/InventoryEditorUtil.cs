using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.CodeDom.Compiler;

namespace Devdog.InventorySystem.Editors
{
    public static class InventoryEditorUtil
    {
        public class InventoryEditorCategoriesAndRaritiesLookup
        {
            public string[] rarities;
            public Color[] rarityColors;

            public string[] categories;
        }



        public static InventoryItemDatabase selectedDatabase { get; set; }
        public static InventoryLangDatabase selectedLangDatabase { get; set; }




        #region Convenience stuff for UI elements

        public static string[] GetCraftingCategoriesStrings(bool forceCurrentDatabase)
        {
            var db = selectedDatabase;
            if (forceCurrentDatabase)
                db = GetItemDatabase(true, false);

            var craftingCategories = new string[db.craftingCategories.Length];
            for (int i = 0; i < db.craftingCategories.Length; i++)
                craftingCategories[i] = db.craftingCategories[i].name;

            return craftingCategories;
        }

        public static string[] propertiesStrings
        {
            get
            {
                if (selectedDatabase == null) return new string[0];
                var propertiesArr = new string[selectedDatabase.properties.Length];
                for (int i = 0; i < selectedDatabase.properties.Length; i++)
                    propertiesArr[i] = selectedDatabase.properties[i].key;

                return propertiesArr;
            }
        }
        public static string[] raritiesStrings
        {
            get
            {
                if (selectedDatabase == null) return new string[0];
                var rarities = new string[selectedDatabase.itemRaritys.Length];
                for (int i = 0; i < selectedDatabase.itemRaritys.Length; i++)
                    rarities[i] = selectedDatabase.itemRaritys[i].name;

                return rarities;
            }
        }

        public static UnityEngine.Color[] raritiesColors
        {
            get
            {
                if (selectedDatabase == null) return new Color[0];
                var colors = new UnityEngine.Color[selectedDatabase.itemRaritys.Length];
                for (int i = 0; i < selectedDatabase.itemRaritys.Length; i++)
                    colors[i] = selectedDatabase.itemRaritys[i].color;

                return colors;
            }
        }
        public static string[] itemCategoriesStrings
        {
            get
            {
                if (selectedDatabase == null) return new string[0];
                var categories = new string[selectedDatabase.itemCategories.Length];
                for (int i = 0; i < selectedDatabase.itemCategories.Length; i++)
                    categories[i] = selectedDatabase.itemCategories[i].name;

                return categories;
            }
        }

        public static string[] equipTypesStrings
        {
            get
            {
                if (selectedDatabase == null) return new string[0];
                var equipTypes = new string[selectedDatabase.equipTypes.Length];
                for (int i = 0; i < selectedDatabase.equipTypes.Length; i++)
                    equipTypes[i] = selectedDatabase.equipTypes[i].name;

                return equipTypes;
            }
        }

        public static string[] GetEquipTypesStrings(bool forceCurrentScene)
        {
            var db = GetItemDatabase(forceCurrentScene, false);
            var equipTypes = new string[db.equipTypes.Length];
            for (int i = 0; i < db.equipTypes.Length; i++)
                equipTypes[i] = db.equipTypes[i].name;

            return equipTypes;
        }


        #endregion




        //public static void Init()
        //{
        //    if (GetItemManager() != null)
        //        selectedDatabase = GetItemManager().itemDatabase;
        //}

        public static InventoryItemDatabase GetItemDatabase(bool forceFromCurrentScene, bool andSetAsDefault)
        {
            if (selectedDatabase != null && forceFromCurrentScene == false)
                return selectedDatabase;

            var manager = GameObject.FindObjectOfType<ItemManager>();
            if (manager != null)
            {
                if (andSetAsDefault)
                    selectedDatabase = manager.itemDatabase;

                return manager.itemDatabase;
            }

            return null;
        }
        public static InventoryLangDatabase GetLangDatabase()
        {
            if (selectedLangDatabase != null)
                return selectedLangDatabase;

            var manager = GameObject.FindObjectOfType<InventoryManager>();
            if (manager != null)
                return manager.lang;

            return null;
        }


        public static ItemManager GetItemManager()
        {
            return GameObject.FindObjectOfType<ItemManager>();
        }

        public static InventoryManager GetInventoryManager()
        {
            return GameObject.FindObjectOfType<InventoryManager>();
        }


        public static InventorySettingsManager GetSettingsManager()
        {
            // Already dropped error on GetItemManager()
            return GameObject.FindObjectOfType<InventorySettingsManager>();
        }


        public static void GetAllFieldsInherited(System.Type startType, List<FieldInfo> appendList)
        {
            if (startType == typeof(UnityEngine.MonoBehaviour) || startType == null)
                return;

            // Copied fields can be restricted with BindingFlags
            FieldInfo[] fields = startType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                appendList.Add(field);
            }

            // Keep going untill we hit UnityEngine.MonoBehaviour type.
            GetAllFieldsInherited(startType.BaseType, appendList);
        }


        #region UI helpers

        public static T SimpleObjectPicker<T>(string name, UnityEngine.Object o, bool sceneObjects, bool required) where T : UnityEngine.Object
        {
            if (o == null && required == true && GUI.enabled)
                GUI.color = Color.red;

            var item = (T)EditorGUILayout.ObjectField(name, o, typeof(T), sceneObjects);
            GUI.color = Color.white;

            return item;
        }
        public static T SimpleObjectPicker<T>(Rect rect, string name, UnityEngine.Object o, bool sceneObjects, bool required) where T : UnityEngine.Object
        {
            if (o == null && required == true && GUI.enabled)
                GUI.color = Color.red;

            var item = (T)EditorGUI.ObjectField(rect, name, o, typeof(T), sceneObjects);
            GUI.color = Color.white;

            return item;
        }

        public static void ErrorIfEmpty(System.Object o, string msg)
        {
            if (o == null)
            {
                EditorGUILayout.HelpBox(msg, MessageType.Error);
            }
        }

        public static void ErrorIfEmpty(UnityEngine.Object o, string msg)
        {
            if (o == null)
            {
                EditorGUILayout.HelpBox(msg, MessageType.Error);
            }
        }

        public static void ErrorIfEmpty(bool o, string msg)
        {
            if (o)
            {
                EditorGUILayout.HelpBox(msg, MessageType.Error);
            }
        }

        public static LayerMask LayerMaskField(string label, LayerMask selected, bool showSpecial)
        {

            List<string> layers = new List<string>();
            List<int> layerNumbers = new List<int>();

            string selectedLayers = "";

            for (int i = 0; i < 32; i++)
            {

                string layerName = LayerMask.LayerToName(i);

                if (layerName != "")
                {
                    if (selected == (selected | (1 << i)))
                    {

                        if (selectedLayers == "")
                        {
                            selectedLayers = layerName;
                        }
                        else
                        {
                            selectedLayers = "Mixed";
                        }
                    }
                }
            }

            //EventType lastEvent = Event.current.type;

            if (Event.current.type != EventType.MouseDown && Event.current.type != EventType.ExecuteCommand)
            {
                if (selected.value == 0)
                {
                    layers.Add("Nothing");
                }
                else if (selected.value == -1)
                {
                    layers.Add("Everything");
                }
                else
                {
                    layers.Add(selectedLayers);
                }
                layerNumbers.Add(-1);
            }

            if (showSpecial)
            {
                layers.Add((selected.value == 0 ? "[X] " : "     ") + "Nothing");
                layerNumbers.Add(-2);

                layers.Add((selected.value == -1 ? "[X] " : "     ") + "Everything");
                layerNumbers.Add(-3);
            }

            for (int i = 0; i < 32; i++)
            {

                string layerName = LayerMask.LayerToName(i);

                if (layerName != "")
                {
                    if (selected == (selected | (1 << i)))
                    {
                        layers.Add("[X] " + layerName);
                    }
                    else
                    {
                        layers.Add("     " + layerName);
                    }
                    layerNumbers.Add(i);
                }
            }

            bool preChange = GUI.changed;

            GUI.changed = false;

            int newSelected = 0;

            if (Event.current.type == EventType.MouseDown)
            {
                newSelected = -1;
            }

            newSelected = EditorGUILayout.Popup(label, newSelected, layers.ToArray(), EditorStyles.layerMaskField);

            if (GUI.changed && newSelected >= 0)
            {
                //newSelected -= 1;
                if (showSpecial && newSelected == 0)
                    selected = 0;
                else if (showSpecial && newSelected == 1)
                    selected = -1;
                else
                {

                    if (selected == (selected | (1 << layerNumbers[newSelected])))
                        selected &= ~(1 << layerNumbers[newSelected]);
                    else
                        selected = selected | (1 << layerNumbers[newSelected]);
                }
            }
            else
                GUI.changed = preChange;

            return selected;
        }


        #endregion

    }
}