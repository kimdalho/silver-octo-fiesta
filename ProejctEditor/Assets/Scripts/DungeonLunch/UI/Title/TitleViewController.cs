using UnityEngine;
using UnityEngine.UI;

public class TitleViewController : MonoBehaviour
{
    [SerializeField] private Button newGameButton;

    public void OnClickNewGame()
    {
        DungeonGameManager.instance.StartGame();
    }
}
