using Effect;
// PLAYED ONCE
// DEAD ONCE
// EVENT ONPLAYED
// BUFFED ONCE
// BUFFED
// WIN ONCE
//-------------------------------------------------------------------Event
public class PadGrid
{
    public Int2D Position => pos_;
    public int Level => buffs_.Compute();
    public bool Empty => property_ == null;

    private PosStatus posStatus_ = PosStatus.EMPTY;
    private PosStatus posStatusBackUp_ = PosStatus.EMPTY;
    private ChessProperty property_ = null;
    private BuffList buffs_;
    private Int2D pos_ = new Int2D(-1, -1);

    private bool neverBuffed_ = true;
    private bool neverDeBuffed_ = true;

    public PosStatus Status
    {
        get => posStatus_;
        set => posStatus_ = value;
    }

    public ChessProperty Chess
    {
        get => property_;
        set
        {
            if (property_ == null && value != null)
            {
                posStatusBackUp_ = posStatus_;
            }
            property_ = value;
        }
    }

    public BuffList BuffList
    {
        get => buffs_;
        set => buffs_ = value;
    }

    public PadGrid(Int2D pos)
    {
        pos_ = pos;
        buffs_ = new BuffList(this);
    }

    public PadGrid(PadGrid padGrid)
    {
        pos_ = padGrid.Position;
        buffs_ = new BuffList(this);

        posStatus_ = padGrid.Status;
        posStatusBackUp_ = padGrid.posStatusBackUp_;
        property_ = padGrid.Chess;
        neverBuffed_ = padGrid.NeverBuffed;
        neverDeBuffed_ = padGrid.NeverDeBuffed;

        foreach (var buff in padGrid.BuffList)
        {
            buffs_.Add(new Buff(buff));
        }
    }

    public bool NeverBuffed
    {
        get{
            if (property_ == null)
            {
                neverBuffed_ = true;
            }
            return neverBuffed_;
        }
        set
        {
            neverBuffed_ = value;
        }
    }

    public bool NeverDeBuffed
    {
        get {
            if (property_ == null)
            {
                neverDeBuffed_ = true;
            }
            return neverDeBuffed_;
        }
        set
        {
            neverDeBuffed_ = value;
        }
    }

    public int BuffValue
    {
        get{
            return buffs_.Compute(b => b.value > 0 && b.source != pos_);
        }
    }

    public int DeBuffValue
    {
        get{
            return buffs_.Compute(b => b.value < 0 && b.source != pos_);
        }
    }

    public void AddBuff(Buff buff)
    {
        buffs_.Add(buff);
    }

    public void Reset()
    {
        Log.TestLine("Reset: Level: " + Level.ToString() + " posStatusBackUp_: " + posStatusBackUp_ + " Pos: " + pos_, TextColor.PURPLE);
        Log.TestLine("  BuffCount before Reset: " + buffs_.Count, TextColor.PURPLE);
        foreach (var b in buffs_)
        {
            Log.TestLine("    buff: value=" + b.value + " owner=" + b.owner + " source=" + b.source + " dstType=" + b.dstType, TextColor.PURPLE);
        }
        neverBuffed_ = true;
        neverDeBuffed_ = true;
        property_ = null;
        posStatus_ = posStatusBackUp_;
    }

    public void ChessDead()
    {
        Reset();
    }
}