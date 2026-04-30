using UnityEngine;

public class InventoryPanelView : MonoBehaviour
{
    public InventoryPanelPresenter presenter;

    [SerializeField] private Transform  slotContainer;
    [SerializeField] private GameObject slotPrefab;

    void Start()
    {
        presenter = new InventoryPanelPresenter(DungeonInventoryManager.instance, this);
    }

    public void Render(DungeonInventoryManager inv)
    {
        foreach (Transform child in slotContainer)
            Destroy(child.gameObject);
        foreach (var slot in inv.Slots)
            Instantiate(slotPrefab, slotContainer).GetComponent<InventorySlotView>().Render(slot);
    }
}
