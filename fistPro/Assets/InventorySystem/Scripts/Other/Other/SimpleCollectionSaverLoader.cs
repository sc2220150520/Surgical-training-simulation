using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Devdog.InventorySystem.Models;
using UnityEngine;

namespace Devdog.InventorySystem
{
    public partial class SimpleCollectionSaverLoader : MonoBehaviour
    {

        public JsonCollectionSerializer serializer { get; private set; }
        public ItemCollectionBase collection { get; private set; }
        public BasicCollectionSaverLoader saverLoader { get; private set; }

        public void Awake()
        {
            serializer = new JsonCollectionSerializer();
            collection = GetComponent<ItemCollectionBase>();
            saverLoader = new BasicCollectionSaverLoader();

            Load();
        }

        public void OnApplicationQuit()
        {
            Save();
        }

        public virtual void Save()
        {
            if (collection.useReferences)
            {
                var r = GetReferenceLookups();
                var bytes = serializer.SerializeItemReferences(r);
                saverLoader.SaveItems(collection, null, bytes, null);
                Debug.Log("Saved " + r.Count + " references");
            }
            else
            {
                var r = GetItemLookups();
                var bytes = serializer.SerializeItems(r);
                saverLoader.SaveItems(collection, null, bytes, null);
                Debug.Log("Saved " + r.Count + " items");
            }
        }

        public virtual void Load()
        {
            if (collection.useReferences)
            {
                //var r = saverLoader.LoadItems(collection, null, (bytes) =>
                //{
                //    var b = serializer.DeserializeItemReferences(bytes);
                //    var itemsArray = new InventoryItemBase[b.Count];
                //    for (int i = 0; i < itemsArray.Length; i++)
                //    {
                //        itemsArray[i] = b[i].itemID
                //    }
                //    collection.SetItems(b.ToArray());

                //    Debug.Log("Saved " + r.Count + " references"); 
                //});
            }
            else
            {
                saverLoader.LoadItems(collection, null, (bytes) =>
                {
                    var b = serializer.DeserializeItems(bytes);
                    var itemsArray = new InventoryItemBase[b.Count];
                    for (int i = 0; i < itemsArray.Length; i++)
                    {
                        if (b[i].itemID != -1)
                        {
                            itemsArray[i] = Instantiate<InventoryItemBase>(ItemManager.instance.items[b[i].itemID]);
                            itemsArray[i].currentStackSize = b[i].amount;
                        }
                        else
                            itemsArray[i] = null;
                    }
                    collection.SetItems(itemsArray, true);

                    Debug.Log("Loaded " + itemsArray.Length + " items"); 
                });
            }
        }


        public virtual List<InventoryItemReferenceSaveLookup> GetReferenceLookups()
        {
            var l = new List<InventoryItemReferenceSaveLookup>();
            foreach (var item in collection.items)
            {
                if (item.item == null)
                    l.Add(new InventoryItemReferenceSaveLookup(-1, 0, string.Empty));
                else
                    l.Add(new InventoryItemReferenceSaveLookup((int)item.item.ID, item.item.currentStackSize, item.item.itemCollection.collectionName));
            }

            return l;
        }

        public virtual List<InventoryItemSaveLookup> GetItemLookups()
        {
            var l = new List<InventoryItemSaveLookup>();
            foreach (var item in collection.items)
            {
                if (item.item == null)
                    l.Add(new InventoryItemSaveLookup(-1, 0));
                else
                    l.Add(new InventoryItemSaveLookup((int)item.item.ID, item.item.currentStackSize));
            }

            return l;
        }

    }
}
