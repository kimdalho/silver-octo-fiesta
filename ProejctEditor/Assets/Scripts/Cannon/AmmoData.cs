using UnityEngine;

/// <summary>
/// 포탄 아이템 데이터. ItemData를 상속하여 인벤토리에 들어간다.
/// 속성 타입과 부여량을 가진다.
/// </summary>
[CreateAssetMenu(fileName = "NewAmmo", menuName = "Inventory/Ammo")]
public class AmmoData : ItemData
{
    [Header("포탄 속성")]
    public AttributeType attribute;

    [Tooltip("명중 시 대상에 부여하는 속성량")]
    public float attributeAmount = 10f;

    [Tooltip("발사 속도 (m/s)")]
    public float launchSpeed = 15f;

    [Tooltip("포탄 색상")]
    public Color projectileColor = Color.white;

    private void OnValidate()
    {
        itemType = ItemType.Consumable;
    }
}
