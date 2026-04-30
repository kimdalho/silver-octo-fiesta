public class InventoryPanelPresenter
{
    public DungeonInventoryManager model;
    public InventoryPanelView view;

    public InventoryPanelPresenter(DungeonInventoryManager model, InventoryPanelView view)
    {
        this.model = model;
        this.view  = view;
        view.presenter = this;
        model.OnInventoryChanged += Refresh;
        Refresh();
    }

    public void Refresh() => view.Render(model);
}
