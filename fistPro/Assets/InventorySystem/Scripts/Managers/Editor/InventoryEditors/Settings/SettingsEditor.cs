using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Devdog.InventorySystem.Editors
{
    public class SettingsEditor : IInventorySystemEditorCrud
    {
        private Editor editor { get; set; }
        private string name { get; set; }
        protected Vector2 scrollPosition;
        public bool requiresDatabase { get; set; }

        public SettingsEditor(string name)
        {
            this.name = name;
            this.requiresDatabase = false;
        }

        public void Focus()
        {
            if (InventoryEditorUtil.GetInventoryManager() != null)
                InventoryEditorUtil.selectedLangDatabase = InventoryEditorUtil.GetInventoryManager().lang;
        }
        
        public void Draw()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace(); // Center horizontally
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, InventoryEditorStyles.boxStyle, GUILayout.MaxWidth(1000));

            if (InventoryEditorUtil.GetSettingsManager() == null)
            {
                EditorGUILayout.LabelField("Settings are scene depended and require the Inventory managers.");
                EditorGUILayout.EndScrollView();
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                return;
            }

            var prevWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 250;
            if (editor == null)
            {
                editor = Editor.CreateEditor(InventoryEditorUtil.GetSettingsManager());
            }



            #region Path selector

            EditorGUILayout.LabelField("Items are saved as prefabs in a folder, this allows you to add components to objects and completely manage them.", InventoryEditorStyles.labelStyle);
            if (EditorPrefs.GetString("InventorySystem_ItemPrefabPath") == string.Empty)
                GUI.color = Color.red;

            EditorGUILayout.BeginHorizontal(InventoryEditorStyles.boxStyle);

            EditorGUILayout.LabelField("Inventory Item prefab folder: " + EditorPrefs.GetString("InventorySystem_ItemPrefabPath"));
            if (GUILayout.Button("Set path", GUILayout.Width(100)))
            {
                string path = EditorUtility.SaveFolderPanel("Choose a folder to save your item prefabs", "", "");


                EditorPrefs.SetString("InventorySystem_ItemPrefabPath", "Assets" + path.Replace(Application.dataPath, ""));
            }
            EditorGUILayout.EndHorizontal();

            GUI.color = Color.white;

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            #endregion


            editor.DrawDefaultInspector();


            EditorGUILayout.EndScrollView();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();


            EditorGUIUtility.labelWidth = prevWidth;


            if (GUI.changed)
                EditorUtility.SetDirty(InventoryEditorUtil.selectedLangDatabase); // To make sure it gets saved.
        }


        public override string ToString()
        {
            return name;
        }
    }
}
