using UnityEngine;

public class InventoryTestHelper : MonoBehaviour
{
    public ItemData[] testItems;

    void Update()
    {
        // F1: testItems 배열의 아이템을 순서대로 1개씩 추가
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (testItems == null || testItems.Length == 0)
            {
                Debug.LogWarning("testItems 배열이 비어있음. Inspector에서 SO 할당 필요.");
                return;
            }

            foreach (var item in testItems)
            {
                if (item != null)
                    InventoryManager.instance.AddItem(item);
            }
            Debug.Log($"아이템 {testItems.Length}개 추가 완료. Tab으로 확인.");
        }

        // F2: 인벤토리 전체 비우기
        if (Input.GetKeyDown(KeyCode.F2))
        {
            var inv = InventoryManager.instance.inventory;
            for (int i = 0; i < Inventory.Size; i++)
                inv.RemoveItem(i);
            Debug.Log("인벤토리 비움.");
        }
    }
}
