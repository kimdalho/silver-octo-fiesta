using UnityEngine;

/// <summary>
/// PlaceableData.gridSize 기준으로 placeholder 프리팹을 자동 구성하는 공통 유틸.
///
/// gridSize가 바뀌면 이 팩토리만 사용하는 곳 전부 자동 반영.
/// 1 타일 = tileSize 미터 (기본 1f).
/// </summary>
public static class BuildingPrefabFactory
{
    const float TileSize    = 1f;    // 1타일 = 1m
    const float VisualInset = 0.9f;  // 타일보다 살짝 작게 (경계 간격)
    const float ColPadding  = 0.6f;  // 트리거 콜라이더 여유 (플레이어 접근 감지)

    /// <summary>
    /// root 아래에 Visual(Cube) + BoxCollider(trigger)를 gridSize 기반으로 생성.
    /// </summary>
    public static void Build(GameObject root, int gridX, int gridZ, float visualHeight, Color color)
    {
        float worldX = gridX * TileSize;
        float worldZ = gridZ * TileSize;

        // ── 시각 큐브 ─────────────────────────────────────────
        var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "Visual";
        visual.transform.SetParent(root.transform);
        visual.transform.localScale    = new Vector3(worldX * VisualInset, visualHeight, worldZ * VisualInset);
        visual.transform.localPosition = new Vector3(0f, visualHeight * 0.5f, 0f);
        Object.DestroyImmediate(visual.GetComponent<Collider>());

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = color;
        visual.GetComponent<MeshRenderer>().sharedMaterial = mat;

        // ── 트리거 콜라이더 (상호작용 감지) ────────────────────
        var col = root.AddComponent<BoxCollider>();
        col.size   = new Vector3(worldX + ColPadding, visualHeight + 1f, worldZ + ColPadding);
        col.center = new Vector3(0f, (visualHeight + 1f) * 0.5f, 0f);
        col.isTrigger = true;
    }
}
