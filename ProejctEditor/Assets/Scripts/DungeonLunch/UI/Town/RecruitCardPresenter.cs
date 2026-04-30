public class RecruitCardPresenter
{
    public PartyMember model;
    public RecruitCardView view;
    private GuildPresenter _guild;

    public RecruitCardPresenter(PartyMember model, RecruitCardView view, GuildPresenter guild)
    {
        this.model = model;
        this.view  = view;
        _guild = guild;
        view.presenter = this;
        view.Render(model);
    }

    public void OnClickRecruit() => _guild.TryRecruit(model);
}
