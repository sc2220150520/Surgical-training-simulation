using System.Collections.Generic;
using System.Linq;
using System.Text;
using Devdog.InventorySystem.Models;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Devdog.InventorySystem.Editors
{
    public class EquipTypeEditor : InventorySystemEditorCrudBase<InventoryEquipType>
    {
        protected override List<InventoryEquipType> crudList
        {
            get { return new List<InventoryEquipType>(InventoryEditorUtil.selectedDatabase.equipTypes); }
            set { InventoryEditorUtil.selectedDatabase.equipTypes = value.ToArray(); }
        }

        private ReorderableList restrictionList;



        public EquipTypeEditor(string singleName, string pluralName, EditorWindow window)
            : base(singleName, pluralName, window)
        {}


        public override void EditItem(InventoryEquipType item)
        {
            base.EditItem(item);
            
            restrictionList = new ReorderableList(selectedItem.blockTypes, typeof(int), false, true, true, true);
            restrictionList.drawHeaderCallback += rect => GUI.Label(rect, "Restrictions");
            restrictionList.drawElementCallback += (rect, index, active, focused) =>
            {
                rect.height = 16;
                rect.y += 2;
                rect.width -= 30;
                rect.x += 30; // Some selection room

                if (crudList[selectedItem.blockTypes[index]] == selectedItem)
                {
                    var t = rect;
                    t.width = 200;

                    GUI.backgroundColor = Color.red;
                    EditorGUI.LabelField(t, "Can't block self");

                    rect.x += 205; // +5 for margin
                    rect.width -= 205;
                }

                selectedItem.blockTypes[index] = EditorGUI.Popup(rect, selectedItem.blockTypes[index], InventoryEditorUtil.equipTypesStrings);
                GUI.backgroundColor = Color.white;
            };
            restrictionList.onAddCallback += list =>
            {
                var l = new List<int>(selectedItem.blockTypes);
                l.Add(0);
                selectedItem.blockTypes = l.ToArray();
                list.list = selectedItem.blockTypes;

                window.Repaint();
            };
            restrictionList.onRemoveCallback += list =>
            {
                var l = new List<int>(selectedItem.blockTypes);
                l.RemoveAt(list.index);
                selectedItem.blockTypes = l.ToArray();
                list.list = selectedItem.blockTypes;

                window.Repaint();
            };
        }


        protected override bool MatchesSearch(InventoryEquipType item, string searchQuery)
        {
            string search = searchQuery.ToLower();
            return (item.ID.ToString().Contains(search) || item.name.ToLower().Contains(search));
        }

        protected override void CreateNewItem()
        {
            var item = new InventoryEquipType();
            item.ID = (crudList.Count > 0) ? crudList[crudList.Count - 1].ID + 1 : 0;
            AddItem(item, true);
        }

        protected override void DrawSidebarRow(InventoryEquipType item, int i)
        {
            //GUI.color = new Color(1.0f,1.0f,1.0f);
            BeginSidebarRow(item, i);

            DrawSidebarRowElement("#" + item.ID.ToString(), 40);
            DrawSidebarRowElement(item.name, 260);

            EndSidebarRow(item, i);
        }

        protected override void DrawDetail(InventoryEquipType item, int itemIndex)
        {
            EditorGUILayout.BeginVertical(InventoryEditorStyles.boxStyle);

            EditorGUILayout.LabelField("#" + item.ID);
            EditorGUILayout.Space();

            item.name = EditorGUILayout.TextField("Name", item.name);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical(InventoryEditorStyles.reorderableListStyle);

            EditorGUILayout.LabelField("You can force other fields to be empty when you set this. For example when equipping a greatsword, you might want to un-equip the shield.", InventoryEditorStyles.labelStyle);
            restrictionList.DoLayoutList();

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
            int lastID = 0;
            foreach (var item in crudList)
                item.ID = lastID++;

            EditorUtility.SetDirty(InventoryEditorUtil.selectedDatabase);
            GUI.changed = true;
        }
    }
}
