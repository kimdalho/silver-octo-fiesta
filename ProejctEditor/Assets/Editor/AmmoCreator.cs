using UnityEditor;
using UnityEngine;

public class AmmoCreator
{
    [MenuItem("Tools/Create Ammo Items (포탄 3종)")]
    static void CreateAmmoItems()
    {
        string path = "Assets/Data/Items";
        if (!AssetDatabase.IsValidFolder("Assets/Data"))
            AssetDatabase.CreateFolder("Assets", "Data");
        if (!AssetDatabase.IsValidFolder(path))
            AssetDatabase.CreateFolder("Assets/Data", "Items");

        // --- 물 포탄 ---
        var water = ScriptableObject.CreateInstance<AmmoData>();
        water.itemName = "물 포탄";
        water.description = "물 속성 포탄. 대상의 습기를 증가시킨다.\n축축해지고, 둔화되고, 결국 멈춘다.";
        water.itemType = ItemType.Consumable;
        water.maxStack = 30;
        water.attribute = AttributeType.Water;
        water.attributeAmount = 10f;
        water.launchSpeed = 15f;
        water.projectileColor = new Color(0.29f, 0.62f, 1f); // #4A9EFF
        AssetDatabase.CreateAsset(water, $"{path}/Ammo_Water.asset");

        // --- 불 포탄 ---
        var fire = ScriptableObject.CreateInstance<AmmoData>();
        fire.itemName = "불 포탄";
        fire.description = "불 속성 포탄. 대상의 열을 증가시킨다.\n건조해지고, 과열되면 도주한다.";
        fire.itemType = ItemType.Consumable;
        fire.maxStack = 30;
        fire.attribute = AttributeType.Fire;
        fire.attributeAmount = 10f;
        fire.launchSpeed = 15f;
        fire.projectileColor = new Color(1f, 0.42f, 0.29f); // #FF6B4A
        AssetDatabase.CreateAsset(fire, $"{path}/Ammo_Fire.asset");

        // --- 전기 포탄 ---
        var elec = ScriptableObject.CreateInstance<AmmoData>();
        elec.itemName = "전기 포탄";
        elec.description = "전기 속성 포탄. 대상의 전도를 증가시킨다.\n대전되어 경직되고, 과전류로 마비된다.";
        elec.itemType = ItemType.Consumable;
        elec.maxStack = 30;
        elec.attribute = AttributeType.Electric;
        elec.attributeAmount = 10f;
        elec.launchSpeed = 15f;
        elec.projectileColor = new Color(1f, 0.85f, 0.24f); // #FFD93D
        AssetDatabase.CreateAsset(elec, $"{path}/Ammo_Electric.asset");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[AmmoCreator] 포탄 3종 생성 완료! (Assets/Data/Items/)");
    }
}
