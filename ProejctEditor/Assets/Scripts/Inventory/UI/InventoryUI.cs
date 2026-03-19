using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public GameObject panel;
    public SlotUI[] slots; // Inspector에서 15개 할당

    private bool isOpen;
    private bool initialized;

    void Start()
    {
        panel.SetActive(false);
        isOpen = false;
        TryInit();
    }

    void TryInit()
    {
        if (initialized) return;
        if (InventoryManager.instance == null) return;

        var inv = InventoryManager.instance.inventory;
        inv.OnSlotChanged += RefreshSlot;

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].slotIndex = i;
            slots[i].isEquipmentSlot = false;
        }
        initialized = true;
    }

    void OnDestroy()
    {
        if (InventoryManager.instance != null)
            InventoryManager.instance.inventory.OnSlotChanged -= RefreshSlot;
    }

    void Update()
    {
        if (!initialized) TryInit();

        if (Input.GetKeyDown(KeyCode.Tab))
            Toggle();
    }

    public void Toggle()
    {
        isOpen = !isOpen;
        panel.SetActive(isOpen);
        if (isOpen)
        {
            RefreshAll();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // ShoulderView면 커서 다시 잠금
            if (CameraFollow.instance != null && CameraFollow.instance.mode == CameraMode.ShoulderView)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    private void RefreshSlot(int index)
    {
        if (index < 0 || index >= slots.Length) return;
        slots[index].SetItem(InventoryManager.instance.inventory.slots[index]);
    }

    private void RefreshAll()
    {
        if (InventoryManager.instance == null) return;
        var inv = InventoryManager.instance.inventory;
        for (int i = 0; i < slots.Length; i++)
            slots[i].SetItem(inv.slots[i]);
    }
}
