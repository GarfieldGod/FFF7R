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
    public PadGrid() {}
    public PadGrid(PadGrid padGrid)
    {
        posStatus_ = padGrid.GetGridStatus();
        posStatusBackUp_ = padGrid.GetGridBackUp();
        property_ = padGrid.GetChess();
        neverBuffed_ = padGrid.GetBuffStatus();
        buffs_ = padGrid.GetBuffList();
        chessId_ = padGrid.GetID();
    }
    public void SetID(string id)
    {
        chessId_ = id;
    }
    public string GetID()
    {
        return chessId_;
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
    public static int Compute(List<Buff> list, InputerType inputerType)
    {
        int result = 0;
        foreach (var buff in list)
        {
            // Log.TestLine(inputerType + " buff.scope: " + buff.scope + " buff.inputerType: " + buff.inputerType);
            if (inputerType == InputerType.PLAYER)
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
            else
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