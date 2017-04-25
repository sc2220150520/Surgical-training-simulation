﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Devdog.InventorySystem.Editors
{
    public abstract partial class InventorySystemEditorCrudBase<T> : IInventorySystemEditorCrud where T : class
    {
        protected T selectedItem { get; set; }
        protected abstract List<T> crudList { get; set; }

        protected Vector2 scrollPosition;
        protected Vector2 scrollPositionDetail;
        protected string singleName;
        protected string pluralName;

        private string searchQuery { get; set; }
        private int searchResultCount = -1;

        public bool canDeleteItems { get; set; }
        public bool canCreateItems { get; set; }

        private Color createColor = new Color(0.4f, 1.0f, 0.4f, 0.8f);
        protected Rect sidebarRowElementOffset;
        public EditorWindow window { get; protected set; }
        public bool requiresDatabase { get; set; }
        public bool forceUpdateIDsWhenOutOfSync { get; protected set; }


        public bool isSearching
        {
            get { return searchQuery != "" && searchQuery != "Search..."; }
        }

        public InventorySystemEditorCrudBase(string singleName, string pluralName, EditorWindow window)
        {
            this.singleName = singleName;
            this.pluralName = pluralName;
            this.window = window;
            this.requiresDatabase = true;

            canCreateItems = true;
            canDeleteItems = true;
            forceUpdateIDsWhenOutOfSync = true;

            Focus();
        }

        /// <summary>
        /// Create a new item and add it to the database
        /// </summary>
        protected abstract void CreateNewItem();


        public virtual void Focus()
        {
            searchResultCount = -1;
            searchQuery = "Search...";

            // Other database or something... item not found
            //if (crudList.Contains(selectedItem) == false)
                //selectedItem = null;
        }


        protected abstract bool IDsOutOfSync();
        protected abstract void SyncIDs();


        public virtual void Draw()
        {
            if (forceUpdateIDsWhenOutOfSync && crudList.Count > 0 && IDsOutOfSync())
            {
                SyncIDs();
            }

            EditorGUILayout.BeginHorizontal();
            DrawSidebar();

            //EditorGUILayout.Space(); // Center editor
            scrollPositionDetail = EditorGUILayout.BeginScrollView(scrollPositionDetail, GUILayout.MaxWidth(600));
            for (int i = 0; i < crudList.Count; i++)
            {
                if (selectedItem != null && selectedItem.Equals(crudList[i]))
                {
                    DrawDetail(selectedItem, i);
                    break;
                }
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw a single item in the sidebar
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        protected abstract void DrawSidebarRow(T item, int index);


        /// <summary>
        /// Does a item match a search query, only called when actually searching.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="searchQuery"></param>
        /// <returns></returns>
        protected abstract bool MatchesSearch(T item, string searchQuery);


        /// <summary>
        /// Add an item to the crud list
        /// </summary>
        /// <param name="item"></param>
        public virtual void AddItem(T item, bool editOnceAdded = true)
        {
            // Strange construction I know..
            // Bypass read-only problem on scriptable object.
            var tempList = new List<T>(crudList.ToArray());
            tempList.Add(item);
            crudList = tempList;
            window.Repaint();

            if(editOnceAdded)
                EditItem(item);
        }

        public virtual void RemoveItem(int index)
        {
            if (selectedItem != null && selectedItem.Equals(crudList[index]))
                selectedItem = null;

            // Strange construction I know..
            // Bypass read-only problem on scriptable object.
            var tempList = new List<T>(crudList.ToArray());
            tempList.RemoveAt(index);
            crudList = tempList;

            window.Repaint();
        }

        public virtual void EditItem(T item)
        {
            selectedItem = item;
        }


        /// <summary>
        /// Begin a sidebar row element
        /// </summary>
        /// <param name="item"></param>
        /// <param name="i"></param>
        protected virtual void BeginSidebarRow(T item, int i, int width = -1)
        {
            Rect rect = EditorGUILayout.GetControlRect(true, 30.0f);
            rect.x -= 7;
            rect.width = width == -1 ? 300 : width;
            //if (canDeleteItems == false)
                //rect.width += 50; // No delete button, so more space

            GUI.backgroundColor = (selectedItem == item) ? new Color(0, 1.0f, 0, 0.3f) : new Color(0, 0, 0, 0.0f);

            if (GUI.Button(rect, "", "MeTransitionSelectHead"))
                EditItem(item);

            GUI.backgroundColor = Color.white;

            sidebarRowElementOffset = rect;
            sidebarRowElementOffset.x += 7;
            sidebarRowElementOffset.y += 7; // For text
            GUI.color = (selectedItem == item) ? Color.white : new Color(1.0f, 1.0f, 1.0f, 0.6f);

            EditorGUILayout.BeginHorizontal(GUILayout.Height(30.0f));
        }

        protected virtual void DrawSidebarRowElement(string text, int width, int height = -1)
        {
            sidebarRowElementOffset.width = width;
            sidebarRowElementOffset.height = (height == -1) ? sidebarRowElementOffset.height : height;
            EditorGUI.LabelField(sidebarRowElementOffset, text);

            sidebarRowElementOffset.x += width;
        }

        protected virtual void DrawSidebarRowSpace(int width)
        {
            sidebarRowElementOffset.x += width;
        }

        protected virtual void DrawSidebarRowElement(string text, GUIStyle guiStyle, int width, int height = -1)
        {
            sidebarRowElementOffset.width = width;
            sidebarRowElementOffset.height = (height == -1) ? sidebarRowElementOffset.height : height;
            EditorGUI.LabelField(sidebarRowElementOffset, text, guiStyle);

            sidebarRowElementOffset.x += width;
        }

        protected virtual bool DrawSidebarRowElementButton(string text, int width, int height = -1)
        {
            sidebarRowElementOffset.width = width;
            sidebarRowElementOffset.height = (height == -1) ? sidebarRowElementOffset.height : height;
            bool clicked = GUI.Button(sidebarRowElementOffset, text);

            sidebarRowElementOffset.x += width;
            return clicked;
        }

        protected virtual bool DrawSidebarRowElementButton(string text, GUIStyle guiStyle, int width, int height = -1)
        {
            sidebarRowElementOffset.width = width;
            sidebarRowElementOffset.height = (height == -1) ? sidebarRowElementOffset.height : height;
            bool clicked = GUI.Button(sidebarRowElementOffset, text, guiStyle);

            sidebarRowElementOffset.x += width;
            return clicked;
        }

        protected virtual bool DrawSidebarRowElementToggle(bool toggled, string text, int width, int height = -1)
        {
            sidebarRowElementOffset.width = width;
            sidebarRowElementOffset.height = (height == -1) ? sidebarRowElementOffset.height : height;
            bool t = GUI.Toggle(sidebarRowElementOffset, toggled, text);

            sidebarRowElementOffset.x += width;
            return t;
        }

        protected virtual bool DrawSidebarRowElementToggle(bool toggled, string text, GUIStyle guiStyle, int width, int height = -1)
        {
            sidebarRowElementOffset.width = width;
            sidebarRowElementOffset.height = (height == -1) ? sidebarRowElementOffset.height : height;
            bool t = GUI.Toggle(sidebarRowElementOffset, toggled, text, guiStyle);

            sidebarRowElementOffset.x += width;
            return t;
        }

        protected virtual void EndSidebarRow(T item, int i)
        {
            sidebarRowElementOffset.x += 10; // 110 - 10 extra for offset
            //sidebarRowElementOffset.y -= 7; // For text
            sidebarRowElementOffset.width = 30;

            if (canDeleteItems)
            {
                GUI.color = Color.red;
                int prevDepth = GUI.depth;
                GUI.depth = 50;
                if (GUI.Button(sidebarRowElementOffset, "", (GUIStyle)"OL Minus"))
                {
                    if (EditorUtility.DisplayDialog("Are you sure?", "Do you want to delete " + singleName + " " + item.ToString(), "Yes", "NO!"))
                    {
                        RemoveItem(i);
                    }
                }
                GUI.depth = prevDepth;
            }
            GUI.color = Color.white;

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw the list, where item can be selected to edit
        /// </summary>
        protected virtual void DrawSidebar()
        {
            EditorGUILayout.BeginVertical(InventoryEditorStyles.boxStyle, GUILayout.Width(375.0f));

            searchQuery = InventoryEditorStyles.SearchBar(searchQuery, window, isSearching);
            //if (isFirstRepaintCycle)
            //{
            //    EditorGUI.FocusTextInControl("SearchField");
            //    isFirstRepaintCycle = false;
            //}

            GUI.color = createColor;
            GUI.enabled = canCreateItems;

            if (GUILayout.Button("Create " + singleName, (GUIStyle)"LargeButton"))
                CreateNewItem();

            GUI.color = Color.white;
            GUI.enabled = true;


            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // BEGIN ROW
            EditorGUILayout.BeginHorizontal();

            if (isSearching)
                GUILayout.Label(searchResultCount + " " + pluralName + " (search result)"); // , InventoryEditorStyles.titleStyle
            else
                GUILayout.Label(crudList.Count + " " + pluralName);

            EditorGUILayout.EndHorizontal();
            // END ROW

            int x = 0;
            searchResultCount = 0;
            int searchResultIndexItem = -1;
            bool nullInList = false;
            foreach (var item in crudList)
            {
                if (item == null)
                {
                    x++;
                    nullInList = true;
                    continue;                    
                }

                if (item.Equals(selectedItem))
                    GUI.color = Color.green;

                if (isSearching)
                {
                    if (MatchesSearch(item, searchQuery))
                    {
                        searchResultCount++;
                        DrawSidebarRow(item, x);
                        searchResultIndexItem = x;
                    }
                }
                else
                    DrawSidebarRow(item, x);

                GUI.color = Color.white;
                x++;
            }

            if (nullInList)
            {
                // Cleanup list
                var l = new List<T>(crudList.ToArray());
                l.RemoveAll(o => o == null);
                crudList = l;
            }

            // Edit item if only 1 search result
            if (searchResultIndexItem != -1 && searchResultCount == 1)
                EditItem(crudList[searchResultIndexItem]);

            if (searchResultCount == 0 && isSearching)
            {
                selectedItem = null;
                window.Repaint();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();

        }
        
        /// <summary>
        /// Draw a single item in detail
        /// </summary>
        protected abstract void DrawDetail(T item, int index);


        /// <summary>
        /// Name of the Editor
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return singleName + " editor";
        }
    }
}
