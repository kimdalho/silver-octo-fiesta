using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopView : MonoBehaviour
{
    public ShopPresenter presenter;

    [SerializeField] private Transform  itemContainer;
    [SerializeField] private GameObject shopItemPrefab;
    [SerializeField] private TMP_Text   goldText;

    void Start()
    {
        presenter = new ShopPresenter(ShopManager.instance, this);
    }

    void OnEnable() => RefreshGold(DungeonInventoryManager.instance.gold);

    public void Render(List<ShopItem> items)
    {
        foreach (Transform child in itemContainer) Destroy(child.gameObject);
        foreach (var item in items)
            Instantiate(shopItemPrefab, itemContainer).GetComponent<ShopItemView>().Init(item, presenter);
    }

    public void RefreshGold(int gold) => goldText.text = $"보유 골드: {gold}G";
}
