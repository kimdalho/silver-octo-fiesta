using System;
using UnityEngine;

public class PartyMember
{
    public string memberName;
    public float hp;
    public float hunger;       // 0~100
    public float protein;      // 0~100
    public float carbs;        // 0~100
    public float fat;          // 0~100
    public float magicPower;   // 0~100
    public GrowthStats growth = new GrowthStats();

    public event Action OnStatsChanged;

    public float MaxHP          => 50f + growth.MaxHPBonus;
    public float Weight         => (protein + carbs + fat) / 3f;
    public bool IsAlive         => hp > 0f;
    public float Attack         => 10f + growth.AttackBonus + protein * 0.3f;
    public float Defense        => fat * 0.2f + Weight * 0.1f;
    public float Evasion        => Mathf.Max(0f, 50f - Weight) * 0.3f;
    public float MagicCastChance => magicPower * 0.005f; // 0~50%

    public float XPMultiplier
    {
        get
        {
            if (hunger >= 80f) return 1.5f;
            if (hunger >= 50f) return 1.0f;
            if (hunger >= 30f) return 0.5f;
            return 0f;
        }
    }

    public PartyMember(string name)
    {
        memberName = name;
        hp = MaxHP;
        hunger = 100f;
    }

    public void TakeDamage(float amount)
    {
        hp = Mathf.Max(0f, hp - amount);
        OnStatsChanged?.Invoke();
    }

    public void Heal(float amount)
    {
        hp = Mathf.Min(MaxHP, hp + amount);
        OnStatsChanged?.Invoke();
    }

    public void OnFloorDescend()
    {
        protein    = Mathf.Max(0f, protein    - 10f);
        carbs      = Mathf.Max(0f, carbs      - 8f);
        fat        = Mathf.Max(0f, fat        - 2f);
        magicPower = Mathf.Max(0f, magicPower - 5f);
        if (hunger <= 0f) hp = Mathf.Max(0f, hp - 10f);
        OnStatsChanged?.Invoke();
    }

    public void Eat(FoodData food)
    {
        hunger     = Mathf.Min(100f, hunger     + food.hungerRestore);
        protein    = Mathf.Min(100f, protein    + food.protein);
        carbs      = Mathf.Min(100f, carbs      + food.carbs);
        fat        = Mathf.Min(100f, fat        + food.fat);
        magicPower = Mathf.Min(100f, magicPower + food.magicPower);
        if (food.fat > 0f) growth.AddConstitutionXP(food.fat * XPMultiplier);
        OnStatsChanged?.Invoke();
    }

    public void AddCombatXP(float baseXP)
    {
        float xp = baseXP * XPMultiplier;
        growth.AddStrengthXP(xp * (1f + protein / 100f));
        if (growth.IsMagicUnlocked || magicPower >= 20f)
            growth.AddMagicXP(xp * 0.5f * (magicPower / 100f));
    }

    public void ResetBodyStats()
    {
        protein = carbs = fat = magicPower = 0f;
        hunger = 100f;
        hp = MaxHP;
        OnStatsChanged?.Invoke();
    }
}
