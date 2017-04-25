using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using Devdog.InventorySystem.Models;

namespace Devdog.InventorySystem.Editors
{
    [CustomEditor(typeof(EquippableInventoryItem), true)]
    [CanEditMultipleObjects()]
    public class EquippableInventoryItemEditor : InventoryItemBaseEditor
    {
        protected SerializedProperty equipType;



        public override void OnEnable()
        {
            base.OnEnable();
            equipType = serializedObject.FindProperty("_equipType");
        }

        protected override void OnCustomInspectorGUI(params CustomOverrideProperty[] extraOverride)
        {

            var l = new List<CustomOverrideProperty>(extraOverride);
            l.Add(new CustomOverrideProperty("_equipType", () =>
            {

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Equip type", GUILayout.Width(EditorGUIUtility.labelWidth - 5));

                EditorGUILayout.BeginVertical();
                EditorGUILayout.HelpBox("Edit types in the Equip editor", MessageType.Info);
                equipType.intValue = EditorGUILayout.Popup(equipType.intValue, InventoryEditorUtil.equipTypesStrings);                
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
            }));

            base.OnCustomInspectorGUI(l.ToArray());
        }
    }
}