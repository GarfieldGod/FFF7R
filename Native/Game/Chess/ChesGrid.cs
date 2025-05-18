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
        foreach (var buff in padGrid.GetBuffList()) {
            buffs_.Add(new Buff(buff));
        }
        pos_ = new Int2D(padGrid.GetPos().x, padGrid.GetPos().y);
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
        Log.TestLine(GetID() + " buffCount: " + buffs_.Count(), TextColor.PURPLE);
        return Buff.Compute(buffs_, posStatus_);
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
        // if (!Empty() && GetLevel() <= 0)
        // { 
        //     Log.TestLine(GetChess().Name + " Dead Level: " + GetLevel().ToString() + "\nstatus:" + GetGridStatus(), TextColor.PURPLE);
        //     Reset();
        // }
        if (property_ != null)
        {
            if (neverBuffed_)
            {
                // FristBuffed(Int2D pos, buff.value);
                neverBuffed_ = false;
            }
        }
        else
        {
            neverBuffed_ = true;
        }

    }
    // public void FristBuffed(Int2D pos, int recvValue)
    // {
    //     if (property_.CardEffects.Item2 == EffectCondition.Frist_Buffed && recvValue > 0)
    //     {
    //         Input input = new Input(pos, property_);
    //         Inputer.DoCardEffcet(input, this, posStatus_ == ChessPosStatus.OCCUPIED_FRIEND ? InputerType.PLAYER : InputerType.RIVAL);
    //     }
    //     else if (chessProperty.CardEffects.Item2 == EffectCondition.Frist_Debuffed && recvValue < 0)
    //     {
    //         Log.TestLine("Frist_Debuffed", TextColor.PURPLE, true);
    //         Input input = new Input(pos, property_);
    //         Inputer.DoCardEffcet(input, this, posStatus_ == ChessPosStatus.OCCUPIED_FRIEND ? InputerType.PLAYER : InputerType.RIVAL);
    //     }
    // }
    public void Reset()
    {
        posStatus_ = posStatusBackUp_;
        neverBuffed_ = true;
        property_ = null;
        chessId_ = null;
    }
    public void AddDeadEffect(Input input)
    {

    }
    public delegate void EffcetHandler(PadGrid sender, EventArgs e);
    public event EffcetHandler Effcet;
    public void DoEffcet()
    {
        Log.TestLine("DoDeadEffcet");
        OnEffcet(EventArgs.Empty);
    }
    protected virtual void OnEffcet(EventArgs e)
    {
        Effcet?.Invoke(this, e);
    }
    public void OnRecvEffcet(PadGrid sender, EventArgs e)
    {
        if (sender == this) return;
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
    public Buff(Buff buff)
    {
        this.id = buff.id;
        this.value = buff.value;
        this.scope = buff.scope;
        this.inputerType = buff.inputerType;
    }
    public static int Compute(List<Buff> list, ChessPosStatus posStatus)
    {
        int result = 0;
        foreach (var buff in list)
        {
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
}