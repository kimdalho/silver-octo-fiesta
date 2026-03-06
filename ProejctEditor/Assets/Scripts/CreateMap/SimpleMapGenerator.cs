using UnityEngine;

public class SimpleMapGenerator : MonoBehaviour
{
    [Header("Ground")]
    public float mapRadius = 40f;
    public Color groundColor = new Color(0.36f, 0.25f, 0.13f);

    [Header("Prefabs (auto-assigned by Tools > Create Nature Prefabs)")]
    public GameObject treePrefab;
    public GameObject rockPrefab;
    public GameObject grassPrefab;
    public GameObject bushPrefab;

    [Header("Counts")]
    public int treeCount = 60;
    public int rockCount = 25;
    public int grassCount = 80;
    public int bushCount = 20;

    [Header("Scale Range")]
    public float treeMinScale = 0.8f;
    public float treeMaxScale = 1.5f;
    public float rockMinScale = 0.5f;
    public float rockMaxScale = 1.3f;

    [Header("Dropped Items")]
    public ItemData[] spawnableItems;
    public int itemSpawnCount = 10;

    private GameObject mapRoot;

    public void Generate()
    {
        Clear();
        mapRoot = new GameObject("MapRoot");
        CreateGround();
        SpawnGroup("Trees", treePrefab, treeCount, 3f, treeMinScale, treeMaxScale);
        SpawnGroup("Rocks", rockPrefab, rockCount, 2f, rockMinScale, rockMaxScale);
        SpawnGroup("Grass", grassPrefab, grassCount, 0.5f, 0.7f, 1.2f);
        SpawnGroup("Bushes", bushPrefab, bushCount, 2f, 0.7f, 1.3f);
        SpawnDroppedItems();
    }

    public void Clear()
    {
        if (mapRoot != null)
            Destroy(mapRoot);
    }

    void CreateGround()
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ground.name = "Ground";
        ground.transform.SetParent(mapRoot.transform);
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(mapRadius * 2f, 0.1f, mapRadius * 2f);

        Destroy(ground.GetComponent<Collider>());
        var box = ground.AddComponent<BoxCollider>();
        box.center = Vector3.zero;
        box.size = Vector3.one;

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = groundColor;
        ground.GetComponent<Renderer>().material = mat;
    }

    void SpawnGroup(string groupName, GameObject prefab, int count, float minDist, float minScale, float maxScale)
    {
        var parent = new GameObject(groupName).transform;
        parent.SetParent(mapRoot.transform);

        for (int i = 0; i < count; i++)
        {
            Vector2 pos = RandomInCircle(mapRadius * 0.9f, minDist);
            float scale = Random.Range(minScale, maxScale);

            GameObject obj;
            if (prefab != null)
            {
                obj = Instantiate(prefab, parent);
            }
            else
            {
                // 프리팹 없으면 기본 스프라이트 폴백
                obj = CreateFallback(groupName);
                obj.transform.SetParent(parent);
            }

            obj.name = $"{groupName}_{i}";
            obj.transform.position = new Vector3(pos.x, 0f, pos.y);
            obj.transform.localScale = Vector3.one * scale;
        }
    }

    GameObject CreateFallback(string type)
    {
        var go = new GameObject(type);

        var spriteObj = new GameObject("Sprite");
        spriteObj.transform.SetParent(go.transform);
        spriteObj.transform.localPosition = Vector3.zero;
        var sr = spriteObj.AddComponent<SpriteRenderer>();
        spriteObj.AddComponent<Billboard>();

        var tex = new Texture2D(2, 2);
        Color c;
        switch (type)
        {
            case "Trees": c = new Color(0.2f, 0.45f, 0.15f); break;
            case "Rocks": c = new Color(0.55f, 0.53f, 0.5f); break;
            case "Grass": c = new Color(0.25f, 0.55f, 0.2f); break;
            default: c = new Color(0.15f, 0.4f, 0.12f); break;
        }
        tex.SetPixels(new[] { c, c, c, c });
        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0f), 2);

        return go;
    }

    void SpawnDroppedItems()
    {
        if (spawnableItems == null || spawnableItems.Length == 0) return;

        var parent = new GameObject("DroppedItems").transform;
        parent.SetParent(mapRoot.transform);

        for (int i = 0; i < itemSpawnCount; i++)
        {
            Vector2 pos = RandomInCircle(mapRadius * 0.8f, 3f);
            var data = spawnableItems[Random.Range(0, spawnableItems.Length)];
            var worldItem = WorldItem.Spawn(data, new Vector3(pos.x, 0f, pos.y));
            worldItem.transform.SetParent(parent);
        }
    }

    Vector2 RandomInCircle(float radius, float minDistFromCenter)
    {
        Vector2 p;
        do
        {
            p = Random.insideUnitCircle * radius;
        } while (p.magnitude < minDistFromCenter);
        return p;
    }
}
