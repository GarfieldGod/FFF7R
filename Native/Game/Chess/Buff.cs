namespace FFF7RCore {
    using System.Collections;
    using System.Collections.Generic;
    using Effect;

    public struct Buff
    {
        public int value;
        public Int2D owner;       // 抽有者位置，用于卡牌死亡时RemoveBuffs清理的标识（一次性buff绑定到被施加者位置）
        public Int2D source;      // 真正的施加者位置（用于判断buff是否来自其他卡牌）
        public EffectTarget dstType;    // buff作用对象类型
        public PlayerType srcType;      // buff施加者类型

        public Buff(Int2D owner, Int2D source, int value, EffectTarget dstType, PlayerType srcType)
        {
            this.owner = owner;
            this.source = source;
            this.value = value;
            this.dstType = dstType;
            this.srcType = srcType;
        }

        public Buff(Buff buff)
        {
            owner = buff.owner;
            source = buff.source;
            value = buff.value;
            dstType = buff.dstType;
            srcType = buff.srcType;
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
            buffList_.RemoveAll(buff => buff.owner == src);
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
                bool validScope = buff.dstType switch
                {
                    EffectTarget.FriendAndEnemy => true,
                    EffectTarget.Self => true,
                    EffectTarget.FriendOnly => buff.srcType == posOwner,
                    EffectTarget.EnemyOnly => buff.srcType != posOwner,
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
}