using UnityEngine;
using UnityEditor;

public class TestItemCreator
{
    [MenuItem("Tools/Create Test Items")]
    static void CreateTestItems()
    {
        string path = "Assets/Data/Items";
        if (!AssetDatabase.IsValidFolder(path))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            AssetDatabase.CreateFolder("Assets/Data", "Items");
        }

        // ===== 장비 =====

        // 무기: 나무 검
        var sword = ScriptableObject.CreateInstance<EquipmentData>();
        sword.itemName = "나무 검";
        sword.description = "대충 깎은 나무 검. 없는 것보다는 낫다.";
        sword.itemType = ItemType.Equipment;
        sword.maxStack = 1;
        sword.equipSlot = EquipSlot.Weapon;
        sword.statModifiers = new StatModifier[]
        {
            new StatModifier(StatType.Attack, ModifierType.Flat, 5f)
        };
        AssetDatabase.CreateAsset(sword, $"{path}/Weapon_WoodSword.asset");

        // 무기: 돌 도끼
        var axe = ScriptableObject.CreateInstance<EquipmentData>();
        axe.itemName = "돌 도끼";
        axe.description = "돌로 만든 도끼. 나무를 벨 수 있다.";
        axe.itemType = ItemType.Equipment;
        axe.maxStack = 1;
        axe.equipSlot = EquipSlot.Weapon;
        axe.statModifiers = new StatModifier[]
        {
            new StatModifier(StatType.Attack, ModifierType.Flat, 8f),
            new StatModifier(StatType.MoveSpeed, ModifierType.Percent, -5f)
        };
        AssetDatabase.CreateAsset(axe, $"{path}/Weapon_StoneAxe.asset");

        // 머리: 풀 모자
        var hat = ScriptableObject.CreateInstance<EquipmentData>();
        hat.itemName = "풀 모자";
        hat.description = "풀을 엮어 만든 모자. 머리가 시원하다.";
        hat.itemType = ItemType.Equipment;
        hat.maxStack = 1;
        hat.equipSlot = EquipSlot.Head;
        hat.statModifiers = new StatModifier[]
        {
            new StatModifier(StatType.Defense, ModifierType.Flat, 2f),
            new StatModifier(StatType.MaxHP, ModifierType.Flat, 10f)
        };
        AssetDatabase.CreateAsset(hat, $"{path}/Head_GrassHat.asset");

        // 몸통: 나무 갑옷
        var armor = ScriptableObject.CreateInstance<EquipmentData>();
        armor.itemName = "나무 갑옷";
        armor.description = "나무 껍질로 만든 갑옷. 움직임이 좀 둔해진다.";
        armor.itemType = ItemType.Equipment;
        armor.maxStack = 1;
        armor.equipSlot = EquipSlot.Body;
        armor.statModifiers = new StatModifier[]
        {
            new StatModifier(StatType.Defense, ModifierType.Flat, 8f),
            new StatModifier(StatType.MoveSpeed, ModifierType.Flat, -0.5f)
        };
        AssetDatabase.CreateAsset(armor, $"{path}/Body_WoodArmor.asset");

        // ===== 소비 아이템 =====

        var berry = ScriptableObject.CreateInstance<ItemData>();
        berry.itemName = "딸기";
        berry.description = "달콤한 야생 딸기. 배고픔을 달래준다.";
        berry.itemType = ItemType.Consumable;
        berry.maxStack = 20;
        AssetDatabase.CreateAsset(berry, $"{path}/Consumable_Berry.asset");

        // ===== 기본 자재 (필드에서 채집) =====

        var twig = ScriptableObject.CreateInstance<ItemData>();
        twig.itemName = "나뭇가지";
        twig.description = "잔가지. 무언가를 만들 수 있을 것 같다.";
        twig.itemType = ItemType.MaterialItem;
        twig.maxStack = 40;
        AssetDatabase.CreateAsset(twig, $"{path}/Material_Twig.asset");

        var stone = ScriptableObject.CreateInstance<ItemData>();
        stone.itemName = "돌";
        stone.description = "단단한 돌멩이. 도구나 포탄의 기본 재료.";
        stone.itemType = ItemType.MaterialItem;
        stone.maxStack = 40;
        AssetDatabase.CreateAsset(stone, $"{path}/Material_Stone.asset");

        var web = ScriptableObject.CreateInstance<ItemData>();
        web.itemName = "거미줄";
        web.description = "끈적이는 거미줄. 접착제 대용으로 쓸 수 있다.";
        web.itemType = ItemType.MaterialItem;
        web.maxStack = 20;
        AssetDatabase.CreateAsset(web, $"{path}/Material_Web.asset");

        var grass = ScriptableObject.CreateInstance<ItemData>();
        grass.itemName = "풀";
        grass.description = "질긴 풀. 엮으면 끈이나 옷감이 된다.";
        grass.itemType = ItemType.MaterialItem;
        grass.maxStack = 40;
        AssetDatabase.CreateAsset(grass, $"{path}/Material_Grass.asset");

        // ===== 채굴/드롭 자재 =====

        var copper = ScriptableObject.CreateInstance<ItemData>();
        copper.itemName = "구리 조각";
        copper.description = "녹슨 구리 파편. 총통의 포신 재료.";
        copper.itemType = ItemType.MaterialItem;
        copper.maxStack = 20;
        AssetDatabase.CreateAsset(copper, $"{path}/Material_Copper.asset");

        var iron = ScriptableObject.CreateInstance<ItemData>();
        iron.itemName = "철 조각";
        iron.description = "무거운 철 파편. 강력한 무기의 핵심 재료.";
        iron.itemType = ItemType.MaterialItem;
        iron.maxStack = 20;
        AssetDatabase.CreateAsset(iron, $"{path}/Material_Iron.asset");

        var gunpowder = ScriptableObject.CreateInstance<ItemData>();
        gunpowder.itemName = "화약 가루";
        gunpowder.description = "위험한 검은 가루. 폭발적인 힘을 지녔다.";
        gunpowder.itemType = ItemType.MaterialItem;
        gunpowder.maxStack = 20;
        AssetDatabase.CreateAsset(gunpowder, $"{path}/Material_Gunpowder.asset");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[TestItemCreator] 아이템 12개 생성 완료! (Assets/Data/Items/)");
    }
}
