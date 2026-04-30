using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "DungeonLunch/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public Sprite sprite;
    public float maxHP;
    public float attack;
    public float defense;
    public int minGoldDrop;
    public int maxGoldDrop;
    public FoodData[] possibleDrops;
    [Range(0f, 1f)] public float[] dropChances;
    public bool isSporeType;
    public int minFloor;
    public int maxFloor;
}
