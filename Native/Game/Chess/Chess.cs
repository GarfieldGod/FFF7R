public class Chess
{
    public ChessProperty property_;
    public string cardCode_;
    public string chessName_;
    public int level_;
    public int cost_;
    public Float3D chessPos = new Float3D(0, 0, 0);
    public Chess(ChessProperty property)
    {
        property_ = property;
        cardCode_ = property.CardCode;
        chessName_ = property.Name;
        level_ = property.Level;
        cost_ = property.Cost;
    }
    public ChessProperty GetChessProperty() {
        return property_;
    }
}

public struct ChessPadInfo {
    public List<List<List<int>>> chessPadStatus;
    public Dictionary<Int2D, ChessProperty> delayEffectsList;
    public ChessPadInfo(List<List<List<int>>> chessPadStatus, Dictionary<Int2D, ChessProperty> delayEffectsList = null){
        this.chessPadStatus = chessPadStatus;
        this.delayEffectsList = delayEffectsList;
    }
}

public class ChessPad {
    private List<List<int>> chessGridStatus_ = new List<List<int>>{};
    private List<List<Chess>> chessStatus_ = new List<List<Chess>>{};

    public ChessPad(List<List<int>> chessGridStatus, List<List<Chess>> chessStatus) {
        chessGridStatus_ = chessGridStatus;
        chessStatus_ = chessStatus;
    }

    public List<List<int>> GetChessGridStatus() {
        return chessGridStatus_;
    }

    public List<List<Chess>> GetChessStatus() {
        return chessStatus_;
    }

    public void AddInput(Input input, ChessPad chessPad) {
    }
    public void RestoreInput() {

    }
    public void CommitInput() {
    }
}