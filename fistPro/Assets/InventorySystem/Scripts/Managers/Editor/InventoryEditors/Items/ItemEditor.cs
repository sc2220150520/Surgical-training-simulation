using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Devdog.InventorySystem.Editors
{
    public class ItemEditor : InventorySystemEditorCrudBase<InventoryItemBase>
    {
        protected override List<InventoryItemBase> crudList
        {
            get { return new List<InventoryItemBase>(InventoryEditorUtil.selectedDatabase.items); }
            set { InventoryEditorUtil.selectedDatabase.items = value.ToArray(); }
        }

        public Editor itemEditorInspector;




        public ItemEditor(string singleName, string pluralName, EditorWindow window)
            : base(singleName, pluralName, window)
        {
            if (selectedItem != null)
                itemEditorInspector = Editor.CreateEditor(selectedItem);
        }

        protected override bool MatchesSearch(InventoryItemBase item, string searchQuery)
        {
            string search = searchQuery.ToLower();
            return (item.name.ToLower().Contains(search) || item.description.ToLower().Contains(search) ||
                item.ID.ToString().Contains(search) || item.GetType().Name.ToLower().Contains(search));
        }

        protected override void CreateNewItem()
        {
            var picker = EditorWindow.GetWindow<InventoryItemTypePicker>(true);
            picker.Show(InventoryEditorUtil.selectedDatabase);

            picker.OnPickObject += (type) =>
            {
                string prefabPath = EditorPrefs.GetString("InventorySystem_ItemPrefabPath") + "/item_" + System.DateTime.Now.ToFileTimeUtc() + "_PFB.prefab";

                var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                var prefab = PrefabUtility.CreatePrefab(prefabPath, obj);

                AssetDatabase.SetLabels(prefab, new string[] { "InventoryItemPrefab" });

                var comp = (InventoryItemBase)prefab.AddComponent(type);
                comp.ID = crudList.Count == 0 ? 0 : crudList[crudList.Count - 1].ID + 1;
                Object.DestroyImmediate(obj);

                AddItem(comp, true);
            };
        }



        public override void RemoveItem(int i)
        {
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(InventoryEditorUtil.selectedDatabase.items[i]));
            base.RemoveItem(i);
        }

        public override void EditItem(InventoryItemBase item)
        {
            base.EditItem(item);

            //var asset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(item), typeof(InventoryItemBase)) as InventoryItemBase;
            //EditorGUIUtility.PingObject(asset);

            if (selectedItem != null)
                itemEditorInspector = Editor.CreateEditor(selectedItem);
        }

        protected override void DrawSidebarRow(InventoryItemBase item, int i)
        {
            //GUI.color = new Color(1.0f,1.0f,1.0f);
            BeginSidebarRow(item, i, 280);

            DrawSidebarRowElement("#" + item.ID.ToString(), 40);
            DrawSidebarRowElement(item.name, 120);
            DrawSidebarRowElement(item.GetType().Name.ToString().Replace("InventoryItem", ""), 120);
            bool t = DrawSidebarRowElementToggle(true, "", "VisibilityToggle", 20);
            if (t == false) // User clicked view icon
                AssetDatabase.OpenAsset(selectedItem);

            EndSidebarRow(item, i);
        }

        protected override void DrawDetail(InventoryItemBase item, int index)
        {
            InventoryEditorUtil.ErrorIfEmpty(EditorPrefs.GetString("InventorySystem_ItemPrefabPath") == string.Empty, "Inventory item prefab folder is not set, items cannot be saved! Please go to settings and define the Inventory item prefab folder.");
            if (EditorPrefs.GetString("InventorySystem_ItemPrefabPath") == string.Empty)
            {
                canCreateItems = false;
                return;
            }
            canCreateItems = true;

            GUILayout.Label("Use the inspector if you want to add custom components.", InventoryEditorStyles.titleStyle);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            itemEditorInspector.OnInspectorGUI();

            string newName = "Item_" + (string.IsNullOrEmpty(item.name) ? string.Empty : item.name.ToLower().Replace(" ", "_")) + "_#" + item.ID + "_" + InventoryEditorUtil.selectedDatabase.name + "_PFB";
            if (AssetDatabase.GetAssetPath(item).EndsWith(newName + ".prefab") == false)
            {
                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(item), newName);
            }
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
            {
                item.ID = lastID++;
                EditorUtility.SetDirty(item);
            }

            GUI.changed = true;
            EditorUtility.SetDirty(InventoryEditorUtil.selectedDatabase);
        }
    }
}
