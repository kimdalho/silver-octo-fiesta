using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Damageable))]
[RequireComponent(typeof(DropTable))]
public class MonsterAI : MonoBehaviour
{
    enum State { Idle, Alert, Flee, Rampage }

    public MonsterData data;

    private State state = State.Idle;
    private CharacterController cc;
    private Damageable damageable;
    private Transform player;

    // Wander (Idle)
    private Vector3 wanderDir;
    private float wanderTimer;

    // Alert
    private float alertTimer;

    // Rampage
    private float rampageTimer;

    // Gravity
    private float verticalVelocity;
    private const float gravity = -20f;

    // 관찰 (외부에서 읽기 가능)
    public float observationProgress { get; private set; }

    // 포획/덫
    public bool isTrapped;
    public bool isCaptured;
    private float trappedTimer;
    private const float trappedDuration = 3f;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        damageable = GetComponent<Damageable>();
    }

    void Start()
    {
        if (data == null) return;

        damageable.maxHP = data.maxHP;
        damageable.currentHP = data.maxHP;

        var dropTable = GetComponent<DropTable>();
        if (dropTable != null)
            dropTable.drops = data.drops;

        if (PlayerStats.instance != null)
            player = PlayerStats.instance.transform;

        PickNewWanderDirection();
    }

    void Update()
    {
        if (data == null || damageable.currentHP <= 0f) return;

        if (player == null && PlayerStats.instance != null)
            player = PlayerStats.instance.transform;

        // 덫 타이머
        if (isTrapped)
        {
            trappedTimer -= Time.deltaTime;
            if (trappedTimer <= 0f)
                isTrapped = false;
        }

        float distToPlayer = player != null
            ? Vector3.Distance(transform.position, player.position)
            : float.MaxValue;

        // 상태 전환
        switch (state)
        {
            case State.Idle:
                if (distToPlayer <= data.detectRange)
                {
                    state = State.Alert;
                    alertTimer = data.alertDuration;
                    CreatureCodex.instance?.RegisterDiscovery(data);
                }
                break;

            case State.Alert:
                if (distToPlayer <= data.fleeRange)
                {
                    state = State.Flee;
                }
                else if (distToPlayer > data.detectRange)
                {
                    state = State.Idle;
                    PickNewWanderDirection();
                }
                else
                {
                    alertTimer -= Time.deltaTime;
                    if (alertTimer <= 0f)
                    {
                        state = State.Idle;
                        PickNewWanderDirection();
                    }
                }
                break;

            case State.Flee:
                if (distToPlayer >= data.safeRange)
                {
                    state = State.Idle;
                    PickNewWanderDirection();
                }
                break;

            case State.Rampage:
                rampageTimer -= Time.deltaTime;
                if (rampageTimer <= 0f)
                {
                    state = State.Idle;
                    PickNewWanderDirection();
                }
                break;
        }

        // 상태 실행
        switch (state)
        {
            case State.Idle:    UpdateIdle();    break;
            case State.Alert:   UpdateAlert();   break;
            case State.Flee:    UpdateFlee();    break;
            case State.Rampage: UpdateRampage(); break;
        }

        // 중력
        if (cc.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;
        verticalVelocity += gravity * Time.deltaTime;
        cc.Move(new Vector3(0f, verticalVelocity * Time.deltaTime, 0f));
    }

    void UpdateIdle()
    {
        if (isTrapped) return;

        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f)
            PickNewWanderDirection();

        cc.Move(wanderDir * data.moveSpeed * 0.5f * Time.deltaTime);
    }

    void UpdateAlert()
    {
        // 정지 상태, 플레이어 방향 주시
        if (player == null) return;

        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(lookDir);
    }

    void UpdateFlee()
    {
        if (player == null || isTrapped) return;

        Vector3 fleeDir = transform.position - player.position;
        fleeDir.y = 0f;
        if (fleeDir.sqrMagnitude > 0.01f)
        {
            fleeDir.Normalize();
            cc.Move(fleeDir * data.fleeSpeed * Time.deltaTime);
        }
    }

    void UpdateRampage()
    {
        if (player == null) return;

        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        float dist = dir.magnitude;

        if (!isTrapped && dist > 1.5f)
        {
            dir.Normalize();
            cc.Move(dir * data.rampageSpeed * Time.deltaTime);
        }
        else if (dist <= 1.5f && PlayerStats.instance != null)
        {
            PlayerStats.instance.TakeDamage(data.rampageAttack * Time.deltaTime);
        }

        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(dir.normalized);
    }

    /// <summary>
    /// 관찰 진행 (PlayerActions에서 매 프레임 호출)
    /// </summary>
    public void Observe(float deltaTime)
    {
        observationProgress = Mathf.Clamp01(observationProgress + deltaTime / data.observeTime);
    }

    /// <summary>
    /// 포획 확률 계산
    /// </summary>
    public float GetCaptureChance()
    {
        float chance = data.baseCaptureChance;
        if (isTrapped) chance += data.trapBonus;
        if (observationProgress >= 1f) chance += data.observeBonus;
        return Mathf.Clamp01(chance);
    }

    /// <summary>
    /// 포획 시도 → true=성공
    /// </summary>
    public bool TryCapture()
    {
        float chance = GetCaptureChance();
        bool success = Random.value <= chance;
        if (success)
        {
            isCaptured = true;
            CreatureCodex.instance?.RegisterCapture(data);
            var dropTable = GetComponent<DropTable>();
            if (dropTable != null) dropTable.SpawnDrops(transform.position);
            Destroy(gameObject);
        }
        else
        {
            Scare();
        }
        return success;
    }

    /// <summary>
    /// 덫에 걸림 (Trap에서 호출)
    /// </summary>
    public void ApplyTrap()
    {
        isTrapped = true;
        trappedTimer = trappedDuration;
        if (state != State.Rampage)
        {
            state = State.Alert;
            alertTimer = trappedDuration;
        }
    }

    /// <summary>
    /// 외부 호출: 플레이어 숨기 등 → 경계 해제 (Alert/Flee → Idle)
    /// </summary>
    public void CalmDown()
    {
        if (state == State.Rampage) return;
        state = State.Idle;
        PickNewWanderDirection();
    }

    /// <summary>
    /// 외부 호출: 돌 던지기 등으로 위협 → 도주 전환
    /// </summary>
    public void Scare()
    {
        if (state == State.Rampage) return;
        state = State.Flee;
    }

    /// <summary>
    /// 외부 호출: 불 등 특정 트리거 → 폭주 진입
    /// </summary>
    public void TriggerRampage()
    {
        state = State.Rampage;
        rampageTimer = data.rampageDuration;
    }

    void PickNewWanderDirection()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        wanderDir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
        wanderTimer = Random.Range(3f, 5f);
    }
}
