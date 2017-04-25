using System.Collections.Generic;
using System.Linq;
using System.Text;
using Devdog.InventorySystem.Models;
using UnityEditor;
using UnityEngine;

namespace Devdog.InventorySystem.Editors
{
    public class ItemCategoryEditor : InventorySystemEditorCrudBase<InventoryItemCategory>
    {
        protected override List<InventoryItemCategory> crudList
        {
            get { return new List<InventoryItemCategory>(InventoryEditorUtil.selectedDatabase.itemCategories); }
            set { InventoryEditorUtil.selectedDatabase.itemCategories = value.ToArray(); }
        }

        public Editor itemEditorInspector;




        public ItemCategoryEditor(string singleName, string pluralName, EditorWindow window)
            : base(singleName, pluralName, window)
        {
            
        }

        protected override bool MatchesSearch(InventoryItemCategory item, string searchQuery)
        {
            string search = searchQuery.ToLower();
            return (item.ID.ToString().Contains(search) || item.name.ToLower().Contains(search));
        }

        protected override void CreateNewItem()
        {
            var item = new InventoryItemCategory();
            item.ID = (crudList.Count > 0) ? crudList[crudList.Count - 1].ID + 1 : 0;
            AddItem(item, true);
        }

        public override void RemoveItem(int i)
        {
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(InventoryEditorUtil.selectedDatabase.items[i]));
            base.RemoveItem(i);
        }

        protected override void DrawSidebarRow(InventoryItemCategory item, int i)
        {
            //GUI.color = new Color(1.0f,1.0f,1.0f);
            BeginSidebarRow(item, i);

            DrawSidebarRowElement("#" + item.ID.ToString(), 40);
            DrawSidebarRowElement(item.name, 260);
            
            EndSidebarRow(item, i);
        }

        protected override void DrawDetail(InventoryItemCategory item, int index)
        {
            EditorGUILayout.BeginVertical(InventoryEditorStyles.boxStyle);

            EditorGUILayout.LabelField("ID", item.ID.ToString());
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("The name of the category, is displayed in the tooltip in UI elements.", InventoryEditorStyles.labelStyle);
            item.name = EditorGUILayout.TextField("Category name", item.name);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();


            EditorGUILayout.LabelField("Items can have a 'global' cooldown. Whenever an item of this category is used, all items with the same category will go into cooldown.", InventoryEditorStyles.labelStyle);
            GUI.color = Color.yellow;
            EditorGUILayout.LabelField("Note, that items can individually override the timeout.", InventoryEditorStyles.labelStyle);
            GUI.color = Color.white;
            item.cooldownTime = EditorGUILayout.Slider("Cooldown time (seconds)", item.cooldownTime, 0.0f, 999.0f);
            EditorGUILayout.Space();

            EditorGUILayout.EndVertical();
        }

        protected override bool IDsOutOfSync()
        {
            uint next = 0;
            foreach (var item in crudList)
            {
                if (item == null || item.ID != next)
                    return true;

                next++;
            }

            return false;
        }

        protected override void SyncIDs()
        {
            Debug.Log("Item ID's out of sync, force updating...");

            crudList = crudList.Where(o => o != null).ToList();
            uint lastID = 0;
            foreach (var item in crudList)
                item.ID = lastID++;
            
            EditorUtility.SetDirty(InventoryEditorUtil.selectedDatabase);
            GUI.changed = true;
        }
    }
}
