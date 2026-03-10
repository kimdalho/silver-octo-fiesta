using UnityEngine;

public class BlockGrid : MonoBehaviour
{
    public const int GridSize = 16;
    public const float BlockScale = 1f;

    public BlockData defaultBlock;

    private BlockData[,] grid = new BlockData[GridSize, GridSize];
    private GameObject[,] blockObjects = new GameObject[GridSize, GridSize];

    /// <summary>
    /// 그리드 원점 (좌하단). 그리드 중앙이 월드 원점에 오도록 오프셋.
    /// </summary>
    public Vector3 Origin => transform.position - new Vector3(GridSize * BlockScale / 2f, 0f, GridSize * BlockScale / 2f);

    /// <summary>
    /// 기본 블록으로 전체 그리드를 채움.
    /// </summary>
    public void Initialize()
    {
        if (defaultBlock == null)
        {
            Debug.LogWarning("BlockGrid: defaultBlock이 설정되지 않았습니다.");
            return;
        }

        for (int x = 0; x < GridSize; x++)
            for (int z = 0; z < GridSize; z++)
                PlaceBlock(x, z, defaultBlock);
    }

    public bool PlaceBlock(int x, int z, BlockData data)
    {
        if (!InBounds(x, z) || data == null) return false;
        if (grid[x, z] != null) return false; // 이미 블록이 있음

        grid[x, z] = data;
        blockObjects[x, z] = CreateBlockCube(x, z, data);
        return true;
    }

    public BlockData RemoveBlock(int x, int z)
    {
        if (!InBounds(x, z) || grid[x, z] == null) return null;

        var removed = grid[x, z];
        grid[x, z] = null;

        if (blockObjects[x, z] != null)
        {
            Destroy(blockObjects[x, z]);
            blockObjects[x, z] = null;
        }

        return removed;
    }

    /// <summary>
    /// 월드 좌표 → 그리드 좌표. 범위 밖이면 (-1,-1).
    /// </summary>
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3 local = worldPos - Origin;
        int x = Mathf.FloorToInt(local.x / BlockScale);
        int z = Mathf.FloorToInt(local.z / BlockScale);

        if (!InBounds(x, z)) return new Vector2Int(-1, -1);
        return new Vector2Int(x, z);
    }

    /// <summary>
    /// 그리드 좌표 → 블록 중앙 월드 좌표.
    /// </summary>
    public Vector3 GridToWorld(int x, int z)
    {
        return Origin + new Vector3(x * BlockScale + BlockScale / 2f, 0f, z * BlockScale + BlockScale / 2f);
    }

    public BlockData GetBlock(int x, int z)
    {
        if (!InBounds(x, z)) return null;
        return grid[x, z];
    }

    public bool InBounds(int x, int z)
    {
        return x >= 0 && x < GridSize && z >= 0 && z < GridSize;
    }

    /// <summary>
    /// 특정 큐브 GameObject로부터 그리드 좌표를 역추적.
    /// </summary>
    public Vector2Int FindBlock(GameObject blockObj)
    {
        for (int x = 0; x < GridSize; x++)
            for (int z = 0; z < GridSize; z++)
                if (blockObjects[x, z] == blockObj)
                    return new Vector2Int(x, z);
        return new Vector2Int(-1, -1);
    }

    private GameObject CreateBlockCube(int x, int z, BlockData data)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = $"Block_{x}_{z}";
        cube.transform.SetParent(transform);
        cube.transform.localScale = new Vector3(BlockScale, BlockScale * 0.5f, BlockScale);

        Vector3 pos = GridToWorld(x, z);
        pos.y = -BlockScale * 0.25f; // 윗면이 y=0에 오도록
        cube.transform.position = pos;

        // 머티리얼
        var renderer = cube.GetComponent<Renderer>();
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = data.blockColor;
        renderer.material = mat;

        // 블록 마커 (삽 체크용)
        cube.AddComponent<BlockMarker>();

        // Damageable 부착 (파괴 가능)
        var dmg = cube.AddComponent<Damageable>();
        dmg.maxHP = 30f;
        dmg.currentHP = 30f;

        // DropTable 부착 (파괴 시 블록 아이템 드롭)
        var drop = cube.AddComponent<DropTable>();
        drop.drops = new DropTable.DropEntry[]
        {
            new DropTable.DropEntry { item = data, count = 1, chance = 1f }
        };

        // 블록 파괴 시 그리드에서도 제거
        dmg.OnDied += () =>
        {
            grid[x, z] = null;
            blockObjects[x, z] = null;
        };

        return cube;
    }
}
