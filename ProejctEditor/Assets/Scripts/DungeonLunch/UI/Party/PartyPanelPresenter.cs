using System.Collections.Generic;

public class PartyPanelPresenter
{
    public PartyPanelView view;
    private readonly List<PartyMemberCardPresenter> _cards = new List<PartyMemberCardPresenter>();

    public PartyPanelPresenter(PartyPanelView view)
    {
        this.view = view;
        view.presenter = this;
        PartyManager.instance.OnPartyChanged += Rebuild;
        Rebuild();
    }

    public void Rebuild()
    {
        view.ClearCards();
        _cards.Clear();
        foreach (var member in PartyManager.instance.members)
        {
            var cardView = view.CreateCard();
            _cards.Add(new PartyMemberCardPresenter(member, cardView));
        }
    }
}
