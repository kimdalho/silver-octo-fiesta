using System.Collections.Generic;
using UnityEngine;

public class CreatureCodex : MonoBehaviour
{
    public static CreatureCodex instance;

    public enum EntryState { Undiscovered, Discovered, Captured }

    private Dictionary<string, EntryState> entries = new Dictionary<string, EntryState>();

    public int DiscoveredCount { get; private set; }
    public int CapturedCount { get; private set; }

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

    public void RegisterDiscovery(MonsterData data)
    {
        if (data == null) return;
        string key = data.name;

        if (!entries.ContainsKey(key))
        {
            entries[key] = EntryState.Discovered;
            DiscoveredCount++;
            Debug.Log($"[도감] 발견: {key}");
        }
    }

    public void RegisterCapture(MonsterData data)
    {
        if (data == null) return;
        string key = data.name;

        if (!entries.ContainsKey(key))
        {
            entries[key] = EntryState.Captured;
            DiscoveredCount++;
            CapturedCount++;
        }
        else if (entries[key] != EntryState.Captured)
        {
            entries[key] = EntryState.Captured;
            CapturedCount++;
        }

        Debug.Log($"[도감] 포획: {key} (발견 {DiscoveredCount} / 포획 {CapturedCount})");
    }

    public EntryState GetState(MonsterData data)
    {
        if (data == null) return EntryState.Undiscovered;
        EntryState state;
        entries.TryGetValue(data.name, out state);
        return state;
    }
}
