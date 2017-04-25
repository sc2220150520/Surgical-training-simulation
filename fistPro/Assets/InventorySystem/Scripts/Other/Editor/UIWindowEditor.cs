using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Collections.Generic;

namespace Devdog.InventorySystem.Editors
{
    [CustomEditor(typeof(UIWindow), true)]
    public class UIWindowEditor : Editor
    {
        private SerializedProperty script;

        private SerializedProperty hideOnStart;
        private SerializedProperty keyCombination;

        private SerializedProperty showAnimation;
        private SerializedProperty hideAnimation;
        private SerializedProperty showAudioClip;
        private SerializedProperty hideAudioClip;

        private ReorderableList keyCombinationList;

        public virtual void OnEnable()
        {
            script = serializedObject.FindProperty("m_Script");

            hideOnStart = serializedObject.FindProperty("hideOnStart");
            keyCombination = serializedObject.FindProperty("keyCombination");

            showAnimation = serializedObject.FindProperty("showAnimation");
            hideAnimation = serializedObject.FindProperty("hideAnimation");
            showAudioClip = serializedObject.FindProperty("showAudioClip");
            hideAudioClip = serializedObject.FindProperty("hideAudioClip");
            

            keyCombinationList = new ReorderableList(serializedObject, keyCombination, false, true, true, true);
            keyCombinationList.drawHeaderCallback += rect => GUI.Label(rect, "Key combination(s)");
            keyCombinationList.drawElementCallback += (rect, index, active, focused) =>
            {
                rect.height = 16;
                rect.y += 2;

                EditorGUI.PropertyField(rect, keyCombination.GetArrayElementAtIndex(index));
            };
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.PropertyField(script);

            EditorGUILayout.PropertyField(hideOnStart);
            keyCombinationList.DoLayoutList();
            //EditorGUILayout.PropertyField(hideOnStart);

            // Draws remaining items
            DrawPropertiesExcluding(serializedObject, new string[]
            {
                "m_Script",
                "hideOnStart",
                "keyCombination",
                "showAnimation",
                "hideAnimation",
                "showAudioClip",
                "hideAudioClip",
            });


            EditorGUILayout.PropertyField(showAnimation);
            EditorGUILayout.PropertyField(hideAnimation);
            EditorGUILayout.PropertyField(showAudioClip);
            EditorGUILayout.PropertyField(hideAudioClip);


            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
    }
}