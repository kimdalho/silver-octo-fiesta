public class ShopPresenter
{
    public ShopManager model;
    public ShopView view;

    public ShopPresenter(ShopManager model, ShopView view)
    {
        this.model = model;
        this.view  = view;
        view.presenter = this;
        Refresh();
    }

    public void Refresh() => view.Render(model.shopItems);

    public void OnBuyItem(ShopItem item)
    {
        if (model.Buy(item))
            view.RefreshGold(DungeonInventoryManager.instance.gold);
    }
}
