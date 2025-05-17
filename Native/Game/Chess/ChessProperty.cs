using Newtonsoft.Json;

public class ChessProperty {
    public string CardCode;
    public string Name;
    public int Level;
    public int Cost;
    public List<List<int>> PosEffects;
    public EffectConfig CardEffectConfig;
    public Tuple<EffectScope, EffectCondition, List<List<int>>> CardEffects;
    public string Description;
    public ChessProperty() {}
    public ChessProperty(ChessProperty chessProperty){
        CardCode = chessProperty.CardCode;
        Name = chessProperty.Name;
        Level = chessProperty.Level;
        Cost = chessProperty.Cost;
        PosEffects = Utils.DeepCopy2DList(chessProperty.PosEffects);
        CardEffects = new Tuple<EffectScope, EffectCondition, List<List<int>>>(
            chessProperty.CardEffects.Item1,chessProperty.CardEffects.Item2, Utils.DeepCopy2DList(chessProperty.CardEffects.Item3));
        Description = chessProperty.Description;
    }
}

public static class Property {
    private static readonly string chessPropertiesJsonPath = "Json/ChessProperties.json";
    private static List<ChessProperty> ChessProperties_ = new List<ChessProperty>{};
    private static HashSet<string> chessNameSet_ = new HashSet<string>{};
    public static ChessProperty GetChessProperty(string cardCode)
    {
        foreach (var chess in ChessProperties_) {
            if (chess.CardCode == cardCode) {
                return new ChessProperty(chess);
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
            ChessNames.Add(chess.Name);
        }
        chessNameSet_ = ChessNames;
    }
}