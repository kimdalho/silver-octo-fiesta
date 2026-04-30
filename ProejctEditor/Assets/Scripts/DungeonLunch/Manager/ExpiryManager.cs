using UnityEngine;

public class ExpiryManager : MonoBehaviour
{
    public static ExpiryManager instance;

    public bool isRunning;
    public float spoilMultiplier = 1f;

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (!isRunning) return;
        float dt = Time.deltaTime;
        LunchboxManager.instance?.TickExpiry(dt, spoilMultiplier);
        DungeonInventoryManager.instance?.TickExpiry(dt, spoilMultiplier);
    }

    public void StartExpiry() { isRunning = true; spoilMultiplier = 1f; }
    public void StopExpiry()  { isRunning = false; }
    public void ApplySporeEffect() => spoilMultiplier = 1.3f;
    public void ResetSporeEffect() => spoilMultiplier = 1f;
}
