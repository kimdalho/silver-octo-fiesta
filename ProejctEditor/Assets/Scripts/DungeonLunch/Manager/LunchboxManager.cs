using System;
using UnityEngine;

[Serializable]
public class LunchboxSlot
{
    public FoodData food;
    public float remainingTime;
    public bool IsEmpty   => food == null;
    public bool IsExpired => food != null && !food.isNonPerishable && remainingTime <= 0f;
}

public class LunchboxManager : MonoBehaviour
{
    public static LunchboxManager instance;
    public const int SlotCount = 4;
    public LunchboxSlot[] slots = new LunchboxSlot[SlotCount];

    public event Action OnLunchboxChanged;

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
        for (int i = 0; i < SlotCount; i++) slots[i] = new LunchboxSlot();
    }

    public bool AddFood(FoodData food)
    {
        for (int i = 0; i < SlotCount; i++)
        {
            if (!slots[i].IsEmpty) continue;
            slots[i].food = food;
            slots[i].remainingTime = food.expiryDuration;
            OnLunchboxChanged?.Invoke();
            return true;
        }
        return false;
    }

    public bool Consume(int index)
    {
        if (index < 0 || index >= SlotCount) return false;
        var slot = slots[index];
        if (slot.IsEmpty || slot.IsExpired) return false;
        PartyManager.instance?.ConsumeLunchbox(slot.food);
        slots[index] = new LunchboxSlot();
        OnLunchboxChanged?.Invoke();
        return true;
    }

    public void TickExpiry(float deltaTime, float multiplier = 1f)
    {
        for (int i = 0; i < SlotCount; i++)
        {
            var s = slots[i];
            if (!s.IsEmpty && !s.food.isNonPerishable)
                s.remainingTime -= deltaTime * multiplier;
        }
        OnLunchboxChanged?.Invoke();
    }
}
