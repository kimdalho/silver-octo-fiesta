using UnityEditor;
using UnityEngine;

public class WeaponPrefabCreator
{
    [MenuItem("Tools/Create Weapon Prefabs (총통 3종)")]
    static void CreateWeaponPrefabs()
    {
        string prefabFolder = "Assets/Prefabs/Weapons";
        string itemFolder = "Assets/Data/Items";

        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder(prefabFolder))
            AssetDatabase.CreateFolder("Assets/Prefabs", "Weapons");
        if (!AssetDatabase.IsValidFolder("Assets/Data"))
            AssetDatabase.CreateFolder("Assets", "Data");
        if (!AssetDatabase.IsValidFolder(itemFolder))
            AssetDatabase.CreateFolder("Assets/Data", "Items");

        // --- 1. 세총통 (Se-Chongtong) ---
        CreateWeapon(prefabFolder, itemFolder,
            name: "세총통",
            fileName: "Weapon_SeChongtong",
            description: "조선 최소형 화기. 집게로 잡고 쏘는 14cm짜리 꼬마 총통.\n연사가 빠르지만 속성 부여량이 적다.",
            // 프리팹 치수 (월드 단위)
            barrelLength: 0.25f,
            barrelRadius: 0.04f,
            chamberLength: 0.08f,
            chamberRadius: 0.055f,
            handleLength: 0.1f,
            handleRadius: 0.02f,
            bodyColor: new Color(0.45f, 0.35f, 0.25f),     // 구리빛 갈색
            accentColor: new Color(0.6f, 0.5f, 0.3f),      // 밝은 황동
            // 스탯
            stats: new StatModifier[]
            {
                new StatModifier(StatType.Attack, ModifierType.Flat, 3f),
                new StatModifier(StatType.MoveSpeed, ModifierType.Flat, 0.5f)  // 가벼워서 속도+
            }
        );

        // --- 2. 승자총통 (Seungja-Chongtong) ---
        CreateWeapon(prefabFolder, itemFolder,
            name: "승자총통",
            fileName: "Weapon_SeungjaChongtong",
            description: "임진왜란의 주력 개인화기. 철환 15발을 장전하는 56cm 총통.\n밸런스가 좋은 중급 무기.",
            barrelLength: 0.5f,
            barrelRadius: 0.05f,
            chamberLength: 0.12f,
            chamberRadius: 0.07f,
            handleLength: 0.2f,
            handleRadius: 0.025f,
            bodyColor: new Color(0.35f, 0.33f, 0.3f),      // 철색
            accentColor: new Color(0.5f, 0.45f, 0.35f),    // 황동 띠
            stats: new StatModifier[]
            {
                new StatModifier(StatType.Attack, ModifierType.Flat, 7f),
            }
        );

        // --- 3. 불랑기 (Bullanggi) ---
        CreateWeapon(prefabFolder, itemFolder,
            name: "불랑기",
            fileName: "Weapon_Bullanggi",
            description: "후장식 교체 포. 자탄통을 바꿔 끼워 속성 전환이 빠르다.\n무겁지만 범위와 위력이 최고.",
            barrelLength: 0.65f,
            barrelRadius: 0.065f,
            chamberLength: 0.18f,
            chamberRadius: 0.09f,
            handleLength: 0.25f,
            handleRadius: 0.03f,
            bodyColor: new Color(0.3f, 0.28f, 0.26f),      // 어두운 철색
            accentColor: new Color(0.55f, 0.4f, 0.2f),     // 금빛 장식
            stats: new StatModifier[]
            {
                new StatModifier(StatType.Attack, ModifierType.Flat, 12f),
                new StatModifier(StatType.MoveSpeed, ModifierType.Flat, -0.8f)  // 무거워서 속도-
            }
        );

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[WeaponPrefabCreator] 총통 3종 프리팹 + SO 생성 완료! (Prefabs/Weapons/, Data/Items/)");
    }

    static void CreateWeapon(
        string prefabFolder, string itemFolder,
        string name, string fileName, string description,
        float barrelLength, float barrelRadius,
        float chamberLength, float chamberRadius,
        float handleLength, float handleRadius,
        Color bodyColor, Color accentColor,
        StatModifier[] stats)
    {
        // ========== 3D 프리팹 ==========
        var root = new GameObject(name);

        // 머티리얼 생성
        var bodyMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        bodyMat.color = bodyColor;
        string bodyMatPath = $"{prefabFolder}/{fileName}_Body.mat";
        AssetDatabase.CreateAsset(bodyMat, bodyMatPath);

        var accentMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        accentMat.color = accentColor;
        string accentMatPath = $"{prefabFolder}/{fileName}_Accent.mat";
        AssetDatabase.CreateAsset(accentMat, accentMatPath);

        var handleMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        handleMat.color = new Color(0.35f, 0.22f, 0.12f); // 나무 손잡이
        string handleMatPath = $"{prefabFolder}/{fileName}_Handle.mat";
        AssetDatabase.CreateAsset(handleMat, handleMatPath);

        // --- 포신 (Barrel) ---
        var barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        barrel.name = "Barrel";
        barrel.transform.SetParent(root.transform);
        barrel.transform.localScale = new Vector3(barrelRadius * 2f, barrelLength * 0.5f, barrelRadius * 2f);
        barrel.transform.localPosition = new Vector3(0f, 0f, chamberLength * 0.5f + barrelLength * 0.5f);
        barrel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        barrel.GetComponent<MeshRenderer>().sharedMaterial = bodyMat;
        Object.DestroyImmediate(barrel.GetComponent<Collider>());

        // --- 포구 링 (Muzzle Ring) ---
        var muzzle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        muzzle.name = "MuzzleRing";
        muzzle.transform.SetParent(root.transform);
        float muzzleRadius = barrelRadius * 1.3f;
        muzzle.transform.localScale = new Vector3(muzzleRadius * 2f, 0.015f, muzzleRadius * 2f);
        muzzle.transform.localPosition = new Vector3(0f, 0f, chamberLength * 0.5f + barrelLength);
        muzzle.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        muzzle.GetComponent<MeshRenderer>().sharedMaterial = accentMat;
        Object.DestroyImmediate(muzzle.GetComponent<Collider>());

        // --- 약실 (Chamber) ---
        var chamber = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        chamber.name = "Chamber";
        chamber.transform.SetParent(root.transform);
        chamber.transform.localScale = new Vector3(chamberRadius * 2f, chamberLength * 0.5f, chamberRadius * 2f);
        chamber.transform.localPosition = Vector3.zero;
        chamber.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        chamber.GetComponent<MeshRenderer>().sharedMaterial = bodyMat;
        Object.DestroyImmediate(chamber.GetComponent<Collider>());

        // --- 약실 띠 장식 (Chamber Band) ---
        var band = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        band.name = "ChamberBand";
        band.transform.SetParent(root.transform);
        float bandRadius = chamberRadius * 1.15f;
        band.transform.localScale = new Vector3(bandRadius * 2f, 0.012f, bandRadius * 2f);
        band.transform.localPosition = new Vector3(0f, 0f, chamberLength * 0.2f);
        band.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        band.GetComponent<MeshRenderer>().sharedMaterial = accentMat;
        Object.DestroyImmediate(band.GetComponent<Collider>());

        // --- 손잡이 (Handle) ---
        var handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        handle.name = "Handle";
        handle.transform.SetParent(root.transform);
        handle.transform.localScale = new Vector3(handleRadius * 2f, handleLength * 0.5f, handleRadius * 2f);
        handle.transform.localPosition = new Vector3(0f, -chamberRadius * 0.7f, -handleLength * 0.3f);
        handle.transform.localRotation = Quaternion.Euler(15f, 0f, 0f); // 약간 기울임
        handle.GetComponent<MeshRenderer>().sharedMaterial = handleMat;
        Object.DestroyImmediate(handle.GetComponent<Collider>());

        // --- 점화구 (Touch Hole) 장식 ---
        var touchHole = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        touchHole.name = "TouchHole";
        touchHole.transform.SetParent(root.transform);
        float thRadius = chamberRadius * 0.2f;
        touchHole.transform.localScale = Vector3.one * thRadius * 2f;
        touchHole.transform.localPosition = new Vector3(0f, chamberRadius * 0.9f, -chamberLength * 0.1f);
        touchHole.GetComponent<MeshRenderer>().sharedMaterial = accentMat;
        Object.DestroyImmediate(touchHole.GetComponent<Collider>());

        // 루트에 BoxCollider 추가
        var col = root.AddComponent<BoxCollider>();
        float totalLength = handleLength * 0.3f + chamberLength + barrelLength;
        col.size = new Vector3(chamberRadius * 2f, chamberRadius * 2f, totalLength);
        col.center = new Vector3(0f, 0f, totalLength * 0.5f - handleLength * 0.3f);

        // 프리팹 저장
        string prefabPath = $"{prefabFolder}/{fileName}.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);

        // ========== EquipmentData SO ==========
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        var equip = ScriptableObject.CreateInstance<EquipmentData>();
        equip.itemName = name;
        equip.description = description;
        equip.itemType = ItemType.Equipment;
        equip.maxStack = 1;
        equip.equipSlot = EquipSlot.Weapon;
        equip.statModifiers = stats;
        equip.modelPrefab = prefab;
        AssetDatabase.CreateAsset(equip, $"{itemFolder}/{fileName}.asset");
    }
}
