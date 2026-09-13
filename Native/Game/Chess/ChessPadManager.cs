namespace FFF7RCore {
    using Effect;

    public class ChessPadManager
    {
        private ChessPad chessPad_;

        private EventSystem eventSystem_;

        private Queue<Action> operationQueue_ = new Queue<Action>();

        public ChessPadManager(ChessPad chessPad)
        {
            chessPad_ = chessPad;
            eventSystem_ = new EventSystem();
            chessPad_.eventSystem = eventSystem_;
        }

        public bool Input(Input input)
        {
            if(!CheckInput(input)) return false;
            RestPos(chessPad_, input.pos);

            PadGrid padGrid = chessPad_[input.pos.x, input.pos.y];
            padGrid.Chess = input.chess;
            padGrid.Status = input.playerType == PlayerType.PLAYER ? PosStatus.OCCUPIED_PLAYER : PosStatus.OCCUPIED_RIVAL;

            Log.TestLine("DoSelfPower", TextColor.BLACK);
            DoSelfPower(input, chessPad_);

            Log.TestLine("DoCardEffect", TextColor.BLACK);
            DoCardEffect(input, chessPad_);
            Log.TestLine("DoPosEffect", TextColor.BLACK);

            Log.TestLine($"1{chessPad_[input.pos].Status}", TextColor.BLACK);
            DoPosEffect(input, chessPad_);
            Log.TestLine($"2{chessPad_[input.pos].Status}", TextColor.BLACK);
            Log.TestLine("RaiseChessPlaced", TextColor.BLACK);

            eventSystem_.RaiseChessPlaced(input);
            return true;
        }

        public ChessPad Preview(Input input)
        {
            if(!CheckInput(input)) return null;

            ChessPad previewPad = new ChessPad(chessPad_);

            previewPad[input.pos.x, input.pos.y].Chess = input.chess;

            DoPosEffect(input, previewPad);
            DoCardEffect(input, previewPad);
            return previewPad;
        }

        public bool CheckInput(Input input)
        {
            if (input.chess == null) return false;

            if (!chessPad_.IsInBoard(input.pos))
            {
                Log.TestLine("CheckInput Failed: pos out of board", TextColor.BLACK);
                return false;
            }

            PosStatus posStatus = chessPad_[input.pos.x, input.pos.y].Status;
            int posLevel = (int)posStatus % 10;
            if (input.chess.Cost > 0)
            {
                if (posStatus >= PosStatus.OCCUPIED_PLAYER || posLevel < input.chess.Cost) return false;
            } 
            else // 覆盖棋子
            {
                if (posStatus < PosStatus.OCCUPIED_PLAYER) return false;
                if (input.playerType == PlayerType.PLAYER && posStatus != PosStatus.OCCUPIED_PLAYER) return false;
                if (input.playerType == PlayerType.RIVAL && posStatus != PosStatus.OCCUPIED_RIVAL) return false;
            }

            return true;
        }

        public static void DoPosEffect(Input input, ChessPad chessPad)
        {
            chessPad.StatusMap = PosEffect.DoPosEffect(input, chessPad);
        }

        private void DoSelfPower(Input input, ChessPad chessPad)
        {
            int power = input.chess.Level;
            Buff selfLevelBuff = new Buff(input.pos, input.pos, power, EffectTarget.Self, input.playerType);
            chessPad.AddBuff(input.pos, selfLevelBuff, true);
        }

        public void DoCardEffect(Input input, ChessPad chessPad, bool preview = false)
        {
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
                    if (!preview) SubscribeDeadEvent(condition, input);
                    break;
                case EffectCondition.BuffedFriendNum:
                case EffectCondition.BuffedEnemyNum:
                case EffectCondition.BuffedFriendOrEnemyNum:
                case EffectCondition.DeBuffedFriendNum:
                case EffectCondition.DeBuffedEnemyNum:
                case EffectCondition.DeBuffedFriendOrEnemyNum:
                    if (!preview) SubscribeBuffedEvent(condition, input);
                    break;
                case EffectCondition.OnFriendPlayed:
                case EffectCondition.OnEnemyPlayed:
                case EffectCondition.OnFriendOrEnemyPlayed:
                    if (!preview) SubscribePlacedEvent(condition, input);
                    break;
                case EffectCondition.OnPowerFirstReach:
                    if (!preview) SubscribeBuffedEvent(condition, input);
                    break;
                case EffectCondition.OnTurnEnd:
                    // TODO
                    break;
                case EffectCondition.OnLineWin:
                    // TODO
                    break;
            }
            CheckDead(chessPad);
        }
    //---------------------------------------------------------------------------------------------PlacedEvent
        private Dictionary<Int2D, EventHandler<Input>> placedEventHandler_ = new();

        private void UnsubscribePlacedEvent(Int2D pos)
        {
            if (placedEventHandler_.TryGetValue(pos, out var handler))
            {
                eventSystem_.OnChessPlaced -= handler;
                placedEventHandler_.Remove(pos);
            }
        }

        private void SubscribePlacedEvent(EffectCondition condition, Input input)
        {
            UnsubscribePlacedEvent(input.pos);
            EventHandler<Input> handler = (sender, placedInput) =>
            {
                if (input.pos == placedInput.pos) return;

                PlayerType owner = placedInput.playerType;
                bool match = 
                (condition == EffectCondition.OnFriendOrEnemyPlayed)
                || (condition == EffectCondition.OnFriendPlayed && 
                    (owner == PlayerType.PLAYER && input.playerType == PlayerType.PLAYER || 
                    owner == PlayerType.RIVAL && input.playerType == PlayerType.RIVAL))
                || (condition == EffectCondition.OnEnemyPlayed &&
                    (owner == PlayerType.PLAYER && input.playerType == PlayerType.RIVAL || 
                     owner == PlayerType.RIVAL && input.playerType == PlayerType.PLAYER));

                if (match)
                {
                    Log.TestLine($"Catch OnChessPlaced: {condition} pos: {input.pos} src: {placedInput.pos}");
                    EffectOperationHelper.TriggerEffect(chessPad_, input.pos);
                }
            };
            placedEventHandler_[input.pos] = handler;
            eventSystem_.OnChessPlaced += handler;
        }
    //---------------------------------------------------------------------------------------------BuffedEvent
        private Dictionary<Int2D, EventHandler<EventSystem.ChessBuffedEventArgs>> buffedEventHandler_ = new();

        private void UnsubscribeBuffedEvent(Int2D pos)
        {
            if (buffedEventHandler_.TryGetValue(pos, out var handler))
            {
                eventSystem_.OnChessBuffed -= handler;
                buffedEventHandler_.Remove(pos);
            }
        }

        private void SubscribeBuffedEvent(EffectCondition condition, Input input)
        {
            UnsubscribeBuffedEvent(input.pos);
            EventHandler<EventSystem.ChessBuffedEventArgs> handler = (sender, buffedOne) =>
            {
                int value = buffedOne.Value;
                bool positive = value > 0;
                PlayerType owner = buffedOne.Owner;
                PlayerType self = input.playerType;
                bool friend = owner == self;
                Int2D pos = buffedOne.Position;
                if (condition != EffectCondition.OnPowerFirstReach && input.pos == pos) return;

                bool firstPowerReach = 
                    condition == EffectCondition.OnPowerFirstReach && buffedOne.Position == input.pos &&
                    chessPad_[input.pos].Level >= input.chess.CardEffect.PowerThreshold &&
                    chessPad_[input.pos].FirstPowerReach == false;
                if (condition == EffectCondition.OnPowerFirstReach) Log.TestLine($"firstPowerReach: {firstPowerReach} pos: {input.pos} src: {buffedOne.Position}");

                bool match = 
                    (condition == EffectCondition.BuffedFriendNum && positive && friend) ||
                    (condition == EffectCondition.BuffedEnemyNum && positive && !friend) ||
                    (condition == EffectCondition.DeBuffedFriendNum && !positive && friend) ||
                    (condition == EffectCondition.DeBuffedEnemyNum && !positive && !friend) ||
                    (condition == EffectCondition.BuffedFriendOrEnemyNum && positive) ||
                    (condition == EffectCondition.DeBuffedFriendOrEnemyNum && !positive) ||
                    firstPowerReach;

                if (firstPowerReach) chessPad_[input.pos].FirstPowerReach = true;

                if (match)
                {
                    Log.TestLine($"Catch OnChessBuffed: {condition} pos: {input.pos} src: {buffedOne.Position}");
                    EffectOperationHelper.TriggerEffect(chessPad_, input.pos);
                }
            };
            buffedEventHandler_[input.pos] = handler;
            eventSystem_.OnChessBuffed += handler;
        }
    //---------------------------------------------------------------------------------------------DeadEvent
        private Dictionary<Int2D, EventHandler<EventSystem.ChessDeadEventArgs>> deadEventHandler_ = new();

        private void UnsubscribeDeadEvent(Int2D pos)
        {
            if(deadEventHandler_.TryGetValue(pos, out var handler))
            {
                eventSystem_.OnChessDead -= handler;
                deadEventHandler_.Remove(pos);
            }
        }

        private void SubscribeDeadEvent(EffectCondition condition, Input input)
        {
            UnsubscribeDeadEvent(input.pos);
            EventHandler<EventSystem.ChessDeadEventArgs> handler = (sender, deadOne) =>
            {
                bool match = 
                (condition == EffectCondition.OnFriendOrEnemyDead)
                || (condition == EffectCondition.OnFriendDead && 
                    (deadOne.Owner == PlayerType.PLAYER && input.playerType == PlayerType.PLAYER || 
                     deadOne.Owner == PlayerType.RIVAL && input.playerType == PlayerType.RIVAL))
                || (condition == EffectCondition.OnEnemyDead &&
                    (deadOne.Owner == PlayerType.PLAYER && input.playerType == PlayerType.RIVAL || 
                     deadOne.Owner == PlayerType.RIVAL && input.playerType == PlayerType.PLAYER));

                if (match)
                {
                    Log.TestLine($"Catch OnChessDead: {condition} Name: {input.chess.Name} pos: {input.pos} src: {deadOne.Chess.Name}");
                    EffectOperationHelper.TriggerEffect(chessPad_, input.pos);
                }
            };
            deadEventHandler_[input.pos] = handler;
            eventSystem_.OnChessDead += handler;
        }

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

            UnsubscribeDeadEvent(padGrid.Position);
            UnsubscribePlacedEvent(padGrid.Position);
            UnsubscribeBuffedEvent(padGrid.Position);

            PlayerType owner = padGrid.Status == PosStatus.OCCUPIED_PLAYER ? PlayerType.PLAYER : PlayerType.RIVAL;
            eventSystem_.RaiseChessDead(new EventSystem.ChessDeadEventArgs(padGrid.Chess, owner));
            chessPad.RestPos(padGrid.Position);
        }

    //---------------------------------------------------------------------------------------------清理
        ~ChessPadManager()
        {
            foreach(var kv in deadEventHandler_)
            {
                eventSystem_.OnChessDead -= kv.Value;
            }
            deadEventHandler_.Clear();

            foreach(var kv in buffedEventHandler_)
            {
                eventSystem_.OnChessBuffed -= kv.Value;
            }
            buffedEventHandler_.Clear();

            foreach(var kv in placedEventHandler_)
            {
                eventSystem_.OnChessPlaced -= kv.Value;
            }
            placedEventHandler_.Clear();

            chessPad_.eventSystem = null;
            eventSystem_.ClearAll();
        }
    }
}