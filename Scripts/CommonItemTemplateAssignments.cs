using UnityEngine;
using System.Collections.Generic;
using Items;
using System;

[CreateAssetMenu(fileName = "ItemsDictionary", menuName = "Inventory/ItemsDictionary")]
public class ItemsDictionary : ScriptableObject
{
    public List<ItemDefinition> itemDefinitions = new();
    public static Dictionary<string, Item> CommonItemDictionary = new();
    public List<ItemIconMapping> itemIconMappings;
    public static Dictionary<string, Sprite> IconDictionary = new();

    public const string UnknownIcon = "???";
    public const string Fist = "Fist";
    public const string Bottle = "Bottle";
    public const string Thingy = "Thingy";

    [Serializable]
    public class ItemIconMapping
    {
        public string itemName;
        public Sprite icon;
    }

    private void OnEnable()
    {
        CommonItemDictionary.Clear();
        IconDictionary.Clear();

        foreach (var itemTemps in itemDefinitions)
        {
            Item itemInstantiation = new(itemTemps);
            string itemName = itemInstantiation.Name.Replace(" ", "").Replace("-", "");

            CommonItemDictionary[itemName] = itemInstantiation;
        }

        foreach (var mapping in itemIconMappings)
        {
            if (!string.IsNullOrEmpty(mapping.itemName) && mapping.icon != null)
            {
                IconDictionary[mapping.itemName] = mapping.icon;
            }

        }
    }
}