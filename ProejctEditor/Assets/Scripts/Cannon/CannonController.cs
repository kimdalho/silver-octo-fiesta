using UnityEngine;

/// <summary>
/// 총통 발사 컨트롤러. PlayerRoot에 부착.
///
/// 조작:
///   1/2/3 = 물/불/전기 포탄 선택
///   좌클릭 = 발사 (인벤토리에서 포탄 소모)
///
/// 특징:
///   - 한 발이 묵직한 타격감 (카메라 흔들림 + 반동 + 긴 장전)
///   - 포물선 궤적
///   - 인벤토리에 해당 포탄이 없으면 발사 불가
/// </summary>
public class CannonController : MonoBehaviour
{
    public static CannonController instance;

    [Header("발사 설정")]
    public float reloadTime = 1.5f;           // 장전 시간 (묵직한 느낌)
    public float recoilForce = 2f;            // 반동 밀림
    public float launchAngle = 25f;           // 기본 발사 각도 (위로)

    [Header("카메라 흔들림")]
    public float shakeIntensity = 0.2f;
    public float shakeDuration = 0.35f;

    [Header("반동 복귀")]
    public float recoilRecoverySpeed = 5f;

    [Header("현재 상태")]
    [SerializeField] private AttributeType selectedAttribute = AttributeType.Water;
    [SerializeField] private bool isReloading;
    [SerializeField] private float reloadTimer;

    // 속성별 색상
    private static readonly Color WaterColor = new Color(0.29f, 0.62f, 1f);     // #4A9EFF
    private static readonly Color FireColor = new Color(1f, 0.42f, 0.29f);      // #FF6B4A
    private static readonly Color ElectricColor = new Color(1f, 0.85f, 0.24f);  // #FFD93D

    private CharacterController cc;
    private Vector3 recoilVelocity;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }
        instance = this;
        cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        // 포탄 선택 (1/2/3)
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            selectedAttribute = AttributeType.Water;
            Debug.Log("[총통] 물 포탄 선택");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selectedAttribute = AttributeType.Fire;
            Debug.Log("[총통] 불 포탄 선택");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            selectedAttribute = AttributeType.Electric;
            Debug.Log("[총통] 전기 포탄 선택");
        }

        // 장전 타이머
        if (isReloading)
        {
            reloadTimer -= Time.deltaTime;
            if (reloadTimer <= 0f)
            {
                isReloading = false;
                reloadTimer = 0f;
            }
        }

        // 발사 (좌클릭, ShoulderView에서만, 커서 잠긴 상태)
        if (Input.GetMouseButtonDown(0) && CanFire())
        {
            Fire();
        }

        // 반동 복귀
        if (recoilVelocity.sqrMagnitude > 0.001f)
        {
            recoilVelocity = Vector3.Lerp(recoilVelocity, Vector3.zero, recoilRecoverySpeed * Time.deltaTime);
            if (cc != null && cc.enabled)
                cc.Move(recoilVelocity * Time.deltaTime);
        }
    }

    bool CanFire()
    {
        // 장전 중이면 불가
        if (isReloading) return false;

        // ShoulderView (배틀씬)에서만
        if (CameraFollow.instance == null || CameraFollow.instance.mode != CameraMode.ShoulderView)
            return false;

        // 커서 잠긴 상태에서만
        if (Cursor.lockState != CursorLockMode.Locked) return false;

        // 인벤토리에서 해당 포탄 확인
        if (!HasAmmo(selectedAttribute)) return false;

        return true;
    }

    void Fire()
    {
        // 1. 포탄 소모
        AmmoData ammoData = ConsumeAmmo(selectedAttribute);
        if (ammoData == null) return;

        // 2. 발사 방향 계산 (카메라 정면 + 위로 각도)
        Transform cam = CameraFollow.instance.transform;
        Vector3 forward = cam.forward;
        forward.y = 0f;
        forward.Normalize();

        // 포물선: forward + 위로 launchAngle
        Vector3 launchDir = Quaternion.AngleAxis(-launchAngle, cam.right) * cam.forward;
        launchDir.Normalize();

        // 발사 위치 (WeaponHolder의 마운트 포인트 또는 플레이어 정면)
        Vector3 spawnPos = transform.position + forward * 0.5f + Vector3.up * 0.8f;
        if (WeaponHolder.instance != null)
        {
            Transform mount = WeaponHolder.instance.transform.Find("WeaponMount");
            if (mount != null)
                spawnPos = mount.position + mount.forward * 0.3f;
        }

        // 발사 속도
        Vector3 velocity = launchDir * ammoData.launchSpeed;

        // 색상
        Color color = GetAttributeColor(selectedAttribute);
        if (ammoData.projectileColor != Color.white)
            color = ammoData.projectileColor;

        // 3. 발사체 생성
        Projectile.Spawn(spawnPos, velocity, selectedAttribute, ammoData.attributeAmount, color);

        // 4. 타격감 연출
        // 카메라 흔들림
        if (CameraFollow.instance != null)
            CameraFollow.instance.Shake(shakeIntensity, shakeDuration);

        // 반동 (플레이어를 뒤로 밀기)
        recoilVelocity = -forward * recoilForce;

        // 5. 장전 시작
        isReloading = true;
        reloadTimer = reloadTime;

        Debug.Log($"[총통] {selectedAttribute} 포탄 발사! 장전 {reloadTime}초");
    }

    /// <summary>
    /// 인벤토리에서 해당 속성의 포탄이 있는지 확인.
    /// </summary>
    bool HasAmmo(AttributeType attr)
    {
        if (InventoryManager.instance == null) return false;
        var inv = InventoryManager.instance.inventory;

        for (int i = 0; i < Inventory.Size; i++)
        {
            if (inv.slots[i] == null) continue;
            var ammo = inv.slots[i].data as AmmoData;
            if (ammo != null && ammo.attribute == attr)
                return true;
        }
        return false;
    }

    /// <summary>
    /// 인벤토리에서 포탄 1개 소모. 소모된 AmmoData 반환.
    /// </summary>
    AmmoData ConsumeAmmo(AttributeType attr)
    {
        if (InventoryManager.instance == null) return null;
        var inv = InventoryManager.instance.inventory;

        for (int i = 0; i < Inventory.Size; i++)
        {
            if (inv.slots[i] == null) continue;
            var ammo = inv.slots[i].data as AmmoData;
            if (ammo != null && ammo.attribute == attr)
            {
                inv.ConsumeOne(i);
                return ammo;
            }
        }
        return null;
    }

    public static Color GetAttributeColor(AttributeType attr)
    {
        switch (attr)
        {
            case AttributeType.Water: return WaterColor;
            case AttributeType.Fire: return FireColor;
            case AttributeType.Electric: return ElectricColor;
            default: return Color.white;
        }
    }

    // --- 외부 접근용 ---
    public AttributeType SelectedAttribute => selectedAttribute;
    public bool IsReloading => isReloading;
    public float ReloadProgress => isReloading ? 1f - (reloadTimer / reloadTime) : 1f;
}
