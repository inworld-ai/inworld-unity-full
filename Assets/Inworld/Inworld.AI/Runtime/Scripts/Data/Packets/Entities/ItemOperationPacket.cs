/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Inworld.Packet
{
    [Serializable]
    public class EntityItem
    {
        public string id;
        public string displayName;
        public string description;
        public Dictionary<string, string> properties;
        
        public EntityItem(string displayName, string description)
        {
            id = InworldAuth.Guid();
            this.displayName = displayName;
            this.description = description;
        }

        public EntityItem(string displayName, string description, Dictionary<string, string> properties)
        {
            id = InworldAuth.Guid();
            this.displayName = displayName;
            this.description = description;
            this.properties = properties;
        }
        
        public EntityItem(string id, string displayName, string description)
        {
            this.id = id;
            this.displayName = displayName;
            this.description = description;
        }

        public EntityItem(string id, string displayName, string description, Dictionary<string, string> properties)
        {
            this.id = id;
            this.displayName = displayName;
            this.description = description;
            this.properties = properties;
        }

        public override string ToString()
        {
            string value = $"EntityItem: {id}\n displayName: {displayName}\n description: {description}\n";
            if (properties != null)
            {
                value += " properties: ";
                foreach (var entry in properties)
                    value += "{" + $"{entry.Key}, {entry.Value}" + "} ";
                value += "\n";
            }
            return value;
        }
    }
    
    [Serializable]
    public class RemoveItemsOperation
    {
        public List<string> itemIds;
    }
    
    [Serializable]
    public class RemoveItemsOperationEvent : ItemsOperationEvent
    {
        public RemoveItemsOperation removeItems;

        public RemoveItemsOperationEvent(List<string> itemIDs)
        {
            removeItems = new RemoveItemsOperation
            {
                itemIds = itemIDs
            };
        }
    }
    
    [Serializable]
    public class ItemsInEntitiesOperation
    {
        public enum Type
        {
            UNSPECIFIED,
            ADD,
            REMOVE,
            REPLACE
        };
        [JsonConverter(typeof(StringEnumConverter))]
        public Type type;
        public List<string> itemIds;
        public List<string> entityNames;

        public ItemsInEntitiesOperation(Type type, List<string> itemIDs, List<string> entityNames)
        {
            this.type = type;
            this.itemIds = itemIDs;
            this.entityNames = entityNames;
        }
    }
    
    [Serializable]
    public class ItemsInEntitiesOperationEvent : ItemsOperationEvent
    {
        public ItemsInEntitiesOperation itemsInEntities;

        public ItemsInEntitiesOperationEvent(ItemsInEntitiesOperation.Type type, List<string> itemIDs, List<string> entityNames)
        {
            itemsInEntities = new ItemsInEntitiesOperation(type, itemIDs, entityNames);
        }
    }
    
    [Serializable]
    public class CreateOrUpdateItemsOperation
    {
        public List<EntityItem> items;
        public List<string> addToEntities;
    }

    [Serializable]
    public class CreateOrUpdateItemsOperationEvent : ItemsOperationEvent
    {
        public CreateOrUpdateItemsOperation createOrUpdateItems;

        public CreateOrUpdateItemsOperationEvent(List<EntityItem> items, List<string> addToEntities)
        {
            createOrUpdateItems = new CreateOrUpdateItemsOperation 
            {
                items = items,
                addToEntities = addToEntities
            };
        }
    }
    
    [Serializable]
    public class ItemsOperationEvent
    {

    }
    
    [Serializable]
    public class ItemOperationPacket : InworldPacket
    {
        public ItemsOperationEvent entitiesItemsOperation;
        
        public ItemOperationPacket()
        {

        }
        
        public override bool PrepareToSend()
        {
            UpdateRouting();
            return true;
        }
        
        protected override void UpdateRouting()
        {
            routing = new Routing("WORLD");
        }
    }
}
