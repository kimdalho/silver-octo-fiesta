using TMPro;
using UnityEngine;

public class TownViewController : MonoBehaviour
{
    [SerializeField] private GameObject guildPanel;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject kitchenPanel;
    [SerializeField] private TMP_Text   goldText;

    void Start()
    {
        CloseAll();
        RefreshGold();
        DungeonInventoryManager.instance.OnInventoryChanged += RefreshGold;
    }

    void OnDestroy()
    {
        if (DungeonInventoryManager.instance != null)
            DungeonInventoryManager.instance.OnInventoryChanged -= RefreshGold;
    }

    public void OpenGuild()   { CloseAll(); guildPanel.SetActive(true); }
    public void OpenShop()    { CloseAll(); shopPanel.SetActive(true); }
    public void OpenKitchen() { CloseAll(); kitchenPanel.SetActive(true); }
    public void CloseAll()    { guildPanel.SetActive(false); shopPanel.SetActive(false); kitchenPanel.SetActive(false); }

    public void OnClickEnterDungeon()
    {
        SaveManager.instance?.Save();
        DungeonGameManager.instance.EnterDungeon();
    }

    private void RefreshGold() => goldText.text = $"골드: {DungeonInventoryManager.instance.gold}G";
}
