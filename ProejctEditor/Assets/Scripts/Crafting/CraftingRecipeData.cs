using System;
using UnityEngine;

[Serializable]
public struct CraftingIngredient
{
    public ItemData item;
    public int count;

    public CraftingIngredient(ItemData item, int count)
    {
        this.item = item;
        this.count = count;
    }
}

public enum CraftingStationType
{
    Hand,       // 맨손 제작 (제작대 건설용)
    Workbench,  // 작업대
    Alchemy     // 연금대
}

/// <summary>
/// 크래프팅 레시피 데이터.
/// 필요한 제작대, 재료 목록, 결과물을 정의한다.
/// </summary>
[CreateAssetMenu(fileName = "NewRecipe", menuName = "Crafting/Recipe")]
public class CraftingRecipeData : ScriptableObject
{
    public string recipeName;
    [TextArea] public string description;

    [Header("제작 조건")]
    public CraftingStationType stationType;

    [Header("재료")]
    public CraftingIngredient[] ingredients;

    [Header("결과물")]
    public ItemData result;
    public int resultCount = 1;

    /// <summary>
    /// 인벤토리에 재료가 충분한지 확인.
    /// </summary>
    public bool CanCraft(Inventory inventory)
    {
        if (ingredients == null) return false;

        foreach (var ing in ingredients)
        {
            if (ing.item == null) return false;
            int have = CountItem(inventory, ing.item);
            if (have < ing.count) return false;
        }
        return true;
    }

    /// <summary>
    /// 인벤토리에서 재료를 소모하고 결과물을 추가.
    /// CanCraft 확인 후 호출할 것.
    /// </summary>
    public bool DoCraft(Inventory inventory)
    {
        if (!CanCraft(inventory)) return false;

        // 재료 소모
        foreach (var ing in ingredients)
        {
            ConsumeItem(inventory, ing.item, ing.count);
        }

        // 결과물 추가
        inventory.AddItem(result, resultCount);
        return true;
    }

    static int CountItem(Inventory inv, ItemData target)
    {
        int total = 0;
        for (int i = 0; i < Inventory.Size; i++)
        {
            if (inv.slots[i] != null && inv.slots[i].data == target)
                total += inv.slots[i].count;
        }
        return total;
    }

    static void ConsumeItem(Inventory inv, ItemData target, int amount)
    {
        for (int i = 0; i < Inventory.Size && amount > 0; i++)
        {
            if (inv.slots[i] == null || inv.slots[i].data != target) continue;

            int take = Mathf.Min(amount, inv.slots[i].count);
            inv.slots[i].count -= take;
            amount -= take;

            if (inv.slots[i].count <= 0)
                inv.slots[i] = null;
        }
    }
}
