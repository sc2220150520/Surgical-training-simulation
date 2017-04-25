﻿using System;
using System.Collections.Generic;
using Devdog.InventorySystem.Models;
using UnityEngine;

namespace Devdog.InventorySystem
{
    public partial interface IItemGenerator
    {
        /// <summary>
        /// The items used to consider when generating the items
        /// </summary>
        InventoryItemGeneratorItem[] items { get; set; }

        /// <summary>
        /// The result of the last Generate() method.
        /// </summary>
        InventoryItemBase[] result { get; set; }


        /// <summary>
        /// Generate n amount of items, results are stored in property result.
        /// </summary>
        /// <param name="amount">Amount of items to generate.</param>
        /// <returns>Array of items</returns>
        InventoryItemBase[] Generate(int amount);

        /// <summary>
        /// Generate n amount of items, results are stored in property result.
        /// </summary>
        /// <param name="minAmount"></param>
        /// <param name="maxAmount"></param>
        /// <returns>Array of items</returns>
        InventoryItemBase[] Generate(int minAmount, int maxAmount);


        /// <summary>
        /// Set a list of items at once
        /// </summary>
        /// <param name="items"></param>
        void SetItems(InventoryItemBase[] items, float chance = 1.0f);
    }
}