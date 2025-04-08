using System;
using System.Collections.Generic;

public class AiRival {
    // public static List<List<List<int>>> DoRivalInput(ChessPadInfo chessPadInfo, List<string> chessInHand) {
    //     ChessInputParms chessInputParms = GetTheBestInput(chessPadInfo, chessInHand);
    //     ChessInputParm parmInput = new ChessInputParm(
    //         chessInputParms,
    //         new ChessPadInfo(chessPadInfo, tasksLasting),
    //         true
    //     );
    //     ChessInputer.GetChessInput(parmInput);
    //     for(int i = 0; i < chessInHand.Count; i++) {
    //         if(chessInHand[i] == bestResultInPosEffect.Item2) {
    //             chessInHand.RemoveAt(i);
    //             break;
    //         }
    //     }
    //     return Rival.GetChessStatusInRivalView(finalChessGridStatusTemp);
    // }
    public static ChessInputParms GetTheBestInput(ChessPadInfo chessPadInfo, List<string> chessInHand) {
        ChessInputParms result = new ChessInputParms();
        List<List<List<int>>> RivalViewChessStatus = Rival.GetChessPadStatusInRivalView(chessPadInfo.chessPadStatus);
        List<Tuple<Int2D, int>> vaildChessGrids = Rival.GetAllVaildChessGrids(RivalViewChessStatus[0]);
        if(vaildChessGrids.Count == 0) {
            return result;
        }
        List<Tuple<Int2D, string, int>> resultInPosEffect = TryTheBestInEveryCardForPosEffect(RivalViewChessStatus[0], vaildChessGrids, chessInHand);
        // List<Tuple<Int2D, string, int>> resultInCardEffect = DoTheBestByEveryCardInPosEffect(RivalViewChessStatus[0], vaildChessGrids, chessInHand);
        // List<Tuple<Int2D, string, int>> resultInSpecialEffect = DoTheBestByEveryCardInPosEffect(RivalViewChessStatus[0], vaildChessGrids, chessInHand);
        if(resultInPosEffect.Count != 0) {
            Tuple<Int2D, string, int> bestResultInPosEffect = FindTheHighestScore(resultInPosEffect);
            // Tuple<Int2D, string, int> bestResultInCardEffect = FindTheHighestScore(resultInCardEffect);
            // Tuple<Int2D, string, int> bestResultInSpecialEffect = FindTheHighestScore(resultInSpecialEffect);
            Int2D chessGridPos = new Int2D(bestResultInPosEffect.Item1.x, bestResultInPosEffect.Item1.y);
            // ChessProperty property = GlobalScope.GetChessProperty(bestResultInPosEffect.Item2);
            result = new ChessInputParms(chessGridPos, bestResultInPosEffect.Item2);
        }
        return result;
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
    public static int GetScoreInOneLine(List<List<List<int>>> originChessStatus, int Line) {
        int score = 0;
        for(int j = 0; j< originChessStatus[0].Count; j++) {
            if(originChessStatus[0][Line][j] == (int)ChessPosStatus.OCCUPIED_FRIEND) {
                score += originChessStatus[1][Line][j] + originChessStatus[2][Line][j];
            }
        }
        return score;
    }
    public static Int2D GetChessGridPosInRivalView(Int2D pos) {
        int chessPadLength = GlobalScope.chessGridNameList_[0].Count - 1;
        // Log.test("Origin: " + pos.y +" RivalView: " + (chessPadLength - pos.y).ToString());
        return new Int2D(pos.x, chessPadLength - pos.y);
    }
    public static List<List<List<int>>> GetChessPadStatusInRivalView(List<List<List<int>>> originChessStatus) {
        List<List<List<int>>> result = GlobalScope.DeepCopy3DList(originChessStatus);
        GetChessPosStatusInRivalView(result[0]);
        GetChessCardStatusInRivalView(result[1]);
        GetChessCardStatusInRivalView(result[2]);
        return result;
    }
    public static void TurnChessPadStatusToRivalView(List<List<List<int>>> originChessStatus) {
        GetChessPosStatusInRivalView(originChessStatus[0]);
        GetChessCardStatusInRivalView(originChessStatus[1]);
        GetChessCardStatusInRivalView(originChessStatus[2]);
    }
    public static void GetChessPosStatusInRivalView(List<List<int>> chessPosStatus) {
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

    public static void GetChessCardStatusInRivalView(List<List<int>> chessCardStatus) {
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
