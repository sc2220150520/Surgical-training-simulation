using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Devdog.InventorySystem.Editors
{
    public class InventoryItemPicker : InventoryObjectPickerBase<InventoryItemBase>
    {

        protected InventoryItemDatabase sceneDatabase;

        public static InventoryItemPicker Get(string title = "Item picker", Vector2 minSize = new Vector2())
        {
            var window = GetWindow<InventoryItemPicker>(true);
            window.windowTitle = title;
            window.minSize = minSize;
            window.isUtility = true;

            return window;
        }


        public virtual void Show(InventoryItemDatabase database)
        {
            if (database != null)
                Show(database.items);
            else
                Debug.LogWarning("Given database is null...");
        }

        public override void Show(IList<InventoryItemBase> objectsToPickFrom)
        {
            base.Show(objectsToPickFrom);

            sceneDatabase = InventoryEditorUtil.GetItemDatabase(true, false);
        }

        protected override IList<InventoryItemBase> FindObjects(bool searchProjectFolder)
        {
            IList<InventoryItemBase> objects = new List<InventoryItemBase>(1024);
            if (InventoryEditorUtil.GetItemDatabase(true, false) != null)
                objects = InventoryEditorUtil.GetItemDatabase(true, false).items.ToArray();

            return objects;
        }


        protected override bool MatchesSearch(InventoryItemBase obj, string search)
        {
            return obj.name.ToLower().Contains(search) || obj.description.ToLower().Contains(search) || 
                obj.ID.ToString().Contains(search) || obj.GetType().Name.ToLower().Contains(search) ||
            (InventoryEditorUtil.GetItemDatabase(true, false) != null && InventoryEditorUtil.GetItemDatabase(true, false).itemRaritys[obj._rarity].name.ToLower().Contains(search));
        }


        protected override void DrawObjectButton(InventoryItemBase item)
        {
            if (sceneDatabase != null)
            {
                var prevColor = GUI.color;
                GUI.color = InventoryEditorUtil.GetItemDatabase(true, false).itemRaritys[item._rarity].color;
                if (GUILayout.Button(InventoryEditorUtil.GetItemDatabase(true, false).itemRaritys[item._rarity].name, "ButtonLeft", GUILayout.Width(80)))
                {
                    searchQuery = InventoryEditorUtil.GetItemDatabase(true, false).itemRaritys[item._rarity].name;
                    Repaint();
                }

                GUI.color = prevColor;
            }

            if (GUILayout.Button("#" + item.ID + " " + item.name, "ButtonRight"))
                NotifyPickedObject(item);
        }
    }
}
