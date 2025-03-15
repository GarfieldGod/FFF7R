using System;
using System.Collections.Generic;

public class AiRival {
    public static List<List<List<int>>> DoAiRivalInput(List<List<List<int>>> chessGridStatus, List<string> chessInHand, Dictionary<Int2D, List<Tuple<Int2D, int>>> tasksLasting) {
        List<List<List<int>>> RivalViewChessStatus = Rival.GetChessStatusInRivalView(chessGridStatus);
        List<Tuple<Int2D, int>> vaildChessGrids = Rival.GetAllVaildChessGrids(RivalViewChessStatus[0]);
        return DoTheBestInEveryChessOnEveryChessGrid(RivalViewChessStatus, vaildChessGrids, chessInHand, tasksLasting);
    }
    private static List<List<List<int>>> DoTheBestInEveryChessOnEveryChessGrid(List<List<List<int>>> chessGridStatus, List<Tuple<Int2D, int>> vaildChessGrids, List<string> chessInHand,
        Dictionary<Int2D, List<Tuple<Int2D, int>>> tasksLasting) {
        List<List<List<int>>> finalChessGridStatusTemp = GlobalScope.DeepCopy3DList(chessGridStatus);
        if(vaildChessGrids.Count == 0) {
            return Rival.GetChessStatusInRivalView(finalChessGridStatusTemp);
        }
        List<Tuple<Int2D, string, int>> resultInPosEffect = TryTheBestInEveryCardForPosEffect(chessGridStatus[0], vaildChessGrids, chessInHand);
        // List<Tuple<Int2D, string, int>> resultInCardEffect = DoTheBestByEveryCardInPosEffect(chessGridStatus[0], vaildChessGrids, chessInHand);
        // List<Tuple<Int2D, string, int>> resultInSpecialEffect = DoTheBestByEveryCardInPosEffect(chessGridStatus[0], vaildChessGrids, chessInHand);
        if(resultInPosEffect.Count != 0) {
            Tuple<Int2D, string, int> bestResultInPosEffect = FindTheHighestScore(resultInPosEffect);
            // Tuple<Int2D, string, int> bestResultInCardEffect = FindTheHighestScore(resultInCardEffect);
            // Tuple<Int2D, string, int> bestResultInSpecialEffect = FindTheHighestScore(resultInSpecialEffect);
            Int2D chessGridPos = new Int2D(bestResultInPosEffect.Item1.x, bestResultInPosEffect.Item1.y);
            ChessProperty property = GlobalScope.GetChessProperty(bestResultInPosEffect.Item2);
            ChessInputParm parmInput = new ChessInputParm(
                chessGridPos,
                property,
                finalChessGridStatusTemp,
                tasksLasting,
                true
            );
            ChessInputer.GetChessInput(parmInput);
            for(int i = 0; i < chessInHand.Count; i++) {
                if(chessInHand[i] == bestResultInPosEffect.Item2) {
                    chessInHand.RemoveAt(i);
                    break;
                }
            }
        }
        return Rival.GetChessStatusInRivalView(finalChessGridStatusTemp);
    }
    private static List<Tuple<Int2D, string, int>> TryTheBestInEveryCardForPosEffect(List<List<int>> chessGridPosStatus, List<Tuple<Int2D, int>> vaildChessGrids, List<string> chessInHand) {
        List<Tuple<Int2D, string, int>> result = new List<Tuple<Int2D, string, int>>{};
        foreach(var chessName in chessInHand) {
            int chessPosPoint = 0;
            int posX = 0;
            int posY = 0;
            ChessProperty property = GlobalScope.GetChessProperty(chessName);
            foreach(var vaildChessGrid in vaildChessGrids) {
                if(vaildChessGrid.Item2 < property.Cost) {
                    continue;
                }
                Int2D chessGridPos = new Int2D(vaildChessGrid.Item1.x, vaildChessGrid.Item1.y);
                List<List<int>> chessGridStatusTemp = PosEffect.DoPosEffect(chessGridPos, property.PosEffects, chessGridPosStatus);
                int effectResult = Rival.GetAllVaildChessGrids(chessGridStatusTemp).Count;
                if (effectResult >= chessPosPoint) {
                    chessPosPoint = effectResult;
                    posX = vaildChessGrid.Item1.x;
                    posY = vaildChessGrid.Item1.y;
                }
            }
            Tuple<Int2D, string, int> oneResult = new Tuple<Int2D, string, int>(new Int2D(posX, posY), chessName, chessPosPoint);
            result.Add(oneResult);
        }
        return result;
    }

    private static Tuple<Int2D, string, int> FindTheHighestScore(List<Tuple<Int2D, string, int>> ScoreList) {
        int maxScore = 0;
        Tuple<Int2D, string, int> result = ScoreList[0];
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
    public static Int2D GetChessGridPosInRivalView(Int2D pos) {
        int chessPadLength = GlobalScope.chessGridNameList_[0].Count - 1;
        Log.test("Origin: " + pos.y +" RivalView: " + (chessPadLength - pos.y).ToString());
        return new Int2D(pos.x, chessPadLength - pos.y);
    }
    public static List<List<List<int>>> GetChessStatusInRivalView(List<List<List<int>>> originChessStatus) {
        List<List<List<int>>> result = GlobalScope.DeepCopy3DList(originChessStatus);
        GetChessPosStatusInRivalView(result[0]);
        GetChessCardStatusInRivalView(result[1]);
        return result;
    }
    private static void GetChessPosStatusInRivalView(List<List<int>> chessPosStatus) {
        foreach(var line in chessPosStatus) {
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
    }

    private static void GetChessCardStatusInRivalView(List<List<int>> chessCardStatus) {
        foreach(var line in chessCardStatus) {
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
    }
    public static List<Tuple<Int2D, int>> GetAllVaildChessGrids(List<List<int>> chessStatus) {
        List<Tuple<Int2D, int>> result = new List<Tuple<Int2D, int>>{};
        for(int i = 0; i < chessStatus.Count; i++) {
            for(int j = 0; j < chessStatus[i].Count; j++) {
                if(chessStatus[i][j] > 0 && chessStatus[i][j] <= (int)ChessPosStatus.LEVEL_THREE_FRIEND) {
                    result.Add(new Tuple<Int2D, int>(new Int2D(i, j), chessStatus[i][j]));
                }
            }
        }
        return result;
    }
}
