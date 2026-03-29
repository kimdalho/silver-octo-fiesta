using UnityEngine;
using UnityEditor;

/// <summary>
/// 몬스터 관련 에셋·프리팹을 자동으로 설정하는 통합 도구.
///
/// 실행 순서:
///   1. Setup All MonsterData   — CapturedMonsterData 자동 생성·연결
///   2. Setup Monster Prefab    — 몬스터 프리팹에 필수 컴포넌트 추가
///   3. Create MonsterPen       — 우리 프리팹 + PlaceableData + 건설 레시피 일괄 생성
/// </summary>
public class MonsterSetupTool
{
    const string MonsterDataPath  = "Assets/Data/Monsters";
    const string ItemPath         = "Assets/Data/Items";
    const string CapturedPath     = "Assets/Data/Items/Captured";
    const string PrefabPath       = "Assets/Prefabs/Monsters";
    const string BuildingPath     = "Assets/Prefabs/Buildings";
    const string RecipePath       = "Assets/Data/Recipes";

    // ════════════════════════════════════════
    // 1. MonsterData → CapturedMonsterData 자동 생성
    // ════════════════════════════════════════

    [MenuItem("Tools/Monster Setup/1. Setup All MonsterData (생포 아이템 자동 생성)")]
    static void SetupAllMonsterData()
    {
        EnsureFolder("Assets/Data/Items", "Captured");

        string[] guids = AssetDatabase.FindAssets("t:MonsterData", new[] { MonsterDataPath });
        int created = 0, skipped = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var md = AssetDatabase.LoadAssetAtPath<MonsterData>(path);
            if (md == null) continue;

            if (md.capturedItemData != null) { skipped++; continue; }

            // CapturedMonsterData 생성
            var cap = ScriptableObject.CreateInstance<CapturedMonsterData>();
            cap.itemName    = $"생포 — {md.monsterName}";
            cap.description = $"생포된 {md.monsterName}.\n로컬 우리(Pen)에 입주시키면 자원을 자동 생산한다.";
            cap.itemType    = ItemType.MaterialItem;
            cap.maxStack    = 1;
            cap.icon        = md.icon;
            cap.sourceMonster = md;

            string savePath = $"{CapturedPath}/Captured_{md.name}.asset";
            AssetDatabase.CreateAsset(cap, savePath);

            // MonsterData에 역참조
            md.capturedItemData  = cap;
            md.captureThreshold  = 70f;
            EditorUtility.SetDirty(md);
            created++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[MonsterSetup] CapturedMonsterData 생성 {created}개 / 이미 있음 {skipped}개");
        EditorUtility.DisplayDialog("완료",
            $"CapturedMonsterData 생성: {created}개\n이미 설정됨: {skipped}개", "OK");
    }

    // ════════════════════════════════════════
    // 2. 몬스터 프리팹 컴포넌트 설정
    // ════════════════════════════════════════

    [MenuItem("Tools/Monster Setup/2. Setup Monster Prefab (컴포넌트 자동 추가)")]
    static void SetupMonsterPrefab()
    {
        // Assets/Prefabs/Monsters 안의 프리팹 전체 처리
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { PrefabPath });
        if (guids.Length == 0)
        {
            EditorUtility.DisplayDialog("없음", $"{PrefabPath} 에 프리팹이 없습니다.", "OK");
            return;
        }

        int count = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            // 프리팹 편집 컨텍스트 열기
            var contents = PrefabUtility.LoadPrefabContents(path);
            bool changed = false;

            changed |= AddIfMissing<Damageable>(contents);
            changed |= AddIfMissing<DropTable>(contents);
            changed |= AddIfMissing<MonsterAttributeState>(contents);
            changed |= AddIfMissing<HarvestSystem>(contents);
            changed |= AddIfMissing<MonsterBehavior>(contents);
            changed |= AddIfMissing<ReactionFeedback>(contents);

            if (changed)
            {
                PrefabUtility.SaveAsPrefabAsset(contents, path);
                count++;
            }
            PrefabUtility.UnloadPrefabContents(contents);
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[MonsterSetup] 몬스터 프리팹 {count}개 업데이트 완료");
        EditorUtility.DisplayDialog("완료", $"몬스터 프리팹 {guids.Length}개 확인, {count}개 컴포넌트 추가됨", "OK");
    }

    // ════════════════════════════════════════
    // 3. 우리(Pen) 프리팹 + PlaceableData + 레시피 일괄 생성
    // ════════════════════════════════════════

    [MenuItem("Tools/Monster Setup/3. Create MonsterPen (우리 프리팹 + 아이템 + 레시피)")]
    static void CreateMonsterPen()
    {
        EnsureFolder("Assets/Prefabs", "Buildings");
        EnsureFolder("Assets/Data", "Recipes");

        // ── 우리 PlaceableData (배치 아이템) ────────────────────
        string penItemPath = $"{ItemPath}/Placeable_MonsterPen.asset";
        var penItem = AssetDatabase.LoadAssetAtPath<PlaceableData>(penItemPath);
        if (penItem == null)
        {
            penItem = ScriptableObject.CreateInstance<PlaceableData>();
            penItem.itemName    = "몬스터 우리";
            penItem.description = "생포된 몬스터를 입주시키면 시간마다 자원을 자동 생산한다.\n[E] 몬스터 넣기 / 수거";
            penItem.itemType    = ItemType.MaterialItem;
            penItem.maxStack    = 1;
            penItem.gridSize    = new UnityEngine.Vector2Int(4, 2);
            penItem.placementY  = 0f;
            AssetDatabase.CreateAsset(penItem, penItemPath);
            Debug.Log("[MonsterSetup] Placeable_MonsterPen.asset 생성");
        }

        // ── 우리 프리팹 ─────────────────────────────────────────
        string prefabSavePath = $"{BuildingPath}/MonsterPen.prefab";
        bool prefabExists = AssetDatabase.LoadAssetAtPath<GameObject>(prefabSavePath) != null;

        if (!prefabExists)
        {
            var root = new GameObject("MonsterPen");

            // gridSize → 월드 크기 자동 계산
            int gx = penItem.gridSize.x, gz = penItem.gridSize.y;
            BuildingPrefabFactory.Build(root, gx, gz, 1.2f, new Color(0.55f, 0.38f, 0.15f));

            var po  = root.AddComponent<PlacedObject>();
            po.objectType = PlacedObjectType.Pen;
            root.AddComponent<MonsterPen>();

            var saved = PrefabUtility.SaveAsPrefabAsset(root, prefabSavePath);
            Object.DestroyImmediate(root);

            // PlaceableData에 프리팹 연결
            penItem.placementPrefab = saved;
            EditorUtility.SetDirty(penItem);

            Debug.Log("[MonsterSetup] MonsterPen.prefab 생성");
        }
        else
        {
            Debug.Log("[MonsterSetup] MonsterPen.prefab 이미 존재 — 스킵");
        }

        // ── 우리 건설 레시피 (Hand) ──────────────────────────────
        string recipeSavePath = $"{RecipePath}/Recipe_BuildMonsterPen.asset";
        if (AssetDatabase.LoadAssetAtPath<CraftingRecipeData>(recipeSavePath) == null)
        {
            var twig  = AssetDatabase.LoadAssetAtPath<ItemData>($"{ItemPath}/Material_Twig.asset");
            var stone = AssetDatabase.LoadAssetAtPath<ItemData>($"{ItemPath}/Material_Stone.asset");

            var recipe = ScriptableObject.CreateInstance<CraftingRecipeData>();
            recipe.recipeName   = "몬스터 우리 건설";
            recipe.description  = "나뭇가지와 돌로 만든 우리. 생포한 몬스터를 입주시켜 자원을 얻는다.";
            recipe.stationType  = CraftingStationType.Hand;
            recipe.ingredients  = new CraftingIngredient[]
            {
                new CraftingIngredient(twig,  8),
                new CraftingIngredient(stone, 4)
            };
            recipe.result       = penItem;
            recipe.resultCount  = 1;
            AssetDatabase.CreateAsset(recipe, recipeSavePath);
            Debug.Log("[MonsterSetup] Recipe_BuildMonsterPen.asset 생성");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("완료",
            "우리 생성 완료!\n\n" +
            "· Placeable_MonsterPen.asset\n" +
            "· MonsterPen.prefab\n" +
            "· Recipe_BuildMonsterPen.asset\n\n" +
            "CraftingUI의 allRecipes 배열에 Recipe_BuildMonsterPen을 등록하세요.", "OK");
    }

    // ════════════════════════════════════════
    // 유틸
    // ════════════════════════════════════════

    static bool AddIfMissing<T>(GameObject go) where T : Component
    {
        if (go.GetComponent<T>() != null) return false;
        go.AddComponent<T>();
        return true;
    }

    static void EnsureFolder(string parent, string child)
    {
        if (!AssetDatabase.IsValidFolder($"{parent}/{child}"))
            AssetDatabase.CreateFolder(parent, child);
    }
}
