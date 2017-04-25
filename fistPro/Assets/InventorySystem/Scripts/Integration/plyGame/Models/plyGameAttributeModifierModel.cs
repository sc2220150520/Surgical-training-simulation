#if PLY_GAME

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using plyCommon;

namespace Devdog.InventorySystem.Integration.plyGame
{
    [System.Serializable]
    public partial class plyGameAttributeModifierModel
    {
        public UniqueID id;
        public int toAdd = 0;
    }
}

#endif