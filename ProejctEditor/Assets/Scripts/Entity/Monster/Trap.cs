using UnityEngine;

public class Trap : MonoBehaviour
{
    public float activationRadius = 2f;
    public float duration = 10f;

    void Update()
    {
        duration -= Time.deltaTime;
        if (duration <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        Collider[] cols = Physics.OverlapSphere(transform.position, activationRadius);
        foreach (var col in cols)
        {
            MonsterAI ai = col.GetComponent<MonsterAI>();
            if (ai != null && !ai.isTrapped && !ai.isCaptured)
            {
                ai.ApplyTrap();
                Debug.Log($"[덫] {ai.data?.name ?? "몬스터"} 포획됨!");
                Destroy(gameObject);
                return;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationRadius);
    }
}
