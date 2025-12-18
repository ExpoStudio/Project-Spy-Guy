using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Items
{
    public enum ItemType
    {
        None,
        Weapon,
        Key,
        Throwable,
        Armor,
        Potion,
        CaseObjectiveItem,
        Miscellaneous
    }

    /// <summary>
    /// Represents a preset for an item in the game.
    /// </summary>
    [CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
    [Serializable]
    public class ItemDefinition : ScriptableObject
    {
        public string Name;
        public ItemType Type;
        public int Value;
        public bool DuplicatesAllowed;
        public bool IsStackable;
        public bool CanDrop;
        public string Description;
        public int MaxValue = 1;
        public Sprite Icon;
    }

    /// <summary>
    /// Represents an actual item in the game.
    /// </summary>
    [Serializable]
    public class Item
    {
        public ItemDefinition Definition;
        public string Name;
        public ItemType Type;
        public int Value;
        public bool DuplicatesAllowed;
        public bool IsStackable;
        public bool CanDrop;
        public string Description;
        public int MaxValue;
        public Sprite Icon;

        /// <summary>
        /// Create item from scratch
        /// </summary>
        public Item(string name, ItemType type, int value, bool duplicatesAllowed = false, bool isStackable = false, bool canDrop = true, string description = "", int maxValue = 1, string iconReference = null)
        {
            Name = name;
            Type = type;
            Value = value >= 0 ? value : 0;
            DuplicatesAllowed = duplicatesAllowed;
            IsStackable = isStackable;
            CanDrop = canDrop;
            Description = description;
            MaxValue = maxValue >= 1 ? maxValue : 1;
            if (!string.IsNullOrEmpty(iconReference) && ItemsDictionary.IconDictionary.TryGetValue(iconReference, out var foundIcon))
            {
                Icon = foundIcon;
            }
            else if (ItemsDictionary.IconDictionary.TryGetValue(ItemsDictionary.UnknownIcon, out var fallbackIcon))
            {
                Icon = fallbackIcon;
            }
            else
            {
                Icon = null; // Or assign a hardcoded default
            }
        }

        /// <summary>
        /// Overloaded constructor for creating overrides to the preset item.
        /// </summary>
        public Item(ItemDefinition preset, string name, ItemType? type = null, int? value = null, bool? duplicatesAllowed = null, bool? isStackable = null, bool? canDrop = null, string description = "", int? maxValue = null, string iconReference = null)
        {
            Name = name ?? (preset != null ? preset.Name : null);
            Type = type ?? (preset != null ? preset.Type : ItemType.None);
            Value = value.HasValue && value.Value >= 0
                ? value.Value
                : (preset != null ? preset.Value : 0);
            DuplicatesAllowed = duplicatesAllowed ?? (preset != null && preset.DuplicatesAllowed);
            IsStackable = isStackable ?? (preset != null && preset.IsStackable);
            CanDrop = canDrop ?? (preset == null || preset.CanDrop);
            Description = !string.IsNullOrEmpty(description)
                ? description
                : (preset != null ? preset.Description : string.Empty);
            MaxValue = maxValue.HasValue && maxValue.Value >= 1
                ? maxValue.Value
                : (preset != null ? preset.MaxValue : 1);


            if (!string.IsNullOrEmpty(iconReference) && ItemsDictionary.IconDictionary.TryGetValue(iconReference, out var foundIcon))
            {
                Icon = foundIcon;
            }
            else if (string.IsNullOrEmpty(iconReference) && preset.Icon != null)
            {
                Icon = preset.Icon;
            }
            else if (ItemsDictionary.IconDictionary.TryGetValue(ItemsDictionary.UnknownIcon, out var unknownIcon))
            {
                Icon = unknownIcon;
            }
            else
            {
                Icon = null; // or assign a hardcoded fallback
            }
        }

        public Item(ItemDefinition preset)
        {
            if (preset == null) return;

            Name = preset.Name;
            Type = preset.Type;
            Value = preset.Value;
            DuplicatesAllowed = preset.DuplicatesAllowed;
            IsStackable = preset.IsStackable;
            CanDrop = preset.CanDrop;
            Description = preset.Description;
            MaxValue = preset.MaxValue;
            Icon = preset.Icon;
        }

        public string GetName()
        {
            return $"{Name}";
        }
    }

    public class ItemUtility
    {
        public static string GetItemTypeName(ItemType type)
        {
            return type switch
            {
                ItemType.None => "None",
                ItemType.Weapon => "Weapon",
                ItemType.Key => "Key",
                ItemType.Throwable => "Throwable",
                ItemType.Armor => "Armor",
                ItemType.Potion => "Potion",
                ItemType.CaseObjectiveItem => "Case Objective Item",
                ItemType.Miscellaneous => "Miscellaneous",
                _ => "Unknown"
            };
        }
        public static ItemType GetItemTypeFromString(string typeName)
        {
            return typeName switch
            {
                "None" => ItemType.None,
                "Weapon" => ItemType.Weapon,
                "Key" => ItemType.Key,
                "Throwable" => ItemType.Throwable,
                "Armor" => ItemType.Armor,
                "Potion" => ItemType.Potion,
                "Case Objective Item" => ItemType.CaseObjectiveItem,
                "Miscellaneous" => ItemType.Miscellaneous,
                _ => ItemType.None
            };
        }

        public static bool HasItem(PlayerInput player, Item obj)
        {
            if (obj == null || !player.TryGetComponent(out Inventory inventory))
                return false;

            return inventory.PlayerInventory.Any(item =>
                item != null &&
                item.Item != null &&
                item.Item.Name == obj.Name &&
                item.Item.Type == obj.Type
            );
        }

        public static bool HasItem(PlayerInput player, string itemName)
        {
            if (string.IsNullOrEmpty(itemName) || !player.TryGetComponent(out Inventory inventory))
                return false;

            return inventory.PlayerInventory.Any(item =>
                item != null &&
                item.Item != null &&
                item.Item.Name == itemName
            );
        }
    }
}