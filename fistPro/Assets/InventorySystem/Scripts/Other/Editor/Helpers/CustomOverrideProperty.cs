using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Devdog.InventorySystem.Editors
{
    public class CustomOverrideProperty
    {
        public string serializedName { get; set; }
        public Action action;

        public CustomOverrideProperty(string serializedName, Action action)
        {
            this.serializedName = serializedName;
            this.action = action;
        }
    }
}
