using System;

public class Equipment
{
    public const int SlotCount = 3; // Weapon, Head, Body

    private EquipmentData[] slots = new EquipmentData[SlotCount];

    public event Action<EquipSlot> OnEquipChanged;

    public EquipmentData GetEquip(EquipSlot slot)
    {
        return slots[(int)slot];
    }

    /// <summary>
    /// 장비 장착. 기존 장비가 있으면 반환.
    /// </summary>
    public EquipmentData Equip(EquipmentData data)
    {
        int idx = (int)data.equipSlot;
        var previous = slots[idx];
        slots[idx] = data;
        OnEquipChanged?.Invoke(data.equipSlot);
        return previous;
    }

    /// <summary>
    /// 장비 해제. 해제된 장비 반환.
    /// </summary>
    public EquipmentData Unequip(EquipSlot slot)
    {
        int idx = (int)slot;
        var removed = slots[idx];
        slots[idx] = null;
        OnEquipChanged?.Invoke(slot);
        return removed;
    }

    public StatModifier[] GetAllModifiers()
    {
        var list = new System.Collections.Generic.List<StatModifier>();
        for (int i = 0; i < SlotCount; i++)
        {
            if (slots[i] != null && slots[i].statModifiers != null)
                list.AddRange(slots[i].statModifiers);
        }
        return list.ToArray();
    }
}
