using System;
using UnityEngine;

// ── 반응 효과 ──────────────────────────────────────────────────
public enum ReactionEffect
{
    None,
    SlowMove,   // 이동 둔화 (effectValue = 속도 배율)
    StopMove,   // 이동 정지
    SpeedUp,    // 이동 가속 (effectValue = 속도 배율)
    Flee,       // 도주
    Stun,       // 경직 (effectValue = 지속 시간)
    Paralyze    // 마비 (effectValue = 지속 시간)
}

// ── 단일 속성 반응 ─────────────────────────────────────────────
[Serializable]
public struct AttributeReaction
{
    public AttributeType attribute;
    public float threshold;
    public string reactionName;
    [TextArea] public string description;
    public ReactionEffect effect;
    public float effectValue;
}

// ── 2속성 조합 → 완성 상태 ────────────────────────────────────
[Serializable]
public struct AttributeCombination
{
    public AttributeType attr1;
    public AttributeType attr2;
    public float threshold1;
    public float threshold2;
    public string completionStateName;
    [TextArea] public string stateDescription;
    public DropTable.DropEntry[] harvestDrops;
}

// ── 기절 후 지배 속성 결과 ────────────────────────────────────
[Serializable]
public struct AttributeDeathResult
{
    public AttributeType dominantAttribute;
    public DropTable.DropEntry[] drops;
}

// ── 우리 속성별 생산물 ────────────────────────────────────────
[Serializable]
public struct PenDropSet
{
    public AttributeType penType;
    public DropTable.DropEntry[] drops;
}

[CreateAssetMenu(menuName = "Entity/Monster")]
public class MonsterData : ScriptableObject
{
    [Header("기본 정보")]
    public string monsterName;
    public Sprite icon;
    public float maxHP = 50f;

    [Header("이동")]
    public float moveSpeed = 2f;
    public float fleeSpeed = 4f;
    public float detectionRange = 8f;
    public bool fleesFromPlayer = false;  // true 시 플레이어 감지 → 도주

    [Header("전투")]
    public float attackDamage = 10f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;

    [Header("속성 초기값")]
    public float initWater    = 0f;
    public float initFire     = 0f;
    public float initElectric = 0f;
    public float initSpore    = 0f;

    [Header("속성 반응 테이블")]
    public AttributeReaction[] reactions;

    [Header("2속성 조합 테이블")]
    public AttributeCombination[] combinations;

    [Header("생포 (전기 속성)")]
    public CapturedMonsterData capturedItemData;
    public float captureThreshold = 70f;

    [Header("기절 후 지배 속성 결과")]
    public AttributeDeathResult[] attributeDeathResults;
    public DropTable.DropEntry[] defaultDeathDrops;

    [Header("우리 생산물 (속성별)")]
    public PenDropSet[] penDropSets;

    [Header("배회 설정")]
    public float wanderRadius = 10f;
    public float wanderInterval = 4f;
}
