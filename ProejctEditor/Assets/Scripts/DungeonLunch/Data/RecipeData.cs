using UnityEngine;

public enum CookingToolTier { Campfire = 0, GasStove = 1, WorkTable = 2 }

[CreateAssetMenu(fileName = "NewRecipe", menuName = "DungeonLunch/RecipeData")]
public class RecipeData : ScriptableObject
{
    public string recipeName;
    public CookingToolTier requiredTier;
    public FoodData[] ingredients;
    public int[] ingredientCounts;
    public FoodData result;
}
