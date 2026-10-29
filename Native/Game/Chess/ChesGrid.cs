namespace FFF7RCore {
    using Effect;

    public class PadGrid
    {
        public Int2D Position => pos_;
        public int Level => buffs_.Compute();
        public bool Empty => property_ == null;
        public int LastPowerSnapshot => deathPowerSnapshot_;
        public bool OnBuffed => Level > property_.Level;
        public bool OnDeBuffed => Level < property_.Level;
        public bool FirstPowerReach = false;

        private PosStatus posStatus_ = PosStatus.EMPTY;
        private PosStatus posStatusBackUp_ = PosStatus.EMPTY;
        private ChessProperty property_ = null;
        private BuffList buffs_;
        private Int2D pos_ = new Int2D(-1, -1);
        private int deathPowerSnapshot_ = 0;

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

            FirstPowerReach = padGrid.FirstPowerReach;
            deathPowerSnapshot_ = padGrid.deathPowerSnapshot_;

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

        public void RemoveBuff(Int2D src)
        {
            buffs_.Remove(src);
        }

        public void Recover()
        {
            neverBuffed_ = true;
            neverDeBuffed_ = true;
            property_ = null;
            deathPowerSnapshot_ = Level;
            posStatus_ = posStatus_ == PosStatus.OCCUPIED_PLAYER ? PosStatus.LEVEL_ONE_PLAYER : PosStatus.LEVEL_ONE_RIVAL;
            Log.TestLine($"Recover pos: {pos_} status: {(int)posStatus_} deathPowerSnapshot_: {deathPowerSnapshot_}");
        }

        public void Reset()
        {
            Recover();
            RemoveBuff(Position);
        }
    }
}