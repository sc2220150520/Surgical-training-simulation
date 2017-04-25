using UnityEngine;
using System.Collections;

namespace Devdog.InventorySystem
{
    public class BasicCurrencyFormatter : ICurrencyFormatter
    {
        public string prefix;
        public string suffix = " Gold";

        public string Format(float val)
        {
            return prefix + Mathf.FloorToInt(val).ToString() + suffix;
        }
    }
}