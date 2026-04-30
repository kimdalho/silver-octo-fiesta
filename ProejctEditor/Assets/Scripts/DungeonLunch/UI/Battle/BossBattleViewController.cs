using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum BossPattern { NORMAL, PARRY, DODGE, FATAL }

public class BossBattleViewController : MonoBehaviour
{
    [SerializeField] private TMP_Text   bossNameText;
    [SerializeField] private Slider     bossHPSlider;
    [SerializeField] private Slider     groggySlider;
    [SerializeField] private TMP_Text   patternText;
    [SerializeField] private GameObject parryPrompt;
    [SerializeField] private GameObject dodgePrompt;
    [SerializeField] private EnemyData  bossData;

    private float _bossHP;
    private float _groggyGauge;
    private const float GroggyMax = 100f;
    private bool _isGroggy;

    void OnEnable()
    {
        _bossHP = bossData.maxHP;
        _groggyGauge = 0f;
        bossNameText.text = bossData.enemyName;
        bossHPSlider.maxValue = bossData.maxHP;
        bossHPSlider.value    = bossData.maxHP;
        groggySlider.value    = 0f;
    }

    // 패링 성공 시 외부에서 호출
    public void OnParrySuccess()
    {
        AddGroggy(30f);
        // TODO: 반격 데미지
    }

    // 회피 성공 시
    public void OnDodgeSuccess()
    {
        AddGroggy(10f);
    }

    private void AddGroggy(float amount)
    {
        if (_isGroggy) return;
        _groggyGauge += amount;
        groggySlider.value = _groggyGauge / GroggyMax;
        if (_groggyGauge >= GroggyMax) EnterGroggy();
    }

    private void EnterGroggy()
    {
        _isGroggy = true;
        patternText.text = "그로기!";
        // 3초 후 재기동
        Invoke(nameof(ExitGroggy), 3f);
    }

    private void ExitGroggy()
    {
        _isGroggy = false;
        _groggyGauge = 0f;
        groggySlider.value = 0f;
    }

    public void TakeBossDamage(float dmg)
    {
        _bossHP = Mathf.Max(0f, _bossHP - (_isGroggy ? dmg * 2f : dmg));
        bossHPSlider.value = _bossHP;
        if (_bossHP <= 0) OnBossDefeated();
    }

    private void OnBossDefeated()
    {
        DungeonInventoryManager.instance.gold += UnityEngine.Random.Range(bossData.minGoldDrop, bossData.maxGoldDrop + 1);
        foreach (var m in PartyManager.instance.members)
            if (m.IsAlive) m.AddCombatXP(100f);
        DungeonGameManager.instance.ChangeState(DungeonState.Explore);
    }

    public void ShowPattern(BossPattern pattern)
    {
        patternText.text     = pattern.ToString();
        parryPrompt.SetActive(pattern == BossPattern.PARRY);
        dodgePrompt.SetActive(pattern == BossPattern.DODGE);
    }
}
