using UnityEngine;

public class PlayerActions : MonoBehaviour
{
    [Header("범위")]
    public float actionRange = 10f;

    [Header("덫")]
    public float trapCooldown = 5f;
    public float trapSpawnDistance = 2f;

    [Header("숨기")]
    public float hideDuration = 2f;

    private float lastTrapTime = -999f;
    private float hideTimer;
    private bool isHiding;
    private CharacterController cc;

    // 리듬 미니게임
    private bool isInRhythmGame;
    private MonsterAI rhythmTargetMonster;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        // 리듬 게임 중 → 전체 입력 차단 (ESC만 허용)
        if (isInRhythmGame)
        {
            HandleRhythmGameInput();
            return;
        }

        // 숨기 중 타이머
        if (isHiding)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0f)
            {
                isHiding = false;
                if (cc != null) cc.enabled = true;
            }
            return; // 숨는 동안 다른 행동 불가
        }

        MonsterAI lockedMonster = GetLockedMonsterAI();

        // Q - 관찰 리듬 미니게임 시작
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TryStartObserveRhythm();
        }

        // W - 돌 던지기 (근처 생물 위협)
        if (Input.GetKeyDown(KeyCode.W))
        {
            bool hit = false;
            foreach (var ai in FindMonstersInRange())
            {
                ai.Scare();
                hit = true;
            }
            if (hit) Debug.Log("[돌] 생물을 위협했다!");
        }

        // E - 숨기 (근처 생물 경계 해제 + 이동 불가)
        if (Input.GetKeyDown(KeyCode.E))
        {
            foreach (var ai in FindMonstersInRange())
                ai.CalmDown();

            isHiding = true;
            hideTimer = hideDuration;
            if (cc != null) cc.enabled = false;
            Debug.Log("[숨기] 숨는 중...");
        }

        // R - 덫 설치
        if (Input.GetKeyDown(KeyCode.R) && Time.time >= lastTrapTime + trapCooldown)
        {
            Vector3 spawnPos = transform.position + transform.forward * trapSpawnDistance;
            spawnPos.y = transform.position.y;

            GameObject trapObj = new GameObject("Trap");
            trapObj.transform.position = spawnPos;
            Trap trap = trapObj.AddComponent<Trap>();
            trap.activationRadius = 2f;
            trap.duration = 10f;

            // 시각적 표시용 간이 큐브
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.transform.SetParent(trapObj.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(1f, 0.2f, 1f);
            Renderer rend = visual.GetComponent<Renderer>();
            if (rend != null) rend.material.color = new Color(0.8f, 0.6f, 0.2f, 0.7f);
            // Collider 제거 (Trap은 OverlapSphere로 감지)
            Collider col = visual.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);

            lastTrapTime = Time.time;
            Debug.Log("[덫] 설치 완료!");
        }

        // F - 불 (근처 생물 폭주)
        if (Input.GetKeyDown(KeyCode.F))
        {
            bool hit = false;
            foreach (var ai in FindMonstersInRange())
            {
                ai.TriggerRampage();
                hit = true;
            }
            if (hit) Debug.Log("[불] 생물이 폭주한다!");
        }

        // 마우스 좌클릭 - 포획 시도
        if (Input.GetMouseButtonDown(0) && lockedMonster != null)
        {
            float chance = lockedMonster.GetCaptureChance();
            bool success = lockedMonster.TryCapture();
            if (success)
                Debug.Log($"[포획] 성공! (확률 {chance * 100f:F0}%)");
            else
                Debug.Log($"[포획] 실패... (확률 {chance * 100f:F0}%)");
        }
    }

    void TryStartObserveRhythm()
    {
        if (LockOnTarget.instance == null) return;

        // 자동 락온
        if (!LockOnTarget.instance.AutoLockOn())
        {
            Debug.Log("[관찰] 시야 내 대상 없음");
            return;
        }

        MonsterAI monster = GetLockedMonsterAI();
        if (monster == null) return;

        // 이미 관찰 완료
        if (monster.observationProgress >= 1f)
        {
            Debug.Log("[관찰] 이미 관찰 완료된 대상");
            return;
        }

        // 폭주 중인 몬스터 관찰 불가
        if (monster.IsRampaging)
        {
            Debug.Log("[관찰] 폭주 중인 대상은 관찰 불가");
            return;
        }

        if (ObserveRhythmUI.instance == null)
        {
            Debug.LogWarning("[관찰] ObserveRhythmUI가 씬에 없습니다!");
            return;
        }

        // 리듬 게임 진입 (이동은 유지)
        isInRhythmGame = true;
        rhythmTargetMonster = monster;

        // 커서 해제
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log($"[관찰] 리듬 미니게임 시작 - {monster.data?.name ?? "???"}");
        ObserveRhythmUI.instance.StartRhythm(monster.data, OnRhythmComplete);
    }

    void HandleRhythmGameInput()
    {
        // ESC → 강제 취소
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (ObserveRhythmUI.instance != null)
                ObserveRhythmUI.instance.ForceCancel();
        }
    }

    void OnRhythmComplete(bool success)
    {
        if (success)
        {
            if (rhythmTargetMonster != null)
            {
                rhythmTargetMonster.CompleteObservation();
                Debug.Log("[관찰] 성공! 관찰 완료.");
            }
        }
        else
        {
            if (rhythmTargetMonster != null)
            {
                rhythmTargetMonster.TriggerRampage();
                Debug.Log("[관찰] 실패! 몬스터가 폭주합니다!");
            }
        }

        // 공통 정리
        isInRhythmGame = false;
        rhythmTargetMonster = null;

        // 락온 해제
        if (LockOnTarget.instance != null)
            LockOnTarget.instance.Release();

        // 커서 재잠금 (ShoulderView)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    MonsterAI GetLockedMonsterAI()
    {
        if (LockOnTarget.instance == null || !LockOnTarget.instance.IsLockedOn)
            return null;

        return LockOnTarget.instance.CurrentTarget.GetComponent<MonsterAI>();
    }

    MonsterAI[] FindMonstersInRange()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, actionRange);
        // 최대 32개 정도면 충분
        System.Collections.Generic.List<MonsterAI> result = new System.Collections.Generic.List<MonsterAI>();
        foreach (var col in cols)
        {
            MonsterAI ai = col.GetComponent<MonsterAI>();
            if (ai != null && !ai.isCaptured)
                result.Add(ai);
        }
        return result.ToArray();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, actionRange);
    }
}
