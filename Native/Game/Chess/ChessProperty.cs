using Effect;

public class ChessProperty {
    public string CardCode;
    public string Name;
    public int Level;
    public int Cost;
    public List<List<int>> PosEffect;
    public CardEffectEntry CardEffect;
    public string Description;

    public Dictionary<Int2D, int> PosOffsetDict;
    public Dictionary<Int2D, int> CardOffsetDict;
    public ChessProperty() {}
    public ChessProperty(ChessProperty chessProperty){
        CardCode = chessProperty.CardCode;
        Name = chessProperty.Name;
        Level = chessProperty.Level;
        Cost = chessProperty.Cost;
        PosEffect = chessProperty.PosEffect;
        CardEffect = chessProperty.CardEffect;
        Description = chessProperty.Description;

        PosOffsetDict = chessProperty.PosOffsetDict;
        CardOffsetDict = chessProperty.CardOffsetDict;
    }
    public void Init()
    {
        if (PosEffect != null)
        {
            var posOffsetList = EffectsParser.ParseEffectsInRelative(PosEffect);
            PosOffsetDict = new Dictionary<Int2D, int>(posOffsetList);
        }

        if (CardEffect != null && CardEffect.Scope != null)
        {
            var cardOffsetList = EffectsParser.ParseEffectsInRelative(CardEffect.Scope);
            CardOffsetDict = new Dictionary<Int2D, int>(cardOffsetList);
        }
    }
}