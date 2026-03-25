using UnityEngine;

/// <summary>
/// 플레이어에 부착. 무기 장착/해제 시 총통 모델을 플레이어 정면에 표시한다.
/// PlayerRoot에 빈 자식 "WeaponMount"를 만들어 위치를 조정할 수 있다.
/// WeaponMount가 없으면 자동 생성 (플레이어 정면 약간 아래).
/// </summary>
public class WeaponHolder : MonoBehaviour
{
    public static WeaponHolder instance;

    [Header("마운트 설정 (WeaponMount 없을 때 자동 생성 위치)")]
    public Vector3 mountOffset = new Vector3(0.15f, 0.6f, 0.4f);
    public Vector3 mountRotation = new Vector3(0f, 0f, 0f);

    private Transform mountPoint;
    private GameObject currentWeaponModel;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }
        instance = this;

        // WeaponMount 자식 찾기 또는 생성
        mountPoint = transform.Find("WeaponMount");
        if (mountPoint == null)
        {
            var mountObj = new GameObject("WeaponMount");
            mountObj.transform.SetParent(transform);
            mountObj.transform.localPosition = mountOffset;
            mountObj.transform.localRotation = Quaternion.Euler(mountRotation);
            mountPoint = mountObj.transform;
        }
    }

    void Start()
    {
        if (InventoryManager.instance != null)
        {
            InventoryManager.instance.equipment.OnEquipChanged += OnEquipChanged;

            // 시작 시 이미 장착된 무기가 있으면 표시
            var current = InventoryManager.instance.equipment.GetEquip(EquipSlot.Weapon);
            if (current != null)
                ShowWeapon(current);
        }
    }

    void OnDestroy()
    {
        if (InventoryManager.instance != null)
            InventoryManager.instance.equipment.OnEquipChanged -= OnEquipChanged;
    }

    void OnEquipChanged(EquipSlot slot)
    {
        if (slot != EquipSlot.Weapon) return;

        var equipped = InventoryManager.instance.equipment.GetEquip(EquipSlot.Weapon);
        if (equipped != null)
            ShowWeapon(equipped);
        else
            HideWeapon();
    }

    void ShowWeapon(EquipmentData data)
    {
        HideWeapon();

        if (data.modelPrefab == null) return;

        currentWeaponModel = Instantiate(data.modelPrefab, mountPoint);
        currentWeaponModel.transform.localPosition = Vector3.zero;
        currentWeaponModel.transform.localRotation = Quaternion.identity;

        // 프리팹의 콜라이더 비활성화 (플레이어 충돌 방지)
        foreach (var col in currentWeaponModel.GetComponentsInChildren<Collider>())
            col.enabled = false;
    }

    void HideWeapon()
    {
        if (currentWeaponModel != null)
        {
            Destroy(currentWeaponModel);
            currentWeaponModel = null;
        }
    }
}
