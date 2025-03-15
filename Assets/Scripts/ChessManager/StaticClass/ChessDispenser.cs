using System.Collections.Generic;
using UnityEngine;

public class ChessDispenser : MonoBehaviour
{
    public static List<string> chessPool = new List<string>{};
    public static void DispenserNewChessToChessSelector() {
        GameObject newChessObj = InstantiateChess(DispenseChess());
        if (newChessObj != null) {
            ChessSelector.PushBackChess(newChessObj.GetComponent<Chess>());
        }
    }

    public static string DispenseChess() {
        if (chessPool.Count == 0) {
            return null;
        }
        System.Random random = new System.Random();
        int randomIndex = random.Next(chessPool.Count);
        string chess = chessPool[randomIndex];
        chessPool.RemoveAt(randomIndex);
        return chess;
    }

    public static void ReDispense(List<int> index) {
        List<Chess> popChessList = new List<Chess>{};
        foreach(int i in index) {
            if(i < 0 || i >= ChessSelector.chessList_.Count) {
                Debug.Log("ReDispense: Invaild chessIndex.");
                continue;
            }
            popChessList.Add(ChessSelector.chessList_[i]);
        }
        foreach(Chess chess in popChessList) {
            ChessSelector.chessList_.Remove(chess);
        }
        foreach(Chess chess in popChessList) {
            ChessSelector.PushBackChess(InstantiateChess(DispenseChess()).GetComponent<Chess>());
        }
        foreach(Chess chess in popChessList) {
            chessPool.Add(chess.cardCode);
        }
    }

    public static GameObject InstantiateChess(string chessName)
    {
        if (chessName == null) {
            return null;
        }
        ChessProperty instantiateData = GlobalScope.GetChessProperty(chessName);
        GameObject instantiateBody = Instantiate(GlobalScope.chessPrefab_static_, GlobalScope.chessSelector_static_.transform);
        GameObject instantiateModel = InstantiateChessModel(GlobalScope.chessModelPrefab_static_, instantiateBody, instantiateData, false);
        if (instantiateData != null) {
            Chess chess = instantiateBody.GetComponent<Chess>();
            chess.InstantiateChessProperty(instantiateData);
            chess.chessModel = instantiateModel;
            return instantiateBody;
        }
        return null;
    }
    public static GameObject InstantiateChessModel(GameObject chessModelPrefab, GameObject father, ChessProperty instantiateData, bool ifChessGridModel)
    {
        if (father == null) {
            return null;
        }
        GameObject instantiateModel = Instantiate(chessModelPrefab);
        instantiateModel.transform.SetParent(father.transform);
        instantiateModel.transform.localPosition = Vector3.zero;
        instantiateModel.transform.localRotation = Quaternion.Euler(-90, -90, 0);
        if (instantiateData != null) {
            foreach(Transform child in instantiateModel.transform) {
                switch (child.name) {
                    case "name":
                        // if (!ifChessGridModel)
                        child.gameObject.GetComponent<TextMesh>().text = instantiateData.Name;
                        break;
                    case "level":
                        if (!ifChessGridModel) {
                            child.gameObject.GetComponent<TextMesh>().text = instantiateData.Level.ToString();
                        }
                        break;
                    case "cost":
                        string costNum = "";
                        if (!ifChessGridModel) {
                            for(int i = 0; i<instantiateData.Cost ;i++) {
                                costNum+="●";
                            }
                        }
                        child.gameObject.GetComponent<TextMesh>().text = costNum;
                        break;
                    default:break;
                }
            }
            return instantiateModel;
        }
        return null;
    }

    public static int GetChessNumInChessSelector() {
        int result = 0;
        foreach(Transform child in GlobalScope.chessSelector_static_.transform) {
            result++;
        }
        return result;
    }
}
