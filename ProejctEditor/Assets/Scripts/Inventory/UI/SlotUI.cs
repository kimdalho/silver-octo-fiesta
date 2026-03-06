using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image iconImage;
    public TextMeshProUGUI countText;

    // 인벤토리 슬롯이면 >= 0, 장비 슬롯이면 -1
    [HideInInspector] public int slotIndex = -1;
    // 장비 슬롯이면 해당 슬롯 타입
    [HideInInspector] public EquipSlot equipSlot;
    [HideInInspector] public bool isEquipmentSlot;

    private ItemStack currentStack;

    public void SetItem(ItemStack stack)
    {
        currentStack = stack;
        if (stack != null && stack.data != null)
        {
            iconImage.sprite = stack.data.icon;
            iconImage.color = Color.white;
            countText.text = stack.count > 1 ? stack.count.ToString() : "";
        }
        else
        {
            Clear();
        }
    }

    public void Clear()
    {
        currentStack = null;
        iconImage.sprite = null;
        iconImage.color = new Color(1, 1, 1, 0);
        countText.text = "";
    }

    public ItemStack GetItem() => currentStack;

    // --- Drag & Drop ---

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentStack == null) return;
        DraggedItemUI.instance.BeginDrag(currentStack, this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        DraggedItemUI.instance.UpdatePosition(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DraggedItemUI.instance.EndDrag();
    }

    public void OnDrop(PointerEventData eventData)
    {
        var source = DraggedItemUI.instance.sourceSlot;
        if (source == null || source == this) return;

        var mgr = InventoryManager.instance;
        var inv = mgr.inventory;

        // 경우 1: 인벤 → 인벤 (스왑)
        if (!source.isEquipmentSlot && !isEquipmentSlot)
        {
            inv.SwapSlots(source.slotIndex, slotIndex);
        }
        // 경우 2: 인벤 → 장비 (장착)
        else if (!source.isEquipmentSlot && isEquipmentSlot)
        {
            var stack = inv.slots[source.slotIndex];
            if (stack?.data is EquipmentData ed && ed.equipSlot == equipSlot)
            {
                ItemStack returned;
                mgr.EquipDirect(stack, equipSlot, out returned);
                inv.SetSlot(source.slotIndex, returned);
            }
        }
        // 경우 3: 장비 → 인벤 (해제)
        else if (source.isEquipmentSlot && !isEquipmentSlot)
        {
            var equipData = mgr.equipment.GetEquip(source.equipSlot);
            if (equipData == null) return;

            var existingStack = inv.slots[slotIndex];

            // 타겟 슬롯에 장비가능 아이템이 있으면 교환
            if (existingStack?.data is EquipmentData targetEquip && targetEquip.equipSlot == source.equipSlot)
            {
                mgr.equipment.Unequip(source.equipSlot);
                inv.SetSlot(slotIndex, new ItemStack(equipData));
                mgr.equipment.Equip(targetEquip);
            }
            else if (existingStack == null)
            {
                mgr.equipment.Unequip(source.equipSlot);
                inv.SetSlot(slotIndex, new ItemStack(equipData));
            }
        }
        // 경우 4: 장비 → 장비 (같은 슬롯이면 무시)
    }

    // --- Tooltip ---

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentStack != null)
            ItemTooltipUI.instance?.Show(currentStack.data, transform.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ItemTooltipUI.instance?.Hide();
    }
}
