using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum DungeonState
{
    Town, Explore, Battle, Gather, Water, Cook, Portal, Boss, Eat, Dead
}

public class DungeonGameManager : MonoBehaviour
{
    public static DungeonGameManager instance;

    public DungeonState CurrentState { get; private set; }
    public int CurrentFloor { get; private set; }
    public string PendingScene { get; private set; }

    private DungeonState _pendingState;

    public event Action<DungeonState> OnStateChanged;

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == SceneNames.Load) return;
        ChangeState(_pendingState);
    }

    public void ChangeState(DungeonState newState)
    {
        CurrentState = newState;
        OnStateChanged?.Invoke(newState);
    }

    private void LoadScene(string sceneName, DungeonState afterState)
    {
        PendingScene  = sceneName;
        _pendingState = afterState;
        SceneManager.LoadScene(SceneNames.Load);
    }

    public void StartGame()    => LoadScene(SceneNames.Town, DungeonState.Town);
    public void EnterDungeon() { CurrentFloor = 1; LoadScene(SceneNames.Dungeon, DungeonState.Explore); }

    public void DescendFloor()
    {
        PartyManager.instance?.OnFloorDescend();
        CurrentFloor++;
        DungeonState next = CurrentFloor % 5 == 0 ? DungeonState.Boss : RollEvent();
        ChangeState(next);
    }

    public void ReturnToTown()
    {
        PartyManager.instance?.OnReturn();
        CurrentFloor = 0;
        LoadScene(SceneNames.Town, DungeonState.Town);
    }

    public void OnPartyWiped()
    {
        PartyManager.instance?.OnDeath();
        DungeonInventoryManager.instance?.HalveInventory();
        CurrentFloor = 0;
        ChangeState(DungeonState.Dead);
    }

    // Battle 30% / Gather 20% / Water 18% / Cook 16% / Portal 16%
    private DungeonState RollEvent()
    {
        float r = UnityEngine.Random.value * 100f;
        if (r < 30f) return DungeonState.Battle;
        if (r < 50f) return DungeonState.Gather;
        if (r < 68f) return DungeonState.Water;
        if (r < 84f) return DungeonState.Cook;
        return DungeonState.Portal;
    }
}
