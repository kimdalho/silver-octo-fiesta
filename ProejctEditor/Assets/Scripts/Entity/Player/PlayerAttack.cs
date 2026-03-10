using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public float attackRange = 1f;
    public float attackRadius = 0.5f;
    public float attackCooldown = 0.5f;

    private float lastAttackTime;
    private Vector3 lastMoveDir = Vector3.forward;

    void Update()
    {
        // 마지막 이동 방향 추적 (카메라 기준, PlayerMoveCC와 동일)
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        if (h != 0f || v != 0f)
        {
            Transform cam = Camera.main.transform;
            Vector3 forward = cam.forward;
            Vector3 right = cam.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            lastMoveDir = (forward * v + right * h).normalized;
        }

        if (Input.GetMouseButtonDown(0) && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }

    void Attack()
    {
        Vector3 attackPos = transform.position + lastMoveDir * attackRange;
        Collider[] hits = Physics.OverlapSphere(attackPos, attackRadius);

        // 삽 장착 여부 확인
        bool hasShovel = false;
        if (InventoryManager.instance != null)
        {
            var weapon = InventoryManager.instance.equipment.GetEquip(EquipSlot.Weapon);
            if (weapon != null && weapon.isShovel)
                hasShovel = true;
        }

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            // 블록은 삽 들고 있을 때만 타격
            if (hit.GetComponent<BlockMarker>() != null && !hasShovel)
                continue;

            var damageable = hit.GetComponent<Damageable>();
            if (damageable != null)
            {
                float damage = PlayerStats.instance != null ? PlayerStats.instance.Attack : 10f;
                damageable.TakeDamage(damage);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + lastMoveDir * attackRange, attackRadius);
    }
}
