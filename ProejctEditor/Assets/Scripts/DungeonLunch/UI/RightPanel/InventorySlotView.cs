using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotView : MonoBehaviour
{
    [SerializeField] private Image    iconImage;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private TMP_Text expiryText;
    [SerializeField] private Image    warningImage;

    public void Render(InventorySlot slot)
    {
        iconImage.sprite  = slot.item.icon;
        iconImage.enabled = slot.item.icon != null;
        countText.text    = slot.count.ToString();

        if (slot.item.isNonPerishable)
        {
            expiryText.text       = "∞";
            warningImage.enabled  = false;
        }
        else
        {
            float h = slot.remainingTime / 3600f;
            expiryText.text = h >= 1f ? $"{h:0.0}h" : $"{slot.remainingTime / 60f:0}m";
            warningImage.enabled = slot.remainingTime < slot.item.expiryDuration * 0.25f;
        }
    }
}
