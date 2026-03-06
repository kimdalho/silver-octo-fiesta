using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform cam;

    void LateUpdate()
    {
        if (cam == null)
        {
            var main = Camera.main;
            if (main == null) return;
            cam = main.transform;
        }

        // 카메라 회전을 그대로 따라감 → 항상 정면으로 보임
        transform.rotation = cam.rotation;
    }
}
