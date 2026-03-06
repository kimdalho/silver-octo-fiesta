using UnityEngine;

public class EquipmentUI : MonoBehaviour
{
    public SlotUI weaponSlot;
    public SlotUI headSlot;
    public SlotUI bodySlot;

    private SlotUI[] slots;
    private bool initialized;

    void Start()
    {
        slots = new[] { weaponSlot, headSlot, bodySlot };

        weaponSlot.isEquipmentSlot = true;
        weaponSlot.equipSlot = EquipSlot.Weapon;

        headSlot.isEquipmentSlot = true;
        headSlot.equipSlot = EquipSlot.Head;

        bodySlot.isEquipmentSlot = true;
        bodySlot.equipSlot = EquipSlot.Body;

        TryInit();
    }

    void Update()
    {
        if (!initialized) TryInit();
    }

    void TryInit()
    {
        if (initialized) return;
        if (InventoryManager.instance == null) return;

        InventoryManager.instance.equipment.OnEquipChanged += RefreshSlot;
        initialized = true;
    }

    void OnDestroy()
    {
        if (InventoryManager.instance != null)
            InventoryManager.instance.equipment.OnEquipChanged -= RefreshSlot;
    }

    private void RefreshSlot(EquipSlot slot)
    {
        var data = InventoryManager.instance.equipment.GetEquip(slot);
        var ui = slots[(int)slot];

        if (data != null)
            ui.SetItem(new ItemStack(data));
        else
            ui.Clear();
    }

    public void RefreshAll()
    {
        for (int i = 0; i < Equipment.SlotCount; i++)
            RefreshSlot((EquipSlot)i);
    }
}
