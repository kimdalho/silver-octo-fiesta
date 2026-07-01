using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    [SerializeField] GameObject floorPrefab;
    [SerializeField] GameObject wallPrefab;
    [SerializeField] GameObject doorPrefab;

    [SerializeField] int width = 5;
    [SerializeField] int height = 5;
    [SerializeField] float cellSize = 1f;

    public enum WallSide { South, North, West, East }

    [System.Serializable]
    public struct DoorPlacement
    {
        public WallSide side;
        public int index;
    }

    [SerializeField] List<DoorPlacement> doors = new List<DoorPlacement>();

    [ContextMenu("Generate")]
    public void Generate()
    {
        Clear();

        float c = cellSize;

        for (int x = 0; x < width; x++)
            for (int z = 0; z < height; z++)
                Spawn(floorPrefab, new Vector3(x * c, 0, z * c), 90, 0, 0);

        for (int x = 0; x < width; x++)
            SpawnWall(WallSide.South, x, new Vector3(x * c, 0, -c), 0);

        for (int x = 0; x < width; x++)
            SpawnWall(WallSide.North, x, new Vector3(x * c, 0, height * c), 180);

        for (int z = 0; z < height; z++)
            SpawnWall(WallSide.West, z, new Vector3(-c, 0, z * c), 270);

        for (int z = 0; z < height; z++)
            SpawnWall(WallSide.East, z, new Vector3(width * c, 0, z * c), 90);


    }

    [ContextMenu("Clear")]
    public void Clear()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
    }

    void SpawnWall(WallSide side, int index, Vector3 pos, float yRot)
    {
        bool isDoor = doors.Exists(d => d.side == side && d.index == index);
        Spawn(isDoor ? doorPrefab : wallPrefab, pos, yRot);
    }

    void Spawn(GameObject prefab, Vector3 localPos, float yRot) =>
        Spawn(prefab, localPos, 0, yRot, 0);

    void Spawn(GameObject prefab, Vector3 localPos, float xRot, float yRot, float zRot)
    {
        var go = Instantiate(prefab, transform);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.Euler(xRot, yRot, zRot);
    }
}
