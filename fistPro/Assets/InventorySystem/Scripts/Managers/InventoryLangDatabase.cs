﻿using System;
using System.Collections;
using System.Collections.Generic;
using Devdog.InventorySystem.Models;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Devdog.InventorySystem
{
    [System.Serializable]
    public partial class InventoryLangDatabase : ScriptableObject
    {
#if UNITY_EDITOR

        [MenuItem("Assets/Create/InventorySystem/Language database", false, -5)]
        protected static void CreatePrefab()
        {
            var selected = Selection.activeObject;
            string path = AssetDatabase.GetAssetPath(selected);

            if (path == "")
            {
                path = "Assets";
            }

            string pathName = AssetDatabase.GenerateUniqueAssetPath(path + "/LanguageDatabase.asset");
            var o = ScriptableObject.CreateInstance<InventoryLangDatabase>();
            AssetDatabase.CreateAsset(o, pathName);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = o;

            Debug.Log("Language database created at path " + pathName);
        }

#endif


        [Header("Item actions")]
        public InventoryNoticeMessage itemCannotBeDropped = new InventoryNoticeMessage("", "Item {0} cannot be dropped", NoticeDuration.Medium, Color.white);
        public InventoryNoticeMessage itemCannotBeStored = new InventoryNoticeMessage("", "Item {0} cannot be stored", NoticeDuration.Medium, Color.white);
        public InventoryNoticeMessage itemCannotBeUsed = new InventoryNoticeMessage("", "Item {0} cannot be used", NoticeDuration.Medium, Color.white);
        public InventoryNoticeMessage itemCannotBeUsedLevelToLow = new InventoryNoticeMessage("", "Item {0} cannot be used required level is {2}", NoticeDuration.Medium, Color.white);
        public InventoryNoticeMessage itemCannotBeSold = new InventoryNoticeMessage("", "Item {0} cannot be sold", NoticeDuration.Medium, Color.white);
        public InventoryNoticeMessage itemCannotBePickedUpToFarAway = new InventoryNoticeMessage("", "Item {0} is too far away to pick up", NoticeDuration.Medium, Color.white);
        public InventoryNoticeMessage itemIsInCooldown = new InventoryNoticeMessage("", "Item {0} is in cooldown {2} more seconds", NoticeDuration.Medium, Color.white);

        //public InventoryNoticeMessage cannotDropItem;

        [Header("Collections")]
        public InventoryNoticeMessage collectionDoesNotAllowType = new InventoryNoticeMessage("", "Does not allow type", NoticeDuration.Medium, Color.white);
        public InventoryNoticeMessage collectionFull = new InventoryNoticeMessage("", "{2} is full", NoticeDuration.Medium, Color.white);
        //public InventoryNoticeMessage collection;
        //public InventoryNoticeMessage collectionDoesNotAllowType;


        [Header("User actions")]
        public InventoryNoticeMessage userNotEnoughGold = new InventoryNoticeMessage("", "Not enough gold", NoticeDuration.Medium, Color.white);


        [Header("Crafting")]
        public InventoryNoticeMessage craftedItem = new InventoryNoticeMessage("", "Successfully crafted {0}", NoticeDuration.Medium, Color.white);
        public InventoryNoticeMessage craftingFailed = new InventoryNoticeMessage("", "Crafting item {0} failed", NoticeDuration.Medium, Color.white);
        public InventoryNoticeMessage craftingCanceled = new InventoryNoticeMessage("", "Crafting item {0} canceled", NoticeDuration.Medium, Color.white);
        public InventoryNoticeMessage craftingDontHaveRequiredItems = new InventoryNoticeMessage("", "You don't have the required items to craft {2}", NoticeDuration.Long, Color.white);


        [Header("Vendor")]
        public InventoryNoticeMessage vendorCannotSellItem = new InventoryNoticeMessage("", "Cannot sell item {0} to this vendor.", NoticeDuration.Medium, Color.white);
        public InventoryNoticeMessage vendorSoldItemToVendor = new InventoryNoticeMessage("", "Sold {2}x {0} to vendor {3} for {4}.", NoticeDuration.Medium, Color.white);
        public InventoryNoticeMessage vendorBoughtItemFromVendor = new InventoryNoticeMessage("", "Bought {2}x {0} from vendor {3} for {4}.", NoticeDuration.Medium, Color.white);


        [Header("Dialogs")]
        public InventoryMessage confirmationDialogDrop = new InventoryMessage("Are you sure?", "Are you sure you want to drop {0}?");
        public InventoryMessage unstackDialog = new InventoryMessage("Unstack item {0}", "How many do you want to unstack?");
        

    }
}
