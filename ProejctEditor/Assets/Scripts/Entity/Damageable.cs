using System;
using System.Collections;
using UnityEngine;

public class Damageable : MonoBehaviour
{
    public float maxHP = 50f;
    public float currentHP;

    public event Action OnDied;

    private Renderer cachedRenderer;
    private Color originalColor;

    void Awake()
    {
        currentHP = maxHP;
        cachedRenderer = GetComponent<Renderer>();
        if (cachedRenderer != null)
            originalColor = cachedRenderer.material.color;
    }

    public void TakeDamage(float amount)
    {
        if (currentHP <= 0f) return;

        currentHP -= amount;

        // 피격 플래시
        if (cachedRenderer != null)
            StartCoroutine(FlashRed());

        if (currentHP <= 0f)
        {
            currentHP = 0f;
            Die();
        }
    }

    IEnumerator FlashRed()
    {
        cachedRenderer.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        if (cachedRenderer != null)
            cachedRenderer.material.color = originalColor;
    }

    void Die()
    {
        var dropTable = GetComponent<DropTable>();
        if (dropTable != null)
            dropTable.SpawnDrops(transform.position);

        OnDied?.Invoke();
        Destroy(gameObject);
    }
}
