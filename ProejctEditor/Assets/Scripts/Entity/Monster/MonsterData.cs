using System;
using UnityEngine;

// ── 속성 타입 ──
public enum AttributeType
{
    Moisture,       // 습기 (물 포탄)
    Heat,           // 열   (불 포탄)
    Conductivity,   // 전도 (전기 포탄)
    Growth,         // 생장 (파생: 습기+전도)
    Structure,      // 구조 (파생: 열+전도)
    Decay           // 부패 (파생: 습기+열)
}

// ── 반응 효과 ──
public enum ReactionEffect
{
    None,
    SlowMove,   // 이동 둔화 (effectValue = 속도 배율, 0.5 = 50%)
    StopMove,   // 이동 정지
    SpeedUp,    // 이동 가속 (effectValue = 속도 배율, 1.3 = 130%)
    Flee,       // 도주
    Stun,       // 경직 (effectValue = 지속 시간)
    Paralyze    // 마비 (effectValue = 지속 시간)
}

// ── 단일 속성 반응 ──
[Serializable]
public struct AttributeReaction
{
    public AttributeType attribute;
    public float threshold;             // 반응 발동 임계값 (0~100)
    public string reactionName;         // "축축해짐", "과열" 등
    [TextArea] public string description; // 외형/행동 변화 설명
    public ReactionEffect effect;
    public float effectValue;           // 효과 수치
}

// ── 2속성 조합 → 완성 상태 ──
[Serializable]
public struct AttributeCombination
{
    public AttributeType attr1;
    public AttributeType attr2;
    public float threshold1;            // attr1 필요 임계값
    public float threshold2;            // attr2 필요 임계값
    public string completionStateName;  // "발아", "결정화" 등
    [TextArea] public string stateDescription;
    public DropTable.DropEntry[] harvestDrops; // 수확 시 보상
}

[CreateAssetMenu(menuName = "Entity/Monster")]
public class MonsterData : ScriptableObject
{
    [Header("기본 정보")]
    public string monsterName;
    public Sprite icon;
    public float maxHP = 50f;
    public float moveSpeed = 2f;

    [Header("속성 초기값")]
    public float initMoisture = 0f;
    public float initHeat = 0f;
    public float initConductivity = 0f;

    [Header("속성 반응 테이블 (몬스터별 고유)")]
    public AttributeReaction[] reactions;

    [Header("2속성 조합 테이블 (몬스터별 고유)")]
    public AttributeCombination[] combinations;

    [Header("AI 행동 (간소화)")]
    public float wanderRadius = 10f;
    public float wanderInterval = 4f;
    public float fleeSpeed = 5f;
}
