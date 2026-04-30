using System.Collections.Generic;

public class KitchenPresenter
{
    public KitchenView view;

    public KitchenPresenter(KitchenView view)
    {
        this.view = view;
        view.presenter = this;
        Refresh();
    }

    public void Refresh()
    {
        var recipes = CookingManager.instance.GetAvailableRecipes();
        view.Render(recipes, CookingManager.instance.unlockedTier);
    }

    public void OnCook(RecipeData recipe)
    {
        if (CookingManager.instance.Cook(recipe))
            Refresh();
    }

    public void OnUpgradeTool()
    {
        if (CookingManager.instance.TryUpgradeTier())
            Refresh();
    }
}
