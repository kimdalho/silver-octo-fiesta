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

    void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    void Update()
    {
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

        // Q (홀드) - 관찰
        if (Input.GetKey(KeyCode.Q) && lockedMonster != null)
        {
            lockedMonster.Observe(Time.deltaTime);
            float pct = lockedMonster.observationProgress * 100f;
            Debug.Log($"[관찰] {lockedMonster.data?.name ?? "???"} - {pct:F0}%");
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
