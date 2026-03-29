using UnityEngine;
using TMPro;

/// <summary>
/// 화면 하단에 "[E] 작업대" 같은 상호작용 힌트를 표시하는 싱글톤.
/// Canvas 하위에 배치하고 panel / labelText를 Inspector에서 연결.
/// </summary>
public class InteractHintUI : MonoBehaviour
{
    public static InteractHintUI instance;

    public GameObject panel;
    public TextMeshProUGUI labelText;

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        panel?.SetActive(false);
    }

    public void Show(string label)
    {
        if (labelText != null) labelText.text = label;
        panel?.SetActive(true);
    }

    public void Hide() => panel?.SetActive(false);
}
