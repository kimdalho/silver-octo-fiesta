using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 10f, -8f);

    [Header("Smooth")]
    public float rotateSmooth = 10f;

    [Header("Zoom")]
    public float zoomSpeed = 2f;
    public float minZoom = 5f;
    public float maxZoom = 15f;

    private static CameraFollow instance;
    private Vector3 currentOffset;
    private Vector3 targetOffset;

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
    }

    void LateUpdate()
    {
        if (!target) return;

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

        // 위치는 즉시 따라감
        transform.position = target.position + currentOffset;
        transform.LookAt(target);
    }
}
