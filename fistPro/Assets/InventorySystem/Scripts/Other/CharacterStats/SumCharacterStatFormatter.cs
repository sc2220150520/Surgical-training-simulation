using System;
using System.Collections.Generic;
using UnityEngine;

namespace Devdog.InventorySystem
{
    [System.Serializable]
    public partial class SumCharacterStatFormatter : CharacterStatFormatterBase
    {
        public override string FormatStat(IEnumerable<float> stats)
        {
            float sum = 0.0f;
            foreach (float item in stats)
            {
                sum += item;
            }

            // Hide values that are 0
            //if (sum == 0.0f)
            //    return string.Empty;

            return Math.Round(sum, 1).ToString();
            //return Mathf.Round(sum).ToString(); // Only ints
        }
    }
}