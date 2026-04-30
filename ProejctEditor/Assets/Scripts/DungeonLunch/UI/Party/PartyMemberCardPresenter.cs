public class PartyMemberCardPresenter
{
    public PartyMember model;
    public PartyMemberCardView view;

    public PartyMemberCardPresenter(PartyMember model, PartyMemberCardView view)
    {
        this.model = model;
        this.view  = view;
        view.presenter = this;
        model.OnStatsChanged += Refresh;
        model.growth.OnLevelUp += Refresh;
        Refresh();
    }

    public void Refresh() => view.Render(model);
    public void OnCardClicked() => view.ToggleExpanded();
}
