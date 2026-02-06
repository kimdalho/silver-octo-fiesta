// WorldGenerator_Stream.cs
// Terrain 없이: 돈스타브 느낌(중앙 Safe + 구부러진 스트림) + Combat 대륙 확장 + Void(바다) + POI 자동 배치
// Unity 60000.1.14f1 (Unity 6) / URP 포함 어떤 파이프라인에서도 동작
//
// 사용법:
// 1) 빈 GameObject 생성 -> WorldRoot
// 2) 이 스크립트 부착
// 3) safeMat/neutralMat/combatMat(필수) + (선택)voidMat 지정
// 4) poiPrefab1~4 지정(큐브 프리팹 테스트 가능)
// 5) Inspector 우클릭 -> Generate 또는 ContextMenu 버튼으로 Generate 실행
//
// 특징:
// - 그리드 논리(바이옴) + 청크 메쉬 지형(완만 언덕) 분리
// - Combat은 Safe와 같거나 더 큰 “대륙” 느낌으로 키우기(구간별 두께/성장)
// - Combat 바깥(및 맵 외곽)은 Void가 감싸는 구조(바다 느낌)
// - Combat 내부 평탄/해안(VOID) 거리/POI 간 거리 조건으로 POI 자동 배치

using System;
using System.Collections.Generic;
using UnityEngine;

public enum BiomeType
{
    Void = 0,     // 바다/빈공간
    Safe = 1,     // 중앙 안전
    Neutral = 2,  // 완충
    Combat = 3,   // 위험
}

[DisallowMultipleComponent]
public class WorldGenerator_Stream : MonoBehaviour
{
    [Header("World Size (cells)")]
    [Min(32)] public int worldWidth = 160;
    [Min(32)] public int worldHeight = 160;

    [Header("Grid / Chunk")]
    [Min(1)] public int chunkSize = 32;
    [Min(0.5f)] public float cellSize = 2.0f;

    [Header("Seed")]
    public int seed = 12345;

    [Header("Don’t Starve-ish Layout")]
    [Tooltip("Radius (cells) of the central Safe zone (ellipse-ish).")]
    [Range(6, 80)] public int safeCoreRadius = 22;

    [Tooltip("How many curved biome streams (arms) from the center.")]
    [Range(2, 8)] public int streamCount = 4;

    [Tooltip("Stream length (cells) from center outward.")]
    [Range(20, 400)] public int streamLength = 140;

    [Tooltip("Base stream thickness (cells).")]
    [Range(2, 20)] public int streamThickness = 10;

    [Tooltip("How strongly streams curve / wander.")]
    [Range(0.1f, 3.0f)] public float streamWander = 1.2f;

    [Tooltip("How much noise roughens the stream edge.")]
    [Range(0f, 1.0f)] public float edgeNoise = 0.35f;

    [Tooltip("Neutral band length (cells) before turning into Combat.")]
    [Range(5, 200)] public int neutralBandLength = 30;

    [Header("Biome Thickness Control")]
    [Tooltip("Neutral band is thinner than base thickness.")]
    [Range(0.2f, 2.0f)] public float neutralWidthMultiplier = 0.65f;

    [Tooltip("Combat band is thicker than base thickness.")]
    [Range(0.5f, 3.0f)] public float combatWidthMultiplier = 1.65f;

    [Tooltip("Combat grows outward (0=no growth, 1=strong growth).")]
    [Range(0f, 1.0f)] public float combatOutwardGrowth = 0.6f;

    [Header("Void Ring / Combat Sea")]
    [Tooltip("World border void ring thickness (cells).")]
    [Range(0, 60)] public int voidRingThickness = 12;

    [Tooltip("Around Combat, carve surrounding cells to Void (sea/coast feel).")]
    [Range(0, 40)] public int voidBufferAroundCombat = 6;

    [Tooltip("If true, don't fill gaps to Neutral. (More sea / islands feel)")]
    public bool disableGapFill = true;

    [Header("Height (A+A: gentle + natural)")]
    [Range(0f, 20f)] public float heightAmplitude = 3.0f;
    [Range(10f, 250f)] public float noiseScale = 75f;

    [Tooltip("Max height delta between adjacent cells (meters). Lower = gentler slopes.")]
    [Range(0.01f, 1.0f)] public float maxNeighborDelta = 0.14f;

    [Header("Rendering")]
    public Material safeMat;
    public Material neutralMat;
    public Material combatMat;
    public Material voidMat; // optional (assign to visualize sea/void)

    [Header("Collision")]
    public bool addMeshCollider = true;

    [Header("POI (Combat) - Prefabs")]
    public GameObject poiPrefab1;
    public GameObject poiPrefab2;
    public GameObject poiPrefab3;
    public GameObject poiPrefab4;

    [Tooltip("How many POIs to place in Combat biome.")]
    [Range(1, 40)] public int combatPoiCount = 6;

    [Tooltip("Min distance between POIs (in cells).")]
    [Range(2, 120)] public int poiMinDistanceCells = 16;

    [Tooltip("Keep POIs away from Void coast by this many cells.")]
    [Range(0, 80)] public int poiMinDistanceToVoidCells = 10;

    [Tooltip("Reject positions if local slope is too steep. (meters delta to neighbors)")]
    [Range(0.01f, 0.8f)] public float poiMaxSlope = 0.12f;

    [Tooltip("How many random attempts per POI.")]
    [Range(10, 5000)] public int poiTriesPerPoi = 700;

    // internal
    private BiomeType[,] biomeGrid;
    private float[,] heightGrid;
    private Vector2Int center;

    private readonly List<GameObject> spawned = new();
    private readonly List<Vector2Int> placedPoiCells = new();

    [ContextMenu("Generate")]
    public void Generate()
    {
        Clear();

        if (safeMat == null || neutralMat == null || combatMat == null)
        {
            Debug.LogError("Assign safeMat / neutralMat / combatMat (required).");
            return;
        }

        UnityEngine.Random.InitState(seed);

        biomeGrid = new BiomeType[worldWidth, worldHeight];
        heightGrid = new float[worldWidth, worldHeight];

        center = new Vector2Int(worldWidth / 2, worldHeight / 2);

        // 1) 중앙 Safe 코어
        PaintSafeCore();

        // 2) 스트림(구부러진 길)로 Neutral->Combat 확장
        PaintStreams();

        // 3) Combat 주변/맵 외곽 Void(바다) 적용
        ApplyVoidRingAndCombatSea();

        // 4) (선택) void가 너무 많으면 자연 연결을 위해 Neutral로 약간 메우기
        if (!disableGapFill)
            FillGapsToNeutral();

        // 5) 높이 생성 + 경사 제한
        GenerateHeights();
        EnforceGentleSlopes();

        // 6) 메쉬 청크 생성
        BuildChunks();

        // 7) Combat 내부 POI 자동 배치
        PlaceCombatPOIs();
    }

    [ContextMenu("Clear")]
    public void Clear()
    {
        for (int i = spawned.Count - 1; i >= 0; i--)
        {
            if (spawned[i] == null) continue;
#if UNITY_EDITOR
            DestroyImmediate(spawned[i]);
#else
            Destroy(spawned[i]);
#endif
        }
        spawned.Clear();
        placedPoiCells.Clear();

        biomeGrid = null;
        heightGrid = null;
    }

    // ---------- Biomes ----------

    private void PaintSafeCore()
    {
        float rx = safeCoreRadius;
        float rz = safeCoreRadius * 0.9f;

        for (int z = 0; z < worldHeight; z++)
            for (int x = 0; x < worldWidth; x++)
            {
                float dx = (x - center.x) / rx;
                float dz = (z - center.y) / rz;
                float d = dx * dx + dz * dz;

                biomeGrid[x, z] = (d <= 1f) ? BiomeType.Safe : BiomeType.Void;
            }
    }

    private void PaintStreams()
    {
        float baseStep = 360f / Mathf.Max(1, streamCount);

        for (int s = 0; s < streamCount; s++)
        {
            float angle = (baseStep * s) + UnityEngine.Random.Range(-baseStep * 0.25f, baseStep * 0.25f);
            Vector2 dir = AngleToDir(angle);

            // 안전코어를 너무 침범하지 않도록 코어 경계 근처에서 시작
            Vector2 pos = center;
            pos += dir * (safeCoreRadius * 0.85f);

            float wanderPhase = UnityEngine.Random.Range(0f, 1000f);
            float localWander = streamWander * UnityEngine.Random.Range(0.85f, 1.25f);
            float thickness = streamThickness * UnityEngine.Random.Range(0.9f, 1.15f);

            for (int i = 0; i < streamLength; i++)
            {
                float t = i / Mathf.Max(1f, (float)streamLength); // 0..1

                // Perlin 기반 좌우 흔들림으로 "구부러진 길" 만들기
                float w = (Mathf.PerlinNoise(wanderPhase + t * 3.7f, wanderPhase + 42.1f) - 0.5f) * 2f;
                Vector2 perp = new Vector2(-dir.y, dir.x);
                Vector2 drift = perp * (w * localWander);

                // 살짝 방향 비틀기
                Vector2 stepDir = (dir + drift * 0.35f).normalized;
                pos += stepDir;

                int x = Mathf.RoundToInt(pos.x);
                int z = Mathf.RoundToInt(pos.y);
                if (!InBounds(x, z)) break;

                BiomeType bandBiome = (i < neutralBandLength) ? BiomeType.Neutral : BiomeType.Combat;

                // 코어 근처에서는 Combat 금지(중앙 안전 유지)
                bool nearCore = IsInsideCoreEllipse(x, z, safeCoreRadius + 2);
                if (nearCore && bandBiome == BiomeType.Combat)
                    bandBiome = BiomeType.Neutral;

                // 구간별 두께: Neutral 얇게 / Combat 크게
                float mult = (bandBiome == BiomeType.Neutral) ? neutralWidthMultiplier : combatWidthMultiplier;

                // Combat은 바깥으로 갈수록 성장(대륙 느낌)
                float growth = 1f;
                if (bandBiome == BiomeType.Combat && combatOutwardGrowth > 0f)
                    growth = Mathf.Lerp(1.0f, 1.0f + combatOutwardGrowth, t);

                float width = thickness * mult * growth;

                PaintBrush(x, z, width, bandBiome);
            }
        }
    }

    private void PaintBrush(int cx, int cz, float radius, BiomeType biome)
    {
        int r = Mathf.CeilToInt(radius);
        float inv = 1f / Mathf.Max(0.0001f, radius);

        for (int dz = -r; dz <= r; dz++)
            for (int dx = -r; dx <= r; dx++)
            {
                int x = cx + dx;
                int z = cz + dz;
                if (!InBounds(x, z)) continue;

                float dist = Mathf.Sqrt(dx * dx + dz * dz) * inv; // 0..~1
                if (dist > 1f) continue;

                // 가장자리 노이즈로 경계 울퉁불퉁
                float n = Mathf.PerlinNoise((x + seed) * 0.07f, (z - seed) * 0.07f);
                float edge = Mathf.Lerp(1f, n, edgeNoise);

                if (dist <= edge)
                {
                    // 중앙 Safe 코어는 최우선 보호
                    if (biomeGrid[x, z] == BiomeType.Safe)
                        continue;

                    // Neutral이 Combat을 덮어서 위험이 갑자기 커지지 않도록(완충 우선)
                    if (biome == BiomeType.Neutral && biomeGrid[x, z] == BiomeType.Combat)
                        continue;

                    biomeGrid[x, z] = biome;
                }
            }
    }

    private void ApplyVoidRingAndCombatSea()
    {
        // 1) 맵 외곽을 Void로 (바다)
        if (voidRingThickness > 0)
        {
            int t = voidRingThickness;
            for (int z = 0; z < worldHeight; z++)
                for (int x = 0; x < worldWidth; x++)
                {
                    bool isBorder = (x < t) || (z < t) || (x >= worldWidth - t) || (z >= worldHeight - t);
                    if (!isBorder) continue;

                    if (biomeGrid[x, z] != BiomeType.Safe) // Safe 코어 보호
                        biomeGrid[x, z] = BiomeType.Void;
                }
        }

        // 2) Combat 주변을 Void로 깎아 "전투 대륙 + 바다" 느낌
        if (voidBufferAroundCombat > 0)
        {
            var copy = (BiomeType[,])biomeGrid.Clone();
            int r = voidBufferAroundCombat;

            for (int z = 0; z < worldHeight; z++)
                for (int x = 0; x < worldWidth; x++)
                {
                    if (copy[x, z] != BiomeType.Combat) continue;

                    for (int dz = -r; dz <= r; dz++)
                        for (int dx = -r; dx <= r; dx++)
                        {
                            int nx = x + dx;
                            int nz = z + dz;
                            if (!InBounds(nx, nz)) continue;

                            if (biomeGrid[nx, nz] == BiomeType.Safe) continue;
                            if (biomeGrid[nx, nz] == BiomeType.Combat) continue;

                            biomeGrid[nx, nz] = BiomeType.Void;
                        }
                }
        }

        // 3) Safe 코어에서 바다까지 아무 길도 없으면 답답할 수 있어서,
        // 코어 주변 일부는 Neutral로 보호(해안이 코어에 붙지 않게)
        int protect = Mathf.Clamp(voidBufferAroundCombat + 2, 4, 20);
        for (int z = center.y - protect; z <= center.y + protect; z++)
            for (int x = center.x - protect; x <= center.x + protect; x++)
            {
                if (!InBounds(x, z)) continue;
                if (IsInsideCoreEllipse(x, z, safeCoreRadius + 1)) continue; // Safe 코어 자체는 Safe 유지

                if (biomeGrid[x, z] == BiomeType.Void)
                    biomeGrid[x, z] = BiomeType.Neutral;
            }
    }

    private void FillGapsToNeutral()
    {
        // Void가 너무 많을 때 연결감을 살리기 위한 옵션
        var copy = (BiomeType[,])biomeGrid.Clone();

        for (int z = 1; z < worldHeight - 1; z++)
            for (int x = 1; x < worldWidth - 1; x++)
            {
                if (copy[x, z] != BiomeType.Void) continue;

                int near = 0;
                if (copy[x - 1, z] != BiomeType.Void) near++;
                if (copy[x + 1, z] != BiomeType.Void) near++;
                if (copy[x, z - 1] != BiomeType.Void) near++;
                if (copy[x, z + 1] != BiomeType.Void) near++;

                if (near >= 3)
                    biomeGrid[x, z] = BiomeType.Neutral;
            }
    }

    // ---------- Height ----------

    private void GenerateHeights()
    {
        float ox = UnityEngine.Random.Range(-10000f, 10000f);
        float oz = UnityEngine.Random.Range(-10000f, 10000f);

        for (int z = 0; z < worldHeight; z++)
            for (int x = 0; x < worldWidth; x++)
            {
                if (biomeGrid[x, z] == BiomeType.Void)
                {
                    heightGrid[x, z] = 0f;
                    continue;
                }

                float wx = (x * cellSize + ox) / noiseScale;
                float wz = (z * cellSize + oz) / noiseScale;

                float n = Mathf.PerlinNoise(wx, wz); // 0..1
                float h = (n - 0.5f) * 2f * heightAmplitude;

                // Safe는 더 평탄
                if (biomeGrid[x, z] == BiomeType.Safe)
                    h *= 0.35f;

                heightGrid[x, z] = h;
            }
    }

    private void EnforceGentleSlopes()
    {
        const int iterations = 6;
        for (int it = 0; it < iterations; it++)
        {
            for (int z = 0; z < worldHeight; z++)
                for (int x = 0; x < worldWidth; x++)
                {
                    if (biomeGrid[x, z] == BiomeType.Void) continue;

                    float h = heightGrid[x, z];
                    TryClampNeighbor(x - 1, z, h);
                    TryClampNeighbor(x + 1, z, h);
                    TryClampNeighbor(x, z - 1, h);
                    TryClampNeighbor(x, z + 1, h);
                }
        }
    }

    private void TryClampNeighbor(int nx, int nz, float h)
    {
        if (!InBounds(nx, nz)) return;
        if (biomeGrid[nx, nz] == BiomeType.Void) return;

        float nh = heightGrid[nx, nz];
        float delta = nh - h;

        if (delta > maxNeighborDelta) nh = h + maxNeighborDelta;
        else if (delta < -maxNeighborDelta) nh = h - maxNeighborDelta;

        heightGrid[nx, nz] = nh;
    }

    // ---------- Mesh Build ----------

    private void BuildChunks()
    {
        int chunkCountX = Mathf.CeilToInt(worldWidth / (float)chunkSize);
        int chunkCountZ = Mathf.CeilToInt(worldHeight / (float)chunkSize);

        for (int cz = 0; cz < chunkCountZ; cz++)
            for (int cx = 0; cx < chunkCountX; cx++)
                CreateChunk(cx, cz);
    }

    private void CreateChunk(int chunkX, int chunkZ)
    {
        int startX = chunkX * chunkSize;
        int startZ = chunkZ * chunkSize;
        int endX = Mathf.Min(startX + chunkSize, worldWidth);
        int endZ = Mathf.Min(startZ + chunkSize, worldHeight);

        BuildChunkForBiome(chunkX, chunkZ, startX, startZ, endX, endZ, BiomeType.Safe, safeMat);
        BuildChunkForBiome(chunkX, chunkZ, startX, startZ, endX, endZ, BiomeType.Neutral, neutralMat);
        BuildChunkForBiome(chunkX, chunkZ, startX, startZ, endX, endZ, BiomeType.Combat, combatMat);

        if (voidMat != null)
            BuildChunkForBiome(chunkX, chunkZ, startX, startZ, endX, endZ, BiomeType.Void, voidMat);
    }

    private void BuildChunkForBiome(int chunkX, int chunkZ, int startX, int startZ, int endX, int endZ, BiomeType biome, Material mat)
    {
        // voidMat이 없으면 Void는 렌더 안 함
        if (biome == BiomeType.Void && voidMat == null) return;

        var verts = new List<Vector3>();
        var tris = new List<int>();
        var uvs = new List<Vector2>();

        for (int z = startZ; z < endZ; z++)
            for (int x = startX; x < endX; x++)
            {
                if (biomeGrid[x, z] != biome) continue;

                float x0 = x * cellSize;
                float z0 = z * cellSize;
                float x1 = (x + 1) * cellSize;
                float z1 = (z + 1) * cellSize;

                float h00 = SampleHeight(x, z);
                float h10 = SampleHeight(x + 1, z);
                float h01 = SampleHeight(x, z + 1);
                float h11 = SampleHeight(x + 1, z + 1);

                int i0 = verts.Count;
                verts.Add(new Vector3(x0, h00, z0));
                verts.Add(new Vector3(x1, h10, z0));
                verts.Add(new Vector3(x0, h01, z1));
                verts.Add(new Vector3(x1, h11, z1));

                uvs.Add(new Vector2(0, 0));
                uvs.Add(new Vector2(1, 0));
                uvs.Add(new Vector2(0, 1));
                uvs.Add(new Vector2(1, 1));

                tris.Add(i0 + 0); tris.Add(i0 + 2); tris.Add(i0 + 1);
                tris.Add(i0 + 1); tris.Add(i0 + 2); tris.Add(i0 + 3);
            }

        if (verts.Count == 0) return;

        var go = new GameObject($"Chunk_{chunkX}_{chunkZ}_{biome}");
        go.transform.SetParent(transform, false);
        spawned.Add(go);

        var mf = go.AddComponent<MeshFilter>();
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = mat;

        var mesh = new Mesh();
        mesh.indexFormat = (verts.Count > 65000)
            ? UnityEngine.Rendering.IndexFormat.UInt32
            : UnityEngine.Rendering.IndexFormat.UInt16;

        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mf.sharedMesh = mesh;

        if (addMeshCollider && biome != BiomeType.Void)
        {
            var mc = go.AddComponent<MeshCollider>();
            mc.sharedMesh = mesh;
        }
    }

    // ---------- POI Placement ----------

    private void PlaceCombatPOIs()
    {
        placedPoiCells.Clear();

        var prefabs = new List<GameObject>();
        if (poiPrefab1 != null) prefabs.Add(poiPrefab1);
        if (poiPrefab2 != null) prefabs.Add(poiPrefab2);
        if (poiPrefab3 != null) prefabs.Add(poiPrefab3);
        if (poiPrefab4 != null) prefabs.Add(poiPrefab4);

        if (prefabs.Count == 0)
        {
            Debug.LogWarning("No POI prefabs assigned. Skipping POI placement.");
            return;
        }

        int placed = 0;

        for (int k = 0; k < combatPoiCount; k++)
        {
            if (!TryFindCombatPoiCell(out var cell))
            {
                Debug.LogWarning($"POI placement failed for index {k}. Increase tries or relax constraints.");
                continue;
            }

            placedPoiCells.Add(cell);

            var prefab = prefabs[UnityEngine.Random.Range(0, prefabs.Count)];
            var worldPos = CellToWorld(cell.x, cell.y);

            var go = Instantiate(prefab, worldPos, Quaternion.identity, transform);
            go.name = $"POI_{prefab.name}_{cell.x}_{cell.y}";

            // IMPORTANT: 너의 정책(소품/조형물은 Collider 0)이면,
            // 프리팹 자체 collider는 제거해두고,
            // 상호작용은 자식 Trigger로 따로 두는 걸 추천.
            spawned.Add(go);
            placed++;
        }

        Debug.Log($"Placed {placed}/{combatPoiCount} combat POIs.");
    }

    private bool TryFindCombatPoiCell(out Vector2Int result)
    {
        for (int t = 0; t < poiTriesPerPoi; t++)
        {
            int x = UnityEngine.Random.Range(2, worldWidth - 2);
            int z = UnityEngine.Random.Range(2, worldHeight - 2);

            if (biomeGrid[x, z] != BiomeType.Combat) continue;

            if (poiMinDistanceToVoidCells > 0 && IsNearVoid(x, z, poiMinDistanceToVoidCells))
                continue;

            if (!IsGentleArea(x, z, poiMaxSlope))
                continue;

            if (!IsFarFromOtherPois(x, z, poiMinDistanceCells))
                continue;

            result = new Vector2Int(x, z);
            return true;
        }

        result = default;
        return false;
    }

    private bool IsNearVoid(int x, int z, int radiusCells)
    {
        int r = radiusCells;
        for (int dz = -r; dz <= r; dz++)
            for (int dx = -r; dx <= r; dx++)
            {
                int nx = x + dx;
                int nz = z + dz;
                if (!InBounds(nx, nz)) continue;

                if (biomeGrid[nx, nz] == BiomeType.Void)
                    return true;
            }
        return false;
    }

    private bool IsGentleArea(int x, int z, float maxDeltaMeters)
    {
        float h = heightGrid[x, z];

        float hL = heightGrid[Mathf.Max(0, x - 1), z];
        float hR = heightGrid[Mathf.Min(worldWidth - 1, x + 1), z];
        float hD = heightGrid[x, Mathf.Max(0, z - 1)];
        float hU = heightGrid[x, Mathf.Min(worldHeight - 1, z + 1)];

        float maxDelta = 0f;
        maxDelta = Mathf.Max(maxDelta, Mathf.Abs(hL - h));
        maxDelta = Mathf.Max(maxDelta, Mathf.Abs(hR - h));
        maxDelta = Mathf.Max(maxDelta, Mathf.Abs(hD - h));
        maxDelta = Mathf.Max(maxDelta, Mathf.Abs(hU - h));

        return maxDelta <= maxDeltaMeters;
    }

    private bool IsFarFromOtherPois(int x, int z, int minDistCells)
    {
        int minD2 = minDistCells * minDistCells;
        for (int i = 0; i < placedPoiCells.Count; i++)
        {
            int dx = x - placedPoiCells[i].x;
            int dz = z - placedPoiCells[i].y;
            int d2 = dx * dx + dz * dz;
            if (d2 < minD2) return false;
        }
        return true;
    }

    // ---------- Utility ----------

    private Vector3 CellToWorld(int x, int z)
    {
        float wx = (x + 0.5f) * cellSize;
        float wz = (z + 0.5f) * cellSize;
        float wy = SampleHeight(x, z);
        return new Vector3(wx, wy, wz);
    }

    private float SampleHeight(int x, int z)
    {
        x = Mathf.Clamp(x, 0, worldWidth - 1);
        z = Mathf.Clamp(z, 0, worldHeight - 1);
        return heightGrid[x, z];
    }

    private bool InBounds(int x, int z)
        => x >= 0 && z >= 0 && x < worldWidth && z < worldHeight;

    private Vector2 AngleToDir(float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    private bool IsInsideCoreEllipse(int x, int z, float radius)
    {
        float rx = radius;
        float rz = radius * 0.9f;
        float dx = (x - center.x) / rx;
        float dz = (z - center.y) / rz;
        return (dx * dx + dz * dz) <= 1f;
    }
}
