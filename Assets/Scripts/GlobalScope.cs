using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class GlobalScope
{
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
    public static void LoadChessProperties()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Json/ChessProperties.json");
        string ChessPropertiesData = System.IO.File.ReadAllText(path);
        ChessProperties = JsonConvert.DeserializeObject<List<ChessProperty>>(ChessPropertiesData);

        HashSet<string> ChessNames = new HashSet<string>();
        foreach (var chess in ChessProperties)
        {
            ChessNames.Add(chess.Name);
        }
        GlobalScope.chessNameSet = ChessNames;
    }

    public static ChessProperty GetChessProperty(string chessName)
    {
        foreach (var chess in ChessProperties) {
            if (chess.Name == chessName) {
                return chess;
            }
        }
        return null;
    }
}
