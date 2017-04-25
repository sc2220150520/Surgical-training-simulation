using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Devdog.InventorySystem.Demo
{
    public partial class CharacterEventTester : MonoBehaviour
    {

        [InventoryRequired]
        public CharacterUI character;
        
        public void Awake()
        {
            character.OnAddedItem += (item, slot, amount) =>
            {
                Debug.Log("Character collection : added item");
            };
            character.OnRemovedItem += (id, slot, amount) =>
            {
                Debug.Log("Character collection : removed item");
            };
            character.OnSwappedItems += (collection, slot, toCollection, toSlot) =>
            {
                Debug.Log("Character collection : swapped items");
            };
            character.OnUsedItem += (id, slot, amount) =>
            {
                Debug.Log("Character collection : use item from collection");
            };
        }
    }
}
