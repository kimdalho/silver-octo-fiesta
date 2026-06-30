using UnityEngine;

public class Player : BaseCharacter
{
    protected override void Move()
    {
        // 마우스 방향 = 플레이어 회전 (손전등 방향)
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position);
        if (groundPlane.Raycast(ray, out float dist))
        {
            Vector3 lookAt = ray.GetPoint(dist);
            lookAt.y = transform.position.y;
            Vector3 lookDir = lookAt - transform.position;
            if (lookDir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(lookDir);
        }

        // 이동 = 카메라가 바라보는 방향 기준
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        dir = (camForward * v + camRight * h) * speed;
        dir.y += Physics.gravity.y * Time.deltaTime;
        cc.Move(dir * Time.deltaTime);
    }
}
