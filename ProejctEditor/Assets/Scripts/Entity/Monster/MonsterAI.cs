using UnityEngine;

/// <summary>
/// v1.0 AI — v2.0에서 MonsterBehavior로 교체 예정.
/// MonsterData v2.0 개편으로 제거된 필드는 임시 상수로 대체.
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Damageable))]
[RequireComponent(typeof(DropTable))]
public class MonsterAI : MonoBehaviour
{
    enum State { Idle, Alert, Flee, Rampage }

    public MonsterData data;

    // ── v1.0 fallback 상수 (MonsterData에서 제거된 필드) ──
    const float DETECT_RANGE = 15f;
    const float FLEE_RANGE = 7f;
    const float SAFE_RANGE = 25f;
    const float ALERT_DURATION = 5f;
    const float RAMPAGE_SPEED = 6f;
    const float RAMPAGE_DURATION = 8f;
    const float RAMPAGE_ATTACK = 10f;
    const float OBSERVE_TIME = 5f;
    const float BASE_CAPTURE_CHANCE = 0.3f;
    const float TRAP_BONUS = 0.15f;
    const float OBSERVE_BONUS = 0.2f;

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
                if (distToPlayer <= DETECT_RANGE)
                {
                    state = State.Alert;
                    alertTimer = ALERT_DURATION;
                    CreatureCodex.instance?.RegisterEncounter(data);
                }
                break;

            case State.Alert:
                if (distToPlayer <= FLEE_RANGE)
                {
                    state = State.Flee;
                }
                else if (distToPlayer > DETECT_RANGE)
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
                if (distToPlayer >= SAFE_RANGE)
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
            cc.Move(dir * RAMPAGE_SPEED * Time.deltaTime);
        }
        else if (dist <= 1.5f && PlayerStats.instance != null)
        {
            PlayerStats.instance.TakeDamage(RAMPAGE_ATTACK * Time.deltaTime);
        }

        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(dir.normalized);
    }

    public void Observe(float deltaTime)
    {
        observationProgress = Mathf.Clamp01(observationProgress + deltaTime / OBSERVE_TIME);
    }

    public void CompleteObservation()
    {
        observationProgress = 1f;
    }

    public bool IsRampaging => state == State.Rampage;

    public float GetCaptureChance()
    {
        float chance = BASE_CAPTURE_CHANCE;
        if (isTrapped) chance += TRAP_BONUS;
        if (observationProgress >= 1f) chance += OBSERVE_BONUS;
        return Mathf.Clamp01(chance);
    }

    public bool TryCapture()
    {
        float chance = GetCaptureChance();
        bool success = Random.value <= chance;
        if (success)
        {
            isCaptured = true;
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

    public void CalmDown()
    {
        if (state == State.Rampage) return;
        state = State.Idle;
        PickNewWanderDirection();
    }

    public void Scare()
    {
        if (state == State.Rampage) return;
        state = State.Flee;
    }

    public void TriggerRampage()
    {
        state = State.Rampage;
        rampageTimer = RAMPAGE_DURATION;
    }

    void PickNewWanderDirection()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        wanderDir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
        wanderTimer = Random.Range(3f, 5f);
    }
}
