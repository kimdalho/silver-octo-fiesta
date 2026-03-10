using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Damageable))]
[RequireComponent(typeof(DropTable))]
public class MonsterAI : MonoBehaviour
{
    enum State { Wander, Chase, Attack }

    public MonsterData data;

    private State state = State.Wander;
    private CharacterController cc;
    private Damageable damageable;
    private Transform player;

    // Wander
    private Vector3 wanderDir;
    private float wanderTimer;

    // Attack
    private float attackTimer;

    // Gravity
    private float verticalVelocity;
    private const float gravity = -20f;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        damageable = GetComponent<Damageable>();
    }

    void Start()
    {
        if (data == null) return;

        // Damageable 초기화
        damageable.maxHP = data.maxHP;
        damageable.currentHP = data.maxHP;

        // DropTable 초기화
        var dropTable = GetComponent<DropTable>();
        if (dropTable != null)
            dropTable.drops = data.drops;

        // 사망 시 처리 (Damageable.Die()에서 이미 Destroy하므로 추가 처리 불필요)
        // OnDied는 Destroy 직전에 호출됨

        // 플레이어 찾기
        if (PlayerStats.instance != null)
            player = PlayerStats.instance.transform;

        PickNewWanderDirection();
    }

    void Update()
    {
        if (data == null || damageable.currentHP <= 0f) return;

        // 플레이어 참조 갱신 (늦게 스폰될 수 있으므로)
        if (player == null && PlayerStats.instance != null)
            player = PlayerStats.instance.transform;

        float distToPlayer = player != null
            ? Vector3.Distance(transform.position, player.position)
            : float.MaxValue;

        // 상태 전환
        switch (state)
        {
            case State.Wander:
                if (distToPlayer <= data.detectRange)
                    state = State.Chase;
                break;

            case State.Chase:
                if (distToPlayer > data.detectRange)
                    state = State.Wander;
                else if (distToPlayer <= data.attackRange)
                    state = State.Attack;
                break;

            case State.Attack:
                if (distToPlayer > data.attackRange)
                {
                    state = distToPlayer <= data.detectRange ? State.Chase : State.Wander;
                }
                break;
        }

        // 상태 실행
        switch (state)
        {
            case State.Wander: UpdateWander(); break;
            case State.Chase:  UpdateChase();  break;
            case State.Attack: UpdateAttack(); break;
        }

        // 중력
        if (cc.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;
        verticalVelocity += gravity * Time.deltaTime;
        cc.Move(new Vector3(0f, verticalVelocity * Time.deltaTime, 0f));
    }

    void UpdateWander()
    {
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f)
            PickNewWanderDirection();

        cc.Move(wanderDir * data.moveSpeed * 0.5f * Time.deltaTime);
    }

    void UpdateChase()
    {
        if (player == null) return;

        Vector3 dir = (player.position - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.01f)
        {
            dir.Normalize();
            cc.Move(dir * data.moveSpeed * Time.deltaTime);
        }
    }

    void UpdateAttack()
    {
        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            attackTimer = data.attackCooldown;
            if (PlayerStats.instance != null)
                PlayerStats.instance.TakeDamage(data.attack);
        }
    }

    void PickNewWanderDirection()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        wanderDir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
        wanderTimer = Random.Range(3f, 5f);
    }
}
