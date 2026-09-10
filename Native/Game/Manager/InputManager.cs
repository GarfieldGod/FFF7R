using System.Collections.Generic;
using Test;

public enum PlayerType {
    PLAYER,
    RIVAL,
    Null
}

public struct Input {
    public Int2D pos;
    public ChessProperty chess;
    public PlayerType playerType;
    public Input(Int2D pos, ChessProperty chess, PlayerType playerType) {
        this.pos = pos;
        this.chess = chess;
        this.playerType = playerType;
    }
    public Input(Int2D pos, string cardCode, PlayerType playerType) {
        this.pos = pos;
        this.chess = Property.GetChessProperty(cardCode);
        this.playerType = playerType;
    }
    public readonly bool Empty => chess == null;
}

// public class InputManager
// {
//     private readonly ChessPad chessPad_;
//     private ChessPad originChessPad_ = new ChessPad(0, 0);
//     private Input input_;
//     public InputManager(ChessPad chessPad)
//     {
//         chessPad_ = chessPad;
//     }

//     bool CheckInput(Input input)
//     {
//         int posLevel = (int)chessPad_[input.pos.x, input.pos.y].Status % 10;
//         if (posLevel > 0 && posLevel < (int)PosStatus.OCCUPIED_PLAYER && posLevel < input.chess.Property.Cost)
//         {
//             Log.TestLine("CheckInput Failed: chessPosLevel: " + posLevel + "\nCost: " + input.chess.Property.Cost, TextColor.BLACK);
//             return false;
//         }

//         HashSet<Int2D> validGrids = Rival.GetEmptyGrids(chessPad_.StatusMap, input.playerType);
//         Log.TestLine("Valid ChessGrids: " + validGrids.Count, TextColor.BLACK);
//         if (validGrids.Contains(input.pos)) return true;

//         Log.TestLine("CheckInput Failed: ---Input is INVAILD---", TextColor.BLACK);
//         return false;
//     }

//     public ChessPad ChessPad
//     {
//         get
//         {
//             return chessPad_;
//         }
//     }

//     public List<Chess> GetChessInHand()
//     {
//         return selector_.GetAllChess();
//     }

//     public bool AddInput(Input input)
//     {
//         if (!CheckInput(input)) return false;
//         input_ = input;

//         ChessPad tempChessPad = GetChessPad().DeepCopy();
//         tempChessPad.SetChess(input.pos, input_.chess);
//         DoPosEffect(input, tempChessPad, inputerType_);
//         DoCardEffcet(input, tempChessPad, inputerType_);

//         originChessPad_.Copy(chessPad_);
//         chessPad_.Copy(GetChessPadByType(tempChessPad));
//         return true;
//     }
//     public void RestoreInput()
//     {
//         chessPad_.Copy(originChessPad_);
//         originChessPad_ = new ChessPad(chessPad_.GetSize());
//     }
//     public void CommitInput()
//     {
//         PadGrid padGrid = GetChessPad().GetGridMap()[input_.pos.x][input_.pos.y];
//         padGrid.SendEffcet("Send Effect In CommitInput");
//         originChessPad_ = new ChessPad(chessPad_.GetSize());
//     }

//     public bool CanInput()
//     {
//         return GetVaildGrids().Count != 0;
//     }

//     public List<Tuple<Int2D, int>> GetVaildGrids()
//     {
//         return Rival.GetAllVaildChessGrids(selector_.GetAllChess(), GetChessPad().GetGridStatusMap());
//     }

//     public List<Tuple<Int2D, int>> GetEmptyFriendGrids()
//     {
//         return Rival.GetAllFriendEmptyGrids(GetChessPad().GetGridStatusMap(), inputerType_);
//     }

//     public List<Tuple<Int2D, int>> GetOccupiedFriendGrids()
//     {
//         return Rival.GetAllFriendOccupiedGrids(GetChessPad().GetGridStatusMap());
//     }
//     public void DoPosEffect(Input input, ChessPad chessPad, PlayerType playerType)
//     {
//         Int2D pos = input.pos;
//         List<List<int>> effect = input.chess.GetChessProperty().PosEffect;
//         if (playerType == PlayerType.RIVAL)
//         {
//             Utils.Reverse(effect);
//         }
//         var padGrid = chessPad.GetGridMap()[input.pos.x][input.pos.y];
//         padGrid.SetGridBackUp(padGrid.GetGridStatus());
//         List<List<int>> tempGridStatus = PosEffect.DoPosEffect(pos, effect, chessPad.GetGridStatusMap(), playerType);
//         chessPad.SetGridStatusMap(tempGridStatus);
//     }
// }