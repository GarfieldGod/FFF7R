using System.Data.Common;
using System.Dynamic;
using System.Net.WebSockets;
using System.Reflection.Metadata.Ecma335;

public class Chess
{
    private ChessProperty property_;
    private string cardCode_;
    private string chessName_;
    private int level_;
    private int cost_;
    private string id_;
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
    public void SetID(string id) {
        id_ = id;
    }
    public string GetID() {
        return id_;
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
    public string id;
    public int value;
    public EffectScope scope;
    public InputerType inputerType;
    public Buff(string id, int value, EffectScope scope, InputerType inputerType)
    {
        this.id = id;
        this.value = value;
        this.scope = scope;
        this.inputerType = inputerType;
    }
    public static int ComputeAll(List<Buff> list) {
        int result = 0;
        foreach(var buff in list) {
            result += buff.value;
        }
        return result;
    }
    public static int Compute(List<Buff> list, InputerType inputerType) {
        int result = 0;
        foreach(var buff in list) {
            if(buff.inputerType == inputerType) result += buff.value;
        }
        return result;
    }
}

public class ChessPad {
    private List<List<int>> chessGridStatus_ = new List<List<int>>{};
    private List<List<Chess>> chessStatus_ = new List<List<Chess>>{};
    private List<List<int>> chessLevelStatus_ = new List<List<int>>{};
    private List<List<List<Buff>>> stayBuffMap_ = new List<List<List<Buff>>>{};
    public ChessPad() {}
    public ChessPad(
        List<List<int>> chessGridStatus, List<List<Chess>> chessStatus,
        List<List<int>> chessLevelStatus, List<List<List<Buff>>> stayBuffMap) {
        chessGridStatus_ = chessGridStatus;
        chessLevelStatus_ = chessLevelStatus;
        chessStatus_ = chessStatus;
        stayBuffMap_ = stayBuffMap;
    }
    public Int2D GetSize() {
        return new Int2D(chessGridStatus_.Count, chessGridStatus_[0].Count);
    }
    public ChessPad DeepCopy() {
        return new ChessPad(
            Utils.DeepCopy2DList(chessGridStatus_),
            Utils.DeepCopy(chessStatus_),
            Utils.DeepCopy2DList(chessLevelStatus_),
            Utils.DeepCopy(stayBuffMap_)
        );
    }
    public List<List<int>> GetChessGridStatus() {
        return chessGridStatus_;
    }
    public void SetChessGridStatus(List<List<int>> src) {
        chessGridStatus_ = src;
    }

    public List<List<Chess>> GetChessStatus() {
        return chessStatus_;
    }
    public void SetChessStatus(List<List<Chess>> src) {
        chessStatus_ = src;
    }

    public List<List<int>> GetChessLevelStatus() {
        return chessLevelStatus_;
    }
    public void SetChessLevelStatus(List<List<int>> src) {
        chessLevelStatus_ = src;
    }

    public List<List<List<Buff>>> GetStayBuffMap() {
        return stayBuffMap_;
    }

    public bool AddStayBuff(Int2D pos, Buff buff) {
        stayBuffMap_[pos.x][pos.y].Add(buff);
        return true;
    }

    public bool RemoveStayBuff(string id) {
        for (int x = 0; x < stayBuffMap_.Count; x++) {
            var line = stayBuffMap_[x];
            for (int y = 0; y < line.Count; y++) {
                var buffs = line[y];
                for (int z = 0; z < buffs.Count; z++)
                {
                    if (buffs[z].id == id) {
                        buffs.Remove(buffs[z]);
                    }
                }
            }
        }
        return true;
    }

    public void Copy(ChessPad chessPad)
    {
        if (chessPad == null) return;
        chessGridStatus_ = chessPad.GetChessGridStatus();
        chessLevelStatus_ = chessPad.GetChessLevelStatus();
        chessStatus_ = chessPad.GetChessStatus();
    }
    public void InitStandard() {
        chessGridStatus_ = new List<List<int>>{
            new List<int> { 1, 10, 10, 10, 11 },
            new List<int> { 1, 10, 10, 10, 11 },
            new List<int> { 1, 10, 10, 10, 11 }
        };
        chessStatus_ = new List<List<Chess>>{
            new List<Chess> { null, null, null, null, null },
            new List<Chess> { null, null, null, null, null },
            new List<Chess> { null, null, null, null, null }
        };
        chessLevelStatus_ = new List<List<int>>{
            new List<int> { 0, 0, 0, 0, 0 },
            new List<int> { 0, 0, 0, 0, 0 },
            new List<int> { 0, 0, 0, 0, 0 }
        };
        stayBuffMap_ = new List<List<List<Buff>>>{
            new List<List<Buff>> { new List<Buff>{}, new List<Buff>{}, new List<Buff>{}, new List<Buff>{}, new List<Buff>{} },
            new List<List<Buff>> { new List<Buff>{}, new List<Buff>{}, new List<Buff>{}, new List<Buff>{}, new List<Buff>{} },
            new List<List<Buff>> { new List<Buff>{}, new List<Buff>{}, new List<Buff>{}, new List<Buff>{}, new List<Buff>{} }
        };
    }
}