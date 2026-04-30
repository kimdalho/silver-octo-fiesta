using UnityEngine;

public class LunchboxPanelView : MonoBehaviour
{
    public LunchboxPanelPresenter presenter;

    [SerializeField] private LunchboxSlotView[] slotViews;

    void Start()
    {
        presenter = new LunchboxPanelPresenter(LunchboxManager.instance, this);
        for (int i = 0; i < slotViews.Length; i++)
            slotViews[i].Init(i, presenter);
    }

    public void Render(LunchboxManager lb)
    {
        for (int i = 0; i < LunchboxManager.SlotCount; i++)
            slotViews[i].Render(lb.slots[i]);
    }
}
