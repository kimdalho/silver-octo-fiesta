using System;
using UnityEngine;

public class DropTable : MonoBehaviour
{
    [Serializable]
    public class DropEntry
    {
        public ItemData item;
        public int count = 1;
        [Range(0f, 1f)] public float chance = 1f;
    }

    public DropEntry[] drops;

    public void SpawnDrops(Vector3 position)
    {
        if (drops == null) return;

        foreach (var drop in drops)
        {
            if (drop.item == null) continue;
            if (UnityEngine.Random.value > drop.chance) continue;

            // 약간 흩뿌리기
            Vector3 offset = new Vector3(
                UnityEngine.Random.Range(-1f, 1f),
                0f,
                UnityEngine.Random.Range(-1f, 1f)
            );
            WorldItem.Spawn(drop.item, position + offset, drop.count);
        }
    }
}
