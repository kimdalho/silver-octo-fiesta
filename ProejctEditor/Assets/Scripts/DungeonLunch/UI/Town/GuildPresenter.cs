using System.Collections.Generic;

public class GuildPresenter
{
    public GuildView view;
    private List<PartyMember> _recruits;
    private const int RecruitCost = 50;

    public GuildPresenter(GuildView view)
    {
        this.view = view;
        view.presenter = this;
        Refresh();
    }

    public void Refresh()
    {
        _recruits = PartyManager.instance.GenerateGuildRecruits();
        view.Render(_recruits, this);
    }

    public void TryRecruit(PartyMember member)
    {
        if (PartyManager.instance.members.Count >= PartyManager.MaxPartySize) return;
        if (DungeonInventoryManager.instance.gold < RecruitCost) return;
        DungeonInventoryManager.instance.gold -= RecruitCost;
        PartyManager.instance.AddMember(member);
        _recruits.Remove(member);
        view.Render(_recruits, this);
    }
}
