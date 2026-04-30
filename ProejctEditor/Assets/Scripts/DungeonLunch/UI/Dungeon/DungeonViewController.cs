using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DungeonViewController : MonoBehaviour
{
    [SerializeField] private TMP_Text   floorText;
    [SerializeField] private TMP_Text   stateText;
    [SerializeField] private GameObject actionButtons;
    [SerializeField] private Button     descendButton;
    [SerializeField] private Button     returnButton;
    [SerializeField] private GameObject battlePanel;
    [SerializeField] private GameObject bossPanel;
    [SerializeField] private AdventureLogView adventureLog;

    void Start()
    {
        DungeonGameManager.instance.OnStateChanged += OnStateChanged;
        ExpiryManager.instance?.StartExpiry();
        RefreshFloor();
    }

    void OnDestroy()
    {
        if (DungeonGameManager.instance != null)
            DungeonGameManager.instance.OnStateChanged -= OnStateChanged;
        ExpiryManager.instance?.StopExpiry();
    }

    private void OnStateChanged(DungeonState state)
    {
        RefreshFloor();
        stateText.text = state.ToString();

        bool isExplore = state == DungeonState.Explore;
        actionButtons.SetActive(isExplore);
        battlePanel.SetActive(state == DungeonState.Battle);
        bossPanel.SetActive(state == DungeonState.Boss);

        adventureLog.Log(GetStateMessage(state));

        if (state == DungeonState.Battle)
            TriggerBattle();
        else if (state == DungeonState.Dead)
            adventureLog.Log("파티가 전멸했습니다...");
    }

    private void RefreshFloor() =>
        floorText.text = $"B{DungeonGameManager.instance.CurrentFloor}F";

    private string GetStateMessage(DungeonState state) => state switch
    {
        DungeonState.Explore => "탐험을 계속한다.",
        DungeonState.Battle  => "몬스터를 발견했다!",
        DungeonState.Gather  => "식물을 채집했다.",
        DungeonState.Water   => "물을 발견했다.",
        DungeonState.Cook    => "취사를 할 수 있다.",
        DungeonState.Portal  => "귀환 포탈을 발견했다.",
        DungeonState.Boss    => "강력한 기운이 느껴진다...",
        DungeonState.Dead    => "전멸...",
        _ => ""
    };

    private void TriggerBattle()
    {
        // 현재 층에 맞는 적은 BattleViewController에서 처리
    }

    public void OnClickDescend() => DungeonGameManager.instance.DescendFloor();
    public void OnClickReturn()  => DungeonGameManager.instance.ReturnToTown();
}
