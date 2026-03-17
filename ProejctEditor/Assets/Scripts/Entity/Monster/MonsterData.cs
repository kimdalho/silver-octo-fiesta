using UnityEngine;

[CreateAssetMenu(menuName = "Entity/Monster")]
public class MonsterData : ScriptableObject
{
    [Header("기본")]
    public float maxHP = 50f;
    public float moveSpeed = 2f;
    public float fleeSpeed = 5f;

    [Header("감지")]
    public float detectRange = 15f;
    public float fleeRange = 7f;
    public float safeRange = 25f;

    [Header("경계")]
    public float alertDuration = 5f;

    [Header("폭주")]
    public float rampageSpeed = 6f;
    public float rampageDuration = 8f;
    public float rampageAttack = 10f;

    [Header("관찰/포획")]
    public float observeTime = 5f;
    public float baseCaptureChance = 0.3f;
    public float trapBonus = 0.15f;
    public float observeBonus = 0.2f;

    public DropTable.DropEntry[] drops;
}
