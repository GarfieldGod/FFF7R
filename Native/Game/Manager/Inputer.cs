using System.Collections.Generic;
using Test;

public enum InputerType {
    PLAYER,
    RIVAL
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
    private ChessPad originChessPad_ = new ChessPad(0, 0);
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
        int chessPosLevel = GetChessPad().GetGridStatusMap()[input.pos.x][input.pos.y] % 10;
        if (chessPosLevel > 0 && chessPosLevel < (int)ChessPosStatus.OCCUPIED_FRIEND && chessPosLevel < input.chess.GetChessProperty().Cost)
        {
            Log.TestLine("CheckInput Failed: chessPosLevel: " + chessPosLevel + "\nCost: " + input.chess.GetChessProperty().Cost, TextColor.BLACK);
            return false;
        }
        List<Tuple<Int2D, int>> vaildChessGrids = Rival.GetAllFriendEmptyGrids(GetChessPad().GetGridStatusMap(), inputerType_);
        Log.TestLine("Vaild ChessGrids: " + vaildChessGrids.Count, TextColor.BLACK);
        foreach (var vaildChessGrid in vaildChessGrids)
        {
            if (input.pos.x == vaildChessGrid.Item1.x && input.pos.y == vaildChessGrid.Item1.y)
            {
                Log.TestLine("CheckInput Success: ---Input is VAILD---", TextColor.BLACK);
                return true;
            }
        }
        Log.TestLine("CheckInput Failed: ---Input is INVAILD---", TextColor.BLACK);
        return false;
    }

    public ChessPad GetChessPad()
    {
        return chessPad_;
    }

    public ChessPad GetChessPadByType(ChessPad chessPad)
    {
        return chessPad;
    }

    public List<Chess> GetChessInHand()
    {
        return selector_.GetAllChess();
    }

    public bool AddInput(Input input)
    {
        if (!CheckInput(input)) return false;
        input_ = input;

        ChessPad tempChessPad = GetChessPad().DeepCopy();
        tempChessPad.SetChess(input.pos, input_.chess);
        DoPosEffect(input, tempChessPad, inputerType_);
        DoCardEffcet(input, tempChessPad, inputerType_);

        originChessPad_.Copy(chessPad_);
        chessPad_.Copy(GetChessPadByType(tempChessPad));
        return true;
    }
    public void RestoreInput()
    {
        chessPad_.Copy(originChessPad_);
        originChessPad_ = new ChessPad(chessPad_.GetSize());
    }
    public void CommitInput()
    {
        PadGrid padGrid = GetChessPad().GetGridMap()[input_.pos.x][input_.pos.y];
        padGrid.SendEffcet("Send Effect In CommitInput");
        originChessPad_ = new ChessPad(chessPad_.GetSize());
    }

    public bool CanInput()
    {
        return GetVaildGrids().Count != 0;
    }

    public List<Tuple<Int2D, int>> GetVaildGrids()
    {
        return Rival.GetAllVaildChessGrids(selector_.GetAllChess(), GetChessPad().GetGridStatusMap());
    }

    public List<Tuple<Int2D, int>> GetEmptyFriendGrids()
    {
        return Rival.GetAllFriendEmptyGrids(GetChessPad().GetGridStatusMap(), inputerType_);
    }

    public List<Tuple<Int2D, int>> GetOccupiedFriendGrids()
    {
        return Rival.GetAllFriendOccupiedGrids(GetChessPad().GetGridStatusMap());
    }
    public void DoPosEffect(Input input, ChessPad chessPad, InputerType inputerType)
    {
        Int2D pos = input.pos;
        List<List<int>> effect = input.chess.GetChessProperty().PosEffects;
        if (inputerType == InputerType.RIVAL)
        {
            Utils.Reverse(effect);
        }
        var padGrid = chessPad.GetGridMap()[input.pos.x][input.pos.y];
        padGrid.SetGridBackUp(padGrid.GetGridStatus());
        List<List<int>> tempGridStatus = PosEffect.DoPosEffect(pos, effect, chessPad.GetGridStatusMap(), inputerType);
        chessPad.SetGridStatusMap(tempGridStatus);
    }
    public void DoCardEffcet(Input input, ChessPad chessPad, InputerType inputerType)
    {
        string id = input.chess.GetChessProperty().Name + "_X_" + input.pos.x.ToString() + "_Y_" + input.pos.y.ToString();
        int level = input.chess.GetChessProperty().Level;
        EffectScope scope = input.chess.GetChessProperty().CardEffects.Item1;
        List<List<PadGrid>> gridMap = chessPad.GetGridMap();
        // Do Self
        Buff selfBuff = new Buff(input.pos, id, level, scope, inputerType);
        chessPad.GetGridMap()[input.pos.x][input.pos.y].SetID(id);

        chessPad.AddBuff(input.pos, selfBuff, inputerType);

        // Do Others
        List<Tuple<Int2D, int>> tasks = CardEffect.ParseCardEffect(input, chessPad);
        switch (input.chess.GetChessProperty().CardEffects.Item2)
        {
            case EffectCondition.ON_PLAYED:
                List<List<Chess>> chessMap = chessPad.GetChessMap();
                foreach (var task in tasks)
                {
                    if (chessMap[task.Item1.x][task.Item1.y] != null)
                    {
                        id = chessMap[task.Item1.x][task.Item1.y].GetChessProperty().Name + "_X_" + task.Item1.x.ToString() + "_Y_" + task.Item1.y.ToString();
                        int value = task.Item2;
                        Buff buff = new Buff(input.pos, id, value, scope, inputerType);
                        chessPad.AddBuff(task.Item1, buff, inputerType);
                    }
                }
                break;
            case EffectCondition.ON_POSITION:
                foreach (var task in tasks)
                {
                    int value = task.Item2;
                    Buff buff = new Buff(input.pos, id, value, scope, inputerType);
                    chessPad.AddBuff(task.Item1, buff, inputerType);
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
        chessPad.CheckDead();
    }
}