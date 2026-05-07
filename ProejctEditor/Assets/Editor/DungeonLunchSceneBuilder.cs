using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public static class DungeonLunchSceneBuilder
{
    private const string SCENE_ROOT = "Assets/Scenes/DL/";

    private static void RegisterScenes()
    {
        var scenes = new[]
        {
            SCENE_ROOT + "0.Title.unity",
            SCENE_ROOT + "1.Town.unity",
            SCENE_ROOT + "2.Dungeon.unity",
            SCENE_ROOT + "3.Loading.unity",
        };

        var builds = new EditorBuildSettingsScene[scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
            builds[i] = new EditorBuildSettingsScene(scenes[i], true);

        EditorBuildSettings.scenes = builds;
        Debug.Log("[DungeonLunch] Build Settings 등록 완료");
    }

    private static void BuildTitleScene()
    {
        var scene = EditorSceneManager.OpenScene(SCENE_ROOT + "0.Title.unity", OpenSceneMode.Single);
        ClearScene();

        CreateManagers();
        CreateOrthoCamera();
        var canvas = CreateCanvas("Canvas");

        var bg = CreateUIImage(canvas, "Background", Color.black);
        SetStretch(bg.GetComponent<RectTransform>());

        CreateText(canvas, "TitleText", "Dungeon Lunch", 72, new Vector2(0, 80), new Vector2(800, 120));

        var btnGO = CreateButton(canvas, "NewGameButton", "새 게임", new Vector2(0, -80), new Vector2(300, 80));

        // TitleViewController
        var titleVC = canvas.AddComponent<TitleViewController>();
        SetField(titleVC, "newGameButton", btnGO.GetComponent<Button>());
        UnityEventTools.AddPersistentListener(btnGO.GetComponent<Button>().onClick, titleVC.OnClickNewGame);

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[DungeonLunch] Title 씬 구성 완료");
    }

    private static void BuildTownScene()
    {
        var scene = EditorSceneManager.OpenScene(SCENE_ROOT + "1.Town.unity", OpenSceneMode.Single);
        ClearScene();

        CreateOrthoCamera();
        var canvas = CreateCanvas("Canvas");

        // 헤더
        var header = CreatePanel(canvas, "Header", new Vector2(0, 490), new Vector2(1920, 80), new Color(0.1f, 0.1f, 0.1f));
        var goldGO  = CreateText(header, "GoldText", "골드: 0G", 28, new Vector2(-700, 0), new Vector2(300, 60));
        var enterGO = CreateButton(header, "EnterDungeonButton", "던전 입장", new Vector2(820, 0), new Vector2(220, 60));

        // 좌측 파티 패널
        var leftPanel  = CreatePanel(canvas, "LeftPanel", new Vector2(-760, -40), new Vector2(360, 900), new Color(0.12f, 0.12f, 0.12f));
        var partyView  = leftPanel.AddComponent<PartyPanelView>();
        var cardContainer = new GameObject("CardContainer");
        cardContainer.transform.SetParent(leftPanel.transform, false);
        var ccRect = cardContainer.AddComponent<RectTransform>();
        ccRect.anchorMin = new Vector2(0, 1); ccRect.anchorMax = new Vector2(1, 1);
        ccRect.pivot = new Vector2(0.5f, 1f);
        ccRect.anchoredPosition = Vector2.zero; ccRect.sizeDelta = new Vector2(0, 0);
        var vlg = cardContainer.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
        vlg.spacing = 8; vlg.padding = new RectOffset(8, 8, 8, 8);
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true; vlg.childForceExpandWidth = true;
        vlg.childControlHeight = false; vlg.childForceExpandHeight = false;
        var cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/DL/PartyMemberCard.prefab");
        SetField(partyView, "cardContainer", ccRect.transform);
        SetField(partyView, "cardPrefab", cardPrefab);

        // 중앙
        var center = CreatePanel(canvas, "CenterPanel", new Vector2(100, -40), new Vector2(1100, 900), new Color(0.08f, 0.08f, 0.08f));

        // 탭바
        var tabBar = CreatePanel(center, "TabBar", new Vector2(0, 390), new Vector2(1080, 72), new Color(0.15f, 0.15f, 0.15f));
        var guildBtnGO   = CreateButton(tabBar, "GuildButton",   "길드", new Vector2(-330, 0), new Vector2(300, 60));
        var shopBtnGO    = CreateButton(tabBar, "ShopButton",    "상점", new Vector2(0,    0), new Vector2(300, 60));
        var kitchenBtnGO = CreateButton(tabBar, "KitchenButton", "주방", new Vector2(330,  0), new Vector2(300, 60));

        // 서브패널
        var guildGO   = CreatePanel(center, "GuildPanel",   new Vector2(0, -36), new Vector2(1060, 760), new Color(0.1f, 0.1f, 0.1f));
        var shopGO    = CreatePanel(center, "ShopPanel",    new Vector2(0, -36), new Vector2(1060, 760), new Color(0.1f, 0.1f, 0.1f));
        var kitchenGO = CreatePanel(center, "KitchenPanel", new Vector2(0, -36), new Vector2(1060, 760), new Color(0.1f, 0.1f, 0.1f));
        var guildView   = guildGO.AddComponent<GuildView>();
        var shopView    = shopGO.AddComponent<ShopView>();
        var kitchenView = kitchenGO.AddComponent<KitchenView>();
        shopGO.SetActive(false);
        kitchenGO.SetActive(false);

        // GuildView 필드 연결
        var guildContainer = CreateVLGContainer(guildGO, "CardContainer");
        var refreshBtnGO   = CreateButton(guildGO, "RefreshButton", "새로고침", new Vector2(0, -330), new Vector2(200, 50));
        SetField(guildView, "cardContainer",     guildContainer);
        SetField(guildView, "recruitCardPrefab", LoadPrefab("RecruitCard"));
        SetField(guildView, "refreshButton",     refreshBtnGO.GetComponent<Button>());

        // ShopView 필드 연결
        var shopContainer = CreateVLGContainer(shopGO, "ItemContainer");
        var shopGoldGO    = CreateText(shopGO, "GoldText", "골드: 0G", 24, new Vector2(0, 330), new Vector2(300, 50));
        SetField(shopView, "itemContainer",  shopContainer);
        SetField(shopView, "shopItemPrefab", LoadPrefab("ShopItem"));
        SetField(shopView, "goldText",       shopGoldGO.GetComponent<TextMeshProUGUI>());

        // KitchenView 필드 연결
        var recipeContainer = CreateVLGContainer(kitchenGO, "RecipeContainer");
        var toolTierGO      = CreateText(kitchenGO, "ToolTierText", "조리도구: 기본", 24, new Vector2(0, 330), new Vector2(400, 50));
        var upgradeBtnGO    = CreateButton(kitchenGO, "UpgradeButton", "업그레이드", new Vector2(0, 270), new Vector2(200, 50));
        SetField(kitchenView, "recipeContainer",   recipeContainer);
        SetField(kitchenView, "recipeEntryPrefab", LoadPrefab("RecipeEntry"));
        SetField(kitchenView, "toolTierText",      toolTierGO.GetComponent<TextMeshProUGUI>());
        SetField(kitchenView, "upgradeButton",     upgradeBtnGO.GetComponent<Button>());
        SetField(kitchenView, "upgradeButtonText", upgradeBtnGO.transform.Find("Text").GetComponent<TextMeshProUGUI>());

        // TownViewController
        var tvc = center.AddComponent<TownViewController>();
        SetField(tvc, "guildPanel",   guildGO);
        SetField(tvc, "shopPanel",    shopGO);
        SetField(tvc, "kitchenPanel", kitchenGO);
        SetField(tvc, "goldText",     goldGO.GetComponent<TextMeshProUGUI>());

        UnityEventTools.AddPersistentListener(guildBtnGO.GetComponent<Button>().onClick,   tvc.OpenGuild);
        UnityEventTools.AddPersistentListener(shopBtnGO.GetComponent<Button>().onClick,    tvc.OpenShop);
        UnityEventTools.AddPersistentListener(kitchenBtnGO.GetComponent<Button>().onClick, tvc.OpenKitchen);
        UnityEventTools.AddPersistentListener(enterGO.GetComponent<Button>().onClick,      tvc.OnClickEnterDungeon);

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[DungeonLunch] Town 씬 구성 완료");
    }

    private static void BuildDungeonScene()
    {
        var scene = EditorSceneManager.OpenScene(SCENE_ROOT + "2.Dungeon.unity", OpenSceneMode.Single);
        ClearScene();

        CreateOrthoCamera();
        var canvas = CreateCanvas("Canvas");

        // 좌측 파티 패널
        var leftPanel  = CreatePanel(canvas, "LeftPanel", new Vector2(-760, 0), new Vector2(360, 1060), new Color(0.12f, 0.12f, 0.12f));
        var partyView  = leftPanel.AddComponent<PartyPanelView>();
        var cardContainerD = new GameObject("CardContainer");
        cardContainerD.transform.SetParent(leftPanel.transform, false);
        var ccRectD = cardContainerD.AddComponent<RectTransform>();
        ccRectD.anchorMin = new Vector2(0, 1); ccRectD.anchorMax = new Vector2(1, 1);
        ccRectD.pivot = new Vector2(0.5f, 1f);
        ccRectD.anchoredPosition = Vector2.zero; ccRectD.sizeDelta = new Vector2(0, 0);
        var vlgD = cardContainerD.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
        vlgD.spacing = 8; vlgD.padding = new RectOffset(8, 8, 8, 8);
        vlgD.childAlignment = TextAnchor.UpperCenter;
        vlgD.childControlWidth = true; vlgD.childForceExpandWidth = true;
        vlgD.childControlHeight = false; vlgD.childForceExpandHeight = false;
        var cardPrefabD = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/DL/PartyMemberCard.prefab");
        SetField(partyView, "cardContainer", ccRectD.transform);
        SetField(partyView, "cardPrefab", cardPrefabD);

        // 중앙
        var center = CreatePanel(canvas, "CenterPanel", new Vector2(0, 0), new Vector2(1160, 1060), new Color(0.05f, 0.05f, 0.05f));
        var floorGO  = CreateText(center, "FloorText",  "B1F",     48, new Vector2(0, 440), new Vector2(400, 70));
        var stateGO  = CreateText(center, "StateText",  "Explore", 28, new Vector2(0, 370), new Vector2(400, 50));

        // 액션 버튼
        var actionGO    = new GameObject("ActionButtons");
        actionGO.transform.SetParent(center.transform, false);
        var descendGO   = CreateButton(actionGO, "DescendButton", "층 내려가기", new Vector2(-130, -460), new Vector2(230, 70));
        var returnGO    = CreateButton(actionGO, "ReturnButton",  "귀환",        new Vector2( 130, -460), new Vector2(230, 70));

        // 전투/보스 패널
        var battleGO = CreatePanel(center, "BattlePanel", Vector2.zero, new Vector2(1100, 600), new Color(0.2f, 0.05f, 0.05f));
        battleGO.AddComponent<BattleViewController>();
        battleGO.SetActive(false);

        var bossGO = CreatePanel(center, "BossPanel", Vector2.zero, new Vector2(1100, 600), new Color(0.3f, 0.05f, 0.1f));
        bossGO.AddComponent<BossBattleViewController>();
        bossGO.SetActive(false);

        // 우측 패널
        var rightPanel = CreatePanel(canvas, "RightPanel", new Vector2(760, 0), new Vector2(360, 1060), new Color(0.12f, 0.12f, 0.12f));
        var logGO      = CreatePanel(rightPanel, "AdventureLog",   new Vector2(0,  280), new Vector2(340, 420), new Color(0.08f, 0.08f, 0.08f));
        var invGO      = CreatePanel(rightPanel, "InventoryPanel", new Vector2(0, -120), new Vector2(340, 260), new Color(0.08f, 0.08f, 0.08f));
        var lunchGO    = CreatePanel(rightPanel, "LunchboxPanel",  new Vector2(0, -420), new Vector2(340, 160), new Color(0.08f, 0.08f, 0.08f));
        var logView   = logGO.AddComponent<AdventureLogView>();
        var invView   = invGO.AddComponent<InventoryPanelView>();
        var lunchView = lunchGO.AddComponent<LunchboxPanelView>();

        // AdventureLogView — ScrollRect 구성
        var scrollViewGO = new GameObject("ScrollView");
        scrollViewGO.transform.SetParent(logGO.transform, false);
        scrollViewGO.AddComponent<Image>().color = new Color(0, 0, 0, 0);
        SetStretch(scrollViewGO.GetComponent<RectTransform>());
        var sr = scrollViewGO.AddComponent<ScrollRect>();
        sr.horizontal = false;
        var vpGO = new GameObject("Viewport");
        vpGO.transform.SetParent(scrollViewGO.transform, false);
        vpGO.AddComponent<Image>().color = new Color(0, 0, 0, 0);
        SetStretch(vpGO.GetComponent<RectTransform>());
        vpGO.AddComponent<Mask>().showMaskGraphic = false;
        var contentGO = new GameObject("Content");
        contentGO.transform.SetParent(vpGO.transform, false);
        var contentRect = contentGO.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1); contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero; contentRect.sizeDelta = new Vector2(0, 600);
        var logTxtGO = new GameObject("LogText");
        logTxtGO.transform.SetParent(contentGO.transform, false);
        var logTmp = logTxtGO.AddComponent<TextMeshProUGUI>();
        logTmp.fontSize = 16; logTmp.color = Color.white; logTmp.enableWordWrapping = true;
        SetStretch(logTxtGO.GetComponent<RectTransform>());
        sr.viewport = vpGO.GetComponent<RectTransform>();
        sr.content  = contentRect;
        SetField(logView, "logText",    logTmp);
        SetField(logView, "scrollRect", sr);

        // InventoryPanelView 필드 연결
        var invContainer = CreateVLGContainer(invGO, "SlotContainer");
        SetField(invView, "slotContainer", invContainer);
        SetField(invView, "slotPrefab",    LoadPrefab("InventorySlot"));

        // LunchboxPanelView — 6 슬롯 인라인 생성
        var slotViewObjs = new Object[6];
        for (int si = 0; si < 6; si++)
        {
            var slotGO = new GameObject($"Slot_{si}");
            slotGO.transform.SetParent(lunchGO.transform, false);
            slotGO.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f);
            var slotRect = slotGO.GetComponent<RectTransform>();
            slotRect.anchoredPosition = new Vector2(-125 + si * 50, 0);
            slotRect.sizeDelta = new Vector2(44, 44);
            var slotView = slotGO.AddComponent<LunchboxSlotView>();

            var iconGO = new GameObject("Icon");
            iconGO.transform.SetParent(slotGO.transform, false);
            iconGO.AddComponent<Image>().color = Color.white;
            SetStretch(iconGO.GetComponent<RectTransform>());

            var expryGO = new GameObject("ExpiryText");
            expryGO.transform.SetParent(slotGO.transform, false);
            var expryTmp = expryGO.AddComponent<TextMeshProUGUI>();
            expryTmp.fontSize = 10; expryTmp.alignment = TextAlignmentOptions.Center; expryTmp.color = Color.white;
            var expryRect = expryGO.GetComponent<RectTransform>();
            expryRect.anchorMin = new Vector2(0, 0); expryRect.anchorMax = new Vector2(1, 0);
            expryRect.pivot = new Vector2(0.5f, 0f);
            expryRect.anchoredPosition = new Vector2(0, 8); expryRect.sizeDelta = new Vector2(0, 16);

            var emptyGO = new GameObject("EmptyIndicator");
            emptyGO.transform.SetParent(slotGO.transform, false);
            emptyGO.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            SetStretch(emptyGO.GetComponent<RectTransform>());

            var expiredGO  = new GameObject("ExpiredOverlay");
            expiredGO.transform.SetParent(slotGO.transform, false);
            var expiredImg = expiredGO.AddComponent<Image>();
            expiredImg.color = new Color(0.8f, 0f, 0f, 0.5f);
            SetStretch(expiredGO.GetComponent<RectTransform>());
            expiredGO.SetActive(false);

            SetField(slotView, "iconImage",      iconGO.GetComponent<Image>());
            SetField(slotView, "expiryText",     expryTmp);
            SetField(slotView, "emptyIndicator", emptyGO);
            SetField(slotView, "expiredOverlay", expiredImg);

            slotViewObjs[si] = slotView;
        }
        SetArrayField(lunchView, "slotViews", slotViewObjs);

        // DungeonViewController
        var dvc = center.AddComponent<DungeonViewController>();
        SetField(dvc, "floorText",     floorGO.GetComponent<TextMeshProUGUI>());
        SetField(dvc, "stateText",     stateGO.GetComponent<TextMeshProUGUI>());
        SetField(dvc, "actionButtons", actionGO);
        SetField(dvc, "descendButton", descendGO.GetComponent<Button>());
        SetField(dvc, "returnButton",  returnGO.GetComponent<Button>());
        SetField(dvc, "battlePanel",   battleGO);
        SetField(dvc, "bossPanel",     bossGO);
        SetField(dvc, "adventureLog",  logView);

        UnityEventTools.AddPersistentListener(descendGO.GetComponent<Button>().onClick, dvc.OnClickDescend);
        UnityEventTools.AddPersistentListener(returnGO.GetComponent<Button>().onClick,  dvc.OnClickReturn);

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[DungeonLunch] Dungeon 씬 구성 완료");
    }

    private static void BuildLoadingScene()
    {
        var scene = EditorSceneManager.OpenScene(SCENE_ROOT + "3.Loading.unity", OpenSceneMode.Single);
        ClearScene();

        CreateOrthoCamera();
        var canvas = CreateCanvas("Canvas");

        var bg = CreateUIImage(canvas, "Background", Color.black);
        SetStretch(bg.GetComponent<RectTransform>());
        CreateText(canvas, "LoadingText", "Loading...", 48, Vector2.zero, new Vector2(400, 80));

        var loaderGO = new GameObject("SceneLoader");
        loaderGO.AddComponent<DungeonSceneLoader>();

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[DungeonLunch] Loading 씬 구성 완료");
    }

    [MenuItem("DungeonLunch/▶ 전체 빌드")]
    public static void BuildAll()
    {
        BuildPartyCardPrefab();
        BuildRecruitCardPrefab();
        BuildShopItemPrefab();
        BuildRecipeEntryPrefab();
        BuildInventorySlotPrefab();
        RegisterScenes();
        BuildTitleScene();
        BuildTownScene();
        BuildDungeonScene();
        BuildLoadingScene();
        Debug.Log("[DungeonLunch] 전체 씬 구성 완료");
    }

    private static void BuildPartyCardPrefab()
    {
        const string prefabDir  = "Assets/Prefabs/DL";
        const string prefabPath = prefabDir + "/PartyMemberCard.prefab";

        if (!System.IO.Directory.Exists(prefabDir))
            System.IO.Directory.CreateDirectory(prefabDir);

        // 임시 루트
        var root = new GameObject("PartyMemberCard");
        var rootImg = root.AddComponent<Image>();
        rootImg.color = new Color(0.18f, 0.18f, 0.18f);
        var rootRect = root.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(320, 130);

        var cardView = root.AddComponent<PartyMemberCardView>();

        // 이름
        var nameTxt = CreateText(root, "NameText", "파티원", 22, new Vector2(0, 44), new Vector2(300, 36));

        // HP 슬라이더
        var hpSlider  = CreateSlider(root, "HPSlider",     new Vector2(0, 14), new Vector2(280, 20), new Color(0.2f, 0.8f, 0.2f));
        var hpTxt     = CreateText(root,  "HPText",        "50/50", 16, new Vector2(100, 14), new Vector2(100, 20));

        // 포만감 슬라이더
        var hungerSlider = CreateSlider(root, "HungerSlider", new Vector2(0, -12), new Vector2(280, 20), new Color(0.9f, 0.7f, 0.2f));

        // 확장 패널
        var expandedPanel = new GameObject("ExpandedPanel");
        expandedPanel.transform.SetParent(root.transform, false);
        expandedPanel.AddComponent<Image>().color = new Color(0.14f, 0.14f, 0.14f);
        var epRect = expandedPanel.GetComponent<RectTransform>();
        epRect.anchoredPosition = new Vector2(0, -110);
        epRect.sizeDelta = new Vector2(320, 160);

        var proteinSlider = CreateSlider(expandedPanel, "ProteinSlider",  new Vector2(0,  56), new Vector2(280, 16), new Color(0.9f, 0.3f, 0.3f));
        var carbsSlider   = CreateSlider(expandedPanel, "CarbsSlider",    new Vector2(0,  32), new Vector2(280, 16), new Color(0.9f, 0.8f, 0.2f));
        var fatSlider     = CreateSlider(expandedPanel, "FatSlider",      new Vector2(0,   8), new Vector2(280, 16), new Color(0.5f, 0.8f, 0.5f));
        var magicSlider   = CreateSlider(expandedPanel, "MagicSlider",    new Vector2(0, -16), new Vector2(280, 16), new Color(0.4f, 0.4f, 0.9f));
        var strLvlTxt     = CreateText(expandedPanel, "StrengthLvText",   "근력 Lv 0", 14, new Vector2(-90, -46), new Vector2(100, 24));
        var conLvlTxt     = CreateText(expandedPanel, "ConstitutionLvText","체력 Lv 0", 14, new Vector2(0,  -46), new Vector2(100, 24));
        var magLvlTxt     = CreateText(expandedPanel, "MagicLvText",      "마력 Lv 0", 14, new Vector2(90, -46), new Vector2(100, 24));

        expandedPanel.SetActive(false);

        // 버튼
        var btn = root.AddComponent<UnityEngine.UI.Button>();
        UnityEventTools.AddPersistentListener(btn.onClick, cardView.OnClickCard);

        // 필드 연결
        SetField(cardView, "nameText",              nameTxt.GetComponent<TextMeshProUGUI>());
        SetField(cardView, "hpSlider",              hpSlider.GetComponent<Slider>());
        SetField(cardView, "hungerSlider",          hungerSlider.GetComponent<Slider>());
        SetField(cardView, "hpText",                hpTxt.GetComponent<TextMeshProUGUI>());
        SetField(cardView, "cardBackground",        rootImg);
        SetField(cardView, "expandedPanel",         expandedPanel);
        SetField(cardView, "proteinSlider",         proteinSlider.GetComponent<Slider>());
        SetField(cardView, "carbsSlider",           carbsSlider.GetComponent<Slider>());
        SetField(cardView, "fatSlider",             fatSlider.GetComponent<Slider>());
        SetField(cardView, "magicSlider",           magicSlider.GetComponent<Slider>());
        SetField(cardView, "strengthLevelText",     strLvlTxt.GetComponent<TextMeshProUGUI>());
        SetField(cardView, "constitutionLevelText", conLvlTxt.GetComponent<TextMeshProUGUI>());
        SetField(cardView, "magicLevelText",        magLvlTxt.GetComponent<TextMeshProUGUI>());
        // 색상 기본값
        SetColorField(cardView, "normalColor",       new Color(0.18f, 0.18f, 0.18f));
        SetColorField(cardView, "dangerColor",       new Color(0.5f,  0.1f,  0.1f));
        SetColorField(cardView, "incapacitatedColor",new Color(0.15f, 0.15f, 0.15f, 0.5f));

        var prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);

        Debug.Log($"[DungeonLunch] PartyMemberCard 프리팹 생성: {prefabPath}");
    }

    // ──────────────────────────────────────────────
    // 헬퍼
    // ──────────────────────────────────────────────

    private static void ClearScene()
    {
        foreach (var go in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            Object.DestroyImmediate(go);
    }

    private static void CreateManagers()
    {
        Create<DungeonGameManager>("DungeonGameManager");
        Create<PartyManager>("PartyManager");
        Create<DungeonInventoryManager>("DungeonInventoryManager");
        Create<BattleManager>("BattleManager");
        Create<ExpiryManager>("ExpiryManager");
        Create<LunchboxManager>("LunchboxManager");
        Create<CookingManager>("CookingManager");
        Create<ShopManager>("ShopManager");
        Create<SaveManager>("SaveManager");
    }

    private static GameObject Create<T>(string name) where T : Component
    {
        var go = new GameObject(name);
        go.AddComponent<T>();
        return go;
    }

    private static Camera CreateOrthoCamera()
    {
        var go = new GameObject("Main Camera");
        go.tag = "MainCamera";
        var cam = go.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.05f, 0.05f, 0.05f);
        go.AddComponent<AudioListener>();
        return cam;
    }

    private static GameObject CreateCanvas(string name)
    {
        var go = new GameObject(name);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        go.AddComponent<GraphicRaycaster>();

        // EventSystem 없으면 생성
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<StandaloneInputModule>();
        }

        return go;
    }

    private static GameObject CreatePanel(GameObject parent, string name, Vector2 pos, Vector2 size, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<Image>().color = color;
        var rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
        return go;
    }

    private static GameObject CreateUIImage(GameObject parent, string name, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<Image>().color = color;
        return go;
    }

    private static GameObject CreateText(GameObject parent, string name, string text, int fontSize, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        var rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
        return go;
    }

    private static GameObject CreateButton(GameObject parent, string name, string label, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<Image>().color = new Color(0.2f, 0.4f, 0.8f);
        go.AddComponent<Button>();
        var rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform, false);
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        SetStretch(textGO.GetComponent<RectTransform>());

        return go;
    }

    private static void SetStretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void SetField(Object target, string fieldName, Object value)
    {
        var so = new SerializedObject(target);
        var prop = so.FindProperty(fieldName);
        if (prop != null) { prop.objectReferenceValue = value; so.ApplyModifiedProperties(); }
        else Debug.LogWarning($"[SceneBuilder] 필드 못 찾음: {target.GetType().Name}.{fieldName}");
    }

    private static void SetColorField(Object target, string fieldName, Color value)
    {
        var so = new SerializedObject(target);
        var prop = so.FindProperty(fieldName);
        if (prop != null) { prop.colorValue = value; so.ApplyModifiedProperties(); }
        else Debug.LogWarning($"[SceneBuilder] 필드 못 찾음: {target.GetType().Name}.{fieldName}");
    }

    private static Transform CreateVLGContainer(GameObject parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<Image>().color = new Color(0, 0, 0, 0);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1); rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = Vector2.zero; rect.sizeDelta = Vector2.zero;
        var vlg = go.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 8; vlg.padding = new RectOffset(8, 8, 8, 8);
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true; vlg.childForceExpandWidth = true;
        vlg.childControlHeight = false; vlg.childForceExpandHeight = false;
        return go.transform;
    }

    private static GameObject LoadPrefab(string name) =>
        AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/DL/{name}.prefab");

    private static void SetArrayField(Object target, string fieldName, Object[] values)
    {
        var so = new SerializedObject(target);
        var prop = so.FindProperty(fieldName);
        if (prop == null) { Debug.LogWarning($"[SceneBuilder] 필드 못 찾음: {target.GetType().Name}.{fieldName}"); return; }
        prop.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        so.ApplyModifiedProperties();
    }

    // ──────────────────────────────────────────────
    // 추가 프리팹 빌더
    // ──────────────────────────────────────────────

    private static void BuildRecruitCardPrefab()
    {
        const string path = "Assets/Prefabs/DL/RecruitCard.prefab";
        if (!System.IO.Directory.Exists("Assets/Prefabs/DL"))
            System.IO.Directory.CreateDirectory("Assets/Prefabs/DL");

        var root = new GameObject("RecruitCard");
        root.AddComponent<Image>().color = new Color(0.18f, 0.18f, 0.18f);
        root.GetComponent<RectTransform>().sizeDelta = new Vector2(320, 100);
        var view = root.AddComponent<RecruitCardView>();

        var nameTxt  = CreateText(root, "NameText",  "이름", 20, new Vector2(0,  28), new Vector2(280, 30));
        var statsTxt = CreateText(root, "StatsText", "스탯", 16, new Vector2(0,   0), new Vector2(280, 24));
        var costTxt  = CreateText(root, "CostText",  "비용", 16, new Vector2(60, -28), new Vector2(120, 24));
        CreateButton(root, "HireButton", "영입", new Vector2(-70, -28), new Vector2(100, 30));

        SetField(view, "nameText",  nameTxt.GetComponent<TextMeshProUGUI>());
        SetField(view, "statsText", statsTxt.GetComponent<TextMeshProUGUI>());
        SetField(view, "costText",  costTxt.GetComponent<TextMeshProUGUI>());

        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        Debug.Log($"[DungeonLunch] RecruitCard 프리팹 생성: {path}");
    }

    private static void BuildShopItemPrefab()
    {
        const string path = "Assets/Prefabs/DL/ShopItem.prefab";
        if (!System.IO.Directory.Exists("Assets/Prefabs/DL"))
            System.IO.Directory.CreateDirectory("Assets/Prefabs/DL");

        var root = new GameObject("ShopItem");
        root.AddComponent<Image>().color = new Color(0.18f, 0.18f, 0.18f);
        root.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 70);
        var view = root.AddComponent<ShopItemView>();

        var iconGO   = CreateUIImage(root, "Icon", new Color(0.3f, 0.3f, 0.3f));
        iconGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(-120, 0);
        iconGO.GetComponent<RectTransform>().sizeDelta = new Vector2(50, 50);
        var nameTxt  = CreateText(root, "NameText",  "아이템", 18, new Vector2(-30,  12), new Vector2(160, 28));
        var priceTxt = CreateText(root, "PriceText", "0G",      16, new Vector2(-30, -12), new Vector2(100, 24));
        var stockTxt = CreateText(root, "StockText", "재고 0",  14, new Vector2( 60, -12), new Vector2(80,  24));
        var buyBtn   = CreateButton(root, "BuyButton", "구매", new Vector2(110, 0), new Vector2(80, 50));

        SetField(view, "iconImage",  iconGO.GetComponent<Image>());
        SetField(view, "nameText",   nameTxt.GetComponent<TextMeshProUGUI>());
        SetField(view, "priceText",  priceTxt.GetComponent<TextMeshProUGUI>());
        SetField(view, "stockText",  stockTxt.GetComponent<TextMeshProUGUI>());
        SetField(view, "buyButton",  buyBtn.GetComponent<Button>());

        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        Debug.Log($"[DungeonLunch] ShopItem 프리팹 생성: {path}");
    }

    private static void BuildRecipeEntryPrefab()
    {
        const string path = "Assets/Prefabs/DL/RecipeEntry.prefab";
        if (!System.IO.Directory.Exists("Assets/Prefabs/DL"))
            System.IO.Directory.CreateDirectory("Assets/Prefabs/DL");

        var root = new GameObject("RecipeEntry");
        root.AddComponent<Image>().color = new Color(0.18f, 0.18f, 0.18f);
        root.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 70);
        var view = root.AddComponent<RecipeEntryView>();

        var nameTxt  = CreateText(root, "NameText",        "레시피", 18, new Vector2(-40,  12), new Vector2(160, 28));
        var ingrTxt  = CreateText(root, "IngredientsText", "재료",   14, new Vector2(-40, -12), new Vector2(200, 24));
        var cookBtn  = CreateButton(root, "CookButton", "조리", new Vector2(110, 0), new Vector2(80, 50));

        SetField(view, "nameText",        nameTxt.GetComponent<TextMeshProUGUI>());
        SetField(view, "ingredientsText", ingrTxt.GetComponent<TextMeshProUGUI>());
        SetField(view, "cookButton",      cookBtn.GetComponent<Button>());

        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        Debug.Log($"[DungeonLunch] RecipeEntry 프리팹 생성: {path}");
    }

    private static void BuildInventorySlotPrefab()
    {
        const string path = "Assets/Prefabs/DL/InventorySlot.prefab";
        if (!System.IO.Directory.Exists("Assets/Prefabs/DL"))
            System.IO.Directory.CreateDirectory("Assets/Prefabs/DL");

        var root = new GameObject("InventorySlot");
        root.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f);
        root.GetComponent<RectTransform>().sizeDelta = new Vector2(60, 70);
        var view = root.AddComponent<InventorySlotView>();

        var iconGO    = CreateUIImage(root, "Icon", Color.white);
        iconGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 8);
        iconGO.GetComponent<RectTransform>().sizeDelta = new Vector2(48, 48);
        var countTxt  = CreateText(root, "CountText",  "1",  12, new Vector2(18,  30), new Vector2(28, 20));
        var expiryTxt = CreateText(root, "ExpiryText", "3d", 11, new Vector2( 0, -22), new Vector2(56, 18));
        var warnGO    = CreateUIImage(root, "WarningImage", new Color(0.9f, 0.2f, 0.1f, 0.8f));
        warnGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 8);
        warnGO.GetComponent<RectTransform>().sizeDelta = new Vector2(48, 48);
        warnGO.SetActive(false);

        SetField(view, "iconImage",    iconGO.GetComponent<Image>());
        SetField(view, "countText",    countTxt.GetComponent<TextMeshProUGUI>());
        SetField(view, "expiryText",   expiryTxt.GetComponent<TextMeshProUGUI>());
        SetField(view, "warningImage", warnGO.GetComponent<Image>());

        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        Debug.Log($"[DungeonLunch] InventorySlot 프리팹 생성: {path}");
    }

    private static GameObject CreateSlider(GameObject parent, string name, Vector2 pos, Vector2 size, Color fillColor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var slider = go.AddComponent<Slider>();
        var rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;

        // Background
        var bg = new GameObject("Background");
        bg.transform.SetParent(go.transform, false);
        bg.AddComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f);
        SetStretch(bg.GetComponent<RectTransform>());

        // Fill Area
        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(go.transform, false);
        var faRect = fillArea.AddComponent<RectTransform>();
        faRect.anchorMin = new Vector2(0, 0); faRect.anchorMax = new Vector2(1, 1);
        faRect.offsetMin = Vector2.zero; faRect.offsetMax = Vector2.zero;

        var fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        fill.AddComponent<Image>().color = fillColor;
        SetStretch(fill.GetComponent<RectTransform>());

        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.value = 1f;
        slider.interactable = false;

        return go;
    }
}
