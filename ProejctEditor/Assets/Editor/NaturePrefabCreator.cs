using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class NaturePrefabCreator
{
    static NaturePrefabCreator()
    {
        // 에디터 시작 시 프리팹 없으면 자동 생성
        EditorApplication.delayCall += () =>
        {
            if (!AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Nature/Tree.prefab"))
                CreateAll();
        };
    }

    [MenuItem("Tools/Create Nature Prefabs")]
    public static void CreateAll()
    {
        string folder = "Assets/Prefabs/Nature";
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/Prefabs", "Nature");

        CreateSpritePrefab(folder, "Tree", 96, 160, new Color(0.2f, 0.45f, 0.15f), 0.3f, true);
        CreateSpritePrefab(folder, "Rock", 64, 48, new Color(0.55f, 0.53f, 0.5f), 0.25f, true);
        CreateSpritePrefab(folder, "Grass", 32, 28, new Color(0.25f, 0.55f, 0.2f), 0f, false);
        CreateSpritePrefab(folder, "Bush", 60, 40, new Color(0.15f, 0.4f, 0.12f), 0.2f, true);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // SimpleMapGenerator에 자동 연결
        AutoAssignPrefabs(folder);

        Debug.Log("[NaturePrefabCreator] Prefabs created & assigned in " + folder);
    }

    static void CreateSpritePrefab(string folder, string name, int w, int h, Color baseColor, float colRadius, bool hasCollider)
    {
        // placeholder 텍스처 생성
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] pixels = new Color[w * h];

        // 간단한 실루엣 생성
        float cx = w * 0.5f, cy = h * 0.5f;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = (x - cx) / cx;
                float dy = (y - cy) / cy;
                float dist = dx * dx + dy * dy;

                if (name == "Tree")
                {
                    // 줄기 + 잎
                    bool trunk = Mathf.Abs(dx) < 0.15f && dy < -0.2f;
                    bool leaves = dist < 0.6f && dy > -0.3f;
                    if (trunk)
                        pixels[y * w + x] = new Color(0.4f, 0.26f, 0.13f);
                    else if (leaves)
                        pixels[y * w + x] = baseColor + new Color(
                            Random.Range(-0.03f, 0.03f),
                            Random.Range(-0.03f, 0.03f), 0f);
                    else
                        pixels[y * w + x] = Color.clear;
                }
                else if (name == "Rock")
                {
                    // 납작한 타원
                    float rd = dx * dx + dy * dy * 2f;
                    if (rd < 0.8f)
                        pixels[y * w + x] = baseColor + new Color(1, 1, 1) * Random.Range(-0.05f, 0.05f);
                    else
                        pixels[y * w + x] = Color.clear;
                }
                else if (name == "Grass")
                {
                    // 풀잎 몇 가닥
                    bool blade1 = Mathf.Abs(dx + 0.2f) < 0.12f && dy > -0.5f + Mathf.Abs(dx) * 0.5f;
                    bool blade2 = Mathf.Abs(dx - 0.2f) < 0.12f && dy > -0.3f + Mathf.Abs(dx) * 0.5f;
                    bool blade3 = Mathf.Abs(dx) < 0.1f && dy > -0.6f;
                    if (blade1 || blade2 || blade3)
                        pixels[y * w + x] = baseColor + new Color(0, Random.Range(-0.05f, 0.05f), 0);
                    else
                        pixels[y * w + x] = Color.clear;
                }
                else // Bush
                {
                    float rd = dx * dx * 0.8f + dy * dy * 1.5f;
                    if (rd < 0.7f)
                        pixels[y * w + x] = baseColor + new Color(
                            Random.Range(-0.02f, 0.02f),
                            Random.Range(-0.04f, 0.04f), 0f);
                    else
                        pixels[y * w + x] = Color.clear;
                }
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();

        // 텍스처 저장
        byte[] png = tex.EncodeToPNG();
        string texPath = $"{folder}/{name}_Placeholder.png";
        System.IO.File.WriteAllBytes(texPath, png);
        AssetDatabase.ImportAsset(texPath);

        var importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 64;
            importer.filterMode = FilterMode.Point;
            importer.spritePivot = new Vector2(0.5f, 0f); // 바닥 기준
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texPath);

        float spriteH = h / 64f;

        // Root = 콜라이더만 (회전 안 함)
        var go = new GameObject(name);

        if (hasCollider)
        {
            var col = go.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, spriteH * 0.5f, 0f);
            col.radius = colRadius;
            col.height = spriteH;
        }

        // 자식 Sprite = 빌보드 회전
        var spriteObj = new GameObject("Sprite");
        spriteObj.transform.SetParent(go.transform);
        spriteObj.transform.localPosition = Vector3.zero;
        var sr = spriteObj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        spriteObj.AddComponent<Billboard>();

        string prefabPath = $"{folder}/{name}.prefab";
        PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        Object.DestroyImmediate(go);
    }

    static void AutoAssignPrefabs(string folder)
    {
        // 씬에 있는 SimpleMapGenerator 찾아서 자동 연결
        var gen = Object.FindFirstObjectByType<SimpleMapGenerator>();
        if (gen == null) return;

        gen.treePrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{folder}/Tree.prefab");
        gen.rockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{folder}/Rock.prefab");
        gen.grassPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{folder}/Grass.prefab");
        gen.bushPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{folder}/Bush.prefab");
        EditorUtility.SetDirty(gen);
    }
}
