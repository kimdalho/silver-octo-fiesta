using UnityEngine;

[CreateAssetMenu(menuName = "Entity/Monster")]
public class MonsterData : ScriptableObject
{
    public float maxHP = 50f;
    public float attack = 5f;
    public float moveSpeed = 2f;
    public float detectRange = 10f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;
    public DropTable.DropEntry[] drops;
}
