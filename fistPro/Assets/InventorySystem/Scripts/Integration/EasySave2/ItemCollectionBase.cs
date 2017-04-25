#if EASY_SAVE_2

using UnityEngine;
using System.Collections;
using Devdog.InventorySystem;
using System.Collections.Generic;
using System.Reflection;
using Devdog.InventorySystem.Models;

namespace Devdog.InventorySystem
{

    public partial class ItemCollectionBase
    {
        private string easySaveCollectionName
        {
            get { return collectionName.ToLower().Replace(" ", "_"); }
        }


        /// <summary>
        /// Save the collection using EasySave2
        /// This method will use the collections name for the file name.
        /// </summary>
        public void SaveEasySave2(params string[] additionalFields)
        {
            SaveEasySave2(easySaveCollectionName + ".txt");
        }

        /// <summary>
        /// Save the collection using EasySave2
        /// This method will write to the given file name.
        /// </summary>
        /// <param name="fileName">The name of the file you want to save to</param>
        public void SaveEasySave2(string fileName, params string[] additionalFields)
        {
            if (useReferences)
            {
                var l = new List<InventoryItemReferenceSaveLookup>();
                foreach (var item in items)
                {
                    if (item.item == null)
                        l.Add(new InventoryItemReferenceSaveLookup(-1, 0, string.Empty));
                    else
                        l.Add(new InventoryItemReferenceSaveLookup((int)item.item.ID, item.item.currentStackSize, item.item.itemCollection.collectionName));
                }

                using (ES2Writer writer = ES2Writer.Create(fileName, new ES2Settings() { fileMode = ES2Settings.ES2FileMode.Create }))
                {
                    writer.Write(l.ToArray(), "itemReferenceLookups_" + easySaveCollectionName);
                    writer.Save();
                }   
            }
            else
            {
                var l = new List<InventoryItemSaveLookup>();
                foreach (var item in items)
                {
                    if (item.item == null)
                        l.Add(new InventoryItemSaveLookup(-1, 0));
                    else
                        l.Add(new InventoryItemSaveLookup((int)item.item.ID, item.item.currentStackSize));
                }

                using (ES2Writer writer = ES2Writer.Create(fileName, new ES2Settings() { fileMode = ES2Settings.ES2FileMode.Create }))
                {
                    writer.Write(l.ToArray(), "itemLookups_" + easySaveCollectionName);
                    writer.Save();
                }    
            }

            using (ES2Writer writer = ES2Writer.Create(fileName))
            {
                var l = new List<float>(additionalFields.Length);
                for (int i = 0; i < additionalFields.Length; i++)
                {
                    var f = GetType().GetField(additionalFields[i], BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (f != null)
                    {
                        //float val = (float);
                        l.Add(float.Parse(f.GetValue(this).ToString()));
                    }
                }

                if(l.Count > 0)
                    writer.Write(l.ToArray(), "additinalFields_" + easySaveCollectionName);

                writer.Save();
            }
        }


        /// <summary>
        /// Load the collection using EasySave2
        /// This method uses the collections name to load the data.
        /// </summary>
        public void LoadEasySave2(params string[] additionalFields)
        {
            LoadEasySave2(easySaveCollectionName + ".txt");
        }

        /// <summary>
        /// Load the collection using EasySave2
        /// </summary>
        /// <param name="fileName"></param>
        public void LoadEasySave2(string fileName, params string[] additionalFields)
        {
            if (ES2.Exists(fileName) == false)
            {
                // No data to load yet
                Debug.LogWarning("Can't load from file " + fileName + " file does not exist.", gameObject);
                return;
            }

            // Load all the items
            if(useReferences)
                _LoadEasySave2References(fileName);
            else
                _LoadEasySave2(fileName);

            using (ES2Reader reader = ES2Reader.Create(fileName))
            {
                if (reader.TagExists("additinalFields_" + easySaveCollectionName) == false)
                    return;

                float[] additional = reader.ReadArray<float>("additinalFields_" + easySaveCollectionName);

                for (int i = 0; i < additional.Length; i++)
                {
                    var f = GetType().GetField(additionalFields[i], BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (f != null)
                    {
                        var t = f.GetValue(this).GetType();
                        if (t == typeof(int))
                            f.SetValue(this, (int)additional[i]);
                        else if(t == typeof(float))
                            f.SetValue(this, (float)additional[i]);
                        else if(t== typeof(uint))
                            f.SetValue(this, (uint) additional[i]);
                        else
                            Debug.LogWarning("Type not found for " + t.ToString());
                    }
                }
            }
        }

        private void _LoadEasySave2References(string fileName, params string[] additionalFields)
        {
            using (ES2Reader reader = ES2Reader.Create(fileName))
            {
                // Read data from the file in any order.
                InventoryItemReferenceSaveLookup[] data = reader.ReadArray<InventoryItemReferenceSaveLookup>("itemReferenceLookups_" + easySaveCollectionName);

                var l = new List<InventoryItemBase>(data.Length);
                var cols = Object.FindObjectsOfType<ItemCollectionBase>();

                foreach (var item in data)
                {
                    if (item.itemID == -1)
                        l.Add(null);
                    else
                    {
                        foreach (var col in cols)
                        {
                            if (col.collectionName == item.referenceOfCollection)
                            {
                                // Found it
                                l.Add(col.Find((uint)item.itemID));
                            }
                        }
  
                    }
                }

                if (items.Length < l.Count)
                {
                    AddSlots((uint)(l.Count - items.Length));
                }
                else if (items.Length > l.Count)
                {
                    RemoveSlots((uint)(items.Length - l.Count));
                }

                SetItems(l.ToArray(), true);
            }
        }

        private void _LoadEasySave2(string fileName, params string[] additionalFields)
        {
            using (ES2Reader reader = ES2Reader.Create(fileName))
            {
                // Read data from the file in any order.
                InventoryItemSaveLookup[] data = reader.ReadArray<InventoryItemSaveLookup>("itemLookups_" + easySaveCollectionName);

                var l = new List<InventoryItemBase>(data.Length);
                var i = ItemManager.instance;

                foreach (var item in data)
                {
                    if (item.itemID == -1)
                        l.Add(null);
                    else
                    {
                        var copy = GameObject.Instantiate<InventoryItemBase>(i.items[item.itemID]);
                        copy.currentStackSize = item.amount;
                        l.Add(copy);
                    }
                }

                if (items.Length < l.Count)
                {
                    AddSlots((uint)(l.Count - items.Length));
                }
                else if (items.Length > l.Count)
                {
                    RemoveSlots((uint)(items.Length - l.Count));
                }

                SetItems(l.ToArray(), true);
            }
        }
    }    
}

#endif