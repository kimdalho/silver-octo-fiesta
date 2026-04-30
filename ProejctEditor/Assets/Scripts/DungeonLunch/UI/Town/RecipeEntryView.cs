using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeEntryView : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text ingredientsText;
    [SerializeField] private Button   cookButton;

    private RecipeData _recipe;
    private KitchenPresenter _presenter;

    public void Init(RecipeData recipe, KitchenPresenter presenter)
    {
        _recipe    = recipe;
        _presenter = presenter;

        nameText.text = recipe.recipeName;

        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < recipe.ingredients.Length; i++)
            sb.Append($"{recipe.ingredients[i].itemName} x{recipe.ingredientCounts[i]}  ");
        ingredientsText.text = sb.ToString().TrimEnd();

        cookButton.interactable = CookingManager.instance.CanCook(recipe);
        cookButton.onClick.AddListener(() => _presenter.OnCook(_recipe));
    }
}
