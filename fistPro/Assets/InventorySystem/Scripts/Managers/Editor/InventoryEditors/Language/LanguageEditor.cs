using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Devdog.InventorySystem.Editors
{
    public class LanguageEditor : IInventorySystemEditorCrud
    {
        private Editor editor { get; set; }
        private string name { get; set; }
        private Vector2 scrollPosition { get; set; }
        public bool requiresDatabase { get; set; }

        public LanguageEditor(string name)
        {
            this.name = name;
            this.requiresDatabase = false;
        }

        public void Focus()
        {
            if (InventoryEditorUtil.GetInventoryManager() != null)
                InventoryEditorUtil.selectedLangDatabase = InventoryEditorUtil.GetInventoryManager().lang;
        }


        public virtual void ShowLangDatabasePicker()
        {
            EditorGUILayout.LabelField("Found the following databases in your project folder:", EditorStyles.largeLabel);

            var dbs = AssetDatabase.FindAssets("t:" + typeof(InventoryLangDatabase).Name);
            foreach (var db in dbs)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(AssetDatabase.GUIDToAssetPath(db), InventoryEditorStyles.labelStyle);
                if (GUILayout.Button("Select", GUILayout.Width(100)))
                    InventoryEditorUtil.selectedLangDatabase = (InventoryLangDatabase)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(db), typeof(InventoryLangDatabase));

                EditorGUILayout.EndHorizontal();
            }

            if (dbs.Length == 0)
            {
                EditorGUILayout.LabelField("No Lang databases found, first create one in your assets folder.");
            }
        }



        public void Draw()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace(); // Center horizontally
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, InventoryEditorStyles.boxStyle, GUILayout.MaxWidth(1000));

            if (InventoryEditorUtil.selectedLangDatabase == null)
            {
                ShowLangDatabasePicker();
                EditorGUILayout.EndScrollView();
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                return;
            }
            
            if (editor == null)
            {
                editor = Editor.CreateEditor(InventoryEditorUtil.selectedLangDatabase);
            }
            
            editor.DrawDefaultInspector();

            EditorGUILayout.EndScrollView();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (GUI.changed)
                EditorUtility.SetDirty(InventoryEditorUtil.selectedLangDatabase); // To make sure it gets saved.
        }


        public override string ToString()
        {
            return name;
        }
    }
}
