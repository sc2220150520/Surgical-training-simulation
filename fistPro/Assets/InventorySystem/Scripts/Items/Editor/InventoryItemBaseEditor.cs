using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using Devdog.InventorySystem.Models;

namespace Devdog.InventorySystem.Editors
{
    [CustomEditor(typeof(InventoryItemBase), true)]
    [CanEditMultipleObjects()]
    public class InventoryItemBaseEditor : InventoryEditorBase
    {
        //private InventoryItemBase item;

        protected SerializedProperty id;
        protected SerializedProperty itemName; // Name is used by Editor.name...
        protected SerializedProperty description;
        protected SerializedProperty properties;
        protected SerializedProperty useCategoryCooldown;
        protected SerializedProperty category;
        protected SerializedProperty icon;
        protected SerializedProperty weight;
        protected SerializedProperty requiredLevel;
        protected SerializedProperty rarity;
        protected SerializedProperty buyPrice;
        protected SerializedProperty sellPrice;
        protected SerializedProperty isDroppable;
        protected SerializedProperty isSellable;
        protected SerializedProperty isStorable;
        protected SerializedProperty maxStackSize;
        protected SerializedProperty cooldownTime;


        private ReorderableList propertiesList { get; set; }

        public override void OnEnable()
        {
            base.OnEnable();

            id = serializedObject.FindProperty("_id");
            itemName = serializedObject.FindProperty("_name");
            description = serializedObject.FindProperty("_description");
            properties = serializedObject.FindProperty("_properties");
            useCategoryCooldown = serializedObject.FindProperty("_useCategoryCooldown");
            category = serializedObject.FindProperty("_category");
            icon = serializedObject.FindProperty("_icon");
            weight = serializedObject.FindProperty("_weight");
            requiredLevel = serializedObject.FindProperty("_requiredLevel");
            rarity = serializedObject.FindProperty("_rarity");
            buyPrice = serializedObject.FindProperty("_buyPrice");
            sellPrice = serializedObject.FindProperty("_sellPrice");
            isDroppable = serializedObject.FindProperty("_isDroppable");
            isSellable = serializedObject.FindProperty("_isSellable");
            isStorable = serializedObject.FindProperty("_isStorable");
            maxStackSize = serializedObject.FindProperty("_maxStackSize");
            cooldownTime = serializedObject.FindProperty("_cooldownTime");


            var t = (InventoryItemBase)target;

            propertiesList = new ReorderableList(serializedObject, properties, true, true, true, true);
            propertiesList.drawHeaderCallback += rect => GUI.Label(rect, "Item properties");
            propertiesList.drawElementCallback += (rect, index, active, focused) =>
            {
                rect.height = 16;
                rect.y += 2;

                var popupRect = rect;
                popupRect.width /= 2;
                popupRect.width -= 5; // Some spacing

                var i = t.properties[index];

                // Variables
                i.ID = EditorGUI.Popup(popupRect, i.ID, InventoryEditorUtil.propertiesStrings);
                if (i.ID >= InventoryEditorUtil.selectedDatabase.properties.Length)
                    i.ID = InventoryEditorUtil.selectedDatabase.properties.Length - 1;

                popupRect.x += popupRect.width;
                popupRect.x += 5;

                i.ID = Mathf.Max(i.ID, 0);
                i.value = EditorGUI.TextField(popupRect, i.value);

                // Changed something, copy property data
                if (GUI.changed)
                {
                    // We're actually copying the values, can't edit source, because of value
                    var db = InventoryEditorUtil.selectedDatabase;
                    if (db.properties.Length > 0)
                    {
                        i.key = db.properties[i.ID].key;
                        i.showInUI = db.properties[i.ID].showInUI;
                        i.uiColor = db.properties[i.ID].uiColor;
                    }

                    EditorUtility.SetDirty(target);
                    serializedObject.ApplyModifiedProperties();
                    Repaint();
                }
            };
            propertiesList.onAddCallback += (list) =>
            {
                var l = new List<InventoryItemProperty>(t.properties);
                l.Add(new InventoryItemProperty());
                t.properties = l.ToArray();

                GUI.changed = true; // To save..
                EditorUtility.SetDirty(target);
                serializedObject.ApplyModifiedProperties();
                Repaint();
            };
        }


        protected override void OnCustomInspectorGUI(params CustomOverrideProperty[] extraOverride)
        {
            base.OnCustomInspectorGUI(extraOverride);

            serializedObject.Update();
            overrides = extraOverride;

            if (InventoryEditorUtil.selectedDatabase == null)
                InventoryEditorUtil.GetItemDatabase(false, true);

            if (InventoryEditorUtil.selectedDatabase == null)
            {
                EditorGUILayout.HelpBox("No item database set, can't modify item!", MessageType.Error);
                return;
            }

            if(buyPrice.intValue < sellPrice.intValue)
                EditorGUILayout.HelpBox("Buy price is lower than the sell price, are you sure?", MessageType.Warning);
                
            // Can't go below 0
            if (cooldownTime.floatValue < 0.0f)
                cooldownTime.floatValue = 0.0f;

            // Just a safety precaution
            if (rarity.intValue >= InventoryEditorUtil.raritiesStrings.Length)
                rarity.intValue = 0;

            // Just a safety precaution
            if (category.intValue >= InventoryEditorUtil.itemCategoriesStrings.Length)
                category.intValue = 0;



            if (InventoryEditorUtil.GetItemDatabase(true, false).items.FirstOrDefault(o => AssetDatabase.GetAssetPath(o) == AssetDatabase.GetAssetPath(target)) == null)
            {
                EditorGUILayout.HelpBox("Note that the item you're editing is not in this scene's database " + InventoryEditorUtil.selectedDatabase.name, MessageType.Warning);


                //GUI.color = Color.yellow;
                //if (GUILayout.Button("Add to database"))
                //{
                //    var l = new List<InventoryItemBase>(InventoryEditorUtil.GetItemDatabase().items);
                //    l.Add((InventoryItemBase)target);
                //    InventoryEditorUtil.GetItemDatabase().items = l.ToArray();
                //}
                //GUI.color = Color.white;
            }

            var excludeList = new List<string>()
            {
                "m_Script",
                "_id",
                "_name",
                "_description",
                "_properties",
                "_category",
                "_useCategoryCooldown",
                "_icon",
                "_weight",
                "_requiredLevel",
                "_rarity",
                "_buyPrice",
                "_sellPrice",
                "_isDroppable",
                "_isSellable",
                "_isStorable",
                "_maxStackSize",
                "_cooldownTime"
            };

            GUILayout.Label("Default", InventoryEditorStyles.titleStyle);
            EditorGUILayout.BeginVertical(InventoryEditorStyles.boxStyle);
            if (FindOverride("_id") != null)
                GUI.enabled = false;

            EditorGUILayout.LabelField("ID: ", id.intValue.ToString());
            GUI.enabled = true;

            if (FindOverride("_name") != null)
                GUI.enabled = false;

            EditorGUILayout.PropertyField(itemName);
            
            GUI.enabled = true;

            if (FindOverride("_description") != null)
                GUI.enabled = false;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Description", GUILayout.Width(EditorGUIUtility.labelWidth - 5));
            EditorGUILayout.BeginVertical();
            EditorGUILayout.HelpBox("Note, that you can use rich text like <b>asd</b> to write bold text and <i>Potato</i> to write italic text.", MessageType.Info);
            description.stringValue = EditorGUILayout.TextArea(description.stringValue, InventoryEditorStyles.richTextArea);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();


            GUI.enabled = true;

            EditorGUILayout.PropertyField(icon);

            EditorGUILayout.EndVertical();


            // Draws remaining items
            GUILayout.Label("Item specific", InventoryEditorStyles.titleStyle);
            EditorGUILayout.BeginVertical(InventoryEditorStyles.boxStyle);

            foreach (var item in extraOverride)
            {
                if (item.action != null)
                    item.action();

                excludeList.Add(item.serializedName);
            }

            DrawPropertiesExcluding(serializedObject, excludeList.ToArray());
            EditorGUILayout.EndVertical();

            #region Properties

            GUILayout.Label("Item properties", InventoryEditorStyles.titleStyle);
            GUILayout.Label("You can create properties in the Item editor / Item property editor");
            
            EditorGUILayout.BeginVertical(InventoryEditorStyles.reorderableListStyle);
            propertiesList.DoLayoutList();
            EditorGUILayout.EndVertical();

            #endregion


            GUILayout.Label("Behavior", InventoryEditorStyles.titleStyle);
            EditorGUILayout.BeginVertical(InventoryEditorStyles.boxStyle);

            GUILayout.Label("Category", InventoryEditorStyles.titleStyle);
            if (InventoryEditorUtil.raritiesColors.Length > 0)
                GUI.color = InventoryEditorUtil.raritiesColors[rarity.intValue];

            rarity.intValue = EditorGUILayout.Popup("Rarity", rarity.intValue, InventoryEditorUtil.raritiesStrings);
            GUI.color = Color.white;

            category.intValue = EditorGUILayout.Popup("Category", category.intValue, InventoryEditorUtil.itemCategoriesStrings);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(useCategoryCooldown);
            if (useCategoryCooldown.boolValue)
                EditorGUILayout.LabelField(string.Format("({0} seconds)", InventoryEditorUtil.selectedDatabase.itemCategories[category == null ? 0 : category.intValue].cooldownTime));

            EditorGUILayout.EndHorizontal();
            if (useCategoryCooldown.boolValue == false)
                EditorGUILayout.PropertyField(cooldownTime);


            GUILayout.Label("Buying & Selling", InventoryEditorStyles.titleStyle);
            EditorGUILayout.PropertyField(buyPrice);
            EditorGUILayout.PropertyField(sellPrice);

            GUILayout.Label("Restrictions", InventoryEditorStyles.titleStyle);
            EditorGUILayout.PropertyField(isDroppable);
            EditorGUILayout.PropertyField(isSellable);
            EditorGUILayout.PropertyField(isStorable);
            EditorGUILayout.PropertyField(maxStackSize);
            EditorGUILayout.PropertyField(weight);
            EditorGUILayout.PropertyField(requiredLevel);


            //GUILayout.Label("Audio & Visuals", InventoryEditorStyles.titleStyle);
            //EditorGUILayout.PropertyField(icon);
            

            //EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();


            serializedObject.ApplyModifiedProperties();
        }
    }
}