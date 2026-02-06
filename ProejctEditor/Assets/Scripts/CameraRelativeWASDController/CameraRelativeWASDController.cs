using UnityEngine;
using UnityEngine.InputSystem;

public class CameraRelativeWASDController_InputSystem : MonoBehaviour
{
    [Header("Refs")]
    public Transform mainCamera;   // 비우면 Camera.main 사용
    public Rigidbody rb;           // 있으면 할당

    [Header("Move")]
    public float moveSpeed = 5f;
    public bool useRigidbodyMove = true;

    [Header("Rotate")]
    public bool rotateToMoveDir = true;
    public float rotateSpeed = 720f; // deg/sec

    // Input System
    private InputAction moveAction;

    void Awake()
    {
        if (mainCamera == null && Camera.main != null)
            mainCamera = Camera.main.transform;

        if (rb == null) rb = GetComponent<Rigidbody>();
        if (rb != null) rb.freezeRotation = true;

        // 코드로 최소 입력 세팅 (프로젝트에 InputAction 에셋 없어도 됨)
        moveAction = new InputAction("Move", InputActionType.Value);

        // WASD
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        // Arrow keys (옵션)
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");

        // Gamepad left stick (옵션)
        moveAction.AddBinding("<Gamepad>/leftStick");
    }

    void OnEnable() => moveAction.Enable();
    void OnDisable() => moveAction.Disable();

    void Update()
    {
        Vector2 move2 = moveAction.ReadValue<Vector2>();
        Vector3 input = new Vector3(move2.x, 0f, move2.y);
        if (input.sqrMagnitude > 1f) input.Normalize();

        Vector3 camForward = Vector3.forward;
        Vector3 camRight = Vector3.right;

        if (mainCamera != null)
        {
            camForward = mainCamera.forward;
            camRight = mainCamera.right;

            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();
        }

        Vector3 moveDir = (camForward * input.z + camRight * input.x);
        Vector3 velocity = moveDir * moveSpeed;

        if (rb != null && useRigidbodyMove)
            rb.MovePosition(rb.position + velocity * Time.deltaTime);
        else
            transform.position += velocity * Time.deltaTime;

        if (rotateToMoveDir && moveDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                rotateSpeed * Time.deltaTime
            );
        }
    }
}
