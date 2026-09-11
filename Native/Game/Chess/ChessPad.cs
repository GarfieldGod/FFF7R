namespace FFF7RCore {
    using System.Collections;
    using System.Collections.Generic;
    using Effect;

    public class ChessPad : IEnumerable<List<PadGrid>>
    {
        public Int2D Size => size_;
        public int Height => padGrids_.Count;
        public int Width { get {return padGrids_.Count > 0 ? padGrids_[0].Count : 0;}}
        public List<List<PadGrid>> GridMap => padGrids_;

        private readonly Int2D size_;
        private List<List<PadGrid>> padGrids_ = new List<List<PadGrid>> { };

        public EventSystem eventSystem;

        public ChessPad(int x, int y)
        {
            size_ = new Int2D(x, y);
            InitGridMap(x, y);
        }

        public ChessPad(Int2D size)
        {
            size_ = size;
            InitGridMap(size.x, size.y);
        }

        public ChessPad(List<List<PadGrid>> padGrids)
        {
            size_ = new Int2D(padGrids.Count, padGrids[0].Count);
            padGrids_ = padGrids;
        }

        // 深拷贝
        public ChessPad(ChessPad chessPad)
        {
            size_ = chessPad.Size;
            padGrids_ = Utils.DeepCopy(chessPad.GridMap);
        }

        public bool IsInBoard(Int2D pos)
        {
            return pos.x >= 0 && pos.x < Size.x && pos.y >= 0 && pos.y < Size.y;
        }

        public PadGrid this[int x, int y]
        {
            get
            {
                return padGrids_[x][y];
            }
        }

        public IEnumerator<List<PadGrid>> GetEnumerator() => padGrids_.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public PadGrid this[Int2D pos]
        {
            get
            {
                return padGrids_[pos.x][pos.y];
            }
        }

        public List<List<int>> StatusMap
        {
            get{
                List<List<int>> result = new List<List<int>>();
                for (int x = 0; x < padGrids_.Count; x++)
                {
                    var line = new List<int>();
                    for (int y = 0; y < padGrids_[0].Count; y++)
                    {
                        line.Add((int)padGrids_[x][y].Status);
                    }
                    result.Add(line);
                }
                return result;
            }
            set
            {
                for (int x = 0; x < padGrids_.Count; x++)
                {
                    for (int y = 0; y < padGrids_[0].Count; y++)
                    {
                        padGrids_[x][y].Status = (PosStatus)value[x][y];
                    }
                }
            }
        }

        public List<List<Chess>> ChessMap
        {
            get {
                List<List<Chess>> result = new List<List<Chess>>();
                for (int x = 0; x < padGrids_.Count; x++)
                {
                    var line = new List<Chess>();
                    for (int y = 0; y < padGrids_[0].Count; y++)
                    {
                        if (padGrids_[x][y].Chess != null)
                        {
                            line.Add(new Chess(padGrids_[x][y].Chess));
                        }
                        else
                        {
                            line.Add(null);
                        }
                    }
                    result.Add(line);
                }
                return result;
            }
        }

        public List<List<BuffList>> BuffsMap
        {
            get 
            {
                List<List<BuffList>> result = new List<List<BuffList>>();
                for (int x = 0; x < padGrids_.Count; x++)
                {
                    var line = new List<BuffList>();
                    for (int y = 0; y < padGrids_[0].Count; y++)
                    {
                        line.Add(padGrids_[x][y].BuffList);
                    }
                    result.Add(line);
                }
                return result;
            }
            set
            {
                for (int x = 0; x < padGrids_.Count; x++)
                {
                    for (int y = 0; y < padGrids_[0].Count; y++)
                    {
                        padGrids_[x][y].BuffList = value[x][y];
                    }
                }
            }
        }

        public List<List<bool>> BuffStatus
        {
            get
            {
                List<List<bool>> result = new List<List<bool>>();
                for (int x = 0; x < padGrids_.Count; x++)
                {
                    var line = new List<bool>();
                    for (int y = 0; y < padGrids_[0].Count; y++)
                    {
                        line.Add(padGrids_[x][y].NeverBuffed);
                    }
                    result.Add(line);
                }
                return result;
            }
            set
            {
                for (int x = 0; x < padGrids_.Count; x++)
                {
                    for (int y = 0; y < padGrids_[0].Count; y++)
                    {
                        padGrids_[x][y].NeverBuffed = value[x][y];
                    }
                }
            }
        }

        public void SetChess(Int2D pos, Chess src)
        {
            if (src != null)
            {
                padGrids_[pos.x][pos.y].Chess = src.Property;
            }
            else
            {
                padGrids_[pos.x][pos.y].Chess = null;
            }
        }

        public List<List<int>> CardLevelMap
        {
            get
            {
                List<List<int>> stayBuffMapResult = Utils.NewEmpty2DList(padGrids_[0].Count, padGrids_.Count);
                for (int x = 0; x < stayBuffMapResult.Count; x++)
                {
                    for (int y = 0; y < stayBuffMapResult[0].Count; y++)
                    {
                        stayBuffMapResult[x][y] = padGrids_[x][y].Level;
                    }
                }
                return stayBuffMapResult;
            }
        }

        public List<List<int>> ScoreMap
        {
            get
            {
                List<List<int>> ret = new List<List<int>>();
                for (int x = 0; x < CardLevelMap.Count; x++)
                {
                    int playerScore = 0;
                    int rivalScore = 0;
                    for (int y = 0; y < CardLevelMap[0].Count; y++)
                    {
                        PosStatus posStatus = (PosStatus)StatusMap[x][y];
                        if (posStatus == PosStatus.OCCUPIED_PLAYER)
                        {
                            playerScore += CardLevelMap[x][y];
                        }
                        else if (posStatus == PosStatus.OCCUPIED_RIVAL)
                        {
                            rivalScore += CardLevelMap[x][y];
                        }
                    }
                    ret.Add(new List<int>() { playerScore, rivalScore });
                }
                return ret;
            }
        }

        public void RestPos(Int2D pos)
        {
            RemoveBuffs(pos);
            padGrids_[pos.x][pos.y].Reset();
        }

        public bool AddBuff(Int2D pos, Buff buff, bool DoSelfPowerBuff = false)
        {
            PadGrid padGrid = padGrids_[pos.x][pos.y];
            padGrid.AddBuff(buff);
            CheckBuffEvent(padGrid, buff.value, DoSelfPowerBuff);
            return false;
        }

        public void CheckBuffEvent(PadGrid padGrid, int buffValue, bool DoSelfPowerBuff = false)
        {
            if (!DoSelfPowerBuff) CheckFirstBuffed(padGrid);
            CheckOnBuffed(padGrid, buffValue);
            if (!DoSelfPowerBuff) InvokeBuffEvent(padGrid, buffValue);
        }

        public void InvokeBuffEvent(PadGrid padGrid, int buffValue)
        {
            if (padGrid.Status == PosStatus.OCCUPIED_PLAYER || padGrid.Status == PosStatus.OCCUPIED_RIVAL)
            {
                PlayerType gridOwner = padGrid.Status == PosStatus.OCCUPIED_PLAYER ? PlayerType.PLAYER : PlayerType.RIVAL;
                eventSystem?.RaiseChessBuffed(new EventSystem.ChessBuffedEventArgs(buffValue, gridOwner, padGrid.Position));
            }
        }

        public void CheckOnBuffed(PadGrid padGrid, int buffValue)
        {
            if (!padGrid.Empty && padGrid.Chess.CardEffect != null)
            {
                ChessProperty property = padGrid.Chess;

                if (property.CardEffect.Condition == EffectCondition.OnBuffed && buffValue > 0)
                {
                    Log.TestLine("OnBuffed", TextColor.PURPLE);
                    EffectOperationHelper.TriggerEffect(this, padGrid.Position);
                }
                if (property.CardEffect.Condition == EffectCondition.OnDeBuffed && buffValue < 0)
                {
                    Log.TestLine("OnBuffed", TextColor.PURPLE);
                    EffectOperationHelper.TriggerEffect(this, padGrid.Position);
                }
                if (property.CardEffect.Condition == EffectCondition.OnBuffedOrDeBuffed && buffValue != 0)
                {
                    Log.TestLine("OnBuffed", TextColor.PURPLE);
                    EffectOperationHelper.TriggerEffect(this, padGrid.Position);
                }
            }
        }

        public void CheckFirstBuffed(PadGrid padGrid) {
            if (!padGrid.Empty && padGrid.Chess.CardEffect != null)
            {
                ChessProperty property = padGrid.Chess;

                bool triggerFirstBuffed = property.CardEffect.Condition == EffectCondition.OnFirstBuffed && padGrid.BuffValue > 0 && padGrid.NeverBuffed;
                if (triggerFirstBuffed)
                {
                    Log.TestLine("OnFirstBuffed", TextColor.PURPLE);
                    padGrid.NeverBuffed = false;
                    EffectOperationHelper.TriggerEffect(this, padGrid.Position);
                }
                if(padGrid.BuffValue != 0) padGrid.NeverBuffed = false;

                bool triggerFirstDeBuffed = property.CardEffect.Condition == EffectCondition.OnFirstDeBuffed && padGrid.DeBuffValue < 0 && padGrid.NeverDeBuffed;
                if (triggerFirstDeBuffed){
                    Log.TestLine("OnFirstDeBuffed", TextColor.PURPLE);
                    padGrid.NeverDeBuffed = false;
                    EffectOperationHelper.TriggerEffect(this, padGrid.Position);
                }
                if(padGrid.DeBuffValue != 0) padGrid.NeverDeBuffed = false;
            }
        }

        public bool RemoveBuffs(Int2D pos)
        {
            // Log.TestLine("---RemoveBuffs--- " + id);
            for (int x = 0; x < padGrids_.Count; x++)
            {
                for (int y = 0; y < padGrids_[x].Count; y++)
                {
                    var buffs = padGrids_[x][y].BuffList;
                    buffs.Remove(pos);
                }
            }
            return true;
        }

        public void InitStandard()
        {
            InitGridMap(3, 5);
            var chessGridStatus = new List<List<int>>{
                new List<int> { 1, 10, 10, 10, 11 },
                new List<int> { 1, 10, 10, 10, 11 },
                new List<int> { 1, 10, 10, 10, 11 }
            };
            StatusMap = chessGridStatus;
        }

        public void InitGridMap(int height, int lengh)
        {
            List<List<PadGrid>> padGrids = new List<List<PadGrid>> { };
            for (int x = 0; x < height; x++)
            {
                List<PadGrid> line = new List<PadGrid>();
                for (int y = 0; y < lengh; y++)
                {
                    PadGrid padGrid = new PadGrid(new Int2D(x, y));
                    // padGridsposMap_.Add(new Int2D(x, y), padGrid);
                    line.Add(padGrid);
                }
                padGrids.Add(line);
            }
            padGrids_ = padGrids;
        }
    }
}