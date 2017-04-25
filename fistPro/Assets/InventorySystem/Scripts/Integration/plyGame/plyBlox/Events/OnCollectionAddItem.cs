#if PLY_GAME__

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using plyBloxKit;
using plyGame;

namespace Devdog.InventorySystem.Integration.plyGame.plyBlox
{
    
    [plyEvent("InventorySystem/On collection add item", "OnCollectionAddItem", Description = "Called when the value of an Attribute of the Actor changes.")]
    public class OnCollectionAddItemEvent : plyEvent
    {
        public override void Run()
        {
            base.Run();


        }

        public override System.Type HandlerType()
        {
            // here the Event is linked to the correct handler
            return typeof(CollectionEventHandler);
        }
    }
}

#endif