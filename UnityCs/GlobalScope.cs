// #define UNITY_ENGINE
using System.Collections.Generic;
using System.IO;
using System;
using Newtonsoft.Json;
using System.Diagnostics;
#if UNITY_ENGINE
using UnityEngine;
using Unity.VisualScripting;
public class GlobalScope : MonoBehaviour
#else 
public class GlobalScope
#endif
{
    private static readonly string chessPropertiesJsonPath = "Json/ChessProperties.json";
    public static List<List<int>> chessGridStatus;
    public static readonly string[,] chessPositionNameList = {
        { "upLine0", "upLine1", "upLine2", "upLine3", "upLine4" },
        { "midLine0", "midLine1", "midLine2", "midLine3", "midLine4" },
        { "downLine0", "downLine1", "downLine2", "downLine3", "downLine4" }
    };
    public static readonly HashSet<string> chessPositionNameSet = new HashSet<string>
    {
        "upLine0", "upLine1", "upLine2", "upLine3", "upLine4",
        "midLine0", "midLine1", "midLine2", "midLine3", "midLine4",
        "downLine0", "downLine1", "downLine2", "downLine3", "downLine4"
    };
    public static HashSet<string> chessNameSet = new HashSet<string>{};
    public class ChessProperty
    {
        public string Name;
        public int Level;
        public int Cost;
        public List<List<int>> PosEffects;
        public List<List<int>> CardEffects;
        public HashSet<string> SpecialEffects;
    }
    private static List<ChessProperty> ChessProperties;
    public static ChessProperty GetChessProperty(string chessName)
    {
        foreach (var chess in ChessProperties) {
            if (chess.Name == chessName) {
                return chess;
            }
        }
        return null;
    }
    public static void LoadChessProperties()
    {
#if UNITY_ENGINE
        string path = Path.Combine(Application.streamingAssetsPath, chessPropertiesJsonPath);
#else
        string path = chessPropertiesJsonPath;
#endif
        string ChessPropertiesData = System.IO.File.ReadAllText(path);
        ChessProperties = JsonConvert.DeserializeObject<List<ChessProperty>>(ChessPropertiesData);

        HashSet<string> ChessNames = new HashSet<string>();
        foreach (var chess in ChessProperties)
        {
            ChessNames.Add(chess.Name);
        }
        chessNameSet = ChessNames;
    }
#if UNITY_ENGINE
    public static List<Tuple<GameObject, Vector3, Quaternion, float>> moveListLocal = new List<Tuple<GameObject, Vector3, Quaternion, float>>{};
    public static void MoveToLocal(GameObject obj, Vector3 targetPosition, Quaternion rotation, float moveSpeed, bool isClearTaskList = false)
    {
        if(isClearTaskList) {
            moveListLocal = new List<Tuple<GameObject, Vector3, Quaternion, float>>{};
        }
        moveListLocal.Add(new Tuple<GameObject, Vector3, Quaternion, float>(obj, targetPosition, rotation, moveSpeed));
    }
    public static List<Tuple<GameObject, Vector3, Quaternion, float>> moveListGlobal = new List<Tuple<GameObject, Vector3, Quaternion, float>>{};
    public static void MoveToGlobal(GameObject obj, Vector3 targetPosition, Quaternion rotation, float moveSpeed, bool isClearTaskList = false)
    {
        if(isClearTaskList) {
            moveListGlobal = new List<Tuple<GameObject, Vector3, Quaternion, float>>{};
        }
        moveListGlobal.Add(new Tuple<GameObject, Vector3, Quaternion, float>(obj, targetPosition, rotation, moveSpeed));
    }
    void Update() {
        if (moveListLocal.Count != 0) {
            for (int i = 0; i < moveListLocal.Count; i++)
            {
                var task = moveListLocal[i];
                if (task.Item1.transform.localPosition != task.Item2)
                {
                    task.Item1.transform.localPosition = Vector3.MoveTowards(task.Item1.transform.localPosition, task.Item2, task.Item4 * Time.deltaTime);
                    task.Item1.transform.localRotation = task.Item3;
                } else {
                    moveListLocal.RemoveAt(i);
                    i--;
                }
            }
        }
        if (moveListGlobal.Count != 0) {
            for (int i = 0; i < moveListGlobal.Count; i++)
            {
                var task = moveListGlobal[i];
                if (task.Item1.transform.position != task.Item2)
                {
                    task.Item1.transform.position = Vector3.MoveTowards(task.Item1.transform.position, task.Item2, task.Item4 * Time.deltaTime);
                    task.Item1.transform.rotation = task.Item3;
                } else {
                    moveListGlobal.RemoveAt(i);
                    i--;
                }
            }
        }
    }
    void Awake()
    {
        LoadChessProperties();
    }
#endif
}
public enum CardEffectsType {
    DOTOALL_ONCE = 0,
    FRIEND_ONLY_ONCE = 1,
    ENEMY_ONLY_ONCE = 2,
    FRIEND_INCREASE_ENEMY_REDUCE_ONCE = 3,
    DOTOALL_LASTING = 10,
    FRIEND_ONLY_LASTING = 11,
    ENEMY_ONLY_LASTING = 12,
    FRIEND_INCREASE_ENEMY_REDUCE_LASTING = 13
}
public enum ChessPosStatus {
    LEVEL_ONE_FRIEND = 1,
    LEVEL_TWO_FRIEND = 2,
    LEVEL_THREE_FRIEND = 3,
    EMPTY = 10,
    LEVEL_ONE_ENEMY = 11,
    LEVEL_TWO_ENEMY = 12,
    LEVEL_THREE_ENEMY = 13,
    OCCUPIED_FRIEND = 14,
    OCCUPIED_ENEMY = 15
}
public struct Int2D
{
    public int x;
    public int y;

    public Int2D(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}
