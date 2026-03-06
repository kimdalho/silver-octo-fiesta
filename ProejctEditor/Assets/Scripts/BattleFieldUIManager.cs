using UnityEngine;
using UnityEngine.UI;

public class BattleFieldUIManager : MonoBehaviour
{
    public Button localfieldBtn;

    private void Start()
    {
        localfieldBtn.onClick.AddListener(GameLoopManager.instance.LoadLocalField);
    }
}
