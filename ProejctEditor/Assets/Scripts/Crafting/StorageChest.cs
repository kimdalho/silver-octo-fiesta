using UnityEngine;

/// <summary>
/// 배치된 보관함의 아이템 데이터를 보관한다.
/// PlacedObject(Storage 타입)와 함께 프리팹에 붙인다.
/// </summary>
public class StorageChest : MonoBehaviour
{
    public const int Capacity = 24;
    public ItemStack[] slots = new ItemStack[Capacity];

    /// <summary>아이템 추가. 빈 슬롯 없으면 false.</summary>
    public bool AddItem(ItemData data, int count)
    {
        // 기존 스택에 합치기
        for (int i = 0; i < Capacity; i++)
        {
            if (slots[i] != null && slots[i].data == data && slots[i].count < data.maxStack)
            {
                int space = data.maxStack - slots[i].count;
                int add = Mathf.Min(count, space);
                slots[i].count += add;
                count -= add;
                if (count <= 0) return true;
            }
        }
        // 빈 슬롯
        for (int i = 0; i < Capacity; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = new ItemStack(data, count);
                return true;
            }
        }
        return false;
    }

    /// <summary>슬롯 전체 꺼내기. 슬롯은 null로.</summary>
    public ItemStack TakeSlot(int index)
    {
        if (index < 0 || index >= Capacity || slots[index] == null) return null;
        var taken = slots[index];
        slots[index] = null;
        return taken;
    }
}
