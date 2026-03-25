using UnityEngine;
using UnityEditor;

/// <summary>
/// 모든 크래프팅 레시피를 한번에 생성하는 에디터 도구.
/// 실행 전에 Tools > Create Test Items, Create Weapon Prefabs, Create Ammo Items를 먼저 실행할 것.
/// </summary>
public class RecipeCreator
{
    [MenuItem("Tools/Create All Recipes (레시피 전체)")]
    static void CreateAllRecipes()
    {
        string itemPath = "Assets/Data/Items";
        string recipePath = "Assets/Data/Recipes";

        if (!AssetDatabase.IsValidFolder("Assets/Data"))
            AssetDatabase.CreateFolder("Assets", "Data");
        if (!AssetDatabase.IsValidFolder(recipePath))
            AssetDatabase.CreateFolder("Assets/Data", "Recipes");

        // --- 자재 로드 ---
        var twig = Load<ItemData>(itemPath, "Material_Twig");
        var stone = Load<ItemData>(itemPath, "Material_Stone");
        var web = Load<ItemData>(itemPath, "Material_Web");
        var grass = Load<ItemData>(itemPath, "Material_Grass");
        var copper = Load<ItemData>(itemPath, "Material_Copper");
        var iron = Load<ItemData>(itemPath, "Material_Iron");
        var gunpowder = Load<ItemData>(itemPath, "Material_Gunpowder");

        // --- 결과물 로드 ---
        var woodSword = Load<ItemData>(itemPath, "Weapon_WoodSword");
        var stoneAxe = Load<ItemData>(itemPath, "Weapon_StoneAxe");
        var grassHat = Load<ItemData>(itemPath, "Head_GrassHat");
        var woodArmor = Load<ItemData>(itemPath, "Body_WoodArmor");

        var seChongtong = Load<ItemData>(itemPath, "Weapon_SeChongtong");
        var seungja = Load<ItemData>(itemPath, "Weapon_SeungjaChongtong");
        var bulanggi = Load<ItemData>(itemPath, "Weapon_Bulanggi");

        var ammoWater = Load<ItemData>(itemPath, "Ammo_Water");
        var ammoFire = Load<ItemData>(itemPath, "Ammo_Fire");
        var ammoElec = Load<ItemData>(itemPath, "Ammo_Electric");

        int count = 0;

        // ========================================
        // 맨손 제작 (Hand) - 제작대 건설용
        // ========================================

        // 작업대는 아직 ItemData가 없으므로 나중에 PlacementSystem에서 처리
        // 여기서는 "작업대 건설" 레시피만 데이터로 정의해둔다
        count += CreateRecipe(recipePath, "Recipe_BuildWorkbench",
            "작업대 건설", "나뭇가지와 돌로 작업대를 설치한다.",
            CraftingStationType.Hand,
            new CraftingIngredient[] {
                new CraftingIngredient(twig, 5),
                new CraftingIngredient(stone, 3)
            },
            null, 1); // result는 PlacementSystem이 처리

        count += CreateRecipe(recipePath, "Recipe_BuildAlchemy",
            "연금대 건설", "돌과 거미줄로 연금대를 설치한다.",
            CraftingStationType.Hand,
            new CraftingIngredient[] {
                new CraftingIngredient(stone, 5),
                new CraftingIngredient(web, 3),
                new CraftingIngredient(twig, 2)
            },
            null, 1);

        // ========================================
        // 작업대 (Workbench) 레시피
        // ========================================

        // 기본 무기/방어구
        if (woodSword != null)
        {
            count += CreateRecipe(recipePath, "Recipe_WoodSword",
                "나무 검", "나뭇가지를 깎아 만든 기본 검.",
                CraftingStationType.Workbench,
                new CraftingIngredient[] {
                    new CraftingIngredient(twig, 4),
                    new CraftingIngredient(stone, 2)
                },
                woodSword, 1);
        }

        if (stoneAxe != null)
        {
            count += CreateRecipe(recipePath, "Recipe_StoneAxe",
                "돌 도끼", "돌날을 나뭇가지에 묶은 도끼.",
                CraftingStationType.Workbench,
                new CraftingIngredient[] {
                    new CraftingIngredient(twig, 3),
                    new CraftingIngredient(stone, 4)
                },
                stoneAxe, 1);
        }

        if (grassHat != null)
        {
            count += CreateRecipe(recipePath, "Recipe_GrassHat",
                "풀 모자", "풀을 엮어 만든 모자.",
                CraftingStationType.Workbench,
                new CraftingIngredient[] {
                    new CraftingIngredient(grass, 6),
                    new CraftingIngredient(web, 2)
                },
                grassHat, 1);
        }

        if (woodArmor != null)
        {
            count += CreateRecipe(recipePath, "Recipe_WoodArmor",
                "나무 갑옷", "나무 껍질을 거미줄로 엮은 갑옷.",
                CraftingStationType.Workbench,
                new CraftingIngredient[] {
                    new CraftingIngredient(twig, 8),
                    new CraftingIngredient(web, 4)
                },
                woodArmor, 1);
        }

        // 총통 무기 3종
        if (seChongtong != null)
        {
            count += CreateRecipe(recipePath, "Recipe_SeChongtong",
                "세총통", "구리로 만든 소형 총통. 가볍고 빠르다.",
                CraftingStationType.Workbench,
                new CraftingIngredient[] {
                    new CraftingIngredient(copper, 3),
                    new CraftingIngredient(twig, 2)
                },
                seChongtong, 1);
        }

        if (seungja != null)
        {
            count += CreateRecipe(recipePath, "Recipe_SeungjaChongtong",
                "승자총통", "구리와 철로 만든 중형 총통.",
                CraftingStationType.Workbench,
                new CraftingIngredient[] {
                    new CraftingIngredient(copper, 5),
                    new CraftingIngredient(iron, 2),
                    new CraftingIngredient(twig, 3)
                },
                seungja, 1);
        }

        if (bulanggi != null)
        {
            count += CreateRecipe(recipePath, "Recipe_Bulanggi",
                "불랑기", "철과 화약으로 만든 대형 총통. 무겁지만 강력하다.",
                CraftingStationType.Workbench,
                new CraftingIngredient[] {
                    new CraftingIngredient(iron, 5),
                    new CraftingIngredient(copper, 3),
                    new CraftingIngredient(gunpowder, 2)
                },
                bulanggi, 1);
        }

        // ========================================
        // 연금대 (Alchemy) 레시피 - 포탄 제조
        // ========================================

        if (ammoWater != null)
        {
            count += CreateRecipe(recipePath, "Recipe_AmmoWater",
                "물 포탄 제조", "돌과 풀의 수분을 응축한 포탄.",
                CraftingStationType.Alchemy,
                new CraftingIngredient[] {
                    new CraftingIngredient(stone, 2),
                    new CraftingIngredient(grass, 1)
                },
                ammoWater, 5);
        }

        if (ammoFire != null)
        {
            count += CreateRecipe(recipePath, "Recipe_AmmoFire",
                "불 포탄 제조", "화약을 넣은 소이 포탄.",
                CraftingStationType.Alchemy,
                new CraftingIngredient[] {
                    new CraftingIngredient(stone, 2),
                    new CraftingIngredient(gunpowder, 1)
                },
                ammoFire, 5);
        }

        if (ammoElec != null)
        {
            count += CreateRecipe(recipePath, "Recipe_AmmoElectric",
                "전기 포탄 제조", "철 파편으로 전도성을 가진 포탄.",
                CraftingStationType.Alchemy,
                new CraftingIngredient[] {
                    new CraftingIngredient(stone, 2),
                    new CraftingIngredient(iron, 1)
                },
                ammoElec, 5);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[RecipeCreator] 레시피 {count}개 생성 완료! (Assets/Data/Recipes/)");
    }

    static int CreateRecipe(string folder, string fileName,
        string name, string desc, CraftingStationType station,
        CraftingIngredient[] ingredients, ItemData result, int resultCount)
    {
        var recipe = ScriptableObject.CreateInstance<CraftingRecipeData>();
        recipe.recipeName = name;
        recipe.description = desc;
        recipe.stationType = station;
        recipe.ingredients = ingredients;
        recipe.result = result;
        recipe.resultCount = resultCount;
        AssetDatabase.CreateAsset(recipe, $"{folder}/{fileName}.asset");
        return 1;
    }

    static T Load<T>(string folder, string fileName) where T : Object
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>($"{folder}/{fileName}.asset");
        if (asset == null)
            Debug.LogWarning($"[RecipeCreator] {fileName}.asset 을 찾을 수 없습니다. 먼저 해당 아이템을 생성하세요.");
        return asset;
    }
}
