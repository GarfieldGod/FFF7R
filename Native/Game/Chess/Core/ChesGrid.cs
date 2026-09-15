namespace FFF7RCore {
    using Effect;

    public class PadGrid
    {
        public Int2D Position => pos_;
        public int Level => buffs_.Compute();
        public bool Empty => property_ == null;
        public bool OnBuffed => Level > property_.Level;
        public bool OnDeBuffed => Level < property_.Level;

        public bool NeverBuffed = true;
        public bool NeverDeBuffed = true;
        public bool NeverBuffedOrDeBuffed = true;
        public int LastPowerSnapshot = 0;
        public bool FirstPowerReach = false;

        private PosStatus posStatus_ = PosStatus.EMPTY;
        private ChessProperty property_ = null;

        private BuffList buffs_;
        private Int2D pos_ = new Int2D(-1, -1);

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
            property_ = padGrid.Chess;
            NeverBuffed = padGrid.NeverBuffed;
            NeverDeBuffed = padGrid.NeverDeBuffed;

            foreach (var buff in padGrid.BuffList)
            {
                buffs_.Add(new Buff(buff));
            }
        }

        public int BuffValue
        {
            get{
                return buffs_.Compute(b => b.source != pos_);
            }
        }

        public void AddBuff(Buff buff)
        {
            buffs_.Add(buff);
        }

        public void RemoveBuff(Int2D src)
        {
            buffs_.Remove(src);
        }

        public void Recover()
        {
            NeverBuffed = true;
            NeverDeBuffed = true;
            NeverBuffedOrDeBuffed = true;
            FirstPowerReach = false;
            LastPowerSnapshot = Level;

            property_ = null;
            posStatus_ = posStatus_ == PosStatus.OCCUPIED_PLAYER ? PosStatus.LEVEL_ONE_PLAYER : PosStatus.LEVEL_ONE_RIVAL;
            Log.TestLine($"Recover pos: {pos_} status: {(int)posStatus_} LastPowerSnapshot: {LastPowerSnapshot}");
        }

        public void Reset()
        {
            Recover();
            RemoveBuff(Position);
        }
    }
}