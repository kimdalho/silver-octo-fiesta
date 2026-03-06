using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 6f, -6f);
    public bool lateUpdate = true;

    void LateUpdate()
    {
        if (!lateUpdate) return;
        Tick();
    }

    void Update()
    {
        if (lateUpdate) return;
        Tick();
    }

    void Tick()
    {
        if (!target) return;
        transform.position = target.position + offset;
    }
}

