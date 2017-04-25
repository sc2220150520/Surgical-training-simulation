using System;
using UnityEngine;

namespace Devdog.InventorySystem.Models
{
    [System.Serializable]
    public partial class InventoryEquipStat
    {
        /// <summary>
        /// Visual name (Strength, Agility, etc)
        /// </summary>
        public string name;

        /// <summary>
        /// Category of this stat?
        /// </summary>
        public string category;

        /// <summary>
        /// assemblyQualifiedTypeName
        /// </summary>
        public string typeName;

        /// <summary>
        /// Used for reflection to get the value.
        /// </summary>
        public string fieldInfoName;

        /// <summary>
        /// Only use this inside the editor.
        /// </summary>
        public string fieldInfoNameVisual;

        /// <summary>
        /// Show this stat?
        /// </summary>
        public bool show = false;

        /// <summary>
        /// The formatter defines how the item should be displayed.
        /// </summary>
        public CharacterStatFormatterBase formatter;
    }
}