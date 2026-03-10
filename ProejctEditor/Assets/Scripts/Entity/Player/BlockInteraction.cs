using UnityEngine;

public class BlockInteraction : MonoBehaviour
{
    public float placeDistance = 5f;
    public LayerMask groundLayer = ~0;

    private BlockGrid blockGrid;
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        if (blockGrid == null)
        {
            blockGrid = FindObjectOfType<BlockGrid>();
            if (blockGrid == null) return;
        }

        // 우클릭 → 블록 설치
        if (Input.GetMouseButtonDown(1))
        {
            TryPlaceBlock();
        }
    }

    void TryPlaceBlock()
    {
        if (mainCam == null) return;
        if (InventoryManager.instance == null) return;

        // 마우스 레이캐스트
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit, placeDistance, groundLayer)) return;

        // 그리드 좌표 계산
        Vector2Int cell = blockGrid.WorldToGrid(hit.point);
        if (cell.x < 0) return;

        // 이미 블록이 있으면 무시
        if (blockGrid.GetBlock(cell.x, cell.y) != null) return;

        // 인벤토리에서 BlockData 타입 아이템 찾기
        var inv = InventoryManager.instance.inventory;
        int slotIndex = -1;
        BlockData blockData = null;

        for (int i = 0; i < Inventory.Size; i++)
        {
            if (inv.slots[i] == null) continue;
            var bd = inv.slots[i].data as BlockData;
            if (bd != null)
            {
                slotIndex = i;
                blockData = bd;
                break;
            }
        }

        if (blockData == null) return; // 블록 아이템 없음

        // 블록 설치
        if (blockGrid.PlaceBlock(cell.x, cell.y, blockData))
        {
            // 인벤토리에서 1개 소비
            var stack = inv.slots[slotIndex];
            stack.count--;
            if (stack.count <= 0)
                inv.SetSlot(slotIndex, null);
            else
                inv.SetSlot(slotIndex, stack); // UI 갱신 트리거
        }
    }
}
