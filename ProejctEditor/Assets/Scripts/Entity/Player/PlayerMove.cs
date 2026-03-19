using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMoveCC : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float walkMultiplier = 0.4f;
    public float gravity = -20f;
    public float turnSpeed = 10f;
    public Transform cameraTransform;

    private CharacterController cc;
    private Vector3 velocity; // y축 중력 누적

    private static PlayerMoveCC instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        cc = GetComponent<CharacterController>();
        if (!cameraTransform) cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        if (!cc.enabled) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 moveDir;
        bool lockedOn = LockOnTarget.instance != null && LockOnTarget.instance.IsLockedOn;

        if (lockedOn)
        {
            // 락온 중: 타겟 기준 스트레이프 이동
            Vector3 dirToTarget = LockOnTarget.instance.CurrentTarget.position - transform.position;
            dirToTarget.y = 0f;
            Vector3 forward = dirToTarget.normalized;
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

            // W/S = 타겟으로 접근/후퇴, A/D = 좌우 스트레이프
            moveDir = forward * v + right * h;
            if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

            // 항상 타겟을 바라봄
            if (forward.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(forward);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
            }
        }
        else
        {
            // 카메라 기준 방향
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            moveDir = (forward * v + right * h);
            if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

            // ShoulderView: 이동 방향으로 캐릭터 회전
            if (CameraFollow.instance != null
                && CameraFollow.instance.mode == CameraMode.ShoulderView
                && moveDir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
            }
        }

        // 수평 이동 (Shift: 천천히 걷기)
        float speed = Input.GetKey(KeyCode.LeftShift) ? moveSpeed * walkMultiplier : moveSpeed;
        cc.Move(moveDir * speed * Time.deltaTime);

        // 중력(바닥 붙이기)
        if (cc.isGrounded && velocity.y < 0)
            velocity.y = -2f; // 바닥에 붙게 하는 트릭

        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);
    }
}