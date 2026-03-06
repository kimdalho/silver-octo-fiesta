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
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "3.Load") return;

        GenerateMap();
        SpawnPlayer(Vector3.zero);
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