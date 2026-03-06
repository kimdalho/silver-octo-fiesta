using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    public Inventory inventory { get; private set; }
    public Equipment equipment { get; private set; }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        inventory = new Inventory();
        equipment = new Equipment();
    }

    public bool AddItem(ItemData data, int count = 1)
    {
        return inventory.AddItem(data, count);
    }

    /// <summary>
    /// 인벤토리 → 장비 장착. 기존 장비는 인벤으로 되돌림.
    /// </summary>
    public bool EquipFromInventory(int inventoryIndex)
    {
        var stack = inventory.slots[inventoryIndex];
        if (stack == null) return false;

        var equipData = stack.data as EquipmentData;
        if (equipData == null) return false;

        var previous = equipment.Equip(equipData);
        inventory.RemoveItem(inventoryIndex);

        if (previous != null)
            inventory.AddItem(previous);

        return true;
    }

    /// <summary>
    /// 장비 해제 → 인벤토리로.
    /// </summary>
    public bool UnequipToInventory(EquipSlot slot)
    {
        var equipData = equipment.GetEquip(slot);
        if (equipData == null) return false;

        // 인벤 빈칸 확인
        bool hasSpace = false;
        for (int i = 0; i < Inventory.Size; i++)
        {
            if (inventory.slots[i] == null) { hasSpace = true; break; }
        }
        if (!hasSpace) return false;

        equipment.Unequip(slot);
        inventory.AddItem(equipData);
        return true;
    }

    /// <summary>
    /// 드래그로 장비슬롯에 직접 놓기.
    /// </summary>
    public bool EquipDirect(ItemStack stack, EquipSlot targetSlot, out ItemStack returned)
    {
        returned = null;
        if (stack == null) return false;

        var equipData = stack.data as EquipmentData;
        if (equipData == null) return false;
        if (equipData.equipSlot != targetSlot) return false;

        var previous = equipment.Equip(equipData);
        if (previous != null)
            returned = new ItemStack(previous);

        return true;
    }
}
