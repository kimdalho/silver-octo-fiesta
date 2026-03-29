using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// 속성 반응·완성 상태 진입 시 몬스터 머리 위에 팝업 텍스트를 표시.
/// MonsterAttributeState와 같은 오브젝트에 붙인다.
///
/// [Inspector 설정]
/// popupPrefab : TextMeshPro (World Space) 프리팹 — 없으면 동적 생성 사용
/// offsetY     : 텍스트 오프셋 높이 (기본 2.5)
/// </summary>
public class ReactionFeedback : MonoBehaviour
{
    [Header("팝업 설정")]
    public GameObject popupPrefab;   // TMP World-Space 프리팹 (선택)
    public float offsetY = 2.5f;
    public float displayDuration = 1.4f;
    public float riseSpeed = 0.8f;

    private MonsterAttributeState attrState;

    void Start()
    {
        attrState = GetComponent<MonsterAttributeState>();
        if (attrState == null) return;

        attrState.OnReactionTriggered    += r   => ShowPopup(r.reactionName, Color.cyan);
        attrState.OnCompletionTriggered  += c   => ShowPopup(c.completionStateName, new Color(1f, 0.85f, 0.2f));
    }

    void OnDestroy()
    {
        if (attrState == null) return;
        attrState.OnReactionTriggered   -= r => ShowPopup(r.reactionName, Color.cyan);
        attrState.OnCompletionTriggered -= c => ShowPopup(c.completionStateName, new Color(1f, 0.85f, 0.2f));
    }

    // ── 팝업 생성 ──────────────────────────────────────────────

    void ShowPopup(string text, Color color)
    {
        if (string.IsNullOrEmpty(text)) return;
        StartCoroutine(PopupRoutine(text, color));
    }

    IEnumerator PopupRoutine(string text, Color color)
    {
        // 텍스트 오브젝트 생성
        GameObject obj;
        TextMeshPro tmp;

        if (popupPrefab != null)
        {
            obj = Instantiate(popupPrefab, transform.position + Vector3.up * offsetY,
                              Quaternion.identity);
            tmp = obj.GetComponentInChildren<TextMeshPro>();
        }
        else
        {
            obj = new GameObject("ReactionPopup");
            obj.transform.position = transform.position + Vector3.up * offsetY;
            tmp = obj.AddComponent<TextMeshPro>();
            tmp.fontSize     = 3f;
            tmp.alignment    = TextAlignmentOptions.Center;
            tmp.fontStyle    = FontStyles.Bold;
        }

        if (tmp == null) { Destroy(obj); yield break; }

        tmp.text  = text;
        tmp.color = color;

        // 카메라를 향해 항상 정면 표시
        Camera cam = Camera.main;

        float elapsed = 0f;
        Vector3 startPos = obj.transform.position;

        while (elapsed < displayDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / displayDuration;

            // 위로 올라가며 페이드 아웃
            obj.transform.position = startPos + Vector3.up * (riseSpeed * elapsed);

            if (cam != null)
                obj.transform.rotation = Quaternion.LookRotation(
                    obj.transform.position - cam.transform.position);

            Color c = tmp.color;
            c.a = Mathf.Lerp(1f, 0f, t);
            tmp.color = c;

            yield return null;
        }

        Destroy(obj);
    }
}
