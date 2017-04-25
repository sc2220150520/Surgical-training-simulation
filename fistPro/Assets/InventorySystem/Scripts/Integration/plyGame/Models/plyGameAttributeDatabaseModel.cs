#if PLY_GAME

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using plyCommon;

namespace Devdog.InventorySystem.Integration.plyGame
{
    [System.Serializable]
    public partial class plyGameAttributeDatabaseModel
    {
        public UniqueID id;
        public bool show;
        public string category;

        /// <summary>
        /// The formatter defines how the item should be displayed.
        /// </summary>
        public CharacterStatFormatterBase formatter;


        public plyGameAttributeDatabaseModel(UniqueID id, string category, bool show, CharacterStatFormatterBase formatter)
        {
            this.id = id;
            this.category = category;
            this.show = show;
            this.formatter = formatter;
        }
    }
}

#endif