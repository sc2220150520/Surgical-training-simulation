using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Devdog.InventorySystem.Models
{
    /// <summary>
    /// Convenience class that holds the final stats.
    /// </summary>
    public partial class InventoryEquipStatRowLookup
    {
        /// <summary>
        /// Name of this stat
        /// </summary>
        public string statName;

        /// <summary>
        /// Value after all calculations, this is a sum of the stat.
        /// </summary>
        public float finalValue;

        /// <summary>
        /// The formatted version of this stat
        /// </summary>
        public string finalValueString;


        public InventoryEquipStatRowLookup(string statName, float finalValue, string finalValueString)
        {
            this.statName = statName;
            this.finalValue = finalValue;
            this.finalValueString = finalValueString;
        }
    }
}