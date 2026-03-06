using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Sprite8DirAnimator : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private Sprite8DirSet spriteSet;

    [Header("Refs")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Rigidbody rb; // 있으면 velocity 기반, 없으면 input 기반

    [Header("Playback")]
    [SerializeField] private float idleFps = 6f;
    [SerializeField] private float walkFps = 10f;
    [SerializeField] private float moveDeadZone = 0.05f;
    [SerializeField] private bool keepLastDirWhenIdle = true;

    private SpriteRenderer sr;
    private AnimState state = AnimState.Idle;
    private int dir = 4; // S
    private int lastDir = 4;

    private float frameTimer;
    private int frameIndex;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (!cameraTransform && Camera.main) cameraTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (!spriteSet) return;

        // 1) 상태 결정 (Idle/Walk)
        Vector3 worldMove = GetMoveVector();
        worldMove.y = 0f;

        bool moving = worldMove.sqrMagnitude >= moveDeadZone * moveDeadZone;
        state = moving ? AnimState.Walk : AnimState.Idle;

        // 2) 방향 결정(8방)
        if (moving || !keepLastDirWhenIdle)
        {
            int newDir = WorldDirTo8(cameraTransform, worldMove);
            if (moving) lastDir = newDir;
            dir = moving ? newDir : (keepLastDirWhenIdle ? lastDir : newDir);
        }
        else
        {
            dir = lastDir;
        }

        // 3) 프레임 재생
        float fps = (state == AnimState.Walk) ? walkFps : idleFps;
        TickFrames(fps);
    }

    Vector3 GetMoveVector()
    {
        // 우선순위: Rigidbody velocity (나중에 넉백/물리도 자연스럽게 반영됨)
        if (rb) return rb.linearVelocity;

        // 프로토타입: input 기반
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (!cameraTransform) return new Vector3(h, 0, v);

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0; right.y = 0;
        forward.Normalize(); right.Normalize();

        return (forward * v + right * h);
    }

    void TickFrames(float fps)
    {
        Sprite[] frames = spriteSet.GetFrames(state, dir);

        if (frames == null || frames.Length == 0)
            return;

        // 상태/방향 바뀌면 프레임 리셋(튀는 걸 줄임)
        // (현재 스프라이트가 frames에 없으면 0부터)
        if (frameIndex >= frames.Length) frameIndex = 0;

        frameTimer += Time.deltaTime;
        float frameTime = 1f / Mathf.Max(1f, fps);

        while (frameTimer >= frameTime)
        {
            frameTimer -= frameTime;
            frameIndex = (frameIndex + 1) % frames.Length;
        }

        sr.sprite = frames[frameIndex];
    }

    static int WorldDirTo8(Transform cam, Vector3 worldDir)
    {
        if (worldDir.sqrMagnitude < 0.0001f) return 4;

        Vector3 v = worldDir;

        // 카메라 기준으로 방향을 잡아 "화면 기준 8방"이 되게
        if (cam)
        {
            v = cam.InverseTransformDirection(worldDir);
            v.y = 0;
        }

        float angle = Mathf.Atan2(v.x, v.z) * Mathf.Rad2Deg; // 0=North
        if (angle < 0) angle += 360f;

        int index = Mathf.FloorToInt((angle + 22.5f) / 45f) % 8; // 0:N ... 7:NW
        return index;
    }
}