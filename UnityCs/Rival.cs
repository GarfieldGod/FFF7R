using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
#if UNITY_ENGINE
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;
public class Rival : MonoBehaviour
#else 
public class Rival
#endif
{
    public static List<List<int>> GetChessStatusInRivalView(List<List<int>> originChessStatus) {
        // List<List<int>> result = new List<List<int>>(originChessStatus);
        List<List<int>> result = originChessStatus.Select(innerList => new List<int>(innerList)).ToList();
        foreach(var line in result) {
            for(int i = 0; i < line.Count; i++) {
                switch (line[i]) {
                    case (int)GlobalScope.ChessPosStatus.LEVEL_ONE_FRIEND:
                    case (int)GlobalScope.ChessPosStatus.LEVEL_TWO_FRIEND:
                    case (int)GlobalScope.ChessPosStatus.LEVEL_THREE_FRIEND:
                        line[i] += (int)GlobalScope.ChessPosStatus.EMPTY;
                        break;
                    case (int)GlobalScope.ChessPosStatus.LEVEL_ONE_ENEMY:
                    case (int)GlobalScope.ChessPosStatus.LEVEL_TWO_ENEMY:
                    case (int)GlobalScope.ChessPosStatus.LEVEL_THREE_ENEMY:
                        line[i] -= (int)GlobalScope.ChessPosStatus.EMPTY;
                        break;
                    case (int)GlobalScope.ChessPosStatus.OCCUPIED_FRIEND:
                        line[i] = (int)GlobalScope.ChessPosStatus.OCCUPIED_ENEMY;
                        break;
                    case (int)GlobalScope.ChessPosStatus.OCCUPIED_ENEMY:
                        line[i] = (int)GlobalScope.ChessPosStatus.OCCUPIED_FRIEND;
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

    public static List<Tuple<Tuple<int, int>, int>> GetAllVaildChessGrids(List<List<int>> chessStatus) {
        List<Tuple<Tuple<int, int>, int>> result = new List<Tuple<Tuple<int, int>, int>>{};
        for(int i = 0; i < chessStatus.Count; i++) {
            for(int j = 0; j < chessStatus[i].Count; j++) {
                if(chessStatus[i][j] > 0 && chessStatus[i][j] <= (int)GlobalScope.ChessPosStatus.LEVEL_THREE_FRIEND) {
                    result.Add(new Tuple<Tuple<int, int>, int>(new Tuple<int, int>(i, j), chessStatus[i][j]));
                }
            }
        }
        return result;
    }
}

public class AiRival {
    public static List<List<int>> DoRivalInput(List<List<int>> chessGridStatus, List<string> chessInHand) {
        List<List<int>> RivalViewChessStatus = Rival.GetChessStatusInRivalView(chessGridStatus);
        List<Tuple<Tuple<int, int>, int>> vaildChessGrids = Rival.GetAllVaildChessGrids(RivalViewChessStatus);
        return DoTheBestInEveryChessOnEveryChessGrid(RivalViewChessStatus, vaildChessGrids, chessInHand);
    }
    public static List<List<int>> DoTheBestInEveryChessOnEveryChessGrid(List<List<int>> chessGridStatus, List<Tuple<Tuple<int, int>, int>> vaildChessGrids, List<string> chessInHand) {
        List<List<int>> finalChessGridStatusTemp = chessGridStatus.Select(innerList => new List<int>(innerList)).ToList();
        if(vaildChessGrids.Count == 0) {
            return Rival.GetChessStatusInRivalView(finalChessGridStatusTemp);
        }
        //PosEffct
        //List<<Pos(y, x), CardName, score>>
        List<Tuple<Tuple<int, int>, string, int>> result = new List<Tuple<Tuple<int, int>, string, int>>{};
        foreach(var chessName in chessInHand) {
            int chessPosPoint = 0;
            int posY = 0;
            int posX = 0;
            foreach(var vaildChessGrid in vaildChessGrids) {
                GlobalScope.ChessProperty property = GlobalScope.GetChessProperty(chessName);
                if(vaildChessGrid.Item2 > property.Level) {
                    continue;
                }
                // List<List<int>> chessGridStatusTemp = new List<List<int>>(chessGridStatus);
                List<List<int>> chessGridStatusTemp = chessGridStatus.Select(innerList => new List<int>(innerList)).ToList();
                chessGridStatusTemp = ChessInputer.DoPosEffect(new Tuple<int, int>(vaildChessGrid.Item1.Item1, vaildChessGrid.Item1.Item2), property.PosEffects, chessGridStatusTemp);
                //OtherEffect // TODO
                if (Rival.GetAllVaildChessGrids(chessGridStatusTemp).Count >= chessPosPoint) {
                    chessPosPoint = Rival.GetAllVaildChessGrids(chessGridStatusTemp).Count;
                    posY = vaildChessGrid.Item1.Item1;
                    posX = vaildChessGrid.Item1.Item2;
                }
            }
            Tuple<Tuple<int, int>, string, int> oneResult = new Tuple<Tuple<int, int>, string, int>(new Tuple<int, int>(posY, posX), chessName, chessPosPoint);
            result.Add(oneResult);
        }
        if(result.Count == 0) {
            return Rival.GetChessStatusInRivalView(finalChessGridStatusTemp);
        }
        int maxScore = 0;
        Tuple<Tuple<int, int>, string, int> finalResult = result[0];
        foreach(var oneResult in result) {
            if (oneResult.Item3 > maxScore) {
                maxScore = oneResult.Item3;
                finalResult = oneResult;
            }
        }
        // List<List<int>> finalChessGridStatusTemp = new List<List<int>>(chessGridStatus);
        GlobalScope.ChessProperty finalProperty = GlobalScope.GetChessProperty(finalResult.Item2);
        finalChessGridStatusTemp = ChessInputer.DoPosEffect(new Tuple<int, int>(finalResult.Item1.Item1, finalResult.Item1.Item2), finalProperty.PosEffects, finalChessGridStatusTemp);
        for(int i = 0; i < chessInHand.Count; i++) {
            if(chessInHand[i] == finalResult.Item2) {
                chessInHand.RemoveAt(i);
                break;
            }
        }
        return Rival.GetChessStatusInRivalView(finalChessGridStatusTemp);
    }
}