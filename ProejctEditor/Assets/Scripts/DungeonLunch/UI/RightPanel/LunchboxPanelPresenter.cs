public class LunchboxPanelPresenter
{
    public LunchboxManager model;
    public LunchboxPanelView view;

    public LunchboxPanelPresenter(LunchboxManager model, LunchboxPanelView view)
    {
        this.model = model;
        this.view  = view;
        view.presenter = this;
        model.OnLunchboxChanged += Refresh;
        Refresh();
    }

    public void Refresh() => view.Render(model);

    public void OnSlotClicked(int index)
    {
        if (DungeonGameManager.instance.CurrentState == DungeonState.Battle) return;
        LunchboxManager.instance.Consume(index);
    }
}
