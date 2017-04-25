using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;


namespace Devdog.InventorySystem.Editors
{
    public class AboutEditor : EditorWindow
    {
        private Vector2 scrollPos { get; set; }


        //[MenuItem("Tools/InventorySystem/About", false, 99)] // Always at bottom
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<AboutEditor>(true, "About", true);
        }

        public void OnGUI()
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos);

            //GUILayout.Label(EditorGUIUtility.FindTexture("InventoryAboutHeader"), GUILayout.Width(400), GUILayout.Height(60));


            GUILayout.EndScrollView();
        }
    }
}