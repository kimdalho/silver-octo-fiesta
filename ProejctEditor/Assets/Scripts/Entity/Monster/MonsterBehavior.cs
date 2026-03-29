using UnityEngine;

/// <summary>
/// 몬스터 이동·공격 AI.
///
/// 행동 우선순위:
///   1. Stun / Paralyze / StopMove → 정지
///   2. IsAggressive → 플레이어 추격 + 근접 공격
///   3. fleesFromPlayer AND 플레이어 감지 → 도주
///   4. CurrentEffect = Flee → 도주
///   5. 그 외 → 배회
///
/// IsAggressive는 MushroomRabbitBehavior 같은 특수 컴포넌트가 설정.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class MonsterBehavior : MonoBehaviour
{
    public bool IsAggressive { get; set; }

    private MonsterData data;
    private MonsterAttributeState attrState;
    private CharacterController cc;

    private Vector3 spawnPos;
    private Vector3 wanderTarget;
    private float wanderTimer;

    private Transform playerTransform;
    private float attackTimer;

    private const float Gravity   = -15f;
    private const float TurnSpeed = 8f;
    private float yVelocity;

    void Awake() => cc = GetComponent<CharacterController>();

    void Start()
    {
        attrState = GetComponent<MonsterAttributeState>();
        data      = attrState?.data;

        spawnPos = transform.position;
        PickWanderTarget();

        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;
    }

    void Update()
    {
        if (data == null || cc == null || !cc.enabled) return;

        ReactionEffect effect    = attrState?.CurrentEffect     ?? ReactionEffect.None;
        float          effectVal = attrState?.CurrentEffectValue ?? 1f;

        // 1. 경직 / 정지
        if (effect == ReactionEffect.Stun    ||
            effect == ReactionEffect.Paralyze ||
            effect == ReactionEffect.StopMove)
        {
            ApplyGravityOnly();
            return;
        }

        // 공격 (공격 범위 내)
        if (IsAggressive && playerTransform != null)
        {
            attackTimer -= Time.deltaTime;
            float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distToPlayer <= data.attackRange && attackTimer <= 0f)
            {
                attackTimer = data.attackCooldown;
                PlayerStats.instance?.TakeDamage(data.attackDamage);
            }
        }

        // 속도 결정
        float speed = data.moveSpeed;
        if      (effect == ReactionEffect.SlowMove) speed *= effectVal;
        else if (effect == ReactionEffect.SpeedUp)  speed *= effectVal;

        // 2. 추격
        if (IsAggressive)
        {
            MoveChase(speed * 1.2f);
            return;
        }

        // 3/4. 도주
        bool playerNear = playerTransform != null
            && Vector3.Distance(transform.position, playerTransform.position) <= data.detectionRange;

        if ((data.fleesFromPlayer && playerNear) || effect == ReactionEffect.Flee)
        {
            MoveFlee(data.fleeSpeed);
            return;
        }

        // 5. 배회
        MoveWander(speed);
    }

    // ── 이동 ──────────────────────────────────────────────────

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
        wanderTimer = Random.Range(data.wanderInterval * 0.7f, data.wanderInterval * 1.4f);
        Vector2 offset = Random.insideUnitCircle * data.wanderRadius;
        wanderTarget   = spawnPos + new Vector3(offset.x, 0f, offset.y);
    }

    void MoveFlee(float speed)
    {
        Vector3 dir = playerTransform != null
            ? transform.position - playerTransform.position
            : transform.forward;

        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) dir = transform.forward;
        dir.Normalize();

        FaceDirection(dir);
        MoveWithGravity(dir * speed);
    }

    void MoveChase(float speed)
    {
        if (playerTransform == null) { ApplyGravityOnly(); return; }

        Vector3 dir = playerTransform.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) { ApplyGravityOnly(); return; }

        dir.Normalize();
        FaceDirection(dir);
        MoveWithGravity(dir * speed);
    }

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
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(dir),
            TurnSpeed * Time.deltaTime);
    }
}
