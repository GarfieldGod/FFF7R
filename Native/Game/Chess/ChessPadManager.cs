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
    }

    public bool Input(Input input)
    {
        if(!CheckInput(input)) return false;

        PadGrid padGrid = chessPad_[input.pos.x, input.pos.y];
        padGrid.Chess = input.chess;

        DoPosEffect(input, chessPad_);
        DoSelfPower(input, chessPad_);
        DoCardEffect(input, chessPad_);

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
        else
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
        chessPad.AddBuff(input.pos, selfLevelBuff, false);
    }

    public void DoCardEffect(Input input, ChessPad chessPad, bool preview = false)
    {
        if (input.chess.CardEffect == null) return;
        // Do Others
        EffectCondition condition = input.chess.CardEffect.Condition;
        switch (condition)
        {
            case EffectCondition.OnPlayed:
                EffectOperationHelper.PowerChangeOnce(input, chessPad);
                break;
            case EffectCondition.OnSelfDead: break;
            case EffectCondition.OnPosition:
                EffectOperationHelper.PowerChangeLasting(input, chessPad);
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
                // TODO
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
            if (condition == EffectCondition.OnFriendOrEnemyPlayed)
            {
                EffectOperationHelper.TriggerEffect(chessPad_, input.pos);
            }
            else if (condition == EffectCondition.OnFriendPlayed && 
                (owner == PlayerType.PLAYER && input.playerType == PlayerType.PLAYER || 
                owner == PlayerType.RIVAL && input.playerType == PlayerType.RIVAL))
            {
                EffectOperationHelper.TriggerEffect(chessPad_, input.pos);
            }
            else if (condition == EffectCondition.OnEnemyPlayed &&
                (owner == PlayerType.PLAYER && input.playerType == PlayerType.RIVAL || 
                owner == PlayerType.RIVAL && input.playerType == PlayerType.PLAYER))
            {
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
            PlayerType owner = buffedOne.Owner;
            Int2D pos = buffedOne.Position;
            if (input.pos == pos) return;

            if (condition == EffectCondition.OnFriendOrEnemyDead)
            {
                EffectOperationHelper.TriggerEffect(chessPad_, input.pos);
            }
            else if (condition == EffectCondition.OnFriendDead && 
                (buffedOne.Owner == PlayerType.PLAYER && input.playerType == PlayerType.PLAYER || 
                buffedOne.Owner == PlayerType.RIVAL && input.playerType == PlayerType.RIVAL))
            {
                EffectOperationHelper.TriggerEffect(chessPad_, input.pos);
            }
            else if (condition == EffectCondition.OnEnemyDead &&
                (buffedOne.Owner == PlayerType.PLAYER && input.playerType == PlayerType.RIVAL || 
                buffedOne.Owner == PlayerType.RIVAL && input.playerType == PlayerType.PLAYER))
            {
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
            if (condition == EffectCondition.OnFriendOrEnemyDead)
            {
                EffectOperationHelper.TriggerEffect(chessPad_, input.pos);
            }
            else if (condition == EffectCondition.OnFriendDead && 
                (deadOne.Owner == PlayerType.PLAYER && input.playerType == PlayerType.PLAYER || 
                deadOne.Owner == PlayerType.RIVAL && input.playerType == PlayerType.RIVAL))
            {
                EffectOperationHelper.TriggerEffect(chessPad_, input.pos);
            }
            else if (condition == EffectCondition.OnEnemyDead &&
                (deadOne.Owner == PlayerType.PLAYER && input.playerType == PlayerType.RIVAL || 
                deadOne.Owner == PlayerType.RIVAL && input.playerType == PlayerType.PLAYER))
            {
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
                    if (padGrid.Chess.CardEffect != null && padGrid.Chess.CardEffect.Condition == EffectCondition.OnSelfDead)
                    {
                        EffectOperationHelper.TriggerEffect(chessPad, padGrid.Position);
                    }

                    PlayerType owner = padGrid.Status == PosStatus.OCCUPIED_PLAYER ? PlayerType.PLAYER : PlayerType.RIVAL;
                    UnsubscribeDeadEvent(padGrid.Position);
                    UnsubscribeBuffedEvent(padGrid.Position);
                    eventSystem_.RaiseChessDead(new EventSystem.ChessDeadEventArgs(padGrid.Chess, owner));
                    chessPad.RestPos(padGrid.Position);
                }
            }
        }
        if (hasDead) CheckDead(chessPad);
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

        eventSystem_.ClearAll();
    }
}