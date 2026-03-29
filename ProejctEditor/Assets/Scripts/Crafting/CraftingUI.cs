using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 맨손 제작(Hand) 및 제작대(Workbench) 공통 UI.
///
/// [설정 방법]
/// 1. Canvas 하위에 Panel(panel)을 만들고 이 컴포넌트를 붙인다.
/// 2. Panel 안에 ScrollView를 만들고 Content Transform을 recipeListParent에 연결.
/// 3. RecipeEntryUI 프리팹을 만들어 recipeEntryPrefab에 연결.
/// 4. allRecipes에 CraftingRecipeData SO를 모두 등록.
/// 5. closeButton을 연결하거나 ESC로 닫는다.
/// </summary>
public class CraftingUI : MonoBehaviour
{
    public static CraftingUI instance;

    [Header("UI References")]
    public GameObject panel;
    public Transform recipeListParent;
    public GameObject recipeEntryPrefab;
    public TextMeshProUGUI titleText;
    public Button closeButton;

    [Header("레시피 목록 (Inspector에서 등록)")]
    public CraftingRecipeData[] allRecipes;

    private CraftingStationType currentType;

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;

        if (panel != null) panel.SetActive(false);
        if (closeButton != null) closeButton.onClick.AddListener(Close);
    }

    /// <summary>
    /// 제작 UI 열기. type에 따라 레시피 필터링.
    /// </summary>
    public void Open(CraftingStationType type)
    {
        currentType = type;
        BuildList();
        panel.SetActive(true);

        if (titleText != null)
            titleText.text = type switch
            {
                CraftingStationType.Hand      => "맨손 제작",
                CraftingStationType.Workbench => "작업대",
                CraftingStationType.Alchemy   => "연금대",
                _                             => "제작"
            };

        CameraFollow.instance?.SetCursorLocked(false);
    }

    public void Close()
    {
        panel.SetActive(false);
        if (PlacementSystem.instance == null || !PlacementSystem.instance.IsPlacing)
            CameraFollow.instance?.SetCursorLocked(true);
    }

    void Update()
    {
        if (panel != null && panel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    public void TryCraft(CraftingRecipeData recipe)
    {
        var inv = InventoryManager.instance.inventory;
        if (recipe.DoCraft(inv))
            BuildList(); // 재료 변동 반영
    }

    void BuildList()
    {
        if (recipeListParent == null || recipeEntryPrefab == null) return;

        // 기존 항목 제거
        for (int i = recipeListParent.childCount - 1; i >= 0; i--)
            Destroy(recipeListParent.GetChild(i).gameObject);

        if (allRecipes == null) return;

        foreach (var recipe in allRecipes)
        {
            if (recipe == null || recipe.stationType != currentType) continue;
            var entry = Instantiate(recipeEntryPrefab, recipeListParent);
            var entryUI = entry.GetComponent<RecipeEntryUI>();
            if (entryUI != null) entryUI.Setup(recipe, this);
        }
    }
}
