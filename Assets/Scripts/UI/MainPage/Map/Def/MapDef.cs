using System;
using UnityEngine;

[Serializable]
public class LevelNodeDef
{
    public string levelId;
    public int level;

    [Tooltip("归一化坐标，左下角 (0,0)，右上角 (1,1)")]
    public Vector2 normalizedPos;
}
