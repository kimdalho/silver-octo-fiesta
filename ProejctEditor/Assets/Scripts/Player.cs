using UnityEngine;

public class Player : BaseCharacter
{
    protected override void Move()
    {
        var h = Input.GetAxis("Horizontal");
        var v = Input.GetAxis("Vertical");

        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        dir = (camForward * v + camRight * h) * speed;

        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);

        dir.y += Physics.gravity.y * Time.deltaTime;
        cc.Move(dir * Time.deltaTime);
    }
}
