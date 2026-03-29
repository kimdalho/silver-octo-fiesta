using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 보관함 슬롯 한 칸. Button + Image(icon) + TextMeshProUGUI(count) 구성.
/// StorageUI가 슬롯 수만큼 인스턴스화해서 Setup()을 호출한다.
/// </summary>
public class StorageSlotUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI countText;

    private StorageChest chest;
    private int slotIndex;
    private StorageUI owner;

    public void Setup(StorageChest chest, int index, StorageUI owner)
    {
        this.chest = chest;
        this.slotIndex = index;
        this.owner = owner;

        GetComponent<Button>()?.onClick.AddListener(OnClick);
        Refresh();
    }

    public void Refresh()
    {
        if (chest == null) return;
        var stack = chest.slots[slotIndex];
        bool has = stack != null && stack.data != null;

        if (iconImage != null)
        {
            iconImage.sprite = has ? stack.data.icon : null;
            iconImage.color = has ? Color.white : new Color(1, 1, 1, 0);
        }
        if (countText != null)
            countText.text = has && stack.count > 1 ? stack.count.ToString() : "";
    }

    // 클릭 → 플레이어 인벤토리로 이동
    void OnClick()
    {
        if (chest == null) return;
        var stack = chest.TakeSlot(slotIndex);
        if (stack == null) return;

        InventoryManager.instance?.inventory.AddItem(stack.data, stack.count);
        owner?.RefreshSlots();
    }
}
