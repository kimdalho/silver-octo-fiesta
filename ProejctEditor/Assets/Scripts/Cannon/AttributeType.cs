/// <summary>
/// 속성 시스템의 6가지 속성.
/// 직접 입력: Water, Fire, Electric (포탄으로 부여)
/// 파생: Growth, Structure, Decay (2속성 조합)
/// </summary>
public enum AttributeType
{
    // 직접 입력 (포탄)
    Water,      // 습기
    Fire,       // 열
    Electric,   // 전도

    // 파생 (2속성 조합)
    Growth,     // 생장 (물+전기)
    Structure,  // 구조 (불+전기)
    Decay       // 부패 (물+불)
}
