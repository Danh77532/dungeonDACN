using UnityEngine;

public enum ItemType { Consumable, Weapon, Equipment, Material }
public enum ItemRarity { Common, Uncommon, Rare, Epic, Legendary }

// Defines an item type; quantities and equipped state belong to the inventory at runtime.
[CreateAssetMenu(fileName = "ItemData", menuName = "Dungeon/Data/Item")]
public sealed class ItemData : ScriptableObject
{
    [Header("Identity")]
    public string itemId;
    public string displayName = "Item";
    [TextArea] public string description;
    public Sprite icon;
    public ItemType type;
    public ItemRarity rarity;

    [Header("Inventory")]
    [Min(1)] public int maxStack = 1;
    [Min(0)] public int sellValue;
}
