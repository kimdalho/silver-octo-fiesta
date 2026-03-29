using UnityEngine;

public class SimpleMapGenerator : MonoBehaviour
{
    [Header("Ground")]
    public float mapRadius = 40f;
    public float battleMapRadius = 80f;
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

    [Header("Monsters")]
    public GameObject monsterPrefab;
    public MonsterData[] spawnableMonsters;
    public int monsterCount = 5;

    [Header("Dropped Items")]
    public ItemData[] spawnableItems;
    public int itemSpawnCount = 10;

    // 로컬맵은 최초 1회만 생성, 배틀맵은 매번 재생성
    private GameObject localMapRoot;
    private GameObject battleMapRoot;
    private bool localGenerated = false;

    private float currentRadius;

    public void Generate()
    {
        bool isBattle = GameLoopManager.instance != null
            && GameLoopManager.instance.CurrentStep != GameStep.Local;

        if (!isBattle)
        {
            // 로컬맵: 이미 생성된 경우 재생성하지 않음
            if (localGenerated && localMapRoot != null) return;

            currentRadius = mapRadius;
            localMapRoot = new GameObject("LocalMapRoot");
            DontDestroyOnLoad(localMapRoot);
            GenerateInto(localMapRoot, isBattle: false);
            localGenerated = true;
        }
        else
        {
            // 배틀맵: 매번 재생성
            if (battleMapRoot != null) Destroy(battleMapRoot);
            currentRadius = battleMapRadius;
            battleMapRoot = new GameObject("BattleMapRoot");
            GenerateInto(battleMapRoot, isBattle: true);
        }
    }

    private void GenerateInto(GameObject root, bool isBattle)
    {
        CreateGround(root);
        SpawnGroup("Trees",  treePrefab,  treeCount,  3f,   treeMinScale, treeMaxScale, root);
        SpawnGroup("Rocks",  rockPrefab,  rockCount,  2f,   rockMinScale, rockMaxScale, root);
        SpawnGroup("Grass",  grassPrefab, grassCount, 0.5f, 0.7f, 1.2f,  root);
        SpawnGroup("Bushes", bushPrefab,  bushCount,  2f,   0.7f, 1.3f,  root);
        SpawnDroppedItems(root);
        if (isBattle) SpawnMonsters(root);
    }

    public void Clear()
    {
        // 배틀맵만 파괴. 로컬맵은 영구 보존.
        if (battleMapRoot != null)
        {
            Destroy(battleMapRoot);
            battleMapRoot = null;
        }
    }

    void CreateGround(GameObject root)
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ground.name = "Ground";
        ground.transform.SetParent(root.transform);
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(currentRadius * 2f, 0.1f, currentRadius * 2f);

        Destroy(ground.GetComponent<Collider>());
        var box = ground.AddComponent<BoxCollider>();
        box.center = Vector3.zero;
        box.size = Vector3.one;

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = groundColor;
        ground.GetComponent<Renderer>().material = mat;
    }

    void SpawnGroup(string groupName, GameObject prefab, int count, float minDist, float minScale, float maxScale, GameObject root)
    {
        var parent = new GameObject(groupName).transform;
        parent.SetParent(root.transform);

        for (int i = 0; i < count; i++)
        {
            Vector2 pos = RandomInCircle(currentRadius * 0.9f, minDist);
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

    void SpawnDroppedItems(GameObject root)
    {
        if (spawnableItems == null || spawnableItems.Length == 0) return;

        var parent = new GameObject("DroppedItems").transform;
        parent.SetParent(root.transform);

        for (int i = 0; i < itemSpawnCount; i++)
        {
            Vector2 pos = RandomInCircle(currentRadius * 0.8f, 3f);
            var data = spawnableItems[Random.Range(0, spawnableItems.Length)];
            var worldItem = WorldItem.Spawn(data, new Vector3(pos.x, 0f, pos.y));
            worldItem.transform.SetParent(parent);
        }
    }

    void SpawnMonsters(GameObject root)
    {
        if (spawnableMonsters == null || spawnableMonsters.Length == 0) return;

        var parent = new GameObject("Monsters").transform;
        parent.SetParent(root.transform);

        float sectorAngle = 360f / monsterCount;

        for (int i = 0; i < monsterCount; i++)
        {
            var data = spawnableMonsters[Random.Range(0, spawnableMonsters.Length)];

            // 각 몬스터에 고유 구역 배정, 구역 안에서 랜덤 배치
            float angle = (sectorAngle * i + Random.Range(0f, sectorAngle)) * Mathf.Deg2Rad;
            float dist = Random.Range(currentRadius * 0.3f, currentRadius * 0.8f);
            Vector2 pos = new Vector2(Mathf.Cos(angle) * dist, Mathf.Sin(angle) * dist);

            GameObject go;
            if (monsterPrefab != null)
            {
                go = Instantiate(monsterPrefab, parent);
            }
            else
            {
                go = CreateMonsterFallback(parent);
            }

            go.name = $"Monster_{data.name}_{i}";
            go.transform.position = new Vector3(pos.x, 0f, pos.y);

            // 속성 상태 + 수확 시스템에 데이터 주입
            var attrState = go.GetComponent<MonsterAttributeState>()
                            ?? go.AddComponent<MonsterAttributeState>();
            attrState.Init(data);

            if (go.GetComponent<HarvestSystem>() == null)
                go.AddComponent<HarvestSystem>();

            if (go.GetComponent<MonsterBehavior>() == null)
                go.AddComponent<MonsterBehavior>();

            if (go.GetComponent<ReactionFeedback>() == null)
                go.AddComponent<ReactionFeedback>();
        }
    }

    GameObject CreateMonsterFallback(Transform parent)
    {
        var go = new GameObject("Monster");
        go.transform.SetParent(parent);

        var cc = go.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.4f;
        cc.center = new Vector3(0f, 0.9f, 0f);

        var spriteObj = new GameObject("Sprite");
        spriteObj.transform.SetParent(go.transform);
        spriteObj.transform.localPosition = new Vector3(0f, 1f, 0f);
        var sr = spriteObj.AddComponent<SpriteRenderer>();

        var tex = new Texture2D(2, 2);
        Color c = new Color(0.7f, 0.15f, 0.15f);
        tex.SetPixels(new[] { c, c, c, c });
        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0f), 2);

        go.AddComponent<Damageable>();
        go.AddComponent<DropTable>();

        return go;
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
