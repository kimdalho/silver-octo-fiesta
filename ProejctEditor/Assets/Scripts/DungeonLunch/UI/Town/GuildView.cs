using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuildView : MonoBehaviour
{
    public GuildPresenter presenter;

    [SerializeField] private Transform  cardContainer;
    [SerializeField] private GameObject recruitCardPrefab;
    [SerializeField] private Button     refreshButton;

    void Start()
    {
        presenter = new GuildPresenter(this);
        refreshButton.onClick.AddListener(presenter.Refresh);
    }

    public void Render(List<PartyMember> recruits, GuildPresenter p)
    {
        foreach (Transform child in cardContainer) Destroy(child.gameObject);
        foreach (var recruit in recruits)
        {
            var cardView = Instantiate(recruitCardPrefab, cardContainer).GetComponent<RecruitCardView>();
            new RecruitCardPresenter(recruit, cardView, p);
        }
    }
}
