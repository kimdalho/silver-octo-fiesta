using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 몬스터 런타임 속성 누적 + 반응/완성/기절 처리.
///
/// HP 0 → Damageable.OnKnockedOut → HandleKnockedOut():
///   - Electric >= captureThreshold → 생포 (인벤 추가 + Destroy)
///   - 그 외 → 지배 속성 기준 attributeDeathResults 드랍 → Destroy
/// </summary>
public class MonsterAttributeState : MonoBehaviour
{
    public MonsterData data;

    private static readonly int AttrCount = Enum.GetValues(typeof(AttributeType)).Length;
    private float[] values;

    private HashSet<string> triggeredReactions = new HashSet<string>();

    public bool IsCompleted { get; private set; }
    private int completedComboIndex = -1;

    public ReactionEffect CurrentEffect    { get; private set; }
    public float          CurrentEffectValue { get; private set; }
    private float effectTimer;

    public Action<AttributeReaction>    OnReactionTriggered;
    public Action<AttributeCombination> OnCompletionTriggered;
    public Action<AttributeType, float> OnAttributeApplied;  // 속성값 적용 직후

    void Start()
    {
        if (data != null) Init(data);
    }

    public void Init(MonsterData monsterData)
    {
        data = monsterData;
        values = new float[AttrCount];
        values[(int)AttributeType.Water]    = data.initWater;
        values[(int)AttributeType.Fire]     = data.initFire;
        values[(int)AttributeType.Electric] = data.initElectric;
        values[(int)AttributeType.Spore]    = data.initSpore;

        CreatureCodex.instance?.RegisterEncounter(data);

        // Damageable 기절 이벤트 구독
        var dmg = GetComponent<Damageable>();
        if (dmg != null)
            dmg.OnKnockedOut += HandleKnockedOut;
    }

    void OnDestroy()
    {
        var dmg = GetComponent<Damageable>();
        if (dmg != null)
            dmg.OnKnockedOut -= HandleKnockedOut;
    }

    public void ApplyAttribute(AttributeType type, float amount)
    {
        if (values == null) return;

        int idx = (int)type;
        values[idx] = Mathf.Clamp(values[idx] + amount, 0f, 100f);

        OnAttributeApplied?.Invoke(type, amount);

        CheckReactions();
        if (!IsCompleted) CheckCombinations();
    }

    public float GetValue(AttributeType type) => values != null ? values[(int)type] : 0f;

    public AttributeCombination GetCompletedCombination()
        => data.combinations[completedComboIndex];

    // ── 지배 속성 ─────────────────────────────────────────────

    public AttributeType GetDominantAttribute()
    {
        if (values == null) return (AttributeType)(-1);

        int   maxIdx = -1;
        float maxVal = 0f;
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] > maxVal) { maxVal = values[i]; maxIdx = i; }
        }
        return maxIdx >= 0 ? (AttributeType)maxIdx : (AttributeType)(-1);
    }

    // ── 기절 처리 ─────────────────────────────────────────────

    void HandleKnockedOut()
    {
        if (data == null) { Destroy(gameObject); return; }

        // 전기 임계치 → 생포 우선
        if (data.capturedItemData != null
            && values[(int)AttributeType.Electric] >= data.captureThreshold)
        {
            InventoryManager.instance?.inventory.AddItem(data.capturedItemData, 1);
            CreatureCodex.instance?.RegisterHarvest(data, "생포");
            Debug.Log($"[생포] {data.monsterName}");
            Destroy(gameObject);
            return;
        }

        // 지배 속성 기준 드랍
        AttributeType dominant = GetDominantAttribute();
        DropTable.DropEntry[] drops = data.defaultDeathDrops;

        if (data.attributeDeathResults != null)
        {
            foreach (var result in data.attributeDeathResults)
            {
                if (result.dominantAttribute == dominant)
                {
                    drops = result.drops;
                    break;
                }
            }
        }

        SpawnDrops(drops);
        CreatureCodex.instance?.RegisterHarvest(data, dominant.ToString());
        Destroy(gameObject);
    }

    void SpawnDrops(DropTable.DropEntry[] drops)
    {
        if (drops == null) return;
        foreach (var drop in drops)
        {
            if (drop.item == null) continue;
            if (UnityEngine.Random.value > drop.chance) continue;
            Vector3 scatter = new Vector3(
                UnityEngine.Random.Range(-0.6f, 0.6f), 0f,
                UnityEngine.Random.Range(-0.6f, 0.6f));
            WorldItem.Spawn(drop.item, transform.position + Vector3.up + scatter, drop.count);
        }
    }

    // ── 반응 체크 ─────────────────────────────────────────────

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

    void ApplyEffect(ReactionEffect effect, float value)
    {
        CurrentEffect      = effect;
        CurrentEffectValue = value;

        if (effect == ReactionEffect.Stun || effect == ReactionEffect.Paralyze)
            effectTimer = value;
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
