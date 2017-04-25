#if PLY_GAME

using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq.Expressions;
using System;
using System.Linq;
using System.Reflection;
using Devdog.InventorySystem;
using Devdog.InventorySystem.Models;
using Devdog.InventorySystem.UI.Models;
using plyCommon;
using plyGame;

namespace Devdog.InventorySystem.Integration.plyGame
{
    [AddComponentMenu("InventorySystem/Windows/plyCharacter")]
    [RequireComponent(typeof(UIWindow))]
    public class plyCharacterUI : CharacterUI
    {
        protected virtual List<ActorAttribute> plyAttributes
        {
            get
            {
                var actor = InventorySettingsManager.instance.playerObject.GetComponent<Actor>();
                return actor.actorClass.attributes;
            }
        }

        protected virtual string[] attributresStrings
        {
            get
            {
                var att = plyAttributes;

                string[] attributes = new string[att.Count];
                for (int i = 0; i < att.Count; i++)
                    attributes[i] = att[i].def.screenName;

                return attributes;
            }
        }



        /// <summary>
        /// Show the status
        /// </summary>
        public override void UpdateCharacterStats(bool repaint = true)
        {
            base.UpdateCharacterStats(false); // Repaint below...

            var a = plyAttributes;
            foreach (var stat in ItemManager.instance.plyAttributes)
            {
                if (stat.show == false)
                    continue;

                var l = new List<float>(items.Length);
                foreach (var item in items)
                {
                    // Handle the stats by attribute and category.
                    if (item.item != null)
                    {
                        var plyStat = a.FirstOrDefault(o => o.id.Value.ToString() == stat.id.Value.ToString());
                        if (plyStat != null)
                            l.Add(plyStat.Value);

                        //l.Add(a.FirstOrDefault(o => o.Value));
                        //l.Add((float) plyAttributes);
                    }
                }

                if (characterStats.ContainsKey(stat.category) == false)
                    characterStats.Add(stat.category, new List<InventoryEquipStatRowLookup>());

                // The stat
                string st = stat.formatter != null ? stat.formatter.FormatStat(l) : InventorySettingsManager.instance.defaultCharacterStatFormatter.FormatStat(l);
                float total = 0.0f;
                foreach (var item in l)
                    total += item;

                characterStats[stat.category].Add(new InventoryEquipStatRowLookup(GetPlyAttributeName(stat.id), total, st));
            }

            if (repaint)
                RepaintStats();
        }

        protected string GetPlyAttributeName(UniqueID id)
        {
            var a = plyAttributes.FirstOrDefault(o => o.id.Value.ToString() == id.Value.ToString());
            if (a == null || a.def == null)
                return string.Empty;

            return a.def.screenName;
        }

        //protected override void RepaintStats()
        //{
        //    if (window.isVisible == false || statusRowPrefab == null || statusCategoryPrefab == null)
        //        return;

        //    // Get rid of the old
        //    categoryPool.DestroyAll();
        //    rowsPool.DestroyAll();

        //    // Maybe make a pool for the items? See some spikes...
        //    foreach (var stat in characterStats)
        //    {
        //        // stat.Key is category
        //        // stat.Value is all items in category 
        //        var cat = categoryPool.Get();
        //        //cat.gameObject.SetActive(window.isVisible);
        //        cat.SetCategory(stat.Key);
        //        cat.transform.SetParent(statsContainer);
        //        cat.transform.localPosition = new Vector3(cat.transform.localPosition.x, cat.transform.localPosition.y, 0.0f);

        //        foreach (var s in stat.Value)
        //        {
        //            var obj = rowsPool.Get();
        //            //obj.gameObject.SetActive(window.isVisible);
        //            //var obj = GameObject.Instantiate<InventoryEquipStatRow>(statusRowPrefab);
        //            obj.SetRow(s.statName, s.finalValueString);

        //            obj.transform.SetParent(cat.container);
        //            obj.transform.localPosition = Vector3.zero; // UI Layout will handle it.
        //        }
        //    }
        //}
    }
}

#endif