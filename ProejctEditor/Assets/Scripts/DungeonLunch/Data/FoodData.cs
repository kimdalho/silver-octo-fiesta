using UnityEngine;

[CreateAssetMenu(fileName = "NewFood", menuName = "DungeonLunch/FoodData")]
public class FoodData : ItemData
{
    public float hungerRestore;
    public float protein;
    public float carbs;
    public float fat;
    public float magicPower;
    public float expiryDuration;   // 유통기한 (초 단위)
    public bool isNonPerishable;   // 소금 등 비부패 아이템
    public bool isRawIngredient;   // true = 조리 재료, false = 바로 먹는 요리
}
