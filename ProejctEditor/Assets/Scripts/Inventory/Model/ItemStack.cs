using System;

[Serializable]
public class ItemStack
{
    public ItemData data;
    public int count;

    public ItemStack(ItemData data, int count = 1)
    {
        this.data = data;
        this.count = count;
    }

    public bool CanStack(ItemData other)
    {
        return data == other && count < data.maxStack;
    }

    public int AddCount(int amount)
    {
        int space = data.maxStack - count;
        int added = Math.Min(amount, space);
        count += added;
        return amount - added; // 남은 수량 반환
    }
}
