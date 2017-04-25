using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devdog.InventorySystem
{
    /// <summary>
    /// Static class that handles the distance checking for the ObjectTriggerer, and avoids GC alloc.
    /// </summary>
    public static class ObjectTriggererChecker
    {
        public static List<ObjectTriggerer> objectTriggerers = new List<ObjectTriggerer>(64);
        private static WaitForSeconds waitTime = new WaitForSeconds(0.5f);
        private static int objectTriggerersCounter = 0;
        private static bool inited = false;

        public static void Init(MonoBehaviour behaviour)
        {
            if (inited)
                return;

            inited = true;
            behaviour.StartCoroutine(DistanceCheck());
        }

        /// <summary>
        /// An infinite loop that checks the distance to the object every 0.5 seconds.
        /// Placed here, to avoid GC alloc on every object triggerer object.
        /// </summary>
        /// <returns></returns>
        private static IEnumerator DistanceCheck()
        {
            while (true)
            {
                yield return waitTime;

                for (objectTriggerersCounter = 0; objectTriggerersCounter < objectTriggerers.Count; objectTriggerersCounter++)
                {
                    if (objectTriggerers[objectTriggerersCounter].isActive && objectTriggerers[objectTriggerersCounter].inRange == false)
                    {
                        objectTriggerers[objectTriggerersCounter].UnUse();
                    }
                }
            }
        }
    }
}
