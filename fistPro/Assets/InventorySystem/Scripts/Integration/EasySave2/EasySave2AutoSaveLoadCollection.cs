#if EASY_SAVE_2

using UnityEngine;
using System.Collections;
using Devdog.InventorySystem;
using System.Collections.Generic;

namespace Devdog.InventorySystem.Integration.EasySave2
{
    [RequireComponent(typeof(ItemCollectionBase))]
    [AddComponentMenu("InventorySystem/Integration/EasySave2/Auto Save Load collection")]
    public class EasySave2AutoSaveLoadCollection : MonoBehaviour
    {
        public string fileName = "myFile.txt";
        public string[] additionalFields = new string[0];

        private ItemCollectionBase collection;

        public virtual void Awake()
        {
            collection = GetComponent<ItemCollectionBase>();

            if (collection.useReferences)
            {
                StartCoroutine(WaitAndLoad());
            }
            else
            {
                collection.LoadEasySave2(fileName, additionalFields);
            }
        }


        /// <summary>
        /// Method used for references, because a reference can only be created when other collections are loaded.
        /// </summary>
        /// <returns></returns>
        private IEnumerator WaitAndLoad()
        {
            yield return null; // Wait a frame to make sure non-reference collections are done.

            collection.LoadEasySave2(fileName, additionalFields);
        }

        /// <summary>
        /// Save when the application quits.
        /// </summary>
        public virtual void OnApplicationQuit()
        {
            collection.SaveEasySave2(fileName, additionalFields);
        }
    }
}

#else

using UnityEngine;
using System.Collections;
using Devdog.InventorySystem;
using System.Collections.Generic;

/// <summary>
/// An empty class that is used if EasySave2 is not present
/// </summary>
public class EasySave2AutoSaveLoadCollection : MonoBehaviour
{
    
}

#endif