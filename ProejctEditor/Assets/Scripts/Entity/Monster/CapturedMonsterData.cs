using UnityEngine;

/// <summary>
/// 전기 속성 누적으로 생포된 몬스터 아이템.
/// Inspector에서 sourceMonster에 MonsterData SO를 연결한다.
/// 플레이어 인벤토리에 추가되고, 로컬 우리(MonsterPen)에 넣으면 자원을 생산한다.
/// </summary>
[CreateAssetMenu(fileName = "Captured_", menuName = "Entity/Captured Monster")]
public class CapturedMonsterData : ItemData
{
    [Header("원본 몬스터")]
    public MonsterData sourceMonster;
}
