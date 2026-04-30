using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ShopItem
{
    public FoodData item;
    public int price;
    public int stock;
}

public class ShopManager : MonoBehaviour
{
    public static ShopManager instance;
    public List<ShopItem> shopItems = new List<ShopItem>();

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool Buy(ShopItem shopItem, int count = 1)
    {
        int total = shopItem.price * count;
        var inv = DungeonInventoryManager.instance;
        if (inv.gold < total || shopItem.stock < count) return false;
        inv.gold -= total;
        inv.AddItem(shopItem.item, count);
        shopItem.stock -= count;
        return true;
    }
}
