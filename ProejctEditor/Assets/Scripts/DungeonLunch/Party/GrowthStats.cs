using System;

[Serializable]
public class GrowthStats
{
    private const float XP_PER_LEVEL = 100f;

    public int strengthLevel;
    public float strengthXP;
    public int constitutionLevel;
    public float constitutionXP;
    public int magicLevel;
    public float magicXP;

    public event Action OnLevelUp;

    public void AddStrengthXP(float amount)
    {
        strengthXP += amount;
        while (strengthXP >= XP_PER_LEVEL) { strengthXP -= XP_PER_LEVEL; strengthLevel++; OnLevelUp?.Invoke(); }
    }

    public void AddConstitutionXP(float amount)
    {
        constitutionXP += amount;
        while (constitutionXP >= XP_PER_LEVEL) { constitutionXP -= XP_PER_LEVEL; constitutionLevel++; OnLevelUp?.Invoke(); }
    }

    public void AddMagicXP(float amount)
    {
        magicXP += amount;
        while (magicXP >= XP_PER_LEVEL) { magicXP -= XP_PER_LEVEL; magicLevel++; OnLevelUp?.Invoke(); }
    }

    public int AttackBonus      => strengthLevel * 3;
    public int MaxHPBonus       => constitutionLevel * 10;
    public int MagicDamageBonus => magicLevel * 5;
    public bool IsMagicUnlocked => magicLevel >= 20;
}
