using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using UnityEditorInternal;

namespace Devdog.InventorySystem.Editors
{
    [CustomEditor(typeof(ItemCollectionBase), true)]
    [CanEditMultipleObjects()]
    public class ItemCollectionBaseEditor : InventoryEditorBase
    {
        private ItemCollectionBase item;
        private SerializedObject serializer;

        private SerializedProperty collectionName;
        private SerializedProperty restrictByWeight;
        private SerializedProperty restrictMaxWeight;
        private SerializedProperty itemButtonPrefab;

        private SerializedProperty items;
        private SerializedProperty useReferences;
        private SerializedProperty canDropFromCollection;
        private SerializedProperty canUseFromCollection;
        private SerializedProperty canDragInCollection;
        private SerializedProperty canPutItemsInCollection;
        private SerializedProperty canStackItemsInCollection;
        private SerializedProperty manuallyDefineCollection;
        private SerializedProperty container;
        private SerializedProperty onlyAllowTypes;

        private static ItemManager itemManager;

        // Script selector
        private ReorderableList manualItemsList;
        private ReorderableList onlyAllowTypesList;

        public override void OnEnable()
        {
            base.OnEnable();

            item = (ItemCollectionBase)target;
            //serializer = new SerializedObject(target);
            serializer = serializedObject;
            
            collectionName = serializer.FindProperty("collectionName");
            restrictByWeight = serializer.FindProperty("restrictByWeight");
            restrictMaxWeight = serializer.FindProperty("restrictMaxWeight");
            itemButtonPrefab = serializer.FindProperty("itemButtonPrefab");

            items = serializer.FindProperty("_items");
            useReferences = serializer.FindProperty("useReferences");
            canDropFromCollection = serializer.FindProperty("canDropFromCollection");
            canUseFromCollection = serializer.FindProperty("canUseFromCollection");
            canDragInCollection = serializer.FindProperty("canDragInCollection");
            canPutItemsInCollection = serializer.FindProperty("canPutItemsInCollection");
            canStackItemsInCollection = serializer.FindProperty("canStackItemsInCollection");
            manuallyDefineCollection = serializer.FindProperty("manuallyDefineCollection");
            container = serializer.FindProperty("container");
            onlyAllowTypes = serializer.FindProperty("_onlyAllowTypes");

            itemManager = Editor.FindObjectOfType<ItemManager>();
            if (itemManager == null)
                Debug.LogError("No item manager found in scene, cannot edit item.");


            manualItemsList = new ReorderableList(serializer, items, true, true, true, true);
            manualItemsList.drawHeaderCallback += rect =>
            {
                EditorGUI.LabelField(rect, "Select items");
            };
            manualItemsList.drawElementCallback += (rect, index, active, focused) =>
            {
                rect.height = 16;
                rect.y += 2;

                EditorGUI.PropertyField(rect, items.GetArrayElementAtIndex(index));
            };

            onlyAllowTypesList = new ReorderableList(serializer, onlyAllowTypes, false, true, true, true);
            onlyAllowTypesList.drawHeaderCallback += rect =>
            {
                EditorGUI.LabelField(rect, "Restrict by type, leave empty to allow all types");
            };
            onlyAllowTypesList.drawElementCallback += (rect, index, active, focused) =>
            {
                rect.height = 16;
                rect.y += 2;

                var r = rect;
                r.width -= 60;

                EditorGUI.LabelField(r, (item.onlyAllowTypes[index] != null) ? item.onlyAllowTypes[index].FullName : "(NOT SET)");

                var r2 = rect;
                r2.width = 60;
                r2.height = 14;
                r2.x += r.width;
                if (GUI.Button(r2, "Set"))
                {
                    var typePicker = InventoryItemTypePicker.Get();
                    typePicker.Show(InventoryEditorUtil.GetItemDatabase(true, false));
                    typePicker.OnPickObject += type =>
                    {
                        item._onlyAllowTypes[index] = type.AssemblyQualifiedName;
                        GUI.changed = true; // To save..
                        EditorUtility.SetDirty(target);
                        serializer.ApplyModifiedProperties();
                        Repaint();
                    };
                }
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            OnCustomInspectorGUI();
            serializedObject.ApplyModifiedProperties();
        }


        protected override void OnCustomInspectorGUI(params CustomOverrideProperty[] extraOverride)
        {
            base.OnCustomInspectorGUI(extraOverride);
            
            GUILayout.Label("General settings", InventoryEditorStyles.titleStyle);
            EditorGUILayout.BeginVertical(InventoryEditorStyles.boxStyle);

            //GUILayout.Label("General", InventoryEditorStyles.titleStyle);
            EditorGUILayout.PropertyField(collectionName);
            EditorGUILayout.PropertyField(useReferences);

            GUILayout.Label("UI", InventoryEditorStyles.titleStyle);
            EditorGUILayout.PropertyField(itemButtonPrefab);
            EditorGUILayout.PropertyField(container);
            //EditorGUILayout.PropertyField(onlyAllowTypes);


            #region Manually define collection

            EditorGUILayout.PropertyField(manuallyDefineCollection);
            if (manuallyDefineCollection.boolValue)
                manualItemsList.DoLayoutList();

            #endregion

            EditorGUILayout.EndVertical();


            // Draws remaining items
            GUILayout.Label("Collection specific", InventoryEditorStyles.titleStyle);
            EditorGUILayout.BeginVertical(InventoryEditorStyles.boxStyle);

            var doNotDrawList = new List<string>()
            {
                "m_Script",
                "collectionName",
                "restrictByWeight",
                "restrictMaxWeight",
                "itemButtonPrefab",
                "_items",
                "useReferences",
                "canDropFromCollection",
                "canUseFromCollection",
                "canDragInCollection",
                "canPutItemsInCollection",
                "canStackItemsInCollection",
                "manuallyDefineCollection",
                "container",
                "onlyAllowTypes"
            };

            foreach (var extra in extraOverride)
            {
                extra.action();
                doNotDrawList.Add(extra.serializedName);
            }

            DrawPropertiesExcluding(serializer, doNotDrawList.ToArray());
            EditorGUILayout.EndVertical();


            GUILayout.Label("Restrictions", InventoryEditorStyles.titleStyle);
            EditorGUILayout.BeginVertical(InventoryEditorStyles.boxStyle);

            //GUILayout.Label("Restrict by type", InventoryEditorStyles.titleStyle);
            onlyAllowTypesList.DoLayoutList();

            GUILayout.Label("Other", InventoryEditorStyles.titleStyle);
            EditorGUILayout.PropertyField(restrictByWeight);
            if (restrictByWeight.boolValue)
                EditorGUILayout.PropertyField(restrictMaxWeight);

            EditorGUILayout.PropertyField(canDropFromCollection);
            EditorGUILayout.PropertyField(canUseFromCollection);
            EditorGUILayout.PropertyField(canDragInCollection);
            EditorGUILayout.PropertyField(canPutItemsInCollection);
            EditorGUILayout.PropertyField(canStackItemsInCollection);
            EditorGUILayout.EndVertical();

        }
    }
}