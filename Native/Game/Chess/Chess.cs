using System.Net.WebSockets;

public class Chess
{
    public static readonly List<List<int>> chessLevelMap_ = [];
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
    // public Chess Clone()
    // {
    //     return new Chess(this.property_);
    // }
    public ChessProperty GetChessProperty() {
        return property_;
    }
    public void SetPos(Float3D chessPos) {
        chessPos_ = chessPos;
    }

    private bool neverBuffed = true;
    public void Buff(int num) {
        if (neverBuffed && property_.CardEffectConfig.condition == EffectCondition.Frist_Buffed) {

        }
        neverBuffed = false;
    }
    // PLAYED ONCE
    // DEAD ONCE
    // EVENT ONPLAYED
    // BUFFED ONCE
    // BUFFED
    // WIN ONCE
    //-------------------------------------------------------------------Event
    public void AddDeadEffect(Input input)
    {
        
    }
    public delegate void EffcetHandler(Chess sender, EventArgs e);
    public event EffcetHandler Effcet;
    public void DoEffcet()
    {
        Log.TestLine("DoDeadEffcet");
        OnEffcet(EventArgs.Empty);
    }
    protected virtual void OnEffcet(EventArgs e) {
        Effcet?.Invoke(this, e);
    }
    public void OnRecvEffcet(Chess sender, EventArgs e) {
        if(sender == this) return;
    }
}

public struct Buff
{
    string id = "";
    int value = 0;
    Buff(int value, string id)
    {
        this.id = id;
        this.value = value;
    }
}

public class ChessPad {
    private List<List<int>> chessGridStatus_ = new List<List<int>>{};
    private List<List<int>> chessLevelStatus_ = new List<List<int>>{};
    private List<List<Chess>> chessStatus_ = new List<List<Chess>>{};
    private readonly List<List<List<Buff>>> buffMap_ = new List<List<List<Buff>>>{};
    public ChessPad() {}
    public ChessPad(
        List<List<int>> chessGridStatus,
        List<List<int>> chessLevelStatus,
        List<List<Chess>> chessStatus) {
        chessGridStatus_ = chessGridStatus;
        chessLevelStatus_ = chessLevelStatus;
        chessStatus_ = chessStatus;
    }

    public List<List<int>> GetChessGridStatus() {
        return chessGridStatus_;
    }

    public List<List<Chess>> GetChessStatus() {
        return chessStatus_;
    }
    
    public List<List<int>> GetChessLevelStatus() {
        return chessLevelStatus_;
    }

    public void Copy(ChessPad chessPad)
    {
        if (chessPad == null) return;
        chessGridStatus_ = chessPad.GetChessGridStatus();
        chessLevelStatus_ = chessPad.GetChessLevelStatus();
        chessStatus_ = chessPad.GetChessStatus();
    }
}