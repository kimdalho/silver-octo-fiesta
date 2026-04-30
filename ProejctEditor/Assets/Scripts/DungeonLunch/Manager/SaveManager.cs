using System;
using System.IO;
using UnityEngine;

[Serializable]
public class SaveData
{
    public int gold;
    public int cookingToolTier;
    public SavedMember[] members;
}

[Serializable]
public class SavedMember
{
    public string memberName;
    public int strengthLevel;
    public int constitutionLevel;
    public int magicLevel;
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;
    private string SavePath => Path.Combine(Application.persistentDataPath, "dungeon_save.json");

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Save()
    {
        var data = new SaveData
        {
            gold = DungeonInventoryManager.instance.gold,
            cookingToolTier = (int)CookingManager.instance.unlockedTier
        };

        var members = PartyManager.instance.members;
        data.members = new SavedMember[members.Count];
        for (int i = 0; i < members.Count; i++)
            data.members[i] = new SavedMember
            {
                memberName       = members[i].memberName,
                strengthLevel    = members[i].growth.strengthLevel,
                constitutionLevel = members[i].growth.constitutionLevel,
                magicLevel       = members[i].growth.magicLevel
            };

        File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
    }

    public void Load()
    {
        if (!File.Exists(SavePath)) return;
        var data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));

        DungeonInventoryManager.instance.gold = data.gold;
        CookingManager.instance.unlockedTier = (CookingToolTier)data.cookingToolTier;

        PartyManager.instance.members.Clear();
        foreach (var saved in data.members)
        {
            var m = new PartyMember(saved.memberName);
            m.growth.strengthLevel     = saved.strengthLevel;
            m.growth.constitutionLevel = saved.constitutionLevel;
            m.growth.magicLevel        = saved.magicLevel;
            m.hp = m.MaxHP;
            PartyManager.instance.AddMember(m);
        }
    }

    public bool HasSave() => File.Exists(SavePath);
}
