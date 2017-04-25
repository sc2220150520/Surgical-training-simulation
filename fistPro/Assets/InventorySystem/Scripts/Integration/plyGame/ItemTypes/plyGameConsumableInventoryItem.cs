#if PLY_GAME

using System;
using System.Collections.Generic;
using System.Linq;
using Devdog.InventorySystem;
using plyCommon;
using plyGame;
using UnityEngine;

namespace Devdog.InventorySystem.Integration.plyGame
{
    public partial class plyGameConsumableInventoryItem : InventoryItemBase
    {
        public plyGameAttributeModifierModel[] plyAttributes = new plyGameAttributeModifierModel[0];
        public AudioClip audioClipWhenUsed;
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
                if(a != null)
                    attributes[i] = new InfoBox.Row(a.def.screenName, plyAttributes[i].toAdd.ToString());
            }

            info.AddAfter(info.First, attributes.ToArray());

            return info;
        }

        public override int Use()
        {
            int used = base.Use();
            if (used < 0)
                return used;

            if (actor.actorClass.currLevel < requiredLevel)
            {
                InventoryManager.instance.lang.itemCannotBeUsedLevelToLow.Show(name, description, requiredLevel);
                return -1;
            }

            SetPlyGameValues();

            if(audioClipWhenUsed != null)
                InventoryUIUtility.AudioPlayOneShot(audioClipWhenUsed);

            currentStackSize--;
            NotifyItemUsed(1, true);
            return 1;
        }

        protected virtual void SetPlyGameValues()
        {
            foreach (var attr in plyAttributes)
            {
                var a = actor.actorClass.attributes.FirstOrDefault(attribute => attribute.id.Value == attr.id.Value);
                if (a != null)
                    a.SetConsumableValue(a.ConsumableValue + attr.toAdd, gameObject);
            }
        }
    }
}

#endif