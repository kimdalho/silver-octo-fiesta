using UnityEngine;

/// <summary>
/// 몬스터 이동 AI.
/// 기본: 스폰 지점 주변을 배회 (wander).
/// 속성 반응 발동 시 MonsterAttributeState.CurrentEffect에 따라 행동 변경:
///   SlowMove  → 이동 속도 감소 (effectValue = 배율)
///   SpeedUp   → 이동 속도 증가 (effectValue = 배율)
///   Flee      → 플레이어 반대 방향으로 도주
///   StopMove  → 정지
///   Stun      → 정지 (duration → MonsterAttributeState가 타이머 관리)
///   Paralyze  → 정지 (duration)
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class MonsterBehavior : MonoBehaviour
{
    // ── 내부 상태 ──────────────────────────────────────────────

    private MonsterData data;
    private MonsterAttributeState attrState;
    private CharacterController cc;

    private Vector3 spawnPos;
    private Vector3 wanderTarget;
    private float wanderTimer;

    private Transform playerTransform;

    private const float Gravity    = -15f;
    private const float TurnSpeed  = 8f;
    private float yVelocity;

    // ── 초기화 ────────────────────────────────────────────────

    void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    void Start()
    {
        attrState = GetComponent<MonsterAttributeState>();
        data      = attrState?.data;

        spawnPos     = transform.position;
        wanderTarget = spawnPos;
        PickWanderTarget();     // 첫 목표 즉시 설정

        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;
    }

    // ── 매 프레임 ─────────────────────────────────────────────

    void Update()
    {
        if (data == null || cc == null || !cc.enabled) return;

        ReactionEffect effect     = attrState?.CurrentEffect     ?? ReactionEffect.None;
        float          effectVal  = attrState?.CurrentEffectValue ?? 1f;

        // 경직·정지 계열 → 중력만 적용
        if (effect == ReactionEffect.Stun    ||
            effect == ReactionEffect.Paralyze ||
            effect == ReactionEffect.StopMove)
        {
            ApplyGravityOnly();
            return;
        }

        // 이동 속도 결정
        float speed = data.moveSpeed;
        switch (effect)
        {
            case ReactionEffect.SlowMove: speed *= effectVal;        break;
            case ReactionEffect.SpeedUp:  speed *= effectVal;        break;
            case ReactionEffect.Flee:     speed  = data.fleeSpeed;   break;
        }

        if (effect == ReactionEffect.Flee)
            MoveFlee(speed);
        else
            MoveWander(speed);
    }

    // ── 배회 ──────────────────────────────────────────────────

    void MoveWander(float speed)
    {
        wanderTimer -= Time.deltaTime;

        float dist = Vector3.Distance(
            new Vector3(transform.position.x, 0f, transform.position.z),
            new Vector3(wanderTarget.x,       0f, wanderTarget.z));

        if (wanderTimer <= 0f || dist < 0.35f)
            PickWanderTarget();

        Vector3 dir = wanderTarget - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.01f)
        {
            dir.Normalize();
            FaceDirection(dir);
            MoveWithGravity(dir * speed);
        }
        else
        {
            ApplyGravityOnly();
        }
    }

    void PickWanderTarget()
    {
        if (data == null) return;

        float interval  = data.wanderInterval;
        wanderTimer = Random.Range(interval * 0.7f, interval * 1.4f);

        Vector2 offset  = Random.insideUnitCircle * data.wanderRadius;
        wanderTarget    = spawnPos + new Vector3(offset.x, 0f, offset.y);
    }

    // ── 도주 ──────────────────────────────────────────────────

    void MoveFlee(float speed)
    {
        Vector3 dir;
        if (playerTransform != null)
        {
            dir = transform.position - playerTransform.position;
            dir.y = 0f;
        }
        else
        {
            dir = transform.forward;
        }

        if (dir.sqrMagnitude < 0.01f) dir = transform.forward;
        dir.Normalize();

        FaceDirection(dir);
        MoveWithGravity(dir * speed);
    }

    // ── 이동 유틸 ─────────────────────────────────────────────

    void MoveWithGravity(Vector3 horizontal)
    {
        if (cc.isGrounded && yVelocity < 0f) yVelocity = -2f;
        yVelocity += Gravity * Time.deltaTime;
        cc.Move((horizontal + Vector3.up * yVelocity) * Time.deltaTime);
    }

    void ApplyGravityOnly()
    {
        if (cc.isGrounded && yVelocity < 0f) yVelocity = -2f;
        yVelocity += Gravity * Time.deltaTime;
        cc.Move(Vector3.up * yVelocity * Time.deltaTime);
    }

    void FaceDirection(Vector3 dir)
    {
        Quaternion target = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, TurnSpeed * Time.deltaTime);
    }
}
