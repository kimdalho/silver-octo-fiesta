using UnityEngine;

/// <summary>
/// 인벤토리에서 꺼내 로컬 필드에 배치할 수 있는 아이템.
/// ItemData를 상속하므로 인벤토리 시스템을 그대로 사용.
/// </summary>
[CreateAssetMenu(fileName = "NewPlaceable", menuName = "Town/Placeable Item")]
public class PlaceableData : ItemData
{
    [Header("배치")]
    public GameObject placementPrefab;   // 실제 배치될 프리팹
    public Vector2Int gridSize = Vector2Int.one; // 차지하는 그리드 칸 수
    public float placementY = 0f;        // 배치 시 Y 오프셋 (지형 위로 띄우기)
}
