using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Devdog.InventorySystem.Models;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

namespace Devdog.InventorySystem.Editors
{
    public class EquipEditor : IInventorySystemEditorCrud
    {

        public virtual InventoryItemDatabase currentItemDatabase
        {
            get { return InventoryEditorUtil.selectedDatabase; }
        }
        public virtual InventoryLangDatabase currentLangDatabase
        {
            get { return InventoryEditorUtil.GetLangDatabase(); }
        }

        public string name { get; set; }
        public EditorWindow window { get; protected set; }
        private ReorderableList typeList { get; set; }
        private ReorderableList resultList { get; set; }
        private Vector2 statsScrollPos;
        public bool requiresDatabase { get; set; }

        public EquipEditor(string name, EditorWindow window)
        {
            this.name = name;
            this.window = window;
            this.requiresDatabase = true;

            typeList = new ReorderableList(InventoryEditorUtil.selectedDatabase != null ? InventoryEditorUtil.selectedDatabase.equipStatTypes : new string[]{}, typeof(System.Type), false, true, true, true);
            typeList.drawHeaderCallback += rect => EditorGUI.LabelField(rect, "Types to scan");
            typeList.drawElementCallback += (rect, index, active, focused) =>
            {
                rect.height = 16;
                rect.y += 2;

                var r = rect;
                r.width -= 60;

                var statTypes = InventoryEditorUtil.selectedDatabase.equipStatTypes;

                EditorGUI.LabelField(r, (string.IsNullOrEmpty(statTypes[index]) == false && System.Type.GetType(statTypes[index]) != null) ? System.Type.GetType(statTypes[index]).FullName : "(NOT SET)");

                var r2 = rect;
                r2.width = 60;
                r2.height = 14;
                r2.x += r.width;
                if (GUI.Button(r2, "Set"))
                {
                    var typePicker = InventoryItemTypePicker.Get();
                    typePicker.Show(InventoryEditorUtil.selectedDatabase);
                    typePicker.OnPickObject += type =>
                    {
                        statTypes[index] = type.AssemblyQualifiedName;
                        window.Repaint();
                        GUI.changed = true; // To save..
                    };
                }
            };
            typeList.onAddCallback += list =>
            {
                var l = new List<string>(InventoryEditorUtil.selectedDatabase.equipStatTypes);
                l.Add(null);
                InventoryEditorUtil.selectedDatabase.equipStatTypes = l.ToArray();
                list.list = InventoryEditorUtil.selectedDatabase.equipStatTypes;

                window.Repaint();
            };
            typeList.onRemoveCallback += list =>
            {
                var l = new List<string>(InventoryEditorUtil.selectedDatabase.equipStatTypes);
                l.RemoveAt(list.index);
                InventoryEditorUtil.selectedDatabase.equipStatTypes = l.ToArray();
                list.list = InventoryEditorUtil.selectedDatabase.equipStatTypes;

                window.Repaint();
            };


            resultList = new ReorderableList(InventoryEditorUtil.selectedDatabase != null ? InventoryEditorUtil.selectedDatabase.equipStats : new InventoryEquipStat[]{}, typeof(InventoryEquipStat), true, true, false, false);
            resultList.drawHeaderCallback += rect =>
            {
                var r = rect;
                r.width = 40;
                r.x += 15; // Little offset on the start

                EditorGUI.LabelField(r, "Show");


                var r2 = rect;
                r2.width -= r.width;
                r2.x += r.width + 20;
                r2.width /= 4.2f;
                EditorGUI.LabelField(r2, "From type");

                r2.x += r2.width;
                EditorGUI.LabelField(r2, "Display name");

                r2.x += r2.width;
                EditorGUI.LabelField(r2, "Category");

                r2.x += r2.width;
                //r2.width *= 2 - 50;
                EditorGUI.LabelField(r2, "Formatter");
            };
            //resultList.elementHeight = 30;
            resultList.drawElementCallback += (rect, index, active, focused) =>
            {
                rect.height = 16;
                rect.y += 2;

                var stat = InventoryEditorUtil.selectedDatabase.equipStats[index];

                var r = rect;
                r.width = 40;
                stat.show = EditorGUI.Toggle(r, stat.show);

                GUI.enabled = stat.show;

                var r2 = rect;
                r2.width -= r.width;
                r2.x += r.width + 5;
                r2.width /= 4.2f;
                EditorGUI.LabelField(r2, stat.fieldInfoNameVisual);

                r2.x += r2.width + 5;
                stat.name = EditorGUI.TextField(r2, stat.name);

                r2.x += r2.width + 5;
                stat.category = EditorGUI.TextField(r2, stat.category);

                r2.x += r2.width + 5;
                stat.formatter = (CharacterStatFormatterBase)EditorGUI.ObjectField(r2, "", stat.formatter, typeof(CharacterStatFormatterBase), false);

                GUI.enabled = true;
            };
        }

        public void OnEnable()
        {

        }

        public virtual void Focus()
        {
            if (InventoryEditorUtil.selectedDatabase == null)
                return;

            typeList.list = InventoryEditorUtil.selectedDatabase.equipStatTypes;
            resultList.list = InventoryEditorUtil.selectedDatabase.equipStats;
        }


        public virtual void Draw()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace(); // Center horizontally
            EditorGUILayout.BeginVertical(InventoryEditorStyles.boxStyle, GUILayout.MaxWidth(1000));
            statsScrollPos = EditorGUILayout.BeginScrollView(statsScrollPos);
            

            #region Types picker

            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("Step 1: Pick the item types that you want to scan for character stats.", InventoryEditorStyles.titleStyle);
            EditorGUILayout.LabelField("Note: You only have to pick the top level classes.", InventoryEditorStyles.labelStyle);
            EditorGUILayout.LabelField("If EquippableInventoryItem extends from InventoryItemBase, you don't need to pick base. The system handles inheritance.", InventoryEditorStyles.labelStyle);


            EditorGUILayout.BeginVertical(InventoryEditorStyles.reorderableListStyle);
            typeList.DoLayoutList();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();

            #endregion

            EditorGUILayout.LabelField("Step 2: Scan the types for stats.", InventoryEditorStyles.titleStyle);
            if (GUILayout.Button("Scan types"))
            {
                var oldList = new List<InventoryEquipStat>(InventoryEditorUtil.selectedDatabase.equipStats);
                var displayList = new List<InventoryEquipStat>(64);
                foreach (var type in InventoryEditorUtil.selectedDatabase.equipStatTypes)
                {
                    var fields = new List<FieldInfo>();
                    InventoryEditorUtil.GetAllFieldsInherited(System.Type.GetType(type, true), fields);
                    foreach (var field in fields)
                    {
                        var attr = field.GetCustomAttributes(typeof(InventoryStatAttribute), true);
                        if (attr.Length > 0)
                        {
                            var m = (InventoryStatAttribute)attr[0];

                            var old = oldList.FindAll(o => o.fieldInfoNameVisual == field.ReflectedType.Name + "." + field.Name);
                            if (old.Count == 0)
                            {
                                displayList.Add(new InventoryEquipStat() { name = m.name, typeName = type, fieldInfoName = field.Name, fieldInfoNameVisual = field.ReflectedType.Name + "." + field.Name, show = false, category = "Default", formatter = InventoryEditorUtil.GetSettingsManager() != null ? InventoryEditorUtil.GetSettingsManager().defaultCharacterStatFormatter : null });
                            }
                            else
                            {
                                // Item exists more than once.
                                var already = displayList.Find(o => o.fieldInfoNameVisual == field.ReflectedType.Name + "." + field.Name);
                                if (already == null)
                                {
                                    displayList.Add(old[0]);
                                }
                            }
                        }
                    }
                }

                InventoryEditorUtil.selectedDatabase.equipStats = displayList.ToArray();
                resultList.list = InventoryEditorUtil.selectedDatabase.equipStats; // Update list view
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Step 3: Choose what you want to display.", InventoryEditorStyles.titleStyle);

            EditorGUILayout.BeginVertical(InventoryEditorStyles.reorderableListStyle);
            resultList.DoLayoutList();
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        public override string ToString()
        {
            return name;
        }
    }
}
