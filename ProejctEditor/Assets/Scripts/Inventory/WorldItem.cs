using UnityEngine;

public class WorldItem : MonoBehaviour
{
    public ItemData itemData;
    public int count = 1;

    private bool playerInRange;
    private Transform spriteChild;
    private float bobOffset;

    void Start()
    {
        bobOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        // 둥실둥실 애니메이션
        if (spriteChild != null)
        {
            float y = Mathf.Sin(Time.time * 2f + bobOffset) * 0.15f + 0.5f;
            spriteChild.localPosition = new Vector3(0f, y, 0f);
        }

        // E키로 줍기
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            if (InventoryManager.instance == null) return;

            if (InventoryManager.instance.AddItem(itemData, count))
            {
                Destroy(gameObject);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    /// <summary>
    /// 팩토리 메서드: 코드에서 월드 아이템 생성
    /// </summary>
    public static WorldItem Spawn(ItemData data, Vector3 position, int count = 1)
    {
        var go = new GameObject("WorldItem_" + data.itemName);
        go.transform.position = position;

        // 트리거 콜라이더
        var col = go.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 1.5f;

        // 스프라이트 자식
        var spriteObj = new GameObject("Sprite");
        spriteObj.transform.SetParent(go.transform);
        spriteObj.transform.localPosition = new Vector3(0f, 0.5f, 0f);

        var sr = spriteObj.AddComponent<SpriteRenderer>();
        if (data.icon != null)
            sr.sprite = data.icon;

        // 아이콘 크기 통일 (월드 기준 0.5x0.5)
        float targetSize = 0.5f;
        if (sr.sprite != null)
        {
            float ppu = sr.sprite.pixelsPerUnit;
            float scaleX = targetSize / (sr.sprite.rect.width / ppu);
            float scaleY = targetSize / (sr.sprite.rect.height / ppu);
            spriteObj.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        spriteObj.AddComponent<Billboard>();

        // WorldItem 컴포넌트
        var worldItem = go.AddComponent<WorldItem>();
        worldItem.itemData = data;
        worldItem.count = count;
        worldItem.spriteChild = spriteObj.transform;

        return worldItem;
    }
}
