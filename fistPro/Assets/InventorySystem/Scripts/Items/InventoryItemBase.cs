using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using Devdog.InventorySystem.Models;

namespace Devdog.InventorySystem
{
    public delegate void UsedItemItem(uint amount);
    public delegate void DroppedItemItem(Vector3 worldPosition);


    /// <summary>
    /// The base item of all the inventory items, contains some default behaviour for items, which can (almost) all be overriden.
    /// </summary>
    [RequireComponent(typeof(ObjectTriggererItem))]
    public partial class InventoryItemBase : MonoBehaviour
    {

        #region Item data

        #region Convenience properties
    
        public uint index { get; set; }
        public ItemCollectionBase itemCollection { get; set; }
    
        #endregion


        [SerializeField]
        [HideInInspector]
        private uint _id;
        /// <summary>
        /// Unique ID of the object
        /// </summary>
        public uint ID {
            get {
                return _id;
            }
            set {
                _id = value;
            }
        }

        [SerializeField]
        private string _name = "";
        /// <summary>
        /// Name of the object (does not have to be unique)
        /// </summary>
        public virtual new string name {
            get {
                return _name;
            }
            set {
                _name = value;
            }
        }

        [SerializeField]
        private string _description = "";
        /// <summary>
        /// Description of the object.
        /// </summary>
        public virtual string description
        {
            get {
                return _description;
            }
            set {
                _description = value;
            }
        }

        /// <summary>
        /// The category of this object, defines global cooldown and others.
        /// </summary>
        [SerializeField]
        [InventoryStat]
        public uint _category;
        public InventoryItemCategory category
        {
            get
            {
                return ItemManager.instance.itemCategories[_category];
            }
            set
            {
                _category = value.ID;
            }
        }

        /// <summary>
        /// Use the cooldown from the category? If true the global cooldown will be used, if false a unique cooldown can be set.
        /// </summary>
        [SerializeField]
        private bool _useCategoryCooldown = true;
        public bool useCategoryCooldown
        {
            get
            {
                return _useCategoryCooldown;
            }
            set
            {
                _useCategoryCooldown = value;
            }
        }

        [SerializeField]
        private Sprite _icon;
        /// <summary>
        /// The icon shown in the UI.
        /// </summary>
        public Sprite icon
        {
            get
            {
                return _icon;
            }
            set
            {
                _icon = value;
            }
        }



        [SerializeField]
        [InventoryStat]
        [Range(0.0f, 999.0f)]
        private float _weight;
        /// <summary>
        /// The weight of the object, KG / LBS / Stone whatever you want, as long as every object uses the same units.
        /// </summary>
        public float weight {
            get {
                return _weight;
            }
            set {
                _weight = value;
            }
        }

        [SerializeField]
        [InventoryStat]
        [Range(0, 100)]
        private uint _requiredLevel;
        /// <summary>
        /// The weight of the object, KG / LBS / Stone whatever you want, as long as every object uses the same units.
        /// </summary>
        public uint requiredLevel
        {
            get
            {
                return _requiredLevel;
            }
            set
            {
                _requiredLevel = value;
            }
        }


        [SerializeField]
        [InventoryStat]
        public uint _rarity = 0;
        /// <summary>
        /// Raritys can be managed through the editor, 
        /// </summary>
        public InventoryItemRarity rarity
        {
            get
            {
                if (_rarity < 0 || _rarity >= ItemManager.instance.itemRaritys.Length)
                    return null;

                return ItemManager.instance.itemRaritys[_rarity];
            }
        }


        [SerializeField]
        private InventoryItemProperty[] _properties = new InventoryItemProperty[0];

        /// <summary>
        /// Item properties, to define your own custom data on items.
        /// If you have a property that repeats itself all the time consider making an itemType (check documentation)
        /// </summary>
        public InventoryItemProperty[] properties
        {   
            get
            {
                return _properties;
            }
            set
            {
                _properties = value;
            }
        }


        [SerializeField]
        [InventoryStat]
        private int _buyPrice;
        public int buyPrice {
            get {
                return _buyPrice;
            }
            set {
                _buyPrice = value;
            }
        }

        [SerializeField]
        [InventoryStat]
        private int _sellPrice;
        public int sellPrice {
            get {
                return _sellPrice;
            }
            set {
                _sellPrice = value;
            }
        }
	
        [SerializeField]
        private bool _isDroppable = true;
        /// <summary>
        /// Can the item be dropped?
        /// </summary>
        public bool isDroppable {
            get {
                return _isDroppable;
            }
            set {
                _isDroppable = value;
            }
        }

        [SerializeField]
        private bool _isSellable = true;
        /// <summary>
        /// Can the item be sold?
        /// </summary>
        public bool isSellable {
            get {
                return _isSellable;
            }
            set {
                _isSellable = value;
            }
        }

        [SerializeField]
        private bool _isStorable = true;
        /// <summary>
        /// Can the item be stored in a bank / or crate / etc.
        /// </summary>
        public bool isStorable {
            get {
                return _isStorable;
            }
            set {
                _isStorable = value;
            }
        }

        [SerializeField]
        [Range(1,999)]
        private uint _maxStackSize = 1;
        /// <summary>
        /// How many items fit in 1 pile / stack
        /// </summary>
        public uint maxStackSize {
            get {
                return _maxStackSize;
            }
            set {
                _maxStackSize = value;
            }
        }

        [NonSerialized]
        private uint _currentStackSize = 1;
        /// <summary>
        /// The current amount of items in this stack
        /// </summary>
        public uint currentStackSize
        {
            get
            {
                return _currentStackSize;
            }
            set
            {
                _currentStackSize = value;
            }
        }


        [SerializeField]
        private float _cooldownTime;
        /// <summary>
        /// The time an item is unusable after it is used.
        /// </summary>
        public float cooldownTime {
            get {
                return _cooldownTime;
            }
            set
            {
                _cooldownTime = value;
            }
        }

        /// <summary>
        /// Used to calculate if the cooldown is over. ((lastUsageTime + cooldown) > Time.TimeSinceStarted).
        /// Only used when useCategoryCooldown is false.
        /// </summary>
        [NonSerialized]
        private float _lastUsageTime;
        public float lastUsageTime {
            get {
                return _lastUsageTime;
            }
        }
        public bool isInCooldown
        {
            get
            {
                if(useCategoryCooldown)
                {
                    if (category.lastUsageTime == 0.0f)
                        return false;

                    if (Time.timeSinceLevelLoad - category.lastUsageTime < category.cooldownTime)
                        return true;
                
                    return false;
                }

                // If the has not been used before
                if (_lastUsageTime == 0.0f)
                    return false;

                //Debug.Log("Is it.. ? " + (Time.timeSinceLevelLoad - _lastUsageTime).ToString() + " cooldown is: " + cooldownTime);
                return Time.timeSinceLevelLoad - _lastUsageTime < cooldownTime;
            }
        }

        /// <summary>
        /// Value from 0 to ... that defines how far the cooldown is. 0 is just started 1 or higher means the cooldown is over.
        /// Use isInCooldown first to verify if item is in cooldown.
        /// </summary>
        public float cooldownFactor
        {
            get
            {
                if(useCategoryCooldown)
                {
                    float e = Time.timeSinceLevelLoad - category.lastUsageTime;
                    return e / category.cooldownTime;
                }

                float exp = Time.timeSinceLevelLoad - _lastUsageTime;
                return exp / _cooldownTime;
            }
        }


        #endregion

        #region Events

        public event UsedItemItem OnUsedItem;
        public event DroppedItemItem OnDroppedItem;

        #endregion


        /// <summary>
        /// Get the info of this box, useful when displaying this item.
        /// 
        /// Some elements are displayed by default, these are:
        /// Item icon
        /// Item name
        /// Item description
        /// Item rarity
        /// 
        /// </summary>
        /// <returns>
        /// Returns a LinkedList , which works as follows.
        /// Each InfoBox.Row is used to define a row / property of an item.
        /// Each row has a title and description, the color, font type, etc, can all be changed.
        /// </returns>
        public virtual LinkedList<InfoBox.Row[]> GetInfo()
        {
            var list = new LinkedList<InfoBox.Row[]>();
        
            list.AddLast(new InfoBox.Row[]{
                new InfoBox.Row("Weight", weight.ToString()),
                new InfoBox.Row("Required level", requiredLevel.ToString()),
                new InfoBox.Row("Category", category.name),
            });

            var extra = new List<InfoBox.Row>(3)
            {
                new InfoBox.Row("Sell price", InventorySettingsManager.instance.currencyFormatter.Format(sellPrice)),
                new InfoBox.Row("Buy price", InventorySettingsManager.instance.currencyFormatter.Format(buyPrice)),
            };

            if (isDroppable == false || isSellable == false || isStorable == false)
                extra.Add(new InfoBox.Row((!isDroppable ? "Not droppable" : "") + (!isSellable ? ", Not sellable" : "") + (!isStorable ? ", Not storable" : ""), Color.yellow));

            list.AddLast(extra.ToArray());

            var extraProperties = new List<InfoBox.Row>();
            foreach (var property in properties)
            {
                if(property.showInUI)
                {
                    extraProperties.Add(new InfoBox.Row(property.key, property.value, Color.white, property.uiColor));
                }
            }

            if(extraProperties.Count > 0)
                list.AddLast(extraProperties.ToArray());
        
            return list;
        }


        /// <summary>
        /// Returns a list of usabilities for this item, what can it do?
        /// </summary>
        public virtual IList<InventoryItemUsability> GetUsabilities()
        {
            var l = new List<InventoryItemUsability>(8);

            if(itemCollection.canUseFromCollection)
            {
                l.Add(new InventoryItemUsability("Use", (item) =>
                {
                    itemCollection[index].TriggerUse();
                }));
            }

            if(currentStackSize > 1 && itemCollection.canPutItemsInCollection)
            {
                l.Add(new InventoryItemUsability("Unstack", (item) =>
                {
                    itemCollection[index].TriggerUnstack();
                }));
            }

            if(isDroppable && itemCollection.canDropFromCollection)
            {
                l.Add(new InventoryItemUsability("Drop", (item) =>
                {
                    itemCollection[index].TriggerDrop(false);
                }));
            }

            return l;
        }


        /// <summary>
        /// Pickups the item and stores it in the Inventory.
        /// </summary>
        /// <returns>Returns 0 if item was stored, -1 if not, -2 for some other unknown reason.</returns>
        public virtual bool PickupItem(bool addToInventory = true)
        {
            if(addToInventory)
                return InventoryManager.AddItem(this);
        
            return true;
        }

        /// <summary>
        /// When an item is used, notify the object so that events can be fired.
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="alsoNotifyCollection">If the collection of the item didn't change in the process it's safe to notify the collection.</param>
        protected virtual void NotifyItemUsed(uint amount, bool alsoNotifyCollection)
        {
            if (itemCollection != null && alsoNotifyCollection)
                itemCollection.NotifyItemUsed(ID, index, amount); // Dont forget the collection

            if (OnUsedItem != null)
                OnUsedItem(amount);
        }


        /// <summary>
        /// Use the item, return true if the item was used, return false is the item was not used.
        /// When overriding this method, do not forget to call base.Use();
        /// <b>Note that the caller has to handle the UI repaint.</b>
        /// </summary>
        /// <param name="usedFromCollection">From what collection was this item used, can be null.</param>
        /// <param name="slot">The slot from where the item was used. -1 if slot coulnd't be determined.</param>
        /// <returns>Returns -1 if in timeout, returns -2 if item use failed, 0 is 0 items were used, 1 if 1 item was used, 2 if 2...</returns>
        public virtual int Use()
        {
            if (itemCollection != null)
            {
                bool overrideBehaviour = itemCollection.OverrideUseMethod(this);
                if (overrideBehaviour)
                    return -2;

                if (itemCollection.canUseFromCollection == false)
                    return -2;                
            }

            if(isInCooldown)
            {
                InventoryManager.instance.lang.itemIsInCooldown.Show(name, description, (useCategoryCooldown ? category.lastUsageTime + category.cooldownTime : lastUsageTime + cooldownTime) - Time.timeSinceLevelLoad, (useCategoryCooldown ? category.cooldownTime : cooldownTime));
                return -1;
            }

            if(useCategoryCooldown)
            {
                category.lastUsageTime = Time.timeSinceLevelLoad;
                return 0;
            }

            // Set the last used time, used to figure out if item is in cooldown
            _lastUsageTime = Time.timeSinceLevelLoad;

            return 0;
        }

        /// <summary>
        /// Drop item at the specified location.
        /// </summary>
        /// <param name="location">Location.</param>
        /// <returns>Returns the object that is dropped. <b>Dropped object does not have to be the same as this object.</b></returns>
        public virtual GameObject Drop(Vector3 location)
        {
            if(isDroppable == false || itemCollection.canDropFromCollection == false)
                return null;

            var settings = InventorySettingsManager.instance;
            float droppableFromDistanceUp = 10.0f; // Start at 10.0f
            if (settings.dropItemRaycastToGround)
            {
                // If there is something above the item, we can't move it up to raycast down, as this would place it on the collider above it. So first check how much we can go up...
                RaycastHit aboveHit;
                if (Physics.Raycast(location, Vector3.up, out aboveHit, 10.0f))
                {
                    float dist = Vector3.Distance(aboveHit.transform.position, location);
                    droppableFromDistanceUp = Mathf.Clamp(dist - 0.1f, 0.1f, 10.0f); // Needs to be at least a little above the ground
                }
            }


            GameObject dropObj = gameObject;
            if(rarity != null && rarity.dropObject != null)
            {
                // Drop a specific item whenever this is dropped
                //rarity.dropObject.CreateCopy<GameObject>();
                dropObj = GameObject.Instantiate<GameObject>(rarity.dropObject);
                var comp = dropObj.AddComponent(this.GetType());
            
                comp.CopyValuesFrom(this, this.GetType()); // Special function that ads an item and copies all values via reflection from source.            
            }

            // Drop item into the world
            dropObj.transform.SetParent(null);

            if (settings.dropAtMousePosition)
            {
                dropObj.transform.position = location;
            }
            else
            {
                // Drop according to offset
                dropObj.transform.position = settings.playerObject.transform.position;
                dropObj.transform.rotation = settings.playerObject.transform.rotation;
                dropObj.transform.Translate(settings.dropOffsetVector);
                dropObj.transform.rotation = Quaternion.identity;
            }
            

            if (settings.dropItemRaycastToGround)
            {
                RaycastHit hit;
                if (Physics.Raycast(location + (Vector3.up * droppableFromDistanceUp), Vector3.down, out hit, 25.0f))
                {
                    // place it on the ground
                    dropObj.transform.position = hit.point + (Vector3.up * 0.1f); // + a little offset to avoid it falling through the ground
                }
            }


            dropObj.SetActive(true);

            // Clear fold collection
            itemCollection.NotifyItemDropped(this, ID, currentStackSize, dropObj);
            itemCollection.SetItem(index, null);

            // Remove old stuff
            itemCollection = null;
            index = 0;

            if (OnDroppedItem != null)
                OnDroppedItem(location);

            return dropObj;
        }

        public override string ToString()
        {
            return name;
        }
    }
}