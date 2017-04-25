using System;
using UnityEngine;

namespace Devdog.InventorySystem.Models
{
    [System.Serializable]
    public partial class InventoryItemProperty
    {
        [HideInInspector]
        public int ID;

        public string key;
        public string value;

        public bool showInUI = true;
        public Color uiColor = Color.white;


        public int intValue
        {
            get
            {
                return int.Parse(value);
            }
        }

        public float floatValue
        {
            get
            {
                return float.Parse(value);
            }
        }

        public string stringValue
        {
            get
            {
                return value;
            }
        }

        public bool boolValue
        {
            get
            {
                return bool.Parse(value);
            }
        }
    }
}