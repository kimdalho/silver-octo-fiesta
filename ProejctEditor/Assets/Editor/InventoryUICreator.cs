using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class InventoryUICreator
{
    [MenuItem("Tools/Create Inventory UI")]
    public static void Create()
    {
        // ── 1. InventoryManager (씬에 없으면 생성) ──
        if (Object.FindFirstObjectByType<InventoryManager>() == null)
        {
            var mgrGo = new GameObject("InventoryManager");
            mgrGo.AddComponent<InventoryManager>();
            Undo.RegisterCreatedObjectUndo(mgrGo, "Create InventoryManager");
        }

        // ── 2. Canvas (씬에 없으면 생성) ──
        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            var canvasGo = new GameObject("Canvas");
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(canvasGo, "Create Canvas");
        }

        // EventSystem
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Undo.RegisterCreatedObjectUndo(esGo, "Create EventSystem");
        }

        var canvasRT = canvas.GetComponent<RectTransform>();

        // ── 3. InventoryRoot (InventoryUI 컴포넌트) ──
        var root = CreateChild(canvasRT, "InventoryRoot");
        var inventoryUI = root.gameObject.AddComponent<InventoryUI>();

        // 화면 우측 앵커
        root.anchorMin = new Vector2(1, 0);
        root.anchorMax = new Vector2(1, 1);
        root.pivot = new Vector2(1, 0.5f);
        root.offsetMin = new Vector2(-320, 20);
        root.offsetMax = new Vector2(-10, -20);

        // ── 4. Panel (토글 대상) ──
        var panel = CreateChild(root, "Panel");
        panel.anchorMin = Vector2.zero;
        panel.anchorMax = Vector2.one;
        panel.offsetMin = Vector2.zero;
        panel.offsetMax = Vector2.zero;

        var panelImg = panel.gameObject.AddComponent<Image>();
        panelImg.color = new Color(0.12f, 0.1f, 0.08f, 0.9f);

        inventoryUI.panel = panel.gameObject;

        // ── 5. 장비 영역 (상단) ──
        var equipArea = CreateChild(panel, "EquipmentArea");
        equipArea.anchorMin = new Vector2(0, 0.7f);
        equipArea.anchorMax = new Vector2(1, 1);
        equipArea.offsetMin = new Vector2(10, 5);
        equipArea.offsetMax = new Vector2(-10, -10);

        var equipLayout = equipArea.gameObject.AddComponent<HorizontalLayoutGroup>();
        equipLayout.spacing = 8;
        equipLayout.padding = new RectOffset(10, 10, 5, 5);
        equipLayout.childAlignment = TextAnchor.MiddleCenter;
        equipLayout.childForceExpandWidth = true;
        equipLayout.childForceExpandHeight = true;

        var equipUI = equipArea.gameObject.AddComponent<EquipmentUI>();

        // 장비 슬롯 3개
        string[] equipNames = { "WeaponSlot", "HeadSlot", "BodySlot" };
        string[] equipLabels = { "무기", "머리", "몸통" };
        SlotUI[] equipSlots = new SlotUI[3];
        for (int i = 0; i < 3; i++)
        {
            var slot = CreateSlot(equipArea, equipNames[i], equipLabels[i]);
            equipSlots[i] = slot;
        }
        equipUI.weaponSlot = equipSlots[0];
        equipUI.headSlot = equipSlots[1];
        equipUI.bodySlot = equipSlots[2];

        // ── 6. 인벤토리 영역 (하단 3x5 그리드) ──
        var gridArea = CreateChild(panel, "GridArea");
        gridArea.anchorMin = new Vector2(0, 0);
        gridArea.anchorMax = new Vector2(1, 0.7f);
        gridArea.offsetMin = new Vector2(10, 10);
        gridArea.offsetMax = new Vector2(-10, -5);

        var grid = gridArea.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(55, 55);
        grid.spacing = new Vector2(5, 5);
        grid.padding = new RectOffset(5, 5, 5, 5);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 5;
        grid.childAlignment = TextAnchor.UpperCenter;

        // 인벤토리 슬롯 15개
        SlotUI[] invSlots = new SlotUI[15];
        for (int i = 0; i < 15; i++)
        {
            var slot = CreateSlot(gridArea, "Slot_" + i, "");
            invSlots[i] = slot;
        }
        inventoryUI.slots = invSlots;

        // ── 7. ItemTooltip ──
        var tooltip = CreateChild(canvasRT, "ItemTooltip");
        tooltip.sizeDelta = new Vector2(200, 120);

        var tooltipImg = tooltip.gameObject.AddComponent<Image>();
        tooltipImg.color = new Color(0.08f, 0.06f, 0.04f, 0.95f);
        tooltipImg.raycastTarget = false;

        var tooltipUI = tooltip.gameObject.AddComponent<ItemTooltipUI>();

        // 이름 TMP
        var nameRT = CreateChild(tooltip, "NameText");
        nameRT.anchorMin = new Vector2(0, 0.7f);
        nameRT.anchorMax = new Vector2(1, 1);
        nameRT.offsetMin = new Vector2(8, 0);
        nameRT.offsetMax = new Vector2(-8, -4);
        var nameTMP = nameRT.gameObject.AddComponent<TextMeshProUGUI>();
        nameTMP.fontSize = 16;
        nameTMP.fontStyle = FontStyles.Bold;
        nameTMP.color = Color.white;
        nameTMP.raycastTarget = false;
        tooltipUI.nameText = nameTMP;

        // 설명 TMP
        var descRT = CreateChild(tooltip, "DescText");
        descRT.anchorMin = new Vector2(0, 0.3f);
        descRT.anchorMax = new Vector2(1, 0.7f);
        descRT.offsetMin = new Vector2(8, 0);
        descRT.offsetMax = new Vector2(-8, 0);
        var descTMP = descRT.gameObject.AddComponent<TextMeshProUGUI>();
        descTMP.fontSize = 12;
        descTMP.color = new Color(0.8f, 0.8f, 0.8f);
        descTMP.raycastTarget = false;
        tooltipUI.descriptionText = descTMP;

        // 스탯 TMP
        var statRT = CreateChild(tooltip, "StatText");
        statRT.anchorMin = new Vector2(0, 0);
        statRT.anchorMax = new Vector2(1, 0.3f);
        statRT.offsetMin = new Vector2(8, 4);
        statRT.offsetMax = new Vector2(-8, 0);
        var statTMP = statRT.gameObject.AddComponent<TextMeshProUGUI>();
        statTMP.fontSize = 12;
        statTMP.color = new Color(0.4f, 0.9f, 0.4f);
        statTMP.raycastTarget = false;
        tooltipUI.statText = statTMP;

        // ── 8. DraggedItem (Canvas 마지막 자식 = 최상단 렌더링) ──
        var dragged = CreateChild(canvasRT, "DraggedItem");
        dragged.sizeDelta = new Vector2(50, 50);

        var dragImg = dragged.gameObject.AddComponent<Image>();
        dragImg.raycastTarget = false;

        var dragCG = dragged.gameObject.AddComponent<CanvasGroup>();
        var dragUI = dragged.gameObject.AddComponent<DraggedItemUI>();
        dragUI.iconImage = dragImg;
        dragUI.canvasGroup = dragCG;

        // ── 완료 ──
        EditorUtility.SetDirty(canvas.gameObject);
        Undo.RegisterCreatedObjectUndo(root.gameObject, "Create Inventory UI");

        Debug.Log("인벤토리 UI 생성 완료! (Tools > Create Inventory UI)");
        Selection.activeGameObject = root.gameObject;
    }

    static SlotUI CreateSlot(RectTransform parent, string name, string label)
    {
        var slotRT = CreateChild(parent, name);

        // 배경
        var bgImg = slotRT.gameObject.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.18f, 0.15f, 0.8f);

        var slotUI = slotRT.gameObject.AddComponent<SlotUI>();

        // 아이콘 이미지
        var iconRT = CreateChild(slotRT, "Icon");
        iconRT.anchorMin = new Vector2(0.1f, 0.1f);
        iconRT.anchorMax = new Vector2(0.9f, 0.9f);
        iconRT.offsetMin = Vector2.zero;
        iconRT.offsetMax = Vector2.zero;
        var iconImg = iconRT.gameObject.AddComponent<Image>();
        iconImg.color = new Color(1, 1, 1, 0); // 비어있으면 투명
        iconImg.raycastTarget = false;
        slotUI.iconImage = iconImg;

        // 수량 텍스트
        var countRT = CreateChild(slotRT, "Count");
        countRT.anchorMin = new Vector2(0.5f, 0);
        countRT.anchorMax = new Vector2(1, 0.35f);
        countRT.offsetMin = Vector2.zero;
        countRT.offsetMax = Vector2.zero;
        var countTMP = countRT.gameObject.AddComponent<TextMeshProUGUI>();
        countTMP.fontSize = 11;
        countTMP.alignment = TextAlignmentOptions.BottomRight;
        countTMP.color = Color.white;
        countTMP.raycastTarget = false;
        slotUI.countText = countTMP;

        // 라벨 (장비 슬롯용)
        if (!string.IsNullOrEmpty(label))
        {
            var labelRT = CreateChild(slotRT, "Label");
            labelRT.anchorMin = new Vector2(0, 0.7f);
            labelRT.anchorMax = new Vector2(1, 1);
            labelRT.offsetMin = Vector2.zero;
            labelRT.offsetMax = Vector2.zero;
            var labelTMP = labelRT.gameObject.AddComponent<TextMeshProUGUI>();
            labelTMP.text = label;
            labelTMP.fontSize = 10;
            labelTMP.alignment = TextAlignmentOptions.Top;
            labelTMP.color = new Color(0.6f, 0.6f, 0.6f);
            labelTMP.raycastTarget = false;
        }

        return slotUI;
    }

    static RectTransform CreateChild(RectTransform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }
}
