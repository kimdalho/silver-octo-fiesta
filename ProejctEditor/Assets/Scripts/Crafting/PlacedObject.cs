using UnityEngine;

public enum PlacedObjectType
{
    Workbench,
    Storage,
    // 나중에 추가
}

/// <summary>
/// 필드에 배치된 오브젝트. Trigger Collider 필요.
/// 플레이어가 접근하면 E 키 상호작용 안내 표시.
/// </summary>
[RequireComponent(typeof(Collider))]
public class PlacedObject : MonoBehaviour
{
    [Header("오브젝트 종류")]
    public PlacedObjectType objectType;

    [Header("상호작용 안내 UI (월드스페이스 Canvas 등)")]
    public GameObject interactPromptUI;  // "E" 키 UI, 없으면 없는 대로 동작

    private bool playerNearby;

    void Start()
    {
        // Trigger로 설정 보장
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        if (interactPromptUI != null)
            interactPromptUI.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerNearby = true;
        interactPromptUI?.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerNearby = false;
        interactPromptUI?.SetActive(false);
    }

    void Update()
    {
        if (!playerNearby) return;
        if (!Input.GetKeyDown(KeyCode.E)) return;

        OnInteract();
    }

    void OnInteract()
    {
        switch (objectType)
        {
            case PlacedObjectType.Workbench:
                CraftingUI.instance?.Open(CraftingStationType.Workbench);
                break;

            case PlacedObjectType.Storage:
                Debug.Log("[PlacedObject] 보관함 - 추후 구현");
                break;
        }
    }
}
