using UnityEngine;

public enum AnimState { Idle, Walk }

[CreateAssetMenu(menuName = "2D/8Dir Sprite Set", fileName = "Sprite8DirSet")]
public class Sprite8DirSet : ScriptableObject
{
    // 0:N, 1:NE, 2:E, 3:SE, 4:S, 5:SW, 6:W, 7:NW
    [Header("Idle Frames (8 dirs)")]
    public Sprite[] idleN;
    public Sprite[] idleNE;
    public Sprite[] idleE;
    public Sprite[] idleSE;
    public Sprite[] idleS;
    public Sprite[] idleSW;
    public Sprite[] idleW;
    public Sprite[] idleNW;

    [Header("Walk Frames (8 dirs)")]
    public Sprite[] walkN;
    public Sprite[] walkNE;
    public Sprite[] walkE;
    public Sprite[] walkSE;
    public Sprite[] walkS;
    public Sprite[] walkSW;
    public Sprite[] walkW;
    public Sprite[] walkNW;

    public Sprite[] GetFrames(AnimState state, int dir)
    {
        dir = Mathf.Clamp(dir, 0, 7);

        if (state == AnimState.Idle)
        {
            return dir switch
            {
                0 => idleN,
                1 => idleNE,
                2 => idleE,
                3 => idleSE,
                4 => idleS,
                5 => idleSW,
                6 => idleW,
                7 => idleNW,
                _ => idleS
            };
        }
        else
        {
            return dir switch
            {
                0 => walkN,
                1 => walkNE,
                2 => walkE,
                3 => walkSE,
                4 => walkS,
                5 => walkSW,
                6 => walkW,
                7 => walkNW,
                _ => walkS
            };
        }
    }
}