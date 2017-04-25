using System.Collections.Generic;
using System.Linq;
using System.Text;
using Devdog.InventorySystem.Models;
using UnityEditor;
using UnityEngine;

namespace Devdog.InventorySystem.Editors
{
    public class ItemPropertyEditor : InventorySystemEditorCrudBase<InventoryItemProperty>
    {
        protected override List<InventoryItemProperty> crudList
        {
            get { return new List<InventoryItemProperty>(InventoryEditorUtil.selectedDatabase.properties); }
            set { InventoryEditorUtil.selectedDatabase.properties = value.ToArray(); }
        }

        public ItemPropertyEditor(string singleName, string pluralName, EditorWindow window)
            : base(singleName, pluralName, window)
        { }

        protected override bool MatchesSearch(InventoryItemProperty item, string searchQuery)
        {
            string search = searchQuery.ToLower();
            return (item.ID.ToString().Contains(search) || item.key.ToLower().Contains(search));
        }

        protected override void CreateNewItem()
        {
            var item = new InventoryItemProperty();
            item.ID = (crudList.Count > 0) ? crudList[crudList.Count - 1].ID + 1 : 0;
            AddItem(item, true);
        }

        protected override void DrawSidebarRow(InventoryItemProperty item, int i)
        {
            //GUI.color = new Color(1.0f,1.0f,1.0f);
            BeginSidebarRow(item, i);

            DrawSidebarRowElement("#" + item.ID.ToString(), 40);
            DrawSidebarRowElement(item.key, 260);

            EndSidebarRow(item, i);
        }

        protected override void DrawDetail(InventoryItemProperty prop, int index)
        {
            EditorGUILayout.BeginVertical(InventoryEditorStyles.boxStyle);

            EditorGUILayout.LabelField("ID", prop.ID.ToString());
            EditorGUILayout.Space();

            prop.key = EditorGUILayout.TextField("Key", prop.key);
            EditorGUILayout.Space();

            prop.uiColor = EditorGUILayout.ColorField("UI Color", prop.uiColor);
            EditorGUILayout.Space();

            prop.showInUI = EditorGUILayout.Toggle("Show in UI", prop.showInUI);
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
            Debug.Log("Property ID's out of sync, force updating...");

            crudList = crudList.Where(o => o != null).ToList();
            int lastID = 0;
            foreach (var item in crudList)
                item.ID = lastID++;

            EditorUtility.SetDirty(InventoryEditorUtil.selectedDatabase);
            GUI.changed = true;
        }
    }
}
