using System.Collections;
using System.Collections.Generic;

public struct Buff
{
    public int value;
    public Int2D source;
    public EffectScope scope;
    public PlayerType type;

    public Buff(Int2D source, int value, EffectScope scope, PlayerType type)
    {
        this.value = value;
        this.scope = scope;
        this.source = source;
        this.type = type;
    }

    public Buff(Buff buff)
    {
        value = buff.value;
        scope = buff.scope;
        source = buff.source;
        type = buff.type;
    }
}

public class BuffList: IEnumerable<Buff>
{
    PadGrid padGrid_;
    List<Buff> buffList_ = new List<Buff>();

    public IEnumerator<Buff> GetEnumerator() => buffList_.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public BuffList(PadGrid padGrid)
    {
        padGrid_ = padGrid;
    }

    public Buff this[int index]
    {
        get { return buffList_[index]; }
        set { buffList_[index] = value; }
    }

    public int Count
    {
        get { return buffList_.Count; }
    }

    public void Add(Buff buff)
    {
        buffList_.Add(buff);
    }

    public void Remove(Buff buff)
    {
        buffList_.Remove(buff);
    }

    public void Remove(Int2D src)
    {
        buffList_.RemoveAll(buff => buff.source == src);
    }

    public void RemoveAt(int index)
    {
        buffList_.RemoveAt(index);
    }

    public void Clear()
    {
        buffList_.Clear();
    }

    public int Compute(Func<Buff, bool> buffFilter = null)
    {
        if (padGrid_.Status != PosStatus.OCCUPIED_PLAYER && padGrid_.Status != PosStatus.OCCUPIED_RIVAL)
            return 0;

        int result = 0;
        PlayerType posOwner = padGrid_.Status == PosStatus.OCCUPIED_PLAYER
            ? PlayerType.PLAYER
            : PlayerType.RIVAL;

        foreach (var buff in buffList_)
        {
            bool validScope = buff.scope switch
            {
                EffectScope.DOTOALL => true,
                EffectScope.Self => true,
                EffectScope.FRIEND_ONLY => buff.type == posOwner,
                EffectScope.ENEMY_ONLY => buff.type != posOwner,
                _ => false
            };
            if (!validScope) continue;

            if (buffFilter != null && !buffFilter(buff))
                continue;

            result += buff.value;
        }
        return result;
    }
}