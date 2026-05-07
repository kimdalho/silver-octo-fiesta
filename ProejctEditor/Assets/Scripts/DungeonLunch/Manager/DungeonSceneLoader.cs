using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonSceneLoader : MonoBehaviour
{
    void Start()
    {
        var target = DungeonGameManager.instance?.PendingScene;
        if (!string.IsNullOrEmpty(target))
            SceneManager.LoadScene(target);
        else
            SceneManager.LoadScene(SceneNames.Title);
    }
}
