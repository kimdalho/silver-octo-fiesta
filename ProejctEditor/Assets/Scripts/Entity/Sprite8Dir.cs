using UnityEngine;

public class Sprite8Dir : MonoBehaviour
{
    public enum Mode { ByVelocity, ByInput }

    [Header("Refs")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Transform cameraTransform;

    [Header("Direction Sprites (Clockwise from North)")]
    // 0:N, 1:NE, 2:E, 3:SE, 4:S, 5:SW, 6:W, 7:NW
    [SerializeField] private Sprite[] sprites8 = new Sprite[8];

    [Header("Drive")]
    [SerializeField] private Mode mode = Mode.ByVelocity;
    [SerializeField] private Rigidbody rb; // mode=ByVelocity일 때
    [SerializeField] private float deadZone = 0.05f; // 너무 작으면 방향 변경 안함

    [Header("Options")]
    [SerializeField] private bool keepLastWhenIdle = true;

    private int lastDir = 4; // 기본 S

    void Reset()
    {
        sr = GetComponent<SpriteRenderer>();
        cameraTransform = Camera.main ? Camera.main.transform : null;
    }

    void Awake()
    {
        if (!sr) sr = GetComponent<SpriteRenderer>();
        if (!cameraTransform && Camera.main) cameraTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (!sr || sprites8 == null || sprites8.Length < 8) return;

        Vector3 worldDir = GetWorldDirection();
        worldDir.y = 0f;

        if (worldDir.sqrMagnitude < deadZone * deadZone)
        {
            if (!keepLastWhenIdle) SetDir(4); // S 고정 같은 처리
            return;
        }

        int dirIndex = WorldDirTo8(cameraTransform, worldDir);
        SetDir(dirIndex);
    }

    Vector3 GetWorldDirection()
    {
        if (mode == Mode.ByVelocity && rb)
        {
            return rb.linearVelocity;
        }

        // ByInput: InputRaw 사용(프로토타입에 좋음)
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (!cameraTransform) return new Vector3(h, 0, v);

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0; right.y = 0;
        forward.Normalize(); right.Normalize();

        return (forward * v + right * h);
    }

    static int WorldDirTo8(Transform cam, Vector3 worldDir)
    {
        // 카메라 기준 좌표로 변환해서 화면 기준 8방향 결정
        if (!cam) return VectorTo8(worldDir);

        Vector3 local = cam.InverseTransformDirection(worldDir);
        local.y = 0;
        return VectorTo8(local);
    }

    static int VectorTo8(Vector3 v)
    {
        // North(0)부터 시계방향
        // local 기준: z+가 North, x+가 East
        float angle = Mathf.Atan2(v.x, v.z) * Mathf.Rad2Deg; // 0=North
        if (angle < 0) angle += 360f;

        // 360을 8등분(45도). 중앙 정렬을 위해 +22.5
        int index = Mathf.FloorToInt((angle + 22.5f) / 45f) % 8;
        return index;
    }

    void SetDir(int dir)
    {
        if (dir == lastDir) return;
        lastDir = dir;

        Sprite s = sprites8[dir];
        if (s) sr.sprite = s;
    }
}