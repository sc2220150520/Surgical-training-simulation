using UnityEngine;
using UnityEditor;
using System;

namespace Devdog.InventorySystem.Editors
{
    [CustomEditor(typeof(ObjectTriggerer), true)]
    [CanEditMultipleObjects]
    public class ObjectTriggererEditor : Editor
    {

        private SerializedProperty window;

        public virtual void OnEnable()
        {
            window = serializedObject.FindProperty("window");
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();



            // Draws remaining items
            //EditorGUILayout.BeginVertical("box");
            DrawPropertiesExcluding(serializedObject, new string[]
            {
                "window"
            });
            //EditorGUILayout.EndVertical();

            var o = ((ObjectTriggerer) target).gameObject.GetComponent(typeof (IObjectTriggerUser));
            if (o != null)
            {
                //if(window.objectReferenceValue != o)
                window.objectReferenceValue = o;

                GUI.enabled = false;
            }

            EditorGUILayout.PropertyField(window);
            GUI.enabled = true;

            if(o != null)
                EditorGUILayout.HelpBox("Window is managed by " + o.GetType().Name, MessageType.Info);

            
            serializedObject.ApplyModifiedProperties();
        }
    }
}