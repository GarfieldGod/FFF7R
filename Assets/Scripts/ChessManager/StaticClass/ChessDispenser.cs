using System.Collections.Generic;
using UnityEngine;

public class ChessDispenser : MonoBehaviour
{
    public ChessDispenser(ChessSelector chessSelector, List<string> chessPool, GameObject chessSelectorObj){
        chessPool_ = chessPool;
        chessSelector_ = chessSelector;
        chessSelectorObj_ = chessSelectorObj;
    }
    private List<string> chessPool_;
    private ChessSelector chessSelector_;
    private GameObject chessSelectorObj_;

    public string DispenseChess() {
        if (chessPool_.Count == 0) {
            return null;
        }
        System.Random random = new System.Random();
        int randomIndex = random.Next(chessPool_.Count);
        string chess = chessPool_[randomIndex];
        chessPool_.RemoveAt(randomIndex);
        return chess;
    }

    public void ReDispense(List<int> index) {
        List<Chess> popChessList = new List<Chess>{};
        foreach(int i in index) {
            if(i < 0 || i >= chessSelector_.chessList_.Count) {
                Debug.Log("ReDispense: Invaild chessIndex.");
                continue;
            }
            popChessList.Add(chessSelector_.chessList_[i]);
        }
        foreach(Chess chess in popChessList) {
            chessSelector_.chessList_.Remove(chess);
        }
        foreach(Chess chess in popChessList) {
            chessSelector_.PushBackChess(InstantiateChess(DispenseChess()).GetComponent<Chess>());
        }
        foreach(Chess chess in popChessList) {
            chessPool_.Add(chess.cardCode);
        }
    }

    public void DispenserNewChessToChessSelector() {
        GameObject newChessObj = InstantiateChess(DispenseChess());
        if (newChessObj != null) {
            chessSelector_.PushBackChess(newChessObj.GetComponent<Chess>());
        }
    }

    public GameObject InstantiateChess(string chessName)
    {
        if (chessName == null) {
            return null;
        }
        ChessProperty instantiateData = GlobalScope.GetChessProperty(chessName);
        GameObject instantiateBody = Instantiate(GlobalScope.chessPrefab_static_, chessSelectorObj_.transform);
        GameObject instantiateModel = InstantiateChessModel(GlobalScope.chessModelPrefab_static_, instantiateBody, instantiateData, false);
        if (instantiateData != null) {
            Chess chess = instantiateBody.GetComponent<Chess>();
            chess.InstantiateChessProperty(instantiateData);
            chess.chessModel = instantiateModel;
            return instantiateBody;
        }
        return null;
    }
    public GameObject InstantiateChessModel(GameObject chessModelPrefab, GameObject father, ChessProperty instantiateData, bool ifChessGridModel)
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

    public List<string> GetChessInChessPool() {
        return chessPool_;
    }
}
