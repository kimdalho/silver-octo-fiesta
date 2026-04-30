using UnityEngine;

public class PartyPanelView : MonoBehaviour
{
    public PartyPanelPresenter presenter;

    [SerializeField] private Transform      cardContainer;
    [SerializeField] private GameObject     cardPrefab;

    void Start()
    {
        presenter = new PartyPanelPresenter(this);
    }

    public void ClearCards()
    {
        foreach (Transform child in cardContainer)
            Destroy(child.gameObject);
    }

    public PartyMemberCardView CreateCard()
    {
        return Instantiate(cardPrefab, cardContainer).GetComponent<PartyMemberCardView>();
    }
}
