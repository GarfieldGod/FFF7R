namespace FFF7RCore {
    using System.Collections;
    using System.Collections.Generic;
    using Effect;
    using System;

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
                foreach (var line in padGrids_)
                {
                    var retLine = new List<BuffList>();
                    foreach (var padGrid in line)
                    {
                        retLine.Add(padGrid.BuffList);
                    }
                    result.Add(retLine);
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
                foreach (var line in padGrids_)
                {
                    var retLine = new List<bool>();
                    foreach (var padGrid in line)
                    {
                        retLine.Add(padGrid.NeverBuffed);
                    }
                    result.Add(retLine);
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

        public (int playerBuff, int playerDeBuff, int rivalBuff, int rivalDeBuff) GetBuffedAndDeBuffedNum(List<Int2D> exclusionList = null)
        {
            int playerBuffed = 0, playerDeBuffed = 0, rivalBuffed = 0, rivalDeBuffed = 0;
            foreach(var line in padGrids_)
            {
                foreach(var padGrid in line)
                {
                    if (padGrid.Empty || (exclusionList != null && exclusionList.Contains(padGrid.Position))) continue;

                    if (padGrid.Status == PosStatus.OCCUPIED_PLAYER)
                    {
                        if (padGrid.OnBuffed) playerBuffed++;
                        else if (padGrid.OnDeBuffed) playerDeBuffed++;
                    } else if (padGrid.Status == PosStatus.OCCUPIED_RIVAL)
                    {
                        if (padGrid.OnBuffed) rivalBuffed++;
                        else if (padGrid.OnDeBuffed) rivalDeBuffed++;
                    }
                }
            }
            return (playerBuffed, playerDeBuffed, rivalBuffed, rivalDeBuffed);
        }

        // 获取所有非空的、但未被占用的格子
        public List<PadGrid> GetGrids(Func<PadGrid, bool> filter = null)
        {
            var result = new List<PadGrid>();
            foreach(var line in padGrids_)
            {
                foreach(var padGrid in line)
                {
                    if (filter != null && !filter.Invoke(padGrid)) continue;
                    result.Add(padGrid);
                }
            }
            return result;
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
            padGrids_[pos.x][pos.y].Reset();
            RemoveAllBuffsFromPos(pos);
        }

        /// <summary>
        /// 加 buff 并广播变化。返回等级是否真的发生了变化。
        /// <para>
        /// 对外一律报告<b>等级实际变化量</b>（Level 的 after - before），而不是本次申请的 buff 值。
        /// 差别只在 CountBased：EffectOperationHelper.PowerChangeEffect 对它采用覆盖语义
        /// （先按 mark 清空旧 buff 再写入新 buff），重新算出的值常常与旧值相同，此时等级并没有动。
        /// 若照发事件，听者会再算一遍同样的值、再覆盖一次，形成与数值大小无关的无限回响
        /// （值有界、递归无界，最终栈溢出）。因此 delta 为 0 时整条通知链路必须静默。
        /// </para>
        /// </summary>
        public bool AddBuff(Int2D pos, Buff buff, bool DoSelfPowerBuff = false)
        {
            PadGrid padGrid = padGrids_[pos.x][pos.y];
            int before = padGrid.Level;

            padGrid.AddBuff(buff);

            return NotifyLevelChanged(padGrid, before, buff, DoSelfPowerBuff);
        }

        /// <summary>
        /// 用 buff 替换掉本格上匹配 match 的旧 buff，作为一个原子操作结算。
        /// <para>
        /// 必须与 <see cref="AddBuff"/> 分开：CountBased 类效果是"先清空同 mark 的旧 buff 再写入新值"，
        /// 若移除和写入分成两步、各自只看自己那一段的变化，写入方看到的恒是 +value（因为 before 已被挖掉），
        /// 即使净变化为 0 也会广播事件，于是听者再算一遍同样的值、再覆盖一次，无限回响（值有界、递归无界）。
        /// 这里把两者放在同一段里取前后值，净变化为 0 就完全静默。
        /// </para>
        /// </summary>
        public bool ReplaceBuff(Int2D pos, Predicate<Buff> match, Buff buff, bool DoSelfPowerBuff = false)
        {
            PadGrid padGrid = padGrids_[pos.x][pos.y];
            int before = padGrid.Level;

            if (match != null) padGrid.BuffList.RemoveAll(match);
            padGrid.AddBuff(buff);

            return NotifyLevelChanged(padGrid, before, buff, DoSelfPowerBuff);
        }

        // 统一出口：只有等级真的变了才广播；没变时仅维护首次被 buff 的标记（它读 padGrid.BuffValue 字段，与入参无关）。
        private bool NotifyLevelChanged(PadGrid padGrid, int before, Buff buff, bool DoSelfPowerBuff)
        {
            int delta = padGrid.Level - before;
            if (delta == 0)
            {
                if (!DoSelfPowerBuff) CheckFirstBuffed(padGrid);
                return false;
            }

            CheckBuffEvent(padGrid, delta, DoSelfPowerBuff, buff.source);
            return true;
        }

        public void CheckBuffEvent(PadGrid padGrid, int buffValue, bool DoSelfPowerBuff = false, Int2D? source = null)
        {
            if (!DoSelfPowerBuff) CheckFirstBuffed(padGrid);
            CheckOnBuffed(padGrid, buffValue);
            if (!DoSelfPowerBuff) InvokeBuffEvent(padGrid, buffValue, source);
        }

        public void InvokeBuffEvent(PadGrid padGrid, int buffValue, Int2D? source = null)
        {
            if (padGrid.Status == PosStatus.OCCUPIED_PLAYER || padGrid.Status == PosStatus.OCCUPIED_RIVAL)
            {
                PlayerType gridOwner = padGrid.Status == PosStatus.OCCUPIED_PLAYER ? PlayerType.PLAYER : PlayerType.RIVAL;
                eventSystem?.RaiseChessBuffed(new EventSystem.ChessBuffedEventArgs(buffValue, gridOwner, padGrid.Position, source ?? Int2D.Unknown));
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

        private bool RemoveAllBuffsFromPos(Int2D pos)
        {
            // Log.TestLine("---RemoveBuffs--- " + id);
            List<EventSystem.BuffExpiredEntry> expired = null;
            for (int x = 0; x < padGrids_.Count; x++)
            {
                for (int y = 0; y < padGrids_[x].Count; y++)
                {
                    PadGrid padGrid = padGrids_[x][y];
                    // 没有以 pos 为来源的 buff 时直接跳过，保持原路径的开销
                    if (!padGrid.BuffList.HasOwner(pos)) continue;

                    int before = padGrid.Level;
                    padGrid.RemoveBuff(pos);
                    int after = padGrid.Level;
                    if (before == after) continue;

                    // 空格/等级格同样会带 buff、同样会掉等级，不能一律当成 RIVAL。
                    PlayerType owner;
                    if (padGrid.Status == PosStatus.OCCUPIED_PLAYER) owner = PlayerType.PLAYER;
                    else if (padGrid.Status == PosStatus.OCCUPIED_RIVAL) owner = PlayerType.RIVAL;
                    else owner = PlayerType.Null;

                    expired ??= new List<EventSystem.BuffExpiredEntry>();
                    expired.Add(new EventSystem.BuffExpiredEntry(padGrid.Position, owner, before, after));
                }
            }
            if (expired != null)
            {
                eventSystem?.RaiseBuffsExpired(new EventSystem.BuffsExpiredEventArgs(pos, expired));
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

        public void InitGridMap(int height, int length)
        {
            List<List<PadGrid>> padGrids = new List<List<PadGrid>> { };
            for (int x = 0; x < height; x++)
            {
                List<PadGrid> line = new List<PadGrid>();
                for (int y = 0; y < length; y++)
                {
                    PadGrid padGrid = new PadGrid(new Int2D(x, y));
                    line.Add(padGrid);
                }
                padGrids.Add(line);
            }
            padGrids_ = padGrids;
        }
    }
}