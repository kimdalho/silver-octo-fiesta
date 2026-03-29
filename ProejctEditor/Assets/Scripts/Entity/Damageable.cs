using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// HP 관리 컴포넌트.
/// 몬스터: HP 0 → OnKnockedOut 이벤트 → MonsterAttributeState가 드랍·파괴 처리.
/// 비몬스터: HP 0 → DropTable 호출 → Destroy (기존 방식).
/// </summary>
public class Damageable : MonoBehaviour
{
    public float maxHP = 50f;
    public float currentHP;

    public bool IsKnockedOut { get; private set; }

    public event Action OnKnockedOut; // 몬스터용 — MonsterAttributeState가 구독
    public event Action OnDied;       // 비몬스터용 (호환성 유지)

    private Renderer cachedRenderer;
    private Color originalColor;

    void Awake()
    {
        currentHP = maxHP;
        cachedRenderer = GetComponentInChildren<Renderer>();
        if (cachedRenderer != null)
            originalColor = cachedRenderer.material.color;
    }

    public void TakeDamage(float amount)
    {
        if (IsKnockedOut) return;

        currentHP -= amount;

        if (cachedRenderer != null)
            StartCoroutine(FlashRed());

        if (currentHP <= 0f)
        {
            currentHP = 0f;
            KnockOut();
        }
    }

    IEnumerator FlashRed()
    {
        cachedRenderer.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        if (cachedRenderer != null)
            cachedRenderer.material.color = originalColor;
    }

    void KnockOut()
    {
        IsKnockedOut = true;
        OnKnockedOut?.Invoke();

        // MonsterAttributeState가 없는 오브젝트는 기존 방식으로 처리
        if (GetComponent<MonsterAttributeState>() == null)
        {
            var dropTable = GetComponent<DropTable>();
            if (dropTable != null)
                dropTable.SpawnDrops(transform.position);

            OnDied?.Invoke();
            Destroy(gameObject);
        }
    }
}
