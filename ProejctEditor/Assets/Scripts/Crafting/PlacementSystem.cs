using UnityEngine;

/// <summary>
/// 로컬씬에서 아이템을 필드에 배치하는 시스템.
/// 인벤토리에서 PlaceableData 아이템 우클릭 → StartPlacement() 호출.
/// </summary>
public class PlacementSystem : MonoBehaviour
{
    public static PlacementSystem instance;

    [Header("설정")]
    public LayerMask groundLayer;        // "Ground" 레이어 지정
    public float gridSize = 1f;          // 그리드 스냅 단위

    [Header("고스트 재질 (선택)")]
    public Material ghostMaterial;       // 반투명 재질. 없으면 기본 색상 유지

    private PlaceableData currentData;
    private int sourceSlotIndex;
    private GameObject ghost;
    private bool isPlacing;

    // 고스트의 원래 재질 보관 (복원용)
    private Material[] originalMaterials;

    // 배치된 오브젝트를 담는 영구 컨테이너 (씬 전환 후에도 유지)
    private static Transform placedObjectsContainer;

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;

        if (placedObjectsContainer == null)
        {
            var go = new GameObject("PlacedObjectsContainer");
            DontDestroyOnLoad(go);
            placedObjectsContainer = go.transform;
        }
    }

    public void StartPlacement(PlaceableData data, int slotIndex)
    {
        if (data == null || data.placementPrefab == null)
        {
            Debug.LogWarning($"[PlacementSystem] {data?.name}: placementPrefab이 비어 있습니다.");
            return;
        }

        if (isPlacing) CancelPlacement();

        currentData = data;
        sourceSlotIndex = slotIndex;
        isPlacing = true;

        // 고스트 생성
        ghost = Instantiate(data.placementPrefab);
        DisableGhostColliders(ghost);
        ApplyGhostMaterial(ghost);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        if (!isPlacing || ghost == null) return;

        // 마우스 위치 → 바닥 레이캐스트
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 200f, groundLayer))
        {
            Vector3 snapped = SnapToGrid(hit.point);
            snapped.y = hit.point.y + currentData.placementY;
            ghost.transform.position = snapped;

            // 좌클릭 → 배치 확정
            if (Input.GetMouseButtonDown(0))
                ConfirmPlacement(snapped);
        }

        // 우클릭 또는 ESC → 취소
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            CancelPlacement();
    }

    void ConfirmPlacement(Vector3 position)
    {
        var inv = InventoryManager.instance.inventory;
        var slot = inv.slots[sourceSlotIndex];

        // 슬롯이 아직 해당 아이템인지 검증
        if (slot == null || slot.data != currentData)
        {
            Debug.Log("[PlacementSystem] 슬롯이 비어있거나 아이템이 달라 배치 취소");
            CancelPlacement();
            return;
        }

        inv.ConsumeOne(sourceSlotIndex);

        Destroy(ghost);
        ghost = null;
        isPlacing = false;

        GameObject placed = Instantiate(currentData.placementPrefab, position, Quaternion.identity);
        placed.transform.SetParent(placedObjectsContainer);
        Debug.Log($"[PlacementSystem] {currentData.itemName} 배치 완료: {position}");

        currentData = null;
    }

    void CancelPlacement()
    {
        if (ghost != null) Destroy(ghost);
        ghost = null;
        isPlacing = false;
        currentData = null;
    }

    Vector3 SnapToGrid(Vector3 worldPos)
    {
        float x = Mathf.Round(worldPos.x / gridSize) * gridSize;
        float z = Mathf.Round(worldPos.z / gridSize) * gridSize;
        return new Vector3(x, worldPos.y, z);
    }

    void DisableGhostColliders(GameObject obj)
    {
        foreach (var col in obj.GetComponentsInChildren<Collider>())
            col.enabled = false;
    }

    void ApplyGhostMaterial(GameObject obj)
    {
        var renderers = obj.GetComponentsInChildren<Renderer>();

        if (ghostMaterial != null)
        {
            foreach (var r in renderers)
            {
                var mats = new Material[r.materials.Length];
                for (int i = 0; i < mats.Length; i++) mats[i] = ghostMaterial;
                r.materials = mats;
            }
        }
        else
        {
            // 고스트 재질 없으면 Standard 기준 반투명 처리 시도
            foreach (var r in renderers)
            {
                foreach (var m in r.materials)
                {
                    m.SetFloat("_Mode", 3);
                    m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    m.SetInt("_ZWrite", 0);
                    m.DisableKeyword("_ALPHATEST_ON");
                    m.EnableKeyword("_ALPHABLEND_ON");
                    m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    m.renderQueue = 3000;
                    Color c = m.color;
                    m.color = new Color(c.r, c.g, c.b, 0.45f);
                }
            }
        }
    }

    public bool IsPlacing => isPlacing;
}
