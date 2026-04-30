using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KitchenView : MonoBehaviour
{
    public KitchenPresenter presenter;

    [SerializeField] private Transform  recipeContainer;
    [SerializeField] private GameObject recipeEntryPrefab;
    [SerializeField] private TMP_Text   toolTierText;
    [SerializeField] private Button     upgradeButton;
    [SerializeField] private TMP_Text   upgradeButtonText;

    private static readonly string[] TierNames = { "모닥불", "가스렌지", "조리대" };
    private static readonly int[]    UpgradeCosts = { 200, 500 };

    void Start()
    {
        presenter = new KitchenPresenter(this);
        upgradeButton.onClick.AddListener(presenter.OnUpgradeTool);
    }

    public void Render(List<RecipeData> recipes, CookingToolTier tier)
    {
        foreach (Transform child in recipeContainer) Destroy(child.gameObject);
        foreach (var recipe in recipes)
        {
            var go = Instantiate(recipeEntryPrefab, recipeContainer);
            go.GetComponent<RecipeEntryView>().Init(recipe, presenter);
        }

        int t = (int)tier;
        toolTierText.text = $"현재 조리도구: {TierNames[t]}";

        bool canUpgrade = t < 2;
        upgradeButton.gameObject.SetActive(canUpgrade);
        if (canUpgrade)
            upgradeButtonText.text = $"{TierNames[t + 1]} 구매 ({UpgradeCosts[t]}G)";
    }
}
