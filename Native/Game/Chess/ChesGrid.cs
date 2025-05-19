// PLAYED ONCE
// DEAD ONCE
// EVENT ONPLAYED
// BUFFED ONCE
// BUFFED
// WIN ONCE
//-------------------------------------------------------------------Event
public class PadGrid
{
    private ChessPosStatus posStatus_ = ChessPosStatus.EMPTY;
    private ChessPosStatus posStatusBackUp_ = ChessPosStatus.EMPTY;
    private ChessProperty property_ = null;
    private bool neverBuffed_ = true;
    private List<Buff> buffs_ = new List<Buff>();
    private string chessId_ = null;
    private Int2D pos_ = new Int2D(-1, -1);
    public PadGrid Reverse()
    {
        PadGrid newPadGrid = new PadGrid(this);
        newPadGrid.SetGridStatus(Utils.Reverse(newPadGrid.GetGridStatus()));
        newPadGrid.SetGridBackUp(Utils.Reverse(newPadGrid.GetGridBackUp()));
        List<Buff> newBuffs = new List<Buff>();
        foreach (var buff in newPadGrid.GetBuffList())
        {
            var newBuff = new Buff(buff);
            if (newBuff.scope == EffectScope.ENEMY_ONLY)
            {
                newBuff.scope = EffectScope.FRIEND_ONLY;
            }
            else if (newBuff.scope == EffectScope.FRIEND_ONLY)
            {
                newBuff.scope = EffectScope.ENEMY_ONLY;
            }
            if (newBuff.inputerType == InputerType.PLAYER)
            {
                newBuff.inputerType = InputerType.RIVAL;
            }
            else if (newBuff.inputerType == InputerType.RIVAL)
            {
                newBuff.inputerType = InputerType.PLAYER;
            }
            newBuffs.Add(newBuff);
        }
        newPadGrid.SetBuffList(newBuffs);
        return newPadGrid;
    }
    public PadGrid(Int2D pos)
    {
        pos_ = pos;
    }
    public PadGrid(PadGrid padGrid)
    {
        posStatus_ = padGrid.GetGridStatus();
        posStatusBackUp_ = padGrid.GetGridBackUp();
        property_ = padGrid.GetChess();
        neverBuffed_ = padGrid.GetBuffStatus();
        chessId_ = padGrid.GetID();
        buffs_ = new List<Buff>();
        foreach (var buff in padGrid.GetBuffList())
        {
            buffs_.Add(new Buff(buff));
        }
        pos_ = new Int2D(padGrid.GetPos().x, padGrid.GetPos().y);
        if (padGrid.Effcet != null)
        {
            Effcet += padGrid.Effcet;
        }
    }
    public bool Empty()
    {
        return property_ == null && chessId_ == null;
    }
    public void SetID(string id)
    {
        chessId_ = id;
    }
    public string GetID()
    {
        return chessId_;
    }
    public Int2D GetPos()
    {
        return pos_;
    }
    public ChessPosStatus GetGridStatus()
    {
        return posStatus_;
    }
    public void SetGridStatus(ChessPosStatus chessPosStatus)
    {
        posStatus_ = chessPosStatus;
    }
    public ChessProperty GetChess()
    {
        return property_;
    }
    public void SetChess(ChessProperty property)
    {
        property_ = property;
    }
    public bool GetBuffStatus()
    {
        if (property_ == null)
        {
            neverBuffed_ = true;
        }
        return neverBuffed_;
    }
    public void SetBuffStatus(bool neverBuffed)
    {
        neverBuffed_ = neverBuffed;
    }
    public int GetLevel()
    {
        // Log.TestLine(GetID() + " buffCount: " + buffs_.Count() + " Result: " + Buff.Compute(buffs_, posStatus_, new Int2D(-1, -1)), TextColor.PURPLE);
        // foreach (var buff in buffs_)
        // {
        //     Log.TestLine("buff.id: " + buff.id + " buff.inputerType: " + buff.inputerType + " buff.scope: " + buff.scope + " buff.value: " + buff.value);
        // }
        return Buff.Compute(buffs_, posStatus_, new Int2D(-1, -1));
    }
    public int GetLevelWithoutSelf()
    {
        // Log.TestLine(GetID() + " buffCount: " + buffs_.Count(), TextColor.PURPLE);
        return Buff.Compute(buffs_, posStatus_, pos_);
    }
    public int GetDebuffValue()
    {
        return Buff.ComputeBuffOrDebuff(buffs_, posStatus_, pos_, true);
    }
    public int GetBuffValue()
    {
        return Buff.ComputeBuffOrDebuff(buffs_, posStatus_, pos_, false);
    }
    public List<Buff> GetBuffList()
    {
        return buffs_;
    }
    public void SetBuffList(List<Buff> buffs)
    {
        buffs_ = buffs;
    }
    public ChessPosStatus GetGridBackUp()
    {
        return posStatusBackUp_;
    }
    public void SetGridBackUp(ChessPosStatus chessPosStatus)
    {
        posStatusBackUp_ = chessPosStatus;
    }
    public void AddBuff(Buff buff)
    {
        buffs_.Add(buff);
    }
    public void Reset()
    {
        Log.TestLine("Reset: " + GetID() + " Level: " + GetLevel().ToString() + " posStatusBackUp_: " + posStatusBackUp_, TextColor.PURPLE);
        posStatus_ = posStatusBackUp_;
        neverBuffed_ = true;
        property_ = null;
        chessId_ = null;
    }
    public void AddDeadEffect(Input input)
    {

    }

    public delegate void EffcetHandler(PadGrid sender, int value);
    public event EffcetHandler Effcet;
    public void SendEffcet(string info)
    {
    }
    protected virtual void OnEffcet(int value)
    {
        Effcet?.Invoke(this, value);
    }
}

public struct Buff
{
    public string id;
    public Int2D source;
    public int value;
    public EffectScope scope;
    public InputerType inputerType;
    public Buff(Int2D source, string id, int value, EffectScope scope, InputerType inputerType)
    {
        this.id = id;
        this.value = value;
        this.scope = scope;
        this.inputerType = inputerType;
        this.source = new Int2D(source.x, source.y);
    }
    public Buff(Buff buff)
    {
        this.id = buff.id;
        this.value = buff.value;
        this.scope = buff.scope;
        this.inputerType = buff.inputerType;
        this.source = new Int2D(buff.source.x, buff.source.y);
    }
    public static int Compute(List<Buff> list, ChessPosStatus posStatus, Int2D source)
    {
        int result = 0;
        foreach (var buff in list)
        {
            if (buff.source == source) continue; 
            // Log.TestLine(inputerType + " buff.scope: " + buff.scope + " buff.inputerType: " + buff.inputerType);
            if (posStatus == ChessPosStatus.OCCUPIED_FRIEND)
            {
                if (buff.scope == EffectScope.DOTOALL)
                {
                    result += buff.value;
                }
                else if (buff.scope == EffectScope.ENEMY_ONLY && buff.inputerType == InputerType.RIVAL)
                {
                    result += buff.value;
                }
                else if (buff.scope == EffectScope.FRIEND_ONLY && buff.inputerType == InputerType.PLAYER)
                {
                    result += buff.value;
                }
            }
            else if (posStatus == ChessPosStatus.OCCUPIED_ENEMY)
            {
                if (buff.scope == EffectScope.DOTOALL)
                {
                    result += buff.value;
                }
                else if (buff.scope == EffectScope.ENEMY_ONLY && buff.inputerType == InputerType.PLAYER)
                {
                    result += buff.value;
                }
                else if (buff.scope == EffectScope.FRIEND_ONLY && buff.inputerType == InputerType.RIVAL)
                {
                    result += buff.value;
                }
            }
        }
        return result;
    }
    public static int ComputeBuffOrDebuff(List<Buff> list, ChessPosStatus posStatus, Int2D source, bool deBuff)
    {
        int result = 0;
        foreach (var buff in list)
        {
            if (deBuff ? buff.value > 0 : buff.value < 0) continue;
            if (buff.source == source) continue; 
            if (posStatus == ChessPosStatus.OCCUPIED_FRIEND)
            {
                if (buff.scope == EffectScope.DOTOALL)
                {
                    result += buff.value;
                }
                else if (buff.scope == EffectScope.ENEMY_ONLY && buff.inputerType == InputerType.RIVAL)
                {
                    result += buff.value;
                }
                else if (buff.scope == EffectScope.FRIEND_ONLY && buff.inputerType == InputerType.PLAYER)
                {
                    result += buff.value;
                }
            }
            else if (posStatus == ChessPosStatus.OCCUPIED_ENEMY)
            {
                if (buff.scope == EffectScope.DOTOALL)
                {
                    result += buff.value;
                }
                else if (buff.scope == EffectScope.ENEMY_ONLY && buff.inputerType == InputerType.PLAYER)
                {
                    result += buff.value;
                }
                else if (buff.scope == EffectScope.FRIEND_ONLY && buff.inputerType == InputerType.RIVAL)
                {
                    result += buff.value;
                }
            }
        }
        return result;
    }
}