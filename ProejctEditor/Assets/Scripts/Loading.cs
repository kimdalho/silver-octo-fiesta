using UnityEngine;
using UnityEngine.SceneManagement;

public class Loading : MonoBehaviour
{
    void Start()
    {
        string sceneName = DungeonGameManager.instance.PendingScene;
        SceneManager.LoadScene(sceneName);
    }
}
