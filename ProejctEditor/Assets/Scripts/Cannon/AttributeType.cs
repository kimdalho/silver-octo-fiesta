/// <summary>
/// 속성 시스템의 6가지 속성.
/// 직접 입력: Water, Fire, Electric (포탄으로 부여)
/// 파생: Growth, Structure, Decay (2속성 조합)
/// </summary>
public enum AttributeType
{
    // 직접 입력 (포탄)
    Water     = 0,  // 습기
    Fire      = 1,  // 열
    Electric  = 2,  // 전도
    Spore     = 3,  // 포자

    // 파생 (2속성 조합)
    Growth    = 4,  // 생장
    Structure = 5,  // 구조
    Decay     = 6   // 부패
}
