using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventorySlot
{
    public FoodData item;
    public int count;
    public float remainingTime;
    public bool IsExpired => !item.isNonPerishable && remainingTime <= 0f;
}

public class DungeonInventoryManager : MonoBehaviour
{
    public static DungeonInventoryManager instance;

    private readonly List<InventorySlot> _slots = new List<InventorySlot>();
    public IReadOnlyList<InventorySlot> Slots => _slots;
    public int gold;

    public event Action OnInventoryChanged;

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddItem(FoodData item, int count = 1)
    {
        var existing = _slots.Find(s => s.item == item);
        if (existing != null) existing.count += count;
        else _slots.Add(new InventorySlot { item = item, count = count, remainingTime = item.expiryDuration });
        OnInventoryChanged?.Invoke();
    }

    public bool RemoveItem(FoodData item, int count = 1)
    {
        var slot = _slots.Find(s => s.item == item && s.count >= count);
        if (slot == null) return false;
        slot.count -= count;
        if (slot.count <= 0) _slots.Remove(slot);
        OnInventoryChanged?.Invoke();
        return true;
    }

    public void HalveInventory()
    {
        foreach (var s in _slots) s.count = Mathf.CeilToInt(s.count / 2f);
        gold /= 2;
        _slots.RemoveAll(s => s.count <= 0);
        OnInventoryChanged?.Invoke();
    }

    public void TickExpiry(float deltaTime, float multiplier = 1f)
    {
        foreach (var s in _slots)
            if (!s.item.isNonPerishable) s.remainingTime -= deltaTime * multiplier;
        _slots.RemoveAll(s => s.IsExpired);
        OnInventoryChanged?.Invoke();
    }
}
