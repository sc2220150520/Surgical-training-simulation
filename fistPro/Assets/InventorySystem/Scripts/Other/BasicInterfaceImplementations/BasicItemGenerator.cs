﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Devdog.InventorySystem.Models;

namespace Devdog.InventorySystem
{
    public partial class BasicItemGenerator : IItemGenerator
    {
        public InventoryItemGeneratorItem[] items { get; set; }

        public InventoryItemBase[] result { get; set; }

        ///// <summary>
        ///// Allow the same item multiple times
        ///// </summary>
        //public bool allowDoubles = false;

        public int minRequiredLevel = 0;
        public int maxRequiredLevel = 999999;

        public int minBuyPrice = 0;
        public int maxBuyPrice = 999999;

        public int minSellPrice = 0;
        public int maxSellPrice = 999999;

        public float minWeight = 0.0f;
        public float maxWeight = 999999.0f;

        public int minStackSize = 0;
        public int maxStackSize = 999999;

        public List<InventoryItemProperty> onlyWithPoperty = new List<InventoryItemProperty>();
        public List<InventoryItemCategory> onlyOfCategory = new List<InventoryItemCategory>();
        public List<InventoryItemRarity> onlyOfRarity = new List<InventoryItemRarity>();
        public List<System.Type> onlyOfType = new List<System.Type>();


        protected static System.Random randomGen = new System.Random();

        
        public BasicItemGenerator()
        {
            //SetItems(ItemManager.instance.items); // Default items, all
        }

        /// <summary>
        /// Generate an array of items.
        /// InventoryItemGeneratorItem's chance is only affected after all the filters are applied, so the item might still be rejected by type, category, etc.
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public InventoryItemBase[] Generate(int amount)
        {
            return Generate(amount, amount);
        }

        /// <summary>
        /// Generate an array of items.
        /// InventoryItemGeneratorItem's chance is only affected after all the filters are applied, so the item might still be rejected by type, category, etc.
        /// </summary>
        /// <param name="minAmount"></param>
        /// <param name="maxAmount"></param>
        /// <returns></returns>
        public InventoryItemBase[] Generate(int minAmount, int maxAmount)
        {
            var toReturn = new List<InventoryItemBase>(maxAmount);

            foreach (int i in Enumerable.Range(0, items.Count()).OrderBy(x => randomGen.Next()))
            {
                var generatorItem = items[i];

                if(toReturn.Count > minAmount)
                {
                    float dif = 1.0f / (maxAmount - minAmount); // Example:  10 - 8 = 2 --- 1.0f / 2 = 0.5f // 50% chance of stopping here
                    if (Random.value > dif)
                        break;
                }
                if (toReturn.Count >= maxAmount)
                    break;


                var item = generatorItem.item;

                // First check all the types and rarity's, categories, as they affect the most items.
                if (onlyOfType.Count > 0 && onlyOfType.Contains(item.GetType()) == false)
                    continue;

                if (onlyOfRarity.Count > 0 && onlyOfRarity.Contains(item.rarity) == false)
                    continue;

                if (onlyOfCategory.Count > 0 && onlyOfCategory.Contains(item.category) == false)
                    continue;

                int hasProps = 0;
                foreach (var prop in onlyWithPoperty)
                {
                    if (onlyWithPoperty.Contains(prop))
                        hasProps++;
                }

                if (onlyWithPoperty.Count > 0 && hasProps < onlyWithPoperty.Count)
                    continue;


                // Check all other values
                if (item.requiredLevel < minRequiredLevel || item.requiredLevel > maxRequiredLevel)
                    continue;

                if(item.buyPrice < minBuyPrice || item.buyPrice > maxBuyPrice)
                    continue;

                if(item.sellPrice < minSellPrice || item.sellPrice > maxSellPrice)
                    continue;

                if(item.weight < minWeight || item.weight > maxWeight)
                    continue;

                if(item.maxStackSize < minStackSize || item.maxStackSize > maxStackSize)
                    continue;

                // Example: Random.value = 0...1.0f, chanceFactor = 0.2f; -> hence 20% chance that the Random.value is below 0.2f;
                if (Random.value > generatorItem.chanceFactor)
                    continue;

                toReturn.Add(item);
            }

            result = toReturn.ToArray();
            return result;
        }

        public void SetItems(InventoryItemBase[] toSet, float chance = 1.0f)
        {
            items = new InventoryItemGeneratorItem[toSet.Length];
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = new InventoryItemGeneratorItem(toSet[i], chance);
            }
        }
    }
}