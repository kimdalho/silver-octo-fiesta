using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdventureLogView : MonoBehaviour
{
    public AdventureLogPresenter presenter;

    [SerializeField] private TMP_Text  logText;
    [SerializeField] private ScrollRect scrollRect;

    void Start()
    {
        presenter = new AdventureLogPresenter(this);
        if (BattleManager.instance != null)
            BattleManager.instance.OnBattleLog += presenter.AddLog;
    }

    void OnDestroy()
    {
        if (BattleManager.instance != null)
            BattleManager.instance.OnBattleLog -= presenter.AddLog;
    }

    public void Render(List<string> logs)
    {
        logText.text = string.Join("\n", logs);
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public void Log(string msg) => presenter?.AddLog(msg);
}
