using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LunchboxSlotView : MonoBehaviour
{
    [SerializeField] private Image    iconImage;
    [SerializeField] private TMP_Text expiryText;
    [SerializeField] private GameObject emptyIndicator;
    [SerializeField] private Image    expiredOverlay;

    private int _slotIndex;
    private LunchboxPanelPresenter _presenter;

    public void Init(int index, LunchboxPanelPresenter presenter)
    {
        _slotIndex  = index;
        _presenter  = presenter;
    }

    public void Render(LunchboxSlot slot)
    {
        bool empty = slot.IsEmpty;
        emptyIndicator.SetActive(empty);
        iconImage.enabled = !empty;
        if (empty) return;

        iconImage.sprite = slot.food.icon;

        if (slot.food.isNonPerishable)
        {
            expiryText.text      = "∞";
            expiredOverlay.enabled = false;
        }
        else
        {
            float pct = slot.remainingTime / slot.food.expiryDuration;
            expiryText.text        = $"{pct * 100f:0}%";
            expiredOverlay.enabled = slot.IsExpired;
        }
    }

    public void OnClickSlot() => _presenter?.OnSlotClicked(_slotIndex);
}
