using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberCardView : MonoBehaviour
{
    public PartyMemberCardPresenter presenter;

    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Slider   hpSlider;
    [SerializeField] private Slider   hungerSlider;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private Image    cardBackground;
    [SerializeField] private Color    normalColor;
    [SerializeField] private Color    dangerColor;
    [SerializeField] private Color    incapacitatedColor;

    [Header("확장 패널")]
    [SerializeField] private GameObject expandedPanel;
    [SerializeField] private Slider     proteinSlider;
    [SerializeField] private Slider     carbsSlider;
    [SerializeField] private Slider     fatSlider;
    [SerializeField] private Slider     magicSlider;
    [SerializeField] private TMP_Text   strengthLevelText;
    [SerializeField] private TMP_Text   constitutionLevelText;
    [SerializeField] private TMP_Text   magicLevelText;

    private bool _isExpanded;

    public void Render(PartyMember m)
    {
        nameText.text     = m.memberName;
        hpSlider.value    = m.hp / m.MaxHP;
        hungerSlider.value = m.hunger / 100f;
        hpText.text       = $"{m.hp:0}/{m.MaxHP:0}";

        if (!m.IsAlive)                    cardBackground.color = incapacitatedColor;
        else if (m.hp / m.MaxHP <= 0.3f)   cardBackground.color = dangerColor;
        else                               cardBackground.color = normalColor;

        if (!_isExpanded) return;
        proteinSlider.value       = m.protein    / 100f;
        carbsSlider.value         = m.carbs      / 100f;
        fatSlider.value           = m.fat        / 100f;
        magicSlider.value         = m.magicPower / 100f;
        strengthLevelText.text    = $"근력 Lv {m.growth.strengthLevel}";
        constitutionLevelText.text = $"체력 Lv {m.growth.constitutionLevel}";
        magicLevelText.text       = $"마력 Lv {m.growth.magicLevel}";
    }

    public void ToggleExpanded()
    {
        _isExpanded = !_isExpanded;
        expandedPanel.SetActive(_isExpanded);
        presenter?.Refresh();
    }

    public void OnClickCard() => presenter?.OnCardClicked();
}
