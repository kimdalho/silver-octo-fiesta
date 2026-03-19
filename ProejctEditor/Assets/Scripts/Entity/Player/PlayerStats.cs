using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats instance;

    [Header("기본 스탯")]
    public float baseMaxHP = 100f;
    public float baseAttack = 10f;
    public float baseDefense = 5f;
    public float baseMoveSpeed = 5f;

    // 최종 스탯 (기본 + 장비 보너스)
    public float MaxHP { get; private set; }
    public float Attack { get; private set; }
    public float Defense { get; private set; }
    public float MoveSpeed { get; private set; }

    public float currentHP;
    public bool isDead;

    private PlayerMoveCC playerMove;
    private System.Action<EquipSlot> onEquipChangedHandler;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }
        instance = this;
    }

    void Start()
    {
        playerMove = GetComponent<PlayerMoveCC>();

        onEquipChangedHandler = _ => Recalculate();
        if (InventoryManager.instance != null)
            InventoryManager.instance.equipment.OnEquipChanged += onEquipChangedHandler;

        Recalculate();
        currentHP = MaxHP;
    }

    void OnDestroy()
    {
        if (InventoryManager.instance != null && onEquipChangedHandler != null)
            InventoryManager.instance.equipment.OnEquipChanged -= onEquipChangedHandler;
    }

    public void Recalculate()
    {
        float flatHP = 0, flatAtk = 0, flatDef = 0, flatSpd = 0;
        float pctHP = 0, pctAtk = 0, pctDef = 0, pctSpd = 0;

        if (InventoryManager.instance != null)
        {
            var mods = InventoryManager.instance.equipment.GetAllModifiers();
            foreach (var mod in mods)
            {
                float v = mod.value;
                bool isFlat = mod.modifierType == ModifierType.Flat;

                switch (mod.statType)
                {
                    case StatType.MaxHP:    if (isFlat) flatHP += v; else pctHP += v; break;
                    case StatType.Attack:   if (isFlat) flatAtk += v; else pctAtk += v; break;
                    case StatType.Defense:  if (isFlat) flatDef += v; else pctDef += v; break;
                    case StatType.MoveSpeed:if (isFlat) flatSpd += v; else pctSpd += v; break;
                }
            }
        }

        MaxHP = (baseMaxHP + flatHP) * (1f + pctHP / 100f);
        Attack = (baseAttack + flatAtk) * (1f + pctAtk / 100f);
        Defense = (baseDefense + flatDef) * (1f + pctDef / 100f);
        MoveSpeed = (baseMoveSpeed + flatSpd) * (1f + pctSpd / 100f);

        // PlayerMoveCC에 이동속도 반영
        if (playerMove != null)
            playerMove.moveSpeed = MoveSpeed;

        // HP가 최대치 초과하지 않도록
        if (currentHP > MaxHP)
            currentHP = MaxHP;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        float damage = Mathf.Max(amount - Defense, 1f);
        currentHP -= damage;

        if (currentHP <= 0f)
        {
            currentHP = 0f;
            isDead = true;

            if (InventoryManager.instance != null)
                InventoryManager.instance.OnPlayerDeath();

            if (GameLoopManager.instance != null)
                GameLoopManager.instance.LoadLocalField();
        }
    }

    public void Respawn()
    {
        isDead = false;
        Recalculate();
        currentHP = MaxHP;
    }
}
