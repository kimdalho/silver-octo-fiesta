using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 몬스터 런타임 속성 누적 + 반응/완성 상태 판정.
/// 포탄 명중 시 ApplyAttribute()를 호출한다.
/// SimpleMapGenerator 또는 MonsterBehavior가 Init()으로 데이터를 주입한다.
/// </summary>
public class MonsterAttributeState : MonoBehaviour
{
    public MonsterData data;

    // 속성값 [0~100], AttributeType 인덱스로 접근
    private float[] values = new float[6];

    // 이미 발동된 반응 (중복 방지)
    private HashSet<string> triggeredReactions = new HashSet<string>();

    // 완성 상태 (Water 조합 수확)
    public bool IsCompleted { get; private set; }
    private int completedComboIndex = -1;

    // 생포 상태 (Electric 누적)
    public bool IsCaptured { get; private set; }

    // 현재 반응 효과
    public ReactionEffect CurrentEffect { get; private set; }
    public float CurrentEffectValue { get; private set; }
    private float effectTimer;

    // 이벤트 (ReactionFeedback 등이 구독)
    public Action<AttributeReaction>   OnReactionTriggered;
    public Action<AttributeCombination> OnCompletionTriggered;

    void Start()
    {
        if (data != null) Init(data);
    }

    public void Init(MonsterData monsterData)
    {
        data = monsterData;
        values = new float[6];
        values[(int)AttributeType.Water]    = data.initWater;
        values[(int)AttributeType.Fire]     = data.initFire;
        values[(int)AttributeType.Electric] = data.initElectric;

        // 최초 조우 등록
        CreatureCodex.instance?.RegisterEncounter(data);
    }

    /// <summary>포탄 명중 시 Projectile이 호출.</summary>
    public void ApplyAttribute(AttributeType type, float amount)
    {
        int idx = (int)type;
        values[idx] = Mathf.Clamp(values[idx] + amount, 0f, 100f);

        CheckReactions();
        if (!IsCompleted) CheckCombinations();
        if (!IsCaptured) CheckCapture();
    }

    public float GetValue(AttributeType type) => values[(int)type];

    public AttributeCombination GetCompletedCombination()
        => data.combinations[completedComboIndex];

    // ── 반응 체크 ────────────────────────────────────────────

    void CheckReactions()
    {
        if (data?.reactions == null) return;

        foreach (var r in data.reactions)
        {
            if (triggeredReactions.Contains(r.reactionName)) continue;
            if (values[(int)r.attribute] < r.threshold) continue;

            triggeredReactions.Add(r.reactionName);
            ApplyEffect(r.effect, r.effectValue);
            CreatureCodex.instance?.RegisterReaction(data, r.reactionName);
            OnReactionTriggered?.Invoke(r);
        }
    }

    // ── 완성 상태 체크 ────────────────────────────────────────

    void CheckCombinations()
    {
        if (data?.combinations == null) return;

        for (int i = 0; i < data.combinations.Length; i++)
        {
            var combo = data.combinations[i];
            if (values[(int)combo.attr1] < combo.threshold1) continue;
            if (values[(int)combo.attr2] < combo.threshold2) continue;

            IsCompleted = true;
            completedComboIndex = i;
            OnCompletionTriggered?.Invoke(combo);
            break;
        }
    }

    // ── 생포 체크 (Electric) ──────────────────────────────────

    void CheckCapture()
    {
        if (data == null || data.capturedItemData == null) return;
        if (values[(int)AttributeType.Electric] < data.captureThreshold) return;

        IsCaptured = true;

        var inv = InventoryManager.instance?.inventory;
        inv?.AddItem(data.capturedItemData, 1);

        CreatureCodex.instance?.RegisterHarvest(data, "생포");
        Debug.Log($"[생포] {data.monsterName} → 인벤토리");

        InteractHintUI.instance?.Hide();
        Destroy(gameObject);
    }

    // ── 반응 효과 적용 ────────────────────────────────────────

    void ApplyEffect(ReactionEffect effect, float value)
    {
        CurrentEffect = effect;
        CurrentEffectValue = value;

        // Stun/Paralyze는 duration 타이머
        if (effect == ReactionEffect.Stun || effect == ReactionEffect.Paralyze)
            effectTimer = value;

        // MonsterBehavior는 CurrentEffect를 매 프레임 폴링 → 별도 호출 불필요
    }

    void Update()
    {
        if (effectTimer > 0f)
        {
            effectTimer -= Time.deltaTime;
            if (effectTimer <= 0f)
                CurrentEffect = ReactionEffect.None;
        }
    }
}
