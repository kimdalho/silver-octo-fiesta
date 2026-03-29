using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow instance { get; private set; }

    public Transform target;

    [Header("3rd Person Shoulder View")]
    public Vector3 shoulderOffset = new Vector3(0.5f, 1.5f, -3f);
    public float mouseSensitivity = 2f;
    public float minPitch = -30f;
    public float maxPitch = 60f;

    [Header("Zoom")]
    public float zoomSpeed = 2f;
    public float minZoomDist = 2f;
    public float maxZoomDist = 8f;

    // 카메라 흔들림
    private float shakeTimer;
    private float shakeIntensity;
    private float shakeDamping = 5f;

    private float yaw;
    private float pitch = 10f;
    private float shoulderDist;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        shoulderDist = Mathf.Abs(shoulderOffset.z);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (!target) return;

        // 마우스 입력 (커서 잠김 상태에서만)
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            yaw   += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch  = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        // 줌
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
            shoulderDist = Mathf.Clamp(shoulderDist - scroll * zoomSpeed * 5f, minZoomDist, maxZoomDist);

        // 위치 / 회전 계산
        Quaternion camRot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 localOffset = new Vector3(shoulderOffset.x, shoulderOffset.y, -shoulderDist);

        Vector3 goalPos = target.position + camRot * localOffset;
        Quaternion goalRot = Quaternion.LookRotation(target.position + Vector3.up * shoulderOffset.y - goalPos);

        transform.position = goalPos;
        transform.rotation = goalRot;

        // 카메라 흔들림 적용
        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;
            float intensity = Mathf.Lerp(0f, shakeIntensity, shakeTimer * shakeDamping);
            transform.position += Random.insideUnitSphere * intensity;
        }
    }

    /// <summary>카메라 흔들림. 대포 발사 시 호출.</summary>
    public void Shake(float intensity = 0.15f, float duration = 0.3f)
    {
        shakeIntensity = intensity;
        shakeTimer = duration;
    }

    /// <summary>커서 잠금 상태를 외부에서 설정 (배치 시스템 등).</summary>
    public void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
