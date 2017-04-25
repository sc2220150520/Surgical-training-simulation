using UnityEngine;
using System.Collections;

namespace Devdog.InventorySystem
{
    public interface ICurrencyFormatter
    {


        /// <summary>
        /// Format the given value into a string to be displayed.
        /// </summary>
        /// <param name="val"></param>
        /// <returns>A currency string.</returns>
        string Format(float val);
    }
}