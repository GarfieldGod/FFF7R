namespace FFF7RCore {
    using Effect;

    public class PadGrid
    {
        public Int2D Position { get; private set; } = new Int2D(-1, -1);
        public bool Empty => property_ == null;
        public int LastPowerSnapshot { get; private set; }

        public BuffList BuffList {get; private set;}
        public int Level => BuffList.Compute();
        public int BuffValue => BuffList.Compute(b => b.value > 0 && b.source != Position);
        public int DeBuffValue => BuffList.Compute(b => b.value < 0 && b.source != Position);
        public bool OnBuffed => Level > property_.Level;
        public bool OnDeBuffed => Level < property_.Level;

        // flags
        public bool NeverBuffed = true;
        public bool NeverDeBuffed = true;
        public bool FirstPowerReach = false;

        public PosStatus Status = PosStatus.EMPTY;
        private PosStatus posStatusBackUp_ = PosStatus.EMPTY;
        private ChessProperty property_ = null;

        public ChessProperty Chess
        {
            get => property_;
            set
            {
                if (property_ == null && value != null) posStatusBackUp_ = Status;
                if (value == null)
                {
                    NeverBuffed = true;
                    NeverDeBuffed = true;
                }
                property_ = value;
            }
        }

        public PadGrid(Int2D pos)
        {
            Position = pos;
            BuffList = new BuffList(this);
        }

        public PadGrid(PadGrid padGrid) : this(padGrid.Position)
        {
            CopyStateFrom(padGrid);
        }

        public bool CopyFrom(PadGrid padGrid)
        {
            if (padGrid == null || ReferenceEquals(padGrid, this)) return false;
            CopyStateFrom(padGrid);
            return true;
        }

        private void CopyStateFrom(PadGrid src)
        {
            BuffList.CopyFrom(src.BuffList);
            Status = src.Status;
            posStatusBackUp_ = src.posStatusBackUp_;
            property_ = src.Chess;
            NeverBuffed = src.NeverBuffed;
            NeverDeBuffed = src.NeverDeBuffed;
            FirstPowerReach = src.FirstPowerReach;
            LastPowerSnapshot = src.LastPowerSnapshot;
        }

        public void AddBuff(Buff buff)
        {
            BuffList.Add(buff);
        }

        public void RemoveBuff(Int2D src)
        {
            BuffList.Remove(src);
        }

        public void Recover()
        {
            NeverBuffed = true;
            NeverDeBuffed = true;
            property_ = null;
            LastPowerSnapshot = Level;
            Status = Status == PosStatus.OCCUPIED_PLAYER ? PosStatus.LEVEL_ONE_PLAYER : PosStatus.LEVEL_ONE_RIVAL;
            Log.TestLine($"Recover pos: {Position} status: {(int)Status} LastPowerSnapshot: {LastPowerSnapshot}");
        }

        public void Reset()
        {
            Recover();
            RemoveBuff(Position);
        }
    }
}