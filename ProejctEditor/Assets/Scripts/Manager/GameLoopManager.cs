using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameStep
{
    Local,
    SelectBattleField,
    EnterBattleField,
    Battle,
    Return
}

public class GameLoopManager : MonoBehaviour
{
    static public GameLoopManager instance;
    public GameStep CurrentStep;
    public SimpleMapGenerator mapGenerator;
    public Transform player;

    public string TargetScene
    {
        get => targetScene;
        set => targetScene = value;
    }
    public string targetScene;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        if (mapGenerator == null)
            mapGenerator = GetComponentInChildren<SimpleMapGenerator>();

        GenerateMap();
        SpawnPlayer(Vector3.zero);

        // 에디터에서 BattleScene 직접 실행 시 셋업
        if (SceneManager.GetActiveScene().name == "2.BattleScene")
            SetupBattleComponents();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "3.Load") return;

        GenerateMap();
        SpawnPlayer(Vector3.zero);

        // 사망 후 복귀 시 HP 리셋
        if (PlayerStats.instance != null && PlayerStats.instance.isDead)
            PlayerStats.instance.Respawn();

        // 씬별 카메라 모드 전환
        if (CameraFollow.instance != null)
        {
            if (scene.name == "2.BattleScene")
                CameraFollow.instance.SetMode(CameraMode.ShoulderView);
            else
                CameraFollow.instance.SetMode(CameraMode.TopView);
        }

        // 배틀씬 전용 컴포넌트 셋업
        if (scene.name == "2.BattleScene")
            SetupBattleComponents();
        else
            CleanupBattleComponents();
    }

    public void ChangeStep(GameStep step)
    {
        CurrentStep = step;

        switch (step)
        {
            case GameStep.Local:
                EnterLocal();
                break;

            case GameStep.SelectBattleField:
                EnterSelect();
                break;

            case GameStep.EnterBattleField:
                EnterBattleField();
                break;

            case GameStep.Battle:
                StartBattle();
                break;

            case GameStep.Return:
                ReturnToLocal();
                break;
        }
    }

    private void ReturnToLocal()
    {
        LoadLocalField();
    }

    private void StartBattle()
    {
        throw new NotImplementedException();
    }

    private void EnterBattleField()
    {
        throw new NotImplementedException();
    }

    private void EnterSelect()
    {
        throw new NotImplementedException();
    }

    private void EnterLocal()
    {
        GenerateMap();
        SpawnPlayer(Vector3.zero);
    }

    private void Update()
    {
        // 로컬씬에서 C 키 → 맨손 제작 UI 열기/닫기
        if (CurrentStep == GameStep.Local && Input.GetKeyDown(KeyCode.C))
        {
            if (CraftingUI.instance == null) return;

            if (CraftingUI.instance.panel != null && CraftingUI.instance.panel.activeSelf)
                CraftingUI.instance.Close();
            else
                CraftingUI.instance.Open(CraftingStationType.Hand);
        }
    }

    public void SpawnPlayer(Vector3 position)
    {
        if (player != null)
            StartCoroutine(SpawnPlayerRoutine(position));
    }

    private System.Collections.IEnumerator SpawnPlayerRoutine(Vector3 position)
    {
        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.position = position + Vector3.up * 2f;

        // 물리/콜라이더 갱신 대기
        yield return new WaitForFixedUpdate();

        if (cc != null) cc.enabled = true;
    }

    public void GenerateMap()
    {
        if (mapGenerator != null)
            mapGenerator.Generate();
    }

    private void SetupBattleComponents()
    {
        // v2.0: CannonController는 플레이어 프리팹에 직접 부착
    }

    private void CleanupBattleComponents()
    {
        // v2.0: 정리할 동적 컴포넌트 없음
    }

    public void LoadBattleField()
    {
        if (mapGenerator != null)
            mapGenerator.Clear();
        CurrentStep = GameStep.Battle;
        TargetScene = "2.BattleScene";
        SceneManager.LoadScene("3.Load");
    }

    public void LoadLocalField()
    {
        if (mapGenerator != null)
            mapGenerator.Clear();
        CurrentStep = GameStep.Local;
        TargetScene = "1.LocalScene";
        SceneManager.LoadScene("3.Load");
    }


}