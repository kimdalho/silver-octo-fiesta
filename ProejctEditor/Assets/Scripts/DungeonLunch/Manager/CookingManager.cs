using System.Collections.Generic;
using UnityEngine;

public class CookingManager : MonoBehaviour
{
    public static CookingManager instance;

    public CookingToolTier unlockedTier = CookingToolTier.Campfire;
    public RecipeData[] allRecipes;

    // 조리도구 업그레이드 가격
    public int gasStovePrice   = 200;
    public int workTablePrice  = 500;

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public List<RecipeData> GetAvailableRecipes()
    {
        var list = new List<RecipeData>();
        foreach (var r in allRecipes)
            if ((int)r.requiredTier <= (int)unlockedTier) list.Add(r);
        return list;
    }

    public bool CanCook(RecipeData recipe)
    {
        if ((int)recipe.requiredTier > (int)unlockedTier) return false;
        var inv = DungeonInventoryManager.instance;
        for (int i = 0; i < recipe.ingredients.Length; i++)
        {
            bool found = false;
            foreach (var s in inv.Slots)
                if (s.item == recipe.ingredients[i] && s.count >= recipe.ingredientCounts[i]) { found = true; break; }
            if (!found) return false;
        }
        return true;
    }

    public bool Cook(RecipeData recipe)
    {
        if (!CanCook(recipe)) return false;
        var inv = DungeonInventoryManager.instance;
        for (int i = 0; i < recipe.ingredients.Length; i++)
            inv.RemoveItem(recipe.ingredients[i], recipe.ingredientCounts[i]);
        LunchboxManager.instance?.AddFood(recipe.result);
        return true;
    }

    public bool TryUpgradeTier()
    {
        if ((int)unlockedTier >= (int)CookingToolTier.WorkTable) return false;
        int cost = unlockedTier == CookingToolTier.Campfire ? gasStovePrice : workTablePrice;
        if (DungeonInventoryManager.instance.gold < cost) return false;
        DungeonInventoryManager.instance.gold -= cost;
        unlockedTier = (CookingToolTier)((int)unlockedTier + 1);
        return true;
    }
}
