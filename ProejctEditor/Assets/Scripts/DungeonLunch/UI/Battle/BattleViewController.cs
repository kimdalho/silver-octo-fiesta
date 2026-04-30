using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleViewController : MonoBehaviour
{
    [SerializeField] private TMP_Text enemyNameText;
    [SerializeField] private Slider   enemyHPSlider;
    [SerializeField] private TMP_Text enemyHPText;
    [SerializeField] private EnemyData[] enemyPool; // 인스펙터에서 층별 적 배열

    void OnEnable()
    {
        BattleManager.instance.OnEnemyHPChanged += OnEnemyHP;
        BattleManager.instance.OnBattleEnd      += OnBattleEnd;
        StartBattle();
    }

    void OnDisable()
    {
        if (BattleManager.instance == null) return;
        BattleManager.instance.OnEnemyHPChanged -= OnEnemyHP;
        BattleManager.instance.OnBattleEnd      -= OnBattleEnd;
    }

    private void StartBattle()
    {
        var enemy = PickEnemy();
        if (enemy == null) return;
        enemyNameText.text   = enemy.enemyName;
        enemyHPSlider.maxValue = enemy.maxHP;
        enemyHPSlider.value  = enemy.maxHP;
        enemyHPText.text     = $"{enemy.maxHP:0}/{enemy.maxHP:0}";
        BattleManager.instance.StartBattle(enemy);
    }

    private EnemyData PickEnemy()
    {
        int floor = DungeonGameManager.instance.CurrentFloor;
        var valid = System.Array.FindAll(enemyPool, e => e.minFloor <= floor && floor <= e.maxFloor);
        return valid.Length > 0 ? valid[UnityEngine.Random.Range(0, valid.Length)] : null;
    }

    private void OnEnemyHP(float current, float max)
    {
        enemyHPSlider.value = Mathf.Max(0f, current);
        enemyHPText.text    = $"{Mathf.Max(0f, current):0}/{max:0}";
    }

    private void OnBattleEnd() => gameObject.SetActive(false);
}
