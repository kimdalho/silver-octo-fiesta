using UnityEngine;
using TMPro;

public class ItemTooltipUI : MonoBehaviour
{
    public static ItemTooltipUI instance;

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI statText;

    private RectTransform rectTransform;

    void Awake()
    {
        instance = this;
        rectTransform = GetComponent<RectTransform>();
        gameObject.SetActive(false);
    }

    public void Show(ItemData data, Vector3 anchorPos)
    {
        nameText.text = data.itemName;
        descriptionText.text = data.description;

        if (data is EquipmentData equip && equip.statModifiers != null && equip.statModifiers.Length > 0)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var mod in equip.statModifiers)
            {
                string sign = mod.value >= 0 ? "+" : "";
                string suffix = mod.modifierType == ModifierType.Percent ? "%" : "";
                sb.AppendLine($"{mod.statType} {sign}{mod.value}{suffix}");
            }
            statText.text = sb.ToString();
        }
        else
        {
            statText.text = "";
        }

        gameObject.SetActive(true);

        // 툴팁 위치: 슬롯 왼쪽
        rectTransform.position = anchorPos + Vector3.left * (rectTransform.rect.width * 0.5f + 10f);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
