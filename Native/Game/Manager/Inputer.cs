using System.Collections.Generic;

public enum InputerType {
    PLAYER,
    RIVAL,
    AI
}

public struct Input {
    public Int2D pos;
    public Chess chess;
    public Input(Int2D pos, Chess chess) {
        this.pos = pos;
        this.chess = chess;
    }
    public Input(Int2D pos, ChessProperty chess) {
        this.pos = pos;
        this.chess = new Chess(chess);
    }
    public Input(Int2D pos, string cardCode) {
        this.pos = pos;
        this.chess = new Chess(Property.GetChessProperty(cardCode));
    }
    public bool Empty()
    {
        if (chess == null)
        {
            return true;
        }
        return false;
    }
}

public class Inputer
{
    private Dispenser dispenser_;
    private Selector selector_;
    private readonly ChessPad chessPad_;
    private InputerType inputerType_;
    //--------------------------------------------
    private ChessPad originChessPad_ = new ChessPad();
    private Input input_;
    public Inputer(Dispenser dispenser, Selector selector, ChessPad chessPad, InputerType inputerType = InputerType.PLAYER)
    {
        dispenser_ = dispenser;
        selector_ = selector;
        chessPad_ = chessPad;
        inputerType_ = inputerType;
    }

    bool CheckInput(Input input)
    {
        int chessPosLevel = GetChessPad().GetChessGridStatus()[input.pos.x][input.pos.y] % 10;
        if (chessPosLevel < input.chess.GetChessProperty().Cost)
        {
            // Log.TestLine("chessPosLevel: " + chessPosLevel + "\nCost: " + input.chess.GetChessProperty().Cost);
            return false;
        }
        List<Tuple<Int2D, int>> vaildChessGrids = Rival.GetAllFriendEmptyGrids(GetChessPad().GetChessGridStatus());
        // Log.TestLine("vaildChessGrids: " + vaildChessGrids.Count);
        foreach (var vaildChessGrid in vaildChessGrids)
        {
            if (input.pos.x == vaildChessGrid.Item1.x && input.pos.y == vaildChessGrid.Item1.y)
            {
                Log.TestLine("---Input is VAILD---", TextColor.BLACK, true);
                return true;
            }
        }
        return false;
    }

    public ChessPad GetChessPad()
    {
        return GetChessPadByType(chessPad_);
    }

    public ChessPad GetChessPadByType(ChessPad chessPad)
    {
        return inputerType_ == InputerType.PLAYER ?
        chessPad : Rival.GetChessPadInRivalView(chessPad);
    }

    public List<Chess> GetChessInHand()
    {
        return selector_.GetAllChess();
    }

    public bool AddInput(Input input)
    {
        if (!CheckInput(input)) return false;
        input_ = input;
        Int2D pos = input.pos;
        ChessProperty property = input.chess.GetChessProperty();

        ChessPad tempChessPad = GetChessPad().DeepCopy();
        List<List<int>> tempGridStatus = PosEffect.DoPosEffect(pos, property.PosEffects, tempChessPad.GetChessGridStatus());
        DoEffcet(input, tempChessPad);
        tempChessPad.SetChessGridStatus(tempGridStatus);

        originChessPad_.Copy(chessPad_);
        chessPad_.Copy(GetChessPadByType(tempChessPad));
        return true;
    }
    public void RestoreInput()
    {
        chessPad_.Copy(originChessPad_);
        originChessPad_ = new ChessPad();
    }
    public void CommitInput()
    {
        ChessPad tempChessPad = GetChessPad().DeepCopy();
        List<List<Chess>> chesses = tempChessPad.GetChessStatus();
        chesses[input_.pos.x][input_.pos.y] = input_.chess;
        tempChessPad.SetChessStatus(chesses);

        chessPad_.Copy(GetChessPadByType(tempChessPad));
        originChessPad_ = new ChessPad();
    }

    public bool CanInput()
    {
        return GetVaildGrids().Count != 0;
    }

    public List<Tuple<Int2D, int>> GetVaildGrids()
    {
        return Rival.GetAllVaildChessGrids(selector_.GetAllChess(), GetChessPad().GetChessGridStatus());
    }

    public List<Tuple<Int2D, int>> GetEmptyFriendGrids()
    {
        return Rival.GetAllFriendEmptyGrids(GetChessPad().GetChessGridStatus());
    }

    public List<Tuple<Int2D, int>> GetOccupiedFriendGrids()
    {
        return Rival.GetAllFriendOccupiedGrids(GetChessPad().GetChessGridStatus());
    }

    public void DoEffcet(Input input, ChessPad chessPad)
    {
        string id = input.chess.GetChessProperty().Name + "_X_" + input.pos.x.ToString() + "_Y_" + input.pos.y.ToString();
        int level = input.chess.GetChessProperty().Level;
        EffectScope scope = input.chess.GetChessProperty().CardEffectConfig.scope;
        InputerType inputerType = inputerType_;

        // Do Self
        Buff selfBuff = new Buff(id, level, scope, inputerType);
        chessPad.AddStayBuff(input.pos, selfBuff);

        // Do Others
        switch (input.chess.GetChessProperty().CardEffects.Item2)
        {
            case EffectCondition.ON_PLAYED:
                chessPad.SetChessLevelStatus(CardEffect.DoCardEffect(input, chessPad));
                break;
            case EffectCondition.ON_POSITION:
                List<Tuple<Int2D, int>> effectTasks = CardEffect.ParseCardEffect(input, GetChessPad());
                // Log.TestLine("ON_POSITION TASK NUM: " + effectTasks.Count);
                foreach (var task in effectTasks)
                {
                    int value = task.Item2;
                    Buff buff = new Buff(id, value, scope, inputerType);
                    chessPad.AddStayBuff(task.Item1, buff);
                }
                break;
            case EffectCondition.Frist_Buffed: break;
            case EffectCondition.Frist_Debuffed: break;
            case EffectCondition.LevelFristReach7: break; // BUFFED ONCE S
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
    }
}