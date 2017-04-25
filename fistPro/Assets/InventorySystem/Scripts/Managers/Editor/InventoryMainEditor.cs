using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using Devdog.InventorySystem.Dialogs;
using Devdog.InventorySystem.Models;
#if PLY_GAME
using Devdog.InventorySystem.Integration.plyGame.Editors;
#endif


namespace Devdog.InventorySystem.Editors
{
    public class InventoryMainEditor : EditorWindow
    {
        private int toolbarIndex { get; set; }

        public static EmptyEditor itemEditor { get; set; }
        public static EmptyEditor equipEditor { get; set; }
        public static CraftingEmptyEditor craftingEditor { get; set; }
        public static LanguageEditor languageEditor { get; set; }
        public static SettingsEditor settingsEditor { get; set; }

        public static List<IInventorySystemEditorCrud> editors = new List<IInventorySystemEditorCrud>(8);
        

        protected string[] editorNames
        {
            get
            {
                string[] items = new string[editors.Count];
                for (int i = 0; i < editors.Count; i++)
                {
                    items[i] = editors[i].ToString();
                }

                return items;
            }
        }

        [MenuItem("Tools/InventorySystem/Main editor", false, -99)] // Always at the top
        public static void ShowWindow()
        {
            GetWindow<InventoryMainEditor>(false, "Main manager", true);
            //GetWindow(typeof(ItemManagerEditor), true, "Item manager", true);
        }

        private void OnEnable()
        {
            minSize = new Vector2(600.0f, 400.0f);
            toolbarIndex = 0;

            //if (InventoryEditorUtil.selectedDatabase == null)
            //    return;

            CreateEditors();
        }

        public virtual void CreateEditors()
        {
            editors.Clear();
            itemEditor = new EmptyEditor("Items editor", this);
            itemEditor.requiresDatabase = true;
            itemEditor.childEditors.Add(new ItemEditor("Item", "Items", this));
            itemEditor.childEditors.Add(new ItemCategoryEditor("Item category", "Item categories", this));
            itemEditor.childEditors.Add(new ItemPropertyEditor("Item property", "Item properties", this));
            itemEditor.childEditors.Add(new ItemRarityEditor("Item Rarity", "Item rarities", this));
            editors.Add(itemEditor);

            equipEditor = new EmptyEditor("Equip editor", this);
            equipEditor.requiresDatabase = true;
            equipEditor.childEditors.Add(new EquipEditor("Stats", this));
#if PLY_GAME
            equipEditor.childEditors.Add(new plyStatsEditor("Ply stats", this));
#endif
            equipEditor.childEditors.Add(new EquipTypeEditor("Equip type", "Equip types", this));
            editors.Add(equipEditor);

            craftingEditor = new CraftingEmptyEditor("Crafting editor", this);
            craftingEditor.requiresDatabase = true;
            editors.Add(craftingEditor);

            languageEditor = new LanguageEditor("Language editor");
            editors.Add(languageEditor);

            settingsEditor = new SettingsEditor("Settings editor");
            editors.Add(settingsEditor);
        }

        protected virtual void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal();
            GUI.color = Color.grey;
            if (GUILayout.Button("< DB", InventoryEditorStyles.toolbarStyle, GUILayout.Width(60)))
            {
                InventoryEditorUtil.selectedDatabase = null;
                toolbarIndex = 0;
            }
            GUI.color = Color.white;

            int before = toolbarIndex;
            toolbarIndex = GUILayout.Toolbar(toolbarIndex, editorNames, InventoryEditorStyles.toolbarStyle);
            if (before != toolbarIndex)
                editors[toolbarIndex].Focus();
            
            EditorGUILayout.EndHorizontal();
        }

        protected virtual bool CheckDatabase()
        {
            if (InventoryEditorUtil.selectedDatabase == null)
            {
                ShowItemDatabasePicker();
                return false;
            }

            return true;
        }

        protected virtual void ShowItemDatabasePicker()
        {
            EditorGUILayout.LabelField("Found the following databases in your project folder:", EditorStyles.largeLabel);

            var dbs = AssetDatabase.FindAssets("t:" + typeof(InventoryItemDatabase).Name);
            foreach (var db in dbs)
            {
                EditorGUILayout.BeginHorizontal();

                if (InventoryEditorUtil.GetItemDatabase(true, false) != null && AssetDatabase.GUIDToAssetPath(db) == AssetDatabase.GetAssetPath(InventoryEditorUtil.GetItemDatabase(true, false)))
                    GUI.color = Color.green;

                EditorGUILayout.LabelField(AssetDatabase.GUIDToAssetPath(db), InventoryEditorStyles.labelStyle);
                if (GUILayout.Button("Select", GUILayout.Width(100)))
                {
                    InventoryEditorUtil.selectedDatabase = (InventoryItemDatabase)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(db), typeof(InventoryItemDatabase));
                    OnEnable(); // Re-do editors
                }

                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();
            }

            if (dbs.Length == 0)
            {
                EditorGUILayout.LabelField("No Item databases found, first create one in your assets folder.");
            }
        }


        public void OnGUI()
        {
            DrawToolbar();

            if (CheckDatabase() == false && editors[toolbarIndex].requiresDatabase)
                return;

            // Draw the editor
            editors[toolbarIndex].Draw();

            if (GUI.changed && InventoryEditorUtil.selectedDatabase != null)
                EditorUtility.SetDirty(InventoryEditorUtil.selectedDatabase); // To make sure it gets saved.
        }
    }
}