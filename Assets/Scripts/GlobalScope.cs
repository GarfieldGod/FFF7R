// #define UNITY_ENGINE
using System.Collections.Generic;
using System.IO;
using System;
using Newtonsoft.Json;
using Unity.VisualScripting;


#if UNITY_ENGINE
using UnityEngine;
using System.Linq;
public class GlobalScope : MonoBehaviour
#else 
public class GlobalScope
#endif
{
    void Awake()
    {
        LoadGlabalScopeStaticGameObject();
        GenerateChessGird(10.5f, 8.5f);
        LoadChessProperties();
    }
    void Update() {
    }
// -----------------------------------------------------------------------------------ChessProperties
    private static readonly string chessPropertiesJsonPath = "Json/ChessProperties.json";
    public static List<List<List<int>>> chessGridStatus;
    public static readonly List<List<string>> chessGridNameList_ = new List<List<string>>{
        new List<string>{ "upLine0", "upLine1", "upLine2", "upLine3", "upLine4" },
        new List<string>{ "midLine0", "midLine1", "midLine2", "midLine3", "midLine4" },
        new List<string>{ "downLine0", "downLine1", "downLine2", "downLine3", "downLine4" }
    };
    public static readonly HashSet<string> chessPositionNameSet = new HashSet<string>
    {
        "upLine0", "upLine1", "upLine2", "upLine3", "upLine4",
        "midLine0", "midLine1", "midLine2", "midLine3", "midLine4",
        "downLine0", "downLine1", "downLine2", "downLine3", "downLine4"
    };
    public static HashSet<string> chessNameSet = new HashSet<string>{};
    private static List<ChessProperty> ChessProperties_;
    public static ChessProperty GetChessProperty(string chessCode)
    {
        foreach (var chess in ChessProperties_) {
            if (chess.CardCode == chessCode) {
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
        ChessProperties_ = JsonConvert.DeserializeObject<List<ChessProperty>>(ChessPropertiesData);

        HashSet<string> ChessNames = new HashSet<string>();
        foreach (var chess in ChessProperties_)
        {
            ChessNames.Add(chess.CardCode);
        }
        chessNameSet = ChessNames;
    }
#if UNITY_ENGINE
// -----------------------------------------------------------------------------------Load GlabalScope Static GameObjects
    public static List<Tuple<GameObject, GameObject>> GirdScoreCounters_ = new List<Tuple<GameObject, GameObject>>();
    void GenerateChessGird(float offsetX, float offsetY) {
        Int2D centerPos = new Int2D((chessGridNameList_.Count)/2, (chessGridNameList_[0].Count)/2);
        for(int i = 0; i < chessGridNameList_.Count ; i++) {
            int j = 0;
            for(; j < chessGridNameList_[0].Count ; j++) {
                GameObject chessGrid = Instantiate(chessGirdPrefab_static_, chessPad_.transform);
                chessGrid.name = chessGridNameList_[i][j];
                int posX = i - centerPos.x;
                int posY = centerPos.y - j;
                chessGrid.transform.localPosition = new Vector3(posY * offsetX, 1, posX * offsetY);
            }
            GameObject scoreCounterPlayer = Instantiate(chessGirdScoreCounterPrefab_static_, chessPad_.transform);
            GameObject scoreCounterRival = Instantiate(chessGirdScoreCounterPrefab_static_, chessPad_.transform);
            scoreCounterPlayer.transform.localPosition = new Vector3((chessGridNameList_[0].Count/2 + 1) * offsetX, 1, (i - centerPos.x) * offsetY);
            scoreCounterRival.transform.localPosition = new Vector3(-(chessGridNameList_[0].Count/2 + 1) * offsetX, 1, (i - centerPos.x) * offsetY);
            scoreCounterPlayer.name = "scoreCounterPlayer" + i.ToString();
            scoreCounterRival.name = "scoreCounterRival" + i.ToString();
            GirdScoreCounters_.Add(new Tuple<GameObject, GameObject>(scoreCounterPlayer, scoreCounterRival));
            Log.test("scoreCounterPlayer" + i.ToString() + " : " + "scoreCounterRival" + i.ToString());
        }
    }
    public GameObject chessSelector_;
    public GameObject chessPad_;
    public GameObject chessPrefab_;
    public GameObject chessModelPrefab_;
    public GameObject chessGirdPrefab_;
    public GameObject chessGirdScoreCounterPrefab_;
    public static GameObject chessSelector_static_;
    public static GameObject chessPad_static_;
    public static GameObject chessPrefab_static_;
    public static GameObject chessModelPrefab_static_;
    public static GameObject chessGirdPrefab_static_;
    public static GameObject chessGirdScoreCounterPrefab_static_;
    private void LoadGlabalScopeStaticGameObject() {
        chessSelector_static_ = chessSelector_;
        chessPad_static_ = chessPad_;
        chessPrefab_static_ = chessPrefab_;
        chessModelPrefab_static_ = chessModelPrefab_;
        chessGirdPrefab_static_ = chessGirdPrefab_;
        chessGirdScoreCounterPrefab_static_ = chessGirdScoreCounterPrefab_;
    }
    private static Dictionary<Int2D, GameObject> ChessGridMap_ = new Dictionary<Int2D, GameObject>();
    public static Int2D InitGlobalScopeChessGridMap(GameObject chessGrid) {
        Int2D chessGridPos = new Int2D();
        for(int i = 0; i < chessGridNameList_.Count; i++) {
            for(int j = 0; j < chessGridNameList_[0].Count; j++) {
                if(chessGrid.name == chessGridNameList_[i][j]) {
                    chessGridPos = new Int2D(i, j);
                    ChessGridMap_.Add(chessGridPos, chessGrid);
                    return chessGridPos;
                }
            }
        }
        return chessGridPos;
    }
    public static List<GameObject> GetAllChessGrid() {
        List<GameObject> result = new List<GameObject>();
        foreach(var pair in ChessGridMap_) {
            result.Add(pair.Value);
        }
        return result;
    }
    public static GameObject GetChessGridObjectByChessGridPos(Int2D chessGridPos) {
        return ChessGridMap_[chessGridPos];
    }
// -----------------------------------------------------------------------------------Functions For All
    public static List<List<List<int>>> DeepCopy3DList(List<List<List<int>>> original) {
        List<List<List<int>>> result = original
        .Select(outerList => outerList
            .Select(innerList => new List<int>(innerList))
            .ToList())
        .ToList();
        return result;
    }
    public static List<List<int>> DeepCopy2DList(List<List<int>> original) {
        List<List<int>> result = original.Select(innerList => new List<int>(innerList)).ToList();
        return result;
    }
#endif
}
// -----------------------------------------------------------------------------------Enums For All
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
// -----------------------------------------------------------------------------------Structs For All
public struct SingleGameConfig {
    public int rivalLevel;
    public List<string> playerChessPool;
    public List<string> rivalChessPool;
    public SingleGameConfig(int rivalLevel, List<string> playerChessPool, List<string> rivalChessPool) {
        this.rivalLevel = rivalLevel;
        this.playerChessPool = playerChessPool;
        this.rivalChessPool = rivalChessPool;
    }
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

    public static bool operator ==(Int2D a, Int2D b)
    {
        return a.x == b.x && a.y == b.y;
    }

    public static bool operator !=(Int2D a, Int2D b)
    {
        return !(a == b);
    }

    public override bool Equals(object obj)
    {
        if (obj is Int2D)
        {
            Int2D other = (Int2D)obj;
            return this == other;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return x.GetHashCode() ^ y.GetHashCode();
    }
}

public class ChessProperty
{
    public string CardCode;
    public string Name;
    public int Level;
    public int Cost;
    public List<List<int>> PosEffects;
    public Tuple<CardEffectsType, List<List<int>>> CardEffects;
    public HashSet<string> SpecialEffects;
}
