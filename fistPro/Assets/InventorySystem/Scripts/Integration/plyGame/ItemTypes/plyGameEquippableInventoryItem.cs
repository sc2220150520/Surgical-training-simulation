#if PLY_GAME

using System;
using System.Collections.Generic;
using System.Linq;
using Devdog.InventorySystem;
using Devdog.InventorySystem.Models;
using plyCommon;
using plyGame;
using UnityEngine;

namespace Devdog.InventorySystem.Integration.plyGame
{
    public partial class plyGameEquippableInventoryItem : EquippableInventoryItem
    {
        public plyGameAttributeModifierModel[] plyAttributes = new plyGameAttributeModifierModel[0];

        public Actor actor
        {
            get
            {
                return InventorySettingsManager.instance.playerObject.GetComponent<Actor>();
            }
        }

        public override LinkedList<InfoBox.Row[]> GetInfo()
        {
            var info = base.GetInfo();

            var attributes = new InfoBox.Row[plyAttributes.Length];
            for (int i = 0; i < plyAttributes.Length; i++)
            {
                var a = actor.actorClass.attributes.FirstOrDefault(attribute => attribute.id.Value == plyAttributes[i].id.Value);
                if (a != null)
                    attributes[i] = new InfoBox.Row(a.def.screenName, plyAttributes[i].toAdd.ToString());
            }

            info.AddAfter(info.First, attributes.ToArray());

            return info;
        }


        public override bool Equip(InventoryEquippableField equipSlot, ItemCollectionBase equipToCollection)
        {
            bool equipped = base.Equip(equipSlot, equipToCollection);
            if (equipped == false)
                return false;

            if (actor.actorClass.currLevel < requiredLevel)
            {
                InventoryManager.instance.lang.itemCannotBeUsedLevelToLow.Show(name, description, requiredLevel);
                return false;
            }

            SetPlyGameValues();

            return true;
        }


        public override bool Unequip()
        {
            bool unequipped = base.Unequip();
            if (unequipped == false)
                return false;

            SetPlyGameValuesNegative();

            return true;
        }


        protected virtual void SetPlyGameValues()
        {
            foreach (var attr in plyAttributes)
            {
                var a = actor.actorClass.attributes.FirstOrDefault(attribute => attribute.id.Value == attr.id.Value);
                if (a != null)
                    a.ChangeSimpleBonus(attr.toAdd);
            }
        }
        protected virtual void SetPlyGameValuesNegative()
        {
            foreach (var attr in plyAttributes)
            {
                var a = actor.actorClass.attributes.FirstOrDefault(attribute => attribute.id.Value == attr.id.Value);
                if (a != null)
                    a.ChangeSimpleBonus(-attr.toAdd); // Remove it
            }
        }
    }
}

#endif