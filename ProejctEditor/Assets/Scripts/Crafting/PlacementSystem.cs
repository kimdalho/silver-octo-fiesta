using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 로컬 필드 건물 배치·저장·불러오기.
///
/// [Inspector 설정]
/// groundLayer       : "Ground" 레이어 마스크
/// placeableRegistry : 게임에 존재하는 모든 PlaceableData SO 등록
/// ghostMaterial     : (선택) 반투명 배치 고스트 재질
/// </summary>
public class PlacementSystem : MonoBehaviour
{
    public static PlacementSystem instance;

    [Header("설정")]
    public LayerMask groundLayer;
    public float gridSize = 1f;

    [Header("배치 가능 아이템 레지스트리 (모든 PlaceableData 등록)")]
    public PlaceableData[] placeableRegistry;

    [Header("고스트 재질 (선택)")]
    public Material ghostMaterial;

    private PlaceableData currentData;
    private int sourceSlotIndex;
    private GameObject ghost;
    private bool isPlacing;

    // 배치된 오브젝트 컨테이너 (씬 전환 후에도 유지)
    private static Transform placedObjectsContainer;

    // 저장 파일 경로
    static string SavePath => Application.persistentDataPath + "/farm.json";

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;

        if (placedObjectsContainer == null)
        {
            var go = new GameObject("PlacedObjectsContainer");
            DontDestroyOnLoad(go);
            placedObjectsContainer = go.transform;
            LoadFarm(); // 최초 1회만
        }
    }

    // ── 배치 시작 ─────────────────────────────────────────────

    public void StartPlacement(PlaceableData data, int slotIndex)
    {
        if (data == null || data.placementPrefab == null)
        {
            Debug.LogWarning($"[PlacementSystem] {data?.name}: placementPrefab 없음");
            return;
        }

        if (isPlacing) CancelPlacement();

        currentData = data;
        sourceSlotIndex = slotIndex;
        isPlacing = true;

        ghost = Instantiate(data.placementPrefab);
        DisableGhostColliders(ghost);
        ApplyGhostMaterial(ghost);

        CameraFollow.instance?.SetCursorLocked(false);
    }

    void Update()
    {
        if (!isPlacing || ghost == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 200f, groundLayer))
        {
            Vector3 snapped = SnapToGrid(hit.point);
            snapped.y = hit.point.y + currentData.placementY;
            ghost.transform.position = snapped;

            if (Input.GetMouseButtonDown(0))
                ConfirmPlacement(snapped);
        }

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            CancelPlacement();
    }

    void ConfirmPlacement(Vector3 position)
    {
        var inv = InventoryManager.instance.inventory;
        var slot = inv.slots[sourceSlotIndex];

        if (slot == null || slot.data != currentData)
        {
            CancelPlacement();
            return;
        }

        inv.ConsumeOne(sourceSlotIndex);

        Destroy(ghost);
        ghost = null;
        isPlacing = false;
        CameraFollow.instance?.SetCursorLocked(true);

        SpawnBuilding(currentData, position, 0f);
        SaveFarm();

        currentData = null;
    }

    void CancelPlacement()
    {
        if (ghost != null) Destroy(ghost);
        ghost = null;
        isPlacing = false;
        currentData = null;
        CameraFollow.instance?.SetCursorLocked(true);
    }

    // ── 픽업 (PlacedObject F키 → 여기 통보) ──────────────────

    public void OnObjectPickedUp(PlacedObject obj)
    {
        SaveFarm();
    }

    // ── 건물 스폰 (배치/로드 공통) ────────────────────────────

    GameObject SpawnBuilding(PlaceableData data, Vector3 pos, float rotY)
    {
        var go = Instantiate(data.placementPrefab, pos, Quaternion.Euler(0f, rotY, 0f));
        go.transform.SetParent(placedObjectsContainer);

        var po = go.GetComponent<PlacedObject>();
        if (po != null)
        {
            po.placeableId = data.name;
            po.sourceData = data;
        }

        return go;
    }

    // ── 저장 ──────────────────────────────────────────────────

    public void SaveFarm()
    {
        var save = new FarmSave();

        foreach (Transform child in placedObjectsContainer)
        {
            var po = child.GetComponent<PlacedObject>();
            if (po == null) continue;

            save.buildings.Add(new BuildingEntry
            {
                id = po.placeableId,
                x  = child.position.x,
                y  = child.position.y,
                z  = child.position.z,
                ry = child.eulerAngles.y
            });
        }

        File.WriteAllText(SavePath, JsonUtility.ToJson(save, true));
    }

    // ── 불러오기 ──────────────────────────────────────────────

    void LoadFarm()
    {
        if (!File.Exists(SavePath)) return;

        FarmSave save;
        try { save = JsonUtility.FromJson<FarmSave>(File.ReadAllText(SavePath)); }
        catch { return; }

        if (save?.buildings == null) return;

        foreach (var entry in save.buildings)
        {
            var data = FindData(entry.id);
            if (data == null)
            {
                Debug.LogWarning($"[PlacementSystem] 레지스트리에 '{entry.id}' 없음 - 스킵");
                continue;
            }
            SpawnBuilding(data, new Vector3(entry.x, entry.y, entry.z), entry.ry);
        }
    }

    PlaceableData FindData(string id)
    {
        if (placeableRegistry == null) return null;
        foreach (var d in placeableRegistry)
            if (d != null && d.name == id) return d;
        return null;
    }

    // ── 유틸 ──────────────────────────────────────────────────

    public bool IsPlacing => isPlacing;

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
        foreach (var r in obj.GetComponentsInChildren<Renderer>())
        {
            if (ghostMaterial != null)
            {
                var mats = new Material[r.materials.Length];
                for (int i = 0; i < mats.Length; i++) mats[i] = ghostMaterial;
                r.materials = mats;
            }
            else
            {
                foreach (var m in r.materials)
                {
                    m.SetFloat("_Mode", 3);
                    m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    m.SetInt("_ZWrite", 0);
                    m.EnableKeyword("_ALPHABLEND_ON");
                    m.renderQueue = 3000;
                    Color c = m.color;
                    m.color = new Color(c.r, c.g, c.b, 0.45f);
                }
            }
        }
    }

    // ── 직렬화 클래스 ──────────────────────────────────────────

    [System.Serializable]
    class FarmSave
    {
        public List<BuildingEntry> buildings = new List<BuildingEntry>();
    }

    [System.Serializable]
    class BuildingEntry
    {
        public string id;
        public float x, y, z, ry;
    }
}
