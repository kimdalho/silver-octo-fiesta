using UnityEngine;
using UnityEditor;

/// <summary>
/// 로컬 건축물 PlaceableData + 프리팹을 일괄 생성.
/// Tools > Building Setup > Create All Buildings
///
/// 타일 크기 정의:
///   작업대 (Workbench) : 2 × 2
///   연금대 (Alchemy)   : 2 × 2
///   우리   (Pen)       : 2 × 2  ← MonsterSetupTool에서도 설정
/// </summary>
public class BuildingSetupTool
{
    const string ItemPath     = "Assets/Data/Items";
    const string BuildingPath = "Assets/Prefabs/Buildings";
    const string RecipePath   = "Assets/Data/Recipes";

    [MenuItem("Tools/Building Setup/Create All Buildings (건축물 전체 생성)")]
    static void CreateAll()
    {
        EnsureFolder("Assets/Prefabs", "Buildings");
        EnsureFolder("Assets/Data",    "Recipes");

        var twig  = Load<ItemData>(ItemPath, "Material_Twig");
        var stone = Load<ItemData>(ItemPath, "Material_Stone");

        CreateBuilding(
            id:          "Workbench",
            displayName: "작업대",
            desc:        "고급 아이템과 장비를 제작하는 작업대.\n[E] 상호작용으로 제작 패널 열기.",
            gridX: 2, gridZ: 2,
            color:       new Color(0.55f, 0.38f, 0.20f),   // 나무 갈색
            objectType:  PlacedObjectType.Workbench,
            ingredients: new[] { new CraftingIngredient(twig, 5), new CraftingIngredient(stone, 3) },
            stationType: CraftingStationType.Hand
        );

        CreateBuilding(
            id:          "Alchemy",
            displayName: "연금대",
            desc:        "포탄과 특수 재료를 조합하는 연금대.\n[E] 상호작용으로 연금 패널 열기.",
            gridX: 2, gridZ: 2,
            color:       new Color(0.25f, 0.45f, 0.55f),   // 청회색
            objectType:  PlacedObjectType.Alchemy,
            ingredients: new[] { new CraftingIngredient(stone, 5), new CraftingIngredient(twig, 2) },
            stationType: CraftingStationType.Hand
        );

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("완료",
            "건축물 생성 완료!\n\n" +
            "· Placeable_Workbench.asset  (2×2)\n" +
            "· Placeable_Alchemy.asset    (2×2)\n" +
            "· Prefabs/Buildings/Workbench.prefab\n" +
            "· Prefabs/Buildings/Alchemy.prefab\n" +
            "· Recipe_BuildWorkbench.asset\n" +
            "· Recipe_BuildAlchemy.asset\n\n" +
            "CraftingUI의 allRecipes 배열에 레시피 2개를 등록하세요.", "OK");
    }

    // ── 건물 1개 생성 ──────────────────────────────────────────

    static void CreateBuilding(
        string id, string displayName, string desc,
        int gridX, int gridZ, Color color,
        PlacedObjectType objectType,
        CraftingIngredient[] ingredients,
        CraftingStationType stationType)
    {
        // ── PlaceableData ────────────────────────────────────
        string itemAssetPath = $"{ItemPath}/Placeable_{id}.asset";
        var item = AssetDatabase.LoadAssetAtPath<PlaceableData>(itemAssetPath);
        bool itemNew = (item == null);

        if (itemNew)
        {
            item = ScriptableObject.CreateInstance<PlaceableData>();
            AssetDatabase.CreateAsset(item, itemAssetPath);
        }

        item.itemName    = displayName;
        item.description = desc;
        item.itemType    = ItemType.MaterialItem;
        item.maxStack    = 1;
        item.gridSize    = new Vector2Int(gridX, gridZ);
        item.placementY  = 0f;
        EditorUtility.SetDirty(item);

        // ── 프리팹 ───────────────────────────────────────────
        string prefabPath = $"{BuildingPath}/{id}.prefab";
        bool prefabExists = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null;

        if (!prefabExists)
        {
            var root = new GameObject(id);

            // gridSize → 월드 크기 자동 계산 (1타일 = 1m)
            BuildingPrefabFactory.Build(root, gridX, gridZ, 1.2f, color);

            var po = root.AddComponent<PlacedObject>();
            po.objectType = objectType;

            var saved = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);

            item.placementPrefab = saved;
            EditorUtility.SetDirty(item);

            Debug.Log($"[BuildingSetup] {id}.prefab 생성 ({gridX}×{gridZ} 타일)");
        }
        else
        {
            // 이미 있으면 PlaceableData 연결만 갱신
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            item.placementPrefab = existing;
            EditorUtility.SetDirty(item);
            Debug.Log($"[BuildingSetup] {id}.prefab 이미 존재 — PlaceableData만 갱신");
        }

        // ── 건설 레시피 ──────────────────────────────────────
        string recipePath = $"{RecipePath}/Recipe_Build{id}.asset";
        if (AssetDatabase.LoadAssetAtPath<CraftingRecipeData>(recipePath) == null)
        {
            var recipe = ScriptableObject.CreateInstance<CraftingRecipeData>();
            recipe.recipeName  = $"{displayName} 건설";
            recipe.description = desc;
            recipe.stationType = stationType;
            recipe.ingredients = ingredients;
            recipe.result      = item;
            recipe.resultCount = 1;
            AssetDatabase.CreateAsset(recipe, recipePath);
            Debug.Log($"[BuildingSetup] Recipe_Build{id}.asset 생성");
        }
    }

    static T Load<T>(string folder, string name) where T : Object
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>($"{folder}/{name}.asset");
        if (asset == null)
            Debug.LogWarning($"[BuildingSetup] {name}.asset 없음 — 재료 연결 실패");
        return asset;
    }

    static void EnsureFolder(string parent, string child)
    {
        if (!AssetDatabase.IsValidFolder($"{parent}/{child}"))
            AssetDatabase.CreateFolder(parent, child);
    }
}
