using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private Button battleBtn;

    private void Start()
    {
        battleBtn.onClick.AddListener(GameLoopManager.instance.LoadBattleField);
    }

}
