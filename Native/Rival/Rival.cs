using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

public class AiRival {
    public static Input GetTheBestInput(ChessPad chessPad, List<Chess> chessInHand) {
        Input result = new Input();
        List<List<int>> RivalViewChessStatus = Utils.DeepCopy2DList(chessPad.StatusMap);
        List<Tuple<Int2D, int>> vaildChessGrids = Rival.GetAllFriendEmptyGrids(RivalViewChessStatus, PlayerType.RIVAL);
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
            result = new Input(chessGridPos, bestResultInPosEffect.Item2, PlayerType.RIVAL);
        }
        return result;
    }
    private static List<Tuple<Int2D, Chess, int>> TryTheBestInEveryCardForPosEffect(List<List<int>> chessGridPosStatus, List<Tuple<Int2D, int>> vaildChessGrids, List<Chess> chessInHand) {
        List<Tuple<Int2D, Chess, int>> result = new List<Tuple<Int2D, Chess, int>>{};
        foreach(var chess in chessInHand) {
            int chessPosPoint = 0;
            int posX = 0;
            int posY = 0;
            ChessProperty property = chess.Property;
            foreach(var vaildChessGrid in vaildChessGrids) {
                if (vaildChessGrid.Item2 < property.Cost) {
                    continue;
                }
                Int2D chessGridPos = new Int2D(vaildChessGrid.Item1.x, vaildChessGrid.Item1.y);
                List<List<int>> chessGridStatusTemp = PosEffect.DoPosEffect(chessGridPos, property.PosEffects, chessGridPosStatus, PlayerType.RIVAL);
                int effectResult = Rival.GetAllFriendEmptyGrids(chessGridStatusTemp, PlayerType.RIVAL).Count;
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
        //             if (buff.playerType == PlayerType.PLAYER) buff.playerType = PlayerType.RIVAL;
        //             else if (buff.playerType == PlayerType.RIVAL) buff.playerType = PlayerType.RIVAL;
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
                    case (int)PosStatus.LEVEL_ONE_PLAYER:
                    case (int)PosStatus.LEVEL_TWO_PLAYER:
                    case (int)PosStatus.LEVEL_THREE_PLAYER:
                        line[i] += (int)PosStatus.EMPTY;
                        break;
                    case (int)PosStatus.LEVEL_ONE_RIVAL:
                    case (int)PosStatus.LEVEL_TWO_RIVAL:
                    case (int)PosStatus.LEVEL_THREE_RIVAL:
                        line[i] -= (int)PosStatus.EMPTY;
                        break;
                    case (int)PosStatus.OCCUPIED_PLAYER:
                        line[i] = (int)PosStatus.OCCUPIED_RIVAL;
                        break;
                    case (int)PosStatus.OCCUPIED_RIVAL:
                        line[i] = (int)PosStatus.OCCUPIED_PLAYER;
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

        List<Tuple<Int2D, int>> friendEmpty = GetAllFriendEmptyGrids(chessStatus, PlayerType.RIVAL);
        List<Tuple<Int2D, int>> friendOccupied = GetAllFriendOccupiedGrids(chessStatus);
        bool hasCoverInput = false;
        int lowestCardLevel = int.MaxValue;
        foreach (Chess chess in chessInHand)
        {
            if (chess.Property.CardEffectConfig.condition == EffectCondition.CoverInput)
            {
                hasCoverInput = true;
            }
            else
            {
                lowestCardLevel = Math.Min(chess.Property.Level, lowestCardLevel);
            }
        }
        if (hasCoverInput)
        {
            result.AddRange(friendOccupied);
        }
        result.AddRange(friendEmpty.Where(grid => grid.Item2 >= lowestCardLevel));
        return result;
    }
    public static List<Tuple<Int2D, int>> GetAllFriendEmptyGrids(List<List<int>> chessStatus, PlayerType playerType) {
        List<Tuple<Int2D, int>> result = new List<Tuple<Int2D, int>>{};
        for(int i = 0; i < chessStatus.Count; i++) {
            for(int j = 0; j < chessStatus[i].Count; j++) {
                if (playerType == PlayerType.PLAYER)
                {
                    if (chessStatus[i][j] > (int)PosStatus.EMPTY % 10 && chessStatus[i][j] <= (int)PosStatus.LEVEL_THREE_PLAYER)
                    {
                        result.Add(new Tuple<Int2D, int>(new Int2D(i, j), chessStatus[i][j]));
                    }
                }
                else
                { 
                    if (chessStatus[i][j] > (int)PosStatus.EMPTY && chessStatus[i][j] <= (int)PosStatus.LEVEL_THREE_RIVAL)
                    {
                        result.Add(new Tuple<Int2D, int>(new Int2D(i, j), chessStatus[i][j]));
                    }
                }
            }
        }
        return result;
    }

    public static HashSet<Int2D> GetEmptyGrids(List<List<int>> chessStatus, PlayerType playerType) {
        HashSet<Int2D> result = [];
        for(int i = 0; i < chessStatus.Count; i++) {
            for(int j = 0; j < chessStatus[i].Count; j++) {
                if (playerType == PlayerType.PLAYER)
                {
                    if (chessStatus[i][j] > (int)PosStatus.EMPTY % 10 && chessStatus[i][j] <= (int)PosStatus.LEVEL_THREE_PLAYER)
                    {
                        result.Add(new Int2D(i, j));
                    }
                }
                else
                { 
                    if (chessStatus[i][j] > (int)PosStatus.EMPTY && chessStatus[i][j] <= (int)PosStatus.LEVEL_THREE_RIVAL)
                    {
                        result.Add(new Int2D(i, j));
                    }
                }
            }
        }
        return result;
    }

    public static List<Tuple<Int2D, int>> GetAllFriendOccupiedGrids(List<List<int>> chessStatus) {
        List<Tuple<Int2D, int>> result = new List<Tuple<Int2D, int>>{};
        for(int i = 0; i < chessStatus.Count; i++) {
            for(int j = 0; j < chessStatus[i].Count; j++) {
                if (chessStatus[i][j] == (int)PosStatus.OCCUPIED_PLAYER) {
                    result.Add(new Tuple<Int2D, int>(new Int2D(i, j), chessStatus[i][j]));
                }
            }
        }
        return result;
    }
}
