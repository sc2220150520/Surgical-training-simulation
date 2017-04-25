using System.Collections.Generic;
using System.Linq;
using System.Text;
using Devdog.InventorySystem.Models;
using UnityEditor;
using UnityEngine;

namespace Devdog.InventorySystem.Editors
{
    public class ItemRarityEditor : InventorySystemEditorCrudBase<InventoryItemRarity>
    {

        protected override List<InventoryItemRarity> crudList
        {
            get { return new List<InventoryItemRarity>(InventoryEditorUtil.selectedDatabase.itemRaritys); }
            set { InventoryEditorUtil.selectedDatabase.itemRaritys = value.ToArray(); }
        }

        public ItemRarityEditor(string singleName, string pluralName, EditorWindow window)
            : base(singleName, pluralName, window)
        { }

        protected override bool MatchesSearch(InventoryItemRarity item, string searchQuery)
        {
            string search = searchQuery.ToLower();
            return (item.ID.ToString().Contains(search) || item.name.ToLower().Contains(search));
        }

        protected override void CreateNewItem()
        {
            var item = new InventoryItemRarity();
            item.ID = (crudList.Count > 0) ? crudList[crudList.Count - 1].ID + 1 : 0;
            AddItem(item, true);
        }

        protected override void DrawSidebarRow(InventoryItemRarity item, int i)
        {
            //GUI.color = new Color(1.0f,1.0f,1.0f);
            BeginSidebarRow(item, i);

            DrawSidebarRowElement("#" + item.ID.ToString(), 40);
            DrawSidebarRowElement(item.name, 260);

            EndSidebarRow(item, i);
        }

        protected override void DrawDetail(InventoryItemRarity item, int index)
        {
            EditorGUILayout.BeginVertical(InventoryEditorStyles.boxStyle);

            EditorGUILayout.LabelField("ID", item.ID.ToString());
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("The name of the rarity, is displayed in the tooltip in UI elements.", InventoryEditorStyles.infoStyle);
            item.name = EditorGUILayout.TextField("Rarity name", item.name);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("The color displayed in the UI.", InventoryEditorStyles.labelStyle);
            item.color = EditorGUILayout.ColorField("Rarity color", item.color);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();


            EditorGUILayout.LabelField("A custom object used when dropping this item like a pouch or chest.", InventoryEditorStyles.infoStyle);
            item.dropObject = (GameObject)EditorGUILayout.ObjectField("Drop object", item.dropObject, typeof(GameObject), false);
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
