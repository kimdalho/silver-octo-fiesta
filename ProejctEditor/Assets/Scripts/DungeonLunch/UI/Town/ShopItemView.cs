using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemView : MonoBehaviour
{
    [SerializeField] private Image    iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text stockText;
    [SerializeField] private Button   buyButton;

    private ShopItem _item;
    private ShopPresenter _presenter;

    public void Init(ShopItem item, ShopPresenter presenter)
    {
        _item      = item;
        _presenter = presenter;
        iconImage.sprite = item.item.icon;
        nameText.text    = item.item.itemName;
        priceText.text   = $"{item.price}G";
        stockText.text   = $"x{item.stock}";
        buyButton.onClick.AddListener(OnClickBuy);
    }

    private void OnClickBuy()
    {
        _presenter.OnBuyItem(_item);
        stockText.text = $"x{_item.stock}";
        buyButton.interactable = _item.stock > 0;
    }
}
