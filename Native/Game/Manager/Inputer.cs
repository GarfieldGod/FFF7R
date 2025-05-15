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
    public bool Empty() {
        if (chess == null) {
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
            return false;
        }
        List<Tuple<Int2D, int>> vaildChessGrids = Rival.GetAllFriendEmptyGrids(GetChessPad().GetChessGridStatus());
        foreach (var vaildChessGrid in vaildChessGrids)
        {
            if (input.pos.x == vaildChessGrid.Item1.x && input.pos.y == vaildChessGrid.Item1.y)
            {
                Log.TestLine("---Input is VAILD---", TextColor.RED, true);
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
        Int2D Pos = input.pos;
        ChessProperty property = input.chess.GetChessProperty();

        List<List<int>> newGridStatus = PosEffect.DoPosEffect(Pos, property.PosEffects, GetChessPad().GetChessGridStatus());
        List<List<int>> newLevelStatus = DoEffcet(input);
        ChessPad tempChessPad = new ChessPad(newGridStatus, GetChessPad().GetChessStatus());
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
        List<List<Chess>> chesses = GetChessPad().GetChessStatus();
        chesses[input_.pos.x][input_.pos.y] = input_.chess;
        ChessPad chessPad = new ChessPad(GetChessPad().GetChessGridStatus(), chesses);
        chessPad_.Copy(GetChessPadByType(chessPad));
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

    public List<List<int>> DoEffcet(Input input)
    {
        switch (input.chess.GetChessProperty().CardEffectConfig.condition)
        {
            case EffectCondition.Played:
                return CardEffect.DoCardEffect(input, GetChessPad());
            case EffectCondition.Stay:
                input.chess.AddDeadEffect(input);
                return CardEffect.DoCardEffect(input, GetChessPad());
            case EffectCondition.Frist_Buffed: break;
            case EffectCondition.Frist_Debuffed: break;
            case EffectCondition.LevelFristReach7: break; // BUFFED ONCE S
            case EffectCondition.Num_All: break;
            case EffectCondition.Num_Friend: break;
            case EffectCondition.Num_Enemy: break;
            case EffectCondition.Dead_All: break;
            case EffectCondition.Dead_Friend: break;
            case EffectCondition.Dead_Enemy: break;
            case EffectCondition.Dead_Self: break;
            case EffectCondition.EveryTime_Buffed: break;
            case EffectCondition.EveryTime_Debuffed: break;
            case EffectCondition.FriendPlayed: break;
            case EffectCondition.EnemyPlayed: break;
            case EffectCondition.CoverInput: break;
            case EffectCondition.LineWin: break;
        }
        return GetChessPad().GetChessLevelStatus();
    }
}