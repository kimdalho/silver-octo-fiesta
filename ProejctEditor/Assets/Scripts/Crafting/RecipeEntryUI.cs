using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 제작 UI의 레시피 한 줄. CraftingUI가 동적으로 생성.
/// </summary>
public class RecipeEntryUI : MonoBehaviour
{
    [Header("References")]
    public Image resultIcon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI ingredientsText;
    public Button craftButton;
    public TextMeshProUGUI craftButtonText;

    private CraftingRecipeData recipe;
    private CraftingUI owner;

    public void Setup(CraftingRecipeData r, CraftingUI ui)
    {
        recipe = r;
        owner = ui;

        if (resultIcon != null && r.result != null)
        {
            resultIcon.sprite = r.result.icon;
            resultIcon.color = r.result.icon != null ? Color.white : Color.clear;
        }

        if (nameText != null)
            nameText.text = $"{r.recipeName} x{r.resultCount}";

        if (ingredientsText != null)
        {
            var sb = new StringBuilder();
            foreach (var ing in r.ingredients)
            {
                if (ing.item == null) continue;
                int have = CountInInventory(ing.item);
                string color = have >= ing.count ? "#aaffaa" : "#ff8888";
                sb.AppendLine($"<color={color}>{ing.item.itemName} {have}/{ing.count}</color>");
            }
            ingredientsText.text = sb.ToString();
        }

        if (craftButton != null)
        {
            bool canCraft = r.CanCraft(InventoryManager.instance.inventory);
            craftButton.interactable = canCraft;
            craftButton.onClick.RemoveAllListeners();
            craftButton.onClick.AddListener(() => owner.TryCraft(recipe));

            if (craftButtonText != null)
                craftButtonText.text = canCraft ? "제작" : "재료 부족";
        }
    }

    static int CountInInventory(ItemData target)
    {
        int total = 0;
        var inv = InventoryManager.instance.inventory;
        for (int i = 0; i < Inventory.Size; i++)
        {
            if (inv.slots[i] != null && inv.slots[i].data == target)
                total += inv.slots[i].count;
        }
        return total;
    }
}
