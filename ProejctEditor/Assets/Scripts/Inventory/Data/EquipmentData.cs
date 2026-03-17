using UnityEngine;

public enum EquipSlot
{
    Weapon,
    Head,
    Body
}

[CreateAssetMenu(fileName = "NewEquipment", menuName = "Inventory/Equipment")]
public class EquipmentData : ItemData
{
    public EquipSlot equipSlot;
    public StatModifier[] statModifiers;

    private void OnValidate()
    {
        itemType = ItemType.Equipment;
        maxStack = 1;
    }
}
