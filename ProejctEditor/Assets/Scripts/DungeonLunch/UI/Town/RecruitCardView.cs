using TMPro;
using UnityEngine;

public class RecruitCardView : MonoBehaviour
{
    public RecruitCardPresenter presenter;

    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private TMP_Text costText;

    private const int RecruitCost = 50;

    public void Render(PartyMember m)
    {
        nameText.text  = m.memberName;
        statsText.text = $"근력 Lv{m.growth.strengthLevel}  체력 Lv{m.growth.constitutionLevel}  마력 Lv{m.growth.magicLevel}";
        costText.text  = $"{RecruitCost}G";
    }

    public void OnClickRecruit() => presenter?.OnClickRecruit();
}
