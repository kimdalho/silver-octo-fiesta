using System;
using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    public static PartyManager instance;
    public const int MaxPartySize = 4;

    public List<PartyMember> members = new List<PartyMember>();
    public bool IsWiped => members.TrueForAll(m => !m.IsAlive);

    public event Action OnPartyChanged;

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        AddMember(new PartyMember("모험가"));
    }

    public bool AddMember(PartyMember member)
    {
        if (members.Count >= MaxPartySize) return false;
        members.Add(member);
        OnPartyChanged?.Invoke();
        return true;
    }

    public void RemoveMember(PartyMember member)
    {
        members.Remove(member);
        OnPartyChanged?.Invoke();
    }

    public void OnFloorDescend()
    {
        foreach (var m in members) m.OnFloorDescend();
    }

    public void OnReturn()
    {
        foreach (var m in members) m.ResetBodyStats();
    }

    public void OnDeath()
    {
        foreach (var m in members) m.ResetBodyStats();
    }

    public void ConsumeLunchbox(FoodData food)
    {
        foreach (var m in members)
            if (m.IsAlive) m.Eat(food);
    }

    // 마을 방문마다 3~4명 랜덤 생성
    public List<PartyMember> GenerateGuildRecruits()
    {
        var list = new List<PartyMember>();
        int count = UnityEngine.Random.Range(3, 5);
        for (int i = 0; i < count; i++)
            list.Add(CreateRandomRecruit());
        return list;
    }

    private static readonly string[] RecruiterNames =
    {
        "아론", "리베카", "토마스", "유나", "케인", "실비아",
        "드레이크", "나오미", "피어스", "에스더", "루카", "한나"
    };

    private PartyMember CreateRandomRecruit()
    {
        var m = new PartyMember(RecruiterNames[UnityEngine.Random.Range(0, RecruiterNames.Length)]);
        m.growth.strengthLevel     = UnityEngine.Random.Range(0, 4);
        m.growth.constitutionLevel = UnityEngine.Random.Range(0, 4);
        m.growth.magicLevel        = UnityEngine.Random.Range(0, 4);
        m.hp = m.MaxHP;
        return m;
    }
}
