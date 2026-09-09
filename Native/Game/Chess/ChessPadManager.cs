public class ChessPadManager
{
    private ChessPad chessPad_;
    public ChessPadManager(ChessPad chessPad)
    {
        chessPad_ = chessPad;
    }

    public bool Input(Input input)
    {
        if(!CheckInput(input)) return false;

        PadGrid padGrid = chessPad_[input.pos.x, input.pos.y];
        padGrid.Chess = input.chess.Property;

        DoPosEffect(input, chessPad_);
        DoCardEffect(input, chessPad_);

        return true;
    }

    public ChessPad Preview(Input input)
    {
        if(!CheckInput(input)) return null;

        ChessPad previewPad = new ChessPad(chessPad_);

        previewPad[input.pos.x, input.pos.y].Chess = input.chess.Property;

        DoPosEffect(input, previewPad);
        DoCardEffect(input, previewPad);
        return previewPad;
    }

    public bool CheckInput(Input input)
    {
        int posLevel = (int)chessPad_[input.pos.x, input.pos.y].Status % 10;
        if (posLevel > 0 && posLevel < (int)PosStatus.OCCUPIED_PLAYER && posLevel < input.chess.Property.Cost)
        {
            Log.TestLine("CheckInput Failed: chessPosLevel: " + posLevel + "\nCost: " + input.chess.Property.Cost, TextColor.BLACK);
            return false;
        }

        HashSet<Int2D> validGrids = Rival.GetEmptyGrids(chessPad_.StatusMap, input.playerType);
        Log.TestLine("Valid ChessGrids: " + validGrids.Count, TextColor.BLACK);
        if (validGrids.Contains(input.pos)) return true;

        Log.TestLine("CheckInput Failed: ---Input is INVAILD---", TextColor.BLACK);
        return false;
    }

    public void DoPosEffect(Input input, ChessPad chessPad)
    {
        List<List<int>> effect = input.chess.Property.PosEffects;
        if (input.playerType == PlayerType.RIVAL)
        {
            Utils.Reverse(effect);
        }
        chessPad.StatusMap = PosEffect.DoPosEffect(input.pos, effect, chessPad.StatusMap, input.playerType);
    }

    public void DoCardEffect(Input input, ChessPad chessPad)
    {
        // Do Self
        // chessPad[input.pos.x][input.pos.y].SetID(id);
        int level = input.chess.Property.Level;
        Buff selfLevelBuff = new Buff(input.pos, level, EffectScope.Self, input.playerType);
        chessPad.AddBuff(input.pos, selfLevelBuff, input.playerType, false);

        EffectScope scope = input.chess.Property.CardEffects.Item1;
        EffectCondition condition = input.chess.Property.CardEffects.Item2;

        // Do Others
        List<Tuple<Int2D, int>> tasks = CardEffect.ParseCardEffect(input, chessPad);
        switch (condition)
        {
            case EffectCondition.ON_PLAYED:
                EffectToBuff(input, true);
                break;
            case EffectCondition.ON_POSITION:
                EffectToBuff(input, false);
                break;
            case EffectCondition.First_Buffed: break;
            case EffectCondition.First_DeBuffed: break;
            case EffectCondition.LevelFirstReach7: break; // BUFFED ONCE S
            case EffectCondition.Num_All: break;
            case EffectCondition.Num_Friend: break;
            case EffectCondition.Num_Enemy: break;
            case EffectCondition.Dead_All: break;
            case EffectCondition.Dead_Friend: break;
            case EffectCondition.ON_ENEMY_DEAD: break;
            case EffectCondition.ON_SELF_DEAD: break;
            case EffectCondition.EveryTime_Buffed: break;
            case EffectCondition.EveryTime_Debuffed: break;
            case EffectCondition.FriendPlayed: break;
            case EffectCondition.EnemyPlayed: break;
            case EffectCondition.CoverInput: break;
            case EffectCondition.LineWin: break;
        }
        chessPad.CheckDead();
    }

    public void EffectToBuff(Input input, bool dstOwned)
    {
        Log.TestLine("OnPlayedEffect", TextColor.BLACK);

        PadGrid grid = chessPad_[input.pos];
        if (dstOwned && grid.Empty) return;

        PlayerType type = grid.Status == PosStatus.OCCUPIED_PLAYER ? PlayerType.PLAYER : PlayerType.RIVAL;
        ChessProperty property = grid.Chess;

        EffectScope scope = property.CardEffects.Item1;
        List<Tuple<Int2D, int>> tasks = dstOwned ? CardEffect.ParseCardEffectInScope(input, chessPad_) : CardEffect.ParseCardEffect(input, chessPad_);

        foreach (var task in tasks)
        {
            Int2D dstPos = task.Item1;
            int value = task.Item2;

            Buff buff = new Buff(dstOwned ? dstPos : input.pos, value, scope, type);
            chessPad_.AddBuff(dstPos, buff, type);
        }
    }
}