using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

public class AiRival {
    public static Input GetTheBestInput(ChessPad chessPad, List<Chess> chessInHand) {
        Input result = new Input();
        List<List<int>> RivalViewChessStatus = Utils.DeepCopy2DList(chessPad.GetGridStatusMap());
        List<Tuple<Int2D, int>> vaildChessGrids = Rival.GetAllFriendEmptyGrids(RivalViewChessStatus);
        if (vaildChessGrids.Count == 0) {
            return result;
        }
        List<Tuple<Int2D, Chess, int>> resultInPosEffect = TryTheBestInEveryCardForPosEffect(RivalViewChessStatus, vaildChessGrids, chessInHand);
        // List<Tuple<Int2D, string, int>> resultInCardEffect = DoTheBestByEveryCardInPosEffect(RivalViewChessStatus[0], vaildChessGrids, chessInHand);
        // List<Tuple<Int2D, string, int>> resultInSpecialEffect = DoTheBestByEveryCardInPosEffect(RivalViewChessStatus[0], vaildChessGrids, chessInHand);
        if (resultInPosEffect.Count != 0) {
            Tuple<Int2D, Chess, int> bestResultInPosEffect = FindTheHighestScore(resultInPosEffect);
            // Tuple<Int2D, string, int> bestResultInCardEffect = FindTheHighestScore(resultInCardEffect);
            // Tuple<Int2D, string, int> bestResultInSpecialEffect = FindTheHighestScore(resultInSpecialEffect);
            Int2D chessGridPos = new Int2D(bestResultInPosEffect.Item1.x, bestResultInPosEffect.Item1.y);
            // ChessProperty property = GlobalScope.GetChessProperty(bestResultInPosEffect.Item2);
            result = new Input(chessGridPos, bestResultInPosEffect.Item2);
        }
        return result;
    }
    private static List<Tuple<Int2D, Chess, int>> TryTheBestInEveryCardForPosEffect(List<List<int>> chessGridPosStatus, List<Tuple<Int2D, int>> vaildChessGrids, List<Chess> chessInHand) {
        List<Tuple<Int2D, Chess, int>> result = new List<Tuple<Int2D, Chess, int>>{};
        foreach(var chess in chessInHand) {
            int chessPosPoint = 0;
            int posX = 0;
            int posY = 0;
            ChessProperty property = chess.GetChessProperty();
            foreach(var vaildChessGrid in vaildChessGrids) {
                if (vaildChessGrid.Item2 < property.Cost) {
                    continue;
                }
                Int2D chessGridPos = new Int2D(vaildChessGrid.Item1.x, vaildChessGrid.Item1.y);
                List<List<int>> chessGridStatusTemp = PosEffect.DoPosEffect(chessGridPos, property.PosEffects, chessGridPosStatus);
                int effectResult = Rival.GetAllFriendEmptyGrids(chessGridStatusTemp).Count;
                if (effectResult >= chessPosPoint) {
                    chessPosPoint = effectResult;
                    posX = vaildChessGrid.Item1.x;
                    posY = vaildChessGrid.Item1.y;
                }
            }
            Tuple<Int2D, Chess, int> oneResult = new Tuple<Int2D, Chess, int>(new Int2D(posX, posY), chess, chessPosPoint);
            result.Add(oneResult);
        }
        return result;
    }

    private static Tuple<Int2D, Chess, int> FindTheHighestScore(List<Tuple<Int2D, Chess, int>> ScoreList) {
        int maxScore = 0;
        Tuple<Int2D, Chess, int> result = ScoreList[0];
        foreach(var Score in ScoreList) {
            if (Score.Item3 > maxScore) {
                maxScore = Score.Item3;
                result = Score;
            }
        }
        return result;
    }
}
public class Rival
{
    public static Int2D GetChessGridPosInRivalView(Int2D pos, int chessPadLength = 4) {
        // Log.TestLine("Origin: " + pos.y +" RivalView: " + (chessPadLength - pos.y).ToString());
        return new Int2D(pos.x, chessPadLength - pos.y);
    }

    public static List<List<int>> GetGridLevelInRivalView(List<List<int>> originChessStatus) {
        return GetChessPosStatusInRivalView(originChessStatus);
    }

    public static List<List<List<Buff>>> GetStayBuffMapInRivalView(List<List<List<Buff>>> src) {
        List<List<List<Buff>>> srcTemp = Utils.DeepCopy(src);
        List<List<List<Buff>>> result = new List<List<List<Buff>>>();
        foreach (var line in srcTemp) {
            List<List<Buff>> reversedLine = new List<List<Buff>>{};
            reversedLine.AddRange(line);
            reversedLine.Reverse();
            result.Add(reversedLine);
        }
        // for (int x = 0; x < result.Count; x++) {
        //     var line = result[x];
        //     for (int y = 0; y < line.Count; y++) {
        //         var buffs = line[y];
        //         for (int z = 0; z < buffs.Count; z++)
        //         {
        //             var buff = buffs[z];
        //             if (buff.inputerType == InputerType.PLAYER) buff.inputerType = InputerType.RIVAL;
        //             else if (buff.inputerType == InputerType.RIVAL) buff.inputerType = InputerType.RIVAL;
        //             if (buff.scope == EffectScope.FRIEND_ONLY) buff.scope = EffectScope.ENEMY_ONLY;
        //             else if (buff.scope == EffectScope.ENEMY_ONLY) buff.scope = EffectScope.FRIEND_ONLY;
        //             buffs[z] = buff;
        //         }
        //     }
        // }
        return result;
    }

    public static List<List<int>> GetChessPosStatusInRivalView(List<List<int>> chessPosStatus) {
        List<List<int>> result = Utils.DeepCopy2DList(chessPosStatus);
        foreach(var line in result) {
            for(int i = 0; i < line.Count; i++) {
                switch (line[i]) {
                    case (int)ChessPosStatus.LEVEL_ONE_FRIEND:
                    case (int)ChessPosStatus.LEVEL_TWO_FRIEND:
                    case (int)ChessPosStatus.LEVEL_THREE_FRIEND:
                        line[i] += (int)ChessPosStatus.EMPTY;
                        break;
                    case (int)ChessPosStatus.LEVEL_ONE_ENEMY:
                    case (int)ChessPosStatus.LEVEL_TWO_ENEMY:
                    case (int)ChessPosStatus.LEVEL_THREE_ENEMY:
                        line[i] -= (int)ChessPosStatus.EMPTY;
                        break;
                    case (int)ChessPosStatus.OCCUPIED_FRIEND:
                        line[i] = (int)ChessPosStatus.OCCUPIED_ENEMY;
                        break;
                    case (int)ChessPosStatus.OCCUPIED_ENEMY:
                        line[i] = (int)ChessPosStatus.OCCUPIED_FRIEND;
                        break;
                    default:
                        break;
                }
            }
            int left = 0;
            int right = line.Count - 1;
            while (left < right)
            {
                int temp = line[left];
                line[left] = line[right];
                line[right] = temp;
                left++;
                right--;
            }
        }
        return result;
    }

    public static List<List<int>> GetChessLevelStatusInRivalView(List<List<int>> src) {
        List<List<int>> srcTemp = Utils.DeepCopy2DList(src);
        foreach(var line in srcTemp) {
            int left = 0;
            int right = line.Count - 1;
            while (left < right)
            {
                int temp = line[left];
                line[left] = line[right];
                line[right] = temp;
                left++;
                right--;
            }
        }
        return srcTemp;
    }

    public static List<List<Chess>> GetChessMapInRivalView(List<List<Chess>> chessCardStatus) {
        List<List<Chess>> result = new List<List<Chess>>();
        foreach (var line in chessCardStatus) {
            List<Chess> reversedLine = new List<Chess>{};
            reversedLine.AddRange(line);
            reversedLine.Reverse();
            result.Add(reversedLine);
        }
        return result;
    }
    public static List<List<bool>> GetBuffStatusInRivalView(List<List<bool>> chessBuffStatus) {
        List<List<bool>> result = new List<List<bool>>();
        foreach (var line in chessBuffStatus) {
            List<bool> reversedLine = new List<bool>{};
            reversedLine.AddRange(line);
            reversedLine.Reverse();
            result.Add(reversedLine);
        }
        return result;
    }

    // public static List<List<PadGrid>> GetPadGridsInRivalView(List<List<PadGrid>> padGrids) {
    //     List<List<PadGrid>> result = new List<List<PadGrid>>();
    //     foreach (var line in padGrids) {
    //         List<PadGrid> reversedLine = new List<PadGrid>{};
    //         for (int i = line.Count; i >= 0; i--)
    //         {
    //             PadGrid newPadGrid = new PadGrid(line[i]);
    //             reversedLine.Add(newPadGrid);
    //         }
    //         result.Add(reversedLine);
    //     }
    //     return result;
    // }

    public static List<Tuple<Int2D, int>> GetAllVaildChessGrids(List<Chess> chessInHand, List<List<int>> chessStatus)
    {
        List<Tuple<Int2D, int>> result = new List<Tuple<Int2D, int>> { };
        if (chessInHand.Count == 0)
        {
            return result;
        }

        List<Tuple<Int2D, int>> friendEmpty = GetAllFriendEmptyGrids(chessStatus);
        List<Tuple<Int2D, int>> friendOccupied = GetAllFriendOccupiedGrids(chessStatus);
        bool hasCoverInput = false;
        int lowestCardLevel = int.MaxValue;
        foreach (Chess chess in chessInHand)
        {
            if (chess.GetChessProperty().CardEffectConfig.condition == EffectCondition.CoverInput)
            {
                hasCoverInput = true;
            }
            else
            {
                lowestCardLevel = Math.Min(chess.GetChessProperty().Level, lowestCardLevel);
            }
        }
        if (hasCoverInput)
        {
            result.AddRange(friendOccupied);
        }
        result.AddRange(friendEmpty.Where(grid => grid.Item2 >= lowestCardLevel));
        return result;
    }
    public static List<Tuple<Int2D, int>> GetAllFriendEmptyGrids(List<List<int>> chessStatus) {
        List<Tuple<Int2D, int>> result = new List<Tuple<Int2D, int>>{};
        for(int i = 0; i < chessStatus.Count; i++) {
            for(int j = 0; j < chessStatus[i].Count; j++) {
                if (chessStatus[i][j] > 0 && chessStatus[i][j] <= (int)ChessPosStatus.LEVEL_THREE_FRIEND) {
                    result.Add(new Tuple<Int2D, int>(new Int2D(i, j), chessStatus[i][j]));
                }
            }
        }
        return result;
    }

    public static List<Tuple<Int2D, int>> GetAllFriendOccupiedGrids(List<List<int>> chessStatus) {
        List<Tuple<Int2D, int>> result = new List<Tuple<Int2D, int>>{};
        for(int i = 0; i < chessStatus.Count; i++) {
            for(int j = 0; j < chessStatus[i].Count; j++) {
                if (chessStatus[i][j] == (int)ChessPosStatus.OCCUPIED_FRIEND) {
                    result.Add(new Tuple<Int2D, int>(new Int2D(i, j), chessStatus[i][j]));
                }
            }
        }
        return result;
    }
}
