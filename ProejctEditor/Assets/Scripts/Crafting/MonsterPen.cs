using UnityEngine;

/// <summary>
/// 로컬 필드 우리(Pen). PlacedObject(Pen 타입)와 함께 붙인다.
///
/// 1. 비어 있을 때 E키 → 인벤토리에서 CapturedMonsterData를 자동으로 찾아 입주
/// 2. 입주 후 productionInterval마다 penDrops 생산 준비
/// 3. 수거 가능 상태에서 E키 → 아이템 인벤토리로 수거
/// </summary>
public class MonsterPen : MonoBehaviour
{
    [Header("생산 간격 (초)")]
    public float productionInterval = 120f;

    private CapturedMonsterData occupant;
    private float timer;
    private bool hasOutput;

    public bool HasOccupant => occupant != null;
    public CapturedMonsterData Occupant => occupant;

    void Update()
    {
        if (occupant == null || hasOutput) return;

        timer += Time.deltaTime;
        if (timer >= productionInterval)
        {
            timer = 0f;
            hasOutput = true;
        }
    }

    /// <summary>PlacedObject.OnInteract()에서 호출.</summary>
    public void Interact()
    {
        if (occupant == null)
        {
            TryDepositFromInventory();
            return;
        }

        if (hasOutput)
            CollectOutput();
        else
            Debug.Log($"[우리] {occupant.sourceMonster?.monsterName} — {productionInterval - timer:F0}초 후 수거 가능");
    }

    public string HintLabel()
    {
        if (occupant == null)  return "[E] 몬스터 넣기";
        if (hasOutput)         return $"[E] {occupant.sourceMonster?.monsterName} 수거";
        return $"[E] 생산 중…  {productionInterval - timer:F0}s";
    }

    void TryDepositFromInventory()
    {
        var inv = InventoryManager.instance?.inventory;
        if (inv == null) return;

        for (int i = 0; i < Inventory.Size; i++)
        {
            if (inv.slots[i]?.data is CapturedMonsterData captured)
            {
                inv.RemoveItem(i);
                Deposit(captured);
                return;
            }
        }
        Debug.Log("[우리] 인벤토리에 생포된 몬스터 없음");
    }

    void Deposit(CapturedMonsterData captured)
    {
        occupant = captured;
        timer = 0f;
        hasOutput = false;
        Debug.Log($"[우리] {captured.sourceMonster?.monsterName} 입주");
    }

    void CollectOutput()
    {
        hasOutput = false;
        var drops = occupant?.sourceMonster?.penDrops;
        if (drops == null) return;

        var inv = InventoryManager.instance?.inventory;
        if (inv == null) return;

        foreach (var drop in drops)
        {
            if (drop.item == null || Random.value > drop.chance) continue;
            inv.AddItem(drop.item, drop.count);
        }

        Debug.Log($"[우리] {occupant.sourceMonster?.monsterName} 생산물 수거 완료");
    }
}
