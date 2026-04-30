using System;
using System.Collections;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;

    public EnemyData currentEnemy;
    private float _enemyHP;

    public event Action<float, float> OnEnemyHPChanged; // current, max
    public event Action<string> OnBattleLog;
    public event Action OnBattleEnd;

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartBattle(EnemyData enemy)
    {
        currentEnemy = enemy;
        _enemyHP = enemy.maxHP;
        StartCoroutine(AutoBattleRoutine());
    }

    private IEnumerator AutoBattleRoutine()
    {
        while (_enemyHP > 0 && !PartyManager.instance.IsWiped)
        {
            // 파티 공격
            foreach (var m in PartyManager.instance.members)
            {
                if (!m.IsAlive) continue;
                float dmg;
                bool isMagic = m.growth.IsMagicUnlocked && UnityEngine.Random.value < m.MagicCastChance;

                if (isMagic)
                {
                    dmg = 15f + m.growth.MagicDamageBonus;
                    OnBattleLog?.Invoke($"{m.memberName}의 마법 공격! {dmg:0}데미지");
                }
                else
                {
                    dmg = Mathf.Max(1f, m.Attack - currentEnemy.defense);
                    OnBattleLog?.Invoke($"{m.memberName}의 공격! {dmg:0}데미지");
                }

                _enemyHP -= dmg;
                OnEnemyHPChanged?.Invoke(_enemyHP, currentEnemy.maxHP);
                if (_enemyHP <= 0) break;
            }

            yield return new WaitForSeconds(1f);
            if (_enemyHP <= 0) break;

            // 적 공격
            foreach (var m in PartyManager.instance.members)
            {
                if (!m.IsAlive) continue;
                float dmg = Mathf.Max(1f, currentEnemy.attack - m.Defense);
                m.TakeDamage(dmg);
                OnBattleLog?.Invoke($"{currentEnemy.enemyName}의 공격! {m.memberName}에게 {dmg:0}데미지");
                if (currentEnemy.isSporeType) ExpiryManager.instance?.ApplySporeEffect();
            }

            yield return new WaitForSeconds(1f);

            if (PartyManager.instance.IsWiped)
            {
                DungeonGameManager.instance.OnPartyWiped();
                yield break;
            }
        }

        if (_enemyHP <= 0) OnVictory();
    }

    private void OnVictory()
    {
        int gold = UnityEngine.Random.Range(currentEnemy.minGoldDrop, currentEnemy.maxGoldDrop + 1);
        DungeonInventoryManager.instance.gold += gold;
        OnBattleLog?.Invoke($"전투 승리! 골드 {gold} 획득");

        for (int i = 0; i < currentEnemy.possibleDrops.Length; i++)
        {
            if (UnityEngine.Random.value <= currentEnemy.dropChances[i])
            {
                DungeonInventoryManager.instance.AddItem(currentEnemy.possibleDrops[i]);
                OnBattleLog?.Invoke($"{currentEnemy.possibleDrops[i].itemName} 획득!");
            }
        }

        foreach (var m in PartyManager.instance.members)
            if (m.IsAlive) m.AddCombatXP(20f);

        ExpiryManager.instance?.ResetSporeEffect();
        OnBattleEnd?.Invoke();
        DungeonGameManager.instance.ChangeState(DungeonState.Explore);
    }
}
