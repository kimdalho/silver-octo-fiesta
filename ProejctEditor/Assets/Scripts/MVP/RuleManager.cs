using UnityEngine;

public class RuleManager : MonoBehaviour
{
    // ====== State (MVP) ======
    // 슬라임 생태계(종) 상태
    [Range(-100, 100)] public int SlimeDisposition = 0; // -100~100
    public int SlimeTempleCount = 0;                    // 신전 개수
    public int SlimeHatcheryCount = 0;                  // 부화장 개수
    public bool HumanSlimeExists = false;               // 단 1명 제한 플래그

    // 세계 축(슬라임 세계든, 별도든 MVP에선 여기로 합침)
    [Range(-100, 100)] public int PurityAxis = 0;       // 정화(+)/부패(-)
    [Range(-100, 100)] public int VitalAxis = 0;        // 생명(+)/파괴(-)

    // ====== Hidden Rule thresholds ======
    public int NeedDisposition = 80;
    public int NeedPurity = 80;
    public int NeedVital = 80;
    public int NeedTempleExactly = 1;
    public int NeedHatcheryAtLeast = 4;

    [Tooltip("0.01% = 0.0001")]
    public float HumanSlimeSpawnChance = 0.0001f; // 0.01%

    private int _spawnCheckCount = 0;

    private void Start()
    {
        LogState("START");
        LogControls();
    }

    private void Update()
    {
        // ====== Inputs (MVP) ======
        // K: 슬라임 죽이기
        if (Input.GetKeyDown(KeyCode.K))
        {
            SlimeDisposition = ClampAxis(SlimeDisposition - 1);
            LogState("EVENT: Kill Slime (Disposition -1)");
            EvaluateHiddenRuleAndTrySpawn("KillSlime");
        }

        // F: 슬라임에게 먹이 주기
        if (Input.GetKeyDown(KeyCode.F))
        {
            SlimeDisposition = ClampAxis(SlimeDisposition + 1);
            LogState("EVENT: Feed Slime (Disposition +1)");
            EvaluateHiddenRuleAndTrySpawn("FeedSlime");
        }

        // P: 정화 올리기(임시)
        if (Input.GetKeyDown(KeyCode.P))
        {
            PurityAxis = ClampAxis(PurityAxis + 5);
            LogState("EVENT: Purity +5");
            EvaluateHiddenRuleAndTrySpawn("PurityUp");
        }

        // O: 생명 올리기(임시)
        if (Input.GetKeyDown(KeyCode.O))
        {
            VitalAxis = ClampAxis(VitalAxis + 5);
            LogState("EVENT: Vital +5");
            EvaluateHiddenRuleAndTrySpawn("VitalUp");
        }

        // T: 신전 설치(토글이 아니라 증가) / Shift+T: 신전 제거
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                SlimeTempleCount = Mathf.Max(0, SlimeTempleCount - 1);
            else
                SlimeTempleCount += 1;

            LogState("EVENT: Temple Count Changed");
            EvaluateHiddenRuleAndTrySpawn("TempleChanged");
        }

        // H: 부화장 설치 / Shift+H: 부화장 제거
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                SlimeHatcheryCount = Mathf.Max(0, SlimeHatcheryCount - 1);
            else
                SlimeHatcheryCount += 1;

            LogState("EVENT: Hatchery Count Changed");
            EvaluateHiddenRuleAndTrySpawn("HatcheryChanged");
        }

        // S: "슬라임 스폰 체크" 1회(확률 굴리는 트리거)
        if (Input.GetKeyDown(KeyCode.S))
        {
            LogState("EVENT: Spawn Check Trigger (S)");
            EvaluateHiddenRuleAndTrySpawn("SpawnCheck");
        }

        // R: 리셋
        if (Input.GetKeyDown(KeyCode.R))
        {
            SlimeDisposition = 0;
            PurityAxis = 0;
            VitalAxis = 0;
            SlimeTempleCount = 0;
            SlimeHatcheryCount = 0;
            HumanSlimeExists = false;
            _spawnCheckCount = 0;
            LogState("RESET");
        }
    }

    // ====== Core loop evaluator ======
    private void EvaluateHiddenRuleAndTrySpawn(string reason)
    {
        bool canAttempt = CanAttemptHumanSlimeSpawn();

        Debug.Log(
            $"[HiddenRule] reason={reason} canAttempt={canAttempt} " +
            $"(D{SlimeDisposition} P{PurityAxis} V{VitalAxis} T{SlimeTempleCount} H{SlimeHatcheryCount} Exists={HumanSlimeExists})"
        );

        if (!canAttempt) return;

        _spawnCheckCount++;
        float roll = Random.value; // 0~1
        bool success = roll < HumanSlimeSpawnChance;

        Debug.Log($"[SpawnRoll #{_spawnCheckCount}] roll={roll:0.000000} < chance={HumanSlimeSpawnChance} => {success}");

        if (success)
        {
            HumanSlimeExists = true;
            Debug.Log("[SPAWN] Human-form Slime Girl Spawned! (Unique) HumanSlimeExists=true");
        }
    }

    private bool CanAttemptHumanSlimeSpawn()
    {
        if (HumanSlimeExists) return false;
        if (SlimeDisposition < NeedDisposition) return false;
        if (PurityAxis < NeedPurity) return false;
        if (VitalAxis < NeedVital) return false;
        if (SlimeTempleCount != NeedTempleExactly) return false;
        if (SlimeHatcheryCount < NeedHatcheryAtLeast) return false;
        return true;
    }

    private static int ClampAxis(int v) => Mathf.Clamp(v, -100, 100);

    private void LogState(string tag)
    {
        Debug.Log(
            $"[{tag}] " +
            $"Disposition={SlimeDisposition}, Purity={PurityAxis}, Vital={VitalAxis}, " +
            $"Temple={SlimeTempleCount}, Hatchery={SlimeHatcheryCount}, HumanExists={HumanSlimeExists}"
        );
    }

    private void LogControls()
    {
        Debug.Log(
            "[Controls] K=KillSlime(-1) | F=FeedSlime(+1) | P=Purity+5 | O=Vital+5 | " +
            "T=Temple+1 (Shift+T -1) | H=Hatchery+1 (Shift+H -1) | S=SpawnCheck | R=Reset"
        );
    }
}