#if PLY_GAME

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using Devdog.InventorySystem.Editors;
using Devdog.InventorySystem.Integration.plyGame;
using Devdog.InventorySystem.Models;
using plyCommon;
using plyGame;
using plyGameEditor;
using UnityEditorInternal;

namespace Devdog.InventorySystem.Integration.plyGame.Editors
{
    [CustomEditor(typeof(plyGameEquippableInventoryItem), true)]
    [CanEditMultipleObjects()]
    public class plyGameEquippableInventoryItemEditor : EquippableInventoryItemEditor
    {
        protected SerializedProperty plyAttributesProperty;
        protected ReorderableList list;

        protected virtual List<ActorAttribute> plyAttributes
        {
            get
            {
                var assets = EdGlobal.GetDataAsset().assets;
                var attributesEditor = (ActorAttributesAsset)assets.FirstOrDefault(o => o.name == "attributes");
                if (attributesEditor == null)
                    return new List<ActorAttribute>(); // No attribute editor tab found??

                return attributesEditor.attributes;
            }
        }

        protected virtual string[] attributresStrings
        {
            get
            {
                var att = plyAttributes;

                string[] attributes = new string[att.Count];
                for (int i = 0; i < att.Count; i++)
                    attributes[i] = att[i].def.screenName;

                return attributes;
            }
        }


        public override void OnEnable()
        {
            base.OnEnable();
            plyAttributesProperty = serializedObject.FindProperty("plyAttributes");

            list = new ReorderableList(serializedObject, plyAttributesProperty, true, true, true, true);
            list.drawHeaderCallback += rect => EditorGUI.LabelField(rect, "plyGame attributes");
            list.drawElementCallback += (rect, index, active, focused) =>
            {
                rect.height = 16;
                rect.y += 3;

                var r2 = rect;
                r2.width /= 2;
                r2.width -= 5;
                r2.x += r2.width + 10;

                rect.width /= 2;

                var atts = plyAttributes;
                var t = (plyGameEquippableInventoryItem)target;
                
                int currentIndex = 0;
                for (int i = 0; i < atts.Count; i++)
                {
                    if (atts[i].id == t.plyAttributes[index].id)
                        currentIndex = i;
                }

                currentIndex = EditorGUI.Popup(rect, currentIndex, attributresStrings);
                t.plyAttributes[index].id = new UniqueID(atts[currentIndex].id.Value.ToString());
                t.plyAttributes[index].toAdd = EditorGUI.IntField(r2, "Add " + atts[currentIndex].def.screenName, t.plyAttributes[index].toAdd);

                if (GUI.changed)
                {
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(this); 
                }
            };
        }

        protected override void OnCustomInspectorGUI(params CustomOverrideProperty[] extraOverride)
        {
            var l = new List<CustomOverrideProperty>(extraOverride);
            l.Add(new CustomOverrideProperty("plyAttributes", () =>
            {
                //EditorGUILayout.EndVertical(); // Close box

                //EditorGUILayout.BeginVertical(InventoryEditorStyles.reorderableListStyle); // Continue
                EditorGUILayout.LabelField("Add value to attributes when this item is consumed.");
                list.DoLayoutList();
                //EditorGUILayout.EndVertical(); // Close box

                //EditorGUILayout.BeginVertical(); // Continue
            }));

            base.OnCustomInspectorGUI(l.ToArray());
        }
    }
}

#endif