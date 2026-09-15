namespace FFF7RCore {
    using Effect;

    public class ChessPadManager
    {
        public ChessPad Pad => chessPad_;

        private ChessPad chessPad_;

        private EventSystem eventSystem_;

        private readonly ChessBoardEventSubscriber eventSubscriber_;

        public ChessPadManager(ChessPad chessPad)
        {
            chessPad_ = chessPad;
            eventSystem_ = new EventSystem();
            eventSubscriber_ = new ChessBoardEventSubscriber(this, eventSystem_, chessPad_);
        }

        public bool Input(Input input)
        {
            if(!CheckInput(input)) return false;
            RestPos(chessPad_, input.pos);

            PadGrid padGrid = chessPad_[input.pos.x, input.pos.y];
            padGrid.Chess = input.chess;
            padGrid.Status = input.playerType == PlayerType.PLAYER ? PosStatus.OCCUPIED_PLAYER : PosStatus.OCCUPIED_RIVAL;

            Log.TestLine("SubscribeCardEffect", TextColor.BLACK);
            SubscribeCardEffect(input, chessPad_);

            Log.TestLine("DoSelfPower", TextColor.BLACK);
            DoSelfPower(input, chessPad_);

            Log.TestLine("RaiseChessPlaced", TextColor.BLACK);
            eventSystem_.RaiseChessPlaced(input);
            CheckDead(chessPad_);

            Log.TestLine("DoPosEffect", TextColor.BLACK);
            DoPosEffect(input, chessPad_);
            return true;
        }

        public ChessPad Preview(Input input)
        {
            if(!CheckInput(input)) return null;

            ChessPad previewPad = new ChessPad(chessPad_);

            previewPad[input.pos.x, input.pos.y].Chess = input.chess;

            DoPosEffect(input, previewPad);
            // DoCardEffect(input, previewPad);
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

        public void AddBuff(Int2D pos, Buff buff)
        {
            PadGrid padGrid = chessPad_[pos];
            padGrid.AddBuff(buff);
            if (padGrid.Status == PosStatus.OCCUPIED_PLAYER || padGrid.Status == PosStatus.OCCUPIED_RIVAL)
            {
                PlayerType gridOwner = padGrid.Status == PosStatus.OCCUPIED_PLAYER ? PlayerType.PLAYER : PlayerType.RIVAL;
                eventSystem_.RaiseChessBuffed(new EventSystem.ChessBuffedEventArgs(buff.value, gridOwner, padGrid.Position));
            }
        }

        private void SubscribeCardEffect(Input input, ChessPad chessPad)
        {
            if (input.chess.CardEffect == null) return;

            EffectCondition condition = input.chess.CardEffect.Condition;
            EffectConditionType conditionType = EffectConditionHelper.GetConditionType(condition);
            switch (conditionType)
            {
                case EffectConditionType.Placed:
                    eventSubscriber_.SubscribePlacedEvent(condition, input);
                    break;
                case EffectConditionType.Buffed:
                    eventSubscriber_.SubscribeBuffedEvent(condition, input);
                    break;
                case EffectConditionType.Dead:
                    eventSubscriber_.SubscribeDeadEvent(condition, input);
                    break;
                default: break;
            }
        }

        private void DoSelfPower(Input input, ChessPad chessPad)
        {
            int power = input.chess.Level;
            Buff selfLevelBuff = new Buff(input.pos, input.pos, power, EffectTarget.Self, input.playerType);
            chessPad[input.pos].AddBuff(selfLevelBuff);
            if (chessPad[input.pos].BuffValue != 0)
            {
                eventSystem_.RaiseChessBuffed(new EventSystem.ChessBuffedEventArgs(chessPad[input.pos].BuffValue, input.playerType, input.pos));
            }
        }

        private void CheckDead(ChessPad chessPad)
        {
            bool hasDead = false;
            foreach (var line in chessPad)
            {
                foreach (var padGrid in line)
                {
                    if (!padGrid.Empty && padGrid.Level <= 0)
                    {
                        hasDead = true;
                        RestPos(chessPad, padGrid.Position);
                    }
                }
            }
            if (hasDead) CheckDead(chessPad);
        }

        private static void DoPosEffect(Input input, ChessPad chessPad)
        {
            chessPad.StatusMap = PosEffect.DoPosEffect(input, chessPad);
        }

        private void RestPos(ChessPad chessPad, Int2D pos)
        {
            if (!chessPad.IsInBoard(pos) || chessPad[pos].Empty) return;

            PadGrid padGrid = chessPad[pos];
            if (padGrid.Chess.CardEffect != null && padGrid.Chess.CardEffect.Condition == EffectCondition.OnSelfDead)
            {
                EffectOperationHelper.TriggerEffect(chessPad, padGrid.Position, true, this);
            }

            eventSubscriber_.UnsubscribeAllAt(pos);

            PlayerType owner = padGrid.Status == PosStatus.OCCUPIED_PLAYER ? PlayerType.PLAYER : PlayerType.RIVAL;
            eventSystem_.RaiseChessDead(new EventSystem.ChessDeadEventArgs(padGrid.Chess, owner, pos));

            padGrid.Reset();
            foreach (var line in chessPad)
            {
                foreach (var grid in line)
                {
                    grid.RemoveBuff(pos);
                }
            }
        }

    //---------------------------------------------------------------------------------------------清理
        ~ChessPadManager()
        {
            eventSystem_.ClearAll();
        }
    }
}