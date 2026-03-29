using UnityEngine;

/// <summary>
/// 완성 상태 몬스터를 수확하는 컴포넌트.
/// MonsterAttributeState와 함께 몬스터에 붙인다.
///
/// 완성 상태 진입 시 [E] 수확 힌트 표시.
/// E 키 → 드랍 스폰 + 도감 등록 + 몬스터 파괴.
/// </summary>
public class HarvestSystem : MonoBehaviour
{
    [Header("수확 감지 범위")]
    public float harvestRange = 2.5f;

    private MonsterAttributeState attrState;
    private Transform playerTransform;
    private bool playerNearby;

    void Start()
    {
        attrState = GetComponent<MonsterAttributeState>();

        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;

        // 완성 상태 진입 시 시각 피드백 (색 변경)
        if (attrState != null)
            attrState.OnCompletionTriggered += OnCompleted;
    }

    void OnDestroy()
    {
        if (attrState != null)
            attrState.OnCompletionTriggered -= OnCompleted;
    }

    void Update()
    {
        if (playerTransform == null) return;

        float dist = Vector3.Distance(transform.position, playerTransform.position);
        bool near = dist <= harvestRange;

        if (near != playerNearby)
        {
            playerNearby = near;
            if (!near) InteractHintUI.instance?.Hide();
        }

        if (!playerNearby) return;

        // 힌트 갱신
        if (attrState != null && attrState.IsCompleted)
            InteractHintUI.instance?.Show("[E] 수확");

        // E 키 → 수확
        if (Input.GetKeyDown(KeyCode.E) && attrState != null && attrState.IsCompleted)
            Harvest();
    }

    void Harvest()
    {
        var combo = attrState.GetCompletedCombination();

        // 드랍 스폰 (chance 반영)
        if (combo.harvestDrops != null)
        {
            foreach (var drop in combo.harvestDrops)
            {
                if (drop.item == null) continue;
                if (Random.value > drop.chance) continue;

                Vector3 scatter = new Vector3(
                    Random.Range(-0.8f, 0.8f), 0f,
                    Random.Range(-0.8f, 0.8f));
                WorldItem.Spawn(drop.item, transform.position + Vector3.up + scatter, drop.count);
            }
        }

        // 도감 등록
        CreatureCodex.instance?.RegisterHarvest(attrState.data, combo.completionStateName);

        InteractHintUI.instance?.Hide();
        Destroy(gameObject);
    }

    void OnCompleted(AttributeCombination combo)
    {
        // 완성 상태 진입 시 스프라이트 색 변경 (황금빛 강조)
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
            sr.color = new Color(1f, 0.85f, 0.2f);

        Debug.Log($"[수확] {attrState.data?.monsterName} → {combo.completionStateName} 완성!");
    }
}
