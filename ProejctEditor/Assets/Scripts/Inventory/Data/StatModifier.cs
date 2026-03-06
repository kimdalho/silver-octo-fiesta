using System;

public enum StatType
{
    MaxHP,
    Attack,
    Defense,
    MoveSpeed
}

public enum ModifierType
{
    Flat,
    Percent
}

[Serializable]
public struct StatModifier
{
    public StatType statType;
    public ModifierType modifierType;
    public float value;

    public StatModifier(StatType statType, ModifierType modifierType, float value)
    {
        this.statType = statType;
        this.modifierType = modifierType;
        this.value = value;
    }
}
