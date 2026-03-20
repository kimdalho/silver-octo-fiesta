using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// v1.0 리듬 미니게임 — v2.0에서 제거 예정.
/// MonsterData v2.0 개편으로 제거된 리듬 필드는 임시 상수로 대체.
/// </summary>
public class ObserveRhythmUI : MonoBehaviour
{
    public static ObserveRhythmUI instance { get; private set; }

    // ── v1.0 fallback 상수 (MonsterData에서 제거된 리듬 필드) ──
    const int RHYTHM_NOTE_COUNT = 8;
    const int RHYTHM_MISS_LIMIT = 3;
    const float RHYTHM_NOTE_INTERVAL = 1.2f;
    const float RHYTHM_APPROACH_TIME = 1.5f;
    const float RHYTHM_PERFECT_WINDOW = 0.15f;
    const float RHYTHM_GOOD_WINDOW = 0.3f;
    const float RHYTHM_MISS_WINDOW = 0.5f;

    // 동적 생성 UI
    private Canvas canvas;
    private GameObject panel;
    private RectTransform noteContainer;
    private Text comboText;
    private Text progressText;
    private Text missCountText;
    private Image progressBarFill;
    private GameObject resultPanel;
    private Text resultText;

    private MonsterData currentData;
    private Action<bool> onComplete;

    private int totalNotes;
    private int resolvedNotes;
    private int combo;
    private int maxCombo;
    private int missCount;
    private int missLimit;
    private bool isActive;

    private Coroutine spawnRoutine;
    private List<RhythmNote> activeNotes = new List<RhythmNote>();

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        BuildUI();
        panel.SetActive(false);
    }

    #region UI 동적 생성

    private void BuildUI()
    {
        // Canvas
        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        gameObject.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        gameObject.AddComponent<GraphicRaycaster>();

        // Panel (전체 반투명 배경)
        panel = CreatePanel("RhythmPanel", transform, Vector2.zero, Vector2.one, new Color(0, 0, 0, 0.5f));

        // 노트 컨테이너 (화면 중앙, 큰 영역)
        GameObject noteContainerObj = CreatePanel("NoteContainer", panel.transform, Vector2.zero, Vector2.one, new Color(0, 0, 0, 0));
        noteContainer = noteContainerObj.GetComponent<RectTransform>();
        noteContainer.anchorMin = new Vector2(0.1f, 0.15f);
        noteContainer.anchorMax = new Vector2(0.9f, 0.85f);
        noteContainer.offsetMin = Vector2.zero;
        noteContainer.offsetMax = Vector2.zero;

        // 상단 HUD 바
        BuildHUD();

        // 결과 패널
        BuildResultPanel();
    }

    private void BuildHUD()
    {
        // 진행률 바 배경 (상단)
        GameObject barBg = CreatePanel("ProgressBarBg", panel.transform, Vector2.zero, Vector2.zero, new Color(0.2f, 0.2f, 0.2f, 0.8f));
        RectTransform barBgRt = barBg.GetComponent<RectTransform>();
        barBgRt.anchorMin = new Vector2(0.2f, 0.9f);
        barBgRt.anchorMax = new Vector2(0.8f, 0.93f);
        barBgRt.offsetMin = Vector2.zero;
        barBgRt.offsetMax = Vector2.zero;

        // 진행률 바 채움
        GameObject barFill = CreatePanel("ProgressBarFill", barBg.transform, Vector2.zero, Vector2.zero, new Color(0f, 0.8f, 1f, 1f));
        progressBarFill = barFill.GetComponent<Image>();
        progressBarFill.type = Image.Type.Filled;
        progressBarFill.fillMethod = Image.FillMethod.Horizontal;
        progressBarFill.fillAmount = 0f;
        RectTransform fillRt = barFill.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;

        // 진행률 텍스트 (바 위)
        progressText = CreateText("ProgressText", barBg.transform, "0 / 0", 18, TextAnchor.MiddleCenter);
        RectTransform ptRt = progressText.GetComponent<RectTransform>();
        ptRt.anchorMin = Vector2.zero;
        ptRt.anchorMax = Vector2.one;
        ptRt.offsetMin = Vector2.zero;
        ptRt.offsetMax = Vector2.zero;

        // 콤보 텍스트 (화면 중앙 상단)
        comboText = CreateText("ComboText", panel.transform, "", 48, TextAnchor.MiddleCenter);
        RectTransform comboRt = comboText.GetComponent<RectTransform>();
        comboRt.anchorMin = new Vector2(0.3f, 0.7f);
        comboRt.anchorMax = new Vector2(0.7f, 0.85f);
        comboRt.offsetMin = Vector2.zero;
        comboRt.offsetMax = Vector2.zero;
        comboText.color = new Color(1f, 0.84f, 0f);

        // 미스 카운트 (우상단)
        missCountText = CreateText("MissCountText", panel.transform, "MISS: 0 / 3", 22, TextAnchor.MiddleRight);
        RectTransform missRt = missCountText.GetComponent<RectTransform>();
        missRt.anchorMin = new Vector2(0.6f, 0.93f);
        missRt.anchorMax = new Vector2(0.95f, 0.98f);
        missRt.offsetMin = Vector2.zero;
        missRt.offsetMax = Vector2.zero;
        missCountText.color = new Color(1f, 0.4f, 0.4f);
    }

    private void BuildResultPanel()
    {
        resultPanel = CreatePanel("ResultPanel", panel.transform, Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0.7f));
        RectTransform rpRt = resultPanel.GetComponent<RectTransform>();
        rpRt.anchorMin = new Vector2(0.25f, 0.3f);
        rpRt.anchorMax = new Vector2(0.75f, 0.7f);
        rpRt.offsetMin = Vector2.zero;
        rpRt.offsetMax = Vector2.zero;

        resultText = CreateText("ResultText", resultPanel.transform, "", 42, TextAnchor.MiddleCenter);
        RectTransform rtRt = resultText.GetComponent<RectTransform>();
        rtRt.anchorMin = Vector2.zero;
        rtRt.anchorMax = Vector2.one;
        rtRt.offsetMin = Vector2.zero;
        rtRt.offsetMax = Vector2.zero;

        resultPanel.SetActive(false);
    }

    private GameObject CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        Image img = obj.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return obj;
    }

    private Text CreateText(string name, Transform parent, string content, int fontSize, TextAnchor alignment)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Text));
        obj.transform.SetParent(parent, false);
        Text txt = obj.GetComponent<Text>();
        txt.text = content;
        txt.fontSize = fontSize;
        txt.alignment = alignment;
        txt.color = Color.white;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.raycastTarget = false;
        return txt;
    }

    #endregion

    #region 게임 로직

    public void StartRhythm(MonsterData data, Action<bool> onComplete)
    {
        currentData = data;
        this.onComplete = onComplete;

        totalNotes = RHYTHM_NOTE_COUNT;
        resolvedNotes = 0;
        combo = 0;
        maxCombo = 0;
        missCount = 0;
        missLimit = RHYTHM_MISS_LIMIT;
        isActive = true;

        activeNotes.Clear();

        panel.SetActive(true);
        resultPanel.SetActive(false);
        UpdateHUD();

        spawnRoutine = StartCoroutine(SpawnNotesRoutine());
    }

    private IEnumerator SpawnNotesRoutine()
    {
        for (int i = 0; i < totalNotes; i++)
        {
            if (!isActive) yield break;

            SpawnNote();
            yield return new WaitForSeconds(RHYTHM_NOTE_INTERVAL);
        }
    }

    private void SpawnNote()
    {
        GameObject obj = RhythmNote.CreateNote(noteContainer);
        RectTransform rt = obj.GetComponent<RectTransform>();

        // noteContainer 내 랜덤 위치
        float halfW = noteContainer.rect.width * 0.35f;
        float halfH = noteContainer.rect.height * 0.35f;
        float x = UnityEngine.Random.Range(-halfW, halfW);
        float y = UnityEngine.Random.Range(-halfH, halfH);
        rt.anchoredPosition = new Vector2(x, y);

        RhythmNote note = obj.GetComponent<RhythmNote>();
        note.Init(
            RHYTHM_APPROACH_TIME,
            RHYTHM_PERFECT_WINDOW,
            RHYTHM_GOOD_WINDOW,
            RHYTHM_MISS_WINDOW,
            OnNoteJudged
        );
        activeNotes.Add(note);
    }

    private void OnNoteJudged(HitJudgement judgement)
    {
        if (!isActive) return;

        resolvedNotes++;

        if (judgement == HitJudgement.Miss)
        {
            missCount++;
            combo = 0;

            if (missCount > missLimit)
            {
                EndRhythm(false);
                return;
            }
        }
        else
        {
            combo++;
            if (combo > maxCombo) maxCombo = combo;
        }

        UpdateHUD();

        if (resolvedNotes >= totalNotes)
            EndRhythm(true);
    }

    private void UpdateHUD()
    {
        comboText.text = combo > 1 ? $"{combo} COMBO" : "";
        progressText.text = $"{resolvedNotes} / {totalNotes}";
        missCountText.text = $"MISS: {missCount} / {missLimit}";
        progressBarFill.fillAmount = totalNotes > 0 ? (float)resolvedNotes / totalNotes : 0f;
    }

    private void EndRhythm(bool success)
    {
        isActive = false;

        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        foreach (var note in activeNotes)
        {
            if (note != null)
                Destroy(note.gameObject);
        }
        activeNotes.Clear();

        StartCoroutine(ShowResultAndFinish(success));
    }

    private IEnumerator ShowResultAndFinish(bool success)
    {
        resultPanel.SetActive(true);

        if (success)
        {
            resultText.text = $"관찰 성공!\nMax Combo: {maxCombo}";
            resultText.color = new Color(0.2f, 1f, 0.2f);
        }
        else
        {
            resultText.text = "관찰 실패...";
            resultText.color = new Color(1f, 0.3f, 0.3f);
        }

        yield return new WaitForSeconds(1.5f);

        panel.SetActive(false);
        resultPanel.SetActive(false);

        onComplete?.Invoke(success);
        onComplete = null;
        currentData = null;
    }

    public void ForceCancel()
    {
        if (!isActive) return;
        EndRhythm(false);
    }

    #endregion
}
