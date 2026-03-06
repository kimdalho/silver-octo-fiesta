using UnityEngine;
using UnityEngine.UI;

public class DraggedItemUI : MonoBehaviour
{
    public static DraggedItemUI instance;

    public Image iconImage;
    public CanvasGroup canvasGroup;

    [HideInInspector] public SlotUI sourceSlot;

    void Awake()
    {
        instance = this;
        gameObject.SetActive(false);
    }

    public void BeginDrag(ItemStack stack, SlotUI source)
    {
        sourceSlot = source;
        iconImage.sprite = stack.data.icon;
        iconImage.color = Color.white;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(true);
    }

    public void UpdatePosition(Vector2 screenPos)
    {
        transform.position = screenPos;
    }

    public void EndDrag()
    {
        sourceSlot = null;
        gameObject.SetActive(false);
        canvasGroup.blocksRaycasts = true;
    }
}
