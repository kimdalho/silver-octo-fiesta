using UnityEngine;
using UnityEngine.SceneManagement;

public class Loading : MonoBehaviour
{

    private void Start()
    {
        var sceneName = GameLoopManager.instance.targetScene;
        GameLoopManager.instance.targetScene = string.Empty;
        SceneManager.LoadScene(sceneName);
    }
}
