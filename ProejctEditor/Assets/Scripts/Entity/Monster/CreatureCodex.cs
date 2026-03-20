using System.Collections.Generic;
using UnityEngine;

public class CreatureCodex : MonoBehaviour
{
    public static CreatureCodex instance;

    // v2.0: 5단계 도감
    public enum EntryState
    {
        Undiscovered,   // 아직 만나지 않음
        Encountered,    // 최초 조우 (이름 + 실루엣)
        Researching,    // 속성 반응 확인 중 (일부만 공개)
        Analyzed,       // 모든 단일 속성 반응 확인
        Mastered        // 모든 완성 상태 수확 완료
    }

    // 몬스터별 도감 데이터
    [System.Serializable]
    public class CodexEntry
    {
        public EntryState state;
        public HashSet<string> discoveredReactions = new HashSet<string>();   // 확인된 반응 이름
        public HashSet<string> harvestedStates = new HashSet<string>();       // 수확 완료된 완성 상태
    }

    private Dictionary<string, CodexEntry> entries = new Dictionary<string, CodexEntry>();

    public int EncounteredCount { get; private set; }
    public int AnalyzedCount { get; private set; }
    public int MasteredCount { get; private set; }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    CodexEntry GetOrCreateEntry(MonsterData data)
    {
        string key = data.name;
        CodexEntry entry;
        if (!entries.TryGetValue(key, out entry))
        {
            entry = new CodexEntry();
            entries[key] = entry;
        }
        return entry;
    }

    // 최초 조우 (시야에 들어옴)
    public void RegisterEncounter(MonsterData data)
    {
        if (data == null) return;
        var entry = GetOrCreateEntry(data);

        if (entry.state == EntryState.Undiscovered)
        {
            entry.state = EntryState.Encountered;
            EncounteredCount++;
            Debug.Log($"[도감] 조우: {data.name}");
        }
    }

    // 속성 반응 확인 (포탄 명중 시 호출)
    public void RegisterReaction(MonsterData data, string reactionName)
    {
        if (data == null || string.IsNullOrEmpty(reactionName)) return;
        var entry = GetOrCreateEntry(data);

        // 최소 Encountered 이상
        if (entry.state == EntryState.Undiscovered)
        {
            entry.state = EntryState.Encountered;
            EncounteredCount++;
        }

        // 반응 기록
        if (entry.discoveredReactions.Add(reactionName))
        {
            Debug.Log($"[도감] 반응 발견: {data.name} — {reactionName} ({entry.discoveredReactions.Count}/{data.reactions.Length})");

            // Researching 전환
            if (entry.state == EntryState.Encountered)
                entry.state = EntryState.Researching;

            // 모든 반응 확인 → Analyzed
            if (entry.state == EntryState.Researching
                && data.reactions != null
                && entry.discoveredReactions.Count >= data.reactions.Length)
            {
                entry.state = EntryState.Analyzed;
                AnalyzedCount++;
                Debug.Log($"[도감] 분석 완료: {data.name}");
            }
        }
    }

    // 수확 완료 (완성 상태 수확 시 호출)
    public void RegisterHarvest(MonsterData data, string completionStateName)
    {
        if (data == null || string.IsNullOrEmpty(completionStateName)) return;
        var entry = GetOrCreateEntry(data);

        if (entry.harvestedStates.Add(completionStateName))
        {
            Debug.Log($"[도감] 수확: {data.name} — {completionStateName} ({entry.harvestedStates.Count}/{data.combinations.Length})");

            // 모든 완성 상태 수확 → Mastered
            if (data.combinations != null
                && entry.harvestedStates.Count >= data.combinations.Length
                && entry.state != EntryState.Mastered)
            {
                entry.state = EntryState.Mastered;
                MasteredCount++;
                Debug.Log($"[도감] 마스터: {data.name}!");
            }
        }
    }

    public EntryState GetState(MonsterData data)
    {
        if (data == null) return EntryState.Undiscovered;
        CodexEntry entry;
        if (entries.TryGetValue(data.name, out entry))
            return entry.state;
        return EntryState.Undiscovered;
    }

    // 특정 반응이 이미 발견되었는지 확인
    public bool IsReactionDiscovered(MonsterData data, string reactionName)
    {
        if (data == null) return false;
        CodexEntry entry;
        if (entries.TryGetValue(data.name, out entry))
            return entry.discoveredReactions.Contains(reactionName);
        return false;
    }

    // 특정 완성 상태가 수확되었는지 확인
    public bool IsStateHarvested(MonsterData data, string completionStateName)
    {
        if (data == null) return false;
        CodexEntry entry;
        if (entries.TryGetValue(data.name, out entry))
            return entry.harvestedStates.Contains(completionStateName);
        return false;
    }
}
