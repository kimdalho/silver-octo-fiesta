using UnityEngine;

public class SettingController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Camera targetCamera;

    [Header("Isometric Camera Preset")]
    [SerializeField] private Vector3 rotationEuler = new Vector3(50f, 45f, 0f);
    [SerializeField] private bool useOrthographic = true;
    [SerializeField] private float orthoSize = 8f;

    [Header("Apply Options")]
    [SerializeField] private bool applyOnAwake = true;   // 게임 시작 시 자동 적용
    [SerializeField] private bool applyInEditMode = true; // 에디터에서 값 바꾸면 즉시 반영

    private void Reset()
    {
        // 컴포넌트 추가 시 자동으로 메인 카메라 잡아주기
        if (!targetCamera) targetCamera = Camera.main;
    }

    private void Awake()
    {
        if (applyOnAwake)
            ApplyCameraPreset();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!applyInEditMode) return;
        if (!targetCamera) targetCamera = Camera.main;

        // 플레이 중이 아닐 때도 반영되게
        ApplyCameraPreset();
    }
#endif

    [ContextMenu("Apply Camera Preset")]
    public void ApplyCameraPreset()
    {
        if (!targetCamera)
        {
            Debug.LogWarning("[SettingController] targetCamera is null.");
            return;
        }

        // Rotation
        targetCamera.transform.rotation = Quaternion.Euler(rotationEuler);

        // Projection
        targetCamera.orthographic = useOrthographic;
        if (useOrthographic)
            targetCamera.orthographicSize = orthoSize;
    }
}