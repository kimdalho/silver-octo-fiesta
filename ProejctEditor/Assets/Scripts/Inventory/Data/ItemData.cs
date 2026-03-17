using UnityEngine;

public enum ItemType
{
    Consumable,
    Equipment,
    MaterialItem
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public ItemType itemType;
    [TextArea] public string description;
    public int maxStack = 1;
}
