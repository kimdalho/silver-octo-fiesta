using System;
using UnityEngine;

public class LockOnTarget : MonoBehaviour
{
    public static LockOnTarget instance { get; private set; }

    [Header("Lock-On Settings")]
    public float lockOnRange = 20f;
    public float maxLockAngle = 60f;
    public float autoReleaseRange = 25f;

    public Transform CurrentTarget { get; private set; }
    public bool IsLockedOn => CurrentTarget != null;

    private Damageable targetDamageable;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    void Update()
    {
        // 마우스 가운데 버튼 클릭 → 토글
        if (Input.GetMouseButtonDown(2))
            Toggle();

        if (IsLockedOn)
            ValidateTarget();
    }

    public void Toggle()
    {
        if (IsLockedOn)
            Release();
        else
            TryLockOn();
    }

    private void TryLockOn()
    {
        Transform best = FindBestTarget();
        if (best == null) return;

        CurrentTarget = best;
        targetDamageable = best.GetComponent<Damageable>();
        if (targetDamageable != null)
            targetDamageable.OnDied += OnTargetDied;
    }

    private Transform FindBestTarget()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, lockOnRange);
        Transform bestTarget = null;
        float bestDist = float.MaxValue;

        Vector3 camForward = Camera.main != null ? Camera.main.transform.forward : transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        foreach (var col in colliders)
        {
            if (col.gameObject == gameObject) continue;

            // Damageable이 있는 대상만 (몬스터 등)
            var dmg = col.GetComponent<Damageable>();
            if (dmg == null || dmg.currentHP <= 0f) continue;

            // 플레이어 자신의 Damageable 제외
            if (col.transform == transform) continue;

            Vector3 dirToTarget = col.transform.position - transform.position;
            dirToTarget.y = 0f;
            float dist = dirToTarget.magnitude;

            // 각도 필터
            float angle = Vector3.Angle(camForward, dirToTarget);
            if (angle > maxLockAngle) continue;

            // 가장 가까운 타겟 선택
            if (dist < bestDist)
            {
                bestDist = dist;
                bestTarget = col.transform;
            }
        }

        return bestTarget;
    }

    private void ValidateTarget()
    {
        if (CurrentTarget == null)
        {
            Release();
            return;
        }

        // 거리 초과 시 해제
        float dist = Vector3.Distance(transform.position, CurrentTarget.position);
        if (dist > autoReleaseRange)
        {
            Release();
            return;
        }

        // 타겟 HP 체크
        if (targetDamageable != null && targetDamageable.currentHP <= 0f)
        {
            Release();
            return;
        }
    }

    private void OnTargetDied()
    {
        Release();
    }

    public bool AutoLockOn()
    {
        if (IsLockedOn) Release();
        TryLockOn();
        return IsLockedOn;
    }

    public void Release()
    {
        if (targetDamageable != null)
            targetDamageable.OnDied -= OnTargetDied;

        CurrentTarget = null;
        targetDamageable = null;
    }

    void OnDestroy()
    {
        if (targetDamageable != null)
            targetDamageable.OnDied -= OnTargetDied;
    }
}
