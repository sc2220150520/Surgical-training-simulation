using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

namespace Devdog.InventorySystem.Editors
{
    public abstract class InventoryEditorBase : Editor
    {
        protected SerializedProperty script;
        protected CustomOverrideProperty[] overrides;

        protected InventoryItemDatabase database
        {
            get { return InventoryEditorUtil.GetItemDatabase(true, false); }
        }


        public virtual void OnEnable()
        {
            if (serializedObject != null)
                script = serializedObject.FindProperty("m_Script");
        }

        protected CustomOverrideProperty FindOverride(string name)
        {
            return overrides.FirstOrDefault(o => o.serializedName == name);
        }

        public override void OnInspectorGUI()
        {
            OnCustomInspectorGUI();
        }

        protected virtual void OnCustomInspectorGUI(params CustomOverrideProperty[] extraOverride)
        {
            serializedObject.Update();

            if(script != null)
                EditorGUILayout.PropertyField(script);

            serializedObject.ApplyModifiedProperties();
        }

    }
}
