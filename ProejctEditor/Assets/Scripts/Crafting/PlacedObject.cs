using UnityEngine;

public enum PlacedObjectType
{
    Workbench,  // 작업대
    Storage,    // 보관함
    Alchemy,    // 연금대
    Pen,        // 우리 (생포 몬스터 입주 → 자동 생산)
}

/// <summary>
/// 로컬 필드에 배치된 건물/시설.
/// Trigger Collider 필수.
/// PlacementSystem이 배치/로드 시 Init()을 호출해 데이터를 주입한다.
/// </summary>
[RequireComponent(typeof(Collider))]
public class PlacedObject : MonoBehaviour
{
    [Header("종류")]
    public PlacedObjectType objectType;

    // PlacementSystem이 설정 (세이브/픽업에 사용)
    [HideInInspector] public string placeableId;
    [HideInInspector] public PlaceableData sourceData;

    private bool playerNearby;

    void Start()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerNearby = true;
        InteractHintUI.instance?.Show(InteractLabel());
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerNearby = false;
        InteractHintUI.instance?.Hide();
    }

    void Update()
    {
        if (!playerNearby) return;

        // E키 : 상호작용
        if (Input.GetKeyDown(KeyCode.E))
            OnInteract();

        // F키 : 회수 (인벤토리로 돌려받기)
        if (Input.GetKeyDown(KeyCode.F))
            PickUp();
    }

    void OnInteract()
    {
        switch (objectType)
        {
            case PlacedObjectType.Workbench:
                CraftingUI.instance?.Open(CraftingStationType.Workbench);
                break;

            case PlacedObjectType.Storage:
                var chest = GetComponent<StorageChest>();
                if (chest != null) StorageUI.instance?.Open(chest);
                break;

            case PlacedObjectType.Alchemy:
                CraftingUI.instance?.Open(CraftingStationType.Alchemy);
                break;

            case PlacedObjectType.Pen:
                GetComponent<MonsterPen>()?.Interact();
                break;
        }
    }

    void PickUp()
    {
        InteractHintUI.instance?.Hide();

        if (sourceData != null)
            InventoryManager.instance?.inventory.AddItem(sourceData, 1);

        PlacementSystem.instance?.OnObjectPickedUp(this);
        Destroy(gameObject);
    }

    string InteractLabel()
    {
        string penLabel = objectType == PlacedObjectType.Pen
            ? GetComponent<MonsterPen>()?.HintLabel() ?? "[E] 우리"
            : null;

        string action = penLabel ?? objectType switch
        {
            PlacedObjectType.Workbench => "작업대",
            PlacedObjectType.Storage   => "보관함",
            PlacedObjectType.Alchemy   => "연금대",
            _                          => "상호작용",
        };
        return $"[E] {action}    [F] 회수";
    }
}
