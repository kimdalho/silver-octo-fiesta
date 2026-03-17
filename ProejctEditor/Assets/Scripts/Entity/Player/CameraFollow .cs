using UnityEngine;

public enum CameraMode { TopView, ShoulderView }

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow instance { get; private set; }

    public Transform target;
    public CameraMode mode = CameraMode.TopView;

    [Header("Top View (Isometric)")]
    public Vector3 offset = new Vector3(0f, 10f, -8f);
    public float rotateSmooth = 10f;

    [Header("Shoulder View (3rd Person)")]
    public Vector3 shoulderOffset = new Vector3(0.5f, 1.5f, -3f);
    public float mouseSensitivity = 2f;
    public float minPitch = -30f;
    public float maxPitch = 60f;
    public float shoulderSmooth = 12f;

    [Header("Zoom")]
    public float zoomSpeed = 2f;
    public float minZoom = 5f;
    public float maxZoom = 15f;

    // TopView 내부 상태
    private Vector3 currentOffset;
    private Vector3 targetOffset;

    // ShoulderView 내부 상태
    private float yaw;
    private float pitch = 10f;
    private float shoulderDist;

    // 모드 전환 보간
    private Vector3 transitionPos;
    private Quaternion transitionRot;
    private float transitionT = 1f;
    private const float TransitionDuration = 0.5f;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        currentOffset = offset;
        targetOffset = offset;
        shoulderDist = shoulderOffset.magnitude;
    }

    public void SetMode(CameraMode newMode, bool instant = false)
    {
        if (mode == newMode) return;

        if (!instant)
        {
            // 전환 보간 시작: 현재 카메라 상태 저장
            transitionPos = transform.position;
            transitionRot = transform.rotation;
            transitionT = 0f;
        }
        else
        {
            transitionT = 1f;
        }

        mode = newMode;

        if (newMode == CameraMode.ShoulderView)
        {
            // 현재 카메라 방향에서 yaw/pitch 추출
            Vector3 dir = transform.forward;
            yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            pitch = -Mathf.Asin(dir.y) * Mathf.Rad2Deg;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            shoulderDist = shoulderOffset.magnitude;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            // TopView 복귀 시 offset 초기화
            currentOffset = offset;
            targetOffset = offset;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void LateUpdate()
    {
        if (!target) return;

        // ESC로 커서 잠금 토글 (ShoulderView)
        if (mode == CameraMode.ShoulderView && Input.GetKeyDown(KeyCode.Escape))
        {
            bool locked = Cursor.lockState == CursorLockMode.Locked;
            Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = locked;
        }

        // 목표 위치/회전 계산
        Vector3 goalPos;
        Quaternion goalRot;

        if (mode == CameraMode.TopView)
            ComputeTopView(out goalPos, out goalRot);
        else
            ComputeShoulderView(out goalPos, out goalRot);

        // 모드 전환 보간
        if (transitionT < 1f)
        {
            transitionT += Time.deltaTime / TransitionDuration;
            transitionT = Mathf.Clamp01(transitionT);
            float t = Mathf.SmoothStep(0f, 1f, transitionT);

            transform.position = Vector3.Lerp(transitionPos, goalPos, t);
            transform.rotation = Quaternion.Slerp(transitionRot, goalRot, t);
        }
        else
        {
            transform.position = goalPos;
            transform.rotation = goalRot;
        }
    }

    private void ComputeTopView(out Vector3 pos, out Quaternion rot)
    {
        // Q/E 90도 스냅 회전
        if (Input.GetKeyDown(KeyCode.Q))
            targetOffset = Quaternion.Euler(0f, -90f, 0f) * targetOffset;
        if (Input.GetKeyDown(KeyCode.E))
            targetOffset = Quaternion.Euler(0f, 90f, 0f) * targetOffset;

        // 줌
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            float dist = targetOffset.magnitude;
            dist = Mathf.Clamp(dist - scroll * zoomSpeed * 10f, minZoom, maxZoom);
            targetOffset = targetOffset.normalized * dist;
        }

        // 오프셋 회전만 부드럽게
        currentOffset = Vector3.Lerp(currentOffset, targetOffset, rotateSmooth * Time.deltaTime);

        pos = target.position + currentOffset;
        rot = Quaternion.LookRotation(target.position - pos);
    }

    private void ComputeShoulderView(out Vector3 pos, out Quaternion rot)
    {
        // 락온 활성화 시 자동 추적 카메라
        if (LockOnTarget.instance != null && LockOnTarget.instance.IsLockedOn)
        {
            ComputeLockOnView(out pos, out rot);
            return;
        }

        // 마우스 입력 (커서 잠김 상태에서만)
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        // 줌 (거리 조절)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            shoulderDist = Mathf.Clamp(shoulderDist - scroll * zoomSpeed * 5f, 2f, 8f);
        }

        // 회전 → 위치 계산
        Quaternion camRotation = Quaternion.Euler(pitch, yaw, 0f);

        // 어깨 오프셋 (X = 우측으로 살짝, Y = 높이, Z = 뒤로 거리)
        Vector3 localOffset = new Vector3(
            shoulderOffset.x,
            shoulderOffset.y,
            -shoulderDist
        );

        pos = target.position + camRotation * localOffset;
        rot = Quaternion.LookRotation(target.position + Vector3.up * shoulderOffset.y - pos);
    }

    private void ComputeLockOnView(out Vector3 pos, out Quaternion rot)
    {
        Transform lockTarget = LockOnTarget.instance.CurrentTarget;

        // 플레이어 → 타겟 방향
        Vector3 dirToTarget = lockTarget.position - target.position;
        dirToTarget.y = 0f;

        // 카메라가 플레이어 뒤쪽에 위치하도록 yaw 계산
        float targetYaw = Mathf.Atan2(dirToTarget.x, dirToTarget.z) * Mathf.Rad2Deg;
        float targetPitch = 15f; // 약간 위에서 내려다봄

        // 부드러운 전환
        yaw = Mathf.LerpAngle(yaw, targetYaw, shoulderSmooth * Time.deltaTime);
        pitch = Mathf.Lerp(pitch, targetPitch, shoulderSmooth * Time.deltaTime);

        Quaternion camRotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 localOffset = new Vector3(
            shoulderOffset.x,
            shoulderOffset.y,
            -shoulderDist
        );

        pos = target.position + camRotation * localOffset;

        // 플레이어와 타겟의 중간점을 바라봄
        Vector3 midPoint = (target.position + lockTarget.position) * 0.5f + Vector3.up * shoulderOffset.y;
        rot = Quaternion.LookRotation(midPoint - pos);
    }
}
