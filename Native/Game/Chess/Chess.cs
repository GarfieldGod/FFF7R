using System.Net.WebSockets;

public class Chess
{
    private ChessProperty property_;
    private string cardCode_;
    private string chessName_;
    private int level_;
    private int cost_;
    private Float3D chessPos_ = new Float3D(0, 0, 0);
    public Chess(ChessProperty property)
    {
        property_ = new ChessProperty(property);
        cardCode_ = property.CardCode;
        chessName_ = property.Name;
        level_ = property.Level;
        cost_ = property.Cost;
    }
    public Chess Clone()
    {
        return new Chess(this.property_);
    }
    public ChessProperty GetChessProperty() {
        return property_;
    }
    public void SetPos(Float3D chessPos) {
        chessPos_ = chessPos;
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
    public ChessPad() {}

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

    public void Copy(ChessPad chessPad) {
        if (chessPad == null) return;
        chessGridStatus_ = chessPad.GetChessGridStatus();
        chessStatus_ = chessPad.GetChessStatus();
    }
}