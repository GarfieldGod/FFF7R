namespace FFF7RCore {
    using Effect;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    public class ChessPadManager
    {
        private ChessPad chessPad_;
        private ChessPad previewPad_;

        public ChessPadManager(ChessPad chessPad)
        {
            chessPad_ = chessPad;
            previewPad_ = new ChessPad(chessPad);
        }

        public bool Input(Input input)
        {
            return InputOnPad(input, chessPad_);
        }

        public bool InputOnPad(Input input, ChessPad chessPad)
        {
            if(!CheckInput(input, chessPad)) return false;

            ProcessInput(chessPad, input);
            return true;
        }

        public ChessPad Preview(Input input)
        {
            if(!CheckInput(input, chessPad_)) return null;
            previewPad_.CopyFrom(chessPad_);

            ProcessInput(previewPad_, input);
            return previewPad_;
        }

        private void ProcessInput(ChessPad pad, Input input)
        {
            RestPos(pad, input.pos);

            PadGrid padGrid = pad[input.pos.x, input.pos.y];
            padGrid.Chess = input.chess;
            padGrid.Status = input.playerType == PlayerType.PLAYER ? PosStatus.OCCUPIED_PLAYER : PosStatus.OCCUPIED_RIVAL;

            DoSelfPower(input, pad);
            DoCardEffect(input, pad);
            DoPosEffect(input, pad);

            pad.EventSystem.RaiseChessPlaced(input);
        }

        public PreviewResult PreviewDetail(Input input)
        {
            PreviewResult r = new PreviewResult { Pos = input.pos };

            if (!CheckInput(input, out InputReject reject, chessPad_))
            {
                r.Valid = false;
                r.Reject = reject;
                return r;
            }

            r.Valid = true;
            r.Reject = InputReject.None;

            previewPad_.CopyFrom(chessPad_);   // 格 + buff + pending_ 全量
            ProcessInput(previewPad_, input);  // 与真落子同一条路径
            BuildDiff(r, input.pos);
            return r;
        }

        private void BuildDiff(PreviewResult r, Int2D placedPos)
        {
            r.Cells  = new List<CellChange>(8);
            r.Deaths = new List<DeathInfo>(2);

            int w = chessPad_.Size.x, h = chessPad_.Size.y;
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    PadGrid before = chessPad_[x, y];
                    PadGrid after  = previewPad_[x, y];

                    // 战力或地形任一变化 → 记一条
                    if (before.Level != after.Level || before.Status != after.Status)
                    {
                        r.Cells.Add(new CellChange(
                            before.Position,
                            before.Level, after.Level,
                            before.Status, after.Status));
                    }

                    // 消失的棋子：原本有、现在空，且不是落点（落点是覆盖，不是死亡）
                    bool isPlaced = x == placedPos.x && y == placedPos.y;
                    if (!before.Empty && after.Empty && !isPlaced)
                    {
                        PlayerType owner = before.Status == PosStatus.OCCUPIED_PLAYER
                            ? PlayerType.PLAYER : PlayerType.RIVAL;
                        r.Deaths.Add(new DeathInfo(
                            before.Position, owner, before.Chess?.CardCode, before.Level));
                    }
                }
            }

            PadGrid placed   = previewPad_[placedPos];
            r.PlacedLevel    = placed.Empty ? 0 : placed.Level;
            r.PlacedSurvives = !placed.Empty;
        }

        public bool CheckInput(Input input, ChessPad chessPad = null) => CheckInput(input, out _, chessPad);

        /// <summary>
        /// 只判落点规则：越界、占用、地形等级不足。手牌归属由 <see cref="Game.CheckInput"/> 负责。
        /// </summary>
        public bool CheckInput(Input input, out InputReject reject, ChessPad chessPad = null)
        {
            if (chessPad == null) chessPad = chessPad_;
            if (input.chess == null) { reject = InputReject.EmptyInput; return false; }

            if (!chessPad.IsInBoard(input.pos))
            {
                Log.TestLine("CheckInput Failed: pos out of board", TextColor.BLACK);
                reject = InputReject.OutOfBoard;
                return false;
            }

            PosStatus posStatus = chessPad[input.pos.x, input.pos.y].Status;
            int posLevel = (int)posStatus % 10;
            if (input.chess.Cost > 0)
            {
                if (input.playerType == PlayerType.PLAYER && posStatus >= PosStatus.EMPTY) {reject = InputReject.NotYourGrid; return false;}
                if (input.playerType == PlayerType.RIVAL && posStatus <= PosStatus.EMPTY) {reject = InputReject.NotYourGrid; return false;}
                if (posStatus >= PosStatus.OCCUPIED_PLAYER) { reject = InputReject.CellOccupied; return false; }
                if (posLevel < input.chess.Cost) { reject = InputReject.CellLevelTooLow; return false; }
            } 
            else // 覆盖棋子
            {
                if (posStatus < PosStatus.OCCUPIED_PLAYER) { reject = InputReject.NoOwnChessToCover; return false; }
                if (input.playerType == PlayerType.PLAYER && posStatus != PosStatus.OCCUPIED_PLAYER) { reject = InputReject.NoOwnChessToCover; return false; }
                if (input.playerType == PlayerType.RIVAL && posStatus != PosStatus.OCCUPIED_RIVAL) { reject = InputReject.NoOwnChessToCover; return false; }
            }

            reject = InputReject.None;
            return true;
        }

        public static void DoPosEffect(Input input, ChessPad chessPad)
        {
            // 只有会发事件的盘才需要 before 快照；预览盘（EventSystem == null）保持原样，不多分配
            List<List<int>> before = chessPad.EventSystem != null ? chessPad.StatusMap : null;
            List<List<int>> after = PosEffect.DoPosEffect(input, chessPad);
            chessPad.StatusMap = after;
            if (before != null) RaiseGridLevelsChanged(input, chessPad, before, after);
        }

        // 纯通知：只做 diff，不参与任何状态计算。预览盘没有 EventSystem，自然不会触发。
        private static void RaiseGridLevelsChanged(Input input, ChessPad chessPad, List<List<int>> before, List<List<int>> after)
        {
            if (chessPad.EventSystem == null) return;

            List<EventSystem.GridLevelChange> changes = null;
            for (int x = 0; x < after.Count; x++)
            {
                for (int y = 0; y < after[x].Count; y++)
                {
                    if (before[x][y] == after[x][y]) continue;
                    changes ??= new List<EventSystem.GridLevelChange>();
                    changes.Add(new EventSystem.GridLevelChange(new Int2D(x, y), before[x][y], after[x][y]));
                }
            }
            if (changes == null) return;
            chessPad.EventSystem.RaiseGridLevelsChanged(new EventSystem.GridLevelsChangedEventArgs(input, changes));
        }

        private void DoSelfPower(Input input, ChessPad chessPad)
        {
            int power = input.chess.Level;
            Buff selfLevelBuff = new Buff(input.pos, input.pos, power, EffectTarget.Self, input.playerType);
            chessPad.AddBuff(input.pos, selfLevelBuff, true);
        }

        public void DoCardEffect(Input input, ChessPad chessPad)
        {
            bool preview = chessPad == previewPad_;
            if (input.chess.CardEffect == null) return;
            // Do Others
            EffectCondition condition = input.chess.CardEffect.Condition;
            switch (condition)
            {
                case EffectCondition.OnPlayed:
                    EffectOperationHelper.TriggerEffect(chessPad, input.pos, true, this);
                    break;
                case EffectCondition.OnSelfDead: break;
                case EffectCondition.OnPosition:
                    EffectOperationHelper.TriggerEffect(chessPad, input.pos, false);
                    break;
                case EffectCondition.OnFirstBuffed:
                case EffectCondition.OnFirstDeBuffed:
                case EffectCondition.OnFirstBuffedOrDeBuffed:
                case EffectCondition.OnBuffed:
                case EffectCondition.OnDeBuffed:
                case EffectCondition.OnBuffedOrDeBuffed:
                    PadGrid padGrid = chessPad[input.pos];
                    chessPad.CheckBuffEvent(padGrid, padGrid.BuffList.Compute(b => b.source != input.pos));
                    break;
                case EffectCondition.OnFriendOrEnemyDead:
                case EffectCondition.OnFriendDead:
                case EffectCondition.OnEnemyDead:
                    chessPad.EffectSystem.WaitFor(condition, input, EventKind.Dead); break;
                case EffectCondition.BuffedFriendNum:
                case EffectCondition.BuffedEnemyNum:
                case EffectCondition.BuffedFriendOrEnemyNum:
                case EffectCondition.DeBuffedFriendNum:
                case EffectCondition.DeBuffedEnemyNum:
                case EffectCondition.DeBuffedFriendOrEnemyNum:
                case EffectCondition.OnPowerFirstReach:
                    chessPad.EffectSystem.WaitFor(condition, input, EventKind.Buffed); break;
                case EffectCondition.OnFriendPlayed:
                case EffectCondition.OnEnemyPlayed:
                case EffectCondition.OnFriendOrEnemyPlayed:
                    chessPad.EffectSystem.WaitFor(condition, input, EventKind.Placed); break;
                case EffectCondition.OnTurnEnd:
                    // TODO
                    break;
                case EffectCondition.OnLineWin:
                    // TODO
                    break;
            }
            CheckDead(chessPad);
        }
    // //---------------------------------------------------------------------------------------------PlacedEvent
    //     private void SubscribePlacedEvent(EffectCondition condition, Input input, ChessPad chessPad)
    //     {
    //         EventHandler<EventSystem.ChessPlacedEventArgs> handler = (sender, placedOne) =>
    //         {
    //             if (input.pos == placedOne.Position) return;

    //             PlayerType owner = placedOne.Owner;
    //             bool match = 
    //             (condition == EffectCondition.OnFriendOrEnemyPlayed)
    //             || (condition == EffectCondition.OnFriendPlayed && 
    //                 (owner == PlayerType.PLAYER && input.playerType == PlayerType.PLAYER || 
    //                 owner == PlayerType.RIVAL && input.playerType == PlayerType.RIVAL))
    //             || (condition == EffectCondition.OnEnemyPlayed &&
    //                 (owner == PlayerType.PLAYER && input.playerType == PlayerType.RIVAL || 
    //                  owner == PlayerType.RIVAL && input.playerType == PlayerType.PLAYER));

    //             if (match)
    //             {
    //                 Log.TestLine($"Catch OnChessPlaced: {condition} pos: {input.pos} src: {placedOne.Position}");
    //                 EffectOperationHelper.TriggerEffect(chessPad, input.pos);
    //             }
    //         };
    //         chessPad.EventSystem.Bind(EventSystem.EventType.Placed, input.pos, handler);
    //     }
    // //---------------------------------------------------------------------------------------------BuffedEvent
    //     private void SubscribeBuffedEvent(EffectCondition condition, Input input, ChessPad chessPad)
    //     {
    //         EventHandler<EventSystem.ChessBuffedEventArgs> handler = (sender, buffedOne) =>
    //         {
    //             int value = buffedOne.Value;          // 等级变化量，非 0（见 ChessPad.AddBuff）
    //             bool positive = value > 0;
    //             PlayerType owner = buffedOne.Owner;
    //             PlayerType self = input.playerType;
    //             bool friend = owner == self;
    //             Int2D pos = buffedOne.Position;
    //             if (condition != EffectCondition.OnPowerFirstReach && input.pos == pos) return;

    //             bool firstPowerReach = 
    //                 condition == EffectCondition.OnPowerFirstReach && buffedOne.Position == input.pos &&
    //                 chessPad[input.pos].Level >= input.chess.CardEffect.PowerThreshold &&
    //                 chessPad[input.pos].FirstPowerReach == false;
    //             if (condition == EffectCondition.OnPowerFirstReach) Log.TestLine($"firstPowerReach: {firstPowerReach} pos: {input.pos} src: {buffedOne.Position}");

    //             bool match = 
    //                 (condition == EffectCondition.BuffedFriendNum && positive && friend) ||
    //                 (condition == EffectCondition.BuffedEnemyNum && positive && !friend) ||
    //                 (condition == EffectCondition.DeBuffedFriendNum && !positive && friend) ||
    //                 (condition == EffectCondition.DeBuffedEnemyNum && !positive && !friend) ||
    //                 (condition == EffectCondition.BuffedFriendOrEnemyNum && positive) ||
    //                 (condition == EffectCondition.DeBuffedFriendOrEnemyNum && !positive) ||
    //                 firstPowerReach;

    //             if (firstPowerReach) chessPad[input.pos].FirstPowerReach = true;

    //             if (match)
    //             {
    //                 Log.TestLine($"Catch OnChessBuffed: {condition} pos: {input.pos} src: {buffedOne.Position}");
    //                 EffectOperationHelper.TriggerEffect(chessPad, input.pos);
    //             }
    //         };
    //         chessPad.EventSystem.Bind(EventSystem.EventType.Buffed, input.pos, handler);
    //     }
    // //---------------------------------------------------------------------------------------------DeadEvent
    //     private void SubscribeDeadEvent(EffectCondition condition, Input input, ChessPad chessPad)
    //     {
    //         EventHandler<EventSystem.ChessDeadEventArgs> handler = (sender, deadOne) =>
    //         {
    //             bool match = 
    //             (condition == EffectCondition.OnFriendOrEnemyDead)
    //             || (condition == EffectCondition.OnFriendDead && 
    //                 (deadOne.Owner == PlayerType.PLAYER && input.playerType == PlayerType.PLAYER || 
    //                  deadOne.Owner == PlayerType.RIVAL && input.playerType == PlayerType.RIVAL))
    //             || (condition == EffectCondition.OnEnemyDead &&
    //                 (deadOne.Owner == PlayerType.PLAYER && input.playerType == PlayerType.RIVAL || 
    //                  deadOne.Owner == PlayerType.RIVAL && input.playerType == PlayerType.PLAYER));

    //             if (match)
    //             {
    //                 Log.TestLine($"Catch OnChessDead: {condition} Name: {input.chess.Name} pos: {input.pos} src: {deadOne.Chess.Name}");
    //                 EffectOperationHelper.TriggerEffect(chessPad, input.pos);
    //             }
    //         };
    //         chessPad.EventSystem.Bind(EventSystem.EventType.Dead, input.pos, handler);
    //     }

        public void CheckDead(ChessPad chessPad)
        {
            bool hasDead = false;
            foreach (var line in chessPad)
            {
                foreach (var padGrid in line)
                {
                    if (!padGrid.Empty && padGrid.Level <= 0)
                    {
                        hasDead = true;
                        // Log.TestLine("Dead: " + padGrid.GetID() + " Name: " + padGrid.GetChess().Name, TextColor.PURPLE);
                        RestPos(chessPad, padGrid.Position);
                    }
                }
            }
            if (hasDead) CheckDead(chessPad);
        }

        private void RestPos(ChessPad chessPad, Int2D pos)
        {
            if (!chessPad.IsInBoard(pos) || chessPad[pos].Empty) return;

            PadGrid padGrid = chessPad[pos];
            if (padGrid.Chess.CardEffect != null && padGrid.Chess.CardEffect.Condition == EffectCondition.OnSelfDead)
            {
                EffectOperationHelper.TriggerEffect(chessPad, padGrid.Position);
            }

            // chessPad.EventSystem.UnbindAll(pos);
            chessPad.EffectSystem.ForgetAll(pos);

            PlayerType owner = padGrid.Status == PosStatus.OCCUPIED_PLAYER ? PlayerType.PLAYER : PlayerType.RIVAL;
            chessPad.EventSystem?.RaiseChessDead(new EventSystem.ChessDeadEventArgs(padGrid.Chess, owner, padGrid.Position));
            chessPad.RestPos(padGrid.Position);
        }
    }
}