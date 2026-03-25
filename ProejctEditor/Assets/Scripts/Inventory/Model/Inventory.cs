using System;

public class Inventory
{
    public const int Size = 15;
    public ItemStack[] slots = new ItemStack[Size];

    public event Action<int> OnSlotChanged;

    public bool AddItem(ItemData data, int count = 1)
    {
        // 기존 스택에 쌓기 시도
        for (int i = 0; i < Size; i++)
        {
            if (slots[i] != null && slots[i].CanStack(data))
            {
                count = slots[i].AddCount(count);
                OnSlotChanged?.Invoke(i);
                if (count <= 0) return true;
            }
        }

        // 빈 슬롯에 넣기
        while (count > 0)
        {
            int emptyIndex = FindEmptySlot();
            if (emptyIndex < 0) return false;

            int toAdd = Math.Min(count, data.maxStack);
            slots[emptyIndex] = new ItemStack(data, toAdd);
            count -= toAdd;
            OnSlotChanged?.Invoke(emptyIndex);
        }
        return true;
    }

    public ItemStack RemoveItem(int index)
    {
        if (index < 0 || index >= Size || slots[index] == null) return null;

        var removed = slots[index];
        slots[index] = null;
        OnSlotChanged?.Invoke(index);
        return removed;
    }

    public void SetSlot(int index, ItemStack stack)
    {
        slots[index] = stack;
        OnSlotChanged?.Invoke(index);
    }

    public void ClearAll()
    {
        for (int i = 0; i < Size; i++)
        {
            slots[i] = null;
            OnSlotChanged?.Invoke(i);
        }
    }

    public void SwapSlots(int a, int b)
    {
        (slots[a], slots[b]) = (slots[b], slots[a]);
        OnSlotChanged?.Invoke(a);
        OnSlotChanged?.Invoke(b);
    }

    /// <summary>
    /// 특정 슬롯에서 아이템 1개 소모. 포탄 발사 등에 사용.
    /// </summary>
    public bool ConsumeOne(int index)
    {
        if (index < 0 || index >= Size || slots[index] == null) return false;

        slots[index].count--;
        if (slots[index].count <= 0)
            slots[index] = null;
        OnSlotChanged?.Invoke(index);
        return true;
    }

    private int FindEmptySlot()
    {
        for (int i = 0; i < Size; i++)
            if (slots[i] == null) return i;
        return -1;
    }
}
