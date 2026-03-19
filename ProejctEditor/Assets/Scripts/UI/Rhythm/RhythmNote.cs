using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum HitJudgement { Perfect, Good, Miss }

public class RhythmNote : MonoBehaviour, IPointerClickHandler
{
    public Image hitCircle;
    public Image approachCircle;

    private float approachTime;
    private float perfectWindow;
    private float goodWindow;
    private float missWindow;
    private float elapsedTime;
    private float hitTime;
    private bool isResolved;
    private Action<HitJudgement> onJudged;

    private const float NoteSize = 80f;

    /// <summary>
    /// 노트 GameObject를 코드로 생성하는 팩토리
    /// </summary>
    public static GameObject CreateNote(RectTransform parent)
    {
        // 루트 (클릭 영역)
        GameObject root = new GameObject("RhythmNote", typeof(RectTransform), typeof(RhythmNote));
        root.transform.SetParent(parent, false);
        RectTransform rootRt = root.GetComponent<RectTransform>();
        rootRt.sizeDelta = new Vector2(NoteSize, NoteSize);

        RhythmNote note = root.GetComponent<RhythmNote>();

        // 접근 링 (뒤에 배치, Raycast 끔)
        GameObject approachObj = new GameObject("ApproachCircle", typeof(RectTransform), typeof(Image));
        approachObj.transform.SetParent(root.transform, false);
        RectTransform approachRt = approachObj.GetComponent<RectTransform>();
        approachRt.anchorMin = new Vector2(0.5f, 0.5f);
        approachRt.anchorMax = new Vector2(0.5f, 0.5f);
        approachRt.sizeDelta = new Vector2(NoteSize, NoteSize);
        approachRt.anchoredPosition = Vector2.zero;
        Image approachImg = approachObj.GetComponent<Image>();
        approachImg.color = new Color(0f, 0.8f, 1f, 0.8f);
        approachImg.raycastTarget = false;

        // 히트 서클 (앞에 배치, Raycast 켬 → 클릭 대상)
        GameObject hitObj = new GameObject("HitCircle", typeof(RectTransform), typeof(Image));
        hitObj.transform.SetParent(root.transform, false);
        RectTransform hitRt = hitObj.GetComponent<RectTransform>();
        hitRt.anchorMin = new Vector2(0.5f, 0.5f);
        hitRt.anchorMax = new Vector2(0.5f, 0.5f);
        hitRt.sizeDelta = new Vector2(NoteSize, NoteSize);
        hitRt.anchoredPosition = Vector2.zero;
        Image hitImg = hitObj.GetComponent<Image>();
        hitImg.color = Color.white;
        hitImg.raycastTarget = true;

        note.hitCircle = hitImg;
        note.approachCircle = approachImg;

        return root;
    }

    public void Init(float approachTime, float perfectWindow, float goodWindow, float missWindow, Action<HitJudgement> onJudged)
    {
        this.approachTime = approachTime;
        this.perfectWindow = perfectWindow;
        this.goodWindow = goodWindow;
        this.missWindow = missWindow;
        this.onJudged = onJudged;

        hitTime = approachTime;
        elapsedTime = 0f;
        isResolved = false;

        approachCircle.rectTransform.localScale = Vector3.one * 3f;
    }

    void Update()
    {
        if (isResolved) return;

        elapsedTime += Time.deltaTime;

        // 접근 링 축소: 3 → 1
        float t = Mathf.Clamp01(elapsedTime / hitTime);
        float scale = Mathf.Lerp(3f, 1f, t);
        approachCircle.rectTransform.localScale = Vector3.one * scale;

        // 접근 링 색상 변화 (먼: 청색 → 가까운: 녹색)
        approachCircle.color = Color.Lerp(
            new Color(0f, 0.8f, 1f, 0.8f),
            new Color(0.2f, 1f, 0.2f, 0.9f),
            t
        );

        // 시간 초과 → 자동 Miss
        if (elapsedTime > hitTime + missWindow)
            Resolve(HitJudgement.Miss);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isResolved) return;

        float diff = Mathf.Abs(elapsedTime - hitTime);

        if (diff <= perfectWindow)
            Resolve(HitJudgement.Perfect);
        else if (diff <= goodWindow)
            Resolve(HitJudgement.Good);
        else
            Resolve(HitJudgement.Miss);
    }

    private void Resolve(HitJudgement judgement)
    {
        if (isResolved) return;
        isResolved = true;

        switch (judgement)
        {
            case HitJudgement.Perfect:
                hitCircle.color = new Color(1f, 0.84f, 0f);
                approachCircle.color = new Color(1f, 0.84f, 0f, 0.5f);
                break;
            case HitJudgement.Good:
                hitCircle.color = new Color(0.2f, 1f, 0.2f);
                approachCircle.color = new Color(0.2f, 1f, 0.2f, 0.5f);
                break;
            case HitJudgement.Miss:
                hitCircle.color = new Color(1f, 0.2f, 0.2f);
                approachCircle.color = new Color(1f, 0.2f, 0.2f, 0.5f);
                break;
        }

        onJudged?.Invoke(judgement);
        Destroy(gameObject, 0.5f);
    }
}
