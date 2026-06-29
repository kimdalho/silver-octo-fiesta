using UnityEngine;

public interface IChacterService
{
    /// <summary>
    /// 맞으면 호출해
    /// </summary>
    /// <param name="hp"></param>
    public void Hit(float attack);

    /// <summary>
    /// 때리면 호출해
    /// </summary>
    /// <param name="attack"></param>
    public void Attack(float attack);
}

public class BaseCharacter : MonoBehaviour , IChacterService
{
    [SerializeField]
    protected Vector3 dir;

    [SerializeField]
    protected CharacterController cc;

    [SerializeField]
    protected float speed;
    [SerializeField]
    protected float attack;

    protected virtual void Update()
    {
        Move();
    }

    protected virtual void Move()
    {
        //if(cc.isGrounded)
        //{

        //}

        var h = Input.GetAxis("Horizontal");
        var v = Input.GetAxis("Vertical");


        dir = new Vector3(h, 0, v) * speed;
        if (dir != Vector3.zero)
        {
            // 진행 방향으로 캐릭터 회전
            transform.rotation = Quaternion.Euler(0, Mathf.Atan2(h, v) * Mathf.Rad2Deg, 0);
        }

        if (Input.GetKeyDown(KeyCode.Space))
            Attack(attack);


        dir.y += Physics.gravity.y * Time.deltaTime;
        cc.Move(dir * Time.deltaTime);
    }

    public void Hit(float attack)
    {
     
    }

    public void Attack(float attack)
    {
     
    }

}
