using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 보관함 UI 패널.
/// PlacedObject(Storage) 가 E 키 상호작용 시 Open(chest)를 호출한다.
///
/// [Inspector 설정]
/// panel          : 전체 패널 루트
/// slotParent     : 슬롯들을 담을 Grid Layout Group이 있는 Transform
/// slotPrefab     : StorageSlotUI 컴포넌트가 붙은 Button 프리팹
/// titleText      : 패널 제목 TMP
/// closeButton    : 닫기 버튼
/// </summary>
public class StorageUI : MonoBehaviour
{
    public static StorageUI instance;

    [Header("UI References")]
    public GameObject panel;
    public Transform slotParent;
    public GameObject slotPrefab;
    public TextMeshProUGUI titleText;
    public Button closeButton;

    private StorageChest currentChest;
    private StorageSlotUI[] slotUIs;

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        panel?.SetActive(false);
        closeButton?.onClick.AddListener(Close);
    }

    public bool IsOpen => panel != null && panel.activeSelf;

    public void Open(StorageChest chest)
    {
        currentChest = chest;
        BuildSlots();
        panel?.SetActive(true);
        if (titleText != null) titleText.text = "보관함";
        CameraFollow.instance?.SetCursorLocked(false);
    }

    public void Close()
    {
        panel?.SetActive(false);
        currentChest = null;
        if (PlacementSystem.instance == null || !PlacementSystem.instance.IsPlacing)
            CameraFollow.instance?.SetCursorLocked(true);
    }

    void Update()
    {
        if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    void BuildSlots()
    {
        if (slotParent == null || slotPrefab == null || currentChest == null) return;

        for (int i = slotParent.childCount - 1; i >= 0; i--)
            Destroy(slotParent.GetChild(i).gameObject);

        slotUIs = new StorageSlotUI[StorageChest.Capacity];
        for (int i = 0; i < StorageChest.Capacity; i++)
        {
            var go = Instantiate(slotPrefab, slotParent);
            var ui = go.GetComponent<StorageSlotUI>();
            if (ui != null)
            {
                ui.Setup(currentChest, i, this);
                slotUIs[i] = ui;
            }
        }
    }

    public void RefreshSlots()
    {
        if (slotUIs == null) return;
        foreach (var s in slotUIs) s?.Refresh();
    }

    /// <summary>
    /// 플레이어 인벤토리 슬롯을 보관함으로 이동.
    /// StorageUI가 열려 있을 때 SlotUI 우클릭으로 호출된다.
    /// </summary>
    public void DepositSlot(int invSlotIndex)
    {
        if (currentChest == null) return;
        var inv = InventoryManager.instance?.inventory;
        if (inv == null) return;

        var stack = inv.slots[invSlotIndex];
        if (stack == null) return;

        if (currentChest.AddItem(stack.data, stack.count))
        {
            inv.RemoveItem(invSlotIndex);
            RefreshSlots();
        }
    }
}
